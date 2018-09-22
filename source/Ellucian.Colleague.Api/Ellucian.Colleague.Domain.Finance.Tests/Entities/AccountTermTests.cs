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
    public class AccountTermTests
    {
        [TestMethod]
        public void AccountTerm_AccountDetails()
        {
            var at = new AccountTerm();
            CollectionAssert.AreEqual(new List<AccountsReceivableDueItem>(), at.AccountDetails);
        }
    }
}
