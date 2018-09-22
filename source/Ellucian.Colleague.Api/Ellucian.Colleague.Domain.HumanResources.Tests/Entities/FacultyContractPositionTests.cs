using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class FacultyContractPositionTests
    {
        public string Id;
        public string LoadPeriodId;
        public decimal? IntendedLoad;
        public string PositionId;
        public List<FacultyContractAssignment> FacultyContractAssignments;
        public string Title;
        public FacultyContractPosition facPosition;
        public FacultyContractAssignment asgmt1;

        [TestInitialize]
        public void Initialize()
        {
            Id = "123";
            LoadPeriodId = "1";
            IntendedLoad = 60;
            PositionId = "MATH511111";
            Title = "Cpt. Beast";
            FacultyContractAssignment asgmt1 = new FacultyContractAssignment("0101", "10101", FacultyContractAssignmentType.CampusOrganizationAdvisor, "321", "12345", "600,000,000,000");
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            facPosition = new FacultyContractPosition(Id, LoadPeriodId, IntendedLoad, PositionId);
            Assert.AreEqual(Id, facPosition.Id);
            Assert.AreEqual(LoadPeriodId, facPosition.LoadPeriodId);
            Assert.AreEqual(IntendedLoad, facPosition.IntendedLoad);
            Assert.AreEqual(PositionId, facPosition.PositionId);
        }

        [TestMethod]
        public void TitleSetter()
        {
            facPosition = new FacultyContractPosition(Id, LoadPeriodId, IntendedLoad, PositionId);
            facPosition.Title = Title;
            Assert.AreEqual(Title, facPosition.Title);
        }

        [TestMethod]
        public void AssignmentAdding()
        {
            facPosition = new FacultyContractPosition(Id, LoadPeriodId, IntendedLoad, PositionId);
            facPosition.FacultyContractAssignments.Add(asgmt1);
            Assert.AreEqual(1, facPosition.FacultyContractAssignments.Count);
            Assert.AreEqual(asgmt1, facPosition.FacultyContractAssignments.First());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullIdInConstructor()
        {
            new FacultyContractPosition(null, LoadPeriodId, IntendedLoad, PositionId);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullLoadPeriodIdInConstructor()
        {
            new FacultyContractPosition(Id, null, IntendedLoad, PositionId);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void EmptyIdInConstructor()
        {
            new FacultyContractPosition("", LoadPeriodId, IntendedLoad, PositionId);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void EmptyLoadPeriodIdInConstructor()
        {
            new FacultyContractPosition(Id, "", IntendedLoad, PositionId);
        }

    }

}
