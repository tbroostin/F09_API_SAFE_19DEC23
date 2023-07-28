/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Services
{
    [TestClass]
    public class OrganizationalChartDomainServiceTests : BaseRepositorySetup
    {
        #region DECLARATION
        private Mock<IOrganizationalChartRepository> orgChartRepoMock;
        private Mock<IPersonBaseRepository> personBaseRepoMock;
        private Mock<IPositionRepository> positionRepoMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;

        private IOrganizationalChartRepository orgChartRepo;
        private IPersonBaseRepository personBaseRepo;
        private IPositionRepository positionRepo;
        private IReferenceDataRepository referenceDataRepository;

        private OrganizationalChartDomainService orgChartDomainService;

        private List<OrgChartNode> orgChartNodes;
        private PersonBase personBaseEmployee;
        private Position position;

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            orgChartRepoMock = new Mock<IOrganizationalChartRepository>();
            personBaseRepoMock = new Mock<IPersonBaseRepository>();
            positionRepoMock = new Mock<IPositionRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            orgChartRepo = orgChartRepoMock.Object;
            personBaseRepo = personBaseRepoMock.Object;
            positionRepo = positionRepoMock.Object;
            referenceDataRepository = referenceDataRepositoryMock.Object;
            orgChartDomainService = new OrganizationalChartDomainService(orgChartRepo, personBaseRepo, positionRepo, referenceDataRepository, logger);

            BuildData();
        }


        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        #endregion

        #region TEST METHODS
        [TestMethod]
        public async Task OrganizationalChartService_GetOrganizationalChartEmployeesAsync_ReturnsData()
        {
            orgChartRepoMock.Setup(repo => repo.GetActiveOrgChartEmployeesAsync(It.IsAny<string>())).ReturnsAsync(orgChartNodes);
            personBaseRepoMock.Setup(repo => repo.GetPersonBaseAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personBaseEmployee);
            positionRepoMock.Setup(repo => repo.GetPositionByIdAsync(It.IsAny<string>())).ReturnsAsync(position);

            var actuals = await orgChartDomainService.GetOrganizationalChartEmployeesAsync("99999");

            Assert.IsNotNull(actuals);
            Assert.AreEqual(orgChartNodes.Count(), actuals.Count());
            Assert.AreEqual(orgChartNodes.FirstOrDefault().PositionCode, actuals.FirstOrDefault().PositionCode);
            Assert.AreEqual(orgChartNodes.FirstOrDefault().PersonPositionId, actuals.FirstOrDefault().PersonPositionId);
            Assert.AreEqual(orgChartNodes.FirstOrDefault().ParentPerposId, actuals.FirstOrDefault().ParentPersonPositionId);
            Assert.AreEqual(position.PositionLocation, actuals.FirstOrDefault().LocationCode);
        }
        #endregion

        private void BuildData()
        {
            orgChartNodes = new List<OrgChartNode>();
            for (var x=0; x < 10; x++)
            {
                var orgChartNode = new OrgChartNode("POS_" + x, x.ToString(), "POS_TEST", "SUP_POS", "99999", "POS_SUP");
                orgChartNodes.Add(orgChartNode);
            }

            personBaseEmployee = new PersonBase("00000", "TEST_LAST_NAME");
            personBaseEmployee.PreferredName = "PREFERRED_NAME";

            position = new Position("ZART0001", "Art Teacher", "ART Teacher", "ART", new DateTime(2022, 10, 1), true);
            position.PositionLocation = "LOU";
        }
    }
}
