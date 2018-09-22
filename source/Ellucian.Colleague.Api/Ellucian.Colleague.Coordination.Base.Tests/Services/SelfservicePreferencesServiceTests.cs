// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class SelfservicePreferencesServiceTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ISelfservicePreferencesRepository> selfservicePreferencesRepoMock;
        private ISelfservicePreferencesRepository selfservicePreferencesRepository;
        private ILogger logger;
        private ICurrentUserFactory currentUserFactory;
        private SelfservicePreferencesService selfservicePreferencesService;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            selfservicePreferencesRepoMock = new Mock<ISelfservicePreferencesRepository>();
            selfservicePreferencesRepository = selfservicePreferencesRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            //selfservicePreferencesRepoMock.Setup(repo => repo.DeletePreferenceAsync("",""));

            selfservicePreferencesService = new SelfservicePreferencesService(selfservicePreferencesRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistry = null;
            roleRepo = null;
            selfservicePreferencesRepository = null;
            selfservicePreferencesService = null;
        }

        #region Preference Tests

        [TestMethod]
        public async Task DeletePreference()
        {
            await selfservicePreferencesService.DeletePreferenceAsync("0000015", "some-preference");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task DeletePreferenceInvalidPersonId()
        {
            await selfservicePreferencesService.DeletePreferenceAsync("invalid", "somepreference");
        }

        #endregion
    }

    // sets up a current user
    public abstract class CurrentUserSetup
    {
        protected Role personRole = new Role(105, "Student");
        protected Ellucian.Colleague.Domain.Entities.Role updatePersonRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.PERSON");
        protected Ellucian.Colleague.Domain.Entities.Role createPersonRole = new Ellucian.Colleague.Domain.Entities.Role(2, "CREATE.PERSON");


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
                        UserName = "Student",
                        Roles = new List<string>() { "Student" },
                        SessionFixationId = "abc123",
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
                        Roles = new List<string>() { "CREATE.PERSON", "UPDATE.PERSON" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
