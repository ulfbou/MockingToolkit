public class MockingFrameworkAdapter { }

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Runtime.CompilerServices;

//using Moq;

//namespace MockingToolkit.Extensions.Moq
//{

//    /// <summary>
//    /// The unified mocking framework adapter interface.
//    /// </summary>
//public interface IMockingFrameworkAdapter
//    {
//        /// <summary>
//        /// Creates a mock instance for the specified type.
//        /// Optionally enables property tracking.
//        /// </summary>
//        Mock<T> CreateMock<T>(bool enablePropertyTracking = false) where T : class;

//        /// <summary>
//        /// Retrieves the property tracker associated with a mock instance.
//        /// Throws an exception if tracking is not enabled.
//        /// </summary>
//        PropertyTracker2 GetPropertyTracker(object mockObject);
//    }

//    /// <summary>
//    /// The Moq adapter implementation that integrates property tracking.
//    /// </summary>
//    public class MoqAdapter : IMockingFrameworkAdapter
//    {
//        // Associates a property tracker with a mock instance using a weak reference mechanism.
//        private static readonly ConditionalWeakTable<object, PropertyTracker2> TrackerMap = new ConditionalWeakTable<object, PropertyTracker2>();

//        /// <summary>
//        /// Creates a Moq mock instance. When property tracking is enabled, the adapter attaches
//        /// tracking callbacks to all public instance properties.
//        /// </summary>
//        public Mock<T> CreateMock<T>(bool enablePropertyTracking = false) where T : class
//        {
//            var mock = new Mock<T>();
//            if (enablePropertyTracking)
//            {
//                AttachPropertyTracking(mock);
//            }
//            return mock;
//        }

//        /// <summary>
//        /// Attaches property tracking for every public instance property on the mock.
//        /// Uses reflection to register properties and dynamically builds expression trees for
//        /// callback interception on both get and set.
//        /// </summary>
//        private void AttachPropertyTracking<T>(Mock<T> mock) where T : class
//        {
//            var tracker = new PropertyTracker2();
//            TrackerMap.Add(mock.Object, tracker);

//            var type = typeof(T);
//            // Consider all public instance properties.
//            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

//            foreach (var prop in properties)
//            {
//                // Skip properties that have no accessible accessor.
//                if (!prop.CanRead && !prop.CanWrite)
//                    continue;

//                // Register the property in the tracker (initialize backing field).
//                tracker.RegisterProperty(prop.Name, prop.PropertyType);

//                // ----- Set up get tracking if the property is readable -----
//                if (prop.CanRead)
//                {
//                    // Build Lambda: (T x) => x.Prop
//                    var parameter = Expression.Parameter(typeof(T), "x");
//                    var propertyAccess = Expression.Property(parameter, prop);
//                    var lambdaType = typeof(Func<,>).MakeGenericType(typeof(T), prop.PropertyType);
//                    var lambda = Expression.Lambda(lambdaType, propertyAccess, parameter);

//                    // Call Moq’s SetupGet<TProperty> (invoked via reflection).
//                    var setupGetMethod = typeof(Mock<T>).GetMethods()
//                        .Where(m => m.Name == "SetupGet" && m.IsGenericMethod && m.GetParameters().Length == 1)
//                        .First()
//                        .MakeGenericMethod(prop.PropertyType);

//                    // Build the getter delegate:
//                    // () => { var value = tracker.GetValue<TProperty>(propName); tracker.RecordGet(propName, value); return value; }
//                    Delegate getterDelegate = CreateGetterDelegate(prop.PropertyType, tracker, prop.Name);

//                    // Invoke SetupGet and then chain Returns(getterDelegate).
//                    var setupGetter = setupGetMethod.Invoke(mock, new object[] { lambda });
//                    var returnsMethod = setupGetter.GetType().GetMethod("Returns", new Type[] { typeof(Func<>).MakeGenericType(prop.PropertyType) });
//                    returnsMethod.Invoke(setupGetter, new object[] { getterDelegate });
//                }

//                // ----- Set up set tracking if the property is writable -----
//                if (prop.CanWrite)
//                {
//                    // Build Lambda: (T x) => x.Prop = default(TProperty)
//                    var parameter = Expression.Parameter(typeof(T), "x");
//                    var propertyAccess = Expression.Property(parameter, prop);
//                    var defaultValue = Expression.Default(prop.PropertyType);
//                    var assignment = Expression.Assign(propertyAccess, defaultValue);
//                    var lambda = Expression.Lambda(assignment, parameter);

//                    // Call Moq’s SetupSet to register the setter behavior.
//                    var setupSetMethod = typeof(Mock<T>).GetMethod("SetupSet", new Type[] { lambda.GetType() });
//                    var setupSetter = setupSetMethod.Invoke(mock, new object[] { lambda });

//                    // Build the setter delegate:
//                    // (TProperty val) => { tracker.SetValue(propName, val); tracker.RecordSet(propName, val); }
//                    Delegate setterDelegate = CreateSetterDelegate(prop.PropertyType, tracker, prop.Name);

//                    // Use Moq’s Callback extension on the setup to attach our setter delegate.
//                    var callbackMethod = setupSetter.GetType().GetMethods().First(m => m.Name == "Callback" && m.GetParameters().Length == 1);
//                    callbackMethod = callbackMethod.MakeGenericMethod(prop.PropertyType);
//                    callbackMethod.Invoke(setupSetter, new object[] { setterDelegate });
//                }
//            }
//        }

//        /// <summary>
//        /// Dynamically creates a getter delegate of type Func[TProperty]
//        /// that retrieves the backing field, records the access, and returns the value.
//        /// </summary>
//        private Delegate CreateGetterDelegate(Type propertyType, PropertyTracker2 tracker, string propertyName)
//        {
//            // Build expression for: tracker.GetValue<TProperty>(propertyName)
//            var methodGetValue = typeof(PropertyTracker2).GetMethod("GetValue", new Type[] { typeof(string) })
//                                                        .MakeGenericMethod(propertyType);
//            var trackerExpr = Expression.Constant(tracker);
//            var propertyNameExpr = Expression.Constant(propertyName);
//            var getValueCall = Expression.Call(trackerExpr, methodGetValue, propertyNameExpr);

//            // Store the result in a variable.
//            var valueVar = Expression.Variable(propertyType, "value");
//            var assignValue = Expression.Assign(valueVar, getValueCall);

//            // Build expression for: tracker.RecordGet(propertyName, value)
//            var recordGetMethod = typeof(PropertyTracker2).GetMethod("RecordGet");
//            var recordCall = Expression.Call(trackerExpr, recordGetMethod, propertyNameExpr, Expression.Convert(valueVar, typeof(object)));

//            // Create block: { var value = tracker.GetValue(...); tracker.RecordGet(...); return value; }
//            var block = Expression.Block(new[] { valueVar }, assignValue, recordCall, valueVar);
//            var delegateType = typeof(Func<>).MakeGenericType(propertyType);
//            return Expression.Lambda(delegateType, block).Compile();
//        }

//        /// <summary>
//        /// Dynamically creates a setter delegate of type Action[TProperty]
//        /// that updates the backing field and records the set event.
//        /// </summary>
//        private Delegate CreateSetterDelegate(Type propertyType, PropertyTracker2 tracker, string propertyName)
//        {
//            // Parameter: (TProperty val)
//            var valParam = Expression.Parameter(propertyType, "val");
//            var trackerExpr = Expression.Constant(tracker);
//            var propertyNameExpr = Expression.Constant(propertyName);

//            // Build expression for: tracker.SetValue(propertyName, val)
//            var setValueMethod = typeof(PropertyTracker2).GetMethod("SetValue");
//            var setValueCall = Expression.Call(trackerExpr, setValueMethod, propertyNameExpr, Expression.Convert(valParam, typeof(object)));

//            // Build expression for: tracker.RecordSet(propertyName, val)
//            var recordSetMethod = typeof(PropertyTracker2).GetMethod("RecordSet");
//            var recordCall = Expression.Call(trackerExpr, recordSetMethod, propertyNameExpr, Expression.Convert(valParam, typeof(object)));

//            // Create block: { tracker.SetValue(...); tracker.RecordSet(...); }
//            var block = Expression.Block(setValueCall, recordCall);
//            var delegateType = typeof(Action<>).MakeGenericType(propertyType);
//            return Expression.Lambda(delegateType, block, valParam).Compile();
//        }

//        /// <summary>
//        /// Retrieves the property tracker for the provided mock object.
//        /// An exception is thrown if property tracking was not enabled.
//        /// </summary>
//        public PropertyTracker2 GetPropertyTracker(object mockObject)
//        {
//            if (TrackerMap.TryGetValue(mockObject, out var tracker))
//                return tracker;

//            throw new InvalidOperationException("Property tracking not enabled on this mock instance.");
//        }
//    }
//}