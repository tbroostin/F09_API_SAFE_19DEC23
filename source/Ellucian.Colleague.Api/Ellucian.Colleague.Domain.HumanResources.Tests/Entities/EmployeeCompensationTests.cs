/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
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
    public class EmployeeCompensationTests
    {
        public string PersonId { get; set; }

        public string OtherBenefits { get; set; }

        public string DisplayEmployeeCosts { get; set; }

        public string TotalCompensationPageHeader { get; set; }

        public decimal? SalaryAmount { get; set; }

        public List<EmployeeBended> Bended { get; set; }

        public List<EmployeeTax> Taxes { get; set; }

        public List<EmployeeStipend> Stipends { get; set; }

        public EmployeeCompensation EmployeeCompensation;

        public void InitializeEmployeeCompensation()
        {
            PersonId = "0014697";
            OtherBenefits = "Additional benefits are available for all full-time employees. These benefits, including free concert and sports tickets, free parking, and use of the athletic facilities, are not listed on this form.";
            DisplayEmployeeCosts = "Y";
            TotalCompensationPageHeader = "This Total Compensation Statement is intended to summarize the estimated value of your current benefits. While every effort has been taken to accurately report this information, discrepancies are possible.";
            SalaryAmount = 19200.00m;
            Bended = new List<EmployeeBended>()
            {
                new EmployeeBended("DEP1","Dental Employee Plus One",780.72m,207.48m),
                new EmployeeBended("MEDE","Medical - Employee Only",3415.44m,379.56m)
            };
            Taxes = new List<EmployeeTax>()
            {
                new EmployeeTax("FICA","FICA Withholding",1154m,1154m),
                new EmployeeTax("FWHM","Federal Withholding - Married",null,681.30m)
            };

            Stipends = new List<EmployeeStipend>()
            {
                new EmployeeStipend("Restricted Stipend",1200.00m),
                new EmployeeStipend("Test GL  Distribution",1000.00m)
            };

        }

    }

    [TestClass]
    public class EmployeeCompensationGenericTests : EmployeeCompensationTests
    {

        [TestInitialize]
        public void Initialize()
        {
            InitializeEmployeeCompensation();
        }
        public new EmployeeCompensation EmployeeCompensation
        {
            get
            {
                return new EmployeeCompensation(PersonId, OtherBenefits, DisplayEmployeeCosts, TotalCompensationPageHeader, SalaryAmount, Bended, Taxes, Stipends);
            }
        }

        #region Constructor Tests
        [TestMethod]
        public void EmployeeCompensation_ConstructorTests()
        {
            Assert.AreEqual(PersonId, EmployeeCompensation.PersonId);
            Assert.AreEqual(OtherBenefits, EmployeeCompensation.OtherBenefits);
            Assert.AreEqual(DisplayEmployeeCosts, EmployeeCompensation.DisplayEmployeeCosts);
            Assert.AreEqual(SalaryAmount, EmployeeCompensation.SalaryAmount);
            Assert.IsNotNull(EmployeeCompensation.Bended);
            Assert.IsNotNull(EmployeeCompensation.Taxes);
            Assert.IsNotNull(EmployeeCompensation.Stipends);
        }
        #endregion

        #region Attribute Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmployeeCompensation_PersonIdIsRequiredTests()
        {
            PersonId = null;
            var expectedException = EmployeeCompensation;
        }

        [TestMethod]
        public void EmployeeCompensation_Bended_ContainsGivenItemTest()
        {
            string benededCode = "MEDE";
            var result = EmployeeCompensation.Bended.Select(b => b.BenededCode).Contains(benededCode);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EmployeeCompensation_Tax_ContainsGivenItemTest()
        {
            string taxCode = "FWHM";
            var result = EmployeeCompensation.Taxes.Select(t=>t.TaxCode).Contains(taxCode);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EmployeeCompensation_Stipend_ContainsGivenItemTest()
        {
            string stipendDesc = "Restricted Stipend";
            var result = EmployeeCompensation.Stipends.Select(s => s.StipendDescription).Contains(stipendDesc);
            Assert.IsTrue(result);
        }
        #endregion

        #region Equals Tests
        [TestMethod]
        public void EmployeeCompensation_ObjectsNotEqualWhenInputIsNullTest()
        {
            Assert.IsFalse(EmployeeCompensation.Equals(null));
        }

        [TestMethod]
        public void EmployeeCompensation_ObjectsNotEqualWhenInputIsDifferentTypeTest()
        {
            Assert.IsFalse(EmployeeCompensation.Equals(new EmployeeStipend("Test", 1155.05m)));
        }
        #endregion
    }
}
