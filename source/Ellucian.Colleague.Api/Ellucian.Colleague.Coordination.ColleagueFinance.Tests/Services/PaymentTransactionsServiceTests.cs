﻿using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class PaymentTransactionsServiceTests
    {
        [TestClass]
        public class PaymentTransactionsServiceTests_V12
        {
            [TestClass]
            public class PaymentTransactionsServiceTests_GET_GETALL : GeneralLedgerCurrentUser
            {
                #region DECLARATION
                protected Domain.Entities.Role viewPaymentTransactionsRequest = new Domain.Entities.Role(1, "VIEW.PAYMENT.TRANSACTIONS");

                private Mock<IPaymentTransactionsRepository> paymentTransactionsRepositoryMock;
                private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
                //private Mock<IAccountsPayableInvoicesRepository> accountsPayableInvoicesRepositoryMock;
                private Mock<IInstitutionRepository> institutionRepositoryMock;
                private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
                private Mock<IAddressRepository> addressRepositoryMock;
                private Mock<IPersonRepository> personRepositoryMock;
                private Mock<IAdapterRegistry> adapterRegistryMock;
                private Mock<IRoleRepository> roleRepositoryMock;
                private Mock<ILogger> loggerMock;
                private Mock<IConfigurationRepository> configurationRepositoryMock;

                private PaymentTransactionsUser currentUserFactory;

                private PaymentTransactionsService paymentTransactionsService;
                private IEnumerable<PaymentTransaction> paymentTransactions;
                private IEnumerable<PaymentTransactionVoucher> Vouchers;
                private Tuple<IEnumerable<PaymentTransaction>, int> paymentTransactionsTuple;
                private IEnumerable<Domain.Base.Entities.Country> countries;
                private IEnumerable<Domain.Base.Entities.State> states;
                private IEnumerable<Domain.Base.Entities.Institution> institutions;

                private string guid = "0ca1a878-3555-4a3f-a17b-20d054d5e101";

                #endregion

                #region TEST SETUP
                [TestInitialize]
                public void Initialize()
                {
                    paymentTransactionsRepositoryMock = new Mock<IPaymentTransactionsRepository>();
                    referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();                    
                    institutionRepositoryMock = new Mock<IInstitutionRepository>();
                    colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                    addressRepositoryMock = new Mock<IAddressRepository>();
                    personRepositoryMock = new Mock<IPersonRepository>();
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    configurationRepositoryMock = new Mock<IConfigurationRepository>();
                    loggerMock = new Mock<ILogger>();

                    currentUserFactory = new PaymentTransactionsUser();

                    InitializeTestData();

                    InitializeTestMock();

                    paymentTransactionsService = new PaymentTransactionsService(paymentTransactionsRepositoryMock.Object, personRepositoryMock.Object,referenceDataRepositoryMock.Object,
                         institutionRepositoryMock.Object,colleagueFinanceReferenceDataRepositoryMock.Object,addressRepositoryMock.Object,adapterRegistryMock.Object,currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

                }
                private void InitializeTestData()
                {
                    paymentTransactions = new List<PaymentTransaction>() {
                        new PaymentTransaction("1","0ca1a878-3555-4a3f-a17b-20d054d5e101", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="USD", HostCountry="USA", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Debitcard, ReferenceNumber="ref_001", State="NY", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Outstanding, Zip="576231" },
              
                        new PaymentTransaction("2","0ca1a878-3555-4a3f-a17b-20d054d5e102", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="USD", HostCountry="USA", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Creditcard, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Paid, Zip="576231"},
                        new PaymentTransaction("3","0ca1a878-3555-4a3f-a17b-20d054d5e103", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="USA", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Echeck, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Reconciled, Zip="576231" },
                        new PaymentTransaction("4","0ca1a878-3555-4a3f-a17b-20d054d5e104", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1",  CurrencyCode="CAN", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Directdeposit, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Voided, Zip="576231" },
                        new PaymentTransaction("5","0ca1a878-3555-4a3f-a17b-20d054d5e105", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="AUS", HostCountry="CAN", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Wire, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.InProgress, Zip="576231" },
                        new PaymentTransaction("5","0ca1a878-3555-4a3f-a17b-20d054d5e105", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="UNITED STATES", CurrencyCode="AUS", HostCountry="USA", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Wire, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.InProgress, Zip="576231" },
                        new PaymentTransaction("6","0ca1a878-3555-4a3f-a17b-20d054d5e106", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=false, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Wire, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.InProgress, Zip="576231" },
                        new PaymentTransaction("7","0ca1a878-3555-4a3f-a17b-20d054d5e107", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Check, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="576231" },
                        new PaymentTransaction("8","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("9","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="IL", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("10","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="IL", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("11","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", CurrencyCode="INT", HostCountry="CAN", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="ON", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("12","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="AUS", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("13","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="BRA", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("14","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="MEX", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("15","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="NLD", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("16","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="GBR", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("17","0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="IND", CurrencyCode="USD", HostCountry="USA", IsOrganization=true, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber="ref_001", State="KA", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip="" },
                        new PaymentTransaction("1","0ca1a878-3555-4a3f-a17b-20d054d5e101", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="USD", HostCountry="USA", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Check, ReferenceNumber="ref_001", State="NY", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Outstanding, Zip="576231"},
                        new PaymentTransaction("2","0ca1a878-3555-4a3f-a17b-20d054d5e102", DateTime.Now ){ Address = new List<string>(){"Address_1", "Address_2" }, Check=true, City="City_Name_1", Comments="Comment_1", Country="USA", CurrencyCode="USD", HostCountry="USA", IsOrganization=false, MiscName = new List<string>(){"Name_1","Name_2" }, PaymentAmount=10, PaymentMethod = PaymentMethod.Check, ReferenceNumber="ref_001", State="OH", Status = new List<string>() { "Status_1", "Status_2"}, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor="Vendor_1",   VoucherStatus = VoucherStatus.Paid, Zip="576231" },
                    };
                    paymentTransactions.FirstOrDefault().AddVoucher(new PaymentTransactionVoucher("1ca1a878-3555-4a3f-a17b-20d054d5e100", "1")
                    {                                                
                        RecurringVoucherId = "1",
                        VoucherInvoiceAmt = new AmountAndCurrency(5m, CurrencyCodes.USD)
                    });
                    paymentTransactions.FirstOrDefault().Vouchers.FirstOrDefault().AddAccountsPayableInvoicesLineItem(new AccountsPayableInvoicesLineItem("1", "Descr 1", 2m, 12.99m, 15.99m));

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);

                    countries = new List<Domain.Base.Entities.Country>() { new Domain.Base.Entities.Country("USA", "UNITED STATES", "US", "USA") { IsoAlpha3Code = "USA" }, new Domain.Base.Entities.Country("AUS", "AUSTRALIA", "AU", "AUS") { IsoAlpha3Code = "AUS" }, new Domain.Base.Entities.Country("CAN", "CANADA", "CA", "CAN") { IsoAlpha3Code = "CAN" }, new Domain.Base.Entities.Country("BRA", "BRAZIL", "BR", "BRA") { IsoAlpha3Code = "BRA" }, new Domain.Base.Entities.Country("MEX", "MEXICO", "MX", "MEX") { IsoAlpha3Code = "MEX" }, new Domain.Base.Entities.Country("NLD", "NETHERLANDS", "NL", "NLD") { IsoAlpha3Code = "NLD" }, new Domain.Base.Entities.Country("GBR", "UNITED KINGDOM", "UK", "GBR") { IsoAlpha3Code = "GBR" }, new Domain.Base.Entities.Country("IND", "INDIA", "IN", "IND") { IsoAlpha3Code = "IND" }, new Domain.Base.Entities.Country("NPS", "NEPAL", "", "NPS") { IsoAlpha3Code = "NPS" } };
                    states = new List<Domain.Base.Entities.State>() { new Domain.Base.Entities.State("NY", "New York", "USA"), new Domain.Base.Entities.State("ON", "Ontario"), new Domain.Base.Entities.State("OH", "Ohio"), new Domain.Base.Entities.State("NJ", "New Jersey"), new Domain.Base.Entities.State("IL", "Illinois","USA") };
                    institutions = new List<Domain.Base.Entities.Institution>() { new Domain.Base.Entities.Institution("1", Domain.Base.Entities.InstType.College), new Domain.Base.Entities.Institution("2", Domain.Base.Entities.InstType.HighSchool), new Domain.Base.Entities.Institution("3", Domain.Base.Entities.InstType.Unknown), new Domain.Base.Entities.Institution("4", Domain.Base.Entities.InstType.HighSchool), new Domain.Base.Entities.Institution("6", Domain.Base.Entities.InstType.HighSchool), new Domain.Base.Entities.Institution("7", Domain.Base.Entities.InstType.HighSchool) };
                    var personGuidCollection = new Dictionary<string, string>();
                    personGuidCollection.Add("Vendor_1", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                    personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                }
                private void InitializeTestMock()
                {
                    viewPaymentTransactionsRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewPaymentTransactionsIntg));
                    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPaymentTransactionsRequest });

                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);
                    //accountsPayableInvoicesRepositoryMock.Setup(x => x.GetAccountsPayableInvoicesGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("5551a878-3555-4a3f-a17b-20d054d5e101");
                   // accountsPayableInvoicesRepositoryMock.Setup(x => x.GetAccountsPayableInvoicesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetIdFromGuidAsync(It.IsAny<string>(), "VOUCHERS")).ReturnsAsync("1");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetIdFromGuidAsync(It.IsAny<string>(), "PURCHASE.ORDERS")).ReturnsAsync("1");
                    
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("1", "3ca1a878-3555-4a3f-a17b-20d054d5e101");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetGuidsCollectionAsync(It.IsAny<string[]>(), "PURCHASE.ORDERS")).ReturnsAsync(dict);

                    Dictionary<string, string> dict1 = new Dictionary<string, string>();
                    dict1.Add("1", "4ca1a878-3555-4a3f-a17b-20d054d5e101");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetGuidsCollectionAsync(It.IsAny<string[]>(), "BPO")).ReturnsAsync(dict1);

                    paymentTransactionsRepositoryMock.Setup(x => x.GetIdFromGuidAsync(It.IsAny<string>(), "BPO")).ReturnsAsync("1");
                    personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("6661a878-3555-4a3f-a17b-20d054d5e101");
                    referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);
                    referenceDataRepositoryMock.Setup(x => x.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(states);
                    institutionRepositoryMock.Setup(x => x.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(institutions);
                    addressRepositoryMock.Setup(x => x.GetHostCountryAsync()).ReturnsAsync("USA");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsByGuidAsync(It.IsAny<string>())).ReturnsAsync(paymentTransactions.FirstOrDefault());

                }
                
                [TestCleanup]
                public void Cleanup()
                {
                    paymentTransactionsRepositoryMock = null;
                    referenceDataRepositoryMock = null;
                    //accountsPayableInvoicesRepositoryMock = null;
                    institutionRepositoryMock = null;
                    colleagueFinanceReferenceDataRepositoryMock = null;
                    addressRepositoryMock = null;
                    personRepositoryMock = null;
                    adapterRegistryMock = null;
                    referenceDataRepositoryMock = null;
                    personRepositoryMock = null;
                    configurationRepositoryMock = null;
                    roleRepositoryMock = null;
                    loggerMock = null;
                    currentUserFactory = null;
                    paymentTransactionsService = null;
                }
                #endregion

                #region GETALL

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_PermissionsException()
                {
                    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                    await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100,"", Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(),false);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync()
                {

                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(), false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_With_Criteria()
                {
                    Dtos.PaymentTransactions criteria = new Dtos.PaymentTransactions()
                    {
                        PaymentsFor = new List<PaymentsForDtoProperty>()
                        {                            
                        }                     
                    };
                    InvoiceDtoProperty invDtoProperty = new InvoiceDtoProperty()
                    {
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            PurchaseOrder = new GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e108")
                        }                       
                    };
                    PaymentsForDtoProperty paymentsFor = new PaymentsForDtoProperty() { Document = invDtoProperty };
                    paymentsFor.Document.ReferenceDocument.BlanketPurchaseOrder = new GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e108");
                    paymentsFor.Document.ReferenceDocument.RecurringVoucher = "RecVou";
                    criteria.PaymentsFor.Add(paymentsFor);

                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, criteria, false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_PurchasingArrangement_EmptyId()
                {
                    Dtos.PaymentTransactions criteria = new Dtos.PaymentTransactions()
                    {
                        PaymentsFor = new List<PaymentsForDtoProperty>()
                        {
                        }
                    };
                    InvoiceDtoProperty invDtoProperty = new InvoiceDtoProperty()
                    {
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            
                        }
                    };
                    PaymentsForDtoProperty paymentsFor = new PaymentsForDtoProperty() { Document = invDtoProperty };
                    paymentTransactionsRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
                    paymentsFor.Document.ReferenceDocument.BlanketPurchaseOrder = new GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e108");
                    criteria.PaymentsFor.Add(paymentsFor);

                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, criteria, false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_BlanketPurchaseOrder_EmptyId()
                {
                    Dtos.PaymentTransactions criteria = new Dtos.PaymentTransactions()
                    {
                        PaymentsFor = new List<PaymentsForDtoProperty>()
                        {
                        }
                    };
                    InvoiceDtoProperty invDtoProperty = new InvoiceDtoProperty()
                    {
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            PurchaseOrder = new GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e108")
                        }
                    };
                    PaymentsForDtoProperty paymentsFor = new PaymentsForDtoProperty() { Document = invDtoProperty };
                    paymentTransactionsRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
                    paymentsFor.Document.ReferenceDocument.BlanketPurchaseOrder = new GuidObject2();
                    //paymentsFor.Document.ReferenceDocument.RecurringVoucher = "RecVou";
                    //paymentsFor.Document.ReferenceDocument.PurchasingArrangement = new GuidObject2("6ca1a878-3555-4a3f-a17b-20d054d5e108");
                    criteria.PaymentsFor.Add(paymentsFor);

                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, criteria, false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_With_Criteria_PurchasingArrangement()
                {
                    Dtos.PaymentTransactions criteria = new Dtos.PaymentTransactions()
                    {
                        PaymentsFor = new List<PaymentsForDtoProperty>()
                        {
                        }
                    };
                    InvoiceDtoProperty invDtoProperty = new InvoiceDtoProperty()
                    {
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            PurchaseOrder = new GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e108")
                        }
                    };
                    PaymentsForDtoProperty paymentsFor = new PaymentsForDtoProperty() { Document = invDtoProperty };
                    paymentsFor.Document.ReferenceDocument.BlanketPurchaseOrder = new GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e108");
                    paymentsFor.Document.ReferenceDocument.RecurringVoucher = "RecVou";
                    paymentsFor.Document.ReferenceDocument.PurchasingArrangement = new GuidObject2("6ca1a878-3555-4a3f-a17b-20d054d5e108");
                    criteria.PaymentsFor.Add(paymentsFor);

                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);

                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, criteria, false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_DocumentId_As_Null()
                {
                    //accountsPayableInvoicesRepositoryMock.Setup(x => x.GetAccountsPayableInvoicesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetIdFromGuidAsync(It.IsAny<string>(), "VOUCHERS")).ReturnsAsync(() => null);
                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(), false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_DocumentType_As_NotSet()
                {
                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.NotSet, It.IsAny<Dtos.PaymentTransactions>(), false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_Person_As_Null()
                {
                    personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                    await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, string.Empty, Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(), false);
                    
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_HostCountry_CANADA()
                {
                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" },
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.Check, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };
                    
                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);

                    addressRepositoryMock.Setup(x => x.GetHostCountryAsync()).ReturnsAsync("CAN");
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);
                    var result = await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(), false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_Country_KeyNotFound()
                {
                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);

                    countries = new List<Domain.Base.Entities.Country>() { new Domain.Base.Entities.Country("IND", "INDIA", "IN", "IN") { IsoAlpha3Code = "IND" } };
                    referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);
                    
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);
                    await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Refund, It.IsAny<Dtos.PaymentTransactions>(), false);
                    
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_Country_NotFound()
                {
                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", Country="NEPAL", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };

                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);

                    countries = new List<Domain.Base.Entities.Country>() { new Domain.Base.Entities.Country("IND", "INDIA", "IN", "IN") { IsoAlpha3Code = "IND" } };
                    referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);
                    await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Refund, It.IsAny<Dtos.PaymentTransactions>(), false);

                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsAsync_ISOCode_NotFound()
                {
                    paymentTransactions = new List<PaymentTransaction>() {
                    new PaymentTransaction("8", "0ca1a878-3555-4a3f-a17b-20d054d5e108", DateTime.Now) { Address = new List<string>() { "Address_1", "Address_2" }, Check = true, City = "City_Name_1", Comments = "Comment_1", Country="NPS", CurrencyCode = "INT", HostCountry = "CAN", IsOrganization = true, MiscName = new List<string>() { "Name_1", "Name_2" }, PaymentAmount = 10, PaymentMethod = PaymentMethod.NotSet, ReferenceNumber = "ref_001", State = "ON", Status = new List<string>() { "Status_1", "Status_2" }, StatusDate = new List<DateTime?>() { DateTime.Now, DateTime.Now }, Vendor = "Vendor_1",   VoucherStatus = VoucherStatus.Cancelled, Zip = "" }
                    };
                    paymentTransactionsTuple = new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, 2);
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<InvoiceOrRefund>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(paymentTransactionsTuple);
                    await paymentTransactionsService.GetPaymentTransactionsAsync(0, 100, "0ca1a878-3555-4a3f-a17b-20d054d5e101", Dtos.EnumProperties.InvoiceTypes.Invoice, It.IsAny<Dtos.PaymentTransactions>(), false);

                }


                #endregion

                #region GETBYID             

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsByGuidAsync_GuidNull()
                {
                    await paymentTransactionsService.GetPaymentTransactionsByGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsByGuidAsync_KeyNotFound()
                {
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                    await paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);
                }
                
                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsByGuidAsync_Repository()
                {
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                    await paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);
                }               

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task PaymentTransactionsService_GetPaymentTransactionsByGuidAsync_Exception()
                {
                    paymentTransactionsRepositoryMock.Setup(x => x.GetPaymentTransactionsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                    await paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                public async Task PaymentTransactionsService_GetPaymentTransactionsByGuidAsync()
                {
                    var result = await paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, paymentTransactions.FirstOrDefault().Guid);
                }

                #endregion
            }

        }

    }
}
