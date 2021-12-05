//Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentFinancialAidAcademicProgressStatusesServiceTests
    {
        [TestClass]
        public class StudentFinancialAidAcademicProgressStatusesServiceTests_V15
        {

            public abstract class CurrentUserSetup
            {
                protected Domain.Entities.Role personRole = new Domain.Entities.Role(1, "VIEW.STU.FA.ACAD.PROGRESS");

                public class PersonUserFactory : ICurrentUserFactory
                {
                    public ICurrentUser CurrentUser
                    {
                        get
                        {
                            return new CurrentUser(new Claims()
                            {
                                ControlId = "123",
                                Name = "George",
                                PersonId = "0000015",
                                SecurityToken = "321",
                                SessionTimeout = 30,
                                UserName = "Faculty",
                                Roles = new List<string>() { "VIEW.STU.FA.ACAD.PROGRESS" },
                                SessionFixationId = "abc123",
                            });
                        }
                    }
                }
            }

            [TestClass]
            public class StudentFinancialAidAcademicProgressStatusesServiceTests_GETALL_GETBYID :CurrentUserSetup
            {


                #region DECLARATIONS

                protected Domain.Entities.Role viewStudentFinAidAcadProgressStatus = new Domain.Entities.Role(1, "VIEW.STU.FA.ACAD.PROGRESS");

                private Mock<IStudentFinancialAidAcademicProgressStatusesRepository> studentFAAcademicProgressStatusesRepositoryMock;
                private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
                private Mock<IStudentReferenceDataRepository> studentreferenceDataRepositoryMock;

                private Mock<IPersonRepository> personRepositoryMock;
                private Mock<ITermRepository> termRepositoryMock;
                private Mock<IAdapterRegistry> adapterRegistryMock;
                private Mock<IRoleRepository> roleRepositoryMock;
                private Mock<IConfigurationRepository> configurationRepositoryMock;
                private Mock<ILogger> loggerMock;
                //private Mock<ICurrentUserFactory> userFactoryMock;
                private ICurrentUserFactory currentUserFactory;
                
                private StudentFinancialAidAcademicProgressStatusesService studentFAAcademicProgressStatusesService;
                
                private IEnumerable<StudentFinancialAidAcademicProgressStatuses> studentFinancialAidAcademicProgressStatuses;

                private Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int> studentFinancialAidAcademicProgressStatusesTuple;

                private IEnumerable<Domain.Entities.Role> roles;

                private IEnumerable<Domain.Student.Entities.SapStatuses> sapStatuses;

                private IEnumerable<SapType> sapType;

                private Tuple<IEnumerable<SapResult>, int> sapResultTuple;

                private IEnumerable<SapResult> sapResult;

                private IEnumerable<Term> terms;

                private Dictionary<string, string> personIds;
                
                private string guid = "adcbf49c-f129-470c-aa31-272493846751";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {

                    studentFAAcademicProgressStatusesRepositoryMock = new Mock<IStudentFinancialAidAcademicProgressStatusesRepository>();
                    personRepositoryMock = new Mock<IPersonRepository>();
                    referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                    studentreferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                    termRepositoryMock = new Mock<ITermRepository>();
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    configurationRepositoryMock = new Mock<IConfigurationRepository>();
                    loggerMock = new Mock<ILogger>();

                    currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                    InitializeTestData();

                    InitializeTestMock();

                    studentFAAcademicProgressStatusesService = new StudentFinancialAidAcademicProgressStatusesService(studentFAAcademicProgressStatusesRepositoryMock.Object, studentreferenceDataRepositoryMock.Object,personRepositoryMock.Object,termRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
                    

                }

                private void InitializeTestData()
                {

                    roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"VIEW.STU.FA.ACAD.PROGRESS")
                    };

                    roles.FirstOrDefault().AddPermission(new Permission(StudentPermissionCodes.ViewStudentFinancialAidAcadProgress));

                    sapResult = new List<SapResult>() { new SapResult("adcbf49c-f129-470c-aa31-272493846751", "1", "code1") { OvrResultsAddDate = DateTime.Now, PersonId = "1", SapTypeId = "code1", SaprEvalPdEndTerm ="code1", SaprEvalPdEndDate = DateTime.Now, SaprCalcThruTerm = "code1"},
                                                        new SapResult("adcbf49c-f129-470c-aa31-272493846752", "2", "code2") { OvrResultsAddDate = DateTime.Now, PersonId = "2", SapTypeId = "code2" },
                                                        new SapResult("adcbf49c-f129-470c-aa31-272493846752", "2", "code2") { OvrResultsAddDate = DateTime.Now, PersonId = "2", SapTypeId = "code2", SaprEvalPdEndDate = DateTime.Now },
                                                        new SapResult("adcbf49c-f129-470c-aa31-272493846752", "2", "code2") { OvrResultsAddDate = DateTime.Now, PersonId = "2", SapTypeId = "code2", SaprCalcThruTerm = "code1" }
                    };
                    
                    sapResultTuple = new Tuple<IEnumerable<SapResult>, int>(sapResult,4);

                    sapStatuses = new List<Domain.Student.Entities.SapStatuses>() { new Domain.Student.Entities.SapStatuses("adcbf49c-f129-470c-aa31-272493846711", "code1", "desc1") { FssiCategory = "cat1" },
                                                            new Domain.Student.Entities.SapStatuses("adcbf49c-f129-470c-aa31-272493846712", "code2", "desc2") { FssiCategory = "cat2" } };

                    personIds = new Dictionary<string, string>() { { "1", "0ca1a878-3555-4a3f-a17b-20d054d5e461" }, { "2", "0ca1a878-3555-4a3f-a17b-20d054d5e462" }, { "3", "0ca1a878-3555-4a3f-a17b-20d054d5e463" }, { "4", "0ca1a878-3555-4a3f-a17b-20d054d5e464" }, { "5", "0ca1a878-3555-4a3f-a17b-20d054d5e465" } };

                    sapType = new List<SapType>() { new SapType("adcbf49c-f129-470c-aa31-272493846721", "code1", "desc1"),
                                                    new SapType("adcbf49c-f129-470c-aa31-272493846722", "code2", "desc2") };

                    terms = new List<Term>() { new Term("code1", "desc1", DateTime.Now, DateTime.Now, 2017, 1, false, false, "term1", false), new Term("code2", "desc2", DateTime.Now, DateTime.Now, 2017, 2, false, false, "term1", false) };


                }

                private void InitializeTestMock()
                {
                    viewStudentFinAidAcadProgressStatus.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentFinancialAidAcadProgress));
                    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentFinAidAcadProgressStatus });
                    roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResultTuple);
                    personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guid);
                    studentreferenceDataRepositoryMock.Setup(r => r.GetSapStatusesAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapStatuses);
                    studentreferenceDataRepositoryMock.Setup(r => r.GetSapTypesAsync(It.IsAny<bool>())).ReturnsAsync(sapType);
                    personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personIds);
                    termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(s => s.GetSapResultByGuidAsync(It.IsAny<string>())).ReturnsAsync(sapResult.FirstOrDefault());
                }

                [TestCleanup]
                public void Cleanup()
                {
                    studentFAAcademicProgressStatusesService = null;
                    studentFAAcademicProgressStatusesRepositoryMock = null;
                    studentreferenceDataRepositoryMock = null;
                    personRepositoryMock = null;
                    referenceDataRepositoryMock = null;
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    configurationRepositoryMock = null;
                    loggerMock = null;
                    currentUserFactory = null;
                }

                #endregion

                #region GETALL

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_CriteriaAsNull()
                {
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_PersonId()
                {

                    var criteria = new StudentFinancialAidAcademicProgressStatuses() { Person = new GuidObject2("adcbf49c-f129-470c-aa31-272493846711") };

                    personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                    
                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, criteria, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_StatusId()
                {

                    var criteria = new StudentFinancialAidAcademicProgressStatuses() { Status = new GuidObject2("adcbf49c-f129-470c-aa31-272493846741") };
                    
                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, criteria, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_ProgressTypeId()
                {

                    var criteria = new StudentFinancialAidAcademicProgressStatuses() {ProgressType = new GuidObject2("adcbf49c-f129-470c-aa31-272493846751") };

                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, criteria, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync()
                {
                    
                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 4);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_Term_Exception()
                {
                    sapResultTuple.Item1.FirstOrDefault().SaprEvalPdEndTerm = "term21";
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResultTuple);
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);     
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_Term_Exception1()
                {
                    sapResultTuple.Item1.FirstOrDefault().SaprEvalPdEndTerm = null;
                    sapResultTuple.Item1.FirstOrDefault().SaprEvalPdEndDate = null;
                    sapResultTuple.Item1.FirstOrDefault().SaprCalcThruTerm = "term21";
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResultTuple);
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_Person_Exception()
                {
                    sapResultTuple.Item1.FirstOrDefault().PersonId = "21";
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResultTuple);
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_ProgressType_Exception()
                {
                    sapResultTuple.Item1.FirstOrDefault().SapTypeId = "21";
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(g => g.GetSapResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapResultTuple);
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_Status_Exception()
                {

                    sapStatuses = new List<Domain.Student.Entities.SapStatuses>() { new Domain.Student.Entities.SapStatuses("adcbf49c-f129-470c-aa31-272493846711", "code11", "desc1") { FssiCategory = "cat1" },
                                                            new Domain.Student.Entities.SapStatuses("adcbf49c-f129-470c-aa31-272493846712", "code22", "desc2") { FssiCategory = "cat2" } };
                    studentreferenceDataRepositoryMock.Setup(r => r.GetSapStatusesAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sapStatuses);
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, null, false);
                }


                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesAsync_Person_BadLookupData_NoException()
                {
                    personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                    Dtos.StudentFinancialAidAcademicProgressStatuses criteria = new StudentFinancialAidAcademicProgressStatuses();
                    criteria.Person = new GuidObject2("12345678");
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(0, 100, criteria, false);

                }

                #endregion

                    #region GETBYID
                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesByGuidAsync_KeyNotFoundException()
                {
                    sapResult.FirstOrDefault().PersonId = "45";
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(s => s.GetSapResultByGuidAsync(It.IsAny<string>())).ReturnsAsync(sapResult.FirstOrDefault());
                    await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync("adcbf49c-f129-470c-aa31-272493846111", true);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesByGuidAsync()
                {

                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(guid,true);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, guid);
                }

                [TestMethod]
                public async Task StudentFAAPSService_GetStudentFinancialAidAcademicProgressStatusesByGuidAsync_EntityAsNull()
                {
                    studentFAAcademicProgressStatusesRepositoryMock.Setup(s => s.GetSapResultByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                    var result = await studentFAAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(guid, true);
                    Assert.IsNull(result.Id,null);
                }

                #endregion  

            }
        }
    }
}
