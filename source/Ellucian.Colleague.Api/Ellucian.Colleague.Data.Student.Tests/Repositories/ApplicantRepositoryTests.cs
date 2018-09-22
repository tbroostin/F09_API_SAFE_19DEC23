// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class ApplicantRepositoryTests
    {
        [TestClass]
        public class GetApplicantTests : BasePersonSetup
        {
            //Data contract objects returned as responses by the mocked datareaders
            private Applicants applicantsResponseData;
            private Collection<FinAid> finAidResponseData;

            //TestRepositories
            private TestApplicantRepository expectedRepository;
            private ApplicantRepository actualRepository;

            //Test data
            private Applicant expectedApplicant;
            private Applicant actualApplicant;

            //used throughout
            private string applicantId;


            [TestInitialize]
            public void Initialize()
            {
                PersonSetupInitialize();

                expectedRepository = new TestApplicantRepository();

                //setup the test data based on the person records from BasePersonSetup
                foreach (var personRecord in personRecords)
                {
                    expectedRepository.personData.Add(new TestApplicantRepository.Person()
                        {
                            Id = personRecord.Key,
                            LastName = personRecord.Value.LastName,
                            PrivacyStatusCode = personRecord.Value.PrivacyFlag
                        });
                }

                applicantId = expectedRepository.personData.First().Id;

                //setup the expected applicant
                expectedApplicant = expectedRepository.GetApplicant(applicantId);

                //set the response data object
                applicantsResponseData = new Applicants() { Recordkey = applicantId };
                finAidResponseData = BuildFinAidResponseData(expectedRepository.faStudentData);

                //build the repository
                actualRepository = BuildRepository();
            }

            private ApplicantRepository BuildRepository()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));
                var finAidResponse = finAidResponseData.FirstOrDefault(f => f.Recordkey == applicantId);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FinAid>(applicantId, true)).Returns(Task.FromResult(finAidResponse));

                return new ApplicantRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private Collection<FinAid> BuildFinAidResponseData(List<TestApplicantRepository.FaStudent> faStudentData)
            {
                var collection = new Collection<FinAid>();
                foreach (var faStudent in faStudentData)
                {
                    var finAidRecord = new FinAid()
                    {
                        Recordkey = faStudent.studentId,
                        FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                    };
                    foreach (var counselor in faStudent.faCounselors)
                    {
                        var counselorEntity = new FinAidFaCounselors()
                        {
                            FaCounselorAssocMember = counselor.counselorId,
                            FaCounselorStartDateAssocMember = counselor.startDate,
                            FaCounselorEndDateAssocMember = counselor.endDate
                        };
                        finAidRecord.FaCounselorsEntityAssociation.Add(counselorEntity);
                    }
                    finAidRecord.FaCounselor = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorAssocMember).ToList();
                    finAidRecord.FaCounselorStartDate = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorStartDateAssocMember).ToList();
                    finAidRecord.FaCounselorEndDate = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorEndDateAssocMember).ToList();

                    collection.Add(finAidRecord);
                }
                return collection;
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                localCacheMock = null;
                loggerMock = null;
                transFactoryMock = null;
                transManagerMock = null;

                applicantsResponseData = null;
                expectedRepository = null;
                actualRepository = null;
                expectedApplicant = null;
                actualApplicant = null;
                applicantId = null;
            }

            [TestMethod]
            public void EqualObjectsTest()
            {
                actualApplicant = actualRepository.GetApplicant(applicantId);
                Assert.AreEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void EqualObjectsTest_PrivacyStatusCode()
            {
                applicantId = expectedRepository.personData.Last().Id;
                expectedApplicant = expectedRepository.GetApplicant(applicantId);
                applicantsResponseData = new Applicants() { Recordkey = applicantId };
                finAidResponseData = BuildFinAidResponseData(expectedRepository.faStudentData);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));
                var finAidResponse = finAidResponseData.FirstOrDefault(f => f.Recordkey == applicantId);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FinAid>(applicantId, true)).Returns(Task.FromResult(finAidResponse));
                actualRepository = new ApplicantRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                actualApplicant = actualRepository.GetApplicant(applicantId);
                Assert.AreEqual(expectedApplicant.PrivacyStatusCode, actualApplicant.PrivacyStatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicantIdRequiredTest()
            {
                actualRepository.GetApplicant(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NullApplicantRecord_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                applicantsResponseData = null;
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));

                actualApplicant = actualRepository.GetApplicant(applicantId);
            }

            [TestMethod]
            public void NullApplicantRecord_LogsErrorTest()
            {
                var exceptionCaught = false;
                try
                {
                    //set the response data object to null and the dataReaderMock
                    applicantsResponseData = null;
                    dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));

                    actualApplicant = actualRepository.GetApplicant(applicantId);
                }
                catch
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public void CounselorId_FirstInListAssignedWithNullStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    }
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_FirstInListAssignedWithTodayInBetweenStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_FirstInListAssignedWithTodayEqualToStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today,
                        FaCounselorEndDateAssocMember = DateTime.Today
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_FirstInListAssignedWithTodayBeforeEndDate_NoStartDate()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_FirstInListAssignedWithTodayAfterStartDate_NoEndDate()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_SkipCounselorIfStartDateAfterToday()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void CounselorId_SkipCounselorIfEndDateBeforeToday()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(-1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public void NullFinAidRecord_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAidResponseData.Remove(finAid);

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

            [TestMethod]
            public void NullCounselorAssociation_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAid.FaCounselorsEntityAssociation = null;

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

            [TestMethod]
            public void EmptyCounselorList_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAid.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();

                actualRepository = BuildRepository();
                actualApplicant = actualRepository.GetApplicant(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

          
        }


        [TestClass]
        public class GetApplicantTestsAsync : BasePersonSetup
        {
            //Data contract objects returned as responses by the mocked datareaders
            private Applicants applicantsResponseData;
            private Collection<FinAid> finAidResponseData;

            //TestRepositories
            private TestApplicantRepository expectedRepository;
            private ApplicantRepository actualRepository;

            //Test data
            private Applicant expectedApplicant;
            private Applicant actualApplicant;

            //used throughout
            private string applicantId;


            [TestInitialize]
            public void Initialize()
            {
                PersonSetupInitialize();

                expectedRepository = new TestApplicantRepository();

                //setup the test data based on the person records from BasePersonSetup
                foreach (var personRecord in personRecords)
                {
                    expectedRepository.personData.Add(new TestApplicantRepository.Person()
                    {
                        Id = personRecord.Key,
                        LastName = personRecord.Value.LastName,
                        PrivacyStatusCode = personRecord.Value.PrivacyFlag
                    });
                }

                applicantId = expectedRepository.personData.First().Id;

                //setup the expected applicant
                expectedApplicant = expectedRepository.GetApplicant(applicantId);

                //set the response data object
                applicantsResponseData = new Applicants() { Recordkey = applicantId };
                finAidResponseData = BuildFinAidResponseData(expectedRepository.faStudentData);

                //build the repository
                actualRepository = BuildRepository();
            }

            private ApplicantRepository BuildRepository()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));
                var finAidResponse = finAidResponseData.FirstOrDefault(f => f.Recordkey == applicantId);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FinAid>(applicantId, true)).Returns(Task.FromResult(finAidResponse));

                return new ApplicantRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private Collection<FinAid> BuildFinAidResponseData(List<TestApplicantRepository.FaStudent> faStudentData)
            {
                var collection = new Collection<FinAid>();
                foreach (var faStudent in faStudentData)
                {
                    var finAidRecord = new FinAid()
                    {
                        Recordkey = faStudent.studentId,
                        FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                    };
                    foreach (var counselor in faStudent.faCounselors)
                    {
                        var counselorEntity = new FinAidFaCounselors()
                        {
                            FaCounselorAssocMember = counselor.counselorId,
                            FaCounselorStartDateAssocMember = counselor.startDate,
                            FaCounselorEndDateAssocMember = counselor.endDate
                        };
                        finAidRecord.FaCounselorsEntityAssociation.Add(counselorEntity);
                    }
                    finAidRecord.FaCounselor = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorAssocMember).ToList();
                    finAidRecord.FaCounselorStartDate = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorStartDateAssocMember).ToList();
                    finAidRecord.FaCounselorEndDate = finAidRecord.FaCounselorsEntityAssociation.Select(c => c.FaCounselorEndDateAssocMember).ToList();

                    collection.Add(finAidRecord);
                }
                return collection;
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                localCacheMock = null;
                loggerMock = null;
                transFactoryMock = null;
                transManagerMock = null;

                applicantsResponseData = null;
                expectedRepository = null;
                actualRepository = null;
                expectedApplicant = null;
                actualApplicant = null;
                applicantId = null;
            }

            [TestMethod]
            public async Task EqualObjectsTest()
            {
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);
                Assert.AreEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public async Task EqualObjectsTest_PrivacyStatusCode()
            {
                applicantId = expectedRepository.personData.Last().Id;
                expectedApplicant = expectedRepository.GetApplicant(applicantId);
                applicantsResponseData = new Applicants() { Recordkey = applicantId };
                finAidResponseData = BuildFinAidResponseData(expectedRepository.faStudentData);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));
                var finAidResponse = finAidResponseData.FirstOrDefault(f => f.Recordkey == applicantId);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FinAid>(applicantId, true)).Returns(Task.FromResult(finAidResponse));
                actualRepository = new ApplicantRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);
                Assert.AreEqual(expectedApplicant.PrivacyStatusCode, actualApplicant.PrivacyStatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ApplicantIdRequiredTest()
            {
                await actualRepository.GetApplicantAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullApplicantRecord_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                applicantsResponseData = null;
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));

                actualApplicant =await actualRepository.GetApplicantAsync(applicantId);
            }

            [TestMethod]
            public async Task NullApplicantRecord_LogsErrorTest()
            {
                var exceptionCaught = false;
                try
                {
                    //set the response data object to null and the dataReaderMock
                    applicantsResponseData = null;
                    dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>(applicantId, true)).Returns(Task.FromResult(applicantsResponseData));

                    actualApplicant =await actualRepository.GetApplicantAsync(applicantId);
                }
                catch
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CounselorId_FirstInListAssignedWithNullStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    }
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_FirstInListAssignedWithTodayInBetweenStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant =await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_FirstInListAssignedWithTodayEqualToStartEndDates()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today,
                        FaCounselorEndDateAssocMember = DateTime.Today
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_FirstInListAssignedWithTodayBeforeEndDate_NoStartDate()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = null,
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_FirstInListAssignedWithTodayAfterStartDate_NoEndDate()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-1),
                        FaCounselorEndDateAssocMember = null
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_SkipCounselorIfStartDateAfterToday()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(1),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task CounselorId_SkipCounselorIfEndDateBeforeToday()
            {
                var expectedCounselorId = "1234567";
                finAidResponseData.First(f => f.Recordkey == applicantId).FaCounselorsEntityAssociation = new List<FinAidFaCounselors>()
                {
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = "foobar",
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(-1)
                    },
                    new FinAidFaCounselors()
                    {
                        FaCounselorAssocMember = expectedCounselorId,
                        FaCounselorStartDateAssocMember = DateTime.Today.AddDays(-2),
                        FaCounselorEndDateAssocMember = DateTime.Today.AddDays(2)
                    }                    
                };

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.AreEqual(expectedCounselorId, actualApplicant.FinancialAidCounselorId);
            }

            [TestMethod]
            public async Task NullFinAidRecord_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAidResponseData.Remove(finAid);

                actualRepository = BuildRepository();
                actualApplicant =await actualRepository.GetApplicantAsync(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

            [TestMethod]
            public async Task NullCounselorAssociation_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAid.FaCounselorsEntityAssociation = null;

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

            [TestMethod]
            public async Task EmptyCounselorList_NoCounselorIdTest()
            {
                var finAid = finAidResponseData.First(f => f.Recordkey == applicantId);
                finAid.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();

                actualRepository = BuildRepository();
                actualApplicant = await actualRepository.GetApplicantAsync(applicantId);

                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }
        }
    }
}
