// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class PreliminaryAnonymousGradeRepositoryTests : BaseRepositorySetup
    {
        public Mock<IColleagueTransactionFactory> transactionFactoryMock;
        public Mock<IColleagueTransactionInvoker> transactionInvokerMock;

        public IColleagueTransactionFactory transactionFactory;
        public IColleagueTransactionInvoker transactionInvoker;

        public PreliminaryAnonymousGradeRepository repository;
        public AcDefaults acDefaults;

        [TestInitialize]
        public void PreliminaryAnonymousGradeRepositoryTests_Initialize()
        {
            dataReaderMock = new Mock<IColleagueDataReader>();
            dataReader = dataReaderMock.Object;

            transactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            transactionInvoker = transactionInvokerMock.Object;

            cacheProviderMock = new Mock<ICacheProvider>();
            cacheProvider = cacheProviderMock.Object;
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null)).ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            transactionFactory = transactionFactoryMock.Object;
            transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReader);
            transactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker);

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            apiSettings = new ApiSettings();

            repository = new PreliminaryAnonymousGradeRepository(cacheProvider, transactionFactory, logger, apiSettings);

            acDefaults = new AcDefaults()
            {
                AcdRandomIdAssign = "S"
            };

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<AcDefaults>("ST.PARMS", "AC.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(acDefaults);
        }
    }

    [TestClass]
    public class GetPreliminaryAnonymousGradesBySectionIdAsync_Tests : PreliminaryAnonymousGradeRepositoryTests
    {
        public string sectionId;
        public List<string> crossListedSectionIds;
        public Collection<StudentCourseSec> studentCourseSecData;
        public Collection<PrelimStuGrdWork> prelimStuGrdWorkData;
        public Collection<StudentAcadCred> studentAcadCredData;
        public Collection<StudentTerms> studentTermsData;

        [TestInitialize]
        public void GetPreliminaryAnonymousGradesBySectionIdAsync_Tests_Initialize()
        {
            base.PreliminaryAnonymousGradeRepositoryTests_Initialize();

            sectionId = "12345";
            crossListedSectionIds = new List<string>() { "12346" };

            studentCourseSecData = new Collection<StudentCourseSec>()
            {
                new StudentCourseSec()
                {
                    RecordGuid = "eaa8ad72-4379-4cd8-8217-2008105f0fe7",
                    Recordkey = "1",
                    ScsCourseSection = sectionId,
                    ScsStudentAcadCred = "1",
                    ScsRandomId = 12345
                },
                new StudentCourseSec()
                {
                    RecordGuid = "92572ec8-0b08-483d-8d43-82f9e82d2b0f",
                    Recordkey = "2",
                    ScsCourseSection = crossListedSectionIds[0],
                    ScsStudentAcadCred = "2",
                    ScsRandomId = 12346
                }
            };

            prelimStuGrdWorkData = new Collection<PrelimStuGrdWork>()
            {
                new PrelimStuGrdWork()
                {
                    Recordkey = "1",
                    PsgFinalGrade = string.Empty,
                    PsgFinalGradeExpireDate = null
                },
                new PrelimStuGrdWork()
                {
                    Recordkey = "2",
                    PsgFinalGrade = "12",
                    PsgFinalGradeExpireDate = DateTime.Today.AddMonths(3)
                }
            };

            studentAcadCredData = new Collection<StudentAcadCred>()
            {
                new StudentAcadCred()
                {
                    RecordGuid = "472e8b26-083f-4f8a-bc2c-80b0adde9578",
                    Recordkey = "1",
                    StcStudentCourseSec = "1",
                    StcPersonId = "0001234",
                    StcTerm = "2021/FA",
                    StcAcadLevel = "UG"
                },
                new StudentAcadCred()
                {
                    RecordGuid = "bea57a94-5288-4f2b-a87c-d65706cc6596",
                    Recordkey = "2",
                    StcStudentCourseSec = "2",
                    StcPersonId = "0001235",
                    StcTerm = "2021/FA",
                    StcAcadLevel = "UG"
                }
            };

            studentTermsData = new Collection<StudentTerms>()
            {
                new StudentTerms()
                {
                    Recordkey = "0001234*2021/FA*UG",
                    RecordGuid = "78e9c9bd-14f1-437d-a657-14442ecf9b64",
                    SttrRandomId = 12347
                },
                new StudentTerms()
                {
                    Recordkey = "0001235*2021/FA*UG",
                    RecordGuid = "e7fcb291-9c69-4461-9bae-e18ba6b15796",
                    SttrRandomId = 12348
                },
            };

            MockRecordsAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecData);
            MockRecordsAsync<PrelimStuGrdWork>("PRELIM.STU.GRD.WORK", prelimStuGrdWorkData);
            MockRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredData);
            MockRecordsAsync<StudentTerms>("STUDENT.TERMS", studentTermsData);

            dataReaderMock.Setup(dr => dr.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '?'", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(studentCourseSecData.Select(scs => scs.Recordkey).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_throws_ArgumentNullException()
        {
            var entity = await repository.GetPreliminaryAnonymousGradesBySectionIdAsync(null, crossListedSectionIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_AcademicRecordConfiguration_AnonymousGradingType_none_throws_ConfigurationException()
        {
            acDefaults.AcdRandomIdAssign = string.Empty;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<AcDefaults>("ST.PARMS", "AC.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(acDefaults);

            var entity = await repository.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, crossListedSectionIds);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_AnonymousGradingType_Section_valid()
        {
            acDefaults.AcdRandomIdAssign = "S";
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<AcDefaults>("ST.PARMS", "AC.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(acDefaults);

            var entity = await repository.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, crossListedSectionIds);

            Assert.IsNotNull(entity);
            Assert.AreEqual(1, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual(1, entity.AnonymousGradesForCrosslistedSections.Count);
            Assert.AreEqual(0, entity.Errors.Count);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_AnonymousGradingType_Term_valid()
        {
            acDefaults.AcdRandomIdAssign = "T";
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<AcDefaults>("ST.PARMS", "AC.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(acDefaults);

            var entity = await repository.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId, crossListedSectionIds);

            Assert.IsNotNull(entity);
            Assert.AreEqual(1, entity.AnonymousGradesForSection.Count);
            Assert.AreEqual(1, entity.AnonymousGradesForCrosslistedSections.Count);
            Assert.AreEqual(0, entity.Errors.Count);
        }
    }

    [TestClass]
    public class UpdatePreliminaryAnonymousGradesBySectionIdAsync_Tests : PreliminaryAnonymousGradeRepositoryTests
    {
        public string sectionId;
        public List<string> crossListedSectionIds;
        public Collection<StudentCourseSec> studentCourseSecData;
        public Collection<PrelimStuGrdWork> prelimStuGrdWorkData;
        public Collection<StudentAcadCred> studentAcadCredData;
        public Collection<StudentTerms> studentTermsData;
        public List<PreliminaryAnonymousGrade> preliminaryAnonymousGrades;

        [TestInitialize]
        public void UpdatePreliminaryAnonymousGradesBySectionIdAsync_Tests_Initialize()
        {
            base.PreliminaryAnonymousGradeRepositoryTests_Initialize();

            sectionId = "12345";

            preliminaryAnonymousGrades = new List<PreliminaryAnonymousGrade>()
            {
                new PreliminaryAnonymousGrade("12345", "1", "12345", "1", null),
                new PreliminaryAnonymousGrade("12346", "2", "12346", "2", DateTime.Today.AddMonths(3))
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_throws_ArgumentNullException()
        {
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(null, preliminaryAnonymousGrades);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_preliminaryAnonymousGrades_throws_ArgumentNullException()
        {
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_empty_preliminaryAnonymousGrades_throws_ArgumentNullException()
        {
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, new List<PreliminaryAnonymousGrade>());
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_ColleagueTransactionException_timeout_throws_TimeoutException()
        {
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ThrowsAsync(new ColleagueTransactionException("Timeout", new Dmi.Runtime.ErrorResponse()
                {
                    ErrorCode = "00002",
                    ErrorCategory = "SECURITY"
                }));
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_ColleagueTransactionException_not_timeout_rethrows_exception()
        {
            bool cteThrown = false;
            ColleagueTransactionException cte = new ColleagueTransactionException("Timeout", new Dmi.Runtime.ErrorResponse()
            {
                ErrorCode = "00001",
                ErrorCategory = "NOT-SECURITY"
            });
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ThrowsAsync(cte);
            try
            {
                var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (ColleagueTransactionException ctex)
            {
                cteThrown = ctex == cte;
            }
            Assert.IsTrue(cteThrown);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueTransactionException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_response_null_throws_ColleagueTransactionException()
        {
            UpdatePreliminaryAnonymousGradesResponse ctxResponse = null;
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ReturnsAsync(ctxResponse);
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_response_with_ErrorMessage_throws_ColleagueException()
        {
            UpdatePreliminaryAnonymousGradesResponse ctxResponse = new UpdatePreliminaryAnonymousGradesResponse()
            {
                ErrorMessage = "Error occurred inside CTX."
            };
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ReturnsAsync(ctxResponse);
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_response_with_null_ProcessingResults_throws_ColleagueException()
        {
            UpdatePreliminaryAnonymousGradesResponse ctxResponse = new UpdatePreliminaryAnonymousGradesResponse()
            {
                ErrorMessage = null,
                ProcessingResults = null
            };
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ReturnsAsync(ctxResponse);
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Ctx_response_with_empty_ProcessingResults_throws_ColleagueException()
        {
            UpdatePreliminaryAnonymousGradesResponse ctxResponse = new UpdatePreliminaryAnonymousGradesResponse()
            {
                ErrorMessage = null,
                ProcessingResults = new List<ProcessingResults>()
            };
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ReturnsAsync(ctxResponse);
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_valid()
        {
            UpdatePreliminaryAnonymousGradesResponse ctxResponse = new UpdatePreliminaryAnonymousGradesResponse()
            {
                ErrorMessage = null,
                ProcessingResults = new List<ProcessingResults>()
                {
                    new ProcessingResults()
                    {
                        OutMessage = "Error",
                        OutStudentCourseSecId = "1",
                        OutUpdateSuccessful = false
                    },
                    new ProcessingResults()
                    {
                        OutMessage = string.Empty,
                        OutStudentCourseSecId = "2",
                        OutUpdateSuccessful = true
                    }                }
            };
            transactionInvokerMock.Setup(ti => ti.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(It.IsAny<UpdatePreliminaryAnonymousGradesRequest>())).
                ReturnsAsync(ctxResponse);
            var entity = await repository.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            Assert.IsNotNull(entity);
            Assert.AreEqual(2, entity.Count());
        }
    }
}
