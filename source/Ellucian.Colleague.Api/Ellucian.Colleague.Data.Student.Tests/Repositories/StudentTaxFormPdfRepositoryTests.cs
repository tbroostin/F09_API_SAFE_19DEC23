// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentTaxFormPdfRepositoryTests : BaseRepositorySetup
    {
        private StudentTaxFormPdfDataRepository actualRepository;
        private Collection<TaxForm1098Forms> form1098contracts = new Collection<TaxForm1098Forms>();
        private Collection<TaxForm1098Boxes> taxForm1098Boxes = new Collection<TaxForm1098Boxes>();
        private Collection<BoxCodes> boxCodes = new Collection<BoxCodes>();
        private TaxForm1098Forms form1098contract;
        private Person personContract;
        private Corp corpContract;
        private CorpFounds corpFoundsContract;
        private Parm1098 parm1098Contract;
        private CnstT2202aRepos t2202aDataContract;
        private Defaults coreDefaults;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            actualRepository = new StudentTaxFormPdfDataRepository(cacheProviderMock.Object,
                transFactoryMock.Object, loggerMock.Object);

            BuildDataContracts();
            dataReaderMock.Setup(x => x.ReadRecordAsync<TaxForm1098Forms>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(form1098contract);
            });
            dataReaderMock.Setup(x => x.ReadRecordAsync<Person>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(personContract);
            });
            dataReaderMock.Setup(x => x.ReadRecordAsync<Corp>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(corpContract);
            });
            dataReaderMock.Setup(x => x.ReadRecordAsync<CorpFounds>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(corpFoundsContract);
            });
            dataReaderMock.Setup(x => x.ReadRecordAsync<Parm1098>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parm1098Contract);
            });
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxForm1098Boxes>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(taxForm1098Boxes);
            });
            dataReaderMock.Setup(x => x.BulkReadRecordAsync<BoxCodes>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(boxCodes);
            });

            dataReaderMock.Setup(x => x.ReadRecordAsync<CnstT2202aRepos>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(t2202aDataContract);
            });

            dataReaderMock.Setup(x => x.ReadRecord<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return coreDefaults;
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
        }
        #endregion

        [TestMethod]
        public async Task GetAsync_1098t_Success()
        {
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(form1098contract.Tf98fStudent, pdfData.StudentId);
            Assert.AreEqual(form1098contract.Tf98fName, pdfData.StudentName);
            Assert.AreEqual(form1098contract.Tf98fName2, pdfData.StudentName2);
            Assert.AreEqual(form1098contract.Tf98fTaxYear.ToString(), pdfData.TaxYear);
            Assert.AreEqual(form1098contract.Tf98fAddress, pdfData.StudentAddressLine1);
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            Assert.AreEqual(line2, pdfData.StudentAddressLine2);
            Assert.AreEqual(form1098contract.Tf98fCorrectionInd.ToUpper() == "Y", pdfData.Correction);
            Assert.AreEqual(form1098contract.Tf98fInstitution, pdfData.InstitutionId);
            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        #region Tests
        [TestMethod]
        public async Task GetAsync_1098t_NullTf98fTaxForm1098Boxes()
        {
            form1098contract.Tf98fTaxForm1098Boxes = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullPdfDataContract()
        {
            form1098contract = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullCorpFoundsContract()
        {
            corpFoundsContract = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullTf98fTaxYear()
        {
            form1098contract.Tf98fTaxYear = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullCorpTaxId()
        {
            corpFoundsContract.CorpTaxId = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullCorpName()
        {
            corpContract.CorpName = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullStudentAddress()
        {
            form1098contract.Tf98fAddress = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.StudentAddressLine1);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyStudentAddress()
        {
            form1098contract.Tf98fAddress = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.StudentAddressLine1);
        }

        [TestMethod]
        public async Task GetAsync_1098_NonUSAAddress()
        {
            form1098contract.Tf98fCountry = "CA";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip + " " + form1098contract.Tf98fCountry;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullStudentCity()
        {
            form1098contract.Tf98fCity = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyCity()
        {
            form1098contract.Tf98fCity = "";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullInstitutionId()
        {
            form1098contract.Tf98fInstitution = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.InstitutionId);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyInstitutionId()
        {
            form1098contract.Tf98fInstitution = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.InstitutionId);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullName()
        {
            form1098contract.Tf98fName = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.StudentName);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyName()
        {
            form1098contract.Tf98fName = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.StudentName);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullState()
        {
            form1098contract.Tf98fState = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyState()
        {
            form1098contract.Tf98fState = "";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullStudentZip()
        {
            form1098contract.Tf98fZip = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyStudentZip()
        {
            form1098contract.Tf98fZip = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_Success()
        {
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");
            form1098contract.Tf98fTaxForm = "1098E";

            Assert.AreEqual(form1098contract.Tf98fStudent, pdfData.StudentId);
            Assert.AreEqual(form1098contract.Tf98fName, pdfData.StudentName);
            Assert.AreEqual(form1098contract.Tf98fName2, pdfData.StudentName2);
            Assert.AreEqual(form1098contract.Tf98fTaxYear.ToString(), pdfData.TaxYear);
            Assert.AreEqual(form1098contract.Tf98fAddress, pdfData.StudentAddressLine1);
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            Assert.AreEqual(line2, pdfData.StudentAddressLine2);
            Assert.AreEqual(form1098contract.Tf98fCorrectionInd.ToUpper() == "Y", pdfData.Correction);
            Assert.AreEqual(form1098contract.Tf98fInstitution, pdfData.InstitutionId);
            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }
        #endregion

        #region Tests
        [TestMethod]
        public async Task GetAsync_1098e_NullTf98fTaxForm1098Boxes()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fTaxForm1098Boxes = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullPdfDataContract()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullCorpFoundsContract()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            corpFoundsContract = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullTf98fTaxYear()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fTaxYear = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullCorpTaxId()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            corpFoundsContract.CorpTaxId = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullCorpName()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            corpContract.CorpName = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullStudentAddress()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fAddress = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.StudentAddressLine1);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyStudentAddress()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fAddress = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.StudentAddressLine1);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NonUSAAddress()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fCountry = "CA";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip + " " + form1098contract.Tf98fCountry;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullStudentCity()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fCity = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyCity()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fCity = "";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullInstitutionId()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fInstitution = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.InstitutionId);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyInstitutionId()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fInstitution = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.InstitutionId);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullName()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fName = null;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(null, actualStatements.StudentName);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyName()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fName = "";
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatements.StudentName);
        }

        [TestMethod]
        public async Task GetAsync_1098e_NullState()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fState = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyState()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fState = "";
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }


        [TestMethod]
        public async Task GetAsync_1098e_NullStudentZip()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fZip = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }

        [TestMethod]
        public async Task GetAsync_1098e_EmptyStudentZip()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            form1098contract.Tf98fZip = null;
            var line2 = form1098contract.Tf98fCity + ", " + form1098contract.Tf98fState + " " + form1098contract.Tf98fZip;
            var actualStatements = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(line2, actualStatements.StudentAddressLine2);
        }
        #endregion

        #region SSN scenarios
        [TestMethod]
        public async Task Get1098TPdfAsync_DefaultSsn()
        {
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_NullPersonContract()
        {
            personContract = null;
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_NullSsn()
        {
            personContract.Ssn = null;
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_EmptySsn()
        {
            personContract.Ssn = "";
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_SsnNotProperLength()
        {
            personContract.Ssn = "000-0";
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_DefaultSsn()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_NullPersonContract()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            personContract = null;
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_NullSsn()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            personContract.Ssn = null;
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_EmptySsn()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            personContract.Ssn = "";
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", actualStatement.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_SsnNotProperLength()
        {
            form1098contract.Tf98fTaxForm = "1098E";
            personContract.Ssn = "000-0";
            var actualStatement = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, actualStatement.SSN);
        }
        #endregion

        #region Masking scenarios
        [TestMethod]
        public async Task Get1098TPdfAsync_MaskedSsn1()
        {
            parm1098Contract.P1098MaskSsn = "y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_MaskedSsn2()
        {
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_NullParm1098Contract()
        {
            parm1098Contract = null;
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_NullMaskParameter()
        {
            parm1098Contract.P1098MaskSsn = null;
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_EmptyMaskParameter()
        {
            parm1098Contract.P1098MaskSsn = "";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_SsnNotFullLength()
        {
            personContract.Ssn = "000-0";
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_SsnExactly4Digits()
        {
            personContract.Ssn = "000-";
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_SsnLessThan4Digits()
        {
            personContract.Ssn = "0";
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_NullSsn_Masked()
        {
            personContract.Ssn = null;
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098TPdfAsync_EmptySsn_Masked()
        {
            personContract.Ssn = "";
            parm1098Contract.P1098MaskSsn = "Y";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_MaskedSsn1()
        {
            parm1098Contract.P1098MaskSsn = "y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_MaskedSsn2()
        {
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_NullParm1098Contract()
        {
            parm1098Contract = null;
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_NullMaskParameter()
        {
            parm1098Contract.P1098MaskSsn = null;
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_EmptyMaskParameter()
        {
            parm1098Contract.P1098MaskSsn = "";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual(personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_SsnNotFullLength()
        {
            personContract.Ssn = "000-0";
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_SsnExactly4Digits()
        {
            personContract.Ssn = "000-";
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn.Substring(personContract.Ssn.Length - 4), pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_SsnLessThan4Digits()
        {
            personContract.Ssn = "0";
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("XXX-XX-" + personContract.Ssn, pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_NullSsn_Masked()
        {
            personContract.Ssn = null;
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_EmptySsn_Masked()
        {
            personContract.Ssn = "";
            parm1098Contract.P1098MaskSsn = "Y";
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.AreEqual("", pdfData.SSN);
        }

        [TestMethod]
        public async Task Get1098EPdfAsync_IsPriorInterestOrFeeExcluded()
        {
            parm1098Contract.P1098EYears = new List<int?>() { 2016, 2015 };
            parm1098Contract.P1098EFeeFlags = new List<string>() { "Y", "N" };
            parm1098Contract.buildAssociations();
            form1098contract.Tf98fTaxForm = "1098E";
            var pdfData = await actualRepository.Get1098PdfAsync("0003946", "1");

            Assert.IsTrue(pdfData.IsPriorInterestOrFeeExcluded);
        }
        #endregion

        #region Private methods
        private void BuildDataContracts()
        {
            form1098contract = new TaxForm1098Forms()
            {
                Recordkey = "1",
                TaxForm1098FormsAdddate = new DateTime(2015, 05, 10, 5, 5, 5),
                TaxForm1098FormsAddtime = new DateTime(2015, 05, 10, 5, 5, 5),
                Tf98fAddress = "1234 Main St.",
                Tf98fCity = "Fairfax",
                Tf98fState = "VA",
                Tf98fZip = "22033",
                Tf98fCountry = "USA",
                Tf98fCorrectionInd = "Y",
                Tf98fInstitution = "0001234",
                Tf98fName = "Andrew Kleehammer",
                Tf98fName2 = "",
                Tf98fStudent = "0003946",
                Tf98fTaxYear = 2016,
                Tf98fTaxForm = "1098T",
                Tf98fTaxForm1098Boxes = new List<string>() { "1", "2", "3", "4" }
            };
            form1098contract.Tf98fTaxForm1098Boxes.Add("Box1");

            personContract = new Person()
            {
                Recordkey = "0003946",
                Ssn = "000-00-0001"
            };

            corpContract = new Corp()
            {
                Recordkey = "0001234",
                CorpName = new List<string>()
            };
            corpContract.CorpName.Add("0001234");

            corpFoundsContract = new CorpFounds()
            {
                CorpTaxId = "0001234",
                Recordkey = "0001234"
            };

            parm1098Contract = new Parm1098()
            {
                P1098TRefundBoxCode = "TUI",
                P1098TInstPhone = "703-259-9000",
                P1098TInstPhoneExt = "1009",
                P1098TFaBoxCode = "",
                P1098TLoadBoxCode = "",
                P1098TGradBoxCode = "",
                P1098TFaRefBoxCode = "",
                P1098TNewYrBoxCode = "",
                P1098TYears = new List<int?>() { 2016, 2015 },
                P1098TYrChgRptMeths = new List<string>() { "N", "Y" },
                P1098ETaxForm = "1098E",
                P1098TTaxForm = "1098T"
            };
            parm1098Contract.buildAssociations();

            taxForm1098Boxes.Add(new TaxForm1098Boxes()
            {
                Recordkey = "1",
                Tf98bAmt = 980,
                Tf98bBoxCode = "TUI",
                Tf98bValue = ""
            });

            taxForm1098Boxes.Add(new TaxForm1098Boxes()
            {
                Recordkey = "2",
                Tf98bAmt = 1000,
                Tf98bBoxCode = "CNY",
                Tf98bValue = ""
            });

            taxForm1098Boxes.Add(new TaxForm1098Boxes()
            {
                Recordkey = "3",
                Tf98bAmt = 150,
                Tf98bBoxCode = "LOD",
                Tf98bValue = ""
            });

            taxForm1098Boxes.Add(new TaxForm1098Boxes()
            {
                Recordkey = "4",
                Tf98bAmt = 275,
                Tf98bBoxCode = "GRD",
                Tf98bValue = ""
            });

            boxCodes.Add(new BoxCodes()
            {
                BxcBoxNumber = "2",
                Recordkey = "TUI"
            });

            boxCodes.Add(new BoxCodes()
            {
                BxcBoxNumber = "7",
                Recordkey = "CNY"
            });

            boxCodes.Add(new BoxCodes()
            {
                BxcBoxNumber = "8",
                Recordkey = "LOD"
            });

            boxCodes.Add(new BoxCodes()
            {
                BxcBoxNumber = "9",
                Recordkey = "GRD"
            });

            coreDefaults = new Defaults()
            {
                DefaultHostCorpId = "1",
                DefaultWebEmailType = "WEB",
                Recordkey = "1"
            };

            t2202aDataContract = new CnstT2202aRepos()
            {
                Recordkey = "1",
                T2ReposStudent = "0003946",
                T2ReposStuProgramTitle = "Some Program",
                T2ReposStudentName = "John Smith",
                T2ReposYear = 2017,
                T2ReposEnrollmentEntityAssociation = new List<CnstT2202aReposT2ReposEnrollment>()
                {
                    new CnstT2202aReposT2ReposEnrollment()
                    {
                        T2ReposEnrollCalcAmtsAssocMember = 1000m,
                        T2ReposEnrollCalcEndDtsAssocMember = new DateTime(2017, 1, 1),
                        T2ReposEnrollCalcFtMthsAssocMember = 1,
                        T2ReposEnrollCalcPtMthsAssocMember = 1,
                        T2ReposEnrollCalcStDtsAssocMember = new DateTime(2017, 1, 1),
                        T2ReposEnrollEndDatesAssocMember = new DateTime(2017, 1, 1),
                        T2ReposEnrollFtMthsAssocMember = 1,
                        T2ReposEnrollLockFlagsAssocMember = "",
                        T2ReposEnrollPtMthsAssocMember = 1,
                        T2ReposEnrollStartDatesAssocMember = new DateTime(2017, 1, 1),
                        T2ReposEnrollTuitionAmtsAssocMember = 1000m
                    }
                },
                T2ReposStudentAddress = new List<string>()
                {
                    "Line 1",
                    "Line 2",
                    "Line 3",
                    "Line 4",
                    "Line 5"
                }
            };
        }
        #endregion

        #region T2202A



        [TestMethod]
        public async Task GetT2202aPdfAsync_Success()
        {
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");

            Assert.AreEqual(t2202aDataContract.T2ReposStudent, pdfData.StudentId);
            Assert.AreEqual(t2202aDataContract.T2ReposYear.ToString(), pdfData.TaxYear);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentName, pdfData.StudentNameAddressLine1);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentAddress.ElementAtOrDefault(0), pdfData.StudentNameAddressLine2);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentAddress.ElementAtOrDefault(1), pdfData.StudentNameAddressLine3);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentAddress.ElementAtOrDefault(2), pdfData.StudentNameAddressLine4);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentAddress.ElementAtOrDefault(3), pdfData.StudentNameAddressLine5);
            Assert.AreEqual(t2202aDataContract.T2ReposStudentAddress.ElementAtOrDefault(4), pdfData.StudentNameAddressLine6);
            Assert.AreEqual(t2202aDataContract.T2ReposStuProgramTitle, pdfData.ProgramName);

            Assert.AreEqual(coreDefaults.DefaultHostCorpId, pdfData.InstitutionId);
            Assert.AreEqual(String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x))), pdfData.InstitutionNameAddressLine1);

            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollStartDatesAssocMember.Value.Year.ToString(), pdfData.SessionPeriods[0].StudentFromYear);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollStartDatesAssocMember.Value.Month.ToString(), pdfData.SessionPeriods[0].StudentFromMonth);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollEndDatesAssocMember.Value.Year.ToString(), pdfData.SessionPeriods[0].StudentToYear);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollEndDatesAssocMember.Value.Month.ToString(), pdfData.SessionPeriods[0].StudentToMonth);

            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollTuitionAmtsAssocMember, pdfData.SessionPeriods[0].BoxAAmount);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollTuitionAmtsAssocMember.Value.ToString("N2"), pdfData.SessionPeriods[0].BoxAAmountString);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollPtMthsAssocMember, pdfData.SessionPeriods[0].BoxBHours);
            Assert.AreEqual(t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollFtMthsAssocMember, pdfData.SessionPeriods[0].BoxCHours);

        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullContract()
        {
            t2202aDataContract = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullCorpName()
        {
            corpContract.CorpName = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_EmptyCorpName()
        {
            corpContract.CorpName = new List<string>();
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullYear()
        {
            t2202aDataContract.T2ReposYear = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullStudent()
        {
            t2202aDataContract.T2ReposStudent = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullT2ReposEnrollStartDatesAssocMember()
        {
            t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollStartDatesAssocMember = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullT2ReposEnrollEndDatesAssocMember()
        {
            t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollEndDatesAssocMember = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        [TestMethod]
        public async Task GetT2202aPdfAsync_NullAmountsAndHours()
        {
            t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollTuitionAmtsAssocMember = null;
            t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollPtMthsAssocMember = null;
            t2202aDataContract.T2ReposEnrollmentEntityAssociation[0].T2ReposEnrollFtMthsAssocMember = null;
            var pdfData = await actualRepository.GetT2202aPdfAsync("0003946", "1");
            Assert.AreEqual(null, pdfData);
        }

        #endregion
    }
}