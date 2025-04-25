// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

using MockingToolkit.Extensions.Moq.PropertyTracking;

using Xunit;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    public class PropertyTrackerTests
    {
        [Fact]
        public void RegisterProperty_InitializesBackingFieldWithDefaultValue()
        {
            // Arrange
            var tracker = new PropertyTracker();

            // Act
            tracker.RegisterProperty("TestInt", typeof(int));
            tracker.RegisterProperty("TestString", typeof(string));

            // Assert
            Assert.Equal(0, tracker.GetValue<int>("TestInt")); // Default value for int
            Assert.Null(tracker.GetValue<string>("TestString")); // Default value for string
        }

        [Fact]
        public void SetValueAndGetValue_WorkCorrectly()
        {
            // Arrange
            var tracker = new PropertyTracker();
            tracker.RegisterProperty("TestProperty", typeof(string));

            // Act
            tracker.SetValue("TestProperty", "NewValue");
            var value = tracker.GetValue<string>("TestProperty");

            // Assert
            Assert.Equal("NewValue", value);
        }

        [Fact]
        public void RecordSetAndRecordGet_AddInteractionsToHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();

            // Act
            tracker.RecordSet("TestProperty", "SetValue");
            tracker.RecordGet("TestProperty", "GetValue");

            // Assert
            // Assert
            Assert.Equal(2, tracker.History.Count());

            var setRecord = tracker.History.FirstOrDefault(r =>
                r.PropertyName == "TestProperty" && r.AccessType == PropertyAccessType.Set);
            Assert.NotNull(setRecord);
            Assert.Equal("SetValue", setRecord.Value);

            var getRecord = tracker.History.FirstOrDefault(r =>
                r.PropertyName == "TestProperty" && r.AccessType == PropertyAccessType.Get);
            Assert.NotNull(getRecord);
            Assert.Equal("GetValue", getRecord.Value);
        }

        [Fact]
        public void Clear_RemovesAllHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();
            tracker.RecordSet("TestProperty", "SetValue");
            tracker.RecordGet("TestProperty", "GetValue");

            // Act
            tracker.Clear();

            // Assert
            Assert.Empty(tracker.History);
        }

        [Fact]
        public void GetValue_ThrowsException_WhenPropertyNotRegistered()
        {
            // Arrange
            var tracker = new PropertyTracker();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => tracker.GetValue<string>("UnregisteredProperty"));
        }

        [Fact]
        public void RecordSetAndRecordGet_HandleNullValues()
        {
            // Arrange
            var tracker = new PropertyTracker();

            // Act
            tracker.RecordSet("TestProperty", null!);
            tracker.RecordGet("TestProperty", null!);

            // Assert
            Assert.Equal(2, tracker.History.Count());

            var setRecord = tracker.History.FirstOrDefault(r =>
                r.PropertyName == "TestProperty" && r.AccessType == PropertyAccessType.Set);
            Assert.NotNull(setRecord);
            Assert.Null(setRecord.Value);

            var getRecord = tracker.History.FirstOrDefault(r =>
                r.PropertyName == "TestProperty" && r.AccessType == PropertyAccessType.Get);
            Assert.NotNull(getRecord);
            Assert.Null(getRecord.Value);
        }

        [Fact]
        public void ValueTypeProperty_ModificationAfterGet_DoesNotAffectBackingField()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Point";
            Type propertyType = typeof(Point);
            var initialPoint = new Point { X = 1, Y = 2 };

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialPoint);
            Point retrievedPoint1 = tracker.GetValue<Point>(propertyName);
            retrievedPoint1.X = 10; // Modify the retrieved copy
            Point retrievedPoint2 = tracker.GetValue<Point>(propertyName);

            // Assert
            Assert.Equal(1, retrievedPoint2.X); // Original backing field should be unchanged
            Assert.Equal(2, retrievedPoint2.Y);
        }

        [Fact]
        public void ReferenceTypeProperty_ModificationAfterGet_AffectsBackingField()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Data";
            Type propertyType = typeof(DataContainer);
            var initialData = new DataContainer { Value = "Initial" };

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialData);
            DataContainer retrievedData1 = tracker.GetValue<DataContainer>(propertyName);
            retrievedData1.Value = "Modified";
            DataContainer retrievedData2 = tracker.GetValue<DataContainer>(propertyName);

            // Assert
            Assert.Equal("Modified", retrievedData2.Value); // Original backing field is affected
        }

        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class DataContainer
        {
            public string Value { get; set; }
        }
        [Fact]
        public void GetValue_UnregisteredProperty_ThrowsKeyNotFoundException()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string unregisteredPropertyName = "NonExistentProperty";

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => tracker.GetValue(unregisteredPropertyName));
        }

        [Fact]
        public void RegisterProperty_IntValueType_InitializesToDefault()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Age";
            Type propertyType = typeof(int);

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            int initialValue = tracker.GetValue<int>(propertyName);

            // Assert
            Assert.Equal(0, initialValue);
        }

        [Fact]
        public void RegisterProperty_StringReferenceType_InitializesToNull()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Name";
            Type propertyType = typeof(string);

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            string initialValue = tracker.GetValue<string>(propertyName);

            // Assert
            Assert.Null(initialValue);
        }

        [Fact]
        public void RegisterProperty_NullableIntValueType_InitializesToNull()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "NullableAge";
            Type propertyType = typeof(int?);

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            int? initialValue = tracker.GetValue<int?>(propertyName);

            // Assert
            Assert.Null(initialValue);
        }
        [Fact]
        public void RecordGet_AddsCorrectRecordToHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Name";
            string value = "John Doe";

            // Act
            tracker.RecordGet(propertyName, value);
            var record = tracker.History.Single();

            // Assert
            Assert.Equal(propertyName, record.PropertyName);
            Assert.Equal(PropertyAccessType.Get, record.AccessType);
            Assert.Equal(value, record.Value);
            Assert.True(DateTime.UtcNow - record.Timestamp < TimeSpan.FromMilliseconds(100)); // Check timestamp within a reasonable tolerance
        }

        [Fact]
        public void RecordSet_AddsCorrectRecordToHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Age";
            int value = 30;

            // Act
            tracker.RecordSet(propertyName, value);
            var record = tracker.History.Single();

            // Assert
            Assert.Equal(propertyName, record.PropertyName);
            Assert.Equal(PropertyAccessType.Set, record.AccessType);
            Assert.Equal(value, record.Value);
            Assert.True(DateTime.UtcNow - record.Timestamp < TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void RecordGetAndSet_MultipleRecordsInHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string nameProperty = "Name";
            string nameValue = "Jane Doe";
            string ageProperty = "Age";
            int ageValue = 25;

            // Act
            tracker.RecordGet(nameProperty, nameValue);
            tracker.RecordSet(ageProperty, ageValue);
            tracker.RecordGet(ageProperty, ageValue);
            var history = tracker.History;

            // Assert
            Assert.Equal(3, history.Count);

            Assert.Contains(history, record =>
                record.PropertyName == nameProperty &&
                record.AccessType == PropertyAccessType.Get &&
                (string)record.Value == nameValue);

            Assert.Contains(history, record =>
                record.PropertyName == ageProperty &&
                record.AccessType == PropertyAccessType.Set &&
                (int)record.Value == ageValue);

            Assert.Contains(history, record =>
                record.PropertyName == ageProperty &&
                record.AccessType == PropertyAccessType.Get &&
                (int)record.Value == ageValue);
        }

        [Fact]
        public void Clear_RemovesAllRecordsFromHistory()
        {
            // Arrange
            var tracker = new PropertyTracker();
            tracker.RecordGet("Name", "John Doe");
            tracker.RecordSet("Age", 30);
            Assert.NotEmpty(tracker.History); // Ensure there are records before clearing

            // Act
            tracker.Clear();

            // Assert
            Assert.Empty(tracker.History);
        }

        [Fact]
        public void History_IsReadOnly_CannotBeModified()
        {
            // Arrange
            PropertyTracker tracker = new PropertyTracker();
            tracker.RecordGet("Name", "John Doe");
            IReadOnlyList<PropertyInteractionRecord> history = tracker.History;

            // Act
            // Try to cast to List<T>
            List<PropertyInteractionRecord>? mutableHistory = history as List<PropertyInteractionRecord>;

            // Assert on the cast
            // While the cast might succeed, attempting to modify should fail.
            if (mutableHistory != null)
            {
                // Record the action of adding a new record to mutableHistory
                // This should throw an exception since mutableHistory is a read-only view of the history.
                var addRecord = Record.Exception(() => mutableHistory.Add(new PropertyInteractionRecord("NewProperty", PropertyAccessType.Get, null!)));

                // Validate addRecord is not null, indicating an exception was thrown
                Assert.NotNull(addRecord);
            }

            // Alternatively, try to use interface methods that would imply modification (if they existed)
            // For IReadOnlyList, there are no such methods. We can only observe the count.
            var initialCount = history.Count;
            // Perform an action that *would* modify a list if 'history' were one.
            // Since it's read-only, the count should remain the same.
            Assert.Equal(initialCount, history.Count);

            // Double-check that the original history in the tracker is still intact.
            Assert.NotEmpty(tracker.History);
            Assert.Single(tracker.History); // Should still have the initial record
        }

        [Fact]
        public void TrackAll_SetsTrackAllOptionToTrue()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();

            // Act
            var options = configurator.Build();

            // Assert
            Assert.True(options.TrackAll);
        }

        [Fact]
        public void IncludeProperties_BuildsOptionsWithPredicate()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            var propertyNames = new[] { "Name", "Age", "Name" }; // Duplicate entries included

            // Act
            var options = configurator.IncludeProperties(propertyNames).Build();

            // Assert
            Assert.False(options.TrackAll); // TrackAll should be false
            Assert.NotNull(options.Predicate); // A predicate should be created for property tracking

            // Simulate predicate evaluation
            var dummyType = typeof(DummyClass); // Replace with a relevant test class
            var nameProperty = dummyType.GetProperty("Name");
            var ageProperty = dummyType.GetProperty("Age");
            var otherProperty = dummyType.GetProperty("Other");

            Assert.True(options.Predicate?.Invoke(nameProperty!)); // Name should be tracked
            Assert.True(options.Predicate?.Invoke(ageProperty!)); // Age should be tracked
            Assert.False(options.Predicate?.Invoke(otherProperty!)); // Other properties should not be tracked
        }

        // Dummy class for testing
        public class DummyClass
        {
            public required string Name { get; set; }
            public int Age { get; set; }
            public required string Other { get; set; }
        }

        [Fact]
        public void TrackProperties_Predicate_SetsPredicateFunction()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.Name.StartsWith("Is");

            // Act
            var options = configurator.IncludeProperties(predicate).Build();

            // Assert
            Assert.False(options.TrackAll);
            Assert.NotNull(options.Predicate);
            Assert.Same(predicate, options.Predicate); // Ensure it's the same instance
        }

        [Fact]
        public void ConfigurationMethods_EnableFluentChaining()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();
            Func<PropertyInfo, bool> predicate = p => p.PropertyType == typeof(string);

            // Act
            var options = configurator
                .IncludeProperties("Id") // Include specific properties by name
                .IncludeProperties(predicate) // Include properties based on a predicate
                .Build();

            // Assert
            Assert.False(options.TrackAll); // TrackAll should be false due to specific configurations
            Assert.NotNull(options.Predicate); // Ensure a predicate is defined
            Assert.True(options.Predicate?.Invoke(typeof(DummyClass).GetProperty("Id")!)); // Verify "Id" is included
            Assert.True(options.Predicate?.Invoke(typeof(DummyClass).GetProperty("StringProperty")!)); // Verify predicate logic
            Assert.False(options.Predicate?.Invoke(typeof(DummyClass).GetProperty("IntProperty")!)); // Verify unrelated properties are excluded
        }

        [Fact]
        public void Build_WithNoConfiguration_EnablesTrackingForAll()
        {
            // Arrange
            var configurator = new PropertyTrackingConfigurator();

            // Act
            var options = configurator.Build();

            // Assert
            Assert.True(options.TrackAll); // Default behavior is to track all properties
            Assert.Null(options.Predicate); // No predicate should be set
            Assert.Null(options.ValueFormatter); // No value formatter should be set
            Assert.Null(options.AccessType); // No access type should be configured
            Assert.Null(options.AccessFilter); // No access filter should be set
        }

        [Fact]
        public void RegisterSetAndGet_ValueTypeProperty_WorksCorrectly()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Count";
            Type propertyType = typeof(int);
            int initialValue = 10;
            int newValue = 25;

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialValue);
            tracker.RecordSet(propertyName, initialValue); // Simulate a set interaction

            int retrievedValueGeneric = tracker.GetValue<int>(propertyName);
            object retrievedValueNonGeneric = tracker.GetValue(propertyName);
            tracker.RecordGet(propertyName, retrievedValueGeneric); // Simulate a get interaction

            // Assert
            Assert.Equal(initialValue, retrievedValueGeneric);
            Assert.Equal(initialValue, retrievedValueNonGeneric);
            Assert.Equal(2, tracker.History.Count);
            Assert.Equal(PropertyAccessType.Set, tracker.History[0].AccessType);
            Assert.Equal(initialValue, tracker.History[0].Value);
            Assert.Equal(PropertyAccessType.Get, tracker.History[1].AccessType);
            Assert.Equal(initialValue, tracker.History[1].Value);
        }

        [Fact]
        public void RegisterSetAndGet_ReferenceTypeProperty_WorksCorrectly()
        {
            // Arrange
            var tracker = new PropertyTracker();
            string propertyName = "Message";
            Type propertyType = typeof(string);
            string initialValue = "Hello";
            string newValue = "World";

            // Act
            tracker.RegisterProperty(propertyName, propertyType);
            tracker.SetValue(propertyName, initialValue);
            tracker.RecordSet(propertyName, initialValue);

            string retrievedValueGeneric = tracker.GetValue<string>(propertyName);
            Console.WriteLine($"Debug: retrievedValueGeneric after first GetValue: {retrievedValueGeneric}"); // Added logging
            object retrievedValueNonGeneric = tracker.GetValue(propertyName);
            tracker.RecordGet(propertyName, retrievedValueGeneric);

            tracker.SetValue(propertyName, newValue);
            tracker.RecordSet(propertyName, newValue);
            string finalValue = tracker.GetValue<string>(propertyName);
            Console.WriteLine($"Debug: finalValue after second GetValue: {finalValue}"); // Added logging

            // Assert
            Assert.Equal(initialValue, retrievedValueGeneric);
            Assert.Equal(initialValue, retrievedValueNonGeneric);
            Assert.Equal(newValue, finalValue);
            Assert.Equal(3, tracker.History.Count);
            Assert.Equal(PropertyAccessType.Set, tracker.History[0].AccessType);
            Assert.Equal(initialValue, tracker.History[0].Value);
            Assert.Equal(PropertyAccessType.Get, tracker.History[1].AccessType);
            Assert.Equal(initialValue, tracker.History[1].Value);
            Assert.Equal(PropertyAccessType.Set, tracker.History[2].AccessType);
            Assert.Equal(newValue, tracker.History[2].Value);
        }
    }
}
