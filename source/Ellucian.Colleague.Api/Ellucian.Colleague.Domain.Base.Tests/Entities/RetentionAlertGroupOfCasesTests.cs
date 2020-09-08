// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertGroupOfCasesTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesTests_Success_1()
        {
            RetentionAlertGroupOfCases groupOfCases = new RetentionAlertGroupOfCases()
            {
                Name = "ADVISOR",
                CaseIds = new List<string>() { "1", "2", "3" }
            };

            Assert.AreEqual(groupOfCases.NumberOfCases, groupOfCases.CaseIds.Count);
        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesTests_Success_2()
        {
            RetentionAlertGroupOfCases groupOfCases = new RetentionAlertGroupOfCases()
            {
                Name = "FACULTY",
                CaseIds = new List<string>() { "1" }
            };

            Assert.AreEqual(groupOfCases.NumberOfCases, groupOfCases.CaseIds.Count);
        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesTests_Success_3()
        {
            RetentionAlertGroupOfCases groupOfCases = new RetentionAlertGroupOfCases()
            {
                Name = "DEAN",
                CaseIds = new List<string>() {  }
            };

            Assert.AreEqual(groupOfCases.NumberOfCases, groupOfCases.CaseIds.Count);
        }
    }
}
