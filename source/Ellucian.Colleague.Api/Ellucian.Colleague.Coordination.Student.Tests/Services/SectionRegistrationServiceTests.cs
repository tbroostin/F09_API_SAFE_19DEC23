// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class SectionRegistrationServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role updateGradesRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.GRADES");
            protected Ellucian.Colleague.Domain.Entities.Role updateRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.REGISTRATIONS");
            protected Ellucian.Colleague.Domain.Entities.Role viewRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.REGISTRATIONS");
            protected Ellucian.Colleague.Domain.Entities.Role createRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.REGISTRATIONS");
            protected Ellucian.Colleague.Domain.Entities.Role deleteRegistrationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "DELETE.REGISTRATIONS");

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
                            Roles = new List<string>() { "UPDATE.GRADES", "UPDATE.REGISTRATIONS", "VIEW.REGISTRATIONS", "CREATE.REGISTRATIONS", "DELETE.REGISTRATIONS" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_Get : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private SectionRegistrationService sectionRegistrationService;
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            SectionRegistrationResponse response;
            TestSectionRegistrationRepository repo;

            [TestInitialize]
            public void Initialize()
            {

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                repo = new TestSectionRegistrationRepository();
                response = repo.GetSectionRegistrationResponse();
                BuildMocksForSectionRegistrationGet();

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            private void BuildMocksForSectionRegistrationGet()
            {
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 5);
                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);
                sectionRegistrationRepositoryMock.Setup(s => s.GetSectionRegistrationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                                                 .ReturnsAsync(tuple);


                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");

                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");

                //GetPersonGuidsCollectionAsync
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("0012297", "0012297");
                personRepositoryMock.Setup(pr => pr.GetPersonGuidsWithNoCorpCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add("19442", "19442");
                sectionRepositoryMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);

                Dictionary<string, KeyValuePair<string, string>> dictKVP = new Dictionary<string, KeyValuePair<string, string>>();
                dictKVP.Add("SBHOLE", new KeyValuePair<string, string>("SBHOLE", "0012297"));
                personRepositoryMock.Setup(pbr => pbr.GetPersonGuidsFromOperKeysAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dictKVP);

                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                statusItems = repo.GetSectionRegistrationStatusItems();
                var statusItem = statusItems.FirstOrDefault(s => s.Code == "Registered");

                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;

                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();

                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);               

                sectionRegistrationRepositoryMock.Setup(sr => sr.GetGradeGuidFromIdAsync(guid)).ReturnsAsync(It.IsAny<string>());
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(statusItems);
                personBaseRepositoryMock.Setup(p => p.GetPersonGuidFromOpersAsync(It.IsAny<string>())).ReturnsAsync("SBHOLE");
                studentReferenceDataRepositoryMock.Setup(a => a.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                //registrationDto = null;
                sectionRegistrationService = null;
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrationsAsync()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrationsAsync(0, 10, "", "");

                var result = results.Item1.FirstOrDefault();
                //Assert
                Assert.AreEqual(1, result.Approvals.Count());
                Assert.AreEqual(result.AwardGradeScheme.Id, "27178aab-a6e8-4d1e-ae27-eca1f7b33363");
                Assert.AreEqual(response.Guid, result.Id);
                Assert.AreEqual(result.Approvals.Count(), 1);
                Assert.AreEqual(response.InvolvementStartOn, result.Involvement.StartOn);
                Assert.AreEqual(response.InvolvementEndOn, result.Involvement.EndOn);
                Assert.AreEqual(result.Process.GradeExtension, null);
                Assert.AreEqual("0012297", result.Registrant.Id);
                Assert.AreEqual("19442", result.Section.Id);
                Assert.AreEqual("USA", result.SectionRegistrationReporting.CountryCode.ToString());
                Assert.AreEqual(null, result.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn);
                Assert.AreEqual("Attended", result.SectionRegistrationReporting.LastDayOfAttendance.Status.ToString());
                Assert.AreEqual("Registered", result.Status.RegistrationStatus.ToString());
                Assert.AreEqual("Registered", result.Status.SectionRegistrationStatusReason.ToString());
                Assert.AreEqual("27178aab-a6e8-4d1e-ae27-eca1f7b33363", result.Transcript.GradeScheme.Id);
                Assert.AreEqual("Standard", result.Transcript.Mode.ToString());

                //Test grades
                Assert.AreEqual(8, result.SectionRegistrationGrades.Count());
            }


            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrationAsync()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistrationAsync(guid);

                //Assert
                Assert.AreEqual(1, result.Approvals.Count());
                Assert.AreEqual(result.AwardGradeScheme.Id, "27178aab-a6e8-4d1e-ae27-eca1f7b33363");
                Assert.AreEqual(response.Guid, result.Id);
                Assert.AreEqual(result.Approvals.Count(), 1);
                Assert.AreEqual(response.InvolvementStartOn, result.Involvement.StartOn);
                Assert.AreEqual(response.InvolvementEndOn, result.Involvement.EndOn);
                Assert.AreEqual(result.Process.GradeExtension, null);
                Assert.AreEqual("0012297", result.Registrant.Id);
                Assert.AreEqual("19442", result.Section.Id);
                Assert.AreEqual("USA", result.SectionRegistrationReporting.CountryCode.ToString());
                Assert.AreEqual(null, result.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn);
                Assert.AreEqual("Attended", result.SectionRegistrationReporting.LastDayOfAttendance.Status.ToString());
                Assert.AreEqual("Registered", result.Status.RegistrationStatus.ToString());
                Assert.AreEqual("Registered", result.Status.SectionRegistrationStatusReason.ToString());
                Assert.AreEqual("27178aab-a6e8-4d1e-ae27-eca1f7b33363", result.Transcript.GradeScheme.Id);
                Assert.AreEqual("Standard", result.Transcript.Mode.ToString());

                //Test grades
                Assert.AreEqual(8, result.SectionRegistrationGrades.Count());
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_GetV7 : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private SectionRegistrationService sectionRegistrationService;
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
            ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();


            SectionRegistrationResponse response;
            TestSectionRegistrationRepository repo;

            [TestInitialize]
            public void Initialize()
            {

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                repo = new TestSectionRegistrationRepository();
                response = repo.GetSectionRegistrationResponse();
                BuildMocksForSectionRegistrationGet();

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            private void BuildMocksForSectionRegistrationGet()
            {
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");

                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");

                //GetPersonGuidsCollectionAsync
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("0012297", "0012297");
                personRepositoryMock.Setup(pr => pr.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(dict);

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add("19442", "19442");
                sectionRepositoryMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);

                Dictionary<string, KeyValuePair<string, string>> dictKVP = new Dictionary<string, KeyValuePair<string, string>>();
                dictKVP.Add("SBHOLE", new KeyValuePair<string, string>("SBHOLE", "0012297"));
                personRepositoryMock.Setup(pbr => pbr.GetPersonGuidsFromOperKeysAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dictKVP);

                //V7 changes
                // Mock Reference Repository for Academic Level Entities
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74588696-D1EC-2267-A0B7-DE602533E3A6", "UG", "Undergraduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74826546-D1EC-2267-A0B7-DE602533E3A6", "GR", "Graduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("54364536-D1EC-2267-A0B7-DE602533E3A6", "CE", "Continuing Education"));
                studentReferenceDataRepositoryMock.Setup(rep => rep.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelCollection);

                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
                studentReferenceDataRepositoryMock.Setup(rep => rep.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);

                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                statusItems = repo.GetSectionRegistrationStatusItems();
                var statusItem = statusItems.FirstOrDefault(s => s.Code == "Registered");

                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;

                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();

                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                sectionRepositoryMock.Setup(sr => sr.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("19442");
                sectionRegistrationRepositoryMock.Setup(sr => sr.GetGradeGuidFromIdAsync(guid)).ReturnsAsync(It.IsAny<string>());
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(statusItems);
                personBaseRepositoryMock.Setup(p => p.GetPersonGuidFromOpersAsync(It.IsAny<string>())).ReturnsAsync("SBHOLE");
                studentReferenceDataRepositoryMock.Setup(a => a.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                //registrationDto = null;
                sectionRegistrationService = null;
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations2Async_With_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("2");

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);
                sectionRegistrationRepositoryMock.Setup(s => s.GetSectionRegistrationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations2Async(0, 10, "19442", "");

                var result = results.Item1.FirstOrDefault();
                //Assert                
                Assert.AreEqual("74588696-d1ec-2267-a0b7-de602533e3a6", result.AcademicLevel.Id);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations2Async_Without_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);
                sectionRegistrationRepositoryMock.Setup(s => s.GetSectionRegistrationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("12234");
                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations2Async(0, 10, "19442", "");

                var result = results.Item1.FirstOrDefault();
                //Assert                
                Assert.AreEqual("74588696-d1ec-2267-a0b7-de602533e3a6", result.AcademicLevel.Id);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual("74588696-d1ec-2267-a0b7-de602533e3a6", result.AcademicLevel.Id);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_VerifiedGradeNull_Return_Null()
            {
                //Arrange
                response.VerifiedTermGrade = null;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.IsNull(result.RepeatedSection);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_ArgumentNullException()
            {
                var result = await sectionRegistrationService.GetSectionRegistration2Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_InvalidOperationException()
            {
                //Arrange
                response.VerifiedTermGrade = null;
                response.Messages = new List<RegistrationMessage>() { new RegistrationMessage() { Message = "Message 1" } };
                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_NotRepeated()
            {
                //Arrange
                response.ReplCode = "";
                response.RepeatedAcadCreds = null;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.NotRepeated, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_RepeatedAcadCredsEmpty_NotRepeated()
            {
                //Arrange
                response.ReplCode = "";
                response.RepeatedAcadCreds = new List<string>();
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.NotRepeated, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_RepeatedIncludeNeither()
            {
                //Arrange
                response.ReplCode = "ReplCode";
                response.AltcumContribCmplCred = 0;
                response.AltcumContribGpaCred = 0;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.RepeatedIncludeNeither, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_RepeatedIncludeBoth()
            {
                //Arrange
                response.ReplCode = "ReplCode";
                response.AltcumContribCmplCred = 1;
                response.AltcumContribGpaCred = 1;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.RepeatedIncludeBoth, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_RepeatedIncludeCredit()
            {
                //Arrange
                response.ReplCode = "ReplCode";
                response.AltcumContribCmplCred = 1;
                response.AltcumContribGpaCred = 0;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.RepeatedIncludeCredit, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_RepeatedSection_RepeatedIncludeQualityPoints()
            {
                //Arrange
                response.ReplCode = "ReplCode";
                response.AltcumContribCmplCred = 0;
                response.AltcumContribGpaCred = 1;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert                
                Assert.AreEqual(RepeatedSection.RepeatedIncludeQualityPoints, result.RepeatedSection);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_Credit_CreditTypes()
            {
                //Arrange
                string[] creditTypes = new string[] { "I", "C", "T", "E", "N", "O" };
                foreach (var creditType in creditTypes)
                {
                    response.CreditType = creditType;
                    viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                    sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                    //Act
                    var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                    //Assert
                    var enumToCheck = (CreditCategoryType3)Enum.Parse(typeof(CreditCategoryType3), result.Credit.CreditCategory.CreditType.ToString());
                    Assert.IsNotNull(enumToCheck);
                }
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_Credit_CreditMeasure2_Credit()
            {
                //Arrange
                response.Credit = 1;
                response.EarnedCredit = 1;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert
                Assert.AreEqual(1, result.Credit.EarnedCredit);
                Assert.AreEqual(1, result.Credit.AttemptedCredit);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistration2Async_Credit_CreditMeasure2_Ceus()
            {
                //Arrange
                response.Ceus = 1;
                response.EarnedCeus = 1;
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(s => s.GetAsync(guid)).ReturnsAsync(response);

                //Act
                var result = await sectionRegistrationService.GetSectionRegistration2Async(guid);

                //Assert
                Assert.AreEqual(1, result.Credit.EarnedCredit);
                Assert.AreEqual(1, result.Credit.AttemptedCredit);
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_GetV16_0_0 : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private SectionRegistrationService sectionRegistrationService;
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
            ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();
            ICollection<Domain.Student.Entities.Term> termsCollection = new List<Domain.Student.Entities.Term>();
            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection = new List<Domain.Student.Entities.AcademicPeriod>();
            ICollection<Domain.Base.Entities.Location> locations = new List<Domain.Base.Entities.Location>();

            SectionRegistrationResponse response;
            TestSectionRegistrationRepository testRepo;

            [TestInitialize]
            public void Initialize()
            {

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                testRepo = new TestSectionRegistrationRepository();
                response = testRepo.GetSectionRegistrationResponse();
                BuildMocksForSectionRegistrationGet();

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            private void BuildMocksForSectionRegistrationGet()
            {
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");

                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");

                //GetPersonGuidsCollectionAsync
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("0012297", "0012297");
                personRepositoryMock.Setup(pr => pr.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(dict);

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add("19442", "19442");
                sectionRepositoryMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);

                Dictionary<string, KeyValuePair<string, string>> dictKVP = new Dictionary<string, KeyValuePair<string, string>>();
                dictKVP.Add("SBHOLE", new KeyValuePair<string, string>("SBHOLE", "0012297"));
                personRepositoryMock.Setup(pbr => pbr.GetPersonGuidsFromOperKeysAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dictKVP);

                //V7 changes
                // Mock Reference Repository for Academic Level Entities
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74588696-D1EC-2267-A0B7-DE602533E3A6", "UG", "Undergraduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74826546-D1EC-2267-A0B7-DE602533E3A6", "GR", "Graduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("54364536-D1EC-2267-A0B7-DE602533E3A6", "CE", "Continuing Education"));
                studentReferenceDataRepositoryMock.Setup(rep => rep.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelCollection);

                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
                studentReferenceDataRepositoryMock.Setup(rep => rep.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);

                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };


                academicPeriodCollection = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("ab66b971-3ee0-4477-9bb7-539721f93435", "2018/S1", "Acad Period1", DateTime.Today.Date,
                    DateTime.Today.Date.AddDays(30), 2019, 1, "2018/S1", "Id1", "1", new List<RegistrationDate>()),
                    new Domain.Student.Entities.AcademicPeriod("bb66b971-3ee0-4477-9bb7-539721f93435", "2018/FA", "Acad Period2", DateTime.Today.Date,
                    DateTime.Today.Date.AddDays(30), 2019, 1, "2018/FA", "Id2", "2", new List<RegistrationDate>())
                };

                locations = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("bb66b971-3ee0-4477-9bb7-539721f93436", "Site1", "Site1 Descr", false),
                    new Domain.Base.Entities.Location("cb66b971-3ee0-4477-9bb7-539721f93437", "Site2", "Site2 Descr", false)
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);

                statusItems = testRepo.GetSectionRegistrationStatusItems();
                var statusItem = statusItems.FirstOrDefault(s => s.Code == "Registered");

                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;

                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(academicPeriodCollection);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");

                personRepositoryMock.Setup(pr => pr.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ReturnsAsync("0012297");

                sectionRepositoryMock.Setup(sr => sr.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("19442");
                sectionRegistrationRepositoryMock.Setup(sr => sr.GetGradeGuidFromIdAsync(guid)).ReturnsAsync(It.IsAny<string>());
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(statusItems);
                personBaseRepositoryMock.Setup(p => p.GetPersonGuidFromOpersAsync(It.IsAny<string>())).ReturnsAsync("SBHOLE");
                studentReferenceDataRepositoryMock.Setup(a => a.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                //registrationDto = null;
                sectionRegistrationService = null;
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_Section_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistration4 sect4 = new SectionRegistration4() { Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435") };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, sect4, It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_RepoException_Section_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistration4 sect4 = new SectionRegistration4() { Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435") };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, sect4, It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435"),
                    Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("12d65fb1-1df7-405c-b0ef-47edd2371392")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                Assert.IsNotNull(results);
                Assert.AreEqual(1, results.Item2);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_EmptyStatus()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435"),
                    Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_EmptyAcadPeriod()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2(""),
                    Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("12d65fb1-1df7-405c-b0ef-47edd2371392")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_InvalidStatus()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435"),
                    Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("invalid")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_InvalidPerson()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2("invalid"),
                    Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("12d65fb1-1df7-405c-b0ef-47edd2371392")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_MissingAcadPeriod()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                     Statuses = new List<Dtos.Filters.SectionRegistrationStatusFilterProperty>()
                    {   new Dtos.Filters.SectionRegistrationStatusFilterProperty()
                        {
                            Detail= new GuidObject2("12d65fb1-1df7-405c-b0ef-47edd2371392")
                        }
                    }
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
                
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_RegistrationStatusesByAcademicPeriod_Filter_MissingStatus()
            {

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var registrationStatusesByAcademicPeriodFilter = new Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter()
                {
                    AcademicPeriod = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                   It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, null, null, registrationStatusesByAcademicPeriodFilter, false);
               
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_Registrant_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistration4 sect4 = new SectionRegistration4() { Registrant = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435") };

                personRepositoryMock.Setup(rep => rep.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ReturnsAsync(() => null);

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, sect4, It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_RepoException_Registrant_Filter()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistration4 sect4 = new SectionRegistration4() { Registrant = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435") };

                personRepositoryMock.Setup(rep => rep.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ThrowsAsync(new Exception());

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, sect4, It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_AcademicPeriod_NamedQuery()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                termRepositoryMock.Setup(rep => rep.GetAcademicPeriods(new List<Term>())).Returns(new List<AcademicPeriod>());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, "AcadPeriod", It.IsAny<string>(),
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_RepoException_AcademicPeriod_NamedQuery()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                termRepositoryMock.Setup(rep => rep.GetAcademicPeriods(new List<Term>())).Throws(new Exception());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, "AcadPeriod", It.IsAny<string>(),
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_SectionInstructor_NamedQuery()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                personRepositoryMock.Setup(rep => rep.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ReturnsAsync(() => null);

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, It.IsAny<string>(), "SectInstructor",
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async_With_RepoException_SectionInstructor_NamedQuery()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);// { response }, 5);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                personRepositoryMock.Setup(rep => rep.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ThrowsAsync(new Exception());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, It.IsAny<string>(), "SectInstructor",
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
                Assert.AreEqual(results.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrations3Async()
            {
                //Arrange
                var tuple = new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>() { response }, 1);

                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistrationResponse>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Tuple<string, List<string>>>())).ReturnsAsync(tuple);

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrations3Async(0, 10, null, "ab66b971-3ee0-4477-9bb7-539721f93435", It.IsAny<string>(), 
                    It.IsAny<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>());
                Assert.IsNotNull(results);
                Assert.AreEqual(1, results.Item2);
            }

            [TestMethod]
            public async Task SectionRegistrationTest_GetSectionRegistrationByGuid3Async()
            {
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationById2Async(It.IsAny<string>())).ReturnsAsync(response);

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationByGuid3Async("ab66b971-3ee0-4477-9bb7-539721f93435", It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegistrationTest_GetSectionRegistrationByGuid3Async_ArgumentNullException()
            {
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrationByGuid3Async("", It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegistrationTest_GetSectionRegistrationByGuid3Async_KeyNotFoundException()
            {
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

                //Act
                var results = await sectionRegistrationService.GetSectionRegistrationByGuid3Async("ab66b971-3ee0-4477-9bb7-539721f93435", It.IsAny<bool>());
            }
        }

        [TestClass]
        public class SectionRegistrationGradeOptionsTests_Get : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private SectionRegistrationService sectionRegistrationService;
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            Dtos.SectionRegistrationStatus2 status;
            IEnumerable<StudentAcadCredCourseSecInfo> studentAcadCredInfo = new List<StudentAcadCredCourseSecInfo>();
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            //ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
            //ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();
            ICollection<Domain.Student.Entities.Term> termsCollection = new List<Domain.Student.Entities.Term>();
            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection = new List<Domain.Student.Entities.AcademicPeriod>();
            //ICollection<Domain.Base.Entities.Location> locations = new List<Domain.Base.Entities.Location>();

            //SectionRegistrationResponse response;
            //TestSectionRegistrationRepository testRepo;

            [TestInitialize]
            public void Initialize()
            {

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                //testRepo = new TestSectionRegistrationRepository();
                //response = testRepo.GetSectionRegistrationResponse();
                BuildMocksForSectionRegistrationGet();

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            private void BuildMocksForSectionRegistrationGet()
            {
                studentAcadCredInfo = new List<StudentAcadCredCourseSecInfo>()
                {
                    new StudentAcadCredCourseSecInfo("b524e045-01a2-4579-a8dd-82eb5d1a4ad5", "1", "19442")
                    {
                        EndDate = DateTime.Today.AddDays(60),
                        GradeScheme = "UG",
                        FinalGrade = "1",
                        StartDate = DateTime.Today,
                        StatusCode = "Y",
                        Term = "2018/FA",
                        VerifiedGrade = "1"
                    },
                    new StudentAcadCredCourseSecInfo("a524e045-01a2-4579-a8dd-82eb5d1a4ad4", "1", "19442")
                    {
                        EndDate = DateTime.Today.AddDays(60),
                        GradeScheme = "UG",
                        FinalGrade = "1",
                        StartDate = DateTime.Today,
                        StatusCode = "Y",
                        Term = "2017/SP",
                        VerifiedGrade = "1"
                    },
                    new StudentAcadCredCourseSecInfo("c524e045-01a2-4579-a8dd-82eb5d1a4ad3", "1", "19442")
                    {
                        EndDate = DateTime.Today.AddDays(60),
                        GradeScheme = "UG",
                        FinalGrade = "1",
                        StartDate = DateTime.Today,
                        StatusCode = "Y",
                        Term = "",
                        VerifiedGrade = ""
                    }
                };
                IEnumerable<Domain.Student.Entities.GradingTerm> gradingTerms = new List<Domain.Student.Entities.GradingTerm>()
                {
                    new GradingTerm("2018/FA", "2018 Fall Semester."),
                    new GradingTerm("2017/SP", "2017 spring semester.")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradingTermsAsync(It.IsAny<bool>())).ReturnsAsync(gradingTerms);                

                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);


                academicPeriodCollection = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("ab66b971-3ee0-4477-9bb7-539721f93435", "2018/S1", "Acad Period1", DateTime.Today.Date,
                    DateTime.Today.Date.AddDays(30), 2019, 1, "2018/S1", "Id1", "1", new List<RegistrationDate>()),
                    new Domain.Student.Entities.AcademicPeriod("bb66b971-3ee0-4477-9bb7-539721f93435", "2018/FA", "Acad Period2", DateTime.Today.Date,
                    DateTime.Today.Date.AddDays(30), 2019, 1, "2018/FA", "Id2", "2", new List<RegistrationDate>())
                };
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(academicPeriodCollection);
            }

            [TestCleanup]
            public void Cleanup()
            {
                //registrationDto = null;
                sectionRegistrationService = null;
            }


            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task GetSectionRegistrationsGradeOptionsAsync_IntegrationApiException()
            //{
            //    ////Act
            //    var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, It.IsAny<SectionRegistrationsGradeOptions>(), It.IsAny<bool>());
            //}

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_NoPerms_IntegrationApiException()
            //{
            //    ////Act
            //    var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("", It.IsAny<bool>());
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_NoGuid_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });
                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("", It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_NoRecordKeyReturned_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });
                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("BadGuid", It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task GetSectionRegistrationsGradeOptionsAsync_Bad_Grade_Scheme()
            {
                //Arrange
                studentAcadCredInfo.FirstOrDefault().GradeScheme = "Bad_Scheme";
                // Arrange
                viewRegistrationRole.AddPermission( new Ellucian.Colleague.Domain.Entities.Permission( Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations ) );
                viewRegistrationRole.AddPermission( new Ellucian.Colleague.Domain.Entities.Permission( Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations ) );
                roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { viewRegistrationRole } );

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2( "ab66b971-3ee0-4477-9bb7-539721f93435" )
                };

                sectionRepositoryMock.Setup( rep => rep.GetSectionIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "19442" );
                Dictionary<string, string> sectionIdDict = new Dictionary<string, string>();
                sectionIdDict.Add( "19442", "z524e045-01a2-4579-a8dd-82eb5d1a4ad3" );
                sectionRepositoryMock.Setup( rep => rep.GetSectionGuidsCollectionAsync( It.IsAny<string[]>() ) ).ReturnsAsync( sectionIdDict );
                sectionRegistrationRepositoryMock.Setup( repo => repo.GetSectionRegistrationGradeOptionsAsync( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcadCredCourseSecInfo>() ) )
                    .ReturnsAsync( new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>( studentAcadCredInfo, 3 ) );

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync( 0, 10, criteriaObj, It.IsAny<bool>() );
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_NoRecordEntityReturned_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsByIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("b524e045-01a2-4579-a8dd-82eb5d1a4ad5", It.IsAny<bool>());
            }

            [TestMethod]
            public async Task GetSectionRegistrationsGradeOptionsAsync_WithFilter_ReturnZeroRecord()
            {
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task GetSectionRegistrationsGradeOptionsAsync_WithFilter_SectionId_Null()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetSectionRegistrationsGradeOptionsAsync_WithFilter_SectionGuid()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<StudentAcadCredCourseSecInfo>())).ThrowsAsync(new RepositoryException());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
            }

            [TestMethod]
            public async Task GetSectionRegistrationsGradeOptionsAsync_NoData()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcadCredCourseSecInfo>()))
                    .ReturnsAsync(new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(new List<StudentAcadCredCourseSecInfo>(), 0));

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task GetSectionRegistrationsGradeOptionsAsync_WithData()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");
                Dictionary<string, string> sectionIdDict = new Dictionary<string, string>();
                sectionIdDict.Add("19442", "z524e045-01a2-4579-a8dd-82eb5d1a4ad3");
                sectionRepositoryMock.Setup(rep => rep.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectionIdDict);
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcadCredCourseSecInfo>()))
                    .ReturnsAsync(new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(studentAcadCredInfo, 3));

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
                Assert.AreEqual(results.Item2, 3);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetSectionRegistrationsGradeOptionsAsync_WithData_SectionGuids_Exception()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                SectionRegistrationsGradeOptions criteriaObj = new SectionRegistrationsGradeOptions()
                {
                    Section = new GuidObject2("ab66b971-3ee0-4477-9bb7-539721f93435")
                };

                sectionRepositoryMock.Setup(rep => rep.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("19442");
                sectionRepositoryMock.Setup(rep => rep.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ThrowsAsync(new Exception());
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcadCredCourseSecInfo>()))
                    .ReturnsAsync(new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(studentAcadCredInfo, 3));

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsAsync(0, 10, criteriaObj, It.IsAny<bool>());
            }

            [TestMethod]
            public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_WithData()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                Dictionary<string, string> sectionIdDict = new Dictionary<string, string>();
                sectionIdDict.Add("19442", "z524e045-01a2-4579-a8dd-82eb5d1a4ad3");
                sectionRepositoryMock.Setup(rep => rep.GetSectionGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(sectionIdDict);
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(studentAcadCredInfo.ToList()[0]);

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("b524e045-01a2-4579-a8dd-82eb5d1a4ad5", It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetSectionRegistrationsGradeOptionsByGuidAsync_WithData_IntegrationApiException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                Dictionary<string, string> sectionIdDict = new Dictionary<string, string>();
                sectionIdDict.Add("19442", "z524e045-01a2-4579-a8dd-82eb5d1a4ad3");
                sectionRepositoryMock.Setup(rep => rep.GetSectionGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(sectionIdDict);
                sectionRegistrationRepositoryMock.Setup(repo => repo.GetSectionRegistrationGradeOptionsByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(studentAcadCredInfo.ToList()[0]);
                sectionRegistrationService.IntegrationApiException = new IntegrationApiException();
                sectionRegistrationService.IntegrationApiException.AddError(new IntegrationApiError());

                ////Act
                var results = await sectionRegistrationService.GetSectionRegistrationsGradeOptionsByGuidAsync("b524e045-01a2-4579-a8dd-82eb5d1a4ad5", It.IsAny<bool>());
            }

        }

        [TestClass]
        public class SectionRegistrationServiceTests_CheckRequiredFields : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";

            private SectionRegistrationService sectionRegistrationService;
            Dtos.SectionRegistration2 registrationDto = new SectionRegistration2();
            SectionRegistrationRequest request;
            TestSectionRegistrationRepository repo;
            string json;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                repo = new TestSectionRegistrationRepository();
                json = repo.GetsectionRegistration2Json();
                request = repo.GetSectionRegistrationRequest();

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repo = null;
                request = null;

                registrationDto = null;
                curntUserFactory = null;
                sectionRegistrationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Registrant_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Registrant = null;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Registrant_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Registrant.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Section_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Section = null;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Section_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Section.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Status_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Status = null;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Status_Detail_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Status.Detail.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_AwardGradeScheme_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.AwardGradeScheme = null;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_AwardGradeScheme_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.AwardGradeScheme.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Transcript_GradeScheme_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Transcript.GradeScheme.Id = string.Empty;
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_GradeCountZero_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>();
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Grade_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>() { null };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_registrationDto_Grade_SectionGrade_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = ""}, SectionGradeType = null,
                        Submission =  new Submission(){ SubmissionMethod= SubmissionMethodType.Auto, SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = "7979"}, SubmittedOn= DateTimeOffset.Now}
                    }
                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_SectionGradeType_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "7373"}, SectionGradeType = null,
                        Submission =  new Submission(){ SubmissionMethod= SubmissionMethodType.Auto, SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = "7979"}, SubmittedOn= DateTimeOffset.Now}
                    }

                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CheckForRequiredFields_SectionGradeType_Duplicate_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                studentReferenceDataRepositoryMock.Setup(gsch => gsch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);


                var sections = new TestSectionRepository().GetAsync().Result.ToList();
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());

                var guid = Guid.NewGuid().ToString();

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
              
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "123"}, SectionGradeType = new GuidObject2(){ Id = guid},
                        Submission =  new Submission(){ SubmissionMethod= SubmissionMethodType.Auto, SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = "7979"}, SubmittedOn= DateTimeOffset.Now}
                    },
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "456"}, SectionGradeType = new GuidObject2(){ Id = guid},
                        Submission =  new Submission(){ SubmissionMethod= SubmissionMethodType.Auto, SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = "7979"}, SubmittedOn= DateTimeOffset.Now}
                    }

                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_SectionGradeType_Id_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "7373"}, SectionGradeType = new GuidObject2(){Id = ""},
                        Submission =  new Submission(){ SubmissionMethod= SubmissionMethodType.Auto, SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = "7979"}, SubmittedOn= DateTimeOffset.Now}
                    }

                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Submission_SubmittedBy_Id_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "7373"}, SectionGradeType = new GuidObject2(){Id = "46446"},
                        Submission =  new Submission()
                        {
                            SubmissionMethod= SubmissionMethodType.Auto,
                            SubmissionReason = new GuidObject2(){ Id = "1235"},
                            SubmittedBy = new GuidObject2(){Id = ""}, SubmittedOn= DateTimeOffset.Now
                        }
                    }

                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Submission_SubmissionReason_Id_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.SectionRegistrationGrades = null;
                registrationDto.SectionRegistrationGrades = new List<SectionRegistrationGrade>()
                {
                    new SectionRegistrationGrade()
                    { SectionGrade = new GuidObject2(){ Id = "7373"}, SectionGradeType = new GuidObject2(){Id = "46446"},
                        Submission =  new Submission()
                        {
                            SubmissionMethod= SubmissionMethodType.Auto,
                            SubmissionReason = new GuidObject2(){ Id = null},
                            SubmittedBy = new GuidObject2(){Id = "12"}, SubmittedOn= DateTimeOffset.Now
                        }
                    }

                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Process_ExpireOn_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess();
                registrationDto.Process.GradeExtension = new GradeExtension()
                {
                    ExpiresOn = null,
                    DefaultGrade = new GuidObject2("123")
                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Process_DefaultGrade_Id_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess();
                registrationDto.Process.GradeExtension = new GradeExtension()
                {
                    ExpiresOn = DateTimeOffset.Now,
                    DefaultGrade = new GuidObject2("")
                };
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Process_Transcript_VerifiedOn_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        ExpiresOn = DateTimeOffset.Now,
                        DefaultGrade = new GuidObject2("1234")
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("1234")
                    }
                };

                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CheckForRequiredFields_Process_Transcript_VerifiedById_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        ExpiresOn = DateTimeOffset.Now,
                        DefaultGrade = new GuidObject2("1234")
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2(""),
                        VerifiedOn = DateTimeOffset.Now
                    }
                };

                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_CheckForBusinessRules : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            private SectionRegistrationService sectionRegistrationService;
            Dtos.SectionRegistration2 registrationDto = new SectionRegistration2();
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            List<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            SectionRegistrationRequest request;
            TestSectionRegistrationRepository repo;
            string json;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                repo = new TestSectionRegistrationRepository();
                json = repo.GetsectionRegistration2Json();
                request = repo.GetSectionRegistrationRequest();
                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();
                sections = new TestSectionRepository().GetAsync().Result.ToList();
                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                studentReferenceDataRepositoryMock.Setup(gsch => gsch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repo = null;
                request = null;
                gradeEntities = null;
                registrationDto = null;
                curntUserFactory = null;
                gradeSchemes = null;
                sections = null;
                status = null;
                gradeTypes = null;
                sectionRegistrationService = null;
            }

            private void GetRegistrationDto()
            {
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CheckForBusinessRules_AwardGradeSchemeId_NotEqual_TranscriptGradeSchemeId_ArgumentException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });
                //studentReferenceDataRepositoryMock.Setup(gsch => gsch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.AwardGradeScheme.Id = "9a1914f6-ee9c-449c-92bc-8928267dfe4d";
                registrationDto.Transcript.GradeScheme.Id = "bb66b971-3ee0-4477-9bb7-539721f93434";
                //Act
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_SectionIsNull_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Section.Id = "asdf";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Section>());

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_GradeEntityIsNull_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                var sectionRegistrationGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault();
                if (sectionRegistrationGrade != null)
                    sectionRegistrationGrade.SectionGrade.Id = "12133";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(It.IsAny<List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>>());

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_GradeType_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                var sectionRegistrationGrade = registrationDto.SectionRegistrationGrades.FirstOrDefault();
                if (sectionRegistrationGrade != null)
                    sectionRegistrationGrade.SectionGradeType.Id = "0000";
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(gsch => gsch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(guid, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_GradeScheme_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"AB", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "CD", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "EF", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_TranscriptGradeScheme_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Transcript.GradeScheme.Id = Guid.NewGuid().ToString();
                registrationDto.AwardGradeScheme.Id = "bb66b971-3ee0-4477-9bb7-539721f93434";

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CheckForBusinessRules_SectionGradeSchemeMismatch_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Transcript.GradeScheme.Id = "bb66b971-3ee0-4477-9bb7-539721f93434";
                registrationDto.AwardGradeScheme.Id = "bb66b971-3ee0-4477-9bb7-539721f93434";

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                //Act
                var section = sections.First();
                section.GradeSchemeCode = "UG";
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CheckForBusinessRules_PersonRepository_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Process_Transcript_VerifiedById_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        DefaultGrade = new GuidObject2("2"),
                        ExpiresOn = DateTimeOffset.Now
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("2"),
                        VerifiedOn = DateTimeOffset.Now
                    }
                };
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };
                string testGuid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
                string personId = "abcd";

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(testGuid)).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Process_TransVerfiedOn_LessThan_SectionStartDate_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        DefaultGrade = new GuidObject2("2"),
                        ExpiresOn = DateTimeOffset.Now
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("2"),
                        VerifiedOn = new DateTimeOffset(2011, 03, 02, 0, 0, 0, 0, new TimeSpan(2, 0, 0))//DateTimeOffset.Now.Subtract(new TimeSpan(7, 18, 0, 0))
                    }
                };
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                string personId = "2";

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);
                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Process_FinalGradeNull_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(json);
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        DefaultGrade = new GuidObject2("2"),
                        ExpiresOn = DateTimeOffset.Now
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("2"),
                        VerifiedOn = new DateTimeOffset(DateTime.Today)//DateTimeOffset.Now.Subtract(new TimeSpan(7, 18, 0, 0))
                    }
                };
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                string personId = "2";

                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task IncompleteGrade_Null_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        DefaultGrade = new GuidObject2("2"),
                        ExpiresOn = DateTimeOffset.Now
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("2"),
                        VerifiedOn = new DateTimeOffset(DateTime.Today)
                    }
                };
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.Add(new Domain.Student.Entities.Grade("d874e05d-9d97-4fa3-8862-5044ef2384d0", "1234", "I", "", "Desc", "scheme"));
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DefaultGradeId_Not_Equal_To_DefaultGradeEntity_Guid_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = new SectionRegistrationProcess()
                {
                    GradeExtension = new GradeExtension()
                    {
                        DefaultGrade = new GuidObject2("2"),
                        ExpiresOn = DateTimeOffset.Now
                    },
                    Transcript = new Transcript()
                    {
                        VerifiedBy = new GuidObject2("2"),
                        VerifiedOn = new DateTimeOffset(DateTime.Today)
                    }
                };
                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.Add(new Domain.Student.Entities.Grade("d874e05d-9d97-4fa3-8862-5044ef2384d0", "1234", "I", "", "Desc", "scheme"));
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Involvement_StartOn_GreaterThan_EndOn_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;
                registrationDto.Involvement.StartOn = DateTimeOffset.MaxValue;
                registrationDto.Involvement.EndOn = DateTimeOffset.MinValue;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Involvement_StartOn_LessThan_SectionStartDate_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;
                registrationDto.Involvement.StartOn = DateTimeOffset.MinValue;
                //sections.First().StartDate = DateTimeOffset.Now.AddDays(1);

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Involvement_StartOn_GreaterThan_SectionStartDate_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;
                registrationDto.Involvement.StartOn = DateTimeOffset.Now;
                //sections.First().StartDate = DateTimeOffset.Now.AddDays(1);

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Reporting_NeverAttended_LastAttendedOnNotNull_InvalidOperationException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting.LastDayOfAttendance.LastAttendedOn = DateTimeOffset.Now;
                registrationDto.SectionRegistrationReporting.LastDayOfAttendance.Status = ReportingStatusType.NeverAttended;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

                string personId = "2";
                registrationDto.SectionRegistrationGrades.ToList().Add(new SectionRegistrationGrade()
                {
                    SectionGrade = new GuidObject2("1234"),
                    SectionGradeType = new GuidObject2("27178aab-a6e8-4d1e-ae27-eca1f7b33363"),
                    Submission = new Submission()
                    {
                        SubmissionMethod = SubmissionMethodType.Auto,
                        SubmissionReason = new GuidObject2("1111"),
                        SubmittedBy = new GuidObject2("2222"),
                        SubmittedOn = DateTime.Now
                    }
                });
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                //studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_UpdateSectionRegistrationAsync_PostPut : CurrentUserSetup
        {
            #region Mocks
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            private Mock<IColleagueTransactionInvoker> colleagueTransactionInvokerMock;
            private Mock<IColleagueDataReader> ColleagueDataReaderMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;

            ICurrentUserFactory curntUserFactory;
            #endregion

            #region private Fields
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            string json;
            private SectionRegistrationService sectionRegistrationService;
            Dtos.SectionRegistration2 registrationDto = new SectionRegistration2();
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            List<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            private IEnumerable<Domain.Base.Entities.GradeChangeReason> allGradeChangeReasons;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            SectionRegistrationRequest srRequest;
            SectionRegistrationResponse srResponse;
            TestSectionRegistrationRepository repo;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            #endregion

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                colleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                ColleagueDataReaderMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(colleagueDataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(colleagueTransactionInvokerMock.Object);

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                repo = new TestSectionRegistrationRepository();
                json = repo.GetsectionRegistration2Json();
                srRequest = repo.GetSectionRegistrationRequest();
                srResponse = repo.GetSectionRegistrationResponse();
                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();
                sections = new TestSectionRepository().GetAsync().Result.ToList();
                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;
                statusItems = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result.ToList(); //new TestSectionRegistrationRepository().GetSectionRegistrationStatusItems();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                allGradeChangeReasons = new TestGradeChangeReasonRepository().Get();
                referenceDataRepositoryMock.Setup(x => x.GetGradeChangeReasonAsync(It.IsAny<bool>())).ReturnsAsync(allGradeChangeReasons);

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repo = null;
                srRequest = null;
                gradeEntities = null;
                registrationDto = null;
                curntUserFactory = null;
                gradeSchemes = null;
                sections = null;
                status = null;
                gradeTypes = null;
                sectionRegistrationService = null;
            }

            private void GetRegistrationDto()
            {
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);
            }

            private void GetRegistrationDtoGrades()
            {
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegService_UpdateSectRegAsync_PersonId_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(string.Empty));

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task SectionRegService_UpdateSectionRegistrationAsync_PermissionsException()
            //{
            //    //Arrange
            //    viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));

            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

            //    GetRegistrationDto();
            //    registrationDto.Process = null;

            //    registrationDto.Involvement = null;

            //    registrationDto.SectionRegistrationReporting = null;

            //    gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
            //    gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

            //    string personId = "2";
            //    //Act
            //    sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First());
            //    gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

            //    studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
            //    studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

            //    //this method is called multiple times, following is the way you can set it up to return different results
            //    personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
            //    sectionRegistrationRepositoryMock
            //        .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            //        .ThrowsAsync(new KeyNotFoundException());
            //    var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(string.Empty, registrationDto);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_NullStatuses_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDtoGrades();
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";


                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(string.Empty, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_SectionId_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync(string.Empty, registrationDto);
            }

            [TestMethod]
            public async Task SectionRegService_UpdateSectionRegistrationAsync()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);

                registrationDto.Process = null;
                registrationDto.Involvement = null;
                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
                updateRequest.RegSections = new List<RegSections>();
                updateRequest.StudentId = srRequest.StudentId;
                updateRequest.CreateStudentFlag = srRequest.CreateStudentFlag;
                updateRequest.SecRegGuid = guid;
                updateRequest.RegSections.Add(new RegSections()
                {
                    SectionIds = srRequest.Section.SectionId,
                    SectionAction = srRequest.Section.Action.ToString(),
                    SectionCredits = srRequest.Section.Credits,
                    SectionDate = srRequest.Section.RegistrationDate
                });

                UpdateSectionRegistrationResponse updateResponse = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = false,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>() { new RegMessages() { Message = "Some Message", MessageSection = "MessageSection" } },
                    IpcRegId = string.Empty,
                    SecRegGuid = registrationDto.Id
                };

                var importGradeResponse = new ImportGradesResponse();
                importGradeResponse.GradeMessages = new List<GradeMessages>()
                {
                    new GradeMessages()
                    {
                        StatusCode = "SUCCESS",
                        ErrorMessge = "Error occured",
                        InfoMessage = "Info Message"
                    }
                };

                importGradeResponse.Guid = "0335b9e8-ba5a-4099-8633-bcb15b5ca9cb";
                importGradeResponse.SectionRegId = "19359";
                importGradeResponse.Grades = BuildGrades();

                Dictionary<string, GuidLookupResult> dictResult = new Dictionary<string, GuidLookupResult>();
                dictResult.Add("SomeKey", new GuidLookupResult() { Entity = "SomeEntity", PrimaryKey = "1359" });

                //Act
                gradeRepositoryMock.Setup(gr => gr.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(srr => srr.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srr => srr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);

                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId));

                sectionRepositoryMock.Setup(sr => sr.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);

                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest)).ReturnsAsync(updateResponse);
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(It.IsAny<ImportGradesRequest>())).ReturnsAsync(importGradeResponse);

                sectionRegistrationRepositoryMock.Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(srResponse);
                sectionRegistrationRepositoryMock.Setup(k => k.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("27875");
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("0beb504d-cfa0-4a72-97f7-98b1fabc29a5", registrationDto);

                Assert.AreEqual(registrationDto.Approvals.Count(), result.Approvals.Count());
                Assert.AreEqual(registrationDto.AwardGradeScheme.Id, result.AwardGradeScheme.Id);
            }

            [TestMethod]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_Grades()
            {
                statusItems.Add(
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem(
                        "12d65fb1-1df7-405c-b0ef-47edd2371392", "Registered", "Registered"));

                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);

                registrationDto.Process = null;
                registrationDto.Involvement = null;
                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
                updateRequest.RegSections = new List<RegSections>();
                updateRequest.StudentId = srRequest.StudentId;
                updateRequest.CreateStudentFlag = srRequest.CreateStudentFlag;
                updateRequest.SecRegGuid = guid;
                updateRequest.RegSections.Add(new RegSections()
                {
                    SectionIds = srRequest.Section.SectionId,
                    SectionAction = srRequest.Section.Action.ToString(),
                    SectionCredits = srRequest.Section.Credits,
                    SectionDate = srRequest.Section.RegistrationDate
                });

                UpdateSectionRegistrationResponse updateResponse = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = false,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>() { new RegMessages() { Message = "Some Message", MessageSection = "MessageSection" } },
                    IpcRegId = string.Empty,
                    SecRegGuid = registrationDto.Id
                };

                var importGradeResponse = new ImportGradesResponse();
                importGradeResponse.GradeMessages = new List<GradeMessages>()
                {
                    new GradeMessages()
                    {
                        StatusCode = "SUCCESS",
                        ErrorMessge = "Error occured",
                        InfoMessage = "Info Message"
                    }
                };

                importGradeResponse.Guid = "0335b9e8-ba5a-4099-8633-bcb15b5ca9cb";
                importGradeResponse.SectionRegId = "19359";
                importGradeResponse.Grades = BuildGrades();

                Dictionary<string, GuidLookupResult> dictResult = new Dictionary<string, GuidLookupResult>();
                dictResult.Add("SomeKey", new GuidLookupResult() { Entity = "SomeEntity", PrimaryKey = "1359" });

                //Act
                gradeRepositoryMock.Setup(gr => gr.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(srr => srr.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srr => srr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);

                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId));

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personId));

                sectionRepositoryMock.Setup(sr => sr.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);

                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest)).ReturnsAsync(updateResponse);
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(It.IsAny<ImportGradesRequest>())).ReturnsAsync(importGradeResponse);

                sectionRegistrationRepositoryMock.Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(srResponse);
                sectionRegistrationRepositoryMock.Setup(k => k.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("27875");
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("0beb504d-cfa0-4a72-97f7-98b1fabc29a5", registrationDto);

                Assert.AreEqual(registrationDto.Approvals.Count(), result.Approvals.Count());
                Assert.AreEqual(registrationDto.AwardGradeScheme.Id, result.AwardGradeScheme.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_Grades_Submittedby_Id_CORP()
            {
                statusItems.Add(
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem(
                        "12d65fb1-1df7-405c-b0ef-47edd2371392", "Registered", "Registered"));

                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);

                registrationDto.Process = null;
                registrationDto.Involvement = null;
                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
                updateRequest.RegSections = new List<RegSections>();
                updateRequest.StudentId = srRequest.StudentId;
                updateRequest.CreateStudentFlag = srRequest.CreateStudentFlag;
                updateRequest.SecRegGuid = guid;
                updateRequest.RegSections.Add(new RegSections()
                {
                    SectionIds = srRequest.Section.SectionId,
                    SectionAction = srRequest.Section.Action.ToString(),
                    SectionCredits = srRequest.Section.Credits,
                    SectionDate = srRequest.Section.RegistrationDate
                });

                UpdateSectionRegistrationResponse updateResponse = new UpdateSectionRegistrationResponse()
                {
                    ErrorOccurred = false,
                    ErrorMessage = "Error",
                    RegMessages = new List<RegMessages>() { new RegMessages() { Message = "Some Message", MessageSection = "MessageSection" } },
                    IpcRegId = string.Empty,
                    SecRegGuid = registrationDto.Id
                };

                var importGradeResponse = new ImportGradesResponse();
                importGradeResponse.GradeMessages = new List<GradeMessages>()
                {
                    new GradeMessages()
                    {
                        StatusCode = "SUCCESS",
                        ErrorMessge = "Error occured",
                        InfoMessage = "Info Message"
                    }
                };

                importGradeResponse.Guid = "0335b9e8-ba5a-4099-8633-bcb15b5ca9cb";
                importGradeResponse.SectionRegId = "19359";
                importGradeResponse.Grades = BuildGrades();

                Dictionary<string, GuidLookupResult> dictResult = new Dictionary<string, GuidLookupResult>();
                dictResult.Add("SomeKey", new GuidLookupResult() { Entity = "SomeEntity", PrimaryKey = "1359" });

                //Act
                gradeRepositoryMock.Setup(gr => gr.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(srr => srr.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srr => srr.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);

                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId))
                .Returns(Task.FromResult(personId));

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personId));
                personRepositoryMock.Setup(p => p.IsCorpAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

                sectionRepositoryMock.Setup(sr => sr.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(sr => sr.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);

                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest)).ReturnsAsync(updateResponse);
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(It.IsAny<ImportGradesRequest>())).ReturnsAsync(importGradeResponse);

                sectionRegistrationRepositoryMock.Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(srResponse);
                sectionRegistrationRepositoryMock.Setup(k => k.GetSectionRegistrationIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("27875");
                var result = await sectionRegistrationService.UpdateSectionRegistrationAsync("0beb504d-cfa0-4a72-97f7-98b1fabc29a5", registrationDto);

                Assert.AreEqual(registrationDto.Approvals.Count(), result.Approvals.Count());
                Assert.AreEqual(registrationDto.AwardGradeScheme.Id, result.AwardGradeScheme.Id);
            }

            private List<Ellucian.Colleague.Data.Student.Transactions.Grades> BuildGrades()
            {
                List<Ellucian.Colleague.Data.Student.Transactions.Grades> grades = new List<Ellucian.Colleague.Data.Student.Transactions.Grades>();

                //Build Final Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades finalGrade =
                    new Ellucian.Colleague.Data.Student.Transactions.Grades
                    {
                        Grade = "14",
                        GradeExpiry = string.Empty,
                        GradeKey = "14",
                        GradeSubmitBy = "0012297",
                        GradeSubmitDate = "01/01/2016",
                        GradeType = "FINAL",
                        InvEndOn = new DateTime(2016, 04, 01),
                        InvStartOn = new DateTime(2016, 01, 01),
                        LastDayAttendDate = string.Empty,
                        NeverAttend = "N",
                        GradeChangeReason = "OE"
                    };
                grades.Add(finalGrade);

                //Build Verified Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades verifiedGrade =
                    new Ellucian.Colleague.Data.Student.Transactions.Grades
                    {
                        Grade = "14",
                        GradeExpiry = string.Empty,
                        GradeKey = "14",
                        GradeSubmitBy = "0012297",
                        GradeSubmitDate = "01/01/2016",
                        GradeType = "VERIFIED",
                        InvEndOn = new DateTime(2016, 04, 01),
                        InvStartOn = new DateTime(2016, 01, 01),
                        LastDayAttendDate = string.Empty,
                        NeverAttend = "N",
                        GradeChangeReason = "IC"
                    };
                grades.Add(verifiedGrade);

                //Build Midterm Grades

                for (int i = 1; i <= 6; i++)
                {
                    Ellucian.Colleague.Data.Student.Transactions.Grades midTermGrade =
                        new Ellucian.Colleague.Data.Student.Transactions.Grades
                        {
                            Grade = "14",
                            GradeExpiry = string.Empty,
                            GradeKey = "14",
                            GradeSubmitBy = "0012297",
                            GradeSubmitDate = "01/01/2016",
                            GradeType = string.Concat("MID", i.ToString()),
                            GradeChangeReason = "OE",
                            InvEndOn = new DateTime(2016, 04, 01),
                            InvStartOn = new DateTime(2016, 01, 01),
                            LastDayAttendDate = string.Empty,
                            NeverAttend = "N"
                        };
                    grades.Add(midTermGrade);
                }
                return grades;
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_UpdateSectionRegistrationAsyncV7_PostPut : CurrentUserSetup
        {
            #region Mocks
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            private Mock<IColleagueTransactionInvoker> colleagueTransactionInvokerMock;
            private Mock<IColleagueDataReader> ColleagueDataReaderMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;

            ICurrentUserFactory curntUserFactory;
            #endregion

            #region private Fields
            string guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";
            string json;
            private SectionRegistrationService sectionRegistrationService;
            Dtos.SectionRegistration3 registrationDto = new SectionRegistration3();
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            List<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            Dtos.SectionRegistrationStatus2 status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeEntities;
            private IEnumerable<Domain.Base.Entities.GradeChangeReason> allGradeChangeReasons;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            SectionRegistrationRequest srRequest;
            SectionRegistrationResponse srResponse;
            TestSectionRegistrationRepository repo;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            #endregion

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                colleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                ColleagueDataReaderMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(colleagueDataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(colleagueTransactionInvokerMock.Object);

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                repo = new TestSectionRegistrationRepository();
                json = repo.GetsectionRegistration2Json();
                srRequest = repo.GetSectionRegistrationRequest();
                srResponse = repo.GetSectionRegistrationResponse();
                gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();
                sections = new TestSectionRepository().GetAsync().Result.ToList();
                gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;
                statusItems = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result.ToList(); //new TestSectionRegistrationRepository().GetSectionRegistrationStatusItems();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                allGradeChangeReasons = new TestGradeChangeReasonRepository().Get();
                referenceDataRepositoryMock.Setup(x => x.GetGradeChangeReasonAsync(It.IsAny<bool>())).ReturnsAsync(allGradeChangeReasons);

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repo = null;
                srRequest = null;
                gradeEntities = null;
                registrationDto = null;
                curntUserFactory = null;
                gradeSchemes = null;
                sections = null;
                status = null;
                gradeTypes = null;
                sectionRegistrationService = null;
            }

            private void GetRegistrationDto()
            {
                //var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2Json();
                //var jsonPayload = "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'academicLevel':{'id':'5b65853c-3d6c-4949-8de1-74861dfe6bb1'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '3cf900894jck'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";                
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration3Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration3>(jsonPayload);
            }

            private void GetRegistrationDtoGrades()
            {
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration3>(jsonPayload);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegServiceV7_UpdateSectRegAsync_PersonId_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDto();
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(string.Empty));

                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration2Async("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task SectionRegServiceV7_UpdateSectionRegistrationAsync_PermissionsException()
            //{
            //    //Arrange
            //    viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));

            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

            //    GetRegistrationDto();
            //    registrationDto.Process = null;

            //    registrationDto.Involvement = null;

            //    registrationDto.SectionRegistrationReporting = null;

            //    gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
            //    gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";

            //    string personId = "2";
            //    //Act
            //    sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First());
            //    gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

            //    studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
            //    studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

            //    //this method is called multiple times, following is the way you can set it up to return different results
            //    personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
            //    sectionRegistrationRepositoryMock
            //        .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            //        .ThrowsAsync(new KeyNotFoundException());
            //    var result = await sectionRegistrationService.UpdateSectionRegistration2Async(string.Empty, registrationDto);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionRegServiceV7_UpdateSectionRegistrationAsync_NullStatuses_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                GetRegistrationDtoGrades();
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";


                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration2Async(string.Empty, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegServiceV7_UpdateSectionRegistrationAsync_SectionId_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration3Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration3>(jsonPayload);
                registrationDto.Process = null;

                registrationDto.Involvement = null;

                registrationDto.SectionRegistrationReporting = null;

                gradeEntities = new TestGradeRepository().GetHedmAsync().Result.ToList();
                gradeEntities.FirstOrDefault(g => g.Guid.Equals("d874e05d-9d97-4fa3-8862-5044ef2384d0", StringComparison.InvariantCultureIgnoreCase)).IncompleteGrade = "1234";
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                gradeRepositoryMock.Setup(g => g.GetHedmAsync(It.IsAny<bool>())).ReturnsAsync(gradeEntities);

                studentReferenceDataRepositoryMock.Setup(t => t.GetSectionGradeTypesAsync(It.IsAny<bool>())).ReturnsAsync(gradeTypes);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.UpdateAsync(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration2Async(string.Empty, registrationDto);
            }

            private List<Ellucian.Colleague.Data.Student.Transactions.Grades> BuildGrades()
            {
                List<Ellucian.Colleague.Data.Student.Transactions.Grades> grades = new List<Ellucian.Colleague.Data.Student.Transactions.Grades>();

                //Build Final Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades finalGrade =
                    new Ellucian.Colleague.Data.Student.Transactions.Grades
                    {
                        Grade = "14",
                        GradeExpiry = string.Empty,
                        GradeKey = "14",
                        GradeSubmitBy = "0012297",
                        GradeSubmitDate = "01/01/2016",
                        GradeType = "FINAL",
                        InvEndOn = new DateTime(2016, 04, 01),
                        InvStartOn = new DateTime(2016, 01, 01),
                        LastDayAttendDate = string.Empty,
                        NeverAttend = "N",
                        GradeChangeReason = "OE"
                    };
                grades.Add(finalGrade);

                //Build Verified Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades verifiedGrade =
                    new Ellucian.Colleague.Data.Student.Transactions.Grades
                    {
                        Grade = "14",
                        GradeExpiry = string.Empty,
                        GradeKey = "14",
                        GradeSubmitBy = "0012297",
                        GradeSubmitDate = "01/01/2016",
                        GradeType = "VERIFIED",
                        InvEndOn = new DateTime(2016, 04, 01),
                        InvStartOn = new DateTime(2016, 01, 01),
                        LastDayAttendDate = string.Empty,
                        NeverAttend = "N",
                        GradeChangeReason = "IC"
                    };
                grades.Add(verifiedGrade);

                //Build Midterm Grades

                for (int i = 1; i <= 6; i++)
                {
                    Ellucian.Colleague.Data.Student.Transactions.Grades midTermGrade =
                        new Ellucian.Colleague.Data.Student.Transactions.Grades
                        {
                            Grade = "14",
                            GradeExpiry = string.Empty,
                            GradeKey = "14",
                            GradeSubmitBy = "0012297",
                            GradeSubmitDate = "01/01/2016",
                            GradeType = string.Concat("MID", i.ToString()),
                            GradeChangeReason = "OE",
                            InvEndOn = new DateTime(2016, 04, 01),
                            InvStartOn = new DateTime(2016, 01, 01),
                            LastDayAttendDate = string.Empty,
                            NeverAttend = "N"
                        };
                    grades.Add(midTermGrade);
                }
                return grades;
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_UpdateSectionRegistrationAsyncV16_0_0_PostPut : CurrentUserSetup
        {
            #region Mocks
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> colleagueDataReaderMock;
            private Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            private Mock<IColleagueTransactionInvoker> colleagueTransactionInvokerMock;
            private Mock<IColleagueDataReader> ColleagueDataReaderMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;

            ICurrentUserFactory curntUserFactory;
            #endregion

            #region private Fields
            string json;
            private SectionRegistrationService sectionRegistrationService;
            Dtos.SectionRegistration4 registrationDto = new SectionRegistration4();
            List<Domain.Student.Entities.GradeScheme> gradeSchemes;
            List<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
            private IEnumerable<Domain.Base.Entities.GradeChangeReason> allGradeChangeReasons;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            SectionRegistrationRequest srRequest;
            SectionRegistrationResponse srResponse;
            TestSectionRegistrationRepository repo;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> statusItems;
            #endregion

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                colleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                ColleagueDataReaderMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(colleagueDataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(colleagueTransactionInvokerMock.Object);

                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                repo = new TestSectionRegistrationRepository();
                json = repo.GetsectionRegistration2Json();
                srRequest = repo.GetSectionRegistrationRequest();
                srResponse = repo.GetSectionRegistrationResponse();
                sections = new TestSectionRepository().GetAsync().Result.ToList();
                statusItems = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result.ToList(); //new TestSectionRegistrationRepository().GetSectionRegistrationStatusItems();
                gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                // Mock Reference Repository for Academic Level Entities
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("5b65853c-3d6c-4949-8de1-74861dfe6bb1", "UG", "Undergraduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74826546-D1EC-2267-A0B7-DE602533E3A6", "GR", "Graduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("54364536-D1EC-2267-A0B7-DE602533E3A6", "CE", "Continuing Education"));
                studentReferenceDataRepositoryMock.Setup(rep => rep.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevelCollection);

                GetRegistrationDto();

                allGradeChangeReasons = new TestGradeChangeReasonRepository().Get();
                referenceDataRepositoryMock.Setup(x => x.GetGradeChangeReasonAsync(It.IsAny<bool>())).ReturnsAsync(allGradeChangeReasons);

                sectionRegistrationService = new SectionRegistrationService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object,
                                                            sectionRepositoryMock.Object, sectionRegistrationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                                            curntUserFactory, roleRepositoryMock.Object, loggerMock.Object, studentRepositoryMock.Object,
                                                            gradeRepositoryMock.Object, termRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repo = null;
                srRequest = null;
                registrationDto = null;
                curntUserFactory = null;
                gradeSchemes = null;
                sections = null;
                academicLevelCollection = null;
                sectionRegistrationService = null;
            }

            private void GetRegistrationDto()
            {
                //var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2Json();
                //var jsonPayload = "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'academicLevel':{'id':'5b65853c-3d6c-4949-8de1-74861dfe6bb1'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '3cf900894jck'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";                
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration4Json();
                registrationDto = JsonConvert.DeserializeObject<SectionRegistration4>(jsonPayload);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegService_UpdateSectRegAsync_PersonId_Null_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                // GetRegistrationDto();

                registrationDto.Involvement = null;

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(personId))
                    .Returns(Task.FromResult(string.Empty));

                sectionRegistrationRepositoryMock
                    .Setup(a => a.Update2Async(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration3Async("d874e05d-9d97-4fa3-8862-5044ef2384d0", registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_PermissionsException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                // GetRegistrationDto();

                registrationDto.Involvement = null;

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.Update2Async(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration3Async(string.Empty, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_NullStatuses_ArgumentNullException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                registrationDto.Involvement = null;

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                
                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.Update2Async(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration3Async(string.Empty, registrationDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionRegService_UpdateSectionRegistrationAsync_SectionId_KeyNotFoundException()
            {
                //Arrange
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.ViewRegistrations));
                viewRegistrationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.SectionPermissionCodes.UpdateRegistrations));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewRegistrationRole });

                //var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration4Json();
                //registrationDto = JsonConvert.DeserializeObject<SectionRegistration4>(jsonPayload);

                registrationDto.Involvement = null;

                string personId = "2";
                //Act
                sectionRepositoryMock.Setup(s => s.GetSectionByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(sections.First());
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(sections.First().Id);
                studentReferenceDataRepositoryMock.Setup(sch => sch.GetGradeSchemesAsync(It.IsAny<bool>())).ReturnsAsync(gradeSchemes);
                studentReferenceDataRepositoryMock.Setup(srs => srs.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                sectionRepositoryMock.Setup(s => s.GetSectionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());

                //this method is called multiple times, following is the way you can set it up to return different results
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                sectionRegistrationRepositoryMock
                    .Setup(a => a.Update2Async(It.IsAny<SectionRegistrationRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var result = await sectionRegistrationService.UpdateSectionRegistration3Async(string.Empty, registrationDto);
            }
        }

        [TestClass]
        public class SectionRegistrationServiceTests_UpdateSectionRegistration_Delete : CurrentUserSetup
        {
            #region Mocks
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IPersonBaseRepository> _personBaseRepositoryMock;
            private Mock<ISectionRepository> _sectionRepositoryMock;
            private Mock<ISectionRegistrationRepository> _sectionRegistrationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<IGradeRepository> _gradeRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<IColleagueDataReader> _colleagueDataReaderMock;
            private Mock<IColleagueTransactionInvoker> _colleagueTransactionInvokerMock;
            Mock<IColleagueTransactionFactory> _transFactoryMock;

            ICurrentUserFactory _currntUserFactory;
            #endregion

            #region private Fields

            private const string Guid = "0beb504d-cfa0-4a72-97f7-98b1fabc29a5";

            private SectionRegistrationService _sectionRegistrationService;
            Dtos.SectionRegistration2 _registrationDto = new SectionRegistration2();
            List<Domain.Student.Entities.GradeScheme> _gradeSchemes;
            List<Ellucian.Colleague.Domain.Student.Entities.Section> _sections;
            // Dtos.SectionRegistrationStatus2 _status;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType> _gradeTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.Grade> _gradeEntities;
            SectionRegistrationRequest _srRequest;
            SectionRegistrationResponse _srResponse;
            TestSectionRegistrationRepository _repo;
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem> _statusItems;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            #endregion

            [TestInitialize]
            public void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _sectionRepositoryMock = new Mock<ISectionRepository>();
                _sectionRegistrationRepositoryMock = new Mock<ISectionRegistrationRepository>();
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _gradeRepositoryMock = new Mock<IGradeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                // _baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                _colleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                _colleagueDataReaderMock = new Mock<IColleagueDataReader>();
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_colleagueDataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                _transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(_colleagueTransactionInvokerMock.Object);

                _currntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                _repo = new TestSectionRegistrationRepository();
                // json = _repo.GetsectionRegistration2Json();
                _srRequest = _repo.GetSectionRegistrationRequest();
                _srResponse = _repo.GetSectionRegistrationResponse();
                _gradeEntities = new TestGradeRepository().GetAsync().Result.ToList();
                _sections = new TestSectionRepository().GetAsync().Result.ToList();
                _gradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeType>;
                _statusItems = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result.ToList();
                _gradeSchemes = new List<Domain.Student.Entities.GradeScheme>()
                {
                    new Domain.Student.Entities.GradeScheme("bb66b971-3ee0-4477-9bb7-539721f93434" ,"CE", "Continuing Education")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30), EffectiveEndDate = DateTime.Today.AddDays(30) },
                    new Domain.Student.Entities.GradeScheme("5aeebc5c-c973-4f83-be4b-f64c95002124", "GR", "Graduate")
                    { EffectiveStartDate = DateTime.Today.AddDays(-30) },
                    new Domain.Student.Entities.GradeScheme("9a1914f6-ee9c-449c-92bc-8928267dfe4d", "UG", "Undergraduate")
                    { EffectiveStartDate = DateTime.Today }
                };

                _sectionRegistrationService = new SectionRegistrationService(_adapterRegistryMock.Object, _personRepositoryMock.Object, _personBaseRepositoryMock.Object,
                                                            _sectionRepositoryMock.Object, _sectionRegistrationRepositoryMock.Object, _studentReferenceDataRepositoryMock.Object,
                                                            _currntUserFactory, _roleRepositoryMock.Object, _loggerMock.Object, _studentRepositoryMock.Object,
                                                            _gradeRepositoryMock.Object, termRepositoryMock.Object, _referenceDataRepositoryMock.Object, baseConfigurationRepository);

            }

            [TestCleanup]
            public void Cleanup()
            {
                _repo = null;
                _srRequest = null;
                _gradeEntities = null;
                _registrationDto = null;
                _currntUserFactory = null;
                _gradeSchemes = null;
                _sections = null;

                _gradeTypes = null;
                _sectionRegistrationService = null;
            }

            private void GetRegistrationDto()
            {
                var jsonPayload = new TestSectionRegistrationRepository().GetsectionRegistration2JsonWithFinalGrade();
                _registrationDto = JsonConvert.DeserializeObject<SectionRegistration2>(jsonPayload);
            }

            private List<Ellucian.Colleague.Data.Student.Transactions.Grades> BuildGrades()
            {
                List<Ellucian.Colleague.Data.Student.Transactions.Grades> grades = new List<Ellucian.Colleague.Data.Student.Transactions.Grades>();

                //Build Final Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades finalGrade = new Ellucian.Colleague.Data.Student.Transactions.Grades();
                finalGrade.Grade = "14";
                finalGrade.GradeExpiry = string.Empty;
                finalGrade.GradeKey = "14";
                finalGrade.GradeSubmitBy = "0012297";
                finalGrade.GradeSubmitDate = "01/01/2016";
                finalGrade.GradeType = "1";
                finalGrade.InvEndOn = new DateTime(2016, 04, 01);
                finalGrade.InvStartOn = new DateTime(2016, 01, 01);
                finalGrade.LastDayAttendDate = string.Empty;
                finalGrade.NeverAttend = "N";
                grades.Add(finalGrade);

                //Build Verified Grades
                Ellucian.Colleague.Data.Student.Transactions.Grades verifiedGrade = new Ellucian.Colleague.Data.Student.Transactions.Grades();
                verifiedGrade.Grade = "14";
                verifiedGrade.GradeExpiry = string.Empty;
                verifiedGrade.GradeKey = "14";
                verifiedGrade.GradeSubmitBy = "0012297";
                verifiedGrade.GradeSubmitDate = "01/01/2016";
                verifiedGrade.GradeType = "1";
                verifiedGrade.InvEndOn = new DateTime(2016, 04, 01);
                verifiedGrade.InvStartOn = new DateTime(2016, 01, 01);
                verifiedGrade.LastDayAttendDate = string.Empty;
                verifiedGrade.NeverAttend = "N";
                grades.Add(verifiedGrade);

                //Build Midterm Grades

                for (int i = 1; i <= 6; i++)
                {
                    Ellucian.Colleague.Data.Student.Transactions.Grades midTermGrade = new Ellucian.Colleague.Data.Student.Transactions.Grades();
                    midTermGrade.Grade = "14";
                    midTermGrade.GradeExpiry = string.Empty;
                    midTermGrade.GradeKey = "14";
                    midTermGrade.GradeSubmitBy = "0012297";
                    midTermGrade.GradeSubmitDate = "01/01/2016";
                    midTermGrade.GradeType = i.ToString();
                    midTermGrade.InvEndOn = new DateTime(2016, 04, 01);
                    midTermGrade.InvStartOn = new DateTime(2016, 01, 01);
                    midTermGrade.LastDayAttendDate = string.Empty;
                    midTermGrade.NeverAttend = "N";
                    grades.Add(midTermGrade);
                }
                return grades;
            }
        }
    }
}