// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentQuickRegistrationRepositoryTests : BaseRepositorySetup
    {
        private string _studentId;
        private StudentQuickRegistrationRepository _repository;

        [TestInitialize]
        public void StudentQuickRegistrationRepositoryTests_Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Set up data reads
            StudentQuickRegistrationRepository_DataReader_Setup();

            // Build the test repository
            _repository = new StudentQuickRegistrationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_Tests : StudentQuickRegistrationRepositoryTests
        {
            [TestInitialize]
            public void StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_Initialize()
            {
                base.StudentQuickRegistrationRepositoryTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_null_StudentId_throws_ArgumentNullException()
            {
                var data = await _repository.GetStudentQuickRegistrationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_empty_StudentId_throws_ArgumentNullException()
            {
                var data = await _repository.GetStudentQuickRegistrationAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_caught_exception_throws_ApplicationException()
            {
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception("Exception thrown by data reader!"));
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_null_BulkRecordsRead_returns_StudentQuickRegistration_with_no_terms()
            {
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = null, InvalidKeys = null, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);
                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(0, data.Terms.Count);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_empty_BulkRecordsRead_returns_StudentQuickRegistration_with_no_terms()
            {
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = new System.Collections.ObjectModel.Collection<DegreePlanTerms>(), InvalidKeys = null, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);
                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(0, data.Terms.Count);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_InvalidKeys_logged_but_returns_StudentQuickRegistration_with_valid_terms()
            {
                //DEGREE_PLAN_TERMS
                var dpt = new Collection<DegreePlanTerms>()
            {
                    new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                    {
                        Recordkey = "1",
                        DptTerm = null,
                        DptSections = null
                    },
                    new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                    {
                        Recordkey = "2",
                        DptTerm = string.Empty,
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "3",
                        DptTerm = "NULLSEC",
                        DptSections = null
                    },
                    new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "4",
                        DptTerm = "EMPTYSEC",
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms()
                    {
                        Recordkey = "5",
                        DptTerm = "VALID",
                        DptSections = new List<string>()
                        {
                            null,
                            string.Empty,
                            "123",
                            "124",
                            "125",
                            "126"
                        },
                        DptCredits = new List<decimal?>()
                        {
                            null,
                            null,
                            3,
                            4,
                            null,
                            5
                        },
                        DptGradingType = new List<string>()
                        {
                            null,
                            string.Empty,
                            "G",
                            "P",
                            "A",
                            "X"
                        },
                        PlannedCoursesEntityAssociation = new List<DegreePlanTermsPlannedCourses>()
                        {
                            null, // Nulls should be handled gracefully
                            new DegreePlanTermsPlannedCourses(), // Empties should be handled gracefully,
                            new DegreePlanTermsPlannedCourses("223", "123", 3, null, "G", "0001234", DateTime.Today, DateTime.Now.AddHours(-1), "N"),
                            new DegreePlanTermsPlannedCourses("224", "124", 4, null, "P", "0001234", DateTime.Today, DateTime.Now.AddHours(-2), "N"),
                            new DegreePlanTermsPlannedCourses("225", "125", null, null, "A", "0001234", DateTime.Today, DateTime.Now.AddHours(-3), "N"),
                            new DegreePlanTermsPlannedCourses("226", "126", 5, null, "X", "0001234", DateTime.Today, DateTime.Now.AddHours(-4), "N"),
                        }
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = dpt, InvalidKeys = new string[] { "6" }, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);
                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(1, data.Terms.Count); // Only term "5" has at least one valid section
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_valid_data_returns_StudentQuickRegistration_with_valid_terms()
            {
                //DEGREE_PLAN_TERMS
                var dpt = new Collection<DegreePlanTerms>()
            {
                    new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                    {
                        Recordkey = "1",
                        DptTerm = null,
                        DptSections = null
                    },
                    new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                    {
                        Recordkey = "2",
                        DptTerm = string.Empty,
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "3",
                        DptTerm = "NULLSEC",
                        DptSections = null
                    },
                    new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "4",
                        DptTerm = "EMPTYSEC",
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms()
                    {
                        Recordkey = "5",
                        DptTerm = "VALID",
                        DptSections = new List<string>()
                        {
                            null,
                            string.Empty,
                            "123",
                            "124",
                            "125",
                            "126"
                        },
                        DptCredits = new List<decimal?>()
                        {
                            null,
                            null,
                            3,
                            4,
                            null,
                            5
                        },
                        DptGradingType = new List<string>()
                        {
                            null,
                            string.Empty,
                            "G",
                            "P",
                            "A",
                            "X"
                        },
                        PlannedCoursesEntityAssociation = new List<DegreePlanTermsPlannedCourses>()
                        {
                            null, // Nulls should be handled gracefully
                            new DegreePlanTermsPlannedCourses(), // Empties should be handled gracefully,
                            new DegreePlanTermsPlannedCourses("223", "123", 3, null, "G", "0001234", DateTime.Today, DateTime.Now.AddHours(-1), "N"),
                            new DegreePlanTermsPlannedCourses("224", "124", 4, null, "P", "0001234", DateTime.Today, DateTime.Now.AddHours(-2), "N"),
                            new DegreePlanTermsPlannedCourses("225", "125", null, null, "A", "0001234", DateTime.Today, DateTime.Now.AddHours(-3), "N"),
                            new DegreePlanTermsPlannedCourses("226", "126", 5, null, "X", "0001234", DateTime.Today, DateTime.Now.AddHours(-4), "N"),
                        }
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = dpt, InvalidKeys = null, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);

                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(1, data.Terms.Count); // Only term "5" has at least one valid section
                Assert.IsNotNull(data.Terms[0].Sections);
                Assert.AreEqual(4, data.Terms[0].Sections.Count);

                Assert.AreEqual("123", data.Terms[0].Sections[0].SectionId);
                Assert.AreEqual(3m, data.Terms[0].Sections[0].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[0].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.Active, data.Terms[0].Sections[0].WaitlistStatus);

                Assert.AreEqual("124", data.Terms[0].Sections[1].SectionId);
                Assert.AreEqual(4m, data.Terms[0].Sections[1].Credits);
                Assert.AreEqual(GradingType.PassFail, data.Terms[0].Sections[1].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister, data.Terms[0].Sections[1].WaitlistStatus);

                Assert.AreEqual("125", data.Terms[0].Sections[2].SectionId);
                Assert.AreEqual(null, data.Terms[0].Sections[2].Credits);
                Assert.AreEqual(GradingType.Audit, data.Terms[0].Sections[2].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[2].WaitlistStatus);

                Assert.AreEqual("126", data.Terms[0].Sections[3].SectionId);
                Assert.AreEqual(5m, data.Terms[0].Sections[3].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[3].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[3].WaitlistStatus);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_null_BulkRecordsRead_Waitlist_returns__returns_StudentQuickRegistration_with_valid_terms()
            {
                dataReaderMock.Setup<Task<BulkReadOutput<WaitList>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<WaitList>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<WaitList>() { BulkRecordsRead = null, InvalidKeys = null, InvalidRecords = null });
                //DEGREE_PLAN_TERMS
                var dpt = new Collection<DegreePlanTerms>()
                {
                    new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                    {
                        Recordkey = "1",
                        DptTerm = null,
                        DptSections = null
                    },
                    new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                    {
                        Recordkey = "2",
                        DptTerm = string.Empty,
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "3",
                        DptTerm = "NULLSEC",
                        DptSections = null
                    },
                    new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "4",
                        DptTerm = "EMPTYSEC",
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms()
                    {
                        Recordkey = "5",
                        DptTerm = "VALID",
                        DptSections = new List<string>()
                        {
                            null,
                            string.Empty,
                            "123",
                            "124",
                            "125",
                            "126"
                        },
                        DptCredits = new List<decimal?>()
                        {
                            null,
                            null,
                            3,
                            4,
                            null,
                            5
                        },
                        DptGradingType = new List<string>()
                        {
                            null,
                            string.Empty,
                            "G",
                            "P",
                            "A",
                            "X"
                        },
                        PlannedCoursesEntityAssociation = new List<DegreePlanTermsPlannedCourses>()
                        {
                            null, // Nulls should be handled gracefully
                            new DegreePlanTermsPlannedCourses(), // Empties should be handled gracefully,
                            new DegreePlanTermsPlannedCourses("223", "123", 3, null, "G", "0001234", DateTime.Today, DateTime.Now.AddHours(-1), "N"),
                            new DegreePlanTermsPlannedCourses("224", "124", 4, null, "P", "0001234", DateTime.Today, DateTime.Now.AddHours(-2), "N"),
                            new DegreePlanTermsPlannedCourses("225", "125", null, null, "A", "0001234", DateTime.Today, DateTime.Now.AddHours(-3), "N"),
                            new DegreePlanTermsPlannedCourses("226", "126", 5, null, "X", "0001234", DateTime.Today, DateTime.Now.AddHours(-4), "N"),
                        }
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = dpt, InvalidKeys = null, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);

                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(1, data.Terms.Count); // Only term "5" has at least one valid section
                Assert.IsNotNull(data.Terms[0].Sections);
                Assert.AreEqual(4, data.Terms[0].Sections.Count);

                Assert.AreEqual("123", data.Terms[0].Sections[0].SectionId);
                Assert.AreEqual(3m, data.Terms[0].Sections[0].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[0].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[0].WaitlistStatus);

                Assert.AreEqual("124", data.Terms[0].Sections[1].SectionId);
                Assert.AreEqual(4m, data.Terms[0].Sections[1].Credits);
                Assert.AreEqual(GradingType.PassFail, data.Terms[0].Sections[1].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[1].WaitlistStatus);

                Assert.AreEqual("125", data.Terms[0].Sections[2].SectionId);
                Assert.AreEqual(null, data.Terms[0].Sections[2].Credits);
                Assert.AreEqual(GradingType.Audit, data.Terms[0].Sections[2].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[2].WaitlistStatus);

                Assert.AreEqual("126", data.Terms[0].Sections[3].SectionId);
                Assert.AreEqual(5m, data.Terms[0].Sections[3].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[3].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[3].WaitlistStatus);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_empty_BulkRecordsRead_Waitlist_returns__returns_StudentQuickRegistration_with_valid_terms()
            {
                dataReaderMock.Setup<Task<BulkReadOutput<WaitList>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<WaitList>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<WaitList>() { BulkRecordsRead = new System.Collections.ObjectModel.Collection<WaitList>(), InvalidKeys = null, InvalidRecords = null });
                //DEGREE_PLAN_TERMS
                var dpt = new Collection<DegreePlanTerms>()
                {
                    new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                    {
                        Recordkey = "1",
                        DptTerm = null,
                        DptSections = null
                    },
                    new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                    {
                        Recordkey = "2",
                        DptTerm = string.Empty,
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "3",
                        DptTerm = "NULLSEC",
                        DptSections = null
                    },
                    new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "4",
                        DptTerm = "EMPTYSEC",
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms()
                    {
                        Recordkey = "5",
                        DptTerm = "VALID",
                        DptSections = new List<string>()
                        {
                            null,
                            string.Empty,
                            "123",
                            "124",
                            "125",
                            "126"
                        },
                        DptCredits = new List<decimal?>()
                        {
                            null,
                            null,
                            3,
                            4,
                            null,
                            5
                        },
                        DptGradingType = new List<string>()
                        {
                            null,
                            string.Empty,
                            "G",
                            "P",
                            "A",
                            "X"
                        },
                        PlannedCoursesEntityAssociation = new List<DegreePlanTermsPlannedCourses>()
                        {
                            null, // Nulls should be handled gracefully
                            new DegreePlanTermsPlannedCourses(), // Empties should be handled gracefully,
                            new DegreePlanTermsPlannedCourses("223", "123", 3, null, "G", "0001234", DateTime.Today, DateTime.Now.AddHours(-1), "N"),
                            new DegreePlanTermsPlannedCourses("224", "124", 4, null, "P", "0001234", DateTime.Today, DateTime.Now.AddHours(-2), "N"),
                            new DegreePlanTermsPlannedCourses("225", "125", null, null, "A", "0001234", DateTime.Today, DateTime.Now.AddHours(-3), "N"),
                            new DegreePlanTermsPlannedCourses("226", "126", 5, null, "X", "0001234", DateTime.Today, DateTime.Now.AddHours(-4), "N"),
                        }
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = dpt, InvalidKeys = null, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);

                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(1, data.Terms.Count); // Only term "5" has at least one valid section
                Assert.IsNotNull(data.Terms[0].Sections);
                Assert.AreEqual(4, data.Terms[0].Sections.Count);

                Assert.AreEqual("123", data.Terms[0].Sections[0].SectionId);
                Assert.AreEqual(3m, data.Terms[0].Sections[0].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[0].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[0].WaitlistStatus);

                Assert.AreEqual("124", data.Terms[0].Sections[1].SectionId);
                Assert.AreEqual(4m, data.Terms[0].Sections[1].Credits);
                Assert.AreEqual(GradingType.PassFail, data.Terms[0].Sections[1].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[1].WaitlistStatus);

                Assert.AreEqual("125", data.Terms[0].Sections[2].SectionId);
                Assert.AreEqual(null, data.Terms[0].Sections[2].Credits);
                Assert.AreEqual(GradingType.Audit, data.Terms[0].Sections[2].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[2].WaitlistStatus);

                Assert.AreEqual("126", data.Terms[0].Sections[3].SectionId);
                Assert.AreEqual(5m, data.Terms[0].Sections[3].Credits);
                Assert.AreEqual(GradingType.Graded, data.Terms[0].Sections[3].GradingType);
                Assert.AreEqual(Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, data.Terms[0].Sections[3].WaitlistStatus);
            }

            [TestMethod]
            public async Task StudentQuickRegistrationRepository_GetStudentQuickRegistrationAsync_InvalidKeys_Waitlist_logged_but_returns_StudentQuickRegistration_with_valid_terms()
            {
                //DEGREE_PLAN_TERMS
                var dpt = new Collection<DegreePlanTerms>()
            {
                    new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                    {
                        Recordkey = "1",
                        DptTerm = null,
                        DptSections = null
                    },
                    new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                    {
                        Recordkey = "2",
                        DptTerm = string.Empty,
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "3",
                        DptTerm = "NULLSEC",
                        DptSections = null
                    },
                    new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                    {
                        Recordkey = "4",
                        DptTerm = "EMPTYSEC",
                        DptSections = new List<string>()
                    },
                    new DegreePlanTerms()
                    {
                        Recordkey = "5",
                        DptTerm = "VALID",
                        DptSections = new List<string>()
                        {
                            null,
                            string.Empty,
                            "123",
                            "124",
                            "125",
                            "126"
                        },
                        DptCredits = new List<decimal?>()
                        {
                            null,
                            null,
                            3,
                            4,
                            null,
                            5
                        },
                        DptGradingType = new List<string>()
                        {
                            null,
                            string.Empty,
                            "G",
                            "P",
                            "A",
                            "X"
                        },
                        PlannedCoursesEntityAssociation = new List<DegreePlanTermsPlannedCourses>()
                        {
                            null, // Nulls should be handled gracefully
                            new DegreePlanTermsPlannedCourses(), // Empties should be handled gracefully,
                            new DegreePlanTermsPlannedCourses("223", "123", 3, null, "G", "0001234", DateTime.Today, DateTime.Now.AddHours(-1), "N"),
                            new DegreePlanTermsPlannedCourses("224", "124", 4, null, "P", "0001234", DateTime.Today, DateTime.Now.AddHours(-2), "N"),
                            new DegreePlanTermsPlannedCourses("225", "125", null, null, "A", "0001234", DateTime.Today, DateTime.Now.AddHours(-3), "N"),
                            new DegreePlanTermsPlannedCourses("226", "126", 5, null, "X", "0001234", DateTime.Today, DateTime.Now.AddHours(-4), "N"),
                        }
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<DegreePlanTerms>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<DegreePlanTerms>() { BulkRecordsRead = dpt, InvalidKeys = null, InvalidRecords = null });

                // WAIT.LIST
                var waitList = new Collection<WaitList>()
                {
                    new WaitList()
                    {
                        Recordkey = "123",
                        RecordGuid = "12300000-0000-0000-0000-000000000123",
                        WaitCourseSection = "123",
                        WaitStatus = "A",
                        WaitStudent = _studentId
                    },
                    new WaitList()
                    {
                        Recordkey = "124",
                        RecordGuid = "12400000-0000-0000-0000-000000000124",
                        WaitCourseSection = "124",
                        WaitStatus = "P",
                        WaitStudent = _studentId
                    },
                    new WaitList()
                    {
                        Recordkey = "125",
                        RecordGuid = "12500000-0000-0000-0000-000000000125",
                        WaitCourseSection = "125",
                        WaitStatus = "E",
                        WaitStudent = _studentId
                    }
                };
                dataReaderMock.Setup<Task<BulkReadOutput<WaitList>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<WaitList>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new BulkReadOutput<WaitList>() { BulkRecordsRead = waitList, InvalidKeys = new string[] { "126" }, InvalidRecords = null });
                var data = await _repository.GetStudentQuickRegistrationAsync(_studentId);
                Assert.AreEqual(_studentId, data.StudentId);
                Assert.AreEqual(1, data.Terms.Count); // Only term "5" has at least one valid section
            }

        }

        /// <summary>
        /// Set up data reads
        /// </summary>
        private void StudentQuickRegistrationRepository_DataReader_Setup()
        {
            _studentId = "0001234";

            // STWEB.DEFAULTS
            var stwebDefaults = new StwebDefaults()
            {
                Recordkey = "STWEB.DEFAULTS",
                StwebEnableQuickReg = "Y",
                StwebQuickRegTerms = new List<string>() { string.Empty, null, "2019/FA", "2020/SP", "2020/SP" }
            };
            MockRecordsAsync<Student.DataContracts.StwebDefaults>("ST.PARMS", new List<Student.DataContracts.StwebDefaults>() { stwebDefaults });

            // REG.DEFAULTS
            var regDefaults = new RegDefaults()
            {
                Recordkey = "REG.DEFAULTS",
                RgdRequireAddAuthFlag = "Y",
                RgdAddAuthStartOffset = 0
            };
            MockRecordsAsync<Student.DataContracts.RegDefaults>("ST.PARMS", new List<Student.DataContracts.RegDefaults>() { regDefaults });

            //DEGREE_PLAN_TERMS
            var dpt = new List<DegreePlanTerms>()
            {
                new DegreePlanTerms() // Null term code will not be included in StudentQuickRegistration
                {
                    Recordkey = "1",
                    DptTerm = null,
                    DptSections = null
                },
                new DegreePlanTerms() // Empty term code term will not be included in StudentQuickRegistration
                {
                    Recordkey = "2",
                    DptTerm = string.Empty,
                    DptSections = new List<string>()
                },
                new DegreePlanTerms() // Term with null sections will not be included in StudentQuickRegistration
                {
                    Recordkey = "3",
                    DptTerm = "NULLSEC",
                    DptSections = null
                },
                new DegreePlanTerms() // Term with no sections will not be included in StudentQuickRegistration
                {
                    Recordkey = "4",
                    DptTerm = "EMPTYSEC",
                    DptSections = new List<string>()
                },
                new DegreePlanTerms()
                {
                    Recordkey = "5",
                    DptTerm = "VALID",
                    DptSections = new List<string>()
                    {
                        null,
                        string.Empty,
                        "123",
                        "124",
                        "125",
                        "126"
                    },
                    DptCredits = new List<decimal?>()
                    {
                        null,
                        null,
                        3,
                        4,
                        null,
                        5
                    },
                    DptGradingType = new List<string>()
                    {
                        null,
                        string.Empty,
                        "G",
                        "P",
                        "A",
                        "X"
                    }
                }
            };
            MockRecordsAsync<Student.DataContracts.DegreePlanTerms>("DEGREE_PLAN_TERMS", dpt);

            //WAIT.LIST
            var waitList = new List<WaitList>()
            {
                new WaitList()
                {
                    Recordkey = "123",
                    RecordGuid = "12300000-0000-0000-0000-000000000123",
                    WaitCourseSection = "123",
                    WaitStatus = "A",
                    WaitStudent = _studentId
                },
                new WaitList()
                {
                    Recordkey = "124",
                    RecordGuid = "12400000-0000-0000-0000-000000000124",
                    WaitCourseSection = "124",
                    WaitStatus = "P",
                    WaitStudent = _studentId
                },
                new WaitList()
                {
                    Recordkey = "125",
                    RecordGuid = "12500000-0000-0000-0000-000000000125",
                    WaitCourseSection = "125",
                    WaitStatus = "E",
                    WaitStudent = _studentId
                }
            };

            MockRecordsAsync<Student.DataContracts.WaitList>("WAIT.LIST", waitList);
            dataReaderMock.Setup(dc => dc.SelectAsync("WAIT.LIST", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "123", "124", "125" });

            // WAIT.LIST.STATUSES
            ApplValcodes waitListStatuses = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValActionCode1AssocMember = "2"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = "4"},
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"}}
            };
            dataReaderMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", It.IsAny<bool>())).ReturnsAsync(waitListStatuses);
        }
    }
}
