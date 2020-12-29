// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class CompletedAdvisementTests
    {
        private CompletedAdvisement entity;
        private DateTime today = DateTime.Today;
        private DateTimeOffset now = DateTimeOffset.Now;
        private string advisorId = "0001234";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompletedAdvisement_Null_AdvisorId()
        {
            entity = new CompletedAdvisement(today, now, null); 
        }

        [TestMethod]
        public void CompletedAdvisement_Valid()
        {
            entity = new CompletedAdvisement(today, now, advisorId);
            Assert.AreEqual(today, entity.CompletionDate);
            Assert.AreEqual(now, entity.CompletionTime);
            Assert.AreEqual(advisorId, entity.AdvisorId);
        }

    }
}
