// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System.Collections.Generic;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ColleagueFinanceTaxFormStatementRepositoryTests : BaseRepositorySetup
    {
        private IColleagueFinanceTaxFormStatementRepository actualRepository;
        private Collection<TaxFormT4aBinRepos> taxFormT4aBinReposContracts = new Collection<TaxFormT4aBinRepos>();
        private Collection<TaxT4aDetailRepos> taxT4aReposContracts = new Collection<TaxT4aDetailRepos>();
        private Collection<TaxFormT4aYears> taxFormT4aYearsContracts = new Collection<TaxFormT4aYears>();
        private ParmT4a parmT4aContract;
        private TaxT4aDetailRepos detailContract = new TaxT4aDetailRepos();

        #region Initialize and Cleanup

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            actualRepository = new ColleagueFinanceTaxFormStatementRepository(apiSettings, cacheProviderMock.Object,
                transFactoryMock.Object, loggerMock.Object);

            // Get the list of TAX.FORM.T4A.BIN.REPOS records for the person
            BuildT4aBinReposcontracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxFormT4aBinRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxFormT4aBinReposContracts);
            });

            // Get the list of TAX.T4A.DETAIL.REPOS for the TAX.FORM.T4A.BIN.REPOS record that is being processed.
            BuildT4aDetailReposcontracts();
            var detailCriteria1 = "WITH TTDR.YEAR EQ '2016' AND WITH TTDR.ID EQ '0004541' AND WITH TTDR.BIN.ID EQ '0000035' AND WITH TTDR.REF.ID EQ 'M'";
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxT4aDetailRepos>(detailCriteria1, true)).Returns(() =>
            {
                var c1 = taxT4aReposContracts.FirstOrDefault(x => x.TtdrYear == 2016);
                return Task.FromResult(new Collection<TaxT4aDetailRepos>() { c1 });
            });

            var detailCriteria2 = "WITH TTDR.YEAR EQ '2015' AND WITH TTDR.ID EQ '0004541' AND WITH TTDR.BIN.ID EQ '0000035' AND WITH TTDR.REF.ID EQ '02'";
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxT4aDetailRepos>(detailCriteria2, true)).Returns(() =>
            {
                var c2 = taxT4aReposContracts.FirstOrDefault(x => x.TtdrYear == 2015);
                return Task.FromResult(new Collection<TaxT4aDetailRepos>() { c2 });
            });

            var detailCriteria3 = "WITH TTDR.YEAR EQ '2014' AND WITH TTDR.ID EQ '0004541' AND WITH TTDR.BIN.ID EQ '0000035' AND WITH TTDR.REF.ID EQ '02'";
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxT4aDetailRepos>(detailCriteria3, true)).Returns(() =>
            {
                var c3 = taxT4aReposContracts.FirstOrDefault(x => x.TtdrYear == 2014);
                return Task.FromResult(new Collection<TaxT4aDetailRepos>() { c3 });
            });

            BuildTaxFormT4aYearsContracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxFormT4aYears>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxFormT4aYearsContracts);
            });

            parmT4aContract = new ParmT4a()
            {
                Pt4aYear = "2016",
                Pt4aMinTotAmt = 50m
            };
            dataReaderMock.Setup(x => x.ReadRecordAsync<ParmT4a>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parmT4aContract);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
        }
        #endregion

        #region GetAsync for T4As

        [TestMethod]
        public async Task GetAsync_T4A_Success()
        {
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            foreach (var dataContract in taxFormT4aBinReposContracts)
            {
                if (dataContract.TftbrYear != 2014)
                {
                    var selectedEntities = actualStatements.Where(x =>
                        x.PdfRecordId == dataContract.Recordkey
                        && x.PersonId == dataContract.TftbrBinId
                        && x.TaxYear == dataContract.TftbrYear.ToString()).ToList();
                    Assert.AreEqual(1, selectedEntities.Count);
                }
            }
            // 2014 is not web enabled, so no statement is returned.
            Assert.AreEqual(taxFormT4aBinReposContracts.Count(), actualStatements.Count() + 1);
        }

        [TestMethod]
        public async Task GetAsync_InvalidTaxFormId()
        {
            var expectedParam = "taxform";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1095C);
            }
            catch (ArgumentException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync(null, Domain.Base.Entities.TaxForms.FormT4A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T4A_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", Domain.Base.Entities.TaxForms.FormT4A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullParmT4ARecord()
        {
            var expectedMessage = "PARM.T4A cannot be null.";
            var actualMessage = "";
            try
            {
                parmT4aContract = null;
                await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);
            }
            catch (NullReferenceException nrex)
            {
                actualMessage = nrex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullTaxFormT4AFormsRecord()
        {
            taxFormT4aBinReposContracts[0] = null;
            var actualT4AEntities = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            // The null record should be excluded from the returned list.
            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualT4AEntities.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullTaxYear()
        {
            taxFormT4aBinReposContracts[0].TftbrYear = null;
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullRecordKey()
        {
            taxFormT4aBinReposContracts[0].Recordkey = null;
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_EmptyRecordKey()
        {
            taxFormT4aBinReposContracts[0].Recordkey = "";
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_NullBinId()
        {
            taxFormT4aBinReposContracts[0].TftbrBinId = null;
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_EmptyBinId()
        {
            taxFormT4aBinReposContracts[0].TftbrBinId = "";
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_T4A_OneDataContractIsNull()
        {
            taxFormT4aBinReposContracts[0] = null;
            var actualStatements = await actualRepository.GetAsync("0004541", Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(taxFormT4aBinReposContracts.Count, actualStatements.Count() + 2);
        }

        #endregion


        #region Private methods

        private void BuildT4aBinReposcontracts()
        {
            TaxFormT4aBinRepos contract = new TaxFormT4aBinRepos()
            {
                Recordkey = "1",
                TftbrYear = 2016,
                TftbrBinId = "0004541",
                TftbrBinCode = "0000035",
                TftbrStatus = "N",
                TftbrCertStatus = new List<string>() { "N", "N" },
                TftbrCertQualifiedFlag = new List<string>() { "Y", "Y" }
            };
            contract.buildAssociations();
            taxFormT4aBinReposContracts.Add(contract);

            contract = new TaxFormT4aBinRepos()
            {
                Recordkey = "2",
                TftbrYear = 2015,
                TftbrBinId = "0004541",
                TftbrBinCode = "0000035",
                TftbrStatus = "U",
                TftbrCertStatus = new List<string>() { "N", "U" },
                TftbrCertQualifiedFlag = new List<string>() { "Y", "Y" }
            };
            contract.buildAssociations();
            taxFormT4aBinReposContracts.Add(contract);

            contract = new TaxFormT4aBinRepos()
            {
                Recordkey = "3",
                TftbrYear = 2014,
                TftbrBinId = "0004541",
                TftbrBinCode = "0000035",
                TftbrStatus = "U",
                TftbrCertStatus = new List<string>() { "U" },
                TftbrCertQualifiedFlag = new List<string>() { "Y" }
            };
            contract.buildAssociations();
            taxFormT4aBinReposContracts.Add(contract);
        }

        private void BuildT4aDetailReposcontracts()
        {
            TaxT4aDetailRepos detailContract = new TaxT4aDetailRepos()
            {
                Recordkey = "1",
                TtdrYear = 2016,
                TtdrId = "0004541",
                TtdrBinId = "0000035",
                TtdrRefId = "M",
                TtdrBoxNumberSub = new List<string>() { "016", "119" },
                TtdrAmt = new List<decimal?>() { 123.45m, 64m }
            };
            detailContract.buildAssociations();
            taxT4aReposContracts.Add(detailContract);

            detailContract = new TaxT4aDetailRepos()
            {
                Recordkey = "2",
                TtdrYear = 2015,
                TtdrId = "0004541",
                TtdrBinId = "0000035",
                TtdrRefId = "02",
                TtdrBoxNumberSub = new List<string>() { "016", "119" },
                TtdrAmt = new List<decimal?>() { 115.45m, 64m }
            };
            detailContract.buildAssociations();
            taxT4aReposContracts.Add(detailContract);

            detailContract = new TaxT4aDetailRepos()
            {
                Recordkey = "3",
                TtdrYear = 2014,
                TtdrId = "0004541",
                TtdrBinId = "0000035",
                TtdrRefId = "01",
                TtdrBoxNumberSub = new List<string>() { "016", "119" },
                TtdrAmt = new List<decimal?>() { 115.45m, 64m }
            };
            detailContract.buildAssociations();
            taxT4aReposContracts.Add(detailContract);
        }

        private void BuildTaxFormT4aYearsContracts()
        {
            TaxFormT4aYears yearContract = new TaxFormT4aYears()
            {
                Recordkey = "1",
                TftyTaxYear = 2016,
                TftyWebEnabled = "Y",
                TftySubmitTitles = new List<string>() { "ORIGINAL" },
                TftySubmitSeqNos = new List<string>() { "01" }
            };
            yearContract.buildAssociations();
            taxFormT4aYearsContracts.Add(yearContract);

            yearContract = new TaxFormT4aYears()
            {
                Recordkey = "2",
                TftyTaxYear = 2015,
                TftyWebEnabled = "Y",
                TftySubmitTitles = new List<string>() { "First Correction", "ORIGINAL" },
                TftySubmitSeqNos = new List<string>() { "02", "01" }
            };

            yearContract.buildAssociations();
            taxFormT4aYearsContracts.Add(yearContract);


            yearContract = new TaxFormT4aYears()
            {
                Recordkey = "3",
                TftyTaxYear = 2014,
                TftyWebEnabled = "",
                TftySubmitTitles = new List<string>() { "ORIGINAL" },
                TftySubmitSeqNos = new List<string>() { "01" }
            };

            yearContract.buildAssociations();
            taxFormT4aYearsContracts.Add(yearContract);
        }

        #endregion
    }
}