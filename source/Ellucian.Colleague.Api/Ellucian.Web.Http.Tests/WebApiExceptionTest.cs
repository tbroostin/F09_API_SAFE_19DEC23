using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Web.Http.Tests
{
    
    
    /// <summary>
    ///This is a test class for WebApiExceptionTest and is intended
    ///to contain all WebApiExceptionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WebApiExceptionTest
    {
        /// <summary>
        ///WebApiException Constructor
        ///</summary>
        [TestMethod()]
        public void WebApiExceptionConstructorTest()
        {
            WebApiException target = new WebApiException();
            Assert.IsNotNull(target.Message);
        }

        /// <summary>
        ///A test for AddConflict where message is null and empty
        ///</summary>
        [TestMethod()]
        public void AddConflictTestNullAndEmpty()
        {
            WebApiException target = new WebApiException();
            target.AddConflict(null);
            target.AddConflict(string.Empty);
            Assert.IsNull(target.Conflicts);
        }

        /// <summary>
        ///A test for AddConflict where message is not empty
        ///</summary>
        [TestMethod()]
        public void AddConflictTestNotEmpty()
        {
            WebApiException target = new WebApiException();
            string conflict = "Conflict 1";
            target.AddConflict(conflict);
            Assert.IsNotNull(target.Conflicts);
            Assert.IsTrue(((List<string>)target.Conflicts)[0] == conflict);
        }


        /// <summary>
        ///A test for AddConflicts with empty list
        ///</summary>
        [TestMethod()]
        public void AddConflictsTestEmpty()
        {
            WebApiException target = new WebApiException();
            IEnumerable<string> conflicts = new List<string>() { }; 
            target.AddConflicts(conflicts);
            Assert.IsNull(target.Conflicts);
        }

        /// <summary>
        ///A test for AddConflicts
        ///</summary>
        [TestMethod()]
        public void AddConflictsTestNotEmpty()
        {
            WebApiException target = new WebApiException();
            IEnumerable<string> conflicts = new List<string>() { "Conflict 1", "Conflict 2", null, string.Empty };
            target.AddConflicts(conflicts);
            Assert.IsNotNull(target.Conflicts);
            Assert.IsTrue(((List<string>)target.Conflicts).Count == 2);
            Assert.IsTrue(((List<string>)target.Conflicts)[0] == ((List<string>)conflicts)[0]);
            Assert.IsTrue(((List<string>)target.Conflicts)[1] == ((List<string>)conflicts)[1]);
        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        public void IsEmptyTest1()
        {
            WebApiException target = new WebApiException();
            Assert.IsTrue(target.IsEmpty);
        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        public void IsEmptyTest2()
        {
            WebApiException target = new WebApiException();
            target.Message = "X";
            target.AddConflict(string.Empty);
            Assert.IsFalse(target.IsEmpty);
        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        public void IsEmptyTest3()
        {
            WebApiException target = new WebApiException();
            target.Message = string.Empty;
            target.AddConflict("C1");
            Assert.IsFalse(target.IsEmpty);
        }

    }
}
