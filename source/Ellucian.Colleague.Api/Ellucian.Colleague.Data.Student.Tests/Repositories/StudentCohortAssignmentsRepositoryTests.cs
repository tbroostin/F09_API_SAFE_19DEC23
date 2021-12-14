// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentCohortAssignmentsRepositoryTests
    {
        [TestClass]
        public class StudentCohortAssignmentsRepoTests : BasePersonSetup
        {
            IStudentCohortAssignmentsRepository studentCohortAssignmentsRepository;
            Collection<Students> students;
            Collection<StudentAcadLevels> studentAcadLevels;
            //protected ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                // Setup mock repositories
                base.MockInitialize();
                BuildData();
                studentCohortAssignmentsRepository = BuildValidStudentChargeRepository();
            }

            private void BuildData()
            {
                students = new Collection<Students>
                {
                    new Students(){ Recordkey = "1746", StuAcadLevels = new List<string>(){"UG", "GR"}},
                    new Students(){ Recordkey = "2746", StuAcadLevels = new List<string>(){"UG"}}
                };
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Students>(It.IsAny<string>(), true)).ReturnsAsync(students);

                studentAcadLevels = new Collection<StudentAcadLevels>()
                {
                    new StudentAcadLevels()
                    {
                        RecordGuid = "18d2e01f-8fa3-49c5-941d-5b37e03204ef",
                        Recordkey = "1746*UG",
                        StaFedCohortGroup = "2009",
                        StaOtherCohortStartDates = new List<DateTime?>(){ DateTime.Today.AddDays(-30) },
                        StaOtherCohortEndDates = new List<DateTime?>(){ DateTime.Today.AddDays(30) },
                        StaOtherCohortGroups = new List<string>(){ "ATHL", "SORO" },
                        StaOtherCohortsEntityAssociation = new List<StudentAcadLevelsStaOtherCohorts>()
                        {
                            new StudentAcadLevelsStaOtherCohorts()
                            {
                                StaOtherCohortGroupsAssocMember = "ATHL",
                                StaOtherCohortStartDatesAssocMember = DateTime.Today.AddDays(-30),
                                StaOtherCohortEndDatesAssocMember = DateTime.Today.AddDays(30)                                
                            },
                            new StudentAcadLevelsStaOtherCohorts()
                            {
                                StaOtherCohortGroupsAssocMember = "SORO",
                                StaOtherCohortStartDatesAssocMember = DateTime.Today.AddDays(-30),
                                StaOtherCohortEndDatesAssocMember = DateTime.Today.AddDays(30)
                            }
                        }
                    }
                };
                //string[] keys = new string[]{ "1746*UG", "1746*GR", "2746*UG" };
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<StudentAcadLevels>(It.IsAny<string[]>(), true)).ReturnsAsync(studentAcadLevels);


                LdmGuid ldmGuid = new LdmGuid()
                {
                    LdmGuidEntity = "STUDENT.ACAD.LEVELS",
                    LdmGuidPrimaryKey = "1746*UG",
                    LdmGuidSecondaryKey = "2009",
                    LdmGuidSecondaryFld = "STA.FED.COHORT.GROUP"
                };

                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<LdmGuid>("LDM.GUID", "38942157-824a-4e49-a277-613f24852be6", true)).ReturnsAsync(ldmGuid);
                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<StudentAcadLevels>("1746*UG", true))
                    .ReturnsAsync(studentAcadLevels.FirstOrDefault());

                RecordKeyLookupResult result = new RecordKeyLookupResult()
                {
                    Guid = "38942157-824a-4e49-a277-613f24852be6",
                    ModelName = "STUDENT.ACAD.LEVELS+1746*UG+2009"
                };
                Dictionary<string, RecordKeyLookupResult> dict1 = new Dictionary<string, RecordKeyLookupResult>();
                dict1.Add("STUDENT.ACAD.LEVELS+1746*UG+2009", result);
                RecordKeyLookupResult result1 = new RecordKeyLookupResult()
                {
                    Guid = "18d2e01f-8fa3-49c5-941d-5b37e03204ef",
                    ModelName = "STUDENT.ACAD.LEVELS+ATHL*14834*" + DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()
                };
                RecordKeyLookupResult result2 = new RecordKeyLookupResult()
                {
                    Guid = "348612b7-3795-4153-892f-11094a83095f",
                    ModelName = "STUDENT.ACAD.LEVELS+SORO*14742*" + DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()
                };
                Dictionary<string, RecordKeyLookupResult> dict2 = new Dictionary<string, RecordKeyLookupResult>();
                dict2.Add(String.Concat("STUDENT.ACAD.LEVELS+1746*UG+ATHL*", DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()), result1);
                dict1.Add(String.Concat("STUDENT.ACAD.LEVELS+1746*UG+SORO*", DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()), result2);

                dataReaderMock.SetupSequence(s => s.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(dict1))
                    .Returns(Task.FromResult(dict2));

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllStudentCohortAssignments",
                    Entity = "",
                    Sublist = new List<string>(){ "1746" , "2746" },
                    TotalCount = 2,
                    KeyCacheInfo = new List<KeyCacheInfo>()
               {
                   new KeyCacheInfo()
                   {
                       KeyCacheMax = 5905,
                       KeyCacheMin = 1,
                       KeyCachePart = "000",
                       KeyCacheSize = 5905
                   },
                   new KeyCacheInfo()
                   {
                       KeyCacheMax = 7625,
                       KeyCacheMin = 5906,
                       KeyCachePart = "001",
                       KeyCacheSize = 1720
                   }
               }
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

            }

            private IStudentCohortAssignmentsRepository BuildValidStudentChargeRepository()
            {
                // Set up data accessor for mocking 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Construct repository
                studentCohortAssignmentsRepository = new StudentCohortAssignmentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentCohortAssignmentsRepository;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentCohortAssignmentsRepository = null;
                students = null;
                studentAcadLevels = null;
                this.MockCleanup();
            }


            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentsAsync()
            {
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentsAsync(0, 10, It.IsAny<StudentCohortAssignment>());
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentsAsync_WithFedFilter()
            {
                StudentCohortAssignment sca = new StudentCohortAssignment()
                {
                    AcadLevel = "UG",
                    CohortId = "2009",
                    CohortType = "FED",
                    PersonId = "1746",
                    StartOn = DateTime.Today.AddDays(-30)
                };
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentsAsync(0, 10, sca);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentsAsync_WithINSTITUTIONFilter()
            {
                StudentCohortAssignment sca = new StudentCohortAssignment()
                {
                    AcadLevel = "UG",
                    CohortId = "ATHL",
                    CohortType = "INSTITUTION",
                    PersonId = "1746",
                    StartOn = DateTime.Today.AddDays(-30),
                    EndOn = DateTime.Today.AddDays(30)
                };
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentsAsync(0, 10, sca);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_FedCohort()
            {
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("38942157-824a-4e49-a277-613f24852be6");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_OtherCohort()
            {
                LdmGuid ldmGuid = new LdmGuid()
                {
                    LdmGuidEntity = "STUDENT.ACAD.LEVELS",
                    LdmGuidPrimaryKey = "1746*UG",
                    LdmGuidSecondaryKey = string.Concat("ATHL*", DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()),
                    LdmGuidSecondaryFld = "STA.OTHER.COHORTS.IDX"
                };

                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<LdmGuid>("LDM.GUID", "38942157-824a-4e49-a277-613f24852be6", true)).ReturnsAsync(ldmGuid);
                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<StudentAcadLevels>("1746*UG", true))
                    .ReturnsAsync(studentAcadLevels.FirstOrDefault());
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("38942157-824a-4e49-a277-613f24852be6");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_FedCohortFilter()
            {
                LdmGuid ldmGuid = new LdmGuid()
                {
                    LdmGuidEntity = "STUDENT.ACAD.LEVELS",
                    LdmGuidPrimaryKey = "1746*UG",
                    LdmGuidSecondaryKey = "2009",
                    LdmGuidSecondaryFld = "STA.FED.COHORT.GROUP"
                };

                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<LdmGuid>("LDM.GUID", "38942157-824a-4e49-a277-613f24852be6", true)).ReturnsAsync(ldmGuid);
                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<StudentAcadLevels>("1746*UG", true))
                    .ReturnsAsync(studentAcadLevels.FirstOrDefault());
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("38942157-824a-4e49-a277-613f24852be6");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_ArgumentNullException()
            {
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_KeyNotFoundException()
            {
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("38942157-824a-4e49-a277-613f24852be5");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentCohortAssignmentsRepository_GetStudentCohortAssignmentByGuidAsync_KeyNotFoundException2()
            {
                LdmGuid ldmGuid = new LdmGuid()
                {
                    LdmGuidEntity = "STUDENT.ACAD.LEVEL",
                    LdmGuidPrimaryKey = "1746*UG",
                    LdmGuidSecondaryKey = string.Concat("ATHL*", DmiString.DateTimeToPickDate(DateTime.Today.AddDays(-30)).ToString()),
                    LdmGuidSecondaryFld = "STA.OTHER.COHORTS.IDX"
                };

                dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<LdmGuid>("LDM.GUID", "38942157-824a-4e49-a277-613f24852be6", true)).ReturnsAsync(ldmGuid);
               // dataReaderMock.Setup(ldm => ldm.ReadRecordAsync<StudentAcadLevels>("1746*UG", true))
                    //.ReturnsAsync(studentAcadLevels.FirstOrDefault());
                var actuals = await studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync("38942157-824a-4e49-a277-613f24852be6");
            }
        }
    }
}
