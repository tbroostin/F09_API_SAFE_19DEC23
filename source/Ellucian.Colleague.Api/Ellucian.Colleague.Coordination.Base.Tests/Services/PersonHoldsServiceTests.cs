using Ellucian.Colleague.Domain.Base.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    public class PersonHoldsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role deleteRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "DELETE.PERSON.HOLD");
            protected Ellucian.Colleague.Domain.Entities.Role viewRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.HOLD");
            protected Ellucian.Colleague.Domain.Entities.Role createUpdateRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.PERSON.HOLD");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "DELETE.PERSON.HOLD", "VIEW.PERSON.HOLD", "UPDATE.PERSON.HOLD" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class PersonHoldsService_Tests: CurrentUserSetup
        {
            private Mock<IPersonHoldsService> personHoldServiceMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IReferenceDataRepository> referenceRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IPersonHoldsRepository> personHoldsRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<ILogger> loggerMock;
            private Mock<IColleagueTransactionInvoker> transManagerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private ICurrentUserFactory currentUserFactory;
            private PersonHoldsService personHoldService;
            private List<Dtos.PersonHold> personHolds;
            private ICollection<PersonRestriction> allPersonRestrictionsEntities;

            private ICollection<Domain.Base.Entities.Restriction> personHoldsTypeCollection = new List<Domain.Base.Entities.Restriction>();

            [TestInitialize]
            public void Initialize()
            {
                personHoldServiceMock = new Mock<IPersonHoldsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                personHoldsRepoMock = new Mock<IPersonHoldsRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                personHoldService = new PersonHoldsService(adapterRegistryMock.Object, referenceRepositoryMock.Object, personRepoMock.Object,
                    personHoldsRepoMock.Object, baseConfigurationRepository, currentUserFactory, roleRepoMock.Object, loggerMock.Object);

                deleteRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.DeletePersonHold));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.ViewPersonHold));
                createUpdateRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.CreateUpdatePersonHold));

                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole, createUpdateRegistrationRole });


                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("73244057-d1ec-4094-a0b7-de602533e3a6", "R0001", "Advisor Approval", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Disciplinary });
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("1df164eb-8178-4321-a9f7-24f12d3991d8", "R0002", "Test", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Financial });
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("4af374ab-8908-4091-a7k7-24i02d9931d8", "R0003", "Business Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Administrative });
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("8e09a1e0-4d22-462c-95fe-b8be971bed33", "R0001", "Academic Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Academic });
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("0f291189-4d3b-48f2-aa62-11c94c32f815", "R0002", "Health Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Academic });
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("d57783ad-8f9e-4805-8e5d-c10452ee375d", "R0004", "Academic2 Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Financial });

                referenceRepositoryMock.Setup(repo => repo.RestrictionsAsync()).ReturnsAsync(personHoldsTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(false)).ReturnsAsync(personHoldsTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(true)).ReturnsAsync(personHoldsTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(false)).ReturnsAsync(personHoldsTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(true)).ReturnsAsync(personHoldsTypeCollection);

                personHolds = new TestPersonHoldsRepository().GetPersonHolds().ToList();
                allPersonRestrictionsEntities = new TestPersonRestrictionRepository().Get().ToList();
                var item = allPersonRestrictionsEntities.Last(i => i.Id == "6");
                allPersonRestrictionsEntities.Remove(item);
                foreach (var personRestrictionsEntity in allPersonRestrictionsEntities)
                {
                    personRestrictionsEntity.Comment = string.Empty;                    
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                personHoldsTypeCollection = null;
                personHolds = null;
                allPersonRestrictionsEntities = null;
            }

            #region All methods
            
            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldsAsync()
            {
                var tuple = new Tuple<IEnumerable<PersonRestriction>, int>( allPersonRestrictionsEntities , 5);
                
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(tuple); //allPersonRestrictionsEntities);
                personHoldsRepoMock.SetupSequence(p => p.GetStudentHoldGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("1"))
                    .Returns(Task.FromResult("2"))
                    .Returns(Task.FromResult("3"))
                    .Returns(Task.FromResult("4"))
                    .Returns(Task.FromResult("5"))
                    .Returns(Task.FromResult("6"));
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var results = await personHoldService.GetPersonHoldsAsync(0,20);

                Assert.AreEqual(5, results.Item2);             
            }
            
            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldByIdAsync()
            {
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync("2")).ReturnsAsync("9a9bdb5f-b827-4ea0-80cc-c8b9ac17325b");
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var result = await personHoldService.GetPersonHoldAsync("2");

                Assert.AreEqual(personRestriction.StartDate.Value.Date, result.StartOn.Value.Date);
                Assert.AreEqual(personRestriction.EndDate.Value.Date, result.EndOn.Value.Date);
            }

            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldByIdAsync_Y_NotifyIndicator()
            {
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                personRestriction.NotificationIndicator = "Y";
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync("2")).ReturnsAsync("9a9bdb5f-b827-4ea0-80cc-c8b9ac17325b");
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var result = await personHoldService.GetPersonHoldAsync("2");
                string notifyValue = result.NotificationIndicator.Value.ToString().Equals("Notify", StringComparison.InvariantCultureIgnoreCase) ? "Y" : "";
                Assert.AreEqual(personRestriction.NotificationIndicator, notifyValue);
            }

            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldByIdAsync_Restr_Academic()
            {
                personHoldsTypeCollection.Add(
                    new Domain.Base.Entities.Restriction("d57783ad-8f9e-4805-8e5d-c10452ee375d", "7", "Academic2 Office Hold", null, null, null, null, null, null, null, null, null) 
                    { 
                        RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Academic 
                    });
                PersonRestriction personRestriction = new PersonRestriction("2", "2", "7", new DateTime(2016,01, 01), new DateTime(2016, 01, 31), null, "");
                
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync("2")).ReturnsAsync("9a9bdb5f-b827-4ea0-80cc-c8b9ac17325b");
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var result = await personHoldService.GetPersonHoldAsync("2");

                Assert.AreEqual(result.PersonHoldTypeType.PersonHoldCategory.Value, PersonHoldCategoryTypes.Academic);
            }

            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldByIdAsync_Restr_Health()
            {
                personHoldsTypeCollection.Add(new Domain.Base.Entities.Restriction("d57783ad-8f9e-4805-8e5d-c10452ee375d", "7", "Academic2 Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Health });
                PersonRestriction personRestriction = new PersonRestriction("2", "2", "7", new DateTime(2016, 01, 01), new DateTime(2016, 01, 31), null, "");

                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync("2")).ReturnsAsync("9a9bdb5f-b827-4ea0-80cc-c8b9ac17325b");
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var result = await personHoldService.GetPersonHoldAsync("2");

                Assert.AreEqual(result.PersonHoldTypeType.PersonHoldCategory.Value, PersonHoldCategoryTypes.Health);
            }

            [TestMethod]
            public async Task PersonHoldService_GetPersonHoldsAsyncBy_PersonId()
            {
                List<Domain.Base.Entities.PersonRestriction> personRestrictionsByPersonId = allPersonRestrictionsEntities.Where(i => i.StudentId.Equals("S0001", StringComparison.InvariantCultureIgnoreCase)).ToList();
                personHoldsRepoMock.Setup(i => i.GetPersonHoldsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(personRestrictionsByPersonId);
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.SetupSequence(p => p.GetStudentHoldGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("1"))
                    .Returns(Task.FromResult("2"))
                    .Returns(Task.FromResult("3"));
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("29c20aa1-c1f1-4be9-8af9-d63cc92afcee");

                var results = await personHoldService.GetPersonHoldsAsync("S0001");

                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task PersonHoldService_DeletePersonHoldAsync()
            {
                deleteRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.DeletePersonHold));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { deleteRegistrationRole });

                List<PersonHoldResponse> resp = new List<PersonHoldResponse>() 
                { 
                    new PersonHoldResponse() { PersonHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3", PersonHoldId = "1", WarningCode = "123", WarningMessage = "Some warning message" } 
                };
                personHoldsRepoMock.Setup(i => i.GetStudentHoldIdFromGuidAsync("23977f85-f200-479f-9eee-3921bb4667d3")).ReturnsAsync("1");
                personHoldsRepoMock.Setup(i => i.DeletePersonHoldsAsync("1")).ReturnsAsync(resp);

                await personHoldService.DeletePersonHoldAsync("23977f85-f200-479f-9eee-3921bb4667d3");
            }

            [TestMethod]
            public async Task DeletePersonHoldAsync_PersonHold_Response()
            {
                deleteRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.DeletePersonHold));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { deleteRegistrationRole });

                personHoldsRepoMock.Setup(i => i.GetStudentHoldIdFromGuidAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee")).ReturnsAsync("45");
                personHoldsRepoMock.Setup(i => i.DeletePersonHoldsAsync("45")).ReturnsAsync(new List<PersonHoldResponse>() 
                {
                    new PersonHoldResponse(){ PersonHoldGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee", PersonHoldId= "123", WarningCode="Error", WarningMessage="Warning Message"}
                });
                await personHoldService.DeletePersonHoldAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");
            }

            [TestMethod]
            public async Task PersonHoldService_UpdatePersonHoldAsync()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                PersonHoldRequest req = new PersonHoldRequest("1", "1234", "R0004", personHold.StartOn, personHold.EndOn, string.Empty);
                PersonHoldResponse resp = new PersonHoldResponse() 
                {
                    PersonHoldGuid = personHoldGuid,
                    PersonHoldId = "1234",
                    WarningCode = "567",
                    WarningMessage = "Some warning message"
                };
                UpdateRestrictionRequest updateRequest = new UpdateRestrictionRequest()
                {
                    StrComments = req.Comments,
                    StrEndDate = (req.EndOn == null) ? default(DateTime?) : req.EndOn.Value.Date,
                    StrGuid = req.Id,
                    StrNotify = true,
                    StrRestriction = req.RestrictionType,
                    StrStartDate = req.StartOn.Value.Date,
                    StrStudent = req.PersonId,
                    StudentRestrictionsId = "2"
                };
                UpdateRestrictionResponse updateResponse = new UpdateRestrictionResponse() { StrGuid = personHoldGuid, StudentRestrictionsId = "1234" };

                personHoldsRepoMock.Setup(i => i.GetStudentHoldIdFromGuidAsync(personHolds.First().Id)).ReturnsAsync("1");
                personRepoMock.Setup(i => i.GetPersonByGuidNonCachedAsync(studentGuid)).ReturnsAsync(new Domain.Base.Entities.Person("1234", "Bhole") { FirstName = "Jon", PersonCorpIndicator = "" });
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.UpdatePersonHoldAsync(It.IsAny < PersonHoldRequest>())).ReturnsAsync(resp);
                personHoldServiceMock.Setup(i => i.GetPersonHoldAsync(It.IsAny<string>())).ReturnsAsync(personHold);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync(It.IsAny<string>())).ReturnsAsync(personRestriction);
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(studentGuid);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(personHoldGuid);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateRestrictionRequest, UpdateRestrictionResponse>(updateRequest)).ReturnsAsync(updateResponse);


                var result = await personHoldService.UpdatePersonHoldAsync("23977f85-f200-479f-9eee-3921bb4667d3", personHold);

                Assert.AreEqual(personHold.Id, result.Id);
                Assert.AreEqual(personHold.Comment, result.Comment);
                Assert.AreEqual(personHold.EndOn.Value.Date, result.EndOn.Value.Date);
                Assert.AreEqual(personHold.StartOn.Value.Date, result.StartOn.Value.Date);
                Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, result.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType.PersonHoldCategory.Value, result.PersonHoldTypeType.PersonHoldCategory.Value);
            }

            [TestMethod]
            public async Task PersonHoldService_CreatePersonHoldAsync()
            {
                string personHoldGuid = "00000000-0000-0000-0000-000000000000";//"23977f85-f200-479f-9eee-3921bb4667d3";
                string newGuid = "eb254904-ef8b-4378-84ed-a2be73ef4f35";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                PersonHoldRequest req = new PersonHoldRequest("1", "1234", "R0004", personHold.StartOn, personHold.EndOn, string.Empty);
                PersonHoldResponse resp = new PersonHoldResponse()
                {
                    PersonHoldGuid = newGuid,
                    PersonHoldId = "1234",
                    WarningCode = "567",
                    WarningMessage = "Some warning message"
                };
                UpdateRestrictionRequest updateRequest = new UpdateRestrictionRequest()
                {
                    StrComments = req.Comments,
                    StrEndDate = (req.EndOn == null) ? default(DateTime?) : req.EndOn.Value.Date,
                    StrGuid = req.Id,
                    StrNotify = true,
                    StrRestriction = req.RestrictionType,
                    StrStartDate = req.StartOn.Value.Date,
                    StrStudent = req.PersonId,
                    StudentRestrictionsId = "2"
                };
                UpdateRestrictionResponse updateResponse = new UpdateRestrictionResponse() { StrGuid = newGuid, StudentRestrictionsId = "1234" };

                personHoldsRepoMock.Setup(i => i.GetStudentHoldIdFromGuidAsync(personHolds.First().Id)).ReturnsAsync("1");
                personRepoMock.Setup(i => i.GetPersonByGuidNonCachedAsync(studentGuid)).ReturnsAsync(new Domain.Base.Entities.Person("1234", "Bhole") { FirstName= "Jon", PersonCorpIndicator = "" });
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHoldsRepoMock.Setup(i => i.UpdatePersonHoldAsync(It.IsAny<PersonHoldRequest>())).ReturnsAsync(resp);
                personHoldServiceMock.Setup(i => i.GetPersonHoldAsync(It.IsAny<string>())).ReturnsAsync(personHold);
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync(It.IsAny<string>())).ReturnsAsync(personRestriction);
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(studentGuid);
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(newGuid);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateRestrictionRequest, UpdateRestrictionResponse>(updateRequest)).ReturnsAsync(updateResponse);


                var result = await personHoldService.CreatePersonHoldAsync(personHold);

                Assert.AreEqual(newGuid, result.Id);
                Assert.AreEqual(personHold.Comment, result.Comment);
                Assert.AreEqual(personHold.EndOn.Value.Date, result.EndOn.Value.Date);
                Assert.AreEqual(personHold.StartOn.Value.Date, result.StartOn.Value.Date);
                Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, result.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType.PersonHoldCategory.Value, result.PersonHoldTypeType.PersonHoldCategory.Value);
            }
            #endregion

            #region All Exceptions
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonHoldByIdAsync_EmptyString_ArgumentNullException()
            {
                var result = await personHoldService.GetPersonHoldAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonHoldByIdAsync_NullId_ArgumentNullException()
            {
                var result = await personHoldService.GetPersonHoldAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonHoldByIdAsync_InvalidId_ArgumentNullException()
            {
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync(It.IsAny<string>())).ReturnsAsync(null);
                var result = await personHoldService.GetPersonHoldAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonHoldByIdAsync_PeronId_Null_ArgumentNullException()
            {
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");

                var result = await personHoldService.GetPersonHoldAsync("2");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonHoldByIdAsync_Restriction_Null_ArgumentNullException()
            {
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                PersonRestriction personRestriction = new PersonRestriction("2", "48285630-3f7f-4b16-b73d-1b1a4bdd27ee", "someId", null, null, 1, "Y");
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");


                var result = await personHoldService.GetPersonHoldAsync("2");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonHoldByIdAsync_PersonHold_Null_ArgumentNullException()
            {
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                PersonRestriction personRestriction = allPersonRestrictionsEntities.FirstOrDefault(i => i.Id.Equals("2"));
                personHoldsRepoMock.Setup(i => i.GetPersonHoldByIdAsync("2")).ReturnsAsync(personRestriction);
                personRepoMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");
                personHoldsRepoMock.Setup(i => i.GetStudentHoldGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");

                var result = await personHoldService.GetPersonHoldAsync("2");
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task GetPersonHoldsAsync_NUll_Id_ArgumentNullException()
            //{
            //    var result = await personHoldService.GetPersonHoldsAsync("");
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DeletePersonHoldAsync_PersonHoldId_Null_ArgumentNullException()
            {
                deleteRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.PersonHoldsPermissionCodes.DeletePersonHold));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { deleteRegistrationRole });

                personHoldsRepoMock.Setup(i => i.GetStudentHoldIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                await personHoldService.DeletePersonHoldAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DeletePersonHoldAsync_PermissionException()
            {
                deleteRegistrationRole.AddPermission(null);
                await personHoldService.DeletePersonHoldAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.Person = null;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonIdNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.Person.Id = string.Empty;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonHoldTypeTypeNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.PersonHoldTypeType = null;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Create_Update_PersonHoldTypeTypeNull_KeyNotFoundException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";

                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.PersonHoldTypeType.Detail.Id = "528ea4c6-b778-4a2c-8a68-b5878c70e9b3";
                personRepoMock.Setup(i => i.GetPersonByGuidNonCachedAsync(studentGuid)).ReturnsAsync(new Domain.Base.Entities.Person("1234", "Bhole") { FirstName = "Jon", PersonCorpIndicator = "" });
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonHoldCategoryNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.PersonHoldTypeType.PersonHoldCategory = null;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonHoldCategoryNull_DetailId_NotNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.PersonHoldTypeType.PersonHoldCategory = null;
                personHold.PersonHoldTypeType.Detail = null;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Create_Update_PersonHoldTypeType_DetailId_Null_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.PersonHoldTypeType.Detail.Id = string.Empty;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Create_Update_StartOn_GT_EndOn_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personHold.StartOn = DateTimeOffset.MaxValue;
                personHold.EndOn = DateTimeOffset.MinValue;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Create_Update_GetPersonIdFromGuidAsync_personIdNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personRepoMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Create_Update_GetPersonIdFromGuidAsync_HoldTypeNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";

                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personRepoMock.Setup(i => i.GetPersonIdFromGuidAsync(studentGuid)).ReturnsAsync("1234");
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHold.PersonHoldTypeType.Detail.Id = "abc";
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Create_Update_GetPersonIdFromGuidAsync_WithDetailNull_HoldTypeNull_ArgumentNullException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";

                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personRepoMock.Setup(i => i.GetPersonByGuidNonCachedAsync(studentGuid)).ReturnsAsync(new Domain.Base.Entities.Person("1234", "Bhole") { FirstName = "Jon", PersonCorpIndicator = "" });
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHold.PersonHoldTypeType.Detail = null;
                personHold.PersonHoldTypeType.PersonHoldCategory = PersonHoldCategoryTypes.Health;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Create_Update_GetPersonIdFromGuidAsync_PersonCorpIndicatorIsY_InvalidOperationException()
            {
                string personHoldGuid = "23977f85-f200-479f-9eee-3921bb4667d3";
                string studentGuid = "48285630-3f7f-4b16-b73d-1b1a4bdd27ee";

                Dtos.PersonHold personHold = personHolds.FirstOrDefault(i => i.Id == personHoldGuid);
                personRepoMock.Setup(i => i.GetPersonByGuidNonCachedAsync(studentGuid)).ReturnsAsync(new Domain.Base.Entities.Person("1234", "Bhole") { FirstName = "Jon", PersonCorpIndicator = "Y" });
                referenceRepositoryMock.Setup(i => i.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ReturnsAsync(personHoldsTypeCollection);
                personHold.PersonHoldTypeType.Detail = null;
                personHold.PersonHoldTypeType.PersonHoldCategory = PersonHoldCategoryTypes.Health;
                await personHoldService.UpdatePersonHoldAsync(personHoldGuid, personHold);
            }
            #endregion
        }      
    }
}
