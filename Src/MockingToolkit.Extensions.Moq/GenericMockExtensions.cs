// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

using Moq;

using System.Linq.Expressions;
using System.Reflection;

namespace MockingToolkit.Extensions.Moq
{
    public static class GenericMockExtensions
    {
        private static readonly Dictionary<Mock, Dictionary<string, object>> _mockPropertyValues =
                    new Dictionary<Mock, Dictionary<string, object>>(new MockEqualityComparer());

        private static readonly Dictionary<Type, object> _defaultValueCache = new Dictionary<Type, object>();


        /// <summary>
        /// Configures the mock to return the default value (Empty) for all non-void methods.
        /// </summary>
        /// <typeparam name="TMock">The type of the mocked object.</typeparam>
        /// <param name="mock">The mock instance.</param>
        public static void ReturnsDefaultValue<TMock>(this Mock<TMock> mock) where TMock : class
        {
            if (mock == null)
            {
                throw new ArgumentNullException(nameof(mock));
            }

            mock.DefaultValue = DefaultValue.Empty;
        }

        /// <summary>
        /// Gets the last value that was set on a specific property of the mocked object instance.
        /// </summary>
        /// <typeparam name="TMock">The type of the mocked object.</typeparam>
        /// <param name="mock">The mock instance.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The last set value, or null if the property was never set or does not exist for this mock.</returns>
        public static object? GetPropertyValue<TMock>(this Mock<TMock> mock, string propertyName) where TMock : class
        {
            if (_mockPropertyValues.TryGetValue(mock, out var instanceValues) && instanceValues.TryGetValue(propertyName, out var value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// Clears the stored property values for all mock instances. Useful between test cases.
        /// </summary>
        public static void ClearPropertyValues()
        {
            _mockPropertyValues.Clear();
        }

        /// <summary>
        /// Extension method to set up a callback for a specific method on the mock.
        /// </summary>
        public static void SetupCallback<TMock>(this Mock<TMock> mock, Expression<Action<TMock>> expression, Action callback) where TMock : class
        {
            mock.Setup(expression).Callback(callback);
        }

        /// <summary>
        /// Extension method to set up a callback for a specific method on the mock with parameters.
        /// </summary>
        public static void SetupCallback<TMock, TResult>(this Mock<TMock> mock, Expression<Func<TMock, TResult>> expression, Action callback) where TMock : class
        {
            mock.Setup(expression).Callback(callback);
        }

        /// <summary>
        /// Extension method to set up a callback for a specific method on the mock with parameters and return value.
        /// </summary>
        public static void SetupCallback<TMock, TResult>(this Mock<TMock> mock, Expression<Func<TMock, TResult>> expression, Func<TResult> callback) where TMock : class
        {
            mock.Setup(expression).Returns(callback);
        }

        private static object GetDefaultValue(Type type)
        {
            if (!_defaultValueCache.TryGetValue(type, out var defaultValue))
            {
                defaultValue = Activator.CreateInstance(type)!;
                _defaultValueCache[type] = defaultValue;
            }
            return defaultValue;
        }

        private class MockEqualityComparer : IEqualityComparer<Mock>
        {
            public bool Equals(Mock? x, Mock? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return ReferenceEquals(x.Object, y.Object); // Compare based on the mocked object instance
            }

            public int GetHashCode(Mock mock)
            {
                return mock.Object.GetHashCode();
            }
        }
    }
}
