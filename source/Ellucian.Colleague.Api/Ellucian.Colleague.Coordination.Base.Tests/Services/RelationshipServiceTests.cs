// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class RelationshipServiceTests
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICurrentUserFactory> curUserFactoryMock;
        ICurrentUserFactory employeeProxyAdminUserMock;
        Mock<IRoleRepository> roleRepoMock;
        Mock<ILogger> loggerMock;
        Mock<IReferenceDataRepository> refDataRepoMock;
        Mock<IRelationshipRepository> relDataRepoMock;
        Mock<IPersonBaseRepository> personBaseRepoMock;
        Mock<ICurrentUser> thisUser;
        
        public Domain.Entities.Role proxyAdminRole;
        public Domain.Entities.Permission addAllEmployeeProxyPermission;

        private const string _primaryId = "PrimaryId";
        private const string _parentId = "ParentId";
        private const string _childId = "ChildId";
        private const string _employeeProxyAdminId = "EmployeeProxyAdminId";
        // other entities
        private const string _firstId = "FirstId";
        private const string _secondId = "SecondId";
        private const string _thirdId = "ThirdId";
        private const string _fourthId = "FourthId";
        private const string _fifthId = "FifthId";
        private string _deceasedId = "DeceasedId";
        // primary entities for different test conditions
        private const string _noRelationships = "NoRelationships";
        private const string _onlyPrimaryRelationships = "OnlyPrimaryRelationships";
        private const string _allHavePrimarySomeHaveNonPrimaryRelationships = "AllHavePrimarySomeHaveNonPrimaryRelationships";
        private const string _onlyInactiveRelationships = "OnlyInactiveRelationships";
        private const string _noneHavePrimaryRelationships = "NoneHavePrimaryRelationships";
        private const string _allHaveMultipleNonPrimaryRelationships = "AllHaveMultipleNonPrimaryRelationships";
        private const string _allHaveMultipleNonPrimaryRelationshipsByDate = "AllHaveMultipleNonPrimaryRelationshipsByDate";
        private List<string> _validPersonIds = new List<string>() { _parentId, _thirdId, _fourthId, _childId };
        private List<string> _invalidPersonIds = new List<string>() { "DNE" };

        RelationshipService relService;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            curUserFactoryMock = new Mock<ICurrentUserFactory>();
            employeeProxyAdminUserMock = new Person002UserFactory();
            roleRepoMock = new Mock<IRoleRepository>();
            refDataRepoMock = new Mock<IReferenceDataRepository>();
            relDataRepoMock = new Mock<IRelationshipRepository>();
            personBaseRepoMock = new Mock<IPersonBaseRepository>();

            // permissions mock
            proxyAdminRole = new Domain.Entities.Role(20, "EMPLOYEE PROXY ADMIN");
            addAllEmployeeProxyPermission = new Domain.Entities.Permission(BasePermissionCodes.AddAllEmployeeProxy);
            proxyAdminRole.AddPermission(addAllEmployeeProxyPermission);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { proxyAdminRole });

            // Mock a user to  test permissions
            thisUser = new Mock<ICurrentUser>();
            thisUser.Setup<bool>(x => x.IsPerson(It.IsAny<string>())).Returns(true); // all other ids are ok
            thisUser.Setup<bool>(x => x.IsPerson(_employeeProxyAdminId)).Returns(false);
            thisUser.Setup<bool>(x => x.IsPerson(_primaryId)).Returns(true); // _primaryId is OK
            thisUser.Setup<bool>(x => x.IsPerson(_parentId)).Returns(false); // _parentId is not OK

            // mock the CurrentUserFactory to return the above CurrentUser
            curUserFactoryMock.SetupGet<ICurrentUser>(y => y.CurrentUser).Returns(thisUser.Object);
            
            // mock data from the reference and relationship repositories
            refDataRepoMock.Setup<Task<IEnumerable<RelationshipType>>>(x => x.GetRelationshipTypesAsync()).Returns(Task.FromResult(RelationTypesFromRefRepo()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_primaryId)).Returns(Task.FromResult(RelationshipsFromRelRepo()));
            // mocks for GetPrimaryRelationships
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_noRelationships)).Returns(Task.FromResult(NoRelationships()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_onlyPrimaryRelationships)).Returns(Task.FromResult(RelationshipsAreOnlyPrimary()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_allHavePrimarySomeHaveNonPrimaryRelationships)).Returns(Task.FromResult(AllHavePrimarySomeHaveNonPrimaryRelationships()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_onlyInactiveRelationships)).Returns(Task.FromResult(OnlyInactiveRelationships()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_noneHavePrimaryRelationships)).Returns(Task.FromResult(NoneHavePrimaryRelationships()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_allHaveMultipleNonPrimaryRelationships)).Returns(Task.FromResult(AllHaveMultipleNonPrimaryRelationships()));
            relDataRepoMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonRelationshipsAsync(_allHaveMultipleNonPrimaryRelationshipsByDate)).Returns(Task.FromResult(AllHaveMultipleNonPrimaryRelationshipsByDate()));
            // mocks for PostRelationship
            relDataRepoMock.Setup<Task<Relationship>>(x => x.PostRelationshipAsync("ThrowException", "P", _primaryId)).Throws<RepositoryException>();
            relDataRepoMock.Setup<Task<Relationship>>(x => x.PostRelationshipAsync("GoodCall", "P", _primaryId)).Returns(Task.FromResult(new Relationship("GoodCall", _primaryId, "P", false, null, null)));

            // mocks for GetRelatedPeople
            personBaseRepoMock.Setup<Task<IEnumerable<PersonBase>>>(x => x.GetPersonsBaseAsync(_validPersonIds, false)).Returns(Task.FromResult(ValidRelatedPeople()));
            personBaseRepoMock.Setup<Task<IEnumerable<PersonBase>>>(x => x.GetPersonsBaseAsync(_invalidPersonIds, false)).Returns(Task.FromResult(InvalidRelatedPeople()));
            personBaseRepoMock.Setup<Task<PersonBase>>(x => x.GetPersonBaseAsync(It.IsAny<string>(), true)).Returns(Task.FromResult(AnyPerson()));
            relService = new RelationshipService(adapterRegistryMock.Object, curUserFactoryMock.Object, roleRepoMock.Object, loggerMock.Object,
                refDataRepoMock.Object, relDataRepoMock.Object, personBaseRepoMock.Object);

        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            adapterRegistryMock = null;
            curUserFactoryMock = null;
            roleRepoMock = null;
            refDataRepoMock = null;
            relDataRepoMock = null;
            thisUser = null;
        }

        #region GetPrimaryRelationships Tests
        #region Null Arguments
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_NullId()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_EmptyId()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(string.Empty);
        }
        #endregion

        #region Business logic
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_NoPermission()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_parentId);
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_NoRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_noRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(NoRelationships().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_OnlyPrimaryRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_onlyPrimaryRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(RelationshipsAreOnlyPrimary().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_EmployeeProxyAdmin()
        {
            relService = new RelationshipService(adapterRegistryMock.Object, employeeProxyAdminUserMock, roleRepoMock.Object, loggerMock.Object,
             refDataRepoMock.Object, relDataRepoMock.Object, personBaseRepoMock.Object);
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_employeeProxyAdminId);
            Assert.IsNotNull(relData);            
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_AllHavePrimarySomeHaveNonPrimaryRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_allHavePrimarySomeHaveNonPrimaryRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(AllHavePrimarySomeHaveNonPrimaryRelationships().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_OnlyInactiveRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_onlyInactiveRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(OnlyInactiveRelationships().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_NoneHavePrimaryRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_noneHavePrimaryRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(NoneHavePrimaryRelationships().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_AllHaveMultipleNonPrimaryRelationships()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_allHaveMultipleNonPrimaryRelationships);
            Assert.IsNotNull(relData);
            Assert.AreEqual(AllHaveMultipleNonPrimaryRelationships().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }

        [TestMethod]
        public async Task RelationshipService_GetPersonPrimaryRelationshipsAsync_AllHaveMultipleNonPrimaryRelationships_ByDate()
        {
            var relData = await relService.GetPersonPrimaryRelationshipsAsync(_allHaveMultipleNonPrimaryRelationshipsByDate);
            Assert.IsNotNull(relData);
            Assert.AreEqual(AllHaveMultipleNonPrimaryRelationshipsByDate().Where(x => x.RelationshipType.Equals("CR")).Count(), relData.Count());
        }
        #endregion
        #endregion

        #region PostRelationship Tests
        #region Null Args
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_NullIsTheId()
        {
            var relData = await relService.PostRelationshipAsync(null, "FOO", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_EmptyIsTheId()
        {
            var relData = await relService.PostRelationshipAsync("", "FOO", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_NullRelType()
        {
            var relData = await relService.PostRelationshipAsync("P1", null, "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_EmptyRelType()
        {
            var relData = await relService.PostRelationshipAsync("P1", "", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_NullOfTheId()
        {
            var relData = await relService.PostRelationshipAsync("P1", "FOO", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipService_PostRelationship_EmptyOfTheId()
        {
            var relData = await relService.PostRelationshipAsync("P1", "FOO", "");
        }
        #endregion
        #region Business logic
        // user not part of the relationship throws exception
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task RelationshipService_PostRelationship_NotForCurrentUser()
        {
            var relData = await relService.PostRelationshipAsync(_parentId, "FOO", _parentId);
        }

        // creating a relationship with oneself throws exception
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RelationshipService_PostRelationship_RelationshipWithOneself()
        {
            var relData = await relService.PostRelationshipAsync(_primaryId, "P", _primaryId);
        }

        // invalid relationship type throws exception
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RelationshipService_PostRelationship_InvalidRelationType()
        {
            var relData = await relService.PostRelationshipAsync(_primaryId, "FOO", _parentId);
        }

        // repository exception is propogated
        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipService_PostRelationship_PropogateRepositoryException()
        {
            var relData = await relService.PostRelationshipAsync("ThrowException", "P", _primaryId);
        }

        // cannot set up relationship with deceased
        [TestMethod]
        public async Task RelationshipService_PostRelationship_CanRelateToDeceased()
        {
            // Override the person base returned to be the deceased person needed for this test
            personBaseRepoMock.Setup<Task<PersonBase>>(x => x.GetPersonBaseAsync(It.IsAny<string>(), true)).Returns(Task.FromResult(DeceasedPerson()));
            var relData = await relService.PostRelationshipAsync(_deceasedId, "P", _primaryId);
        }

        // Good call returns proper DTO
        [TestMethod]
        public async Task RelationshipService_PostRelationship_Success()
        {
            var relData = await relService.PostRelationshipAsync("GoodCall", "P", _primaryId);
            Assert.AreEqual("GoodCall", relData.PrimaryEntity);
            Assert.AreEqual(_primaryId, relData.OtherEntity);
            Assert.AreEqual("P", relData.RelationshipType);
        }
        #endregion
        #endregion

        /// <summary>
        /// ICurrentUserFactory implementation for employee proxy admins
        /// </summary>
        public class Person002UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "125",
                        Name = "John",
                        PersonId = "0001200",
                        SecurityToken = "320",
                        SessionTimeout = 30,
                        UserName = "Admin",
                        Roles = new List<string>() { "EMPLOYEE PROXY ADMIN" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        #region private methods
        private IEnumerable<RelationshipType> RelationTypesFromRefRepo()
        {
            return new Collection<RelationshipType>()
                {
                    new RelationshipType("P", "Parent", "C"),
                    new RelationshipType("C", "Child", "P"),
                    new RelationshipType("CZ", "Contact", "CZ"),
                    new RelationshipType("O", "Other", "O"),
                    new RelationshipType("CO", "Companion", "CO"),
                    new RelationshipType("CZ", "Contact", "CZ"),
                    new RelationshipType("CT", "Cousin", "CT"),
                    new RelationshipType("CR", "Correct Relationship", "CR"),
                };
        }

        #region private methods for GetEntityRelationship
        private IEnumerable<Relationship> RelationshipsFromRelRepo()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_primaryId, _parentId, "C", true, null, null), // will show up in GetEntityPrimaryRelationshipsAsync
                    new Relationship(_childId, _primaryId, "C", true, null, null), // will show up in GetEntityPrimaryRelationshipsAsync
                    new Relationship(_primaryId, _thirdId, "CT", true, null, null), // will show up in GetEntityPrimaryRelationshipsAsync
                    new Relationship(_primaryId, _thirdId, "CZ", false, null, null), // won't show up in GetEntityPrimaryRelationshipsAsync - not prime
                    new Relationship(_primaryId, _fourthId, "CZ", false, null, null), // won't show up in GetEntityPrimaryRelationshipsAsync - not prime
                    new Relationship(_primaryId, _fifthId, "CZ", true, null, DateTime.Now.AddDays(-1)), // won't show up in GetEntityPrimaryRelationshipsAsync - inactive
                };
        }
        #endregion

        #region private methods for GetPrimaryRelationship
        private IEnumerable<Relationship> NoRelationships()
        {
            return new Collection<Relationship>();
        }

        private IEnumerable<Relationship> RelationshipsAreOnlyPrimary()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_onlyPrimaryRelationships, _firstId, "CR", true, null, null), // selected
                    new Relationship(_onlyPrimaryRelationships, _secondId, "CR", true, null, null), // selected
                    new Relationship(_onlyPrimaryRelationships, _thirdId, "CR", true, null, null), // selected
                };
        }

        private IEnumerable<Relationship> AllHavePrimarySomeHaveNonPrimaryRelationships()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _firstId, "CR", true, null, null), // selected
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _secondId, "CR", true, null, null), // selected
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _secondId, "O", false, null, null), // not selected
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _thirdId, "O", false, null, null), // not selected
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _thirdId, "O", false, null, null), // not selected
                    new Relationship(_allHavePrimarySomeHaveNonPrimaryRelationships, _thirdId, "CR", true, null, null), // selected
                };
        }

        private IEnumerable<Relationship> OnlyInactiveRelationships()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_onlyInactiveRelationships, _firstId, "O", true, null, DateTime.Now.AddDays(-1)), // not selected
                    new Relationship(_onlyInactiveRelationships, _secondId, "O", true, null, DateTime.Now.AddDays(-1)), // not selected
                    new Relationship(_onlyInactiveRelationships, _thirdId, "O", true, DateTime.Now.AddDays(1), null), // not selected
                };
        }

        private IEnumerable<Relationship> NoneHavePrimaryRelationships()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_noneHavePrimaryRelationships, _firstId, "CR", false, null, null), // selected
                    new Relationship(_noneHavePrimaryRelationships, _secondId, "CR", false, null, null), // selected
                    new Relationship(_noneHavePrimaryRelationships, _thirdId, "CR", false, null, null), // selected
                };
        }

        private IEnumerable<Relationship> AllHaveMultipleNonPrimaryRelationships()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _firstId, "CR", false, null, null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _secondId,"CR", false, null, null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _thirdId, "CR", false, null, null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _firstId, "O", false, null, null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _secondId,"O", false, null, null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationships, _thirdId, "O", false, null, null), // not selected
                };
        }


        private IEnumerable<Relationship> AllHaveMultipleNonPrimaryRelationshipsByDate()
        {
            return new Collection<Relationship>()
                {
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _secondId,"O", false, null, null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _firstId, "CR", false, DateTime.Now.AddDays(-1), null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _secondId,"CR", false, DateTime.Now.AddDays(-1), null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _secondId,"O", false, DateTime.Now.AddDays(-2), null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _firstId, "O", false, null, null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _thirdId, "O", false, null, null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _thirdId, "O", false, DateTime.Now.AddDays(-2), null), // not selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _thirdId, "CR", false, DateTime.Now.AddDays(-1), null), // selected
                    new Relationship(_allHaveMultipleNonPrimaryRelationshipsByDate, _firstId, "O", false, DateTime.Now.AddDays(-2), null), // not selected
                };
        }


        #endregion

        #region private methods for GetRelatedPeople

        private IEnumerable<PersonBase> ValidRelatedPeople()
        {
            var relatedPeople = new List<PersonBase>()
            {
                new PersonBase(_parentId, "Jones") { PreferredName = "Tim A. Jones" },
                new PersonBase(_childId, "Smith") { PreferredName = "Lisa B. Smith" },
                new PersonBase(_thirdId, "James") { PreferredName = "Richard C. James" },
                new PersonBase(_fourthId, "Martin") { PreferredName = "Tonya D. Martin" },
            };

            relatedPeople[0].AddEmailAddress(new EmailAddress("tim.a.jones@ellucianuniversity.edu", "PRI"));
            return relatedPeople;
        }

        private IEnumerable<PersonBase> InvalidRelatedPeople()
        {
            var relatedPeople = new List<PersonBase>()
            {
                null
            };

            return relatedPeople;
        }

        private PersonBase DeceasedPerson()
        {
            return new PersonBase(_deceasedId, "Brown") { IsDeceased = true };
        }

        private PersonBase AnyPerson()
        {
            return new PersonBase(_deceasedId, "LastName");
        }

        #endregion

        #endregion
    }
}
