// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentPersonMatchResultTests
    {
        private List<PersonMatchResult> matchResults;

        [TestInitialize]
        public void Initialize_InstantEnrollmentPersonMatchResultTests()
        {
            matchResults = new List<PersonMatchResult>()
            {
                null,  // Nulls should be handled gracefully
                new PersonMatchResult("0003315", 100, "D"),
                new PersonMatchResult("0003316", 100, "P"),
            };
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_null_matches()
        {
            var entity = new InstantEnrollmentPersonMatchResult(null, false);
            Assert.AreEqual(null, entity.PersonId);
            Assert.AreEqual(false, entity.HasPotentialMatches);
            Assert.AreEqual(false, entity.DuplicateGovernmentIdFound);
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_no_matches()
        {
            var entity = new InstantEnrollmentPersonMatchResult(new List<PersonMatchResult>(), false);
            Assert.AreEqual(null, entity.PersonId);
            Assert.AreEqual(false, entity.HasPotentialMatches);
            Assert.AreEqual(false, entity.DuplicateGovernmentIdFound);
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_multiple_matches()
        {
            var entity = new InstantEnrollmentPersonMatchResult(matchResults, false);
            Assert.AreEqual(null, entity.PersonId);
            Assert.AreEqual(true, entity.HasPotentialMatches);
            Assert.AreEqual(false, entity.DuplicateGovernmentIdFound);
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_1_definite_match()
        {
            var entity = new InstantEnrollmentPersonMatchResult(new List<PersonMatchResult>() { matchResults[1] }, false);
            Assert.AreEqual(matchResults[1].PersonId, entity.PersonId);
            Assert.AreEqual(false, entity.HasPotentialMatches);
            Assert.AreEqual(false, entity.DuplicateGovernmentIdFound);
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_1_potential_match()
        {
            var entity = new InstantEnrollmentPersonMatchResult(new List<PersonMatchResult>() { matchResults[2] }, false);
            Assert.AreEqual(null, entity.PersonId);
            Assert.AreEqual(true, entity.HasPotentialMatches);
            Assert.AreEqual(false, entity.DuplicateGovernmentIdFound);
        }

        [TestMethod]
        public void InstantEnrollmentPersonMatchResult_duplicate_government_ID_true()
        {
            var entity = new InstantEnrollmentPersonMatchResult(new List<PersonMatchResult>() { matchResults[1] }, true);
            Assert.AreEqual(matchResults[1].PersonId, entity.PersonId);
            Assert.AreEqual(false, entity.HasPotentialMatches);
            Assert.AreEqual(true, entity.DuplicateGovernmentIdFound);
        }
    }
}
