//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Frozen;
using System.Reflection;

namespace MockingToolkit.PropertyTracking.Tests
{
    public class PropertyTrackingConfigurationTests
    {
        [Fact]
        public void Configuration_PropertiesAreImmutable()
        {
            var includedProperties = new HashSet<string> { "Prop1" };
            var excludedProperties = new HashSet<string> { "Prop2" };
            var includedFields = new HashSet<string> { "FieldA" };
            var excludedFields = new HashSet<string> { "FieldB" };
            var propertyInclusionPredicates = new List<Func<PropertyInfo, bool>> { p => true };
            var propertyExclusionPredicates = new List<Func<PropertyInfo, bool>> { p => false };
            var fieldInclusionPredicates = new List<Func<FieldInfo, bool>> { f => true };
            var fieldExclusionPredicates = new List<Func<FieldInfo, bool>> { f => false };
            const bool trackNonPublic = true;

            var config = new PropertyTrackingConfiguration(
                includedProperties.ToFrozenSet(),
                excludedProperties.ToFrozenSet(),
                includedFields.ToFrozenSet(),
                excludedFields.ToFrozenSet(),
                propertyInclusionPredicates,
                propertyExclusionPredicates,
                fieldInclusionPredicates,
                fieldExclusionPredicates,
                trackNonPublic);

            Assert.Equal(new HashSet<string> { "Prop1" }, config.IncludedProperties);
            Assert.Equal(new HashSet<string> { "Prop2" }, config.ExcludedProperties);
            Assert.Equal(new HashSet<string> { "FieldA" }, config.IncludedFields);
            Assert.Equal(new HashSet<string> { "FieldB" }, config.ExcludedFields);
            Assert.Equal(propertyInclusionPredicates, config.PropertyInclusionPredicates);
            Assert.Equal(propertyExclusionPredicates, config.PropertyExclusionPredicates);
            Assert.Equal(fieldInclusionPredicates, config.FieldInclusionPredicates);
            Assert.Equal(fieldExclusionPredicates, config.FieldExclusionPredicates);
            Assert.True(config.TrackNonPublicMembers);

            // Attempting to modify the original collections should not affect the configuration
            includedProperties.Add("Prop3");
            excludedProperties.Remove("Prop2");
            Assert.NotEqual(new HashSet<string> { "Prop1", "Prop3" }, config.IncludedProperties);
            Assert.Equal(new HashSet<string> { "Prop2" }, config.ExcludedProperties);
        }
    }
}
