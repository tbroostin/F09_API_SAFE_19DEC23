﻿//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class RequisitionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IRequisitionService> requisitionsServiceMock;
        private Mock<ILogger> loggerMock;
        private RequisitionsController requisitionsController;
        private List<Dtos.Requisitions> requisitionsCollection;
        private Tuple<IEnumerable<Dtos.Requisitions>, int> requisiotionsCollectionTuple;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        int offset = 0;
        int limit = 2;

        private string guid = "83f78f38-cb00-403b-a107-557dabf0f451";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            requisitionsServiceMock = new Mock<IRequisitionService>();
            loggerMock = new Mock<ILogger>();
            requisitionsCollection = new List<Dtos.Requisitions>();

            BuildData();

            requisitionsController = new RequisitionsController(requisitionsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            requisitionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            requisitionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            requisitionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(requisitionsCollection.FirstOrDefault()));
        }

        private void BuildData()
        {
            requisitionsCollection = new List<Requisitions>()
            {
                new Requisitions()
                {
                    Id = expectedGuid,
                   Buyer = new GuidObject2("ef200d4f-9d98-471f-871b-e09dc95a3be0"),
                   Comments = new List<CommentsDtoProperty>()
                   {
                        new CommentsDtoProperty() { Comment = "Comment 1", Type = Dtos.EnumProperties.CommentTypes.NotPrinted },
                        new CommentsDtoProperty() { Comment = "Comment 2", Type = Dtos.EnumProperties.CommentTypes.Printed }
                   },
                   DeliveredBy = DateTime.Today.AddDays(2),
                   Initiator = new Dtos.DtoProperties.InitiatorDtoProperty()
                   {
                       Detail = new GuidObject2("4512560d-4a81-465a-a225-37d75c834a02"),
                       Email = "email1@abc.com",
                       Fax = new PhoneDtoProperty()
                       {
                           Extension = "555",
                           Guid = "3eb53c38-3c10-409b-9d5e-127578e21b3a",
                           Number = "6171112233" ,
                           PhoneType = PhoneType.Work
                       },
                       Phone = new PhoneDtoProperty()
                       {
                           Extension = "444",
                           Guid = "2eb53c38-3c10-409b-9d5e-127578e21b3b",
                           Number = "6172223344" ,
                           PhoneType = PhoneType.Mobile
                       }
                   },
                   LineItems = new List<RequisitionsLineItemsDtoProperty>()
                   {
                       new RequisitionsLineItemsDtoProperty()
                       {
                          AccountDetail = new List<Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty>()
                          {
                              new Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty()
                              {
                                  AccountingString = "",
                                  Allocation = new Dtos.DtoProperties.RequisitionsAllocationDtoProperty()
                                  {
                                      AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() {Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 200.00m },
                                      Allocated = new Dtos.DtoProperties.RequisitionsAllocatedDtoProperty()
                                      {
                                          Amount = new Dtos.DtoProperties.Amount2DtoProperty() {Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 50.00m },
                                          Percentage = 100,
                                          Quantity = 1
                                      },
                                      DiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                      {
                                          Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                          Value = 5
                                      },
                                      TaxAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                      {
                                          Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                          Value = 5.75m
                                      }
                                  },
                                  BudgetCheck = Dtos.EnumProperties.PurchaseOrdersAccountBudgetCheck.Override
                              }
                          },                          
                          AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                          {
                              Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                              Value = 20
                          },
                          Comments = new List<CommentsDtoProperty>()
                          {
                              new CommentsDtoProperty() {Comment = "Comment", Type = Dtos.EnumProperties.CommentTypes.Printed  }
                          },
                          CommodityCode = new GuidObject2("8cdcd418-ddbe-4ade-b724-144af2590b56"),
                          Description = "Description 1",
                          DesiredDate = DateTime.Today.AddDays(1),
                          DiscountAmount = "10",                          
                          LineItemNumber = "1",
                          PartNumber = "25",
                          Quantity = 1,
                          Taxes = new List<GuidObject2>() {new GuidObject2("ac32d9bb-269a-45be-b276-1d0b27d012c0"), new GuidObject2("f3580d8d-6165-4a0f-aeb6-acd866c03bcf") },
                          TradeDiscount = new TradeDiscountDtoProperty()
                          {
                              Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                              {
                                  Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                  Value = 10
                              },
                              Percent = 2
                          },
                          UnitOfMeasure = new GuidObject2("ecee60c6-1f31-4540-ae71-4d9ef340df6d"),
                          UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty()
                          {
                              Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                              Value = 45
                          }
                       }
                   },
                   OverrideShippingDestination = new OverrideShippingDestinationDtoProperty()
                   {
                      AddressLines = new List<string>() {"123 Some str", "1234 Some str" },
                      Contact = new PhoneDtoProperty()
                       {
                           Extension = "444",
                           Guid = "4eb53c38-3c10-409b-9d5e-127578e21b3c",
                           Number = "6172224455" ,
                           PhoneType = PhoneType.Mobile
                       },
                      Description = "OverrideShippingDestination description",
                      Place = new AddressPlace()
                      {
                          Country = new AddressCountry()
                          {
                              CarrierRoute = "CarrierRoute",
                              Code = Dtos.EnumProperties.IsoCode.USA,
                              CorrectionDigit = "CorrectionDigit",
                              DeliveryPoint = "DeliveryPoint",
                              Locality = "Locality",
                              PostalCode = "02144",
                              PostalTitle = "PostalTitle",
                              Region = new AddressRegion() {Code = "Code1", Title = "Title1" }
                          }
                      }                      
                   },
                   PaymentTerms = new GuidObject2("7fdffea9-6a2b-4eb4-9835-339bc0a7c3ba"),
                   ReferenceNumber = "ReferenceNumber 1",
                   RequestedOn = DateTime.Today,
                   RequisitionNumber = "RequisitionNumber 1",
                   Shipping = new Dtos.DtoProperties.ShippingDtoProperty()
                   {
                       Attention = "Attention",
                       FreeOnBoard = new GuidObject2("0c43b649-418b-47be-98da-c82559064104"),
                       ShipTo = new GuidObject2("a37d1d27-be63-47fb-b0dc-bb6bf337e7ac")
                   },
                   Status = Dtos.EnumProperties.RequisitionsStatus.Inprogress,
                   SubmittedBy = new GuidObject2("d5c95b06-6894-4b2a-976a-3769cd6151b9"),
                   TransactionDate = DateTime.Today,
                   Vendor = new VendorDtoProperty()
                   {
                       ExistingVendor = new ExistingVendorDetailsDtoProperty()
                       {
                           AlternativeVendorAddress = new GuidObject2("f01074f1-1be5-4247-a300-040368b9e688"),
                           Vendor = new GuidObject2("c25c1e78-240f-4e79-bfa8-d6b2fb34bef4")
                       }
                   }
                },
                new Requisitions()
                {
                    Id = "0e86c3ce-9423-41a8-aba4-b038078da80c",
                   Buyer = new GuidObject2("c1db43fa-a0c1-49bf-8830-f1517f908c81"),
                   Comments = new List<CommentsDtoProperty>()
                   {
                        new CommentsDtoProperty() { Comment = "Comment 1", Type = Dtos.EnumProperties.CommentTypes.NotPrinted },
                        new CommentsDtoProperty() { Comment = "Comment 2", Type = Dtos.EnumProperties.CommentTypes.Printed }
                   },
                   DeliveredBy = DateTime.Today.AddDays(2),
                   Initiator = new Dtos.DtoProperties.InitiatorDtoProperty()
                   {
                       Detail = new GuidObject2("dc899d25-a8c6-46a8-8d5d-b90ce7e8b293"),
                       Email = "email1@abc.com",
                       Fax = new PhoneDtoProperty()
                       {
                           Extension = "555",
                           Guid = "1f036fd5-62df-414d-bcb5-09a4eee4535b",
                           Number = "6171112233" ,
                           PhoneType = PhoneType.Work
                       }
                   },
                   LineItems = new List<RequisitionsLineItemsDtoProperty>()
                   {
                       new RequisitionsLineItemsDtoProperty()
                       {
                          AccountDetail = new List<Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty>()
                          {
                              new Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty()
                              {
                                  AccountingString = "",
                                  Allocation = new Dtos.DtoProperties.RequisitionsAllocationDtoProperty()
                                  {
                                      AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() {Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 200.00m },
                                      Allocated = new Dtos.DtoProperties.RequisitionsAllocatedDtoProperty()
                                      {
                                          Amount = new Dtos.DtoProperties.Amount2DtoProperty() {Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 50.00m },
                                          Percentage = 100,
                                          Quantity = 1
                                      },
                                      DiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                      {
                                          Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                          Value = 5
                                      },
                                      TaxAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                      {
                                          Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                          Value = 5.75m
                                      }
                                  },
                                  BudgetCheck = Dtos.EnumProperties.PurchaseOrdersAccountBudgetCheck.Override
                              }
                          },
                          AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                          {
                              Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                              Value = 20
                          },
                          Comments = new List<CommentsDtoProperty>()
                          {
                              new CommentsDtoProperty() {Comment = "Comment", Type = Dtos.EnumProperties.CommentTypes.Printed  }
                          },
                          CommodityCode = new GuidObject2("b5c8b241-3393-4603-8db9-fa026df82699"),
                          Description = "Description 1",
                          DesiredDate = DateTime.Today.AddDays(1),
                          DiscountAmount = "10",
                          LineItemNumber = "1",
                          PartNumber = "25",
                          Quantity = 1,
                          Taxes = new List<GuidObject2>() {new GuidObject2("18bdfb8a-c4cc-4bdf-96ec-237554136265"), new GuidObject2("047b0c68-1c31-48ce-879c-508d0a8a8f82") },
                          TradeDiscount = new TradeDiscountDtoProperty()
                          {
                              Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                              {
                                  Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                                  Value = 10
                              },
                              Percent = 2
                          },
                          UnitOfMeasure = new GuidObject2("458753f6-225d-4863-9e8e-b746b20ff246"),
                          UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty()
                          {
                              Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                              Value = 45
                          }
                       }
                   }
                }
            };
            requisiotionsCollectionTuple = new Tuple<IEnumerable<Requisitions>, int>(requisitionsCollection, requisitionsCollection.Count);
            requisitionsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            //requisitionsServiceMock.Setup(s => s.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).ReturnsAsync(true);
            requisitionsServiceMock.Setup(s => s.GetRequisitionsByGuidAsync(It.IsAny<string>(), true)).ReturnsAsync(requisitionsCollection.FirstOrDefault());
        }

        [TestCleanup]
        public void Cleanup()
        {
            requisitionsController = null;
            requisitionsCollection = null;
            loggerMock = null;
            requisitionsServiceMock = null;
        }

        #region GET & GETALL

        [TestMethod]
        public async Task RequisitionsController_GetRequisitions_ValidateFields_Nocache()
        {
            requisitionsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(requisiotionsCollectionTuple);

            var requisitions = await requisitionsController.GetRequisitionsAsync(new Web.Http.Models.Paging(limit, offset));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await requisitions.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Requisitions>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Requisitions>;

            Assert.AreEqual(requisitionsCollection.Count, actuals.Count());            
        }

        [TestMethod]
        public async Task RequisitionsController_GetRequisitions_ValidateFields_Cache()
        {
            requisitionsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(requisiotionsCollectionTuple);

            var requisitions = await requisitionsController.GetRequisitionsAsync(new Web.Http.Models.Paging(limit, offset));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await requisitions.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Dtos.Requisitions>>)httpResponseMessage.Content)
                                .Value as IEnumerable<Dtos.Requisitions>;
            Assert.IsNotNull(actuals);

            Assert.AreEqual(requisitionsCollection.Count, actuals.Count());
        }

        [TestMethod]
        public async Task RequisitionsController_GetRequisitionsByGuidAsync_ValidateFields()
        {
            requisitionsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = requisitionsCollection.FirstOrDefault();
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(expected.Id, false)).ReturnsAsync(expected);

            var actual = await requisitionsController.GetRequisitionsByGuidAsync(expected.Id);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Buyer.Id, actual.Buyer.Id);
            Assert.AreEqual(expected.Comments.Count, actual.Comments.Count);
            Assert.AreEqual(expected.DeliveredBy, actual.DeliveredBy);
            Assert.AreEqual(expected.Initiator.Detail.Id, actual.Initiator.Detail.Id);
            Assert.AreEqual(expected.Initiator.Email, actual.Initiator.Email);

            Assert.AreEqual(expected.Initiator.Fax.Extension, actual.Initiator.Fax.Extension);
            Assert.AreEqual(expected.Initiator.Fax.Guid, actual.Initiator.Fax.Guid);
            Assert.AreEqual(expected.Initiator.Fax.Number, actual.Initiator.Fax.Number);
            Assert.AreEqual(expected.Initiator.Fax.PhoneType, actual.Initiator.Fax.PhoneType);

            Assert.AreEqual(expected.Initiator.Name, actual.Initiator.Name);

            Assert.AreEqual(expected.Initiator.Phone.Extension, actual.Initiator.Phone.Extension);
            Assert.AreEqual(expected.Initiator.Phone.Guid, actual.Initiator.Phone.Guid);
            Assert.AreEqual(expected.Initiator.Phone.Number, actual.Initiator.Phone.Number);
            Assert.AreEqual(expected.Initiator.Phone.PhoneType, actual.Initiator.Phone.PhoneType);

            Assert.AreEqual(expected.LineItems.Count, actual.LineItems.Count);

            for (int i = 0; i < actual.LineItems.Count; i++)
            {
                Assert.AreEqual(expected.LineItems[i].AccountDetail.Count, actual.LineItems[i].AccountDetail.Count);
                Assert.AreEqual(expected.LineItems[i].AdditionalAmount, actual.LineItems[i].AdditionalAmount);
                Assert.AreEqual(expected.LineItems[i].Comments.Count, actual.LineItems[i].Comments.Count);
                Assert.AreEqual(expected.LineItems[i].CommodityCode.Id, actual.LineItems[i].CommodityCode.Id);
                Assert.AreEqual(expected.LineItems[i].Description, actual.LineItems[i].Description);
                Assert.AreEqual(expected.LineItems[i].DesiredDate, actual.LineItems[i].DesiredDate);
                Assert.AreEqual(expected.LineItems[i].DiscountAmount, actual.LineItems[i].DiscountAmount);
                Assert.AreEqual(expected.LineItems[i].LineItemNumber, actual.LineItems[i].LineItemNumber);
                Assert.AreEqual(expected.LineItems[i].PartNumber, actual.LineItems[i].PartNumber);
                Assert.AreEqual(expected.LineItems[i].Quantity, actual.LineItems[i].Quantity);
                Assert.AreEqual(expected.LineItems[i].Taxes.Count, actual.LineItems[i].Taxes.Count);
                Assert.AreEqual(expected.LineItems[i].TradeDiscount.Amount, actual.LineItems[i].TradeDiscount.Amount);
                Assert.AreEqual(expected.LineItems[i].TradeDiscount.Percent, actual.LineItems[i].TradeDiscount.Percent);
            }

            Assert.AreEqual(expected.OverrideShippingDestination.AddressLines.Count, actual.OverrideShippingDestination.AddressLines.Count);
            Assert.AreEqual(expected.OverrideShippingDestination.Contact.Extension, actual.OverrideShippingDestination.Contact.Extension);
            Assert.AreEqual(expected.OverrideShippingDestination.Contact.Guid, actual.OverrideShippingDestination.Contact.Guid);
            Assert.AreEqual(expected.OverrideShippingDestination.Contact.Number, actual.OverrideShippingDestination.Contact.Number);
            Assert.AreEqual(expected.OverrideShippingDestination.Contact.PhoneType, actual.OverrideShippingDestination.Contact.PhoneType);
            Assert.AreEqual(expected.OverrideShippingDestination.Description, actual.OverrideShippingDestination.Description);
            Assert.AreEqual(expected.OverrideShippingDestination.Place.Country, actual.OverrideShippingDestination.Place.Country);

            Assert.AreEqual(expected.PaymentTerms.Id, actual.PaymentTerms.Id);
            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
            Assert.AreEqual(expected.PaymentTerms.Id, actual.PaymentTerms.Id);
            Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);

            Assert.AreEqual(expected.RequestedOn, actual.RequestedOn);
            Assert.AreEqual(expected.RequisitionNumber, actual.RequisitionNumber);

            Assert.AreEqual(expected.Shipping.Attention, actual.Shipping.Attention);
            Assert.AreEqual(expected.Shipping.FreeOnBoard.Id, actual.Shipping.FreeOnBoard.Id);
            Assert.AreEqual(expected.Shipping.ShipTo.Id, actual.Shipping.ShipTo.Id);

            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.SubmittedBy.Id, actual.SubmittedBy.Id);
            Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);

            Assert.AreEqual(expected.Vendor.ExistingVendor.AlternativeVendorAddress.Id, actual.Vendor.ExistingVendor.AlternativeVendorAddress.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_KeyNotFoundException()
        {            
            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_PermissionsException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_ArgumentException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_RepositoryException()
        {

            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_IntegrationApiException()
        {

            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }       

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitions_Exception()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<Exception>();

            await requisitionsController.GetRequisitionsAsync(It.IsAny<Web.Http.Models.Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuidAsync_Exception()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await requisitionsController.GetRequisitionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_KeyNotFoundException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_PermissionsException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_ArgumentException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_RepositoryException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_IntegrationApiException()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionsByGuid_Exception()
        {
            requisitionsServiceMock.Setup(x => x.GetRequisitionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await requisitionsController.GetRequisitionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionAsync_Id_Null()
        {
            await requisitionsController.GetRequisitionAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionAsync_ArgumentNullException()
        {
            requisitionsServiceMock.Setup(r => r.GetRequisitionAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await requisitionsController.GetRequisitionAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionAsync_Exception()
        {
            requisitionsServiceMock.Setup(r => r.GetRequisitionAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await requisitionsController.GetRequisitionAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionAsync_KeyNotFoundException()
        {
            requisitionsServiceMock.Setup(r => r.GetRequisitionAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await requisitionsController.GetRequisitionAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_GetRequisitionAsync_ApplicationException()
        {
            requisitionsServiceMock.Setup(r => r.GetRequisitionAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await requisitionsController.GetRequisitionAsync("1");
        }

        [TestMethod]
        public async Task RequisitionsController_GetRequisitionAsync_By_RequisitionId()
        {
            requisitionsServiceMock.Setup(r => r.GetRequisitionAsync(It.IsAny<string>())).ReturnsAsync(new Dtos.ColleagueFinance.Requisition() { Id = guid });
            var result = await requisitionsController.GetRequisitionAsync("1");

            Assert.IsNotNull(result);
        }

        #endregion

        #region Delete

        [TestMethod]
        public async Task RequisitionsController_DeleteRequisitionsAsync()
        {
            System.Net.Http.HttpResponseMessage httpResponseMessage = new HttpResponseMessage() {StatusCode = System.Net.HttpStatusCode.OK };
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync("1234")).Returns(Task.FromResult(httpResponseMessage));
            await requisitionsController.DeleteRequisitionsAsync("1234");                
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_Null_Id_ArgumentNullException()
        {
            await requisitionsController.DeleteRequisitionsAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_PermissionsException()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_KeyNotFoundException()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_IntegrationApiException()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_InvalidOperationException()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<InvalidOperationException>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_RepositoryException()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_DeleteRequisitionsAsync_Exception()
        {
            requisitionsServiceMock.Setup(x => x.DeleteRequisitionAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await requisitionsController.DeleteRequisitionsAsync("1234");
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_Dto_Null()
        {
            await requisitionsController.PostRequisitionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_Dto_Id_Null()
        {
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = string.Empty });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_KeyNotFoundException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new KeyNotFoundException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_PermissionsException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new PermissionsException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_ArgumentException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new ArgumentException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_RepositoryException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new RepositoryException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_IntegrationApiException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new IntegrationApiException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_ConfigurationException()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new ConfigurationException());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PostRequisitionsAsync_Exception()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new Exception());
            await requisitionsController.PostRequisitionsAsync(new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        public async Task RequisitionsController_PostRequisitionsAsync()
        {
            requisitionsServiceMock.Setup(r => r.CreateRequisitionsAsync(It.IsAny<Requisitions>())).ReturnsAsync(requisitionsCollection.FirstOrDefault());
            var result = await requisitionsController.PostRequisitionsAsync(requisitionsCollection.FirstOrDefault());

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, requisitionsCollection.FirstOrDefault().Id);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_Guid_Null()
        {
            await requisitionsController.PutRequisitionsAsync(string.Empty, new Requisitions());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_Dto_Null()
        {
            await requisitionsController.PutRequisitionsAsync(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_Guid_Empty()
        {
            await requisitionsController.PutRequisitionsAsync(Guid.Empty.ToString(), new Requisitions());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_Dto_Id_NotSameAs_Guid()
        {
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_PermissionsException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new PermissionsException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_ArgumentException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new ArgumentException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_RepositoryException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new RepositoryException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_IntegrationApiException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new IntegrationApiException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_ConfigurationException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new ConfigurationException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_KeyNotFoundException()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new KeyNotFoundException());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RequisitionsController_PutRequisitionsAsync_Exception()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ThrowsAsync(new Exception());
            await requisitionsController.PutRequisitionsAsync(guid, new Requisitions() { Id = string.Empty });
        }

        [TestMethod]
        public async Task RequisitionsController_PutRequisitionsAsync()
        {
            requisitionsServiceMock.Setup(r => r.UpdateRequisitionsAsync(It.IsAny<Requisitions>())).ReturnsAsync(requisitionsCollection.FirstOrDefault());
            var result = await requisitionsController.PutRequisitionsAsync(requisitionsCollection.FirstOrDefault().Id, requisitionsCollection.FirstOrDefault());
            Assert.IsNotNull(result);
        }

        #endregion
    }
}