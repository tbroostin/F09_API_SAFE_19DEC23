// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class GeneralLedgerAccountServiceTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerAccountService service = null;
        private GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts currentUserFactory = new GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts();
        private GeneralLedgerCurrentUser.UserFactoryAll userFactory_InsufficientAccess = new GeneralLedgerCurrentUser.UserFactoryAll();
        private TestGeneralLedgerAccountRepository testGlAccountRepository = new TestGeneralLedgerAccountRepository();
        private TestGeneralLedgerUserRepository testGlUserRepository = new TestGeneralLedgerUserRepository();
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories.GeneralLedgerCurrentUser.UserFactoryAll glUserFactoryAll = new GeneralLedgerCurrentUser.UserFactoryAll();
        private List<string> majorComponentStartPositions = new List<string>() { "1", "4", "7", "10", "13", "19" };

        [TestInitialize]
        public void Initialize()
        {
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testGlAccountRepository = null;
            testGlUserRepository = null;
            testGlConfigurationRepository = null;
        }
        #endregion

        #region GetAsync
        [TestMethod]
        public async Task GetAsync_Success()
        {
            var glAccountId = "11_00_02_01_20601_53011";
            var glAccountEntity = await testGlAccountRepository.GetAsync(glAccountId, majorComponentStartPositions);
            var glAccountDto = await service.GetAsync(glAccountId);

            Assert.AreEqual(glAccountEntity.Id, glAccountDto.Id);
            Assert.AreEqual(glAccountEntity.FormattedGlAccount, glAccountDto.FormattedId);
            Assert.AreEqual(glAccountEntity.Description, glAccountDto.Description);
        }

        [TestMethod]
        public async Task GetAsync_UserHasNoAccessToGlNumber()
        {
            // Set up the service to have insufficient GL access.
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, userFactory_InsufficientAccess);

            var glAccountId = "11_00_02_01_20601_53011";
            var glAccountEntity = await testGlAccountRepository.GetAsync(glAccountId, majorComponentStartPositions);
            var glAccountDto = await service.GetAsync(glAccountId);

            Assert.AreEqual(glAccountEntity.Id, glAccountDto.Id);
            Assert.IsTrue(string.IsNullOrEmpty(glAccountDto.FormattedId));
            Assert.IsTrue(string.IsNullOrEmpty(glAccountDto.Description));
        }

        [TestMethod]
        public async Task GetAsync_NoAccessToGlAccount()
        {
            var glAccountId = "11_00_02_01_00000_00000";
            var glAccountEntity = await testGlAccountRepository.GetAsync(glAccountId, majorComponentStartPositions);
            var glAccountDto = await service.GetAsync(glAccountId);

            Assert.AreEqual(glAccountEntity.Id, glAccountDto.Id);
            Assert.AreEqual("", glAccountDto.Description);
        }

        [TestMethod]
        public async Task GetAsync_NullGlAccountId()
        {
            var expectedParamName = "generalledgeraccountid";
            var actualParamName = "";
            try
            {
                await service.GetAsync(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetAsync_EmptyGlAccountId()
        {
            var expectedParamName = "generalledgeraccountid";
            var actualParamName = "";
            try
            {
                await service.GetAsync("");
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetAsync_GlAccountRepositoryReturnsNull()
        {
            var expectedMessage = "No general ledger account entity returned.";
            var actualMessage = "";

            // Set up the GL account repository to return null.
            GeneralLedgerAccount glAccountEntity = null;
            var glAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            glAccountRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(() =>
            {
                return Task.FromResult(glAccountEntity);
            });

            try
            {
                BuildService(testGlUserRepository, testGlConfigurationRepository, glAccountRepositoryMock.Object, currentUserFactory);
                await service.GetAsync("10_00_01_00_20601_51000");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_GlConfigurationRepositoryReturnsNullAccountStructure()
        {
            var expectedMessage = "Account structure must be defined.";
            var actualMessage = "";

            // Set up the GL account repository to return null.
            GeneralLedgerAccountStructure glAccountStructure = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(glAccountStructure);
            });

            try
            {
                BuildService(testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository, currentUserFactory);
                await service.GetAsync("10_00_01_00_20601_51000");
            }
            catch (ConfigurationException cnex)
            {
                actualMessage = cnex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_GlConfigurationRepositoryReturnsNullGlClassConfiguration()
        {
            var expectedMessage = "GL class configuration must be defined.";
            var actualMessage = "";

            // Set up the GL account repository to return null.
            GeneralLedgerClassConfiguration glAccountStructure = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            var accountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            glConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glAccountStructure);
            });

            try
            {
                BuildService(testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository, currentUserFactory);
                await service.GetAsync("10_00_01_00_20601_51000");
            }
            catch (ConfigurationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_GlUserRepositoryReturnsNull()
        {
            var expectedMessage = "GL user must be defined.";
            var actualMessage = "";

            // Set up the GL account repository to return null.
            GeneralLedgerUser glUser = null;
            var glUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            glUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });

            try
            {
                BuildService(glUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);
                await service.GetAsync("10_00_01_00_20601_51000");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region Validate GL Account Async

        [TestMethod]
        public async Task ValidateGlAccountAsync_Success()
        {
            var expenseIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddExpenseAccounts(expenseIds);
            allIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddAllAccounts(allIds);
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);

            string glAccountId = "11_00_01_02_ACTIV_50000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponse = testGlAccountRepository.glAccountValidationResponses.FirstOrDefault(x => x.Id == glAccountId);
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);

            Assert.AreEqual(glAccountValidationResponse.Id, glAccountValidationResponseDto.Id);
            Assert.AreEqual(glAccountValidationResponse.Status, glAccountValidationResponseDto.Status);
            Assert.AreEqual(glAccountValidationResponse.ErrorMessage, glAccountValidationResponseDto.ErrorMessage);
            Assert.AreEqual(glAccountValidationResponse.RemainingBalance, glAccountValidationResponseDto.RemainingBalance);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateGlAccountAsync_NullGlAccountId()
        {
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(null, fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateGlAccountAsync_EmptyGlAccountId()
        {
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync("", fiscalYear);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_ReplaceDashesSuccess()
        {
            var expenseIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddExpenseAccounts(expenseIds);
            allIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddAllAccounts(allIds);
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);

            string glAccountId = "11_00_01_02_ACTIV_50000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponse = testGlAccountRepository.glAccountValidationResponses.FirstOrDefault(x => x.Id == glAccountId);
            glAccountId = "11-00-01-02-ACTIV-50000";
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);

            Assert.AreEqual(glAccountValidationResponse.Id, glAccountValidationResponseDto.Id);
            Assert.AreEqual(glAccountValidationResponse.Status, glAccountValidationResponseDto.Status);
            Assert.AreEqual(glAccountValidationResponse.ErrorMessage, glAccountValidationResponseDto.ErrorMessage);
            Assert.AreEqual(glAccountValidationResponse.RemainingBalance, glAccountValidationResponseDto.RemainingBalance);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task ValidateGlAccountAsync_GlConfigurationRepositoryReturnsNullAccountStructure()
        {
            testGlConfigurationRepository.accountStructure = null;
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);

            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync("11_00_01_02_ACTIV_50000", fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task ValidateGlAccountAsync_GlConfigurationRepositoryReturnsNullClassConfigurationStructure()
        {
            // Set up the GL account repository to return null.
            GeneralLedgerClassConfiguration glClassStructure = null;
            var glConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            glConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glClassStructure);
            });
            var accountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            glConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).Returns(() =>
            {
                return Task.FromResult(accountStructure);
            });

            BuildService(testGlUserRepository, glConfigurationRepositoryMock.Object, testGlAccountRepository, currentUserFactory);

            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync("11_00_01_02_ACTIV_50000", fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ValidateGlAccountAsync_GeneralLedgerUserReturnsNullGlUser()
        {
            GeneralLedgerUser glUser = null;
            GeneralLedgerClassConfiguration glClassStructure = null;
            var generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync2("", "", glClassStructure)).Returns(() =>
            {
                return Task.FromResult(glUser);
            });
            BuildService(generalLedgerUserRepositoryMock.Object, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);

            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync("11_00_01_02_ACTIV_50000", fiscalYear);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_UserHasNoGlAccounts()
        {
            testGlUserRepository.GeneralLedgerUsers[32].RemoveAllAccounts();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync("11_00_01_02_ACTIV_50000", DateTime.Now.Year.ToString());

            Assert.AreEqual("11_00_01_02_ACTIV_50000", glAccountValidationResponseDto.Id);
            Assert.AreEqual("failure", glAccountValidationResponseDto.Status);
            Assert.AreEqual("You do not have access to this GL Account.", glAccountValidationResponseDto.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_UserHasNoAccessToThisGlAccount()
        {
            string glAccountId = "11_00_01_02_ACTIV_50000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponse = testGlAccountRepository.glAccountValidationResponses.FirstOrDefault(x => x.Id == glAccountId);
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);

            Assert.AreEqual(glAccountValidationResponse.Id, glAccountValidationResponseDto.Id);
            Assert.AreEqual("failure", glAccountValidationResponseDto.Status);
            Assert.AreEqual("You do not have access to this GL Account.", glAccountValidationResponseDto.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_GlAccountIsExcluded()
        {
            string glAccountId = "11_00_02_01_20601_40000";
            string fiscalYear = DateTime.Now.Year.ToString();
            testGlConfigurationRepository.exclusions = new BudgetAdjustmentAccountExclusions()
            {
                ExcludedElements = new List<BudgetAdjustmentExcludedElement>() {
                    new BudgetAdjustmentExcludedElement()
                        {
                            ExclusionComponent = new GeneralLedgerComponent("FULLACCOUNT", false, GeneralLedgerComponentType.FullAccount, "1", "18"),
                            ExclusionRange = new GeneralLedgerComponentRange("110002012060140000", "110002012060140000")
                    }
                }
            };
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);
            

            Assert.AreEqual("11_00_02_01_20601_40000", glAccountValidationResponseDto.Id);
            Assert.AreEqual("failure", glAccountValidationResponseDto.Status);
            Assert.AreEqual("The account is excluded from online budget adjustments.", glAccountValidationResponseDto.ErrorMessage);

            testGlConfigurationRepository.exclusions = new BudgetAdjustmentAccountExclusions();
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_GlAccountIsNotExpense()
        {
            string glAccountId = "11_00_02_01_20601_40000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);

            Assert.AreEqual("11_00_02_01_20601_40000", glAccountValidationResponseDto.Id);
            Assert.AreEqual("failure", glAccountValidationResponseDto.Status);
            Assert.AreEqual("The GL account is not an expense type.", glAccountValidationResponseDto.ErrorMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ValidateGlAccountAsync_GlAccountValidationResponseIsNull()
        {
            var expenseIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddExpenseAccounts(expenseIds);
            allIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddAllAccounts(allIds);

            GlAccountValidationResponse glAccountValidationResponse = null;
            var glAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            glAccountRepositoryMock.Setup(x => x.ValidateGlAccountAsync("", "")).Returns(() =>
            {
                return Task.FromResult(glAccountValidationResponse);
            });
            BuildService(testGlUserRepository, testGlConfigurationRepository, glAccountRepositoryMock.Object, currentUserFactory);

            string glAccountId = "11_00_01_02_ACTIV_50000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ValidateGlAccountAsync_GetAsyncResponseIsNull()
        {
            var expenseIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddExpenseAccounts(expenseIds);
            allIds.Add("11_00_01_02_ACTIV_50000");
            testGlUserRepository.GeneralLedgerUsers[32].AddAllAccounts(allIds);

            GeneralLedgerAccount glAccount = null;
            var glAccountRepositoryMock = new Mock<IGeneralLedgerAccountRepository>();
            glAccountRepositoryMock.Setup(x => x.GetAsync("11_00_01_02_ACTIV_50000", testGlConfigurationRepository.accountStructure.MajorComponentStartPositions)).Returns(() =>
            {
                return Task.FromResult(glAccount);
            });
            BuildService(testGlUserRepository, testGlConfigurationRepository, glAccountRepositoryMock.Object, currentUserFactory);

            string glAccountId = "11_00_01_02_ACTIV_50000";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponse = testGlAccountRepository.glAccountValidationResponses.FirstOrDefault(x => x.Id == glAccountId);
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_RepositoryReturnsFailure()
        {
            var expenseIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("11_00_01_02_INCTV_59999");
            testGlUserRepository.GeneralLedgerUsers[32].AddExpenseAccounts(expenseIds);
            allIds.Add("11_00_01_02_INCTV_59999");
            testGlUserRepository.GeneralLedgerUsers[32].AddAllAccounts(allIds);
            BuildService(testGlUserRepository, testGlConfigurationRepository, testGlAccountRepository, currentUserFactory);

            string glAccountId = "11_00_01_02_INCTV_59999";
            string fiscalYear = DateTime.Now.Year.ToString();
            var glAccountValidationResponse = testGlAccountRepository.glAccountValidationResponses.FirstOrDefault(x => x.Id == glAccountId);
            var glAccountValidationResponseDto = await service.ValidateGlAccountAsync(glAccountId, fiscalYear);

            Assert.AreEqual("11_00_01_02_INCTV_59999", glAccountValidationResponseDto.Id);
            Assert.AreEqual("failure", glAccountValidationResponseDto.Status);
            Assert.AreEqual(glAccountValidationResponse.ErrorMessage, glAccountValidationResponseDto.ErrorMessage);
        }

        #endregion

        #region Private methods
        private void BuildService(IGeneralLedgerUserRepository glUserRepository,
            IGeneralLedgerConfigurationRepository glConfigurationRepository,
            IGeneralLedgerAccountRepository glAccountRepository,
            ICurrentUserFactory userFactory)
        {
            // Use Mock to create mock implementations that are based on the same interfaces
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var journalEntryDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.JournalEntry, Dtos.ColleagueFinance.JournalEntry>()).Returns(journalEntryDtoAdapter);

            // Set up the services
            service = new GeneralLedgerAccountService(glUserRepository, glConfigurationRepository, glAccountRepository, adapterRegistry.Object, userFactory, roleRepository, loggerObject);
        }
        #endregion
    }
}