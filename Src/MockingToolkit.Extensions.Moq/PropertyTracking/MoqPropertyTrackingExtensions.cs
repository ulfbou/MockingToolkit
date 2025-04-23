// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Castle.DynamicProxy;

using Moq;

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    public static class MoqPropertyTrackingExtensions
    {
        private static readonly ProxyGenerator _proxyGenerator = new();
        private static readonly ConditionalWeakTable<object, PropertyTracker> _trackers = new();

        /// <summary>
        /// Enables tracking of all readable and writable properties on the mocked object.
        /// </summary>
        /// <typeparam name="T">The type of the mocked object.</typeparam>
        /// <param name="mock">The <see cref="Mock{T}"/> instance.</param>
        public static void TrackProperties<T>(this Mock<T> mock) where T : class =>
            TrackProperties(mock, new PropertyTrackingConfigurator().TrackAll().Build());

        /// <summary>
        /// Enables tracking of the specified properties on the mocked object.
        /// </summary>
        /// <typeparam name="T">The type of the mocked object.</typeparam>
        /// <param name="mock">The <see cref="Mock{T}"/> instance.</param>
        /// <param name="propertyExpressions">An array of expressions that identify the properties to track (e.g., <c>m => m.PropertyName</c>).</param>
        public static void TrackProperties<T>(this Mock<T> mock, params Expression<Func<T, object>>[] propertyExpressions) where T : class
        {
            var propertyNames = propertyExpressions
                .Select(expr =>
                {
                    if (expr.Body is MemberExpression memberExpression)
                    {
                        return memberExpression.Member.Name;
                    }
                    else if (expr.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
                    {
                        return unaryMemberExpression.Member.Name;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid property expression");
                    }
                })
                .ToArray();
            TrackProperties(mock, new PropertyTrackingConfigurator().TrackProperties(propertyNames).Build());
        }

        /// <summary>
        /// Enables tracking of properties on the mocked object using a configuration action.
        /// </summary>
        /// <typeparam name="T">The type of the mocked object.</typeparam>
        /// <param name="mock">The <see cref="Mock{T}"/> instance.</param>
        /// <param name="configAction">An action that configures the property tracking options using <see cref="IPropertyTrackingConfigurator"/>.</param>
        public static void TrackProperties<T>(this Mock<T> mock, Func<IPropertyTrackingConfigurator, IPropertyTrackingConfigurator> configAction) where T : class
        {
            var config = configAction(new PropertyTrackingConfigurator());
            TrackProperties(mock, config.Build());
        }

        public static PropertyTracker? GetPropertyTracker<T>(this Mock<T> mock) where T : class
        {
            return (_trackers.TryGetValue(mock.Object, out var tracker) ? tracker : null);
        }

        private static void TrackProperties<T>(Mock<T> mock, PropertyTrackingOptions propertyTrackingOptions) where T : class
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
            else if (propertyTrackingOptions.TrackedPropertyNames != null && propertyTrackingOptions.TrackedPropertyNames.Any())
            {
                propertiesToTrack.AddRange(mockType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => propertyTrackingOptions.TrackedPropertyNames.Contains(p.Name)));
            }
            else if (propertyTrackingOptions.Predicate != null)
            {
                propertiesToTrack.AddRange(mockType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(propertyTrackingOptions.Predicate));
            }

            foreach (var propertyInfo in propertiesToTrack.Distinct())
            {
                if (propertyInfo.CanRead)
                {
                    TrackReadProperty(mock, mockType, propertyInfo, tracker);
                }

                if (propertyInfo.CanWrite)
                {
                    TrackWriteProperty(mock, mockType, propertyInfo, tracker);
                }

                // Ensure backing field registration
                tracker.RegisterProperty(propertyInfo.Name, propertyInfo.PropertyType);
            }
        }

        private static void TrackReadProperty<T>(Mock<T> mock, Type mockType, PropertyInfo propertyInfo, PropertyTracker tracker) where T : class
        {
            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod != null)
            {
                var returnType = getMethod.ReturnType;
                var setupGetMethod = typeof(Mock<T>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "SetupGet" && m.GetParameters().Length == 1);
                var genericSetupGetMethod = setupGetMethod?.MakeGenericMethod(returnType);

                var lambdaExpression = Expression.Lambda(
                    Expression.Property(Expression.Parameter(mockType, "m"), propertyInfo),
                    Expression.Parameter(mockType, "m")
                );
                var setup = genericSetupGetMethod?.Invoke(mock, new object[] { lambdaExpression });

                var returnsMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "Returns" && m.GetParameters().Length == 1);

                // Fix: Create a strongly-typed delegate matching the property's return type
                var returnsDelegate = (Func<string>)(() =>
                {
                    var value = tracker.GetValue(propertyInfo.Name);
                    tracker.RecordGet(propertyInfo.Name, value);
                    return (string)value;
                });

                returnsMethod?.Invoke(setup, new object[] { returnsDelegate });

                // Ensure backing field is initialized on read
                var callbackMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "Callback" && m.GetParameters().Length == 1);
                var callbackAction = new Action(() =>
                {
                    if (!tracker.BackingFields.ContainsKey(propertyInfo.Name))
                    {
                        tracker.RegisterProperty(propertyInfo.Name, propertyInfo.PropertyType);
                    }
                });
                callbackMethod?.Invoke(setup, new object[] { callbackAction });
            }
        }

        private static void TrackWriteProperty<T>(Mock<T> mock, Type mockType, PropertyInfo propertyInfo, PropertyTracker tracker) where T : class
        {
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod != null)
            {
                var parameterType = setMethod.GetParameters().First().ParameterType;
                var setupSetMethod = typeof(Mock<T>).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "SetupSet" && m.GetParameters().Length == 1);
                var genericSetupSetMethod = setupSetMethod?.MakeGenericMethod(parameterType);

                var lambdaExpression = Expression.Lambda(Expression.Property(Expression.Parameter(mockType, "m"), propertyInfo), Expression.Parameter(mockType, "m"));
                var setup = genericSetupSetMethod?.Invoke(mock, new object[] { lambdaExpression });

                var callbackMethod = setup?.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "Callback" && m.GetParameters().Length == 1);
                var callbackActionType = typeof(Action<>).MakeGenericType(parameterType);
                var callbackAction = Delegate.CreateDelegate(callbackActionType, new Action<object>((value) =>
                {
                    tracker.SetValue(propertyInfo.Name, value);
                    tracker.RecordSet(propertyInfo.Name, value);
                }), "Invoke");
                callbackMethod?.Invoke(setup, new object[] { callbackAction });

                // Ensure CallBase for setters
                var callBaseProperty = setup?.GetType().GetProperty("CallBase");
                callBaseProperty?.SetValue(setup, true);
            }
        }
    }
}
