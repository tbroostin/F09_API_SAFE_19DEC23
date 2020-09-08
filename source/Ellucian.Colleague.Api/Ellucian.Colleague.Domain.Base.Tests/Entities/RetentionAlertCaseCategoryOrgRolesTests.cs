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
    public class RetentionAlertCaseCategoryOrgRolesTests
    {
        private RetentionAlertCaseCategoryOrgRole caseCatOrgRole1;
        private RetentionAlertCaseCategoryOrgRole caseCatOrgRole2;

        [TestInitialize]
        public void Initialize()
        {
            caseCatOrgRole1 = new RetentionAlertCaseCategoryOrgRole()
            {
                OrgRoleId = "155",
                OrgRoleName = "ADVISOR",
                IsAssignedInitially = "Y",
                IsAvailableForReassignment = "Y",
                IsReportingAndAdministrative = "A"
            };

            caseCatOrgRole2 = new RetentionAlertCaseCategoryOrgRole()
            {
                OrgRoleId = "12",
                OrgRoleName = "FACULTY",
                IsAssignedInitially = "N",
                IsAvailableForReassignment = "Y",
                IsReportingAndAdministrative = "Y"
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRolesTests_Success_1()
        {
            List<RetentionAlertCaseCategoryOrgRole> caseCatOrgRoleList = new List<RetentionAlertCaseCategoryOrgRole>();
            caseCatOrgRoleList.Add( caseCatOrgRole1 );

            RetentionAlertCaseCategoryOrgRoles caseCatOrgRoles = new RetentionAlertCaseCategoryOrgRoles()
            {
                CaseCategoryId = "1",
                CaseCategoryOrgRoles = caseCatOrgRoleList
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRolesTests_Success_2()
        {
            List<RetentionAlertCaseCategoryOrgRole> caseCatOrgRoleList = new List<RetentionAlertCaseCategoryOrgRole>();
            caseCatOrgRoleList.Add(caseCatOrgRole1);
            caseCatOrgRoleList.Add(caseCatOrgRole2);

            RetentionAlertCaseCategoryOrgRoles caseCatOrgRoles = new RetentionAlertCaseCategoryOrgRoles()
            {
                CaseCategoryId = "1",
                CaseCategoryOrgRoles = caseCatOrgRoleList
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRolesTests_Success_3()
        {
            List<RetentionAlertCaseCategoryOrgRole> caseCatOrgRoleList = new List<RetentionAlertCaseCategoryOrgRole>();            

            RetentionAlertCaseCategoryOrgRoles caseCatOrgRoles = new RetentionAlertCaseCategoryOrgRoles()
            {
                CaseCategoryId = "1",
                CaseCategoryOrgRoles = caseCatOrgRoleList
            };
        }

        [TestMethod]
        public void RetentionAlertCaseCategoryOrgRolesTests_Success_4()
        {
            List<RetentionAlertCaseCategoryOrgRole> caseCatOrgRoleList = null;

            RetentionAlertCaseCategoryOrgRoles caseCatOrgRoles = new RetentionAlertCaseCategoryOrgRoles()
            {
                CaseCategoryId = "1",
                CaseCategoryOrgRoles = caseCatOrgRoleList
            };
        }
    }
}
