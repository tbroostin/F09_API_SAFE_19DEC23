using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class FacultyContractTests
    {
        public string id;
        public string contractDescription;
        public string contractNumber;
        public string contractType;
        public DateTime? startDate;
        public DateTime? endDate;
        public string loadPeriodId;
        public decimal? intendedTotalLoad;
        public decimal? totalValue;
        public List<FacultyContractPosition> facultyContractPositions;
        private FacultyContract facultyContracts;

        [TestInitialize]
        public void Initialize()
        {
            id = "1001";
            contractDescription = "something cool to test descriptions";
            contractNumber = "117";
            contractType = "S";
            startDate = new DateTime(2011,01,20);
            endDate = new DateTime(2011,05,11);
            loadPeriodId = "1293213" ;
            intendedTotalLoad = 20;
            totalValue = 354000;
            facultyContractPositions = new List<FacultyContractPosition>();
            facultyContracts = new FacultyContract(id, contractDescription, contractNumber, contractType,
                startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }
        [TestMethod]
        public void FacultyContract_Id()
        {
            Assert.AreEqual(id, facultyContracts.Id);
        }

        [TestMethod]
        public void FacultyContract_Description()
        {
            Assert.AreEqual(contractDescription, facultyContracts.ContractDescription);
        }

        [TestMethod]
        public void FacultyContract_ContractNumber()
        {
            Assert.AreEqual(contractNumber, facultyContracts.ContractNumber);
        }

        [TestMethod]
        public void FacultyContract_ContractType()
        {
            Assert.AreEqual(contractType, facultyContracts.ContractType);
        }

        [TestMethod]
        public void FacultyContract_StartDate()
        {
            Assert.AreEqual(startDate, facultyContracts.StartDate);
        }

        [TestMethod]
        public void FacultyContract_EndDate()
        {
            Assert.AreEqual(endDate, facultyContracts.EndDate);
        }

        [TestMethod]
        public void FacultyContract_LoadPeriodId()
        {
            Assert.AreEqual(loadPeriodId, facultyContracts.LoadPeriodId);
        }

        [TestMethod]
        public void FacultyContract_IntendedTotalLoad()
        {
            Assert.AreEqual(intendedTotalLoad, facultyContracts.IntendedTotalLoad);
        }

        [TestMethod]
        public void FacultyContract_TotalValue()
        {
            Assert.AreEqual(totalValue, facultyContracts.TotalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContract_IdNullException()
        {
            new FacultyContract(null, contractDescription, contractNumber, contractType, startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContract_ContractDescriptionNullException()
        {
            new FacultyContract(id, null, contractNumber, contractType, startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContract_ContractTypeNullException()
        {
            new FacultyContract(id, contractDescription, contractNumber, null, startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContract_StartDateNullException()
        {
            new FacultyContract(id, contractDescription, contractNumber, contractType, null, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContract_LoadPeriodIdNullException()
        {
            new FacultyContract(id, contractDescription, contractNumber, contractType, startDate, endDate, null, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContractId_EmptyStringException()
        {
            new FacultyContract(string.Empty, contractDescription, contractNumber, contractType, startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContractDescription_EmptyStringException()
        {
            new FacultyContract(id, string.Empty, contractNumber, contractType, startDate, endDate, loadPeriodId, intendedTotalLoad, totalValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyContractLoadPeriodId_EmptyStringException()
        {
            new FacultyContract(id, contractDescription, contractNumber, contractType, startDate, endDate, string.Empty, intendedTotalLoad, totalValue);
        }
    }
}
