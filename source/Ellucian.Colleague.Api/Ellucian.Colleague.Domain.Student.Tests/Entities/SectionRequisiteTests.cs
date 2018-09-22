using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionRequisiteTests
    {
        [TestClass]
        public class RequisiteConstructor_MultiSectionRequisite
        {
            private List<string> sectionIds;
            private int numberNeeded;
            private SectionRequisite requisite;

            [TestInitialize]
            public void Initialize()
            {
                sectionIds = new List<string>() { "1", "2", "3" };
                numberNeeded = 2;
                requisite = new SectionRequisite(sectionIds, numberNeeded);
            }

            [TestMethod]
            public void CorequisiteSectionIds()
            {
                Assert.AreEqual(3, requisite.CorequisiteSectionIds.Count());
            }

            [TestMethod]
            public void NumberNeeded_CanBeSet()
            {
                Assert.AreEqual(numberNeeded, requisite.NumberNeeded);
            }

            [TestMethod]
            public void Required_SetToTrue()
            {
                Assert.IsTrue(requisite.IsRequired);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsExceptionIfSingleSection()
            {
                List<string> sectionIds = new List<string>() {"123"};
                new SectionRequisite(sectionIds, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsExceptionIfNumberNeededLessThanZero()
            {
                List<string> sectionIds = new List<string>() {"123","456"};
                new SectionRequisite(sectionIds, -1);                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsExceptionIfNumberNeededGreaterThanNumberOfSections()
            {
                List<string> sectionIds = new List<string>() {"123","456"};
                new SectionRequisite(sectionIds, 3);                                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void SectionIds_NullThrowsException()
            {
                List<string> nullSectionIds = null;
                new SectionRequisite(nullSectionIds, numberNeeded);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void SectionIds_EmptyThrowsException()
            {
                List<string> emptySectionIds = new List<string>();
                new SectionRequisite(emptySectionIds, numberNeeded);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NumberNeeded_ZeroThrowsException()
            {
                new SectionRequisite(sectionIds, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NumberNeeded_NegativeThrowsException()
            {
                new SectionRequisite(sectionIds, -1);
            }
        }

        [TestClass]
        public class RequisiteConstructor_SingleSectionRequisite
        {
            private List<string> sectionIds;
            private SectionRequisite requisite;

            [TestInitialize]
            public void Initialize()
            {
                sectionIds = new List<string>() { "1", "2", "3" };
                requisite = new SectionRequisite(sectionIds.ElementAt(1));
            }

            [TestMethod]
            public void CorequisiteSectionIds()
            {
                Assert.AreEqual(1, requisite.CorequisiteSectionIds.Count());
            }

            [TestMethod]
            public void NumberNeeded_IsZero()
            {
                Assert.AreEqual(1, requisite.NumberNeeded);
            }

            [TestMethod]
            public void IsRequired_IsFalse()
            {
                Assert.IsFalse(requisite.IsRequired);
            }

            [TestMethod]
            public void SectionIds()
            {
                Assert.AreEqual("2", requisite.CorequisiteSectionIds.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionIds_NullThrowsException()
            {
                string nullSectionId = null;
                new SectionRequisite(nullSectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionIds_EmptyThrowsException()
            {
                string emptySectionId = "";
                new SectionRequisite(emptySectionId);
            }
        }
    }
}
