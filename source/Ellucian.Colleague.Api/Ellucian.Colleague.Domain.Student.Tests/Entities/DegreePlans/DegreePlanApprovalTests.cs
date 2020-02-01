// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class DegreePlanApprovalTests
    {
        [TestClass]
        public class DegreePlanApprovalConstructor
        {
            private string personId;
            private DegreePlanApprovalStatus status;
            private DateTime timeStamp;
            private string termCode;
            private string courseId;
            private DegreePlanApproval approval;

            [TestInitialize]
            public void Initialize()
            {
                personId = "1111111";
                status = DegreePlanApprovalStatus.Approved;
                timeStamp = DateTime.Now;
                termCode = "2012/FA";
                courseId = "123";
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            public void PersonId()
            {
                Assert.AreEqual(personId, approval.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdThrowsExceptionIfNull()
            {
                personId = null;
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdThrowsExceptionIfEmpty()
            {
                personId = "";
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            public void Status()
            {
                Assert.AreEqual(status, approval.Status);
            }

            [TestMethod]
            public void Date()
            {
                // Can't exactly check the time stamp, but verify that it was set
                // to the current date/time when the approval item was constructed.
                Assert.IsTrue(timeStamp <= approval.Date);
                Assert.IsTrue(DateTime.Now >= approval.Date);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseIdThrowsExceptionIfNull()
            {
                courseId = null;
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseIdThrowsExceptionIfEmpty()
            {
                courseId = "";
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            public void CourseId()
            {
                Assert.AreEqual(courseId, approval.CourseId);
            }

            [TestMethod]
            public void TermCode()
            {
                Assert.AreEqual(termCode, approval.TermCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermCodeThrowsExceptionIfNull()
            {
                termCode = null;
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermCodeThrowsExceptionIfEmpty()
            {
                termCode = "";
                approval = new DegreePlanApproval(personId, status, timeStamp, courseId, termCode);
            }
        }
    }

}
