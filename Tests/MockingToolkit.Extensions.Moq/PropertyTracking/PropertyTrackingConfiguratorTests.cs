//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using MockingToolkit.Extensions.Moq.PropertyTracking;

using System.Reflection;

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
            var options = configurator.Build();

            // Assert
            Assert.True(options.TrackAll);
        }

        [Fact]
        public void TrackProperties_StringArray_SetsIncludedProperties()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            string[] propertyNames = { "Name", "Age", "Name" }; // Include a duplicate

            // Act
            var options = configurator.IncludeProperties(propertyNames).Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.True(options.Predicate != null);
            Assert.True(options.Predicate(new PropertyInfoStub("Name")));
            Assert.True(options.Predicate(new PropertyInfoStub("Age")));
            Assert.False(options.Predicate(new PropertyInfoStub("OtherProperty")));
        }

        [Fact]
        public void TrackProperties_Predicate_SetsPredicateFunction()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.Name.StartsWith("Is");

            // Act
            var options = configurator.IncludeProperties(predicate).Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.NotNull(options.Predicate);
            Assert.True(options.Predicate(new PropertyInfoStub("IsEnabled")));
            Assert.False(options.Predicate(new PropertyInfoStub("Name")));
        }

        [Fact]
        public void ConfigurationMethods_CanBeChained()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.PropertyType == typeof(string);

            // Act
            var options = configurator
                .IncludeProperties("Id")
                .IncludeProperties(predicate)
                .WithAccess(PropertyAccessType.Get)
                .Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.NotNull(options.Predicate);
            Assert.True(options.Predicate(new PropertyInfoStub("Id")));
            Assert.True(options.Predicate(new PropertyInfoStub("Name", typeof(string))));
            Assert.False(options.Predicate(new PropertyInfoStub("Age", typeof(int))));
            Assert.Equal(PropertyAccessType.Get, options.AccessType);
        }

        [Fact]
        public void Build_WithNoConfiguration_ReturnsDefaultOptions()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();

            // Act
            var options = configurator.Build();

            // Assert
            Assert.True(options.TrackAll);
            Assert.Null(options.Predicate);
            Assert.Null(options.AccessType);
        }
    }
}
