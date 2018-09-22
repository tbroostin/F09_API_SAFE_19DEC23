// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ColleagueFinanceTaxFormPdfDataRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private ColleagueFinanceTaxFormPdfDataRepository repository;
        private string[] taxFormT4aYearsIds = new string[] { "1" };
        private int TaxYear = DateTime.Now.Year;
        private string personId = "0003946";
        private string t4aBinReposId = "1";

        #region Data contracts
        private TaxFormT4aBinRepos binReposContract = null;
        private ParmT4a parmT4aContract = null;
        private TaxT4aDetailRepos detailReposContract = null;
        private Collection<TaxT4aDetailRepos> detailReposContractsList = null;
        private GetHierarchyNamesForIdsResponse payerNameResponse = null;
        private TaxFormT4aYears t4aYearsContract = null;
        private TaxFormT4aRepos t4aReposContract = null;
        private Countries countriesContract = null;
        private Person personContract = null;

        private void InitializeDataContracts()
        {
            binReposContract = new TaxFormT4aBinRepos()
            {
                Recordkey = "1",
                TftbrYear = TaxYear,
                TftbrBinId = personId,
                TftbrStatus = "U",
                TftbrQualifiedFlag = "Y",
                TftbrCertQualifiedFlag = new List<string>() { "Y" },
                TftbrCertStatus = new List<string>() { "N" },
                TftbrCorpId = "0000002",
                TftbrReposId = "1"
            };

            detailReposContract = new TaxT4aDetailRepos()
            {
                Recordkey = "1",
                TtdrBoxNumberSub = new List<string>()
                {
                    "119",
                    "022",
                    "134",
                    "040",
                    "040122",
                    "042",
                    "016",
                    "018",
                    "020",
                    "024",
                    "048",
                    "018102",
                    "018108",
                    "018110",
                    "018158",
                    "018180",
                    "018190",
                    "024111",
                    "024115",
                    "032",
                    "032126",
                    "032162",
                    "105",
                    "105196"
                },
                TtdrAmt = new List<decimal?>()
            };

            parmT4aContract = new ParmT4a()
            {
                Pt4aYear = TaxYear.ToString(),
                Pt4aMinTotAmt = 25m,
                Pt4aNameAddrHierarchy = "NameAddrHierarchy",
                Pt4aBoxNoSub = detailReposContract.TtdrBoxNumberSub
            };

            payerNameResponse = new GetHierarchyNamesForIdsResponse()
            {
                OutPersonNames = new List<string>() { "Ellucian University" }
            };

            t4aYearsContract = new TaxFormT4aYears()
            {
                Recordkey = "1",
                TftySubmitTitles = new List<string>() { "ORIGINAL" },
                TftySubmitSeqNos = new List<string>() { "01" },
            };

            t4aReposContract = new TaxFormT4aRepos()
            {
                TftrId = personId,
                TftrSurname = "Kleehammer",
                TftrFirstName = "Andy",
                TftrMiddleInitial = "J",
                TftrCountry = "US",
                TftrAddress = "123 Main St.",
                TftrAddress2 = "Apt. 1A",
                TftrCity = "Reston",
                TftrProvince = "AB",
                TftrPostalCode = "12345",
                TftrRecipientBn = "111-11-1111",
                TftrSin = "222-22-2222",
                TftrCertifySurname = new List<string>() { "Smith" },
                TftrCertifyFirstName = new List<string>() { "John" },
                TftrCertifyMiddleInitial = new List<string>() { "B" },
                TftrCertifyCountry = new List<string>() { "CA" },
                TftrCertifyAddress = new List<string>() { "345 Center St." },
                TftrCertifyAddress2 = new List<string>() { "Apt. Z9" },
                TftrCertifyCity = new List<string>() { "Vancouver" },
                TftrCertifyProvince = new List<string>() { "BC" },
                TftrCertifyPostalCode = new List<string>() { "67890" },
                TftrCertifyRecipientBn = new List<string>() { "333-33-3333" },
                TftrCertifySin = new List<string>() { "444-44-4444" },
            };

            countriesContract = new Countries()
            {
                Recordkey = "US",
                CtryDesc = "United States"
            };

            personContract = new Person()
            {
                Recordkey = personId,
                PersonCorpIndicator = "Y"
            };

            #region Build associations
            InitializeT4aDetailReposBoxAmounts();
            binReposContract.buildAssociations();
            t4aYearsContract.buildAssociations();
            t4aReposContract.buildAssociations();
            #endregion
        }

        private void InitializeT4aDetailReposBoxAmounts()
        {
            int index = 1;
            foreach (var box in detailReposContract.TtdrBoxNumberSub)
            {
                detailReposContract.TtdrAmt.Add(Convert.ToDecimal(index));
                index++;
            }

            detailReposContract.buildAssociations();

            detailReposContractsList = new Collection<TaxT4aDetailRepos>() { detailReposContract };
        }
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            repository = new ColleagueFinanceTaxFormPdfDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            InitializeDataContracts();

            #region Mock initializations
            dataReaderMock.Setup(x => x.ReadRecordAsync<TaxFormT4aBinRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(binReposContract);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<ParmT4a>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parmT4aContract);
            });

            dataReaderMock.Setup(x => x.SelectAsync("TAX.FORM.T4A.YEARS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(taxFormT4aYearsIds);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<TaxFormT4aYears>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(t4aYearsContract);
            });

            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxT4aDetailRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(detailReposContractsList);
            });

            transManagerMock.Setup(x => x.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(() =>
            {
                return Task.FromResult(payerNameResponse);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<Countries>(It.IsAny<string>(), true)).Returns((string countryCode, bool replaceTextVMs) =>
            {
                if (countryCode != "US")
                {
                    countriesContract.Recordkey = "CA";
                    countriesContract.CtryDesc = "CANADA";
                }
                return Task.FromResult(countriesContract);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<TaxFormT4aRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(t4aReposContract);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<Person>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(personContract);
            });
            #endregion
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }
        #endregion

        #region GetFormT4aPdfDataAsync
        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_NullPersonId()
        {
            var expetedParam = "personId";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync(null, "1");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_EmptyPersonId()
        {
            var expetedParam = "personId";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("", "1");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_NullRecordId()
        {
            var expetedParam = "recordId";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("1", null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_EmptyRecordId()
        {
            var expetedParam = "recordId";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("1", "");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_NullBinReposContract()
        {
            binReposContract = null;
            var expetedParam = "pdfDataContract cannot be null.";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (NullReferenceException nrex)
            {
                actualParam = nrex.Message;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_NullYearFromBinReposContract()
        {
            binReposContract.TftbrYear = null;
            var expetedParam = "TftbrYear cannot be null.";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (NullReferenceException nrex)
            {
                actualParam = nrex.Message;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_NullParmT4aContract()
        {
            parmT4aContract = null;
            var expetedParam = "PARM.T4A cannot be null.";
            var actualParam = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (NullReferenceException nrex)
            {
                actualParam = nrex.Message;
            }
            Assert.AreEqual(expetedParam, actualParam);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_TaxFormT4aYearsReturnsNull()
        {
            taxFormT4aYearsIds = null;
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but null returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_TaxFormT4aYearsReturnsZeroIds()
        {
            taxFormT4aYearsIds = new string[] { };
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but zero returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_TaxFormT4aYearsReturnsMultipleIds()
        {
            taxFormT4aYearsIds = new string[] { "1", "2" };
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but more than one returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsNotCurrent_TaxFormT4aYearsReturnsNull()
        {
            parmT4aContract.Pt4aYear = (TaxYear - 1).ToString();
            taxFormT4aYearsIds = null;
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but null returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsNotCurrent_TaxFormT4aYearsReturnsZeroIds()
        {
            parmT4aContract.Pt4aYear = (TaxYear - 1).ToString();
            taxFormT4aYearsIds = new string[] { };
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but zero returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsNotCurrent_TaxFormT4aYearsReturnsMultipleIds()
        {
            parmT4aContract.Pt4aYear = (TaxYear - 1).ToString();
            taxFormT4aYearsIds = new string[] { "1", "2" };
            var expetedMessage = "One TAX.FORM.T4A.YEARS ID expected but more than one returned for record ID: " + binReposContract.TftbrYear;
            var actualMessage = "";
            try
            {
                await repository.GetFormT4aPdfDataAsync("0003946", "1");
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expetedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_TotalBoxAmountLessThanParmMinAmount()
        {
            parmT4aContract.Pt4aMinTotAmt = null;
            detailReposContract.TtdrBoxNumberSub = new List<string>();
            detailReposContract.TtdrAmt = new List<decimal?>();
            InitializeT4aDetailReposBoxAmounts();

            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Check the payer name and specific boxes.
            Assert.AreEqual(TaxYear.ToString(), actualPdfData.TaxYear);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Amended));
            Assert.AreEqual(payerNameResponse.OutPersonNames[0], actualPdfData.PayerName);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Sin));
            Assert.AreEqual(t4aReposContract.TftrCertifyRecipientBn[0], actualPdfData.RecipientAccountNumber);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "016").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Pension);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "022").Sum(x => x.TtdrAmtAssocMember), actualPdfData.IncomeTaxDeducted);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "018").Sum(x => x.TtdrAmtAssocMember), actualPdfData.LumpSumPayment);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "020").Sum(x => x.TtdrAmtAssocMember), actualPdfData.SelfEmployedCommissions);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "024").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Annuities);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "048").Sum(x => x.TtdrAmtAssocMember), actualPdfData.FeesForServices);

            // Check the recipient name and address.
            Assert.AreEqual(t4aReposContract.TftrCertifySurname[0] + " " + t4aReposContract.TftrCertifyFirstName[0] + " " + t4aReposContract.TftrCertifyMiddleInitial[0], actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress[0], actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress2[0], actualPdfData.RecipientAddr2);
            Assert.AreEqual(t4aReposContract.TftrCertifyCity[0] + " " + t4aReposContract.TftrCertifyProvince[0] + " " + t4aReposContract.TftrCertifyPostalCode[0], actualPdfData.RecipientAddr3);
            Assert.AreEqual("CANADA", actualPdfData.RecipientAddr4);

            // Check the "other information" boxes, but first.
            foreach (var box in detailReposContract.TtdrSubEntityAssociation)
            {
                if (box.TtdrBoxNumberSubAssocMember != "016" && box.TtdrBoxNumberSubAssocMember != "022" && box.TtdrBoxNumberSubAssocMember != "018"
                    && box.TtdrBoxNumberSubAssocMember != "020" && box.TtdrBoxNumberSubAssocMember != "024" && box.TtdrBoxNumberSubAssocMember != "048")
                {
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 3)
                    {
                        // Get all data contract boxes that have this box number.
                        var dataContractBoxes = detailReposContract.TtdrSubEntityAssociation.Where(x => box.TtdrBoxNumberSubAssocMember == x.TtdrBoxNumberSubAssocMember.Substring(0, 3)).ToList();


                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember);
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(dataContractBoxes.Sum(x => x.TtdrAmtAssocMember), pdfDataBox.Amount);
                    }

                    // Associated boxes get new box codes created using the last 3 digits of the long box number.
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 6)
                    {
                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember.Substring(3, 3));
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(box.TtdrAmtAssocMember, pdfDataBox.Amount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_Level01()
        {
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Check the payer name and specific boxes.
            Assert.AreEqual(TaxYear.ToString(), actualPdfData.TaxYear);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Amended));
            Assert.AreEqual(payerNameResponse.OutPersonNames[0], actualPdfData.PayerName);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Sin));
            Assert.AreEqual(t4aReposContract.TftrCertifyRecipientBn[0], actualPdfData.RecipientAccountNumber);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "016").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Pension);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "022").Sum(x => x.TtdrAmtAssocMember), actualPdfData.IncomeTaxDeducted);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "018").Sum(x => x.TtdrAmtAssocMember), actualPdfData.LumpSumPayment);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "020").Sum(x => x.TtdrAmtAssocMember), actualPdfData.SelfEmployedCommissions);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "024").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Annuities);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "048").Sum(x => x.TtdrAmtAssocMember), actualPdfData.FeesForServices);

            // Check the recipient name and address.
            Assert.AreEqual(t4aReposContract.TftrCertifySurname[0] + " " + t4aReposContract.TftrCertifyFirstName[0] + " " + t4aReposContract.TftrCertifyMiddleInitial[0], actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress[0], actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress2[0], actualPdfData.RecipientAddr2);
            Assert.AreEqual(t4aReposContract.TftrCertifyCity[0] + " " + t4aReposContract.TftrCertifyProvince[0] + " " + t4aReposContract.TftrCertifyPostalCode[0], actualPdfData.RecipientAddr3);
            Assert.AreEqual("CANADA", actualPdfData.RecipientAddr4);

            // Check the "other information" boxes, but first.
            var boxesToDisplay = new List<TaxT4aDetailReposTtdrSub>();
            foreach (var box in detailReposContract.TtdrSubEntityAssociation)
            {
                if (box.TtdrBoxNumberSubAssocMember != "016" && box.TtdrBoxNumberSubAssocMember != "022" && box.TtdrBoxNumberSubAssocMember != "018"
                    && box.TtdrBoxNumberSubAssocMember != "020" && box.TtdrBoxNumberSubAssocMember != "024" && box.TtdrBoxNumberSubAssocMember != "048")
                {
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 3)
                    {
                        // Get all data contract boxes that have this box number.
                        var dataContractBoxes = detailReposContract.TtdrSubEntityAssociation.Where(x => box.TtdrBoxNumberSubAssocMember == x.TtdrBoxNumberSubAssocMember.Substring(0, 3)).ToList();
                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember);
                        boxesToDisplay.Add(new TaxT4aDetailReposTtdrSub(pdfDataBox.Amount, pdfDataBox.BoxNumber));
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(dataContractBoxes.Sum(x => x.TtdrAmtAssocMember), pdfDataBox.Amount);
                    }

                    // Associated boxes get new box codes created using the last 3 digits of the long box number.
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 6)
                    {
                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember.Substring(3, 3));
                        boxesToDisplay.Add(new TaxT4aDetailReposTtdrSub(pdfDataBox.Amount, pdfDataBox.BoxNumber));
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(box.TtdrAmtAssocMember, pdfDataBox.Amount);
                    }
                }
            }

            // Confirm that the boxes are sorted.
            var sortedBoxes = boxesToDisplay.OrderBy(x => x.TtdrBoxNumberSubAssocMember).ToList();
            for (int i = 0; i < sortedBoxes.Count; i++)
            {
                Assert.AreEqual(sortedBoxes[i].TtdrBoxNumberSubAssocMember, actualPdfData.TaxFormBoxesList[i].BoxNumber);
                Assert.AreEqual(sortedBoxes[i].TtdrAmtAssocMember, actualPdfData.TaxFormBoxesList[i].Amount);
            }
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsNotCurrent_Level01()
        {
            parmT4aContract.Pt4aYear = (TaxYear-1).ToString();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Check the payer name and specific boxes.
            Assert.AreEqual(TaxYear.ToString(), actualPdfData.TaxYear);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Amended));
            Assert.AreEqual(payerNameResponse.OutPersonNames[0], actualPdfData.PayerName);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.Sin));
            Assert.AreEqual(t4aReposContract.TftrCertifyRecipientBn[0], actualPdfData.RecipientAccountNumber);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "016").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Pension);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "022").Sum(x => x.TtdrAmtAssocMember), actualPdfData.IncomeTaxDeducted);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "018").Sum(x => x.TtdrAmtAssocMember), actualPdfData.LumpSumPayment);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "020").Sum(x => x.TtdrAmtAssocMember), actualPdfData.SelfEmployedCommissions);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "024").Sum(x => x.TtdrAmtAssocMember), actualPdfData.Annuities);
            Assert.AreEqual(detailReposContract.TtdrSubEntityAssociation.Where(x => x.TtdrBoxNumberSubAssocMember == "048").Sum(x => x.TtdrAmtAssocMember), actualPdfData.FeesForServices);

            // Check the recipient name and address.
            Assert.AreEqual(t4aReposContract.TftrCertifySurname[0] + " " + t4aReposContract.TftrCertifyFirstName[0] + " " + t4aReposContract.TftrCertifyMiddleInitial[0], actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress[0], actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress2[0], actualPdfData.RecipientAddr2);
            Assert.AreEqual(t4aReposContract.TftrCertifyCity[0] + " " + t4aReposContract.TftrCertifyProvince[0] + " " + t4aReposContract.TftrCertifyPostalCode[0], actualPdfData.RecipientAddr3);
            Assert.AreEqual("CANADA", actualPdfData.RecipientAddr4);

            // Check the "other information" boxes, but first.
            foreach (var box in detailReposContract.TtdrSubEntityAssociation)
            {
                if (box.TtdrBoxNumberSubAssocMember != "016" && box.TtdrBoxNumberSubAssocMember != "022" && box.TtdrBoxNumberSubAssocMember != "018"
                    && box.TtdrBoxNumberSubAssocMember != "020" && box.TtdrBoxNumberSubAssocMember != "024" && box.TtdrBoxNumberSubAssocMember != "048")
                {
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 3)
                    {
                        // Get all data contract boxes that have this box number.
                        var dataContractBoxes = detailReposContract.TtdrSubEntityAssociation.Where(x => box.TtdrBoxNumberSubAssocMember == x.TtdrBoxNumberSubAssocMember.Substring(0, 3)).ToList();


                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember);
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(dataContractBoxes.Sum(x => x.TtdrAmtAssocMember), pdfDataBox.Amount);
                    }

                    // Associated boxes get new box codes created using the last 3 digits of the long box number.
                    if (box.TtdrBoxNumberSubAssocMember.Count() == 6)
                    {
                        var pdfDataBox = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == box.TtdrBoxNumberSubAssocMember.Substring(3, 3));
                        Assert.IsNotNull(pdfDataBox);
                        Assert.AreEqual(box.TtdrAmtAssocMember, pdfDataBox.Amount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_MultipleDetailRecords()
        {
            var detailContract = new TaxT4aDetailRepos()
            {
                Recordkey = "1",
                TtdrBoxNumberSub = new List<string>()
                {
                    "032126",
                    "032162",
                    "032",
                    "040122",
                    "040",
                    "105196",
                    "105",
                },
                TtdrAmt = new List<decimal?>()
                {
                    10m,
                    10m,
                    10m,
                    10m,
                    10m,
                    10m,
                    10m,
                }
            };
            detailContract.buildAssociations();
            detailReposContractsList = new Collection<TaxT4aDetailRepos>();
            detailReposContractsList.Add(detailContract);
            detailReposContractsList.Add(detailReposContract);
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Confirm the amounts for box 032.
            var expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "032")
                .Sum(x => x.TtdrAmtAssocMember);
            var actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "032").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 032126.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "032126")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "126").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 032162.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "032162")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "162").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 040.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "040")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "040").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 040122.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "040122")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "122").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_MultipleDetailRecords2()
        {
            var detailContract = new TaxT4aDetailRepos()
            {
                Recordkey = "1",
                TtdrBoxNumberSub = new List<string>()
                {
                    "032162",
                    "032",
                },
                TtdrAmt = new List<decimal?>()
                {
                    10m,
                    10m,
                }
            };
            detailContract.buildAssociations();
            detailReposContractsList = new Collection<TaxT4aDetailRepos>();
            detailReposContractsList.Add(detailContract);
            detailReposContractsList.Add(detailReposContract);
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Confirm the amounts for box 032.
            var expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "032")
                .Sum(x => x.TtdrAmtAssocMember);
            var actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "032").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 032126.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "032126")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "126").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 032162.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "032162")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "162").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 040.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember.Substring(0, 3) == "040")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "040").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);

            // Confirm the amounts for box 040122.
            expectedBoxAmount = detailReposContractsList.SelectMany(x => x.TtdrSubEntityAssociation)
                .Where(x => x.TtdrBoxNumberSubAssocMember == "040122")
                .Sum(x => x.TtdrAmtAssocMember);
            actualBoxAmount = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "122").Amount;
            Assert.AreEqual(expectedBoxAmount, actualBoxAmount);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_LevelM_FullAddress()
        {
            binReposContract.TftbrStatus = "N";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // the recipient name and address should come from the single-value properties in the data contract.
            Assert.AreEqual(t4aReposContract.TftrSurname + " " + t4aReposContract.TftrFirstName + " " + t4aReposContract.TftrMiddleInitial, actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrAddress, actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrAddress2, actualPdfData.RecipientAddr2);
            Assert.AreEqual(t4aReposContract.TftrCity + " " + t4aReposContract.TftrProvince + " " + t4aReposContract.TftrPostalCode, actualPdfData.RecipientAddr3);
            Assert.AreEqual(countriesContract.CtryDesc, actualPdfData.RecipientAddr4);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_LevelM_RecipientIsPerson()
        {
            binReposContract.TftbrStatus = "N";
            personContract.PersonCorpIndicator = "N";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Use the SIN from the T4A contract.
            Assert.AreEqual(t4aReposContract.TftrSin.Replace("-", ""), actualPdfData.Sin);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_Leve01_RecipientIsPerson()
        {
            binReposContract.TftbrStatus = "U";
            personContract.PersonCorpIndicator = "n";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Use the association SIN from the T4A contract.
            Assert.AreEqual(t4aReposContract.TftrCertifySin[0].Replace("-", ""), actualPdfData.Sin);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_LevelM_NoAddressLine2()
        {
            binReposContract.TftbrStatus = "N";
            t4aReposContract.TftrAddress2 = "";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // the recipient name and address should come from the single-value properties in the data contract.
            Assert.AreEqual(t4aReposContract.TftrSurname + " " + t4aReposContract.TftrFirstName + " " + t4aReposContract.TftrMiddleInitial, actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrAddress, actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrCity + " " + t4aReposContract.TftrProvince + " " + t4aReposContract.TftrPostalCode, actualPdfData.RecipientAddr2);
            Assert.AreEqual(countriesContract.CtryDesc, actualPdfData.RecipientAddr3);
            Assert.IsTrue(string.IsNullOrEmpty(actualPdfData.RecipientAddr4));
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_Level01_NoAddressLine2()
        {
            binReposContract.TftbrStatus = "U";
            t4aReposContract.TftrCertifyAddress2 = new List<string>();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // Check the recipient name and address.
            Assert.AreEqual(t4aReposContract.TftrCertifySurname[0] + " " + t4aReposContract.TftrCertifyFirstName[0] + " " + t4aReposContract.TftrCertifyMiddleInitial[0], actualPdfData.RecipientsName);
            Assert.AreEqual(t4aReposContract.TftrCertifyAddress[0], actualPdfData.RecipientAddr1);
            Assert.AreEqual(t4aReposContract.TftrCertifyCity[0] + " " + t4aReposContract.TftrCertifyProvince[0] + " " + t4aReposContract.TftrCertifyPostalCode[0], actualPdfData.RecipientAddr2);
            Assert.AreEqual("CANADA", actualPdfData.RecipientAddr3);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_CorrectedStatus_G()
        {
            binReposContract.TftbrStatus = "g";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // the recipient name and address should come from the single-value properties in the data contract.
            Assert.AreEqual("AMENDED", actualPdfData.Amended);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_RequestedYearIsCurrent_CorrectedStatus_c()
        {
            binReposContract.TftbrStatus = "c";
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // the recipient name and address should come from the single-value properties in the data contract.
            Assert.AreEqual("AMENDED", actualPdfData.Amended);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_OnlyHasBox134()
        {
            detailReposContract.TtdrBoxNumberSub = new List<string>() { "134" };
            detailReposContract.TtdrAmt = new List<decimal?>() { 50.01m };
            detailReposContract.buildAssociations();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // There should only be one box - 134
            Assert.AreEqual(1, actualPdfData.TaxFormBoxesList.Count);
            Assert.AreEqual(detailReposContract.TtdrBoxNumberSub[0], actualPdfData.TaxFormBoxesList[0].BoxNumber);
            Assert.AreEqual(detailReposContract.TtdrAmt[0], actualPdfData.TaxFormBoxesList[0].Amount);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_OnlyHasBox040()
        {
            detailReposContract.TtdrBoxNumberSub = new List<string>() { "040" };
            detailReposContract.TtdrAmt = new List<decimal?>() { 50m };
            detailReposContract.buildAssociations();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // There should only be one box - 040
            Assert.AreEqual(1, actualPdfData.TaxFormBoxesList.Count);
            Assert.AreEqual(detailReposContract.TtdrBoxNumberSub[0], actualPdfData.TaxFormBoxesList[0].BoxNumber);
            Assert.AreEqual(detailReposContract.TtdrAmt[0], actualPdfData.TaxFormBoxesList[0].Amount);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_OnlyHasBox040122()
        {
            detailReposContract.TtdrBoxNumberSub = new List<string>() { "040122" };
            detailReposContract.TtdrAmt = new List<decimal?>() { 50m };
            detailReposContract.buildAssociations();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // There should only be two boxes - 040 and 122
            Assert.AreEqual(2, actualPdfData.TaxFormBoxesList.Count);

            var box040 = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "040");
            var box122 = actualPdfData.TaxFormBoxesList.FirstOrDefault(x => x.BoxNumber == "122");
            Assert.AreEqual(detailReposContract.TtdrAmt[0], box040.Amount);
            Assert.AreEqual(detailReposContract.TtdrAmt[0], box122.Amount);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_OnlyHasBox042()
        {
            detailReposContract.TtdrBoxNumberSub = new List<string>() { "042" };
            detailReposContract.TtdrAmt = new List<decimal?>() { 50m };
            detailReposContract.buildAssociations();
            var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

            // There should only be one box - 042
            Assert.AreEqual(1, actualPdfData.TaxFormBoxesList.Count);
            Assert.AreEqual(detailReposContract.TtdrBoxNumberSub[0], actualPdfData.TaxFormBoxesList[0].BoxNumber);
            Assert.AreEqual(detailReposContract.TtdrAmt[0], actualPdfData.TaxFormBoxesList[0].Amount);
        }

        //[TestMethod]
        //public async Task GetFormT4aPdfDataAsync_Box022IsNegative()
        //{
        //    detailReposContract.TtdrBoxNumberSub = new List<string>() { "022" };
        //    detailReposContract.TtdrAmt = new List<decimal?>() { -50m };
        //    detailReposContract.buildAssociations();
        //    var actualPdfData = await repository.GetFormT4aPdfDataAsync(personId, t4aBinReposId);

        //    // Box 022 should be positive.
        //    Assert.AreEqual(detailReposContract.TtdrAmt[0] * -1, actualPdfData.IncomeTaxDeducted);
        //}
        #endregion
    }
}