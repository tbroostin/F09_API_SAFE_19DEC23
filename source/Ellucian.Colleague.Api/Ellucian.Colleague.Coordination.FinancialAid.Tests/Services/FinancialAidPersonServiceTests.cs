/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class FinancialAidPersonServiceTests : FinancialAidServiceTestsSetup
    {
        public TestFinancialAidPersonRepository faPersonsRepository;

        public Mock<IFinancialAidPersonRepository> faPersonRepositoryMock;
        public Mock<IPersonBaseRepository> personBaseRepositoryMock;

        public AutoMapperAdapter<Domain.Base.Entities.PersonBase, Ellucian.Colleague.Dtos.Base.Person> personAdapter;

        public PrivacyWrapper<IEnumerable<Dtos.Base.Person>> expectedResult;
        public PrivacyWrapper<IEnumerable<Dtos.Base.Person>> actualResult;

        public FinancialAidPersonService actualService
        {
            get
            {
                return new FinancialAidPersonService(adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, faPersonRepositoryMock.Object, personBaseRepositoryMock.Object, baseConfigurationRepository);
            }
        }

        public void FAPersonServiceTestsInitialize()
        {
            BaseInitialize();

            faPersonsRepository = new TestFinancialAidPersonRepository();
            faPersonRepositoryMock = new Mock<IFinancialAidPersonRepository>();
            personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            faPersonRepositoryMock.Setup(r => r.SearchFinancialAidPersonsByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(
                (ids) => faPersonsRepository.SearchFinancialAidPersonsByIdsAsync(ids));
            faPersonRepositoryMock.Setup(r => r.SearchFinancialAidPersonsByKeywordAsync(It.IsAny<string>())).Returns<string>(
                (criteria) => faPersonsRepository.SearchFinancialAidPersonsByKeywordAsync(criteria));
            personAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonBase, Dtos.Base.Person>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup<ITypeAdapter<Domain.Base.Entities.PersonBase, Dtos.Base.Person>>(a => a.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.Base.Person>()).Returns(personAdapter);
        }

        [TestClass]
        public class SearchFinancialAidPersonsAsyncTests : FinancialAidPersonServiceTests
        {
            private FinancialAidPersonQueryCriteria criteria = new FinancialAidPersonQueryCriteria(){
                FinancialAidPersonIds = null,
                FinancialAidPersonQueryKeyword = string.Empty
            };

            [TestInitialize]
            public void Initialize()
            {
                FAPersonServiceTestsInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                faPersonRepositoryMock = null;
                faPersonsRepository = null;
                adapterRegistryMock = null;
                loggerMock = null;
                personAdapter = null;
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public async Task NullCriteria_SearchFinancialAidPersonsAsync_ThrowsArgumentNullExceptionTest()
            {
                await actualService.SearchFinancialAidPersonsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyCriteria_SearchFinancialAidPersonsAsync_ThrowsArgumentExceptionTest()
            {
                await actualService.SearchFinancialAidPersonsAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SearchFinancialAidPersonsAsync_ThrowsPermissionsExceptionTest()
            {
                criteria.FinancialAidPersonQueryKeyword = "foo";
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>());
                await actualService.SearchFinancialAidPersonsAsync(criteria);
            }

            [TestMethod]
            public async Task QueryWordPassed_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                var person = faPersonsRepository.personData.First();
                expectedResult = new PrivacyWrapper<IEnumerable<Dtos.Base.Person>>(new List<Dtos.Base.Person>(){
                        personAdapter.MapToType(new Person(person.id, person.lastName, person.privacyCode))
                    }, true);
                criteria.FinancialAidPersonQueryKeyword = person.id;
                actualResult = await actualService.SearchFinancialAidPersonsAsync(criteria);
                Assert.AreEqual(expectedResult.HasPrivacyRestrictions, actualResult.HasPrivacyRestrictions);
                Assert.AreEqual(expectedResult.Dto.First().Id, actualResult.Dto.First().Id);
                Assert.AreEqual(expectedResult.Dto.First().LastName, actualResult.Dto.First().LastName);
            }

            [TestMethod]
            public async Task NoPrivacyRestriction_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                var person = faPersonsRepository.personData.First();
                person.privacyCode = string.Empty;
                expectedResult = new PrivacyWrapper<IEnumerable<Dtos.Base.Person>>(new List<Dtos.Base.Person>(){
                        personAdapter.MapToType(new Person(person.id, person.lastName, person.privacyCode))
                    }, !string.IsNullOrEmpty(person.privacyCode));
                criteria.FinancialAidPersonQueryKeyword = person.id;
                actualResult = await actualService.SearchFinancialAidPersonsAsync(criteria);
                Assert.AreEqual(expectedResult.HasPrivacyRestrictions, actualResult.HasPrivacyRestrictions);
                Assert.AreEqual(expectedResult.Dto.First().Id, actualResult.Dto.First().Id);
                Assert.AreEqual(expectedResult.Dto.First().LastName, actualResult.Dto.First().LastName);
            }

            [TestMethod]
            public async Task ListOfIdsPassed_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                var person = faPersonsRepository.personData.First();
                expectedResult = new PrivacyWrapper<IEnumerable<Dtos.Base.Person>>(new List<Dtos.Base.Person>(){
                        personAdapter.MapToType(new Person(person.id, person.lastName, person.privacyCode))
                    }, true);
                criteria.FinancialAidPersonIds = new List<string>(){person.id};
                actualResult = await actualService.SearchFinancialAidPersonsAsync(criteria);
                Assert.AreEqual(expectedResult.HasPrivacyRestrictions, actualResult.HasPrivacyRestrictions);
                Assert.AreEqual(expectedResult.Dto.Count(), actualResult.Dto.Count());
            }
        }
    }
}
