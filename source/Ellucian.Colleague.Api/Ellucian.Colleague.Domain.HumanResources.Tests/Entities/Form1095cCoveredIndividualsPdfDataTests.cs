// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class Form1095cCoveredIndividualsPdfDataTests
    {
        [TestMethod]
        public void Constructor()
        {
            var coveredIndividual = new Form1095cCoveredIndividualsPdfData();
            Assert.AreEqual("", coveredIndividual.CoveredIndividualSsn);
        }
    }
}