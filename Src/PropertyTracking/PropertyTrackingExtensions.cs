//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Provides extension methods related to property tracking.
    /// </summary>
    public static class PropertyTrackingExtensions
    {
        /// <summary>
        /// Applies the specified property tracking configuration to a mocked object.
        /// </summary>
        /// <typeparam name="TMockedObject">The type of the mocked object.</typeparam>
        /// <param name="mockedObject">The mocked object to apply tracking to.</param>
        /// <param name="configuration">The property tracking configuration.</param>
        /// <returns>The mocked object with property tracking applied.</returns>
        /// <remarks>Implementation will be completed in later phases.</remarks>
        public static TMockedObject TrackProperties<TMockedObject>(this TMockedObject mockedObject, PropertyTrackingConfiguration configuration)
        {
            // Implementation will be added in later phases (e.g., Phase 4 for Moq)
            return mockedObject;
        }

        internal static bool IsPublic(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod?.IsPublic == true || propertyInfo.SetMethod?.IsPublic == true;
        }
    }
}
