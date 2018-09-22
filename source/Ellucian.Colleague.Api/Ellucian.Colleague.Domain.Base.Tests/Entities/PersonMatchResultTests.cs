// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonMatchResultTests
    {
        private string person;
        private string categoryD;
        private string categoryP;
        private string categoryZ;
        private int? scoreNull;
        private int? scoreGood;
        private PersonMatchResult result;

        [TestInitialize]
        public void Initialize()
        {
            person = "person";
            categoryD = "D";
            categoryP = "P";
            categoryZ = "Z";
            scoreGood = 100;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchResult_Constructor_NullPerson()
        {
            result = new PersonMatchResult(null, scoreGood, categoryD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchResult_Constructor_EmptyPerson()
        {
            result = new PersonMatchResult(string.Empty, scoreGood, categoryD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchResult_Constructor_NullCategory()
        {
            result = new PersonMatchResult(person, scoreGood, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PersonMatchResult_Constructor_InvalidCategory()
        {
            result = new PersonMatchResult(person, scoreGood, categoryZ);
        }

        [TestMethod]
        public void PersonMatchResult_Constructor_GoodD()
        {
            result = new PersonMatchResult(person, scoreGood, categoryD);
            Assert.AreEqual(person, result.PersonId);
            Assert.AreEqual(scoreGood, result.MatchScore);
            Assert.AreEqual(PersonMatchCategoryType.Definite, result.MatchCategory);
        }

        [TestMethod]
        public void PersonMatchResult_Constructor_GoodP()
        {
            result = new PersonMatchResult(person, scoreGood, categoryP);
            Assert.AreEqual(person, result.PersonId);
            Assert.AreEqual(scoreGood, result.MatchScore);
            Assert.AreEqual(PersonMatchCategoryType.Potential, result.MatchCategory);
        }

        [TestMethod]
        public void PersonMatchResult_Constructor_GoodNullScore()
        {
            result = new PersonMatchResult(person, scoreNull, categoryD);
            Assert.AreEqual(person, result.PersonId);
            Assert.AreEqual(default(int), result.MatchScore);
            Assert.AreEqual(PersonMatchCategoryType.Definite, result.MatchCategory);
        }
    }
}
