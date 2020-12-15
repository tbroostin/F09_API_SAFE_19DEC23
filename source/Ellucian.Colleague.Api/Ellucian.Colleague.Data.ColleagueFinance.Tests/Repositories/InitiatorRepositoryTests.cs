//Copyright 2020 Ellucian Company L.P.and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class InitiatorRepositoryTests : BaseRepositorySetup
    {
        #region DECLARATIONS
        private Collection<Base.DataContracts.Staff> staffs;
        private GetHierarchyNamesForIdsResponse hierarchyNamesForIdsResponse;
        private GetPersonLookupStringResponse personLookupStringResponse;
        private InitiatorRepository initiatorRepository;
        private string queryKeyword = "0000143";
        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            apiSettings = new ApiSettings("TEST");

            MockInitialize();

            InitializeTestData();

            InitializeTestMock();

            initiatorRepository = new InitiatorRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            initiatorRepository = null;
            hierarchyNamesForIdsResponse = null;
            personLookupStringResponse = null;
        }

        private void InitializeTestData()
        {

            staffs = new Collection<Base.DataContracts.Staff>() { new Base.DataContracts.Staff() { Recordkey = "1", StaffInitials = "JHN", StaffLoginId = "JHN" },
                new Base.DataContracts.Staff() { Recordkey = "2", StaffInitials = "AAA", StaffLoginId = "AAA" },
                new Base.DataContracts.Staff() { Recordkey = "3", StaffInitials = "BBB", StaffLoginId = "BBB" }
            };

            hierarchyNamesForIdsResponse = new GetHierarchyNamesForIdsResponse()
            {
                IoPersonIds = new List<string>() { "1", "2", "3" },
                IoHierarchies = new List<string>() { "PREFERRED", "PREFERRED", "PREFERRED" },
                OutPersonNames = new List<string>() { "John Doe", "John Doe2", "John Doe3" }
            };

            personLookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX JHN_DOE", ErrorMessage = "" };

        }

        private void InitializeTestMock()
        {
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Staff>("STAFF", It.IsAny<string[]>(), true)).ReturnsAsync(staffs);
            dataReaderMock.Setup(d => d.SelectAsync("STAFF", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2", "3" }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2", "3" }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2", "3" }.ToArray<string>());
            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transManagerMock.Setup(tio => tio.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(hierarchyNamesForIdsResponse);
            transManagerMock.Setup(tio => tio.ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>())).ReturnsAsync(personLookupStringResponse);

        }


        #endregion

        #region TEST METHODS
        #region GET BY KEYWORD

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InitiatorRepository_QueryInitiatorByKeywordAsync_SearchCriteria_As_Null()
        {
            await initiatorRepository.QueryInitiatorByKeywordAsync(null);

        }

        [TestMethod]
        public async Task InitiatorRepository_QueryInitiatorByKeywordAsync_No_Results()
        {
            dataReaderMock.Setup(d => d.SelectAsync("STAFF", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            var result =  await initiatorRepository.QueryInitiatorByKeywordAsync(queryKeyword);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task InitiatorRepository_QueryInitiatorByKeywordAsync_No_Results1()
        {
            queryKeyword = "John";
            dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            var result = await initiatorRepository.QueryInitiatorByKeywordAsync(queryKeyword);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task InitiatorRepository_QueryInitiatorByKeywordAsync_No_Results2()
        {
            queryKeyword = "John Doe";
            dataReaderMock.Setup(d => d.SelectAsync("STAFF", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            var result = await initiatorRepository.QueryInitiatorByKeywordAsync(queryKeyword);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task InitiatorRepository_QueryInitiatorByKeywordAsync()
        {
            queryKeyword = "John Doe";
            var result = await initiatorRepository.QueryInitiatorByKeywordAsync(queryKeyword);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count(), 3);
        }

    }

    #endregion

    #endregion
}
