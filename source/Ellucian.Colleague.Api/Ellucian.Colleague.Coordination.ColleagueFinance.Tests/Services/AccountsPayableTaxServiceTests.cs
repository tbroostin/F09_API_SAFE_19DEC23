// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// This class tests the AccountsPayableTaxService class.
    /// </summary>
    [TestClass]
    public class AccountsPayableTaxServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private AccountsPayableTaxService accountsPayableTaxService = null;
        private TestColleagueFinanceReferenceDataRepository testColleagueFinanceReferenceRepository = null;
        private UserFactorySubset currentUserFactory = new GeneralLedgerCurrentUser.UserFactorySubset();

        [TestInitialize]
        public void Initialize()
        {
            // Build the service object to use each of the user factories built above
            BuildValidAccountsPayableTaxService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            accountsPayableTaxService = null;
            testColleagueFinanceReferenceRepository = null;
        }
        #endregion

        [TestMethod]
        public async Task GetAccountsPayableTaxes()
        {
            // Get the accounts payable types
            var accountsPayableTaxDtos = await accountsPayableTaxService.GetAccountsPayableTaxesAsync();
            var accountsPayableTaxDomainEntities = await testColleagueFinanceReferenceRepository.GetAccountsPayableTaxCodesAsync();

            // Confirm that we have the correct number of accounts payable taxes
            Assert.IsTrue(accountsPayableTaxDtos.Count() == accountsPayableTaxDomainEntities.Count());

            // Confirm that the DTO and domain entities lists have the same data
            foreach (var type in accountsPayableTaxDtos)
            {
                Assert.IsTrue(accountsPayableTaxDomainEntities.Any(x =>
                    x.Code == type.Code
                    && x.Description == type.Description));
            }
        }

        /// <summary>
        /// Builds a accounts payable type service object.
        /// </summary>
        /// <returns>Nothing.</returns>
        private void BuildValidAccountsPayableTaxService()
        {
            // An AccountsPayableTaxService requires six parameters for its constructor:
            //   1. an accounts payable tax repository object.
            //   2. an adapterRegistry object.
            //   3. a UserFactory object.
            //   4. a role repository object.
            //   5. a logger object.
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            // Set up mock objects
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            // Initialize test repository
            testColleagueFinanceReferenceRepository = new TestColleagueFinanceReferenceDataRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var accountsPayableTaxDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.AccountsPayableTax, Dtos.ColleagueFinance.AccountsPayableTax>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.AccountsPayableTax, Dtos.ColleagueFinance.AccountsPayableTax>()).Returns(accountsPayableTaxDtoAdapter);

            // Set up a list of project types and set up the service.
            accountsPayableTaxService = new AccountsPayableTaxService(testColleagueFinanceReferenceRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
        }
    }
}