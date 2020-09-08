//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
    public class CollectionConfigurationSettingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICollectionConfigurationSettingsService> collectionConfigurationSettingsServiceMock;
        private Mock<ILogger> loggerMock;
        private CollectionConfigurationSettingsController collectionConfigurationSettingsController;
        private IEnumerable<Domain.Base.Entities.CollectionConfigurationSettings> allCollectionConfigurationSettings;
        private List<CollectionConfigurationSettings> collectionConfigurationSettingsCollection;
        private readonly string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            collectionConfigurationSettingsServiceMock = new Mock<ICollectionConfigurationSettingsService>();
            loggerMock = new Mock<ILogger>();
            collectionConfigurationSettingsCollection = new List<CollectionConfigurationSettings>();

            allCollectionConfigurationSettings = new List<Domain.Base.Entities.CollectionConfigurationSettings>()
                {
                    new Domain.Base.Entities.CollectionConfigurationSettings("b51d5c02-1010-44de-9fae-671ca7769170", "1", "Include in Enrolled Headcount")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "section-registration-statuses",
                                PropertyName = "headcountStatus"
                            }
                        },
                        Source = new List<Domain.Base.Entities.CollectionConfigurationSettingsSource>()
                        {
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "New",
                                SourceValue = "N"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Add",
                                SourceValue = "A"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Dropped",
                                SourceValue = "D"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Withdrawn",
                                SourceValue = "W"
                            }
                        },
                        FieldHelp = "For the section-registration-statuses resource, Colleague assigns...",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "STUDENT.ACAD.CRED.STATUSES",
                        FieldName = "LDMD.INCLUDE.ENRL.HEADCOUNTS"
                    },
                    new Domain.Base.Entities.CollectionConfigurationSettings("8f724aa8-5e7d-41dd-99b6-07d5ae4374c9","2","Office Codes")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "admission-application-supporting-item-types"
                            },
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "admission-application-supporting-types"
                            }
                        },
                        Source = new List<Domain.Base.Entities.CollectionConfigurationSettingsSource>()
                        {
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Admissions",
                                SourceValue = "ADM"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Admissions 2",
                                SourceValue = "AM"
                            }
                        },
                        FieldHelp = "Office codes in Colleague identify which communication codes and correspondence...",
                        EntityName = "CORE.VALCODES",
                        ValcodeTableName = "OFFICE.CODES",
                        FieldName = "LDMD.DFLT.ADM.OFFICE.CODES"
                    },
                    new Domain.Base.Entities.CollectionConfigurationSettings("c84e4997-235f-4fb8-af86-ac800768f39f", "3", "Employee Benefits to Exclude")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "employees",
                                PropertyName = "benefitsStatus"
                            }
                        },
                        Source = new List<Domain.Base.Entities.CollectionConfigurationSettingsSource>()
                        {
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Janus Funds 403(b)",
                                SourceValue = "4JAN"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Standard Funds 403(b)",
                                SourceValue = "STAN"
                            }
                        },
                        FieldHelp = "You can specify one or more benefit or deduction codes to exclude...",
                        EntityName = "BENDED",
                        FieldName = "LDMD.EXCLUDE.BENEFITS"
                    },
                    new Domain.Base.Entities.CollectionConfigurationSettings("fb8bb3c4-71ca-4f94-9054-fbf2694a4a58", "4", "HR Status Codes Denoting Leave")
                    {
                       EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "employees",
                                PropertyName = "leave"
                            }
                        },
                        Source = new List<Domain.Base.Entities.CollectionConfigurationSettingsSource>()
                        {
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Probation",
                                SourceValue = "PR"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Temporary",
                                SourceValue = "TE"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Student Worker",
                                SourceValue = "SW"
                            }
                        },
                        FieldHelp = "You can specify one or more human resource status codes that indicate an employee...",
                        EntityName = "HR.VALCODES",
                        ValcodeTableName = "HR.STATUSES",
                        FieldName = "LDMD.LEAVE.STATUS.CODES"
                    },
                    new Domain.Base.Entities.CollectionConfigurationSettings("2333a672-6c2b-4dfd-9261-1e65983f75a8", "5", "Guardian Relation Types")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "person-guardians"
                            }
                        },
                        Source = new List<Domain.Base.Entities.CollectionConfigurationSettingsSource>()
                        {
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Guardian",
                                SourceValue = "GU"
                            },
                            new Domain.Base.Entities.CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Parent",
                                SourceValue = "P"
                            }
                        },
                        FieldHelp = "You can specify one or more relation types that are considered guardians...",
                        EntityName = "RELATION.TYPES",
                        FieldName = "LDMD.GUARDIAN.REL.TYPES"
                    }
                };

            foreach (var source in allCollectionConfigurationSettings)
            {
                var collectionConfigurationSettings = new CollectionConfigurationSettings
                {
                    Id = source.Guid,
                    Title = source.Description,
                    Description = source.FieldHelp
                };
                var collectionConfigurationSettingsEthos = new List<DefaultSettingsEthos>();
                foreach (var ethos in source.EthosResources)
                {
                    collectionConfigurationSettingsEthos.Add(new DefaultSettingsEthos()
                    {
                        Resource = ethos.Resource,
                        PropertyName = ethos.PropertyName
                    });
                }
                collectionConfigurationSettings.Ethos = collectionConfigurationSettingsEthos;

                var collectionConfigurationSettingsSource = new List<DefaultSettingsSource>();
                foreach (var sourceValue in source.Source)
                {
                    collectionConfigurationSettingsSource.Add(new DefaultSettingsSource()
                    {
                        Title = sourceValue.SourceTitle,
                        Value = sourceValue.SourceValue
                    });
                }
                collectionConfigurationSettings.Source = collectionConfigurationSettingsSource;

                collectionConfigurationSettingsCollection.Add(collectionConfigurationSettings);
            }

            collectionConfigurationSettingsController = new CollectionConfigurationSettingsController(collectionConfigurationSettingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            collectionConfigurationSettingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            collectionConfigurationSettingsController = null;
            allCollectionConfigurationSettings = null;
            collectionConfigurationSettingsCollection = null;
            loggerMock = null;
            collectionConfigurationSettingsServiceMock = null;
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_ValidateFields_Nocache()
        {
            collectionConfigurationSettingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false)).ReturnsAsync(collectionConfigurationSettingsCollection);

            var sourceContexts = (await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(collectionConfigurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = collectionConfigurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_ValidateFields_Cache()
        {
            collectionConfigurationSettingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), true)).ReturnsAsync(collectionConfigurationSettingsCollection);

            var sourceContexts = (await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(collectionConfigurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = collectionConfigurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_KeyNotFoundException()
        {
            //
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<KeyNotFoundException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_PermissionsException()
        {

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<PermissionsException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_ArgumentException()
        {

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<ArgumentException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_RepositoryException()
        {

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<RepositoryException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_IntegrationApiException()
        {

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false))
                .Throws<IntegrationApiException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuidAsync_ValidateFields()
        {
            var expected = collectionConfigurationSettingsCollection.FirstOrDefault();
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettings_Exception()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsAsync(It.IsAny<List<DefaultSettingsEthos>>(), false)).Throws<Exception>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuidAsync_Exception()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_KeyNotFoundException()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_PermissionsException()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_ArgumentException()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_RepositoryException()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_IntegrationApiException()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_GetCollectionConfigurationSettingsByGuid_Exception()
        {
            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await collectionConfigurationSettingsController.GetCollectionConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_PostCollectionConfigurationSettingsAsync_Exception()
        {
            await collectionConfigurationSettingsController.PostCollectionConfigurationSettingsAsync(collectionConfigurationSettingsCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsController_PutCollectionConfigurationSettingsAsync_ValidateFields()
        {
            var expected = collectionConfigurationSettingsCollection.ToArray().FirstOrDefault();
            collectionConfigurationSettingsServiceMock.Setup(x => x.UpdateCollectionConfigurationSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = collectionConfigurationSettingsCollection.FirstOrDefault();
            var actual = await collectionConfigurationSettingsController.PutCollectionConfigurationSettingsAsync(sourceContext.Id, sourceContext);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_PutCollectionConfigurationSettingsAsync_UpdateDesc()
        {

            var expected = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" } }
            };

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
           
            collectionConfigurationSettingsServiceMock.Setup(x => x.UpdateCollectionConfigurationSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "INVALID",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" } }
            };
            await collectionConfigurationSettingsController.PutCollectionConfigurationSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_PutCollectionConfigurationSettingsAsync_UpdateSourceTitle()
        {
            var expected = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" } }
            };

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            collectionConfigurationSettingsServiceMock.Setup(x => x.UpdateCollectionConfigurationSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "INVALID", Value = "G" } }
            };
            await collectionConfigurationSettingsController.PutCollectionConfigurationSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_PutCollectionConfigurationSettingsAsync_UpdateTitle()
        {
            var expected = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "Default Privacy Code",
                Description = "Long Description for field help.",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" } }
            };

            collectionConfigurationSettingsServiceMock.Setup(x => x.GetCollectionConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            collectionConfigurationSettingsServiceMock.Setup(x => x.UpdateCollectionConfigurationSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = new CollectionConfigurationSettings
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Title = "INVALID",
                Description = "Default Privacy Code",
                Ethos = new List<DefaultSettingsEthos>() { new DefaultSettingsEthos() { Resource = "persons", PropertyName = "privacyStatus" } },
                Source = new List<DefaultSettingsSource>() { new DefaultSettingsSource() { Title = "Don't Release Grades", Value = "G" } }
            };
            await collectionConfigurationSettingsController.PutCollectionConfigurationSettingsAsync(sourceContext.Id, sourceContext);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CollectionConfigurationSettingsController_DeleteCollectionConfigurationSettingsAsync_Exception()
        {
            await collectionConfigurationSettingsController.DeleteCollectionConfigurationSettingsAsync(collectionConfigurationSettingsCollection.FirstOrDefault().Id);
        }
    }
}