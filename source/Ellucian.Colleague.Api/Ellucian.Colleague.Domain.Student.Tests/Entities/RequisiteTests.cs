using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RequisiteTests
    {
        [TestClass]
        public class RequisiteConstructor3Parms
        {
            private string requirementCode;
            private bool isRequired;
            private RequisiteCompletionOrder completionOrder;
            private Requisite requisite;
            private bool isProtected;

            [TestInitialize]
            public void Initialize()
            {
                requirementCode = "12345";
                isRequired = true;
                completionOrder = RequisiteCompletionOrder.PreviousOrConcurrent;
                isProtected = true;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void requirementCode_Null()
            {
                requisite = new Requisite(null, isRequired, completionOrder, isProtected);
            }

            [TestMethod]
            public void RequirementCode_Set()
            {
                requisite = new Requisite(requirementCode, isRequired, completionOrder, isProtected);
                Assert.AreEqual(requirementCode, requisite.RequirementCode);
            }

            [TestMethod]
            public void IsRequired_Set()
            {
                requisite = new Requisite(requirementCode, isRequired, completionOrder, isProtected);
                Assert.AreEqual(isRequired, requisite.IsRequired);
            }

            [TestMethod]
            public void CompletionOrder_Set()
            {
                requisite = new Requisite(requirementCode, isRequired, completionOrder, isProtected);
                Assert.AreEqual(completionOrder, requisite.CompletionOrder);
            }

            [TestMethod]
            public void CoreqCourseId_NotSet()
            {
                requisite = new Requisite(requirementCode, isRequired, completionOrder, isProtected);
                Assert.IsNull(requisite.CorequisiteCourseId);
            }

            [TestMethod]
            public void IsProtected_Set()
            {
                requisite = new Requisite(requirementCode, isRequired, completionOrder, isProtected);
                Assert.AreEqual(isProtected, requisite.IsProtected);
            }
        }

        [TestClass]
        public class RequisiteConstructor2Parms
        {
            private bool isRequired;
            private string coreqCourseId;
            private Requisite requisite;

            [TestInitialize]
            public void Initialize()
            {
                isRequired = true;
                coreqCourseId = "45678";
            }

            [TestMethod]
            public void CoreqCourseId_Set()
            {
                requisite = new Requisite(coreqCourseId, isRequired);
                Assert.AreEqual(coreqCourseId, requisite.CorequisiteCourseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CoreqCourseId_Empty()
            {
                new Requisite("", isRequired);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CoreqCourseId_Null()
            {
                string courseId = null;
                new Requisite(courseId, isRequired);
            }

            [TestMethod]
            public void RequirementCode_NotSet()
            {
                requisite = new Requisite(coreqCourseId, isRequired);
                Assert.IsNull(requisite.RequirementCode);
            }

            [TestMethod]
            public void IsRequired_Set()
            {
                requisite = new Requisite(coreqCourseId, isRequired);
                Assert.AreEqual(isRequired, requisite.IsRequired);
            }

            [TestMethod]
            public void CompletionOrder_DefaultsToPreviousOrConcurrent()
            {
                requisite = new Requisite(coreqCourseId, isRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, requisite.CompletionOrder);
            }

            [TestMethod]
            public void IsProtected_Set()
            {
                requisite = new Requisite(coreqCourseId, isRequired);
                Assert.AreEqual(false, requisite.IsProtected);
            }
        }
    }
}
