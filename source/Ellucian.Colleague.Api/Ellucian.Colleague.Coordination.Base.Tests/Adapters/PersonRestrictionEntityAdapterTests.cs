// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonRestrictionEntityAdapterTests
    {
        private Ellucian.Colleague.Domain.Base.Entities.PersonRestriction personRestrictionEntity1;
        private PersonRestriction personRestrictionDto;
        private Ellucian.Colleague.Domain.Base.Entities.Restriction restriction;
        private PersonRestrictionEntityAdapter personRestrictionEntityAdapter;
        
        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            
            // Create the entity to convert
            string guid = Guid.NewGuid().ToString();
            string id = "1";
            string studentId = "0001111";
            DateTime startDate = new DateTime(2014, 1, 15);
            string restrictionId = "111";
            int severity = 1;
            string visibleToUsers = "Y";
            personRestrictionEntity1 = new Ellucian.Colleague.Domain.Base.Entities.PersonRestriction(id, studentId, restrictionId, startDate, null, severity, visibleToUsers);

            // Create the associated restriction
            restriction = new Domain.Base.Entities.Restriction(guid, "111", "Description", severity, visibleToUsers, "Title", "Details", "FollowUpApp", "FollowUpLinkDef", "FollowUpWAForm", "FollowUpLabel", "FollowUpIsMiscText");

            personRestrictionEntityAdapter = new PersonRestrictionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            personRestrictionDto = personRestrictionEntityAdapter.MapToType(personRestrictionEntity1, restriction);
        }
        
        [TestMethod]
        public void PersonRestriction_Id()
        {
            Assert.AreEqual(personRestrictionDto.Id, personRestrictionEntity1.Id);
        }

        [TestMethod]
        public void PersonRestriction_StartDate()
        {
            Assert.AreEqual(personRestrictionDto.StartDate, personRestrictionEntity1.StartDate);
        }

        [TestMethod]
        public void PersonRestriction_Severity()
        {
            Assert.AreEqual(personRestrictionDto.Severity, personRestrictionEntity1.Severity);
        }

        [TestMethod]
        public void PersonRestriction_Title()
        {
            Assert.AreEqual(personRestrictionDto.Title, restriction.Title);
        }

        [TestMethod]
        public void PersonRestriction_Details()
        {
            Assert.AreEqual(personRestrictionDto.Details, restriction.Details);
        }

        [TestMethod]
        public void PersonRestriction_Hyperlink()
        {
            Assert.AreEqual(personRestrictionDto.Hyperlink, restriction.Hyperlink);
        }

        [TestMethod]
        public void PersonRestriction_FollowUpLabel()
        {
            Assert.AreEqual(personRestrictionDto.HyperlinkText, restriction.FollowUpLabel);
        }

        [TestMethod]
        public void PersonRestriction_OfficeUseOnly()
        {
            Assert.AreEqual(personRestrictionDto.OfficeUseOnly, restriction.OfficeUseOnly);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonRestrictions_NullSource_ThowsException()
        {
            var prDto = personRestrictionEntityAdapter.MapToType(null, restriction);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonRestrictions_NullRestriction_ThowsException()
        {
            var prDto = personRestrictionEntityAdapter.MapToType(personRestrictionEntity1, null);
        }
    }
}
