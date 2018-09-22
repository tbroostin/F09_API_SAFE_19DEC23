// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionRosterTests
    {
        private string sectionId;
        private string personId;
        private string personId2;
        private SectionRoster entity;

		[TestInitialize]
		public void Initialize()
        {
            sectionId = "123";
            personId = "0001234";
            personId2 = "0001235";
        }

        [TestClass]
        public class SectionRoster_Constructor_Tests : SectionRosterTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRoster_Constructor_Null_SectionId()
            {
                entity = new SectionRoster(null);
            }

            [TestMethod]
            public void SectionRoster_Constructor_Valid()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.StudentIds);
                Assert.IsFalse(entity.StudentIds.Any());
            }
        }

        [TestClass]
        public class SectionRoster_AddStudentId_Tests : SectionRosterTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRoster_AddStudentId_Null_StudentId()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.StudentIds);
                Assert.IsFalse(entity.StudentIds.Any());
                entity.AddStudentId(null);
            }

            [TestMethod]
            public void SectionRoster_AddStudentId_Valid()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.StudentIds);
                Assert.IsFalse(entity.StudentIds.Any());
                entity.AddStudentId(personId);
                Assert.AreEqual(1, entity.StudentIds.Count);
                Assert.AreEqual(personId, entity.StudentIds[0]);
                entity.AddStudentId(personId2);
                Assert.AreEqual(2, entity.StudentIds.Count);
                Assert.AreEqual(personId, entity.StudentIds[0]);
                Assert.AreEqual(personId2, entity.StudentIds[1]);
            }

            [TestMethod]
            public void SectionRoster_AddStudentId_Ignores_Duplicates()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.StudentIds);
                Assert.IsFalse(entity.StudentIds.Any());
                entity.AddStudentId(personId);
                Assert.AreEqual(1, entity.StudentIds.Count);
                Assert.AreEqual(personId, entity.StudentIds[0]);
                entity.AddStudentId(personId);
                Assert.AreEqual(1, entity.StudentIds.Count);
                Assert.AreEqual(personId, entity.StudentIds[0]);
            }
        }

        [TestClass]
        public class SectionRoster_AddFacultyId_Tests : SectionRosterTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRoster_AddFacultyId_Null_FacultyId()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.FacultyIds);
                Assert.IsFalse(entity.FacultyIds.Any());
                entity.AddFacultyId(null);
            }

            [TestMethod]
            public void SectionRoster_AddFacultyId_Valid()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.FacultyIds);
                Assert.IsFalse(entity.FacultyIds.Any());
                entity.AddFacultyId(personId);
                Assert.AreEqual(1, entity.FacultyIds.Count);
                Assert.AreEqual(personId, entity.FacultyIds[0]);
                entity.AddFacultyId(personId2);
                Assert.AreEqual(2, entity.FacultyIds.Count);
                Assert.AreEqual(personId, entity.FacultyIds[0]);
                Assert.AreEqual(personId2, entity.FacultyIds[1]);
            }

            [TestMethod]
            public void SectionRoster_AddFacultyId_Ignores_Duplicates()
            {
                entity = new SectionRoster(sectionId);
                Assert.AreEqual(sectionId, entity.SectionId);
                Assert.IsNotNull(entity.FacultyIds);
                Assert.IsFalse(entity.FacultyIds.Any());
                entity.AddFacultyId(personId);
                Assert.AreEqual(1, entity.FacultyIds.Count);
                Assert.AreEqual(personId, entity.FacultyIds[0]);
                entity.AddFacultyId(personId);
                Assert.AreEqual(1, entity.FacultyIds.Count);
                Assert.AreEqual(personId, entity.FacultyIds[0]);
            }
        }

    }
}
