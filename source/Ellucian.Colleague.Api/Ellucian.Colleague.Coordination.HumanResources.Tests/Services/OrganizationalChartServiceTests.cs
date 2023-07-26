//Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class OrganizationChartServiceTests
    {
        private List<Domain.HumanResources.Entities.OrgChartEmployee> _orgChartEmployees;
        private OrganizationalChartService _orgChartService;
        public ICurrentUserFactory currentUserFactory;
        public Domain.Entities.Role role;

        private Mock<IOrganizationalChartRepository> _referenceRepositoryMock;
        private Mock<IOrganizationalChartDomainService> _orgChartDomainMock;
        private Mock<IPersonBaseRepository> _personBaseRepositoryMock;
        private Mock<IEmployeeRepository> _employeeRepositoryMock;
        private Mock<IHumanResourcesReferenceDataRepository> _humanResourcesReferenceDataRepositoryMock;

        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;



        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IOrganizationalChartRepository>();
            _orgChartDomainMock = new Mock<IOrganizationalChartDomainService>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            _employeeRepositoryMock = new Mock<IEmployeeRepository>();
            _humanResourcesReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();

            var orgChartEmployeeEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.OrgChartEmployee, Dtos.HumanResources.OrgChartEmployee>(_adapterRegistryMock.Object, _loggerMock.Object);
            orgChartEmployeeEntityToDtoAdapter.AddMappingDependency<Domain.HumanResources.Entities.OrgChartEmployeeName, Dtos.HumanResources.OrgChartEmployeeName>();
            _adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.HumanResources.Entities.OrgChartEmployee, Dtos.HumanResources.OrgChartEmployee>()).Returns(orgChartEmployeeEntityToDtoAdapter);

            BuildData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _orgChartService = null;
            _orgChartEmployees = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task OrganizationChartServiceTests_PermissionsError()
        {
            // user without VIEW.ANY.PERSON permission
            currentUserFactory = new CurrentUserSetup.BenefitsEnrollmentDifferentUserFactory();

            _orgChartService = new OrganizationalChartService(_orgChartDomainMock.Object, 
                _personBaseRepositoryMock.Object,
                _employeeRepositoryMock.Object,
                _humanResourcesReferenceDataRepositoryMock.Object,
                _adapterRegistryMock.Object, currentUserFactory,
                _roleRepositoryMock.Object, _loggerMock.Object);
            await _orgChartService.GetOrganizationalChartAsync("99999");
        }

        [TestMethod]
        public async Task OrganizationalChartServiceTests_HasPermission()
        {
            currentUserFactory = new CurrentUserSetup.GeneralEmployeeUserFactory();

            _orgChartDomainMock.Setup(domain => domain.GetOrganizationalChartEmployeesAsync(It.IsAny<string>())).ReturnsAsync(_orgChartEmployees);
            _roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { role });

            _orgChartService = new OrganizationalChartService(_orgChartDomainMock.Object,
                _personBaseRepositoryMock.Object,
                _employeeRepositoryMock.Object,
                _humanResourcesReferenceDataRepositoryMock.Object,
                _adapterRegistryMock.Object, currentUserFactory,
                _roleRepositoryMock.Object, _loggerMock.Object);

            await _orgChartService.GetOrganizationalChartAsync("99999");

            var actuals = await _orgChartService.GetOrganizationalChartAsync("99999");
            Assert.IsNotNull(actuals);

            Assert.AreEqual(_orgChartEmployees.Count(), actuals.Count());
            Assert.AreEqual(_orgChartEmployees.FirstOrDefault().PositionCode, actuals.FirstOrDefault().PositionCode);
            Assert.AreEqual(_orgChartEmployees.FirstOrDefault().PersonPositionId, actuals.FirstOrDefault().PersonPositionId);
            Assert.AreEqual(_orgChartEmployees.FirstOrDefault().ParentPersonPositionId, actuals.FirstOrDefault().ParentPersonPositionId);
            Assert.AreEqual(_orgChartEmployees.FirstOrDefault().LocationCode, actuals.FirstOrDefault().LocationCode);
            Assert.AreEqual(_orgChartEmployees.FirstOrDefault().EmployeeName.LastName, actuals.FirstOrDefault().EmployeeName.LastName);
        }

        private void BuildData()
        {
            _orgChartEmployees = new List<Domain.HumanResources.Entities.OrgChartEmployee>();
            for (var x = 0; x < 10; x++)
            {
                var employeeName = new Domain.HumanResources.Entities.OrgChartEmployeeName()
                {
                    LastName = "EMPLOYEE_NAME_" + x,
                    FirstName = "EMPLOYEE_NAME_" + x,
                    FullName = "EMPLOYEE_NAME_" + x
                };
                var orgChartEmployee = new Domain.HumanResources.Entities.OrgChartEmployee("POS_" + x, x.ToString(), "POS_TEST", "POS_TEST", "POS_TEST", "123", "123", "LOU", employeeName);
                _orgChartEmployees.Add(orgChartEmployee);
            }

            role = new Domain.Entities.Role(223456, "EMPLOYEE");
            role.AddPermission(new Domain.Entities.Permission("VIEW.ORG.CHART"));
        }
    }
}