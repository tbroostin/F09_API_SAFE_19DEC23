// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Logging;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientTests
    {
        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _content = "..content..";
        private const string _username = "my-username";
        private const string _password = "my-password";
        private const string _proxyUsername = "proxy-username";
        private const string _newPassword = "my-new-password";

        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;
            _logger = _loggerMock.Object;
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.HttpRequestFailedException))]
        public void EnsureCorrectRequestFailedExceptionThrown()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = client.GetVersion();

            //Verify (done by ExpectedException attribute)
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.HttpRequestFailedException))]
        public async Task EnsureCorrectRequestFailedExceptionThrownAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.GetVersionAsync();

            //Verify (done by ExpectedException attribute)
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.LoginException))]
        public void EnsureCorrectLoginExceptionThrown()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = client.GetVersion();

            //Verify (done by ExpectedException attribute)
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.LoginException))]
        public async Task EnsureCorrectLoginExceptionThrownAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.GetVersionAsync();

            //Verify (done by ExpectedException attribute)
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.ResourceNotFoundException))]
        public void  EnsureCorrectResourceNotFoundExceptionThrown()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = client.GetVersion();

            //Verify (done by ExpectedException attribute)
        }

        // ensure that the correct namespaced version of HttpRequestFailedException is thrown.
        // important to throw the correct namespaced version as callers catching this specific
        // exception will miss it. Ellucian.Rest.Client.Exceptions has the other version.
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.ResourceNotFoundException))]
        public async Task EnsureCorrectResourceNotFoundExceptionThrownAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.GetVersionAsync();

            //Verify (done by ExpectedException attribute)
        }

        [TestMethod]
        public void EnsureLoginRequestNotLogged()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            client.Login(_username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        public async Task EnsureLoginRequestNotLoggedAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.LoginAsync(_username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        public async Task EnsureLoginRequestNotLoggedAsync2()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.Login2Async(_username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        [ExpectedException(typeof(ListenerNotFoundException))]
        public async Task Login2Async_HTTPNotFoundResponse_ThrowsListenerNotFoundException()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.Login2Async(_username, _password);
        }

        [TestMethod]
        public void EnsureProxyLoginRequestNotLogged()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            client.ProxyLogin(_proxyUsername, _username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_proxyUsername));
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        public async Task EnsureProxyLoginRequestNotLoggedAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.ProxyLoginAsync(_proxyUsername, _username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_proxyUsername));
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        public async Task EnsureProxyLoginRequestNotLoggedAsync2()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.ProxyLogin2Async(_proxyUsername, _username, _password);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_proxyUsername));
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
        }

        [TestMethod]
        [ExpectedException(typeof(ListenerNotFoundException))]
        public async Task ProxyLogin2Async_HTTPNotFoundResponse_ThrowsListenerNotFoundException()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.Login2Async(_username, _password);
        }

        [TestMethod]
        public void EnsureChangePasswordRequestNotLogged()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            client.ChangePassword(_username, _password, _newPassword);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
            Assert.IsFalse(log.Contains(_newPassword));
        }

        [TestMethod]
        public async Task EnsureChangePasswordRequestNotLoggedAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(_content);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            ILogger logger = new StringLogger();

            var client = new ColleagueApiClient(testHttpClient, logger);

            // Act
            await client.ChangePasswordAsync(_username, _password, _newPassword);

            // Assert
            string log = logger.ToString();
            Assert.IsFalse(log.Contains(_username));
            Assert.IsFalse(log.Contains(_password));
            Assert.IsFalse(log.Contains(_newPassword));
        }


        [TestMethod]
        public async Task Client_PostBackupConfigDataAsync()
        {
            // Arrange
            Ellucian.Colleague.Dtos.Base.BackupConfiguration fakeConfigData = new Dtos.Base.BackupConfiguration();
            fakeConfigData.Id = "id";
            fakeConfigData.Namespace = "testnamespace";
            fakeConfigData.ConfigData = "test{} data";
            fakeConfigData.ProductId = "testid";
            fakeConfigData.ProductVersion = "testver";
            fakeConfigData.ConfigVersion = "1";

            var serializedResponse = JsonConvert.SerializeObject(fakeConfigData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.PostBackupConfigDataAsync(fakeConfigData);

            // Assert that the expected item is found in the response
            Assert.AreEqual(fakeConfigData.Id, clientResponse.Id);
            Assert.AreEqual(fakeConfigData.Namespace, clientResponse.Namespace);
            Assert.AreEqual(fakeConfigData.ConfigData, clientResponse.ConfigData);
            Assert.AreEqual(fakeConfigData.ProductId, clientResponse.ProductId);
            Assert.AreEqual(fakeConfigData.ProductVersion, clientResponse.ProductVersion);
            Assert.AreEqual(fakeConfigData.ConfigVersion, clientResponse.ConfigVersion);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Client_PostBackupConfigDataAsync_NullArg()
        {
            // Arrange
            Ellucian.Colleague.Dtos.Base.BackupConfiguration fakeConfigData = new Dtos.Base.BackupConfiguration();
            fakeConfigData.Id = "id";
            fakeConfigData.Namespace = "testnamespace";
            fakeConfigData.ConfigData = "test{} data";
            fakeConfigData.ProductId = "testid";
            fakeConfigData.ProductVersion = "testver";
            fakeConfigData.ConfigVersion = "1";

            var serializedResponse = JsonConvert.SerializeObject(fakeConfigData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.PostBackupConfigDataAsync(null);
        }



        [TestMethod]
        public async Task Client_GetBackupConfigDataAsync()
        {
            // Arrange
            Ellucian.Colleague.Dtos.Base.BackupConfiguration fakeConfigData = new Dtos.Base.BackupConfiguration();
            fakeConfigData.Id = "id";
            fakeConfigData.Namespace = "testnamespace";
            fakeConfigData.ConfigData = "test{} data";
            fakeConfigData.ProductId = "testid";
            fakeConfigData.ProductVersion = "testver";
            fakeConfigData.ConfigVersion = "1";

            var serializedResponse = JsonConvert.SerializeObject(fakeConfigData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetBackupConfigDataAsync(fakeConfigData.Id);

            // Assert that the expected item is found in the response
            Assert.AreEqual(fakeConfigData.Id, clientResponse.Id);
            Assert.AreEqual(fakeConfigData.Namespace, clientResponse.Namespace);
            Assert.AreEqual(fakeConfigData.ConfigData, clientResponse.ConfigData);
            Assert.AreEqual(fakeConfigData.ProductId, clientResponse.ProductId);
            Assert.AreEqual(fakeConfigData.ProductVersion, clientResponse.ProductVersion);
            Assert.AreEqual(fakeConfigData.ConfigVersion, clientResponse.ConfigVersion);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Client_GetBackupConfigDataAsync_NullArg()
        {
            // Arrange                        
            Ellucian.Colleague.Dtos.Base.BackupConfiguration fakeConfigData = new Dtos.Base.BackupConfiguration();
            fakeConfigData.Id = "id";
            fakeConfigData.Namespace = "testnamespace";
            fakeConfigData.ConfigData = "test{} data";
            fakeConfigData.ProductId = "testid";
            fakeConfigData.ProductVersion = "testver";
            fakeConfigData.ConfigVersion = "1";

            var serializedResponse = JsonConvert.SerializeObject(fakeConfigData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetBackupConfigDataAsync(null);
        }

        [TestMethod]
        public async Task Client_QueryBackupConfigDataByPostAsync()
        {
            // Arrange
            BackupConfiguration fakeConfigDataRecord = new Dtos.Base.BackupConfiguration();
            fakeConfigDataRecord.Id = "id";
            fakeConfigDataRecord.Namespace = "testnamespace";
            fakeConfigDataRecord.ConfigData = "test{} data";
            fakeConfigDataRecord.ProductId = "testid";
            fakeConfigDataRecord.ProductVersion = "testver";
            fakeConfigDataRecord.ConfigVersion = "1";

            Dtos.Base.BackupConfigurationQueryCriteria fakeConfigDataQueryCriteria = new Dtos.Base.BackupConfigurationQueryCriteria();
            fakeConfigDataQueryCriteria.Namespace = fakeConfigDataRecord.Namespace;
            var fakeResultSet = new List<BackupConfiguration>() { fakeConfigDataRecord };
            var serializedResponse = JsonConvert.SerializeObject(fakeResultSet);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponseSet = await client.QueryBackupConfigDataByPostAsync(fakeConfigDataQueryCriteria);
            var actual = clientResponseSet.FirstOrDefault();

            // Assert that the expected item is found in the response
            Assert.AreEqual(fakeConfigDataRecord.Id, actual.Id);
            Assert.AreEqual(fakeConfigDataRecord.Namespace, actual.Namespace);
            Assert.AreEqual(fakeConfigDataRecord.ConfigData, actual.ConfigData);
            Assert.AreEqual(fakeConfigDataRecord.ProductId, actual.ProductId);
            Assert.AreEqual(fakeConfigDataRecord.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(fakeConfigDataRecord.ConfigVersion, actual.ConfigVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Client_QueryBackupConfigDataByPostAsync_Nullargs()
        {
            // Arrange
            BackupConfiguration fakeConfigDataRecord = new Dtos.Base.BackupConfiguration();
            fakeConfigDataRecord.Id = "id";
            fakeConfigDataRecord.Namespace = "testnamespace";
            fakeConfigDataRecord.ConfigData = "test{} data";
            fakeConfigDataRecord.ProductId = "testid";
            fakeConfigDataRecord.ProductVersion = "testver";
            fakeConfigDataRecord.ConfigVersion = "1";

            Dtos.Base.BackupConfigurationQueryCriteria fakeConfigDataQueryCriteria = new Dtos.Base.BackupConfigurationQueryCriteria();
            fakeConfigDataQueryCriteria.Namespace = fakeConfigDataRecord.Namespace;
            var fakeResultSet = new List<BackupConfiguration>() { fakeConfigDataRecord };
            var serializedResponse = JsonConvert.SerializeObject(fakeResultSet);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.QueryBackupConfigDataByPostAsync(null);
        }

        [TestMethod]
        public async Task Client_PostBackupApiConfigDataAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            await client.PostBackupApiConfigDataAsync();

            // no response content to assert
        }

        [TestMethod]
        public async Task Client_PostRestoreApiConfigDataAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            await client.PostRestoreApiConfigDataAsync();

            // no response content to assert
        }

    }
}
