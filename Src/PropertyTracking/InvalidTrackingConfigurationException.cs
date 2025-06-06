﻿//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MockingToolkit.PropertyTracking
{
    /// <summary>
    /// Exception thrown when the property tracking configuration is invalid.
    /// </summary>
    public class InvalidTrackingConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTrackingConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidTrackingConfigurationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTrackingConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public InvalidTrackingConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
