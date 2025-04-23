//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using MockingToolkit.Extensions.Moq.PropertyTracking;

using Moq;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    public class MoqPropertyTrackingExtensionsExpressionTests
    {
        [Fact]
        public void TrackProperties_SingleValidPropertyExpression_DoesNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(m => m.Name));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TrackProperties_MultipleValidPropertyExpressions_DoesNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(m => m.Name, m => m.Age));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TrackProperties_ValueTypePropertyExpression_DoesNotThrow()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act
            var exception = Record.Exception(() => mock.TrackProperties(m => m.Age));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TrackProperties_InvalidExpression_MethodCall_ThrowsInvalidOperationException()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => mock.TrackProperties(m => m.ToString()));
        }

        [Fact]
        public void TrackProperties_InvalidExpression_Constant_ThrowsInvalidOperationException()
        {
            // Arrange
            var mock = new Mock<IFoo>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => mock.TrackProperties(m => "Constant"));
        }

        [Fact]
        public void TrackProperties_InvalidExpression_FieldAccess_ThrowsInvalidOperationException()
        {
            // Arrange
            var mock = new Mock<FooWithField>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => mock.TrackProperties(m => m.SomeField));
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
