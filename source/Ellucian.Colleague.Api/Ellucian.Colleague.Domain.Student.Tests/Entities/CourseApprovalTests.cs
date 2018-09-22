// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseApprovalTests
    {
        private string statusCode;
        private DateTime statusDate;
        private string approvingAgencyId;
        private string approvingPersonId;
        private DateTime approvalDate;
        private CourseStatus status;

        private CourseApproval approval;

        [TestClass]
        public class CourseApprovalConstructor : CourseApprovalTests
        {
            [TestInitialize]
            public void Initialize()
            {
                statusCode = "A";
                statusDate = DateTime.Today.AddDays(-3);
                approvingAgencyId = "0000043";
                approvingPersonId = "0003315";
                approvalDate = DateTime.Today;
                status = CourseStatus.Active;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseApprovalConstructorNullStatusCode()
            {
                approval = new CourseApproval(null, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseApprovalConstructorEmptyStatusCode()
            {
                approval = new CourseApproval(string.Empty, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
            }

            [TestMethod]
            public void CourseApprovalConstructorValidStatusCode()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(statusCode, approval.StatusCode);
            }

            [TestMethod]
            public void CourseApprovalConstructorValidStatusDate()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(statusDate, approval.StatusDate);
            }

            [TestMethod]
            public void CourseApprovalConstructorNullApprovingAgencyIdAndValidApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, null, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(null, approval.ApprovingAgencyId);
            }

            [TestMethod]
            public void CourseApprovalConstructorEmptyApprovingAgencyIdAndValidApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, string.Empty, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(string.Empty, approval.ApprovingAgencyId);
            }

            [TestMethod]
            public void CourseApprovalConstructorValidApprovingAgencyIdAndValidApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(approvingAgencyId, approval.ApprovingAgencyId);
                Assert.AreEqual(approvingPersonId, approval.ApprovingPersonId);
            }

            [TestMethod]
            public void CourseApprovalConstructorValidApprovingAgencyIdAndNullApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, null, approvalDate) { Status = status };
                Assert.AreEqual(approvingAgencyId, approval.ApprovingAgencyId);
            }

            [TestMethod]
            public void CourseApprovalConstructorValidApprovingAgencyIdAndEmptyApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, string.Empty, approvalDate) { Status = status };
                Assert.AreEqual(approvingAgencyId, approval.ApprovingAgencyId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CourseApprovalConstructorNullApprovingAgencyIdAndNullApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, null, null, approvalDate) { Status = status };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CourseApprovalConstructorEmptyApprovingAgencyIdAndEmptyApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, string.Empty, string.Empty, approvalDate) { Status = status };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CourseApprovalConstructorNullApprovingAgencyIdAndEmptyApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, null, string.Empty, approvalDate) { Status = status };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CourseApprovalConstructorEmptyApprovingAgencyIdAndNullApprovingPersonId()
            {
                approval = new CourseApproval(statusCode, statusDate, string.Empty, null, approvalDate) { Status = status };
            }

            [TestMethod]
            public void CourseApprovalConstructorValidApprovalDate()
            {
                approval = new CourseApproval(statusCode, statusDate, approvingAgencyId, approvingPersonId, approvalDate) { Status = status };
                Assert.AreEqual(approvalDate, approval.Date);
            }
        }
    }
}
