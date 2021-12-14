//Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
  
    public class VendorContactsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVendorContactsService> _vendorContactServiceMock;
        private Mock<ILogger> _loggerMock;

        private VendorContactsController _vendorContactController;

        private List<Dtos.VendorContacts> _vendorContactsCollection;
        private Dtos.VendorContacts _vendorContactDto;
        private Dtos.VendorContacts _vendorContactDto2;
        private Dtos.VendorContacts _vendorContactReturnInitDto;
        private PersonMatchingRequests _returnPersonMatchReq;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string VendorsContactGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";
        private const string personGuid = "b830e686-7692-4012-8da5-b1b5d44389b5";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vendorContactServiceMock = new Mock<IVendorContactsService>();
            _loggerMock = new Mock<ILogger>();

            _vendorContactsCollection = new List<Dtos.VendorContacts>();

            var vendorContact1 = new Ellucian.Colleague.Dtos.VendorContacts
            {
                Id = VendorsContactGuid,
             
            };

            _vendorContactsCollection.Add(vendorContact1);

            BuildData();

            _vendorContactController = new VendorContactsController(_vendorContactServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _vendorContactController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        private void BuildData()
        {
            _vendorContactDto = new Ellucian.Colleague.Dtos.VendorContacts
            {
                Id = VendorsContactGuid,
                Vendor = new GuidObject2("venguid1")
              
            };

            _vendorContactDto2 = new Ellucian.Colleague.Dtos.VendorContacts
            {
                Id = VendorsContactGuid,
                Vendor = new GuidObject2("venguid2")

            };

            _vendorContactReturnInitDto = new VendorContacts()
            {
                Id = VendorsContactGuid,
                Vendor = new GuidObject2( "venguid3" ),
                Contact = new Dtos.DtoProperties.VendorContactsContact()
                {
                    EndOn = DateTime.Now.AddDays( 5 ),
                    Person = new Dtos.DtoProperties.VendorContactsPerson()
                    {
                        Detail = new GuidObject2( personGuid ),
                        Name = "Lastname M Firstname"
                    },
                    Phones = new List<Dtos.DtoProperties.VendorContactsPhones>()
                    {
                        new Dtos.DtoProperties.VendorContactsPhones()
                        {
                            Extension = "Ext1",
                            Number = "800 555 1212",
                            Type = new GuidObject2( "c830e686-7692-4012-8da5-b1b5d44389b6" )
                        }
                    },
                    RelationshipType = new GuidObject2( "d830e686-7692-4012-8da5-b1b5d44389b7" ),
                    StartOn = DateTime.Now
                }
            };
            _returnPersonMatchReq = new PersonMatchingRequests() 
            { 
                Id = "a830e686-7692-4012-8da5-b1b5d44389b4", 
                Originator = "Originator", 
                Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>(), 
                Person = new GuidObject2() 
            };        
    }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorContactController = null;
            _vendorContactsCollection = null;
            _loggerMock = null;
            _vendorContactServiceMock = null;
        }

        #region VendorContacts

        [TestMethod]
        public async Task VendorsController_GetVendorContacts()
        {
            _vendorContactController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorContactController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.VendorContacts>, int>(_vendorContactsCollection, 1);

            _vendorContactServiceMock
                .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var vendors = await _vendorContactController.GetVendorContactsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.VendorContacts>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.VendorContacts>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorContactsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }      


        [TestMethod]
        public async Task VendorsController_GetVendorContacts_Filter()
        {
            _vendorContactController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorContactController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var tuple = new Tuple<IEnumerable<Dtos.VendorContacts>, int>(_vendorContactsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorContactController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.VendorContacts() { Vendor = new GuidObject2("venguid1") });

            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .ReturnsAsync(tuple);

            var vendors = await _vendorContactController.GetVendorContactsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.VendorContacts>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.VendorContacts>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _vendorContactsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task VendorsController_GetVendorContacts_Filter_Empty()
        {
            _vendorContactController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _vendorContactController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var tuple = new Tuple<IEnumerable<Dtos.VendorContacts>, int>(_vendorContactsCollection, 1);
            // var criteria = @"{'relatedReference':'parentVendor'}";
            // var criteria = @"{'relatedReference':[{'type':'parentVendor'}]}";

            var filterGroupName = "criteria";
            _vendorContactController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.VendorContacts() { Vendor = null });

            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .ReturnsAsync(tuple);

            var vendors = await _vendorContactController.GetVendorContactsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await vendors.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.VendorContacts>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.VendorContacts>;

            Assert.IsNotNull(actuals);
            //Assert.AreEqual(actuals.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_KeyNotFoundException()
        {
            //var paging = new Paging(100, 0);
            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .Throws<KeyNotFoundException>();
            await _vendorContactController.GetVendorContactsAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .Throws<PermissionsException>();
            await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _vendorContactServiceMock
               .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
               .Throws<ArgumentException>();
            await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .Throws<RepositoryException>();
            await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _vendorContactServiceMock
                 .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
                 .Throws<IntegrationApiException>();
            await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorsController_GetVendorContacts_Exception()
        {
            var paging = new Paging(100, 0);
            _vendorContactServiceMock
               .Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>()))
               .Throws<Exception>();
            await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
        }

        //GET v1.0.0
        //Successful
        //GetVendorContactsAsync
        [TestMethod]
        public async Task VendorContactsController_GetVendorContactsAsync_Permissions()
        {
            var expected = _vendorContactsCollection.FirstOrDefault(x => x.Id.Equals(VendorsContactGuid, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "VendorContacts" },
                    { "action", "GetVendorContactsAsync" }
                };
            HttpRoute route = new HttpRoute("vendor-contacts", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _vendorContactController.Request.SetRouteData(data);
            _vendorContactController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts);

            var controllerContext = _vendorContactController.ControllerContext;
            var actionDescriptor = _vendorContactController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.VendorContacts>, int>(_vendorContactsCollection, 1);

            _vendorContactServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var vendors = await _vendorContactController.GetVendorContactsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _vendorContactController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewVendorContacts));


        }

        //GET v1.0.0
        //Exception
        //GetVendorContactsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorContactsController_GetVendorContactsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "VendorContacts" },
                    { "action", "GetVendorContactsAsync" }
                };
            HttpRoute route = new HttpRoute("vendor-contacts", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _vendorContactController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _vendorContactController.ControllerContext;
            var actionDescriptor = _vendorContactController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _vendorContactServiceMock.Setup(x => x.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<VendorContacts>(), It.IsAny<bool>())).Throws<PermissionsException>();
                _vendorContactServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view vendor-contacts."));
                await _vendorContactController.GetVendorContactsAsync(paging, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetVendorContacts

        #region GetVendorContactsByGuid

        [TestMethod]
        public async Task VendorsController_GetVendorContactsByGuid()
        {
            _vendorContactController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = _vendorContactsCollection.FirstOrDefault(x => x.Id.Equals(VendorsContactGuid, StringComparison.OrdinalIgnoreCase));

            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_NullException()
        {
            await _vendorContactController.GetVendorContactsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_KeyNotFoundException()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_PermissionsException()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_ArgumentException()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_RepositoryException()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_IntegrationApiException()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorController_GetVendorContactsByGuid_Exception()
        {
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
        }

        //GET by id v1.0.0
        //Successful
        //GetVendorContactsByGuidAsync
        [TestMethod]
        public async Task VendorContactsController_GetVendorContactsByGuidAsync_Permissions()
        {
            var expected = _vendorContactsCollection.FirstOrDefault(x => x.Id.Equals(VendorsContactGuid, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "VendorContacts" },
                    { "action", "GetVendorContactsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("vendor-contacts", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _vendorContactController.Request.SetRouteData(data);
            _vendorContactController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts);

            var controllerContext = _vendorContactController.ControllerContext;
            var actionDescriptor = _vendorContactController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _vendorContactServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);

            Object filterObject;
            _vendorContactController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewVendorContacts));


        }

        //GET by id v1.0.0
        //Exception
        //GetVendorContactsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VendorContactsController_GetVendorContactsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "VendorContacts" },
                    { "action", "GetVendorContactsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("vendor-contacts", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _vendorContactController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _vendorContactController.ControllerContext;
            var actionDescriptor = _vendorContactController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _vendorContactServiceMock.Setup(x => x.GetVendorContactsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                _vendorContactServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view vendor-contacts."));
                await _vendorContactController.GetVendorContactsByGuidAsync(VendorsContactGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetVendorContactsByGuid

        #region VendorContactsInitiationProcess

        [TestMethod]
        public async Task VendorContactInitiationProcess_PostVendorContactInitiationProcessAsync_Dtos_VendorContacts()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ReturnsAsync( _vendorContactReturnInitDto );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
            Assert.IsNotNull( result );
            Assert.IsInstanceOfType( result, typeof( Dtos.VendorContacts ) );
            Assert.IsInstanceOfType( ( (Dtos.VendorContacts) result ).Contact, typeof( VendorContactsContact ) );
            Assert.IsInstanceOfType( ( (Dtos.VendorContacts) result ).Contact.Phones, typeof( List<VendorContactsPhones> ) );
            Assert.IsInstanceOfType( ( (Dtos.VendorContacts) result ).Contact.Person, typeof( VendorContactsPerson ) );
        }

        [TestMethod]
        public async Task VendorContactInitiationProcess_PostVendorContactInitiationProcessAsync_PersonMatchRequest()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ReturnsAsync( _returnPersonMatchReq );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
            Assert.IsNotNull( result );
            Assert.IsNotNull( ( (Dtos.PersonMatchingRequests) result ).Outcomes );
            Assert.IsInstanceOfType( result, typeof( Dtos.PersonMatchingRequests ) );
            Assert.IsInstanceOfType( ( (Dtos.PersonMatchingRequests) result ).Person, typeof( GuidObject2 ) );
        }

        [TestMethod]
        [ExpectedException(typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_KeyNotFoundException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new KeyNotFoundException() );
            try
            {
                var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
            }
            catch( HttpResponseException e )
            {
                Assert.AreEqual( HttpStatusCode.NotFound, e.Response.StatusCode );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_PermissionsException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new PermissionsException() );
            try
            {
                var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
            }
            catch( HttpResponseException e )
            {
                Assert.AreEqual( HttpStatusCode.Forbidden, e.Response.StatusCode );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_ArgumentException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new ArgumentException() );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_RepositoryException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new RepositoryException() );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_IntegrationApiException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new IntegrationApiException() );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_ConfigurationException()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new ConfigurationException() );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
        }

        [TestMethod]
        [ExpectedException( typeof( HttpResponseException ) )]
        public async Task PostVendorContactInitiationProcessAsync_Exception()
        {
            _vendorContactServiceMock.Setup( s => s.CreateVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() ) ).ThrowsAsync( new Exception() );
            var result = await _vendorContactController.PostVendorContactInitiationProcessAsync( It.IsAny<VendorContactInitiationProcess>() );
        }
        #endregion
    }
}