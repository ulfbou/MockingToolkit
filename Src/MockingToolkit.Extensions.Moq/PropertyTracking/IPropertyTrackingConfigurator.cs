// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    // IPropertyTrackingConfigurator.cs
    /// <summary>
    /// Defines an interface for configuring property tracking options.
    /// </summary>
    public interface IPropertyTrackingConfigurator
    {
        /// <summary>
        /// Configures the tracking to include all readable and writable properties.
        /// </summary>
        /// <returns>The current <see cref="IPropertyTrackingConfigurator"/> instance for chaining.</returns>
        IPropertyTrackingConfigurator TrackAll();

        /// <summary>
        /// Configures the tracking to include the specified property names.
        /// </summary>
        /// <param name="propertyNames">An array of property names to track.</param>
        /// <returns>The current <see cref="IPropertyTrackingConfigurator"/> instance for chaining.</returns>
        IPropertyTrackingConfigurator TrackProperties(params string[] propertyNames);

        /// <summary>
        /// Configures the tracking to include properties that satisfy the provided predicate.
        /// </summary>
        /// <param name="predicate">A function that takes a <see cref="PropertyInfo"/> and returns <c>true</c> if the property should be tracked, <c>false</c> otherwise.</param>
        /// <returns>The current <see cref="IPropertyTrackingConfigurator"/> instance for chaining.</returns>
        IPropertyTrackingConfigurator TrackProperties(Func<PropertyInfo, bool> predicate);

        /// <summary>
        /// Builds the <see cref="PropertyTrackingOptions"/> based on the current configuration.
        /// </summary>
        /// <returns>A new instance of <see cref="PropertyTrackingOptions"/>.</returns>
        PropertyTrackingOptions Build();
    }
}
