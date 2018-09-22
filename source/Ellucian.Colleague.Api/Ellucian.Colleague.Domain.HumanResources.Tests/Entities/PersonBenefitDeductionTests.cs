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
    public class PersonBenefitDeductionTests
    {
        public string personId;
        public string benefitDeductionId;
        public DateTime enrollmentDate;
        public DateTime? cancelDate;
        public DateTime? lastPayDate;
        public PersonBenefitDeduction personBenefitDeduction
        {
            get
            {
                return new PersonBenefitDeduction(personId, benefitDeductionId, enrollmentDate, cancelDate, lastPayDate);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            personId = "0003914";
            benefitDeductionId = "401k";
            enrollmentDate = new DateTime(2017, 1, 1);
            cancelDate = null;
            lastPayDate = null;
        }

        [TestMethod]
        public void AttributesTest()
        {
            Assert.AreEqual(personId, personBenefitDeduction.PersonId);
            Assert.AreEqual(benefitDeductionId, personBenefitDeduction.BenefitDeductionId);
            Assert.AreEqual(enrollmentDate, personBenefitDeduction.EnrollmentDate);
            Assert.AreEqual(cancelDate, personBenefitDeduction.CancelDate);
            Assert.AreEqual(lastPayDate, personBenefitDeduction.LastPayDate);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdRequiredTest()
        {
            personId = null;
            var error = personBenefitDeduction;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BenefitDeductionIdRequiredTest()
        {
            benefitDeductionId = null;
            var error = personBenefitDeduction;
        }
    }
}
