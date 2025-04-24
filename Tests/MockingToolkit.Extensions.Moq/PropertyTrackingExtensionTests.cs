//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//using Moq;
//using MockingToolkit;
//using Xunit;

//namespace MockingToolkit.Extensions.Moq.Tests
//{
//    public class PropertyTrackingExtensionTests
//    {
//        [Fact]
//        public void TrackAllProperties_ReadableProperty_ReturnsDefault()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            Assert.Equal(default(int), mock.Object.Age);
//            Assert.Null(mock.Object.Name);
//            Assert.Equal(default(bool), mock.Object.IsActive);
//            Assert.Equal(default(int), mock.Object.ReadOnlyValue);
//        }

//        [Fact]
//        public void TrackAllProperties_ReadOnlyProperty_CanGetDefault()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            Assert.Equal(default(int), mock.Object.ReadOnlyValue);
//        }

//        [Fact]
//        public void TrackAllProperties_WriteOnlyProperty_DoesNotThrowOnSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            var exception = Record.Exception(() => mock.Object.WriteOnlyValue = "Some Value");
//            Assert.Null(exception);
//        }

//        [Fact]
//        public void GetLastPropertyValue_ReturnsSetValue()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Name = "GetValue Test";
//            Assert.Equal("GetValue Test", mock.GetLastPropertyValue(m => m.Name));
//        }

//        [Fact]
//        public void GetLastPropertyValue_ReturnsDefaultIfNotSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            Assert.Null(mock.GetLastPropertyValue(m => m.Name));
//            Assert.Equal(default(int), mock.GetLastPropertyValue(m => m.Age));
//        }

//        [Fact]
//        public void GetPropertyValueHistory_TracksMultipleSets()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Name = "First";
//            mock.Object.Name = "Second";
//            var history = mock.GetPropertyValueHistory(m => m.Name);
//            Assert.Equal(2, history.Count);
//            Assert.Equal("First", history[0]);
//            Assert.Equal("Second", history[1]);
//        }

//        [Fact]
//        public void GetPropertyValueHistory_ReturnsEmptyListIfNotSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            var history = mock.GetPropertyValueHistory(m => m.Name);
//            Assert.Empty(history);
//        }

//        [Fact]
//        public void GetPropertySetCount_ReturnsCorrectCount()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Age = 10;
//            mock.Object.Age = 15;
//            Assert.Equal(2, mock.GetPropertySetCount(m => m.Age));
//        }

//        [Fact]
//        public void GetPropertySetCount_ReturnsZeroIfNotSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            Assert.Equal(0, mock.GetPropertySetCount(m => m.Name));
//        }

//        [Fact]
//        public void AssertPropertyWasSet_SucceedsIfSetCorrectNumberOfTimes()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.IsActive = true;
//            mock.Object.IsActive = true;
//            mock.AssertPropertyWasSet(m => m.IsActive, true, times: 2);
//        }

//        [Fact]
//        public void AssertPropertyWasSet_FailsIfSetIncorrectNumberOfTimes()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Name = "Test";
//            Assert.Throws<InvalidOperationException>(() => mock.AssertPropertyWasSet(m => m.Name, "Test", times: 3));
//        }

//        [Fact]
//        public void AssertPropertyWasSetAny_SucceedsIfSetAtLeastOnce()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Age = 50;
//            mock.AssertPropertyWasSetAny(m => m.Age);
//        }

//        [Fact]
//        public void AssertPropertyWasNotSet_SucceedsIfNeverSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.AssertPropertyWasNotSet(m => m.IsActive);
//        }

//        [Fact]
//        public void AssertPropertyWasNotSet_FailsIfSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Name = "Never Set? Wrong!";
//            Assert.Throws<InvalidOperationException>(() => mock.AssertPropertyWasNotSet(m => m.Name));
//        }

//        [Fact]
//        public void AssertPropertyValueHistoryContains_SucceedsIfValueInHistory()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Name = "Alpha";
//            mock.Object.Name = "Beta";
//            Assert.Contains("Alpha", mock.GetPropertyValueHistory(m => m.Name));
//        }

//        [Fact]
//        public void AssertPropertyValueHistoryContains_FailsIfValueNotInHistory()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            mock.Object.Age = 100;
//            Assert.Throws<InvalidOperationException>(() => mock.AssertPropertyValueHistoryContains(m => m.Age, 200));
//        }

//        [Fact]
//        public void OnPropertySet_HandlerIsCalledOnSet()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            string capturedName = null!;
//            mock.OnPropertySet(m => m.Name, name => capturedName = name!);
//            mock.Object.Name = "Handler Test";
//            Assert.Equal("Handler Test", capturedName);
//        }

//        [Fact]
//        public void OnPropertySet_MultipleHandlersAreCalled()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();
//            int callCount1 = 0;
//            int callCount2 = 0;
//            mock.OnPropertySet(m => m.Age, age => callCount1++);
//            mock.OnPropertySet(m => m.Age, age => callCount2 += 2);
//            mock.Object.Age = 1;
//            Assert.Equal(1, callCount1);
//            Assert.Equal(2, callCount2);
//        }

//        [Fact]
//        public void TrackAllProperties_WritableProperty_CanSetAndGetTrackedValue()
//        {
//            var mock = new Mock<ITestInterface> { CallBase = true };
//            mock.TrackAllProperties<ITestInterface>();

//            mock.Object.Name = "Test Name";
//            Assert.Equal("Test Name", mock.Object.Name); // This fails

//            mock.Object.Age = 25;
//            Assert.Equal(25, mock.Object.Age);

//            mock.Object.IsActive = true;
//            Assert.True(mock.Object.IsActive);
//        }

//        [Fact]
//        public void TrackAllProperties_TrackingIsIsolatedPerMockInstance()
//        {
//            var mock1 = new Mock<ITestInterface> { CallBase = true };
//            var mock2 = new Mock<ITestInterface> { CallBase = true };

//            mock1.TrackAllProperties<ITestInterface>();
//            mock2.TrackAllProperties<ITestInterface>();

//            // Assert initial default values of mock2
//            Assert.Null(mock2.Object.Name);
//            Assert.Equal(default(int), mock2.Object.Age);

//            mock1.Object.Name = "Mock 1 Name";
//            mock1.Object.Age = 30;

//            // Assert that mock2's values remain unchanged
//            Assert.Null(mock2.Object.Name);
//            Assert.Equal(default(int), mock2.Object.Age);

//            // Assert that mock1's values have been changed
//            Assert.Equal("Mock 1 Name", mock1.Object.Name); // this fails
//            Assert.Equal(30, mock1.Object.Age);
//        }
//    }
//}
