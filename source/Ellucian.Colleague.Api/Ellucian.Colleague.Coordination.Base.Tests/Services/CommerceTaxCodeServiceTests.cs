// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CommerceTaxCodeServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetCommerceTaxCode
        {         
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private IReferenceDataRepository _referenceDataRepository;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private ILogger _logger;
            private CommerceTaxCodeService _commerceTaxCodeService;
            private ICollection<Colleague.Domain.Base.Entities.CommerceTaxCode> _commerceTaxCodeCollection = new List<Colleague.Domain.Base.Entities.CommerceTaxCode>();

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                _commerceTaxCodeCollection.Add(new Colleague.Domain.Base.Entities.CommerceTaxCode("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CODE1", "Description1"));
                _commerceTaxCodeCollection.Add(new Colleague.Domain.Base.Entities.CommerceTaxCode("e986b8a5-25f3-4aa0-bd0e-90982865e749", "CODE2", "Description2"));
                _commerceTaxCodeCollection.Add(new Colleague.Domain.Base.Entities.CommerceTaxCode("b5cc288b-8692-474e-91be-bdc55778e2f5", "CODE3", "Description3")); 
                _referenceDataRepositoryMock.Setup(repo => repo.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(_commerceTaxCodeCollection);

                _commerceTaxCodeService = new CommerceTaxCodeService(_adapterRegistry, _referenceDataRepository, _configurationRepositoryMock.Object,
                    _currentUserFactory, _roleRepo, _logger);         
            }

            [TestCleanup]
            public void Cleanup()
            {
                _commerceTaxCodeCollection = null;
                _referenceDataRepository = null;
                _commerceTaxCodeService = null;               
            }

            [TestMethod]
            public async Task CommerceTaxCodeService__CommerceTaxCodes()
            {
                var results = await _commerceTaxCodeService.GetCommerceTaxCodesAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.CommerceTaxCode>); 
                Assert.IsNotNull(results);
            }

            public async Task CommerceTaxCodeService_CommerceTaxCodes_Count()
            {
                var results = await _commerceTaxCodeService.GetCommerceTaxCodesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CommerceTaxCodeService_CommerceTaxCodes_Properties()
            {
                var results = await _commerceTaxCodeService.GetCommerceTaxCodesAsync();
                var commerceTaxCode = results.First(x => x.Code == "CODE1");
                Assert.IsNotNull(commerceTaxCode.Id);
                Assert.IsNotNull(commerceTaxCode.Code);
            }

            [TestMethod]
            public async Task CommerceTaxCodeService_CommerceTaxCodes_Expected()
            {
                var expectedResults = _commerceTaxCodeCollection.First(c => c.Code == "CODE1");
                var results = await _commerceTaxCodeService.GetCommerceTaxCodesAsync();
                var commerceTaxCode = results.First(s => s.Code == "CODE1");
                Assert.AreEqual(expectedResults.Guid, commerceTaxCode.Id);
                Assert.AreEqual(expectedResults.Code, commerceTaxCode.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CommerceTaxCodeService_GetCommerceTaxCodeByGuid_Empty()
            {
                await _commerceTaxCodeService.GetCommerceTaxCodeByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CommerceTaxCodeService_GetCommerceTaxCodeByGuid_Null()
            {
                await _commerceTaxCodeService.GetCommerceTaxCodeByGuidAsync(null);
            }

            [TestMethod]
            public async Task CommerceTaxCodeService_GetCommerceTaxCodeByGuid_Expected()
            {
                var expectedResults = _commerceTaxCodeCollection.First(c => c.Guid == "840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                var commerceTaxCode = await _commerceTaxCodeService.GetCommerceTaxCodeByGuidAsync("840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                Assert.AreEqual(expectedResults.Guid, commerceTaxCode.Id);
                Assert.AreEqual(expectedResults.Code, commerceTaxCode.Code);
            }

            [TestMethod]
            public async Task CommerceTaxCodeService_GetCommerceTaxCodeByGuid_Properties()
            {
                var commerceTaxCode = await _commerceTaxCodeService.GetCommerceTaxCodeByGuidAsync("840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                Assert.IsNotNull(commerceTaxCode.Id);
                Assert.IsNotNull(commerceTaxCode.Code);
            }

        }
    }
}