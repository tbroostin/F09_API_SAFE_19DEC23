using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AdvisementTests
    {
        Advisement advisement;
        DateTime today;
        DateTime pastDate;
        DateTime futureDate;
        string advisorId;

        [TestInitialize]
        public void Initialize()
        {
            today = DateTime.Today;
            pastDate = new DateTime(2014, 1, 1);
            futureDate = today.AddDays(100);
            advisorId = "AdvisorId";
            advisement = new Advisement(advisorId, pastDate);
        }
        
        [TestMethod]
        public void AdvisorId()
        {
            Assert.AreEqual(advisorId, advisement.AdvisorId);
        }

        [TestMethod]
        public void StartDate()
        {
            Assert.AreEqual(pastDate, advisement.StartDate);
        }

        [TestMethod]
        public void NullEndDate()
        {
            Assert.IsNull(advisement.EndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IdNullException()
        {
            Advisement badAdvisement = new Advisement(null, pastDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IdEmptyException()
        {
            Advisement badAdvisement = new Advisement(string.Empty, pastDate);
        }

        [TestMethod]
        public void NullStartDate()
        {
            Advisement anotherAdvisement = new Advisement(advisorId, null);
            Assert.IsNull(anotherAdvisement.StartDate);
        }

        [TestMethod]
        public void EndDateEntered()
        {
            Advisement anotherAdvisement = new Advisement(advisorId, pastDate);
            anotherAdvisement.EndDate = futureDate;
            Assert.AreEqual(futureDate, anotherAdvisement.EndDate);
        }

        [TestMethod]
        public void AdvisorType()
        {
            Advisement anotherAdvisement = new Advisement(advisorId, pastDate);
            anotherAdvisement.AdvisorType = "SomeType";
            Assert.AreEqual("SomeType", anotherAdvisement.AdvisorType);
        }
    }
}
