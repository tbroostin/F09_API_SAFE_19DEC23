//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class DefaultSettingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IDefaultSettingsService> defaultSettingsServiceMock;
        private Mock<ILogger> loggerMock;
        private DefaultSettingsController defaultSettingsController;
        private IEnumerable<Domain.Base.Entities.DefaultSettings> allDefaultSettings;
        private List<DefaultSettings> defaultSettingsCollection;
        private readonly string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            defaultSettingsServiceMock = new Mock<IDefaultSettingsService>();
            loggerMock = new Mock<ILogger>();
            defaultSettingsCollection = new List<DefaultSettings>();

            allDefaultSettings = new List<Domain.Base.Entities.DefaultSettings>()
                {
                    new Domain.Base.Entities.DefaultSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Default Privacy Code")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "persons",
                                PropertyName = "privacyStatus"
                            }
                        },
                        SourceTitle = "Don't Release Grades",
                        SourceValue = "G",
                        FieldHelp = "Long Description for field help.",
                        SearchType = "A",
                        SearchMinLength = 3
                    },
                    new Domain.Base.Entities.DefaultSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "Pledge Ben/Ded Code 1")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTartet.commitment.type"
                            }
                        },
                        SourceTitle = "Accounts Receivable - Balance",
                        SourceValue = "ARBL",
                        FieldHelp = "Long Description for field help.",
                        SearchType = "R",
                        SearchMinLength = 3
                    },
                    new Domain.Base.Entities.DefaultSettings("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "Course Inactive Status Default")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        SourceTitle = "Inactive",
                        SourceValue = "I",
                        FieldHelp = "Long Description for field help."
                    }
                };

            foreach (var source in allDefaultSettings)
            {
                var defaultSettings = new DefaultSettings
                {
                    Id = source.Guid,
                    Title = source.Description,
                    Description = source.FieldHelp,
                    Source = new DefaultSettingsSource() { Title = source.SourceTitle, Value = source.SourceValue }
                };
                if(source.SearchType == "A")
                {
                    defaultSettings.AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                    {
                        AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable,
                        MinSearchLength  = source.SearchMinLength
                    };
                }
                else if (source.SearchType == "R")
                {
                    defaultSettings.AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                    {
                        AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired,
                        MinSearchLength = source.SearchMinLength
                    };
                }
                var defaultSettingsEthos = new List<DefaultSettingsEthos>();
                foreach (var ethos in source.EthosResources)
                {
                    defaultSettingsEthos.Add(new DefaultSettingsEthos()
                    {
                        Resource = ethos.Resource,
                        PropertyName = ethos.PropertyName
                    });
                }
                defaultSettings.Ethos = defaultSettingsEthos;
                defaultSettingsCollection.Add(defaultSettings);
            }

            defaultSettingsController = new DefaultSettingsController(defaultSettingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            defaultSettingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            defaultSettingsController = null;
            allDefaultSettings = null;
            defaultSettingsCollection = null;
            loggerMock = null;
            defaultSettingsServiceMock = null;
        }

        [TestMethod]
        public async Task DefaultSettingsController_GetDefaultSettings_ValidateFields_Nocache()
        {
            defaultSettingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false)).ReturnsAsync(defaultSettingsCollection);

            var sourceContexts = (await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(defaultSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = defaultSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                if (expected.AdvancedSearch != null)
                {
                    if (expected.AdvancedSearch.AdvanceSearchType == Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable)
                    {
                        Assert.AreEqual(expected.AdvancedSearch.AdvanceSearchType, Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable, "AdvancedSearch.AdvanceSearchType, Index=" + i.ToString());
                    }
                    else if(expected.AdvancedSearch.AdvanceSearchType == Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired){
                        Assert.AreEqual(expected.AdvancedSearch.AdvanceSearchType, Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired, "AdvancedSearch.AdvanceSearchType, Index=" + i.ToString());
                    }
                    Assert.AreEqual(expected.AdvancedSearch.MinSearchLength, actual.AdvancedSearch.MinSearchLength, "AdvancedSearch.MinSearchLength, Index=" + i.ToString());
                }
            }
        }

        [TestMethod]
        public async Task DefaultSettingsController_GetDefaultSettings_ValidateFields_Cache()
        {
            defaultSettingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), true)).ReturnsAsync(defaultSettingsCollection);

            var sourceContexts = (await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(defaultSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = defaultSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_KeyNotFoundException()
        {
            //
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<KeyNotFoundException>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_PermissionsException()
        {

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<PermissionsException>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_ArgumentException()
        {

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<ArgumentException>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_RepositoryException()
        {

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<RepositoryException>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_IntegrationApiException()
        {

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<IntegrationApiException>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuidAsync_ValidateFields()
        {
            var expected = defaultSettingsCollection.FirstOrDefault();
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await defaultSettingsController.GetDefaultSettingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettings_Exception()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false)).Throws<Exception>();
            await defaultSettingsController.GetDefaultSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuidAsync_Exception()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_KeyNotFoundException()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_PermissionsException()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_ArgumentException()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_RepositoryException()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_IntegrationApiException()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsByGuid_Exception()
        {
            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await defaultSettingsController.GetDefaultSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PostDefaultSettingsAsync_Exception()
        {
            await defaultSettingsController.PostDefaultSettingsAsync(defaultSettingsCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_ValidateFields()
        {
            var expected = defaultSettingsCollection.ToArray().FirstOrDefault();
            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = defaultSettingsCollection.FirstOrDefault();
            var actual = await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateDesc()
        {

            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
           
            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "INVALID",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateSourceTitle()
        {
            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "INVALID", Value = "G" }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateTitle()
        {
            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "INVALID",
                Description = "Default Privacy Code",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateSourceEmptyAdvanceSearch()
        {
            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired,
                    MinSearchLength = 4
                }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    
                }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateSourceChangeAdvanceSearch()
        {
            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired,
                    MinSearchLength = 4
                }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable,
                    MinSearchLength = 3
                }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAsync_UpdateSourceAdvanceSearch()
        {
            var expected = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable,
                    MinSearchLength = 3
                }
            };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            defaultSettingsServiceMock.Setup(x => x.UpdateDefaultSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new DefaultSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" },
                AdvancedSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty()
                {
                    AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired,
                    MinSearchLength = 4
                }
            };
            await defaultSettingsController.PutDefaultSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_DeleteDefaultSettingsAsync_Exception()
        {
            await defaultSettingsController.DeleteDefaultSettingsAsync(defaultSettingsCollection.FirstOrDefault().Id);
        }
        
        [TestMethod]
        public async Task DefaultSettingsController_GetDefaultSettingsAdvancedSearchOptionsAsync()
        {
            var advancedSearch = new Dtos.Filters.DefaultSettingsFilter()
            {
                Keyword = "Professor",
                DefaultSettings = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };
            
            defaultSettingsController.Request.Properties.Add(string.Format("FilterObject{0}", "advancedSearch"), advancedSearch);

            var option1 = new Dtos.DefaultSettingsAdvancedSearchOptions
            {
                Title = "Adj Faculty",
                Value = "1",
                Origin = "POSITION"
            };
            var option2 = new Dtos.DefaultSettingsAdvancedSearchOptions
            {
                Title = "Professor",
                Value = "2",
                Origin = "POSITION"
            };

            var advancedSearchOptionsCollection = new List<Dtos.DefaultSettingsAdvancedSearchOptions>() { option1, option2 };

            defaultSettingsServiceMock.Setup(x => x.GetDefaultSettingsAdvancedSearchOptionsAsync(It.IsAny<Dtos.Filters.DefaultSettingsFilter>(),
                true)).ReturnsAsync(advancedSearchOptionsCollection);

            defaultSettingsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            await defaultSettingsController.GetDefaultSettingsAdvancedSearchOptionsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsAdvancedSearchOptionsAsync_NoAdvancedSearch()
        {
            defaultSettingsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            await defaultSettingsController.GetDefaultSettingsAdvancedSearchOptionsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsAdvancedSearchOptionsAsync_NoKeyword()
        {
            var advancedSearch = new Dtos.Filters.DefaultSettingsFilter()
            {
                DefaultSettings = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            defaultSettingsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            await defaultSettingsController.GetDefaultSettingsAdvancedSearchOptionsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsAdvancedSearchOptionsAsync_NoDefaultSetting()
        {
            var advancedSearch = new Dtos.Filters.DefaultSettingsFilter()
            {
                Keyword = "Professor"
            };

            defaultSettingsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            await defaultSettingsController.GetDefaultSettingsAdvancedSearchOptionsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_GetDefaultSettingsAdvancedSearchOptionsByGuidAsync_Exception()
        {
            var defaultSettingsOptionsDto = new Dtos.DefaultSettingsOptions();
            await defaultSettingsController.GetDefaultSettingsAdvancedSearchOptionsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PutDefaultSettingsAdvancedSearchOptionsAsync_Exception()
        {
            var defaultSettingsOptionsDto = new Dtos.DefaultSettingsOptions();
            await defaultSettingsController.PutDefaultSettingsAdvancedSearchOptionsAsync(defaultSettingsOptionsDto);
        }
                
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DefaultSettingsController_PostDefaultSettingsAdvancedSearchOptionsAsync_Exception()
        {
            var defaultSettingsOptionsDto = new Dtos.DefaultSettingsOptions();
            await defaultSettingsController.PostDefaultSettingsAdvancedSearchOptionsAsync(defaultSettingsOptionsDto);
        }

    }
}