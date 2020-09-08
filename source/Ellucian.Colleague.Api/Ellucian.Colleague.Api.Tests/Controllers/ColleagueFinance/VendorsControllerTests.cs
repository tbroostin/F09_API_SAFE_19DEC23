// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    #region vendors v8
    [TestClass]
    public class VendorsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorsService> _vendorsServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorsController _vendorsController;

        private List<Dtos.Vendors> _vendorsCollection;
        private Dtos.Vendors _vendorDto;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string Vendors1Guid = "a830e686-7692-4012-8da5-b1b5d44389b4";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorsServiceMock = new Mock<IVendorsService>();
            _loggerMock = new Mock<ILogger>();

            _vendorsCollection = new List<Dtos.Vendors>();

            var vendors1 = new Ellucian.Colleague.Dtos.Vendors
            {
                Id = Vendors1Guid,
                //StartOn = new DateDtoProperty() {Day = 17, Month = 3, Year = 2015},
                //EndOn = new DateDtoProperty() {Day = 18, Month = 4, Year = 2016},
            };

            _vendorsCollection.Add(vendors1);

            BuildData();

            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _vendorsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _vendorsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_vendorDto));
        }

        private void BuildData()
        {
            _vendorDto = new Ellucian.Colleague.Dtos.Vendors
            {
                Id = Vendors1Guid,
                Comment = "Some comment",
                DefaultCurrency = CurrencyIsoCode.USD,
                PaymentSources = new List<GuidObject2>()
                {
                    new GuidObject2("03ef76f3-61be-4990-8a99-9a80282fc420")
                },
                RelatedVendor = new List<RelatedVendorDtoProperty>()
                {
                   new RelatedVendorDtoProperty()
                   {
                       Type = Dtos.EnumProperties.VendorType.ParentVendor,
                       Vendor = new GuidObject2("4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46")
                   }
                },
                StartOn = DateTime.Today,
                Classifications = new List<GuidObject2>()
                {
                    new GuidObject2("d82d70be-9229-48d8-b673-4d87528726d0")
                },
                VendorDetail = new VendorDetailsDtoProperty()
                {
                    Organization = new GuidObject2("b42ca98d-edee-42da-8ddf-2a9e915221e7")
                },
                PaymentTerms = new List<GuidObject2>()
                {
                    new GuidObject2("88393aeb-8239-4324-8203-707aa1181122")
                },
                VendorHoldReasons = new List<GuidObject2>()
                {
                    new GuidObject2("c8263488-bf7d-45a7-9190-39b9587561a1")
                },
                Statuses = new List<Dtos.EnumProperties.VendorsStatuses?>()
                {
                    Dtos.EnumProperties.VendorsStatuses.Holdpayment
                }

            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorsController = null;
            _vendorsCollection = null;
            _loggerMock = null;
            _vendorsServiceMock = null;
        }

        #region Vendors

        [TestMethod]
        public async Task VendorsController_GetVendors()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors>, int>(_vendorsCollection, 1);

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorFilter>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                //Assert.AreEqual(expected.StartOn.Day, actual.StartOn.Day);
                //Assert.AreEqual(expected.StartOn.Month, actual.StartOn.Month);
                //Assert.AreEqual(expected.StartOn.Year, actual.StartOn.Year);

                //Assert.AreEqual(expected.EndOn.Day, actual.EndOn.Day);
                //Assert.AreEqual(expected.EndOn.Month, actual.EndOn.Month);
                //Assert.AreEqual(expected.EndOn.Year, actual.EndOn.Year);

                //Assert.AreEqual(expected.ClassPercentile, actual.ClassPercentile);
                //Assert.AreEqual(expected.ClassRank, actual.ClassRank);
                //Assert.AreEqual(expected.ClassSize, actual.ClassSize);
                //Assert.AreEqual(expected.Credential.Id, actual.Credential.Id);
                //Assert.AreEqual(expected.CredentialsDate, actual.CredentialsDate);
                //Assert.AreEqual(expected.CreditsEarned, actual.CreditsEarned);
                //Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
                //Assert.AreEqual(expected.Disciplines.FirstOrDefault().Id, actual.Disciplines.FirstOrDefault().Id);
                //Assert.AreEqual(expected.PerformanceMeasure, actual.PerformanceMeasure);
                //Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                //Assert.AreEqual(expected.Institution.Id, actual.Institution.Id);
                //Assert.AreEqual(expected.Recognition.FirstOrDefault().Id, actual.Recognition.FirstOrDefault().Id);

            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_vendordetail()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors>, int>(_vendorsCollection, 1);
            //var criteria = "{\"vendordetail\":\"PersonGUID123\"}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors() { VendorDetail = new Dtos.DtoProperties.VendorDetailsDtoProperty() { Person = new GuidObject2("PersonGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorFilter>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_status()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors>, int>(_vendorsCollection, 1);
            //var criteria = "{\"status\":\"active\"}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors() { Statuses = new List<VendorsStatuses?>() { VendorsStatuses.Active } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorFilter>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_classifications()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors>, int>(_vendorsCollection, 1);
            //var criteria = "{\"classifications\":\"classificationsGUID123\"}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors() { Classifications = new List<GuidObject2>() { new GuidObject2("classificationsGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorFilter>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_KeyNotFoundException()
        {
            //var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _vendorsController.GetVendorsAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _vendorsController.GetVendorsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _vendorsController.GetVendorsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _vendorsController.GetVendorsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _vendorsController.GetVendorsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_Exception()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>()))
                .Throws<Exception>();
            await _vendorsController.GetVendorsAsync(paging, criteriaFilter);
        }

        #endregion GetVendors

        #region GetVendorsByGuid

        [TestMethod]
        public async Task VendorsController_GetVendorByGuid()
        {
            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _vendorsCollection.FirstOrDefault(x => x.Id.Equals(Vendors1Guid, StringComparison.OrdinalIgnoreCase));

            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            //Assert.AreEqual(expected.StartOn.Day, actual.StartOn.Day);
            //Assert.AreEqual(expected.StartOn.Month, actual.StartOn.Month);
            //Assert.AreEqual(expected.StartOn.Year, actual.StartOn.Year);

            //Assert.AreEqual(expected.EndOn.Day, actual.EndOn.Day);
            //Assert.AreEqual(expected.EndOn.Month, actual.EndOn.Month);
            //Assert.AreEqual(expected.EndOn.Year, actual.EndOn.Year);

            //Assert.AreEqual(expected.ClassPercentile, actual.ClassPercentile);
            //Assert.AreEqual(expected.ClassRank, actual.ClassRank);
            //Assert.AreEqual(expected.ClassSize, actual.ClassSize);
            //Assert.AreEqual(expected.Credential.Id, actual.Credential.Id);
            //Assert.AreEqual(expected.CredentialsDate, actual.CredentialsDate);
            //Assert.AreEqual(expected.CreditsEarned, actual.CreditsEarned);
            //Assert.AreEqual(expected.GraduatedOn, actual.GraduatedOn);
            //Assert.AreEqual(expected.Disciplines.FirstOrDefault().Id, actual.Disciplines.FirstOrDefault().Id);
            //Assert.AreEqual(expected.PerformanceMeasure, actual.PerformanceMeasure);
            //Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            //Assert.AreEqual(expected.Institution.Id, actual.Institution.Id);
            //Assert.AreEqual(expected.Recognition.FirstOrDefault().Id, actual.Recognition.FirstOrDefault().Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_NullException()
        {
            await _vendorsController.GetVendorsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_PermissionsException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_ArgumentException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_RepositoryException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_Exception()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _vendorsController.GetVendorsByGuidAsync(Vendors1Guid);
        }

        #endregion GetVendorByGuid

        #region Put

        [TestMethod]
        public async Task VendorController_PUT()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, It.IsAny<Vendors>())).ReturnsAsync(_vendorDto);
            _vendorsServiceMock.Setup(i => i.GetVendorsByGuidAsync(Vendors1Guid)).ReturnsAsync(_vendorDto);
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_vendorDto.Id, result.Id);
            Assert.AreEqual(_vendorDto.Classifications.Count(), result.Classifications.Count());
            Assert.AreEqual(_vendorDto.Comment, result.Comment);
            Assert.AreEqual(_vendorDto.DefaultCurrency, result.DefaultCurrency);
            Assert.AreEqual(_vendorDto.PaymentSources.Count(), result.PaymentSources.Count());
            Assert.AreEqual(_vendorDto.PaymentTerms.Count(), result.PaymentTerms.Count());
            Assert.AreEqual(_vendorDto.RelatedVendor.Count(), result.RelatedVendor.Count());
            Assert.AreEqual(_vendorDto.Statuses.Count(), result.Statuses.Count());
            Assert.AreEqual(_vendorDto.VendorDetail.Organization.Id, result.VendorDetail.Organization.Id);
            Assert.AreEqual(_vendorDto.VendorHoldReasons, result.VendorHoldReasons);
        }

        [TestMethod]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_RequestId_Null()
        {
            _vendorsServiceMock.Setup(i => i.GetVendorsByGuidAsync(Vendors1Guid)).ReturnsAsync(_vendorDto);
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, It.IsAny<Vendors>())).ReturnsAsync(_vendorDto);
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task VendorController_POST()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ReturnsAsync(_vendorDto);
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_Null_Dto()
        {
            var result = await _vendorsController.PostVendorsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_InstitutionId_Null()
        {
            _vendorDto.VendorDetail.Institution = new GuidObject2("");
            _vendorDto.VendorDetail.Organization = null;
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_OrganizationId_Null()
        {
            _vendorDto.VendorDetail.Organization = new GuidObject2("");
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_PersonId_Null()
        {
            _vendorDto.VendorDetail.Organization = null;
            _vendorDto.VendorDetail.Person = new GuidObject2("");
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new KeyNotFoundException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_PermissionsException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new PermissionsException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_ArgumentException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new ArgumentException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_RepositoryException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new RepositoryException());
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_Exception()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync(_vendorDto)).ThrowsAsync(new Exception());
            var result = await _vendorsController.PostVendorsAsync(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_PermissionsException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new PermissionsException());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_RepositoryException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new RepositoryException());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_Exception1()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new Exception());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task VendorController_PutVendor_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task VendorController_PutVendor_ArgumentException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync(Vendors1Guid, _vendorDto)).ThrowsAsync(new ArgumentException());
            var result = await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_Guid_Null_Exception()
        {
            await _vendorsController.PutVendorsAsync(string.Empty, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_Request_Null_Exception()
        {
            await _vendorsController.PutVendorsAsync(Vendors1Guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_EmptyGuid_Null_Exception()
        {
            await _vendorsController.PutVendorsAsync(new Guid().ToString(), _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_RequestIdIsEmptyGuid_Null_Exception()
        {
            _vendorDto.Id = new Guid().ToString();
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_GuidsNotMatching_Exception()
        {
            _vendorDto.Id = Guid.NewGuid().ToString();
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_EndOnHasValue_Exception()
        {
            _vendorDto.EndOn = DateTime.Today;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_VendorDetail_Null__Exception()
        {
            _vendorDto.VendorDetail = null;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_Organization_Null__Exception()
        {
            _vendorDto.VendorDetail.Organization = null;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_MoreThanOneVendorDetail__Exception()
        {
            _vendorDto.VendorDetail.Institution = new GuidObject2("");
            _vendorDto.VendorDetail.Person = new GuidObject2("");

            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_OrgNotNull_PersonNotNull_Exception()
        {
            _vendorDto.VendorDetail.Institution = null;
            _vendorDto.VendorDetail.Person = new GuidObject2("");

            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_PersonNotNull_OrgNotNull_Exception()
        {
            _vendorDto.VendorDetail.Organization = null;
            _vendorDto.VendorDetail.Institution = new GuidObject2("");
            _vendorDto.VendorDetail.Person = new GuidObject2("");

            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_ClassificationId_Null_Exception()
        {
            _vendorDto.Classifications.First().Id = string.Empty;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_PaymentTermId_Null_Exception()
        {
            _vendorDto.PaymentTerms.First().Id = string.Empty;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_ValidateVendor_VendorHoldReasonsId_Null_Exception()
        {
            _vendorDto.VendorHoldReasons.First().Id = string.Empty;
            await _vendorsController.PutVendorsAsync(Vendors1Guid, _vendorDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_Exception()
        {
            var expected = _vendorsCollection.FirstOrDefault();
            await _vendorsController.PutVendorsAsync(expected.Id, expected);
        }

        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PostVendor_Exception()
        {
            var expected = _vendorsCollection.FirstOrDefault();
            await _vendorsController.PostVendorsAsync(expected);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_DeleteVendor_Exception()
        {
            var expected = _vendorsCollection.FirstOrDefault();
            await _vendorsController.DeleteVendorsAsync(expected.Id);
        }

        #endregion
    }
    #endregion

    #region vendors v11

    [TestClass]
    public class VendorsControllerTests_v11
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorsService> _vendorsServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorsController _vendorsController;

        private List<Dtos.Vendors2> _vendorsCollection;
        private Dtos.Vendors2 _vendorDto;
        private Dtos.Vendors2 _vendorDto2;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string Vendors1Guid = "a830e686-7692-4012-8da5-b1b5d44389b4";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
        private QueryStringFilter vendorDetailFilter = new QueryStringFilter("vendorDetail", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorsServiceMock = new Mock<IVendorsService>();
            _loggerMock = new Mock<ILogger>();

            _vendorsCollection = new List<Dtos.Vendors2>();

            var vendors1 = new Ellucian.Colleague.Dtos.Vendors2
            {
                Id = Vendors1Guid,
                //StartOn = new DateDtoProperty() {Day = 17, Month = 3, Year = 2015},
                //EndOn = new DateDtoProperty() {Day = 18, Month = 4, Year = 2016},
            };

            _vendorsCollection.Add(vendors1);

            BuildData();

            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _vendorsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _vendorsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_vendorDto));
        }

        private void BuildData()
        {
            _vendorDto = new Ellucian.Colleague.Dtos.Vendors2
            {
                Id = Vendors1Guid,
                Comment = "Some comment",
                DefaultCurrency = CurrencyIsoCode.USD,
                PaymentSources = new List<GuidObject2>()
                {
                    new GuidObject2("03ef76f3-61be-4990-8a99-9a80282fc420")
                },
                RelatedVendor = new List<RelatedVendorDtoProperty>()
                {
                   new RelatedVendorDtoProperty()
                   {
                       Type = Dtos.EnumProperties.VendorType.ParentVendor,
                       Vendor = new GuidObject2("4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46")
                   }
                },
                StartOn = DateTime.Today,
                Classifications = new List<GuidObject2>()
                {
                    new GuidObject2("d82d70be-9229-48d8-b673-4d87528726d0")
                },
                VendorDetail = new VendorDetailsDtoProperty()
                {
                    Organization = new GuidObject2("b42ca98d-edee-42da-8ddf-2a9e915221e7")
                },
                PaymentTerms = new List<GuidObject2>()
                {
                    new GuidObject2("88393aeb-8239-4324-8203-707aa1181122")
                },
                VendorHoldReasons = new List<GuidObject2>()
                {
                    new GuidObject2("c8263488-bf7d-45a7-9190-39b9587561a1")
                },
                Statuses = new List<Dtos.EnumProperties.VendorsStatuses?>()
                {
                    Dtos.EnumProperties.VendorsStatuses.Holdpayment
                },
                Types = new List<VendorTypes>() { VendorTypes.Travel }
            };

            _vendorDto2 = new Ellucian.Colleague.Dtos.Vendors2
            {
                Id = Vendors1Guid,
                Comment = "Some comment",
                DefaultCurrency = CurrencyIsoCode.USD,
                PaymentSources = new List<GuidObject2>()
                {
                    new GuidObject2("03ef76f3-61be-4990-8a99-9a80282fc420")
                },
                RelatedVendor = new List<RelatedVendorDtoProperty>()
                {
                   new RelatedVendorDtoProperty()
                   {
                       Type = Dtos.EnumProperties.VendorType.ParentVendor,
                       Vendor = new GuidObject2("4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46")
                   }
                },
                StartOn = DateTime.Today,
                Classifications = new List<GuidObject2>()
                {
                    new GuidObject2("d82d70be-9229-48d8-b673-4d87528726d0")
                },
                VendorDetail = new VendorDetailsDtoProperty()
                {
                    Organization = new GuidObject2("b42ca98d-edee-42da-8ddf-2a9e915221e7")
                },
                PaymentTerms = new List<GuidObject2>()
                {
                    new GuidObject2("88393aeb-8239-4324-8203-707aa1181122")
                },
                VendorHoldReasons = new List<GuidObject2>()
                {
                    new GuidObject2("c8263488-bf7d-45a7-9190-39b9587561a1")
                },
                Statuses = new List<Dtos.EnumProperties.VendorsStatuses?>()
                {
                    Dtos.EnumProperties.VendorsStatuses.Holdpayment
                },
                Types = new List<VendorTypes>() { VendorTypes.Travel }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorsController = null;
            _vendorsCollection = null;
            _loggerMock = null;
            _vendorsServiceMock = null;
        }

        #region Vendors

        [TestMethod]
        public async Task VendorsController_GetVendors()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_vendordetail()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = "{\"vendordetail\":\"PersonGUID123\"}";
            // var criteria = @"{'vendorDetail':{'person': {'id':'PersonGUID123'}}}";
            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { VendorDetail = new Dtos.DtoProperties.VendorDetailsDtoProperty() { Person = new GuidObject2("PersonGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_vendordetail_organizations()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = "{\"vendordetail\":\"PersonGUID123\"}";
            // var criteria = @"{'vendorDetail':{'person': {'id':'PersonGUID123'}}}";
            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { VendorDetail = new Dtos.DtoProperties.VendorDetailsDtoProperty() { Organization = new GuidObject2("PersonGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_vendordetail_institution()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = "{\"vendordetail\":\"PersonGUID123\"}";
            // var criteria = @"{'vendorDetail':{'person': {'id':'PersonGUID123'}}}";
            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { VendorDetail = new Dtos.DtoProperties.VendorDetailsDtoProperty() { Institution = new GuidObject2("PersonGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

       

        [TestMethod]
        public async Task VendorsController_GetVendors_vendordetail_NameQuery()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = "{\"vendordetail\":\"PersonGUID123\"}";
            // var criteria = @"{'vendorDetail':{'person': {'id':'PersonGUID123'}}}";
            var filterGroupName = "vendorDetail";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Filters.VendorDetail() { vendorDetail = new GuidObject2("PersonGUID123") } );

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_relatedReference()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { relatedReference = new List<RelatedVendorDtoProperty>() { new RelatedVendorDtoProperty() { Type = Dtos.EnumProperties.VendorType.ParentVendor } } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_relatedReference_PaymentVendor()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { relatedReference = new List<RelatedVendorDtoProperty>() { new RelatedVendorDtoProperty() { Type = Dtos.EnumProperties.VendorType.PaymentVendor } } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_relatedVendors()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { RelatedVendor = new List<RelatedVendorDtoProperty>() { new RelatedVendorDtoProperty() { Type = Dtos.EnumProperties.VendorType.ParentVendor } } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_relatedVendors_PaymentVendors()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { RelatedVendor = new List<RelatedVendorDtoProperty>() { new RelatedVendorDtoProperty() { Type = Dtos.EnumProperties.VendorType.PaymentVendor} } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_status()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);

            // var criteria = @"{ 'statuses':['active','approved'] }";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { Statuses = new List<VendorsStatuses?>() { VendorsStatuses.Active, VendorsStatuses.Approved, VendorsStatuses.Holdpayment } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_classifications()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            //var criteria = "{\"classifications\":\"classificationsGUID123\"}";
            var criteria = @"{'classifications':[{'id':'classificationsGUID123'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { Classifications = new List<GuidObject2>() { new GuidObject2("classificationsGUID123") } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_taxId()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            //var criteria = "{\"classifications\":\"classificationsGUID123\"}";
            var criteria = @"{'classifications':[{'id':'classificationsGUID123'}]}";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { TaxId = "tax123" });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendors_taxId_empty()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
        
            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { TaxId = string.Empty });
            _vendorsController.Request.Properties.Add("EmptyFilterProperties", true);
            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }
        [TestMethod]
        public async Task VendorsController_GetVendors_vendorType()
        {
            _vendorsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.Vendors2>, int>(_vendorsCollection, 1);
            // var criteria = "";
            // var vendorType = @"{'vendorType':'travel'} ";

            var filterGroupName = "criteria";
            _vendorsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Vendors2() { Types = new List<VendorTypes>() { VendorTypes.Travel } });

            _vendorsServiceMock
                .Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorsController.GetVendorsAsync2(new Paging(10, 0), criteriaFilter, vendorDetailFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Vendors2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_KeyNotFoundException()
        {
            //var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _vendorsController.GetVendorsAsync2(null, criteriaFilter, vendorDetailFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _vendorsController.GetVendorsAsync2(paging, criteriaFilter, vendorDetailFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _vendorsController.GetVendorsAsync2(paging, criteriaFilter, vendorDetailFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _vendorsController.GetVendorsAsync2(paging, criteriaFilter, vendorDetailFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _vendorsController.GetVendorsAsync2(paging, criteriaFilter, vendorDetailFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendors_Exception()
        {
            var paging = new Paging(100, 0);
            _vendorsServiceMock.Setup(x => x.GetVendorsAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _vendorsController.GetVendorsAsync2(paging, criteriaFilter, vendorDetailFilter);
        }

        #endregion GetVendors

        #region GetVendorsByGuid

        [TestMethod]
        public async Task VendorsController_GetVendorByGuid()
        {
            _vendorsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _vendorsCollection.FirstOrDefault(x => x.Id.Equals(Vendors1Guid, StringComparison.OrdinalIgnoreCase));

            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_NullException()
        {
            await _vendorsController.GetVendorsByGuidAsync2(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_PermissionsException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_ArgumentException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_RepositoryException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorByGuid_Exception()
        {
            _vendorsServiceMock.Setup(x => x.GetVendorsByGuidAsync2(It.IsAny<string>()))
                .Throws<Exception>();
            await _vendorsController.GetVendorsByGuidAsync2(Vendors1Guid);
        }

        #endregion GetVendorByGuid

        #region Put

        [TestMethod]
        public async Task VendorController_PUT()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, It.IsAny<Vendors2>())).ReturnsAsync(_vendorDto);
            _vendorsServiceMock.Setup(i => i.GetVendorsByGuidAsync2(Vendors1Guid)).ReturnsAsync(_vendorDto);
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_vendorDto.Id, result.Id);
            Assert.AreEqual(_vendorDto.Classifications.Count(), result.Classifications.Count());
            Assert.AreEqual(_vendorDto.Comment, result.Comment);
            Assert.AreEqual(_vendorDto.DefaultCurrency, result.DefaultCurrency);
            Assert.AreEqual(_vendorDto.PaymentSources.Count(), result.PaymentSources.Count());
            Assert.AreEqual(_vendorDto.PaymentTerms.Count(), result.PaymentTerms.Count());
            Assert.AreEqual(_vendorDto.RelatedVendor.Count(), result.RelatedVendor.Count());
            Assert.AreEqual(_vendorDto.Statuses.Count(), result.Statuses.Count());
            Assert.AreEqual(_vendorDto.VendorDetail.Organization.Id, result.VendorDetail.Organization.Id);
            Assert.AreEqual(_vendorDto.VendorHoldReasons, result.VendorHoldReasons);
        }

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task VendorController_PUT_Unset_VendorDetail_organization_id()
        //{
        //    _vendorDto2.VendorDetail.Organization = null;
        //    _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, It.IsAny<Vendors2>())).ReturnsAsync(_vendorDto2);
        //    _vendorsServiceMock.Setup(i => i.GetVendorsByGuidAsync2(Vendors1Guid)).ReturnsAsync(_vendorDto);
        //    var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto2);

        //}

        [TestMethod]
        public async Task VendorController_PutVendor_ValidateUpdateRequest_RequestId_Null()
        {
            _vendorsServiceMock.Setup(i => i.GetVendorsByGuidAsync2(Vendors1Guid)).ReturnsAsync(_vendorDto);
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, It.IsAny<Vendors2>())).ReturnsAsync(_vendorDto);
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task VendorController_POST()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ReturnsAsync(_vendorDto);
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_Null_Dto()
        {
            var result = await _vendorsController.PostVendorsAsync2(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_InstitutionId_Null()
        {
            _vendorDto.VendorDetail.Institution = new GuidObject2("");
            _vendorDto.VendorDetail.Organization = null;
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_OrganizationId_Null()
        {
            _vendorDto.VendorDetail.Organization = new GuidObject2("");
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_PersonId_Null()
        {
            _vendorDto.VendorDetail.Organization = null;
            _vendorDto.VendorDetail.Person = new GuidObject2("");
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new KeyNotFoundException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_PermissionsException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new PermissionsException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_ArgumentException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new ArgumentException());
            _vendorDto.Id = string.Empty;
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_RepositoryException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new RepositoryException());
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_POST_Exception()
        {
            _vendorsServiceMock.Setup(i => i.PostVendorAsync2(_vendorDto)).ThrowsAsync(new Exception());
            var result = await _vendorsController.PostVendorsAsync2(_vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_PermissionsException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new PermissionsException());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_RepositoryException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new RepositoryException());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_IntegrationApiException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PutVendor_Exception1()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new Exception());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task VendorController_PutVendor_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task VendorController_PutVendor_ArgumentException()
        {
            _vendorsServiceMock.Setup(i => i.PutVendorAsync2(Vendors1Guid, _vendorDto)).ThrowsAsync(new ArgumentException());
            var result = await _vendorsController.PutVendorsAsync2(Vendors1Guid, _vendorDto);
        }
        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_PostVendor_Exception()
        {
            var expected = _vendorsCollection.FirstOrDefault();
            await _vendorsController.PostVendorsAsync2(expected);
        }

        #endregion

    }

    #endregion

    #region QueryVendorsByPostAsync Tests

    [TestClass]
    public class QueryVendorsByPostAsyncTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorsService> _vendorsServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorsController _vendorsController;

        private Dtos.ColleagueFinance.VendorSearchCriteria criteriaDto;
        private Dtos.ColleagueFinance.VendorSearchResult resultsDto;
        private Dtos.ColleagueFinance.VendorSearchResult resultsDto1;
        private List<Dtos.ColleagueFinance.VendorSearchResult> searchResult;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorsServiceMock = new Mock<IVendorsService>();
            _loggerMock = new Mock<ILogger>();

            criteriaDto = new Dtos.ColleagueFinance.VendorSearchCriteria()
            {
                QueryKeyword = "Office supply"
            };

            BuildData();
            _vendorsServiceMock.Setup(s => s.QueryVendorsByPostAsync(criteriaDto)).ReturnsAsync(searchResult);
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            searchResult = new List<Dtos.ColleagueFinance.VendorSearchResult>();
            resultsDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorSearchResult
            {
                VendorId = "0000100",
                VendorName = "Office & Supply",
                VendorAddress = "Test address"
            };
            resultsDto1 = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorSearchResult
            {
                VendorId = "0000101",
                VendorName = "Office & Supply - 1",
                VendorAddress = "Test address - 1"
            };
            searchResult.Add(resultsDto);
            searchResult.Add(resultsDto1);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorsController = null;
            _loggerMock = null;
            _vendorsServiceMock = null;
            searchResult = null;
        }

        [TestMethod]
        public async Task VendorsController_QueryVendorsByPostAsync_Success()
        {
            var results = await _vendorsController.QueryVendorsByPostAsync(criteriaDto);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count() > 0);
            Assert.AreEqual(searchResult.Count, results.Count());

            Assert.IsTrue(results.Where(x => x.VendorId == resultsDto.VendorId).Count() == 1);
            var actualDto = results.Where(x => x.VendorId == resultsDto.VendorId).FirstOrDefault();
            Assert.AreEqual(resultsDto.VendorId, actualDto.VendorId);
            Assert.AreEqual(resultsDto.VendorName, actualDto.VendorName);
            Assert.AreEqual(resultsDto.VendorAddress, actualDto.VendorAddress);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorsByPostAsync_NullCriteria()
        {
            var results = await _vendorsController.QueryVendorsByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorsByPostAsync_EmptyCriteria()
        {
            criteriaDto = new Dtos.ColleagueFinance.VendorSearchCriteria()
            {
                QueryKeyword = ""
            };
            var results = await _vendorsController.QueryVendorsByPostAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorsByPostAsync_ArgumentNullException()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorsByPostAsync(criteriaDto)).Throws(new ArgumentNullException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorsByPostAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorsByPostAsync_Exception()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorsByPostAsync(criteriaDto)).Throws(new Exception());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorsByPostAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorsByPostAsync_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorsByPostAsync(criteriaDto)).Throws(new KeyNotFoundException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorsByPostAsync(criteriaDto);
        }
    }

    #endregion

    #region QueryVendorForVoucherAsync Tests

    [TestClass]
    public class QueryVendorForVoucherAsyncTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorsService> _vendorsServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorsController _vendorsController;

        private Dtos.ColleagueFinance.VendorSearchCriteria criteriaDto;
        private Dtos.ColleagueFinance.VendorsVoucherSearchResult resultsDto;
        private Dtos.ColleagueFinance.VendorsVoucherSearchResult resultsDto1;
        private List<Dtos.ColleagueFinance.VendorsVoucherSearchResult> searchResult;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorsServiceMock = new Mock<IVendorsService>();
            _loggerMock = new Mock<ILogger>();

            criteriaDto = new Dtos.ColleagueFinance.VendorSearchCriteria()
            {
                QueryKeyword = "Office supply"
            };

            BuildData();
            _vendorsServiceMock.Setup(s => s.QueryVendorForVoucherAsync(criteriaDto)).ReturnsAsync(searchResult);
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            searchResult = new List<Dtos.ColleagueFinance.VendorsVoucherSearchResult>();
            resultsDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult
            {
                VendorId = "0000192",
                VendorNameLines = new List<string>() { "Blue Cross Office supply" },
                VendorMiscName = null,
                AddressLines = new List<string>() { "PO Box 69845" },
                City = "Minneapolis",
                State = "MN",
                Zip = "55430",
                Country = "",
                FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                AddressId = "143"
            };
            resultsDto1 = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult
            {
                VendorId = "0000193",
                VendorNameLines = new List<string>() { "Logistic Office supply" },
                VendorMiscName = null,
                AddressLines = new List<string>() { "PO Box 7777", "test" },
                City = "New York",
                State = "MN",
                Zip = "55430",
                Country = "USA",
                FormattedAddress = "PO Box 7777 New York MN 55430",
                AddressId = "144"
            };
            searchResult.Add(resultsDto);
            searchResult.Add(resultsDto1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorsController = null;
            _loggerMock = null;
            _vendorsServiceMock = null;
            searchResult = null;
        }

        [TestMethod]
        public async Task VendorsController_QueryVendorForVoucherAsync_Success()
        {
            var results = await _vendorsController.QueryVendorForVoucherAsync(criteriaDto);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count() > 0);
            Assert.AreEqual(searchResult.Count, results.Count());

            Assert.IsTrue(results.Where(x => x.VendorId == resultsDto.VendorId).Count() == 1);
            var actualDto = results.Where(x => x.VendorId == resultsDto.VendorId).FirstOrDefault();
            Assert.AreEqual(resultsDto.VendorId, actualDto.VendorId);
            Assert.AreEqual(resultsDto.VendorNameLines, actualDto.VendorNameLines);
            Assert.AreEqual(resultsDto.VendorMiscName, actualDto.VendorMiscName);
            Assert.AreEqual(resultsDto.AddressLines, actualDto.AddressLines);
            Assert.AreEqual(resultsDto.City, actualDto.City);
            Assert.AreEqual(resultsDto.State, actualDto.State);
            Assert.AreEqual(resultsDto.Zip, actualDto.Zip);
            Assert.AreEqual(resultsDto.Country, actualDto.Country);
            Assert.AreEqual(resultsDto.FormattedAddress, actualDto.FormattedAddress);
            Assert.AreEqual(resultsDto.AddressId, actualDto.AddressId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorForVoucherAsync_NullCriteria()
        {
            var results = await _vendorsController.QueryVendorForVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorForVoucherAsync_EmptyCriteria()
        {
            criteriaDto = new Dtos.ColleagueFinance.VendorSearchCriteria()
            {
                QueryKeyword = ""
            };
            var results = await _vendorsController.QueryVendorForVoucherAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorForVoucherAsync_ArgumentNullException()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorForVoucherAsync(criteriaDto)).Throws(new ArgumentNullException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorForVoucherAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorForVoucherAsync_Exception()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorForVoucherAsync(criteriaDto)).Throws(new Exception());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorForVoucherAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_QueryVendorForVoucherAsync_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(s => s.QueryVendorForVoucherAsync(criteriaDto)).Throws(new KeyNotFoundException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.QueryVendorForVoucherAsync(criteriaDto);
        }
    }

    #endregion

    #region GetVendorDefaultTaxFormInfoAsync Tests

    [TestClass]
    public class GetVendorDefaultTaxFormInfoAsyncTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorsService> _vendorsServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorsController _vendorsController;

        private string vendorId;
        private string apType;
        private Dtos.ColleagueFinance.VendorDefaultTaxFormInfo resultDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorsServiceMock = new Mock<IVendorsService>();
            _loggerMock = new Mock<ILogger>();

            vendorId = "0000190";
            apType = "AP";

            BuildData();
            _vendorsServiceMock.Setup(s => s.GetVendorDefaultTaxFormInfoAsync(vendorId, apType)).ReturnsAsync(resultDto);
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {

            resultDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorDefaultTaxFormInfo
            {
                VendorId = vendorId,
                TaxForm = "1098",
                TaxFormBoxCode = "MTG",
                TaxFormState = "FL"
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorsController = null;
            _loggerMock = null;
            _vendorsServiceMock = null;
            resultDto = null;
        }

        [TestMethod]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_Success()
        {
            var actualDto = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
            Assert.IsNotNull(actualDto);

            Assert.AreEqual(resultDto.VendorId, actualDto.VendorId);
            Assert.AreEqual(resultDto.TaxForm, actualDto.TaxForm);
            Assert.AreEqual(resultDto.TaxFormBoxCode, actualDto.TaxFormBoxCode);
            Assert.AreEqual(resultDto.TaxFormState, actualDto.TaxFormState);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_NullCriteria()
        {
            var actualDto = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(null, null);
            Assert.IsNull(actualDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_EmptyCriteria()
        {
            var actualDto = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(string.Empty, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_ArgumentNullException()
        {
            _vendorsServiceMock.Setup(s => s.GetVendorDefaultTaxFormInfoAsync(null, null)).Throws(new ArgumentNullException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_Exception()
        {
            _vendorsServiceMock.Setup(s => s.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorDefaultTaxFormInfoAsync_KeyNotFoundException()
        {
            _vendorsServiceMock.Setup(s => s.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new KeyNotFoundException());
            _vendorsController = new VendorsController(_vendorsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vendorsController.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
        }
    }

    #endregion
}