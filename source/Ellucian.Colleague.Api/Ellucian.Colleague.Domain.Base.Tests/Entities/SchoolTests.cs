using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SchoolTests
    {

        [TestClass]
        public class ScholConstructor
        {
            private string code;
            private string description;
            private IEnumerable<string> locationcodes;
            private IEnumerable<string> divisioncodes;
            private IEnumerable<string> departmentcodes;
            private string academiclevel;
            private School school;

            [TestInitialize]
            public void Initialize()
            {
                code = "NUR";
                description = "School of Nursing";
                locationcodes = new List<string> { "MC", "OC" };
                divisioncodes = new List<string> { "NUR", "HEA" };
                departmentcodes = new List<string> { "NURSE", "HEALTH" };
                academiclevel = "UG";
                
                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                school = new School(Guid.NewGuid().ToString(), code, description);
                school.AcademicLevelCode = academiclevel;
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SchoolCodeExceptionIfNull()
            {
                new School(Guid.NewGuid().ToString(), null, description);
            }
            
            [TestMethod]
            public void SchoolCode()
            {
                Assert.AreEqual(code, school.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void SchoolCode_CannotBeChanged()
            {
                Assert.AreEqual(code, school.Code);
                school.Code = code + "A";
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SchoolDescriptionExceptionIfNull()
            {
                new School(Guid.NewGuid().ToString(), code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SchoolDescriptionExceptionIfEmpty()
            {
                new School(Guid.NewGuid().ToString(), code, string.Empty);
            }

            [TestMethod]
            public void SchoolDescription()
            {
                Assert.AreEqual(description, school.Description);
            }

            [TestMethod]
            public void AcademicLevel()
            {
                Assert.AreEqual(academiclevel, school.AcademicLevelCode);
            }

            [TestMethod]
            public void AddLocation()
            {
                foreach (var dtcd in locationcodes)
                {
                    school.AddLocationCode(dtcd);
                }
            }
            
            [TestMethod]
            public void AddDivision()
            {
                foreach (var dtcd in divisioncodes)
                {
                    school.AddDivisionCode(dtcd);
                }
            }
            
            [TestMethod]
            public void AddDepartment()
            {
                foreach (var dtcd in departmentcodes)
                {
                    school.AddDepartmentCode(dtcd);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddLocationCode_Null()
            {
                school.AddLocationCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddLocationCode_Empty()
            {
                school.AddLocationCode(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void School_AddLocationCode_Duplicate()
            {
                school.AddLocationCode("MC");
                school.AddLocationCode("MC");
            }

            [TestMethod]
            public void School_AddLocationCode_Valid()
            {
                school.AddLocationCode("MC");
                school.AddLocationCode("SC");
                Assert.AreEqual(2, school.LocationCodes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddDepartmentCode_Null()
            {
                school.AddDepartmentCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddDepartmentCode_Empty()
            {
                school.AddDepartmentCode(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void School_AddDepartmentCode_Duplicate()
            {
                school.AddDepartmentCode("MATH");
                school.AddDepartmentCode("MATH");
            }

            [TestMethod]
            public void School_AddDepartmentCode_Valid()
            {
                school.AddDepartmentCode("MATH");
                school.AddDepartmentCode("BUSN");
                Assert.AreEqual(2, school.DepartmentCodes.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddDivisionCode_Null()
            {
                school.AddDivisionCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void School_AddDivisionCode_Empty()
            {
                school.AddDivisionCode(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void School_AddDivisionCode_Duplicate()
            {
                school.AddDivisionCode("MATH");
                school.AddDivisionCode("MATH");
            }

            [TestMethod]
            public void School_AddDivisionCode_Valid()
            {
                school.AddDivisionCode("MATH");
                school.AddDivisionCode("BUSN");
                Assert.AreEqual(2, school.DivisionCodes.Count());
            }
        }
    }
}
