// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Interface for tracked mocks.
    /// </summary>
    public interface ITrackedMock
    {
        /// <summary>
        /// Gets the property tracking options.
        /// </summary>
        PropertyTracker PropertyTracker { get; }
    }
}
