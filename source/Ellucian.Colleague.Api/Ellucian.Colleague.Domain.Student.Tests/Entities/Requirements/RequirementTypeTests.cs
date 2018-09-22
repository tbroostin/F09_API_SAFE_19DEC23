using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class RequirementTypeTests_Constructor
    {
        [TestMethod]
        public void RequirementType_CodeItem()
        {
            var code = "MAJ";
            var description = "Major Requirement";
            var priority = "3";
            var requirementType = new RequirementType(code, description, priority);
            Assert.AreEqual(code, requirementType.Code);
            Assert.AreEqual(description, requirementType.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequirementType_Code_Empty_ThrowsException()
        {
            var code = string.Empty;
            var description = "Major Requirement";
            var priority = "3";
            var requirementType = new RequirementType(code, description, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequirementType_Code_Null_ThrowsException()
        {
            var description = "Major Requirement";
            var priority = "3";
            var requirementType = new RequirementType(null, description, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequirementType_Description_Empty_ThrowsException()
        {
            var code = "MAJ";
            var description = string.Empty;
            var priority = "3";
            var requirementType = new RequirementType(code, description, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequirementType_Description_Null_ThrowsException()
        {
            var code = "MAJ";
            var priority = "3";
            var requirementType = new RequirementType(code, null, priority);
        }

        [TestMethod]
        public void RequirementType_Priority_StringConvertedToInteger()
        {
            var priority = "3";
            var requirementType = new RequirementType("MAJ", "Major Requirement", priority);
            Assert.AreEqual(int.Parse(priority), requirementType.Priority);
        }

        [TestMethod]
        public void RequirementType_Priority_Invalid_ConvertedToMaxValue()
        {
            var priority = "A";
            var requirementType = new RequirementType("MAJ", "Major Requirement", priority);
            Assert.AreEqual(int.MaxValue, requirementType.Priority);
        }

        [TestMethod]
        public void RequirementType_Priority_Empty_ConvertedToMaxValue()
        {
            var priority = string.Empty;
            var requirementType = new RequirementType("MAJ", "Major Requirement", priority);
            Assert.AreEqual(int.MaxValue, requirementType.Priority);
        }

        [TestMethod]
        public void RequirementType_Priority_Null_ConvertedToMaxValue()
        {
            var requirementType = new RequirementType("MAJ", "Major Requirement", null);
            Assert.AreEqual(int.MaxValue, requirementType.Priority);
        }
    }
}
