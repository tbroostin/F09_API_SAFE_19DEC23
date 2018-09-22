// Copyright 2018 Ellucian Company L.P. and its affiliates.using System;

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GlAccountValidationResponseTests
    {
        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion
        
        [TestMethod]
        public void GlAccountValidationResponse_Constructor()
        {
            string glAccount = "10_00_01_01_33333_51001";
            var glAccountValidationRespone = new GlAccountValidationResponse(glAccount);

            Assert.AreEqual(glAccount, glAccountValidationRespone.Id);
        }
    }
}