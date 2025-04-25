//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

//using Moq;

//using System.Linq.Expressions;
//using System.Runtime.CompilerServices;

//namespace MockingToolkit.Extensions.Moq
//{
//    public static class MoqTrackingHelpers
//    {
//        internal static readonly ConditionalWeakTable<Mock, PropertyTracker> _trackers = new();

//        internal static void SetPropertyTracker<T>(this Mock<T> mock, PropertyTracker tracker) where T : class
//            => _trackers.Add(mock, tracker);

//        public static void TrackProperties<TMock>(this Mock<TMock> mock) where TMock : class
//        {
//            if (!_trackers.TryGetValue(mock, out var tracker))
//            {
//                tracker = new PropertyTracker();
//                mock.SetPropertyTracker(tracker);

//                // Automatically register all properties of the mock interface
//                foreach (var property in typeof(TMock).GetProperties())
//                {
//                    tracker.RegisterProperty(property.Name, property.PropertyType);
//                }
//            }
//        }

//        public static TProp? GetLastPropertyValue<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression)
//            where TMock : class => (TProp?)mock.GetTracker().GetLastValue(GetPropertyName(expression));

//        public static IReadOnlyList<TProp?> GetPropertyValueHistory<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression)
//            where TMock : class => mock.GetTracker().GetHistory(GetPropertyName(expression)).Cast<TProp?>().ToList();

//        public static int GetPropertySetCount<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression)
//            where TMock : class => mock.GetTracker().GetSetCount(GetPropertyName(expression));

//        public static void AssertPropertyWasSet<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression, TProp expected, int times)
//            where TMock : class
//        {
//            var history = mock.GetPropertyValueHistory(expression);
//            var matchCount = history.Count(v => EqualityComparer<TProp?>.Default.Equals(v, expected));
//            if (matchCount != times)
//                throw new InvalidOperationException($"Expected property '{GetPropertyName(expression)}' to be set to '{expected}' {times} times, but was {matchCount}.");
//        }

//        public static void AssertPropertyWasSetAny<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression)
//            where TMock : class
//        {
//            if (mock.GetPropertySetCount(expression) == 0)
//                throw new InvalidOperationException($"Property '{GetPropertyName(expression)}' was never set.");
//        }

//        public static void AssertPropertyWasNotSet<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression)
//            where TMock : class
//        {
//            if (mock.GetPropertySetCount(expression) > 0)
//                throw new InvalidOperationException($"Property '{GetPropertyName(expression)}' was set.");
//        }

//        public static void AssertPropertyValueHistoryContains<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression, TProp value)
//            where TMock : class
//        {
//            if (!mock.GetPropertyValueHistory(expression).Contains(value))
//                throw new InvalidOperationException($"Value '{value}' was not found in the history of '{GetPropertyName(expression)}'.");
//        }

//        public static void OnPropertySet<TMock, TProp>(this Mock<TMock> mock, Expression<Func<TMock, TProp>> expression, Action<TProp?> handler)
//            where TMock : class
//        {
//            mock.GetTracker().AddHandler(GetPropertyName(expression), v => handler((TProp?)v));
//        }

//        private static string GetPropertyName<TMock, TProp>(Expression<Func<TMock, TProp>> expression)
//        {
//            return (expression.Body as MemberExpression)?.Member.Name
//                ?? throw new ArgumentException("Invalid property expression.");
//        }

//        public static void SetupProperty<TMock>(this Mock<TMock> mock, string propName) where TMock : class
//        {
//            var prop = typeof(TMock).GetProperty(propName);
//            if (prop == null)
//                throw new ArgumentException($"Property '{propName}' not found on type '{typeof(TMock).Name}'.");

//            if (!_trackers.TryGetValue(mock, out var tracker))
//                throw new InvalidOperationException("Property tracking not initialized.");

//            tracker.RegisterProperty(propName, prop.PropertyType);
//        }
//#if false
//        public static void SetupGet<TMock>(Mock<TMock> mock, Expression<Func<TMock, object?>> func) where TMock : class
//        {
//            var propName = GetPropertyName(func);
//            var compiledFunc = func.Compile();
//            mock.Setup(m => compiledFunc(m)).Returns(() => mock.GetTracker().GetValue(propName));
//        }
//#endif
//    }
//}
