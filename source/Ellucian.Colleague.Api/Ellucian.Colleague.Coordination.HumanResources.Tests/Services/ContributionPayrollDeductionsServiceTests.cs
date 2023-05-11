/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{

    [TestClass]
    public class ContributionPayrollDeductionsServiceTestsGet : CurrentUserSetup
    {

        private Mock<IContributionPayrollDeductionsRepository> _contributionPayrollDeductionsRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private ContributionPayrollDeductionsService _contributionPayrollDeductionsService;
        private IEnumerable<Domain.HumanResources.Entities.PayrollDeduction> _contributionPayrollDeductionsEntities;
        private Tuple<IEnumerable<Domain.HumanResources.Entities.PayrollDeduction>, int> _contributionPayrollDeductionsEntityTuple;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
    
        private readonly int offset = 0;
        private readonly int limit = 4;
        private readonly string contributionPayrollDeductionsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private readonly string arrangementGuid = "775c69ff-280b-4ed3-9474-662a43616a8a";


        [TestInitialize]
        public void Initialize()
        {
            _contributionPayrollDeductionsRepositoryMock = new Mock<IContributionPayrollDeductionsRepository>();

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            BuildData();
            // Set up current user
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            _contributionPayrollDeductionsService = new ContributionPayrollDeductionsService(_contributionPayrollDeductionsRepositoryMock.Object, baseConfigurationRepository,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _contributionPayrollDeductionsEntityTuple = null;
            _contributionPayrollDeductionsEntities = null;
            _contributionPayrollDeductionsRepositoryMock = null;
            _adapterRegistryMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task ContributionPayrollDeductions_GETAllAsync()
        {
            var actualsTuple =
                await
                    _contributionPayrollDeductionsService.GetContributionPayrollDeductionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);

            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = _contributionPayrollDeductionsEntities.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductions_GETAllFilterAsync()
        {
            var actualsTuple =
                await
                    _contributionPayrollDeductionsService.GetContributionPayrollDeductionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = _contributionPayrollDeductionsEntities.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Amount, actual.Amount.Value);
                Assert.AreEqual(expected.DeductionDate, actual.DeductedOn);
                
                
            }
        }

        [TestMethod]
        public async Task ContributionPayrollDeductions_GETAllAsync_EmptyTuple()
        {
            _contributionPayrollDeductionsEntities = new List<Domain.HumanResources.Entities.PayrollDeduction>()
            {
            };
            _contributionPayrollDeductionsEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayrollDeduction>, int>(_contributionPayrollDeductionsEntities, 0);
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).ReturnsAsync(_contributionPayrollDeductionsEntityTuple);
            var actualsTuple = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task ContributionPayrollDeductions_GET_ById()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            var expected = _contributionPayrollDeductionsEntities.ToList()[0];
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionByGuidAsync(id)).ReturnsAsync(expected);
            var actual = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsByGuidAsync(id);

            Assert.IsNotNull(actual);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(expected.Amount, actual.Amount.Value);
            Assert.AreEqual(expected.DeductionDate, actual.DeductedOn);
           
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task ContributionPayrollDeductions_GET_ById_NullId_ArgumentNullException()
        {
            var actual = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ContributionPayrollDeductions_GET_ById_ReturnsNullEntity_KeyNotFoundException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionByGuidAsync(id)).Throws<KeyNotFoundException>();
            var actual = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsByGuidAsync(id);
        }

        
        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task ContributionPayrollDeductions_GET_ById_ReturnsNullEntity_Exception()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionByGuidAsync(id)).Throws<Exception>();
            var actual = await _contributionPayrollDeductionsService.GetContributionPayrollDeductionsByGuidAsync(id);
        }

        private void BuildData()
        {

            _contributionPayrollDeductionsEntities = new List<Domain.HumanResources.Entities.PayrollDeduction>()
            {
                new PayrollDeduction(contributionPayrollDeductionsGuid, "123", "456", new DateTime(2017, 01, 01), "USD", 52),
                new PayrollDeduction("905c69ff-280b-4ed3-9474-662a43616a8a", "123", "456", new DateTime(2017, 01, 02), "USD", 60)

            };
            _contributionPayrollDeductionsEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayrollDeduction>, int>(_contributionPayrollDeductionsEntities, _contributionPayrollDeductionsEntities.Count());
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).ReturnsAsync(_contributionPayrollDeductionsEntityTuple);
            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetContributionPayrollDeductionByGuidAsync(It.IsAny<string>())).ReturnsAsync(_contributionPayrollDeductionsEntities.ToList()[0]);

            var personGuidCollection = new Dictionary<string, string>();
            personGuidCollection.Add("123", arrangementGuid);
            personGuidCollection.Add("456", arrangementGuid);

            _contributionPayrollDeductionsRepositoryMock.Setup(i => i.GetPerbenGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                


        }
    }
}