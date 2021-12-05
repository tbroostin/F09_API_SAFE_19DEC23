using System;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Class containing tests for the StudentLoanSummary domain object
    /// </summary>
    [TestClass]
    public class StudentLoanSummaryTests
    {
        /// <summary>
        /// StudentLoanSummary Constructor
        /// </summary>
        [TestClass]
        public class StudentLoanSummaryConstructorTests
        {
            private string studentId;

            //private bool needsSubsidizedLoanEntranceInterview;
            private DateTime? subsidizedLoanEntranceInterviewDate;

            //private bool needsGraduatePlusLoanInterview;
            private DateTime? graduatePlusLoanInterviewDate;

            //private bool needsDirectLoanMpn;
            private DateTime? directLoanMpnExpirationDate;

            //private bool needsDirectPlusLoanMpn;
            private DateTime? directPlusLoanMpnExpirationDate;

            private List<StudentLoanHistory> studentLoanHistory;

            private StudentLoanSummary studentLoanSummary;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003915";
                //needsSubsidizedLoanEntranceInterview = true;
                subsidizedLoanEntranceInterviewDate = DateTime.Today;

                //needsGraduatePlusLoanInterview = true;
                graduatePlusLoanInterviewDate = DateTime.Today;

                //needsDirectLoanMpn = true;
                directLoanMpnExpirationDate = DateTime.Today;

                //needsDirectPlusLoanMpn = true;
                directPlusLoanMpnExpirationDate = DateTime.Today;

                studentLoanHistory = new List<StudentLoanHistory>();

                studentLoanSummary = new StudentLoanSummary(studentId);

            }

            [TestMethod]
            public void NumberOfStudentLoanSummaryPropertiesTest()
            {
                var studentLoanSummaryProperties = typeof(StudentLoanSummary).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(12, studentLoanSummaryProperties.Count());
            }

            /// <summary>
            /// Verify the constructor set the studentID correctly
            /// </summary>
            [TestMethod]
            public void StudentId()
            {
                Assert.AreEqual(studentId, studentLoanSummary.StudentId);
            }

            /// <summary>
            /// Test that the constructor initializes the StudentLoanHistory list.
            /// </summary>
            [TestMethod]
            public void StudentLoanHistoryListInitializedTest()
            {
                Assert.IsTrue(studentLoanSummary.StudentLoanHistory != null);
                Assert.AreEqual(0, studentLoanSummary.StudentLoanHistory.Count);
            }

            /// <summary>
            /// StudentId is required. Verify an ArgumentNullException is thrown when creating a StudentLoanSummary with a null studentId
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdNullException()
            {
                new StudentLoanSummary(null);
            }

            [TestMethod]
            public void SubLoanInterviewDate_InitNullTest()
            {
                Assert.IsNull(studentLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public void SubLoanInterviewDate_GetSetTest()
            {
                studentLoanSummary.DirectLoanEntranceInterviewDate = subsidizedLoanEntranceInterviewDate;
                Assert.AreEqual(subsidizedLoanEntranceInterviewDate, studentLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public void GradPlusInterviewDate_InitNullTest()
            {
                Assert.IsNull(studentLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public void GradPlusInterviewDate_GetSetTest()
            {
                studentLoanSummary.GraduatePlusLoanEntranceInterviewDate = graduatePlusLoanInterviewDate;
                Assert.AreEqual(graduatePlusLoanInterviewDate, studentLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public void DlMpnExpirationDate_InitNullTest()
            {
                Assert.IsNull(studentLoanSummary.DirectLoanMpnExpirationDate);
            }

            [TestMethod]
            public void DlMpnExpirationDate_GetSetTest()
            {
                studentLoanSummary.DirectLoanMpnExpirationDate = directLoanMpnExpirationDate;
                Assert.AreEqual(directLoanMpnExpirationDate, studentLoanSummary.DirectLoanMpnExpirationDate);
            }

            [TestMethod]
            public void DlPlusMpnExpirationDate_InitNullTest()
            {
                Assert.IsNull(studentLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public void DlPlusMpnExpirationDate_GetSetTest()
            {
                studentLoanSummary.PlusLoanMpnExpirationDate = directPlusLoanMpnExpirationDate;
                Assert.AreEqual(directPlusLoanMpnExpirationDate, studentLoanSummary.PlusLoanMpnExpirationDate);
            }
        }

        [TestClass]
        public class StudentLoanSummaryEqualsTests
        {
            private string studentId;
            private StudentLoanSummary actualStudentLoanSummary;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                actualStudentLoanSummary = new StudentLoanSummary(studentId);
            }

            [TestMethod]
            public void SameStudentId_EqualsTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary(studentId);
                Assert.AreEqual(expectedStudentLoanSummary, actualStudentLoanSummary);
            }

            [TestMethod]
            public void DiffStudentId_NotEqualsTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary("foobar");
                Assert.AreNotEqual(expectedStudentLoanSummary, actualStudentLoanSummary);
            }

            [TestMethod]
            public void SameStudentIdDiffAttributes_EqualsTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary(studentId)
                {
                    DirectLoanEntranceInterviewDate = DateTime.Today,
                    GraduatePlusLoanEntranceInterviewDate = DateTime.Today,
                    DirectLoanMpnExpirationDate = DateTime.Today,
                    PlusLoanMpnExpirationDate = DateTime.Today
                };

                Assert.AreEqual(expectedStudentLoanSummary, actualStudentLoanSummary);
            }
        }

        [TestClass]
        public class StudentLoanSummaryHashCodeTests
        {
            private string studentId;
            private StudentLoanSummary actualStudentLoanSummary;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                actualStudentLoanSummary = new StudentLoanSummary(studentId);
            }

            [TestMethod]
            public void SameStudentId_SameHashCodeTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary(studentId);
                Assert.AreEqual(expectedStudentLoanSummary.GetHashCode(), actualStudentLoanSummary.GetHashCode());
            }

            [TestMethod]
            public void DiffStudentId_NotEqualsTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary("foobar");
                Assert.AreNotEqual(expectedStudentLoanSummary.GetHashCode(), actualStudentLoanSummary.GetHashCode());
            }

            [TestMethod]
            public void SameStudentIdDiffAttributes_EqualsTest()
            {
                var expectedStudentLoanSummary = new StudentLoanSummary(studentId)
                {
                    DirectLoanEntranceInterviewDate = DateTime.Today,
                    GraduatePlusLoanEntranceInterviewDate = DateTime.Today,
                    DirectLoanMpnExpirationDate = DateTime.Today,
                    PlusLoanMpnExpirationDate = DateTime.Today
                };

                Assert.AreEqual(expectedStudentLoanSummary.GetHashCode(), actualStudentLoanSummary.GetHashCode());
            }
        }
    }
}
