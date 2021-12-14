//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.


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
using System.Collections;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentRegistrationEligibilitiesServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewStudentRegistrationEligibilitiesRequest = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STU.REGISTRATION.ELIGIBILITY");

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
                            Roles = new List<string>() { "VIEW.STU.REGISTRATION.ELIGIBILITY" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentRegistrationEligibilitiesServiceTests_GET : CurrentUserSetup
        {
            #region DECLARATIONS

            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRegistrationPriorityRepository> registrationPriorityRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<RegistrationEligibility> registrationEligibilityMock;
            private string studentId = "2cb5e697-8168-4203-b48b-c667556cfb8a";

            private ICurrentUserFactory currentUserFactory;

            StudentRegistrationEligibilitiesService studentRegEligibilitiesService;

            IEnumerable<Domain.Student.Entities.Term> terms;
            IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            IEnumerable<Domain.Student.Entities.RegistrationEligibility> registrationEligibilities;
            IEnumerable<RegistrationMessage> registrationMessages;
            IEnumerable<RegistrationPriority> registrationPriorities;
            RegistrationEligibilityTerm termEligibiligy;


            #endregion

            [TestInitialize]
            public void Initialize()
            {
                personRepositoryMock = new Mock<IPersonRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                registrationPriorityRepositoryMock = new Mock<IRegistrationPriorityRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                registrationEligibilityMock = new Mock<RegistrationEligibility>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentRegistrationEligibilitiesRequest });
                termRepositoryMock.Setup(t => t.GetAsync()).ReturnsAsync(terms);

                studentRegEligibilitiesService = new StudentRegistrationEligibilitiesService(personRepositoryMock.Object, studentRepositoryMock.Object, registrationPriorityRepositoryMock.Object,
                    termRepositoryMock.Object, referenceDataRepositoryMock.Object, adapterRegistryMock.Object, _configurationRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            private void BuildData()
            {
                terms = new List<Term>()
                {
                    new Term("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "Description", new DateTime(2016, 01,01), new DateTime(2016, 05,01), 2016, 1, false, false, "Spring", false)
                };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                { new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01,01), new DateTime(2016, 05,01), 2016, 1, "Spring", "", "", null) };

                registrationMessages = new List<RegistrationMessage>()
                {
                    new RegistrationMessage() {Message = "Message", SectionId = "SectionId"}
                };

                registrationPriorities = new List<RegistrationPriority>()
                {
                    new RegistrationPriority("998", "0000894", "2016/Spr", DateTime.Today.AddYears(1), DateTime.Today.AddYears(2))
                };

                registrationEligibilities = new List<RegistrationEligibility>()
                {
                    new RegistrationEligibility(registrationMessages, true),
                    new RegistrationEligibility(registrationMessages, false)
                };

                termEligibiligy = new RegistrationEligibilityTerm("2016/Spr", false, true)
                {
                    Message = "Message",
                    AnticipatedTimeForAdds = DateTime.Today,
                    Status = RegistrationEligibilityTermStatus.NotEligible
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personRepositoryMock = null;
                studentRepositoryMock = null;
                referenceDataRepositoryMock = null;
                termRepositoryMock = null;
                registrationPriorityRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                studentRegEligibilitiesService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_StudentId_ArgumentNullException()
            {
                await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(null, It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_AcademicPeriodId_ArgumentNullException()
            {
                await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, null, It.IsAny<bool>());
            }

            [TestMethod]
           
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_PersonId_Invalid()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, "AcademicPeriodId", It.IsAny<bool>());
            }

           
            [TestMethod]
            
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_AcademicPeriod_Invalid()
            {
                viewStudentRegistrationEligibilitiesRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStuRegistrationEligibility));
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");

                await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, "AcademicPeriodId", It.IsAny<bool>());
            }

            [TestMethod]
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_Eligible()
            {
                viewStudentRegistrationEligibilitiesRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStuRegistrationEligibility));
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                registrationPriorityRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>())).ReturnsAsync(registrationPriorities);

                var registrationEligibility = registrationEligibilities.FirstOrDefault();

                studentRepositoryMock.Setup(s => s.CheckRegistrationEligibilityEthosAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(registrationEligibility);
                //registrationEligibilityMock.Setup(r => r.UpdateRegistrationPriorities(It.IsAny<IEnumerable<RegistrationPriority>>(), It.IsAny<IEnumerable<Term>>()));

                registrationEligibility.AddRegistrationEligibilityTerm(new RegistrationEligibilityTerm("2016/Spr", true, true));

                var result = await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, academicPeriods.FirstOrDefault().Guid, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Student.Id, studentId);
                Assert.AreEqual(result.AcademicPeriod.Id, academicPeriods.FirstOrDefault().Guid);
                Assert.AreEqual(result.EligibilityStatus, Dtos.EnumProperties.StudentRegistrationEligibilitiesEligibilityStatus.Eligible);
                Assert.IsNull(result.IneligibilityReasons);
            }

            [TestMethod]
            public async Task StudentRegElgService_GetStudentRegistrationEligibilitiesAsync_Ineligible()
            {
                viewStudentRegistrationEligibilitiesRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStuRegistrationEligibility));
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                registrationPriorityRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>())).ReturnsAsync(registrationPriorities);

                var registrationEligibility = registrationEligibilities.LastOrDefault();

                studentRepositoryMock.Setup(s => s.CheckRegistrationEligibilityEthosAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(registrationEligibility);

                registrationEligibility.AddRegistrationEligibilityTerm(termEligibiligy);

                var result = await studentRegEligibilitiesService.GetStudentRegistrationEligibilitiesAsync(studentId, academicPeriods.FirstOrDefault().Guid, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Student.Id, studentId);
                Assert.AreEqual(result.AcademicPeriod.Id, academicPeriods.FirstOrDefault().Guid);
                Assert.AreEqual(result.EligibilityStatus, Dtos.EnumProperties.StudentRegistrationEligibilitiesEligibilityStatus.Ineligible);


                var message = string.Join(",", registrationMessages.Select(r => r.Message).ToArray()) + "," + termEligibiligy.Message;

                Assert.AreEqual(string.Join(",", result.IneligibilityReasons.ToArray()), message);
            }
        }
    }
}