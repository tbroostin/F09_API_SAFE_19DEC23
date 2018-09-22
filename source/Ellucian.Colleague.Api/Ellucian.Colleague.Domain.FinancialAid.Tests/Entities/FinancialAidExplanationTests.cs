//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidExplanationTests
    {
        private string explanation;
        private FinancialAidExplanationType type;

        private FinancialAidExplanation faExplanation;

        [TestInitialize]
        public void Initialize()
        {
            explanation = "This is pell leu section explanation";
            type = FinancialAidExplanationType.PellLEU;

            faExplanation = new FinancialAidExplanation(explanation, type);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(faExplanation);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExplanationString_ExceptionThrownTest()
        {
            new FinancialAidExplanation(null, type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyExplanationString_ExceptionThrownTest()
        {
            new FinancialAidExplanation(string.Empty, type);
        }

        [TestMethod]
        public void ExplanationText_EqualsExpectedTest()
        {
            Assert.AreEqual(explanation, faExplanation.ExplanationText);
        }

        [TestMethod]
        public void ExplanationType_EqualsExpectedTest()
        {
            Assert.AreEqual(type, faExplanation.ExplanationType);
        }
    }
}
