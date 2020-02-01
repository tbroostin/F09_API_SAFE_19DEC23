// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class AuthenticationSchemeServiceTests
    {
        private AuthenticationSchemeService authenticationSchemeService;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IAuthenticationSchemeRepository> authenticationSchemeRepoMock;
        private IAuthenticationSchemeRepository authenticationSchemeRepo;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            authenticationSchemeRepoMock = new Mock<IAuthenticationSchemeRepository>();
            authenticationSchemeRepo = authenticationSchemeRepoMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            var currentUserFactory = currentUserFactoryMock.Object;
            authenticationSchemeService = new AuthenticationSchemeService(adapterRegistry, currentUserFactory, roleRepo, logger, authenticationSchemeRepo);

            var authSchemeAdapter = new AutoMapperAdapter<Domain.Base.Entities.AuthenticationScheme, Dtos.Base.AuthenticationScheme>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.AuthenticationScheme, Dtos.Base.AuthenticationScheme>()).Returns(authSchemeAdapter);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_ColleagueAuthUser_Succeeds()
        {
            var expectedResult = new Domain.Base.Entities.AuthenticationScheme("COLLEAGUE");
            authenticationSchemeRepoMock.Setup(asr => asr.GetAuthenticationSchemeAsync("colleagueauthuser"))
                .ReturnsAsync(expectedResult);
            var actualResult = await authenticationSchemeService.GetAuthenticationSchemeAsync("colleagueauthuser");
            Assert.AreEqual(expectedResult.Code, actualResult.Code);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_NullAuthUser_Succeeds()
        {
            Domain.Base.Entities.AuthenticationScheme expectedResult = null;
            authenticationSchemeRepoMock.Setup(asr => asr.GetAuthenticationSchemeAsync("nullauthuser"))
                .ReturnsAsync(expectedResult);
            var actualResult = await authenticationSchemeService.GetAuthenticationSchemeAsync("nullauthuser");
            Assert.IsNull(actualResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAuthenticationSchemeAsync_RepoThrowsException_Rethrows()
        {
            authenticationSchemeRepoMock.Setup(asr => asr.GetAuthenticationSchemeAsync("repoexceptionuser"))
                .ThrowsAsync(new ApplicationException("repo exception"));
            var actualResult = await authenticationSchemeService.GetAuthenticationSchemeAsync("repoexceptionuser");
        }

    }
}
