// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class MyProgressConfigurationTests
    {
        [TestClass]
        public class MyProgressConfiguration_Constructor
        {           


            [TestMethod]
            public void MyProgressDefaultConfiguration()
            {
                var config = new MyProgressConfiguration();
                Assert.IsFalse(config.ShowAcademicLevelsStanding);
            }

            [TestMethod]
            public void MyProgressParameterizedConfiguration()
            {
                var config = new MyProgressConfiguration(false, false, false, false);
                Assert.IsFalse(config.ShowAcademicLevelsStanding);
            }
            [TestMethod]
            public void MyProgressParameterizedConfiguration_true()
            {
                var config = new MyProgressConfiguration(true, false, false, false);
                Assert.IsTrue(config.ShowAcademicLevelsStanding);
            }
        }

    }
}
