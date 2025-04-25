//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//using Moq;
//using MockingToolkit.Extensions.Moq.PropertyTracking;
//using Xunit;

//namespace MockingToolkit.Extensions.Moq.Tests.PropertyTracking
//{
//    public class MoqPropertyTrackingExtensionTests
//    {
//        public interface ITestInterface
//        {
//            string Name { get; set; }
//            int Age { get; set; }
//        }

//        [Fact]
//        public void TrackProperties_TrackAll_TracksAllProperties()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(); // Ensure this initializes property tracking correctly.

//            // Act
//            mock.Object.Name = "TestName";
//            mock.Object.Age = 25;

//            // Assert
//            Assert.Equal("TestName", mock.GetLastPropertyValue(m => m.Name));
//            Assert.Equal(25, mock.GetLastPropertyValue(m => m.Age));
//        }

//        [Fact]
//        public void TrackProperties_SpecificProperties_TracksOnlySpecifiedProperties()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(m => m.Name);

//            // Act
//            mock.Object.Name = "TestName";
//            mock.Object.Age = 25;

//            // Assert
//            Assert.Equal("TestName", mock.GetLastPropertyValue(m => m.Name));
//            Assert.Throws<InvalidOperationException>(() => mock.GetLastPropertyValue(m => m.Age));
//        }

//        // MoqPropertyTrackingExtensionTests.cs line 51

//        [Fact]
//        public void GetLastPropertyValue_ReturnsLastSetValue()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(m => m.Name); // Ensure tracking is initialized for the property

//            // Act
//            mock.Object.Name = "FirstName";
//            mock.Object.Name = "LastName";

//            // Assert
//            Assert.Equal("LastName", mock.GetLastPropertyValue(m => m.Name)); // Verify the last set value
//        }

//        [Fact]
//        public void GetPropertyValueHistory_ReturnsHistoryOfValues()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(m => m.Name);

//            // Act
//            mock.Object.Name = "FirstName";
//            mock.Object.Name = "SecondName";

//            // Assert
//            var history = mock.GetPropertyValueHistory(m => m.Name);
//            Assert.Equal(2, history.Count);
//            Assert.Contains("FirstName", history);
//            Assert.Contains("SecondName", history);
//        }

//        [Fact]
//        public void GetPropertySetCount_ReturnsSetCount()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(m => m.Name);

//            // Act
//            mock.Object.Name = "FirstName";
//            mock.Object.Name = "SecondName";

//            // Assert
//            Assert.Equal(2, mock.GetPropertySetCount(m => m.Name));
//        }

//        [Fact]
//        public void OnPropertySet_InvokesHandlerOnPropertySet()
//        {
//            // Arrange
//            var mock = new Mock<ITestInterface>();
//            mock.TrackProperties(m => m.Name);

//            var handlerInvoked = false;
//            mock.OnPropertySet(m => m.Name, _ => handlerInvoked = true);

//            // Act
//            mock.Object.Name = "TestName";

//            // Assert
//            Assert.True(handlerInvoked);
//        }
//    }
//}
