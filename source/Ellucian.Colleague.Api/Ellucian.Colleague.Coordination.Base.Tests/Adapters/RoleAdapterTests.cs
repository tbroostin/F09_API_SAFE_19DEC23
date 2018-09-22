// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class RoleAdapterTests
    {
        int roleId;
        string roleTitle;

        Role roleDto;
        Ellucian.Colleague.Domain.Entities.Role roleEntity;
        RoleAdapter roleEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            roleId = 1234567;
            roleTitle = "Role Title";

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            roleEntityAdapter = new RoleAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var permissionEntityAdapter = new AutoMapperAdapter<Domain.Entities.Permission, Permission>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Entities.Permission, Permission>()).Returns(permissionEntityAdapter);

            roleEntity = new Domain.Entities.Role(roleId, roleTitle);
            roleEntity.AddPermission(new Domain.Entities.Permission("ABC"));
            roleEntity.AddPermission(new Domain.Entities.Permission("DEF"));

            roleDto = roleEntityAdapter.MapToType(roleEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void RoleEntityAdapterTests_Title()
        {
            Assert.AreEqual(roleTitle, roleDto.Title);
        }

        [TestMethod]
        public void RoleEntityAdapterTests_Permissions()
        {
            List<Permission> dtoPermissions = new List<Permission>(roleDto.Permissions);
            Assert.AreEqual("ABC", dtoPermissions[0].Code);
            Assert.AreEqual("DEF", dtoPermissions[1].Code);
        }
    }
}