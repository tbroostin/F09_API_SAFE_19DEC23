using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class OverrideTests
    {
        private TestProgramRequirementsRepository tprr;
        private TestAcademicCreditRepository tacr;

        //private IDictionary<string, string> cred;  // for credits not enumerated below

        private string hist100;
        private string hist200;
        private string math100;
        private string math200;
        private string groupid = "100";


        [TestInitialize]
        public void MyTestInitialize()
        {
            tprr = new TestProgramRequirementsRepository();
            tacr = new TestAcademicCreditRepository();

            hist100 = tacr.GetAsync("HIST*100").Result.Id;
            hist200 = tacr.GetAsync("HIST*200").Result.Id;
            math100 = tacr.GetAsync("MATH*100").Result.Id;
            math200 = tacr.GetAsync("MATH*200").Result.Id;

        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            tprr = null;
            tacr = null;
        }

        [TestClass]
        public class OverrideConstructor : OverrideTests
        {

            [TestMethod]
            public void OverrideConstructorTest_IdPopulated()
            {
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = new List<string>() { math100 };
                Override target = new Override(groupid, allowed, denied);
                Assert.AreEqual("100", target.GroupId);

            }
            [TestMethod]
            public void OverrideConstructorTest_DenyListNull_NoException()
            {
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = null;
                Override target = new Override(groupid, allowed, denied);
            }
            [TestMethod]
            public void OverrideConstructorTest_AllowListNull_NoException()
            {
                IEnumerable<string> allowed = null;
                IEnumerable<string> denied = new List<string>() { hist100, hist200 };
                Override target = new Override(groupid, allowed, denied);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OverrideConstructorTest_GroupNull_ThrowsException()
            {
                string nullgroupid = null;
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = new List<string>() { math100, math200 };
                Override target = new Override(nullgroupid, allowed, denied);
            }
            [TestMethod]
            [ExpectedException(typeof(NotSupportedException))]
            public void OverrideConstructorTest_ListsIntersect_ThrowsException()
            {
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = new List<string>() { hist100, hist200 };
                Override target = new Override(groupid, allowed, denied);
            }

        }
        [TestClass]
        public class AllowsCredit : OverrideTests
        {
            [TestMethod]
            public void OverrideAllowsCredit()
            {
                IEnumerable<AcademicCredit> allowedcred = tacr.GetAsync(new List<string>() { "HIST-100", "HIST-200" }).Result;
                IEnumerable<AcademicCredit> deniedcred = tacr.GetAsync(new List<string>() { "MATH-100" }).Result;
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = new List<string>() { math100 };

                Override target = new Override(groupid, allowed, denied);

                foreach (AcademicCredit ac in allowedcred)
                {
                    Assert.IsTrue(target.AllowsCredit(ac.Id));
                }
                foreach (AcademicCredit ac in deniedcred)
                {
                    Assert.IsFalse(target.AllowsCredit(ac.Id));
                }
            }
        }
        [TestClass]
        public class DeniesCredit : OverrideTests
        {
            [TestMethod]
            public void OverrideDeniesCredit()
            {
                IEnumerable<AcademicCredit> allowedcred = tacr.GetAsync(new List<string>() { "HIST-100", "HIST-200" }).Result;
                IEnumerable<AcademicCredit> deniedcred = tacr.GetAsync(new List<string>() { "MATH-100" }).Result;
                IEnumerable<string> allowed = new List<string>() { hist100, hist200 };
                IEnumerable<string> denied = new List<string>() { math100 };

                Override target = new Override(groupid, allowed, denied);

                foreach (AcademicCredit ac in allowedcred)
                {
                    Assert.IsFalse(target.DeniesCredit(ac.Id));
                }
                foreach (AcademicCredit ac in deniedcred)
                {
                    Assert.IsTrue(target.DeniesCredit(ac.Id));
                }
            }
        }
    }
}
