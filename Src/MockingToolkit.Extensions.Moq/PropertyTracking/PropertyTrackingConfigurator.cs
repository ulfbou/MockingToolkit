// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    // PropertyTrackingConfigurator.cs
    /// <summary>
    /// Provides a fluent API for configuring property tracking options.
    /// </summary>
    public class PropertyTrackingConfigurator : IPropertyTrackingConfigurator
    {
        private readonly PropertyTrackingOptions _options = new();

        /// <inheritdoc />
        public IPropertyTrackingConfigurator TrackAll()
        {
            _options.TrackAll = true;
            return this;
        }

        /// <inheritdoc />
        public IPropertyTrackingConfigurator TrackProperties(params string[] propertyNames)
        {
            _options.TrackedPropertyNames = propertyNames.Distinct().ToList();
            return this;
        }

        /// <inheritdoc />
        public IPropertyTrackingConfigurator TrackProperties(Func<PropertyInfo, bool> predicate)
        {
            _options.Predicate = predicate;
            return this;
        }

        /// <inheritdoc />
        public PropertyTrackingOptions Build() => _options;
    }
}