//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

using MockingToolkit.Extensions.Moq.PropertyTracking;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    public class PropertyTrackingConfiguratorTests
    {
        [Fact]
        public void TrackAll_SetsTrackAllOptionToTrue()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();

            // Act
            var options = configurator.TrackAll().Build();

            // Assert
            Assert.True(options.TrackAll);
        }

        [Fact]
        public void TrackProperties_StringArray_SetsTrackedPropertyNames()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            string[] propertyNames = { "Name", "Age", "Name" }; // Include a duplicate

            // Act
            var options = configurator.TrackProperties(propertyNames).Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.Equal(2, options.TrackedPropertyNames.Count);
            Assert.Contains("Name", options.TrackedPropertyNames);
            Assert.Contains("Age", options.TrackedPropertyNames);
        }

        [Fact]
        public void TrackProperties_Predicate_SetsPredicateFunction()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.Name.StartsWith("Is");

            // Act
            var options = configurator.TrackProperties(predicate).Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.NotNull(options.Predicate);
            Assert.Same(predicate, options.Predicate); // Ensure it's the same instance
        }

        [Fact]
        public void ConfigurationMethods_CanBeChained()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.PropertyType == typeof(string);

            // Act
            var options = configurator
                .TrackAll()
                .TrackProperties("Id")
                .TrackProperties(predicate)
                .Build();

            // Assert
            Assert.True(options.TrackAll); // TrackAll should take precedence based on the current design
            Assert.Contains("Id", options.TrackedPropertyNames);
            Assert.Same(predicate, options.Predicate);
        }

        [Fact]
        public void Build_WithNoConfiguration_ReturnsDefaultOptions()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();

            // Act
            var options = configurator.Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.Empty(options.TrackedPropertyNames);
            Assert.Null(options.Predicate);
        }
    }
}