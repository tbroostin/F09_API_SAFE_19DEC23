// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Exceptions
{
    [TestClass]
    public class ExistingResourceExceptionTests
    {
        [TestMethod]
        public void ExistingResourceException_Default()
        {
            var exe = new ExistingResourceException();
            Assert.AreEqual("Exception of type 'Ellucian.Colleague.Domain.Base.Exceptions.ExistingResourceException' was thrown.", exe.Message);
        }

        [TestMethod]
        public void ExistingResourceException_ExistingResourceId()
        {
            var exe = new ExistingResourceException("Resource already exists.", "123");
            Assert.AreEqual("123", exe.ExistingResourceId);
        }
    }
}
