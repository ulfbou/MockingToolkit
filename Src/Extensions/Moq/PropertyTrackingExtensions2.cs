// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

using Moq;

using System.Linq.Expressions;
using System.Reflection;

#if false
namespace MockingToolkit.Extensions.Moq
{
    public static class PropertyTrackingExtensions1
    {
        private static readonly string TrackedPropertiesKey = "__TrackedProperties__";
        private static readonly Dictionary<Mock, Dictionary<string, List<object>>> _trackedPropertyStores = new Dictionary<Mock, Dictionary<string, List<object>>>(new ReferenceEqualityComparer());
        private static readonly Dictionary<Mock, Dictionary<string, List<object>>> _propertySetLog = new Dictionary<Mock, Dictionary<string, List<object>>>();
        private static readonly Dictionary<Mock, Dictionary<string, object>> _propertyValues = new Dictionary<Mock, Dictionary<string, object>>();
        private static readonly Dictionary<Mock, Dictionary<string, List<Delegate>>> _propertySetHandlers = new Dictionary<Mock, Dictionary<string, List<Delegate>>>();
        private static readonly Dictionary<Mock, Dictionary<string, List<object>>> _propertyValueHistory = new Dictionary<Mock, Dictionary<string, List<object>>>();
        private static readonly Dictionary<Mock, Dictionary<string, int>> _propertySetCounts = new Dictionary<Mock, Dictionary<string, int>>();

        private static Dictionary<string, List<object>> GetTrackedPropertyStore(Mock mock)
        {
            if (!mock.CallBase)
            {
                throw new InvalidOperationException("Mock.CallBase must be enabled to track properties with this implementation.");
            }
            if (!_trackedPropertyStores.TryGetValue(mock, out var store))
            {
                store = new Dictionary<string, List<object>>();
                _trackedPropertyStores[mock] = store;
            }
            return store;
        }

        private class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public new static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();
            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }
            public override int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }

        public static void TrackAllProperties<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (!_propertySetLog.ContainsKey(mock))
            {
                _propertySetLog[mock] = new Dictionary<string, List<object>>();
                _propertyValues[mock] = new Dictionary<string, object>();
                _propertySetHandlers[mock] = new Dictionary<string, List<Delegate>>();
                _propertyValueHistory[mock] = new Dictionary<string, List<object>>();
                _propertySetCounts[mock] = new Dictionary<string, int>();
            }
            if (!_trackedPropertyStores.ContainsKey(mock))
            {
                _trackedPropertyStores[mock] = new Dictionary<string, List<object>>();
            }

            foreach (var property in typeof(TMock).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyName = property.Name;

                if (property.CanWrite)
                {
                    var propertyType = property.PropertyType;
                    var mockT = (Mock<TMock>)mock;
                    var parameter = Expression.Parameter(typeof(TMock), "m");
                    var propertyAccess = Expression.Property(parameter, propertyName);
                    var isAnyMethodInfo = typeof(It).GetMethod("IsAny", BindingFlags.Public | BindingFlags.Static);
                    var genericIsAnyMethod = isAnyMethodInfo?.MakeGenericMethod(propertyType);
                    var itIsAnyCall = Expression.Call(null, genericIsAnyMethod!);
                    var assignExpression = Expression.Assign(propertyAccess, itIsAnyCall);
                    var setupExpression = Expression.Lambda<Action<TMock>>(assignExpression, parameter);

                    mockT.SetupSet(setupExpression.Compile())
                        .Callback<object>(value =>
                        {
                            var store = GetTrackedPropertyStore(mock);
                            if (!store.ContainsKey(propertyName))
                            {
                                store[propertyName] = new List<object>();
                            }
                            store[propertyName].Add(value!);
                            _propertyValues[mock][propertyName] = value;
                            if (!_propertySetLog[mock].ContainsKey(propertyName))
                            {
                                _propertySetLog[mock][propertyName] = new List<object>();
                            }
                            _propertySetLog[mock][propertyName].Add(value);
                            if (!_propertyValueHistory[mock].ContainsKey(propertyName))
                            {
                                _propertyValueHistory[mock][propertyName] = new List<object>();
                            }
                            _propertyValueHistory[mock][propertyName].Add(value);
                            if (!_propertySetCounts[mock].ContainsKey(propertyName))
                            {
                                _propertySetCounts[mock][propertyName] = 0;
                            }
                            _propertySetCounts[mock][propertyName]++;

                            if (_propertySetHandlers.TryGetValue(mock, out var handlersForMock) &&
                                handlersForMock.TryGetValue(propertyName, out var handlersForProperty))
                            {
                                foreach (var handler in handlersForProperty)
                                {
                                    handler.DynamicInvoke(value);
                                }
                            }
                            typeof(TMock).GetProperty(propertyName)?.SetValue(mock.Object, value);
                        });
                }

                if (property.CanRead)
                {
                    var propertyType = property.PropertyType;
                    var mockT = (Mock<TMock>)mock;
                    var parameter = Expression.Parameter(typeof(TMock), "p");
                    var propertyAccess = Expression.Property(parameter, propertyName);
                    var lambda = Expression.Lambda(propertyAccess, parameter);

                    var setupGetMethod = typeof(Mock<TMock>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == "SetupGet" && m.GetParameters().Length == 1);
                    var genericSetupGetMethod = setupGetMethod?.MakeGenericMethod(propertyType);
                    if (genericSetupGetMethod != null)
                    {
                        var setup = genericSetupGetMethod.Invoke(mockT, new object[] { lambda });
                        var returnsMethod = setup?.GetType().GetMethod("Returns", new[] { typeof(Func<>).MakeGenericType(propertyType) });
                        if (returnsMethod != null)
                        {
                            var funcType = typeof(Func<>).MakeGenericType(propertyType);
                            var delegateInstance = Delegate.CreateDelegate(funcType, null, typeof(PropertyTrackingExtensions)
                                .GetMethod(nameof(GetTrackedPropertyValue), BindingFlags.NonPublic | BindingFlags.Static)
                                ?.MakeGenericMethod(typeof(TMock), propertyType)
                                ?? throw new InvalidOperationException("Failed to resolve method for GetTrackedPropertyValue."));
                            returnsMethod.Invoke(setup, new object[] { delegateInstance });
                        }
                    }
                }
            }
        }
        }

        private static object? GetTrackedPropertyValue<TMock>(Mock<TMock> mock, string propertyName, Type propertyType) where TMock : class
        {
            if (_trackedPropertyStores.TryGetValue(mock, out var store) &&
                store.TryGetValue(propertyName, out List<object>? history) &&
                history.Count > 0)
            {
                return Convert.ChangeType(history.LastOrDefault()!, propertyType);
            }
            return propertyType.IsValueType ? Activator.CreateInstance(propertyType)! : null;
        }

        public static TProperty? GetLastPropertyValue<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            return _propertyValues.TryGetValue(mock, out var values) && values.TryGetValue(propertyName, out var value) ? (TProperty)value : default;
        }

        public static List<object> GetPropertyValueHistory<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            return _propertyValueHistory.TryGetValue(mock, out var history) && history.TryGetValue(propertyName, out var values) ? values.ToList() : new List<object>();
        }

        public static int GetPropertySetCount<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            return _propertySetCounts.TryGetValue(mock, out var counts) && counts.TryGetValue(propertyName, out var count) ? count : 0;
        }

        public static Dictionary<string, object?> GetAllTrackedPropertyChanges<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            var store = GetTrackedPropertyStore(mock);
            return store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.LastOrDefault());
        }

        public static List<(string PropertyName, object Value)> GetPropertyChangeHistory<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            var store = GetTrackedPropertyStore(mock);
            return store.SelectMany(kvp => kvp.Value.Select(v => (kvp.Key, v))).ToList();
        }

        public static void AssertPropertyWasSet<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property, TProperty expectedValue, Times times) where TMock : class
        {
            var propertyName = GetPropertyName(property);
            var actualCount = 0;
            if (!_propertySetLog.TryGetValue(mock, out var propertyLogs) || !propertyLogs.TryGetValue(propertyName, out var log))
            {
                if (!IsTimesSatisfied(times, actualCount))
                {
                    throw new InvalidOperationException($"Expected property '{propertyName}' to be set with value '{expectedValue}' {times}, but it was never set.");
                }
                return;
            }

            actualCount = log.Count(v => EqualityComparer<TProperty>.Default.Equals((TProperty)v, expectedValue));

            if (!IsTimesSatisfied(times, actualCount))
            {
                throw new InvalidOperationException($"Expected property '{propertyName}' to be set with value '{expectedValue}' {times}, but it was set {actualCount} times.");
            }
        }

        private static bool IsTimesSatisfied(Times times, int actualCount)
        {
            if (times == default)
            {
                throw new ArgumentNullException(nameof(times));
            }
            if (times == Times.AtLeastOnce())
            {
                return actualCount >= 1;
            }
            if (times == Times.Never())
            {
                return actualCount == 0;
            }
            if (times == Times.Once())
            {
                return actualCount == 1;
            }
            if (times is Exactly exactlyTimes)
            {
                return actualCount == exactlyTimes.ExpectedInvocationCount;
            }
            if (times is AtLeast atLeastTimes)
            {
                return actualCount >= atLeastTimes.ExpectedInvocationCount;
            }
            if (times is AtMost atMostTimes)
            {
                return actualCount <= atMostTimes.ExpectedInvocationCount;
            }
            throw new ArgumentException("Unsupported Times condition.");
        }

        public static void AssertPropertyWasSet<T, TProperty>(this Mock<T> mock, Expression<Func<T, TProperty>> property, TProperty expectedValue, int times) where T : class
            => AssertPropertyWasSet(mock, property, expectedValue, Times.Exactly(times));

        public static void AssertPropertyWasSetAny<T, TProperty>(this Mock<T> mock, Expression<Func<T, TProperty>> property) where T : class
        {
            if (GetPropertySetCount(mock, property) == 0)
            {
                throw new InvalidOperationException($"Expected property '{GetPropertyName(property)}' to be set at least once, but it was never set.");
            }
        }

        public static void AssertPropertyWasNotSet<T, TProperty>(this Mock<T> mock, Expression<Func<T, TProperty>> property) where T : class
        {
            if (GetPropertySetCount(mock, property) > 0)
            {
                throw new InvalidOperationException($"Expected property '{GetPropertyName(property)}' to not be set, but it was set {GetPropertySetCount(mock, property)} times.");
            }
        }

        public static void AssertPropertyValueHistoryContains<T, TProperty>(this Mock<T> mock, Expression<Func<T, TProperty>> property, TProperty expectedValue) where T : class
        {
            if (!GetPropertyValueHistory(mock, property).Contains(expectedValue))
            {
                throw new InvalidOperationException($"Expected property '{GetPropertyName(property)}' history to contain value '{expectedValue}', but it did not.");
            }
        }

        public static void OnPropertySet<T, TProperty>(this Mock<T> mock, Expression<Func<T, TProperty>> property, Action<TProperty> handler) where T : class
        {
            var propertyName = GetPropertyName(property);
            if (!_propertySetHandlers.TryGetValue(mock, out var handlersForMock))
            {
                handlersForMock = new Dictionary<string, List<Delegate>>();
                _propertySetHandlers[mock] = handlersForMock;
            }
            if (!handlersForMock.TryGetValue(propertyName, out var handlersForProperty))
            {
                handlersForProperty = new List<Delegate>();
                handlersForMock[propertyName] = handlersForProperty;
            }
            handlersForProperty.Add(handler);
        }

        private static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo.Name;
            }
            throw new ArgumentException($"The lambda expression '{property}' should target a property.");
        }

        private static Expression<Func<T, object>> GetPropertyExpression<T>(PropertyInfo propertyInfo) where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var convert = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, parameter);
        }

        private static Expression<Action<T, object>> SetPropertyExpression<T>(PropertyInfo propertyInfo) where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var property = Expression.Property(parameter, propertyInfo);
            var convert = Expression.Convert(valueParameter, propertyInfo.PropertyType);
            return Expression.Lambda<Action<T, object>>(Expression.Assign(property, convert), parameter, valueParameter);
        }
    }
}
namespace MockingToolkit.Extensions.Moq
{
    public static class PropertyTrackingExtensions2
    {
        private static readonly string TrackedPropertiesKey = "__TrackedProperties__";
        private static readonly Dictionary<Mock, Dictionary<string, List<object>>> _trackedPropertyStores = new Dictionary<Mock, Dictionary<string, List<object>>>(new ReferenceEqualityComparer());

        private static Dictionary<string, List<object>> GetTrackedPropertyStore(Mock mock)
        {
            if (!mock.CallBase)
            {
                throw new InvalidOperationException("Mock.CallBase must be enabled to track properties with this implementation.");
            }

            if (!_trackedPropertyStores.TryGetValue(mock, out var store))
            {
                store = new Dictionary<string, List<object>>();
                _trackedPropertyStores[mock] = store;
            }
            return store;
        }

        private class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public new static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();

            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }

        public static void TrackAllProperties<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));

            foreach (var property in typeof(TMock).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite)
                {
                    var propertyName = property.Name;
                    var propertyType = property.PropertyType;
                    var mockT = (Mock<TMock>)mock;

                    var parameter = Expression.Parameter(typeof(TMock), "m");
                    var propertyAccess = Expression.Property(parameter, propertyName);

                    var isAnyMethodInfo = typeof(It).GetMethod("IsAny", BindingFlags.Public | BindingFlags.Static);
                    var genericIsAnyMethod = isAnyMethodInfo?.MakeGenericMethod(propertyType);
                    var itIsAnyCall = Expression.Call(null, genericIsAnyMethod!);
                    var assignExpression = Expression.Assign(propertyAccess, itIsAnyCall);
                    var setupExpression = Expression.Lambda<Action<TMock>>(assignExpression, parameter);
                    mockT.SetupSet(setupExpression.Compile())
                        .Callback<object>(value =>
                        {
                            var store = GetTrackedPropertyStore(mock);
                            if (!store.ContainsKey(propertyName))
                            {
                                store[propertyName] = new List<object>();
                            }
                            store[propertyName].Add(value!);
                            System.Diagnostics.Debug.WriteLine($"Setter for {propertyName} called with value: {value}");

                            // Also try to set the underlying mock object's property
                            typeof(TMock).GetProperty(propertyName)?.SetValue(mock.Object, value);
                        });
                }

                if (property.CanRead)
                {
                    System.Diagnostics.Debug.WriteLine($"Executing CanRead for {property.Name}.");
                    var propertyName = property.Name;
                    var propertyType = property.PropertyType;
                    var mockT = (Mock<TMock>)mock;

                    // Construct a simple property access expression
                    var parameter = Expression.Parameter(typeof(TMock), "p");
                    var propertyAccess = Expression.Property(parameter, propertyName);
                    var lambda = Expression.Lambda(propertyAccess, parameter);

                    // Use the generic SetupGet with the correct property type
                    var setupGetMethod = typeof(Mock<TMock>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == "SetupGet" && m.GetParameters().Length == 1);
                    var genericSetupGetMethod = setupGetMethod?.MakeGenericMethod(propertyType);

                    if (genericSetupGetMethod != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Before invoking Getter for {propertyName} is being set up.");
                        var setup = genericSetupGetMethod.Invoke(mockT, new object[] { lambda });
                        var returnsMethod = setup?.GetType().GetMethod("Returns", new[] { typeof(Func<>).MakeGenericType(propertyType) });

                        returnsMethod?.Invoke(setup, new object[] {
   Delegate.CreateDelegate(
       typeof(Func<>).MakeGenericType(propertyType),
       (Func<object>)(() =>
       {
           if (_trackedPropertyStores.TryGetValue(mock, out var store) &&
               store.TryGetValue(propertyName, out List<object>? history) &&
               history.Count > 0)
           {
               return Convert.ChangeType(history.LastOrDefault()!, propertyType);
           }
           return propertyType.IsValueType ? Activator.CreateInstance(propertyType)! : null!;
       })
   )
});

                        System.Diagnostics.Debug.WriteLine($"After invoking Getter for {propertyName} is being set up.");
                    }
                }
            }
        }

        private static object? GetTrackedPropertyValue<TMock, TProperty>(Mock<TMock> mock, string propertyName) where TMock : class
        {
            if (_trackedPropertyStores.TryGetValue(mock, out var store) &&
                store.TryGetValue(propertyName, out var history) &&
                history.Count > 0)
            {
                return history.LastOrDefault();
            }
            return default(TProperty);
        }

        public static TProperty? GetLastPropertyValue<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            var store = GetTrackedPropertyStore(mock);
            return store.TryGetValue(propertyName, out var history) && history.Any()
                ? (TProperty)history.LastOrDefault()!
                : default;
        }

        public static List<object> GetPropertyValueHistory<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            var store = GetTrackedPropertyStore(mock);
            return store.TryGetValue(propertyName, out var history) ? history.ToList() : new List<object>();
        }

        public static int GetPropertySetCount<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));
            var store = GetTrackedPropertyStore(mock);
            return store.TryGetValue(propertyName, out var history) ? history.Count : 0;
        }

        public static Dictionary<string, object?> GetAllTrackedPropertyChanges<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            var store = GetTrackedPropertyStore(mock);
            return store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.LastOrDefault());
        }

        public static List<(string PropertyName, object Value)> GetPropertyChangeHistory<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            var store = GetTrackedPropertyStore(mock);
            return store.SelectMany(kvp => kvp.Value.Select(v => (kvp.Key, v))).ToList();
        }

        public static void AssertPropertyWasSet<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property, TProperty expectedValue, int times = 1) where TMock : class
        {
            var history = GetPropertyValueHistory(mock, property);
            if (history.Count(v => EqualityComparer<TProperty>.Default.Equals((TProperty)v, expectedValue)) != times)
            {
                throw new InvalidOperationException($"Expected property '{((property.Body as MemberExpression)?.Member.Name)}' to be set to '{expectedValue}' {times} times, but it was set {history.Count(v => EqualityComparer<TProperty>.Default.Equals((TProperty)v, expectedValue))} times.");
            }
        }

        public static void AssertPropertyWasSetAny<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property, int atLeastTimes = 1) where TMock : class
        {
            var history = GetPropertyValueHistory(mock, property);
            if (history.Count >= atLeastTimes) return;
            throw new InvalidOperationException($"Expected property '{((property.Body as MemberExpression)?.Member.Name)}' to be set at least {atLeastTimes} times, but it was set {history.Count} times.");
        }

        public static void AssertPropertyWasNotSet<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property) where TMock : class
        {
            if (GetPropertySetCount(mock, property) > 0)
            {
                throw new InvalidOperationException($"Expected property '{((property.Body as MemberExpression)?.Member.Name)}' to not be set, but it was set {GetPropertySetCount(mock, property)} times.");
            }
        }

        public static void AssertPropertyValueHistoryContains<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property, TProperty expectedValue) where TMock : class
        {
            var history = GetPropertyValueHistory(mock, property);
            if (!history.Any(v => EqualityComparer<TProperty>.Default.Equals((TProperty)v, expectedValue)))
            {
                throw new InvalidOperationException($"Expected property '{((property.Body as MemberExpression)?.Member.Name)}' history to contain '{expectedValue}', but it did not.");
            }
        }

        private static readonly Dictionary<Mock, Dictionary<string, List<Delegate>>> _propertySetHandlers = new Dictionary<Mock, Dictionary<string, List<Delegate>>>(new ReferenceEqualityComparer());

        public static void OnPropertySet<TMock, TProperty>(this Mock<TMock> mock, Expression<Func<TMock, TProperty>> property, Action<TProperty> handler) where TMock : class
        {
            if (mock == null) throw new ArgumentNullException(nameof(mock));
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var propertyName = (property.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Invalid property expression.", nameof(property));

            if (!_propertySetHandlers.TryGetValue(mock, out var mockHandlers))
            {
                mockHandlers = new Dictionary<string, List<Delegate>>();
                _propertySetHandlers[mock] = mockHandlers;
            }

            if (!mockHandlers.TryGetValue(propertyName, out var handlers))
            {
                handlers = new List<Delegate>();
                mockHandlers[propertyName] = handlers;
            }

            handlers.Add(handler);

            // Set up the Callback only once per property per mock using SetupSet(Action)
            if (handlers.Count == 1)
            {
                var parameter = Expression.Parameter(typeof(TMock), "m");
                var propertyAccess = Expression.Property(parameter, propertyName);
                var itIsAnyMethod = typeof(It).GetMethod("IsAny", BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(typeof(TProperty));
                var assignExpression = Expression.Assign(propertyAccess, Expression.Call(null, itIsAnyMethod!));
                var setupExpression = Expression.Lambda<Action<TMock>>(assignExpression, parameter);

                mock.SetupSet(setupExpression.Compile())
                    .Callback<TProperty>(value =>
                    {
                        var store = GetTrackedPropertyStore(mock);
                        if (!store.ContainsKey(propertyName))
                        {
                            store[propertyName] = new List<object>();
                        }
                        store[propertyName].Add(value!);

                        if (_propertySetHandlers.TryGetValue(mock, out var currentMockHandlers) &&
                            currentMockHandlers.TryGetValue(propertyName, out var currentHandlers))
                        {
                            foreach (var h in currentHandlers)
                            {
                                if (h is Action<TProperty> typedHandler)
                                {
                                    typedHandler(value);
                                }
                            }
                        }
                    });
            }
        }
    }
}
#endif