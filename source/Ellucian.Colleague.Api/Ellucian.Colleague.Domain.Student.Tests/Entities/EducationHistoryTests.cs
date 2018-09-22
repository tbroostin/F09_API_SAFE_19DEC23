using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the EducationHistory Entity
    public class EducationHistoryTests
    {
        private string Id = "0000001";
        private string HighSchoolId = "004";
        private string CollegeId = "001";
        private string DegreeCode = "Bachelor of Art";

        Student.Entities.EducationHistory educationHistoryEntity;
        Student.Entities.HighSchool highSchoolEntity;
        Student.Entities.College collegeEntity;
        Student.Entities.Credential credentialEntity;

        [TestInitialize]
        public void Initialize()
        {
            educationHistoryEntity = new Student.Entities.EducationHistory(Id);
            highSchoolEntity = new Student.Entities.HighSchool(HighSchoolId);
            collegeEntity = new Student.Entities.College(CollegeId);
            credentialEntity = new Student.Entities.Credential();
            credentialEntity.Degree = DegreeCode;
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationHistoryEntity = null;
            highSchoolEntity = null;
            collegeEntity = null;
            credentialEntity = null;
        }

        [TestMethod]
        public void VerifyIdProp_Set()
        {
            Assert.AreEqual(Id, educationHistoryEntity.Id);
            Assert.AreEqual(HighSchoolId, highSchoolEntity.HighSchoolId);
            Assert.AreEqual(CollegeId, collegeEntity.CollegeId);
            Assert.AreEqual(DegreeCode, credentialEntity.Degree);
        }

        [TestMethod]
        public void VerifyHighSchoolWithGpa_Set()
        {
            decimal? gpa = decimal.Parse("3.04");
            string lastAttendYear = "1998";
            highSchoolEntity = new Student.Entities.HighSchool(HighSchoolId, gpa, lastAttendYear);
            Assert.AreEqual(gpa, highSchoolEntity.Gpa);
            Assert.AreEqual(lastAttendYear, highSchoolEntity.LastAttendedYear);
        }

        [TestMethod]
        public void VerifyCollegeWithGpa_Set()
        {
            decimal? gpa = decimal.Parse("3.04");
            string lastAttendYear = "1998";
            collegeEntity = new Student.Entities.College(CollegeId, gpa, lastAttendYear);
            Assert.AreEqual(gpa, collegeEntity.Gpa);
            Assert.AreEqual(lastAttendYear, collegeEntity.LastAttendedYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Education_History_Id_NotNull()
        {
            new Student.Entities.EducationHistory(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HighSchool_Id_NotNull()
        {
            new Student.Entities.HighSchool(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void College_Id_NotNull()
        {
            new Student.Entities.College(null);
        }
    }
}
