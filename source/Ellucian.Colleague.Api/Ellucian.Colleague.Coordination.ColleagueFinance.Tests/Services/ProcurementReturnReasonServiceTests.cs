using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
   public class ProcurementReturnReasonServiceTests : CurrentUserSetup
    {
        #region DECLARATIONS

        private const string returnReasonCode = "CR";
        private ProcurementReturnReasonService procurementReturnReasonService;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ILogger> _loggerMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private IEnumerable<Domain.Base.Entities.ItemCondition> returnReasonCodeCollection;

        private Domain.Entities.Permission permissionViewAnyPerson;

        #endregion

        #region TEST SETUP
        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);

            InitializeTestData();

            InitializeTestMock();
            
            procurementReturnReasonService = new ProcurementReturnReasonService(_referenceRepositoryMock.Object, adapterRegistry, currentUserFactory, roleRepoMock.Object, _loggerMock.Object);
        }

        private void InitializeTestData()
        {
            returnReasonCodeCollection = new List<ItemCondition>()
                {
                    new Domain.Base.Entities.ItemCondition("765765765","BR","Broken"),
                    new Domain.Base.Entities.ItemCondition("567455646","CR","Crushed")
                };
        }

        private void InitializeTestMock()
        {
            // Mock permissions
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            _referenceRepositoryMock.Setup(repo => repo.GetItemConditionsAsync(It.IsAny<bool>())).ReturnsAsync(returnReasonCodeCollection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            procurementReturnReasonService = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            returnReasonCodeCollection = null;
        }
        #endregion

        #region TEST METHODS

        [TestMethod]
        public async Task ProcurementReturnReasonService_GetProcurementReturnReasonsAsync()
        {
            var results = await procurementReturnReasonService.GetProcurementReturnReasonsAsync();
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementReturnReason>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ProcurementReturnReasonService_GetProcurementReturnReasonsAsync_Count()
        {
            var results = await procurementReturnReasonService.GetProcurementReturnReasonsAsync();
            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        public async Task ProcurementReturnReasonService_GetProcurementReturnReasonsAsync_Properties()
        {
            var result =
                (await procurementReturnReasonService.GetProcurementReturnReasonsAsync()).FirstOrDefault(x => x.Code == returnReasonCode);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Description);
        }

        #endregion
    }
}
