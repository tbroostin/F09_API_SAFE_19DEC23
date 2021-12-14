// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class DegreePlanNoteTests
    {
        [TestClass]
        public class DegreePlanNoteBaseConstructor
        {
            private string personId;
            private string text;
            private DegreePlanNote dpNote;

            [TestInitialize]
            public void Initialize()
            {
                personId = null;
                text = "Note text added by faculty 0000001";
                dpNote = new DegreePlanNote(text);
            }

            [TestCleanup]
            public void Cleanup()
            {
                dpNote = null;
            }

            [TestMethod]
            public void Id()
            {
                // Id defaults to zero
                Assert.AreEqual(0, dpNote.Id);
            }

            [TestMethod]
            public void PersonId()
            {
                Assert.AreEqual(personId, null);
            }

            [TestMethod]
            public void Date()
            {
                Assert.AreEqual(dpNote.Date, null);
            }

            [TestMethod]
            public void Text()
            {
                Assert.AreEqual(text, dpNote.Text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenNull()
            {
                text = null;
                dpNote = new DegreePlanNote(text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenEmpty()
            {
                text = "";
                dpNote = new DegreePlanNote(text);
            }
        }

        [TestClass]
        public class DegreePlanNoteBaseConstructor1
        {
            private string personId;
            private string text;
            private PersonType personType;
            private DegreePlanNote dpNote;

            [TestInitialize]
            public void Initialize()
            {
                personId = null;
                text = "Note text added by faculty 0000002";
                personType = Dtos.Student.PersonType.Advisor;
                dpNote = new DegreePlanNote(text, personType);
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                dpNote = null;
            }

            [TestMethod]
            public void Id()
            {
                // Id defaults to zero
                Assert.AreEqual(0, dpNote.Id);
            }

            [TestMethod]
            public void PersonId()
            {
                Assert.AreEqual(personId, null);
            }

            [TestMethod]
            public void Date()
            {
                Assert.AreEqual(dpNote.Date, null);
            }

            [TestMethod]
            public void Text()
            {
                Assert.AreEqual(text, dpNote.Text);
            }

            [TestMethod]
            public void PersonType()
            {
                Assert.AreEqual(dpNote.PersonType, Dtos.Student.PersonType.Advisor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenNull()
            {
                text = null;
                dpNote = new DegreePlanNote(text, Dtos.Student.PersonType.Advisor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenEmpty()
            {
                text = "";
                dpNote = new DegreePlanNote(text, Dtos.Student.PersonType.Advisor);
            }
        }

        [TestClass]
        public class DegreePlanNoteConstructor
        {
            private int id;
            private string personId;
            private DateTime date;
            private string text;
            private DegreePlanNote dpNote;

            [TestInitialize]
            public void Initialize()
            {
                id = 2;
                personId = "0000001";
                date = DateTime.Now;
                text = "Note text added by faculty 0000001";
                dpNote = new DegreePlanNote(id, personId, date, text);
            }

            [TestCleanup]
            public void Cleanup()
            {
                dpNote = null;
            }

            [TestMethod]
            public void Id()
            {
                // Id set to specified value
                Assert.AreEqual(id, dpNote.Id);
            }

            [TestMethod]
            public void PersonId()
            {
                Assert.AreEqual(personId, dpNote.PersonId);
            }

            [TestMethod]
            public void Date()
            {
                Assert.AreEqual(date, dpNote.Date);
            }

            [TestMethod]
            public void Text()
            {
                Assert.AreEqual(text, dpNote.Text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdThrowsExceptionWhenNull()
            {
                personId = null;
                dpNote = new DegreePlanNote(id, personId, date, text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdThrowsExceptionWhenEmpty()
            {
                personId = "";
                dpNote = new DegreePlanNote(id, personId, date, text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenNull()
            {
                text = null;
                dpNote = new DegreePlanNote(id, personId, date, text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TextThrowsExceptionWhenEmpty()
            {
                text = "";
                dpNote = new DegreePlanNote(id, personId, date, text);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void IdThrowsErrorWhenChanged()
            {
                dpNote.Id = 9;
            }
        }

        [TestClass]
        public class DegreePlanNoteEquals
        {
            DegreePlanNote dpNote1;
            DegreePlanNote dpNote2;
            DegreePlanNote dpNote3;

            [TestInitialize]
            public void Initialize()
            {
                dpNote1 = new DegreePlanNote(1, "0000010", DateTime.Now, "Note1");
                dpNote2 = new DegreePlanNote(2, "0000010", DateTime.Now, "Note2");
                dpNote3 = new DegreePlanNote(1, "0000010", DateTime.Now, "Note3");
            }

            [TestMethod]
            public void NotesWithEqualIdsAreEqual()
            {
                Assert.IsTrue(dpNote1.Equals(dpNote3));
            }

            [TestMethod]
            public void NotesWithUnequalIdsAreNotEqual()
            {
                Assert.IsFalse(dpNote1.Equals(dpNote2));
            }
        }
    }
}
