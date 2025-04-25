//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Reflection;

namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
{
    /// <summary>
    /// A stub implementation of PropertyInfo for testing purposes.
    /// This class is used to simulate property information without requiring a real object.
    /// </summary>
    public class PropertyInfoStub : PropertyInfo
    {
        private readonly string _name;
        private readonly Type _propertyType;
        private readonly bool _canRead;
        private readonly bool _canWrite;

        public PropertyInfoStub(string name, Type propertyType = null!, bool canRead = false, bool canWrite = false)
        {
            _name = name;
            _propertyType = propertyType ?? typeof(string);
            _canRead = canRead;
            _canWrite = canWrite;
        }

        public override string Name => _name;
        public override Type PropertyType => _propertyType;

        // Other members can throw NotImplementedException as they are not used in tests
        public override PropertyAttributes Attributes => throw new NotImplementedException();
        public override bool CanRead => _canRead;
        public override bool CanWrite => _canWrite;
        public override MethodInfo GetGetMethod(bool nonPublic) => throw new NotImplementedException();
        public override MethodInfo GetSetMethod(bool nonPublic) => throw new NotImplementedException();
        public override MethodInfo[] GetAccessors(bool nonPublic) => throw new NotImplementedException();
        public override object GetValue(object obj, object[] index) => throw new NotImplementedException();
        public override void SetValue(object obj, object value, object[] index) => throw new NotImplementedException();
        public override Type DeclaringType => throw new NotImplementedException();
        public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotImplementedException();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
        public override ParameterInfo[] GetIndexParameters() => throw new NotImplementedException();
        public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture) => throw new NotImplementedException();
        public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture) => throw new NotImplementedException();

        public override Type ReflectedType => throw new NotImplementedException();
    }
}
