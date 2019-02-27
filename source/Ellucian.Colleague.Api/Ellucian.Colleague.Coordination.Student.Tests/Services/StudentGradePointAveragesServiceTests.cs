//Copyright 2018 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{


    [TestClass]
    public class StudentGradePointAveragesServiceTests
    {

        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentGradePointAverageRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.GRADE.POINT.AVERAGES");
            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "VIEW.STUDENT.GRADE.POINT.AVERAGES" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentGradePointAverages_Get : CurrentUserSetup
        {
            private const string studentGradePointAveragesGuid = "635a3ad5-59ab-47ca-af87-8538c2ad727f";
            private ICollection<Domain.Student.Entities.StudentAcademicCredit> _studentAcademicCreditsCollection;
            private StudentGradePointAveragesService _studentGradePointAveragesService;
            private Mock<ILogger> _loggerMock;
            private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
            private Mock<IStudentGradePointAveragesRepository> _studentGradePointAveragesRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            //private Mock<IAdvisorTypesService> _advisorTypesServiceMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IAcademicCredentialsRepository> _academicCredentialsRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            ICurrentUserFactory curntUserFactory;

            protected Ellucian.Colleague.Domain.Entities.Role viewStudentGradePointAverageRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.GRADE.POINT.AVERAGES");

            private List<Dtos.StudentGradePointAverages> _dtoStudentGradePointAveragesList;
            private List<Domain.Student.Entities.Term> termList;
            private List<Domain.Student.Entities.AcademicPeriod> academicPeriodList;
            private List<Domain.Student.Entities.AcademicLevel> academicLevelList;
            private List<Domain.Student.Entities.CreditCategory> creditCategoryList;

            private Tuple<IEnumerable<StudentAcademicCredit>, int> studentAcademicCreditTuple;

            [TestInitialize]
            public void Initialize()
            {
                _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _loggerMock = new Mock<ILogger>();
                _studentGradePointAveragesRepositoryMock = new Mock<IStudentGradePointAveragesRepository>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                //_advisorTypesServiceMock = new Mock<IAdvisorTypesService>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _academicCredentialsRepoMock = new Mock<IAcademicCredentialsRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                viewStudentGradePointAverageRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentGradePointAverages));
                _roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { viewStudentGradePointAverageRole });

                _studentAcademicCreditsCollection = new List<Domain.Student.Entities.StudentAcademicCredit>()
                {
                    new Domain.Student.Entities.StudentAcademicCredit("3632ece0-8b9e-495f-a697-b5c9e053aad5", "stu1") { StudentGPAInfoList = new List<StudentGPAInfo>() { new StudentGPAInfo() } },
                    new Domain.Student.Entities.StudentAcademicCredit("176d35fb-5f7a-4c06-b3ae-65a7662c8b43", "stu2") { StudentGPAInfoList = new List<StudentGPAInfo>() { new StudentGPAInfo() } },
                    new Domain.Student.Entities.StudentAcademicCredit("635a3ad5-59ab-47ca-af87-8538c2ad727f", "stu3") { StudentGPAInfoList = new List<StudentGPAInfo>() { new StudentGPAInfo() } },
                };

                studentAcademicCreditTuple = new Tuple<IEnumerable<StudentAcademicCredit>, int>(_studentAcademicCreditsCollection, _studentAcademicCreditsCollection.Count);

                _dtoStudentGradePointAveragesList = new List<Dtos.StudentGradePointAverages>()
            {
                new Dtos.StudentGradePointAverages()
                {
                    Id = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                    Student = new GuidObject2("stuGuid1")
                },
                new Dtos.StudentGradePointAverages()
                {
                    Id = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                    Student = new GuidObject2("stuGuid2")
                },
                new Dtos.StudentGradePointAverages()
                {
                    Id = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                    Student = new GuidObject2("stuGuid3")
                },

            };

                termList = new List<Domain.Student.Entities.Term>()
            {
                new Domain.Student.Entities.Term("progguid1", "ProgCode1", "Prog description", DateTime.Now, DateTime.Now, 1988, 0, false, false, "reportTerm", false)
            };

                academicPeriodList = new List<Domain.Student.Entities.AcademicPeriod>()
            {
                new Domain.Student.Entities.AcademicPeriod("progguid1", "ProgCode1", "Prog description", DateTime.Now, DateTime.Now, 1988, 0, "reportingTerm", "precedingId", "parentId", new List<RegistrationDate>())
            };

                academicLevelList = new List<Domain.Student.Entities.AcademicLevel>()
            {
                new Domain.Student.Entities.AcademicLevel("grdGuid1", "grd1", "Ad type Descpt"),
                new Domain.Student.Entities.AcademicLevel("grdGuid2", "grd2", "Ad type Descpt"),
                new Domain.Student.Entities.AcademicLevel("grdGuid3", "grd3", "Ad type Descpt"),
            };

                creditCategoryList = new List<Domain.Student.Entities.CreditCategory>()
            {
                new Domain.Student.Entities.CreditCategory("grdGuid1", "grd1", "Ad type Descpt", CreditType.ContinuingEducation) { Category = "I" },
                new Domain.Student.Entities.CreditCategory("grdGuid2", "grd2", "Ad type Descpt", CreditType.Institutional){ Category = "I" },
                new Domain.Student.Entities.CreditCategory("grdGuid3", "grd3", "Ad type Descpt", CreditType.Transfer){ Category = "I" },
            };

                _termRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(termList);
                _termRepositoryMock.Setup(x => x.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriodList);

                _referenceRepositoryMock.Setup(x => x.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryList);
                _referenceRepositoryMock.Setup(x => x.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelList);

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec1")).ReturnsAsync("secGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec2")).ReturnsAsync("secGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("sec3")).ReturnsAsync("secGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu1")).ReturnsAsync("stuGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu2")).ReturnsAsync("stuGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu3")).ReturnsAsync("stuGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid1")).ReturnsAsync("sec1");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid2")).ReturnsAsync("sec2");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("secGuid3")).ReturnsAsync("sec3");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid1")).ReturnsAsync("stu1");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid2")).ReturnsAsync("stu2");
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync("stuGuid3")).ReturnsAsync("stu3");

                _studentGradePointAveragesRepositoryMock.Setup(x => x.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("gradeDate");
                _studentGradePointAveragesRepositoryMock.Setup(x => x.GetStudentGpasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicCredit>(), It.IsAny<string>())).ReturnsAsync(studentAcademicCreditTuple);
                _studentGradePointAveragesRepositoryMock.Setup(x => x.GetStudentCredProgramInfoAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAcademicCredentialProgramInfo>());
                _studentGradePointAveragesRepositoryMock.Setup(x => x.UseAlternativeCumulativeValuesAsync()).ReturnsAsync(false);

                Dictionary<string, string> studentDictionary = new Dictionary<string, string>();
                studentDictionary.Add("stu1", "stuGuid1");
                studentDictionary.Add("stu2", "stuGuid2");
                studentDictionary.Add("stu3", "stuGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(studentDictionary);
                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(studentDictionary);
                Dictionary<string, string> studentAcadCredDictionary = new Dictionary<string, string>();
                studentAcadCredDictionary.Add("secGuid1", "secGuid1");
                studentAcadCredDictionary.Add("secGuid2", "secGuid2");
                studentAcadCredDictionary.Add("secGuid3", "secGuid3");
                //_studentGradePointAveragesRepositoryMock.Setup(x => x.GetGuidsCollectionAsync(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(studentAcadCredDictionary);

                _studentGradePointAveragesService = new StudentGradePointAveragesService(_studentGradePointAveragesRepositoryMock.Object, _personRepositoryMock.Object,
                   _studentRepositoryMock.Object, _termRepositoryMock.Object, _academicCredentialsRepoMock.Object, _referenceRepositoryMock.Object,
                   _adapterRegistryMock.Object, curntUserFactory, _roleRepositoryMock.Object, baseConfigurationRepository, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentGradePointAveragesService = null;
                _studentAcademicCreditsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task StudentGradePointAveragesService_GetStudentGradePointAveragesAsync()
            {
                var results = await _studentGradePointAveragesService.GetStudentGradePointAveragesAsync(0, 100, _dtoStudentGradePointAveragesList.FirstOrDefault(), "gradeDate", false);

                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Item2);
                Assert.AreEqual(results.Item1.Count(), results.Item2);

                foreach (var actual in results.Item1)
                {
                    var expected = _dtoStudentGradePointAveragesList.FirstOrDefault(x => x.Id == actual.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                    //if (actual.SectionRegistration != null || expected.SectionRegistration != null)
                    //{
                    //    Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
                    //}
                    //if (actual.AwardGradeScheme != null || expected.AwardGradeScheme != null)
                    //{
                    //    Assert.AreEqual(expected.AwardGradeScheme.Id, actual.AwardGradeScheme.Id);
                    //}
                }

            }

            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_FailAcademicPeriodfilters()
            {
                StudentGradePointAveragesPeriodBasedDtoProperty periodBased = new StudentGradePointAveragesPeriodBasedDtoProperty() { AcademicPeriod = new GuidObject2("BadGUid") };
                _dtoStudentGradePointAveragesList.FirstOrDefault().PeriodBased = new List<StudentGradePointAveragesPeriodBasedDtoProperty>() { periodBased };

                var results = await _studentGradePointAveragesService.GetStudentGradePointAveragesAsync(0, 100, _dtoStudentGradePointAveragesList.FirstOrDefault(), "", false);

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }
            
            [TestMethod]
            public async Task StudentUnverifiedGradesService_GetStudentUnverifiedGradesAsync_FailStudentGradePointAveragesfilters()
            {
                _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

                var results = await _studentGradePointAveragesService.GetStudentGradePointAveragesAsync(0, 100, _dtoStudentGradePointAveragesList.FirstOrDefault(), "", false);

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

        }

    }
}