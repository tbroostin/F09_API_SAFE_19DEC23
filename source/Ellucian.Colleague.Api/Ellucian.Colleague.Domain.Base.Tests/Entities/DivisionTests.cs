// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class DivisionTests
    {
        [TestClass]
        public class DivisionConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string institutionId;
            private Division div;

            [TestInitialize]
            public void Initialize()
            {
                code = "DIV";
                desc = "Division";
                guid = Guid.NewGuid().ToString();
                institutionId = "000001";
                div = new Division(guid, code, desc) 
                    { InstitutionId =  institutionId};
            }
            
            [TestMethod]
            public void TypeGuid()
            {
                Assert.AreEqual(div.Guid, guid);
            }


            [TestMethod]
            public void TypeCode()
            {
                Assert.AreEqual(div.Code, code);
            }

            [TestMethod]
            public void TypeDescription()
            {
                Assert.AreEqual(div.Description, desc);
            }

            [TestMethod]
            public void TypeInstitutionId()
            {
                Assert.AreEqual(div.InstitutionId, institutionId);
            }
        }
    }
}
