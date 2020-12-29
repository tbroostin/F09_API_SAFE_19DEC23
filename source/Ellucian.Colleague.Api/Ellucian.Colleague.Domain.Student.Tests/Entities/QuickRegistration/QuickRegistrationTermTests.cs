// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.QuickRegistration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.QuickRegistration
{
    [TestClass]
    public class QuickRegistrationTermTests
    {
        private string _termCode;

        [TestInitialize]
        public void QuickRegistrationTermTests_Initialize()
        {
            _termCode = "2019/FA";
        }

        [TestClass]
        public class QuickRegistrationTerm_Constructor_Tests : QuickRegistrationTermTests
        {
            [TestInitialize]
            public void QuickRegistrationTerm_Constructor_Initialize()
            {
                base.QuickRegistrationTermTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void QuickRegistrationTerm_Constructor_null_TermCode_throws_ArgumentNullException()
            {
                var entity = new QuickRegistrationTerm(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void QuickRegistrationTerm_Constructor_empty_TermCode_throws_ArgumentNullException()
            {
                var entity = new QuickRegistrationTerm(string.Empty);
            }

            [TestMethod]
            public void QuickRegistrationTerm_Constructor_valid_TermCode_creates_object_and_initializes_properties()
            {
                var entity = new QuickRegistrationTerm(_termCode);
                Assert.AreEqual(_termCode, entity.TermCode);
                Assert.AreEqual(0, entity.Sections.Count);
            }
        }

        [TestClass]
        public class QuickRegistrationTerm_AddSection_Tests : QuickRegistrationTermTests
        {
            private QuickRegistrationTerm _entity;
            [TestInitialize]
            public void QuickRegistrationTerm_AddSection_Initialize()
            {
                base.QuickRegistrationTermTests_Initialize();
                _entity = new QuickRegistrationTerm(_termCode);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void QuickRegistrationTerm_AddSection_null_SectionId_throws_ArgumentNullException()
            {
                _entity.AddSection(null);
            }

            [TestMethod]
            public void QuickRegistrationTerm_AddSection_valid_section_ID_added_to_Sections()
            {
                var sectionId = "123";
                var sectionId2 = "124";
                Assert.AreEqual(0, _entity.Sections.Count);
                _entity.AddSection(new QuickRegistrationSection(sectionId, null, Student.Entities.GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted));
                Assert.AreEqual(1, _entity.Sections.Count);
                Assert.AreEqual(sectionId, _entity.Sections[0].SectionId);
                _entity.AddSection(new QuickRegistrationSection(sectionId2, null, Student.Entities.GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted));
                Assert.AreEqual(2, _entity.Sections.Count);
                Assert.AreEqual(sectionId, _entity.Sections[0].SectionId);
                Assert.AreEqual(sectionId2, _entity.Sections[1].SectionId);
            }

            [TestMethod]
            public void QuickRegistrationTerm_AddSection_duplicate_section_ID_not_added_to_Sections()
            {
                var sectionId = "123";
                Assert.AreEqual(0, _entity.Sections.Count);
                _entity.AddSection(new QuickRegistrationSection(sectionId, null, Student.Entities.GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted));
                Assert.AreEqual(1, _entity.Sections.Count);
                Assert.AreEqual(sectionId, _entity.Sections[0].SectionId);
                _entity.AddSection(new QuickRegistrationSection(sectionId, 3, Student.Entities.GradingType.PassFail, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted));
                Assert.AreEqual(1, _entity.Sections.Count);
                Assert.AreEqual(sectionId, _entity.Sections[0].SectionId);
            }
        }
    }
}
