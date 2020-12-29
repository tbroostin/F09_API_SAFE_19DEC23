// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.QuickRegistration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.QuickRegistration
{
    [TestClass]
    public class StudentQuickRegistrationTests
    {
        private string _studentId;

        [TestInitialize]
        public void StudentQuickRegistrationTests_Initialize()
        {
            _studentId = "0001234";
        }

        [TestClass]
        public class StudentQuickRegistration_Constructor_Tests : StudentQuickRegistrationTests
        {
            [TestInitialize]
            public void StudentQuickRegistration_Constructor_Initialize()
            {
                base.StudentQuickRegistrationTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentQuickRegistration_Constructor_null_StudentId_throws_ArgumentNullException()
            {
                var entity = new StudentQuickRegistration(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentQuickRegistration_Constructor_empty_StudentId_throws_ArgumentNullException()
            {
                var entity = new StudentQuickRegistration(string.Empty);
            }

            [TestMethod]
            public void StudentQuickRegistration_Constructor_valid_StudentId_creates_object_and_initializes_properties()
            {
                var entity = new StudentQuickRegistration(_studentId);
                Assert.AreEqual(_studentId, entity.StudentId);
                Assert.AreEqual(0, entity.Terms.Count);
            }
        }

        [TestClass]
        public class StudentQuickRegistration_AddTerm_Tests : StudentQuickRegistrationTests
        {
            private StudentQuickRegistration _entity;
            [TestInitialize]
            public void StudentQuickRegistration_AddTerm_Initialize()
            {
                base.StudentQuickRegistrationTests_Initialize();
                _entity = new StudentQuickRegistration(_studentId);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentQuickRegistration_AddTerm_null_term_throws_ArgumentNullException()
            {
                _entity.AddTerm(null);
            }

            [TestMethod]
            public void StudentQuickRegistration_AddTerm_valid_term_added_to_Terms()
            {
                var term = new QuickRegistrationTerm("2019/FA");
                var term2 = new QuickRegistrationTerm("2020/SP");
                Assert.AreEqual(0, _entity.Terms.Count);
                _entity.AddTerm(term);
                Assert.AreEqual(1, _entity.Terms.Count);
                Assert.AreEqual(term.TermCode, _entity.Terms[0].TermCode);
                _entity.AddTerm(term2);
                Assert.AreEqual(2, _entity.Terms.Count);
                Assert.AreEqual(term.TermCode, _entity.Terms[0].TermCode);
                Assert.AreEqual(term2.TermCode, _entity.Terms[1].TermCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void StudentQuickRegistration_AddTerm_duplicate_term_not_added_to_Terms()
            {
                var term = new QuickRegistrationTerm("2019/FA");
                var term2 = new QuickRegistrationTerm("2019/FA");
                Assert.AreEqual(0, _entity.Terms.Count);
                _entity.AddTerm(term);
                Assert.AreEqual(1, _entity.Terms.Count);
                Assert.AreEqual(term.TermCode, _entity.Terms[0].TermCode);
                _entity.AddTerm(term2);
            }
        }
    }
}
