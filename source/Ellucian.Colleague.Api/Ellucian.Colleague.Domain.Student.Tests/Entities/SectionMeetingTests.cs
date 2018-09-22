using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionMeetingTests
    {
        [TestClass]
        public class SectionMeeting_Constructor
        {
            string guid;
            string id;
            string sectionId;
            SectionMeeting mp;
            string instMethod;
            DateTime startDate = new DateTime(2013, 8, 15);
            DateTime endDate = new DateTime(2013, 12, 15);
            string frequency = "W";

            [TestInitialize]
            public void SectionMeeting_Initialize()
            {
                guid = Guid.NewGuid().ToString();
                id = "12345";
                sectionId = "987";
                instMethod = "LEC";
                mp = new SectionMeeting(id, sectionId, instMethod, startDate, endDate, frequency);
            }

            [TestMethod]
            public void SectionMeeting_Guid()
            {
                mp.Guid = guid;
                Assert.AreEqual(guid, mp.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SectionMeeting_Guid_ChangeException()
            {
                mp.Guid = guid;
                mp.Guid = "234";
            }

            [TestMethod]
            public void SectionMeeting_Id()
            {
                Assert.AreEqual(id, mp.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SectionMeeting_Id_ChangeException()
            {
                mp.Id = "234";
            }

            [TestMethod]
            public void SectionMeeting_SectionId()
            {
                Assert.AreEqual(sectionId, mp.SectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SectionMeeting_SectionId_ChangeException()
            {
                mp.SectionId = "234";
            }

            [TestMethod]
            public void InstrMethod()
            {
                Assert.AreEqual(instMethod, mp.InstructionalMethodCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeeting_InstrMethod_NullException()
            {
                new SectionMeeting(id, sectionId, null, startDate, endDate, frequency);
            }

            [TestMethod]
            public void SectionMeeting_StartDate()
            {
                Assert.AreEqual(startDate, mp.StartDate);
            }

            [TestMethod]
            public void SectionMeeting_EndDate()
            {
                Assert.AreEqual(endDate, mp.EndDate);
            }

            [TestMethod]
            public void SectionMeeting_Frequency()
            {
                Assert.AreEqual(frequency, mp.Frequency);
            }

            [TestMethod]
            public void SectionMeeting_IsOnline_False()
            {
                // Defaults to false
                Assert.IsFalse(mp.IsOnline);
            }

            [TestMethod]
            public void SectionMeeting_IsOnline_True()
            {
                mp.IsOnline = true;
                Assert.IsTrue(mp.IsOnline);
            }
        }
    }
}
