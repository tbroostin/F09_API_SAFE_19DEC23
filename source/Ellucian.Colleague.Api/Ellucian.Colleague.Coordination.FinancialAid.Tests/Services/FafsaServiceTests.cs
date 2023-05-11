//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class FafsaServiceTests : FinancialAidServiceTestsSetup
    {
        public Mock<IFafsaRepository> fafsaRepositoryMock;
        public Mock<ITermRepository> termRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        public TestFinancialAidOfficeRepository expectedFinancialAidOfficeRepository;
        public TestStudentAwardYearRepository expectedStudentAwardYearRepository;
        public TestTermRepository expectedTermRepository;
        public Domain.FinancialAid.Tests.TestFafsaRepository expectedFafsaRepository;

        public ITypeAdapter<Colleague.Domain.FinancialAid.Entities.Fafsa, Fafsa> fafsaDtoAdapter;

        public FunctionEqualityComparer<Fafsa> fafsaDtoComparer;

        public void FafsaServiceTestsInitialize()
        {
            BaseInitialize();

            fafsaRepositoryMock = new Mock<IFafsaRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            expectedFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            expectedStudentAwardYearRepository = new TestStudentAwardYearRepository();
            expectedTermRepository = new TestTermRepository();
            expectedFafsaRepository = new Domain.FinancialAid.Tests.TestFafsaRepository();

            fafsaDtoComparer = new FunctionEqualityComparer<Fafsa>(
                (f1, f2) => (f1.Id == f2.Id && f1.StudentId == f2.StudentId && f1.AwardYear == f2.AwardYear),
                (f) => (f.Id.GetHashCode()));

            fafsaDtoAdapter = new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.Fafsa, Fafsa>(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetStudentFafsasTests : FafsaServiceTests
        {
            public string studentId;

            //Domain entities can be modified for tests by changing the record representations in the test repositories
            public IEnumerable<Colleague.Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
            {
                get
                {
                    return expectedFinancialAidOfficeRepository.GetFinancialAidOffices();
                }
            }
            public IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
            {
                get
                {
                    return expectedStudentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeEntities));
                }
            }
            public IEnumerable<Colleague.Domain.FinancialAid.Entities.Fafsa> fafsaEntities
            {
                get
                {
                    return expectedFafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYearEntities.Select(y => y.Code)).Result;
                }
            }

            //Dtos
            public IEnumerable<Fafsa> expectedFafsas
            {
                get
                {
                    return fafsaEntities.Select(fafsa => fafsaDtoAdapter.MapToType(fafsa));
                }
            }
            public IEnumerable<Fafsa> actualFafsas;
            
            public FafsaService fafsaService;

            [TestInitialize]
            public void Initialize()
            {
                FafsaServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                    .Returns(() => expectedFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult(expectedStudentAwardYearRepository.GetStudentAwardYears(id, currentOfficeService)));

                fafsaRepositoryMock.Setup(r => r.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>(
                        (ids, awardYearCodes) => expectedFafsaRepository.GetFafsasAsync(ids, awardYearCodes));

                adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.Fafsa, Fafsa>())
                    .Returns(fafsaDtoAdapter);

                BuildFafsaService();
            }

            private void BuildFafsaService()
            {
                fafsaService = new FafsaService(adapterRegistryMock.Object, fafsaRepositoryMock.Object, termRepositoryMock.Object, financialAidOfficeRepositoryMock.Object, studentAwardYearRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);
                Assert.IsTrue(expectedFafsas.Count() > 0);
                Assert.AreEqual(expectedFafsas.Count(), actualFafsas.Count());
                CollectionAssert.AreEqual(expectedFafsas.ToList(), actualFafsas.ToList(), fafsaDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await fafsaService.GetStudentFafsasAsync(null);
            }

            /// <summary>
            /// User is not self, nor counselor, nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndUserNotCounselorNorProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                try
                {
                    await fafsaService.GetStudentFafsasAsync("foobar");
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to access fafsa information for {1}", currentUserFactory.CurrentUser.PersonId, "foobar")));
                    throw;
                }
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildFafsaService();
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                CollectionAssert.AreEqual(expectedFafsas.ToList(), actualFafsas.ToList(), fafsaDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsCounselorNoPermissionsTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildFafsaService();
                await fafsaService.GetStudentFafsasAsync(studentId);                
            }

            /// <summary>
            /// User is Proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildFafsaService();
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                CollectionAssert.AreEqual(expectedFafsas.ToList(), actualFafsas.ToList(), fafsaDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyForDifferentPersonsTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildFafsaService();
                await fafsaService.GetStudentFafsasAsync(studentId);
            }

            [TestMethod]
            public async Task StudentAwardYearsIsNull_ReturnEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award years", studentId)));
            }

            [TestMethod]
            public async Task StudentAwardYearsIsEmpty_ReturnEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award years", studentId)));
            }

            [TestMethod]
            public async Task FafsaEntitiesIsNull_ReturnEmptyListTest()
            {
                fafsaRepositoryMock.Setup(r => r.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>(
                        (ids, awardYearCodes) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.Fafsa>)null));
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no fafsas", studentId)));
            }

            [TestMethod]
            public async Task FafsaEntitiesIsEmpty_ReturnEmptyListTest()
            {
                fafsaRepositoryMock.Setup(r => r.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>(
                        (ids, awardYearCodes) => Task.FromResult((new List<Domain.FinancialAid.Entities.Fafsa>().AsEnumerable())));
                actualFafsas = await fafsaService.GetStudentFafsasAsync(studentId);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no fafsas", studentId)));
            }


        }
    }
}
