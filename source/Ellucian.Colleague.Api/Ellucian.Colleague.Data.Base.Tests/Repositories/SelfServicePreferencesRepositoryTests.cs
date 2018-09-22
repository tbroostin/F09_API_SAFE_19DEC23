// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class SelfServicePreferencesRepositoryTests : BaseRepositorySetup
    {
        ISelfservicePreferencesRepository repository;
        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            
            repository = BuildValidRepository();
        }

        private SelfservicePreferencesRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Mock the call for creating/updating a person
            transManagerMock.Setup<Task<DeleteSelfservicePreferenceResponse>>(
                manager => manager.ExecuteAsync<DeleteSelfservicePreferenceRequest, DeleteSelfservicePreferenceResponse>(
                    It.IsAny<DeleteSelfservicePreferenceRequest>())
                ).Returns<DeleteSelfservicePreferenceRequest>(request =>
                {
                    if (request.Prefkey == "error")
                    {
                        return Task.FromResult(new DeleteSelfservicePreferenceResponse()
                        {
                            ErrorMessage = "Error has occurred",
                            ErrorOccurred = "1"
                        });
                    }
                    else
                    {
                        return Task.FromResult(new DeleteSelfservicePreferenceResponse()
                        {
                            ErrorMessage = "",
                            ErrorOccurred = "0"
                        });
                    }

                });

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();

            // Set up data accessor for mocking 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

            return new SelfservicePreferencesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletePreferenceAsync_NullPersonIdException()
        {
            await repository.DeletePreferenceAsync(null, "prefKey");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBasePersonAsync_NullPrefKeyException()
        {
            await repository.DeletePreferenceAsync("0000015", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task DeletePreferenceAsync_Error()
        {
            await repository.DeletePreferenceAsync("0000015", "error");
        }

        [TestMethod]
        public async Task DeletePreferenceAsync_Success()
        {
            await repository.DeletePreferenceAsync("0000015", "success");
        }
    }
}
