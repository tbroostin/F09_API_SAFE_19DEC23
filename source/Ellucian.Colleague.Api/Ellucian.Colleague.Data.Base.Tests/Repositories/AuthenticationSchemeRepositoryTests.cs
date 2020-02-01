// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class AuthenticationSchemeRepositoryTests : BaseRepositorySetup
    {
        private AuthenticationSchemeRepository authenticationSchemeRepo;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            transManagerMock.Setup(tm => tm.ExecuteAnonymousAsync<GetAuthenticationSchemeRequest, GetAuthenticationSchemeResponse>(It.IsAny<GetAuthenticationSchemeRequest>())).Returns(
                (GetAuthenticationSchemeRequest request) =>
                {
                    var response = new GetAuthenticationSchemeResponse()
                    {


                    };
                    if (request.Username == "notfound")
                    {
                        response.AuthenticationScheme = "";
                        response.ErrorMessage = "user not found";
                        response.ErrorOccurred = "1";
                    }
                    else if (request.Username == "colleagueauthuser")
                    {
                        response.AuthenticationScheme = "COLLEAGUE";
                        response.ErrorMessage = "";
                        response.ErrorOccurred = "";
                    }
                    else if (request.Username == "nullauthuser")
                    {
                        response.AuthenticationScheme = "";
                        response.ErrorMessage = "";
                        response.ErrorOccurred = "";
                    }
                    else if (request.Username == "error")
                    {
                        response = null;
                    }
                    return Task.FromResult(response);
                });

            authenticationSchemeRepo = new AuthenticationSchemeRepository(cacheProvider, transFactory, logger, new Configuration.ColleagueSettings());
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            authenticationSchemeRepo = null;
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_ColleagueAuthUser_Succeeds()
        {
            var actualResult = await authenticationSchemeRepo.GetAuthenticationSchemeAsync("colleagueauthuser");
            Assert.AreEqual("COLLEAGUE", actualResult.Code);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_NullAuthUser_Succeeds()
        {
            var actualResult = await authenticationSchemeRepo.GetAuthenticationSchemeAsync("nullauthuser");
            Assert.IsNull(actualResult);

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAuthenticationSchemeAsync_ErrorOccurred_ThrowsException()
        {
            var actualResult = await authenticationSchemeRepo.GetAuthenticationSchemeAsync("notfound");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAuthenticationSchemeAsync_NoTransactionResponse_ThrowsException()
        {
            var actualResult = await authenticationSchemeRepo.GetAuthenticationSchemeAsync("error");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAuthenticationSchemeAsync_NoUserGiven_ThrowsException()
        {
            var actualResult = await authenticationSchemeRepo.GetAuthenticationSchemeAsync(null);
        }

    }
}
