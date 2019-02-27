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

        private Parm1099mi parm1099miContractNull = null;
        private Parm1099mi parm1099miContract;
        private Collection<TaxForm1099miYears> taxForm1099miYearsContracts = new Collection<TaxForm1099miYears>();
        private Collection<TaxForm1099miYears> taxForm1099miYearsWebDisabledContracts = new Collection<TaxForm1099miYears>();
        private Collection<Tax1099miDetailRepos> tax1099miDetailContracts = new Collection<Tax1099miDetailRepos>();

        #region Initialize and Cleanup

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            actualRepository = new ColleagueFinanceTaxFormStatementRepository(apiSettings, cacheProviderMock.Object,
                transFactoryMock.Object, loggerMock.Object);

            #region T4a records
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
            #endregion

            #region 1099mi records
            // Get the list of TAX.FORM.1099MI.DETAIL.REPOS for the person

            //vendor 0011029 - > for empty data records
            Build1099miDetailYearWiseContracts();
            var detailCriteriaFor1099miAllYears = "WITH TMIDTLR.VENDOR.ID EQ '0011029'";
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<Tax1099miDetailRepos>(detailCriteriaFor1099miAllYears, true)).Returns(() =>
            {
                var c1 = tax1099miDetailContracts;
                return Task.FromResult(c1);
            });
            //vendor 0011030 - > for empty data records
            var detailCriteriaFor1099mi0 = "WITH TMIDTLR.VENDOR.ID EQ '0011030'";
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<Tax1099miDetailRepos>(detailCriteriaFor1099mi0, true)).Returns(() =>
            {
                var c1 = new Collection<Tax1099miDetailRepos>();
                return Task.FromResult(c1);
            });
            //build mi year contracts
            BuildTaxForm1099miYearsContracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxForm1099miYears>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1099miYearsContracts);
            });
            //set current year as 2017
            parm1099miContract = new Parm1099mi()
            {
                P1099miYear = "2017"
            };
            dataReaderMock.Setup(x => x.ReadRecordAsync<Parm1099mi>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parm1099miContract);
            });
            #endregion
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

        #region GetAsync_1099mi_statements

        [TestMethod]
        public async Task GetAsync_1099Mi_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync(null, Domain.Base.Entities.TaxForms.Form1099MI);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1099Mi_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", Domain.Base.Entities.TaxForms.Form1099MI);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "PARM.1099MI cannot be null.")]
        public async Task GetAsync_1099mi_Parm1099Null()
        {
            dataReaderMock.Setup(x => x.ReadRecordAsync<Parm1099mi>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parm1099miContractNull);
            });

            var actualStatements = await actualRepository.GetAsync("0011031", Domain.Base.Entities.TaxForms.Form1099MI);


            Assert.AreEqual(tax1099miDetailContracts.Count(), actualStatements.Count() + 1);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_NoRecordsFound_Success()
        {
            var actualStatements = await actualRepository.GetAsync("0011030", Domain.Base.Entities.TaxForms.Form1099MI);
            //check whether it returns empty statements
            Assert.IsTrue(!actualStatements.Any());
        }

        [TestMethod]
        public async Task GetAsync_1099mi_CurrentYear_Verified_NotCertified_Success()
        {
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);
            //check whether it pulls the record with RefId M and qualified, so no notations
            Assert.IsTrue(actualStatements.Any(x => x.Notation == Domain.Base.Entities.TaxFormNotations.None));
        }

        [TestMethod]
        public async Task GetAsync_1099mi_CurrentYear_NonCertified_RecordKeyNull()
        {
            var actualStatements = await actualRepository.GetAsync("0011032", Domain.Base.Entities.TaxForms.Form1099MI);
            //check whether it returns empty statements
            Assert.IsTrue(!actualStatements.Any());
        }

        [TestMethod]
        public async Task GetAsync_Invalid1099miTaxFormId()
        {
            var expectedParam = "taxform";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("11031", Domain.Base.Entities.TaxForms.Form1095C);
            }
            catch (ArgumentException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync(null, Domain.Base.Entities.TaxForms.Form1099MI);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", Domain.Base.Entities.TaxForms.Form1099MI);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        [TestMethod]
        public async Task GetAsync_1099mi_Result_NullTaxForm1099miFormsRecord()
        {
            tax1099miDetailContracts[0] = null;
            tax1099miDetailContracts[1] = null;
            var actual1099miEntities = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            // The null record should be excluded from the returned list.
            Assert.AreEqual(tax1099miDetailContracts.Count, actual1099miEntities.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_NullStateId()
        {
            tax1099miDetailContracts[0].TmidtlrStateId = null;
            tax1099miDetailContracts[1].TmidtlrStateId = null;
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);
            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_NullTaxYear()
        {
            tax1099miDetailContracts[0].TmidtlrYear = null;
            tax1099miDetailContracts[1].TmidtlrYear = null;
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);
            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_NullRecordKey()
        {
            tax1099miDetailContracts[0].Recordkey = null;
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_EmptyRecordKey()
        {
            tax1099miDetailContracts[0].Recordkey = "";
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_NullPersonId()
        {
            tax1099miDetailContracts[0].TmidtlrVendorId = null;
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_EmptyPersonId()
        {
            tax1099miDetailContracts[0].TmidtlrVendorId = "";
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_Result_OneDataContractIsNull()
        {
            tax1099miDetailContracts[0] = null;
            tax1099miDetailContracts[1] = null;
            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(tax1099miDetailContracts.Count, actualStatements.Count() + 2);
        }

        [TestMethod]
        public async Task GetAsync_1099mi_WebEnabledIsFalse()
        {
            BuildTaxForm1099miYearsWebDisabledContracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxForm1099miYears>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1099miYearsWebDisabledContracts);
            });

            var actualStatements = await actualRepository.GetAsync("0011029", Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(actualStatements.Select(x=>x.Notation).FirstOrDefault(), Domain.Base.Entities.TaxFormNotations.NotAvailable);
        }
        #endregion

        #region Private methods

        #region T4A Private methods
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

        #region 1099mi_PrivateRegion
             
        private void Build1099miDetailYearWiseContracts()
        {
            Tax1099miDetailRepos contract = new Tax1099miDetailRepos()
            {
                Recordkey = "1",
                TmidtlrYear = "2017",
                TmidtlrVendorId = "0011029",
                TmidtlrStateId = "VA",
                TmidtlrEin = "43",
                TmidtlrStatus = "",
                TmidtlrQualifiedFlag = "Y",
                TmidtlrRefId = "M"
            };
            contract.buildAssociations();
            tax1099miDetailContracts.Add(contract);

            contract = new Tax1099miDetailRepos()
            {
                Recordkey = "2",
                TmidtlrYear = "2016",
                TmidtlrVendorId = "0011029",
                TmidtlrStateId = "VA",
                TmidtlrEin = "43",
                TmidtlrStatus = "N",
                TmidtlrQualifiedFlag = "Y",
                TmidtlrRefId = "01"
            };
            contract.buildAssociations();
            tax1099miDetailContracts.Add(contract);
        }

        private void BuildTaxForm1099miYearsContracts()
        {
            TaxForm1099miYears yearContract = new TaxForm1099miYears()
            {
                Recordkey = "2017",
                TfmyWebEnabled = "Y",
                TfmySubmitSeqNos = new List<string>() { "01" }
            };
            yearContract.buildAssociations();
            taxForm1099miYearsContracts.Add(yearContract);
            yearContract = new TaxForm1099miYears()
            {
                Recordkey = "2016",
                TfmyWebEnabled = "Y",
                TfmySubmitSeqNos = new List<string>() { "01" }
            };
            yearContract.buildAssociations();
            taxForm1099miYearsContracts.Add(yearContract);
        }

        private void BuildTaxForm1099miYearsWebDisabledContracts()
        {
            TaxForm1099miYears yearContract = new TaxForm1099miYears()
            {
                Recordkey = "2017",
                TfmyWebEnabled = "N",
                TfmySubmitSeqNos = new List<string>() { "01" }
            };
            yearContract.buildAssociations();
            taxForm1099miYearsWebDisabledContracts.Add(yearContract);
            yearContract = new TaxForm1099miYears()
            {
                Recordkey = "2016",
                TfmyWebEnabled = "N",
                TfmySubmitSeqNos = new List<string>() { "01" }
            };
            yearContract.buildAssociations();
            taxForm1099miYearsWebDisabledContracts.Add(yearContract);
        }

        #endregion 

        #endregion
    }
}