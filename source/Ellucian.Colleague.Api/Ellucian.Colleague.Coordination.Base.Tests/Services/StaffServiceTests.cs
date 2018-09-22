using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class StaffServiceTests
    {
        // Sets up a Current user that is a staff (no roles)
        public abstract class CurrentUserSetup
        {
            public class StaffS001UserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Fred",
                            PersonId = "S001",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
            
        }
        [TestClass]
        public class GetStaffRestrictions
        {
            private StaffService staffService;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IPersonRestrictionRepository> perRestrRepoMock;
            private IPersonRestrictionRepository perRestrRepo;
            private Mock<IReferenceDataRepository> refDataRepoMock;
            private IReferenceDataRepository refDataRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Base.Entities.Staff staff1;
            private Domain.Base.Entities.Restriction restriction1;
            private Domain.Base.Entities.Restriction restriction2;
            private Domain.Base.Entities.PersonRestriction studentRestriction1;
            private Domain.Base.Entities.PersonRestriction studentRestriction2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                perRestrRepoMock = new Mock<IPersonRestrictionRepository>();
                perRestrRepo = perRestrRepoMock.Object;
                refDataRepoMock = new Mock<IReferenceDataRepository>();
                refDataRepo = refDataRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // mock the first Staff
                staff1 = new Domain.Base.Entities.Staff("S001", "Klemperer");
                staff1.IsActive = true;
                staffRepoMock.Setup(r => r.Get("S001")).Returns(staff1);
                // mock the restrictions in ref data repo
                restriction1 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R001", "Restriction1", (int?)4, "Y", "First Restriction", "First Restriction Details", "ST", "WMPT", "ST001", "Pay Me", "N");
                restriction1.Hyperlink = "http://www.someschool.edu.link1";
                restriction2 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R002", "Restriction2", null, "Y", "Second Restriction", "Second Restriction Details", "ST", "WRGS", "ST002", "Add/Drop", "N");
                restriction2.Hyperlink = "http://www.someschool.edu/link2";
                IEnumerable<Restriction> restList = new List<Domain.Base.Entities.Restriction>() { restriction1, restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(restList);
                // mock the student restrictions for the second Staff
                studentRestriction1 = new Domain.Base.Entities.PersonRestriction("SR001", "S001", "R001", new DateTime(2012, 12, 20), null, (int?)4, "Y");
                studentRestriction2 = new Domain.Base.Entities.PersonRestriction("SR002", "S001", "R002", new DateTime(2012, 12, 22), null, null, "Y");
                IEnumerable<Domain.Base.Entities.PersonRestriction> stu2RestList = new List<Domain.Base.Entities.PersonRestriction>() { studentRestriction1, studentRestriction2 };
                perRestrRepoMock.Setup(r => r.GetAsync("S001", true)).ReturnsAsync(stu2RestList);

                // setup a current user
                currentUserFactory = new CurrentUserSetup.StaffS001UserFactory();

                staffService = new StaffService(adapterRegistry, refDataRepo, staffRepo, perRestrRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                staffRepo = null;
                perRestrRepo = null;
                refDataRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsPermissionExceptionIfNotSelf()
            {
                Domain.Base.Entities.Staff staffx = new Staff("9999999", "Smith");
                staffRepoMock.Setup(repo => repo.Get("9999999")).Returns(staffx);
                await staffService.GetStaffRestrictionsAsync("9999999");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfStaffNotFound()
            {
                Domain.Base.Entities.Staff nullStaff = null;
                staffRepoMock.Setup(repo => repo.Get("S001")).Returns(nullStaff);
                await staffService.GetStaffRestrictionsAsync("S001");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfStaffRepoThrowsException()
            {
                staffRepoMock.Setup(repo => repo.Get("S001")).Throws(new Exception());
                await staffService.GetStaffRestrictionsAsync("S001");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfStaffNotActive()
            {
                Staff inactiveStaff = new Staff("S001", "Staff");
                staffRepoMock.Setup(repo => repo.Get("S001")).Returns(inactiveStaff);
                await staffService.GetStaffRestrictionsAsync("S001");
            }

            [TestMethod]
            public async Task ReturnsAllRestrictions()
            {
                var results = await staffService.GetStaffRestrictionsAsync("S001");
                Assert.AreEqual(2, results.Count());
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoPersonRestrictions()
            {
                perRestrRepoMock.Setup(r => r.GetAsync("S001", true)).ReturnsAsync(new List<PersonRestriction>());
                var results = await staffService.GetStaffRestrictionsAsync("S001");
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task ReturnsSingleRestrictionIfInvalidRestriction()
            {
                IEnumerable<Restriction> limitedRestList = new List<Domain.Base.Entities.Restriction>() { restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(limitedRestList);
                var results = await staffService.GetStaffRestrictionsAsync("S001");
                Assert.AreEqual(1, results.Count());
            }
        }
    }
}
