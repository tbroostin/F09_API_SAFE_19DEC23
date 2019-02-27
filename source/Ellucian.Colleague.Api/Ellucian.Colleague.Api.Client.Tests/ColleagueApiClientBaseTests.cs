// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientBaseTests
    {
        #region Constants

        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _studentId = "123456";
        private const string _studentId2 = "678";
        private const string _personId = "0003914";
        private const string _token = "1234567890";
        private const string _courseId = "MATH-100";
        private const string _courseId2 = "ENGL-101";

        private List<CorrespondenceRequest> expectedCorrespondenceRequestsResponse = new List<CorrespondenceRequest>()
            {
                new CorrespondenceRequest()
                {
                    PersonId = _personId,
                    Code ="Doc1",
                    StatusDescription = "Complete",
                    Status = CorrespondenceRequestStatus.Received
                },

                new CorrespondenceRequest()
                {
                    PersonId = _personId,
                    Code ="Doc2",
                    StatusDescription = "Waived",
                    Status = CorrespondenceRequestStatus.Waived
                }
            };

        #endregion
        
        private ColleagueApiClient client;
        private MockHandler mockHandler;
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;

            _logger = _loggerMock.Object;

            mockHandler = new MockHandler();
        }

        #region Version Tests

        [TestMethod]
        public void GetVersion()
        {
            // Arrange
            ApiVersion version = new ApiVersion() { ProductVersion = "1.1.0" };
            string serializedResponse = JsonConvert.SerializeObject(version).ToString();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            ApiVersion result = client.GetVersion();

            // Assert
            Assert.AreEqual(version.ProductVersion, result.ProductVersion);
        }

        [TestMethod]
        public async Task GetVersionAsync()
        {
            // Arrange
            ApiVersion version = new ApiVersion() { ProductVersion = "1.1.0" };
            string serializedResponse = JsonConvert.SerializeObject(version).ToString();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            ApiVersion result = await client.GetVersionAsync();

            // Assert
            Assert.AreEqual(version.ProductVersion, result.ProductVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestFailedException))]
        public void GetVersion_BadRequest()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act

            var result = client.GetVersion();

        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestFailedException))]
        public async Task GetVersionAsync_BadRequest()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.GetVersionAsync();
        }

        #endregion

        #region CommunicationCodeTests

        public FunctionEqualityComparer<CommunicationCode2> communicationCode2DtoComparer = new FunctionEqualityComparer<CommunicationCode2>(
            (c1, c2) => c1.Code == c2.Code && c1.AwardYear == c2.AwardYear && c1.Hyperlinks.Count() == c2.Hyperlinks.Count(),
            (c) => c.Code.GetHashCode() ^ c.AwardYear.GetHashCode());

        [TestMethod]
        public void GetCommunicationCodes2Test()
        {
            var communicationCodes = new List<CommunicationCode2>()
            {
                new CommunicationCode2()
                {
                    Code = "CODE1",
                    Description = "Description 1",
                    AwardYear = "",
                    Explanation = "Explanation 1",
                    IsStudentViewable = true,
                    OfficeCodeId = "AR",
                    Hyperlinks = new List<CommunicationCodeHyperlink>()
                    {
                        new CommunicationCodeHyperlink() {Url = "www.ellucian.com", Title = "Ellucian"},
                        new CommunicationCodeHyperlink() {Url = "luci/", Title = "Ellucian Internal"}
                    }
                },
                new CommunicationCode2()
                {
                    Code = "CODE2",
                    Description = "Description 2",
                    AwardYear = "2014",
                    Explanation = "Explanation 2",
                    IsStudentViewable = false,
                    OfficeCodeId = "FA",
                    Hyperlinks = new List<CommunicationCodeHyperlink>()
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(communicationCodes);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            var actualResult = client.GetCommunicationCodes2();

            CollectionAssert.AreEqual(communicationCodes, actualResult.ToList(), communicationCode2DtoComparer);
        }

        #endregion

        #region GetProfileTests

        [TestClass]
        public class GetProfileTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetProfile()
            {
                // Arrange
                var profile = new Dtos.Base.Profile()
                {
                    Id = "0000012",
                    LastName = "Brown",
                    FirstName = "Janie",
                    PreferredEmailAddress = "jbrown-999@ellucianx.com",
                    AddressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3)),
                    EmailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3)),
                    PhoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3)),
                };

                var serializedResponse = JsonConvert.SerializeObject(profile);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetProfile("0000012");

                // Assert that the expected item is found in the response
                Assert.AreEqual(profile.Id, clientResponse.Id);
                Assert.AreEqual(profile.LastName, clientResponse.LastName);
                Assert.AreEqual(profile.FirstName, clientResponse.FirstName);
                Assert.AreEqual(profile.PreferredEmailAddress, clientResponse.PreferredEmailAddress);
                Assert.AreEqual(profile.AddressConfirmationDateTime, clientResponse.AddressConfirmationDateTime);
                Assert.AreEqual(profile.EmailAddressConfirmationDateTime, clientResponse.EmailAddressConfirmationDateTime);
                Assert.AreEqual(profile.PhoneConfirmationDateTime, clientResponse.PhoneConfirmationDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetProfile_ThrowsExceptionForNullPersonId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetProfile(null);
            }
        }

        #endregion

        #region UpdateProfileTests

        [TestClass]
        public class UpdateProfileTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_UpdateProfile_ReturnsUpdatedProfile()
            {

                var profile = new Dtos.Base.Profile()
                {
                    Id = "0000012",
                    LastName = "Brown",
                    FirstName = "Janie",
                    PreferredEmailAddress = "jbrown-999@ellucianx.com",
                    AddressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3)),
                    EmailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3)),
                    PhoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3)),
                };

                var serializedResponse = JsonConvert.SerializeObject(profile);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                var clientResponse = await client.UpdateProfileAsync(profile);

                Assert.AreEqual(profile.Id, clientResponse.Id);
                Assert.AreEqual(profile.LastName, clientResponse.LastName);
                Assert.AreEqual(profile.FirstName, clientResponse.FirstName);
                Assert.AreEqual(profile.PreferredEmailAddress, clientResponse.PreferredEmailAddress);
                Assert.AreEqual(profile.EmailAddressConfirmationDateTime, clientResponse.EmailAddressConfirmationDateTime);
                Assert.AreEqual(profile.AddressConfirmationDateTime, clientResponse.AddressConfirmationDateTime);
                Assert.AreEqual(profile.PhoneConfirmationDateTime, clientResponse.PhoneConfirmationDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_UpdateProfile_WithNullProfile_ShouldThrow()
            {
                Dtos.Base.Profile profile = null;
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                var clientResponse = await client.UpdateProfileAsync(profile);

            }
        }

        #endregion

        #region GetEmergencyInformationTests

        [TestClass]
        public class GetEmergencyInformationTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetEmergencyInformation()
            {
                // Arrange
                var emergencyInformation = new Dtos.Base.EmergencyInformation()
                {
                    PersonId = "0000012",
                    HospitalPreference = "Memorial",
                    InsuranceInformation = "BCBS"
                };

                var serializedResponse = JsonConvert.SerializeObject(emergencyInformation);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetPersonEmergencyInformationAsync("0000012");

                // Assert that the expected item is found in the response
                Assert.AreEqual(emergencyInformation.PersonId, clientResponse.PersonId);
                Assert.AreEqual(emergencyInformation.HospitalPreference, clientResponse.HospitalPreference);
                Assert.AreEqual(emergencyInformation.InsuranceInformation, clientResponse.InsuranceInformation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetEmergencyInformation_ThrowsExceptionForNullPersonId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetPersonEmergencyInformationAsync(null);
            }

        }

        #endregion

        #region UpdateEmergencyInformationTests
        [TestClass]
        public class UpdateEmergencyInformationTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_UpdateEmergencyInformation()
            {
                // Arrange
                var emergencyInformation = new Dtos.Base.EmergencyInformation()
                {
                    PersonId = "0000012",
                    HospitalPreference = "Memorial",
                    InsuranceInformation = "BCBS"
                };

                var serializedResponse = JsonConvert.SerializeObject(emergencyInformation);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.UpdatePersonEmergencyInformation(emergencyInformation);

                // Assert that the expected item is found in the response
                Assert.AreEqual(emergencyInformation.PersonId, clientResponse.PersonId);
                Assert.AreEqual(emergencyInformation.HospitalPreference, clientResponse.HospitalPreference);
                Assert.AreEqual(emergencyInformation.InsuranceInformation, clientResponse.InsuranceInformation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_UpdateEmergencyInformation_ThrowsExceptionForNullInput()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.UpdatePersonEmergencyInformation(null);
            }
        }

        #endregion

        #region GetCountriesAsync

        [TestClass]
        public class GetCountriesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }


            [TestMethod]
            public async Task ClientGetCountriesAsync_ReturnsSerializedCountries()
            {
                // Arrange
                var countries = new List<Country>();
                countries.Add(new Country() { Code = "USA", Description = "United States", IsoCode = "IsoCode1" });
                countries.Add(new Country() { Code = "CA", Description = "Canada", IsoCode = "IsoCode2" });

                var serializedResponse = JsonConvert.SerializeObject(countries);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetCountriesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(countries.Count(), clientResponse.Count());
                foreach (var country in countries)
                {
                    var countryResult = clientResponse.Where(c => c.Code == country.Code).FirstOrDefault();
                    Assert.IsNotNull(countryResult);
                    Assert.AreEqual(country.Description, countryResult.Description);
                    Assert.AreEqual(country.IsoCode, countryResult.IsoCode);
                }
            }

        }
        #endregion

        #region GetCommencementSitesAsync
        [TestMethod]
        public async Task ClientGetCommencementSitesAsync_ReturnsSerializedCommencementSite()
        {
            // Arrange
            var commencementSites = new List<CommencementSite>();
            commencementSites.Add(new CommencementSite() { Code = "PATC", Description = "Patriot Center" });
            commencementSites.Add(new CommencementSite() { Code = "OTHER", Description = "Other location" });




            var serializedResponse = JsonConvert.SerializeObject(commencementSites);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);


            var clientResponse = await client.GetCommencementSitesAsync();

            Assert.IsNotNull(clientResponse);
            Assert.AreEqual(commencementSites.Count(), clientResponse.Count());
            foreach (var site in commencementSites)
            {
                Assert.IsNotNull(site.Code);
                Assert.IsNotNull(site.Description);
                var x = clientResponse.Where(c => c.Code == site.Code).FirstOrDefault();
                Assert.AreEqual(x.Code, site.Code);
                Assert.AreEqual(x.Description, site.Description);

            }

        }

        #endregion

        #region GetStatesAsync

        [TestClass]
        public class GetStatesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }


            [TestMethod]
            public async Task ClientGetStatesAsync_ReturnsSerializedStates()
            {
                // Arrange
                var states = new List<State>();
                states.Add(new State() { Code = "VA", Description = "Virginia" });
                states.Add(new State() { Code = "TX", Description = "Texas" });

                var serializedResponse = JsonConvert.SerializeObject(states);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStatesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(states.Count(), clientResponse.Count());
                foreach (var state in states)
                {
                    var stateResult = clientResponse.Where(c => c.Code == state.Code).FirstOrDefault();
                    Assert.IsNotNull(stateResult);
                    Assert.AreEqual(state.Description, stateResult.Description);
                }
            }

        }
        #endregion

        #region GetBankingInformationConfigurationTests

        [TestMethod]
        public async Task BankingInformationConfiguration_GetAsync_GetTest()
        {
            var responseObj = new BankingInformationConfiguration() { AddEditAccountTermsAndConditions = "These are the terms" };
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(responseObj), Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetBankingInformationConfigurationAsync();

            Assert.AreEqual(responseObj.AddEditAccountTermsAndConditions, actualResult.AddEditAccountTermsAndConditions);
        }

        #endregion

        #region GetBanksTests
        [TestMethod]
        public async Task Banks_GetBankAsync_GetTest()
        {
            var responseUsBank = new Domain.Base.Entities.Bank("011000015", "Federal Resrve Bank", "011000015");
            //var responseCaBank = new Domain.Base.Entities.Bank("BANK OF MONTREAL", "001");

            var serializedResponse = JsonConvert.SerializeObject(responseUsBank);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetBankAsync("id");

            Assert.IsInstanceOfType(actualResult, typeof(Bank));
            Assert.AreEqual(responseUsBank.Id, actualResult.Id);
            Assert.AreEqual(responseUsBank.Name, actualResult.Name);
        }

        #endregion

        #region GetRelationshipTypesAsyncTests
        [TestClass]
        public class GetRelationshipTypesAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<RelationshipType> relTypes;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                relTypes = new List<Dtos.Base.RelationshipType>()
                    {
                        new RelationshipType() { Code = "P", Description = "Parent", InverseCode = "C" },
                        new RelationshipType() { Code = "C", Description = "Child", InverseCode = "P"}
                    };
            }

            [TestMethod]
            public async Task Client_GetRelationshipTypesAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(relTypes);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetRelationshipTypesAsync();

                // Assert that the expected item is found in the response
                Assert.AreEqual(relTypes.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < relTypes.Count; i++)
                {
                    Assert.AreEqual(relTypes[i].Code, respList[i].Code);
                    Assert.AreEqual(relTypes[i].Description, respList[i].Description);
                    Assert.AreEqual(relTypes[i].InverseCode, respList[i].InverseCode);
                }
            }
        }

        #endregion

        #region GetPersonPrimaryRelationshipsAsyncTests
        [TestClass]
        public class GetPersonPrimaryRelationshipsAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<Relationship> rels;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                rels = new List<Dtos.Base.Relationship>()
                    {
                        new Relationship() { EndDate = DateTime.MaxValue, IsActive = true, IsPrimaryRelationship = true, OtherEntity = "0001234", PrimaryEntity = "0001235", RelationshipType = "P", StartDate = DateTime.MinValue },
                        new Relationship() { EndDate = DateTime.Today.AddDays(-3), IsActive = false, IsPrimaryRelationship = true, OtherEntity = "0001236", PrimaryEntity = "0001235", RelationshipType = "C", StartDate = DateTime.MinValue },
                    };
            }

            [TestMethod]
            public async Task Client_GetPersonPrimaryRelationshipsAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(rels);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetPersonPrimaryRelationshipsAsync("0001235");

                // Assert that the expected item is found in the response
                Assert.AreEqual(rels.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < rels.Count; i++)
                {
                    Assert.AreEqual(rels[i].EndDate, respList[i].EndDate);
                    Assert.AreEqual(rels[i].IsActive, respList[i].IsActive);
                    Assert.AreEqual(rels[i].IsPrimaryRelationship, respList[i].IsPrimaryRelationship);
                    Assert.AreEqual(rels[i].OtherEntity, respList[i].OtherEntity);
                    Assert.AreEqual(rels[i].PrimaryEntity, respList[i].PrimaryEntity);
                    Assert.AreEqual(rels[i].RelationshipType, respList[i].RelationshipType);
                    Assert.AreEqual(rels[i].StartDate, respList[i].StartDate);
                }
            }
        }
        #endregion

        #region GetUserProxyPermissionsAsyncTests
        [TestClass]
        public class GetUserProxyPermissionsAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<ProxyUser> users;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                users = new List<Dtos.Base.ProxyUser>()
                    {
                        new ProxyUser() { Id = "0003316", EffectiveDate = DateTime.Today, Permissions = new List<Dtos.Base.ProxyAccessPermission>() { new Dtos.Base.ProxyAccessPermission() { Id = "1", IsGranted = true, ProxySubjectId = "0003315", ProxyUserId = "0003316", ProxyWorkflowCode = "SFAA", StartDate = DateTime.Today}}},
                        new ProxyUser() { Id = "0004000", EffectiveDate = DateTime.Today, Permissions = new List<Dtos.Base.ProxyAccessPermission>() { new Dtos.Base.ProxyAccessPermission() { Id = "1", IsGranted = true, ProxySubjectId = "0003315", ProxyUserId = "0004000", ProxyWorkflowCode = "SFMAP", StartDate = DateTime.Today}}}
                    };
            }

            [TestMethod]
            public async Task Client_GetUserProxyPermissionsAsyncTests()
            {
                var serializedResponse = JsonConvert.SerializeObject(users);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetUserProxyPermissionsAsync("0001235");

                // Assert that the expected item is found in the response
                Assert.AreEqual(users.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < users.Count; i++)
                {
                    Assert.AreEqual(users[i].Id, respList[i].Id);
                    Assert.AreEqual(users[i].Permissions.Count(), respList[i].Permissions.Count());
                }
            }
        }
        #endregion

        #region GetUserProxySubjectsAsyncTests

        [TestClass]
        public class GetUserProxySubjectsAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<ProxySubject> proxySubjects;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                proxySubjects = new List<Dtos.Base.ProxySubject>()
                {
                    new ProxySubject() { Id = "0003316", FullName = "Jane Doe", EffectiveDate = DateTime.Today, Permissions = new List<Dtos.Base.ProxyAccessPermission>() { new Dtos.Base.ProxyAccessPermission() { Id = "1", IsGranted = true, ProxySubjectId = "0003315", ProxyUserId = "0003316", ProxyWorkflowCode = "SFAA", StartDate = DateTime.Today}}},
                    new ProxySubject() { Id = "0004000", FullName = "John Doe Jr.", EffectiveDate = DateTime.Today, Permissions = new List<Dtos.Base.ProxyAccessPermission>() { new Dtos.Base.ProxyAccessPermission() { Id = "1", IsGranted = true, ProxySubjectId = "0003315", ProxyUserId = "0004000", ProxyWorkflowCode = "SFMAP", StartDate = DateTime.Today}}}
                };
            }

            [TestMethod]
            public async Task Client_GetUserProxySubjectsAsyncTests()
            {
                var serializedResponse = JsonConvert.SerializeObject(proxySubjects);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetUserProxySubjectsAsync("0001235");

                // Assert that the expected item is found in the response
                Assert.AreEqual(proxySubjects.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < proxySubjects.Count; i++)
                {
                    Assert.AreEqual(proxySubjects[i].Id, respList[i].Id);
                    Assert.AreEqual(proxySubjects[i].FullName, respList[i].FullName);
                    Assert.AreEqual(proxySubjects[i].EffectiveDate, respList[i].EffectiveDate);
                    Assert.AreEqual(proxySubjects[i].Permissions.Count(), respList[i].Permissions.Count());
                }
            }
        }

        #endregion

        #region QueryPersonMatchResultsByPostAsyncTests
        [TestClass]
        public class QueryPersonMatchResultsByPostAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private PersonMatchCriteria criteria;
            private List<PersonMatchResult> results;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                criteria = new Dtos.Base.PersonMatchCriteria()
                {
                    MatchCriteriaIdentifier = "PROXY.PERSON",
                    MatchNames = new List<Dtos.Base.PersonName>()
                    {
                        new Dtos.Base.PersonName() { GivenName = "given", FamilyName = "family" }
                    }
                };

                results = new List<Dtos.Base.PersonMatchResult>()
                {
                    new Dtos.Base.PersonMatchResult() { MatchCategory = Dtos.Base.PersonMatchCategoryType.Potential, MatchScore = 60, PersonId = "0003315" },
                    new Dtos.Base.PersonMatchResult() { MatchCategory = Dtos.Base.PersonMatchCategoryType.Potential, MatchScore = 50, PersonId = "0003315" },
                };

            }

            [TestMethod]
            public async Task Client_QueryPersonMatchResultsByPostAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(results);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryPersonMatchResultsByPostAsync(criteria);

                // Assert that the expected item is found in the response
                Assert.AreEqual(results.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < results.Count; i++)
                {
                    Assert.AreEqual(results[i].MatchCategory, respList[i].MatchCategory);
                    Assert.AreEqual(results[i].MatchScore, respList[i].MatchScore);
                    Assert.AreEqual(results[i].PersonId, respList[i].PersonId);
                }
            }
        }

        #endregion

        #region PostProxyCandidateAsyncTests
        [TestClass]
        public class PostProxyCandidateAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private ProxyCandidate candidate;
            private ProxyCandidate result;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                candidate = new Dtos.Base.ProxyCandidate()
                {
                    BirthDate = System.DateTime.Today.AddYears(-19),
                    EmailAddress = "test.person@ellucian.edu",
                    EmailType = "PRI",
                    FirstName = "Test",
                    LastName = "Person",
                    Gender = "M",
                    GrantedPermissions = new List<string>()
                    {
                        "SFAA"
                    },
                    Id = "0001234",
                    ProxyMatchResults = new List<Dtos.Base.PersonMatchResult>()
                    {
                        new Dtos.Base.PersonMatchResult()
                        {
                            MatchCategory = Dtos.Base.PersonMatchCategoryType.Potential,
                            MatchScore = 50,
                            PersonId = "0001235"
                        }
                    },
                    RelationType = "PX"
                };

                result = candidate;
                result.Id = "1";
            }

            [TestMethod]
            public async Task Client_PostProxyCandidateAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(result);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.PostProxyCandidateAsync(candidate);

                // Assert that the expected item is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(result.BirthDate, clientResponse.BirthDate);
                Assert.AreEqual(result.EmailAddress, clientResponse.EmailAddress);
                Assert.AreEqual(result.EmailType, clientResponse.EmailType);
                Assert.AreEqual(result.FirstName, clientResponse.FirstName);
                Assert.AreEqual(result.FormerFirstName, clientResponse.FormerFirstName);
                Assert.AreEqual(result.FormerLastName, clientResponse.FormerLastName);
                Assert.AreEqual(result.FormerMiddleName, clientResponse.FormerMiddleName);
                Assert.AreEqual(result.Gender, clientResponse.Gender);
                CollectionAssert.AreEqual(result.GrantedPermissions.ToList(), clientResponse.GrantedPermissions.ToList());
                Assert.AreEqual(result.LastName, clientResponse.LastName);
                Assert.AreEqual(result.MiddleName, clientResponse.MiddleName);
                Assert.AreEqual(result.Phone, clientResponse.Phone);
                Assert.AreEqual(result.PhoneExtension, clientResponse.PhoneExtension);
                Assert.AreEqual(result.PhoneType, clientResponse.PhoneType);
                Assert.AreEqual(result.Prefix, clientResponse.Prefix);
                Assert.AreEqual(result.Id, clientResponse.Id);
                Assert.AreEqual(result.ProxySubject, clientResponse.ProxySubject);
                Assert.AreEqual(result.RelationType, clientResponse.RelationType);
                Assert.AreEqual(result.GovernmentId, clientResponse.GovernmentId);
                Assert.AreEqual(result.Suffix, clientResponse.Suffix);
            }
        }

        #endregion

        #region GetUserProxyCandidatesAsync

        [TestClass]
        public class GetUserProxyCandidatesAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<ProxyCandidate> proxyCandidates;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                proxyCandidates = new List<Dtos.Base.ProxyCandidate>()
                {
                    new ProxyCandidate() { EmailAddress = "abc@123.com", FirstName = "AB", LastName = "CD", RelationType = "P", GrantedPermissions = new List<string>() { "SFAA" } },
                    new ProxyCandidate() { EmailAddress = "efg@456.com", FirstName = "EF", LastName = "GH", RelationType = "C", GrantedPermissions = new List<string>() { "SFMAP" } },
                };
            }

            [TestMethod]
            public async Task Client_GetUserProxyCandidatesAsyncTests()
            {
                var serializedResponse = JsonConvert.SerializeObject(proxyCandidates);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetUserProxyCandidatesAsync("0001235");

                // Assert that the expected item is found in the response
                Assert.AreEqual(proxyCandidates.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < proxyCandidates.Count; i++)
                {
                    Assert.AreEqual(proxyCandidates[i].EmailAddress, respList[i].EmailAddress);
                    Assert.AreEqual(proxyCandidates[i].FirstName, respList[i].FirstName);
                    Assert.AreEqual(proxyCandidates[i].LastName, respList[i].LastName);
                    Assert.AreEqual(proxyCandidates[i].GrantedPermissions.Count(), respList[i].GrantedPermissions.Count());
                }
            }
        }
        #endregion

        #region PostProxyUserAsync

        [TestClass]
        public class PostProxyUserAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private PersonProxyUser inPerson;
            private PersonProxyUser outPerson;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                inPerson = new PersonProxyUser()
                {
                    BirthDate = DateTime.Now.Date.AddDays(-1),
                    EmailAddresses = new List<EmailAddress>()
                    {
                        new EmailAddress(){Value = "mail1@mail.com", TypeCode = "PRI",},
                        new EmailAddress(){Value = "mail2@mail.com", TypeCode = "BUS",},
                    },
                    FirstName = "First",
                    FormerNames = new List<PersonName>()
                    {
                        new PersonName(){GivenName = "given1", FamilyName = "Family1", MiddleName = "Middle1",},
                        new PersonName(){GivenName = "given2", FamilyName = "Family2", MiddleName = "Middle2",},
                    },
                    Gender = "M",
                    GovernmentId = "123456789",
                    Id = "",
                    LastName = "Last",
                    MiddleName = "Middle",
                    Phones = new List<Phone>()
                    {
                        new Phone(){Extension = "Ext1", Number = "phone1", TypeCode = "HO",},
                        new Phone(){Extension = "Ext2", Number = "phone2", TypeCode = "HO",},
                    },
                    Prefix = "MR",
                    Suffix = "JR",
                };
                outPerson = new PersonProxyUser()
                {
                    BirthDate = inPerson.BirthDate,
                    EmailAddresses = inPerson.EmailAddresses,
                    FirstName = inPerson.FirstName,
                    FormerNames = inPerson.FormerNames,
                    Gender = inPerson.Gender,
                    GovernmentId = inPerson.GovernmentId,
                    Id = "0000001",
                    LastName = inPerson.LastName,
                    MiddleName = inPerson.MiddleName,
                    Phones = inPerson.Phones,
                    Prefix = inPerson.Prefix,
                    Suffix = inPerson.Suffix,
                };
            }

            [TestMethod]
            public async Task Client_PostProxyUserAsyncTests()
            {
                var serializedResponse = JsonConvert.SerializeObject(outPerson);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.PostProxyUserAsync(inPerson);

                // Assert that the expected item is found in the response
                Assert.AreEqual("0000001", clientResponse.Id);
                Assert.AreEqual(inPerson.BirthDate, clientResponse.BirthDate);
                Assert.AreEqual(inPerson.EmailAddresses.Count, clientResponse.EmailAddresses.Count);
                Assert.AreEqual(inPerson.EmailAddresses[0].Value, clientResponse.EmailAddresses[0].Value);
                Assert.AreEqual(inPerson.EmailAddresses[0].TypeCode, clientResponse.EmailAddresses[0].TypeCode);
                Assert.AreEqual(inPerson.EmailAddresses[1].Value, clientResponse.EmailAddresses[1].Value);
                Assert.AreEqual(inPerson.EmailAddresses[1].TypeCode, clientResponse.EmailAddresses[1].TypeCode);
                Assert.AreEqual(inPerson.FirstName, clientResponse.FirstName);
                Assert.AreEqual(inPerson.FormerNames.Count, clientResponse.FormerNames.Count);
                Assert.AreEqual(inPerson.FormerNames[0].GivenName, clientResponse.FormerNames[0].GivenName);
                Assert.AreEqual(inPerson.FormerNames[0].MiddleName, clientResponse.FormerNames[0].MiddleName);
                Assert.AreEqual(inPerson.FormerNames[0].FamilyName, clientResponse.FormerNames[0].FamilyName);
                Assert.AreEqual(inPerson.FormerNames[1].GivenName, clientResponse.FormerNames[1].GivenName);
                Assert.AreEqual(inPerson.FormerNames[1].MiddleName, clientResponse.FormerNames[1].MiddleName);
                Assert.AreEqual(inPerson.FormerNames[1].FamilyName, clientResponse.FormerNames[1].FamilyName);
                Assert.AreEqual(inPerson.Gender, clientResponse.Gender);
                Assert.AreEqual(inPerson.GovernmentId, clientResponse.GovernmentId);
                Assert.AreEqual("0000001", clientResponse.Id);
                Assert.AreEqual(inPerson.LastName, clientResponse.LastName);
                Assert.AreEqual(inPerson.MiddleName, clientResponse.MiddleName);
                Assert.AreEqual(inPerson.Prefix, clientResponse.Prefix);
                Assert.AreEqual(inPerson.Suffix, clientResponse.Suffix);
                Assert.AreEqual(inPerson.Phones.Count, clientResponse.Phones.Count);
                Assert.AreEqual(inPerson.Phones[0].Number, clientResponse.Phones[0].Number);
                Assert.AreEqual(inPerson.Phones[0].TypeCode, clientResponse.Phones[0].TypeCode);
                Assert.AreEqual(inPerson.Phones[0].Extension, clientResponse.Phones[0].Extension);
                Assert.AreEqual(inPerson.Phones[1].Number, clientResponse.Phones[1].Number);
                Assert.AreEqual(inPerson.Phones[1].TypeCode, clientResponse.Phones[1].TypeCode);
                Assert.AreEqual(inPerson.Phones[1].Extension, clientResponse.Phones[1].Extension);
            }
        }
        #endregion

        #region GetWorkTasksAsyncTests

        [TestClass]
        public class GetWorkTasksAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private List<WorkTask> workTasks;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                workTasks = new List<Dtos.Base.WorkTask>()
                {
                    new WorkTask() { Id = "1", Category = "Category 1", Description = "Description 1"},
                    new WorkTask() { Id = "2", Category = "Category 2", Description = "Description 2"},
                };
            }

            [TestMethod]
            public async Task Client_GetWorkTasksAsync_Success()
            {
                var serializedResponse = JsonConvert.SerializeObject(workTasks);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetWorkTasksAsync("0001235");

                // Assert that the expected item is found in the response
                Assert.AreEqual(workTasks.Count, clientResponse.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetWorkTasksAsync_ThrowsExceptionForMissingPersonId()
            {
                var serializedResponse = JsonConvert.SerializeObject(workTasks);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetWorkTasksAsync(null);

            }
        }

        #endregion

        #region GetSelfservicePreferencesAsyncTests

        [TestClass]
        public class GetSelfservicePreferencesAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private SelfservicePreference selfservicePreference;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                selfservicePreference = new SelfservicePreference()
                {
                    Id = "1",
                    PersonId = "900001",
                    PreferenceType = "selfservice",
                    Preferences = new Dictionary<string, dynamic>()
                    {
                        { "Homepage", "" }
                    }
                };
            }

            [TestMethod]
            public async Task Client_GetSelfservicePreferenceAsync_Success()
            {
                var serializedResponse = JsonConvert.SerializeObject(selfservicePreference);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSelfservicePreferenceAsync("900001", "selfservice");

                // Assert that the expected item is found in the response
                Assert.AreEqual(selfservicePreference.Preferences["Homepage"], clientResponse.Preferences["Homepage"]);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetSelfservicePreferenceAsync_ThrowsExceptionForMissingPersonId()
            {
                var serializedResponse = JsonConvert.SerializeObject(selfservicePreference);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSelfservicePreferenceAsync(null, null);

            }
        }
        #endregion

        #region GetTextDocumentAsyncTests

        [TestClass]
        public class GetTextDocumentAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private TextDocument textDocument;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                textDocument = new TextDocument() { Text = new List<string>() { "This is line 1.", "This is line 2." }, Subject = "Subject" };
            }

            [TestMethod]
            public async Task Client_GetTextDocumentAsync_Success()
            {
                var serializedResponse = JsonConvert.SerializeObject(textDocument);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTextDocumentAsync("DOC", "ENT", "KEY", "0001235");

                // Assert that the expected item is found in the response
                Assert.IsNotNull(textDocument);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetTextDocumentAsync_ThrowsExceptionForMissingDocumentId()
            {
                var serializedResponse = JsonConvert.SerializeObject(textDocument);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTextDocumentAsync(null, "ENT", "KEY", "0001235");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetTextDocumentAsync_ThrowsExceptionForMissingPrimaryEntity()
            {
                var serializedResponse = JsonConvert.SerializeObject(textDocument);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTextDocumentAsync("DOC", null, "KEY", "0001235");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetTextDocumentAsync_ThrowsExceptionForMissingPrimaryKey()
            {
                var serializedResponse = JsonConvert.SerializeObject(textDocument);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTextDocumentAsync("DOC", "ENT", null, "0001235");
            }
        }


        #endregion

        #region GetPrivacyStatusesTests
        [TestMethod]
        public async Task PrivacyStatuses_GetPrivacyStatusesAsync_GetTest()
        {
            var responseEntity = new List<Ellucian.Colleague.Dtos.PrivacyStatus>();
            responseEntity.Add(new Ellucian.Colleague.Dtos.PrivacyStatus() { Id = "12345", Code = "TestCode", Title = "TestTitle", Description = "TestDesc", privacyStatusType = Ellucian.Colleague.Dtos.PrivacyStatusType.Restricted });

            var serializedResponse = JsonConvert.SerializeObject(responseEntity);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetPrivacyStatusesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus>));
            Assert.IsTrue(responseEntity.Count == actualResult.Count(), "Count of objects should match");
            Assert.AreEqual(responseEntity.ElementAt(0).Id, actualResult.ElementAt(0).Id);
        }
        #endregion

        #region GetPrivacyConfigurationTests

        [TestClass]
        public class GetPrivacyConfigurationAsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private PrivacyConfiguration privacyConfiguration;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                privacyConfiguration = new PrivacyConfiguration() { RecordDenialMessage = "Record not accesible due to a privacy request" };
            }

            [TestMethod]
            public async Task Client_GetPrivacyConfigurationAsync_Success()
            {
                var serializedResponse = JsonConvert.SerializeObject(privacyConfiguration);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetPrivacyConfigurationAsync();

                // Assert that the expected item is found in the response
                Assert.IsNotNull(privacyConfiguration);
            }
        }

        #endregion

        #region GetAddressTypesTests
        [TestMethod]
        public async Task AddressTypes_GetAddressTypesAsync_GetTest()
        {
            var responseEntity = new List<Ellucian.Colleague.Dtos.AddressType2>();
            responseEntity.Add(new Ellucian.Colleague.Dtos.AddressType2() { Id = "12345", Code = "TestCode", Title = "TestTitle", Description = "TestDesc", AddressTypeList = Dtos.AddressTypeList.Other });

            var serializedResponse = JsonConvert.SerializeObject(responseEntity);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetAddressTypesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Ellucian.Colleague.Dtos.AddressType2>));
            Assert.IsTrue(responseEntity.Count == actualResult.Count(), "Count of objects should match");
            Assert.AreEqual(responseEntity.ElementAt(0).Id, actualResult.ElementAt(0).Id);
        }
        #endregion

        #region OrganizationalPersonPositions
        [TestClass]
        public class OrganizationalPersonPositionsClientMethodTests
        {
            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task OrganizationalPersonPositions_GetOrganizationalPersonPositionByIdAsync()
            {
                var responseEntity = new Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition()
                {
                    Id = "12345",
                    PersonId = "123",
                    PersonName = "Barbara Johnson",
                    PositionId = "P1",
                    PositionTitle = "Title1",
                    Relationships = new List<OrganizationalRelationship>()
                    {
                        new OrganizationalRelationship() { Id = "5", OrganizationalPersonPositionId = "PP1", RelatedOrganizationalPersonPositionId = "PP2" },
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(responseEntity);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                var actualResult = await client.GetOrganizationalPersonPositionByIdAsync("123");

                Assert.IsInstanceOfType(actualResult, typeof(Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition));
                Assert.AreEqual(responseEntity.Id, actualResult.Id);
                Assert.AreEqual(responseEntity.Relationships.ElementAt(0).Id, actualResult.Relationships.ElementAt(0).Id);

            }

        }
        #endregion

        #region OrganizationalRelationships
        [TestClass]
        public class OrganizationalRelationshipClientMethodTests
        {
            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }


            #region GetOrganizationalPersonPositionTests
            [TestMethod]
            public async Task OrganizationalPersonPosition_GetOrganizationalPersonPositionsAsync()
            {
                var responseEntity = new List<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>();
                responseEntity.Add(new Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition()
                {
                    Id = "12345",
                    PersonId = "123",
                    PersonName = "Barbara Johnson",
                    PositionId = "P1",
                    PositionTitle = "Title1",
                    Relationships = new List<OrganizationalRelationship>()
                {
                    new OrganizationalRelationship() { Id = "5", OrganizationalPersonPositionId = "PP1", RelatedOrganizationalPersonPositionId = "PP2" },
                }
                });
                responseEntity.Add(new Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition()
                {
                    Id = "6789",
                    PersonId = "123",
                    PersonName = "Barbara Johnson",
                    PositionId = "P1",
                    PositionTitle = "Title1",
                    Relationships = new List<OrganizationalRelationship>()
                {
                    new OrganizationalRelationship() { Id = "4", OrganizationalPersonPositionId = "PP1", RelatedOrganizationalPersonPositionId = "PP2" },
                }
                });

                var serializedResponse = JsonConvert.SerializeObject(responseEntity);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                var criteria = new OrganizationalPersonPositionQueryCriteria() { SearchString = "123" };
                var actualResult = await client.GetOrganizationalPersonPositionsAsync(criteria);

                Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>));
                Assert.AreEqual(2, actualResult.Count());
                Assert.AreEqual(responseEntity.ElementAt(0).Id, actualResult.ElementAt(0).Id);
                Assert.AreEqual(responseEntity.ElementAt(0).Relationships.ElementAt(0).Id, actualResult.ElementAt(0).Relationships.ElementAt(0).Id);
            }
            #endregion

            #region CreateOrganizationalRelationshipsTests
            [TestMethod]
            public async Task OrganizationalRelationships_CreateOrganizationalRelationshipAsync()
            {
                var responseEntity = new Ellucian.Colleague.Dtos.Base.OrganizationalRelationship
                {
                    Id = "5",
                    OrganizationalPersonPositionId = "PP1",
                    RelatedOrganizationalPersonPositionId = "PP2"
                };

                var serializedResponse = JsonConvert.SerializeObject(responseEntity);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                var actualResult = await client.CreateOrganizationalRelationshipAsync(responseEntity);

                // Asserts
                Assert.AreEqual(responseEntity.Id, actualResult.Id);
                Assert.AreEqual(responseEntity.OrganizationalPersonPositionId, actualResult.OrganizationalPersonPositionId);
                Assert.AreEqual(responseEntity.RelatedOrganizationalPersonPositionId, actualResult.RelatedOrganizationalPersonPositionId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task OrganizationalRelationships_CreateOrganizationalRelationshipAsync_BadRequest()
            {
                // Arrange
                var responseEntity = new Ellucian.Colleague.Dtos.Base.OrganizationalRelationship
                {
                    Id = "5",
                    OrganizationalPersonPositionId = "PP1",
                    RelatedOrganizationalPersonPositionId = "PP2"
                };
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Delete, _serviceUrl);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var actualResult = await client.CreateOrganizationalRelationshipAsync(responseEntity);
            }
            #endregion

            #region UpdateOrganizationalRelationshipsTests
            [TestMethod]
            public async Task OrganizationalRelationships_UpdateOrganizationalRelationshipAsync()
            {
                var responseEntity = new Ellucian.Colleague.Dtos.Base.OrganizationalRelationship
                {
                    Id = "5",
                    OrganizationalPersonPositionId = "PP1",
                    RelatedOrganizationalPersonPositionId = "PP2"
                };

                var serializedResponse = JsonConvert.SerializeObject(responseEntity);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                var actualResult = await client.UpdateOrganizationalRelationshipAsync(responseEntity);

                // Asserts
                Assert.AreEqual(responseEntity.Id, actualResult.Id);
                Assert.AreEqual(responseEntity.OrganizationalPersonPositionId, actualResult.OrganizationalPersonPositionId);
                Assert.AreEqual(responseEntity.RelatedOrganizationalPersonPositionId, actualResult.RelatedOrganizationalPersonPositionId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task OrganizationalRelationships_UpdateOrganizationalRelationshipAsync_BadRequest()
            {
                // Arrange
                var responseEntity = new Ellucian.Colleague.Dtos.Base.OrganizationalRelationship
                {
                    Id = "5",
                    OrganizationalPersonPositionId = "PP1",
                    RelatedOrganizationalPersonPositionId = "PP2"
                };
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var actualResult = await client.UpdateOrganizationalRelationshipAsync(responseEntity);
            }
            #endregion

            #region DeleteOrganizationalRelationshipsTests
            [TestMethod]
            public async Task OrganizationalRelationships_DeleteOrganizationalRelationshipAsync()
            {
                var deletedId = "1";

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                await client.DeleteOrganizationalRelationshipAsync(deletedId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task OrganizationalRelationships_DeleteOrganizationalRelationshipAsync_BadRequest()
            {
                // Arrange
                var deletedId = "1";
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Delete, _serviceUrl);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                await client.DeleteOrganizationalRelationshipAsync(deletedId);
            }
            #endregion

            #region OrganizationalRelationshipConfigurationTests
            [TestMethod]
            public async Task OrganizationalRelationships_GetOrganizationalRelationshipConfigurationAsync()
            {
                var responseEntity = new OrganizationalRelationshipConfiguration()
                {
                    RelationshipTypeCodeMapping = new Dictionary<OrganizationalRelationshipType, List<string>>
                    {
                        { OrganizationalRelationshipType.Manager, new List<string> { "MGR" } }
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(responseEntity);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
                var actualResult = await client.GetOrganizationalRelationshipConfigurationAsync();

                Assert.IsInstanceOfType(actualResult, typeof(Ellucian.Colleague.Dtos.Base.OrganizationalRelationshipConfiguration));
                Assert.AreEqual(1, actualResult.RelationshipTypeCodeMapping.Keys.Count);
                Assert.AreEqual(responseEntity.RelationshipTypeCodeMapping.Keys.First(), actualResult.RelationshipTypeCodeMapping.Keys.First());
                Assert.AreEqual(responseEntity.RelationshipTypeCodeMapping[OrganizationalRelationshipType.Manager].First(), actualResult.RelationshipTypeCodeMapping[OrganizationalRelationshipType.Manager].First());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task OrganizationalRelationships_GetOrganizationalRelationshipConfigurationAsync_BadRequest()
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var actualResult = await client.GetOrganizationalRelationshipConfigurationAsync();
            }
            #endregion
        }
        #endregion

        #region GetPersonalPronounTypesTests
        [TestMethod]
        public async Task PersonalPronounTypes_GetPersonalPronounTypesAsync_GetTest()
        {
            var responseEntity = new List<Ellucian.Colleague.Dtos.Base.PersonalPronounType>();
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.PersonalPronounType() { Code = "HE", Description = "He/Him/His" });
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.PersonalPronounType() { Code = "SHE", Description = "She/Her/Hers" });
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.PersonalPronounType() { Code = "ZE", Description = "Ze/Zir/Zirs" });

            var serializedResponse = JsonConvert.SerializeObject(responseEntity);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetPersonalPronounTypesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>));
            Assert.IsTrue(responseEntity.Count == actualResult.Count(), "Count of objects should match");
            Assert.AreEqual(responseEntity.ElementAt(0).Code, actualResult.ElementAt(0).Code);
            Assert.AreEqual(responseEntity.ElementAt(0).Description, actualResult.ElementAt(0).Description);
        }
        #endregion

        #region GetGenderIdentityTypesTests
        [TestMethod]
        public async Task GenderIdentityTypes_GetGenderIdentityTypesAsync_GetTest()
        {
            var responseEntity = new List<Ellucian.Colleague.Dtos.Base.GenderIdentityType>();
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.GenderIdentityType() { Code = "FEM", Description = "Female" });
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.GenderIdentityType() { Code = "MAL", Description = "Male" });
            responseEntity.Add(new Ellucian.Colleague.Dtos.Base.GenderIdentityType() { Code = "TFM", Description = "Transexual (F/M)" });

            var serializedResponse = JsonConvert.SerializeObject(responseEntity);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            var actualResult = await client.GetGenderIdentityTypesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>));
            Assert.IsTrue(responseEntity.Count == actualResult.Count(), "Count of objects should match");
            Assert.AreEqual(responseEntity.ElementAt(0).Code, actualResult.ElementAt(0).Code);
            Assert.AreEqual(responseEntity.ElementAt(0).Description, actualResult.ElementAt(0).Description);
        }
        #endregion

        #region GetCorrespondenceRequestsAsync
        
        [TestMethod]
        public async Task GetCorrespondenceRequests_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedCorrespondenceRequestsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var expectedResult = expectedCorrespondenceRequestsResponse.First();
            var actualResult = (await client.GetCorrespondenceRequestsAsync(_personId)).First();

            Assert.AreEqual(expectedResult.Code, actualResult.Code);
            Assert.AreEqual(expectedResult.PersonId, actualResult.PersonId);
            Assert.AreEqual(expectedResult.Status, actualResult.Status);
            Assert.AreEqual(expectedResult.StatusDescription, actualResult.StatusDescription);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullPersonId_ThrowsExceptionTest()
        {
            setResponse(string.Empty, HttpStatusCode.OK);
            await client.GetCorrespondenceRequestsAsync(null);
        }

        [TestMethod]
        public async Task GetCorrespondenceRequests_RethrowsBadRequestExceptionTest()
        {
            bool exceptionCaught = false;

            var serializedResponse = JsonConvert.SerializeObject(expectedCorrespondenceRequestsResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetCorrespondenceRequestsAsync(_personId);
            }
            catch { exceptionCaught = true; }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task GetCorrespondenceRequests_RethrowsNotFoundExceptionTest()
        {
            bool exceptionCaught = false;

            var serializedResponse = JsonConvert.SerializeObject(expectedCorrespondenceRequestsResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetCorrespondenceRequestsAsync(_personId);
            }
            catch { exceptionCaught = true; }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task GetCorrespondenceRequests_ReturnsEmptyCorrespondenceRequestsListTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<CorrespondenceRequest>());
            setResponse(serializedResponse, HttpStatusCode.OK);
            var actualResult = await client.GetCorrespondenceRequestsAsync(_personId);
            Assert.IsFalse(actualResult.Any());
        }

        #endregion

        #region Helpers

        private void setResponse(string serializedResponse, HttpStatusCode responseStatusCode)
        {
            var response = new HttpResponseMessage(responseStatusCode);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            client = new ColleagueApiClient(testHttpClient, _loggerMock.Object);
            client.Credentials = "otorres";
        }

        #endregion

    #region QueryEmployeeNamesByPostAsync

        [TestClass]
        public class QueryEmployeeNamesByPostAsync
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            private PersonNameQueryCriteria personCriteria;
            private EmployeeNameQueryCriteria employeeCriteria;
            private List<Person> results;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                personCriteria = new Dtos.Base.PersonNameQueryCriteria()
                {
                    Ids = new List<string> { },
                    QueryKeyword = "First Last",

                };

                employeeCriteria = new Dtos.Base.EmployeeNameQueryCriteria()
                {
                    Ids = new List<string> { },
                    QueryKeyword = "First Last",
                    ActiveOnly = false,
                    IncludeNonEmployees = false
                };

                results = new List<Dtos.Base.Person>()
                {
                    new Dtos.Base.Person() { Id = "1234567", LastName = "Last", FirstName = "First", MiddleName = "Forward", BirthNameLast = "Last", BirthNameFirst = "",
                                            BirthNameMiddle = "", PreferredName = "", PreferredAddress =  new List<string> { }, Prefix = "", Gender = "", BirthDate = null, GovernmentId = "",
                                            RaceCodes = new List<string> { }, EthnicCodes = new List<string> { }, Ethnicities = new List<EthnicOrigin> { }, MaritalStatus = null, PrivacyStatusCode = "", PersonalPronounCode = ""},
                    new Dtos.Base.Person() { Id = "7654321", LastName = "Last", FirstName = "First", MiddleName = "Reverse", BirthNameLast = "Last", BirthNameFirst = "",
                                            BirthNameMiddle = "", PreferredName = "", PreferredAddress =  new List<string> { }, Prefix = "", Gender = "", BirthDate = null, GovernmentId = "",
                                            RaceCodes = new List<string> { }, EthnicCodes = new List<string> { }, Ethnicities = new List<EthnicOrigin> { }, MaritalStatus = null, PrivacyStatusCode = "", PersonalPronounCode = ""}
                };

            }

            [TestMethod]
            public async Task Client_QueryPersonMatchResultsByPostAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(results);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryEmployeeNamesByPostAsync(personCriteria);

                // Assert that the expected item is found in the response
                Assert.AreEqual(results.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < results.Count; i++)
                {
                    Assert.AreEqual(results[i].Id, respList[i].Id);
                    Assert.AreEqual(results[i].LastName, respList[i].LastName);
                    Assert.AreEqual(results[i].FirstName, respList[i].FirstName);
                    Assert.AreEqual(results[i].MiddleName, respList[i].MiddleName);
                }
            }

            [TestMethod]
            public async Task Client_QueryEmployeeNameByPostAsync()
            {
                var serializedResponse = JsonConvert.SerializeObject(results);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryEmployeeNamesByPostAsync(employeeCriteria);

                // Assert that the expected item is found in the response
                Assert.AreEqual(results.Count, clientResponse.Count());
                var respList = clientResponse.ToList();
                for (int i = 0; i < results.Count; i++)
                {
                    Assert.AreEqual(results[i].Id, respList[i].Id);
                    Assert.AreEqual(results[i].LastName, respList[i].LastName);
                    Assert.AreEqual(results[i].FirstName, respList[i].FirstName);
                    Assert.AreEqual(results[i].MiddleName, respList[i].MiddleName);
                }
            }
        }
    }

    #endregion

    #region GetSelfServiceConfigurationAsyncTests

    [TestClass]
    public class GetSelfServiceConfigurationAsync
    {
        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _token = "1234567890";

        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        private SelfServiceConfiguration selfServiceConfiguration;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;

            _logger = _loggerMock.Object;

            selfServiceConfiguration = new SelfServiceConfiguration() { AlwaysUseClipboardForBulkMailToLinks = true };
        }

        [TestMethod]
        public async Task Client_GetSelfServiceConfigurationAsync_Success()
        {
            var serializedResponse = JsonConvert.SerializeObject(selfServiceConfiguration);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetSelfServiceConfigurationAsync();

            // Assert that the expected item is found in the response
            Assert.IsNotNull(selfServiceConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestFailedException))]
        public async Task Client_GetSelfServiceConfigurationAsync_Error()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);
            var clientResponse = await client.GetSelfServiceConfigurationAsync();

            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to retrieve Self-Service configuration data."));
        }
    }
    #endregion
    #region GetRequiredDocumentConfigurationAsyncTests

    [TestClass]
    public class GetRequiredDocumentConfigurationAsync
    {
        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _token = "1234567890";

        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        private RequiredDocumentConfiguration requiredDocumentConfiguration;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;

            _logger = _loggerMock.Object;

            requiredDocumentConfiguration = new RequiredDocumentConfiguration() {
                SuppressInstance = false,
                PrimarySortField = WebSortField.Status,
                SecondarySortField = WebSortField.OfficeDescription,
                TextForBlankStatus = "",
                TextForBlankDueDate = ""
            };
        }

        [TestMethod]
        public async Task Client_GetRequiredDocumentConfigurationAsync_Success()
        {
            var serializedResponse = JsonConvert.SerializeObject(requiredDocumentConfiguration);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetRequiredDocumentConfigurationAsync();

            // Assert that the expected item is found in the response
            Assert.IsNotNull(requiredDocumentConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestFailedException))]
        public async Task Client_GetRequiredDocumentConfigurationAsync_Error()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);
            var clientResponse = await client.GetRequiredDocumentConfigurationAsync();

            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to retrieve Required Document configuration data."));
        }
    }
    #endregion

}

