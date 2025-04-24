//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//namespace MockingToolkit.Extensions.Moq
//{
//    /// <summary>
//    /// Tracks the history of property accesses (get/set) for a mock instance.
//    /// Also provides a per‐property backing field storage.
//    /// </summary>
//    public partial class PropertyTracker2
//    {
//        // List of all property interaction records.
//        private readonly List<PropertyInteractionRecord> _history = new List<PropertyInteractionRecord>();

//        // Dictionary for storing the backing field values for each property.
//        private readonly Dictionary<string, object> _backingFields = new Dictionary<string, object>();

//        /// <summary>
//        /// The readonly history of property interactions.
//        /// </summary>
//        public IReadOnlyList<PropertyInteractionRecord> History => _history;

//        /// <summary>
//        /// Registers a property to be tracked; initializes its backing field with a default value.
//        /// </summary>
//        public void RegisterProperty(string propertyName, Type propertyType)
//        {
//            _backingFields[propertyName] = (propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null)!;
//        }

//        /// <summary>
//        /// Retrieves the backing field value for a property in a strongly-typed manner.
//        /// </summary>
//        public T GetValue<T>(string propertyName)
//        {
//            return (T)_backingFields[propertyName];
//        }

//        /// <summary>
//        /// Retrieves the backing field value for a property.
//        /// </summary>
//        public object GetValue(string propertyName)
//        {
//            return _backingFields[propertyName];
//        }

//        /// <summary>
//        /// Sets the backing field value for a property.
//        /// </summary>
//        public void SetValue(string propertyName, object value)
//        {
//            _backingFields[propertyName] = value;
//        }

//        /// <summary>
//        /// Records a property get interaction.
//        /// </summary>
//        public object RecordGet(string propertyName, object value)
//        {
//            _history.Add(new PropertyInteractionRecord2(propertyName, PropertyAccessType2.Get, value));
//            return value;
//        }

//        /// <summary>
//        /// Records a property set interaction.
//        /// </summary>
//        public void RecordSet(string propertyName, object value)
//        {
//            _history.Add(new PropertyInteractionRecord(propertyName, PropertyAccessType.Set, value));
//        }

//        /// <summary>
//        /// Clears the interaction history.
//        /// </summary>
//        public void Clear() => _history.Clear();
//    }
//}