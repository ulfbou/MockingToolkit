//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

using MockingToolkit.PropertyTracking.Extensions;

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Provides an abstract base class for intercepting property and field access.
    /// </summary>
    public abstract class PropertyTrackingInterceptorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTrackingInterceptorBase"/> class.
        /// </summary>
        /// <param name="configuration">The property tracking configuration.</param>
        protected PropertyTrackingInterceptorBase(PropertyTrackingConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the property tracking configuration.
        /// </summary>
        protected PropertyTrackingConfiguration Configuration { get; }

        /// <summary>
        /// Determines whether a specific property should be tracked based on the configuration.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the property being accessed.</param>
        /// <returns>True if the property should be tracked; otherwise, false.</returns>
        protected virtual bool ShouldTrack(PropertyInfo propertyInfo)
        {
            if (!Configuration.TrackNonPublicMembers && !propertyInfo.IsPublic())
            {
                return false;
            }

            if (Configuration.ExcludedProperties.Contains(propertyInfo.Name))
            {
                return false;
            }

            if (Configuration.PropertyExclusionPredicates.Any(predicate => predicate(propertyInfo)))
            {
                return false;
            }

            if (Configuration.IncludedProperties.Contains(propertyInfo.Name))
            {
                return true;
            }

            if (Configuration.PropertyInclusionPredicates.Any(predicate => predicate(propertyInfo)))
            {
                return true;
            }

            // If no explicit include rules, and no exclude rules matched, default to not tracking
            return Configuration.IncludedProperties.Any() || Configuration.PropertyInclusionPredicates.Any();
        }

        /// <summary>
        /// Determines whether a specific field should be tracked based on the configuration.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> of the field being accessed.</param>
        /// <returns>True if the field should be tracked; otherwise, false.</returns>
        protected virtual bool ShouldTrack(FieldInfo fieldInfo)
        {
            if (!Configuration.TrackNonPublicMembers && !fieldInfo.IsPublic)
            {
                return false;
            }

            if (Configuration.ExcludedFields.Contains(fieldInfo.Name))
            {
                return false;
            }

            if (Configuration.FieldExclusionPredicates.Any(predicate => predicate(fieldInfo)))
            {
                return false;
            }

            if (Configuration.IncludedFields.Contains(fieldInfo.Name))
            {
                return true;
            }

            if (Configuration.FieldInclusionPredicates.Any(predicate => predicate(fieldInfo)))
            {
                return true;
            }

            // If no explicit include rules, and no exclude rules matched, default to not tracking
            return Configuration.IncludedFields.Any() || Configuration.FieldInclusionPredicates.Any();
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
