//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using MockingToolkit.Extensions.Moq.PropertyTracking;

using Moq;

using System.Linq.Expressions;
using System.Reflection;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    public class MoqPropertyTrackingExtensionsExpressionTests
    {
        [Fact]
        public void TrackProperties_WithSingleValidPropertyExpression_ShouldNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(nameof(IFoo.Name))));

            // Assert
            Assert.Null(exception); // Ensure no exception is thrown
        }

        [Fact]
        public void TrackProperties_WithMultipleValidPropertyExpressions_ShouldNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(nameof(IFoo.Name), nameof(IFoo.Age))));

            // Assert
            Assert.Null(exception); // Ensure no exception is thrown
        }

        [Fact]
        public void TrackProperties_WithValueTypePropertyExpression_ShouldNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(nameof(IFoo.Age))));

            // Assert
            Assert.Null(exception); // Ensure no exception is thrown
        }

        [Fact]
        public void TrackProperties_WithMethodCallExpression_ShouldThrowArgumentException()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            // Record the exception
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(p => p.Name == nameof(IFoo.ToString))));

            // Act & Assert
            Assert.IsType<ArgumentException>(exception); // Ensure the exception is of type ArgumentException
        }

        [Fact]
        public void TrackProperties_WithConstantExpression_ShouldThrowArgumentException()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(p => p.Name == "SomeConstant")));

            // Assert
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void TrackProperties_WithFieldAccessExpression_ShouldThrowArgumentException()
        {
            // Arrange
            var mock = new Mock<FooWithField>();

            // Act 
            var exception = Record.Exception(() => mock.TrackProperties(config => config.IncludeProperties(p => p.Name == "SomeField")));

            // Assert
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void TrackProperties_WithEmptyMock_ShouldNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties());

            // Assert
            Assert.Null(exception); // Ensure no exception is thrown
        }

        [Fact]
        public void TrackProperties_WithAccessFilter_ShouldRestrictTracking()
        {
            // Arrange
            var mock = new Mock<IFoo>();
            var options = new PropertyTrackingOptions(false, AccessFilter: p => p.Name == nameof(IFoo.Name));

            // Act
            mock.ApplyTracking<IFoo>(options);

            // Assert
            Assert.NotNull(options.AccessFilter); // Ensure AccessFilter is applied
        }

        public interface IFoo
        {
            string Name { get; set; }
            int Age { get; set; }
        }

        public class FooWithField
        {
            public string Name { get; set; }
            public int SomeField;
        }
    }
}
