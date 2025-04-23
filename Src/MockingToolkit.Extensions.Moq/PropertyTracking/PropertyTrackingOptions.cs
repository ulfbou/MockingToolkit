// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Defines the options for property tracking.
    /// </summary>
    public class PropertyTrackingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether all readable and writable properties should be tracked.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool TrackAll { get; set; }

        /// <summary>
        /// Gets or sets a collection of property names to be tracked.
        /// This is considered only if <see cref="TrackAll"/> is <c>false</c>.
        /// Defaults to an empty read-only collection.
        /// </summary>
        public IReadOnlyCollection<string> TrackedPropertyNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a predicate that determines whether a property should be tracked based on its <see cref="PropertyInfo"/>.
        /// This is considered only if <see cref="TrackAll"/> is <c>false</c> and the property name is not in <see cref="TrackedPropertyNames"/>.
        /// Defaults to <c>null</c>.
        /// </summary>
        public Func<PropertyInfo, bool>? Predicate { get; set; }
    }
}
