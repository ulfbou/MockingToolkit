//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//// TODO: Offer a configurable trade-off between performance, memory usage, thread-safety and complexity.

//namespace MockingToolkit.Extensions.Moq
//{
//    public partial class PropertyTracker2
//    {
//        // Dictionary to store handlers for property set events
//        private readonly Dictionary<string, List<Action<object>>> _propertySetHandlers = new();

//        /// <summary>
//        /// Adds a handler for a property set event.
//        /// </summary>
//        /// <param name="propertyName">The name of the property.</param>
//        /// <param name="handler">The handler to invoke when the property is set.</param>
//        internal void AddHandler(string propertyName, Action<object> handler)
//        {
//            if (!_propertySetHandlers.TryGetValue(propertyName, out var handlers))
//            {
//                handlers = new List<Action<object>>();
//                _propertySetHandlers[propertyName] = handlers;
//            }

//            handlers.Add(handler);
//        }

//        /// <summary>
//        /// Invokes all handlers for a property set event.
//        /// </summary>
//        /// <param name="propertyName">The name of the property.</param>
//        /// <param name="value">The value being set.</param>
//        internal void InvokeHandlers(string propertyName, object value)
//        {
//            if (_propertySetHandlers.TryGetValue(propertyName, out var handlers))
//            {
//                foreach (var handler in handlers)
//                {
//                    handler(value);
//                }
//            }
//        }
//    }
//}
