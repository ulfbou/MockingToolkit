//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.PropertyTracking.Tests
{
    public class PropertyTrackingInterceptorBaseTests
    {
        private class TestInterceptor : PropertyTrackingInterceptorBase
        {
            public TestInterceptor(PropertyTrackingConfiguration configuration) : base(configuration)
            {
            }

            public bool TestShouldTrackProperty(PropertyInfo propertyInfo) => ShouldTrack(propertyInfo);

            public bool TestShouldTrackField(FieldInfo fieldInfo) => ShouldTrack(fieldInfo);

            protected override void PreTrack(string memberName)
            {
                // No implementation needed for testing ShouldTrack
            }

            protected override void PostTrack(string memberName)
            {
                // No implementation needed for testing ShouldTrack
            }
        }

        private class TestClass
        {
            public string PublicProperty { get; set; }
            internal string InternalProperty { get; set; }
            private string PrivateProperty { get; set; }
            public int PublicField;
            internal int InternalField;
            private int PrivateField;
        }

        [Fact]
        public void ShouldTrackProperty_IncludedByName_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().IncludeProperties("PublicProperty").Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.True(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_ExcludedByName_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().ExcludeProperties("PublicProperty").Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.False(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_IncludedByPredicate_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().IncludeProperties(p => p.Name.Contains("Public")).Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.True(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_ExcludedByPredicate_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().ExcludeProperties(p => p.Name.StartsWith("Public")).Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.False(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_NonPublicNotTrackedByDefault_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().Build();
            var interceptor = new TestInterceptor(config);
            var internalPropertyInfo = typeof(TestClass).GetProperty("InternalProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            var privatePropertyInfo = typeof(TestClass).GetProperty("PrivateProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.False(interceptor.TestShouldTrackProperty(internalPropertyInfo));
            Assert.False(interceptor.TestShouldTrackProperty(privatePropertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_NonPublicTrackedWhenEnabled_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().TrackNonPublicMembers().Build();
            var interceptor = new TestInterceptor(config);
            var internalPropertyInfo = typeof(TestClass).GetProperty("InternalProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            var privatePropertyInfo = typeof(TestClass).GetProperty("PrivateProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.True(interceptor.TestShouldTrackProperty(internalPropertyInfo));
            Assert.True(interceptor.TestShouldTrackProperty(privatePropertyInfo));
        }

        [Fact]
        public void ShouldTrackField_IncludedByName_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().IncludeFields("PublicField").Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.True(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_ExcludedByName_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().ExcludeFields("PublicField").Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.False(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_IncludedByPredicate_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().IncludeFields(f => f.Name.Contains("Public")).Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.True(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_ExcludedByPredicate_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().ExcludeFields(f => f.Name.StartsWith("Public")).Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.False(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_NonPublicNotTrackedByDefault_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder().Build();
            var interceptor = new TestInterceptor(config);
            var internalFieldInfo = typeof(TestClass).GetField("InternalField", BindingFlags.Instance | BindingFlags.NonPublic);
            var privateFieldInfo = typeof(TestClass).GetField("PrivateField", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.False(interceptor.TestShouldTrackField(internalFieldInfo));
            Assert.False(interceptor.TestShouldTrackField(privateFieldInfo));
        }

        [Fact]
        public void ShouldTrackField_NonPublicTrackedWhenEnabled_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder().TrackNonPublicMembers().Build();
            var interceptor = new TestInterceptor(config);
            var internalFieldInfo = typeof(TestClass).GetField("InternalField", BindingFlags.Instance | BindingFlags.NonPublic);
            var privateFieldInfo = typeof(TestClass).GetField("PrivateField", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.True(interceptor.TestShouldTrackField(internalFieldInfo));
            Assert.True(interceptor.TestShouldTrackField(privateFieldInfo));
        }

        [Fact]
        public void ShouldTrackProperty_IncludeOverridesExcludeByName_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeProperties("PublicProperty")
                .ExcludeProperties("PublicProperty")
                .Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.True(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_IncludeOverridesExcludeByPredicate_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeProperties(p => p.Name.Contains("Public"))
                .ExcludeProperties(p => p.Name.StartsWith("Public"))
                .Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.True(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_ExcludeByNameTakesPrecedenceOverIncludeByPredicate_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeProperties(p => p.Name.Contains("Public"))
                .ExcludeProperties("PublicProperty")
                .Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.False(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackProperty_ExcludeByPredicateTakesPrecedenceOverIncludeByName_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeProperties("PublicProperty")
                .ExcludeProperties(p => p.Name.StartsWith("Public"))
                .Build();
            var interceptor = new TestInterceptor(config);
            var propertyInfo = typeof(TestClass).GetProperty("PublicProperty");
            Assert.False(interceptor.TestShouldTrackProperty(propertyInfo));
        }

        [Fact]
        public void ShouldTrackField_IncludeOverridesExcludeByName_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeFields("PublicField")
                .ExcludeFields("PublicField")
                .Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.True(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_IncludeOverridesExcludeByPredicate_ReturnsTrue()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeFields(f => f.Name.Contains("Public"))
                .ExcludeFields(f => f.Name.StartsWith("Public"))
                .Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.True(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_ExcludeByNameTakesPrecedenceOverIncludeByPredicate_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeFields(f => f.Name.Contains("Public"))
                .ExcludeFields("PublicField")
                .Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.False(interceptor.TestShouldTrackField(fieldInfo));
        }

        [Fact]
        public void ShouldTrackField_ExcludeByPredicateTakesPrecedenceOverIncludeByName_ReturnsFalse()
        {
            var config = new PropertyTrackingConfigurationBuilder()
                .IncludeFields("PublicField")
                .ExcludeFields(f => f.Name.StartsWith("Public"))
                .Build();
            var interceptor = new TestInterceptor(config);
            var fieldInfo = typeof(TestClass).GetField("PublicField");
            Assert.False(interceptor.TestShouldTrackField(fieldInfo));
        }
    }
}
