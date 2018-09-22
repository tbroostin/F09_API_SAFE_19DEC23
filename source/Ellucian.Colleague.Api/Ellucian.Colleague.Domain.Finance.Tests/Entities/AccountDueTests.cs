// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class AccountDueTests
    {
        [TestMethod]
        public void AccountDue_AccountTerms()
        {
            var ad = new AccountDue();
            CollectionAssert.AreEqual(new List<AccountTerm>(), ad.AccountTerms);
        }
    }
}
