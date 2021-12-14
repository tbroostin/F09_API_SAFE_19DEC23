// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class AcademicRequirementGroupTests
    {
        private AcademicRequirementGroup entity;
        private string academicRequirementCode;
        private string subrequirementId;
        private string groupId;

        [TestInitialize]
        public void AcademicRequirementGroupTests_Initialize()
        {
            academicRequirementCode = "REQ1";
            subrequirementId = "12345";
            groupId = "12346";
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AcademicRequirementGroup_null_academic_requirement_code_with_subrequirement_ID_and_group_ID()
        {
            entity = new AcademicRequirementGroup(null, subrequirementId, groupId);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AcademicRequirementGroup_null_academic_requirement_code_with_subrequirement_ID_and_no_group_ID()
        {
            entity = new AcademicRequirementGroup(null, subrequirementId, null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AcademicRequirementGroup_null_academic_requirement_code_no_subrequirement_ID_and_group_ID()
        {
            entity = new AcademicRequirementGroup(null, null, groupId);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AcademicRequirementGroup_with_academic_requirement_code_no_subrequirement_ID_and_group_ID()
        {
            entity = new AcademicRequirementGroup(academicRequirementCode, null, groupId);
        }

        [TestMethod]
        public void AcademicRequirementGroup_with_academic_requirement_code_no_subrequirement_ID_no_group_ID()
        {
            entity = new AcademicRequirementGroup(academicRequirementCode, null, null);
            Assert.AreEqual(academicRequirementCode, entity.AcademicRequirementCode);
            Assert.IsNull(entity.SubrequirementId);
            Assert.IsNull(entity.GroupId);
        }

        [TestMethod]
        public void AcademicRequirementGroup_with_academic_requirement_code_and_subrequirement_ID_no_group_ID()
        {
            entity = new AcademicRequirementGroup(academicRequirementCode, subrequirementId, null);
            Assert.AreEqual(academicRequirementCode, entity.AcademicRequirementCode);
            Assert.AreEqual(subrequirementId, entity.SubrequirementId);
            Assert.IsNull(entity.GroupId);
        }

        [TestMethod]
        public void AcademicRequirementGroup_with_academic_requirement_code_and_subrequirement_ID_and_group_ID()
        {
            entity = new AcademicRequirementGroup(academicRequirementCode, subrequirementId, groupId);
            Assert.AreEqual(academicRequirementCode, entity.AcademicRequirementCode);
            Assert.AreEqual(subrequirementId, entity.SubrequirementId);
            Assert.AreEqual(groupId, entity.GroupId);
        }
    }
}
