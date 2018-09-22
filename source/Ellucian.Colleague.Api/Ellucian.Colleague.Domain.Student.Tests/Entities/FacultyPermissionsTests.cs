// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class FacultyPermissionsTests
    {
        private List<string> permissionCodes;
        private FacultyPermissions entity;

        [TestInitialize]
        public void FacultyPermissionsTests_Initialize()
        {
            permissionCodes = new List<string>();
        }

        [TestMethod]
        public void FacultyPermissions_default_constructor_sets_all_permissions_to_false()
        {
            entity = new FacultyPermissions();
            Assert.IsFalse(entity.CanGrantFacultyConsent);
            Assert.IsFalse(entity.CanGrantStudentPetition);
            Assert.IsFalse(entity.CanUpdateGrades);
            Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyPermissions_null_PermissionCodes_throws_Exception()
        {
            entity = new FacultyPermissions(null);
        }

        [TestClass]
        public class FacultyPermissions_with_Logger : FacultyPermissionsTests
        {
            [TestInitialize]
            public void FacultyPermissionsTests_with_Logger_Initialize()
            {
                base.FacultyPermissionsTests_Initialize();
            }

            [TestMethod]
            public void FacultyPermissions_CreateFacultyConsent()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateFacultyConsent });
                Assert.IsTrue(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
            }

            [TestMethod]
            public void FacultyPermissions_CreateStudentPetition()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateStudentPetition });
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsTrue(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
            }

            [TestMethod]
            public void FacultyPermissions_CreatePrerequisiteWaiver()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreatePrerequisiteWaiver });
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsTrue(entity.CanWaivePrerequisiteRequirement);
            }

            [TestMethod]
            public void FacultyPermissions_UpdateGrades()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.UpdateGrades });
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsTrue(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
            }



            [TestMethod]
            public void FacultyPermissions_AllFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateStudentCharges, StudentPermissionCodes.CreateFacultyConsent, StudentPermissionCodes.CreateUpdateHousingAssignment, StudentPermissionCodes.CreatePrerequisiteWaiver, StudentPermissionCodes.UpdateGrades, StudentPermissionCodes.CreateStudentPetition });
                Assert.IsTrue(entity.CanGrantFacultyConsent);
                Assert.IsTrue(entity.CanGrantStudentPetition);
                Assert.IsTrue(entity.CanUpdateGrades);
                Assert.IsTrue(entity.CanWaivePrerequisiteRequirement);
            }

            [TestMethod]
            public void FacultyPermissions_NoMatchingFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateStudentCharges });
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
            }

            [TestMethod]
            public void FacultyPermissions_EmptyFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>());
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
            }
        }

        

    }
}
