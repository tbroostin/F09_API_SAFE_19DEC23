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
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    public class PersonVisasServiceTests
    {
        
        [TestClass]
        public class PersonVisas_Tests
        {
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IPersonVisaRepository> personVisasRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ILogger> loggerMock;
            private Mock<IColleagueTransactionInvoker> transManagerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            
            private ICurrentUserFactory currentUserFactory;

            protected Ellucian.Colleague.Domain.Entities.Role viewPersonVisa = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.VISA");
            protected Ellucian.Colleague.Domain.Entities.Role updatePersonVisa = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.PERSON.VISA");

            private PersonVisasService personVisasService;
            Dtos.PersonVisa personVisaDto = null;
            List<Domain.Base.Entities.VisaTypeGuidItem> visaTypesGuidItems = new List<Domain.Base.Entities.VisaTypeGuidItem>();

            Domain.Base.Entities.PersonVisa personVisaEntity = new Domain.Base.Entities.PersonVisa("0012297", "F1");
            IEnumerable<Domain.Base.Entities.PersonVisa> personVisaEntities;

            Tuple<IEnumerable<Domain.Base.Entities.PersonVisa>, int> entitiesTuple;

            string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            string personId = "bfc549d4-c1fa-4dc5-b186-f2aabd8386c0";
            GuidLookupResult guidLookUpResult = new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = "0012297" };

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                personVisasRepositoryMock = new Mock<IPersonVisaRepository>();
                loggerMock = new Mock<ILogger>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();

                BuildObjects();

                // Set up current user
                currentUserFactory = new GenericUserFactory.PersonVisaUserFactory();

                viewPersonVisa.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.ViewAnyPersonVisa));
                updatePersonVisa.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdateAnyPersonVisa));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPersonVisa, updatePersonVisa });

                personVisasService = new PersonVisasService(adapterRegistry, personVisasRepositoryMock.Object, personRepoMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepoMock.Object, loggerMock.Object);
           }

            private void BuildObjects()
            {
                personVisaDto = new Dtos.PersonVisa()
                {
                    Id = id,
                    Entries = new List<PersonVisaEntry>() { new PersonVisaEntry() { EnteredOn = new DateTime(2016, 02, 05) } },
                    ExpiresOn = DateTime.Today.AddDays(31),
                    IssuedOn = new DateTime(2015, 10, 17),
                    Person = new GuidObject2() { Id = personId },
                    RequestedOn = new DateTime(2015, 09, 17),
                    VisaId = "V1234",
                    VisaStatus = Dtos.EnumProperties.VisaStatus.Current,
                    VisaType = new VisaType2() { Detail = new GuidObject2("4fdb11b2-2e53-4a5a-890e-0e04858ca2b5"), VisaTypeCategory = Dtos.VisaTypeCategory.NonImmigrant }
                };

                personVisaEntities = new List<Domain.Base.Entities.PersonVisa>() 
                {
                    new Domain.Base.Entities.PersonVisa("1", "F1"){ Guid = "7e18c8e4-9036-4d39-9e51-0eea9576f941", EntryDate = new DateTime(2016, 02, 05), ExpireDate = new DateTime(2017, 10, 17), IssueDate = new DateTime(2015, 10, 17), RequestDate = new DateTime(2015, 09, 17), VisaNumber = "1"},
                    new Domain.Base.Entities.PersonVisa("2", "F1"){ Guid = "a4431ef5-0463-48ff-9a67-85e912346a1c", EntryDate = new DateTime(2016, 02, 05), ExpireDate = DateTime.Today.AddMonths(11), IssueDate = new DateTime(2015, 10, 17), RequestDate = new DateTime(2015, 09, 17), VisaNumber = "2"},
                    new Domain.Base.Entities.PersonVisa("3", "F1"){ Guid = "8820ed57-e930-4b3f-b9bc-30a01a291d35", EntryDate = new DateTime(2016, 02, 05), ExpireDate = DateTime.Today.AddMonths(21), IssueDate = new DateTime(2015, 10, 17), RequestDate = new DateTime(2015, 09, 17), VisaNumber = "3"},
                    new Domain.Base.Entities.PersonVisa("4", "F1"){ Guid = "f5698dcd-d189-4717-b60c-6a6806da9a73", EntryDate = new DateTime(2016, 02, 05), ExpireDate = DateTime.Today.AddDays(31), IssueDate = new DateTime(2015, 10, 17), RequestDate = new DateTime(2015, 09, 17), VisaNumber = "4"}
                };
                entitiesTuple = new Tuple<IEnumerable<Domain.Base.Entities.PersonVisa>, int>(personVisaEntities, personVisaEntities.Count());

                personVisaEntity.Guid = id;
                personVisaEntity.PersonGuid = personId;
                personVisaEntity.VisaNumber = "V1234";
                personVisaEntity.RequestDate = new DateTime(2015, 09, 17);
                personVisaEntity.IssueDate = new DateTime(2015, 10, 17);
                personVisaEntity.ExpireDate = DateTime.Today.AddDays(31);
                personVisaEntity.EntryDate = new DateTime(2016, 02, 05);

                visaTypesGuidItems = new TestVisaTypeRepository().GetVisaTypes().ToList();
                visaTypesGuidItems.Add(new VisaTypeGuidItem("4fdb11b2-2e53-4a5a-890e-0e04858ca2b5", "F1", "Nonimmigrant student", Domain.Base.Entities.VisaTypeCategory.NonImmigrant));
            }

            [TestCleanup]
            public void Cleanup()
            {
                personVisasService = null;
                personVisaDto = null;
                personVisaEntity = null;
            }

            #region All methods

            #region GET

            [TestMethod]
            public async Task PersonVisas_GetPersonVisaAllAsync()
            {
                personVisasRepositoryMock.Setup(i => i.GetAllPersonVisasAsync(0, 4, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(entitiesTuple);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);

                var actuals = await personVisasService.GetAllAsync(0, 4, It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actuals);
                foreach (var actual in actuals.Item1)
                {
                    var expected = personVisaEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.VisaNumber, actual.VisaId);
                    Assert.AreEqual(expected.IssueDate, actual.IssuedOn);
                    Assert.AreEqual(expected.ExpireDate, actual.ExpiresOn);
                    Assert.AreEqual(expected.EntryDate, actual.Entries.First().EnteredOn);
                }
            }
            [TestMethod]
            public async Task PersonVisas_GetPersonVisaAllAsync_DoesNotThrowIfPagedPastEndOfList()
            {
                personVisasRepositoryMock.Setup(i => i.GetAllPersonVisasAsync(0, 900, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);

                var actuals = await personVisasService.GetAllAsync(0, 900, It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actuals);
                Assert.IsInstanceOfType(actuals.Item1, typeof(IEnumerable<Dtos.PersonVisa>));
                Assert.IsTrue(actuals.Item1.Count() == 0);
                Assert.IsTrue(actuals.Item2 == 0);
   
            }
            [TestMethod]
            public async Task PersonVisas_GetPersonVisaByIdAsync()
            {
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);

                var result = await personVisasService.GetPersonVisaByIdAsync(id);
                Assert.AreEqual(personVisaEntity.EntryDate, result.Entries.FirstOrDefault().EnteredOn);
                Assert.AreEqual(personVisaEntity.ExpireDate, result.ExpiresOn);
                Assert.AreEqual(personVisaEntity.Guid, result.Id);
                Assert.AreEqual(personVisaEntity.IssueDate, result.IssuedOn);
                Assert.AreEqual(personVisaEntity.PersonGuid, result.Person.Id);
                Assert.AreEqual(personVisaEntity.RequestDate, result.RequestedOn);
                Assert.AreEqual(personVisaEntity.VisaNumber, result.VisaId);
            }            
            #endregion

            #region PUT
            [TestMethod]
            public async Task PersonVisas_PutPersonVisaAsync()
            {
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);
                personVisasRepositoryMock.Setup(i => i.UpdatePersonVisaAsync(It.IsAny<PersonVisaRequest>())).ReturnsAsync(new PersonVisaResponse() { PersonId = personId, StrGuid = id });

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);

                Assert.AreEqual(personVisaDto.Entries.FirstOrDefault().EnteredOn, result.Entries.FirstOrDefault().EnteredOn);
                Assert.AreEqual(personVisaDto.ExpiresOn, result.ExpiresOn);
                Assert.AreEqual(personVisaDto.Id, result.Id);
                Assert.AreEqual(personVisaDto.IssuedOn, result.IssuedOn);
                Assert.AreEqual(personVisaDto.Person.Id, result.Person.Id);
                Assert.AreEqual(personVisaDto.RequestedOn, result.RequestedOn);
                Assert.AreEqual(personVisaDto.VisaId, result.VisaId);
                Assert.AreEqual(personVisaDto.VisaStatus, result.VisaStatus);
                Assert.AreEqual(personVisaDto.VisaType.Detail.Id, result.VisaType.Detail.Id);
                Assert.AreEqual(personVisaDto.VisaType.VisaTypeCategory, result.VisaType.VisaTypeCategory);
            }

            [TestMethod]
            public async Task PersonVisas_PutPersonVisaAsync_VisaTypeDetailNull_Immigrant()
            {
                personVisaDto.VisaType.Detail = null;
                personVisaDto.VisaType.VisaTypeCategory = Dtos.VisaTypeCategory.Immigrant;

                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);
                personVisasRepositoryMock.Setup(i => i.UpdatePersonVisaAsync(It.IsAny<PersonVisaRequest>())).ReturnsAsync(new PersonVisaResponse() { PersonId = personId, StrGuid = id });

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);

                Assert.AreEqual(personVisaDto.Entries.FirstOrDefault().EnteredOn, result.Entries.FirstOrDefault().EnteredOn);
                Assert.AreEqual(personVisaDto.ExpiresOn, result.ExpiresOn);
                Assert.AreEqual(personVisaDto.Id, result.Id);
                Assert.AreEqual(personVisaDto.IssuedOn, result.IssuedOn);
                Assert.AreEqual(personVisaDto.Person.Id, result.Person.Id);
                Assert.AreEqual(personVisaDto.RequestedOn, result.RequestedOn);
                Assert.AreEqual(personVisaDto.VisaId, result.VisaId);
                Assert.AreEqual(personVisaDto.VisaStatus, result.VisaStatus);
            }

            [TestMethod]
            public async Task PersonVisas_PutPersonVisaAsync_VisaTypeDetailNull_NonImmigrant()
            {
                personVisaDto.VisaType.Detail = null;
                personVisaDto.VisaType.VisaTypeCategory = Dtos.VisaTypeCategory.NonImmigrant;

                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);
                personVisasRepositoryMock.Setup(i => i.UpdatePersonVisaAsync(It.IsAny<PersonVisaRequest>())).ReturnsAsync(new PersonVisaResponse() { PersonId = personId, StrGuid = id });

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);

                Assert.AreEqual(personVisaDto.Entries.FirstOrDefault().EnteredOn, result.Entries.FirstOrDefault().EnteredOn);
                Assert.AreEqual(personVisaDto.ExpiresOn, result.ExpiresOn);
                Assert.AreEqual(personVisaDto.Id, result.Id);
                Assert.AreEqual(personVisaDto.IssuedOn, result.IssuedOn);
                Assert.AreEqual(personVisaDto.Person.Id, result.Person.Id);
                Assert.AreEqual(personVisaDto.RequestedOn, result.RequestedOn);
                Assert.AreEqual(personVisaDto.VisaId, result.VisaId);
                Assert.AreEqual(personVisaDto.VisaStatus, result.VisaStatus);
            }
            #endregion
            
            #region POST
            [TestMethod]
            public async Task PersonVisas_PostPersonVisaAsync()
            {
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);
                personVisasRepositoryMock.Setup(i => i.UpdatePersonVisaAsync(It.IsAny<PersonVisaRequest>())).ReturnsAsync(new PersonVisaResponse() { PersonId = personId, StrGuid = id });

                var result = await personVisasService.PostPersonVisaAsync(personVisaDto);

                Assert.AreEqual(personVisaDto.Entries.FirstOrDefault().EnteredOn, result.Entries.FirstOrDefault().EnteredOn);
                Assert.AreEqual(personVisaDto.ExpiresOn, result.ExpiresOn);
                Assert.AreEqual(personVisaDto.Id, result.Id);
                Assert.AreEqual(personVisaDto.IssuedOn, result.IssuedOn);
                Assert.AreEqual(personVisaDto.Person.Id, result.Person.Id);
                Assert.AreEqual(personVisaDto.RequestedOn, result.RequestedOn);
                Assert.AreEqual(personVisaDto.VisaId, result.VisaId);
                Assert.AreEqual(personVisaDto.VisaStatus, result.VisaStatus);
                Assert.AreEqual(personVisaDto.VisaType.Detail.Id, result.VisaType.Detail.Id);
                Assert.AreEqual(personVisaDto.VisaType.VisaTypeCategory, result.VisaType.VisaTypeCategory);
            }
            #endregion

            #region DELETE
            [TestMethod]
            public async Task PersonVisas_DeletePersonVisaAsync()
            {
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                await personVisasService.DeletePersonVisaAsync(id);
            }
            #endregion

            #endregion

            #region All Exceptions


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonVisas_GetPersonVisaByIdAsync_ArgumentNullException()
            {
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ThrowsAsync(new ArgumentNullException());
                var result  = await personVisasService.GetPersonVisaByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_GetPersonVisaByIdAsync_Entity_KeyNotFoundException()
            {
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ThrowsAsync(new KeyNotFoundException());
                var result = await personVisasService.GetPersonVisaByIdAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonVisas_GetPersonVisaByIdAsync_VisaType_Empty_KeyNotFoundException()
            {
                personVisaEntity = new Domain.Base.Entities.PersonVisa("0012297", "");
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);

                var result = await personVisasService.GetPersonVisaByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_GetPersonVisaByIdAsync_VisaTypeGuidItem_KeyNotFoundException()
            {
                personVisaEntity = new Domain.Base.Entities.PersonVisa("0012297", " F-1");
                personVisasRepositoryMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisaEntity);
                referenceDataRepositoryMock.Setup(i => i.GetVisaTypesAsync(It.IsAny<bool>())).ReturnsAsync(visaTypesGuidItems);

                var result = await personVisasService.GetPersonVisaByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_Put_VisaTypeDetailNull_KeyNotFoundException()
            {
                personVisaDto.VisaType.Detail = null;
                personVisaDto.VisaType.VisaTypeCategory = Dtos.VisaTypeCategory.Immigrant;
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_Put_VisaTypeEntityNull_KeyNotFoundException()
            {
                personVisaDto.VisaType.Detail = new GuidObject2("5678");
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }           

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_Update_PersonLookUpNull_KeyNotFoundException()
            {
                personVisaDto.Person = new GuidObject2("1234");
                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonVisas_Update_PersonLookUpPrimKeysDontMatch_InvalidOperationException()
            {
                GuidLookupResult tempGuidLookUpResult = new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = "0012676" };
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(tempGuidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_Update_BothGuidLookUpNull_KeyNotFoundException()
            {
                GuidLookupResult tempGuidLookUpResult = new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = "" };
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(tempGuidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(tempGuidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonVisas_Update_BothGuidLookUpNull_InvalidOperationException()
            {
                personVisaDto.ExpiresOn = DateTime.Today.Subtract(new TimeSpan(24, 0, 0));
                personVisaDto.VisaStatus = Dtos.EnumProperties.VisaStatus.Current;
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonVisas_Update_ExpiredOnToday_VisaStatusExpired_InvalidOperationException()
            {
                personVisaDto.ExpiresOn = DateTime.Today;
                personVisaDto.VisaStatus = Dtos.EnumProperties.VisaStatus.Expired;
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonVisas_Update_ExpiredOnNull_VisaStatusExpired_InvalidOperationException()
            {
                personVisaDto.ExpiresOn = null;
                personVisaDto.VisaStatus = Dtos.EnumProperties.VisaStatus.Expired;
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                personVisasRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(personId)).ReturnsAsync(guidLookUpResult);

                var result = await personVisasService.PutPersonVisaAsync(id, personVisaDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonVisas_DeletePersonVisaAsync_ArgumentNullException()
            {
                await personVisasService.DeletePersonVisaAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisas_DeletePersonVisaAsync_KeyNotFoundException()
            {
                await personVisasService.DeletePersonVisaAsync("1234");
            }
            #endregion
        }      
    }
}
