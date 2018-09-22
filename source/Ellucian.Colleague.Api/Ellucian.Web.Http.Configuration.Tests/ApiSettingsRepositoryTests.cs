// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Web.Http.Configuration.Tests
{
    [TestClass]
    public class ApiSettingsRepositoryTests
    {
        [TestClass]
        public class ApiSettingsRepositoryGetNames
        {
            public Mock<IColleagueTransactionFactory> transFactoryMock;
            public Mock<ICacheProvider> cacheProviderMock;
            public Mock<IColleagueDataReader> dataReaderMock;
            public Mock<ILogger> loggerMock;
            public Mock<IColleagueTransactionInvoker> transManagerMock;
            public UpdateWebAPIConfigRequest updateRequest;
            public ApiSettingsRepository apiSettingsRepo;
            public ApiSettings apiSettings;
            public string name;

            [TestInitialize]
            public void Initialize()
            {
                apiSettingsRepo = BuildValidApiSettingsRepository();
                name = "production";
            }

            [TestMethod]
            public void ReturnsListOfWebApiConfigNames()
            {
                var selectResponse = new string[] { "PRODUCTION", "TEST", "DEV" };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);

                var result = apiSettingsRepo.GetNames();

                for (int i = 0; i < selectResponse.Length; i++)
                {
                    Assert.AreEqual(selectResponse.ElementAt(i), result.ElementAt(i));
                }
            }

            [TestMethod]
            public void ReturnsEmptyListIfReaderResponseIsNull()
            {
                var selectResponse = new string[]{};
                selectResponse = null;
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);
                
                var result = apiSettingsRepo.GetNames();
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfSelectThrowsException()
            {
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidProgramException());
                var result = apiSettingsRepo.GetNames();
            }

            private ApiSettingsRepository BuildValidApiSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                // Logger mock
                loggerMock = new Mock<ILogger>();
                // Set up transaction manager for mocking 
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                return new ApiSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

        }

        [TestClass]
        public class ApiSettingsRepositoryGetSingle
        {
            public Mock<IColleagueTransactionFactory> transFactoryMock;
            public Mock<ICacheProvider> cacheProviderMock;
            public Mock<IColleagueDataReader> dataReaderMock;
            public Mock<IColleagueDataReader> anonymousDataReaderMock;
            public Mock<ILogger> loggerMock;
            public Mock<IColleagueTransactionInvoker> transManagerMock;
            public UpdateWebAPIConfigRequest updateRequest;
            public ApiSettingsRepository apiSettingsRepo;
            public ApiSettings apiSettings;
            public string name;

            [TestInitialize]
            public void Initialize()
            {
                apiSettingsRepo = BuildValidApiSettingsRepository();
                name = "production";
            }

            [TestMethod]
            public void ReturnsApiSettingsWithAllAttributes()
            {
                // Mock select that finds single matching item
                var selectResponse = new string[] { "1" };
                anonymousDataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);
                
                // Mock data contract read that brings in WebApiConfig object
                var webApiConfigResponse = BuildWebAPIConfigResponse();
                anonymousDataReaderMock.Setup(rdr => rdr.ReadRecord<DataContracts.WebApiConfig>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(webApiConfigResponse);

                // Arrange WEBAPI.CACHE.PROVIDERS and DEBUG.TRACE.LEVELS read mocks
                List<string> cacheProviderCodes = new List<string>() { "INPROC" };
                List<string> cacheProviderDescs = new List<string>() { "In-process Caching (Mock)" };
                List<string> traceLevelCodes = new List<string>() { "OFF", "ERROR", "WARNING", "INFO", "VERBOSE" };
                List<string> traceLevelDescs = new List<string>() { "Off (Mock)", "Error (Mock)", "Warning (Mock)", "Info (Mock)", "Verbose (Mock)" };

                // Set some real values for the mock read
                var cacheProvidersResponse = new ApplValcodes() { ValInternalCode = cacheProviderCodes, ValExternalRepresentation = cacheProviderDescs };
                anonymousDataReaderMock.Setup(rdr => rdr.ReadRecord<ApplValcodes>("UT.VALCODES", "WEBAPI.CACHE.PROVIDERS", true)).Returns(cacheProvidersResponse);
                var debugTraceLevelsResponse = new ApplValcodes() { ValInternalCode = traceLevelCodes, ValExternalRepresentation = traceLevelDescs };
                anonymousDataReaderMock.Setup(rdr => rdr.ReadRecord<ApplValcodes>("UT.VALCODES", "DEBUG.TRACE.LEVELS", true)).Returns(debugTraceLevelsResponse);
                
                // Arrange time zone read mocks
                var timeZoneSettingsResponse = new DataContracts.TimeZoneSettings() { ColleagueTimeZone = "PACIFIC_STANDARD_TIME" };
                anonymousDataReaderMock.Setup(rdr => rdr.ReadRecord<DataContracts.TimeZoneSettings>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(timeZoneSettingsResponse);
                var timeZonesResponse = new DataContracts.TimeZones() { Recordkey = "PACIFIC_STANDARD_TIME", TimeZonesStandardName = "Pacific Standard Time" };
                anonymousDataReaderMock.Setup(rdr => rdr.ReadRecord<DataContracts.TimeZones>(It.IsAny<string>(), true)).Returns(timeZonesResponse);
                
                var result = apiSettingsRepo.Get(name);

                Assert.AreEqual(int.Parse(webApiConfigResponse.Recordkey), result.Id);
                Assert.AreEqual(webApiConfigResponse.WacConfigurationName, result.Name);
                Assert.AreEqual(webApiConfigResponse.WacPhotoType, result.PhotoType);
                Assert.AreEqual(webApiConfigResponse.WacPhotoUrl, result.PhotoURL);
                Assert.AreEqual(webApiConfigResponse.WacLogoPath, result.ReportLogoPath);
                Assert.AreEqual(webApiConfigResponse.WacUnofficlWatermarkPath, result.UnofficialWatermarkPath);
                for (int i = 0; i < webApiConfigResponse.WebApiConfigPhotoHdrEntityAssociation.Count; i++)
                {
                    var nameKey = webApiConfigResponse.WebApiConfigPhotoHdrEntityAssociation[i].WacPhotoHeaderNameAssocMember;
                    Assert.IsTrue(result.PhotoHeaders.ContainsKey(nameKey));
                    Assert.AreEqual(webApiConfigResponse.WebApiConfigPhotoHdrEntityAssociation[i].WacPhotoHeaderValueAssocMember, result.PhotoHeaders[nameKey]);
                }
                Assert.AreEqual(timeZonesResponse.TimeZonesStandardName, result.ColleagueTimeZone);
                Assert.AreEqual(cacheProvidersResponse.ValInternalCode.Count, result.SupportedCacheProviders.Count);
                for (int i = 0; i < cacheProvidersResponse.ValInternalCode.Count; i++)
                {
                    var resultDescription = result.SupportedCacheProviders[i].Key;
                    Assert.AreEqual(cacheProvidersResponse.ValExternalRepresentation[i], resultDescription);
                }
                Assert.AreEqual(debugTraceLevelsResponse.ValInternalCode.Count, result.DebugTraceLevels.Count);
                for (int i = 0; i < debugTraceLevelsResponse.ValInternalCode.Count; i++)
                {
                    var resultDescription = result.DebugTraceLevels[i].Key;
                    Assert.AreEqual(debugTraceLevelsResponse.ValExternalRepresentation[i], resultDescription);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfNameArgumentIsNull()
            {
                ApiSettings result = apiSettingsRepo.Get(null);    
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfNameArgumentIsEmpty()
            {
                ApiSettings result = apiSettingsRepo.Get(string.Empty); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ThrowsExceptionIfNoneFoundWithSpecifiedName()
            {
                // Mock select that finds no matching item
                var selectResponse = new string[] { };
                anonymousDataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);
                var result = apiSettingsRepo.Get("nonexistent name");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfSelectThrowsException()
            {
                // Mock select that finds no matching item
                var selectResponse = new string[] { };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidProgramException());

                var result = apiSettingsRepo.Get("nonexistent name");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfNullRecordRead()
            {
                // Mock select that finds a matching item
                var selectResponse = new string[] { "1" };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);
                // Mock read that comes back with an empty response
                var webApiConfigResponse = new DataContracts.WebApiConfig();
                webApiConfigResponse = null;
                dataReaderMock.Setup(rdr => rdr.ReadRecord<DataContracts.WebApiConfig>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(webApiConfigResponse);
                var result = apiSettingsRepo.Get(name);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfReadThrowsException()
            {
                // Mock select that finds a matching item
                var selectResponse = new string[] { "1" };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);
                // Mock read that comes back with an empty response
                var webApiConfigResponse = new DataContracts.WebApiConfig();
                webApiConfigResponse = null;
                dataReaderMock.Setup(rdr => rdr.ReadRecord<DataContracts.WebApiConfig>(It.IsAny<string>(), It.IsAny<string>(), true)).Throws(new InvalidProgramException());
                var result = apiSettingsRepo.Get(name);
            }

            private ApiSettingsRepository BuildValidApiSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                anonymousDataReaderMock = dataReaderMock = new Mock<IColleagueDataReader>();
                // Logger mock
                loggerMock = new Mock<ILogger>();
                // Set up transaction manager for mocking 
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetDataReader(true)).Returns(anonymousDataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                return new ApiSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            private DataContracts.WebApiConfig BuildWebAPIConfigResponse()
            {
                var apiConfig = new DataContracts.WebApiConfig()
                {
                    Recordkey = "1",
                    WacConfigurationName = "Production",
                    WacVersion = "3",
                    WacPhotoType = "image",
                    WacPhotoUrl = "/my/photo/url.com",
                    WacLogoPath = "~/images/image.png",
                    WacUnofficlWatermarkPath = "~/images/watermark.png",
                    WebApiConfigPhotoHdrEntityAssociation = new List<DataContracts.WebApiConfigWebApiConfigPhotoHdr>(),
                };
                apiConfig.WacPhotoHeaderName = new List<string>() { "header1", "header2" };
                apiConfig.WacPhotoHeaderValue = new List<string>() { "value1", "value2" };

                apiConfig.buildAssociations();

                return apiConfig;
            }
        }

        [TestClass]
        public class ApiSettingsRepositoryUpdate
        {
            public Mock<IColleagueTransactionFactory> transFactoryMock;
            public Mock<ICacheProvider> cacheProviderMock;
            public Mock<IColleagueDataReader> dataReaderMock;
            public Mock<ILogger> loggerMock;
            public Mock<IColleagueTransactionInvoker> transManagerMock;
            public UpdateWebAPIConfigRequest updateRequest;
            public ApiSettingsRepository apiSettingsRepo;
            public ApiSettings apiSettings;
            public string name;

            [TestInitialize]
            public void Initialize()
            {
                apiSettingsRepo = BuildValidApiSettingsRepository();
                name = "production";

                apiSettings = new ApiSettings(name);
            }

            [TestMethod]
            public void SuccessfulRequestReturnsTrueResponse()
            {
                // Successful data update response
                var response = new UpdateWebAPIConfigResponse() { ErrorMessage = "", UpdateSuccessful = true };
                transManagerMock.Setup(mgr => mgr.Execute<UpdateWebAPIConfigRequest, UpdateWebAPIConfigResponse>(It.IsAny<UpdateWebAPIConfigRequest>()))
                    .Returns(response);
                var result = apiSettingsRepo.Update(apiSettings);
                Assert.AreEqual(true, result);
            }

            [TestMethod]
            public void RequestIsBuiltCorrectly()
            {
                // Build full ApiSettings object
                apiSettings = new ApiSettings(3, name, 7)
                {
                    PhotoType = "phototype",
                    PhotoURL = "/this/is/the/photo/url.com",
                };
                apiSettings.PhotoHeaders = new Dictionary<string, string>();
                apiSettings.PhotoHeaders.Add("header1", "value1");
                apiSettings.PhotoHeaders.Add("header2", "value2");

                // Successful data update response
                var response = new UpdateWebAPIConfigResponse() { ErrorMessage = "", UpdateSuccessful = true };
                transManagerMock.Setup(mgr => mgr.Execute<UpdateWebAPIConfigRequest, UpdateWebAPIConfigResponse>(It.IsAny<UpdateWebAPIConfigRequest>()))
                    .Returns(response)
                    .Callback<UpdateWebAPIConfigRequest>(req => updateRequest = req);
                var result = apiSettingsRepo.Update(apiSettings);

                // Assert each field in the request is set to the original value from ApiSettings.
                Assert.AreEqual(apiSettings.Id.ToString(), updateRequest.ApiConfigId);
                Assert.AreEqual(apiSettings.Name, updateRequest.ConfigurationName);
                Assert.AreEqual(apiSettings.Version, updateRequest.Version);
                Assert.AreEqual(2, updateRequest.PhotoHeaders.Count);
                foreach (var item in updateRequest.PhotoHeaders)
                {
                    Assert.AreEqual(apiSettings.PhotoHeaders[item.PhotoHeaderName], item.PhotoHeaderValue);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ThrowsExceptionIfNameOfNewConfigurationAlreadyExists()
            {
                // Not found response for datareader select based on name
                var response = new string[] { "1" };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(response);
                var result = apiSettingsRepo.Update(apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfSelectThrowsException()
            {
                // Not found response for datareader select based on name
                var response = new string[] { "1" };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidProgramException());
                var result = apiSettingsRepo.Update(apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsErrorIfResponseIsNull()
            {
                // Nothing is mocked so response will be null
                var result = apiSettingsRepo.Update(apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsErrorIfResponseReturnsErrorMessage()
            {
                var response = new UpdateWebAPIConfigResponse() { ErrorMessage = "Could not update for whatever reason", UpdateSuccessful = true };
                transManagerMock.Setup(mgr => mgr.Execute<UpdateWebAPIConfigRequest, UpdateWebAPIConfigResponse>(It.IsAny<UpdateWebAPIConfigRequest>()))
                    .Returns(response);
                var result = apiSettingsRepo.Update(apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsErrorIfResponseReturnsFalseWithNoErrorMessage()
            {
                var response = new UpdateWebAPIConfigResponse() { ErrorMessage = "", UpdateSuccessful = false };
                transManagerMock.Setup(mgr => mgr.Execute<UpdateWebAPIConfigRequest, UpdateWebAPIConfigResponse>(It.IsAny<UpdateWebAPIConfigRequest>()))
                    .Returns(response);
                var result = apiSettingsRepo.Update(apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ThrowsExceptionIfTransactionThrowsException()
            {
                var response = new UpdateWebAPIConfigResponse() { ErrorMessage = "", UpdateSuccessful = false };
                transManagerMock.Setup(mgr => mgr.Execute<UpdateWebAPIConfigRequest, UpdateWebAPIConfigResponse>(It.IsAny<UpdateWebAPIConfigRequest>()))
                    .Throws(new InvalidProgramException());
                var result = apiSettingsRepo.Update(apiSettings);
            }

            private ApiSettingsRepository BuildValidApiSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                // Logger mock
                loggerMock = new Mock<ILogger>();
                // Set up transaction manager for mocking 
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Not found response for datareader select based on name
                var selectResponse = new string[] { };
                dataReaderMock.Setup(rdr => rdr.Select(It.IsAny<string>(), It.IsAny<string>())).Returns(selectResponse);

                return new ApiSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }
    }
}
