// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MockingToolkit.Extensions.Moq.PropertyTracking;
using Xunit;
using System;
using System.Collections.Generic;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    public class PropertyTrackerPropertyNameTests
    {
        [Fact]
        public void RegisterProperty_EmptyName_DoesNotThrow()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "";
            Type propertyType = typeof(int);

            // Act
            var exception = Record.Exception(() => tracker.RegisterProperty(propertyName, propertyType));

            // Assert
            Assert.Null(exception); // No exception should be thrown
            Assert.Equal(0, tracker.GetValue<int>(propertyName));
        }

        [Fact]
        public void GetValue_EmptyName_ExistingProperty_ReturnsValue()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "";
            Type propertyType = typeof(string);
            string initialValue = "Empty Name Property";
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialValue);

            // Act
            string retrievedValue = tracker.GetValue<string>(propertyName);

            // Assert
            Assert.Equal(initialValue, retrievedValue);
        }

        [Fact]
        public void RegisterProperty_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string? propertyName = null;
            Type propertyType = typeof(int);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.RegisterProperty(propertyName!, propertyType));
        }

        [Fact]
        public void GetValue_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "SomeProperty";
            tracker.RegisterProperty(propertyName, typeof(int));

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.GetValue(null!));
        }

        [Fact]
        public void SetValue_NullName_ThrowsAArgumentNullException()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "SomeProperty";
            tracker.RegisterProperty(propertyName, typeof(int));

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.SetValue(null!, 5));
        }

        [Fact]
        public void RegisterProperty_WhitespaceName_DoesNotThrow()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "   ";
            Type propertyType = typeof(bool);

            // Act
            var exception = Record.Exception(() => tracker.RegisterProperty(propertyName, propertyType));

            // Assert
            Assert.Null(exception); // No exception should be thrown
            Assert.False(tracker.GetValue<bool>(propertyName));
        }

        [Fact]
        public void RegisterProperty_SpecialCharactersName_WorksCorrectly()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "user-id.123[index]";
            Type propertyType = typeof(string);
            string initialValue = "Special Name Value";

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialValue);
            string retrievedValue = tracker.GetValue<string>(propertyName);

            // Assert
            Assert.Equal(initialValue, retrievedValue);
        }

        [Fact]
        public void GetValue_CaseSensitiveName_ReturnsKeyNotFoundException()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string registeredName = "PropertyName";
            Type propertyType = typeof(int);
            tracker.RegisterProperty(registeredName, propertyType);
            tracker.SetValue(registeredName, 5);
            string differentCaseName = "propertyname";

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => tracker.GetValue(differentCaseName));
        }
    }
}
