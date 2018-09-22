// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RelationshipTests
    {
        private const string _primaryId = "PRIMARY";
        private const string _otherId = "OTHER";
        private const string _relTypeCode = "TEST";
        private const string _relDescription = "Relationship Description";
        private bool _isPrimaryRelationship = true;
        private DateTime _pastStartDate = DateTime.Now.Date.AddDays(-1);
        private DateTime _futureStartDate = DateTime.Now.Date.AddDays(1);
        private DateTime _pastEndDate = DateTime.Now.Date.AddDays(-1);
        private DateTime _futureEndDate = DateTime.Now.Date.AddDays(1);

        [TestMethod]
        public void Relationship_Constructor()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
            Assert.IsTrue(
                x.PrimaryEntity.Equals(_primaryId) &&
                x.OtherEntity.Equals(_otherId) &&
                x.RelationshipType.Equals(_relTypeCode) &&
                x.StartDate.Equals(_pastStartDate) &&
                x.EndDate.Equals(_futureEndDate)
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_PrimaryId_Null()
        {
            var x = new Relationship(null, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_PrimaryId_Empty()
        {
            var x = new Relationship(string.Empty, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_OtherId_Null()
        {
            var x = new Relationship(_primaryId, null, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_OtherId_Empty()
        {
            var x = new Relationship(_primaryId, string.Empty, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_RelationTypeCode_Null()
        {
            var x = new Relationship(_primaryId, _otherId, null, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Relationship_Constructor_RelationTypeCode_Empty()
        {
            var x = new Relationship(_primaryId, _otherId, string.Empty, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateNull_EndDateNull()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, null, null);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDatePast_EndDateNull()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, null);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDatePast_EndDateFuture()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDatePast_EndDateToday()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, DateTime.Now);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDatePast_EndDatePast()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _pastEndDate);
            Assert.IsFalse(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateToday_EndDateToday()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, DateTime.Today, DateTime.Today);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateNull_EndDatePast()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, null, _pastEndDate);
            Assert.IsFalse(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateNull_EndDateToday()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, null, DateTime.Now);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateNull_EndDateFuture()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, null, _futureEndDate);
            Assert.IsTrue(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsActive_StartDateFuture()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _futureStartDate, null);
            Assert.IsFalse(x.IsActive);
        }

        [TestMethod]
        public void Relationship_IsPrimaryRelationship_IsTrue()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, _isPrimaryRelationship, _pastStartDate, _futureEndDate);
            Assert.IsTrue(x.IsPrimaryRelationship);
        }

        [TestMethod]
        public void Relationship_IsPrimaryRelationship_IsFalse()
        {
            var x = new Relationship(_primaryId, _otherId, _relTypeCode, false, _pastStartDate, _futureEndDate);
            Assert.IsFalse(x.IsPrimaryRelationship);
        }
    }
}
