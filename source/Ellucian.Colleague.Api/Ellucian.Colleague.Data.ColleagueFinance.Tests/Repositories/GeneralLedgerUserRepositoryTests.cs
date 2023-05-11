// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    /// <summary>
    /// This class tests the methods in the GeneralLedgerUser repository. 
    /// </summary>
    [TestClass]
    public class GeneralLedgerUserRepositoryTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerUserRepository repository = null;
        private Mock<IColleagueDataReader> DataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository = null;
        private GeneralLedgerUser generalLedgerUserDomainEntity;
        private Mock<ICacheProvider> cacheProviderMock = new Mock<ICacheProvider>();
        private ApiSettings apiSettings;

        // Set variables that are constant for all unit tests.
        private GeneralLedgerUser glUser = null;
        private IEnumerable<string> expenseValues = new List<string>() { "5", "7" };
        private IEnumerable<string> revenueValues = new List<string>() { "4" };
        private IEnumerable<string> assetValues = new List<string>() { "1" };
        private IEnumerable<string> liabilityValues = new List<string>() { "2" };
        private IEnumerable<string> fundBalanceValues = new List<string>() { "3" };
        private string classificationName = "GL.CLASS";
        private GeneralLedgerClassConfiguration glClassConfiguration;
        private string[] glAcctsRolesList;
        private string[] expenseGlAcctsList;
        private string[] revenueGlAcctsList;
        string[] fullAccessGlAccts;

        #region GeneralLedgerUserDataContract
        public class GeneralLedgerUserDataContract
        {
            public Person personDataContract { get; set; }
            public Staff staffDataContract { get; set; }
            public Glusers glUsersDataContract { get; set; }
            public Glroles glRolesDataContract { get; set; }

            public GeneralLedgerUserDataContract()
            {
                personDataContract = null;
                staffDataContract = null;
                glUsersDataContract = null;
                glRolesDataContract = null;
            }
        }

        private List<GeneralLedgerUserDataContract> generalLedgerUserDataContracts;
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            // Set up a mock data reader. All Colleague repositories have a local instance of an IColleagueDataReader.
            // We don't want the unit tests to rely on a real Colleague data reader 
            // (which would require a real Colleague environment).
            // Instead, we create a mock data reader which we can control locally.
            DataReader = new Mock<IColleagueDataReader>();
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();
            apiSettings = new ApiSettings("TEST");
            repository = BuildValidGeneralLedgerUserRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            // Initialize variables to be used by mock
            generalLedgerUserDataContracts = new List<GeneralLedgerUserDataContract>();

            // Initialize GL class configuration values.
            expenseValues = new List<string>() { "5", "7" };
            revenueValues = new List<string>() { "4" };
            assetValues = new List<string>() { "1" };
            liabilityValues = new List<string>() { "2" };
            fundBalanceValues = new List<string>() { "3" };
            classificationName = "GL.CLASS";
            glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, assetValues, liabilityValues, fundBalanceValues);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
            DataReader = null;
            transactionInvoker = null;
            testGeneralLedgerUserRepository = null;
            generalLedgerUserDataContracts = null;

            expenseValues = null;
            revenueValues = null;
            classificationName = null;
            glClassConfiguration = null;
        }
        #endregion

        #region GetGeneralLedgerUserAsync
        [TestMethod]
        public async Task GetGeneralLedgerUser_AccessSpecified()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // Make sure the All GL accounts list is correct.
            Assert.AreEqual(this.glAcctsRolesList.Count(), glUser.AllAccounts.Count);
            foreach (var expectedGlAccount in this.glAcctsRolesList)
            {
                var actualGlAccount = glUser.AllAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(expectedGlAccount, actualGlAccount);
            }

            // Make sure the expense accounts are correct.
            Assert.AreEqual(this.expenseGlAcctsList.Count(), glUser.ExpenseAccounts.Count);
            foreach (var expectedExpenseAccount in this.expenseGlAcctsList)
            {
                var actualExpenseAccount = glUser.ExpenseAccounts.FirstOrDefault(x => x == expectedExpenseAccount);
                Assert.IsNotNull(expectedExpenseAccount, actualExpenseAccount);
            }

            Assert.AreEqual(GlAccessLevel.Possible_Access, glUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_GlAcctsRolesSelectReturnsNull()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result.
            this.glAcctsRolesList = null;
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // AllAccounts and ExpenseAccounts should be empty.
            Assert.IsTrue(!glUser.AllAccounts.Any());
            Assert.IsTrue(!glUser.ExpenseAccounts.Any());
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_GlAcctsRolesSelectReturnsEmptySet()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result.
            var tempGlAcctsList = this.glAcctsRolesList.ToList();
            tempGlAcctsList.RemoveAll(x => true);
            this.glAcctsRolesList = tempGlAcctsList.ToArray();
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // AllAccounts and ExpenseAccounts should be empty.
            Assert.IsTrue(!glUser.AllAccounts.Any());
            Assert.IsTrue(!glUser.ExpenseAccounts.Any());
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_GlAcctsSelectReturnsNull()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result.
            this.expenseGlAcctsList = null;
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // The GL user should have GL access, but no access to expense accounts.
            Assert.IsTrue(glUser.AllAccounts.Any());
            Assert.IsTrue(!glUser.ExpenseAccounts.Any());

            // The GL user should have GL access level 'Possible_Access'
            Assert.AreEqual(GlAccessLevel.Possible_Access, glUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_GlAcctsSelectReturnsEmpty()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result.
            var tempGlAcctsList = this.expenseGlAcctsList.ToList();
            tempGlAcctsList.RemoveAll(x => true);
            this.expenseGlAcctsList = tempGlAcctsList.ToArray();
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // The GL user should have GL access, but no access to expense accounts.
            Assert.IsTrue(glUser.AllAccounts.Any());
            Assert.IsTrue(!glUser.ExpenseAccounts.Any());

            // The GL user should have GL access level 'Possible_Access'
            Assert.AreEqual(GlAccessLevel.Possible_Access, glUser.GlAccessLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_NullId()
        {
            string personId = null;
            var fullAccessRole = "ALL-ACCESS";
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_EmptyId()
        {
            string personId = "";
            var fullAccessRole = "ALL-ACCESS";
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_NullGlClassificationName()
        {
            var personId = "0000005";
            var fullAccessRole = "ALL-ACCESS";
            classificationName = null;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_EmptyGlClassificationName()
        {
            var personId = "0000005";
            var fullAccessRole = "ALL-ACCESS";
            classificationName = "";
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_NullExpenseClassValues()
        {
            var personId = "0000005";
            var fullAccessRole = "ALL-ACCESS";
            expenseValues = null;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_EmptyExpenseClassValues()
        {
            var personId = "0000005";
            var fullAccessRole = "ALL-ACCESS";
            expenseValues = new List<string>();
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetGeneralLedgerUser_MissingPersonDataContract()
        {
            var personId = "000999Z";
            var fullAccessRole = "ALL-ACCESS";

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NullFullAccessRole()
        {
            var personId = "0000006";
            string fullAccessRole = null;

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NoActiveGlRoles()
        {
            var personId = "0000007";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.IsTrue(!generalLedgerUser.AllAccounts.Any());
            Assert.IsTrue(!generalLedgerUser.ExpenseAccounts.Any());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_ExpiredGlAccessRecord()
        {
            var personId = "0000008";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NoGlAccessRecord()
        {
            var personId = "0000009";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NoStaffLoginId()
        {
            var personId = "0000010";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NoStaffRecord()
        {
            var personId = "0000011";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullAccess()
        {
            var personId = "0000005";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var glUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(this.fullAccessGlAccts.Count(), glUser.AllAccounts.Count);
            foreach (var expectedGlAccount in this.fullAccessGlAccts)
            {
                var actualGlAccount = glUser.AllAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(expectedGlAccount, actualGlAccount);
            }

            // Make sure the expense accounts are correct.
            Assert.AreEqual(this.expenseGlAcctsList.Count(), glUser.ExpenseAccounts.Count);
            foreach (var expectedExpenseAccount in this.expenseGlAcctsList)
            {
                var actualExpenseAccount = glUser.ExpenseAccounts.FirstOrDefault(x => x == expectedExpenseAccount);
                Assert.IsNotNull(expectedExpenseAccount, actualExpenseAccount);
            }

            Assert.AreEqual(GlAccessLevel.Full_Access, glUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullAccessRoleThatStartsAndEndsToday()
        {
            var personId = "0000027";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullAccess_AndOtherActiveRoles()
        {
            var personId = "0000012";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), 3);
            Assert.AreEqual(GlAccessLevel.Full_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NoFullAccess_AndOtherActiveRoles()
        {
            var personId = "0000006";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), 3);
            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NullFullGlAccessRoleWithActiveRole()
        {
            var personId = "0000015";
            var fullAccessRole = "ALL-GL";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_NullFullGlAccessRoleWithNoActiveRole()
        {
            var personId = "0000016";
            var fullAccessRole = "ALL-GL";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
        }
        [TestMethod]
        public async Task GetGeneralLedgerUser_MissingFullGlAccessRoleWithActiveRoles()
        {
            var personId = "0000013";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullGLAccessRoleNotForWebWithActiveRoles()
        {
            var personId = "0000014";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_ThreeActiveRegularRolesWithSecondRoleMissingStartDate()
        {
            var personId = "0000017";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_ThreeActiveRegularRolesWithSecondRoleMissingEndDate()
        {
            var personId = "0000018";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_ThreeActiveRegularRolesWithLastRoleMissingEndDate()
        {
            var personId = "0000019";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_ThreeActiveRegularRolesWithLastRoleMissingStartDate()
        {
            var personId = "0000020";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Possible_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_OneActiveRegularRoleWithRoleMissingStartDate()
        {
            var personId = "0000021";
            var fullAccessRole = "FULL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_MissingGlusersStartDate()
        {
            var personId = "0000022";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_OneActiveFullAccessRoleWithStartAndEndDate()
        {
            var personId = "0000023";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Full_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_OneActiveFullAccessRoleWithStartAndEndDateWebFullAccessRole()
        {
            var personId = "0000024";
            var fullAccessRole = "WALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            Assert.AreEqual(GlAccessLevel.Full_Access, generalLedgerUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullAccessWithGlusersEndDate()
        {
            var personId = "0000025";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);


            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), 3);
            Assert.AreEqual(GlAccessLevel.Full_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FutureGlAccessRecord()
        {
            var personId = "0000026";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullGlAccessWithNoGlAccounts()
        {
            var personId = "0000030";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser_FullGlAccessWithNoGlAcctsRoles()
        {
            var personId = "0000031";
            var fullAccessRole = "ALL-ACCESS";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var generalLedgerUser = await repository.GetGeneralLedgerUserAsync(personId, fullAccessRole, classificationName, expenseValues);

            #region Execute the tests
            // Make sure the GeneralLedgerUser information is correct
            Assert.AreEqual(generalLedgerUser.ExpenseAccounts.Count(), generalLedgerUserDomainEntity.ExpenseAccounts.Count());
            Assert.AreEqual(GlAccessLevel.No_Access, generalLedgerUser.GlAccessLevel);
            #endregion
        }

        #endregion

        #region GetGeneralLedgerUserAsync2
        [TestMethod]
        public async Task GetGeneralLedgerUserAsync2_Success()
        {
            var personId = "0000033";
            var fullAccessRole = "ALL-GL";
            this.generalLedgerUserDomainEntity = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            // Run the repository GetGeneralLedgerUser method and check the result;
            var glUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);

            // Make sure the All GL accounts list is correct.
            Assert.AreEqual(this.glAcctsRolesList.Count(), glUser.AllAccounts.Count);
            foreach (var expectedGlAccount in this.glAcctsRolesList)
            {
                var actualGlAccount = glUser.AllAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(expectedGlAccount, actualGlAccount);
            }

            // Make sure the expense accounts are correct.
            Assert.AreEqual(this.expenseGlAcctsList.Count(), glUser.ExpenseAccounts.Count);
            foreach (var expectedExpenseAccount in this.expenseGlAcctsList)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(expectedExpenseAccount));
            }

            // Make sure the revenue accounts are correct.
            Assert.AreEqual(this.revenueGlAcctsList.Count(), glUser.RevenueAccounts.Count);
            foreach (var expectedRevenueAccount in this.revenueGlAcctsList)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(expectedRevenueAccount));
            }

            Assert.AreEqual(GlAccessLevel.Possible_Access, glUser.GlAccessLevel);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                string personId = null;
                var fullAccessRole = "ALL-ACCESS";
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                string personId = "";
                var fullAccessRole = "ALL-ACCESS";
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullExpenseClassValues()
        {
            var expectedParamName = "expenseclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, null, revenueValues, assetValues, liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyExpenseClassValues()
        {
            var expectedParamName = "expenseclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, new List<string>(), revenueValues, assetValues, liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullRevenueClassValues()
        {
            var expectedParamName = "revenueclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, null, assetValues, liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyRevenueClassValues()
        {
            var expectedParamName = "revenueclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, new List<string>(), assetValues, liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullAssetClassValues()
        {
            var expectedParamName = "assetclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, null, liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyAssetClassValues()
        {
            var expectedParamName = "assetclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, new List<string>(), liabilityValues, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullLiabilityClassValues()
        {
            var expectedParamName = "liabilityclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, assetValues, null, fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyLiabilityClassValues()
        {
            var expectedParamName = "liabilityclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, assetValues, new List<string>(), fundBalanceValues);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_NullFundBalanceClassValues()
        {
            var expectedParamName = "fundbalanceclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, assetValues, liabilityValues, null);
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetGeneralLedgerUser2_EmptyFundBalanceClassValues()
        {
            var expectedParamName = "fundbalanceclassvalues";
            var actualParamName = "";
            try
            {
                var personId = "0000005";
                var fullAccessRole = "ALL-ACCESS";
                glClassConfiguration = new GeneralLedgerClassConfiguration(classificationName, expenseValues, revenueValues, assetValues, liabilityValues, new List<string>());
                var generalLedgerUser = await repository.GetGeneralLedgerUserAsync2(personId, fullAccessRole, glClassConfiguration);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region GetGlUserApprovalAndGlAccessAccountsAsync

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_SuccessWithGlAccessAndApprovalAccess()
        {
            var personId = "3333333";

            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            var allAccessAndApprovalAccountsFromTest = await testGeneralLedgerUserRepository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            // Make sure the All GL accounts list is correct.
            Assert.AreEqual(allAccessAndApprovalAccountsFromTest.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var testGlAccount in allAccessAndApprovalAccountsFromTest)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == testGlAccount);
                Assert.IsNotNull(testGlAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_SuccessWithGlAccessOnly()
        {
            var personId = "3333333";

            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            glAcctsRolesList = null;
            DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS.ROLES", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).Returns(() =>
            {
                return Task.FromResult(glAcctsRolesList);
            });

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            // Make sure the All GL accounts list is correct.
            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_SuccessWithNoGlAccessndApprovalAccess()
        {
            var personId = "3333333";

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, new List<string>());

            // Make sure the All GL accounts list is correct.
            Assert.AreEqual(glAcctsRolesList.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAcctsRolesList)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_SuccessWithNoGlAccessAndNoApprovalAccess()
        {
            var personId = "3333333";

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            glAcctsRolesList = null;
            DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS.ROLES", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).Returns(() =>
            {
                return Task.FromResult(glAcctsRolesList);
            });

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, new List<string>());

            Assert.AreEqual(0, allAccessAndApprovalAccountsFromRepo.Count());
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_NoStaffRecord()
        {
            var personId = "0000011";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_NoStaffLoginId()
        {
            var personId = "0000010";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_NoGlusersRecord()
        {
            var personId = "0000009";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_MissingGlusersStartDate()
        {
            var personId = "0000022";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_FutureGlusersStartDate()
        {
            var personId = "0000026";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_PastGlusersEndDate()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = DateTime.Today.AddDays(-1),
            };
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_NullApprovalRolesAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null
            };
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_EmptyApprovalRolesAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string>()
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_EmptyApprovalRoleIdAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string> { string.Empty },
                GlusApprStartDates = new List<DateTime?> { DateTime.Today.AddDays(-5) },
                GlusApprPolicyFlag = new List<string> { "Y" }
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_OneApprovalRolesIdWithNullStartDateAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string> { "APPROVAL1" },
                GlusApprStartDates = null,
                GlusApprPolicyFlag = new List<string> { "Y" }
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_OneApprovalRolesIdWithFutureStartDateAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string> { "APPROVAL1" },
                GlusApprStartDates = new List<DateTime?> { DateTime.Today.AddDays(+1) },
                GlusApprPolicyFlag = new List<string> { "Y" }
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_OneApprovalRolesIdWithPastEndDateAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string> { "APPROVAL1" },
                GlusApprStartDates = new List<DateTime?> { DateTime.Today.AddDays(-30) },
                GlusApprEndDates = new List<DateTime?> { DateTime.Today.AddDays(-3) },
                GlusApprPolicyFlag = new List<string> { "Y" }
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(glAccessAccounts.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var glAccount in glAccessAccounts)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == glAccount);
                Assert.IsNotNull(glAccount, repoGlAccount);
            }
        }

        [TestMethod]
        public async Task GetGlUserApprovalAndGlAccessAccountsAsync_DuplicateApprovalRolesIdWithDifferentDatesAssoc()
        {
            var personId = "3333333";
            IEnumerable<string> glAccessAccounts = new List<string>() { "11_00_02_01_33333_51111", "11_00_02_01_33333_55555" };
            var allAccessAndApprovalAccountsFromTest = await testGeneralLedgerUserRepository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            InitializeCtxResponsesAndDataContracts();
            InitializeMockMethods(personId);
            var glUsersRecord = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = DateTime.Today.AddDays(-5),
                GlusEndDate = null,
                GlusApprRoleIds = new List<string> { "APPROVAL1", "APPROVAL1" },
                GlusApprStartDates = new List<DateTime?> { DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-10) },
                GlusApprEndDates = new List<DateTime?> { DateTime.Today.AddDays(+100), null },
                GlusApprPolicyFlag = new List<string> { "Y", "Y" }
            };
            glUsersRecord.buildAssociations();
            DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUsersRecord.Recordkey, true)).ReturnsAsync(glUsersRecord);

            IEnumerable<string> allAccessAndApprovalAccountsFromRepo = await repository.GetGlUserApprovalAndGlAccessAccountsAsync(personId, glAccessAccounts);

            Assert.AreEqual(allAccessAndApprovalAccountsFromTest.Count(), allAccessAndApprovalAccountsFromRepo.Count());
            foreach (var testGlAccount in allAccessAndApprovalAccountsFromTest)
            {
                var repoGlAccount = allAccessAndApprovalAccountsFromRepo.FirstOrDefault(x => x == testGlAccount);
                Assert.IsNotNull(testGlAccount, repoGlAccount);
            }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// This private method builds a valid general ledger user repository, 
        /// with the appropriate mocks, from which we can test.
        /// </summary>
        /// <returns>Returns a GeneralLedgerUserRepository object.</returns>
        private GeneralLedgerUserRepository BuildValidGeneralLedgerUserRepository()
        {
            // A GeneralLedgerUserRepository requires four objects for its constructor:
            //    1. an implementation of a cache provider
            //    2. an implementation of a Colleague transaction factory
            //    3. an implementation of a logger
            //    4. an implementation of apisettings
            // We need the unit tests to be independent of "real" implementations of these classes, so we use
            // Moq to create mock implementations that are based on the same interfaces
            var cacheProviderObject = cacheProviderMock.Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(DataReader.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            // Using the three mock objects, we can now construct a repository.
            var generalLedgerUserRepository = new GeneralLedgerUserRepository(cacheProviderObject, transactionFactoryObject, loggerObject, apiSettings);


            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            return generalLedgerUserRepository;
        }

        /// <summary>
        /// Take the list of general ledger user domain entities and convert all of the contained objects into data contracts
        /// </summary>
        private void InitializeCtxResponsesAndDataContracts()
        {
            Glroles glRolesDataContract = null;
            Person personDataContract = null;
            Staff staffDataContract = null;
            Glusers glUsersDataContract = null;
            GeneralLedgerUserDataContract generalLedgerUserDataContract = null;

            #region GeneralLedgerUser full GL access
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };
            personDataContract = new Person()
            {
                Recordkey = "0000005",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000005",
                StaffLoginId = "GTT"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GTT",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser possible GL access
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "W"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000006",
                LastName = "Kleehammer"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000006",
                StaffLoginId = "AJK"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "AJK",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ANDY" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser no GL active roles
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000007",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000007",
                StaffLoginId = "TGL"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "TGL",
                GlusStartDate = new DateTime(2014, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2014, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with an active role
            glRolesDataContract = new Glroles()
            {
                Recordkey = null,
                GlrRoleUse = null
            };

            personDataContract = new Person()
            {
                Recordkey = "0000033",
                LastName = "Kleehammer"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000033",
                StaffLoginId = "AJK"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT6",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL=GL", "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with an expired GL access record
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000008",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000008",
                StaffLoginId = "TL2"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "TL2",
                GlusStartDate = new DateTime(2014, 01, 01),
                GlusEndDate = new DateTime(2014, 07, 31),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2014, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with no GL access record
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000009",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000009",
                StaffLoginId = "TL3"
            };

            glUsersDataContract = new Glusers()
            {

            };

            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with no staff login ID
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000010",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000010",
                StaffLoginId = ""
            };

            glUsersDataContract = new Glusers()
            {

            };

            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with no staff record
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000011",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {

            };

            glUsersDataContract = new Glusers()
            {

            };

            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with no person data contract
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "000999Z",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {

            };

            glUsersDataContract = new Glusers()
            {

            };

            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access and other roles
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000012",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000012",
                StaffLoginId = "GT5"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT5",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS", "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access and other roles, missing full access role
            glRolesDataContract = new Glroles()
            {
                Recordkey = null,
                GlrRoleUse = null
            };

            personDataContract = new Person()
            {
                Recordkey = "0000013",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000013",
                StaffLoginId = "GT2"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT2",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS", "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access and other roles, full access role for Colleague only
            glRolesDataContract = new Glroles()
            {
                Recordkey = "FULL-ACCESS",
                GlrRoleUse = "C"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000014",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000014",
                StaffLoginId = "GT2"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT2",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "FULL-ACCESS", "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access and other roles, null full access role
            glRolesDataContract = new Glroles()
            {
                Recordkey = null,
                GlrRoleUse = null
            };

            personDataContract = new Person()
            {
                Recordkey = "0000015",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000015",
                StaffLoginId = "GT6"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT6",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL=GL", "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has full GL access which is null and no other active roles
            glRolesDataContract = new Glroles()
            {
                Recordkey = null,
                GlrRoleUse = null
            };

            personDataContract = new Person()
            {
                Recordkey = "0000016",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000016",
                StaffLoginId = "GT7"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT7",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL=GL", "GTT3" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has 3 roles with 2nd role missing start date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000017",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000017",
                StaffLoginId = "GT8"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT8",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "GTT4", "ANDY", "GTT3" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), null, new DateTime(2015, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2120, 04, 01), new DateTime(2120, 04, 01), new DateTime(2120, 04, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has 3 roles with 2nd role missing end date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000018",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000018",
                StaffLoginId = "GT9"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT9",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "GTT4", "ANDY", "GTT3" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2015, 04, 01), null, new DateTime(2015, 04, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has 3 roles with 3rd role missing end date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000019",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000019",
                StaffLoginId = "GT10"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT10",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "GTT4", "ANDY", "GTT3" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01), new DateTime(2015, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2120, 04, 01), new DateTime(2120, 04, 01), null }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has 3 roles with 3rd role missing start date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000020",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000020",
                StaffLoginId = "GT11"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT11",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "GTT4", "ANDY", "GTT3" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01), new DateTime(2015, 01, 01), null },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2120, 04, 01), new DateTime(2120, 04, 01), new DateTime(2120, 04, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser has 1 roles with role missing start date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000021",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000021",
                StaffLoginId = "GT12"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GT12",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "GTT4" },
                GlusRoleStartDates = new List<DateTime?>() { null },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2120, 04, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with missing GL user start date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000022",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000022",
                StaffLoginId = "SSS"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "SSS",
                GlusStartDate = null,
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2014, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with full access role with start and end date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000023",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000023",
                StaffLoginId = "TTT"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "TTT",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2200, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with Web full access role with start and end date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "WALL-ACCESS",
                GlrRoleUse = "W"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000024",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000024",
                StaffLoginId = "UUU"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "UUU",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "WALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2200, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access with gl user end date
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };
            personDataContract = new Person()
            {
                Recordkey = "0000025",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000025",
                StaffLoginId = "GHG"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GHG",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusEndDate = new DateTime(2200, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01) }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with a future GL access record
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000026",
                LastName = "Longerbeam"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000026",
                StaffLoginId = "TL9"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "TL9",
                GlusStartDate = new DateTime(2200, 01, 01),
                GlusEndDate = new DateTime(2200, 07, 31),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2014, 01, 01) },
                GlusRoleEndDates = new List<DateTime?>() { new DateTime(2014, 07, 31) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access role that starts and ends today
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };
            personDataContract = new Person()
            {
                Recordkey = "0000027",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000027",
                StaffLoginId = "GFG"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GFG",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { DateTime.Today },
                GlusRoleEndDates = new List<DateTime?>() { DateTime.Today }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access role that returns no GL accounts
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "B"
            };
            personDataContract = new Person()
            {
                Recordkey = "0000030",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000030",
                StaffLoginId = "GZZ"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GZZ",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ALL-ACCESS" },
                GlusRoleStartDates = new List<DateTime?>() { DateTime.Today },
                GlusRoleEndDates = new List<DateTime?>() { DateTime.Today }
            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser full GL access role that returns no GL accounts from GL.ACCTS.ROLES
            glRolesDataContract = new Glroles()
            {
                Recordkey = "ALL-ACCESS",
                GlrRoleUse = "W"
            };

            personDataContract = new Person()
            {
                Recordkey = "0000031",
                LastName = "Thorne"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "0000031",
                StaffLoginId = "GTZ"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "GTZ",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ANDY" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01) }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion

            #region GeneralLedgerUser with possible access in approval roles

            glRolesDataContract = new Glroles()
            {
                Recordkey = "APPROVAL1",
                GlrRoleUse = "W"
            };

            personDataContract = new Person()
            {
                Recordkey = "3333333",
                LastName = "Last name for 3333333"
            };

            staffDataContract = new Staff()
            {
                Recordkey = "3333333",
                StaffLoginId = "APR"
            };

            glUsersDataContract = new Glusers()
            {
                Recordkey = "APR",
                GlusStartDate = new DateTime(2015, 01, 01),
                GlusRoleIds = new List<string>() { "ANDY" },
                GlusRoleStartDates = new List<DateTime?>() { new DateTime(2015, 01, 01) },
                ApprovalRolesEntityAssociation = new List<GlusersApprovalRoles>()
                {
                    (new GlusersApprovalRoles( "APPROVAL1", DateTime.Today.AddDays(-5), null, null, null, null, null, null, "Y"))
                }

            };
            generalLedgerUserDataContract = new GeneralLedgerUserDataContract();
            generalLedgerUserDataContract.glRolesDataContract = glRolesDataContract;
            generalLedgerUserDataContract.personDataContract = personDataContract;
            generalLedgerUserDataContract.staffDataContract = staffDataContract;
            generalLedgerUserDataContract.glUsersDataContract = glUsersDataContract;
            this.generalLedgerUserDataContracts.Add(generalLedgerUserDataContract);
            #endregion
        }

        /// <summary>
        /// Set up all mock methods to return pre-defined data
        /// </summary>
        private void InitializeMockMethods(string personId)
        {
            // Mock ReadRecord to return a pre-defined generalLedgerUserDataContract
            foreach (var glUser in this.generalLedgerUserDataContracts)
            {
                // Mock DataReadRecord to return a Person data contract
                if (glUser.personDataContract.Recordkey != "000999Z")
                {
                    DataReader.Setup(acc => acc.ReadRecordAsync<Person>("PERSON", glUser.personDataContract.Recordkey, true)).ReturnsAsync(glUser.personDataContract);
                }
                else
                {
                    var tempPersonDataContract = new Person();
                    tempPersonDataContract = null;
                    DataReader.Setup(acc => acc.ReadRecordAsync<Person>("PERSON", glUser.personDataContract.Recordkey, true)).ReturnsAsync(tempPersonDataContract);
                }

                // Mock DataReadRecord to return a Staff data contract
                DataReader.Setup(acc => acc.ReadRecordAsync<Staff>("STAFF", glUser.staffDataContract.Recordkey, true)).ReturnsAsync(glUser.staffDataContract);

                // Mock DataReadRecord to return a Glusers data contract
                DataReader.Setup(acc => acc.ReadRecordAsync<Glusers>("GLUSERS", glUser.glUsersDataContract.Recordkey, true)).ReturnsAsync(glUser.glUsersDataContract);

                // Mock DataReadRecord to return a Glroles data contract
                DataReader.Setup(acc => acc.ReadRecordAsync<Glroles>("GLROLES", glUser.glRolesDataContract.Recordkey, true)).ReturnsAsync(glUser.glRolesDataContract);
            }

            // Mock the get of the name hierarchy for the preferred name.
            DataReader.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "PREFERRED",
                    NahNameHierarchy = new List<string>() { "PF" }
                });

            // Mock the DataReader.Select on GLROLES
            var glRolesList = new string[] { "ALL-ACCESS", "ANDY", "GTT4", "WALL-ACCESS" };
            if (personId == "3333333")
            {
                glRolesList = new string[] { "APPROVAL1" };
            }
            DataReader.Setup(acc => acc.SelectAsync("GLROLES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(glRolesList);

            // Mock DataReader.SelectAsync on GL.ACCTS.ROLES for a user with full access
            fullAccessGlAccts = new string[]
            {
                    "1000001101001",
                    "1000005308001",
                    "1010005308001",
                    "1020005308001",
                    "1020005308002",
                    "1020005308003",
                    "1020005308004",
                    "1020005308005",
                    "1020005308006",
                    "1020005308007",
            };
            DataReader.Setup(dr => dr.SelectAsync("GL.ACCTS.ROLES", "")).Returns(() =>
                {
                    return Task.FromResult(fullAccessGlAccts);
                });

            // Mock the DataReader.SelectAsync on GL.ACCTS.ROLES for a user with possible access.
            glAcctsRolesList = null;
            if (personId != "0000031")
            {
                glAcctsRolesList = new string[] { "1000001101001", "1000005308001", "1010005308001", "1020005308001" };
            }
            if (personId == "0000033")
            {
                glAcctsRolesList = this.generalLedgerUserDomainEntity.AllAccounts.ToArray();
            }

            // User with approval role.
            if (personId == "3333333")
            {
                glAcctsRolesList = new string[] { "11_00_02_01_33333_52222", "11_00_02_01_33333_53333", "11_00_02_01_33333_54444" };
            }
            DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS.ROLES", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).Returns(() =>
                {
                    return Task.FromResult(glAcctsRolesList);
                });

            if (personId == "0000033")
            {
                // Mock the data reader for expense accounts
                StringBuilder expenseValues = new StringBuilder(null);
                foreach (string expenseValue in this.glClassConfiguration.ExpenseClassValues)
                {
                    if (!string.IsNullOrEmpty(expenseValue))
                    {
                        expenseValues.Append("'" + expenseValue + "'");
                    }
                }
                string expenseCriteria = glClassConfiguration.ClassificationName + " EQ " + expenseValues;
                expenseGlAcctsList = this.generalLedgerUserDomainEntity.ExpenseAccounts.ToArray();
                DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS", It.IsAny<string[]>(), expenseCriteria)).Returns(() =>
                {
                    return Task.FromResult(expenseGlAcctsList);
                });

                // Mock the data reader for revenue accounts
                StringBuilder revenueValues = new StringBuilder(null);
                foreach (string revenueValue in this.glClassConfiguration.RevenueClassValues)
                {
                    if (!string.IsNullOrEmpty(revenueValue))
                    {
                        revenueValues.Append("'" + revenueValue + "'");
                    }
                }
                string revenueCriteria = glClassConfiguration.ClassificationName + " EQ " + revenueValues;
                revenueGlAcctsList = this.generalLedgerUserDomainEntity.RevenueAccounts.ToArray();
                DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS", It.IsAny<string[]>(), revenueCriteria)).Returns(() =>
                {
                    return Task.FromResult(revenueGlAcctsList);
                });

            }
            else
            {
                // Mock the DataReader.Select on GL.ACCTS
                expenseGlAcctsList = null;
                if (personId != "0000030")
                {
                    expenseGlAcctsList = new string[] { "1000005308001", "1010005308001", "1020005308001" };
                }
                DataReader.Setup(acc => acc.SelectAsync("GL.ACCTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
                {
                    return Task.FromResult(expenseGlAcctsList);
                });
            }
        }
        #endregion
    }
}