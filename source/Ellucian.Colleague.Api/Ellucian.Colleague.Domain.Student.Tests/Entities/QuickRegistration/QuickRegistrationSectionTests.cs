// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.QuickRegistration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.QuickRegistration
{
    [TestClass]
    public class QuickRegistrationSectionTests
    {
        private string _sectionId;
        private decimal? _credits;
        private GradingType _gradingType;
        private Domain.Student.Entities.DegreePlans.WaitlistStatus _waitlistStatus;

        [TestInitialize]
        public void QuickRegistrationSectionTests_Initialize()
        {
            _sectionId = "123";
            _credits = 3m;
            _gradingType = GradingType.Graded;
            _waitlistStatus = Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
        }

        [TestClass]
        public class QuickRegistrationSection_Constructor_Tests : QuickRegistrationSectionTests
        {
            [TestInitialize]
            public void QuickRegistrationSection_Constructor_Initialize()
            {
                base.QuickRegistrationSectionTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void QuickRegistrationSection_Constructor_null_SectionId_throws_ArgumentNullException()
            {
                var entity = new QuickRegistrationSection(null, _credits, _gradingType, _waitlistStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void QuickRegistrationSection_Constructor_empty_SectionId_throws_ArgumentNullException()
            {
                var entity = new QuickRegistrationSection(string.Empty, _credits, _gradingType, _waitlistStatus);
            }

            [TestMethod]
            public void QuickRegistrationSection_Constructor_valid_SectionId_creates_object_and_initializes_properties()
            {
                var entity = new QuickRegistrationSection(_sectionId, _credits, _gradingType, _waitlistStatus);
                Assert.AreEqual(_sectionId, entity.SectionId);
                Assert.AreEqual(_credits, entity.Credits);
                Assert.AreEqual(_gradingType, entity.GradingType);
                Assert.AreEqual(_waitlistStatus, entity.WaitlistStatus);
            }
        }
    }
}
