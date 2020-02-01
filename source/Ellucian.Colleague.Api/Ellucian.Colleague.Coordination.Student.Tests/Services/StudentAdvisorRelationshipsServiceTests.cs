//Copyright 2017 Ellucian Company L.P. and its affiliates.


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

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{


    [TestClass]
    public class StudentAdvisorRelationshipsServiceTests
    {

        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STU.ADV.RELATIONSHIPS");
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
                            Roles = new List<string>() { "VIEW.STU.ADV.RELATIONSHIPS" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentAdvisorRel_Get : CurrentUserSetup
        {
            private const string studentAdvisorRelationshipsGuid = "635a3ad5-59ab-47ca-af87-8538c2ad727f";
            private ICollection<StudentAdvisorRelationship> _studentAdvisorRelationshipsCollection;
            private StudentAdvisorRelationshipsService _studentAdvisorRelationshipsService;
            private Mock<ILogger> _loggerMock;
            private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
            private Mock<IStudentAdvisorRelationshipsRepository> _studentAdvisorRelationshipsRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IAdvisorTypesService> _advisorTypesServiceMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IStudentRepository> _studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            ICurrentUserFactory curntUserFactory;

            protected Ellucian.Colleague.Domain.Entities.Role viewStudentAdvisorRelRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STU.ADV.RELATIONSHIPS");

            private List<Dtos.StudentAdvisorRelationships> _dtoStudentAdvisorRelList;
            private List<Domain.Student.Entities.AcademicProgram> prgList;
            private List<Domain.Student.Entities.AdvisorType> advisorTypeList;

            [TestInitialize]
            public void Initialize()
            {
                _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _loggerMock = new Mock<ILogger>();
                _studentAdvisorRelationshipsRepositoryMock = new Mock<IStudentAdvisorRelationshipsRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _advisorTypesServiceMock = new Mock<IAdvisorTypesService>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _studentRepoMock = new Mock<IStudentRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                viewStudentAdvisorRelRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAdivsorRelationships));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentAdvisorRelRole });

                _studentAdvisorRelationshipsCollection = new List<StudentAdvisorRelationship>()
                {
                    new StudentAdvisorRelationship() {
                        Id = "1",
                        Guid = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                        Advisor = "ad1",
                        AdvisorType = "Type1",
                        StartOn = new DateTime(2001, 10,15),
                         Program = "ProgCode1",
                          Student = "stu1"
                    },
                    new StudentAdvisorRelationship() {
                        Id = "2",
                        Guid = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                        Advisor = "ad2",
                        StartOn = new DateTime(2001, 09,01),
                        EndOn = new DateTime(2004, 05,15),
                          Student = "stu2"
                    },
                    new StudentAdvisorRelationship() {
                        Id = "3",
                        Guid = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                        Advisor = "ad3",
                        AdvisorType = "Type1",
                        StartOn = new DateTime(2009, 07,17),
                         Program = "ProgCode1",
                          Student = "stu3"
                    },
                };

                _dtoStudentAdvisorRelList = new List<StudentAdvisorRelationships>()
            {
                new StudentAdvisorRelationships()
                {
                    Id = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                     Advisor = new GuidObject2("adGuid1"),
                     AdvisorType = new GuidObject2("typeguid1"),
                    StartOn =    new DateTime(2001, 10,15),
                    Program = new GuidObject2("progguid1"),
                    Student = new GuidObject2("stuGuid1")
                },
                new StudentAdvisorRelationships()
                {
                    Id = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                     Advisor = new GuidObject2("adGuid2"),
                    StartOn =    new DateTime(2001, 09,01),
                    EndOn = new DateTime(2004, 05,15),
                    Student = new GuidObject2("stuGuid2")
                },
                new StudentAdvisorRelationships()
                {
                    Id = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                     Advisor = new GuidObject2("adGuid3"),
                     AdvisorType = new GuidObject2("typeguid1"),
                    StartOn =    new DateTime(2009, 07,17),
                    Program = new GuidObject2("progguid1"),
                    Student = new GuidObject2("stuGuid3")
                },

            };

            prgList = new List<Domain.Student.Entities.AcademicProgram>()
            {
                new Domain.Student.Entities.AcademicProgram("progguid1", "ProgCode1", "Prog description")
            };

            advisorTypeList = new List<AdvisorType>()
            {
                new AdvisorType("typeguid1", "Type1","Ad type Descpt","1")
            };


                _studentAdvisorRelationshipsService = new StudentAdvisorRelationshipsService(_referenceRepositoryMock.Object, _studentAdvisorRelationshipsRepositoryMock.Object,
                    _personRepositoryMock.Object, _adapterRegistryMock.Object, _advisorTypesServiceMock.Object, curntUserFactory,
                    _roleRepositoryMock.Object, _loggerMock.Object, _studentRepoMock.Object, baseConfigurationRepository);

                _referenceRepositoryMock.Setup(x => x.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ReturnsAsync("progguid1");
                //_referenceRepositoryMock.Setup(x => x.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(prgList);

                _referenceRepositoryMock.Setup(x => x.GetAdvisorTypeGuidAsync(It.IsAny<string>())).ReturnsAsync("typeguid1");
                //_referenceRepositoryMock.Setup(x => x.GetAdvisorTypesAsync(It.IsAny<bool>())).ReturnsAsync(advisorTypeList);

                Dictionary<string, string> ids = new Dictionary<string, string>();
                ids.Add("ad1", "adGuid1");
                ids.Add("ad2", "adGuid2");
                ids.Add("ad3", "adGuid3");
                ids.Add("stu1", "stuGuid1");
                ids.Add("stu2", "stuGuid2");
                ids.Add("stu3", "stuGuid3");

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(ids);
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("ad1")).ReturnsAsync("adGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("ad2")).ReturnsAsync("adGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("ad3")).ReturnsAsync("adGuid3");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu1")).ReturnsAsync("stuGuid1");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu2")).ReturnsAsync("stuGuid2");
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("stu3")).ReturnsAsync("stuGuid3");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentAdvisorRelationshipsService = null;
                _studentAdvisorRelationshipsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100);

                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Item2);
                Assert.AreEqual(results.Item1.Count(), results.Item2);

                foreach (var actual in results.Item1)
                {
                    var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == actual.Id);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Advisor.Id, actual.Advisor.Id);
                    if (actual.AdvisorType != null || expected.AdvisorType != null)
                    {
                        Assert.AreEqual(expected.AdvisorType.Id, actual.AdvisorType.Id);
                    }

                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    if (actual.Program != null || expected.Program != null)
                    {
                        Assert.AreEqual(expected.Program.Id, actual.Program.Id);
                    }
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                }

            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync_filters()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);
                advisorTypeList = new List<AdvisorType>()
                {
                    new AdvisorType("typeguid1", "Type1","Ad type Descpt","1")
                };
                _personRepositoryMock.SetupSequence(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult("ad1")).Returns(Task.FromResult("stu1"));
                _referenceRepositoryMock.Setup(x => x.GetAdvisorTypesAsync(It.IsAny<bool>())).ReturnsAsync(advisorTypeList);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100,
                    false, "stuGuid1", "adGuid1", "typeguid1");

                Assert.IsNotNull(results);
                Assert.AreEqual(3, results.Item2);
                Assert.AreEqual(results.Item1.Count(), results.Item2);
            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                var actual = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync(entity.Guid);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Advisor.Id, actual.Advisor.Id);
                if (actual.AdvisorType != null || expected.AdvisorType != null)
                {
                    Assert.AreEqual(expected.AdvisorType.Id, actual.AdvisorType.Id);
                }

                Assert.AreEqual(expected.EndOn, actual.EndOn);
                if (actual.Program != null || expected.Program != null)
                {
                    Assert.AreEqual(expected.Program.Id, actual.Program.Id);
                }
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);

            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync_FailStudentfilters()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100,
                    false, "stuGuid1", "", "");

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync_FailAdvisorfilters()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100,
                    false, "", "anything", "");

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync_FailAdvisorTypefilters()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tupleResult);

                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100,
                    false, "", "", "anything");

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsAsync_FailstartAcademicPeriodfilters()
            {
                Tuple<IEnumerable<StudentAdvisorRelationship>, int> tupleResult = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(_studentAdvisorRelationshipsCollection, 3);


                var results = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(0, 100,
                    false, "", "", "", "anything");

                Assert.AreEqual(results.Item1.Count(), 0);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_Empty()
            {
                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_Null()
            {
                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_InvalidId()
            {
                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_InvalidOperationException()
            {
                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).Throws<InvalidOperationException>();

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_SourceInvalidGuidException()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Guid = null;

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_SourceInvalidAdvisorException()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Advisor = null;

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_SourceInvalidStudentException()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Student = null;

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_SourceInvalidStartOnException()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.StartOn = null;

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_NoAdvisorGuidFound()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Advisor = "notFoundID";

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_NoStudentGuidFound()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Student = "notFoundID";

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_NoAdvsiorTypeGuidFound()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.AdvisorType = "notFoundID";
                _referenceRepositoryMock.Setup(x => x.GetAdvisorTypeGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_NoProgramGuidFound()
            {
                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Program = "notFoundID";
                _referenceRepositoryMock.Setup(x => x.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentAdvisorRelationshipsService_GetStudentAdvisorRelationshipsByGuidAsync_NoPermission()
            {
                curntUserFactory = new CurrentUserSetup.StudentUserFactory();
                _studentAdvisorRelationshipsService = new StudentAdvisorRelationshipsService(_referenceRepositoryMock.Object, _studentAdvisorRelationshipsRepositoryMock.Object,
                    _personRepositoryMock.Object, _adapterRegistryMock.Object, _advisorTypesServiceMock.Object, curntUserFactory,
                    _roleRepositoryMock.Object, _loggerMock.Object, _studentRepoMock.Object, baseConfigurationRepository);

                StudentAdvisorRelationship entity = _studentAdvisorRelationshipsCollection.First();
                var expected = _dtoStudentAdvisorRelList.FirstOrDefault(x => x.Id == entity.Guid);

                entity.Program = "notFoundID";

                _studentAdvisorRelationshipsRepositoryMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).ReturnsAsync(entity);

                await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync("99");
            }


        }

    }
}