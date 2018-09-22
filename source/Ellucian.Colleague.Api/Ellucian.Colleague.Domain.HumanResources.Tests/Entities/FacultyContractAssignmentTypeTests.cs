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
    public class FacultyContractAssignmentTypeTests
    {
        public string id;
        public string hrpId;
        public FacultyContractAssignmentType advisorType;
        public FacultyContractAssignmentType memberType;
        public FacultyContractAssignmentType facultyType;
        public string positionId;
        public DateTime? startDate;
        public DateTime? endDate;
        public string amount;
        public string assignmentId;
        private FacultyContractAssignment assignment;


        [TestInitialize]
        public void Initialize()
        {
            id = "10";
            hrpId = "00000052";
            advisorType = FacultyContractAssignmentType.CampusOrganizationAdvisor;
            memberType = FacultyContractAssignmentType.CampusOrganizationMember;
            facultyType = FacultyContractAssignmentType.CourseSectionFaculty;
            positionId = "10";
            startDate = new DateTime(2011, 01, 20);
            endDate = new DateTime(2011, 05, 11);
            assignmentId = "31";
            amount = "200000";
        }

        [TestMethod]
        public void FacultyContractAssignmentType_CourseSectionFaculty()
        {
            assignment = new FacultyContractAssignment(id, hrpId, facultyType, positionId, assignmentId, amount);
            Assert.AreEqual(assignment.AssignmentType, FacultyContractAssignmentType.CourseSectionFaculty);
        }

        [TestMethod]
        public void FacultyContractAssignmentType_CampusOrganizationAdvisor()
        {
            assignment = new FacultyContractAssignment(id, hrpId, advisorType, positionId, assignmentId, amount);
            Assert.AreEqual(assignment.AssignmentType, FacultyContractAssignmentType.CampusOrganizationAdvisor);
        }

        [TestMethod]
        public void FacultyContractAssignmentType_CampusOrganizationMember()
        {
            assignment = new FacultyContractAssignment(id, hrpId, memberType, positionId, assignmentId, amount);
            Assert.AreEqual(assignment.AssignmentType, FacultyContractAssignmentType.CampusOrganizationMember);
        }
    }
}
