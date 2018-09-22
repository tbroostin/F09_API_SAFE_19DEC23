// Copyright 2014 Ellucian Company .P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class PreferredSectionTests
    {
        [TestClass]
        public class PreferredSectionConstructor
        {
            private string studentId;
            private string sectionId;
            private decimal? hasCredits;
            private decimal? noCredits;
            PreferredSection preferredSectionWithCredit;
            PreferredSection preferredSectionSansCredit;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "STU001";
                sectionId = "SEC001";
                hasCredits = decimal.Parse("3.00");
                noCredits = null;

                preferredSectionWithCredit = new PreferredSection(studentId, sectionId, hasCredits);
                preferredSectionSansCredit = new PreferredSection(studentId, sectionId, noCredits);
            }

            [TestMethod]
            public void PreferredSection_StudentId()
            {
                Assert.AreEqual(studentId, preferredSectionWithCredit.StudentId);
                Assert.AreEqual(studentId, preferredSectionSansCredit.StudentId);
            }

            [TestMethod]
            public void PreferredSection_SectionId()
            {
                Assert.AreEqual(sectionId, preferredSectionWithCredit.SectionId);
                Assert.AreEqual(sectionId, preferredSectionSansCredit.SectionId);
            }

            [TestMethod]
            public void PreferredSection_Credits_WithCredits()
            {
                Assert.AreEqual(true, preferredSectionWithCredit.Credits.HasValue);
                Assert.AreEqual(hasCredits.Value, preferredSectionWithCredit.Credits.Value);
            }

            [TestMethod]
            public void PreferredSection_Credits_WithoutCredits()
            {
                Assert.AreEqual(false, preferredSectionSansCredit.Credits.HasValue);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdNullException()
            {
                preferredSectionWithCredit = new PreferredSection(null, sectionId, hasCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdEmptyException()
            {
                preferredSectionWithCredit = new PreferredSection("", sectionId, hasCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionIdNullException()
            {
                preferredSectionWithCredit = new PreferredSection(studentId, null, hasCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionIdEmptyException()
            {
                preferredSectionWithCredit = new PreferredSection(studentId, "", hasCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NegativeCreditsException()
            {
                decimal? badCredits = decimal.Parse("-1.00");
                preferredSectionWithCredit = new PreferredSection(studentId, sectionId, badCredits);
            }

        }

        [TestClass]
        public class PreferredSectionGetHashCode
        {
            private string studentId1;
            private string studentId2;
            private string sectionId1;
            private string sectionId2;

            PreferredSection prefSec1;
            PreferredSection prefSec2;
            PreferredSection prefSec3;

            [TestInitialize]
            public void Initialize()
            {
                studentId1 = "Samwise";
                studentId2 = "Pippin";
                sectionId1 = "Lorien";
                sectionId2 = "Moria";
                prefSec1 = new PreferredSection(studentId1, sectionId1, null);
                prefSec2 = new PreferredSection(studentId2, sectionId2, null);
                prefSec3 = new PreferredSection(studentId1, sectionId1, null);
            }

            [TestMethod]
            public void PreferredSection_GetHashCode()
            {
                Assert.AreEqual(prefSec1.GetHashCode(), prefSec3.GetHashCode());
                Assert.AreNotEqual(prefSec1.GetHashCode(), prefSec2.GetHashCode());
            }

        }

        [TestClass]
        public class PreferredSectionEquals
        {
            private string studentId1;
            private string studentId2;
            private string sectionId1;
            private string sectionId2;

            PreferredSection prefSec1;
            PreferredSection prefSec2;
            PreferredSection prefSec3;

            [TestInitialize]
            public void Initialize()
            {
                studentId1 = "Samwise";
                studentId2 = "Pippin";
                sectionId1 = "Lorien";
                sectionId2 = "Moria";
                prefSec1 = new PreferredSection(studentId1, sectionId1, null);
                prefSec2 = new PreferredSection(studentId2, sectionId2, null);
                prefSec3 = new PreferredSection(studentId1, sectionId1, null);
            }

            [TestMethod]
            public void PreferredSection_EqualsTest()
            {
                Assert.IsTrue(prefSec1.Equals(prefSec3));
                Assert.IsFalse(prefSec1.Equals(prefSec2));
            }

        }
    }
}
