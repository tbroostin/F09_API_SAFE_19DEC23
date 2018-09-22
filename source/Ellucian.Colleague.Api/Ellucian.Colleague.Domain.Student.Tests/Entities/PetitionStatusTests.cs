// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class PetitionStatusTests
    {
        [TestMethod]
        public void PetitionStatus_Constructor_DefaultIsGranted()
        {
            string code = "DEN";
            string description = "Denied";
            var petitionStatus = new PetitionStatus(code, description);
            Assert.AreEqual(code, petitionStatus.Code);
            Assert.AreEqual(description, petitionStatus.Description);
            Assert.IsFalse(petitionStatus.IsGranted);
        }

        [TestMethod]
        public void PetitionStatus_Constructor_SetIsGranted()
        {
            string code = "ACC";
            string description = "Accepted";
            var petitionStatus = new PetitionStatus(code, description, true);
            Assert.AreEqual(code, petitionStatus.Code);
            Assert.AreEqual(description, petitionStatus.Description);
            Assert.IsTrue(petitionStatus.IsGranted);
        }
    }
}
