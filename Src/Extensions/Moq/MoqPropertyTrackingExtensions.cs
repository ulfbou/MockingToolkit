//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

//using FluentAssertions;

//using Moq;

//using System.Diagnostics;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace MockingToolkit.Extensions.Moq
//{
//    public static class MoqPropertyTrackingExtensions
//    {
//        public static void TrackAllProperties<T>(this Mock<T> mock) where T : class
//        {
//            var tracker = new PropertyTracker();
//            mock.SetPropertyTracker(tracker);

//            var type = typeof(T);
//            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
//            {
//                var propName = prop.Name;
//                var propertyType = prop.PropertyType;

//                if (!prop.CanWrite && !prop.CanRead)
//                {
//                    Debug.WriteLine($"Property '{propName}' is neither readable nor writable. Skipping.");
//                    continue;
//                }

//                mock.SetupProperty(propName);

//                if (prop.CanWrite)
//                {
//                    var setupSetMethods = typeof(Mock<T>).GetMethods()
//                        .Where(m => m.Name == "SetupSet" &&
//                                    m.GetParameters().Length == 1 &&
//                                    m.GetParameters()[0].ParameterType.IsGenericType &&
//                                    m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

//                    Debug.WriteLine($"Found {setupSetMethods.Count()} SetupSet methods for property '{propName}' of type '{propertyType}'.");

//                    var setupSetMethod = setupSetMethods
//                        .FirstOrDefault(m => m.GetParameters()[0].ParameterType.IsGenericType &&
//                                             m.GetParameters()[0].ParameterType.GetGenericArguments().Length == 1 &&
//                                             m.GetParameters()[0].ParameterType.GetGenericArguments()[0] == propertyType)?
//                        .MakeGenericMethod(propertyType);

//                    if (setupSetMethod != null)
//                    {
//                        Type actionType = typeof(Action<>).MakeGenericType(propertyType);

//                        // Create a method that matches the delegate signature
//                        MethodInfo handlerMethod = typeof(MoqPropertyTrackingExtensions)
//                            .GetMethod(nameof(HandlePropertySet), BindingFlags.NonPublic | BindingFlags.Static)!
//                            .MakeGenericMethod(propertyType);

//                        Delegate action = Delegate.CreateDelegate(actionType, null, handlerMethod);

//                        setupSetMethod.Invoke(mock, new object[] { action });
//                    }
//                    else
//                    {
//                        Debug.WriteLine($"  Could not find a suitable SetupSet method for property '{propName}' of type '{propertyType}'.");
//                    }
//                }

//                if (prop.CanRead)
//                {
//                    // Use a direct property access expression for SetupGet
//                    var param = Expression.Parameter(typeof(T), "instance");
//                    var propertyAccess = Expression.Property(param, propName);

//                    // Create a lambda with the correct property type
//                    var lambda = Expression.Lambda(
//                        Expression.MakeMemberAccess(param, prop),
//                        param
//                    );

//                    // Use the property type explicitly in SetupGet
//                    var setupGetMethod = typeof(Mock<T>)
//                        .GetMethod(nameof(Mock<T>.SetupGet))
//                        ?.MakeGenericMethod(propertyType);

//                    if (setupGetMethod != null)
//                    {
//                        setupGetMethod.Invoke(mock, new object[] { lambda });
//                    }
//                    else
//                    {
//                        Debug.WriteLine($"Could not find SetupGet method for property '{propName}' of type '{propertyType}'.");
//                    }

//                    // Track property access separately
//                    tracker.AddHandler(propName, value => tracker.RecordGet(propName, value));
//                }
//            }
//        }

//        private static void HandlePropertySet<TProperty>(string propertyName, PropertyTracker tracker, TProperty value)
//        {
//            tracker.RecordSet(propertyName, value);
//            tracker.InvokeHandlers(propertyName, value);
//        }
//        private static Expression<Func<T, object>> BuildExpressionFunc<T>(string propertyName, Type propertyType, PropertyTracker tracker)
//        {
//            Debug.WriteLine($"Building expression for property '{propertyName}' of type '{propertyType}'.");
//            var param = Expression.Parameter(typeof(T), "instance");
//            var propertyAccess = Expression.Property(param, propertyName);
//            MethodInfo recordGetMethod = typeof(PropertyTracker).GetMethod("RecordGet", new[] { typeof(string), typeof(object) })!;

//            if (recordGetMethod == null)
//            {
//                Debug.WriteLine($"Error: RecordGet method not found on PropertyTracker with signature (string, object).");
//                return Expression.Lambda<Func<T, object>>(Expression.Default(typeof(object)), param); // Return a default expression to avoid NullReferenceException
//            }

//            var body = Expression.Convert(
//                Expression.Call(
//                    Expression.Constant(tracker),
//                    recordGetMethod,
//                    Expression.Constant(propertyName),
//                    Expression.Convert(propertyAccess, typeof(object))
//                ),
//                typeof(object)
//            );
//            return Expression.Lambda<Func<T, object>>(body, param);
//        }

//        private static Func<object?> BuildFunc(string propName, Type propertyType, PropertyTracker tracker)
//        {
//            return () =>
//            {
//                var lastValue = tracker.GetLastValue(propName);
//                if (lastValue != null)
//                {
//                    return lastValue;
//                }

//                return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
//            };
//        }

//        private static Expression<Action<T, T>> BuildExpressionAction<T>(string propName, PropertyTracker tracker)
//        {
//            var param = Expression.Parameter(typeof(T), "value");
//            var body = Expression.Call(Expression.Constant(tracker), typeof(PropertyTracker).GetMethod("RecordSet")!, Expression.Constant(propName), param);
//            return Expression.Lambda<Action<T, T>>(body, param);
//        }

//        private static Action<T> BuildAction<T>(string propName, PropertyTracker tracker)
//        {
//            return value =>
//            {
//                tracker.RecordSet(propName, value);
//                tracker.InvokeHandlers(propName, value);
//            };
//        }

//    }
//}
