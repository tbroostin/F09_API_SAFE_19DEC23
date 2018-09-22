// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentTaxFormStatementRepositoryTests : BaseRepositorySetup
    {
        private StudentTaxFormStatementRepository actualRepository;
        private Collection<TaxForm1098Forms> form1098contracts = new Collection<TaxForm1098Forms>();
        private Parm1098 parm1098Contract;
        private Collection<CnstT2202aRepos> formT2202acontracts = new Collection<CnstT2202aRepos>();
        private CnstRptParms cnstRptParmsContract;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            actualRepository =  new StudentTaxFormStatementRepository(apiSettings, cacheProviderMock.Object,
                transFactoryMock.Object, loggerMock.Object);

            Build1098contracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxForm1098Forms>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(form1098contracts);
                });

            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098T", P1098ETaxForm = "1098E" };
            dataReaderMock.Setup(x => x.ReadRecordAsync<Parm1098>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(parm1098Contract);
                });

            BuildT2202acontracts();
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<CnstT2202aRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(formT2202acontracts);
            });

            cnstRptParmsContract = new CnstRptParms() { CnstConsentText = "Consent" };
            dataReaderMock.Setup(x => x.ReadRecordAsync<CnstRptParms>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(cnstRptParmsContract);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
        }
        #endregion

        #region Invalid tax form
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
        #endregion

        #region GetAsync for 1098s
        [TestMethod]
        public async Task GetAsync_1098_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync(null, Domain.Base.Entities.TaxForms.Form1098);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", Domain.Base.Entities.TaxForms.Form1098);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullParm1098Record()
        {
            var expectedMessage = "PARM.1098 cannot be null.";
            var actualMessage = "";
            try
            {
                parm1098Contract = null;
                await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);
            }
            catch (NullReferenceException nrex)
            {
                actualMessage = nrex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_1098t_NullTaxForm1098FormsRecord()
        {
            form1098contracts[0] = null;
            var actual1098Entities = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            // The null record should be excluded from the returned list.
            Assert.AreEqual(form1098contracts.Count, actual1098Entities.Count() + 1);
        }

        [TestMethod]
        public async Task GetAsync_1098t_NullTaxYear()
        {
            form1098contracts[0].Tf98fTaxYear = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_NullRecordKey()
        {
            form1098contracts[0].Recordkey = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_EmptyRecordKey()
        {
            form1098contracts[0].Recordkey = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_NullStudentId()
        {
            form1098contracts[0].Tf98fStudent = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_EmptyStudentId()
        {
            form1098contracts[0].Tf98fStudent = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_OneDataContractIsNull()
        {
            form1098contracts[0] = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098t_Success()
        {
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            foreach (var dataContract in form1098contracts.Where(x => x.Tf98fTaxForm == "1098T").ToList())
            {
                var selectedEntities = actualStatements.Where(x =>
                    x.PdfRecordId == dataContract.Recordkey
                    && x.PersonId == dataContract.Tf98fStudent
                    && x.TaxForm.ToString() == "Form" + dataContract.Tf98fTaxForm
                    && x.TaxYear == dataContract.Tf98fTaxYear.ToString()).ToList();
                Assert.AreEqual(1, selectedEntities.Count);
            }
        }

        [TestMethod]
        public async Task GetAsync_1098e_Success()
        {
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            foreach (var dataContract in form1098contracts.Where(x => x.Tf98fTaxForm == "1098E").ToList())
            {
                var selectedEntities = actualStatements.Where(x =>
                    x.PdfRecordId == dataContract.Recordkey
                    && x.PersonId == dataContract.Tf98fStudent
                    && x.TaxForm.ToString() == "Form" + dataContract.Tf98fTaxForm
                    && x.TaxYear == dataContract.Tf98fTaxYear.ToString()).ToList();
                Assert.AreEqual(1, selectedEntities.Count);
            }
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullTaxForm1098FormsRecord()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0] = null;
            var actual1098Entities = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count, actual1098Entities.Count() + 1);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullTaxYear()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0].Tf98fTaxYear = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullRecordKey()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0].Recordkey = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyRecordKey()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0].Recordkey = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullStudentId()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0].Tf98fStudent = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyStudentId()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0].Tf98fStudent = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_1098e_OneDataContractIsNull()
        {
            parm1098Contract = new Parm1098() { P1098TTaxForm = "1098E" };
            form1098contracts.All(currentRecord => { currentRecord.Tf98fTaxForm = "1098E"; return true; });
            form1098contracts[0] = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(form1098contracts.Count - 1, actualStatements.Count());
        }
        #endregion

        #region GetAsync for T2202As
        [TestMethod]
        public async Task GetAsync_T2202a_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync(null, Domain.Base.Entities.TaxForms.FormT2202A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T2202a_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", Domain.Base.Entities.TaxForms.FormT2202A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T2202a_NullCnstRptParmsRecord()
        {
            var expectedMessage = "CNST.RPT.PARMS cannot be null.";
            var actualMessage = "";
            try
            {
                cnstRptParmsContract = null;
                await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);
            }
            catch (NullReferenceException nrex)
            {
                actualMessage = nrex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAsync_T2202a_NullCnstT2202aRepostRecord()
        {
            formT2202acontracts[0] = null;
            var actual1098Entities = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            // The null record should be excluded from the returned list.
            Assert.AreEqual(formT2202acontracts.Count, actual1098Entities.Count() + 1);
        }

        [TestMethod]
        public async Task GetAsync_T2202a_NullTaxYear()
        {
            formT2202acontracts[0].T2ReposYear = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_NullRecordKey()
        {
            formT2202acontracts[0].Recordkey = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_EmptyRecordKey()
        {
            formT2202acontracts[0].Recordkey = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_NullStudentId()
        {
            formT2202acontracts[0].T2ReposStudent = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_EmptyStudentId()
        {
            formT2202acontracts[0].T2ReposStudent = "";
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_OneDataContractIsNull()
        {
            formT2202acontracts[0] = null;
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(formT2202acontracts.Count - 1, actualStatements.Count());
        }

        [TestMethod]
        public async Task GetAsync_T2202a_Success()
        {
            var actualStatements = await actualRepository.GetAsync("1", Domain.Base.Entities.TaxForms.FormT2202A);

            foreach (var dataContract in formT2202acontracts.ToList())
            {
                var selectedEntities = actualStatements.Where(x =>
                    x.PdfRecordId == dataContract.Recordkey
                    && x.PersonId == dataContract.T2ReposStudent
                    && x.TaxYear == dataContract.T2ReposYear.ToString()).ToList();
                Assert.AreEqual(1, selectedEntities.Count);
            }
        }
        #endregion

        #region Private methods
        private void Build1098contracts()
        {
            form1098contracts.Add(new TaxForm1098Forms()
            {
                Recordkey = "1",
                Tf98fStudent = "0003946",
                Tf98fTaxYear = 2016,
                Tf98fTaxForm = "1098T"
            });

            form1098contracts.Add(new TaxForm1098Forms()
            {
                Recordkey = "2",
                Tf98fStudent = "0003946",
                Tf98fTaxYear = 2015,
                Tf98fTaxForm = "1098T"
            });
        }

        private void BuildT2202acontracts()
        {
            formT2202acontracts.Add(new CnstT2202aRepos()
            {
                Recordkey = "1",
                T2ReposStudent = "0003946",
                T2ReposYear = 2016
            });

            formT2202acontracts.Add(new CnstT2202aRepos()
            {
                Recordkey = "2",
                T2ReposStudent = "0003946",
                T2ReposYear = 2015
            });
        }
        #endregion
    }
}