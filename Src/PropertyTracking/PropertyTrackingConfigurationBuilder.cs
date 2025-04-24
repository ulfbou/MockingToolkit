//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Reflection;

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Provides a fluent API for building <see cref="PropertyTrackingConfiguration"/>.
    /// </summary>
    public class PropertyTrackingConfigurationBuilder
    {
        private readonly HashSet<string> _includedProperties = new HashSet<string>();
        private readonly HashSet<string> _excludedProperties = new HashSet<string>();
        private readonly HashSet<string> _includedFields = new HashSet<string>();
        private readonly HashSet<string> _excludedFields = new HashSet<string>();
        private readonly List<Func<PropertyInfo, bool>> _propertyInclusionPredicates = new List<Func<PropertyInfo, bool>>();
        private readonly List<Func<PropertyInfo, bool>> _propertyExclusionPredicates = new List<Func<PropertyInfo, bool>>();
        private readonly List<Func<FieldInfo, bool>> _fieldInclusionPredicates = new List<Func<FieldInfo, bool>>();
        private readonly List<Func<FieldInfo, bool>> _fieldExclusionPredicates = new List<Func<FieldInfo, bool>>();
        private bool _trackNonPublicMembers = false;

        /// <summary>
        /// Includes the specified property names for tracking.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to include.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder IncludeProperties(params string[] propertyNames)
        {
            foreach (var name in propertyNames ?? [])
            {
                _includedProperties.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Includes properties for tracking based on the provided predicate.
        /// </summary>
        /// <param name="predicate">A function to test each property for inclusion.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder IncludeProperties(Func<PropertyInfo, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            _propertyInclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Excludes the specified property names from tracking.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to exclude.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder ExcludeProperties(params string[] propertyNames)
        {
            foreach (var name in propertyNames ?? [])
            {
                _excludedProperties.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Excludes properties from tracking based on the provided predicate.
        /// </summary>
        /// <param name="predicate">A function to test each property for exclusion.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder ExcludeProperties(Func<PropertyInfo, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            _propertyExclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Includes the specified field names for tracking.
        /// </summary>
        /// <param name="fieldNames">The names of the fields to include.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder IncludeFields(params string[] fieldNames)
        {
            foreach (var name in fieldNames ?? [])
            {
                _includedFields.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Includes fields for tracking based on the provided predicate.
        /// </summary>
        /// <param name="predicate">A function to test each field for inclusion.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder IncludeFields(Func<FieldInfo, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            _fieldInclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Excludes the specified field names from tracking.
        /// </summary>
        /// <param name="fieldNames">The names of the fields to exclude.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder ExcludeFields(params string[] fieldNames)
        {
            foreach (var name in fieldNames ?? [])
            {
                _excludedFields.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Excludes fields from tracking based on the provided predicate.
        /// </summary>
        /// <param name="predicate">A function to test each field for exclusion.</param>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder ExcludeFields(Func<FieldInfo, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            _fieldExclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Enables tracking of non-public properties and fields.
        /// </summary>
        /// <returns>The current <see cref="PropertyTrackingConfigurationBuilder"/> instance.</returns>
        public PropertyTrackingConfigurationBuilder TrackNonPublicMembers()
        {
            _trackNonPublicMembers = true;
            return this;
        }

        /// <summary>
        /// Validates the current configuration for any conflicting rules.
        /// </summary>
        /// <exception cref="InvalidTrackingConfigurationException">Thrown if conflicting rules are found.</exception>
        private void ValidateConfiguration()
        {
            var overlappingIncludedExcludedProperties = _includedProperties.Intersect(_excludedProperties);
            if (overlappingIncludedExcludedProperties.Any())
            {
                throw new InvalidTrackingConfigurationException(
                    $"The following properties are both included and excluded: {string.Join(", ", overlappingIncludedExcludedProperties)}");
            }

            var overlappingIncludedExcludedFields = _includedFields.Intersect(_excludedFields);
            if (overlappingIncludedExcludedFields.Any())
            {
                throw new InvalidTrackingConfigurationException(
                    $"The following fields are both included and excluded: {string.Join(", ", overlappingIncludedExcludedFields)}");
            }
        }

        /// <summary>
        /// Builds the <see cref="PropertyTrackingConfiguration"/> based on the current builder state.
        /// </summary>
        /// <returns>A new instance of <see cref="PropertyTrackingConfiguration"/>.</returns>
        /// <exception cref="InvalidTrackingConfigurationException">Thrown if the configuration is invalid.</exception>
        public PropertyTrackingConfiguration Build()
        {
            ValidateConfiguration();
            return new PropertyTrackingConfiguration(
                _includedProperties,
                _excludedProperties,
                _includedFields,
                _excludedFields,
                _propertyInclusionPredicates,
                _propertyExclusionPredicates,
                _fieldInclusionPredicates,
                _fieldExclusionPredicates,
                _trackNonPublicMembers);
        }
    }
}
