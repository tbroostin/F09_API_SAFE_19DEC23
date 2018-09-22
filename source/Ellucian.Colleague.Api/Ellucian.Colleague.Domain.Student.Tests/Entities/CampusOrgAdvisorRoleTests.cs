using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class FacultyContractPositionTests
    {
        public string Id;
        public string Role;
        public decimal? IntendedLoad;
        public string Assignment;
        public DateTime? StartDate;
        public DateTime? EndDate;
        public CampusOrgAdvisorRole campOrgAdvRole;

        [TestInitialize]
        public void Initialize()
        {
            Id = "ACM*123";
            Role = "1";
            IntendedLoad = 60;
            Assignment = "333";
            StartDate = new DateTime(01/01/2015);
            EndDate = new DateTime(05/05/2015);
            
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            campOrgAdvRole = new CampusOrgAdvisorRole(Id, Role, IntendedLoad, Assignment, StartDate, EndDate);
            Assert.AreEqual(Id, campOrgAdvRole.Id);
            Assert.AreEqual(Role, campOrgAdvRole.Role);
            Assert.AreEqual(IntendedLoad, campOrgAdvRole.IntendedLoad);
            Assert.AreEqual(Assignment, campOrgAdvRole.Assignment);
            Assert.AreEqual(StartDate, campOrgAdvRole.StartDate);
            Assert.AreEqual(EndDate, campOrgAdvRole.EndDate);
        }

        [TestMethod]
        public void NullIntendedLoadDoesntBreak()
        {
            campOrgAdvRole = new CampusOrgAdvisorRole(Id, Role, null, Assignment, StartDate, EndDate);
            Assert.AreEqual(null, campOrgAdvRole.IntendedLoad);
        }

        [TestMethod]
        public void NullStartDateDoesntBreak()
        {
            campOrgAdvRole = new CampusOrgAdvisorRole(Id, Role, IntendedLoad, Assignment, null, EndDate);
            Assert.AreEqual(null, campOrgAdvRole.StartDate);
        }

        [TestMethod]
        public void NullEndDateDoesntBreak()
        {
            campOrgAdvRole = new CampusOrgAdvisorRole(Id, Role, IntendedLoad, Assignment, StartDate, null);
            Assert.AreEqual(null, campOrgAdvRole.EndDate);
        }
    }
}
