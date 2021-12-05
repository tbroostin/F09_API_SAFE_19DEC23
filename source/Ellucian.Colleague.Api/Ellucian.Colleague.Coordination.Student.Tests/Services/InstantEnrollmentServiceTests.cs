// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
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
    public class InstantEnrollmentServiceTests
    {
        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0000001",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "iePermission" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public IAdapterRegistry adapterRegistry;
        public Mock<IRoleRepository> roleRepositoryMock;
        public IRoleRepository roleRepository;
        public Mock<ILogger> loggerMock;
        public ILogger logger;
        public ICurrentUserFactory currentUserFactory;
        private Mock<IInstantEnrollmentRepository> instantEnrollmentRepositoryMock;
        private IInstantEnrollmentRepository instantEnrollmentRepository;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<IStudentProgramRepository> studentProgramRepoMock;
        private IStudentProgramRepository studentProgramRepo;

        private InstantEnrollmentService service;
        private string personId = "0000001";
        private string username = "aaubergine";
        private Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic personDemographic;
        private Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration instantEnrollmentProposedRegistration;
        private Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration zeroCostRegistration;
        private Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration echeckReg;

        [TestInitialize]
        public void Initialize()
        {
            //input and output data
            personDemographic = new Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic();
            personDemographic.FirstName = "firstname";
            personDemographic.LastName = "lastname";
            personDemographic.EmailAddress = "joe@email.com";

            instantEnrollmentProposedRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration();
            instantEnrollmentProposedRegistration.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>() {
                new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){ SectionId="s001", AcademicCredits=300 },
                new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){ SectionId="s002", AcademicCredits=null }
            };
            instantEnrollmentProposedRegistration.PersonId = "";
            instantEnrollmentProposedRegistration.AcademicProgram = "math.ba";
            instantEnrollmentProposedRegistration.Catalog = "2012";
            instantEnrollmentProposedRegistration.PersonDemographic = personDemographic;


            //initialize result from repository
            List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection> proposedSections = new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
            proposedSections.Add(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("s001", 300));
            proposedSections.Add(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("s002", null));


            List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage> proposedMessages = new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>();
            proposedMessages.Add(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage("s003", "messge 1"));
            proposedMessages.Add(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage(null, "messge 2"));


            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistrationResult result = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistrationResult(true, proposedSections, proposedMessages);

            //mock
            loggerMock = new Mock<ILogger>();
            loggerMock.Setup(lgr => lgr.IsDebugEnabled).Returns(true);
            logger = loggerMock.Object;

            currentUserFactory = new StudentUserFactory();
            //adaptors
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            var phoneDtoAdapter = new Coordination.Base.Adapters.PhoneDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.Phone, Ellucian.Colleague.Domain.Base.Entities.Phone>()).Returns(phoneDtoAdapter);
            var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>(); // An empty mock adapter registry to instantiate AutoMapperAdapter

            var sectionsToregisterAdapter = new InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter(emptyAdapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()).Returns(sectionsToregisterAdapter);

            var personDemographicAdapter = new InstantEnrollmentPersonDemographicsDtoToEntityAdapter(emptyAdapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>()).Returns(personDemographicAdapter);

            var instantEnrollmentDtoToEntityAdapter = new InstantEnrollmentProposedRegistrationDtoToEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration>()).Returns(instantEnrollmentDtoToEntityAdapter);

            var instantEnrollmentEntityToDtoAdapter = new InstantEnrollmentProposedRegistrationResultEntityToDtoAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistrationResult>()).Returns(instantEnrollmentEntityToDtoAdapter);

            var pmtGatewayDtoToEntityAdapter = new InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>()).Returns(pmtGatewayDtoToEntityAdapter);

            var pmtGatewayEntityToDtoAdapter = new InstantEnrollmentStartPaymentGatewayRegistrationResultEntityToDtoAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult,
                Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult>()).Returns(pmtGatewayEntityToDtoAdapter);

            var instantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter = new InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>()).Returns(instantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter);

            var studentProgramDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentProgram, Dtos.Student.StudentProgram2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>()).Returns(studentProgramDtoAdapter);

            //repos
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepository = roleRepositoryMock.Object;
            var iePermission = new Domain.Entities.Permission(StudentPermissionCodes.InstantEnrollmentAllowAll);
            var ieRole = new Role(1, currentUserFactory.CurrentUser.Roles.FirstOrDefault());
            ieRole.AddPermission(iePermission);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { ieRole });

            instantEnrollmentRepositoryMock = new Mock<IInstantEnrollmentRepository>();
            instantEnrollmentRepositoryMock.Setup(repo => repo.GetProposedRegistrationResultAync(It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration>())).ReturnsAsync(result);
            instantEnrollmentRepository = instantEnrollmentRepositoryMock.Object;

            // Echeck Registration
            echeckReg = new Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration()
            {
                AcademicProgram = "Program",
                BankAccountCheckNumber = "100",
                BankAccountNumber = "AcctNumber",
                BankAccountOwner = "AcctOwner",
                BankAccountRoutingNumber = "AcctRoute",
                BankAccountType = "Check",
                Catalog = "Catalog",
                ConvenienceFeeAmount = 100,
                ConvenienceFeeDesc = "Fee Description",
                ConvenienceFeeGlAccount = "FeeGlAccount",
                EducationalGoal = "Goal",
                GovernmentId = "License",
                GovernmentIdState = "VA",
                PayerAddress = "123 Main St.",
                PayerCity = "Fairfax",
                PayerEmailAddress = "Payer@email.com",
                PayerPostalCode = "22033",
                PayerState = "VA",
                PaymentAmount = 10000,
                PaymentMethod = "PayMethod",
                PersonDemographic = new Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic()
                {
                    FirstName = "Joe",
                    LastName = "Echeck",
                    City = "Fairfax",
                    State = "VA",
                    ZipCode = "22033",
                    EmailAddress = "Student@email.com",
                },
                PersonId = personId,
                ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()
                {
                    new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){AcademicCredits = 300, SectionId = "SECT1",},
                    new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){AcademicCredits = 400, SectionId = "SECT2",},
                },
                ProviderAccount = "PoviderAccount",
            };

            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult echeckResult;
            echeckResult = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult(
                false,
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>()
                {
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("SECT1", 10000),
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("SECT2", 20000),
                },
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>()
                {
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage("SECT1", "SECT1 Message"),
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage("SECT2", "SECT2 Message"),
                },
                "0000001",
                "RECEIPT",
                "aaubergine"
                );
            var echeckDtoToEntityAdapter = new InstantEnrollmentEcheckRegistrationDtoToEntityAdapter(adapterRegistry, loggerMock.Object);
            var echeckEntityToDtoAdapter = new InstantEnrollmentEcheckRegistrationResultEntityToDtoAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration>()).Returns(echeckDtoToEntityAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult>()).Returns(echeckEntityToDtoAdapter);
            instantEnrollmentRepositoryMock.Setup(repo => repo.GetEcheckRegistrationResultAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration>())).ReturnsAsync(echeckResult);


            //zero cost registration
            zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration()
            {
                AcademicProgram = "POLI.BA",
                Catalog = "2018",
                EducationalGoal = "Goal",
                PersonDemographic = new Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic()
                {
                    FirstName = "Joe",
                    LastName = "ZeroCost",
                    City = "Fairfax",
                    State = "VA",
                    ZipCode = "22033",
                    EmailAddress = "Student@email.com",
                },
                PersonId = personId,
                ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()
                {
                    new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){AcademicCredits = 300, SectionId = "SECT1",},
                    new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(){AcademicCredits = null, SectionId = "SECT2",},
                },
            };

            //attept to register two proposed sections resulting in one registered, one returned multiple errors
            var zeroCostRegistrationResult = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult(
                true,
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>()
                {
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("SECT1", 10000),
                },
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>()
                {
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage("SECT2", "SECT1 Error Message Number 1"),
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage("SECT2", "SECT2 Error Message Number 2"),
                },
                personId,
                username
                );

            var zeroCostDtoToEntityAdapter = new InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter(adapterRegistry, loggerMock.Object);
            var zeroCostEntityToDtoAdapter = new InstantEnrollmentZeroCostRegistrationResultEntityToDtoAdapter(adapterRegistry, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration,
                Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>()).Returns(zeroCostDtoToEntityAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult,
                Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult>()).Returns(zeroCostEntityToDtoAdapter);

            instantEnrollmentRepositoryMock.Setup(repo => repo.GetZeroCostRegistrationResultAsync(
                It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).ReturnsAsync(zeroCostRegistrationResult);

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            var ieSections = new List<Section>();
            foreach (var sec in echeckReg.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }
            foreach (var sec in zeroCostRegistration.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }
            foreach (var sec in instantEnrollmentProposedRegistration.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).ReturnsAsync(ieSections);

            studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            studentProgramRepo = studentProgramRepoMock.Object;

            service = new InstantEnrollmentService(adapterRegistry, instantEnrollmentRepository, sectionRepo, studentProgramRepo, currentUserFactory, roleRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
            studentProgramRepoMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstantEnrollmentService_ProposedRegistrationForClassesAsync_Null_parameter()
        {
            var dtos = await service.ProposedRegistrationForClassesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task InstantEnrollmentService_ProposedRegistrationForClassesAsync_Sections_AreNull()
        {
            instantEnrollmentProposedRegistration.ProposedSections = null;
            var dtos = await service.ProposedRegistrationForClassesAsync(instantEnrollmentProposedRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstantEnrollmentService_proposedRegistrationForClassesAsync_Null_Section_Id()
        {
            // Add a proposed section with a null section ID
            instantEnrollmentProposedRegistration.ProposedSections.Add(new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister() { SectionId = null, AcademicCredits = 300 });
            var ieSections = new List<Section>();
            foreach (var sec in instantEnrollmentProposedRegistration.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).ReturnsAsync(ieSections);

            var dtos = await service.ProposedRegistrationForClassesAsync(instantEnrollmentProposedRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task InstantEnrollmentService_ProposedRegistrationForClassesAsync_Sections_Invalid()
        {
            instantEnrollmentProposedRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "NOT_CE_SECTION",
                    AcademicCredits = 3m,
                    MarketingSource = "MKTG",
                    RegistrationReason = "RSN"
                }
            };
            var dtos = await service.ProposedRegistrationForClassesAsync(instantEnrollmentProposedRegistration);
        }

        [TestMethod]
        public async Task InstantEnrollmentService_proposedRegistrationForClassesAsync_Success()
        {
            var dtos = await service.ProposedRegistrationForClassesAsync(instantEnrollmentProposedRegistration);
            Assert.IsNotNull(dtos);
            Assert.IsTrue(dtos.ErrorOccurred);
            Assert.AreEqual(2, dtos.RegisteredSections.Count());
            Assert.AreEqual(2, dtos.RegistrationMessages.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Null_parameter()
        {
            await service.ZeroCostRegistrationForClassesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Sections_AreNull()
        {
            zeroCostRegistration.ProposedSections = null;
            await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Sections_AreEmpty()
        {
            zeroCostRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
            await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Null_Section_Id()
        {
            // Add a proposed section with a null section ID
            zeroCostRegistration.ProposedSections.Add(new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister() { SectionId = null, AcademicCredits = 300 });
            var ieSections = new List<Section>();
            foreach (var sec in zeroCostRegistration.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }

            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).ReturnsAsync(ieSections);
            var dto = await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Sections_Invalid()
        {
            zeroCostRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "NOT_CE_SECTION",
                    AcademicCredits = 3m,
                    MarketingSource = "MKTG",
                    RegistrationReason = "RSN"
                }
            };
            var dto = await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
        }

        [TestMethod]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Errors()
        {
            //One section registered, one section failed
            var registrationResult = await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
            Assert.IsNotNull(registrationResult);
            Assert.AreEqual(personId, registrationResult.PersonId);
            Assert.AreEqual(username, registrationResult.UserName);
            Assert.IsTrue(registrationResult.ErrorOccurred);
            Assert.AreEqual(1, registrationResult.RegisteredSections.Count());
            Assert.AreEqual(2, registrationResult.RegistrationMessages.Count());
        }

        [TestMethod]
        public async Task InstantEnrollmentService_ZeroCostRegistrationForClassesAsync_Success()
        {

            var zeroCostRegistrationResult = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult(
                false,
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>()
                {
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("SECT1", 10000),
                    new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection("SECT2", 20000),
                },
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>(),
                personId,
                username
                );

            instantEnrollmentRepositoryMock.Setup(repo => repo.GetZeroCostRegistrationResultAsync(
                It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).ReturnsAsync(zeroCostRegistrationResult);

            //Two sections registered, with no errors or messages
            var registrationResult = await service.ZeroCostRegistrationForClassesAsync(zeroCostRegistration);
            Assert.IsNotNull(registrationResult);
            Assert.IsFalse(registrationResult.ErrorOccurred);
            Assert.AreEqual(personId, registrationResult.PersonId);
            Assert.AreEqual(username, registrationResult.UserName);
            Assert.AreEqual(2, registrationResult.RegisteredSections.Count());
            Assert.AreEqual(0, registrationResult.RegistrationMessages.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstantEnrollmentService_EcheckRegistrationForClassesAsync_NullParameter()
        {
            var dtos = await service.EcheckRegistrationForClassesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task InstantEnrollmentService_EcheckRegistrationForClassesAsync_Sections_Invalid()
        {
            echeckReg.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "NOT_CE_SECTION",
                    AcademicCredits = 3m,
                    MarketingSource = "MKTG",
                    RegistrationReason = "RSN"
                }
            };
            var dto = await service.EcheckRegistrationForClassesAsync(echeckReg);
        }

        [TestMethod]
        public async Task InstantEnrollmentService_EcheckRegistrationForClassesAsync_Valid()
        {
            var dto = await service.EcheckRegistrationForClassesAsync(echeckReg);
            Assert.IsNotNull(dto);
            Assert.IsFalse(dto.ErrorOccurred);
            Assert.AreEqual("SECT1", dto.RegisteredSections[0].SectionId);
            Assert.AreEqual(10000, dto.RegisteredSections[0].SectionCost);
            Assert.AreEqual("SECT2", dto.RegisteredSections[1].SectionId);
            Assert.AreEqual(20000, dto.RegisteredSections[1].SectionCost);
            Assert.AreEqual("SECT1", dto.RegistrationMessages[0].MessageSection);
            Assert.AreEqual("SECT1 Message", dto.RegistrationMessages[0].Message);
            Assert.AreEqual("SECT2", dto.RegistrationMessages[1].MessageSection);
            Assert.AreEqual("SECT2 Message", dto.RegistrationMessages[1].Message);
            Assert.AreEqual("0000001", dto.PersonId);
            Assert.AreEqual("RECEIPT", dto.CashReceipt);
            Assert.AreEqual("aaubergine", dto.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StartInstantEnrollmentPaymentGatewayTransaction_Null_Criteria()
        {
            var dto = await service.StartInstantEnrollmentPaymentGatewayTransaction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task StartInstantEnrollmentPaymentGatewayTransaction_Null_ProposedSections()
        {
            Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration criteria = new InstantEnrollmentPaymentGatewayRegistration();
            criteria.ProposedSections = null;
            var dto = await service.StartInstantEnrollmentPaymentGatewayTransaction(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task InstantEnrollmentService_StartInstantEnrollmentPaymentGatewayTransaction_Sections_Invalid()
        {
            Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration criteria = new InstantEnrollmentPaymentGatewayRegistration();
            criteria.PersonId = currentUserFactory.CurrentUser.PersonId;
            criteria.ReturnUrl = "ReturnUrl";
            criteria.PersonDemographic = null;
            criteria.PaymentMethod = "CC";
            criteria.PaymentAmount = 100;
            criteria.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "NOT_CE_SECTION",
                    AcademicCredits = 3m,
                    MarketingSource = "MKTG",
                    RegistrationReason = "RSN"
                }
            };
            var dto = await service.StartInstantEnrollmentPaymentGatewayTransaction(criteria);
        }

        [TestMethod]
        public async Task StartInstantEnrollmentPaymentGatewayTransaction_Success()
        {

            // Mock a succesful repository call that returns a url and no messages.
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult resultEntity;
            resultEntity = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult(null, "RedirectUrl");
            instantEnrollmentRepositoryMock.Setup(repo => repo.StartInstantEnrollmentPaymentGatewayTransactionAsync(
                    It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>())).ReturnsAsync(resultEntity);

            Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration criteria = new InstantEnrollmentPaymentGatewayRegistration();
            criteria.PersonId = currentUserFactory.CurrentUser.PersonId;
            criteria.ReturnUrl = "ReturnUrl";
            criteria.PersonDemographic = null;
            criteria.PaymentMethod = "CC";
            criteria.PaymentAmount = 100;
            criteria.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>() { new InstantEnrollmentRegistrationBaseSectionToRegister() { SectionId = "Sect1" } };

            var ieSections = new List<Section>();
            foreach (var sec in criteria.ProposedSections)
            {
                ieSections.Add(new Section(sec.SectionId, "1", "01", DateTime.Today.AddDays(30), 3m, null, sec.SectionId, "CE", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false));
            }
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).ReturnsAsync(ieSections);

            var dto = await service.StartInstantEnrollmentPaymentGatewayTransaction(criteria);

            Assert.AreEqual(dto.PaymentProviderRedirectUrl, "RedirectUrl");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_null_request()
        {
            var text = await service.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_user_does_not_have_permission()
        {
            var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
            {
                PersonId = "0001234",
                CashReceiptId = "0001234"
            };
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>());

            var text = await service.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_repo_throws_exception()
        {
            var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                CashReceiptId = "0001234"
            };

            instantEnrollmentRepositoryMock.Setup(repo => repo.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                ThrowsAsync(new ArgumentException());
            var text = await service.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_repo_returns_null()
        {
            var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                CashReceiptId = "0001234"
            };

            instantEnrollmentRepositoryMock.Setup(repo => repo.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                ReturnsAsync(() => null);
            var text = await service.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
        }

        [TestMethod]
        public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_repo_returns_text()
        {
            var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                CashReceiptId = "0001234"
            };
            var repoText = new List<string>()
            {
                "Line 1",
                "Line 2"
            };
            instantEnrollmentRepositoryMock.Setup(repo => repo.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                ReturnsAsync(repoText);
            var text = await service.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            CollectionAssert.AreEqual(repoText, text.ToList());
        }

        [TestClass]
        public class QueryPersonMatchResultsInstantEnrollmentByPostAsync : InstantEnrollmentServiceTests
        {
            private Dtos.Student.InstantEnrollment.PersonMatchCriteriaInstantEnrollment criteria;

            [TestInitialize]
            public void Initialize_QueryPersonMatchResultsInstantEnrollmentByPostAsync()
            {
                base.Initialize();

                logger = new Mock<ILogger>().Object;

                criteria = new Dtos.Student.InstantEnrollment.PersonMatchCriteriaInstantEnrollment()
                {
                    LastName = "Enrollment",
                    FirstName = "Instant"
                };

                //var criteriaDtoAdapter = new Ellucian.Colleague.Coordination.Base.Adapters.PersonMatchCriteriaInstantEnrollmentDtoAdapter(adapterRegistry, logger);
                var criteriaDtoAdapter = new AutoMapperAdapter<PersonMatchCriteriaInstantEnrollment, Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PersonMatchCriteriaInstantEnrollment, Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>()).Returns(criteriaDtoAdapter);

                var criteriaAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>()).Returns(criteriaAdapter);

                var resultAdapter = new AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult, Dtos.Student.InstantEnrollment.InstantEnrollmentPersonMatchResult>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult, Dtos.Student.InstantEnrollment.InstantEnrollmentPersonMatchResult>()).Returns(resultAdapter);




                service = new InstantEnrollmentService(adapterRegistry, instantEnrollmentRepository, sectionRepo, studentProgramRepo, currentUserFactory, roleRepository, logger);
            }

            [TestCleanup]
            public void Cleanup_QueryPersonMatchResultsInstantEnrollmentByPostAsync()
            {
                adapterRegistry = null;
                service = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_NullCriteria()
            {
                var results = await service.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null);
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_NullResults()
            {
                instantEnrollmentRepositoryMock.Setup(repo => repo.GetMatchingPersonResultsInstantEnrollmentAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>())).ReturnsAsync(() => null);
                service = new InstantEnrollmentService(adapterRegistry, instantEnrollmentRepository, sectionRepo, studentProgramRepo, currentUserFactory, roleRepository, logger);

                var results = await service.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteria);
                Assert.AreEqual(null, results.PersonId);
                Assert.AreEqual(false, results.HasPotentialMatches);
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_NoResults()
            {
                instantEnrollmentRepositoryMock.Setup(repo => repo.GetMatchingPersonResultsInstantEnrollmentAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>())).ReturnsAsync(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult(null, true));
                service = new InstantEnrollmentService(adapterRegistry, instantEnrollmentRepository, sectionRepo, studentProgramRepo, currentUserFactory, roleRepository, logger);

                var results = await service.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteria);
                Assert.AreEqual(null, results.PersonId);
                Assert.AreEqual(false, results.HasPotentialMatches);
                Assert.AreEqual(true, results.DuplicateGovernmentIdFound);
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_Valid_Definite_Matches()
            {
                instantEnrollmentRepositoryMock.Setup(repo => repo.GetMatchingPersonResultsInstantEnrollmentAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>())).ReturnsAsync(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult(new List<Domain.Base.Entities.PersonMatchResult>()
                {
                    new PersonMatchResult("0001234", 100, "D"),
                }, false));

                var results = await service.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteria);
                Assert.AreEqual("0001234", results.PersonId);
                Assert.AreEqual(false, results.HasPotentialMatches);
                Assert.AreEqual(false, results.DuplicateGovernmentIdFound);
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_Valid_Potential_Matches()
            {
                instantEnrollmentRepositoryMock.Setup(repo => repo.GetMatchingPersonResultsInstantEnrollmentAsync(It.IsAny<Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>())).ReturnsAsync(new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult(new List<Domain.Base.Entities.PersonMatchResult>()
                {
                    new PersonMatchResult("0001234", 50, "P"),
                    new PersonMatchResult("0001235", 60, "P")
                }, false));

                var results = await service.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteria);
                Assert.AreEqual(null, results.PersonId);
                Assert.AreEqual(true, results.HasPotentialMatches);
                Assert.AreEqual(false, results.DuplicateGovernmentIdFound);
            }
        }


        [TestClass]
        public class GetInstantEnrollmentStudentPrograms2Async : InstantEnrollmentServiceTests
        {
            private List<StudentProgram> studentProgEntities;

            [TestInitialize]
            public void Initialize_GetInstantEnrollmentStudentPrograms2Async()
            {
                base.Initialize();

                studentProgEntities = new List<StudentProgram>()
                {
                    new StudentProgram(personId, "BS-COMP", "2000"),  //no start date or end date  - not active program
                    new StudentProgram(personId, "BS-MATH", "2001") { StartDate = DateTime.Today.AddDays(-10) }, //start date in past, no end date - active program
                    new StudentProgram(personId, "BA-ACCT", "2002") { StartDate = DateTime.Today }, //start date today, no end date - active program
                    new StudentProgram(personId, "BA-BUSN", "2003") { StartDate = DateTime.Today.AddYears(1) }, //start date future date, no end date - not active program

                    new StudentProgram(personId, "BA-ECON", "2004") { StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(-10) }, //start date in past, end date past - not active program
                    new StudentProgram(personId, "BA-HIST", "2005") { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(-10) }, //start date today, end date past - not active program
                    new StudentProgram(personId, "BA-POLI", "2006") { StartDate = DateTime.Today.AddYears(1), EndDate = DateTime.Today.AddDays(-10) }, //start date future date, end date past - not active program
                    
                    new StudentProgram(personId, "BA-FREN", "2007") { StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today }, //start date in past, end date today - active program
                    new StudentProgram(personId, "BA-SPAN", "2008") { StartDate = DateTime.Today, EndDate = DateTime.Today }, //start date today,  end date today - active program
                    new StudentProgram(personId, "BA-ITAL", "2009") { StartDate = DateTime.Today.AddYears(1), EndDate = DateTime.Today }, //start date future date, end date today - not active program
                    
                    new StudentProgram(personId, "BA-ENGL", "2010") { StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddYears(10) }, //start date in past, end date future - active program
                    new StudentProgram(personId, "CE-DFLT", "2011") { StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(10) }, //start date today, end date future - active program
                    new StudentProgram(personId, "AA-BUSN", "2012") { StartDate = DateTime.Today.AddYears(1), EndDate = DateTime.Today.AddYears(10) }, //start date future date, end date future - not active program
                };
                studentProgramRepoMock.Setup(repo => repo.GetAsync(personId)).ReturnsAsync(studentProgEntities);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetInstantEnrollmentStudentPrograms2Async_NullStudentId()
            {
                await service.GetInstantEnrollmentStudentPrograms2Async(null, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetInstantEnrollmentStudentPrograms2Async_EmptyStudentId()
            {
                await service.GetInstantEnrollmentStudentPrograms2Async(string.Empty, false);
            }

            [TestMethod]
            public async Task GetInstantEnrollmentStudentPrograms2Async_BoolDefaultValueIsCurrentOnly()
            {
                var expectedStudentPrograms = studentProgEntities.Where(x => 
                    (x.StartDate != null && x.StartDate <= DateTime.Today) && 
                    (x.EndDate == null || x.EndDate >= DateTime.Today)).ToList();

                var actualStudentPrograms = (await service.GetInstantEnrollmentStudentPrograms2Async(personId)).ToList();

                Assert.AreEqual(expectedStudentPrograms.Count, actualStudentPrograms.Count);
                for(int i = 0; i < expectedStudentPrograms.Count; i++)
                {
                    Assert.AreEqual(expectedStudentPrograms[i].ProgramCode, actualStudentPrograms[i].ProgramCode);
                }
            }

            [TestMethod]
            public async Task GetInstantEnrollmentStudentPrograms2Async_IsCurrentOnlyIsTrue()
            {
                var expectedStudentPrograms = studentProgEntities.Where(x =>
                    (x.StartDate != null && x.StartDate <= DateTime.Today) &&
                    (x.EndDate == null || x.EndDate >= DateTime.Today)).ToList();

                var actualStudentPrograms = (await service.GetInstantEnrollmentStudentPrograms2Async(personId, true)).ToList();

                Assert.AreEqual(expectedStudentPrograms.Count, actualStudentPrograms.Count);
                for (int i = 0; i < expectedStudentPrograms.Count; i++)
                {
                    Assert.AreEqual(expectedStudentPrograms[i].ProgramCode, actualStudentPrograms[i].ProgramCode);
                }
            }

            [TestMethod]
            public async Task GetInstantEnrollmentStudentPrograms2Async_BoolDefaultValueIsCurrentOnlyIsFalse()
            {
                var actualStudentPrograms = (await service.GetInstantEnrollmentStudentPrograms2Async(personId, false)).ToList();

                Assert.AreEqual(studentProgEntities.Count, actualStudentPrograms.Count);
                for (int i = 0; i < studentProgEntities.Count; i++)
                {
                    Assert.AreEqual(studentProgEntities[i].ProgramCode, actualStudentPrograms[i].ProgramCode);
                }
            }
        }

    }
}
