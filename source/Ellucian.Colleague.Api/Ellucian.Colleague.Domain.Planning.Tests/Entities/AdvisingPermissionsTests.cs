// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class AdvisingPermissionsTests
    {
        private List<string> permissionCodes;
        private Mock<ILogger> loggerMock;
        private ILogger logger;
        private AdvisingPermissions entity;

        [TestInitialize]
        public void AdvisingPermissionsTests_Initialize()
        {
            permissionCodes = new List<string>();
        }

        [TestMethod]
        public void AdvisingPermissions_default_constructor_sets_all_permissions_to_false()
        {
            entity = new AdvisingPermissions();
            Assert.IsFalse(entity.CanReviewAnyAdvisee);
            Assert.IsFalse(entity.CanReviewAssignedAdvisees);
            Assert.IsFalse(entity.CanUpdateAnyAdvisee);
            Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
            Assert.IsFalse(entity.CanViewAnyAdvisee);
            Assert.IsFalse(entity.CanViewAssignedAdvisees);
            Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdvisingPermissions_null_PermissionCodes_throws_Exception()
        {
            entity = new AdvisingPermissions(null);
        }

        [TestClass]
        public class AdvisingPermissions_with_Logger : AdvisingPermissionsTests
        {
            [TestInitialize]
            public void AdvisingPermissionsTests_with_Logger_Initialize()
            {
                base.AdvisingPermissionsTests_Initialize();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
            }

            [TestMethod]
            public void AdvisingPermissions_ViewAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ViewAssignedAdvisees }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ReviewAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ReviewAssignedAdvisees }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_UpdateAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.UpdateAssignedAdvisees }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_AllAccessAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.AllAccessAssignedAdvisees }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsTrue(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ViewAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ViewAnyAdvisee }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ReviewAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ReviewAnyAdvisee }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_UpdateAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.UpdateAnyAdvisee }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsTrue(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_AllAccessAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.AllAccessAnyAdvisee }, logger);
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsTrue(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsTrue(entity.CanUpdateAnyAdvisee);
                Assert.IsTrue(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_NoAdvisingPermissions()
            {
                entity = new AdvisingPermissions(new List<string>() { "VIEW.STUDENT.INFORMATION" });
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
            }
        }

        [TestClass]
        public class AdvisingPermissions_without_Logger : AdvisingPermissionsTests
        {
            [TestInitialize]
            public void AdvisingPermissionsTests_without_Logger_Initialize()
            {
                base.AdvisingPermissionsTests_Initialize();
            }

            [TestMethod]
            public void AdvisingPermissions_ViewAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ViewAssignedAdvisees });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ReviewAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ReviewAssignedAdvisees });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_UpdateAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.UpdateAssignedAdvisees });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_AllAccessAssignedAdvisees()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.AllAccessAssignedAdvisees });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsTrue(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ViewAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ViewAnyAdvisee });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_ReviewAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.ReviewAnyAdvisee });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_UpdateAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.UpdateAnyAdvisee });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsTrue(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_AllAccessAnyAdvisee()
            {
                entity = new AdvisingPermissions(new List<string>() { PlanningPermissionCodes.AllAccessAnyAdvisee });
                Assert.IsTrue(entity.CanViewAssignedAdvisees);
                Assert.IsTrue(entity.CanReviewAssignedAdvisees);
                Assert.IsTrue(entity.CanUpdateAssignedAdvisees);
                Assert.IsTrue(entity.HasFullAccessForAssignedAdvisees);
                Assert.IsTrue(entity.CanViewAnyAdvisee);
                Assert.IsTrue(entity.CanReviewAnyAdvisee);
                Assert.IsTrue(entity.CanUpdateAnyAdvisee);
                Assert.IsTrue(entity.HasFullAccessForAnyAdvisee);
            }

            [TestMethod]
            public void AdvisingPermissions_NoAdvisingPermissions()
            {
                entity = new AdvisingPermissions(new List<string>() { "VIEW.STUDENT.INFORMATION" });
                Assert.IsFalse(entity.CanReviewAnyAdvisee);
                Assert.IsFalse(entity.CanReviewAssignedAdvisees);
                Assert.IsFalse(entity.CanUpdateAnyAdvisee);
                Assert.IsFalse(entity.CanUpdateAssignedAdvisees);
                Assert.IsFalse(entity.CanViewAnyAdvisee);
                Assert.IsFalse(entity.CanViewAssignedAdvisees);
                Assert.IsFalse(entity.HasFullAccessForAnyAdvisee);
                Assert.IsFalse(entity.HasFullAccessForAssignedAdvisees);
            }
        }
    }
}
