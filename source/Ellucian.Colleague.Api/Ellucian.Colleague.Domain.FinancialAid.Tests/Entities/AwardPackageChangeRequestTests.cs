/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardPackageChangeRequestTests
    {
        public string id;
        public string studentId;
        public string awardYearId;
        public string awardId;
        public string assignedToCounselorId;
        public DateTimeOffset? createDate;
        public List<AwardPeriodChangeRequest> awardPeriodChangeRequests;

        public AwardPackageChangeRequest awardPackageChangeRequest;


        public void AwardPackageChangeRequestTestsInitialize()
        {
            id = "1";
            studentId = "0003914";
            awardYearId = "2015";
            awardId = "LOOPY";
            assignedToCounselorId = "0015740";
            createDate = new DateTimeOffset(DateTime.Today);
            awardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
            {
                new AwardPeriodChangeRequest("foo"),
                new AwardPeriodChangeRequest("bar")
            };

            awardPackageChangeRequest = new AwardPackageChangeRequest(id, studentId, awardYearId, awardId);
        }

        [TestClass]
        public class AwardPackageChangeRequestConstructorTests : AwardPackageChangeRequestTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestTestsInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, awardPackageChangeRequest.Id);
            }

            [TestMethod]
            public void IdSetterIgnoresNewValuesIfIdNotEmpty()
            {
                awardPackageChangeRequest.Id = "foobar";
                Assert.AreEqual(id, awardPackageChangeRequest.Id);
            }

            [TestMethod]
            public void IdNotRequiredTest()
            {
                awardPackageChangeRequest = new AwardPackageChangeRequest(string.Empty, studentId, awardYearId, awardId);
                Assert.AreEqual(string.Empty, awardPackageChangeRequest.Id);
            }

            [TestMethod]
            public void IdSetterWorksIfIdInitiallyEmptyTest()
            {
                awardPackageChangeRequest = new AwardPackageChangeRequest(string.Empty, studentId, awardYearId, awardId);
                id = "foobar";
                awardPackageChangeRequest.Id = id;
                Assert.AreEqual(id, awardPackageChangeRequest.Id);
            }

            [TestMethod]
            public void StudentIdTest()
            {
                Assert.AreEqual(studentId, awardPackageChangeRequest.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                new AwardPackageChangeRequest(id, null, awardYearId, awardId);
            }

            [TestMethod]
            public void AwardYearIdTest()
            {
                Assert.AreEqual(awardYearId, awardPackageChangeRequest.AwardYearId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearIdRequiredTest()
            {
                new AwardPackageChangeRequest(id, studentId, null, awardId);
            }

            [TestMethod]
            public void AwardIdTest()
            {
                Assert.AreEqual(awardId, awardPackageChangeRequest.AwardId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardIdIdRequiredTest()
            {
                new AwardPackageChangeRequest(id, studentId, awardYearId, string.Empty);
            }

            [TestMethod]
            public void AssignedToCounselorIdGetSetTest()
            {
                awardPackageChangeRequest.AssignedToCounselorId = assignedToCounselorId;
                Assert.AreEqual(assignedToCounselorId, awardPackageChangeRequest.AssignedToCounselorId);
            }

            [TestMethod]
            public void CreateDateGetSetTest()
            {
                awardPackageChangeRequest.CreateDateTime = createDate;
                Assert.AreEqual(createDate, awardPackageChangeRequest.CreateDateTime);
            }

            [TestMethod]
            public void AwardPeriodChangeRequestsGetSetTest()
            {
                awardPackageChangeRequest.AwardPeriodChangeRequests = awardPeriodChangeRequests;
                CollectionAssert.AreEqual(awardPeriodChangeRequests, awardPackageChangeRequest.AwardPeriodChangeRequests);
            }

        }

        [TestClass]
        public class IsForStudentAwardTest : AwardPackageChangeRequestTests
        {
            public StudentAwardYear studentAwardYear;
            public Award award;
            public StudentAward studentAward;

            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestTestsInitialize();

                studentAwardYear = new StudentAwardYear(studentId, awardYearId);
                award = new Award(awardId, "foo", null);
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
            }

            [TestMethod]
            public void NullStudentAward_FalseTest()
            {
                Assert.IsFalse(awardPackageChangeRequest.IsForStudentAward(null));
            }

            [TestMethod]
            public void ChangeRequestIsForStudentAward_TrueTest()
            {
                Assert.IsTrue(awardPackageChangeRequest.IsForStudentAward(studentAward));
            }

            [TestMethod]
            public void DifferentAwardYear_FalseTest()
            {
                studentAwardYear = new StudentAwardYear(studentId, "foobar");
                award = new Award(awardId, "foo", null);
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);

                Assert.IsFalse(awardPackageChangeRequest.IsForStudentAward(studentAward));
            }

            [TestMethod]
            public void DifferentStudentId_FalseTest()
            {
                studentAwardYear = new StudentAwardYear("foobar", awardYearId);
                award = new Award(awardId, "foo", null);
                studentAward = new StudentAward(studentAwardYear, "foobar", award, false);

                Assert.IsFalse(awardPackageChangeRequest.IsForStudentAward(studentAward));
            }

            [TestMethod]
            public void DifferentAwardId_FalseTest()
            {
                studentAwardYear = new StudentAwardYear(studentId, awardYearId);
                award = new Award("foobar", "foo", null);
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);

                Assert.IsFalse(awardPackageChangeRequest.IsForStudentAward(studentAward));
            }
        }

        [TestClass]
        public class EqualsTest : AwardPackageChangeRequestTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestTestsInitialize();
            }

            [TestMethod]
            public void IdEqual_TrueTest()
            {
                var testRequest = new AwardPackageChangeRequest(id, "foo", "bar", "foobar");
                Assert.AreEqual(testRequest, awardPackageChangeRequest);
            }

            [TestMethod]
            public void IdNotEqualButOthersAreEqual_TrueTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, awardYearId, awardId);
                Assert.AreEqual(testRequest, awardPackageChangeRequest);
            }

            [TestMethod]
            public void IdAndStudentIdNotEqual_FalseTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", "foo", awardYearId, awardId);
                Assert.AreNotEqual(testRequest, awardPackageChangeRequest);
            }

            [TestMethod]
            public void IdAndAwardIdNotEqual_FalseTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, awardYearId, "foo");
                Assert.AreNotEqual(testRequest, awardPackageChangeRequest);
            }

            [TestMethod]
            public void IdAndAwardYearIdNotEqual_FalseTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, "foo", awardId);
                Assert.AreNotEqual(testRequest, awardPackageChangeRequest);
            }

            [TestMethod]
            public void HashCodeEqualTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, awardYearId, awardId);
                Assert.AreEqual(testRequest.GetHashCode(), awardPackageChangeRequest.GetHashCode());
            }

            [TestMethod]
            public void StudentIdNotEqual_HashCodeNotEqualTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", "foo", awardYearId, awardId);
                Assert.AreNotEqual(testRequest.GetHashCode(), awardPackageChangeRequest.GetHashCode());
            }

            [TestMethod]
            public void AwardYearIdNotEqual_HashCodeNotEqualTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, "foo", awardId);
                Assert.AreNotEqual(testRequest.GetHashCode(), awardPackageChangeRequest.GetHashCode());
            }

            [TestMethod]
            public void AwardIdNotEqual_HashCodeNotEqualTest()
            {
                var testRequest = new AwardPackageChangeRequest("foobar", studentId, awardYearId, "foo");
                Assert.AreNotEqual(testRequest.GetHashCode(), awardPackageChangeRequest.GetHashCode());
            }
        }
    }
}
