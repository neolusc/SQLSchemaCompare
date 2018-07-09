﻿using System;
using SQLCompare.Core.Entities.Exceptions;
using SQLCompare.Test.Infrastructure.DatabaseProviders;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Core.Exceptions
{
    /// <summary>
    /// Test class for the PropertyNotFoundException class
    /// </summary>
    public class PropertyNotFoundExceptionTest : BaseTests<DatabaseProviderTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundExceptionTest"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public PropertyNotFoundExceptionTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the retrieval of database list with all the databases
        /// </summary>
        [Fact]
        [UnitTest]
        public void ExceptionMessage()
        {
            var ex = new PropertyNotFoundException(typeof(PropertyNotFoundException), "Test");
            Assert.True(ex.ClassType != null && ex.PropopertyName == "Test");
            Assert.Equal($"The property has not been found in the class.{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message", typeof(PropertyNotFoundException), "Test");
            Assert.True(ex.ClassType != null && ex.PropopertyName == "Test");
            Assert.Equal($"Test message{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message 2", typeof(PropertyNotFoundException), "Test", new InvalidOperationException());
            Assert.True(ex.ClassType != null && ex.PropopertyName == "Test");
            Assert.True(ex.InnerException != null);
            Assert.Equal($"Test message 2{Environment.NewLine}Class: PropertyNotFoundException; Property: Test", ex.Message);

            ex = new PropertyNotFoundException("Test message 3");
            Assert.True(ex.ClassType == null && ex.PropopertyName == null);
            Assert.Equal("Test message 3", ex.Message);
        }
    }
}
