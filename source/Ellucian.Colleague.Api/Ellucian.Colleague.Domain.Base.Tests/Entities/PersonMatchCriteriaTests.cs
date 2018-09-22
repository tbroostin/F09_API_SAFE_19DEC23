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
    public class PersonMatchCriteriaTests
    {
        private string criteria;
        private IEnumerable<PersonName> names;
        private PersonMatchCriteria result;

        [TestInitialize]
        public void Initialize()
        {
            criteria = "person";
            names = new List<PersonName>() {
                new PersonName("First","Middle","Last")
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchCriteria_Constructor_NullCriteria()
        {
            result = new PersonMatchCriteria(null, names);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchCriteria_Constructor_EmptyCriteria()
        {
            result = new PersonMatchCriteria(string.Empty, names);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchCriteria_Constructor_NullNames()
        {
            result = new PersonMatchCriteria(criteria, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PersonMatchCriteria_Constructor_NoNames()
        {
            result = new PersonMatchCriteria(criteria, new List<PersonName>());
        }

        [TestMethod]
        public void PersonMatchCriteria_Constructor_Valid()
        {
            result = new PersonMatchCriteria(criteria, names);
            Assert.AreEqual(criteria, result.MatchCriteriaIdentifier);
            Assert.AreEqual(names.Count(), result.MatchNames.Count());
        }
    }
}
