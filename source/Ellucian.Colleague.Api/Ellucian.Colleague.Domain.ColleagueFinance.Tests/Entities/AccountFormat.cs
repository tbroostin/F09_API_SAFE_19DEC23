//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class AccountFormat
    { private string guid;
    private string code;
    private string description;

    [TestInitialize]
    public void Initialize()
    {
        guid = Guid.NewGuid().ToString();
        code = "T";
        description = "Tests";
    }

    [TestMethod]
    public void AccountFormatsConstructorTest()
    {
        var entity = new AccountingFormat(guid, code, description);
        Assert.AreEqual(code, entity.Code);
        Assert.AreEqual(description, entity.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AccountFormatsConstructorNullCodeTest()
    {
        new AccountingFormat(guid, null, description);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AccountFormatsConstructorNullDescriptionTest()
    {
        new AccountingFormat(guid, code, null);
    }
}
}