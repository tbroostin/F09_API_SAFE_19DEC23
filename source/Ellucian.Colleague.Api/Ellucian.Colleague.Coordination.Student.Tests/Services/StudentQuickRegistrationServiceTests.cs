// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class StudentQuickRegistrationServiceTests
    {
        // Sets up a Current user that is a student
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
                            Name = "Johnny",
                            PersonId = "0000894",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentQuickRegistrationService_GetStudentQuickRegistrationAsync_Tests : StudentQuickRegistrationServiceTests
        {
            private Mock<IStudentQuickRegistrationRepository> _repoMock;
            private IStudentQuickRegistrationRepository _repo;
            private Mock<IStudentRepository> _studentRepoMock;
            private IStudentRepository _studentRepo;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private Mock<IConfigurationRepository> _baseConfigurationRepositoryMock;
            private IConfigurationRepository _baseConfigurationRepository;
            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private string _studentId;
            private Domain.Student.Entities.QuickRegistration.StudentQuickRegistration _entity;

            private StudentQuickRegistrationService _service;

            [TestInitialize]
            public void Initialize()
            {
                _repoMock = new Mock<IStudentQuickRegistrationRepository>();
                _repo = _repoMock.Object;
                _studentRepoMock = new Mock<IStudentRepository>();
                _studentRepo = _studentRepoMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                _baseConfigurationRepository = _baseConfigurationRepositoryMock.Object;
                _loggerMock = new Mock<ILogger>();
                _logger = _loggerMock.Object;

                _studentId = _currentUserFactory.CurrentUser.PersonId;
                _entity = new Domain.Student.Entities.QuickRegistration.StudentQuickRegistration(_studentId);
                var term = new Domain.Student.Entities.QuickRegistration.QuickRegistrationTerm("2019/FA");
                term.AddSection(new Domain.Student.Entities.QuickRegistration.QuickRegistrationSection("123", 3, Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.Active));
                term.AddSection(new Domain.Student.Entities.QuickRegistration.QuickRegistrationSection("234", 3, Domain.Student.Entities.GradingType.Audit, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted));
                term.AddSection(new Domain.Student.Entities.QuickRegistration.QuickRegistrationSection("345", null, Domain.Student.Entities.GradingType.PassFail, Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister));
                _entity.AddTerm(term);

                _repoMock.Setup(repo => repo.GetStudentQuickRegistrationAsync(It.IsAny<string>())).ReturnsAsync(_entity);

                var quickRegistrationSectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationSection, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationSection>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationSection, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationSection>()).Returns(quickRegistrationSectionAdapter);

                var quickRegistrationTermAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationTerm, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationTerm>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationTerm, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationTerm>()).Returns(quickRegistrationTermAdapter);

                var studentQuickRegistrationAdapter = new StudentQuickRegistrationEntityToStudentQuickRegistrationDtoAdapter(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.StudentQuickRegistration, Ellucian.Colleague.Dtos.Student.QuickRegistration.StudentQuickRegistration>()).Returns(studentQuickRegistrationAdapter);

                _service = new StudentQuickRegistrationService(_repo, _adapterRegistry, _currentUserFactory, _roleRepo, _logger, _studentRepo, _baseConfigurationRepository);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentQuickRegistrationService_GetStudentQuickRegistrationAsync_null_StudentId_throws_ArgumentNullException()
            {
                var data = await _service.GetStudentQuickRegistrationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentQuickRegistrationService_GetStudentQuickRegistrationAsync_empty_StudentId_throws_ArgumentNullException()
            {
                var data = await _service.GetStudentQuickRegistrationAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentQuickRegistrationService_GetStudentQuickRegistrationAsync_throws_permissions_exception_for_user_requesting_another_user_data()
            {
                var data = await _service.GetStudentQuickRegistrationAsync("NOT_ME");
            }

            [TestMethod]
            public async Task StudentQuickRegistrationService_GetStudentQuickRegistrationAsync_returns_StudentQuickRegistration_for_user_requesting_own_data()
            {
                var data = await _service.GetStudentQuickRegistrationAsync(_currentUserFactory.CurrentUser.PersonId);
                Assert.AreEqual(_entity.StudentId, data.StudentId);
                Assert.AreEqual(_entity.Terms.Count, data.Terms.Count());
                for(int i = 0; i < _entity.Terms.Count; i++)
                {
                    Assert.AreEqual(_entity.Terms[i].TermCode, data.Terms.ElementAt(i).TermCode);
                    Assert.AreEqual(_entity.Terms[i].Sections.Count, data.Terms.ElementAt(i).Sections.Count());
                    for(int j = 0; j < _entity.Terms[i].Sections.Count; j++)
                    {
                        Assert.AreEqual(_entity.Terms[i].Sections[j].SectionId, data.Terms.ElementAt(i).Sections.ElementAt(j).SectionId);
                        Assert.AreEqual(_entity.Terms[i].Sections[j].Credits, data.Terms.ElementAt(i).Sections.ElementAt(j).Credits);
                        Assert.AreEqual(_entity.Terms[i].Sections[j].GradingType.ToString(), data.Terms.ElementAt(i).Sections.ElementAt(j).GradingType.ToString());
                        Assert.AreEqual(_entity.Terms[i].Sections[j].WaitlistStatus.ToString(), data.Terms.ElementAt(i).Sections.ElementAt(j).WaitlistStatus.ToString());
                    }
                }
            }
        }
    }
}
