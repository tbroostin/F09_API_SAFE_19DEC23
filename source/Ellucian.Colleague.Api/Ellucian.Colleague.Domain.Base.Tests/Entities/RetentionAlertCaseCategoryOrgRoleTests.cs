using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RetentionAlertCaseCategoryOrgRoleTests
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRoleTests_Success_1()
        {
            RetentionAlertCaseCategoryOrgRole caseCatOrgRole = new RetentionAlertCaseCategoryOrgRole()
            {
                OrgRoleId = "155",
                OrgRoleName = "ADVISOR",
                IsAssignedInitially = "Y",
                IsAvailableForReassignment = "Y",
                IsReportingAndAdministrative = "A"
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRoleTests_Success_2()
        {
            RetentionAlertCaseCategoryOrgRole caseCatOrgRole = new RetentionAlertCaseCategoryOrgRole()
            {
                OrgRoleId = "155",
                OrgRoleName = "ADVISOR",
                IsAssignedInitially = "N",
                IsAvailableForReassignment = "N",
                IsReportingAndAdministrative = "Y"
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRoleTests_Success_3()
        {
            RetentionAlertCaseCategoryOrgRole caseCatOrgRole = new RetentionAlertCaseCategoryOrgRole()
            {
                OrgRoleId = "155",
                OrgRoleName = "ADVISOR",
                IsAssignedInitially = "Y",
                IsAvailableForReassignment = "Y",
                IsReportingAndAdministrative = "N"
            };
        }
    }
}
