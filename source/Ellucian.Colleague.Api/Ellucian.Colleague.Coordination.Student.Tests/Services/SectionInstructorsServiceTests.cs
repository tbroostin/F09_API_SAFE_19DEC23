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
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class SectionInstructorsServiceTests_V10
    {
        [TestClass]
        public class SectionInstructorsServiceTests_GET : StudentUserFactory
        {
            #region DECLARATIONS

            protected Domain.Entities.Role viewsectionInstructors = new Domain.Entities.Role(1, "VIEW.SECTION.INSTRUCTORS");

            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configRepoMock;

            private ICurrentUserFactory currentUserFactory;

            private SectionInstructorsService service;

            private int offset = 0, limit = 10;

            private Tuple<IEnumerable<SectionFaculty>, int> tupleResult;

            private SectionFaculty sectionFaculty;
            private IEnumerable<Domain.Student.Entities.InstructionalMethod> instMethods;

            private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configRepoMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new SectionInstructorsUser();

                InitializeTestData();

                InitializeMock();

                service = new SectionInstructorsService(referenceDataRepositoryMock.Object, sectionRepositoryMock.Object, personRepositoryMock.Object,
                    configRepoMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                sectionRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            private void InitializeTestData()
            {
                instMethods = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("1", "1", "desc", true){ }
                };

                sectionFaculty = new SectionFaculty("1", "1", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), 10)
                {
                    SecMeetingIds = new List<string>() { "1" },
                    PrimaryIndicator = true,
                    LoadFactor = 10,
                };

                tupleResult = new Tuple<IEnumerable<SectionFaculty>, int>(new List<SectionFaculty>() { sectionFaculty }, 1);
            }

            private void InitializeMock()
            {
                viewsectionInstructors.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewSectionInstructors));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewsectionInstructors });
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(s => s.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(tupleResult);
                sectionRepositoryMock.Setup(s => s.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(s => s.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1");
                referenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodsAsync(false)).ReturnsAsync(instMethods);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_GetSectionInstructorsAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                try
                {
                    await service.GetSectionInstructorsAsync(offset, limit, "", "", new List<string>());
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("User 'StudentPO' is not authorized to create or view section-instructors.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionInstructorsService_GetSectionInstructorsAsync()
            {
                var result = await service.GetSectionInstructorsAsync(offset, limit, "", "", new List<string>());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item1.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_GetSectionInstructorsByGuidAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                try
                {
                    await service.GetSectionInstructorsByGuidAsync(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("User 'StudentPO' is not authorized to create or view section-instructors.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_GetSectionInstructorsByGuidAsync_IntegrationApiException()
            {
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await service.GetSectionInstructorsByGuidAsync(guid);
            }
            
            [TestMethod]
            public async Task SectionInstructorsService_GetSectionInstructorsByGuidAsync()
            {
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionFaculty);
                var result = await service.GetSectionInstructorsByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual("1", result.Id);
            }
        }

        [TestClass]
        public class SectionInstructorsServiceTests_POST : StudentUserFactory
        {
            #region DECLARATIONS

            protected Domain.Entities.Role createSectionInstructors = new Domain.Entities.Role(1, "UPDATE.SECTION.INSTRUCTORS");

            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configRepoMock;

            private ICurrentUserFactory currentUserFactory;

            private SectionInstructorsService service;

            private SectionInstructors sectionInstructor;
            private SectionMeeting sectionMeeting;
            private SectionFaculty sectionFaculty;

            private IEnumerable<Domain.Student.Entities.InstructionalMethod> instMethods;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configRepoMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new SectionInstructorsUser();

                InitializeTestData();

                InitializeMock();

                service = new SectionInstructorsService(referenceDataRepositoryMock.Object, sectionRepositoryMock.Object, personRepositoryMock.Object,
                    configRepoMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                sectionRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            private void InitializeTestData()
            {
                sectionMeeting = new SectionMeeting("1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), "1") { };

                instMethods = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "desc", true){ }
                };

                sectionInstructor = new SectionInstructors()
                {
                    Id = "1",
                    InstructionalMethod = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    InstructionalEvents = new List<GuidObject2>() { new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc") },
                    Section = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    Instructor = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    WorkStartOn = DateTime.Today,
                    WorkEndOn = DateTime.Today.AddDays(10),
                    ResponsibilityPercentage = 10,
                    InstructorRole = Dtos.EnumProperties.SectionInstructorsInstructorRole.Primary
                };

                sectionFaculty = new SectionFaculty("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), 10)
                {
                    SecMeetingIds = new List<string>() { "1" },
                    PrimaryIndicator = true,
                    LoadFactor = 10,
                };
            }

            private void InitializeMock()
            {
                createSectionInstructors.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateSectionInstructors));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createSectionInstructors });
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(s => s.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                sectionRepositoryMock.Setup(s => s.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                personRepositoryMock.Setup(s => s.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionMeeting);
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                referenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodsAsync(false)).ReturnsAsync(instMethods);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_IntegrationApiException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("User StudentPO does not have permission to create or update section-instructors.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_IntegrationApiException_SectionInstructor_Null()
            {
                try
                {
                    await service.CreateSectionInstructorsAsync(null);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("SectionInstructors body is required in PUT or POST request. ", ex.Errors[0].Message);
                    Assert.AreEqual("The section instructors is a required parameter for updates/creates.", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_IntegrationApiException_InstructorMethod_Null()
            {
                sectionInstructor.InstructionalMethod = null;
                sectionInstructor.InstructionalEvents = new List<GuidObject2>() { };
                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The instructional Method is required for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Section faculty record $NEW for section 1 missing instructional method\r\nParameter name: instrMethod", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_IntegrationApiException_Section_Null()
            {
                sectionInstructor.Section = null;

                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The section id is a required parameter for updates/creates.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_IntegrationApiException_InstructorId_Null()
            {
                sectionInstructor.Instructor.Id = null;
                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The instructor id is a required parameter for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Instructor is a required property.", ex.Errors[1].Message);
                    Assert.AreEqual("Error processing section faculty record $NEW for section 1: missing faculty ID \r\nParameter name: facultyId", ex.Errors[2].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_DtoToEntity_IntegrationApiException_InstMethods_Null()
            {
                referenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodsAsync(false)).ReturnsAsync(null);
                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Instructional Methods call returns no values.  Required for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Section faculty record $NEW for section 1 missing instructional method\r\nParameter name: instrMethod", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync_EntityToDto_IntegrationApiException_SectionFaculty_Null()
            {
                sectionInstructor.InstructionalMethod = null;

                sectionRepositoryMock.Setup(s => s.PostSectionFacultyAsync(It.IsAny<SectionFaculty>(), It.IsAny<string>())).ReturnsAsync(null);
                try
                {
                    await service.CreateSectionInstructorsAsync(sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("SectionFaculty Entity is a required parameter.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionInstructorsService_CreateSectionInstructorsAsync()
            {
                sectionRepositoryMock.Setup(s => s.PostSectionFacultyAsync(It.IsAny<SectionFaculty>(), It.IsAny<string>())).ReturnsAsync(sectionFaculty);

                var result = await service.CreateSectionInstructorsAsync(sectionInstructor);

                Assert.IsNotNull(result);
                Assert.AreEqual(sectionFaculty.Guid, result.Id);
            }
        }

        [TestClass]
        public class SectionInstructorsServiceTests_PUT : StudentUserFactory
        {
            #region DECLARATIONS

            protected Domain.Entities.Role createSectionInstructors = new Domain.Entities.Role(1, "UPDATE.SECTION.INSTRUCTORS");

            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configRepoMock;

            private ICurrentUserFactory currentUserFactory;

            private SectionInstructorsService service;

            private SectionInstructors sectionInstructor;
            private SectionMeeting sectionMeeting;
            private SectionFaculty sectionFaculty;

            private IEnumerable<Domain.Student.Entities.InstructionalMethod> instMethods;

            private string guid = "1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configRepoMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new SectionInstructorsUser();

                InitializeTestData();

                InitializeMock();

                service = new SectionInstructorsService(referenceDataRepositoryMock.Object, sectionRepositoryMock.Object, personRepositoryMock.Object,
                    configRepoMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                sectionRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            private void InitializeTestData()
            {
                sectionMeeting = new SectionMeeting("1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), "1") { };

                instMethods = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "desc", true){ }
                };

                sectionInstructor = new SectionInstructors()
                {
                    Id = "1",
                    InstructionalMethod = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    InstructionalEvents = new List<GuidObject2>() { new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc") },
                    Section = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    Instructor = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    WorkStartOn = DateTime.Today,
                    WorkEndOn = DateTime.Today.AddDays(10),
                    ResponsibilityPercentage = 10
                };

                sectionFaculty = new SectionFaculty("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), 10)
                {
                    SecMeetingIds = new List<string>() { "1" },
                    PrimaryIndicator = true,
                    LoadFactor = 10
                };
            }

            private void InitializeMock()
            {
                createSectionInstructors.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateSectionInstructors));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createSectionInstructors });
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(s => s.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                sectionRepositoryMock.Setup(s => s.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                personRepositoryMock.Setup(s => s.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionMeeting);
                sectionRepositoryMock.Setup(s => s.GetSectionMeetingGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                referenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodsAsync(false)).ReturnsAsync(instMethods);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_IntegrationApiException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("User StudentPO does not have permission to create or update section-instructors.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_IntegrationApiException_SectionInstructor_Null()
            {
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, null);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("SectionInstructors body is required in PUT or POST request. ", ex.Errors[0].Message);
                    Assert.AreEqual("The section instructors is a required parameter for updates/creates.", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_IntegrationApiException_InstructorMethod_Null()
            {
                sectionInstructor.InstructionalMethod.Id = null;
                sectionInstructor.InstructionalEvents = new List<GuidObject2>() { };
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The instructional Method is required for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Instructional method not found for GUID ''.", ex.Errors[1].Message);
                    Assert.AreEqual("Section faculty record $NEW for section 1 missing instructional method\r\nParameter name: instrMethod", ex.Errors[2].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_IntegrationApiException_Section_Null()
            {
                sectionInstructor.Section.Id = null;
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The section id is a required parameter for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Section is a required property.", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_IntegrationApiException_InstructorId_Null()
            {
                sectionInstructor.Instructor = null;
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("The instructor id is a required parameter for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Instructor is a required property.", ex.Errors[1].Message);
                    Assert.AreEqual("Error processing section faculty record $NEW for section 1: missing faculty ID \r\nParameter name: facultyId", ex.Errors[2].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_DtoToEntity_IntegrationApiException_InstMethods_Null()
            {
                referenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodsAsync(false)).ReturnsAsync(null);
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Instructional Methods call returns no values.  Required for updates/creates.", ex.Errors[0].Message);
                    Assert.AreEqual("Section faculty record $NEW for section 1 missing instructional method\r\nParameter name: instrMethod", ex.Errors[1].Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync_EntityToDto_IntegrationApiException_SectionFaculty_Null()
            {
                sectionInstructor.InstructionalMethod = null;

                sectionRepositoryMock.Setup(s => s.PutSectionFacultyAsync(It.IsAny<SectionFaculty>(), It.IsAny<string>())).ReturnsAsync(null);
                try
                {
                    await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("SectionFaculty Entity is a required parameter.", ex.Errors[0].Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionInstructorsService_UpdateSectionInstructorsAsync()
            {
                sectionRepositoryMock.Setup(s => s.PutSectionFacultyAsync(It.IsAny<SectionFaculty>(), It.IsAny<string>())).ReturnsAsync(sectionFaculty);

                var result = await service.UpdateSectionInstructorsAsync(guid, sectionInstructor);

                Assert.IsNotNull(result);
                Assert.AreEqual(sectionFaculty.Guid, result.Id);
            }
        }

        [TestClass]
        public class SectionInstructorsServiceTests_DELETE : StudentUserFactory
        {
            #region DECLARATIONS

            protected Domain.Entities.Role deleteSectionInstructors = new Domain.Entities.Role(1, "DELETE.SECTION.INSTRUCTORS");

            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configRepoMock;

            private ICurrentUserFactory currentUserFactory;

            private SectionInstructorsService service;

            private SectionFaculty sectionFaculty;

            private string guid = "1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configRepoMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new SectionInstructorsUser();

                InitializeTestData();

                InitializeMock();

                service = new SectionInstructorsService(referenceDataRepositoryMock.Object, sectionRepositoryMock.Object, personRepositoryMock.Object,
                    configRepoMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                sectionRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            private void InitializeTestData()
            {
                sectionFaculty = new SectionFaculty("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10), 10) { };
            }

            private void InitializeMock()
            {
                deleteSectionInstructors.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.DeleteSectionInstructors));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { deleteSectionInstructors });
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionInstructorsService_DeleteSectionInstructorsAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await service.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionInstructorsService_DeleteSectionInstructorsAsync_ArgumentNullException_Guid_Null()
            {
                await service.DeleteSectionInstructorsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionInstructorsService_DeleteSectionInstructorsAsync_KeyNotFoundException_SectionInstructor_NotFound()
            {
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await service.DeleteSectionInstructorsAsync(guid);
            }

            [TestMethod]
            public async Task SectionInstructorsService_DeleteSectionInstructorsAsync()
            {
                sectionRepositoryMock.Setup(s => s.GetSectionFacultyByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionFaculty);
                sectionRepositoryMock.Setup(s => s.DeleteSectionFacultyAsync(It.IsAny<SectionFaculty>(), It.IsAny<string>())).Returns(Task.FromResult(true));

                await service.DeleteSectionInstructorsAsync(guid);
            }
        }
    }
}