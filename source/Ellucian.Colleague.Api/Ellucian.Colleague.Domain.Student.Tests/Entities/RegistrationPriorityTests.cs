using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationPriorityTests
    {
        RegistrationPriority regPriority;
        string id = "1";
        string studentId = "0000001";
        string termCode = "2014/FA";
        DateTime? start = new DateTime(2014, 8, 1, 0, 0, 0);
        DateTime? end = new DateTime(2014, 8, 1, 8, 59, 59);

        [TestInitialize]
        public void Initialize()
        {
            regPriority = new RegistrationPriority(id, studentId, termCode, start, end);
        }

        [TestMethod]
        public void Id()
        {
            Assert.AreEqual(id, regPriority.Id);
        }

        [TestMethod]
        public void StudentId()
        {
            Assert.AreEqual(studentId, regPriority.StudentId);
        }

        [TestMethod]
        public void TermCode()
        {
            Assert.AreEqual(termCode, regPriority.TermCode);
        }

        [TestMethod]
        public void Start()
        {
            Assert.AreEqual(start.Value, regPriority.Start.Value);
        }

        [TestMethod]
        public void End()
        {
            Assert.AreEqual(end.Value, regPriority.End.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyStudentIdNotAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, "", termCode, start, end);
            Assert.IsNull(test.TermCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentIdNotAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, null, termCode, start, end);
            Assert.IsNull(test.TermCode);
        }

        [TestMethod]
        public void EmptyTermCodeAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, studentId, "", start, end);
            Assert.IsNull(test.TermCode);
        }

        [TestMethod]
        public void NullTermCodeAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, studentId, null, start, end);
            Assert.IsNull(test.TermCode);
        }


        [TestMethod]
        public void NullStartAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, studentId, "2014/FA", null, end);
            Assert.IsNull(test.Start);
        }

        [TestMethod]
        public void NullEndAllowed()
        {
            RegistrationPriority test = new RegistrationPriority(id, studentId, "2014/FA", start, null);
            Assert.IsNull(test.End);
        }

    }
}
