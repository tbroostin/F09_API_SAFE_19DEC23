// Copyright 2017 Ellucian Company L.P. and its affiliates.using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerAccountTests
    {
        #region Initialize and Cleanup
        GeneralLedgerAccountBuilder glAccountBuilder;
        private List<string> majorComponentStartPositions;

        [TestInitialize]
        public void Initialize()
        {
            glAccountBuilder = new GeneralLedgerAccountBuilder();
            majorComponentStartPositions = new List<string>() { "1", "4", "7", "10", "13", "19" };
        }

        [TestCleanup]
        public void Cleanup()
        {
            glAccountBuilder = null;
            majorComponentStartPositions = null;
        }
        #endregion
        
        [TestMethod]
        public void Contrsuctor()
        {
            var glAccountId = "10_00_01_01_33333_51001";
            var glAccount = glAccountBuilder.WithGlNumber(glAccountId)
                .WithStartPositions(majorComponentStartPositions).Build();

            Assert.AreEqual(glAccountId, glAccount.Id);
        }

        #region Tests for GetFormattedGlAccount
        [TestMethod]
        public void GetFormattedGlAccount_LongGLNumber()
        {
            var glAccountId = "10_00_01_01_33333_51001";
            var glAccount = glAccountBuilder.WithGlNumber(glAccountId)
                .WithStartPositions(majorComponentStartPositions).Build();
            Assert.AreEqual(glAccount.Id.Replace("_", "-"), glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_GlNumberLengthExactly15()
        {
            string expectedGlAccountNumber = "01-02-0304050-6077";
            var startPositions = new List<string>() { "1", "3", "5", "12" };
            var glAccount = glAccountBuilder.WithGlNumber("010203040506077")
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_ShortGLNumber()
        {
            string expectedGlAccountNumber = "01-0203-0405";
            var startPositions = new List<string>() { "1", "3", "7" };
            var glAccount = glAccountBuilder.WithGlNumber("0102030405")
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_TypicalShortGlNumber()
        {
            string expectedGlAccountNumber = "10-0000-65030-01";
            var startPositions = new List<string>() { "1", "3", "7", "12" };
            var glAccount = glAccountBuilder.WithGlNumber("1000006503001")
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_InvalidStartPosition()
        {
            string expectedGlAccountNumber = "010203-0405";
            var startPositions = new List<string>() { "0", "7" };
            var glAccount = glAccountBuilder.WithGlNumber("0102030405")
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_StartPositionsIsNull()
        {
            string expectedGlAccountNumber = "0102030405";
            List<string> startPositions = null;
            var glAccount = glAccountBuilder.WithGlNumber(expectedGlAccountNumber)
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }

        [TestMethod]
        public void GetFormattedGlAccount_StartPositionsIsEmpty()
        {
            string expectedGlAccountNumber = "0102030405";
            List<string> startPositions = new List<string>();
            var glAccount = glAccountBuilder.WithGlNumber(expectedGlAccountNumber)
                .WithStartPositions(startPositions).Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.FormattedGlAccount);
        }
        #endregion
    }
}