// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentRequestConfigurationTests
    {
        [TestClass]
        public class StudentRequestConfiguration_Constructor
        {           
            [TestMethod]
            public void StudentRequestConfiguration_DefaultValues()
            {
                var config = new StudentRequestConfiguration();
                Assert.IsFalse(config.SendTranscriptRequestConfirmation);
                Assert.IsFalse(config.SendEnrollmentRequestConfirmation);
                Assert.IsNull(config.DefaultWebEmailType);
            }
        }

    }
}
