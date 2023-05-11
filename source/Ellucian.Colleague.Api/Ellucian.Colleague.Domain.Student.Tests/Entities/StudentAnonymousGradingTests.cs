// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.AnonymousGrading
{
    [TestClass]
    public class StudentAnonymousGradingTests
    {
        [TestInitialize]
        public void StudentAnonymousGrading_Initialize()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAnonymousGrading_null_anonymousGradingId_Message_throws_ArgumentNullException()
        {
            var entity = new StudentAnonymousGrading(null, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAnonymousGrading_null_termId_sectionId_Message_throws_ArgumentNullException()
        {
            var entity = new StudentAnonymousGrading("1234", string.Empty, null, string.Empty);
        }

        [TestMethod]
        public void StudentAnonymousGrading_WithoutMidTermGradingId_section()
        {
            var entity = new StudentAnonymousGrading("1234", null, "s01", null);
            Assert.IsNotNull(entity);
            Assert.AreEqual("1234", entity.AnonymousGradingId);
            Assert.AreEqual(null, entity.MidTermGradingId);
            Assert.AreEqual("s01", entity.SectionId);
            Assert.AreEqual(null, entity.TermId);
            Assert.AreEqual(null, entity.Message);
        }

        [TestMethod]
        public void StudentAnonymousGrading_WithoutMidTermGradingId_term()
        {
            var entity = new StudentAnonymousGrading("1234", "2022FA", string.Empty, string.Empty);
            Assert.IsNotNull(entity);
            Assert.AreEqual("1234", entity.AnonymousGradingId);
            Assert.AreEqual(null, entity.MidTermGradingId);
            Assert.AreEqual(string.Empty, entity.SectionId);
            Assert.AreEqual("2022FA", entity.TermId);
            Assert.AreEqual(string.Empty, entity.Message);
        }
        [TestMethod]
        public void StudentAnonymousGrading_WithMidTermGradingId_section()
        {
            var entity = new StudentAnonymousGrading("1234","m1234" , null, "s01", null);
            Assert.IsNotNull(entity);
            Assert.AreEqual("1234", entity.AnonymousGradingId);
            Assert.AreEqual("m1234", entity.MidTermGradingId);
            Assert.AreEqual("s01", entity.SectionId);
            Assert.AreEqual(null, entity.TermId);
            Assert.AreEqual(null, entity.Message);
        }

        [TestMethod]
        public void StudentAnonymousGrading_WithMidTermGradingId_term()
        {
            var entity = new StudentAnonymousGrading("1234","m1234", "2022FA", string.Empty, string.Empty);
            Assert.IsNotNull(entity);
            Assert.AreEqual("1234", entity.AnonymousGradingId);
            Assert.AreEqual("m1234", entity.MidTermGradingId);
            Assert.AreEqual(string.Empty, entity.SectionId);
            Assert.AreEqual("2022FA", entity.TermId);
            Assert.AreEqual(string.Empty, entity.Message);
        }
    }
}
