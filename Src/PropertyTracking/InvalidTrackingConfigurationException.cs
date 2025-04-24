//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Represents an exception thrown when an invalid property tracking configuration is detected.
    /// </summary>
    public class InvalidTrackingConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTrackingConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidTrackingConfigurationException(string message) : base(message) { }
    }
}
