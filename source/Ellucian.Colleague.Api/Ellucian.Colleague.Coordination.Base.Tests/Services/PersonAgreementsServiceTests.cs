// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonAgreementsServiceTests
    {
        private Mock<IAgreementsRepository>  _repositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;

        private IAgreementsRepository _repository;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;

        private PersonAgreementsService _service;

        // Sets up a Current user
        private abstract class CurrentUserSetup
        {
            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Matt",
                            PersonId = "0003914",
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

        [TestInitialize]
        public void PersonAgreementServiceTests_Initialize()
        {
            _repositoryMock = new Mock<IAgreementsRepository>();
            _repository = _repositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleRepository = _roleRepositoryMock.Object;

            _loggerMock = new Mock<ILogger>();
            _logger = _loggerMock.Object;

            _service = new PersonAgreementsService(_repository, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
        }

        [TestCleanup]
        public void PersonAgreementServiceTests_Cleanup()
        {
            _repositoryMock = null;
            _repository = null;

            _adapterRegistryMock = null;
            _adapterRegistry = null;

            _currentUserFactory = null;

            _roleRepositoryMock = null;
            _roleRepository = null;

            _loggerMock = null;
            _logger = null;

            _service = null;
        }

        [TestClass]
        public class PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_Tests : PersonAgreementsServiceTests
        {
            private List<Domain.Base.Entities.PersonAgreement> _entities;

            [TestInitialize]
            public void PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_Initialize()
            {
                base.PersonAgreementServiceTests_Initialize();

                _entities = new List<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement>()
                {
                    new Domain.Base.Entities.PersonAgreement("1", _currentUserFactory.CurrentUser.PersonId, "AGR1", "PER1", true, "Agreement 1", DateTime.Today.AddDays(30), new List<string>() {"Agreement 1 Text" }, Domain.Base.Entities.PersonAgreementStatus.Accepted, DateTime.Now.AddDays(-3)),
                    new Domain.Base.Entities.PersonAgreement("2", _currentUserFactory.CurrentUser.PersonId, "AGR2", "PER2", true, "Agreement 2", DateTime.Today.AddDays(29), new List<string>() {"Agreement 2 Text" }, Domain.Base.Entities.PersonAgreementStatus.Declined, DateTime.Now.AddDays(-7)),
                    new Domain.Base.Entities.PersonAgreement("3", _currentUserFactory.CurrentUser.PersonId, "AGR3", "PER3", false, "Agreement 3", DateTime.Today.AddDays(28), new List<string>() {"Agreement 3 Text" }, null, null),
                    new Domain.Base.Entities.PersonAgreement("4", _currentUserFactory.CurrentUser.PersonId, "AGR4", "PER4", true, "Agreement 4", DateTime.Today.AddDays(27), new List<string>() {"Agreement 4 Text" }, null, null)
                };

                _repositoryMock.Setup(repo => repo.GetPersonAgreementsByPersonIdAsync(_currentUserFactory.CurrentUser.PersonId)).ReturnsAsync(_entities);

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>()).Returns(adapter);
            }

            [TestCleanup]
            public void PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_Cleanup()
            {
                base.PersonAgreementServiceTests_Cleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_null_ID_throws_ArgumentNullException()
            {
                var agreements = await _service.GetPersonAgreementsByPersonIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_other_user_data_throws_PermissionsException()
            {
                var agreements = await _service.GetPersonAgreementsByPersonIdAsync(_currentUserFactory.CurrentUser.PersonId + "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_logs_exception_from_repository_and_throws_ApplicationException()
            {
                _repositoryMock.Setup(repo => repo.GetPersonAgreementsByPersonIdAsync(_currentUserFactory.CurrentUser.PersonId)).ThrowsAsync(new Exception("Repository exception!"));

                var agreements = await _service.GetPersonAgreementsByPersonIdAsync(_currentUserFactory.CurrentUser.PersonId);

                _loggerMock.Verify(log => log.Error(It.IsAny<Exception>(), string.Format("An error occurred while retrieving person agreements for person {0}.", _currentUserFactory.CurrentUser.PersonId)));
            }

            [TestMethod]
            public async Task PersonAgreementsService_GetPersonAgreementsByPersonIdAsync_returns_user_data()
            {
                var agreements = await _service.GetPersonAgreementsByPersonIdAsync(_currentUserFactory.CurrentUser.PersonId);

                Assert.IsNotNull(agreements);
                Assert.AreEqual(_entities.Count, agreements.Count());
                for(int i = 0; i < _entities.Count; i++)
                {
                    Assert.AreEqual(_entities[i].ActionTimestamp, agreements.ElementAt(i).ActionTimestamp);
                    Assert.AreEqual(_entities[i].AgreementCode, agreements.ElementAt(i).AgreementCode);
                    Assert.AreEqual(_entities[i].AgreementPeriodCode, agreements.ElementAt(i).AgreementPeriodCode);
                    Assert.AreEqual(_entities[i].Title, agreements.ElementAt(i).Title);
                    Assert.AreEqual(_entities[i].DueDate, agreements.ElementAt(i).DueDate);
                    Assert.AreEqual(_entities[i].Id, agreements.ElementAt(i).Id);
                    Assert.AreEqual(_entities[i].PersonCanDeclineAgreement, agreements.ElementAt(i).PersonCanDeclineAgreement);
                    Assert.AreEqual(_entities[i].PersonId, agreements.ElementAt(i).PersonId);
                    Assert.AreEqual(_entities[i].Status.ToString(), agreements.ElementAt(i).Status.ToString());
                    CollectionAssert.AreEqual(_entities[i].Text.ToList(), agreements.ElementAt(i).Text.ToList());
                }
            }
        }

        [TestClass]
        public class PersonAgreementsService_UpdatePersonAgreementAsync_Tests : PersonAgreementsServiceTests
        {
            private List<Domain.Base.Entities.PersonAgreement> _entities;

            [TestInitialize]
            public void PersonAgreementsService_UpdatePersonAgreementAsync_Initialize()
            {
                base.PersonAgreementServiceTests_Initialize();

                _entities = new List<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement>()
                {
                    new Domain.Base.Entities.PersonAgreement("1", _currentUserFactory.CurrentUser.PersonId, "AGR1", "PER1", true, "Agreement 1", DateTime.Today.AddDays(30), new List<string>() {"Agreement 1 Text" }, Domain.Base.Entities.PersonAgreementStatus.Accepted, DateTime.Now.AddDays(-3)),
                    new Domain.Base.Entities.PersonAgreement("2", _currentUserFactory.CurrentUser.PersonId, "AGR2", "PER2", true, "Agreement 2", DateTime.Today.AddDays(29), new List<string>() {"Agreement 2 Text" }, Domain.Base.Entities.PersonAgreementStatus.Declined, DateTime.Now.AddDays(-7)),
                    new Domain.Base.Entities.PersonAgreement("3", _currentUserFactory.CurrentUser.PersonId, "AGR3", "PER3", false, "Agreement 3", DateTime.Today.AddDays(28), new List<string>() {"Agreement 3 Text" }, null, null),
                    new Domain.Base.Entities.PersonAgreement("4", _currentUserFactory.CurrentUser.PersonId, "AGR4", "PER4", true, "Agreement 4", DateTime.Today.AddDays(27), new List<string>() {"Agreement 4 Text" }, null, null)
                };

                _repositoryMock.Setup(repo => repo.UpdatePersonAgreementAsync(It.IsAny<Domain.Base.Entities.PersonAgreement>())).ReturnsAsync(_entities[0]);

                var entityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>()).Returns(entityAdapter);

                var dtoAdapter = new AutoMapperAdapter<PersonAgreement, Ellucian.Colleague.Domain.Base.Entities.PersonAgreement>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<PersonAgreement, Ellucian.Colleague.Domain.Base.Entities.PersonAgreement>()).Returns(dtoAdapter);

            }

            [TestCleanup]
            public void PersonAgreementsService_UpdatePersonAgreementAsync_Cleanup()
            {
                base.PersonAgreementServiceTests_Cleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_Id_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = null,
                    PersonId = _currentUserFactory.CurrentUser.PersonId,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_PersonId_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = null,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_Status_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = null,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = (PersonAgreementStatus?)null,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_ActionTimestamp_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = null,
                    ActionTimestamp = (DateTimeOffset?)null,
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_null_agreement_Title_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = null,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = null,
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_empty_agreement_Title_throws_ArgumentNullException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = null,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = string.Empty,
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_other_user_data_throws_PermissionsException()
            {
                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = _currentUserFactory.CurrentUser.PersonId+"1",
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_logs_exception_from_repository_and_throws_ApplicationException()
            {
                _repositoryMock.Setup(repo => repo.UpdatePersonAgreementAsync(It.IsAny<Domain.Base.Entities.PersonAgreement>())).ThrowsAsync(new Exception("Repository exception!"));

                var agreements = await _service.UpdatePersonAgreementAsync(new PersonAgreement() { Id = "1", PersonId = _currentUserFactory.CurrentUser.PersonId, ActionTimestamp = DateTime.Now.AddDays(-3), AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1", Title = "Agreement 1", DueDate = DateTime.Today.AddDays(30), PersonCanDeclineAgreement = true, Status = PersonAgreementStatus.Accepted, Text = new List<string>() { "Agreement 1 Text" } });

                _loggerMock.Verify(log => log.Error(It.IsAny<Exception>(), string.Format("An error occurred while updating person agreement 1 for person {0}.", _currentUserFactory.CurrentUser.PersonId)));
            }

            [TestMethod]
            public async Task PersonAgreementsService_UpdatePersonAgreementAsync_returns_correct_data()
            {
                var updatedAgreement = await _service.UpdatePersonAgreementAsync(new PersonAgreement()
                {
                    Id = "1",
                    PersonId = _currentUserFactory.CurrentUser.PersonId,
                    ActionTimestamp = DateTime.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(30),
                    PersonCanDeclineAgreement = true,
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() { "Agreement 1 Text" }
                });

                Assert.IsNotNull(updatedAgreement);
                Assert.AreEqual(_entities[0].ActionTimestamp, updatedAgreement.ActionTimestamp);
                Assert.AreEqual(_entities[0].AgreementCode, updatedAgreement.AgreementCode);
                Assert.AreEqual(_entities[0].AgreementPeriodCode, updatedAgreement.AgreementPeriodCode);
                Assert.AreEqual(_entities[0].Title, updatedAgreement.Title);
                Assert.AreEqual(_entities[0].DueDate, updatedAgreement.DueDate);
                Assert.AreEqual(_entities[0].Id, updatedAgreement.Id);
                Assert.AreEqual(_entities[0].PersonCanDeclineAgreement, updatedAgreement.PersonCanDeclineAgreement);
                Assert.AreEqual(_entities[0].PersonId, updatedAgreement.PersonId);
                Assert.AreEqual(_entities[0].Status.ToString(), updatedAgreement.Status.ToString());
                CollectionAssert.AreEqual(_entities[0].Text.ToList(), updatedAgreement.Text.ToList());
            }
        }
    }
}
