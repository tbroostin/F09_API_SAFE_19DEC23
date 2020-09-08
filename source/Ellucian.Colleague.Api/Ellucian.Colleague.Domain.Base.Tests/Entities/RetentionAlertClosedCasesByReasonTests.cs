using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertClosedCasesByReasonTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }
        [TestMethod]
        public void RetentionAlertClosedCasesByReasonTests_Success_0()
        {
            RetentionAlertClosedCasesByReason item = new RetentionAlertClosedCasesByReason();

            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Cases);
        }

        [TestMethod]
        public void RetentionAlertClosedCasesByReasonTests_Success_1()
        {
            RetentionAlertClosedCasesByReason item = new RetentionAlertClosedCasesByReason()
            {
                ClosureReasonId = "1"               
            };

            Assert.IsNotNull(item);
            //Assert.IsNotNull(item.CaseIds);
        }

        [TestMethod]
        public void RetentionAlertClosedCasesByReasonTests_Success_2()
        {
            
            RetentionAlertClosedCasesByReason item = new RetentionAlertClosedCasesByReason()
            {
                ClosureReasonId = "1",
                Cases = new List<RetentionAlertClosedCase>()
            };
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Cases);
            
        }

        [TestMethod]
        public void RetentionAlertClosedCasesByReasonTests_Success_3()
        {
            RetentionAlertClosedCasesByReason item = new RetentionAlertClosedCasesByReason()
            {
                ClosureReasonId = "1",
                Cases = new List<RetentionAlertClosedCase>()
                {
                    new RetentionAlertClosedCase()
                    {
                        CasesId = "1",
                        LastActionDate = new DateTime(2020, 1, 1)
                    }
                }
            };
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Cases);            
        }

        [TestMethod]
        public void RetentionAlertClosedCasesByReasonTests_Success_4()
        {
            RetentionAlertClosedCasesByReason item = new RetentionAlertClosedCasesByReason()
            {
                ClosureReasonId = "1",
                Cases = new List<RetentionAlertClosedCase>()
                {
                    new RetentionAlertClosedCase()
                    {
                        CasesId = "1",
                        LastActionDate = new DateTime(2020, 1, 1)
                    },
                    new RetentionAlertClosedCase()
                    {
                        CasesId = "2",
                        LastActionDate = new DateTime(2020, 2, 1)
                    }
                }
            };
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Cases);
        }
    }
}
