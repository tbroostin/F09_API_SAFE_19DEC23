// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class NonAcademicAttendanceRequirementTests
    {
        private string id;
        private string personId;
        private string termCode;
        private List<string> nonAcademicAttendanceIds;
        private decimal? defaultRequiredUnits;
        private decimal? requiredUnitsOverride;

        private NonAcademicAttendanceRequirement entity;

        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            personId = "0001234";
            termCode = "2017/FA";
            nonAcademicAttendanceIds = new List<string>() { "456", "457", "458", "459", "460" };
            defaultRequiredUnits = 30m;
            requiredUnitsOverride = 27m;
        }

        [TestClass]
        public class NonAcademicAttendanceRequirement_Constructor_Tests : NonAcademicAttendanceRequirementTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendanceRequirement_Constructor_Null_Id()
            {
                entity = new NonAcademicAttendanceRequirement(null, personId, termCode, null, defaultRequiredUnits, requiredUnitsOverride);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendanceRequirement_Constructor_Null_PersonId()
            {
                entity = new NonAcademicAttendanceRequirement(id, null, termCode, null, defaultRequiredUnits, requiredUnitsOverride);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendanceRequirement_Constructor_Null_TermCode()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, null, null, defaultRequiredUnits, requiredUnitsOverride);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NonAcademicAttendanceRequirement_Constructor_Invalid_DefaultRequiredUnits()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, null, -30m, requiredUnitsOverride);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NonAcademicAttendanceRequirement_Constructor_Invalid_RequiredUnitsOverride()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, null, defaultRequiredUnits, -30m);
            }

            [TestMethod]
            public void NonAcademicAttendanceRequirement_Constructor_Default_Values()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode);
                Assert.AreEqual(id, entity.Id);
                Assert.AreEqual(personId, entity.PersonId);
                Assert.AreEqual(termCode, entity.TermCode);
                CollectionAssert.AreEqual(new List<string>(), entity.NonAcademicAttendanceIds);
                Assert.IsNull(entity.DefaultRequiredUnits);
                Assert.IsNull(entity.RequiredUnitsOverride);
            }

            [TestMethod]
            public void NonAcademicAttendanceRequirement_Constructor_with_NonAcademicAttendanceIds()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, nonAcademicAttendanceIds);
                CollectionAssert.AreEqual(nonAcademicAttendanceIds, entity.NonAcademicAttendanceIds);
            }

            [TestMethod]
            public void NonAcademicAttendanceRequirement_Constructor_with_NonAcademicAttendanceIds_nulls_and_duplicates_ignored()
            {
                var nonAcademicAttendanceIdsWithNullsAndDuplicates = new List<string>(nonAcademicAttendanceIds);
                nonAcademicAttendanceIdsWithNullsAndDuplicates.Add(null);
                nonAcademicAttendanceIdsWithNullsAndDuplicates.Add(nonAcademicAttendanceIds[0]);

                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, nonAcademicAttendanceIdsWithNullsAndDuplicates);
                CollectionAssert.AreEqual(nonAcademicAttendanceIds, entity.NonAcademicAttendanceIds);
            }
        }

        [TestClass]
        public class NonAcademicAttendanceRequirement_RequiredUnits_Tests : NonAcademicAttendanceRequirementTests
        {
            [TestMethod]
            public void NonAcademicAttendanceRequirement_RequiredUnits_returns_RequiredUnitsOverride_if_specified()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, null, null, requiredUnitsOverride);
                Assert.AreEqual(entity.RequiredUnitsOverride, entity.RequiredUnits);
            }

            [TestMethod]
            public void NonAcademicAttendanceRequirement_RequiredUnits_returns_DefaultRequiredUnitsOverride_if_RequiredUnitsOverride_not_specified()
            {
                entity = new NonAcademicAttendanceRequirement(id, personId, termCode, null, defaultRequiredUnits, null);
                Assert.AreEqual(entity.DefaultRequiredUnits, entity.RequiredUnits);
            }
        }

    }
}
