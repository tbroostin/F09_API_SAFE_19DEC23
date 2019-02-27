// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class HumanResourcesTaxFormPdfDataRepositoryTests
    {
        #region Initialize and Cleanup
        private Mock<ICacheProvider> mockCacheProvider = new Mock<ICacheProvider>();
        private Mock<IColleagueDataReader> dataReaderMock = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private TxGetHierarchyNameResponse hierarchyNameResponse;
        private TxGetHierarchyAddressResponse hierarchyAddressResponse;
        private HumanResourcesTaxFormPdfDataRepository pdfDataRepository;
        private static string webW2OnlineId;
        private static string webW2cOnlineId;
        private static string personId;
        private string[] pdfIds = new string[] { "8675309" };

        private WebW2Online webW2OnlineDataContract;
        private WebW2cOnline webW2cOnlineDataContract;
        private TaxForm1095cWhist taxForm1095cWhistContract;
        private WebT4Online webT4OnlineDataContract;
        private Collection<TaxForm1095cChist> taxForm1095cChistContracts;
        private Collection<Person> personContracts;
        private Person personContract;
        private Paymstr paymasterContract;
        private CorpFounds corpFoundsContract;
        private InstalledAppls installedApplsContract;
        private Defaults defaultsContract;
        private QtdYtdParameter1095CPDF qtdYtdParameter1095CPDFContract;
        private Ellucian.Colleague.Data.HumanResources.DataContracts.HrwebDefaults hrWebDefaults;

        private string caDesc = "Canada";
        private string usaDesc = "United States of America";

        [TestInitialize]
        public void Initialize()
        {
            dataReaderMock = new Mock<IColleagueDataReader>();
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();
            pdfDataRepository = BuildRepository();

            webW2OnlineId = "1";
            personId = "0004936";
            webW2OnlineDataContract = new WebW2Online()
            {
                Recordkey = "1",
                Ww2oYear = "2011",
                Ww2oEmployeeId = "0003948",
                Ww2oEmplyeAddrLine1 = "Employee line 1",
                Ww2oEmplyeAddrLine2 = "Employee line 2",
                Ww2oEmplyeAddrLine3 = "Employee line 3",
                Ww2oEmplyeAddrLine4 = "Employee line 4",
                Ww2oEmplyeAddrLine5 = "Employee line 5",
                Ww2oEmplyeAddrLine6 = "Employee line 6",
                Ww2oEmplyrAddrLine1 = "Employer line 1",
                Ww2oEmplyrAddrLine2 = "Employer line 2",
                Ww2oEmplyrAddrLine3 = "Employer line 3",
                Ww2oEmplyrAddrLine4 = "Employer line 4",
                Ww2oEmplyrId = "38-1234567",
                Ww2oEmplyrName = "Susty Corporation, Inc., LLC",
                Ww2oFirstName = "Gary",
                Ww2oLastName = "Thorne",
                Ww2oMiddleName = "Todd",
                Ww2oSuffix = "Jr.",
                Ww2oSsn = "000-00-0001",
                Ww2oCodeBoxCodeE = "E",
                Ww2oCodeBoxAmountE = "1000",
                Ww2oCodeBoxCodeF = "F",
                Ww2oCodeBoxAmountF = "1100",
                Ww2oOtherBoxCodeE = "E",
                Ww2oOtherBoxAmountE = "1200",
                Ww2oOtherBoxCodeF = "F",
                Ww2oOtherBoxAmountF = "1300",
                Ww2oOtherBoxCodeC = "C",
                Ww2oOtherBoxAmountC = "1400",
                Ww2oOtherBoxCodeD = "D",
                Ww2oOtherBoxAmountD = "1500",
                Ww2oStateCodeC = "C",
                Ww2oStateCodeD = "D",
                Ww2oStateIdC = "C",
                Ww2oStateIdD = "D",
                Ww2oStateWagesC = "1000",
                Ww2oStateWagesD = "2000",
                Ww2oStateWithheldC = "1000",
                Ww2oStateWithheldD = "2000",
                Ww2oLocalWagesC = "20000",
                Ww2oLocalWagesD = "20100",
                Ww2oLocalWithheldC = "3000",
                Ww2oLocalWithheldD = "3100",
                Ww2oLocalNameC = "Fairfax",
                Ww2oLocalNameD = "Barry"
            };

            webW2cOnlineId = "1";
            personId = "0004936";
            webW2cOnlineDataContract = new WebW2cOnline()
            {
                Recordkey = "1",
                Ww2coCorrectionYear ="2011",
                Ww2coEmployeeId = "0003948",
                Ww2coEmplyeAddrLine1 = "Employee line 1",
                Ww2coEmplyeAddrLine2 = "Employee line 2",
                Ww2coEmplyeAddrLine3 = "Employee line 3",
                Ww2coEmplyeAddrLine4 = "Employee line 4",
                Ww2coEmplyeAddrLine5 = "Employee line 5",
                Ww2coEmplyeAddrLine6 = "Employee line 6",
                Ww2coEmplyrAddrLine1 = "Employer line 1",
                Ww2coEmplyrAddrLine2 = "Employer line 2",
                Ww2coEmplyrAddrLine3 = "Employer line 3",
                Ww2coEmplyrAddrLine4 = "Employer line 4",
                Ww2coEmplyrId = "38-1234567",
                Ww2coEmplyrName = "Susty Corporation, Inc., LLC",
                Ww2coFirstName = "Gary",
                Ww2coLastName = "Thorne",
                Ww2coMiddleName = "Todd",
                Ww2coSuffix = "Jr.",
                Ww2coSsn = "000-00-0001",
                Ww2coCodeBoxCodeE = "E",
                Ww2coCodeBoxAmountE = "1000",
                Ww2coCodeBoxCodeF = "F",
                Ww2coCodeBoxAmountF = "1100",
                Ww2coOtherBoxCodeE = "E",
                Ww2coOtherBoxAmountE = "1200",
                Ww2coOtherBoxCodeF = "F",
                Ww2coOtherBoxAmountF = "1300",
                Ww2coOtherBoxCodeC = "C",
                Ww2coOtherBoxAmountC = "1400",
                Ww2coOtherBoxCodeD = "D",
                Ww2coOtherBoxAmountD = "1500",
                Ww2coStateCodeC = "C",
                Ww2coStateCodeD = "D",
                Ww2coStateIdC = "C",
                Ww2coStateIdD = "D",
                Ww2coStateWagesC = "1000",
                Ww2coStateWagesD = "2000",
                Ww2coStateWithheldC = "1000",
                Ww2coStateWithheldD = "2000",
                Ww2coLocalWagesC = "20000",
                Ww2coLocalWagesD = "20100",
                Ww2coLocalWithheldC = "3000",
                Ww2coLocalWithheldD = "3100",
                Ww2coLocalNameC = "Fairfax",
                Ww2coLocalNameD = "Barry"
            };

            dataReaderMock.Setup<Task<string[]>>(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(pdfIds);
            });

            dataReaderMock.Setup<Task<WebW2Online>>(dr => dr.ReadRecordAsync<WebW2Online>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(webW2OnlineDataContract);
            });

            dataReaderMock.Setup<Task<WebW2cOnline>>(dr => dr.ReadRecordAsync<WebW2cOnline>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(webW2cOnlineDataContract);
            });

            webT4OnlineDataContract = new WebT4Online()
            {
                Recordkey = "1",
                WebT4OnlineAdddate = new DateTime(2017, 1, 1),
                WebT4OnlineAddtime = new DateTime(2017, 1, 1, 12, 0, 0),
                Wt4oAddr1 = "123 Edmond Halley Dr",
                Wt4oAddr2 = "Apt 1",
                Wt4oCity = "Reston",
                Wt4oCountryCode = "USA",
                Wt4oPostalCode = "12345",
                Wt4oProvinceCode = "VA",
                Wt4oEmployeeId = "1",
                Wt4oCppExempt = "X",
                Wt4oEiExempt = "X",
                Wt4oFirstName = "John",
                Wt4oInitial = "T",
                Wt4oSurname = "Smith",
                Wt4oEmploymentCode = "",
                Wt4oEmploymentProvince = "ON",
                Wt4oPayerName1 = "Ellucian",
                Wt4oPayerName2 = "And Sons",
                Wt4oPayerAddr1 = "123 Main St",
                Wt4oPayerAddr2 = "Suite 1",
                Wt4oPayerCity = "Somewhere",
                Wt4oPayerProvCode = "ON",
                Wt4oPayerPostalCode = "12345",
                Wt4oPayerCountry = "CA",
                Wt4oSin = "123456789",
                Wt4oPensionRgstNo = new List<string>() { "321654" },
                Wt4oYear = "2017",
                Wt4oPpipExempt = "X",
                T4BoxInformationEntityAssociation = new List<WebT4OnlineT4BoxInformation>() {
                    new WebT4OnlineT4BoxInformation("31", "1000", "Y"),
                    new WebT4OnlineT4BoxInformation("33", "1000", "Y"),
                    new WebT4OnlineT4BoxInformation(null, "1000", "Y"),
                    new WebT4OnlineT4BoxInformation("14", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("16", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("17", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("18", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("20", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("22", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("24", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("26", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("44", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("46", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("50", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("52", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("55", "1000", "N"),
                    new WebT4OnlineT4BoxInformation("56", "1000", "N")
                }
            };

            dataReaderMock.Setup<Task<WebT4Online>>(dr => dr.ReadRecordAsync<WebT4Online>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(webT4OnlineDataContract);
            });

            // Mock the read for TAX.FORM.1095C.WHIST
            taxForm1095cWhistContract = new TaxForm1095cWhist()
            {
                Recordkey = "99",
                TfcwhTaxYear = "2014",
                TfcwhStatus = "SUB",
                TfcwhVoidInd = "",
                TfcwhHrperId = "0001111",
                TfcwhCorrectedInd = "",
                TfcwhCoveredIndivInd = "1",
                TfcwhFirstName = "Adriana",
                TfcwhMiddleName = "Marina",
                TfcwhLastName = "Romero Schwartz",
                TfcwhAddressLine1Text = "First line of address",
                TfcwhAddressLine2Text = "Second line of address",
                TfcwhCityName = "Fairfax",
                TfcwhStateProvCode = "VA",
                TfcwhPostalCode = "22033",
                TfcwhZipExtension = "2089",
                TfcwhCountryName = "United States of America",
                TfcwhLowestCostAmt12mnth = 90,
                TfcwhOfferCode12mnth = "1C",
                TfcwhSafeHarborCd12mnth = "2C",
                TfcwhLowestCostAmtJan = null,
                TfcwhOfferCodeJan = null,
                TfcwhSafeHarborCodeJan = null,
                TfcwhLowestCostAmtFeb = null,
                TfcwhOfferCodeFeb = null,
                TfcwhSafeHarborCodeFeb = null,
                TfcwhLowestCostAmtMar = null,
                TfcwhOfferCodeMar = null,
                TfcwhSafeHarborCodeMar = null,
                TfcwhLowestCostAmtApr = null,
                TfcwhOfferCodeApr = null,
                TfcwhSafeHarborCodeApr = null,
                TfcwhLowestCostAmtMay = null,
                TfcwhOfferCodeMay = null,
                TfcwhSafeHarborCodeMay = null,
                TfcwhLowestCostAmtJun = null,
                TfcwhOfferCodeJun = null,
                TfcwhSafeHarborCodeJun = null,
                TfcwhLowestCostAmtJul = null,
                TfcwhOfferCodeJul = null,
                TfcwhSafeHarborCodeJul = null,
                TfcwhLowestCostAmtAug = null,
                TfcwhOfferCodeAug = null,
                TfcwhSafeHarborCodeAug = null,
                TfcwhLowestCostAmtSep = null,
                TfcwhOfferCodeSep = null,
                TfcwhSafeHarborCodeSep = null,
                TfcwhLowestCostAmtOct = null,
                TfcwhOfferCodeOct = null,
                TfcwhSafeHarborCodeOct = null,
                TfcwhLowestCostAmtNov = null,
                TfcwhOfferCodeNov = null,
                TfcwhSafeHarborCodeNov = null,
                TfcwhLowestCostAmtDec = null,
                TfcwhOfferCodeDec = null,
            };

            dataReaderMock.Setup<Task<TaxForm1095cWhist>>(tw => tw.ReadRecordAsync<TaxForm1095cWhist>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1095cWhistContract);
            });

            // Mock the BulkRead for TAX.FORM.1095C.CHIST
            taxForm1095cChistContracts = new Collection<TaxForm1095cChist>()
            {
                new TaxForm1095cChist() { Recordkey = "320", Tfcch1095cId = "99", TfcchPersonId = "0001111", TfcchFirstName = "Adriana", TfcchMiddleName = "Marina", TfcchLastName = "Romero Schwartz", TfcchCoveredInd12mnth = "X", TfcchCoverageCode = "S" },
                new TaxForm1095cChist() { Recordkey = "321", Tfcch1095cId = "99", TfcchPersonId = "0002222", TfcchFirstName = "Kevin", TfcchMiddleName = "F", TfcchLastName = "Smith", TfcchCoveredInd12mnth = "X", TfcchCoverageCode = "D" },
                new TaxForm1095cChist() { Recordkey = "322", Tfcch1095cId = "99", TfcchPersonId = "0003333",TfcchFirstName = "Mary", TfcchMiddleName = "", TfcchLastName = "Reno", TfcchCoveredInd12mnth = "X", TfcchCoverageCode = "D" }
            };

            dataReaderMock.Setup<Task<Collection<TaxForm1095cChist>>>(tc => tc.BulkReadRecordAsync<TaxForm1095cChist>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1095cChistContracts);
            });

            // Mock the read for PAYROLL.MASTER
            paymasterContract = new Paymstr() { Recordkey = "PAYROLL.MASTER", PmInstitutionName = "Ellucian, Inc.", PmInstitutionEin = "12-3456789", PmInstitutionCity = "Fairfax", PmInstitutionState = "VA", PmInstitutionZipcode = "22033" };
            paymasterContract.PmInstitutionAddress = new List<string>() { "Ellucian first line of address", "Ellucian second line of address" };

            // DataReader.ReadRecordAsync<Paymstr>("ACCOUNT.PARAMETERS", "PAYROLL.MASTER");
            dataReaderMock.Setup<Task<Paymstr>>(pm => pm.ReadRecordAsync<Paymstr>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(paymasterContract);
            });

            // Mock the read for DEFAULTS
            defaultsContract = new Defaults() { DefaultHostCorpId = "0000043" };
            dataReaderMock.Setup<Task<Defaults>>(acc => acc.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(defaultsContract);
            });

            // Mock the read for CORP.FOUNDS
            corpFoundsContract = new CorpFounds() { Recordkey = "0000043", CorpTaxId = "987654321" };
            dataReaderMock.Setup<Task<CorpFounds>>(dr => dr.ReadRecordAsync<CorpFounds>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(corpFoundsContract);
            });

            // Mock the read for INSTALLED.APPLICATIONS
            installedApplsContract = new InstalledAppls() { IaModuleNames = new List<string>() { "HR", "FA", "PR" } };
            installedApplsContract.buildAssociations();
            dataReaderMock.Setup<Task<InstalledAppls>>(dr => dr.ReadRecordAsync<InstalledAppls>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(installedApplsContract);
            });

            // Set up the single read of Person that we need for employee "0001111"
            personContract = new Person() { Recordkey = "0001111", Ssn = "111-11-1111", BirthDate = new DateTime(1956, 04, 12) };
            dataReaderMock.Setup<Task<Person>>(p => p.ReadRecordAsync<Person>("0001111", true)).Returns(() =>
            {
                return Task.FromResult(personContract);
            });

            // Mock the  bulkRead for PERSON for the covered Individuals
            personContracts = new Collection<Person>()
             {
                 new Person() { Recordkey = "0001111", FirstName = "Andrew", MiddleName = "John", LastName = "Kleehammer", Ssn = "111-11-1111", BirthDate = new DateTime(1956, 04, 12) },
                 new Person() { Recordkey = "0002222", FirstName = "Teresa", MiddleName = "Maria", LastName = "Longerbeam", Ssn = "222-22-2222", BirthDate = new DateTime(1984, 09, 19) },
                 new Person() { Recordkey = "0003333", FirstName = "Gary", MiddleName = "Todd", LastName = "Thorne", Ssn = "333-33-3333", BirthDate = new DateTime(1962, 12, 30) },
             };

            // Set up the bulkRead of Person that we need to get the dependents SSN
            dataReaderMock.Setup<Task<Collection<Person>>>(br => br.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(personContracts);
            });

            // Mock the read for QtdYtdParameter1095CPDF
            qtdYtdParameter1095CPDFContract = new QtdYtdParameter1095CPDF() { Qyp1095cContactPhone = "7035556789", Qyp1095cContactExt = "968", Qyp1095cPlanStartMonth = "01" };
            dataReaderMock.Setup<Task<QtdYtdParameter1095CPDF>>(qr => qr.ReadRecordAsync<QtdYtdParameter1095CPDF>("HR.PARMS", "QTD.YTD.PARAMETER", true)).Returns(() =>
            {
                return Task.FromResult(qtdYtdParameter1095CPDFContract);
            });

            hrWebDefaults = new DataContracts.HrwebDefaults()
            {
                HrwebW2oMaskSsn = "Y",
                Hrweb1095cMaskSsn = "Y"
            };
            dataReaderMock.Setup(x => x.ReadRecordAsync<Ellucian.Colleague.Data.HumanResources.DataContracts.HrwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(hrWebDefaults);
                });

            // Mock some countries for the T4 read
            dataReaderMock.Setup(x => x.ReadRecordAsync<Countries>("CA", true)).Returns(() =>
            {
                return Task.FromResult(new Countries() { CtryDesc = caDesc });
            });
            dataReaderMock.Setup(x => x.ReadRecordAsync<Countries>("USA", true)).Returns(() =>
            {
                return Task.FromResult(new Countries() { CtryDesc = usaDesc });
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReaderMock = null;
            transactionInvoker = null;
            hierarchyNameResponse = null;
            hierarchyAddressResponse = null;
            webW2OnlineDataContract = null;
            webT4OnlineDataContract = null;
            taxForm1095cWhistContract = null;
            taxForm1095cChistContracts = null;
            personContracts = null;
            personContract = null;
            paymasterContract = null;
            corpFoundsContract = null;
            installedApplsContract = null;
            defaultsContract = null;
            qtdYtdParameter1095CPDFContract = null;
            webW2OnlineId = null;
        }

        #endregion

        #region W-2
        [TestMethod]
        public async Task GetW2PdfData_Success()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrId, actualDomainEntity.EmployerEin);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrName, actualDomainEntity.EmployerName);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrAddrLine1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrAddrLine2, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrAddrLine3, actualDomainEntity.EmployerAddressLine3);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyrAddrLine4, actualDomainEntity.EmployerAddressLine4);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oLastName, actualDomainEntity.EmployeeLastName);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oMiddleName, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oSuffix, actualDomainEntity.EmployeeSuffix);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyeAddrLine1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyeAddrLine2, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyeAddrLine3, actualDomainEntity.EmployeeAddressLine3);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oEmplyeAddrLine4, actualDomainEntity.EmployeeAddressLine4);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oFederalWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.FederalWages);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oFederalWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.FederalWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oSocSecWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityWages);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oSocSecWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oMedicareWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.MedicareWages);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oMedicareWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.MedicareWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oSocSecTips) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityTips);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oAllocatedTips) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.AllocatedTips);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oDependentCare) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.DependentCare);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oNonqualTotal) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.NonqualifiedTotal);
        }

        #region SSN scenarios
        [TestMethod]
        public async Task GetW2PdfAsync_DefaultSsn()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_SsnNotProperLength()
        {
            webW2OnlineDataContract.Ww2oSsn = "000-0";
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_NullSsn()
        {
            webW2OnlineDataContract.Ww2oSsn = null;
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_EmptySsn()
        {
            webW2OnlineDataContract.Ww2oSsn = "";
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        #region Masking scenarios
        [TestMethod]
        public async Task GetW2PdfAsync_MaskedSsn1()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2OnlineDataContract.Ww2oSsn.Substring(webW2OnlineDataContract.Ww2oSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_MaskedSsn2()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2OnlineDataContract.Ww2oSsn.Substring(webW2OnlineDataContract.Ww2oSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_NullHrWebDefaultsContract()
        {
            hrWebDefaults = null;
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_NullMaskParameter()
        {
            hrWebDefaults.HrwebW2oMaskSsn = null;
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_EmptyMaskParameter()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_SsnNotFullLength()
        {
            webW2OnlineDataContract.Ww2oSsn = "000-0";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2OnlineDataContract.Ww2oSsn.Substring(webW2OnlineDataContract.Ww2oSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_SsnExactly4Digits()
        {
            webW2OnlineDataContract.Ww2oSsn = "000-";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2OnlineDataContract.Ww2oSsn.Substring(webW2OnlineDataContract.Ww2oSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_SsnLessThan4Digits()
        {
            webW2OnlineDataContract.Ww2oSsn = "0";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2OnlineDataContract.Ww2oSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_NullSsn_Masked()
        {
            webW2OnlineDataContract.Ww2oSsn = null;
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2PdfAsync_EmptySsn_Masked()
        {
            webW2OnlineDataContract.Ww2oSsn = "";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2PdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        [TestMethod]
        public async Task UnmaskedSsn()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oSsn, actualDomainEntity.EmployeeSsn);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_DataReaderReturnsNull()
        {
            webW2OnlineDataContract = null;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2PdfData_NullPersonId()
        {
            await pdfDataRepository.GetW2PdfAsync(null, "11");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2PdfData_EmptyPersonId()
        {
            await pdfDataRepository.GetW2PdfAsync(string.Empty, "11");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2PdfData_NullId()
        {
            await pdfDataRepository.GetW2PdfAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2PdfData_EmptyId()
        {
            webW2OnlineId = string.Empty;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_DataReaderSelectReturnsNull()
        {
            pdfIds = null;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_DataReaderReturnsZeroW2Ids()
        {
            pdfIds = new string[] {};
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_DataReaderReturnsMultipleW2Ids()
        {
            pdfIds = new string[] { "1", "2" };
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_NullTaxYear()
        {
            webW2OnlineDataContract.Ww2oYear = string.Empty;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_NullEmployerId()
        {
            webW2OnlineDataContract.Ww2oEmplyrId = string.Empty;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box12aCode_CodeEData()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oCodeBoxCodeE, actualDomainEntity.Box12aCode);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oCodeBoxAmountE) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12aAmount);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box12aCode_CodeAData()
        {
            webW2OnlineDataContract.Ww2oCodeBoxCodeE = string.Empty;
            webW2OnlineDataContract.Ww2oCodeBoxAmountE = string.Empty;
            webW2OnlineDataContract.Ww2oCodeBoxCodeA = "A";
            webW2OnlineDataContract.Ww2oCodeBoxAmountA = "2000";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oCodeBoxCodeA, actualDomainEntity.Box12aCode);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oCodeBoxAmountA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12aAmount);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box12bCode_CodeFData()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oCodeBoxCodeF, actualDomainEntity.Box12bCode);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oCodeBoxAmountF) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12bAmount);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box12bCode_CodeBData()
        {
            webW2OnlineDataContract.Ww2oCodeBoxCodeF = string.Empty;
            webW2OnlineDataContract.Ww2oCodeBoxAmountF = string.Empty;
            webW2OnlineDataContract.Ww2oCodeBoxCodeB = "B";
            webW2OnlineDataContract.Ww2oCodeBoxAmountB = "2100";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oCodeBoxCodeB, actualDomainEntity.Box12bCode);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oCodeBoxAmountB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12bAmount);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line1_OtherBoxCodeE()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeE + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountE) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line1_OtherBoxCodeA()
        {
            webW2OnlineDataContract.Ww2oOtherBoxCodeE = string.Empty;
            webW2OnlineDataContract.Ww2oOtherBoxAmountE = string.Empty;
            webW2OnlineDataContract.Ww2oOtherBoxCodeA = "A";
            webW2OnlineDataContract.Ww2oOtherBoxAmountA = "2200";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeA + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line2_OtherBoxCodeF()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeF + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountF) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line2_OtherBoxCodeB()
        {
            webW2OnlineDataContract.Ww2oOtherBoxCodeF = string.Empty;
            webW2OnlineDataContract.Ww2oOtherBoxAmountF = string.Empty;
            webW2OnlineDataContract.Ww2oOtherBoxCodeB = "B";
            webW2OnlineDataContract.Ww2oOtherBoxAmountB = "2300";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeB + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line3_OtherBoxCodeC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeC + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line3);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box14Line4_OtherBoxCodeD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oOtherBoxCodeD + " - " + (Convert.ToDecimal(webW2OnlineDataContract.Ww2oOtherBoxAmountD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line4);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line1Section1_StateCodeC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateCodeC, actualDomainEntity.Box15Line1Section1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line1Section1_StateCodeA()
        {
            webW2OnlineDataContract.Ww2oStateCodeC = string.Empty;
            webW2OnlineDataContract.Ww2oStateCodeA = "A";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateCodeA, actualDomainEntity.Box15Line1Section1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line2Section1_StateCodeD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateCodeD, actualDomainEntity.Box15Line2Section1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line2Section1_StateCodeB()
        {
            webW2OnlineDataContract.Ww2oStateCodeD = string.Empty;
            webW2OnlineDataContract.Ww2oStateCodeB = "B";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateCodeB, actualDomainEntity.Box15Line2Section1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line1Section2_StateIdC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateIdC, actualDomainEntity.Box15Line1Section2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line1Section2_StateIdA()
        {
            webW2OnlineDataContract.Ww2oStateIdC = string.Empty;
            webW2OnlineDataContract.Ww2oStateIdA = "A";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateIdA, actualDomainEntity.Box15Line1Section2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line2Section2_StateIdD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateIdD, actualDomainEntity.Box15Line2Section2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box15Line2Section2_StateIdB()
        {
            webW2OnlineDataContract.Ww2oStateIdD = string.Empty;
            webW2OnlineDataContract.Ww2oStateIdB = "B";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oStateIdB, actualDomainEntity.Box15Line2Section2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box16Line1_StateWagesC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWagesC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box16Line1_StateWagesA()
        {
            webW2OnlineDataContract.Ww2oStateWagesC = string.Empty;
            webW2OnlineDataContract.Ww2oStateWagesA = "2000";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWagesA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box16Line2_StateWagesD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWagesD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box16Line2_StateWagesB()
        {
            webW2OnlineDataContract.Ww2oStateWagesD = string.Empty;
            webW2OnlineDataContract.Ww2oStateWagesB = "2500";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWagesB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box17Line1_StateWithheldC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWithheldC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box17Line1_StateWithheldA()
        {
            webW2OnlineDataContract.Ww2oStateWithheldC = string.Empty;
            webW2OnlineDataContract.Ww2oStateWithheldA = "2600";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWithheldA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box17Line2_StateWithheldD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWithheldD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box17Line2_StateWithheldB()
        {
            webW2OnlineDataContract.Ww2oStateWithheldD = string.Empty;
            webW2OnlineDataContract.Ww2oStateWithheldB = "2700";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oStateWithheldB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box18Line1_LocalWagesC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWagesC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box18Line1_LocalWagesA()
        {
            webW2OnlineDataContract.Ww2oLocalWagesC = string.Empty;
            webW2OnlineDataContract.Ww2oLocalWagesA = "2900";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWagesA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box18Line2_LocalWagesD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWagesD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box18Line2_LocalWagesB()
        {
            webW2OnlineDataContract.Ww2oLocalWagesD = string.Empty;
            webW2OnlineDataContract.Ww2oLocalWagesB = "3100";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWagesB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box19Line1_LocalWithheldC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWithheldC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box19Line1_LocalWithheldA()
        {
            webW2OnlineDataContract.Ww2oLocalWithheldC = string.Empty;
            webW2OnlineDataContract.Ww2oLocalWithheldA = "1100";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWithheldA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box19Line2_LocalWithheldD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWithheldD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box19Line2_LocalWithheldB()
        {
            webW2OnlineDataContract.Ww2oLocalWithheldD = string.Empty;
            webW2OnlineDataContract.Ww2oLocalWithheldB = "1200";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2OnlineDataContract.Ww2oLocalWithheldB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box20Line1_LocalNameC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oLocalNameC, actualDomainEntity.Box20Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box20Line1_LocalNameA()
        {
            webW2OnlineDataContract.Ww2oLocalNameC = string.Empty;
            webW2OnlineDataContract.Ww2oLocalNameA = "Grant";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oLocalNameA, actualDomainEntity.Box20Line1);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box20Line2_LocalNameD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(webW2OnlineDataContract.Ww2oLocalNameD, actualDomainEntity.Box20Line2);
        }

        [TestMethod]
        public async Task GetW2PdfData_Box20Line2_LocalNameB()
        {
            webW2OnlineDataContract.Ww2oLocalNameD = string.Empty;
            webW2OnlineDataContract.Ww2oLocalNameB = "Hampshire";
            var actualDomainEntity = await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);

            Assert.AreEqual(webW2OnlineDataContract.Ww2oLocalNameB, actualDomainEntity.Box20Line2);
        }
        #endregion

        #region W-2
        [TestMethod]
        public async Task GetW2cPdfData_Success()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coCorrectionYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrId, actualDomainEntity.EmployerEin);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrName, actualDomainEntity.EmployerName);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrAddrLine1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrAddrLine2, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrAddrLine3, actualDomainEntity.EmployerAddressLine3);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyrAddrLine4, actualDomainEntity.EmployerAddressLine4);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coLastName, actualDomainEntity.EmployeeLastName);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coMiddleName, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSuffix, actualDomainEntity.EmployeeSuffix);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyeAddrLine1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyeAddrLine2, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyeAddrLine3, actualDomainEntity.EmployeeAddressLine3);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coEmplyeAddrLine4, actualDomainEntity.EmployeeAddressLine4);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coFederalWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.FederalWages);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coFederalWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.FederalWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coSocSecWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityWages);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coSocSecWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coMedicareWages) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.MedicareWages);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coMedicareWithholding) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.MedicareWithholding);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coSocSecTips) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.SocialSecurityTips);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coAllocatedTips) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.AllocatedTips);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coDependentCare) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.DependentCare);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coNonqualTotal) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.NonqualifiedTotal);
        }

        #region SSN scenarios
        [TestMethod]
        public async Task GetW2cPdfAsync_DefaultSsn()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_SsnNotProperLength()
        {
            webW2cOnlineDataContract.Ww2coSsn = "000-0";
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_NullSsn()
        {
            webW2cOnlineDataContract.Ww2coSsn = null;
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_EmptySsn()
        {
            webW2cOnlineDataContract.Ww2coSsn = "";
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        #region Masking scenarios
        [TestMethod]
        public async Task GetW2cPdfAsync_MaskedSsn1()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2cOnlineDataContract.Ww2coSsn.Substring(webW2cOnlineDataContract.Ww2coSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_MaskedSsn2()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2cOnlineDataContract.Ww2coSsn.Substring(webW2cOnlineDataContract.Ww2coSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_NullHrWebDefaultsContract()
        {
            hrWebDefaults = null;
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_NullMaskParameter()
        {
            hrWebDefaults.HrwebW2oMaskSsn = null;
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_EmptyMaskParameter()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_SsnNotFullLength()
        {
            webW2cOnlineDataContract.Ww2coSsn = "000-0";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2cOnlineDataContract.Ww2coSsn.Substring(webW2cOnlineDataContract.Ww2coSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_SsnExactly4Digits()
        {
            webW2cOnlineDataContract.Ww2coSsn = "000-";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2cOnlineDataContract.Ww2coSsn.Substring(webW2cOnlineDataContract.Ww2coSsn.Length - 4),
                pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_SsnLessThan4Digits()
        {
            webW2cOnlineDataContract.Ww2coSsn = "0";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + webW2cOnlineDataContract.Ww2coSsn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_NullSsn_Masked()
        {
            webW2cOnlineDataContract.Ww2coSsn = null;
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task GetW2cPdfAsync_EmptySsn_Masked()
        {
            webW2cOnlineDataContract.Ww2coSsn = "";
            hrWebDefaults.HrwebW2oMaskSsn = "Y";
            var pdfData = await pdfDataRepository.GetW2cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        [TestMethod]
        public async Task UnmaskedSsnW2c()
        {
            hrWebDefaults.HrwebW2oMaskSsn = "N";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coSsn, actualDomainEntity.EmployeeSsn);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2cPdfData_DataReaderReturnsNull()
        {
            webW2cOnlineDataContract = null;
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2cPdfData_NullPersonId()
        {
            await pdfDataRepository.GetW2cPdfAsync(null, "11");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2cPdfData_EmptyPersonId()
        {
            await pdfDataRepository.GetW2cPdfAsync(string.Empty, "11");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2cPdfData_NullId()
        {
            await pdfDataRepository.GetW2cPdfAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2cPdfData_EmptyId()
        {
            webW2cOnlineId = string.Empty;
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2cPdfData_DataReaderSelectReturnsNull()
        {
            pdfIds = null;
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetWc2PdfData_DataReaderReturnsZeroW2Ids()
        {
            pdfIds = new string[] { };
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2PdfData_DataReaderReturnsMultipleW2cIds()
        {
            pdfIds = new string[] { "1", "2" };
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2cPdfData_NullTaxYear()
        {
            webW2cOnlineDataContract.Ww2coCorrectionYear = string.Empty;
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetW2cPdfData_NullEmployerId()
        {
            webW2cOnlineDataContract.Ww2coEmplyrId = string.Empty;
            await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box12aCode_CodeEData()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coCodeBoxCodeE, actualDomainEntity.Box12aCode);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coCodeBoxAmountE) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12aAmount);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box12aCode_CodeAData()
        {
            webW2cOnlineDataContract.Ww2coCodeBoxCodeE = string.Empty;
            webW2cOnlineDataContract.Ww2coCodeBoxAmountE = string.Empty;
            webW2cOnlineDataContract.Ww2coCodeBoxCodeA = "A";
            webW2cOnlineDataContract.Ww2coCodeBoxAmountA = "2000";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coCodeBoxCodeA, actualDomainEntity.Box12aCode);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coCodeBoxAmountA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12aAmount);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box12bCode_CodeFData()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coCodeBoxCodeF, actualDomainEntity.Box12bCode);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coCodeBoxAmountF) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12bAmount);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box12bCode_CodeBData()
        {
            webW2cOnlineDataContract.Ww2coCodeBoxCodeF = string.Empty;
            webW2cOnlineDataContract.Ww2coCodeBoxAmountF = string.Empty;
            webW2cOnlineDataContract.Ww2coCodeBoxCodeB = "B";
            webW2cOnlineDataContract.Ww2coCodeBoxAmountB = "2100";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coCodeBoxCodeB, actualDomainEntity.Box12bCode);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coCodeBoxAmountB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box12bAmount);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line1_OtherBoxCodeE()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeE + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountE) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line1_OtherBoxCodeA()
        {
            webW2cOnlineDataContract.Ww2coOtherBoxCodeE = string.Empty;
            webW2cOnlineDataContract.Ww2coOtherBoxAmountE = string.Empty;
            webW2cOnlineDataContract.Ww2coOtherBoxCodeA = "A";
            webW2cOnlineDataContract.Ww2coOtherBoxAmountA = "2200";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeA + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line2_OtherBoxCodeF()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeF + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountF) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line2_OtherBoxCodeB()
        {
            webW2cOnlineDataContract.Ww2coOtherBoxCodeF = string.Empty;
            webW2cOnlineDataContract.Ww2coOtherBoxAmountF = string.Empty;
            webW2cOnlineDataContract.Ww2coOtherBoxCodeB = "B";
            webW2cOnlineDataContract.Ww2coOtherBoxAmountB = "2300";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeB + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line3_OtherBoxCodeC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeC + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line3);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box14Line4_OtherBoxCodeD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coOtherBoxCodeD + " - " + (Convert.ToDecimal(webW2cOnlineDataContract.Ww2coOtherBoxAmountD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box14Line4);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line1Section1_StateCodeC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateCodeC, actualDomainEntity.Box15Line1Section1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line1Section1_StateCodeA()
        {
            webW2cOnlineDataContract.Ww2coStateCodeC = string.Empty;
            webW2cOnlineDataContract.Ww2coStateCodeA = "A";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateCodeA, actualDomainEntity.Box15Line1Section1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line2Section1_StateCodeD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateCodeD, actualDomainEntity.Box15Line2Section1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line2Section1_StateCodeB()
        {
            webW2cOnlineDataContract.Ww2coStateCodeD = string.Empty;
            webW2cOnlineDataContract.Ww2coStateCodeB = "B";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateCodeB, actualDomainEntity.Box15Line2Section1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line1Section2_StateIdC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateIdC, actualDomainEntity.Box15Line1Section2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line1Section2_StateIdA()
        {
            webW2cOnlineDataContract.Ww2coStateIdC = string.Empty;
            webW2cOnlineDataContract.Ww2coStateIdA = "A";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateIdA, actualDomainEntity.Box15Line1Section2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line2Section2_StateIdD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateIdD, actualDomainEntity.Box15Line2Section2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box15Line2Section2_StateIdB()
        {
            webW2cOnlineDataContract.Ww2coStateIdD = string.Empty;
            webW2cOnlineDataContract.Ww2coStateIdB = "B";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coStateIdB, actualDomainEntity.Box15Line2Section2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box16Line1_StateWagesC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWagesC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box16Line1_StateWagesA()
        {
            webW2cOnlineDataContract.Ww2coStateWagesC = string.Empty;
            webW2cOnlineDataContract.Ww2coStateWagesA = "2000";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWagesA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box16Line2_StateWagesD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWagesD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box16Line2_StateWagesB()
        {
            webW2cOnlineDataContract.Ww2coStateWagesD = string.Empty;
            webW2cOnlineDataContract.Ww2coStateWagesB = "2500";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWagesB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box16Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box17Line1_StateWithheldC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWithheldC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box17Line1_StateWithheldA()
        {
            webW2cOnlineDataContract.Ww2coStateWithheldC = string.Empty;
            webW2cOnlineDataContract.Ww2coStateWithheldA = "2600";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWithheldA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box17Line2_StateWithheldD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWithheldD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box17Line2_StateWithheldB()
        {
            webW2cOnlineDataContract.Ww2coStateWithheldD = string.Empty;
            webW2cOnlineDataContract.Ww2coStateWithheldB = "2700";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coStateWithheldB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box17Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box18Line1_LocalWagesC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWagesC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box18Line1_LocalWagesA()
        {
            webW2cOnlineDataContract.Ww2coLocalWagesC = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalWagesA = "2900";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWagesA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box18Line2_LocalWagesD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWagesD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box18Line2_LocalWagesB()
        {
            webW2cOnlineDataContract.Ww2coLocalWagesD = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalWagesB = "3100";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWagesB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box18Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box19Line1_LocalWithheldC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWithheldC) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box19Line1_LocalWithheldA()
        {
            webW2cOnlineDataContract.Ww2coLocalWithheldC = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalWithheldA = "1100";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWithheldA) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box19Line2_LocalWithheldD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWithheldD) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box19Line2_LocalWithheldB()
        {
            webW2cOnlineDataContract.Ww2coLocalWithheldD = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalWithheldB = "1200";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual((Convert.ToDecimal(webW2cOnlineDataContract.Ww2coLocalWithheldB) / 100).ToString("N2", CultureInfo.InvariantCulture), actualDomainEntity.Box19Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box20Line1_LocalNameC()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coLocalNameC, actualDomainEntity.Box20Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box20Line1_LocalNameA()
        {
            webW2cOnlineDataContract.Ww2coLocalNameC = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalNameA = "Grant";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coLocalNameA, actualDomainEntity.Box20Line1);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box20Line2_LocalNameD()
        {
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);
            Assert.AreEqual(webW2cOnlineDataContract.Ww2coLocalNameD, actualDomainEntity.Box20Line2);
        }

        [TestMethod]
        public async Task GetW2cPdfData_Box20Line2_LocalNameB()
        {
            webW2cOnlineDataContract.Ww2coLocalNameD = string.Empty;
            webW2cOnlineDataContract.Ww2coLocalNameB = "Hampshire";
            var actualDomainEntity = await pdfDataRepository.GetW2cPdfAsync(personId, webW2cOnlineId);

            Assert.AreEqual(webW2cOnlineDataContract.Ww2coLocalNameB, actualDomainEntity.Box20Line2);
        }
        #endregion

        #region 1095-C
        [TestMethod]
        public async Task Get1095cPdfData_Success_WithNoDependentsAndPayrollModule()
        {
            taxForm1095cWhistContract.TfcwhCoveredIndivInd = "";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from PAYMSTR
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, paymasterContract.PmInstitutionEin);
            Assert.AreEqual(form1095cDomainEntity.EmployerName, paymasterContract.PmInstitutionName);
            Assert.AreEqual(form1095cDomainEntity.EmployerAddressLine, paymasterContract.PmInstitutionAddress.FirstOrDefault());
            Assert.AreEqual(form1095cDomainEntity.EmployerCityName, paymasterContract.PmInstitutionCity);
            Assert.AreEqual(form1095cDomainEntity.EmployerStateCode, paymasterContract.PmInstitutionState);
            Assert.AreEqual(form1095cDomainEntity.EmployerZipCode, paymasterContract.PmInstitutionZipcode);

            // Make sure the phone number is formatted correctly.
            var p = qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone;
            var pn = String.Format("{0}-{1}-{2}", p.Substring(0, 3), p.Substring(3, 3), p.Substring(6, 4));
            Assert.AreEqual(form1095cDomainEntity.EmployerContactPhoneNumber, pn);


            Assert.AreEqual(form1095cDomainEntity.TaxYear, taxForm1095cWhistContract.TfcwhTaxYear);
            //Assert.AreEqual(form1095cDomainEntity.EmployeeSsn, personContract.Ssn);
            Assert.AreEqual(form1095cDomainEntity.EmployeeFirstName, taxForm1095cWhistContract.TfcwhFirstName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeLastName, taxForm1095cWhistContract.TfcwhLastName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeMiddleName, taxForm1095cWhistContract.TfcwhMiddleName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine1, taxForm1095cWhistContract.TfcwhAddressLine1Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine2, taxForm1095cWhistContract.TfcwhAddressLine2Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCityName, taxForm1095cWhistContract.TfcwhCityName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeStateCode, taxForm1095cWhistContract.TfcwhStateProvCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeePostalCode, taxForm1095cWhistContract.TfcwhPostalCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeeZipExtension, taxForm1095cWhistContract.TfcwhZipExtension);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCountry, taxForm1095cWhistContract.TfcwhCountryName);

            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverage12Month, taxForm1095cWhistContract.TfcwhOfferCode12mnth);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJanuary, taxForm1095cWhistContract.TfcwhOfferCodeJan);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageFebruary, taxForm1095cWhistContract.TfcwhOfferCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMarch, taxForm1095cWhistContract.TfcwhOfferCodeMar);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageApril, taxForm1095cWhistContract.TfcwhOfferCodeApr);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMay, taxForm1095cWhistContract.TfcwhOfferCodeMay);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJune, taxForm1095cWhistContract.TfcwhOfferCodeJun);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJuly, taxForm1095cWhistContract.TfcwhOfferCodeJul);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageAugust, taxForm1095cWhistContract.TfcwhOfferCodeAug);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageSeptember, taxForm1095cWhistContract.TfcwhOfferCodeSep);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageOctober, taxForm1095cWhistContract.TfcwhOfferCodeOct);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageNovember, taxForm1095cWhistContract.TfcwhOfferCodeNov);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageDecember, taxForm1095cWhistContract.TfcwhOfferCodeDec);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmount12Month, taxForm1095cWhistContract.TfcwhLowestCostAmt12mnth);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJanuary, taxForm1095cWhistContract.TfcwhLowestCostAmtJan);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountFebruary, taxForm1095cWhistContract.TfcwhLowestCostAmtFeb);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMarch, taxForm1095cWhistContract.TfcwhLowestCostAmtMar);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountApril, taxForm1095cWhistContract.TfcwhLowestCostAmtApr);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMay, taxForm1095cWhistContract.TfcwhLowestCostAmtMay);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJune, taxForm1095cWhistContract.TfcwhLowestCostAmtJun);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJuly, taxForm1095cWhistContract.TfcwhLowestCostAmtJul);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountAugust, taxForm1095cWhistContract.TfcwhLowestCostAmtAug);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountSeptember, taxForm1095cWhistContract.TfcwhLowestCostAmtSep);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountOctober, taxForm1095cWhistContract.TfcwhLowestCostAmtOct);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountNovember, taxForm1095cWhistContract.TfcwhLowestCostAmtNov);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountDecember, taxForm1095cWhistContract.TfcwhLowestCostAmtDec);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCode12Month, taxForm1095cWhistContract.TfcwhSafeHarborCd12mnth);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJanuary, taxForm1095cWhistContract.TfcwhSafeHarborCodeJan);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeFebruary, taxForm1095cWhistContract.TfcwhSafeHarborCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMarch, taxForm1095cWhistContract.TfcwhSafeHarborCodeMar);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeApril, taxForm1095cWhistContract.TfcwhSafeHarborCodeApr);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMay, taxForm1095cWhistContract.TfcwhSafeHarborCodeMay);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJune, taxForm1095cWhistContract.TfcwhSafeHarborCodeJun);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJuly, taxForm1095cWhistContract.TfcwhSafeHarborCodeJul);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeAugust, taxForm1095cWhistContract.TfcwhSafeHarborCodeAug);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeSeptember, taxForm1095cWhistContract.TfcwhSafeHarborCodeSep);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeOctober, taxForm1095cWhistContract.TfcwhSafeHarborCodeOct);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeNovember, taxForm1095cWhistContract.TfcwhSafeHarborCodeNov);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeDecember, taxForm1095cWhistContract.TfcwhSafeHarborCodeDec);
            Assert.IsFalse(form1095cDomainEntity.EmployeeIsSelfInsured);
            Assert.IsFalse(form1095cDomainEntity.IsCorrected);
            Assert.IsFalse(form1095cDomainEntity.IsVoided);
            Assert.AreEqual(qtdYtdParameter1095CPDFContract.Qyp1095cPlanStartMonth, form1095cDomainEntity.PlanStartMonthCode);
        }

        #region Recipient SSN scenarios
        [TestMethod]
        public async Task Get1095cPdfAsync_DefaultSsn()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual(personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_SsnNotProperLength()
        {
            personContract.Ssn = "000-0";
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual(personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_NullPersonContract()
        {
            personContract = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_NullSsn()
        {
            personContract.Ssn = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_EmptySsn()
        {
            personContract.Ssn = "";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        #region Covered individual SSN scenarios
        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_DefaultSsn()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are NOT masked.
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_SsnNotProperLength()
        {
            personContracts[0].Ssn = "000-0";
            personContracts[1].Ssn = "00-00";
            personContracts[2].Ssn = "0-000";
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are NOT masked.
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_NullPersonContracts()
        {
            personContracts[0] = null;
            personContracts[1] = null;
            personContracts[2] = null;
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are blank.
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                Assert.AreEqual("", coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_NullSsn()
        {
            personContracts[0].Ssn = null;
            personContracts[1].Ssn = null;
            personContracts[2].Ssn = null;
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are blank.
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                Assert.AreEqual("", coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_EmptySsn()
        {
            personContracts[0].Ssn = "";
            personContracts[1].Ssn = "";
            personContracts[2].Ssn = "";
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are blank.
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                Assert.AreEqual("", coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }
        #endregion

        #region Recipient Masking scenarios
        [TestMethod]
        public async Task Get1095cPdfAsync_MaskedSsn1()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_MaskedSsn2()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_NullHrWebDefaultsContract()
        {
            hrWebDefaults = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual(personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_NullMaskParameter()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual(personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_EmptyMaskParameter()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual(personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_SsnNotFullLength()
        {
            personContract.Ssn = "000-0";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_SsnExactly4Digits()
        {
            personContract.Ssn = "000-";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_SsnLessThan4Digits()
        {
            personContract.Ssn = "0";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn, pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_NullSsn_Masked()
        {
            personContract.Ssn = null;
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_EmptySsn_Masked()
        {
            personContract.Ssn = "";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
        #endregion

        #region Recipient Masking scenarios
        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_MaskedSsn1()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual("XXX-XX-" + selectedContract.Ssn.Substring(selectedContract.Ssn.Length - 4), coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_MaskedSsn2()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual("XXX-XX-" + selectedContract.Ssn.Substring(selectedContract.Ssn.Length - 4), coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_NullHrWebDefaultsContract()
        {
            hrWebDefaults = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_NullMaskParameter()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = null;
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_EmptyMaskParameter()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_SsnNotFullLength()
        {
            personContracts[0].Ssn = "000-0";
            personContracts[1].Ssn = "00-00";
            personContracts[2].Ssn = "0-000";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual("XXX-XX-" + selectedContract.Ssn.Substring(selectedContract.Ssn.Length - 4), coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_SsnExactly4Digits()
        {
            personContracts[0].Ssn = "00-0";
            personContracts[1].Ssn = "0-00";
            personContracts[2].Ssn = "-000";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual("XXX-XX-" + selectedContract.Ssn.Substring(selectedContract.Ssn.Length - 4), coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_SsnLessThan4Digits()
        {
            personContracts[0].Ssn = "0";
            personContracts[1].Ssn = "1";
            personContracts[2].Ssn = "2";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual("XXX-XX-" + selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_NullSsn_Masked()
        {
            personContracts[0].Ssn = null;
            personContracts[1].Ssn = null;
            personContracts[2].Ssn = null;
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                Assert.AreEqual("", coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfAsync_CoveredIndividuals_EmptySsn_Masked()
        {
            personContracts[0].Ssn = "";
            personContracts[1].Ssn = "";
            personContracts[2].Ssn = "";
            hrWebDefaults.Hrweb1095cMaskSsn = "Y";
            var pdfData = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the covered individual SSNs are masked.
            Assert.AreEqual(personContracts.Count, pdfData.CoveredIndividuals.Count);
            foreach (var coveredIndivdualEntity in pdfData.CoveredIndividuals)
            {
                Assert.AreEqual("", coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }
        #endregion

        [TestMethod]
        public async Task Form1095UnmaskedSsn()
        {
            hrWebDefaults.Hrweb1095cMaskSsn = "N";
            var actualDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, webW2OnlineId);
            Assert.AreEqual(personContract.Ssn, actualDomainEntity.EmployeeSsn);

            // Make sure the covered individual SSNs are NOT masked.
            foreach (var coveredIndivdualEntity in actualDomainEntity.CoveredIndividuals)
            {
                var selectedContract = personContracts.FirstOrDefault(x =>
                    x.FirstName == coveredIndivdualEntity.CoveredIndividualFirstName
                    && x.MiddleName == coveredIndivdualEntity.CoveredIndividualMiddleName
                    && x.LastName == coveredIndivdualEntity.CoveredIndividualLastName);

                Assert.IsNotNull(selectedContract);
                Assert.AreEqual(selectedContract.Ssn, coveredIndivdualEntity.CoveredIndividualSsn);
            }
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_Single_Digit_PlanStartMonth()
        {
            qtdYtdParameter1095CPDFContract.Qyp1095cPlanStartMonth = "2";
            var entity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.AreEqual("02", entity.PlanStartMonthCode);
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_FormIsCorrected()
        {
            taxForm1095cWhistContract.TfcwhStatus = "COR";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.IsTrue(form1095cDomainEntity.IsCorrected);
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_FormIsVoided()
        {
            taxForm1095cWhistContract.TfcwhVoidInd = "Y";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            Assert.IsTrue(form1095cDomainEntity.IsVoided);
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_WithNoDependentsAndPayrollModule_IncompletePhoneNumber()
        {
            qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone = "703555678";

            taxForm1095cWhistContract.TfcwhCoveredIndivInd = "";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Make sure the phone number is formatted correctly.
            Assert.AreEqual(form1095cDomainEntity.EmployerContactPhoneNumber, qtdYtdParameter1095CPDFContract.Qyp1095cContactPhone);
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_WithDependentsAndPayrollModule()
        {
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from PAYMSTR
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, paymasterContract.PmInstitutionEin);
            Assert.AreEqual(form1095cDomainEntity.EmployerName, paymasterContract.PmInstitutionName);
            Assert.AreEqual(form1095cDomainEntity.EmployerAddressLine, paymasterContract.PmInstitutionAddress.FirstOrDefault());
            Assert.AreEqual(form1095cDomainEntity.EmployerCityName, paymasterContract.PmInstitutionCity);
            Assert.AreEqual(form1095cDomainEntity.EmployerStateCode, paymasterContract.PmInstitutionState);
            Assert.AreEqual(form1095cDomainEntity.EmployerZipCode, paymasterContract.PmInstitutionZipcode);

            Assert.AreEqual(form1095cDomainEntity.TaxYear, taxForm1095cWhistContract.TfcwhTaxYear);
            Assert.AreEqual(form1095cDomainEntity.EmployeeSsn, "XXX-XX-" + personContract.Ssn.Substring(7));
            Assert.AreEqual(form1095cDomainEntity.EmployeeFirstName, taxForm1095cWhistContract.TfcwhFirstName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeLastName, taxForm1095cWhistContract.TfcwhLastName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeMiddleName, taxForm1095cWhistContract.TfcwhMiddleName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine1, taxForm1095cWhistContract.TfcwhAddressLine1Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine2, taxForm1095cWhistContract.TfcwhAddressLine2Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCityName, taxForm1095cWhistContract.TfcwhCityName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeStateCode, taxForm1095cWhistContract.TfcwhStateProvCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeePostalCode, taxForm1095cWhistContract.TfcwhPostalCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeeZipExtension, taxForm1095cWhistContract.TfcwhZipExtension);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCountry, taxForm1095cWhistContract.TfcwhCountryName);

            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverage12Month, taxForm1095cWhistContract.TfcwhOfferCode12mnth);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJanuary, taxForm1095cWhistContract.TfcwhOfferCodeJan);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageFebruary, taxForm1095cWhistContract.TfcwhOfferCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMarch, taxForm1095cWhistContract.TfcwhOfferCodeMar);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageApril, taxForm1095cWhistContract.TfcwhOfferCodeApr);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMay, taxForm1095cWhistContract.TfcwhOfferCodeMay);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJune, taxForm1095cWhistContract.TfcwhOfferCodeJun);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJuly, taxForm1095cWhistContract.TfcwhOfferCodeJul);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageAugust, taxForm1095cWhistContract.TfcwhOfferCodeAug);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageSeptember, taxForm1095cWhistContract.TfcwhOfferCodeSep);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageOctober, taxForm1095cWhistContract.TfcwhOfferCodeOct);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageNovember, taxForm1095cWhistContract.TfcwhOfferCodeNov);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageDecember, taxForm1095cWhistContract.TfcwhOfferCodeDec);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmount12Month, taxForm1095cWhistContract.TfcwhLowestCostAmt12mnth);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJanuary, taxForm1095cWhistContract.TfcwhLowestCostAmtJan);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountFebruary, taxForm1095cWhistContract.TfcwhLowestCostAmtFeb);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMarch, taxForm1095cWhistContract.TfcwhLowestCostAmtMar);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountApril, taxForm1095cWhistContract.TfcwhLowestCostAmtApr);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMay, taxForm1095cWhistContract.TfcwhLowestCostAmtMay);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJune, taxForm1095cWhistContract.TfcwhLowestCostAmtJun);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJuly, taxForm1095cWhistContract.TfcwhLowestCostAmtJul);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountAugust, taxForm1095cWhistContract.TfcwhLowestCostAmtAug);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountSeptember, taxForm1095cWhistContract.TfcwhLowestCostAmtSep);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountOctober, taxForm1095cWhistContract.TfcwhLowestCostAmtOct);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountNovember, taxForm1095cWhistContract.TfcwhLowestCostAmtNov);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountDecember, taxForm1095cWhistContract.TfcwhLowestCostAmtDec);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCode12Month, taxForm1095cWhistContract.TfcwhSafeHarborCd12mnth);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJanuary, taxForm1095cWhistContract.TfcwhSafeHarborCodeJan);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeFebruary, taxForm1095cWhistContract.TfcwhSafeHarborCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMarch, taxForm1095cWhistContract.TfcwhSafeHarborCodeMar);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeApril, taxForm1095cWhistContract.TfcwhSafeHarborCodeApr);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMay, taxForm1095cWhistContract.TfcwhSafeHarborCodeMay);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJune, taxForm1095cWhistContract.TfcwhSafeHarborCodeJun);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJuly, taxForm1095cWhistContract.TfcwhSafeHarborCodeJul);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeAugust, taxForm1095cWhistContract.TfcwhSafeHarborCodeAug);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeSeptember, taxForm1095cWhistContract.TfcwhSafeHarborCodeSep);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeOctober, taxForm1095cWhistContract.TfcwhSafeHarborCodeOct);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeNovember, taxForm1095cWhistContract.TfcwhSafeHarborCodeNov);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeDecember, taxForm1095cWhistContract.TfcwhSafeHarborCodeDec);
            Assert.IsTrue(form1095cDomainEntity.EmployeeIsSelfInsured);

            // Validate that the correct covered individuals data contracts have been
            // used to create a covered individual domain entity for the employee
            var selectedCoveredIndividualsContracts = taxForm1095cChistContracts.Where(x => x.Tfcch1095cId == taxForm1095cWhistContract.Recordkey).ToArray();
            // Validate that we have the correct number of covered individuals domain entities
            Assert.AreEqual(selectedCoveredIndividualsContracts.Count(), form1095cDomainEntity.CoveredIndividuals.Count());

            // Validate that the covered individual domain entity for the employee has the same names as the contract
            foreach (var cChistContract in selectedCoveredIndividualsContracts)
            {
                if (cChistContract.TfcchCoverageCode.ToUpper() == "S")
                {
                    foreach (var dependent in form1095cDomainEntity.CoveredIndividuals)
                    {
                        if (dependent.IsEmployeeItself == true)
                        {
                            // Get the person contract so we can compare the name...
                            var selectedPersonContract = personContracts.FirstOrDefault(x => x.Recordkey == cChistContract.TfcchPersonId);
                            if (selectedPersonContract != null)
                            {
                                Assert.AreEqual(selectedPersonContract.FirstName, dependent.CoveredIndividualFirstName);
                                Assert.AreEqual(selectedPersonContract.MiddleName, dependent.CoveredIndividualMiddleName);
                                Assert.AreEqual(selectedPersonContract.LastName, dependent.CoveredIndividualLastName);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_WithNoDependentsAndNoPayrollModule()
        {
            taxForm1095cWhistContract.TfcwhCoveredIndivInd = "";
            paymasterContract.PmInstitutionEin = null;

            // Mock the CTX to get the preferred name
            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0000043",
                OutPersonName = new List<string>() { "Host Organization Name" }
            };
            transactionInvoker.Setup<TxGetHierarchyNameResponse>(tx => tx.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>()))
                .Returns<TxGetHierarchyNameRequest>(request =>
                {
                    return this.hierarchyNameResponse;
                });

            // Mock the CTX to get the preferred address
            this.hierarchyAddressResponse = new TxGetHierarchyAddressResponse()
            {
                IoPersonId = "0000043",
                OutAddressLines = new List<string>() { "Host Organization address line1", "Host Organization address line2", "Host Organization address line3" },
                OutAddressCity = "Host Organization City",
                OutAddressState = "VA",
                OutAddressZip = "20498",
                OutAddressCountryDesc = "United States of America"
            };
            transactionInvoker.Setup<TxGetHierarchyAddressResponse>(tx => tx.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(It.IsAny<TxGetHierarchyAddressRequest>()))
                .Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    return this.hierarchyAddressResponse;
                });

            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from the default host organization
            var ein = corpFoundsContract.CorpTaxId.Insert(2, "-");
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, ein);
            // Add [0] to the second argument because the EmployerName picks off the first value in the array
            Assert.AreEqual(form1095cDomainEntity.EmployerName, hierarchyNameResponse.OutPersonName[0]);
            Assert.AreEqual(form1095cDomainEntity.EmployerAddressLine, hierarchyAddressResponse.OutAddressLines.FirstOrDefault());
            Assert.AreEqual(form1095cDomainEntity.EmployerCityName, hierarchyAddressResponse.OutAddressCity);
            Assert.AreEqual(form1095cDomainEntity.EmployerStateCode, hierarchyAddressResponse.OutAddressState);
            Assert.AreEqual(form1095cDomainEntity.EmployerZipCode, hierarchyAddressResponse.OutAddressZip);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCountry, hierarchyAddressResponse.OutAddressCountryDesc);

            Assert.AreEqual(form1095cDomainEntity.TaxYear, taxForm1095cWhistContract.TfcwhTaxYear);
            Assert.AreEqual(form1095cDomainEntity.EmployeeSsn, "XXX-XX-" + personContract.Ssn.Substring(7));
            Assert.AreEqual(form1095cDomainEntity.EmployeeFirstName, taxForm1095cWhistContract.TfcwhFirstName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeLastName, taxForm1095cWhistContract.TfcwhLastName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeMiddleName, taxForm1095cWhistContract.TfcwhMiddleName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine1, taxForm1095cWhistContract.TfcwhAddressLine1Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine2, taxForm1095cWhistContract.TfcwhAddressLine2Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCityName, taxForm1095cWhistContract.TfcwhCityName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeStateCode, taxForm1095cWhistContract.TfcwhStateProvCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeePostalCode, taxForm1095cWhistContract.TfcwhPostalCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeeZipExtension, taxForm1095cWhistContract.TfcwhZipExtension);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCountry, taxForm1095cWhistContract.TfcwhCountryName);

            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverage12Month, taxForm1095cWhistContract.TfcwhOfferCode12mnth);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJanuary, taxForm1095cWhistContract.TfcwhOfferCodeJan);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageFebruary, taxForm1095cWhistContract.TfcwhOfferCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMarch, taxForm1095cWhistContract.TfcwhOfferCodeMar);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageApril, taxForm1095cWhistContract.TfcwhOfferCodeApr);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMay, taxForm1095cWhistContract.TfcwhOfferCodeMay);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJune, taxForm1095cWhistContract.TfcwhOfferCodeJun);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJuly, taxForm1095cWhistContract.TfcwhOfferCodeJul);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageAugust, taxForm1095cWhistContract.TfcwhOfferCodeAug);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageSeptember, taxForm1095cWhistContract.TfcwhOfferCodeSep);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageOctober, taxForm1095cWhistContract.TfcwhOfferCodeOct);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageNovember, taxForm1095cWhistContract.TfcwhOfferCodeNov);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageDecember, taxForm1095cWhistContract.TfcwhOfferCodeDec);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmount12Month, taxForm1095cWhistContract.TfcwhLowestCostAmt12mnth);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJanuary, taxForm1095cWhistContract.TfcwhLowestCostAmtJan);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountFebruary, taxForm1095cWhistContract.TfcwhLowestCostAmtFeb);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMarch, taxForm1095cWhistContract.TfcwhLowestCostAmtMar);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountApril, taxForm1095cWhistContract.TfcwhLowestCostAmtApr);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMay, taxForm1095cWhistContract.TfcwhLowestCostAmtMay);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJune, taxForm1095cWhistContract.TfcwhLowestCostAmtJun);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJuly, taxForm1095cWhistContract.TfcwhLowestCostAmtJul);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountAugust, taxForm1095cWhistContract.TfcwhLowestCostAmtAug);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountSeptember, taxForm1095cWhistContract.TfcwhLowestCostAmtSep);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountOctober, taxForm1095cWhistContract.TfcwhLowestCostAmtOct);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountNovember, taxForm1095cWhistContract.TfcwhLowestCostAmtNov);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountDecember, taxForm1095cWhistContract.TfcwhLowestCostAmtDec);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCode12Month, taxForm1095cWhistContract.TfcwhSafeHarborCd12mnth);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJanuary, taxForm1095cWhistContract.TfcwhSafeHarborCodeJan);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeFebruary, taxForm1095cWhistContract.TfcwhSafeHarborCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMarch, taxForm1095cWhistContract.TfcwhSafeHarborCodeMar);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeApril, taxForm1095cWhistContract.TfcwhSafeHarborCodeApr);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMay, taxForm1095cWhistContract.TfcwhSafeHarborCodeMay);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJune, taxForm1095cWhistContract.TfcwhSafeHarborCodeJun);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJuly, taxForm1095cWhistContract.TfcwhSafeHarborCodeJul);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeAugust, taxForm1095cWhistContract.TfcwhSafeHarborCodeAug);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeSeptember, taxForm1095cWhistContract.TfcwhSafeHarborCodeSep);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeOctober, taxForm1095cWhistContract.TfcwhSafeHarborCodeOct);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeNovember, taxForm1095cWhistContract.TfcwhSafeHarborCodeNov);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeDecember, taxForm1095cWhistContract.TfcwhSafeHarborCodeDec);
            // EmployeeIsSelfInsured is a boolean
            Assert.AreEqual(form1095cDomainEntity.EmployeeIsSelfInsured, taxForm1095cWhistContract.TfcwhCoveredIndivInd != "");
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_WithNoDependentsAndNoPayrollModule_EinJustLongEnough()
        {
            taxForm1095cWhistContract.TfcwhCoveredIndivInd = "";
            paymasterContract.PmInstitutionEin = null;

            // Mock the CTX to get the preferred name
            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0000043",
                OutPersonName = new List<string>() { "Host Organization Name" }
            };
            transactionInvoker.Setup<TxGetHierarchyNameResponse>(tx => tx.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>()))
                .Returns<TxGetHierarchyNameRequest>(request =>
                {
                    return this.hierarchyNameResponse;
                });

            // Mock the CTX to get the preferred address
            this.hierarchyAddressResponse = new TxGetHierarchyAddressResponse()
            {
                IoPersonId = "0000043",
                OutAddressLines = new List<string>() { "Host Organization address line1", "Host Organization address line2", "Host Organization address line3" },
                OutAddressCity = "Host Organization City",
                OutAddressState = "VA",
                OutAddressZip = "20498",
                OutAddressCountryDesc = "United States of America"
            };
            transactionInvoker.Setup<TxGetHierarchyAddressResponse>(tx => tx.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(It.IsAny<TxGetHierarchyAddressRequest>()))
                .Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    return this.hierarchyAddressResponse;
                });

            corpFoundsContract.CorpTaxId = "266";

            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from the default host organization
            var ein = corpFoundsContract.CorpTaxId.Insert(2, "-");
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, ein);
        }

        [TestMethod]
        public async Task Get1095cPdfData_Success_WithNoDependentsAndNoPayrollModule_EinTooShort()
        {
            taxForm1095cWhistContract.TfcwhCoveredIndivInd = "";
            paymasterContract.PmInstitutionEin = null;

            // Mock the CTX to get the preferred name
            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0000043",
                OutPersonName = new List<string>() { "Host Organization Name" }
            };
            transactionInvoker.Setup<TxGetHierarchyNameResponse>(tx => tx.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>()))
                .Returns<TxGetHierarchyNameRequest>(request =>
                {
                    return this.hierarchyNameResponse;
                });

            // Mock the CTX to get the preferred address
            this.hierarchyAddressResponse = new TxGetHierarchyAddressResponse()
            {
                IoPersonId = "0000043",
                OutAddressLines = new List<string>() { "Host Organization address line1", "Host Organization address line2", "Host Organization address line3" },
                OutAddressCity = "Host Organization City",
                OutAddressState = "VA",
                OutAddressZip = "20498",
                OutAddressCountryDesc = "United States of America"
            };
            transactionInvoker.Setup<TxGetHierarchyAddressResponse>(tx => tx.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(It.IsAny<TxGetHierarchyAddressRequest>()))
                .Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    return this.hierarchyAddressResponse;
                });

            corpFoundsContract.CorpTaxId = "26";

            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from the default host organization
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, corpFoundsContract.CorpTaxId);
        }
        
        [TestMethod]
        public async Task Get1095cPdfData_Success_WithDependentsAndNoPayrollModule()
        {
            paymasterContract = null;

            // // Mock the CTX to get the preferred name
            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0000043",
                OutPersonName = new List<string>() { "Host Organization Name" }
            };
            transactionInvoker.Setup<TxGetHierarchyNameResponse>(tx => tx.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>()))
                .Returns<TxGetHierarchyNameRequest>(request =>
                {
                    return this.hierarchyNameResponse;
                });

            // // Mock the CTX to get the preferred address
            this.hierarchyAddressResponse = new TxGetHierarchyAddressResponse()
            {
                IoPersonId = "0000043",
                OutAddressLines = new List<string>() { "Host Organization address line1", "Host Organization address line2", "Host Organization address line3" },
                OutAddressCity = "Host Organization City",
                OutAddressState = "VA",
                OutAddressZip = "20498",
                OutAddressCountryDesc = "United States of America"
            };
            transactionInvoker.Setup<TxGetHierarchyAddressResponse>(tx => tx.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(It.IsAny<TxGetHierarchyAddressRequest>()))
                .Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    return this.hierarchyAddressResponse;
                });

            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");

            // Get the institution name and address from the default host organization
            var ein = corpFoundsContract.CorpTaxId.Insert(2, "-");
            Assert.AreEqual(form1095cDomainEntity.EmployerEin, ein);
            Assert.AreEqual(form1095cDomainEntity.EmployerName, hierarchyNameResponse.OutPersonName[0]);
            Assert.AreEqual(form1095cDomainEntity.EmployerAddressLine, hierarchyAddressResponse.OutAddressLines.FirstOrDefault());
            Assert.AreEqual(form1095cDomainEntity.EmployerCityName, hierarchyAddressResponse.OutAddressCity);
            Assert.AreEqual(form1095cDomainEntity.EmployerStateCode, hierarchyAddressResponse.OutAddressState);
            Assert.AreEqual(form1095cDomainEntity.EmployerZipCode, hierarchyAddressResponse.OutAddressZip);

            Assert.AreEqual(form1095cDomainEntity.TaxYear, taxForm1095cWhistContract.TfcwhTaxYear);
            Assert.AreEqual(form1095cDomainEntity.EmployeeSsn, "XXX-XX-" + personContract.Ssn.Substring(7));
            Assert.AreEqual(form1095cDomainEntity.EmployeeFirstName, taxForm1095cWhistContract.TfcwhFirstName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeLastName, taxForm1095cWhistContract.TfcwhLastName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeMiddleName, taxForm1095cWhistContract.TfcwhMiddleName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine1, taxForm1095cWhistContract.TfcwhAddressLine1Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeAddressLine2, taxForm1095cWhistContract.TfcwhAddressLine2Text);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCityName, taxForm1095cWhistContract.TfcwhCityName);
            Assert.AreEqual(form1095cDomainEntity.EmployeeStateCode, taxForm1095cWhistContract.TfcwhStateProvCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeePostalCode, taxForm1095cWhistContract.TfcwhPostalCode);
            Assert.AreEqual(form1095cDomainEntity.EmployeeZipExtension, taxForm1095cWhistContract.TfcwhZipExtension);
            Assert.AreEqual(form1095cDomainEntity.EmployeeCountry, taxForm1095cWhistContract.TfcwhCountryName);

            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverage12Month, taxForm1095cWhistContract.TfcwhOfferCode12mnth);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJanuary, taxForm1095cWhistContract.TfcwhOfferCodeJan);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageFebruary, taxForm1095cWhistContract.TfcwhOfferCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMarch, taxForm1095cWhistContract.TfcwhOfferCodeMar);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageApril, taxForm1095cWhistContract.TfcwhOfferCodeApr);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageMay, taxForm1095cWhistContract.TfcwhOfferCodeMay);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJune, taxForm1095cWhistContract.TfcwhOfferCodeJun);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageJuly, taxForm1095cWhistContract.TfcwhOfferCodeJul);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageAugust, taxForm1095cWhistContract.TfcwhOfferCodeAug);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageSeptember, taxForm1095cWhistContract.TfcwhOfferCodeSep);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageOctober, taxForm1095cWhistContract.TfcwhOfferCodeOct);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageNovember, taxForm1095cWhistContract.TfcwhOfferCodeNov);
            Assert.AreEqual(form1095cDomainEntity.OfferOfCoverageDecember, taxForm1095cWhistContract.TfcwhOfferCodeDec);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmount12Month, taxForm1095cWhistContract.TfcwhLowestCostAmt12mnth);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJanuary, taxForm1095cWhistContract.TfcwhLowestCostAmtJan);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountFebruary, taxForm1095cWhistContract.TfcwhLowestCostAmtFeb);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMarch, taxForm1095cWhistContract.TfcwhLowestCostAmtMar);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountApril, taxForm1095cWhistContract.TfcwhLowestCostAmtApr);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountMay, taxForm1095cWhistContract.TfcwhLowestCostAmtMay);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJune, taxForm1095cWhistContract.TfcwhLowestCostAmtJun);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountJuly, taxForm1095cWhistContract.TfcwhLowestCostAmtJul);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountAugust, taxForm1095cWhistContract.TfcwhLowestCostAmtAug);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountSeptember, taxForm1095cWhistContract.TfcwhLowestCostAmtSep);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountOctober, taxForm1095cWhistContract.TfcwhLowestCostAmtOct);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountNovember, taxForm1095cWhistContract.TfcwhLowestCostAmtNov);
            Assert.AreEqual(form1095cDomainEntity.LowestCostAmountDecember, taxForm1095cWhistContract.TfcwhLowestCostAmtDec);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCode12Month, taxForm1095cWhistContract.TfcwhSafeHarborCd12mnth);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJanuary, taxForm1095cWhistContract.TfcwhSafeHarborCodeJan);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeFebruary, taxForm1095cWhistContract.TfcwhSafeHarborCodeFeb);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMarch, taxForm1095cWhistContract.TfcwhSafeHarborCodeMar);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeApril, taxForm1095cWhistContract.TfcwhSafeHarborCodeApr);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeMay, taxForm1095cWhistContract.TfcwhSafeHarborCodeMay);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJune, taxForm1095cWhistContract.TfcwhSafeHarborCodeJun);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeJuly, taxForm1095cWhistContract.TfcwhSafeHarborCodeJul);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeAugust, taxForm1095cWhistContract.TfcwhSafeHarborCodeAug);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeSeptember, taxForm1095cWhistContract.TfcwhSafeHarborCodeSep);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeOctober, taxForm1095cWhistContract.TfcwhSafeHarborCodeOct);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeNovember, taxForm1095cWhistContract.TfcwhSafeHarborCodeNov);
            Assert.AreEqual(form1095cDomainEntity.SafeHarborCodeDecember, taxForm1095cWhistContract.TfcwhSafeHarborCodeDec);
            Assert.IsTrue(form1095cDomainEntity.EmployeeIsSelfInsured);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cPdfData_NullId()
        {
            await pdfDataRepository.Get1095cPdfAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cPdfData_EmptyId()
        {
            await pdfDataRepository.Get1095cPdfAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cPdfData_NullPersonId()
        {
            await pdfDataRepository.Get1095cPdfAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cPdfData_EmptyPersonId()
        {
            await pdfDataRepository.Get1095cPdfAsync(string.Empty, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_DataReaderSelectReturnsNull()
        {
            pdfIds = null;
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_DataReaderReturnsZero1095CIds()
        {
            pdfIds = new string[] { };
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_DataReaderReturnsMultipleW1095CIds()
        {
            pdfIds = new string[] { "1", "2" };
            await pdfDataRepository.GetW2PdfAsync(personId, webW2OnlineId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_Null1095cDataContract()
        {
            taxForm1095cWhistContract = null;
            await pdfDataRepository.Get1095cPdfAsync(personId, "99");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_NullTaxYear()
        {
            this.taxForm1095cWhistContract.TfcwhTaxYear = null;
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_EmptyTaxYear()
        {
            this.taxForm1095cWhistContract.TfcwhTaxYear = "";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_NullEmployeeId()
        {
            this.taxForm1095cWhistContract.TfcwhHrperId = null;
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1095cPdfData_EmptyEmployeeId()
        {
            this.taxForm1095cWhistContract.TfcwhHrperId = "";
            var form1095cDomainEntity = await pdfDataRepository.Get1095cPdfAsync(personId, "99");
        }
        #endregion

        #region T4

        [TestMethod]
        public async Task GetT4PdfData_Success()
        {
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1");

            Assert.AreEqual(webT4OnlineDataContract.Wt4oYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oInitial, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oSurname.ToUpper(), actualDomainEntity.EmployeeLastName);
            var expectedSIN = webT4OnlineDataContract.Wt4oSin.Substring(0, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(3, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(6); ;
            Assert.AreEqual(expectedSIN, actualDomainEntity.SocialInsuranceNumber);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentCode, actualDomainEntity.EmploymentCode);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentProvince, actualDomainEntity.ProvinceOfEmployment);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oAddr1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oAddr2, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oCity + ", " + webT4OnlineDataContract.Wt4oProvinceCode + " " + webT4OnlineDataContract.Wt4oPostalCode, actualDomainEntity.EmployeeAddressLine3);
            Assert.AreEqual("United States of America", actualDomainEntity.EmployeeAddressLine4);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName2, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr1, actualDomainEntity.EmployerAddressLine3);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr2, actualDomainEntity.EmployerAddressLine4);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerCity + ", " + webT4OnlineDataContract.Wt4oPayerProvCode + " " + webT4OnlineDataContract.Wt4oPayerPostalCode, actualDomainEntity.EmployerAddressLine5);

            // Box data

            Assert.AreEqual("10.00", actualDomainEntity.EmploymentIncome);
            Assert.AreEqual("10.00", actualDomainEntity.EmployeesCPPContributions);
            Assert.AreEqual("10.00", actualDomainEntity.EmployeesQPPContributions);
            Assert.AreEqual("10.00", actualDomainEntity.EmployeesEIPremiums);
            Assert.AreEqual("10.00", actualDomainEntity.RPPContributions);
            Assert.AreEqual("10.00", actualDomainEntity.IncomeTaxDeducted);
            Assert.AreEqual("10.00", actualDomainEntity.EIInsurableEarnings);
            Assert.AreEqual("10.00", actualDomainEntity.CPPQPPPensionableEarnings);
            Assert.AreEqual("10.00", actualDomainEntity.UnionDues);
            Assert.AreEqual("10.00", actualDomainEntity.CharitableDonations);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPensionRgstNo.FirstOrDefault(), actualDomainEntity.RPPorDPSPRegistrationNumber);
            Assert.AreEqual("10.00", actualDomainEntity.PensionAdjustment);
            Assert.AreEqual("10.00", actualDomainEntity.EmployeesPPIPPremiums);
            Assert.AreEqual("10.00", actualDomainEntity.PPIPInsurableEarnings);
        }

        [TestMethod]
        public async Task GetT4PdfData_SuccessWithMissingData()
        {
            webT4OnlineDataContract.Wt4oInitial = null;
            webT4OnlineDataContract.Wt4oAddr2 = null;
            webT4OnlineDataContract.Wt4oCity = null;
            webT4OnlineDataContract.Wt4oProvinceCode = null;
            webT4OnlineDataContract.Wt4oPostalCode = null;
            webT4OnlineDataContract.Wt4oPensionRgstNo = new List<string>();

            caDesc = null;
            usaDesc = null;

            webT4OnlineDataContract.Wt4oPayerName2 = null;
            webT4OnlineDataContract.Wt4oPayerAddr2 = null;
            webT4OnlineDataContract.Wt4oPayerCity = null;
            webT4OnlineDataContract.Wt4oPayerProvCode = null;
            webT4OnlineDataContract.Wt4oPayerPostalCode = null;

            foreach(var association in webT4OnlineDataContract.T4BoxInformationEntityAssociation)
            {
                association.Wt4oBoxFnAmtAssocMember = null;
            }
            webT4OnlineDataContract.T4BoxInformationEntityAssociation[0] = null;

            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1-0");

            Assert.AreEqual(webT4OnlineDataContract.Wt4oYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oSurname.ToUpper(), actualDomainEntity.EmployeeLastName);
            var expectedSIN = webT4OnlineDataContract.Wt4oSin.Substring(0, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(3, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(6); ;
            Assert.AreEqual(expectedSIN, actualDomainEntity.SocialInsuranceNumber);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentCode, actualDomainEntity.EmploymentCode);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentProvince, actualDomainEntity.ProvinceOfEmployment);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oAddr1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(string.Empty + ", " + string.Empty + " " + string.Empty, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeAddressLine3);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr1, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(string.Empty + ", " + string.Empty + " " + string.Empty, actualDomainEntity.EmployerAddressLine3);

            // Box data

            Assert.AreEqual("0.00", actualDomainEntity.EmploymentIncome);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesCPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesQPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesEIPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.RPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.IncomeTaxDeducted);
            Assert.AreEqual("0.00", actualDomainEntity.EIInsurableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.CPPQPPPensionableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.UnionDues);
            Assert.AreEqual("0.00", actualDomainEntity.CharitableDonations);
            Assert.AreEqual(null, actualDomainEntity.RPPorDPSPRegistrationNumber);
            Assert.AreEqual("0.00", actualDomainEntity.PensionAdjustment);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesPPIPPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.PPIPInsurableEarnings);
        }

        [TestMethod]
        public async Task GetT4PdfData_SuccessWithMissingData2()
        {
            webT4OnlineDataContract.Wt4oInitial = null;
            webT4OnlineDataContract.Wt4oAddr2 = null;
            webT4OnlineDataContract.Wt4oCity = null;
            webT4OnlineDataContract.Wt4oProvinceCode = null;
            webT4OnlineDataContract.Wt4oPostalCode = null;
            webT4OnlineDataContract.Wt4oPensionRgstNo = new List<string>();

            caDesc = null;
            usaDesc = null;

            webT4OnlineDataContract.Wt4oPayerName2 = null;
            webT4OnlineDataContract.Wt4oPayerCity = null;
            webT4OnlineDataContract.Wt4oPayerProvCode = null;
            webT4OnlineDataContract.Wt4oPayerPostalCode = null;

            foreach (var association in webT4OnlineDataContract.T4BoxInformationEntityAssociation)
            {
                association.Wt4oBoxFnAmtAssocMember = null;
            }
            webT4OnlineDataContract.T4BoxInformationEntityAssociation[0] = null;

            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1-0");

            Assert.AreEqual(webT4OnlineDataContract.Wt4oYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oSurname.ToUpper(), actualDomainEntity.EmployeeLastName);
            var expectedSIN = webT4OnlineDataContract.Wt4oSin.Substring(0, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(3, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(6); ;
            Assert.AreEqual(expectedSIN, actualDomainEntity.SocialInsuranceNumber);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentCode, actualDomainEntity.EmploymentCode);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentProvince, actualDomainEntity.ProvinceOfEmployment);


            Assert.AreEqual(webT4OnlineDataContract.Wt4oAddr1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oCity + ", " + webT4OnlineDataContract.Wt4oProvinceCode + " " + webT4OnlineDataContract.Wt4oPostalCode, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeAddressLine3);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr1, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr2, actualDomainEntity.EmployerAddressLine3);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerCity + ", " + webT4OnlineDataContract.Wt4oPayerProvCode + " " + webT4OnlineDataContract.Wt4oPayerPostalCode, actualDomainEntity.EmployerAddressLine4);
            
            // Box data

            Assert.AreEqual("0.00", actualDomainEntity.EmploymentIncome);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesCPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesQPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesEIPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.RPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.IncomeTaxDeducted);
            Assert.AreEqual("0.00", actualDomainEntity.EIInsurableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.CPPQPPPensionableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.UnionDues);
            Assert.AreEqual("0.00", actualDomainEntity.CharitableDonations);
            Assert.AreEqual(null, actualDomainEntity.RPPorDPSPRegistrationNumber);
            Assert.AreEqual("0.00", actualDomainEntity.PensionAdjustment);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesPPIPPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.PPIPInsurableEarnings);
        }

        [TestMethod]
        public async Task GetT4PdfData_SuccessWithMissingData3()
        {
            webT4OnlineDataContract.Wt4oInitial = null;
            webT4OnlineDataContract.Wt4oAddr2 = null;
            webT4OnlineDataContract.Wt4oCity = null;
            webT4OnlineDataContract.Wt4oProvinceCode = null;
            webT4OnlineDataContract.Wt4oPostalCode = null;
            webT4OnlineDataContract.Wt4oPensionRgstNo = new List<string>();

            caDesc = null;
            usaDesc = null;
            
            webT4OnlineDataContract.Wt4oPayerAddr2 = null;
            webT4OnlineDataContract.Wt4oPayerCity = null;
            webT4OnlineDataContract.Wt4oPayerProvCode = null;
            webT4OnlineDataContract.Wt4oPayerPostalCode = null;

            foreach (var association in webT4OnlineDataContract.T4BoxInformationEntityAssociation)
            {
                association.Wt4oBoxFnAmtAssocMember = null;
            }
            webT4OnlineDataContract.T4BoxInformationEntityAssociation[0] = null;

            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1-0");

            Assert.AreEqual(webT4OnlineDataContract.Wt4oYear, actualDomainEntity.TaxYear);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oFirstName, actualDomainEntity.EmployeeFirstName);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeMiddleName);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oSurname.ToUpper(), actualDomainEntity.EmployeeLastName);
            var expectedSIN = webT4OnlineDataContract.Wt4oSin.Substring(0, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(3, 3) + " " + webT4OnlineDataContract.Wt4oSin.Substring(6); ;
            Assert.AreEqual(expectedSIN, actualDomainEntity.SocialInsuranceNumber);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentCode, actualDomainEntity.EmploymentCode);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oEmploymentProvince, actualDomainEntity.ProvinceOfEmployment);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oAddr1, actualDomainEntity.EmployeeAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oCity + ", " + webT4OnlineDataContract.Wt4oProvinceCode + " " + webT4OnlineDataContract.Wt4oPostalCode, actualDomainEntity.EmployeeAddressLine2);
            Assert.AreEqual(string.Empty, actualDomainEntity.EmployeeAddressLine3);

            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName1, actualDomainEntity.EmployerAddressLine1);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerName2, actualDomainEntity.EmployerAddressLine2);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerAddr1, actualDomainEntity.EmployerAddressLine3);
            Assert.AreEqual(webT4OnlineDataContract.Wt4oPayerCity + ", " + webT4OnlineDataContract.Wt4oPayerProvCode + " " + webT4OnlineDataContract.Wt4oPayerPostalCode, actualDomainEntity.EmployerAddressLine4);


            // Box data

            Assert.AreEqual("0.00", actualDomainEntity.EmploymentIncome);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesCPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesQPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesEIPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.RPPContributions);
            Assert.AreEqual("0.00", actualDomainEntity.IncomeTaxDeducted);
            Assert.AreEqual("0.00", actualDomainEntity.EIInsurableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.CPPQPPPensionableEarnings);
            Assert.AreEqual("0.00", actualDomainEntity.UnionDues);
            Assert.AreEqual("0.00", actualDomainEntity.CharitableDonations);
            Assert.AreEqual(null, actualDomainEntity.RPPorDPSPRegistrationNumber);
            Assert.AreEqual("0.00", actualDomainEntity.PensionAdjustment);
            Assert.AreEqual("0.00", actualDomainEntity.EmployeesPPIPPremiums);
            Assert.AreEqual("0.00", actualDomainEntity.PPIPInsurableEarnings);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task GetT4PdfData_NullPersonId()
        {
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT4PdfData_NullRecordId()
        {
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetT4PdfData_NullPdfIds()
        {
            pdfIds = null;
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetT4PdfData_TooManyPdfIds()
        {
            pdfIds = new string[] { "1", "2" };
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetT4PdfData_NoPdfIds()
        {
            pdfIds = new string[0];
            var actualDomainEntity = await pdfDataRepository.GetT4PdfAsync(personId, "1");
        }

        #endregion

        private HumanResourcesTaxFormPdfDataRepository BuildRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new HumanResourcesTaxFormPdfDataRepository(cacheProviderMock.Object, transactionFactoryObject, loggerObject);
        }
    }
}