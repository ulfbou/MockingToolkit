// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Defines a fluent API for configuring property tracking options.
    /// </summary>
    public interface IPropertyTrackingConfigurator : IDisposable
    {
        /// <summary>
        /// Includes specified properties in the tracking process by their names.
        /// </summary>
        /// <param name="propertyNames">An array of property names to be included.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator IncludeProperties(params string[] propertyNames);

        /// <summary>
        /// Includes properties that match the specified predicate in the tracking process.
        /// </summary>
        /// <param name="predicate">A function that defines the filtering criteria for properties to be included.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator IncludeProperties(Func<PropertyInfo, bool> predicate);

        /// <summary>
        /// Excludes specified properties from the tracking process by their names.
        /// </summary>
        /// <param name="propertyNames">An array of property names to be excluded.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator ExcludeProperties(params string[] propertyNames);

        /// <summary>
        /// Excludes properties that match the specified predicate from the tracking process.
        /// </summary>
        /// <param name="predicate">A function that defines the filtering criteria for properties to be excluded.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator ExcludeProperties(Func<PropertyInfo, bool> predicate);

        /// <summary>
        /// Configures a custom formatter for property values during tracking.
        /// </summary>
        /// <param name="formatter">A function that formats property values into strings.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator WithValueFormatter(Func<object?, string> formatter);

        /// <summary>
        /// Configures a custom access filter for properties during tracking.
        /// </summary>
        /// <param name="filter">A function that defines the access filter logic for properties.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator WithAccessFilter(Func<PropertyInfo, bool> filter);

        /// <summary>
        /// Specifies the access type for properties during tracking.
        /// </summary>
        /// <param name="accessType">The type of access to be configured for properties.</param>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator WithAccess(PropertyAccessType accessType);

        /// <summary>
        /// Clears all tracking configurations, resetting the configurator to its default state.
        /// </summary>
        /// <returns>The current instance of the configurator for fluent chaining.</returns>
        IPropertyTrackingConfigurator Clear();

        /// <summary>
        /// Builds and returns the configured <see cref="PropertyTrackingOptions"/>.
        /// </summary>
        /// <returns>A <see cref="PropertyTrackingOptions"/> instance with the configured tracking settings.</returns>
        PropertyTrackingOptions Build();
    }
}
