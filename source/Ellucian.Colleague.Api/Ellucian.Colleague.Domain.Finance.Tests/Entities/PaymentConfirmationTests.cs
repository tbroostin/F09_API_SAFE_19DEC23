// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentConfirmationTests
    {
        [TestMethod]
        public void PaymentConfirmation_ConfirmationText()
        {
            var pmtConf = new PaymentConfirmation();
            CollectionAssert.AreEqual(new List<string>(), pmtConf.ConfirmationText);
        }
    }
}
