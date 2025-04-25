// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Moq;

using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    public static class MoqPropertyTrackingExtensions
    {
        private static readonly ConditionalWeakTable<object, PropertyTracker> _trackers = new();

        /// <summary>
        /// Tracks all properties of the mocked type.
        /// </summary>
        /// <typeparam name="T">The type of the mock object.</typeparam>
        /// <param name="mock">The mock object to track properties for.</param>
        public static void TrackProperties<T>(this Mock<T> mock) where T : class
        {
            var configurator = new PropertyTrackingConfigurator();
            var options = configurator.Build();
            ApplyTracking(mock, options);
        }

        /// <summary>
        /// Tracks properties using a custom configuration.
        /// </summary>
        /// <typeparam name="T">The type of the mock object.</typeparam>
        /// <param name="mock">The mock object to track properties for.</param>
        /// <param name="configAction">A lambda to configure property tracking options.</param>
        public static void TrackProperties<T>(this Mock<T> mock, Func<IPropertyTrackingConfigurator, IPropertyTrackingConfigurator> configAction) where T : class
        {
            var configurator = new PropertyTrackingConfigurator();
            var options = configAction(configurator).Build();
            ApplyTracking(mock, options);
        }

        /// <summary>
        /// Applies property tracking to the specified mock object using the provided options.
        /// </summary>
        /// <typeparam name="T">The type of the mock object.</typeparam>
        /// <param name="mock">The mock object to apply tracking to.</param>
        /// <param name="options">The property tracking options to use.</param>
        public static void ApplyTracking<T>(this Mock<T> mock, PropertyTrackingOptions options) where T : class
        {
            var type = typeof(T);
            var tracker = new PropertyTracker();

            // Retrieve properties based on access filter and tracking options.
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead || p.CanWrite)
                .Where(p => options.ShouldTrackProperty(p))
                .ToList();

            foreach (var prop in properties)
            {
                tracker.RegisterProperty(prop.Name, prop.PropertyType);

                // Setup getter tracking.
                if (options.CanTrackForRead(prop))
                {
                    try
                    {
                        var propertyExpression = GetPropertyExpression<T>(prop);
                        mock.SetupGet(propertyExpression)
                            .Returns(() =>
                            {
                                var value = tracker.GetValue(prop.Name);
                                tracker.RecordGet(prop.Name, value);
                                return value;
                            });
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.WriteLine($"Skipping property '{prop.Name}' due to invalid getter expression: {ex.Message}");
                        continue;
                    }
                }

                // Setup setter tracking.
                if (options.CanTrackForWrite(prop))
                {
                    try
                    {
                        var propertyAction = SetPropertyAction<T>(prop).Compile();
                        mock.SetupSet(propertyAction)
                            .Callback((object? val) =>
                            {
                                tracker.SetValue(prop.Name, val!);
                                tracker.RecordSet(prop.Name, val!);
                            });
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.WriteLine($"Skipping property '{prop.Name}' due to invalid setter expression: {ex.Message}");
                        continue;
                    }
                }
            }

            // Ensure the mock implements the ITrackedMock interface for PropertyTracker access.
            mock.As<ITrackedMock>().SetupGet(m => m.PropertyTracker).Returns(tracker);
        }

        /// <summary>
        /// Retrieves a property name from a lambda expression.
        /// </summary>
        /// <typeparam name="T">The type of the mock object.</typeparam>
        /// <param name="expression">The property lambda expression.</param>
        /// <returns>The name of the property.</returns>
        private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Invalid property expression");
        }

        /// <summary>
        /// Builds a getter expression for the property.
        /// </summary>
        private static Expression<Func<T, object>> GetPropertyExpression<T>(PropertyInfo property) where T : class
        {
            var param = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(param, property);
            var convert = Expression.Convert(member, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param);
        }

        /// <summary>
        /// Builds a setter action for the property.
        /// </summary>
        private static Expression<Action<T>> SetPropertyAction<T>(PropertyInfo property) where T : class
        {
            var param = Expression.Parameter(typeof(T), "x");
            var valueParam = Expression.Parameter(typeof(object), "val");
            var member = Expression.Property(param, property);
            var assignment = Expression.Assign(member, Expression.Convert(valueParam, property.PropertyType));
            return Expression.Lambda<Action<T>>(assignment, param);
        }

        private static void ObsoleteTrackProperties<T>(Mock<T> mock, PropertyTrackingOptions propertyTrackingOptions) where T : class
        {
            if (!_trackers.TryGetValue(mock.Object, out var tracker))
            {
                tracker = new PropertyTracker();
                _trackers.Add(mock.Object, tracker);
            }

            var mockType = typeof(T);
            var propertiesToTrack = new List<PropertyInfo>();

            if (propertyTrackingOptions.TrackAll)
            {
                propertiesToTrack.AddRange(mockType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead || p.CanWrite));
            }
            else if (propertyTrackingOptions.Predicate != null)
            {
                propertiesToTrack.AddRange(mockType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(propertyTrackingOptions.Predicate));
            }
            else
            {
                throw new InvalidOperationException("No properties to track. Please provide a valid configuration.");
            }

            foreach (var propertyInfo in propertiesToTrack.Distinct())
            {
                if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
                {
                    Debug.WriteLine($"Property '{propertyInfo.Name}' is neither readable nor writable. Skipping tracking.");
                    continue;
                }

                if (propertyInfo.CanRead)
                {
                    TrackReadPropertyWithProxy(mock, mockType, propertyInfo, tracker);
                }

                if (propertyInfo.CanWrite)
                {
                    TrackWritePropertyWithProxy(mock, mockType, propertyInfo, tracker);
                }

                // Ensure backing field registration
                tracker.RegisterProperty(propertyInfo.Name, propertyInfo.PropertyType);
            }
        }

        private static void TrackReadPropertyWithProxy<T>(Mock<T> mock, Type mockType, PropertyInfo propertyInfo, PropertyTracker propertyTracker) where T : class
        {
            var getMethod = propertyInfo.GetGetMethod();

            if (getMethod is null)
            {
                // This should not happen, but just in case the property does not have a getter, we log and skip tracking.
                Debug.WriteLine($"Property '{propertyInfo.Name}' does not have a getter. Skipping tracking.");
                return;
            }

            var returnType = getMethod.ReturnType;
            var setupGetMethod = typeof(Mock<T>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "SetupGet" && m.GetParameters().Length == 1);
            var genericSetupGetMethod = setupGetMethod?.MakeGenericMethod(returnType);

            if (genericSetupGetMethod is null)
            {
                Debug.WriteLine($"Unable to find SetupGet method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            var parameter = Expression.Parameter(mockType, "m");
            var lambdaExpression = Expression.Lambda(
                Expression.Property(parameter, propertyInfo),
                parameter
            );
            var setup = genericSetupGetMethod?.Invoke(mock, new object[] { lambdaExpression });
            var returnsMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Returns" && m.GetParameters().Length == 1);

            if (returnsMethod is null)
            {
                Debug.WriteLine($"Unable to find Returns method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            // Fix: Explicitly resolve the correct GetValue<T> method to avoid ambiguity
            var getValueMethod = typeof(PropertyTracker)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == nameof(PropertyTracker.GetValue) && m.IsGenericMethod);

            if (getValueMethod is null)
            {
                Debug.WriteLine($"Unable to find GetValue method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            var genericGetValueMethod = getValueMethod.MakeGenericMethod(returnType);
            var returnsDelegateType = typeof(Func<>).MakeGenericType(returnType);

            // Fix: Ensure the delegate matches the expected signature and handles null values
            var returnsDelegate = Delegate.CreateDelegate(
                returnsDelegateType,
                propertyTracker,
                genericGetValueMethod
            );

            // Invoke the Returns method with the correct delegate
            returnsMethod.Invoke(setup, new object[] { returnsDelegate });

            // Ensure backing field is initialized on read
            var callbackMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Callback" && m.GetParameters().Length == 1);

            if (callbackMethod is null)
            {
                Debug.WriteLine($"Unable to find Callback method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            var callbackAction = new Action(() =>
            {
                if (!propertyTracker.BackingFields.ContainsKey(propertyInfo.Name))
                {
                    propertyTracker.RegisterProperty(propertyInfo.Name, propertyInfo.PropertyType);
                }
            });

            callbackMethod.Invoke(setup, new object[] { callbackAction });

            // Ensure CallBase for getters
            var callBaseProperty = setup?.GetType().GetProperty("CallBase");
            callBaseProperty?.SetValue(setup, true);
        }

        private static void TrackWritePropertyWithProxy<T>(Mock<T> mock, Type mockType, PropertyInfo propertyInfo, PropertyTracker propertyTracker) where T : class
        {
            var setMethod = propertyInfo.GetSetMethod();

            if (setMethod is null)
            {
                // This should not happen, but just in case the property does not have a setter, we log and skip tracking.
                Debug.WriteLine($"Property '{propertyInfo.Name}' does not have a setter. Skipping tracking.");
                return;
            }

            var parameterType = setMethod.GetParameters().First().ParameterType;
            var setupSetMethod = typeof(Mock<T>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "SetupSet" && m.GetParameters().Length == 1);
            var genericSetupSetMethod = setupSetMethod?.MakeGenericMethod(parameterType);

            if (genericSetupSetMethod is null)
            {
                Debug.WriteLine($"Unable to find SetupSet method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            var lambdaExpression = Expression.Lambda(Expression.Property(Expression.Parameter(mockType, "m"), propertyInfo), Expression.Parameter(mockType, "m"));
            var setup = genericSetupSetMethod?.Invoke(mock, new object[] { lambdaExpression });
            var callbackMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Callback" && m.GetParameters().Length == 1);

            if (callbackMethod is null)
            {
                Debug.WriteLine($"Unable to find Callback method for property '{propertyInfo.Name}'. Skipping tracking.");
                return;
            }

            // Fix: Ensure the delegate matches the expected signature and handles null values
            var callbackActionType = typeof(Action<>).MakeGenericType(parameterType);
            var callbackAction = Delegate.CreateDelegate(callbackActionType, new Action<object>((value) =>
            {
                propertyTracker.SetValue(propertyInfo.Name, value);
                propertyTracker.RecordSet(propertyInfo.Name, value);
            }), "Invoke");

            callbackMethod.Invoke(setup, new object[] { callbackAction });

            // Ensure CallBase for setters
            var callBaseProperty = setup?.GetType().GetProperty("CallBase");

            callBaseProperty?.SetValue(setup, true);

            // Ensure backing field is initialized on write
            var callbackActionForWrite = new Action(() =>
            {
                if (!propertyTracker.BackingFields.ContainsKey(propertyInfo.Name))
                {
                    propertyTracker.RegisterProperty(propertyInfo.Name, propertyInfo.PropertyType);
                }
            });

            callbackMethod?.Invoke(setup, new object[] { callbackActionForWrite });
        }
    }
}
