// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    [TestClass]
    public class AcademicCreditsWithInvalidKeysTests
    {
        private AcademicCreditsWithInvalidKeys acadCreditsWithInvalidKeys;
        private List<AcademicCredit> academicCredits = new List<AcademicCredit>();
        private Course c;
        private List<string> invalidKeys = new List<string>();
        [TestInitialize]
        public void Initialize()
        {
            c = new TestCourseRepository().GetAsync("21").Result;
            academicCredits.Add(new AcademicCredit("1001", c, "s001"));
            academicCredits.Add(new AcademicCredit("1002", c, "s002"));

            invalidKeys.Add("1003");
            invalidKeys.Add("1004");
        }

        [TestClass]
        public class Constructor : AcademicCreditsWithInvalidKeysTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicCredit_is_Null()
            {
                acadCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(null, null);
            }
            [TestMethod]
            public void AcademicCredit_is_Empty()
            {
                acadCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(new List<AcademicCredit>(), new List<string>());
                Assert.AreEqual(0,acadCreditsWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0,acadCreditsWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }
            [TestMethod]
            public void AcademicCredits_not_null_InvalidKeys_is_null()
            {
                acadCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(academicCredits, null);
                Assert.AreEqual(academicCredits.Count(), acadCreditsWithInvalidKeys.AcademicCredits.Count());
                Assert.IsNull(acadCreditsWithInvalidKeys.InvalidAcademicCreditIds);
            }

            [TestMethod]
            public void AcademicCredits_not_null_InvalidKeys_is_empty()
            {
                acadCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(academicCredits, new List<string>());
                Assert.AreEqual(academicCredits.Count(), acadCreditsWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0,acadCreditsWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }
            [TestMethod]
            public void AcademicCredits_not_null_InvalidKeys_not_null()
            {
                acadCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(academicCredits, invalidKeys);
                Assert.AreEqual(academicCredits.Count(), acadCreditsWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(invalidKeys.Count(),acadCreditsWithInvalidKeys.InvalidAcademicCreditIds.Count());

                Assert.AreEqual(academicCredits[0].Id, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[0].Id);
                Assert.AreEqual(academicCredits[0].SectionId, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[0].SectionId);
                Assert.AreEqual(academicCredits[0].Course, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[0].Course);


                Assert.AreEqual(academicCredits[1].Id, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[1].Id);
                Assert.AreEqual(academicCredits[1].SectionId, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[1].SectionId);
                Assert.AreEqual(academicCredits[1].Course, acadCreditsWithInvalidKeys.AcademicCredits.ToList()[1].Course);

                Assert.AreEqual(invalidKeys[0], acadCreditsWithInvalidKeys.InvalidAcademicCreditIds.ToList()[0]);
                Assert.AreEqual(invalidKeys[1], acadCreditsWithInvalidKeys.InvalidAcademicCreditIds.ToList()[1]);



            }

        }
    }
}
