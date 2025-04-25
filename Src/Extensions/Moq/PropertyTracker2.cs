//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

//namespace MockingToolkit.Extensions.Moq
//{
//    public partial class PropertyTracker2
//    {
//        internal object GetLastValue(string propertyName)
//        {
//            // Retrieve the last value from the history for the specified property
//            var record = _history.LastOrDefault(r => r.PropertyName == propertyName && r.AccessType == PropertyAccessType.Set);
//            return record?.Value ?? throw new InvalidOperationException($"No value found for property '{propertyName}'.");
//        }

//        internal IReadOnlyList<object> GetHistory(string propertyName)
//        {
//            // Retrieve all values from the history for the specified property
//            var records = _history
//                .Where(r => r.PropertyName == propertyName && r.AccessType == PropertyAccessType.Set)
//                .Select(r => r.Value)
//                .ToList();

//            return records.Any() ? records : throw new InvalidOperationException($"No history found for property '{propertyName}'.");
//        }

//        internal int GetSetCount(string propertyName)
//        {
//            // Count the number of set interactions for the specified property
//            return _history.Count(r => r.PropertyName == propertyName && r.AccessType == PropertyAccessType.Set);
//        }
//    }
//}
