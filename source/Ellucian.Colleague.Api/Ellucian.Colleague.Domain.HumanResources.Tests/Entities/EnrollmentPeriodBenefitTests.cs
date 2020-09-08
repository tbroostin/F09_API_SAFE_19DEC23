/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class EnrollmentPeriodBenefitTests
    {
        private string bendedId;
        private string enrPerBenId;
        private string benTypeId;
        private string benDesc;

        private EnrollmentPeriodBenefit benefit;

        [TestInitialize]
        public void Initialize()
        {
            bendedId = "MED";
            enrPerBenId = "MED2020";
            benTypeId = "MEDICAL";
            benDesc = "Medical Plan for 2020";
            benefit = new EnrollmentPeriodBenefit(bendedId, enrPerBenId, benTypeId);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(benefit);
        }

        [TestMethod]
        public void Id_AssignedTest()
        {
            Assert.IsNotNull(benefit);
            Assert.AreEqual(bendedId, benefit.BenefitId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoId_ExceptionThrownTest()
        {
            benefit = new EnrollmentPeriodBenefit(null, enrPerBenId, benTypeId);
        }

        [TestMethod]
        public void BenefitDescription_GetSetTest()
        {
            Assert.IsNotNull(benefit);
            benefit.BenefitDescription = benDesc;
            Assert.AreEqual(benDesc, benefit.BenefitDescription);
        }

        [TestMethod]
        public void EnrPerBenId_AssignedTest()
        {
            Assert.IsNotNull(benefit);
            Assert.AreEqual(enrPerBenId, benefit.EnrollmentPeriodBenefitId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoEnrPerBenId_ExceptionThrownTest()
        {
            benefit = new EnrollmentPeriodBenefit(bendedId, null, benTypeId);
        }

        [TestMethod]
        public void BenefitTypeId_AssignedTest()
        {
            Assert.IsNotNull(benefit);
            Assert.AreEqual(benTypeId, benefit.BenefitTypeId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoBenTypeId_ExceptionThrownTest()
        {
            benefit = new EnrollmentPeriodBenefit(bendedId, enrPerBenId, null);
        }

    }
}
