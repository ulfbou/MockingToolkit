//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

using MockingToolkit.PropertyTracking.Extensions;

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Base class for interceptors that track property and field access.
    /// </summary>
    public abstract class PropertyTrackingInterceptorBase
    {
        private readonly IReadOnlySet<string> _includedProperties;
        private readonly IReadOnlySet<string> _excludedProperties;
        private readonly IReadOnlySet<string> _includedFields;
        private readonly IReadOnlySet<string> _excludedFields;
        private readonly IReadOnlyList<Func<PropertyInfo, bool>> _propertyInclusionPredicates;
        private readonly IReadOnlyList<Func<PropertyInfo, bool>> _propertyExclusionPredicates;
        private readonly IReadOnlyList<Func<FieldInfo, bool>> _fieldInclusionPredicates;
        private readonly IReadOnlyList<Func<FieldInfo, bool>> _fieldExclusionPredicates;
        private readonly bool _trackNonPublicMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTrackingInterceptorBase"/> class.
        /// </summary>
        /// <param name="configuration">The property tracking configuration.</param>
        protected PropertyTrackingInterceptorBase(PropertyTrackingConfiguration configuration)
        {
            _includedProperties = configuration.IncludedProperties;
            _excludedProperties = configuration.ExcludedProperties;
            _includedFields = configuration.IncludedFields;
            _excludedFields = configuration.ExcludedFields;
            _propertyInclusionPredicates = configuration.PropertyInclusionPredicates;
            _propertyExclusionPredicates = configuration.PropertyExclusionPredicates;
            _fieldInclusionPredicates = configuration.FieldInclusionPredicates;
            _fieldExclusionPredicates = configuration.FieldExclusionPredicates;
            _trackNonPublicMembers = configuration.TrackNonPublicMembers;
        }

        /// <summary>
        /// Determines whether a specific property should be tracked based on the configuration.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the property being accessed.</param>
        /// <returns>True if the property should be tracked; otherwise, false.</returns>
        protected virtual bool ShouldTrack(PropertyInfo propertyInfo)
        {
            // 1. Check exclusion by name (takes highest precedence)
            if (_excludedProperties.Contains(propertyInfo.Name))
            {
                return false;
            }

            // 2. Check inclusion by name
            if (_includedProperties.Contains(propertyInfo.Name))
            {
                return true;
            }

            // 3. Evaluate exclusion predicates
            if (_propertyExclusionPredicates.Any(predicate => predicate(propertyInfo)))
            {
                return false;
            }

            // 4. Evaluate inclusion predicates
            if (_propertyInclusionPredicates.Any(predicate => predicate(propertyInfo)))
            {
                return true;
            }

            // 5. Default behavior for non-public members
            if (!propertyInfo.GetMethod?.IsPublic == true)
            {
                return _trackNonPublicMembers;
            }

            // 6. Default to not tracking if no explicit inclusion rule matches
            return false;
        }

        /// <summary>
        /// Determines whether a specific field should be tracked based on the configuration.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> of the field being accessed.</param>
        /// <returns>True if the field should be tracked; otherwise, false.</returns>
        protected virtual bool ShouldTrack(FieldInfo fieldInfo)
        {
            // 1. Check exclusion by name (takes highest precedence)
            if (_excludedFields.Contains(fieldInfo.Name))
            {
                return false;
            }

            // 2. Check inclusion by name
            if (_includedFields.Contains(fieldInfo.Name))
            {
                return true;
            }

            // 3. Evaluate exclusion predicates
            if (_fieldExclusionPredicates.Any(predicate => predicate(fieldInfo)))
            {
                return false;
            }

            // 4. Evaluate inclusion predicates
            if (_fieldInclusionPredicates.Any(predicate => predicate(fieldInfo)))
            {
                return true;
            }

            // 5. Default behavior for non-public members
            if (!fieldInfo.IsPublic)
            {
                return _trackNonPublicMembers;
            }

            // 6. Default to not tracking if no explicit inclusion rule matches
            return false;
        }

        /// <summary>
        /// Logic to execute before tracking an access to a property or field.
        /// </summary>
        /// <param name="memberName">The name of the property or field being accessed.</param>
        protected abstract void PreTrack(string memberName);
        /// <summary>
        /// Logic to execute after tracking an access to a property or field.
        /// </summary>
        /// <param name="memberName">The name of the property or field that was accessed.</param>
        protected abstract void PostTrack(string memberName);
    }
}
