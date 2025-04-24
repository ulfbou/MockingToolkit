//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

using MockingToolkit.PropertyTracking;

using Xunit;

namespace MockingToolkit.PropertyTracking.Tests
{
    public class PropertyTrackingConfigurationBuilderTests
    {
        [Fact]
        public void IncludeProperties_AddsToIncludedSet()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            builder.IncludeProperties("Property1", "Property2");
            var config = builder.Build();
            Assert.Contains("Property1", config.IncludedProperties);
            Assert.Contains("Property2", config.IncludedProperties);
            Assert.DoesNotContain(string.Empty, config.IncludedProperties);
        }

        [Fact]
        public void IncludeProperties_WithPredicate_AddsToPredicateList()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            Func<PropertyInfo, bool> predicate = p => p.Name.StartsWith("Test");
            builder.IncludeProperties(predicate);
            var config = builder.Build();
            Assert.Contains(predicate, config.PropertyInclusionPredicates);
        }

        [Fact]
        public void ExcludeProperties_AddsToExcludedSet()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            builder.ExcludeProperties("PropertyA", "PropertyB");
            var config = builder.Build();
            Assert.Contains("PropertyA", config.ExcludedProperties);
            Assert.Contains("PropertyB", config.ExcludedProperties);
            Assert.DoesNotContain(string.Empty, config.ExcludedProperties);
        }

        [Fact]
        public void ExcludeProperties_WithPredicate_AddsToPredicateList()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            Func<PropertyInfo, bool> predicate = p => p.Name.EndsWith("Prop");
            builder.ExcludeProperties(predicate);
            var config = builder.Build();
            Assert.Contains(predicate, config.PropertyExclusionPredicates);
        }

        [Fact]
        public void IncludeFields_AddsToIncludedSet()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            builder.IncludeFields("field1", "field2");
            var config = builder.Build();
            Assert.Contains("field1", config.IncludedFields);
            Assert.Contains("field2", config.IncludedFields);
            Assert.DoesNotContain(string.Empty, config.IncludedFields);
        }

        [Fact]
        public void IncludeFields_WithPredicate_AddsToPredicateList()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            Func<FieldInfo, bool> predicate = f => f.Name.StartsWith("_");
            builder.IncludeFields(predicate);
            var config = builder.Build();
            Assert.Contains(predicate, config.FieldInclusionPredicates);
        }

        [Fact]
        public void ExcludeFields_AddsToExcludedSet()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            builder.ExcludeFields("fieldA", "fieldB");
            var config = builder.Build();
            Assert.Contains("fieldA", config.ExcludedFields);
            Assert.Contains("fieldB", config.ExcludedFields);
            Assert.DoesNotContain(string.Empty, config.ExcludedFields);
        }

        [Fact]
        public void ExcludeFields_WithPredicate_AddsToPredicateList()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            Func<FieldInfo, bool> predicate = f => f.Name.Contains("temp");
            builder.ExcludeFields(predicate);
            var config = builder.Build();
            Assert.Contains(predicate, config.FieldExclusionPredicates);
        }

        [Fact]
        public void TrackNonPublicMembers_SetsFlagToTrue()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            builder.TrackNonPublicMembers();
            var config = builder.Build();
            Assert.True(config.TrackNonPublicMembers);
        }

        [Fact]
        public void Build_WithoutRules_CreatesEmptyConfiguration()
        {
            var builder = new PropertyTrackingConfigurationBuilder();
            var config = builder.Build();
            Assert.Empty(config.IncludedProperties);
            Assert.Empty(config.ExcludedProperties);
            Assert.Empty(config.IncludedFields);
            Assert.Empty(config.ExcludedFields);
            Assert.Empty(config.PropertyInclusionPredicates);
            Assert.Empty(config.PropertyExclusionPredicates);
            Assert.Empty(config.FieldInclusionPredicates);
            Assert.Empty(config.FieldExclusionPredicates);
            Assert.False(config.TrackNonPublicMembers);
        }

        [Fact]
        public void Build_DetectsConflictingPropertyRules_ThrowsException()
        {
            var builder = new PropertyTrackingConfigurationBuilder()
                .IncludeProperties("CommonProperty")
                .ExcludeProperties("CommonProperty");

            Assert.Throws<InvalidTrackingConfigurationException>(() => builder.Build());
        }

        [Fact]
        public void Build_DetectsConflictingFieldRules_ThrowsException()
        {
            var builder = new PropertyTrackingConfigurationBuilder()
                .IncludeFields("commonField")
                .ExcludeFields("commonField");

            Assert.Throws<InvalidTrackingConfigurationException>(() => builder.Build());
        }
    }
}
