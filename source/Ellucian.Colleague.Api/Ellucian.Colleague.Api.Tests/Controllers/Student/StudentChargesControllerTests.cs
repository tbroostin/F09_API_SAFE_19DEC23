// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;


namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentChargesControllerTests
    {
        [TestClass]
        public class Get
        {
            public TestContext TestContext { get; set; }

            Mock<IStudentChargeService> _studentChargesServiceMock;
            Mock<ILogger> _loggerMock;

            StudentChargesController _studentChargesController;
            List<Dtos.StudentCharge> _studentChargeDtos;
            List<Dtos.StudentCharge1> _studentCharge1Dtos;
            private const int Offset = 0;
            private const int Limit = 2;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                _studentChargesServiceMock = new Mock<IStudentChargeService>();
                _loggerMock = new Mock<ILogger>();

                BuildData();
                BuildData1();

                _studentChargesController = new StudentChargesController(_studentChargesServiceMock.Object, _loggerMock.Object) { Request = new HttpRequestMessage() };
                _studentChargesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                _studentChargesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentChargesController = null;
                _studentChargeDtos = null;
                _studentCharge1Dtos = null;
                _studentChargesServiceMock = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll_NoCache_True()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var studentCharges = _studentChargeDtos.Take(2);
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentCharges, 4);
                _studentChargesServiceMock.Setup(i => i.GetAsync(Offset, Limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = new Paging(Limit, Offset);
                var actuals = await _studentChargesController.GetAsync(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentCharge> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge>>) httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge>;

                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());

                foreach (var actual in results)
                {
                    var expected = studentCharges.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                    Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);

                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments, actual.Comments);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                    if (expected.ChargedAmount.Amount != null)
                    {
                        Assert.AreEqual(expected.ChargedAmount.Amount.Currency, actual.ChargedAmount.Amount.Currency);
                        Assert.AreEqual(expected.ChargedAmount.Amount.Value, actual.ChargedAmount.Amount.Value);
                    }
                    else if (expected.ChargedAmount.UnitCost != null)
                    {
                        Assert.AreEqual(expected.ChargedAmount.UnitCost.Quantity, actual.ChargedAmount.UnitCost.Quantity);
                    }
                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll1_NoCache_True()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var studentCharges = _studentCharge1Dtos.Take(2);
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentCharges, 4);
                _studentChargesServiceMock.Setup(i => i.GetAsync1(Offset, Limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = new Paging(Limit, Offset);
                var actuals = await _studentChargesController.GetAsync1(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.StudentCharge1> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());

                foreach (var actual in results)
                {
                    var expected = studentCharges.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                    Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);

                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments, actual.Comments);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                    if (expected.ChargedAmount.Amount != null)
                    {
                        Assert.AreEqual(expected.ChargedAmount.Amount.Currency, actual.ChargedAmount.Amount.Currency);
                        Assert.AreEqual(expected.ChargedAmount.Amount.Value, actual.ChargedAmount.Amount.Value);
                    }
                    else if (expected.ChargedAmount.UnitCost != null)
                    {
                        Assert.AreEqual(expected.ChargedAmount.UnitCost.Quantity, actual.ChargedAmount.UnitCost.Quantity);
                    }
                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll_PagingNull()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge>, int>(_studentChargeDtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = null;
                var results = await _studentChargesController.GetAsync(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(3, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                   
                }
            }



            [TestMethod]
            public async Task StudentChargesController_GetAll1_PagingNull()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }


            [TestMethod]
            public async Task StudentChargesController_GetAll1_Student_Filters()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                
                QueryStringFilter criteria = new QueryStringFilter("criteria", @"{'student':{'id':'b371fba4-797d-4c2c-8adc-bedd6d9db730'}}");


                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging, criteria);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll1_AcademicPeriod_Filters()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                QueryStringFilter criteria = new QueryStringFilter("criteria", @"{'academicPeriod':{'id':'e9d544d7-d7cc-47e6-84b7-77d414e5d1d3'}}");
                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging, criteria);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll1_fundingSource_Filters()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                QueryStringFilter criteria = new QueryStringFilter("criteria", @"{'fundingSource':{'id':'b371fba4-797d-4c2c-8adc-bedd6d9db730'}}");
                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging, criteria);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll1_fundingDestination_Filters()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                QueryStringFilter criteria = new QueryStringFilter("criteria", @"{'fundingDestination':{'id':'b371fba4-797d-4c2c-8adc-bedd6d9db730'}}");
                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging, criteria);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetAll1_chargeType_Filters()
            {
                _studentChargesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(_studentCharge1Dtos, It.IsAny<int>());
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                QueryStringFilter criteria = new QueryStringFilter("criteria", @"{'chargeType':'tuition'}");
                Paging paging = null;
                var results = await _studentChargesController.GetAsync1(paging, criteria);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentCharge1>;

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);


                }
            }


            [TestMethod]
            public async Task StudentChargesController_GetById()
            {
                string id = "af4d47eb-f06b-4add-b5bf-d9529742387a";
                var expected = _studentChargeDtos[0];

                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentChargesController.GetByIdAsync(id);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);               
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);

                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                if (expected.ChargedAmount.Amount != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.Amount.Currency, actual.ChargedAmount.Amount.Currency);
                    Assert.AreEqual(expected.ChargedAmount.Amount.Value, actual.ChargedAmount.Amount.Value);
                }
                else if (expected.ChargedAmount.UnitCost != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Currency, actual.ChargedAmount.UnitCost.Cost.Currency);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Quantity, actual.ChargedAmount.UnitCost.Quantity);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Value, actual.ChargedAmount.UnitCost.Cost.Value);                
                }
            }

            [TestMethod]
            public async Task StudentChargesController_GetById1()
            {
                string id = "af4d47eb-f06b-4add-b5bf-d9529742387a";
                var expected = _studentCharge1Dtos[0];

                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentChargesController.GetByIdAsync1(id);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);

                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                if (expected.ChargedAmount.Amount != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.Amount.Currency, actual.ChargedAmount.Amount.Currency);
                    Assert.AreEqual(expected.ChargedAmount.Amount.Value, actual.ChargedAmount.Amount.Value);
                }
                else if (expected.ChargedAmount.UnitCost != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Currency, actual.ChargedAmount.UnitCost.Cost.Currency);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Quantity, actual.ChargedAmount.UnitCost.Quantity);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Value, actual.ChargedAmount.UnitCost.Cost.Value);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync_PermissionException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                 await _studentChargesController.GetAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync1_PermissionException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await _studentChargesController.GetAsync1(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync_ArgumentException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await _studentChargesController.GetAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync1_ArgumentException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await _studentChargesController.GetAsync1(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync_RepositoryException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await _studentChargesController.GetAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync1_RepositoryException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await _studentChargesController.GetAsync1(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync_IntegrationApiException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                 await _studentChargesController.GetAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync1_IntegrationApiException()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await _studentChargesController.GetAsync1(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync_Exception()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                 await _studentChargesController.GetAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetAsync1_Exception()
            {
                _studentChargesServiceMock.Setup(i => i.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                await _studentChargesController.GetAsync1(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_PermissionException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_PermissionException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await _studentChargesController.GetByIdAsync1("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_KeyNotFoundException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_KeyNotFoundException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await _studentChargesController.GetByIdAsync1("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_ArgumentNullException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
               await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_ArgumentNullException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                await _studentChargesController.GetByIdAsync1(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_RepositoryException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                 await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_RepositoryException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await _studentChargesController.GetByIdAsync1("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_IntegrationApiException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_IntegrationApiException()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await _studentChargesController.GetByIdAsync1("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById_Exception()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                await _studentChargesController.GetByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_GetById1_Exception()
            {
                _studentChargesServiceMock.Setup(i => i.GetByIdAsync1(It.IsAny<string>())).ThrowsAsync(new Exception());
                await _studentChargesController.GetByIdAsync1(It.IsAny<string>());
            }

            #region PUT POST DELETE
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_PUT_Not_Supported()
            {
                await _studentChargesController.UpdateAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentCharge>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_PUT_Not_SupportedV11()
            {
                await _studentChargesController.UpdateAsync1(It.IsAny<string>(), It.IsAny<Dtos.StudentCharge1>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_POST_Not_Supported()
            {
                await _studentChargesController.CreateAsync(It.IsAny<Dtos.StudentCharge>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_DELETE_Not_Supported()
            {
                await _studentChargesController.DeleteAsync(It.IsAny<string>());
            }

            [TestMethod]
            public async Task StudentChargesController_CreateAsync1()
            {
                var expected = _studentCharge1Dtos[3];

                _studentChargesServiceMock.Setup(i => i.CreateAsync1(It.IsAny<Dtos.StudentCharge1>())).ReturnsAsync(expected);

                var actual = await _studentChargesController.CreateAsync1(_studentCharge1Dtos[3]);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);

                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                if (expected.ChargedAmount.Amount != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.Amount.Currency, actual.ChargedAmount.Amount.Currency);
                    Assert.AreEqual(expected.ChargedAmount.Amount.Value, actual.ChargedAmount.Amount.Value);
                }
                else if (expected.ChargedAmount.UnitCost != null)
                {
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Currency, actual.ChargedAmount.UnitCost.Cost.Currency);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Quantity, actual.ChargedAmount.UnitCost.Quantity);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost.Cost.Value, actual.ChargedAmount.UnitCost.Cost.Value);
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_PermissionException()
            {
                _studentChargesServiceMock.Setup(i => i.CreateAsync1(It.IsAny<Dtos.StudentCharge1>())).ThrowsAsync(new PermissionsException());
                await _studentChargesController.CreateAsync1(_studentCharge1Dtos[3]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_ArgumentException()
            {
                _studentChargesServiceMock.Setup(i => i.CreateAsync1(It.IsAny<Dtos.StudentCharge1>())).ThrowsAsync(new ArgumentException());
                await _studentChargesController.CreateAsync1(It.IsAny<Dtos.StudentCharge1>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_ArgumentNullException()
            {
                await _studentChargesController.CreateAsync1(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_ArgumentNullException_GUID()
            {
                await _studentChargesController.CreateAsync1(_studentCharge1Dtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_RepositoryException()
            {
                _studentChargesServiceMock.Setup(i => i.CreateAsync1(It.IsAny<Dtos.StudentCharge1>())).ThrowsAsync(new RepositoryException());
                await _studentChargesController.CreateAsync1(_studentCharge1Dtos[3]);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentChargesController_CreateAsync1_Exception()
            {
                _studentChargesServiceMock.Setup(i => i.CreateAsync1(It.IsAny<Dtos.StudentCharge1>())).ThrowsAsync(new Exception());
                await _studentChargesController.CreateAsync1( _studentCharge1Dtos[3]);
            }

            #endregion

            private void BuildData()
            {
                #region Building StudentCharges
                _studentChargeDtos = new List<StudentCharge>() 
                {
                    new StudentCharge(){ 
                        Id = "54c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("e9d544d7-d7cc-47e6-84b7-77d414e5d1d3"),
                        AccountingCode = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        AccountReceivableType = new GuidObject2( "2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-13"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300 },
                            
                        },
                        ChargeType = StudentChargeTypes.tuition,
                        Comments = new List<string>() { "This is a comment" },
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")
                       
                    },
                     new StudentCharge(){ 
                        Id = "62c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        AccountingCode = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        AccountReceivableType = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-16"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                           
                            UnitCost = new ChargedAmountUnitCostDtoProperty()
                            {
                                Cost = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300 },
                                Quantity = 8
                            }
                        },
                        ChargeType = StudentChargeTypes.housing,
                        Comments = new List<string>() { "This is a comment" },
                        Person = new GuidObject2("ea7b7e26-3b78-4257-a365-857d317a97af")
                       
                    },
                     new StudentCharge(){ 
                        Id = "62c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("e9d544d7-d7cc-47e6-84b7-77d414e5d1d3"),
                        AccountingCode = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        AccountReceivableType = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-16"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300},
                           
                            UnitCost = new ChargedAmountUnitCostDtoProperty()
                            {
                                Cost = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  -300 },
                                Quantity = 0
                            }
                        },
                        ChargeType = StudentChargeTypes.housing,
                        Comments = new List<string>() { "Test" },
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")
                    },
                    
                };
                #endregion
            }

            private void BuildData1()
            {
                #region Building StudentCharges
                _studentCharge1Dtos = new List<StudentCharge1>()
                {
                    new StudentCharge1(){
                        Id = "54c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("e9d544d7-d7cc-47e6-84b7-77d414e5d1d3"),
                        FundingSource = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        FundingDestination = new GuidObject2( "2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-13"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300 },

                        },
                        ChargeType = StudentChargeTypes.tuition,
                        Comments = new List<string>() { "This is a comment" },
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")

                    },
                     new StudentCharge1(){
                        Id = "62c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        FundingSource = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        FundingDestination = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-16"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {

                            UnitCost = new ChargedAmountUnitCostDtoProperty()
                            {
                                Cost = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300 },
                                Quantity = 8
                            }
                        },
                        ChargeType = StudentChargeTypes.housing,
                        Comments = new List<string>() { "This is a comment" },
                        Person = new GuidObject2("ea7b7e26-3b78-4257-a365-857d317a97af")

                    },
                     new StudentCharge1(){
                        Id = "62c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("e9d544d7-d7cc-47e6-84b7-77d414e5d1d3"),
                        FundingSource = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        FundingDestination = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-16"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300},

                            UnitCost = new ChargedAmountUnitCostDtoProperty()
                            {
                                Cost = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  -300 },
                                Quantity = 0
                            }
                        },
                        ChargeType = StudentChargeTypes.housing,
                        Comments = new List<string>() { "Test" },
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")
                    },
                     new StudentCharge1(){
                        Id = "00000000-0000-0000-0000-000000000000",
                        AcademicPeriod = new GuidObject2("e9d544d7-d7cc-47e6-84b7-77d414e5d1d3"),
                        FundingSource = new GuidObject2("3693d5df-51c1-4071-aa53-164badfb2986"),
                        FundingDestination = new GuidObject2("2ed632ee-e4a8-4771-af46-220c245b3e74"),
                        ChargeableOn = Convert.ToDateTime("2016-10-16"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  300},

                            UnitCost = new ChargedAmountUnitCostDtoProperty()
                            {
                                Cost = new AmountDtoProperty(){ Currency = CurrencyCodes.USD, Value =  -300 },
                                Quantity = 0
                            }
                        },
                        ChargeType = StudentChargeTypes.housing,
                        Comments = new List<string>() { "Test" },
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")
                    },



                };
                #endregion
            }

        }
    }
}