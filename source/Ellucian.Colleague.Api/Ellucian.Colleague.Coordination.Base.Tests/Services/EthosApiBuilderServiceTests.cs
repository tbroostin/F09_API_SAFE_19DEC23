// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EthosApiBuilderServiceTests
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
        public class GetEthosApiBuilder : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private IEthosApiBuilderRepository _ethosApiBuilderRepository;
            private Mock<IEthosApiBuilderRepository> _ethosApiBuilderRepositoryMock;
            private Mock<IColleagueTransactionInvoker> _transManagerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private ILogger _logger;
            private EthosApiBuilderService _ethosApiBuilderService;

            private EthosApiConfiguration ethosApiConfiguration;
            private EthosExtensibleData ethosExtensibleData;

            private List<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilderCollection;
            private const string ethosApiBuilderGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

            private Domain.Entities.Permission permissionViewAnyPerson;
            private Domain.Entities.Permission permissionCreatePerson;
            private Domain.Entities.Permission permissionDeletePerson;
            Dictionary<string, string> ethosApiBuilderGuidDictionary = new Dictionary<string, string>();


            [TestInitialize]
            public void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                _ethosApiBuilderRepositoryMock = new Mock<IEthosApiBuilderRepository>();
                _ethosApiBuilderRepository = _ethosApiBuilderRepositoryMock.Object;

                _transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                permissionCreatePerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePerson);
                permissionDeletePerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.DeletePersonContact);
                facultyRole.AddPermission(permissionViewAnyPerson);
                facultyRole.AddPermission(permissionCreatePerson);
                facultyRole.AddPermission(permissionDeletePerson);
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });

                ethosApiBuilderCollection = new List<Domain.Base.Entities.EthosApiBuilder>();
                var testConfigurationRepository = new TestConfigurationRepository();

                ethosApiConfiguration = testConfigurationRepository.GetEthosApiConfigurationByResource("x-person-health", false).GetAwaiter().GetResult();

                ethosExtensibleData = testConfigurationRepository.GetExtendedEthosDataByResource("x-person-health", "1.0.0", "141", new List<string>() { ethosApiBuilderGuid }, true, false).GetAwaiter().GetResult().FirstOrDefault();
                var ethosExtensibleDataDto = new Web.Http.EthosExtend.EthosExtensibleData()
                {
                    ApiResourceName = ethosExtensibleData.ApiResourceName,
                    ApiVersionNumber = ethosExtensibleData.ApiVersionNumber,
                    ColleagueTimeZone = ethosExtensibleData.ColleagueTimeZone,
                    ResourceId = ethosExtensibleData.ResourceId,
                    ExtendedSchemaType = ethosExtensibleData.ExtendedSchemaType
                };

                var ethosApiBuilder = new Domain.Base.Entities.EthosApiBuilder(ethosApiBuilderGuid, "1", "ethosApiBuilder");

                var output = string.Empty;
                if (!ethosApiBuilderGuidDictionary.TryGetValue("1", out output))
                    ethosApiBuilderGuidDictionary.Add("1", ethosExtensibleData.ResourceId);

                ethosApiBuilderCollection.Add(ethosApiBuilder);

                var Limit = ethosApiBuilderCollection.Count();
                var filterDictionary = new Dictionary<string, EthosExtensibleDataFilter>();

                var expectedCollection = new Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<EthosApiConfiguration>(), filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

                baseConfigurationRepositoryMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ethosApiConfiguration);

                _ethosApiBuilderService = new EthosApiBuilderService(_adapterRegistry, _ethosApiBuilderRepository,
                    baseConfigurationRepository, _currentUserFactory, _roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                ethosExtensibleData = null;
                _ethosApiBuilderService = null;
                _adapterRegistry = null;
                _adapterRegistryMock = null;         
                _logger = null;
                _ethosApiBuilderRepository = null;
                _ethosApiBuilderRepositoryMock = null;
                _roleRepo = null;
                _roleRepoMock = null;
                _transManagerMock = null;
                
            }

            #region GetEthosApiBuilderAsync

            [TestMethod]
            public async Task EthosApiBuilderService_GetAllEthosApiBuilder()
            {
                var filterDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
                var filterDictionaryDto = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();
                Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int> ethosApiBuilder = new Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int>(ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid), 1);
                _ethosApiBuilderRepositoryMock.Setup( x => x.GetEthosApiBuilderAsync(It.IsAny<int>(), It.IsAny<int>(), ethosApiConfiguration, filterDictionary, It.IsAny<bool>())).ReturnsAsync(ethosApiBuilder);

                var actualEthosApiBuilder = (await _ethosApiBuilderService.GetEthosApiBuilderAsync(0,100,"person=health", filterDictionaryDto, false));

                var expectedEthosApiBuilder = (ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid)).ToList();

                Assert.IsTrue(actualEthosApiBuilder is Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>);
                
                var actual = actualEthosApiBuilder.Item1.ElementAtOrDefault(0);
                var expected = expectedEthosApiBuilder.ElementAtOrDefault(0);

                Assert.AreEqual(expected.Guid, actual.Id, "Id");
            }


            #endregion GetEthosApiBuilderAsync

            #region GetEthosApiBuilderById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById_ArgumentNullException()
            {
                await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync("", "person-health");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById_InvalidID()
            {
                IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid);
                var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilder);

                var actual = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync("invalid", "peson-health");
            }

            [TestMethod]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById()
            { 
                IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid);
                var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilder);
                
                var actual = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, "peson-health");

                var expected = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilder.Guid);

                Assert.IsTrue(actual is Dtos.EthosApiBuilder);

                Assert.AreEqual(expected.Guid, actual.Id, "Id");
            }
            #endregion GetEthosApiBuilderById

            #region PostEthosApiBuilder

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_ArgumentNullException()
            //{
            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(null);
            //}

           
            //[TestMethod]       
            //public async Task EthosApiBuilderService_PostEthosApiBuilder()
            //{
            //   var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);
        
            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");
            //    Dictionary<string, Dictionary<string, string>> personDict = new Dictionary<string, Dictionary<string, string>>();
            //    personDict.Add("0011905", new Dictionary<string, string>());
            //    personDict["0011905"].Add("PERSON.CORP.INDICATOR", "");
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.GetPersonDictionaryCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personDict);
               
            
            //    var actual = await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);

            //    Assert.AreEqual(comment.Id, actual.Id, "Id");
            //    Assert.AreEqual(comment.EthosApiBuilder, actual.EthosApiBuilder, "EthosApiBuilder");
            //    Assert.AreEqual(comment.EthosApiBuilderSubjectArea.Id, actual.EthosApiBuilderSubjectArea.Id, "EthosApiBuilderSubjectArea");
            //    Assert.AreEqual(comment.Confidentiality, actual.Confidentiality, "Confidentiality");
             
            //    Assert.AreEqual(comment.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
            //    Assert.AreEqual(comment.EnteredOn, actual.EnteredOn, "EnteredOn");
            //    Assert.AreEqual(comment.Source.Id, actual.Source.Id, "Source");
            //    Assert.AreEqual(comment.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
            //}


            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_NullId()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.Id = null;

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_EthosApiBuilderNull()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.EthosApiBuilder = null;

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_EthosApiBuilderEmpty()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.EthosApiBuilder = string.Empty;

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_SubjectMatterNull()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.SubjectMatter = null;

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_SubjectMatterPersonIdNull()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.SubjectMatter = new SubjectMatterDtoProperty()
            //    {
            //        Person = new GuidObject2() {  Id = null}
            //    };

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_EthosApiBuilderSubjectAreaIdNull()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.EthosApiBuilderSubjectArea = new GuidObject2 { Id = null };

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task EthosApiBuilderService_PostEthosApiBuilder_SourceIdNull()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

            //    comment.Source = new GuidObject2 { Id = null };

            //    await _ethosApiBuilderService.PostEthosApiBuilderAsync(comment);
            //}
          
            #endregion PostEthosApiBuilder

            #region PutEthosApiBuilder

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_PutEthosApiBuilder_NullArguments()
            //{
            //    await _ethosApiBuilderService.PutEthosApiBuilderAsync(null, null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_PutEthosApiBuilder_NullId()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);
            //    await _ethosApiBuilderService.PutEthosApiBuilderAsync(null, comment);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_PutEthosApiBuilder_NullEthosApiBuilder()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);
            //    await _ethosApiBuilderService.PutEthosApiBuilderAsync(comment.Id, null);
            //}

            //[TestMethod]
            //public async Task EthosApiBuilderService_PutEthosApiBuilder()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");
            //    Dictionary<string, Dictionary<string, string>> personDict = new Dictionary<string, Dictionary<string, string>>();
            //    personDict.Add("0011905", new Dictionary<string, string>());
            //    personDict["0011905"].Add("PERSON.CORP.INDICATOR", "");
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.GetPersonDictionaryCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personDict);


            //    var actual = await _ethosApiBuilderService.PutEthosApiBuilderAsync(comment.Id, comment);

            //    Assert.AreEqual(comment.Id, actual.Id, "Id");
            //    Assert.AreEqual(comment.EthosApiBuilder, actual.EthosApiBuilder, "EthosApiBuilder");
            //    Assert.AreEqual(comment.EthosApiBuilderSubjectArea.Id, actual.EthosApiBuilderSubjectArea.Id, "EthosApiBuilderSubjectArea");
            //    Assert.AreEqual(comment.Confidentiality, actual.Confidentiality, "Confidentiality");

            //    Assert.AreEqual(comment.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
            //    Assert.AreEqual(comment.EnteredOn, actual.EnteredOn, "EnteredOn");
            //    Assert.AreEqual(comment.Source.Id, actual.Source.Id, "Source");
            //    Assert.AreEqual(comment.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
            //}
            #endregion

            #region DeleteEthosApiBuilder

          
            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_DeleteEthosApiBuilder_NullId()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);
            //    await _ethosApiBuilderService.DeleteEthosApiBuilderByIdAsync(null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task EthosApiBuilderService_DeleteEthosApiBuilder_EmptyId()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);
            //    await _ethosApiBuilderService.DeleteEthosApiBuilderByIdAsync("");
            //}
          
            //[TestMethod]
            //public async Task EthosApiBuilderService_DeleteEthosApiBuilder()
            //{
            //    var comment = ethosApiBuilderCollection.FirstOrDefault(x => x.Id == ethosApiBuilderGuid);

            //    IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosExtensibleData.Where(x => x.Guid == ethosApiBuilderGuid);
            //    var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateDomain.Base.Entities.EthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>())).ReturnsAsync(ethosApiBuilder);
            //    _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
            //    _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");
            //    _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByGuidAsync(ethosApiBuilder.Guid)).ReturnsAsync(ethosApiBuilder);
            //    await _ethosApiBuilderService.DeleteEthosApiBuilderByIdAsync(comment.Id);
            //}
            #endregion
            
        }
    }
}