using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertGroupOfCasesSummaryTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesSummaryTests_Success_1()
        {
            RetentionAlertGroupOfCasesSummary groupOfCasesSummary = new RetentionAlertGroupOfCasesSummary()
            {
                Summary = "Advising Alert"
            };

            Assert.AreEqual(0, groupOfCasesSummary.TotalCases);
        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesSummaryTests_Success_2()
        {
            RetentionAlertGroupOfCasesSummary groupOfCasesSummary = new RetentionAlertGroupOfCasesSummary()
            {
                Summary = "Advising Alert"
            };

            RetentionAlertGroupOfCases groupOfCases1 = new RetentionAlertGroupOfCases()
            {
                Name = "ADVISOR",
                CaseIds = new List<string>() { "1", "2", "3" }
            };

            RetentionAlertGroupOfCases groupOfCases2 = new RetentionAlertGroupOfCases()
            {
                Name = "FACULTY",
                CaseIds = new List<string>() { "3", "4", "5" }
            };

            groupOfCasesSummary.AddRoleCase(groupOfCases1);
            groupOfCasesSummary.AddRoleCase(groupOfCases2);

            Assert.IsNotNull(groupOfCasesSummary);
            Assert.AreEqual(5, groupOfCasesSummary.TotalCases);
            
        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesSummaryTests_Success_3()
        {
            RetentionAlertGroupOfCasesSummary groupofCaseSummary = new RetentionAlertGroupOfCasesSummary()
            {
                Summary = "Advising Alert"
            };

            RetentionAlertGroupOfCases g1 = new RetentionAlertGroupOfCases()
            {
                Name = "Dean Smith",
                CaseIds = new List<string>() { "100", "200", "300", "400" }
            };

            RetentionAlertGroupOfCases g2 = new RetentionAlertGroupOfCases()
            {
                Name = "Advisor Yoder",
                CaseIds = new List<string>() { "100", "300" }
            };

            RetentionAlertGroupOfCases g3 = new RetentionAlertGroupOfCases()
            {
                Name = "Advisor Jones",
                CaseIds = new List<string>() { "200", "400" }
            };

            groupofCaseSummary.AddEntityCase(g1);
            groupofCaseSummary.AddEntityCase(g2);
            groupofCaseSummary.AddEntityCase(g3);

            Assert.IsNotNull(groupofCaseSummary);
            Assert.AreEqual(4, groupofCaseSummary.TotalCases);
        }

        [TestMethod]
        public void RetentionAlertGroupOfCasesSummaryTests_Success_4()
        {
            RetentionAlertGroupOfCasesSummary groupofCaseSummary = new RetentionAlertGroupOfCasesSummary()
            {
                Summary = "Advising Alert"
            };

            RetentionAlertGroupOfCases g1 = new RetentionAlertGroupOfCases()
            {
                Name = "Dean Smith",
                CaseIds = new List<string>() { "100", "200", "300", "400" }
            };

            RetentionAlertGroupOfCases g2 = new RetentionAlertGroupOfCases()
            {
                Name = "Advisor Yoder",
                CaseIds = new List<string>() { "100", "300" }
            };

            RetentionAlertGroupOfCases g3 = new RetentionAlertGroupOfCases()
            {
                Name = "Advisor Jones",
                CaseIds = new List<string>() { "200", "400" }
            };

            RetentionAlertGroupOfCases g4 = new RetentionAlertGroupOfCases()
            {
                Name = "ADVISOR",
                CaseIds = new List<string>() { "1", "2", "3" }
            };

            RetentionAlertGroupOfCases g5 = new RetentionAlertGroupOfCases()
            {
                Name = "FACULTY",
                CaseIds = new List<string>() { "3", "4", "5" }
            };

            groupofCaseSummary.AddEntityCase(g1);
            groupofCaseSummary.AddEntityCase(g2);
            groupofCaseSummary.AddEntityCase(g3);
            groupofCaseSummary.AddRoleCase(g4);
            groupofCaseSummary.AddRoleCase(g5);

            Assert.IsNotNull(groupofCaseSummary);
            Assert.AreEqual(9, groupofCaseSummary.TotalCases);
        }
    }
}
