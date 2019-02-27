// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionsMaximumControllerTests
    {
        [TestClass]
        public class SectionsMaximumControllerTests_V6
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            Mock<ILogger> loggerMock;

            SectionsMaximumController sectionsMaximumController;
            Tuple<IEnumerable<Dtos.SectionMaximum2>, int> sectionMaximimTuple;
            IEnumerable<Dtos.SectionMaximum2> sectionMaximumV6Dtos;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                sectionsMaximumController = new SectionsMaximumController(sectionCoordinationServiceMock.Object, loggerMock.Object);
                sectionsMaximumController.Request = new HttpRequestMessage();
                sectionsMaximumController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionCoordinationServiceMock = null;
                loggerMock = null;
                sectionsMaximumController = null;
                sectionMaximimTuple = null;
                sectionMaximumV6Dtos = null;
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV6_GGetHedmSectionsMaximum2Async()
            {
                sectionsMaximumController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(sectionMaximimTuple);

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.SectionMaximum2> sectionMaximumV6Results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionMaximum2>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.SectionMaximum2>;

                int count = sectionMaximumV6Dtos.Count();

                Assert.AreEqual(count, sectionMaximumV6Results.Count());
                for (int i = 0; i < count; i++)
                {
                    var expected = sectionMaximumV6Dtos.ToList()[i];
                    var actual = sectionMaximumV6Results[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Duration, actual.Duration);

                    Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                    Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                    Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                    Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                    Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                    Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                    Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                    Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                    Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                    Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                    Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                    Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                    Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                    Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                    Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                    Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                    Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                    Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                    Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                    Assert.AreEqual(expected.Site.Title, actual.Site.Title);
                }
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async()
            {
                var expected = sectionMaximumV6Dtos.ToList()[0];
                var id = "0e21d5e1-d798-4e45-961b-9780f0de05bc";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async(id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Number, actual.Number);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Duration, actual.Duration);

                Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                Assert.AreEqual(expected.Site.Title, actual.Site.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_ArgumentNullException()
            {
                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_PermissionsException()
            {
                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_ArgumentException()
            {
                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_RepositoryException()
            {
                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_IntegrationApiException()
            {
                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionsMaximum2Async_Exception()
            {
                var title = "Some Title";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum2Async(It.IsAny<int>(), It.IsAny<int>(), title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum2Async(null, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_IntegrationApiException()
            {
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_IntegrationApiException1()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV6_GetHedmSectionMaximumByGuid2Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid2Async("1");
            }

            private void BuildData()
            {
                sectionMaximumV6Dtos = new List<Dtos.SectionMaximum2>()
                {
                    #region 1stDto
                    new Dtos.SectionMaximum2()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code1",
                            Detail = new GuidObject2("4b3cd307-8e40-476c-822b-59eca5b9bde1"),
                            Title = "Code1 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        AwardGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("9a1914f6-ee9c-449c-92bc-8928267dfe4d"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        TranscriptGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("9a1914f6-ee9c-449c-92bc-8928267dfe4d"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }                        
                    },
                    #endregion
                    #region 2ndDto
                    new Dtos.SectionMaximum2()
                    {
                        Id = "1e21d5e1-d798-4e45-961b-9780f0de05bd",
                        Number = "01",
                        Code = "MATH-100-01",
                        Title = "MATH 101",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code2",
                            Detail = new GuidObject2("65a92541-dd49-40a6-953b-b3444cecf950"),
                            Title = "Code2 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("f15ea599-17bc-415d-b6de-b0a8e9682019"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("29f6e2cd-1e5f-4485-9b27-60d4f4e4b1fg"),
                                Code = "200",
                                Title = "Second Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 17,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("hfc48097-ced7-4cab-bb7a-928e84c38b49"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("da4c74f8-5e2c-45f6-a41a-508b0aa3f2b7"),
                            Title = "Accounting w/Prog",
                            Number = "200",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("cd54668d-50d9-416c-81e9-2318e88571a2"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        AwardGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("0a1914f6-ee9c-449c-92bc-8928267dfe4e"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        TranscriptGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("6b65853c-3d6c-4949-8de1-74861dfe6bb2"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("9a1914f6-ee9c-449c-92bc-8928267dfe4d"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }                        
                    },
                    #endregion
                    #region 3rdDto
                    new Dtos.SectionMaximum2()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code3",
                            Detail = new GuidObject2("2972dd38-d983-449d-a88a-327441bdc162"),
                            Title = "Code3 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                       Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        AwardGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("9a1914f6-ee9c-449c-92bc-8928267dfe4d"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        TranscriptGradeSchemes = new List<Dtos.DtoProperties.GradeSchemeDtoProperty>()
                        {
                            new Dtos.DtoProperties.GradeSchemeDtoProperty()
                            {
                                Start = new DateTime(1991,07,01),
                                AcademicLevel = new Dtos.DtoProperties.AcademicLevelDtoProperty()
                                {
                                    Code = "UG",
                                    Detail = new GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                    Title = "Undergraduate"
                                },
                                Detail = new GuidObject2("9a1914f6-ee9c-449c-92bc-8928267dfe4d"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty()
                        { 
                            Code = "IPP", 
                            Detail = new GuidObject2("5f269758-b3bd-4765-bfba-45a9ad427ee6"), 
                            Title = "Instructional PlatformDto Property" 
                        }                        
                    }
                    #endregion
                };
                sectionMaximimTuple = new Tuple<IEnumerable<SectionMaximum2>, int>(sectionMaximumV6Dtos, sectionMaximumV6Dtos.Count());
            }
        }

        [TestClass]
        public class SectionsMaximumControllerTests_V8
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            Mock<ILogger> loggerMock;

            SectionsMaximumController sectionsMaximumController;
            Tuple<IEnumerable<Dtos.SectionMaximum3>, int> sectionMaximimTuple;
            IEnumerable<Dtos.SectionMaximum3> sectionMaximumV8Dtos;

            private string criteria = "{ 'title': 'title', 'starton': '01/01/2016', 'endon': '01/31/2016', 'code': 'code', 'number': '5', 'instructionalplatform': 'instructionalplatform', 'academicperiod': 'academicperiod', 'academiclevels': 'UG', 'course': 'MATH 101', 'site': 'site 1', 'status': 'open', 'owningInstitutionUnits': 'owningInstitutionUnits' }";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                sectionsMaximumController = new SectionsMaximumController(sectionCoordinationServiceMock.Object, loggerMock.Object);
                sectionsMaximumController.Request = new HttpRequestMessage();
                sectionsMaximumController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                 
        }

        [TestCleanup]
            public void Cleanup()
            {
                sectionCoordinationServiceMock = null;
                loggerMock = null;
                sectionsMaximumController = null;
                sectionMaximimTuple = null;
                sectionMaximumV8Dtos = null;
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async()
            {
                sectionsMaximumController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(sectionMaximimTuple);

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(It.IsAny<Paging>(), criteriaFilter);

                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.SectionMaximum3> sectionMaximumV8Results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionMaximum3>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.SectionMaximum3>;

                int count = sectionMaximumV8Dtos.Count();

                Assert.AreEqual(count, sectionMaximumV8Results.Count());
                for (int i = 0; i < count; i++)
                {
                    var expected = sectionMaximumV8Dtos.ToList()[i];
                    var actual = sectionMaximumV8Results[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Duration, actual.Duration);

                    Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                    Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                    Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                    Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                    Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                    Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                    Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                    Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                    Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                    Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                    Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                    Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                    Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                    Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                    Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                    Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                    Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                    Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                    Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                    Assert.AreEqual(expected.Site.Title, actual.Site.Title);
                }
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_Empty_Criteria_Values()
            {
                sectionsMaximumController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(sectionMaximimTuple);

               // var tempcriteria = "{ 'title': '', 'starton': '', 'endon': '', 'code': '', 'number': '', 'instructionalplatform': '', 'academicperiod': '', 'academiclevels': '', 'course': '', 'site': '', 'status': '', 'owningInstitutionUnits': '' }";

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(It.IsAny<Paging>(), criteriaFilter);

                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async()
            {
                var expected = sectionMaximumV8Dtos.ToList()[0];
                var id = "0e21d5e1-d798-4e45-961b-9780f0de05bc";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async(id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Number, actual.Number);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Duration, actual.Duration);

                Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                Assert.AreEqual(expected.Site.Title, actual.Site.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ThrowsAsync(new PermissionsException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                     It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                     .ThrowsAsync(new ArgumentException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_In_Switch_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                     It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                     .ThrowsAsync(new ArgumentException());
                string tempCriteria = "{ 'starttOn': '01/01/2017' }";
                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                      It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                      .ThrowsAsync(new RepositoryException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_IntegrationApiException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                       .ThrowsAsync(new IntegrationApiException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionsMaximum3Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                       .ThrowsAsync(new Exception());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum3Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_IntegrationApiException()
            {
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid3Async_IntegrationApiException1()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV8_GetHedmSectionMaximumByGuid2Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid3Async(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid3Async("1");
            }

            private void BuildData()
            {
                sectionMaximumV8Dtos = new List<Dtos.SectionMaximum3>()
                {
                    #region 1stDto
                    new Dtos.SectionMaximum3()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code1",
                            Detail = new GuidObject2("4b3cd307-8e40-476c-822b-59eca5b9bde1"),
                            Title = "Code1 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }                        
                    },
                    #endregion
                    #region 2ndDto
                    new Dtos.SectionMaximum3()
                    {
                        Id = "1e21d5e1-d798-4e45-961b-9780f0de05bd",
                        Number = "01",
                        Code = "MATH-100-01",
                        Title = "MATH 101",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code2",
                            Detail = new GuidObject2("65a92541-dd49-40a6-953b-b3444cecf950"),
                            Title = "Code2 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("f15ea599-17bc-415d-b6de-b0a8e9682019"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("29f6e2cd-1e5f-4485-9b27-60d4f4e4b1fg"),
                                Code = "200",
                                Title = "Second Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 17,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("hfc48097-ced7-4cab-bb7a-928e84c38b49"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("da4c74f8-5e2c-45f6-a41a-508b0aa3f2b7"),
                            Title = "Accounting w/Prog",
                            Number = "200",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("cd54668d-50d9-416c-81e9-2318e88571a2"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }                        
                    },
                    #endregion
                    #region 3rdDto
                    new Dtos.SectionMaximum3()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code3",
                            Detail = new GuidObject2("2972dd38-d983-449d-a88a-327441bdc162"),
                            Title = "Code3 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = SectionStatus2.Open,
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                       Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""                                                    
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            { 
                                                Code = "SCI", 
                                                Detail = new GuidObject2(""), 
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }                                        
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }                                    
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty()
                        { 
                            Code = "IPP", 
                            Detail = new GuidObject2("5f269758-b3bd-4765-bfba-45a9ad427ee6"), 
                            Title = "Instructional PlatformDto Property" 
                        }                        
                    }
                    #endregion
                };
                sectionMaximimTuple = new Tuple<IEnumerable<SectionMaximum3>, int>(sectionMaximumV8Dtos, sectionMaximumV8Dtos.Count());
            }
        }

        [TestClass]
        public class SectionsMaximumControllerTests_V11
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            Mock<ILogger> loggerMock;

            SectionsMaximumController sectionsMaximumController;
            Tuple<IEnumerable<Dtos.SectionMaximum4>, int> sectionMaximimTuple;
            IEnumerable<Dtos.SectionMaximum4> sectionMaximumV11Dtos;

            private string criteria = "{ 'title': 'title', 'starton': '01/01/2016', 'endon': '01/31/2016', 'code': 'code', 'number': '5', 'instructionalplatform': 'instructionalplatform', 'academicperiod': 'academicperiod', 'academiclevels': 'UG', 'course': 'MATH 101', 'site': 'site 1', 'status': 'open', 'owningInstitutionUnits': 'owningInstitutionUnits' }";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                sectionsMaximumController = new SectionsMaximumController(sectionCoordinationServiceMock.Object, loggerMock.Object);
                sectionsMaximumController.Request = new HttpRequestMessage();
                sectionsMaximumController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionCoordinationServiceMock = null;
                loggerMock = null;
                sectionsMaximumController = null;
                sectionMaximimTuple = null;
                sectionMaximumV11Dtos = null;
            }

          
            [TestMethod]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_Empty_Criteria_Values()
            {
                sectionsMaximumController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ReturnsAsync(sectionMaximimTuple);

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(It.IsAny<Paging>(), criteriaFilter);

                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid3Async()
            {
                var expected = sectionMaximumV11Dtos.ToList()[0];
                var id = "0e21d5e1-d798-4e45-961b-9780f0de05bc";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async(id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Number, actual.Number);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Duration, actual.Duration);

                Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                Assert.AreEqual(expected.Site.Title, actual.Site.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                    .ThrowsAsync(new PermissionsException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                     It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                     .ThrowsAsync(new ArgumentException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_In_Switch_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                     It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                     .ThrowsAsync(new ArgumentException());
                string tempCriteria = "{ 'starttOn': '01/01/2017' }";
                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                      It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                      .ThrowsAsync(new RepositoryException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_IntegrationApiException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                       .ThrowsAsync(new IntegrationApiException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionsMaximum4Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                       .ThrowsAsync(new Exception());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum4Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_IntegrationApiException()
            {
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_IntegrationApiException1()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV11_GetHedmSectionMaximumByGuid4Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid4Async(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid4Async("1");
            }

            private void BuildData()
            {
                sectionMaximumV11Dtos = new List<Dtos.SectionMaximum4>()
                {
                    #region 1stDto
                    new Dtos.SectionMaximum4()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code1",
                            Detail = new GuidObject2("4b3cd307-8e40-476c-822b-59eca5b9bde1"),
                            Title = "Code1 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status =  new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }
                    },
                    #endregion
                    #region 2ndDto
                    new Dtos.SectionMaximum4()
                    {
                        Id = "1e21d5e1-d798-4e45-961b-9780f0de05bd",
                        Number = "01",
                        Code = "MATH-100-01",
                        Title = "MATH 101",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code2",
                            Detail = new GuidObject2("65a92541-dd49-40a6-953b-b3444cecf950"),
                            Title = "Code2 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("f15ea599-17bc-415d-b6de-b0a8e9682019"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("29f6e2cd-1e5f-4485-9b27-60d4f4e4b1fg"),
                                Code = "200",
                                Title = "Second Year"
                            }
                        },
                        Status = new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 17,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("hfc48097-ced7-4cab-bb7a-928e84c38b49"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("da4c74f8-5e2c-45f6-a41a-508b0aa3f2b7"),
                            Title = "Accounting w/Prog",
                            Number = "200",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("cd54668d-50d9-416c-81e9-2318e88571a2"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }
                    },
                    #endregion
                    #region 3rdDto
                    new Dtos.SectionMaximum4()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code3",
                            Detail = new GuidObject2("2972dd38-d983-449d-a88a-327441bdc162"),
                            Title = "Code3 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty2()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Title = "Cost Accounting w/Prog",
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                       Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty2>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty2()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                {
                                    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                    {
                                        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                        {
                                            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                            {
                                                new Dtos.DtoProperties.CredentialDtoProperty()
                                                {
                                                    EndOn = null,
                                                    StartOn = null,
                                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                                    Value = ""
                                                }
                                            }
                                        }
                                    }
                                },
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty()
                        {
                            Code = "IPP",
                            Detail = new GuidObject2("5f269758-b3bd-4765-bfba-45a9ad427ee6"),
                            Title = "Instructional PlatformDto Property"
                        }
                    }
                    #endregion
                };
                sectionMaximimTuple = new Tuple<IEnumerable<SectionMaximum4>, int>(sectionMaximumV11Dtos, sectionMaximumV11Dtos.Count());
            }
        }

        [TestClass]
        public class SectionsMaximumControllerTests_V16
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            Mock<ILogger> loggerMock;

            SectionsMaximumController sectionsMaximumController;
            Tuple<IEnumerable<Dtos.SectionMaximum5>, int> sectionMaximimTuple;
            IEnumerable<Dtos.SectionMaximum5> sectionMaximumV16Dtos;

            private string criteria = "{ 'title': 'title', 'starton': '01/01/2016', 'endon': '01/31/2016', 'code': 'code', 'number': '5', 'instructionalplatform': 'instructionalplatform', 'academicperiod': 'academicperiod', 'academiclevels': 'UG', 'course': 'MATH 101', 'site': 'site 1', 'status': 'open', 'owningInstitutionUnits': 'owningInstitutionUnits' }";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                sectionsMaximumController = new SectionsMaximumController(sectionCoordinationServiceMock.Object, loggerMock.Object);
                sectionsMaximumController.Request = new HttpRequestMessage();
                sectionsMaximumController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionCoordinationServiceMock = null;
                loggerMock = null;
                sectionsMaximumController = null;
                sectionMaximimTuple = null;
                sectionMaximumV16Dtos = null;
            }


            [TestMethod]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum4Async_Empty_Criteria_Values()
            {
                sectionsMaximumController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),  It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(sectionMaximimTuple);

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(It.IsAny<Paging>(), criteriaFilter);

                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async()
            {
                var expected = sectionMaximumV16Dtos.ToList()[0];
                var id = "0e21d5e1-d798-4e45-961b-9780f0de05bc";
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async(id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Number, actual.Number);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Duration, actual.Duration);

                Assert.AreEqual(expected.AcademicPeriod.Code, actual.AcademicPeriod.Code);
                Assert.AreEqual(expected.AcademicPeriod.Detail.Id, actual.AcademicPeriod.Detail.Id);
                Assert.AreEqual(expected.AcademicPeriod.End, actual.AcademicPeriod.End);
                Assert.AreEqual(expected.AcademicPeriod.Start, actual.AcademicPeriod.Start);
                Assert.AreEqual(expected.AcademicPeriod.Title, actual.AcademicPeriod.Title);

                Assert.AreEqual(expected.Course.Detail.Id, actual.Course.Detail.Id);
                Assert.AreEqual(expected.Course.Number, actual.Course.Number);
                Assert.AreEqual(expected.Course.Subject, actual.Course.Subject);
                //Assert.AreEqual(expected.Course.Title, actual.Course.Title);
                Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());

                Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                Assert.AreEqual(expected.InstructionalEvents.Count(), actual.InstructionalEvents.Count());

                Assert.AreEqual(expected.InstructionalPlatform.Code, actual.InstructionalPlatform.Code);
                Assert.AreEqual(expected.InstructionalPlatform.Detail.Id, actual.InstructionalPlatform.Detail.Id);
                Assert.AreEqual(expected.InstructionalPlatform.Title, actual.InstructionalPlatform.Title);

                Assert.AreEqual(expected.MaximumEnrollment, actual.MaximumEnrollment);
                Assert.AreEqual(expected.OwningOrganizations.Count(), actual.OwningOrganizations.Count());

                Assert.AreEqual(expected.Site.Code, actual.Site.Code);
                Assert.AreEqual(expected.Site.Detail.Id, actual.Site.Detail.Id);
                Assert.AreEqual(expected.Site.Title, actual.Site.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_In_Switch_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                string tempCriteria = "{ 'starttOn': '01/01/2017' }";
                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_IntegrationApiException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionsMaximum5Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionsMaximum5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await sectionsMaximumController.GetHedmSectionsMaximum5Async(null, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_IntegrationApiException()
            {
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_PermissionsException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_ArgumentException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_IntegrationApiException1()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsMaximumControllerV16_GetHedmSectionMaximumByGuid5Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetSectionMaximumByGuid5Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actual = await sectionsMaximumController.GetHedmSectionMaximumByGuid5Async("1");
            }

            private void BuildData()
            {
                sectionMaximumV16Dtos = new List<Dtos.SectionMaximum5>()
                {
                    #region 1stDto
                    new Dtos.SectionMaximum5()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code1",
                            Detail = new GuidObject2("4b3cd307-8e40-476c-822b-59eca5b9bde1"),
                            Title = "Code1 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty3()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status =  new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty2()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Titles = new List<CoursesTitlesDtoProperty2>() { new CoursesTitlesDtoProperty2()
                            { Type = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                                Value = "Cost Accounting w/Prog" } },
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty3>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty3()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                //InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty3>()
                                //{
                                //    new Dtos.DtoProperties.InstructorRosterDtoProperty3()
                                //    {
                                //        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                //        {
                                //            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                //            {
                                //                new Dtos.DtoProperties.CredentialDtoProperty()
                                //                {
                                //                    EndOn = null,
                                //                    StartOn = null,
                                //                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                //                    Value = ""
                                //                }
                                //            }
                                //        }
                                //    }
                                //},
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }
                    },
                    #endregion
                    #region 2ndDto
                    new Dtos.SectionMaximum5()
                    {
                        Id = "1e21d5e1-d798-4e45-961b-9780f0de05bd",
                        Number = "01",
                        Code = "MATH-100-01",
                        Title = "MATH 101",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code2",
                            Detail = new GuidObject2("65a92541-dd49-40a6-953b-b3444cecf950"),
                            Title = "Code2 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty3()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("f15ea599-17bc-415d-b6de-b0a8e9682019"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("29f6e2cd-1e5f-4485-9b27-60d4f4e4b1fg"),
                                Code = "200",
                                Title = "Second Year"
                            }
                        },
                        Status = new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 17,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("hfc48097-ced7-4cab-bb7a-928e84c38b49"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty2()
                        {
                            Detail = new GuidObject2("da4c74f8-5e2c-45f6-a41a-508b0aa3f2b7"),
                            Titles = new List<CoursesTitlesDtoProperty2>() { new CoursesTitlesDtoProperty2()
                            { Type = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                                Value = "Accounting w/Prog" } },
                            Number = "200",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("cd54668d-50d9-416c-81e9-2318e88571a2"),
                                Title = "Accounting"
                            }
                        },
                        Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty3>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty3()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                //InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                //{
                                //    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                //    {
                                //        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                //        {
                                //            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                //            {
                                //                new Dtos.DtoProperties.CredentialDtoProperty()
                                //                {
                                //                    EndOn = null,
                                //                    StartOn = null,
                                //                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                //                    Value = ""
                                //                }
                                //            }
                                //        }
                                //    }
                                //},
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty(){ Code = "", Detail = new GuidObject2(""), Title = "" }
                    },
                    #endregion
                    #region 3rdDto
                    new Dtos.SectionMaximum5()
                    {
                        Id = "0e21d5e1-d798-4e45-961b-9780f0de05bc",
                        Number = "01",
                        Code = "ACCT-100-01",
                        Title = "Cost Accounting",
                        StartOn = new DateTime(2010, 08, 26),
                        EndOn = new DateTime(2010, 12, 15),
                        Site = new SiteDtoProperty()
                        {
                            Code = "Code3",
                            Detail = new GuidObject2("2972dd38-d983-449d-a88a-327441bdc162"),
                            Title = "Code3 Title"
                        },
                        AcademicPeriod = new Dtos.DtoProperties.AcademicPeriodDtoProperty3()
                        {
                            Start = new DateTime(2010, 08, 26),
                            End = new DateTime(2010, 12, 15),
                            Detail = new Dtos.GuidObject2("e15ea599-17bc-415d-b6de-b0a8e9682018"),
                            Code = "2010/FA",
                            Title = "2010 Fall Term"
                        },
                        AcademicLevels = new List<Dtos.DtoProperties.AcademicLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.AcademicLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("5b65853c-3d6c-4949-8de1-74861dfe6bb1"),
                                Title = "Undergraduate",
                                Code = "UG"
                            }
                        },
                        CourseLevels = new List<Dtos.DtoProperties.CourseLevelDtoProperty>()
                        {
                            new Dtos.DtoProperties.CourseLevelDtoProperty()
                            {
                                Detail = new Dtos.GuidObject2("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff"),
                                Code = "100",
                                Title = "First Year"
                            }
                        },
                        Status = new SectionStatusDtoProperty() { Category = SectionStatus2.Open },
                        Duration = new SectionDuration2()
                        {
                            Length = 16,
                            Unit = DurationUnit2.Weeks
                        },
                        MaximumEnrollment = 30,
                        OwningOrganizations = new List<Dtos.DtoProperties.OwningOrganizationDtoProperty>()
                        {
                            new Dtos.DtoProperties.OwningOrganizationDtoProperty()
                            {
                                OwnershipPercentage = 100,
                                Code = "BUSN",
                                Detail = new GuidObject2("ffc48097-ced7-4cab-bb7a-928e84c38b47"),
                                Title = "Business Administration"
                            }
                        },
                        Course = new Dtos.DtoProperties.CourseDtoProperty2()
                        {
                            Detail = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                            Titles = new List<CoursesTitlesDtoProperty2>() { new CoursesTitlesDtoProperty2()
                            { Type = new GuidObject2("ca4c74f8-5e2c-45f6-a41a-508b0aa3f2b6"),
                                Value = "Cost Accounting w/Prog" } },
                            Number = "100",
                            Subject = new Dtos.DtoProperties.SubjectDtoProperty()
                            {
                                Abbreviation = "ACCT",
                                Detail = new GuidObject2("bd54668d-50d9-416c-81e9-2318e88571a1"),
                                Title = "Accounting"
                            }
                        },
                       Credits = new List<Dtos.DtoProperties.Credit2DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credit2DtoProperty()
                            {
                                CreditCategory = new Dtos.DtoProperties.CreditCategory2DtoProperty()
                                {
                                    Code = "IN",
                                    CreditType  = CreditCategoryType3.Institutional,
                                    Detail = new GuidObject2("32bd05e1-948f-4434-8e9e-a0a1701265c8"),
                                    Title = "Institutional"
                                },
                                Increment = 10,
                                Minimum = 3,
                                Maximum = 100,
                                Measure = CreditMeasure2.Credit
                            }
                        },
                        InstructionalEvents = new List<Dtos.DtoProperties.InstructionalEventDtoProperty3>()
                        {
                            new Dtos.DtoProperties.InstructionalEventDtoProperty3()
                            {
                                Detail = new GuidObject2("547e0dc3-47ec-428f-aa22-1a050dfa50a8"),
                                InstructionalMethod = new Dtos.DtoProperties.InstructionalMethodDtoProperty()
                                {
                                     Abbreviation = "LEC",
                                     Detail = new GuidObject2("44a168bb-7d55-4d6b-a3d6-d0da6032e2eb"),
                                     Title = "Lecture"
                                },
                                //InstructorRoster = new List<Dtos.DtoProperties.InstructorRosterDtoProperty2>()
                                //{
                                //    new Dtos.DtoProperties.InstructorRosterDtoProperty2()
                                //    {
                                //        Instructor = new Dtos.DtoProperties.InstructorDtoProperty2()
                                //        {
                                //            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                                //            {
                                //                new Dtos.DtoProperties.CredentialDtoProperty()
                                //                {
                                //                    EndOn = null,
                                //                    StartOn = null,
                                //                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                //                    Value = ""
                                //                }
                                //            }
                                //        }
                                //    }
                                //},
                                Locations = new List<Dtos.LocationDtoProperty>()
                                {
                                    new Dtos.LocationDtoProperty()
                                    {
                                        Location = new LocationRoomDtoProperty()
                                        {
                                            Building = new BuildingDtoProperty()
                                            {
                                                Code = "SCI",
                                                Detail = new GuidObject2(""),
                                                Title = "Science Building"
                                            },
                                            Detail = new GuidObject2("22a168bb-7d55-4d6b-a3d6-d0da6032e2fd"),
                                            Floor = "2",
                                            LocationType = InstructionalLocationType.InstructionalRoom,
                                            Number = "12A",
                                            Title = "Science Lab"
                                        }
                                    }
                                },
                                Recurrence = new Recurrence3()
                                {
                                    TimePeriod = new RepeatTimePeriod2()
                                    {
                                        StartOn = new DateTime(2010, 08, 26),
                                        EndOn = new DateTime(2010, 12, 15)
                                    },
                                    RepeatRule = new Dtos.RepeatRuleDaily()
                                    {
                                        Interval = 1,
                                        Ends = new RepeatRuleEnds() { Date = new DateTime(2017, 12, 31) },
                                        Type = FrequencyType2.Weekly
                                    }
                                }
                            }
                        },
                        InstructionalPlatform = new InstructionalPlatformDtoProperty()
                        {
                            Code = "IPP",
                            Detail = new GuidObject2("5f269758-b3bd-4765-bfba-45a9ad427ee6"),
                            Title = "Instructional PlatformDto Property"
                        }
                    }
                    #endregion
                };
                sectionMaximimTuple = new Tuple<IEnumerable<SectionMaximum5>, int>(sectionMaximumV16Dtos, sectionMaximumV16Dtos.Count());
            }
        }
    }
}
