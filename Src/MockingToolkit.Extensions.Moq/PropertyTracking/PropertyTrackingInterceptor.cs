// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Castle.DynamicProxy;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    public class PropertyTrackingInterceptor : IInterceptor
    {
        private readonly PropertyTracker _tracker;
        private readonly PropertyTrackingOptions _options;

        public PropertyTrackingInterceptor(PropertyTracker tracker, PropertyTrackingOptions options)
        {
            _tracker = tracker;
            _options = options;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            var type = invocation.TargetType ?? invocation.Proxy.GetType();

            // Property GET
            if (method.IsSpecialName && method.Name.StartsWith("get_"))
            {
                var propertyName = method.Name.Substring(4); // Remove "get_"
                if (_options.ShouldTrackProperty(propertyName, type))
                {
                    var value = _tracker.GetValue(propertyName);
                    _tracker.RecordGet(propertyName, value);
                    invocation.ReturnValue = value;
                    return;
                }
            }

            // Property SET
            if (method.IsSpecialName && method.Name.StartsWith("set_"))
            {
                var propertyName = method.Name.Substring(4); // Remove "set_"
                if (_options.ShouldTrackProperty(propertyName, type))
                {
                    var value = invocation.Arguments[0];
                    _tracker.SetValue(propertyName, value);
                    _tracker.RecordSet(propertyName, value);
                    return;
                }
            }

            // Default behavior
            invocation.Proceed();
        }
    }
}
