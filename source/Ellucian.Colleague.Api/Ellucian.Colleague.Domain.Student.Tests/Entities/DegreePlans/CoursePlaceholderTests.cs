// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class CoursePlaceholderTests
    {
        private string id = "1";
        private string title = "Placeholder 1 Title";
        private string description = "Placeholder 1 Description";
        private DateTime? startDate = DateTime.Today.AddDays(-7);
        private DateTime? endDate = DateTime.Today.AddDays(7);
        private string credits = "3 to 5 credits";

        private CoursePlaceholder entity;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CoursePlaceholder_null_ID_throws_ArgumentNullException()
        {
            entity = new CoursePlaceholder(null, title, description, startDate, endDate, credits, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CoursePlaceholder_null_title_throws_ArgumentNullException()
        {
            entity = new CoursePlaceholder(id, null, description, startDate, endDate, credits, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CoursePlaceholder_null_description_throws_ArgumentNullException()
        {
            entity = new CoursePlaceholder(id, title, null, startDate, endDate, credits, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CoursePlaceholder_invalid_dates_throws_ArgumentOutOfRangeException()
        {
            entity = new CoursePlaceholder(id, title, description, endDate, startDate, credits, null);
        }

        [TestMethod]
        public void CoursePlaceholder_valid()
        {
            entity = new CoursePlaceholder(id, title, description, startDate, endDate, credits, new Student.Entities.Requirements.AcademicRequirementGroup("1", "2", "3"));
            Assert.AreEqual(id, entity.Id);
            Assert.AreEqual(title, entity.Title);
            Assert.AreEqual(description, entity.Description);
            Assert.AreEqual(startDate, entity.StartDate);
            Assert.AreEqual(endDate, entity.EndDate);
            Assert.AreEqual(credits, entity.CreditInformation);
            Assert.IsNotNull(entity.AcademicRequirement);
            Assert.AreEqual("1", entity.AcademicRequirement.AcademicRequirementCode);
            Assert.AreEqual("2", entity.AcademicRequirement.SubrequirementId);
            Assert.AreEqual("3", entity.AcademicRequirement.GroupId);
        }
    }
}
