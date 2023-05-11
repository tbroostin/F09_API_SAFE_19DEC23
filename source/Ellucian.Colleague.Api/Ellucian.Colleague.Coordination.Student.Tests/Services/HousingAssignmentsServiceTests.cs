//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using System.Collections;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class HousingAssignmentsServiceTests
    {
        [TestClass]
        public class HousingAssignmentServiceTests_GETALL_GETBYID : StudentUserFactory
        {
            #region DECLCARATIONS

            protected Domain.Entities.Role viewHousingAssignment = new Domain.Entities.Role(1, "VIEW.ROOM.ASSIGNMENT");

            private Mock<IHousingAssignmentRepository> housingAssignmentRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private HousingAssignmentUser currentUserFactory;

            private HousingAssignmentService housingAssignmentService;

            private Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int> domainHousingAssignmentsTuple;
            private IEnumerable<HousingAssignmentAdditionalChargeProperty> additionalCharges;
            private Dictionary<string, string> dicPersons;
            private IEnumerable<Domain.Base.Entities.Room> rooms;
            private IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            private IEnumerable<Domain.Student.Entities.RoomRate> roomRates;
            private IEnumerable<Domain.Student.Entities.HousingAssignmentStatus> statuses;
            private IEnumerable<Domain.Student.Entities.BillingOverrideReasons> overrideReasons;
            private IEnumerable<Domain.Student.Entities.ArAdditionalAmount> additionalAmounts;
            private IEnumerable<Domain.Student.Entities.AccountingCode> accountCodes;
            private IEnumerable<Domain.Student.Entities.HousingResidentType> residentTypes;
            private List<Domain.Student.Entities.HousingAssignment> housingAssignments;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                housingAssignmentRepositoryMock = new Mock<IHousingAssignmentRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new HousingAssignmentUser();

                InitializeTestData();

                InitializeTestMock();

                housingAssignmentService = new HousingAssignmentService(housingAssignmentRepositoryMock.Object, personRepositoryMock.Object,
                    termRepositoryMock.Object, roomRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            private void InitializeTestData()
            {
                rooms = new List<Domain.Base.Entities.Room>()
                { new Domain.Base.Entities.Room("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1*1", "Description1") };

                dicPersons = new Dictionary<string, string>()
                { { "1", "1a59eed8-5fe7-4120-b1cf-f23266b9e874"} };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                { new Domain.Student.Entities.AcademicPeriod("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc", DateTime.Today, DateTime.Today.AddDays(100), 2016, 1, "TERM1", "P", "P", null) };

                roomRates = new List<Domain.Student.Entities.RoomRate>()
                { new RoomRate("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "DESC") };

                statuses = new List<Domain.Student.Entities.HousingAssignmentStatus>()
                { new HousingAssignmentStatus() { Status = "1", StatusDate = DateTime.Today } };

                overrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>()
                { new Domain.Student.Entities.BillingOverrideReasons("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") };

                additionalAmounts = new List<Domain.Student.Entities.ArAdditionalAmount>()
                {new ArAdditionalAmount() { AraaArCode = "1", AraaChargeAmt = 100, AraaCrAmt = 100 } };

                accountCodes = new List<Domain.Student.Entities.AccountingCode>()
                { new Domain.Student.Entities.AccountingCode("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") {} };

                residentTypes = new List<Domain.Student.Entities.HousingResidentType>()
                {new HousingResidentType("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") {} };

                additionalCharges = new List<HousingAssignmentAdditionalChargeProperty>()
                {
                    new HousingAssignmentAdditionalChargeProperty()
                    {
                        AccountingCode = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                        HousingAssignmentRate = new HousingAssignmentRateChargeProperty() {RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.USD, RateValue =2 }
                    }
                };

                housingAssignments = new List<Domain.Student.Entities.HousingAssignment>()
                {
                    new Domain.Student.Entities.HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(100))
                    {
                        Building = "1",
                        Term = "1",
                        Statuses = statuses,
                        RoomRate = "1",
                        RoomRateTable = "1",
                        RateOverride = 100,
                        RateOverrideReason = "1",
                        ResidentStaffIndicator = "1",
                        ArAdditionalAmounts = additionalAmounts

                    }
                };

                domainHousingAssignmentsTuple = new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(housingAssignments, housingAssignments.Count());
            }

            private void InitializeTestMock()
            {
                viewHousingAssignment.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewHousingAssignment));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewHousingAssignment });

                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(domainHousingAssignmentsTuple);
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(housingAssignments.FirstOrDefault());
                housingAssignmentRepositoryMock.Setup(h => h.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dicPersons);
                personRepositoryMock.Setup(h => h.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                roomRepositoryMock.Setup(r => r.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(rooms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(s => s.GetRoomRatesAsync(It.IsAny<bool>())).ReturnsAsync(roomRates);
                studentReferenceDataRepositoryMock.Setup(s => s.GetHostCountryAsync()).ReturnsAsync("USA");
                studentReferenceDataRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Now.Date.ToString());
                studentReferenceDataRepositoryMock.Setup(s => s.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(overrideReasons);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountCodes);
                studentReferenceDataRepositoryMock.Setup(s => s.GetHousingResidentTypesAsync(It.IsAny<bool>())).ReturnsAsync(residentTypes);


            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentRepositoryMock = null;
                termRepositoryMock = null;
                roomRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                housingAssignmentService = null;
            }

            #endregion

            #region GETALL

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task HousingAssignmentService_GetHousingAssignmentsAsync_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(),It.IsAny<bool>());
            //}

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_Exception()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_Student_NotFound()
            {
                housingAssignments.Add(new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10)));
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_Room_NotFound()
            {
                housingAssignments.Add(new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "2", DateTime.Today, DateTime.Today.AddDays(10)));

                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_AcademicPeriod_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().Term = "2";
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_InvalidOperationException_When_Statuses_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().Statuses = null;
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_RoomRate_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().RoomRateTable = "2";
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_RateOverrideReason_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().RateOverrideReason = "2";
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_AdditionalAmount_AccountCode_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_KeyNotFoundException_When_ResidentType_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().ResidentStaffIndicator = "2";
                await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync()
            {
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment>(), It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_PersonFilter()
            {
                var filter = new Dtos.HousingAssignment();
                filter.Person = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_academicPeriodFilter()
            {
                var filter = new Dtos.HousingAssignment();
                filter.AcademicPeriod = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_StatusFilter()
            {
                var filter = new Dtos.HousingAssignment();
                filter.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned;
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_StartOnFilter()
            {
                var filter = new Dtos.HousingAssignment();
                filter.StartOn = DateTime.Today;
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentsAsync_EndOnFilter()
            {
                var filter = new Dtos.HousingAssignment();
                filter.EndOn = DateTime.Today.AddDays(100);
                var result = await housingAssignmentService.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task GetHousingAssignments2Async_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            //}

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetHousingAssignments2Async_Exception()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_Student_NotFound()
            {
                housingAssignments.Add(new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10)));
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_Room_NotFound()
            {
                housingAssignments.Add(new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "2", DateTime.Today, DateTime.Today.AddDays(10)));

                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_AcademicPeriod_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().Term = "2";
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_InvalidOperationException_When_Statuses_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().Statuses = null;
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentsAsync_KeyNotFoundException_When_RoomRate_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().RoomRateTable = "2";
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_RateOverrideReason_NullOrEmpty()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().RateOverrideReason = "2";
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_AdditionalAmount_AccountCode_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignments2Async_KeyNotFoundException_When_ResidentType_NotFound()
            {
                domainHousingAssignmentsTuple.Item1.FirstOrDefault().ResidentStaffIndicator = "2";
                await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async()
            {
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.HousingAssignment2>(), It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_PersonFilter_ZeroRecords()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.Person = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");

                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.Any(), false);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_AcademicPeriod_ZeroRecords()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.AcademicPeriod = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Throws(new Exception());
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.Any(), false);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_Null_AcademicPeriod_ZeroRecords()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.AcademicPeriod = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                //List<AcademicPeriod> terms = null;
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(() => null);

                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.Any(), false);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_AcademicPeriod_NoRecordMatch_ZeroRecords()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.AcademicPeriod = new Dtos.GuidObject2(Guid.NewGuid().ToString());
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.Any(), false);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_DateConvert_ZeroRecords()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.StartOn = DateTime.Today;
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.Any(), false);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetHousingAssignments2Async_PersonFilter()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.Person = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetHousingAssignmentsAsync_academicPeriodFilter()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.AcademicPeriod = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874");
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetHousingAssignmentsAsync_StatusFilter()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned;
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetHousingAssignmentsAsync_StartOnFilter()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.StartOn = DateTime.Today;
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetHousingAssignmentsAsync_EndOnFilter()
            {
                var filter = new Dtos.HousingAssignment2();
                filter.EndOn = DateTime.Today.AddDays(100);
                var result = await housingAssignmentService.GetHousingAssignments2Async(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());

                Assert.IsNotNull(result);
            }

            #endregion

            #region GETBYID

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            //}

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_Student_NotFound()
            {
                var record = new Domain.Student.Entities.HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(100)) { };
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(record);

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_Room_NotFound()
            {
                var record = new Domain.Student.Entities.HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "2", DateTime.Today, DateTime.Today.AddDays(100)) { };
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(record);

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_AcademicPeriod_NotFound()
            {
                housingAssignments.FirstOrDefault().Term = "2";

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_InvalidOperationException_When_Statuses_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().Statuses = null;

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_RoomRate_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().RoomRateTable = "2";

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_RateOverrideReason_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().RateOverrideReason = "2";

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_AdditionalAmount_AccountCode_NotFound()
            {
                housingAssignments.FirstOrDefault().ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_ResidentType_NotFound()
            {
                housingAssignments.FirstOrDefault().ResidentStaffIndicator = "2";

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_ArgumentNullException()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync_InvalidOperationException()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

                await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            public async Task HousingAssignmentService_GetHousingAssignmentByGuidAsync()
            {
                var result = await housingAssignmentService.GetHousingAssignmentByGuidAsync(It.IsAny<string>());

                Assert.IsNotNull(result);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task GetHousingAssignmentByGuid2Async_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            //}

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuid2Async_KeyNotFoundException_When_Student_NotFound()
            {
                var record = new Domain.Student.Entities.HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(100)) { };
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(record);

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuid2Async_KeyNotFoundException_When_Room_NotFound()
            {
                var record = new Domain.Student.Entities.HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "2", DateTime.Today, DateTime.Today.AddDays(100)) { };
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(record);

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuid2Async_KeyNotFoundException_When_AcademicPeriod_NotFound()
            {
                housingAssignments.FirstOrDefault().Term = "2";

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuidAsync_InvalidOperationException_When_Statuses_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().Statuses = null;

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuid2Async_KeyNotFoundException_When_RoomRate_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().RoomRateTable = "2";

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_RateOverrideReason_NullOrEmpty()
            {
                housingAssignments.FirstOrDefault().RateOverrideReason = "2";

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_AdditionalAmount_AccountCode_NotFound()
            {
                housingAssignments.FirstOrDefault().ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_ResidentType_NotFound()
            {
                housingAssignments.FirstOrDefault().ResidentStaffIndicator = "2";

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetHousingAssignmentByGuid2Async_ArgumentNullException()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetHousingAssignmentByGuidAsync_InvalidOperationException()
            {
                housingAssignmentRepositoryMock.Setup(h => h.GetHousingAssignmentByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

                await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            public async Task GetHousingAssignmentByGuidAsync()
            {
                var result = await housingAssignmentService.GetHousingAssignmentByGuid2Async(It.IsAny<string>());

                Assert.IsNotNull(result);
            }
            #endregion  
        }

        [TestClass]
        public class HousingAssignmentServiceTests_POST : StudentUserFactory
        {
            #region DECLARATION

            protected Domain.Entities.Role createHousingAssignment = new Domain.Entities.Role(1, "UPDATE.ROOM.ASSIGNMENT");

            private Mock<IHousingAssignmentRepository> housingAssignmentRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private HousingAssignmentUser currentUserFactory;

            private HousingAssignmentService housingAssignmentService;

            private Dtos.HousingAssignment dtoHousingAssingment;
            private Dtos.HousingAssignment2 dtoHousingAssingment2;
            private Domain.Student.Entities.HousingAssignment domainHousingAssignment;

            private IEnumerable<Domain.Base.Entities.Room> rooms;
            private IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            private IEnumerable<RoomRate> roomRates;
            private HousingAssignmentRateOverrideProperty rateOverride;
            private IEnumerable<Domain.Student.Entities.BillingOverrideReasons> billingOverrideReasons;
            private IEnumerable<HousingAssignmentAdditionalChargeProperty> additionalCharges;
            private HousingAssignmentRateChargeProperty rateChange;
            private IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodes;
            private IEnumerable<Domain.Student.Entities.HousingResidentType> residentTypes;
            private Dictionary<string, string> dicPersons;
            private IEnumerable<ArAdditionalAmount> additionalAmounts;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                housingAssignmentRepositoryMock = new Mock<IHousingAssignmentRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new HousingAssignmentUser();

                InitializeTestData();

                InitializeMock();

                housingAssignmentService = new HousingAssignmentService(housingAssignmentRepositoryMock.Object, personRepositoryMock.Object,
                    termRepositoryMock.Object, roomRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentRepositoryMock = null;
                termRepositoryMock = null;
                roomRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                housingAssignmentService = null;

                dtoHousingAssingment = null;
                dtoHousingAssingment2 = null;
                academicPeriods = null;
                rooms = null;
                roomRates = null;
                rateOverride = null;
                billingOverrideReasons = null;
                additionalCharges = null;
                rateChange = null;
                accountingCodes = null;
                residentTypes = null;
                dicPersons = null;
            }

            private void InitializeTestData()
            {
                additionalAmounts = new List<ArAdditionalAmount>()
                {
                    new ArAdditionalAmount()
                    {
                        AraaArCode = "1",
                        AraaChargeAmt = 100,
                        AraaCrAmt = 100,
                        AraaRoomAssignmentId = "1",
                        Recordkey = "1"
                    }
                };

                dicPersons = new Dictionary<string, string>() { { "1", "1a59eed8-5fe7-4120-b1cf-f23266b9e874" } };

                residentTypes = new List<Domain.Student.Entities.HousingResidentType>()
                {
                    new Domain.Student.Entities.HousingResidentType("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc"){ }
                };

                accountingCodes = new List<Domain.Student.Entities.AccountingCode>()
                {
                    new Domain.Student.Entities.AccountingCode("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") {}
                };

                rateChange = new HousingAssignmentRateChargeProperty()
                {
                    RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                    RateValue = 100
                };

                additionalCharges = new List<HousingAssignmentAdditionalChargeProperty>()
                {
                    new HousingAssignmentAdditionalChargeProperty()
                    {
                        AccountingCode = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                        HousingAssignmentRate = rateChange
                    }
                };

                billingOverrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>()
                {
                    new Domain.Student.Entities.BillingOverrideReasons("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc") { }
                };

                rateOverride = new HousingAssignmentRateOverrideProperty()
                {
                    HousingAssignmentRate = rateChange,
                    RateOverrideReason = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874")
                };

                roomRates = new List<RoomRate>()
                {
                    new RoomRate("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc"){ EndDate = DateTime.Today.AddDays(5) }
                };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc", DateTime.Today, DateTime.Today.AddDays(10), 2010, 1, "1", "1", "1", null){ }
                };

                rooms = new List<Domain.Base.Entities.Room>()
                {
                    new Domain.Base.Entities.Room("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1*1", "Desc"){}
                };

                dtoHousingAssingment = new Dtos.HousingAssignment()
                {
                    Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874",
                    Person = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AcademicPeriod = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(10),
                    RatePeriod = Dtos.EnumProperties.RatePeriod.Term,
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    StatusDate = DateTime.Today,
                    RoomRate = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    RateOverride = rateOverride,
                    AdditionalCharges = additionalCharges,
                    ResidentType = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    ContractNumber = "123-456-789",
                    Comment = "comment"
                };

                dtoHousingAssingment2 = new Dtos.HousingAssignment2()
                {
                    Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874",
                    Person = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AcademicPeriod = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(10),
                    RatePeriod = Dtos.EnumProperties.RatePeriod.Term,
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    StatusDate = DateTime.Today,
                    RoomRates = new List<GuidObject2>(){ new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874") },
                    RateOverride = rateOverride,
                    AdditionalCharges = additionalCharges,
                    ResidentType = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    ContractNumber = "123-456-789",
                    Comment = "comment"
                };

                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10))
                {
                    Building = "1",
                    Term = "1",
                    StatusDate = DateTime.Today,
                    Statuses = new List<HousingAssignmentStatus>() { new HousingAssignmentStatus() { Status = "A", StatusDate = DateTime.Today } },
                    ContractNumber = "123-456-789",
                    Comments = "Comments",
                    RoomRate = "1",
                    RatePeriod = "D",
                    RateOverride = 100,
                    RateOverrideReason = "1",
                    RoomRateTable = "1",
                    ArAdditionalAmounts = additionalAmounts,
                    ResidentStaffIndicator = "1"
                };
            }

            private void InitializeMock()
            {
                createHousingAssignment.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateUpdateHousingAssignment));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createHousingAssignment });

                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                roomRepositoryMock.Setup(r => r.GetRoomsAsync(true)).ReturnsAsync(rooms);
                termRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(new List<Term>());
                termRepositoryMock.Setup(r => r.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(r => r.GetRoomRatesAsync(true)).ReturnsAsync(roomRates);
                studentReferenceDataRepositoryMock.Setup(r => r.GetBillingOverrideReasonsAsync(true)).ReturnsAsync(billingOverrideReasons);
                studentReferenceDataRepositoryMock.Setup(r => r.GetAccountingCodesAsync(true)).ReturnsAsync(accountingCodes);
                studentReferenceDataRepositoryMock.Setup(r => r.GetHousingResidentTypesAsync(true)).ReturnsAsync(residentTypes);
                studentReferenceDataRepositoryMock.Setup(r => r.GetHostCountryAsync()).ReturnsAsync("USA");
                housingAssignmentRepositoryMock.Setup(r => r.GetPersonGuidsAsync(It.IsAny<List<string>>())).ReturnsAsync(dicPersons);
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateHousingAssignmentAsync_ArgumentNullException_HousingAssignment_Null()
            {
                await housingAssignmentService.CreateHousingAssignmentAsync(null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task CreateHousingAssignmentAsync_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

            //    await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            //}

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_PersonKey_Null()
            {
                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RoomEntity_Null()
            {
                dtoHousingAssingment.Room.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AcademicPeriodId_Null()
            {
                dtoHousingAssingment.AcademicPeriod.Id = null;
                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_AcademicPeriod_Null()
            {
                dtoHousingAssingment.AcademicPeriod.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_StatusDate_Null()
            {
                dtoHousingAssingment.StatusDate = null;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RoomRateId_Null()
            {
                dtoHousingAssingment.RoomRate.Id = null;
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Canceled;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RoomRate_NotFound()
            {
                dtoHousingAssingment.RoomRate.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Pending;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RoomRate_EndDate_GreaterThan_HousingAssignmentEndOn()
            {
                dtoHousingAssingment.EndOn = DateTime.Today.AddDays(15);
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Prorated;

                roomRates.FirstOrDefault().EndDate = DateTime.Today.AddDays(20);

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RateOverride_RateValue_Null()
            {
                rateOverride.HousingAssignmentRate.RateValue = null;
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Terminated;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RateOverride_RateOverrideReasonId_Null()
            {
                rateOverride.RateOverrideReason.Id = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Day;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RateOverride_RateOverrideReason_NotFound()
            {
                rateOverride.RateOverrideReason.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Month;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_AccoutingCode_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Week;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_AccoutingCodeId_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Year;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate = null;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_InValid_RateCurrency()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.NotSet;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_RateValue_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = null;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_AdditionalCharges_AccoutingCode_NotFound()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_ResidentTypeId_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = 0;
                dtoHousingAssingment.ResidentType.Id = null;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_ResidentType_NotFound()
            {
                dtoHousingAssingment.ResidentType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_ContractNumber_Invalid_Length()
            {
                dtoHousingAssingment.ContractNumber = "123-456-789-123-456";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Person_NotFound()
            {
                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10));
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Room_NotFound()
            {
                domainHousingAssignment.Building = "2";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Term_NotFound()
            {
                domainHousingAssignment.Term = "2";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_InvalidOperationException_Statuses_Empty()
            {
                domainHousingAssignment.Statuses = null;

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_RoomRate_NotFound()
            {
                domainHousingAssignment.RoomRateTable = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "L";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_RateOverrideReason_NotFound()
            {
                domainHousingAssignment.RateOverrideReason = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "B";
                domainHousingAssignment.RatePeriod = "T";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_AdditionalAmounts_AraaArCode_NotFound()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                domainHousingAssignment.RatePeriod = "A";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_ResidentStaffIndicator_NotFound()
            {
                domainHousingAssignment.ResidentStaffIndicator = "2";

                await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);
            }

            [TestMethod]
            public async Task HousingAssignmentService_CreateHousingAssignmentAsync()
            {
                domainHousingAssignment.ResidentStaffIndicator = null;

                var result = await housingAssignmentService.CreateHousingAssignmentAsync(dtoHousingAssingment);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtoHousingAssingment.Id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateHousingAssignment2Async_ArgumentNullException_HousingAssignment_Null()
            {
                await housingAssignmentService.CreateHousingAssignment2Async(null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task CreateHousingAssignment2Async_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

            //    await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            //}

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_PersonKey_Null()
            {
                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomEntity_Null()
            {
                dtoHousingAssingment2.Room.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AcademicPeriodId_Null()
            {
                dtoHousingAssingment2.AcademicPeriod.Id = null;
                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AcademicPeriod_Null()
            {
                dtoHousingAssingment2.AcademicPeriod.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_StatusDate_Null()
            {
                dtoHousingAssingment2.StatusDate = null;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRateId_Null()
            {
                dtoHousingAssingment2.RoomRates.FirstOrDefault().Id = null;
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Canceled;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRate_NotFound()
            {
                dtoHousingAssingment2.RoomRates.FirstOrDefault().Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Pending;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRate_EndDate_GreaterThan_HousingAssignmentEndOn()
            {
                dtoHousingAssingment2.EndOn = DateTime.Today.AddDays(15);
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Prorated;

                roomRates.FirstOrDefault().EndDate = DateTime.Today.AddDays(20);

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateValue_Null()
            {
                rateOverride.HousingAssignmentRate.RateValue = null;
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Terminated;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateOverrideReasonId_Null()
            {
                rateOverride.RateOverrideReason.Id = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Day;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateOverrideReason_NotFound()
            {
                rateOverride.RateOverrideReason.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Month;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCode_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Week;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCodeId_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Year;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate = null;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_InValid_RateCurrency()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.NotSet;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_RateValue_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = null;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCode_NotFound()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ResidentTypeId_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = 0;
                dtoHousingAssingment2.ResidentType.Id = null;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ResidentType_NotFound()
            {
                dtoHousingAssingment2.ResidentType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ContractNumber_Invalid_Length()
            {
                dtoHousingAssingment2.ContractNumber = "123-456-789-123-456";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Person_NotFound()
            {
                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10));
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Room_NotFound()
            {
                domainHousingAssignment.Building = "2";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Term_NotFound()
            {
                domainHousingAssignment.Term = "2";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_InvalidOperationException_Statuses_Empty()
            {
                domainHousingAssignment.Statuses = null;

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_RoomRate_NotFound()
            {
                domainHousingAssignment.RoomRateTable = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "L";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_RateOverrideReason_NotFound()
            {
                domainHousingAssignment.RateOverrideReason = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "B";
                domainHousingAssignment.RatePeriod = "T";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_AdditionalAmounts_AraaArCode_NotFound()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                domainHousingAssignment.RatePeriod = "A";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateHousingAssignment2Async_EntityToDto_KeyNotFoundException_ResidentStaffIndicator_NotFound()
            {
                domainHousingAssignment.ResidentStaffIndicator = "2";

                await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);
            }

            [TestMethod]
            public async Task HousingAssignmentService_CreateHousingAssignment2Async()
            {
                domainHousingAssignment.ResidentStaffIndicator = null;

                var result = await housingAssignmentService.CreateHousingAssignment2Async(dtoHousingAssingment2);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtoHousingAssingment.Id, result.Id);
            }
        }

        [TestClass]
        public class HousingAssignmentServiceTests_PUT : StudentUserFactory
        {
            #region DECLARATION

            protected Domain.Entities.Role createHousingAssignment = new Domain.Entities.Role(1, "UPDATE.ROOM.ASSIGNMENT");

            private Mock<IHousingAssignmentRepository> housingAssignmentRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private HousingAssignmentUser currentUserFactory;

            private HousingAssignmentService housingAssignmentService;

            private Dtos.HousingAssignment dtoHousingAssingment;
            private Dtos.HousingAssignment2 dtoHousingAssingment2;
            private Domain.Student.Entities.HousingAssignment domainHousingAssignment;

            private IEnumerable<Domain.Base.Entities.Room> rooms;
            private IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            private IEnumerable<RoomRate> roomRates;
            private HousingAssignmentRateOverrideProperty rateOverride;
            private IEnumerable<Domain.Student.Entities.BillingOverrideReasons> billingOverrideReasons;
            private IEnumerable<HousingAssignmentAdditionalChargeProperty> additionalCharges;
            private HousingAssignmentRateChargeProperty rateChange;
            private IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodes;
            private IEnumerable<Domain.Student.Entities.HousingResidentType> residentTypes;
            private Dictionary<string, string> dicPersons;
            private IEnumerable<ArAdditionalAmount> additionalAmounts;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                housingAssignmentRepositoryMock = new Mock<IHousingAssignmentRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new HousingAssignmentUser();

                InitializeTestData();

                InitializeMock();

                housingAssignmentService = new HousingAssignmentService(housingAssignmentRepositoryMock.Object, personRepositoryMock.Object,
                    termRepositoryMock.Object, roomRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingAssignmentRepositoryMock = null;
                termRepositoryMock = null;
                roomRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                housingAssignmentService = null;

                dtoHousingAssingment = null;
                academicPeriods = null;
                rooms = null;
                roomRates = null;
                rateOverride = null;
                billingOverrideReasons = null;
                additionalCharges = null;
                rateChange = null;
                accountingCodes = null;
                residentTypes = null;
                dicPersons = null;
            }

            private void InitializeTestData()
            {
                additionalAmounts = new List<ArAdditionalAmount>()
                {
                    new ArAdditionalAmount()
                    {
                        AraaArCode = "1",
                        AraaChargeAmt = 100,
                        AraaCrAmt = 100,
                        AraaRoomAssignmentId = "1",
                        Recordkey = "1"
                    }
                };

                dicPersons = new Dictionary<string, string>() { { "1", "1a59eed8-5fe7-4120-b1cf-f23266b9e874" } };

                residentTypes = new List<Domain.Student.Entities.HousingResidentType>()
                {
                    new Domain.Student.Entities.HousingResidentType("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc"){ }
                };

                accountingCodes = new List<Domain.Student.Entities.AccountingCode>()
                {
                    new Domain.Student.Entities.AccountingCode("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc") {}
                };

                rateChange = new HousingAssignmentRateChargeProperty()
                {
                    RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                    RateValue = 100
                };

                additionalCharges = new List<HousingAssignmentAdditionalChargeProperty>()
                {
                    new HousingAssignmentAdditionalChargeProperty()
                    {
                        AccountingCode = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                        HousingAssignmentRate = rateChange
                    }
                };

                billingOverrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>()
                {
                    new Domain.Student.Entities.BillingOverrideReasons("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc") { }
                };

                rateOverride = new HousingAssignmentRateOverrideProperty()
                {
                    HousingAssignmentRate = rateChange,
                    RateOverrideReason = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874")
                };

                roomRates = new List<RoomRate>()
                {
                    new RoomRate("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc"){ EndDate = DateTime.Today.AddDays(5) }
                };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "Desc", DateTime.Today, DateTime.Today.AddDays(10), 2010, 1, "1", "1", "1", null){ }
                };

                rooms = new List<Domain.Base.Entities.Room>()
                {
                    new Domain.Base.Entities.Room("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1*1", "Desc"){}
                };

                dtoHousingAssingment = new Dtos.HousingAssignment()
                {
                    Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874",
                    Person = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AcademicPeriod = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(10),
                    RatePeriod = Dtos.EnumProperties.RatePeriod.Term,
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    StatusDate = DateTime.Today,
                    RoomRate = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    RateOverride = rateOverride,
                    AdditionalCharges = additionalCharges,
                    ResidentType = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    ContractNumber = "123-456-789",
                    Comment = "comment"
                };

                dtoHousingAssingment2 = new Dtos.HousingAssignment2()
                {
                    Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874",
                    Person = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    Room = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    AcademicPeriod = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(10),
                    RatePeriod = Dtos.EnumProperties.RatePeriod.Term,
                    Status = Dtos.EnumProperties.HousingAssignmentsStatus.Assigned,
                    StatusDate = DateTime.Today,
                    RoomRates = new List<GuidObject2>() { new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874") },
                    RateOverride = rateOverride,
                    AdditionalCharges = additionalCharges,
                    ResidentType = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                    ContractNumber = "123-456-789",
                    Comment = "comment"
                };

                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10))
                {
                    Building = "1",
                    Term = "1",
                    StatusDate = DateTime.Today,
                    Statuses = new List<HousingAssignmentStatus>() { new HousingAssignmentStatus() { Status = "A", StatusDate = DateTime.Today } },
                    ContractNumber = "123-456-789",
                    Comments = "Comments",
                    RoomRate = "1",
                    RatePeriod = "D",
                    RateOverride = 100,
                    RateOverrideReason = "1",
                    RoomRateTable = "1",
                    ArAdditionalAmounts = additionalAmounts,
                    ResidentStaffIndicator = "1"
                };
            }

            private void InitializeMock()
            {
                createHousingAssignment.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateUpdateHousingAssignment));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createHousingAssignment });

                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                roomRepositoryMock.Setup(r => r.GetRoomsAsync(true)).ReturnsAsync(rooms);
                termRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(new List<Term>());
                termRepositoryMock.Setup(r => r.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(r => r.GetRoomRatesAsync(true)).ReturnsAsync(roomRates);
                studentReferenceDataRepositoryMock.Setup(r => r.GetBillingOverrideReasonsAsync(true)).ReturnsAsync(billingOverrideReasons);
                studentReferenceDataRepositoryMock.Setup(r => r.GetAccountingCodesAsync(true)).ReturnsAsync(accountingCodes);
                studentReferenceDataRepositoryMock.Setup(r => r.GetHousingResidentTypesAsync(true)).ReturnsAsync(residentTypes);
                studentReferenceDataRepositoryMock.Setup(r => r.GetHostCountryAsync()).ReturnsAsync("USA");
                housingAssignmentRepositoryMock.Setup(r => r.GetPersonGuidsAsync(It.IsAny<List<string>>())).ReturnsAsync(dicPersons);
                housingAssignmentRepositoryMock.Setup(r => r.GetHousingAssignmentKeyAsync(It.IsAny<string>())).ReturnsAsync("1");
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateHousingAssignmentAsync_ArgumentNullException_HousingAssignment_Null()
            {
                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task UpdateHousingAssignmentAsync_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

            //    await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            //}

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_PersonKey_Null()
            {
                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RoomEntity_Null()
            {
                dtoHousingAssingment.Room.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AcademicPeriodId_Null()
            {
                dtoHousingAssingment.AcademicPeriod.Id = null;
                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_AcademicPeriod_Null()
            {
                dtoHousingAssingment.AcademicPeriod.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_StatusDate_Null()
            {
                dtoHousingAssingment.StatusDate = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RoomRateId_Null()
            {
                dtoHousingAssingment.RoomRate.Id = null;
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Canceled;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RoomRate_NotFound()
            {
                dtoHousingAssingment.RoomRate.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Pending;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RoomRate_EndDate_GreaterThan_HousingAssignmentEndOn()
            {
                dtoHousingAssingment.EndOn = DateTime.Today.AddDays(15);
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Prorated;

                roomRates.FirstOrDefault().EndDate = DateTime.Today.AddDays(20);

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RateOverride_RateValue_Null()
            {
                rateOverride.HousingAssignmentRate.RateValue = null;
                dtoHousingAssingment.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Terminated;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_RateOverride_RateOverrideReasonId_Null()
            {
                rateOverride.RateOverrideReason.Id = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Day;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_RateOverride_RateOverrideReason_NotFound()
            {
                rateOverride.RateOverrideReason.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Month;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_AccoutingCode_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Week;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_AccoutingCodeId_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = null;
                dtoHousingAssingment.RatePeriod = Dtos.EnumProperties.RatePeriod.Year;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_InValid_RateCurrency()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.NotSet;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_AdditionalCharges_HousingAssignmentRate_RateValue_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_AdditionalCharges_AccoutingCode_NotFound()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_ResidentTypeId_Null()
            {
                dtoHousingAssingment.ResidentType.Id = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_KeyNotFoundException_ResidentType_NotFound()
            {
                dtoHousingAssingment.ResidentType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdateHousingAssignmentAsync_DtoToEntity_InvalidOperationException_ContractNumber_Invalid_Length()
            {
                dtoHousingAssingment.ContractNumber = "123-456-789-123-456";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Person_NotFound()
            {
                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10));
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Room_NotFound()
            {
                domainHousingAssignment.Building = "2";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_Term_NotFound()
            {
                domainHousingAssignment.Term = "2";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_InvalidOperationException_Statuses_Empty()
            {
                domainHousingAssignment.Statuses = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_RoomRate_NotFound()
            {
                domainHousingAssignment.RoomRateTable = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "C";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_RateOverrideReason_NotFound()
            {
                domainHousingAssignment.RateOverrideReason = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "T";
                domainHousingAssignment.RatePeriod = null;

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_AdditionalAmounts_AraaArCode_NotFound()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "T";
                domainHousingAssignment.RatePeriod = "W";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_EntityToDto_KeyNotFoundException_ResidentStaffIndicator_NotFound()
            {
                domainHousingAssignment.ResidentStaffIndicator = "2";
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaChargeAmt = null;
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "R";
                domainHousingAssignment.RatePeriod = "M";

                await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);
            }

            [TestMethod]
            public async Task HousingAssignmentService_UpdateHousingAssignmentAsync()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaChargeAmt = null;
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaCrAmt = null;
                domainHousingAssignment.RatePeriod = "Y";

                var result = await housingAssignmentService.UpdateHousingAssignmentAsync(guid, dtoHousingAssingment);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtoHousingAssingment.Id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateHousingAssignment2Async_ArgumentNullException_HousingAssignment_Null()
            {
                await housingAssignmentService.UpdateHousingAssignment2Async(guid, null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task UpdateHousingAssignment2Async_PermissionsException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

            //    await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            //}

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_PersonKey_Null()
            {
                personRepositoryMock.Setup(r => r.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomEntity_Null()
            {
                dtoHousingAssingment2.Room.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AcademicPeriodId_Null()
            {
                dtoHousingAssingment2.AcademicPeriod.Id = null;
                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AcademicPeriod_Null()
            {
                dtoHousingAssingment2.AcademicPeriod.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_StatusDate_Null()
            {
                dtoHousingAssingment2.StatusDate = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRateId_Null()
            {
                dtoHousingAssingment2.RoomRates.FirstOrDefault().Id = null;
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Canceled;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRate_NotFound()
            {
                dtoHousingAssingment2.RoomRates.FirstOrDefault().Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Pending;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RoomRate_EndDate_GreaterThan_HousingAssignmentEndOn()
            {
                dtoHousingAssingment2.EndOn = DateTime.Today.AddDays(15);
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Prorated;

                roomRates.FirstOrDefault().EndDate = DateTime.Today.AddDays(20);

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateValue_Null()
            {
                rateOverride.HousingAssignmentRate.RateValue = null;
                dtoHousingAssingment2.Status = Dtos.EnumProperties.HousingAssignmentsStatus.Terminated;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateOverrideReasonId_Null()
            {
                rateOverride.RateOverrideReason.Id = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Day;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_RateOverride_RateOverrideReason_NotFound()
            {
                rateOverride.RateOverrideReason.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Month;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCode_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Week;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCodeId_Null()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = null;
                dtoHousingAssingment2.RatePeriod = Dtos.EnumProperties.RatePeriod.Year;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_InValid_RateCurrency()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateCurrency = Dtos.EnumProperties.CurrencyIsoCode.NotSet;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_HousingAssignmentRate_RateValue_Null()
            {
                additionalCharges.FirstOrDefault().HousingAssignmentRate.RateValue = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_AdditionalCharges_AccoutingCode_NotFound()
            {
                additionalCharges.FirstOrDefault().AccountingCode.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ResidentTypeId_Null()
            {
                dtoHousingAssingment2.ResidentType.Id = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ResidentType_NotFound()
            {
                dtoHousingAssingment2.ResidentType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_DtoToEntity_IntegrationApiException_ContractNumber_Invalid_Length()
            {
                dtoHousingAssingment2.ContractNumber = "123-456-789-123-456";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Person_NotFound()
            {
                domainHousingAssignment = new Domain.Student.Entities.HousingAssignment("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "2", "1", DateTime.Today, DateTime.Today.AddDays(10));
                housingAssignmentRepositoryMock.Setup(r => r.UpdateHousingAssignmentAsync(It.IsAny<Domain.Student.Entities.HousingAssignment>())).ReturnsAsync(domainHousingAssignment);

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Room_NotFound()
            {
                domainHousingAssignment.Building = "2";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_Term_NotFound()
            {
                domainHousingAssignment.Term = "2";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_InvalidOperationException_Statuses_Empty()
            {
                domainHousingAssignment.Statuses = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_RoomRate_NotFound()
            {
                domainHousingAssignment.RoomRateTable = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "C";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_RateOverrideReason_NotFound()
            {
                domainHousingAssignment.RateOverrideReason = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "T";
                domainHousingAssignment.RatePeriod = null;

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_AdditionalAmounts_AraaArCode_NotFound()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaArCode = "2";
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "T";
                domainHousingAssignment.RatePeriod = "W";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateHousingAssignment2Async_EntityToDto_KeyNotFoundException_ResidentStaffIndicator_NotFound()
            {
                domainHousingAssignment.ResidentStaffIndicator = "2";
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaChargeAmt = null;
                domainHousingAssignment.Statuses.FirstOrDefault().Status = "R";
                domainHousingAssignment.RatePeriod = "M";

                await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);
            }

            [TestMethod]
            public async Task HousingAssignment2Service_UpdateHousingAssignmentAsync()
            {
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaChargeAmt = null;
                domainHousingAssignment.ArAdditionalAmounts.FirstOrDefault().AraaCrAmt = null;
                domainHousingAssignment.RatePeriod = "Y";

                var result = await housingAssignmentService.UpdateHousingAssignment2Async(guid, dtoHousingAssingment2);

                Assert.IsNotNull(result);
                Assert.AreEqual(dtoHousingAssingment.Id, result.Id);
            }
        }
    }
}