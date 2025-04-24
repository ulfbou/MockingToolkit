// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Tracks the history of property accesses (get/set) for a mock instance.
    /// Also provides a per‐property backing field storage.
    /// </summary>
    public class PropertyTracker
    {
        private readonly ConcurrentBag<PropertyInteractionRecord> _history = new ConcurrentBag<PropertyInteractionRecord>();
        private readonly IDictionary<string, object> _backingFields = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// The readonly history of property interactions.
        /// </summary>
        public IReadOnlyList<PropertyInteractionRecord> History => _history.ToList().AsReadOnly();

        /// <summary>
        /// The backing fields for properties.
        /// </summary>
        /// <remarks>
        /// This is internal to allow access from the PropertyTrackingInterceptor for testing purposes only.
        /// </remarks>
        internal IReadOnlyDictionary<string, object> BackingFields => _backingFields.AsReadOnly();

        /// <summary>
        /// Registers a property to be tracked; initializes its backing field with a default value.
        /// </summary>
        public void RegisterProperty(string propertyName, Type propertyType)
        {
            _backingFields[propertyName] = (propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null)!;
        }

        /// <summary>
        /// Retrieves the backing field value for a property in a strongly-typed manner.
        /// </summary>
        public T GetValue<T>(string propertyName)
        {
            return (T)_backingFields[propertyName];
        }

        /// <summary>
        /// Retrieves the backing field value for a property.
        /// </summary>
        public object GetValue(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            }

            if (!_backingFields.ContainsKey(propertyName))
            {
                throw new KeyNotFoundException($"Property '{propertyName}' not found in backing fields.");
            }

            return _backingFields[propertyName];
        }

        /// <summary>
        /// Sets the backing field value for a property.
        /// </summary>
        public void SetValue(string propertyName, object value)
        {
            _backingFields[propertyName] = value;
        }

        /// <summary>
        /// Records a property get interaction.
        /// </summary>
        public void RecordGet(string propertyName, object value)
        {
            _history.Add(new PropertyInteractionRecord(propertyName, PropertyAccessType.Get, value));
        }

        /// <summary>
        /// Records a property set interaction.
        /// </summary>
        public void RecordSet(string propertyName, object value)
        {
            _history.Add(new PropertyInteractionRecord(propertyName, PropertyAccessType.Set, value));
        }

        /// <summary>
        /// Clears the interaction history.
        /// </summary>
        public void Clear() => _history.Clear();
    }
}
