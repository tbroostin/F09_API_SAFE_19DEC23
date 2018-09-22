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
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentSectionWaitlistsServiceTests 
    {

        # region CurrentUserSetup
        public abstract class CurrentUserSetup
        {
            

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
                            Roles = new List<string>() { "VIEW.STUDENT.SECTION.WAITLIST" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        #endregion



        [TestClass]
        public class WaitlistTests : CurrentUserSetup
        {

            private const string studentSectionWaitlistsGuid = "wait1xxx-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private ICollection<Domain.Student.Entities.StudentSectionWaitlist> _studentSectionWaitlistsCollection;
            private StudentSectionWaitlistsService _studentSectionWaitlistsService;
            private Mock<ILogger> _loggerMock;
            private Mock<ISectionRepository> _sectionRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private ICurrentUserFactory _currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {

                 Ellucian.Colleague.Domain.Entities.Role ViewStudentSectionWaitlist = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.SECTION.WAITLIST");
                 ViewStudentSectionWaitlist.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.SECTION.WAITLIST"));
                _sectionRepositoryMock = new Mock<ISectionRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentSectionWaitlist });
                _studentSectionWaitlistsCollection = new List<Domain.Student.Entities.StudentSectionWaitlist>()
                {
                    new Domain.Student.Entities.StudentSectionWaitlist("wait1xxx-cdcd-4c8f-b5d8-3053bf5b3fbc", "stud1xxx-cdcd-4c8f-b5d8-3053bf5b3fbc","sect1xxx-cdcd-4c8f-b5d8-3053bf5b3fbc",null),
                    new Domain.Student.Entities.StudentSectionWaitlist("wait2xxx-6cd4-4f98-8a73-ab0aa3627f0d", "stud2xxx-6cd4-4f98-8a73-ab0aa3627f0d","sect2xxx-6cd4-4f98-8a73-ab0aa3627f0d",1),
                    new Domain.Student.Entities.StudentSectionWaitlist("wait3xxx-9931-4560-b42f-1fccd43c952e", "stud3xxx-9931-4560-b42f-1fccd43c952e","sect3xxx-9931-4560-b42f-1fccd43c952e",2)
                };

                Tuple<IEnumerable<Domain.Student.Entities.StudentSectionWaitlist>, int> _studentSectionWaitlistsTuple
                    = new Tuple<IEnumerable<Domain.Student.Entities.StudentSectionWaitlist>, int>(_studentSectionWaitlistsCollection, 3);


                _sectionRepositoryMock.Setup(repo => repo.GetWaitlistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(_studentSectionWaitlistsTuple);
                _sectionRepositoryMock.Setup(repo => repo.GetWaitlistFromGuidAsync(studentSectionWaitlistsGuid)).ReturnsAsync(_studentSectionWaitlistsCollection.ElementAt(0));




                _studentSectionWaitlistsService = new StudentSectionWaitlistsService(_sectionRepositoryMock.Object, baseConfigurationRepository, _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);


            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentSectionWaitlistsService = null;
                _studentSectionWaitlistsCollection = null;
                _sectionRepositoryMock = null;
                _roleRepositoryMock = null;
                _currentUserFactory = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsAsync()
            {
                var results = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(1, 3);
                Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentSectionWaitlist>, int>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsAsync_Count()
            {
                var results = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(1, 3);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsAsync_Properties()
            {
                var result =
                    (await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(1, 3)).Item1.FirstOrDefault(x => x.Id == studentSectionWaitlistsGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Person);
                Assert.IsNotNull(result.Section);

            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsAsync_Expected()
            {
                var expectedResults = _studentSectionWaitlistsCollection.FirstOrDefault(c => c.Guid == studentSectionWaitlistsGuid);
                var actualResult =
                    (await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(1, 3)).Item1.FirstOrDefault(x => x.Id == studentSectionWaitlistsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.PersonId, actualResult.Person.personId);
                Assert.AreEqual(expectedResults.SectionId, actualResult.Section.sectionId);
                Assert.AreEqual(expectedResults.Priority, actualResult.Priority);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsByGuidAsync_Empty()
            {
                await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentSectionWaitlistsService_GetWaitlistFromGuidAsync_Null()
            {
                await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsByGuidAsync_InvalidId()
            {
                _sectionRepositoryMock.Setup(repo => repo.GetWaitlistFromGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync("99");
            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsByGuidAsync_Expected()
            {
                var expectedResults =
                    _studentSectionWaitlistsCollection.First(c => c.Guid == studentSectionWaitlistsGuid);
                var actualResult =
                    await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync(studentSectionWaitlistsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.PersonId, actualResult.Person.personId);
                Assert.AreEqual(expectedResults.SectionId, actualResult.Section.sectionId);
                Assert.AreEqual(expectedResults.Priority, actualResult.Priority);

            }

            [TestMethod]
            public async Task StudentSectionWaitlistsService_GetStudentSectionWaitlistsByGuidAsync_Properties()
            {
                var result =
                    await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync(studentSectionWaitlistsGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Person.personId);
                Assert.IsNotNull(result.Section.sectionId);
                
            }
        }

    }
}