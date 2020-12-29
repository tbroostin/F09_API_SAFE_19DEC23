// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AgreementPeriodTests
    {
        private string code;
        private string description;
        private AgreementPeriod agreementPeriod;

        [TestInitialize]
        public void Initialize()
        {
            code = "2019FA";
            description = "2019 Fall Term";
            agreementPeriod = new AgreementPeriod(code, description);
        }

        [TestClass]
        public class AgreementPeriodConstructor : AgreementPeriodTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AgreementPeriodConstructorNullCode()
            {
                agreementPeriod = new AgreementPeriod(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AgreementPeriodConstructorEmptyCode()
            {
                agreementPeriod = new AgreementPeriod(string.Empty, description);
            }

            [TestMethod]
            public void AgreementPeriodConstructorValidCode()
            {
                agreementPeriod = new AgreementPeriod(code, description);
                Assert.AreEqual(code, agreementPeriod.Code);
            }

            [TestMethod]
            public void AgreementPeriodConstructorDescription()
            {
                agreementPeriod = new AgreementPeriod(code, description);
                Assert.AreEqual(description, agreementPeriod.Description);
            }
        }
    }
}
