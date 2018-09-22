/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FafsaTests
    {
        public string id;
        public string awardYear;
        public string studentId;

        public Fafsa fafsa;

        [TestClass]
        public class FafsaConstructorTests : FafsaTests
        {
            public bool isPellEligible;
            public int? parentAgi;
            public int? studentAgi;

            [TestInitialize]
            public void Initialize()
            {
                id = "12345";
                awardYear = "2014";
                studentId = "0003914";

                isPellEligible = true;
                parentAgi = 12345;
                studentAgi = 54321;

                fafsa = new Fafsa(id, awardYear, studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                id = null;
                studentId = null;
                awardYear = null;
                fafsa = null;
            }

            [TestMethod]
            public void IsPellEligibleGetSetTest()
            {
                //init to false
                Assert.IsFalse(fafsa.IsPellEligible);

                fafsa.IsPellEligible = isPellEligible;
                Assert.AreEqual(isPellEligible, fafsa.IsPellEligible);

            }

            [TestMethod]
            public void ParentsAdjustedGrossIncomeGetSetTest()
            {
                //init to null
                Assert.IsNull(fafsa.ParentsAdjustedGrossIncome);

                fafsa.ParentsAdjustedGrossIncome = parentAgi;
                Assert.AreEqual(parentAgi, fafsa.ParentsAdjustedGrossIncome);
            }

            [TestMethod]
            public void StudentsAdjustedGrossIncomeGetSetTest()
            {
                //init to null
                Assert.IsNull(fafsa.StudentsAdjustedGrossIncome);

                fafsa.StudentsAdjustedGrossIncome = studentAgi;
                Assert.AreEqual(studentAgi, fafsa.StudentsAdjustedGrossIncome);
            }

            /// <summary>
            /// Tests if the housing code dictionary gets initialized to an empty
            /// dictionary
            /// </summary>
            [TestMethod]
            public void HousingCodesDictionary_InitializedToEmptyDictionaryTest()
            {
                Assert.IsNotNull(fafsa.HousingCodes);
                Assert.IsTrue(fafsa.HousingCodes.Count() == 0);
            }

            /// <summary>
            /// Tests if the housing codes dictionary values get set and retrieved
            /// correctly
            /// </summary>
            [TestMethod]
            public void HousingCodesDictionary_GetSetTest()
            {
                string titleIV1 = "H6785";
                HousingCode? housingCode1 = HousingCode.OffCampus;
                fafsa.HousingCodes.Add(titleIV1, housingCode1);

                Assert.AreEqual(titleIV1, fafsa.HousingCodes.FirstOrDefault().Key);
                Assert.AreEqual(housingCode1, fafsa.HousingCodes.FirstOrDefault(hc => hc.Key == titleIV1).Value);
            }

        }

    }
}
