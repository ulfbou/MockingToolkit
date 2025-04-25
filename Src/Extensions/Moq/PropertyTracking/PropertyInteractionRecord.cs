// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Represents a single record of a property interaction.
    /// </summary>
    public record PropertyInteractionRecord
    {
        public string PropertyName { get; }
        public PropertyAccessType AccessType { get; }
        public object Value { get; }
        public DateTime Timestamp { get; }

        public PropertyInteractionRecord(string propertyName, PropertyAccessType accessType, object value)
        {
            PropertyName = propertyName;
            AccessType = accessType;
            Value = value;
            Timestamp = DateTime.UtcNow;
        }
    }
}