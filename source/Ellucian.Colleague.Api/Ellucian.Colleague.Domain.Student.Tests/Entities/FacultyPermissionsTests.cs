// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
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
            Assert.IsFalse(entity.CanDropStudent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FacultyPermissions_null_PermissionCodes_throws_Exception()
        {
            entity = new FacultyPermissions(null, false, false);
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
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateFacultyConsent }, false, false);
                Assert.IsTrue(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_CreateStudentPetition()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateStudentPetition }, false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsTrue(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_CreatePrerequisiteWaiver()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreatePrerequisiteWaiver }, false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsTrue(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_UpdateGrades()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.UpdateGrades }, false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsTrue(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_CanDropStudent()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CanDropStudent }, false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsTrue(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_AllFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>() 
                { 
                    StudentPermissionCodes.CreateStudentCharges, 
                    StudentPermissionCodes.CreateFacultyConsent, 
                    StudentPermissionCodes.CreateUpdateHousingAssignment, 
                    StudentPermissionCodes.CreatePrerequisiteWaiver, 
                    StudentPermissionCodes.UpdateGrades, 
                    StudentPermissionCodes.CreateStudentPetition,
                    StudentPermissionCodes.CanDropStudent
                }, false, false);
                Assert.IsTrue(entity.CanGrantFacultyConsent);
                Assert.IsTrue(entity.CanGrantStudentPetition);
                Assert.IsTrue(entity.CanUpdateGrades);
                Assert.IsTrue(entity.CanWaivePrerequisiteRequirement);
                Assert.IsTrue(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_NoMatchingFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>() { StudentPermissionCodes.CreateStudentCharges }, false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_EmptyFacultyPermissions()
            {
                entity = new FacultyPermissions(new List<string>(), false, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
            }

            [TestMethod]
            public void FacultyPermissions_IsElibibleToDrop()
            {
                entity = new FacultyPermissions(new List<string>(), true, false);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
                Assert.IsTrue(entity.IsEligibleToDrop);
                Assert.IsFalse(entity.HasEligibilityOverrides);
            }

            [TestMethod]
            public void FacultyPermissions_HasEligibilityOverrides()
            {
                entity = new FacultyPermissions(new List<string>(), false, true);
                Assert.IsFalse(entity.CanGrantFacultyConsent);
                Assert.IsFalse(entity.CanGrantStudentPetition);
                Assert.IsFalse(entity.CanUpdateGrades);
                Assert.IsFalse(entity.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(entity.CanDropStudent);
                Assert.IsFalse(entity.IsEligibleToDrop);
                Assert.IsTrue(entity.HasEligibilityOverrides);
            }
        }



    }
}
