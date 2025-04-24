//// PropertyTrackingOptions.cs

//using Castle.DynamicProxy;

//namespace MockingToolkit.Extensions.Moq.PropertyTracking
//{
//    public class PropertyTrackingInterceptorTests
//    {
//        public class TestClass
//        {
//            public virtual string Name { get; set; }
//            public virtual int Age { get; set; }
//        }

//        [Fact]
//        public void Intercept_TracksLastSetValue()
//        {
//            // Arrange
//            var options = new PropertyTrackingOptions { TrackAll = true };
//            var interceptor = new PropertyTrackingInterceptor(options);
//            var proxyGenerator = new ProxyGenerator();
//            var proxy = proxyGenerator.CreateClassProxy<TestClass>(interceptor);

//            // Act
//            proxy.Name = "TestName";

//            // Assert
//            Assert.Equal("TestName", interceptor.GetLastValue("Name"));
//        }

//        [Fact]
//        public void Intercept_TracksSetCount()
//        {
//            // Arrange
//            var options = new PropertyTrackingOptions { TrackAll = true };
//            var interceptor = new PropertyTrackingInterceptor(options);
//            var proxyGenerator = new ProxyGenerator();
//            var proxy = proxyGenerator.CreateClassProxy<TestClass>(interceptor);

//            // Act
//            proxy.Name = "TestName";
//            proxy.Name = "AnotherName";

//            // Assert
//            Assert.Equal(2, interceptor.GetSetCount("Name"));
//        }

//        [Fact]
//        public void Intercept_TracksPropertyHistory()
//        {
//            // Arrange
//            var options = new PropertyTrackingOptions { TrackAll = true };
//            var interceptor = new PropertyTrackingInterceptor(options);
//            var proxyGenerator = new ProxyGenerator();
//            var proxy = proxyGenerator.CreateClassProxy<TestClass>(interceptor);

//            // Act
//            proxy.Name = "FirstName";
//            proxy.Name = "SecondName";

//            // Assert
//            var history = interceptor.GetHistory("Name");
//            Assert.Equal(2, history.Count);
//            Assert.Contains("FirstName", history);
//            Assert.Contains("SecondName", history);
//        }

//        [Fact]
//        public void Intercept_InvokesCustomHandler()
//        {
//            // Arrange
//            var options = new PropertyTrackingOptions { TrackAll = true };
//            var interceptor = new PropertyTrackingInterceptor(options);
//            var proxyGenerator = new ProxyGenerator();
//            var proxy = proxyGenerator.CreateClassProxy<TestClass>(interceptor);

//            var handlerInvoked = false;
//            interceptor.AddHandler("Name", _ => handlerInvoked = true);

//            // Act
//            proxy.Name = "TestName";

//            // Assert
//            Assert.True(handlerInvoked);
//        }

//        [Fact]
//        public void Intercept_IgnoresUntrackedProperties()
//        {
//            // Arrange
//            var options = new PropertyTrackingOptions { TrackAll = false };
//            var interceptor = new PropertyTrackingInterceptor(options);
//            var proxyGenerator = new ProxyGenerator();
//            var proxy = proxyGenerator.CreateClassProxy<TestClass>(interceptor);

//            // Act
//            proxy.Name = "TestName";

//            // Assert
//            Assert.Null(interceptor.GetLastValue("Name"));
//            Assert.Equal(0, interceptor.GetSetCount("Name"));
//            Assert.Empty(interceptor.GetHistory("Name"));
//        }
//    }
//}
