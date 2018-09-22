using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class ProfileApplicationTests
    {

        /// <summary>
        /// ProfileApplication just inherits FinancialAidApplication, which has its own test class.
        /// So there's nothing to test here
        /// </summary>
        [TestClass]
        public class ProfileApplicationConstructorTests
        {
            public string id;
            public string studentId;
            public string awardYear;

            public ProfileApplication profileApplication;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                studentId = "0003914";
                awardYear = "2015";

                profileApplication = new ProfileApplication(id, awardYear, studentId);
            }

            [TestMethod]
            public void ProfileApplicationTest()
            {
                Assert.AreEqual(id, profileApplication.Id);
                Assert.AreEqual(awardYear, profileApplication.AwardYear);
                Assert.AreEqual(studentId, profileApplication.StudentId);
            }
        }



    }
}
