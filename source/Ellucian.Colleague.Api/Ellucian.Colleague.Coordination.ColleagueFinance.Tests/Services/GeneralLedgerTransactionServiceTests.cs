// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using CreditOrDebit = Ellucian.Colleague.Domain.ColleagueFinance.Entities.CreditOrDebit;
using CurrencyCodes = Ellucian.Colleague.Domain.ColleagueFinance.Entities.CurrencyCodes;
using GeneralLedgerTransaction = Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{

    [TestClass]
    public class GeneralLedgerTransactionServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private GeneralLedgerTransactionService _generalLedgerTransactionService = null;
      
        private TestGeneralLedgerUserRepository _testGeneralLedgerUserRepository;
        private TestGeneralLedgerConfigurationRepository _testGlConfigurationRepository;
        private GeneralLedgerAccountStructure _testGlAccountStructure;
        private GeneralLedgerClassConfiguration _testGlClassConfiguration;
        private Mock<IGeneralLedgerTransactionRepository>  _generalLedgerTransactionRepository;
        private Mock<IColleagueFinanceReferenceDataRepository> _referenceDataRepository;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        // Define user factories
        private readonly UserFactoryAll _glUserFactoryAll = new GeneralLedgerCurrentUser.UserFactoryAll();
      
        [TestInitialize]
        public void Initialize()
        {
            BuildValidGeneralLedgerTransactionService();

        }

        [TestCleanup]
        public void Cleanup()
        {
            _generalLedgerTransactionService = null;
            _testGeneralLedgerUserRepository = null;
            _testGlConfigurationRepository = null;
            _testGlAccountStructure = null;
            _testGlClassConfiguration = null;
            _personRepositoryMock = null;
           
        }
        #endregion

        #region GetGeneralLedgerTransactionsAsync test methods

        [TestMethod]
        public async Task GetGeneralLedgerTransactions_GetAsync()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
               .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(generalLedgerUser);



            var glTransactions = new List<GeneralLedgerTransaction>();
            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString()
            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));

            glTransactions.Add(glTransaction);

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };

            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransactions);

             var generalLedgerTransactionDtos =
                await _generalLedgerTransactionService.GetAsync();

            Assert.IsNotNull(generalLedgerTransactionDtos);

            var generalLedgerTransactionDto = generalLedgerTransactionDtos.FirstOrDefault();

            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected =
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }

        [TestMethod]
        public async Task GetGeneralLedgerTransactions_Get2Async()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions2().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
               .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(generalLedgerUser);


            _personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("123456");
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("123456");

            var glTransactions = new List<GeneralLedgerTransaction>();
            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString(),
                SubmittedBy = generalLedgerTransaction.SubmittedBy.ToString()
            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";

            glTransactions.Add(glTransaction);

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };

            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransactions);

            var generalLedgerTransactionDtos =
               await _generalLedgerTransactionService.Get2Async();

            Assert.IsNotNull(generalLedgerTransactionDtos);

            var generalLedgerTransactionDto = generalLedgerTransactionDtos.FirstOrDefault();

            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);
            Assert.AreEqual(generalLedgerTransaction.SubmittedBy.Id, generalLedgerTransactionDto.SubmittedBy.Id);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected =
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.SubmittedBy.Id, actualTransactionDetailLine.SubmittedBy.Id);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }
        
      
        [TestMethod]
        public async Task GetGeneralLedgerTransactions_GetByIdAsync()
        {

            const string accountNumber = "01-02-03-04-05550-66077";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
           var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

           //var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

           var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
           generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

          // var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
                .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            //var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(generalLedgerUser);

            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = "001234",
                ProcessMode = "Update"
            };
            var genLedgrTransaction = new GenLedgrTransaction("DN", DateTimeOffset.Now)
            {
                ReferenceNumber = referenceNumber, TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionNumber = "1"
                
            };

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(accountNumber, projectId, "DESC",
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };
            

             //var generalLedgerTransactionDomainEntity = await generalLedgerTransactionRepository.GetByIdAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            //_generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync("001234", generalLedgerUser.Id, GlAccessLevel.Full_Access)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);

           
            var generalLedgerTransactionDto = await _generalLedgerTransactionService.GetByIdAsync("001234");

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);

            var generalLedgerTransaction = generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(referenceNumber, generalLedgerTransaction.ReferenceNumber);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, generalLedgerTransaction.Type);

            Assert.IsNotNull(generalLedgerTransaction.TransactionDetailLines);
            //var transactionDetailLine =
            //    generalLedgerTransaction.TransactionDetailLines.FirstOrDefault(x => x.Description == "DESC");

            //Assert.AreEqual("DESC", transactionDetailLine.Description);
            //Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, transactionDetailLine.Amount.Currency);
            //Assert.AreEqual(25, transactionDetailLine.Amount.Value);
            //Assert.AreEqual(Dtos.EnumProperties.CreditOrDebit.Credit, transactionDetailLine.Type);
            //Assert.AreEqual( string.Concat(accountNumber,"*",projectId), transactionDetailLine.AccountingString);

        }

        [TestMethod]
        public async Task GetGeneralLedgerTransactions_GetById2Async()
        {

            const string accountNumber = "01-02-03-04-05550-66077";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            //var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            // var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
                .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            //var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(generalLedgerUser);

            _personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("123456");
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("123456");

            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = "001234",
                ProcessMode = "Update",
                SubmittedBy = "123456"
            };
            var genLedgrTransaction = new GenLedgrTransaction("DN", DateTimeOffset.Now)
            {
                ReferenceNumber = referenceNumber,
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                TransactionNumber = "1"

            };

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(accountNumber, projectId, "DESC",
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };


            //var generalLedgerTransactionDomainEntity = await generalLedgerTransactionRepository.GetByIdAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            //_generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync("001234", generalLedgerUser.Id, GlAccessLevel.Full_Access)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);


            var generalLedgerTransactionDto = await _generalLedgerTransactionService.GetById2Async("001234");

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);
            Assert.AreEqual("123456", generalLedgerTransactionDto.SubmittedBy.Id);

            var generalLedgerTransaction = generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(referenceNumber, generalLedgerTransaction.ReferenceNumber);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, generalLedgerTransaction.Type);

            Assert.IsNotNull(generalLedgerTransaction.TransactionDetailLines);
            //var transactionDetailLine =
            //    generalLedgerTransaction.TransactionDetailLines.FirstOrDefault(x => x.Description == "DESC");

            //Assert.AreEqual("DESC", transactionDetailLine.Description);
            //Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, transactionDetailLine.Amount.Currency);
            //Assert.AreEqual(25, transactionDetailLine.Amount.Value);
            //Assert.AreEqual("123456", transactionDetailLine.SubmittedBy.Id);
            //Assert.AreEqual(Dtos.EnumProperties.CreditOrDebit.Credit, transactionDetailLine.Type);
            //Assert.AreEqual(string.Concat(accountNumber, "*", projectId), transactionDetailLine.AccountingString);

        }
     
        #endregion

        #region CreateGeneralLedgerTransactionsAsync test methods
      
        [TestMethod]
        public async Task CreateGeneralLedgerTransactions_CreateAsync()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

             generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
                .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
             generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(generalLedgerUser);

            _personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0002839");

            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString()
            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            var genLedgrTransactionDetail2 = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Debit, new AmountAndCurrency(25, CurrencyCodes.USD));

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail, genLedgrTransactionDetail2 };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };
            
            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);


            var generalLedgerTransactionDto =
                await _generalLedgerTransactionService.CreateAsync(generalLedgerTransaction);

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected = 
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }

        [TestMethod]
        public async Task CreateGeneralLedgerTransactions_Create2Async()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions2().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
               .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(generalLedgerUser);

            _personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
            _personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("123456");
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("123456");

            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString(),
                SubmittedBy = generalLedgerTransaction.SubmittedBy.ToString()

            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";
            var genLedgrTransactionDetail2 = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Debit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail2.SubmittedBy = "123456";
            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail, genLedgrTransactionDetail2 };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };

            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);


            var generalLedgerTransactionDto =
                await _generalLedgerTransactionService.Create2Async(generalLedgerTransaction);

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);
            Assert.AreEqual(generalLedgerTransaction.SubmittedBy.Id, generalLedgerTransactionDto.SubmittedBy.Id);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected =
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.SubmittedBy.Id, actualTransactionDetailLine.SubmittedBy.Id);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }

        #endregion

        #region UpdateGeneralLedgerTransactionsAsync test methods

        [TestMethod]
        public async Task UpdateGeneralLedgerTransactions_UpdateAsync()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
               .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(generalLedgerUser);

            _personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0002839");

            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString()
            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            var genLedgrTransactionDetail2 = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Debit, new AmountAndCurrency(25, CurrencyCodes.USD));

            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail, genLedgrTransactionDetail2 };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };

            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);


            var generalLedgerTransactionDto =
                await _generalLedgerTransactionService.UpdateAsync(generalLedgerTransaction.Id, generalLedgerTransaction);

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected =
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }

        [TestMethod]
        public async Task UpdateGeneralLedgerTransactions_Update2Async()
        {

            const string accountingString = "01-02-03-04-05550-66077*A1";
            const string referenceNumber = "GL122312321";
            const string projectId = "A1";
            const string id = "0001234";

            var generalLedgerTransaction = GetTestGeneralLedgerTransactions2().FirstOrDefault(x => x.Id == id);

            // Get the necessary configuration settings and build the GL user object.
            _testGlAccountStructure = await _testGlConfigurationRepository.GetAccountStructureAsync();
            _testGlClassConfiguration = await _testGlConfigurationRepository.GetClassConfigurationAsync();
            var generalLedgerUser = await _testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync(_glUserFactoryAll.CurrentUser.PersonId, _testGlAccountStructure.FullAccessRole, _testGlClassConfiguration.ClassificationName, _testGlClassConfiguration.ExpenseClassValues);

            var generalLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            generalLedgerConfigurationRepository.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(_testGlAccountStructure);

            generalLedgerConfigurationRepository.Setup(x => x.GetClassConfigurationAsync())
               .ReturnsAsync(_testGlClassConfiguration);

            var generalLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            generalLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(generalLedgerUser);

            _personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
            _personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("123456");
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("123456");
            var glTransaction = new GeneralLedgerTransaction()
            {
                Id = generalLedgerTransaction.Id,
                ProcessMode = generalLedgerTransaction.ProcessMode.ToString(),
                SubmittedBy = generalLedgerTransaction.SubmittedBy.ToString()
            };

            var transaction = generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            var genLedgrTransaction = new GenLedgrTransaction("DN", transaction.LedgerDate)
            {
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                TransactionNumber = transaction.TransactionNumber

            };

            var transactionDetail = transaction.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);

            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";
            var genLedgrTransactionDetail2 = new GenLedgrTransactionDetail(transactionDetail.AccountingString, projectId, transactionDetail.Description,
                CreditOrDebit.Debit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail2.SubmittedBy = "123456";
            genLedgrTransaction.TransactionDetailLines = new List<GenLedgrTransactionDetail>() { genLedgrTransactionDetail, genLedgrTransactionDetail2 };

            glTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>() { genLedgrTransaction };

            _generalLedgerTransactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);
            _generalLedgerTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), _testGlAccountStructure)).ReturnsAsync(glTransaction);


            var generalLedgerTransactionDto =
                await _generalLedgerTransactionService.Update2Async(generalLedgerTransaction.Id, generalLedgerTransaction);

            Assert.IsNotNull(generalLedgerTransactionDto);
            Assert.AreEqual(Dtos.EnumProperties.ProcessMode.Update, generalLedgerTransactionDto.ProcessMode);
            Assert.AreEqual(generalLedgerTransaction.SubmittedBy.Id, generalLedgerTransactionDto.SubmittedBy.Id);

            var actual =
                generalLedgerTransactionDto.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);
            var expected =
                generalLedgerTransaction.Transactions.FirstOrDefault(x => x.ReferenceNumber == referenceNumber);

            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.LedgerDate.Value.Date, actual.LedgerDate.Value.Date);
            Assert.AreEqual(Dtos.EnumProperties.GeneralLedgerTransactionType.Donation, actual.Type);

            Assert.IsNotNull(actual.TransactionDetailLines);
            //var actualTransactionDetailLine =
            //    actual.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //var expectedTransactionDetailLine =
            //   expected.TransactionDetailLines.FirstOrDefault(x => x.AccountingString == accountingString);
            //Assert.AreEqual(expectedTransactionDetailLine.Description, actualTransactionDetailLine.Description);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Currency, actualTransactionDetailLine.Amount.Currency);
            //Assert.AreEqual(expectedTransactionDetailLine.Amount.Value, actualTransactionDetailLine.Amount.Value);
            //Assert.AreEqual(expectedTransactionDetailLine.Type, actualTransactionDetailLine.Type);
            //Assert.AreEqual(expectedTransactionDetailLine.SubmittedBy.Id, actualTransactionDetailLine.SubmittedBy.Id);
            //Assert.AreEqual(expectedTransactionDetailLine.AccountingString, actualTransactionDetailLine.AccountingString);

        }


        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildValidGeneralLedgerTransactionService()
        {
            #region Initialize mock objects
         
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            _generalLedgerTransactionRepository = new Mock<IGeneralLedgerTransactionRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            _referenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
            var generalLedgerTransactionRepository = _generalLedgerTransactionRepository.Object; 

            //   var currentUserFactory = new Mock<ICurrentUserFactory>().Object;
            
            //testGeneralLedgerTransactionRepository = new TestGeneralLedgerTransactionRepository();
            _testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();
            _testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();

            _personRepositoryMock = new Mock<IPersonRepository>();
            
            #endregion

            #region Set up the service

            // Set up the current user with all cost centers and set up the service.
            _generalLedgerTransactionService = new GeneralLedgerTransactionService
            (generalLedgerTransactionRepository, _personRepositoryMock.Object, _testGlConfigurationRepository, _testGeneralLedgerUserRepository, 
            _referenceDataRepository.Object, baseConfigurationRepositoryMock.Object,  adapterRegistry.Object, _glUserFactoryAll, roleRepository, loggerObject);
           
            #endregion
        }
        #endregion

        #region Create Data

        private List<Dtos.GeneralLedgerTransaction> GetTestGeneralLedgerTransactions()
        {
            var generalLedgerTransactions = new List<Dtos.GeneralLedgerTransaction>();

            #region record 1
            var generalLedgerDetailDtoProperty1Record1 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66077*A1",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty2Record1 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66077*A1",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new Dtos.DtoProperties.GeneralLedgerReferenceDtoProperty()
                {
                    //Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D"),
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Donation,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty1Record1, generalLedgerDetailDtoProperty2Record1 }
            };

            var generalLedgerTransaction1 = new Dtos.GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() { gltDtoProperty }
            };

            generalLedgerTransactions.Add(generalLedgerTransaction1);
            #endregion 
             
            #region record 2
            var transactions = new List<GeneralLedgerTransactionDtoProperty>(); 
            
            var generalLedgerDetailDtoProperty1 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66078*A1",
                Description = "US",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty2 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66078*A1",
                Description = "US",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };        

            var gltDtoProperty1 = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL45323220",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                   // Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "2",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Donation,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty1, generalLedgerDetailDtoProperty2 }
            };           
            transactions.Add(gltDtoProperty1);

            var generalLedgerDetailDtoProperty3 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66079*A1",
                Description = "Canadian",
                SequenceNumber = 2,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value = 100 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty4 = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "01-02-03-04-05550-66079*A1",
                Description = "Canadian",
                SequenceNumber = 2,
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value = 100 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };

            var gltDtoProperty2 = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL45323221",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    //Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "2",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Pledge,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty3, generalLedgerDetailDtoProperty4 }
            };
            transactions.Add(gltDtoProperty2);

            var generalLedgerTransaction2 = new Dtos.GeneralLedgerTransaction
            {
                Id = "0002345",
                ProcessMode = ProcessMode.Update,
                Transactions = transactions
            };

            generalLedgerTransactions.Add(generalLedgerTransaction2 );
           #endregion

            return generalLedgerTransactions;
        }

        private List<Dtos.GeneralLedgerTransaction2> GetTestGeneralLedgerTransactions2()
        {
            var generalLedgerTransactions = new List<Dtos.GeneralLedgerTransaction2>();

            #region record 1
            var generalLedgerDetailDtoProperty1Record1 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66077*A1",
                Description = "description",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2 () { Id = "123456"},
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty2Record1 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66077*A1",
                Description = "description",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    //Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Donation,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty1Record1, generalLedgerDetailDtoProperty2Record1 }
            };

            var generalLedgerTransaction1 = new Dtos.GeneralLedgerTransaction2
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Transactions = new List<GeneralLedgerTransactionDtoProperty2>() { gltDtoProperty }
            };

            generalLedgerTransactions.Add(generalLedgerTransaction1);
            #endregion

            #region record 2
            var transactions = new List<GeneralLedgerTransactionDtoProperty2>();

            var generalLedgerDetailDtoProperty1 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66078*A1",
                Description = "US",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty2 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66078*A1",
                Description = "US",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 25 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };

            var gltDtoProperty1 = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL45323220",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    //Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "2",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Donation,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty1, generalLedgerDetailDtoProperty2 }
            };
            transactions.Add(gltDtoProperty1);

            var generalLedgerDetailDtoProperty3 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66079*A1",
                Description = "Canadian",
                SequenceNumber = 2,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value = 100 },
                Type = Dtos.EnumProperties.CreditOrDebit.Credit
            };

            var generalLedgerDetailDtoProperty4 = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "01-02-03-04-05550-66079*A1",
                Description = "Canadian",
                SequenceNumber = 2,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value = 100 },
                Type = Dtos.EnumProperties.CreditOrDebit.Debit
            };

            var gltDtoProperty2 = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL45323221",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    //Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "2",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.Pledge,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty3, generalLedgerDetailDtoProperty4 }
            };
            transactions.Add(gltDtoProperty2);

            var generalLedgerTransaction2 = new Dtos.GeneralLedgerTransaction2
            {
                Id = "0002345",
                ProcessMode = ProcessMode.Update,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Transactions = transactions
            };

            generalLedgerTransactions.Add(generalLedgerTransaction2);
            #endregion

            return generalLedgerTransactions;
        }
           
        #endregion

       
    }
    [TestClass]
    public class GeneralLedgerTransactionServiceTests_V12 : GeneralLedgerCurrentUser
    {
        [TestClass]

        public class GeneralLedgerTransactionServiceTests_GET_POST_PUT
        {
            #region DECLARATION

            //protected Domain.Entities.Role viewGeneralLedgerTransaction = new Domain.Entities.Role(1, "VIEW.GL.POSTINGS");
            protected Domain.Entities.Role createGeneralLedgerTransactionPostings = new Domain.Entities.Role(2, "CREATE.GL.POSTINGS");
            protected Domain.Entities.Role createGeneralLedgerTransactionJournalEntries = new Domain.Entities.Role(3, "CREATE.JOURNAL.ENTRIES");
            protected Domain.Entities.Role createGeneralLedgerTransactionBudgetEntries = new Domain.Entities.Role(4, "CREATE.BUDGET.ENTRIES");
            protected Domain.Entities.Role createGeneralLedgerTransactionEncumbranceEntries = new Domain.Entities.Role(5, "CREATE.ENCUMBRANCE.ENTRIES");


            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
            private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
            private Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IGeneralLedgerTransactionRepository> generalLedgerTransactionRepositoryMock;
            

            private GeneralLedgerTransactionsUser currentUserFactory;

            private GeneralLedgerTransactionService generalLedgerTransactionService;

            private string guid = "0ca1a878-3555-4a3f-a17b-20d054d5e001";

            private GeneralLedgerAccountStructure generalLedgerAccountStructure;

            private GeneralLedgerClassConfiguration generalLedgerClassConfiguration;

            private GeneralLedgerUser generalLedgerUser;

            private IEnumerable<GeneralLedgerTransaction> generalLedgerTransactions;

            private IEnumerable<FiscalPeriodsIntg> fiscalPeriodsIntg;

            private IEnumerable<FiscalYear> fiscalYears;

            private GeneralLedgerTransaction3 generalLedgerTransactionDto;
            private GeneralLedgerTransaction3 generalLedgerTransactionDtoForPerms;

            IDictionary<string, string> _projectReferenceIds;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                personRepositoryMock = new Mock<IPersonRepository>();
                generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
                generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                generalLedgerTransactionRepositoryMock = new Mock<IGeneralLedgerTransactionRepository>();
                loggerMock = new Mock<ILogger>();
                currentUserFactory = new GeneralLedgerTransactionsUser();                

                InitializeTestData();

                InitializeTestMock();

                generalLedgerTransactionService = new GeneralLedgerTransactionService(generalLedgerTransactionRepositoryMock.Object, personRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object, generalLedgerUserRepositoryMock.Object, referenceDataRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

            }

            private void InitializeTestData() {
                generalLedgerAccountStructure = new GeneralLedgerAccountStructure() { AccountOverrideTokens = new List<string>() { "T1", "T2", "T3" }, CheckAvailableFunds = "Check", FullAccessRole = "ALL-ACCOUNTS", glDelimiter = "-"};
                generalLedgerClassConfiguration = new GeneralLedgerClassConfiguration("GL.CLASS", new List<string>() { "5" }, new List<string>() { "reven_1", "reven_2" }, new List<string>() { "asset_1", "asset_2" }, new List<string>() { "liability_1", "liability_2" }, new List<string>() { "fund_1", "fund_2" });
                generalLedgerUser = new GeneralLedgerUser("0004319", "vaidya") { };
                generalLedgerTransactions = new List<GeneralLedgerTransaction>() {
                    new GeneralLedgerTransaction() {Id = "",  ProcessMode = "PM", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("source_1", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_007", "desc", CreditOrDebit.Credit, amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber="0001",EncAdjustmentType= "PARTIAL", EncCommitmentType="COMMITTED", EncGiftUnits="10", EncLineItemNumber="001", SequenceNumber=10 }, new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", CreditOrDebit.Credit, amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "PARTIAL", EncCommitmentType = "COMMITTED", EncGiftUnits = "10", EncLineItemNumber = "001", SequenceNumber = 10 } }   } } } ,
                    new GeneralLedgerTransaction() { Id = "", Comment = "", ProcessMode = "UPDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("DN", DateTime.Now) { BudgetPeriodDate = new DateTime(2007,10,20,10,50,20), ReferenceNumber = "ref_num1", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", CreditOrDebit.Debit, amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "ADJUSTMENT", EncCommitmentType = "UNCOMMITTED", EncGiftUnits = "" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "UPDATEIMMEDIATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("DNE", DateTime.Now) { BudgetPeriodDate = new DateTime(2016, 10, 20, 10, 50, 20), ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "TOTAL", EncCommitmentType="NONCOMMITTED", EncGiftUnits = "20.90", SubmittedBy= "0004319" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "UPDATEBATCH", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("PL", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "HALF", EncCommitmentType = "NONCOMMITTED" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("PLE", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("AOB", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("AB", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("CB", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("EOB", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("GEC", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("MGLT", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("TB", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("TBA", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("ABA", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("source_1", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) } } } } };

                fiscalPeriodsIntg = new List<FiscalPeriodsIntg>() { new FiscalPeriodsIntg("0ca1a878-3555-4a3f-a17b-20d054d5e469", "0001") { FiscalYear = DateTime.Now.Year, Month = DateTime.Now.Month, Year = DateTime.Now.Year } };
                fiscalYears = new List<FiscalYear>() { new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e000", "2019") { CurrentFiscalYear = 2018, FiscalStartMonth = 7 },
                                                      new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e001", "2018") { CurrentFiscalYear = 2018, FiscalStartMonth = 7 },
                                                      new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e002", "2019") { CurrentFiscalYear = 2018, FiscalStartMonth = 1 }
                };

                generalLedgerTransactionDto = new GeneralLedgerTransaction3() { Comment="comment_1", Id= "0ca1a878-3555-4a3f-a17b-20d054d5e469", ProcessMode= ProcessMode2.UpdateImmediate,SubmittedBy= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Transactions = new List<GeneralLedgerTransactionDtoProperty3>() { new GeneralLedgerTransactionDtoProperty3()
                { LedgerDate = new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, ReferenceNumber="0001", TransactionNumber="0002", Type= GeneralLedgerTransactionType.Donation, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123_567*Proj_1", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123_568_THYUI", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },

                  new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ActualOpenBalance, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ApprovedBudget, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.EncumbranceOpenBalance, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ContingentBudget, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.GeneralEncumbranceCreate, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.MiscGeneralLedgerTransaction, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.TemporaryBudget, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.TemporaryBudgetAdjustment, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ApprovedBudgetAdjustment, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.Donation, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.DonationEndowed, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.Pledge, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } },
                    new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.PledgeEndowed, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } }
                } };

                _projectReferenceIds = new Dictionary<string, string>();
                _projectReferenceIds.Add("Proj1_001", "Proj1_002");
                createGeneralLedgerTransactionPostings.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.CreateGLPostings));
                createGeneralLedgerTransactionBudgetEntries.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.CreateBudgetEntries));
                createGeneralLedgerTransactionEncumbranceEntries.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.CreateEncumbranceEntries));
                createGeneralLedgerTransactionJournalEntries.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.CreateJournalEntries));

                generalLedgerTransactionDtoForPerms = new GeneralLedgerTransaction3()
                {
                    Comment = "comment_1",
                    Id = "0ca1a878-3555-4a3f-a17b-20d054d5e469",
                    ProcessMode = ProcessMode2.UpdateImmediate,
                    SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                    Transactions = new List<GeneralLedgerTransactionDtoProperty3>()
                    {
                        new GeneralLedgerTransactionDtoProperty3()
                        { Reference = new GeneralLedgerReferenceDtoProperty(){ Organization = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460") },  LedgerDate = new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, ReferenceNumber="0001", TransactionNumber="0002",
                                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                            TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>()
                            {
                                new GeneralLedgerDetailDtoProperty3()
                                {
                                    AccountingString ="70001_123_567*", Amount= new AmountDtoProperty()
                                    {
                                        Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10
                                    },
                                    BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                                    Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance()
                                    {
                                        AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12
                                    }
                                },
                                new GeneralLedgerDetailDtoProperty3()
                                {
                                    AccountingString ="70001_123_568_THYUI", Amount= new AmountDtoProperty()
                                    {
                                        Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5
                                    }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                                    Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance()
                                    {
                                        AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12
                                    }
                                },
                                new GeneralLedgerDetailDtoProperty3()
                                {
                                    AccountingString ="70001_123", Amount= new AmountDtoProperty()
                                    {
                                        Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5
                                    }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                                    Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance()
                                    {
                                        AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12
                                    }
                                }
                            }
                        }
                    }
                };
            }

            private void InitializeTestMock()
            {
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries,
                    createGeneralLedgerTransactionJournalEntries
                });
                
                generalLedgerConfigurationRepositoryMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(generalLedgerAccountStructure);
                generalLedgerConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(generalLedgerClassConfiguration);
                generalLedgerUserRepositoryMock.Setup(x => x.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(generalLedgerUser);
                generalLedgerTransactionRepositoryMock.Setup(x => x.Get2Async(It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(generalLedgerTransactions);
                personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<bool>());
                referenceDataRepositoryMock.Setup(x => x.GetFiscalPeriodsIntgAsync(It.IsAny<bool>())).ReturnsAsync(fiscalPeriodsIntg);
                referenceDataRepositoryMock.Setup(x => x.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);
                generalLedgerTransactionRepositoryMock.Setup(x => x.GetById2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(generalLedgerTransactions.FirstOrDefault());
                generalLedgerTransactionRepositoryMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<GeneralLedgerAccountStructure>())).ReturnsAsync(generalLedgerTransactions.FirstOrDefault());
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("123456");
                generalLedgerTransactionRepositoryMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<GeneralLedgerAccountStructure>())).ReturnsAsync(generalLedgerTransactions.FirstOrDefault());

                generalLedgerTransactionRepositoryMock.Setup(repo => repo.GetProjectReferenceIds(It.IsAny<string[]>())).ReturnsAsync(_projectReferenceIds);
            }
            

           [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                referenceDataRepositoryMock = null;
                personRepositoryMock = null;
                generalLedgerTransactionRepositoryMock = null;
                generalLedgerConfigurationRepositoryMock = null;
                generalLedgerUserRepositoryMock = null;
                configurationRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                generalLedgerTransactionService = null;
            }

            #endregion

            #region GETALL

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_Get3Async_Null_Entity() {
                generalLedgerTransactionRepositoryMock.Setup(x => x.Get2Async(It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(null);
                await generalLedgerTransactionService.Get3Async(false);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionService_Get3Async()
            {
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);
                var result = await generalLedgerTransactionService.Get3Async(false);
                Assert.IsNotNull(result);
                
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionService_Get3Async_Corp_False()
            {
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
                var result = await generalLedgerTransactionService.Get3Async(false);
                Assert.IsNotNull(result);

            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_GetById3Async_Null_Entity()
            {
                generalLedgerTransactionRepositoryMock.Setup(x => x.GetById2Async(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<GlAccessLevel>())).ReturnsAsync(null);
                await generalLedgerTransactionService.GetById3Async(guid);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionService_GetById3Async()
            {
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);
                var result = await generalLedgerTransactionService.GetById3Async(guid);
                Assert.IsNotNull(result);

            }

            [TestMethod]
            public async Task GeneralLedgerTransactionService_GetById3Async_Corp_False()
            {
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
                var result = await generalLedgerTransactionService.GetById3Async(guid);
                Assert.IsNotNull(result);

            }
            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException()
            {
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException_NoCrateTransactionPostings()
            {
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    //createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries,
                    createGeneralLedgerTransactionJournalEntries
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException_ActualOpenBalance()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.ActualOpenBalance;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    //createGeneralLedgerTransactionJournalEntries
                    createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDtoForPerms);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException_MiscGeneralLedgerTransaction()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.MiscGeneralLedgerTransaction;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    //createGeneralLedgerTransactionJournalEntries
                    createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDtoForPerms);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_CrateBudgetEntries_ApprovedBudget()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.ApprovedBudget;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionJournalEntries,
                    //createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries,
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_CrateBudgetEntries_ContingentBudget()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.ContingentBudget;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionJournalEntries,
                    //createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries,
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_CrateBudgetEntries_ApprovedBudgetAdjustment()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.ApprovedBudgetAdjustment;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionJournalEntries,
                    //createGeneralLedgerTransactionBudgetEntries,
                    createGeneralLedgerTransactionEncumbranceEntries,
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException_EncumbranceOpenBalance()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.EncumbranceOpenBalance;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionBudgetEntries,
                    //createGeneralLedgerTransactionEncumbranceEntries,
                    createGeneralLedgerTransactionJournalEntries
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GeneralLedgerTransactionService_Create3Async_PermissionsException_GeneralEncumbranceCreate()
            {
                generalLedgerTransactionDtoForPerms.Transactions.First().Type = GeneralLedgerTransactionType.GeneralEncumbranceCreate;
                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    createGeneralLedgerTransactionPostings,
                    createGeneralLedgerTransactionBudgetEntries,
                    //createGeneralLedgerTransactionEncumbranceEntries,
                    createGeneralLedgerTransactionJournalEntries
                });
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [Ignore]
            public async Task GeneralLedgerTransactionService_Create3Async_()
            {
                var result = await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_ArgumentNull_Exception()
            {
                await generalLedgerTransactionService.Create3Async(null);
             
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Transaction_Null()
            {
                generalLedgerTransactionDto.Transactions = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Different_ProcessMode()
            {
                generalLedgerTransactionDto.ProcessMode = ProcessMode2.NotSet;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_UnSupported_TransactionType()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Type = GeneralLedgerTransactionType.GrantPayment;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_LedgerDate_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().LedgerDate = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Organization_NotNull()
            {

                generalLedgerTransactionDto = new GeneralLedgerTransaction3()
                {
                    Comment = "comment_1",
                    Id = "0ca1a878-3555-4a3f-a17b-20d054d5e469",
                    ProcessMode = ProcessMode2.UpdateImmediate,
                    SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                    Transactions = new List<GeneralLedgerTransactionDtoProperty3>() {
                        new GeneralLedgerTransactionDtoProperty3()
                    { Reference = new GeneralLedgerReferenceDtoProperty(){ Organization = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460") },  LedgerDate = new DateTimeOffset(2017,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, ReferenceNumber="0001", TransactionNumber="0002", Type= GeneralLedgerTransactionType.Donation,
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123_567*", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123_568_THYUI", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                }
                        }
                    }
                };
                personRepositoryMock.SetupSequence(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("123456")).Returns(Task.FromResult<string>(null));
                personRepositoryMock.Setup(x => x.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);

                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_TransactionDetailLines_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_EncumbranceHasValue_WhenType_AE()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Type = GeneralLedgerTransactionType.EncumbranceOpenBalance;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Encumbrance.Number = "10";
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_AccountingString_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().AccountingString = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Description_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Description = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Amount_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Amount = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Amount_Zero()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=0 };
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_Currency_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Amount = new AmountDtoProperty() { };
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_TransactionDetail_Type_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Type = null;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_ValidateGeneralLedgerDto3_CreditAndDebit_NotEqual()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().Amount = new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=100 };
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionService_GetFiscalYear_FiscalStartMonth_NoValue()
            {
                fiscalYears = new List<FiscalYear>() { new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e000", (DateTime.Now.Year + 1).ToString()) { CurrentFiscalYear = DateTime.Now.Year } };
                referenceDataRepositoryMock.Setup(x => x.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionService_GetFiscalYear_CurrentFiscalYear_NoValue()
            {
                fiscalYears = new List<FiscalYear>() { new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e000", (DateTime.Now.Year + 1).ToString()) { FiscalStartMonth = 1 } };
                referenceDataRepositoryMock.Setup(x => x.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionService_Create3Async_LedgerDate_NotEqual()
            {

                generalLedgerTransactionDto = new GeneralLedgerTransaction3()
                {
                    Comment = "comment_1",
                    Id = "0ca1a878-3555-4a3f-a17b-20d054d5e469",
                    ProcessMode = ProcessMode2.UpdateImmediate,
                    SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                    Transactions = new List<GeneralLedgerTransactionDtoProperty3>() {
                  new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(DateTime.Today), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ActualOpenBalance, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } }
                }
                };
                fiscalYears = new List<FiscalYear>() { new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e000", (DateTime.Now.Year + 1).ToString()) { CurrentFiscalYear = DateTime.Now.Year, FiscalStartMonth = 1 } };
                referenceDataRepositoryMock.Setup(x => x.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionService_Create3Async_LedgerMonth_LessThan_FiscalMonth()
            {
                generalLedgerTransactionDto = new GeneralLedgerTransaction3()
                {
                    Comment = "comment_1",
                    Id = "0ca1a878-3555-4a3f-a17b-20d054d5e469",
                    ProcessMode = ProcessMode2.Validate,
                    SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"),
                    Transactions = new List<GeneralLedgerTransactionDtoProperty3>() {
                  new GeneralLedgerTransactionDtoProperty3() { LedgerDate =new DateTimeOffset(DateTime.Now.Year,7,1,10,10,10,10,TimeSpan.Zero), TransactionTypeReferenceDate= DateTimeOffset.Now, TransactionNumber="0002", Type= GeneralLedgerTransactionType.ActualOpenBalance, Reference = new GeneralLedgerReferenceDtoProperty() { Person= new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e465") },
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() {
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_123*5435354*", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Debit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Adjustment, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_124*hgfgf*7655", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Partial, CommitmentType = CommitmentType.Uncommitted, LineItemNumber=1, Number="120", SequenceNumber=12 } },
                    new GeneralLedgerDetailDtoProperty3() { AccountingString="70001_12*jgjhg", Amount= new AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=5 }, BudgetPeriod = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e569"), Description="desc", GiftUnits=10, SequenceNumber=2, SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e460"), Type = Dtos.EnumProperties.CreditOrDebit.Credit, Encumbrance = new Encumbrance() { AdjustmentType = AdjustmentType.Total, CommitmentType = CommitmentType.Committed, LineItemNumber=1, Number="120", SequenceNumber=12 } }
                } }
                    }
                };
                fiscalYears = new List<FiscalYear>() { new FiscalYear("0ca1a878-3555-4a3f-a17b-20d054d5e000", (DateTime.Now.Year).ToString()) { CurrentFiscalYear = DateTime.Now.Year, FiscalStartMonth = 8 } };
                referenceDataRepositoryMock.Setup(x => x.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_ConvertLedgerTransactionDtoToEntity3Async_SubmittedBy_Null()
            {
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_ConvertLedgerTransactionDtoToEntity3Async_PersonId_Null()
            {
                generalLedgerTransactionDto.SubmittedBy = null;
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_ConvertLedgerTransactionDtoToEntity3Async_Organization_Null()
            {
                generalLedgerTransactionDto.SubmittedBy = null;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Reference.Person = null;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Reference.Organization = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e469");
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionService_ConvertLedgerTransactionDtoToEntity3Async_Reference_Null()
            {
                generalLedgerTransactionDto.Transactions.FirstOrDefault().ReferenceNumber = "00001";
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Type = GeneralLedgerTransactionType.ActualOpenBalance;
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GeneralLedgerTransactionService_Create3Async_TransactionDetail_SubmittedBy_Null()
            {
                generalLedgerTransactionDto.SubmittedBy = null;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Reference.Person = null;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().Reference.Organization = null;
                generalLedgerTransactionDto.Transactions.FirstOrDefault().TransactionDetailLines.FirstOrDefault().SubmittedBy = new GuidObject2("0ca1a878-3555-4a3f-a17b-20d054d5e469");
                personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await generalLedgerTransactionService.Create3Async(generalLedgerTransactionDto);
            }

            #endregion

            #region PUT
            [TestMethod]
            [Ignore]
            public async Task GeneralLedgerTransactionService_Update3Async()
            {
                var result = await generalLedgerTransactionService.Update3Async(generalLedgerTransactionDto.Id, generalLedgerTransactionDto);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GeneralLedgerTransactionService_Update3Async_Different_Id()
            {
                var result = await generalLedgerTransactionService.Update3Async(guid, generalLedgerTransactionDto);
            }

            #endregion
        }
    }
}