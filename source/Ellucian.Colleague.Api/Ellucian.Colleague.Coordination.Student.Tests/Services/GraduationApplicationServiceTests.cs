// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
     public class GraduationApplicationServiceTests
     {
          // Sets up a Current user 
          public abstract class CurrentUserSetup
          {
            protected Role advisorRole = new Role(105, "Advisor");

            public class UserFactory : ICurrentUserFactory
               {
                    public ICurrentUser CurrentUser
                    {
                         get
                         {
                              return new CurrentUser(new Claims()
                              {
                                   ControlId = "123",
                                   Name = "George Smith",
                                   PersonId = "0000011",
                                   SecurityToken = "321",
                                   SessionTimeout = 30,
                                   UserName = "GSmith",
                                   Roles = new List<string>() { "Faculty" },
                                   SessionFixationId = "abc123"
                              });
                         }
                    }
               }

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000111",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

          [TestClass]
          public class GetGraduationApplication
          {
               private Mock<IAdapterRegistry> adapterRegistryMock;
               private IAdapterRegistry adapterRegistry;
               private ILogger logger;
               private ICurrentUserFactory currentUserFactory;
               private Mock<IRoleRepository> roleRepositoryMock;
               private IRoleRepository roleRepository;
               private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
               private IGraduationApplicationRepository graduationApplicationRepository;
               private Mock<ITermRepository> termRepositoryMock;
               private ITermRepository termRepository;
               private Mock<IAddressRepository> addressRepositoryMock;
               private IAddressRepository addressRepository;
               private Mock<IProgramRepository> programRepositoryMock;
               private IProgramRepository programRepository;
               private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
               private IStudentConfigurationRepository configurationRepository;
               private Mock<IStudentRepository> studentRepositoryMock;
               private IStudentRepository studentRepository;
               private IGraduationApplicationService graduationApplicationService;
               private GraduationApplication graduationApplicationEntityData;
               private List<Address> addresses = new List<Address>();
               private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
               private IConfigurationRepository baseConfigurationRepository;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;

            [TestInitialize]
               public void Initialize()
               {
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    adapterRegistry = adapterRegistryMock.Object;
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    roleRepository = roleRepositoryMock.Object;
                    termRepositoryMock = new Mock<ITermRepository>();
                    termRepository = termRepositoryMock.Object;
                    addressRepositoryMock = new Mock<IAddressRepository>();
                    addressRepository = addressRepositoryMock.Object;
                    programRepositoryMock = new Mock<IProgramRepository>();
                    programRepository = programRepositoryMock.Object;
                    studentRepositoryMock = new Mock<IStudentRepository>();
                    studentRepository = studentRepositoryMock.Object;
                    configurationRepositoryMock=new Mock<IStudentConfigurationRepository>();
                    configurationRepository = configurationRepositoryMock.Object;
                    graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                    graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                    baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                    baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                    logger = new Mock<ILogger>().Object;
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;
                currentUserFactory = new CurrentUserSetup.UserFactory();
                    var graduationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Returns(graduationDtoAdapter);
                    graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository,addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
                    addresses.Add(new Address() { AddressLines = new List<string>() { "line1" }, City = "my city", State = "VA", PostalCode = "123456", CountryCode = "USA" });
                   
               }

               [TestCleanup]
               public void Cleanup()
               {
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    termRepositoryMock = null;
                    programRepositoryMock = null;
                    studentRepositoryMock = null;
                    graduationApplicationRepositoryMock = null;
               }
               [TestMethod]
               public async Task GetGraduationApplication_ReturnsGraduationApplicationDto()
               {
                    var studentId = "0000011";
                    var programCode = "MATH.BA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    addressRepositoryMock.Setup(x => x.GetPersonAddresses(studentId)).Returns(addresses);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationEntityData));
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(studentId, programCode);
                    Assert.IsTrue(graduationApplicationDto is Dtos.Student.GraduationApplication);
                    Assert.AreEqual(graduationApplicationDto.StudentId, studentId);
                    Assert.AreEqual(graduationApplicationDto.ProgramCode, programCode);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_BothIdsNull_ArgumentNullException()
               {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(null, null);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_BothIdsEmpty_ArgumentNullException()
               {
                   var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(string.Empty, string.Empty);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_ProgramCodeEmpty_ArgumentNullException()
               {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000011", string.Empty);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_ProgramCodeNull_ArgumentNullException()
               {
                   var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000011", null);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_StudentIdNull_ArgumentNullException()
               {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(null, "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_StudentIdEmpty_ArgumentNullException()
               {
                   var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(string.Empty, "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(PermissionsException))]
               public async Task GetGraduationApplication_PermissionsException()
               {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000012", "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(PermissionsException))]
               public async Task GetGraduationApplication_RetrievedDtoNotHaveSameIdsAsProvided_Exception()
               {
                    var studentId = "0000012";
                    var programCode = "ENG.BA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationEntityData));
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000011", "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(KeyNotFoundException))]
               public async Task GetGraduationApplication_KeyNotFoundExceptionFromRepository()
               {
                    var studentId = "0000012";
                    var programCode = "ENG.BA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new KeyNotFoundException());
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000011", "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(Exception))]
               public async Task GetGraduationApplication_ExceptionFromRepository()
               {
                    var studentId = "0000012";
                    var programCode = "ENG.BA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync("0000011", "MATH.BA");
               }
               [TestMethod]
               [ExpectedException(typeof(Exception))]
               public async Task GetGraduationApplication_AdapterThrowsException()
               {
                   var studentId = "0000011";
                   var programCode = "MATH.BA";
                   var id = string.Concat(studentId, "*", programCode);
                   graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                   graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationEntityData));
                   adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Throws(new Exception()); 
                   var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationAsync(studentId, programCode);
               }
              
          }

          [TestClass]
          public class PostGraduationApplication
          {
               private Mock<IAdapterRegistry> adapterRegistryMock;
               private IAdapterRegistry adapterRegistry;
               private ILogger logger;
               private ICurrentUserFactory currentUserFactory;
               private Mock<IRoleRepository> roleRepositoryMock;
               private IRoleRepository roleRepository;
               private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
               private IGraduationApplicationRepository graduationApplicationRepository;
               private Mock<ITermRepository> termRepositoryMock;
               private ITermRepository termRepository;
               private Mock<IAddressRepository> addressRepositoryMock;
               private IAddressRepository addressRepository;
               private Mock<IProgramRepository> programRepositoryMock;
               private IProgramRepository programRepository;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
               private IStudentConfigurationRepository configurationRepository;
               private Mock<IStudentRepository> studentRepositoryMock;
               private IStudentRepository studentRepository;
               private IGraduationApplicationService graduationApplicationService;
               private GraduationApplication graduationApplicationEntityData;
               private List<Address> addresses = new List<Address>();
               private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
               private IConfigurationRepository baseConfigurationRepository;


            [TestInitialize]
               public void Initialize()
               {
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    adapterRegistry = adapterRegistryMock.Object;
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    roleRepository = roleRepositoryMock.Object;
                    termRepositoryMock = new Mock<ITermRepository>();
                    termRepository = termRepositoryMock.Object;
                    addressRepositoryMock = new Mock<IAddressRepository>();
                    addressRepository = addressRepositoryMock.Object;
                    programRepositoryMock = new Mock<IProgramRepository>();
                    programRepository = programRepositoryMock.Object;
                    studentRepositoryMock = new Mock<IStudentRepository>();
                    studentRepository = studentRepositoryMock.Object;
                    configurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                    configurationRepository = configurationRepositoryMock.Object;
                    graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                    graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                    baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                    baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                    logger = new Mock<ILogger>().Object;
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;
                currentUserFactory = new CurrentUserSetup.UserFactory();
                    var graduationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Returns(graduationDtoAdapter);
                    var graduationEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.GraduationApplication, Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.GraduationApplication, Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>()).Returns(graduationEntityAdapter);
                    graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository, addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
                    addresses.Add(new Address() { AddressLines = new List<string>() { "line1" }, City = "my city", State = "VA", PostalCode = "123456", CountryCode = "USA" });


               }

               [TestCleanup]
               public void Cleanup()
               {
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    termRepositoryMock = null;
                    programRepositoryMock = null;
                    studentRepositoryMock = null;
                    graduationApplicationRepositoryMock = null;
               }

               [TestMethod]
               public async Task PostGraduationApplication_ReturnGraduationApplicationDto()
               {

                    //create dto with  proper id's and same as current user id
                    //mock term repo to have term entity returned with same term as provided in graduation dto
                    //mock program repo to have program entity returned with same program code as provided in graduation dto
                    //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                    //also in planning student entity, add same program Id as in graduation dto in program codes collection
                    var studentId = "0000011";
                    var programCode = "MATH.BA";
                    var graduationDto = new Dtos.Student.GraduationApplication();
                    graduationDto.StudentId = studentId;
                    graduationDto.ProgramCode = programCode;
                    graduationDto.GraduationTerm = "2015/FA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    addressRepositoryMock.Setup(x => x.GetPersonAddresses(studentId)).Returns(addresses);
                    graduationApplicationRepositoryMock.Setup(x => x.CreateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Returns(Task.FromResult(graduationApplicationEntityData));
                    BuildValidRepositories();
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(graduationDto);
                    Assert.IsNotNull(graduationApplicationDto);
                    Assert.IsNotNull(graduationApplicationDto.Id);
                    Assert.AreEqual(graduationApplicationDto.Id, id);
                    Assert.AreEqual(graduationApplicationDto.StudentId, graduationDto.StudentId);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task PostGraduationApplication_DtoPassedIsNull_ArgumentNullException()
               {
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(null);
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentException))]
               public async Task PostGraduationApplication_DtoMissingRequiredParams_ArgumentException()
               {
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication());
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentException))]
               public async Task PostGraduationApplication_IncorrectTerm_ArgumentException()
               {
                    var term = new Term("2014/FA", "2014-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                    termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentException))]
               public async Task PostGraduationApplication_IncorrectProgram_ArgumentException()
               {
                    var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("ENG.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                    programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }
               [TestMethod]
               [ExpectedException(typeof(ArgumentException))]
               public async Task PostGraduationApplication_IncorrectStudent_ArgumentException()
               {
                   var student = new Domain.Student.Entities.Student("0000012", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                    studentRepositoryMock.Setup(x => x.Get(It.IsAny<string>())).Returns(student);
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }
               [TestMethod]
               [ExpectedException(typeof(PermissionsException))]
               public async Task PostGraduationApplication_PermissionException()
               {
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000012", "MATH/BA"));
               }
               [TestMethod]
               [ExpectedException(typeof(KeyNotFoundException))]
               public async Task PostGraduationApplication_KeyNotFoundExceptionFromRepository()
               {
                    BuildValidRepositories();
                    graduationApplicationRepositoryMock.Setup(x => x.CreateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new KeyNotFoundException());
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }

               [TestMethod]
               [ExpectedException(typeof(Exception))]
               public async Task PostGraduationApplication_ExceptionFromRepository()
               {
                    BuildValidRepositories();
                    graduationApplicationRepositoryMock.Setup(x => x.CreateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new Exception());
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }

               [TestMethod]
               [ExpectedException(typeof(ExistingResourceException))]
               public async Task PostGraduationApplication_ExceptionFromRepositoryAlreadyExists()
               {
                    BuildValidRepositories();
                    graduationApplicationRepositoryMock.Setup(x => x.CreateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new ExistingResourceException());
                    var graduationApplicationDto = await graduationApplicationService.CreateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
               }
               private void BuildValidRepositories()
               {
                    //mock term repo to have term entity returned with same term as provided in graduation dto
                    var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                    termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                    //mock program repo to have program entity returned with same program code as provided in graduation dto
                    var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                    programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                    //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                    var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                    studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                    addressRepositoryMock.Setup(x => x.GetPersonAddresses("0000011")).Returns(addresses);

               }
          }

          [TestClass]
          public class GetGraduationApplications
          {
               private Mock<IAdapterRegistry> adapterRegistryMock;
               private IAdapterRegistry adapterRegistry;
               private ILogger logger;
               private ICurrentUserFactory currentUserFactory;
               private Mock<IRoleRepository> roleRepositoryMock;
               private IRoleRepository roleRepository;
               private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
               private IGraduationApplicationRepository graduationApplicationRepository;
               private Mock<ITermRepository> termRepositoryMock;
               private ITermRepository termRepository;
               private IAddressRepository addressRepository;
               private Mock<IProgramRepository> programRepositoryMock;
               private IProgramRepository programRepository;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
               private IStudentConfigurationRepository configurationRepository;
               private Mock<IStudentRepository> studentRepositoryMock;
               private IStudentRepository studentRepository;
               private IGraduationApplicationService graduationApplicationService;
               private GraduationApplication graduationApplicationEntityData;
               List<Colleague.Dtos.Student.GraduationApplication> graduationApplicationsDtoList = new List<Colleague.Dtos.Student.GraduationApplication>();
               List<GraduationApplication> graduationApplicationEntityList = new List<GraduationApplication>();
               private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
               private IConfigurationRepository baseConfigurationRepository;

            [TestInitialize]
               public void Initialize()
               {
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    adapterRegistry = adapterRegistryMock.Object;
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    roleRepository = roleRepositoryMock.Object;
                    termRepositoryMock = new Mock<ITermRepository>();
                    termRepository = termRepositoryMock.Object;
                    programRepositoryMock = new Mock<IProgramRepository>();
                    programRepository = programRepositoryMock.Object;
                    studentRepositoryMock = new Mock<IStudentRepository>();
                    studentRepository = studentRepositoryMock.Object;
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                    configurationRepository = configurationRepositoryMock.Object;
                    graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                    graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                    baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                    baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                    logger = new Mock<ILogger>().Object;
                    currentUserFactory = new CurrentUserSetup.UserFactory();
                    var graduationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Returns(graduationDtoAdapter);
                    graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository, addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
               }

               [TestCleanup]
               public void Cleanup()
               {
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    termRepositoryMock = null;
                    programRepositoryMock = null;
                    studentRepositoryMock = null;
                    graduationApplicationRepositoryMock = null;
               }

               [TestMethod]
               public async Task GetGraduationApplications_ReturnsGraduationApplicationDtoList()
               {
                    CreateValidEntities();
                    var studentId = "0000011";
                    graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                    var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                    Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                    Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
               }

               [TestMethod]
               [ExpectedException(typeof(KeyNotFoundException))]
               public async Task GetGraduationApplications_ForNullEntityList_ThrowsKeyNotFoundException()
               {
                    graduationApplicationEntityList = null;
                    var studentId = "0000011";
                    graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                    var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
               }

               [TestMethod]
               public async Task GetGraduationApplications_ReturnsCorrectObjectsInGraduationApplicationDtoList()
               {
                    CreateValidEntities();
                    var studentId = "0000011";
                    graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                    var receivedGraduationApplicationsDtoList =(await graduationApplicationService.GetGraduationApplicationsAsync(studentId)).Dto.ToList();
                    Assert.IsTrue(receivedGraduationApplicationsDtoList is List<Dtos.Student.GraduationApplication>);
                    Assert.AreEqual(receivedGraduationApplicationsDtoList[0].Id, graduationApplicationEntityList[0].Id);
                    Assert.AreEqual(receivedGraduationApplicationsDtoList[1].Id, graduationApplicationEntityList[1].Id);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplicationsAsync_ForNullInput_ReturnsArgumentNullException()
               {
                    var graduationApplicationDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(null);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplicationsAsync_ForEmptyInput_ReturnsArgumentNullException()
               {
                    var graduationApplicationDtoList = await graduationApplicationService.GetGraduationApplicationsAsync("");
               }

               [TestMethod]
               [ExpectedException(typeof(PermissionsException))]
               public async Task GetGraduationApplicationsAsync_ForInvalidId_Returns_PermissionsException()
               {
                    var graduationApplicationDtoList = await graduationApplicationService.GetGraduationApplicationsAsync("1234567");
               }

               public void CreateValidEntities()
               {
                    var studentId = "0000011";
                    var programCode = "MATH.BA";
                    var id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    graduationApplicationEntityData.GraduationTerm = "2015/FA";
                    graduationApplicationEntityList.Add(graduationApplicationEntityData);

                    studentId = "0000011";
                    programCode = "CS.BS";
                    id = string.Concat(studentId, "*", programCode);
                    graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                    graduationApplicationEntityData.GraduationTerm = "2015/FA";
                    graduationApplicationEntityList.Add(graduationApplicationEntityData);



               }
          }

        [TestClass]
        public class GetGraduationApplications_AdvisorUser_PrivacyWrapper:CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
            private IGraduationApplicationRepository graduationApplicationRepository;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private IAddressRepository addressRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IProgramRepository programRepository;
            private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
            private IStudentConfigurationRepository configurationRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private IGraduationApplicationService graduationApplicationService;
            private GraduationApplication graduationApplicationEntityData;
            List<Colleague.Dtos.Student.GraduationApplication> graduationApplicationsDtoList = new List<Colleague.Dtos.Student.GraduationApplication>();
            List<GraduationApplication> graduationApplicationEntityList = new List<GraduationApplication>();
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;
                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                var graduationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Returns(graduationDtoAdapter);
                graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository, addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                termRepositoryMock = null;
                programRepositoryMock = null;
                studentRepositoryMock = null;
                graduationApplicationRepositoryMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            
            public async Task GetGraduationApplications_Advisor_NoPermissions()
            { 
                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
            }

            [TestMethod]
            public async Task GetGraduationApplications_Advisor_ViewAnyAdvisees()
            {
                Domain.Student.Entities.Student student1;
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                CreateValidEntities();
                var studentId = "0000011";
                student1 = new Domain.Student.Entities.Student("0000011", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000011")).ReturnsAsync(student1);
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
            }

            [TestMethod]
            public async Task GetGraduationApplications_Advisor_ViewAssignedAdvisees()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                Domain.Student.Entities.Student student1;
                // Add advisor to student's advisor list
                student1 = new Domain.Student.Entities.Student("0000011", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000011")).ReturnsAsync(student1);

                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]

            public async Task GetGraduationApplications_Advisor_ViewAssignedAdvisees_ButNotAssigned()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
            }

            [TestMethod]
            public async Task GetGraduationApplications_Student_HasPrivacyWrapper_ButAdvisorDoNot()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                Domain.Student.Entities.Student student1;
                // Add advisor to student's advisor list
                student1 = new Domain.Student.Entities.Student("0000011", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>(), "S") { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000011")).ReturnsAsync(student1);

                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
                Assert.IsNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].GraduationTerm);
                Assert.IsNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].GraduationTerm);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].Id);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].Id);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[0].StudentId,"0000011");
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[1].StudentId, "0000011");
            }

            [TestMethod]
            public async Task GetGraduationApplications_Student_HasPrivacyWrapper_Advisor_Staff_record_Have_same_code()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                //staff record for the advisor
                var staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                staff.IsActive = true;
                staff.PrivacyCodes = new List<string>() { "S" };
                staffRepositoryMock.Setup(r => r.Get("0000111")).Returns(staff);

                Domain.Student.Entities.Student student1;
                // Add advisor to student's advisor list
                student1 = new Domain.Student.Entities.Student("0000011", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>(), "S") { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000011")).ReturnsAsync(student1);

                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].GraduationTerm);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].GraduationTerm);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].Id);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].Id);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[0].StudentId, "0000011");
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[1].StudentId, "0000011");
            }

            [TestMethod]
            public async Task GetGraduationApplications_Student_HasPrivacyWrapper_Advisor_Staff_record_Have_different_code()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                //staff record for the advisor
                var staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                staff.IsActive = true;
                staff.PrivacyCodes = new List<string>() { "A" };
                staffRepositoryMock.Setup(r => r.Get("0000111")).Returns(staff);

                Domain.Student.Entities.Student student1;
                // Add advisor to student's advisor list
                student1 = new Domain.Student.Entities.Student("0000011", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>(), "S") { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000011")).ReturnsAsync(student1);

                CreateValidEntities();
                var studentId = "0000011";
                graduationApplicationRepositoryMock.Setup<Task<List<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(graduationApplicationEntityList);
                var receivedGraduationApplicationsDtoList = await graduationApplicationService.GetGraduationApplicationsAsync(studentId);
                Assert.IsTrue(receivedGraduationApplicationsDtoList.Dto is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.Count(), graduationApplicationEntityList.Count());
                Assert.IsNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].GraduationTerm);
                Assert.IsNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].GraduationTerm);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[0].Id);
                Assert.IsNotNull(receivedGraduationApplicationsDtoList.Dto.ToList()[1].Id);
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[0].StudentId, "0000011");
                Assert.AreEqual(receivedGraduationApplicationsDtoList.Dto.ToList()[1].StudentId, "0000011");
            }

            public void CreateValidEntities()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var id = string.Concat(studentId, "*", programCode);
                graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                graduationApplicationEntityData.GraduationTerm = "2015/FA";
                graduationApplicationEntityList.Add(graduationApplicationEntityData);

                studentId = "0000011";
                programCode = "CS.BS";
                id = string.Concat(studentId, "*", programCode);
                graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                graduationApplicationEntityData.GraduationTerm = "2015/FA";
                graduationApplicationEntityList.Add(graduationApplicationEntityData);



            }
        }

        [TestClass]
          public class PutGraduationApplication
          {
              private Mock<IAdapterRegistry> adapterRegistryMock;
              private IAdapterRegistry adapterRegistry;
              private ILogger logger;
              private ICurrentUserFactory currentUserFactory;
              private Mock<IRoleRepository> roleRepositoryMock;
              private IRoleRepository roleRepository;
              private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
              private IGraduationApplicationRepository graduationApplicationRepository;
              private Mock<ITermRepository> termRepositoryMock;
              private ITermRepository termRepository;
              private Mock<IAddressRepository> addressRepositoryMock;
              private IAddressRepository addressRepository;
              private Mock<IProgramRepository> programRepositoryMock;
              private IProgramRepository programRepository;
              private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
              private IStudentConfigurationRepository configurationRepository;
              private Mock<IStudentRepository> studentRepositoryMock;
              private IStudentRepository studentRepository;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private IGraduationApplicationService graduationApplicationService;
              private GraduationApplication graduationApplicationEntityData;
              private List<Address> addresses=new List<Address>();
              private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
              private IConfigurationRepository baseConfigurationRepository;

            [TestInitialize]
              public void Initialize()
              {
                  adapterRegistryMock = new Mock<IAdapterRegistry>();
                  adapterRegistry = adapterRegistryMock.Object;
                  roleRepositoryMock = new Mock<IRoleRepository>();
                  roleRepository = roleRepositoryMock.Object;
                  termRepositoryMock = new Mock<ITermRepository>();
                  termRepository = termRepositoryMock.Object;
                  addressRepositoryMock = new Mock<IAddressRepository>();
                  addressRepository = addressRepositoryMock.Object;
                  programRepositoryMock = new Mock<IProgramRepository>();
                  programRepository = programRepositoryMock.Object;
                  studentRepositoryMock = new Mock<IStudentRepository>();
                  studentRepository = studentRepositoryMock.Object;
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                  configurationRepository = configurationRepositoryMock.Object;
                  graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                  graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                  baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                  baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                  logger = new Mock<ILogger>().Object;
                  currentUserFactory = new CurrentUserSetup.UserFactory();
                  var graduationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>(adapterRegistry, logger);
                  adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplication, Ellucian.Colleague.Dtos.Student.GraduationApplication>()).Returns(graduationDtoAdapter);
                  var graduationEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.GraduationApplication, Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>(adapterRegistry, logger);
                  adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.GraduationApplication, Ellucian.Colleague.Domain.Student.Entities.GraduationApplication>()).Returns(graduationEntityAdapter);
                  graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository, addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
              }

              [TestCleanup]
              public void Cleanup()
              {
                  adapterRegistryMock = null;
                  roleRepositoryMock = null;
                  termRepositoryMock = null;
                  programRepositoryMock = null;
                  studentRepositoryMock = null;
                  graduationApplicationRepositoryMock = null;
              }

              [TestMethod]
              public async Task PutGraduationApplication_ReturnGraduationApplicationDto()
              {

                  //create dto with  proper id's and same as current user id
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  //also in planning student entity, add same program Id as in graduation dto in program codes collection
                  var studentId = "0000011";
                  var programCode = "MATH.BA";
                  var graduationDto = new Dtos.Student.GraduationApplication();
                  graduationDto.StudentId = studentId;
                  graduationDto.ProgramCode = programCode;
                  graduationDto.GraduationTerm = "2015/FA";
                  var id = string.Concat(studentId, "*", programCode);
                  graduationApplicationEntityData = new GraduationApplication(id, studentId, programCode);
                  graduationApplicationRepositoryMock.Setup(x => x.UpdateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Returns(Task.FromResult(graduationApplicationEntityData));
                  BuildValidRepositories();
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(graduationDto);
                  Assert.IsNotNull(graduationApplicationDto);
                  Assert.IsNotNull(graduationApplicationDto.Id);
                  Assert.AreEqual(graduationApplicationDto.Id, id);
                  Assert.AreEqual(graduationApplicationDto.StudentId, graduationDto.StudentId);
              }

              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task PutGraduationApplication_DtoPassedIsNull_ArgumentNullException()
              {
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(null);
              }
              [TestMethod]
              [ExpectedException(typeof(ArgumentException))]
              public async Task PutGraduationApplication_DtoMissingRequiredParams_ArgumentException()
              {
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication());
              }
              [TestMethod]
              [ExpectedException(typeof(ArgumentException))]
              public async Task PutGraduationApplication_IncorrectProgram_ArgumentException()
              {
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("ENG.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }
              [TestMethod]
              [ExpectedException(typeof(ArgumentException))]
              public async Task PutGraduationApplication_IncorrectStudent_ArgumentException()
              {
                  var student = new Domain.Student.Entities.Student("0000012", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.Get(It.IsAny<string>())).Returns(student);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }
              [TestMethod]
              [ExpectedException(typeof(PermissionsException))]
              public async Task PutGraduationApplication_PermissionException()
              {
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000012", "MATH/BA"));
              }
              [TestMethod]
              [ExpectedException(typeof(KeyNotFoundException))]
              public async Task PutGraduationApplication_KeyNotFoundExceptionFromRepository()
              {
                  BuildValidRepositories();
                  graduationApplicationRepositoryMock.Setup(x => x.UpdateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new KeyNotFoundException());
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }

              [TestMethod]
              [ExpectedException(typeof(Exception))]
              public async Task PutGraduationApplication_ExceptionFromRepository()
              {
                  BuildValidRepositories();
                  graduationApplicationRepositoryMock.Setup(x => x.UpdateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new Exception());
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }

              [TestMethod]
              [ExpectedException(typeof(ExistingResourceException))]
              public async Task PutGraduationApplication_ExceptionFromRepositoryAlreadyExists()
              {
                  BuildValidRepositories();
                  graduationApplicationRepositoryMock.Setup(x => x.UpdateGraduationApplicationAsync(It.IsAny<GraduationApplication>())).Throws(new ExistingResourceException());
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }

              //grad repo
              [TestMethod]
              [ExpectedException(typeof(KeyNotFoundException))]
              public async Task UpdateGraduationApplication_ApplicationDoesNotExist()
              {
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                  termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                  var graduationEntity = graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));

              }

              //graduation term not retrived after reading
              [TestMethod]
              [ExpectedException(typeof(Exception))]
              public async Task UpdateGraduationApplication_GraduationTermRetrievedIsNull()
              {
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                  termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                  var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                  graduationEntity.Id = "0000011*MATH.BA";
                  graduationEntity.GraduationTerm = null;
                  configurationRepositoryMock.Setup(conf => conf.GetGraduationConfigurationAsync()).ReturnsAsync(GetGraduationConfigurationWithTerms());
                  graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(graduationEntity);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));

              }

              //graduation term not exist in list
              [TestMethod]
              [ExpectedException(typeof(Exception))]
              public async Task UpdateGraduationApplication_GraduationTermRetrievedIsClosed()
              {
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                  termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                  var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                  graduationEntity.Id = "0000011*MATH.BA";
                  graduationEntity.GraduationTerm = "2016/FA";
                  configurationRepositoryMock.Setup(conf => conf.GetGraduationConfigurationAsync()).ReturnsAsync(GetGraduationConfigurationWithTerms());
                  graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(graduationEntity);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }

              //terms collections is empty
              [TestMethod]
              [ExpectedException(typeof(Exception))]
              public async Task UpdateGraduationApplication_AllTermsClosed()
              {
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                  termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                  var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                  graduationEntity.Id = "0000011*MATH.BA";
                  graduationEntity.GraduationTerm = "2015/FA";
                  configurationRepositoryMock.Setup(conf => conf.GetGraduationConfigurationAsync()).ReturnsAsync(GetGraduationConfigurationWithNoTerms());
                  graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(graduationEntity);
                  var graduationApplicationDto = await graduationApplicationService.UpdateGraduationApplicationAsync(new Dtos.Student.GraduationApplication("0000011", "MATH.BA"));
              }

              private void BuildValidRepositories()
              {
                  //mock term repo to have term entity returned with same term as provided in graduation dto
                  var term = new Term("2015/FA", "2015-fall term", DateTime.Now, DateTime.Now, 2015, 1, false, false, "fA", false);
                  termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(term);
                  //mock program repo to have program entity returned with same program code as provided in graduation dto
                  var program = new Ellucian.Colleague.Domain.Student.Entities.Requirements.Program("MATH.BA", "math", new List<string>() { "math" }, true, "2", new CreditFilter(), true, null);
                  programRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(program);
                  //mock student repo to have planning student returned with same  studentid as provided in graduation dto
                  var student = new Domain.Student.Entities.Student("0000011", "bhaumik", null, new List<string>() { "MATH.BA" }, null, null);
                  studentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                  var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                  graduationEntity.Id = "0000011*MATH.BA";
                  graduationEntity.GraduationTerm = "2015/FA";
                  graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(graduationEntity);
                  configurationRepositoryMock.Setup(conf => conf.GetGraduationConfigurationAsync()).ReturnsAsync(GetGraduationConfigurationWithTerms());
                  addressRepositoryMock.Setup(x => x.GetPersonAddresses("0000011")).Returns(addresses);


              }
              private GraduationConfiguration GetGraduationConfigurationWithTerms()
              {
                  GraduationConfiguration configuration = new GraduationConfiguration();
                  configuration.AddGraduationTerm("2015/FA");
                  configuration.AddGraduationTerm("2016/SP");
                  return configuration;
              }

              private GraduationConfiguration GetGraduationConfigurationWithNoTerms()
              {
                  GraduationConfiguration configuration = new GraduationConfiguration();
                  return configuration;
              }
          }

            [TestClass]
            public class GetGraduationApplicationFeeAsyncTests
            {
                private Mock<IAdapterRegistry> adapterRegistryMock;
                private IAdapterRegistry adapterRegistry;
                private ILogger logger;
                private ICurrentUserFactory currentUserFactory;
                private Mock<IRoleRepository> roleRepositoryMock;
                private IRoleRepository roleRepository;
                private Mock<IGraduationApplicationRepository> graduationApplicationRepositoryMock;
                private IGraduationApplicationRepository graduationApplicationRepository;
                private Mock<ITermRepository> termRepositoryMock;
                private ITermRepository termRepository; 
                private IAddressRepository addressRepository;
                private Mock<IProgramRepository> programRepositoryMock;
                private IProgramRepository programRepository;
                private Mock<IStudentConfigurationRepository> configurationRepositoryMock;
                private IStudentConfigurationRepository configurationRepository;
                private Mock<IStudentRepository> studentRepositoryMock;
                private IStudentRepository studentRepository;
                private Mock<IStaffRepository> staffRepositoryMock;
                private IStaffRepository staffRepository;
                private IGraduationApplicationService graduationApplicationService;
                private GraduationApplicationFee graduationApplicationFeeEntityData;
                private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
                private IConfigurationRepository baseConfigurationRepository;

                [TestInitialize]
                public void Initialize()
                {
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    adapterRegistry = adapterRegistryMock.Object;
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    roleRepository = roleRepositoryMock.Object;
                    termRepositoryMock = new Mock<ITermRepository>();
                    termRepository = termRepositoryMock.Object;
                    programRepositoryMock = new Mock<IProgramRepository>();
                    programRepository = programRepositoryMock.Object;
                    studentRepositoryMock = new Mock<IStudentRepository>();
                    studentRepository = studentRepositoryMock.Object;
                    staffRepositoryMock = new Mock<IStaffRepository>();
                    staffRepository = staffRepositoryMock.Object;
                    configurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                    configurationRepository = configurationRepositoryMock.Object;
                    graduationApplicationRepositoryMock = new Mock<IGraduationApplicationRepository>();
                    graduationApplicationRepository = graduationApplicationRepositoryMock.Object;
                    baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                    baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                    logger = new Mock<ILogger>().Object;
                    currentUserFactory = new CurrentUserSetup.UserFactory();
                    var graduationFeeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplicationFee, Ellucian.Colleague.Dtos.Student.GraduationApplicationFee>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationApplicationFee, Ellucian.Colleague.Dtos.Student.GraduationApplicationFee>()).Returns(graduationFeeDtoAdapter);
                    graduationApplicationService = new GraduationApplicationService(adapterRegistry, graduationApplicationRepository, termRepository, programRepository, studentRepository, configurationRepository,addressRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository, staffRepository);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    termRepositoryMock = null;
                    programRepositoryMock = null;
                    studentRepositoryMock = null;
                    graduationApplicationRepositoryMock = null;
                }

                [TestMethod]
                public async Task GetGraduationApplicationFeeAsync_GetsGraduationApplicationFeeDto()
                {
                    var studentId = currentUserFactory.CurrentUser.PersonId;
                    var programCode = "MATH.BA";
                    var amount = 50m;
                    var distribution = "DIST";
                    graduationApplicationFeeEntityData = new GraduationApplicationFee(studentId, programCode, amount,distribution);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationFeeEntityData));
                    var graduationApplicationFeeDto = await graduationApplicationService.GetGraduationApplicationFeeAsync(studentId, programCode);
                    Assert.IsTrue(graduationApplicationFeeDto is Dtos.Student.GraduationApplicationFee);
                    Assert.AreEqual(studentId, graduationApplicationFeeDto.StudentId);
                    Assert.AreEqual(programCode, graduationApplicationFeeDto.ProgramCode);
                    Assert.AreEqual(amount, graduationApplicationFeeDto.Amount);
                    Assert.AreEqual(distribution, graduationApplicationFeeDto.PaymentDistributionCode);
                }

                [TestMethod]
                [ExpectedException(typeof(PermissionsException))]
                public async Task GetGraduationApplicationFeeAsync_GetsGraduationApplicationFeeDto_throws_PermissionsException_if_accessing_other_user_data()
                {
                    var studentId = currentUserFactory.CurrentUser.PersonId+"1";
                    var programCode = "MATH.BA";
                    var amount = 50m;
                    var distribution = "DIST";
                    graduationApplicationFeeEntityData = new GraduationApplicationFee(studentId, programCode, amount, distribution);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationFeeEntityData));
                    var graduationApplicationFeeDto = await graduationApplicationService.GetGraduationApplicationFeeAsync(studentId, programCode);
                    Assert.IsTrue(graduationApplicationFeeDto is Dtos.Student.GraduationApplicationFee);
                    Assert.AreEqual(studentId, graduationApplicationFeeDto.StudentId);
                    Assert.AreEqual(programCode, graduationApplicationFeeDto.ProgramCode);
                    Assert.AreEqual(amount, graduationApplicationFeeDto.Amount);
                    Assert.AreEqual(distribution, graduationApplicationFeeDto.PaymentDistributionCode);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_BothParametersEmpty_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync("", "");
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_BothParametersNull_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync(null, null);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_ProgramCodeEmpty_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync("0000011", "");
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_ProgramCodeNull_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync("0000011", null);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_StudentIdNull_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync(null, "MATH.BA");
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GetGraduationApplicationFeeAsync_StudentIdEmpty_ArgumentNullException()
                {
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync("", "MATH.BA");
                }

                [TestMethod]
                [ExpectedException(typeof(Exception))]
                public async Task GetGraduationApplicationFeeAsync_ExceptionFromRepository()
                {
                    var studentId = "0000011";
                    var programCode = "MATH.BA";
                    var amount = 50m;
                    var distribution = "DIST";
                    GraduationApplicationFee graduationApplicationEntityFeeData = new GraduationApplicationFee(studentId, programCode, amount, distribution);
                    graduationApplicationRepositoryMock.Setup(x => x.GetGraduationApplicationFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
                    var graduationApplicationDto = await graduationApplicationService.GetGraduationApplicationFeeAsync("0000011", "MATH.BA");
                }
            }
        }
    }
