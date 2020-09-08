/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class EmployeeBenefitTypeTests
    {
        private string id;
        private string description;
        private string iconType;
        private string benefitsSelectionPageCustomText;
        private EmployeeBenefitType type;

        [TestInitialize]
        public void Initialize()
        {
            id = "med";
            description = "Medical benefit";
            iconType = "M";
            benefitsSelectionPageCustomText = "Testing benefits selection custom text";
            type = new EmployeeBenefitType(id, description)
            {
                BenefitTypeSpecialProcessingCode = iconType,
                BenefitsSelectionPageCustomText = benefitsSelectionPageCustomText
            };
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBenefitType_ExceptionThrownTest()
        {
            new EmployeeBenefitType(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBenefitTypeDescription_ExceptionThrownTest()
        {
            new EmployeeBenefitType(id, null);
        }

        [TestMethod]
        public void BenefitType_GetSetTest()
        {
            Assert.AreEqual(id, type.BenefitType);
        }

        [TestMethod]
        public void BenefitDescription_GetSetTest()
        {
            Assert.AreEqual(description, type.BenefitTypeDescription);
        }

        [TestMethod]
        public void BenefitIcon_GetSetTest()
        {
            Assert.AreEqual(iconType, type.BenefitTypeSpecialProcessingCode);
        }

        [TestMethod]
        public void BenefitsSelectionPageCustomText_GetSetTest()
        {
            Assert.AreEqual(benefitsSelectionPageCustomText, type.BenefitsSelectionPageCustomText);
        }
    }
}
