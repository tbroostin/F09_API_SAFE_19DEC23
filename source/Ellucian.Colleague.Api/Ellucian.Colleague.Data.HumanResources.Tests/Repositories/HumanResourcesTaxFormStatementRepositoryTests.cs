// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class HumanResourcesTaxFormStatementRepositoryTests
    {
        #region Initialize and Cleanup

        private ApiSettings apiSettings = new ApiSettings("API") { ColleagueTimeZone = "Eastern Standard Time" };
        private Mock<IColleagueDataReader> dataReaderMock = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private HumanResourcesTaxFormStatementRepository taxFormStatementRepository;
        private static string personId = "0000001";
        private Collection<WebW2Online> webW2OnlineDataContracts;
        private Collection<TaxForm1095cWhist> taxForm1095CWhistDataContracts;
        private Collection<WebT4Online> webT4OnlineDataContracts;

        [TestInitialize]
        public void Initialize()
        {
            dataReaderMock = new Mock<IColleagueDataReader>();
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();
            taxFormStatementRepository = BuildTaxFormStatementRepository();

            webW2OnlineDataContracts = new Collection<WebW2Online>()
            {
                new WebW2Online() { Recordkey = "1", Ww2oEmployeeId = personId, Ww2oYear = "2011" },
                new WebW2Online() { Recordkey = "2", Ww2oEmployeeId = personId, Ww2oYear = "2012" },
                new WebW2Online() { Recordkey = "3", Ww2oEmployeeId = personId, Ww2oYear = "2013" },
                new WebW2Online() { Recordkey = "4", Ww2oEmployeeId = personId, Ww2oYear = "2014" },
            };
            dataReaderMock.Setup<Task<Collection<WebW2Online>>>(dr => dr.BulkReadRecordAsync<WebW2Online>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(webW2OnlineDataContracts);
            });

            taxForm1095CWhistDataContracts = new Collection<TaxForm1095cWhist>()
            {
                new TaxForm1095cWhist() { Recordkey = "1", TfcwhHrperId = personId, TfcwhTaxYear = "2011", TfcwhStatus = "SUB", TaxForm1095cWhistAdddate = new DateTime(2012,01,02), TaxForm1095cWhistAddtime = new DateTime(1,1,1,12, 35, 20) },
                new TaxForm1095cWhist() { Recordkey = "2", TfcwhHrperId = personId, TfcwhTaxYear = "2012", TfcwhStatus = "COR", TaxForm1095cWhistAdddate = new DateTime(2013,01,03), TaxForm1095cWhistAddtime = new DateTime(1,1,1,12, 35, 20) },
                new TaxForm1095cWhist() { Recordkey = "3", TfcwhHrperId = personId, TfcwhTaxYear = "2013", TfcwhStatus = "SUB", TaxForm1095cWhistAdddate = new DateTime(2014,01,04), TaxForm1095cWhistAddtime = new DateTime(1,1,1,12, 35, 20) },
                new TaxForm1095cWhist() { Recordkey = "4", TfcwhHrperId = personId, TfcwhTaxYear = "2014", TfcwhStatus = "FRO", TaxForm1095cWhistAdddate = new DateTime(2015,01,05), TaxForm1095cWhistAddtime = new DateTime(1,1,1,12, 35, 20) },
            };
            dataReaderMock.Setup<Task<Collection<TaxForm1095cWhist>>>(dr => dr.BulkReadRecordAsync<TaxForm1095cWhist>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1095CWhistDataContracts);
            });

            webT4OnlineDataContracts = new Collection<WebT4Online>()
            {
                new WebT4Online() { Recordkey = "1", Wt4oEmployeeId = personId, Wt4oYear = "2011" },
                new WebT4Online() { Recordkey = "2", Wt4oEmployeeId = personId, Wt4oYear = "2012" },
                new WebT4Online() { Recordkey = "3", Wt4oEmployeeId = personId, Wt4oYear = "2013" },
                new WebT4Online() { Recordkey = "4", Wt4oEmployeeId = personId, Wt4oYear = "2014" },
            };
            dataReaderMock.Setup<Task<Collection<WebT4Online>>>(dr => dr.BulkReadRecordAsync<WebT4Online>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(webT4OnlineDataContracts);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReaderMock = null;
            webW2OnlineDataContracts = null;
            taxForm1095CWhistDataContracts = null;
            webT4OnlineDataContracts = null;
            transactionInvoker = null;
            taxFormStatementRepository = null;
        }

        #endregion

        #region W-2
        [TestMethod]
        public async Task StatementRepository_Success_W2()
        {
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in webW2OnlineDataContracts)
            {
                var expectedPersonId = dataContract.Ww2oEmployeeId;
                var expectedTaxYear = dataContract.Ww2oYear;

                var selectedStatementEntities = statementEntities.Where(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxYear == expectedTaxYear
                    && x.TaxForm == TaxForms.FormW2);
                Assert.AreEqual(1, selectedStatementEntities.Count(), "Each data contract should be represented in the set of domain entities.");
            }
        }


        [TestMethod]
        public async Task NullEmployeeId_W2()
        {
            this.webW2OnlineDataContracts[0].Ww2oEmployeeId = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyEmployeeId_W2()
        {
            this.webW2OnlineDataContracts[0].Ww2oEmployeeId = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task NullTaxFormYear_W2()
        {
            this.webW2OnlineDataContracts[0].Ww2oYear = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyTaxFormYear_W2()
        {
            this.webW2OnlineDataContracts[0].Ww2oYear = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task InvalidDataContractData_W2()
        {
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "99", Ww2oEmployeeId = null, Ww2oYear = "2012" });
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var entity in statementEntities)
            {
                var expectedPersonId = entity.PersonId;
                var expectedTaxYear = entity.TaxYear;

                var selectedDataContracts = this.webW2OnlineDataContracts.Where(x =>
                    x.Ww2oEmployeeId == expectedPersonId
                    && x.Ww2oYear == expectedTaxYear);
                Assert.AreEqual(1, selectedDataContracts.Count(), "Each domain entity should be represented in the data contracts list.");
            }
        }

        [TestMethod]
        public async Task NullDataContract_W2()
        {
            this.webW2OnlineDataContracts.Add(null);
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webW2OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in this.webW2OnlineDataContracts)
            {
                if (dataContract != null)
                {
                    var contractPersonId = dataContract.Ww2oEmployeeId;
                    var taxYear = dataContract.Ww2oYear;

                    var selectedEntities = statementEntities.Where(x =>
                        x.PersonId == contractPersonId
                        && x.TaxYear == taxYear);
                    Assert.AreEqual(1, selectedEntities.Count(), "Each domain entity should be represented in the data contracts list.");
                }
            }
        }

        [TestMethod]
        public async Task MultipleFormsForOneYearButCorrectionIssued_W2()
        {
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "5", Ww2oEmployeeId = personId, Ww2oYear = "2012" });
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "6", Ww2oEmployeeId = personId, Ww2oYear = "2012", Ww2oW2cFlag = "Y" });

            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure the number of entities returned matches the data contract set after it's been reduced to a unique set of years.
            Assert.AreEqual(this.webW2OnlineDataContracts.GroupBy(x => x.Ww2oYear).Select(x => x.First()).Count(), statementEntities.Count());

            // Make sure there's only one entity per year.
            foreach (var contract in this.webW2OnlineDataContracts)
            {
                // Also make sure that the W2 marked as the correction is the only one in the list for the given year.
                if (!string.IsNullOrEmpty(contract.Ww2oW2cFlag))
                {
                    var statementsForGivenYearWithCorrections = statementEntities.Where(x => x.TaxYear == contract.Ww2oYear && x.Notation == TaxFormNotations.Correction).ToList();
                    Assert.AreEqual(1, statementsForGivenYearWithCorrections.Count);
                }
                else
                {
                    if (contract.Ww2oYear != "2012")
                    {
                        var statementsForGivenYearWithoutNotation = statementEntities.Where(x => x.TaxYear == contract.Ww2oYear && x.Notation == TaxFormNotations.None).ToList();
                        Assert.AreEqual(1, statementsForGivenYearWithoutNotation.Count);
                    }
                }
            }
        }

        [TestMethod]
        public async Task MultipleFormsForOneYearNoCorrections_W2()
        {
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "5", Ww2oEmployeeId = personId, Ww2oYear = "2012" });
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "6", Ww2oEmployeeId = personId, Ww2oYear = "2012" });

            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure the number of entities returned matches the data contract set after it's been reduced to a unique set of years.
            Assert.AreEqual(this.webW2OnlineDataContracts.Count(), statementEntities.Count());

            // Make sure there's only one entity per year.
            foreach (var contract in this.webW2OnlineDataContracts)
            {
                // Duplicate forms should have the 'MultipleForms' notation, otherwise 'None'
                var formsForGivenYear = statementEntities.Where(x => x.TaxYear == contract.Ww2oYear).Count();
                if (formsForGivenYear > 1)
                {
                    var formsForGivenYearMarkedAsDuplicate = statementEntities.Where(x => x.TaxYear == contract.Ww2oYear && x.Notation == TaxFormNotations.MultipleForms).Count();
                    Assert.AreEqual(formsForGivenYear, formsForGivenYearMarkedAsDuplicate);
                }
                else
                {
                    Assert.AreEqual(1, statementEntities.Where(x => x.TaxYear == contract.Ww2oYear && x.Notation == TaxFormNotations.None).Count());
                }
            }
        }

        [TestMethod]
        public async Task MultipleFormsForOneYear_TwoFormsHaveOverflowData_W2()
        {
            // Set overflow data fields for two forms
            this.webW2OnlineDataContracts[3].Ww2oCodeBoxCodeE = "MD";
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "5", Ww2oEmployeeId = personId, Ww2oYear = "2012", Ww2oStateCodeC = "NY" });
            this.webW2OnlineDataContracts.Add(new WebW2Online() { Recordkey = "6", Ww2oEmployeeId = personId, Ww2oYear = "2012" });

            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);

            // Make sure the number of entities returned matches the data contract set after it's been reduced to a unique set of years.
            Assert.AreEqual(this.webW2OnlineDataContracts.Count() + 2, statementEntities.Count());

            // Make sure there's only one entity per year.
            foreach (var contract in this.webW2OnlineDataContracts)
            {
                // Make sure the correct number of forms have been marked appropriately.
                var formsForGivenYear = statementEntities.Where(x => x.TaxYear == contract.Ww2oYear).Count();
                if (formsForGivenYear > 1)
                {
                    var hasOverflowCount = statementEntities.Where(x =>
                        x.TaxYear == contract.Ww2oYear
                        && x.Notation == TaxFormNotations.HasOverflowData).Count();
                    var isOverflowCount = statementEntities.Where(x =>
                        x.TaxYear == contract.Ww2oYear
                        && x.Notation == TaxFormNotations.IsOverflow).Count();
                    var multipleFormsCount = statementEntities.Where(x =>
                        x.TaxYear == contract.Ww2oYear
                        && x.Notation == TaxFormNotations.MultipleForms).Count();
                    Assert.AreEqual(1, hasOverflowCount);
                    Assert.AreEqual(1, isOverflowCount);
                    Assert.AreEqual(formsForGivenYear, hasOverflowCount + isOverflowCount + multipleFormsCount);
                }
                else
                {
                    Assert.AreEqual(1, statementEntities.Where(x => x.TaxYear == contract.Ww2oYear && x.Notation == TaxFormNotations.None).Count());
                }
            }
        }

        [TestMethod]
        public async Task WebW2OnlineBulkReadReturnsNull_W2()
        {
            webW2OnlineDataContracts = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormW2);
            Assert.AreEqual(0, statementEntities.Count());
        }

        #region Tests valid for all tax forms using W2 information

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullPersonId()
        {
            await taxFormStatementRepository.GetAsync(null, TaxForms.FormW2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmptyPersonId()
        {
            await taxFormStatementRepository.GetAsync(string.Empty, TaxForms.FormW2);
        }

        #endregion

        #endregion

        #region 1095-C

        [TestMethod]
        public async Task StatementRepository_Success_1095C()
        {
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in taxForm1095CWhistDataContracts)
            {
                var expectedPersonId = dataContract.TfcwhHrperId;
                var expectedTaxYear = dataContract.TfcwhTaxYear;

                var selectedStatementEntities = statementEntities.Where(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxYear == expectedTaxYear
                    && x.TaxForm == TaxForms.Form1095C);
                Assert.AreEqual(1, selectedStatementEntities.Count(), "Each data contract should be represented in the set of domain entities.");
            }
        }

        [TestMethod]
        public async Task NullEmployeeId_1095C()
        {
            this.taxForm1095CWhistDataContracts[0].TfcwhHrperId = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyEmployeeId_1095C()
        {
            this.taxForm1095CWhistDataContracts[0].TfcwhHrperId = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task NullTaxFormYear_1095C()
        {
            this.taxForm1095CWhistDataContracts[0].TfcwhTaxYear = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyTaxFormYear_1095C()
        {
            this.taxForm1095CWhistDataContracts[0].TfcwhTaxYear = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task InvalidDataContractData_1095C()
        {
            this.taxForm1095CWhistDataContracts.Add(new TaxForm1095cWhist() { Recordkey = "99", TfcwhHrperId = null, TfcwhTaxYear = "2012" });
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var entity in statementEntities)
            {
                var expectedPersonId = entity.PersonId;
                var expectedTaxYear = entity.TaxYear;

                var selectedDataContracts = this.taxForm1095CWhistDataContracts.Where(x =>
                    x.TfcwhHrperId == expectedPersonId
                    && x.TfcwhTaxYear == expectedTaxYear);
                Assert.AreEqual(1, selectedDataContracts.Count(), "Each domain entity should be represented in the data contracts list.");
            }
        }

        [TestMethod]
        public async Task NullDataContract_1095C()
        {
            this.taxForm1095CWhistDataContracts.Add(null);
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(taxForm1095CWhistDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in this.taxForm1095CWhistDataContracts)
            {
                if (dataContract != null)
                {
                    var contractPersonId = dataContract.TfcwhHrperId;
                    var taxYear = dataContract.TfcwhTaxYear;

                    var selectedEntities = statementEntities.Where(x =>
                        x.PersonId == contractPersonId
                        && x.TaxYear == taxYear);
                    Assert.AreEqual(1, selectedEntities.Count(), "Each domain entity should be represented in the data contracts list.");
                }
            }
        }

        [TestMethod]
        public async Task TaxForm1095cWhistBulkReadReturnsNull_1095C()
        {
            taxForm1095CWhistDataContracts = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.Form1095C);
            Assert.AreEqual(0, statementEntities.Count());
        }

        #endregion

        #region T4

        [TestMethod]
        public async Task StatementRepository_Success_T4()
        {
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in webT4OnlineDataContracts)
            {
                var expectedPersonId = dataContract.Wt4oEmployeeId;
                var expectedTaxYear = dataContract.Wt4oYear;

                var selectedStatementEntities = statementEntities.Where(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxYear == expectedTaxYear
                    && x.TaxForm == TaxForms.FormT4);
                Assert.AreEqual(1, selectedStatementEntities.Count(), "Each data contract should be represented in the set of domain entities.");
            }
        }

        [TestMethod]
        public async Task StatementRepository_Success_T4_Overflow()
        {
            webT4OnlineDataContracts = new Collection<WebT4Online>();
            webT4OnlineDataContracts.Add(new WebT4Online()
            {
                Recordkey = "1",
                Wt4oEmployeeId = personId,
                Wt4oYear = "2011",
                Wt4oOtherInfoFlags = new System.Collections.Generic.List<string>() { "Y", "Y", "N", "Y", "Y", "Y", "Y", "Y", "Y"}
            });
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count + 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in webT4OnlineDataContracts)
            {
                var expectedPersonId = dataContract.Wt4oEmployeeId;
                var expectedTaxYear = dataContract.Wt4oYear;

                var selectedStatementEntities = statementEntities.Where(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxYear == expectedTaxYear
                    && x.TaxForm == TaxForms.FormT4);
                Assert.AreEqual(2, selectedStatementEntities.Count(), "Each data contract should be represented in the set of domain entities.");
            }
        }

        [TestMethod]
        public async Task NullEmployeeId_T4()
        {
            this.webT4OnlineDataContracts[0].Wt4oEmployeeId = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyEmployeeId_T4()
        {
            this.webT4OnlineDataContracts[0].Wt4oEmployeeId = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task NullTaxFormYear_T4()
        {
            this.webT4OnlineDataContracts[0].Wt4oYear = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task EmptyTaxFormYear_T4()
        {
            this.webT4OnlineDataContracts[0].Wt4oYear = "";
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");
        }

        [TestMethod]
        public async Task InvalidDataContractData_T4()
        {
            this.webT4OnlineDataContracts.Add(new WebT4Online() { Recordkey = "99", Wt4oEmployeeId = null, Wt4oYear = "2012" });
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var entity in statementEntities)
            {
                var expectedPersonId = entity.PersonId;
                var expectedTaxYear = entity.TaxYear;

                var selectedDataContracts = this.webT4OnlineDataContracts.Where(x =>
                    x.Wt4oEmployeeId == expectedPersonId
                    && x.Wt4oYear == expectedTaxYear);
                Assert.AreEqual(1, selectedDataContracts.Count(), "Each domain entity should be represented in the data contracts list.");
            }
        }

        [TestMethod]
        public async Task NullDataContract_T4()
        {
            this.webT4OnlineDataContracts.Add(null);
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);

            // Make sure we have the correct total number of domain entities.
            Assert.AreEqual(webT4OnlineDataContracts.Count - 1, statementEntities.Count(), "We should have built the correct number of domain entities.");

            // Make sure each data contract has been used to create a domain entity.
            foreach (var dataContract in this.webT4OnlineDataContracts)
            {
                if (dataContract != null)
                {
                    var contractPersonId = dataContract.Wt4oEmployeeId;
                    var taxYear = dataContract.Wt4oYear;

                    var selectedEntities = statementEntities.Where(x =>
                        x.PersonId == contractPersonId
                        && x.TaxYear == taxYear);
                    Assert.AreEqual(1, selectedEntities.Count(), "Each domain entity should be represented in the data contracts list.");
                }
            }
        }

        [TestMethod]
        public async Task WebT4OnlineDataContractsBulkReadReturnsNull_T4()
        {
            webT4OnlineDataContracts = null;
            var statementEntities = await taxFormStatementRepository.GetAsync(personId, TaxForms.FormT4);
            Assert.AreEqual(0, statementEntities.Count());
        }

        #endregion

        #region Private Methods

        private HumanResourcesTaxFormStatementRepository BuildTaxFormStatementRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new HumanResourcesTaxFormStatementRepository(apiSettings, cacheProviderObject, transactionFactoryObject, loggerObject);
        }
        #endregion
    }
}
