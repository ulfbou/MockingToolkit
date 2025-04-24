//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Represents the immutable configuration for property tracking.
    /// </summary>
    public class PropertyTrackingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTrackingConfiguration"/> class.
        /// </summary>
        /// <param name="includedProperties">The set of property names to include for tracking.</param>
        /// <param name="excludedProperties">The set of property names to exclude from tracking.</param>
        /// <param name="includedFields">The set of field names to include for tracking.</param>
        /// <param name="excludedFields">The set of field names to exclude from tracking.</param>
        /// <param name="propertyInclusionPredicates">The collection of predicates to include properties for tracking.</param>
        /// <param name="propertyExclusionPredicates">The collection of predicates to exclude properties from tracking.</param>
        /// <param name="fieldInclusionPredicates">The collection of predicates to include fields for tracking.</param>
        /// <param name="fieldExclusionPredicates">The collection of predicates to exclude fields from tracking.</param>
        /// <param name="trackNonPublicMembers">A value indicating whether non-public members should be tracked.</param>
        internal PropertyTrackingConfiguration(
            IReadOnlySet<string> includedProperties,
            IReadOnlySet<string> excludedProperties,
            IReadOnlySet<string> includedFields,
            IReadOnlySet<string> excludedFields,
            IReadOnlyList<Func<PropertyInfo, bool>> propertyInclusionPredicates,
            IReadOnlyList<Func<PropertyInfo, bool>> propertyExclusionPredicates,
            IReadOnlyList<Func<FieldInfo, bool>> fieldInclusionPredicates,
            IReadOnlyList<Func<FieldInfo, bool>> fieldExclusionPredicates,
            bool trackNonPublicMembers)
        {
            IncludedProperties = (includedProperties ?? new HashSet<string>()).ToFrozenSet();
            ExcludedProperties = (excludedProperties ?? new HashSet<string>()).ToFrozenSet();
            IncludedFields = (includedFields ?? new HashSet<string>()).ToFrozenSet();
            ExcludedFields = (excludedFields ?? new HashSet<string>()).ToFrozenSet();
            PropertyInclusionPredicates = (propertyInclusionPredicates ?? new List<Func<PropertyInfo, bool>>()).ToImmutableList();
            PropertyExclusionPredicates = (propertyExclusionPredicates ?? new List<Func<PropertyInfo, bool>>()).ToImmutableList();
            FieldInclusionPredicates = (fieldInclusionPredicates ?? new List<Func<FieldInfo, bool>>()).ToImmutableList();
            FieldExclusionPredicates = (fieldExclusionPredicates ?? new List<Func<FieldInfo, bool>>()).ToImmutableList();
            TrackNonPublicMembers = trackNonPublicMembers;
        }

        /// <summary>
        /// Gets the set of property names to include for tracking.
        /// </summary>
        public IReadOnlySet<string> IncludedProperties { get; }

        /// <summary>
        /// Gets the set of property names to exclude from tracking.
        /// </summary>
        public IReadOnlySet<string> ExcludedProperties { get; }

        /// <summary>
        /// Gets the set of field names to include for tracking.
        /// </summary>
        public IReadOnlySet<string> IncludedFields { get; }

        /// <summary>
        /// Gets the set of field names to exclude from tracking.
        /// </summary>
        public IReadOnlySet<string> ExcludedFields { get; }

        /// <summary>
        /// Gets the collection of predicates to include properties for tracking.
        /// </summary>
        public IReadOnlyList<Func<PropertyInfo, bool>> PropertyInclusionPredicates { get; }

        /// <summary>
        /// Gets the collection of predicates to exclude properties from tracking.
        /// </summary>
        public IReadOnlyList<Func<PropertyInfo, bool>> PropertyExclusionPredicates { get; }

        /// <summary>
        /// Gets the collection of predicates to include fields for tracking.
        /// </summary>
        public IReadOnlyList<Func<FieldInfo, bool>> FieldInclusionPredicates { get; }

        /// <summary>
        /// Gets the collection of predicates to exclude fields from tracking.
        /// </summary>
        public IReadOnlyList<Func<FieldInfo, bool>> FieldExclusionPredicates { get; }

        /// <summary>
        /// Gets a value indicating whether non-public members should be tracked.
        /// </summary>
        public bool TrackNonPublicMembers { get; }
    }
}
