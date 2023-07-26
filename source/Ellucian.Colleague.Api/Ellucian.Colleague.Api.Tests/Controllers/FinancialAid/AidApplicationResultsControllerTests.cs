// Copyright 2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{

    [TestClass]
    public class AidApplicationResultsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IAidApplicationResultsService> _aidApplicationResultsServiceMock;
        private Mock<ILogger> _loggerMock;

        private AidApplicationResultsController _aidApplicationResultsController;

        private List<AidApplicationResults> _aidApplicationResultsCollection;
        private AidApplicationResults _aidApplicationResultsDto;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string AidApplicationResultsId = "1";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _aidApplicationResultsServiceMock = new Mock<IAidApplicationResultsService>();
            _loggerMock = new Mock<ILogger>();

            _aidApplicationResultsCollection = new List<AidApplicationResults>();

            var results1 = new AidApplicationResults
            {
                Id = AidApplicationResultsId,
                AppDemoId = AidApplicationResultsId,
                PersonId = "0002990",
                AidYear = "2021",
                ApplicationType = "CALISIR",
                ApplicantAssignedId = "987654321",
                TransactionNumber = 1,
                DependencyOverride = "1",
                DependencyOverSchoolCode = "E20234",
                DependencyStatus = "I",
                TransactionSource = "4B",
                TransactionReceiptDate = new DateTime(2001, 1, 20),
                SpecialCircumstances = "2",
                ParentAssetExceeded = true,
                StudentAssetExceeded = true,
                DestinationNumber = "TG87902",
                StudentCurrentPseudoId = "675023526",
                CorrectionAppliedAgainst = "15",
                ProfJudgementIndicator = JudgementIndicator.AdjustmentFailed,
                ApplicationDataSource = "2B",
                ApplicationReceiptDate = new DateTime(2001, 1, 20),
                AddressOnlyChangeFlag = "4",
                PushedApplicationFlag = true,
                EfcChangeFlag = EfcChangeFlag.EfcDecrease,
                LastNameChange = LastNameChange.N,
                RejectStatusChange = false,
                SarcChange = false,
                ComputeNumber = "325",
                CorrectionSource = "S",
                DuplicateIdIndicator = true,
                GraduateFlag = false,
                TransactionProcessedDate = new DateTime(2003, 11, 22),
                ProcessedRecordType = "H",
                RejectReasonCodes = new List<string>() { "r1" },
                AutomaticZeroIndicator = false,
                SimplifiedNeedsTest = SimplifiedNeedsTest.N,
                ParentCalculatedTaxStatus = "3",
                StudentCalculatedTaxStatus = "3",
                StudentAddlFinCalcTotal = 20602400,
                studentOthUntaxIncomeCalcTotal = 20802300,
                ParentAddlFinCalcTotal = 13000208,
                ParentOtherUntaxIncomeCalcTotal = 40802400,
                InvalidHighSchool = false,
                Assumed = new Dtos.FinancialAid.AssumedStudentDetails()
                {
                    Citizenship = AssumedCitizenshipStatus.AssumedCitizen,
                    StudentMaritalStatus = AssumedStudentMaritalStatus.AssumedMarriedRemarried,
                    StudentAgi = 4182240,
                    StudentTaxPaid = 5130135,
                    StudentWorkIncome = 1291250,
                    SpouseWorkIncome = 7540278,
                    StudentAddlFinInfoTotal = 22703403,
                    BirthDatePrior = AssumedYesNo.AssumedNo,
                    StudentMarried = AssumedYesNo.AssumedNo,
                    DependentChildren = AssumedYesNo.AssumedNo,
                    OtherDependents = "2",
                    studentFamilySize = 99,
                    StudentNumberInCollege = 1,
                    StudentAssetThreshold = false,
                    ParentMaritalStatus = AssumedParentMaritalStatus.AssumedMarriedRemarried,
                    FirstParentSsn = true,
                    SecondParentSsn = false,
                    ParentFamilySize = 82,
                    ParentNumCollege = 7,
                    ParentAgi = 3322502,
                    ParentTaxPaid = 4530234,
                    FirstParentWorkIncome = 4260275,
                    SecondParentWorkIncome = 2350357,
                    ParentAddlFinancial = 15213506,
                    ParentAssetThreshold = true
                },
                PrimaryEfc = 351250,
                SecondaryEfc = 572026,
                SignatureRejectEfc = 662032,
                PrimaryEfcType = "6",
                AlternatePrimaryEfc = new Dtos.FinancialAid.AlternatePrimaryEfc()
                {
                    OneMonth = 345671,
                    TwoMonths = 345672,
                    ThreeMonths = 345673,
                    FourMonths = 345674,
                    FiveMonths = 345675,
                    SixMonths = 345676,
                    SevenMonths = 345677,
                    EightMonths = 345678,
                    TenMonths = 345680,
                    ElevenMonths = 345681,
                    TwelveMonths = 345682
                },
                TotalIncome = 72370346,
                AllowancesAgainstTotalIncome = 3234512,
                TaxAllowance = 8314355,
                EmploymentAllowance = 2685413,
                IncomeProtectionAllowance = 3459835,
                AvailableIncome = 83383552,
                AvailableIncomeContribution = 3349835,
                DiscretionaryNetWorth = 432498350,
                NetWorth = 598512160,
                AssetProtectionAllowance = 426286233,
                ParentContributionAssets = 962861,
                AdjustedAvailableIncome = 49628617,
                TotalPrimaryStudentContribution = 1985426,
                TotalPrimaryParentContribution = 5459833,
                ParentContribution = 3285134,
                StudentTotalIncome = 79628618,
                StudentAllowanceAgainstIncome = 5962823,
                DependentStudentIncContrib = 4328261,
                StudentDiscretionaryNetWorth = 373598352,
                StudentAssetContribution = 6459839,
                FisapTotalIncome = 49853222,
                CorrectionFlags = "Test10",
                HighlightFlags = "Test11",
                CommentCodes = new List<string>() { "c1" },
                ElectronicFedSchoolCodeInd = "1",
                ElectronicTransactionIndicator = "Y",
                VerificationSelected = "*"
            };

            _aidApplicationResultsCollection.Add(results1);

            BuildData();

            _aidApplicationResultsController = new AidApplicationResultsController(_aidApplicationResultsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _aidApplicationResultsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _aidApplicationResultsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_aidApplicationResultsDto));
        }

        private void BuildData()
        {
            _aidApplicationResultsDto = new AidApplicationResults
            {
                Id = AidApplicationResultsId,
                AppDemoId = AidApplicationResultsId,
                PersonId = "0002990",
                AidYear = "2021",
                ApplicationType = "CALISIR",
                ApplicantAssignedId = "987654321",
                TransactionNumber = 1,
                DependencyOverride = "1",
                DependencyOverSchoolCode = "E20234",
                DependencyStatus = "I",
                TransactionSource = "4B",
                TransactionReceiptDate = new DateTime(2001, 1, 20),
                SpecialCircumstances = "2",
                ParentAssetExceeded = true,
                StudentAssetExceeded = true,
                DestinationNumber = "TG87902",
                StudentCurrentPseudoId = "675023526",
                CorrectionAppliedAgainst = "15",
                ProfJudgementIndicator = JudgementIndicator.AdjustmentFailed,
                ApplicationDataSource = "2B",
                ApplicationReceiptDate = new DateTime(2001, 1, 20),
                AddressOnlyChangeFlag = "4",
                PushedApplicationFlag = true,
                EfcChangeFlag = EfcChangeFlag.EfcDecrease,
                LastNameChange = LastNameChange.N,
                RejectStatusChange = false,
                SarcChange = false,
                ComputeNumber = "325",
                CorrectionSource = "S",
                DuplicateIdIndicator = true,
                GraduateFlag = false,
                TransactionProcessedDate = new DateTime(2003, 11, 22),
                ProcessedRecordType = "H",
                RejectReasonCodes = new List<string>() { "r1" },
                AutomaticZeroIndicator = false,
                SimplifiedNeedsTest = SimplifiedNeedsTest.N,
                ParentCalculatedTaxStatus = "3",
                StudentCalculatedTaxStatus = "3",
                StudentAddlFinCalcTotal = 20602400,
                studentOthUntaxIncomeCalcTotal = 20802300,
                ParentAddlFinCalcTotal = 13000208,
                ParentOtherUntaxIncomeCalcTotal = 40802400,
                InvalidHighSchool = false,
                Assumed = new Dtos.FinancialAid.AssumedStudentDetails()
                {
                    Citizenship = AssumedCitizenshipStatus.AssumedCitizen,
                    StudentMaritalStatus = AssumedStudentMaritalStatus.AssumedMarriedRemarried,
                    StudentAgi = 4182240,
                    StudentTaxPaid = 5130135,
                    StudentWorkIncome = 1291250,
                    SpouseWorkIncome = 7540278,
                    StudentAddlFinInfoTotal = 22703403,
                    BirthDatePrior = AssumedYesNo.AssumedNo,
                    StudentMarried = AssumedYesNo.AssumedNo,
                    DependentChildren = AssumedYesNo.AssumedNo,
                    OtherDependents = "2",
                    studentFamilySize = 99,
                    StudentNumberInCollege = 1,
                    StudentAssetThreshold = false,
                    ParentMaritalStatus = AssumedParentMaritalStatus.AssumedMarriedRemarried,
                    FirstParentSsn = true,
                    SecondParentSsn = false,
                    ParentFamilySize = 82,
                    ParentNumCollege = 7,
                    ParentAgi = 3322502,
                    ParentTaxPaid = 4530234,
                    FirstParentWorkIncome = 4260275,
                    SecondParentWorkIncome = 2350357,
                    ParentAddlFinancial = 15213506,
                    ParentAssetThreshold = true
                },
                PrimaryEfc = 351250,
                SecondaryEfc = 572026,
                SignatureRejectEfc = 662032,
                PrimaryEfcType = "6",
                AlternatePrimaryEfc = new Dtos.FinancialAid.AlternatePrimaryEfc()
                {
                    OneMonth = 345671,
                    TwoMonths = 345672,
                    ThreeMonths = 345673,
                    FourMonths = 345674,
                    FiveMonths = 345675,
                    SixMonths = 345676,
                    SevenMonths = 345677,
                    EightMonths = 345678,
                    TenMonths = 345680,
                    ElevenMonths = 345681,
                    TwelveMonths = 345682
                },
                TotalIncome = 72370346,
                AllowancesAgainstTotalIncome = 3234512,
                TaxAllowance = 8314355,
                EmploymentAllowance = 2685413,
                IncomeProtectionAllowance = 3459835,
                AvailableIncome = 83383552,
                AvailableIncomeContribution = 3349835,
                DiscretionaryNetWorth = 432498350,
                NetWorth = 598512160,
                AssetProtectionAllowance = 426286233,
                ParentContributionAssets = 962861,
                AdjustedAvailableIncome = 49628617,
                TotalPrimaryStudentContribution = 1985426,
                TotalPrimaryParentContribution = 5459833,
                ParentContribution = 3285134,
                StudentTotalIncome = 79628618,
                StudentAllowanceAgainstIncome = 5962823,
                DependentStudentIncContrib = 4328261,
                StudentDiscretionaryNetWorth = 373598352,
                StudentAssetContribution = 6459839,
                FisapTotalIncome = 49853222,
                CorrectionFlags = "Test10",
                HighlightFlags = "Test11",
                CommentCodes = new List<string>() { "c1" },
                ElectronicFedSchoolCodeInd = "1",
                ElectronicTransactionIndicator = "Y",
                VerificationSelected = "*"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aidApplicationResultsController = null;
            _aidApplicationResultsCollection = null;
            _loggerMock = null;
            _aidApplicationResultsServiceMock = null;
        }

        #region GetAidApplicationResults

        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResults()
        {
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationResultsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationResults>, int>(_aidApplicationResultsCollection, 1);

            _aidApplicationResultsServiceMock
                .Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationResults>()))
                .ReturnsAsync(tuple);

            var aidApplicationResultsRecord = await _aidApplicationResultsController.GetAidApplicationResultsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationResultsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationResults>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationResults>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationResultsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResults_AppDemoId()
        {
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationResultsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationResults>, int>(_aidApplicationResultsCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationResultsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplicationResults() { AppDemoId = AidApplicationResultsId });

            _aidApplicationResultsServiceMock
                .Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationResults>()))
                .ReturnsAsync(tuple);

            var aidApplicationResultsRecord = await _aidApplicationResultsController.GetAidApplicationResultsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationResultsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationResults>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationResults>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationResultsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_KeyNotFoundException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<KeyNotFoundException>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<ArgumentException>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<RepositoryException>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<IntegrationApiException>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResults_Exception()
        {
            var paging = new Paging(100, 0);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<Exception>();
            await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
        }

        //Successful
        //GetAidApplicationResultsAsync
        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResultsAsync_Permissions()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationResults>, int>(_aidApplicationResultsCollection, 1);

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock
                .Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationResults>()))
                .ReturnsAsync(tuple);
            var aidApplicationResultsRecord = await _aidApplicationResultsController.GetAidApplicationResultsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationResults));


        }

        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResultsAsync_UpdatePermissions()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationResults>, int>(_aidApplicationResultsCollection, 1);

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock
                .Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationResults>()))
                .ReturnsAsync(tuple);
            var aidApplicationResultsRecord = await _aidApplicationResultsController.GetAidApplicationResultsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationResults));


        }

        //Exception
        //GetAidApplicationResultsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
                _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-results."));
                await _aidApplicationResultsController.GetAidApplicationResultsAsync(paging, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationResults

        #region GetAidApplicationResultsById

        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById()
        {
            _aidApplicationResultsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));

            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_NullException()
        {
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_KeyNotFoundException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_PermissionsException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_ArgumentException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_RepositoryException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_IntegrationApiException()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsById_Exception()
        {
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
        }

        //Successful
        //GetAidApplicationResultsByIdAsync
        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResultsByIdAsync_Permissions()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationResults));


        }

        [TestMethod]
        public async Task AidApplicationResultsController_GetAidApplicationResultsByIdAsync_UpdatePermissions()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationResults));


        }

        //Exception
        //GetAidApplicationResultsByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_GetAidApplicationResultsByIdAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "GetAidApplicationResultsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-results."));
                await _aidApplicationResultsController.GetAidApplicationResultsByIdAsync(AidApplicationResultsId);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationResultsById

        #region Put

        [TestMethod]
        public async Task AidApplicationResultsController_PUT()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, It.IsAny<AidApplicationResults>())).ReturnsAsync(_aidApplicationResultsDto);
            _aidApplicationResultsServiceMock.Setup(i => i.GetAidApplicationResultsByIdAsync(AidApplicationResultsId)).ReturnsAsync(_aidApplicationResultsDto);
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_aidApplicationResultsDto.Id, result.Id);
        }

        [TestMethod]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_RequestId_Null()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.GetAidApplicationResultsByIdAsync(AidApplicationResultsId)).ReturnsAsync(_aidApplicationResultsDto);
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, It.IsAny<AidApplicationResults>())).ReturnsAsync(_aidApplicationResultsDto);
            _aidApplicationResultsDto.Id = string.Empty;
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AidApplicationResultsController_POST()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ReturnsAsync(_aidApplicationResultsDto);
            _aidApplicationResultsDto.Id = string.Empty;
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AidApplicationResultsController_POST_Null_Dto()
        {
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_KeyNotFoundException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new KeyNotFoundException());
            _aidApplicationResultsDto.Id = string.Empty;
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_PermissionsException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new PermissionsException());
            _aidApplicationResultsDto.Id = string.Empty;
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_ArgumentException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new ArgumentException());
            _aidApplicationResultsDto.Id = string.Empty;
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_RepositoryException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_IntegrationApiException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_POST_Exception()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_PermissionsException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new PermissionsException());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_RepositoryException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_IntegrationApiException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_Exception1()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationResultsController_PutAidApplicationResults_KeyNotFoundException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationResultsController_PutAidApplicationResults_ArgumentException()
        {
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new ArgumentException());
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_Id_Null_Exception()
        {
            await _aidApplicationResultsController.PutAidApplicationResultsAsync(string.Empty, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_Request_Null_Exception()
        {
            await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_EmptyGuid_Null_Exception()
        {
            await _aidApplicationResultsController.PutAidApplicationResultsAsync("1", _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_RequestIdIsEmptyGuid_Null_Exception()
        {
            _aidApplicationResultsDto.Id = "";
            await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_ValidateUpdateRequest_GuidsNotMatching_Exception()
        {
            _aidApplicationResultsDto.Id = "2";
            await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResults_Exception()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault();
            await _aidApplicationResultsController.PutAidApplicationResultsAsync(expected.Id, expected);
        }

        //Successful
        //PutAidApplicationResultsAsync
        [TestMethod]
        public async Task AidApplicationResultsController_PutAidApplicationResultsAsync_Permissions()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationResultsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "PutAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock.Setup(x => x.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(_aidApplicationResultsDto);
            _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, It.IsAny<AidApplicationResults>())).ReturnsAsync(_aidApplicationResultsDto);
            var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationResults));


        }

        //Exception
        //PutAidApplicationResultsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PutAidApplicationResultsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "PutAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationResultsServiceMock.Setup(i => i.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update aid-application-results."));
                var result = await _aidApplicationResultsController.PutAidApplicationResultsAsync(AidApplicationResultsId, _aidApplicationResultsDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PostAidApplicationResults_Exception()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault();
            await _aidApplicationResultsController.PostAidApplicationResultsAsync(expected);
        }

        //Successful
        //PostAidApplicationResultsAsync
        [TestMethod]
        public async Task AidApplicationResultsController_PostAidApplicationResultsAsync_Permissions()
        {
            _aidApplicationResultsDto.Id = string.Empty;
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "PostAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);
            _aidApplicationResultsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationResults);

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ReturnsAsync(_aidApplicationResultsDto);
            var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);

            Object filterObject;
            _aidApplicationResultsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationResults));


        }

        //Exception
        //PostAidApplicationResultsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_PostAidApplicationResultsAsync_Invalid_Permissions()
        {
            _aidApplicationResultsDto.Id = string.Empty;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationResults" },
                    { "action", "PostAidApplicationResultsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationResults", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationResultsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationResultsController.ControllerContext;
            var actionDescriptor = _aidApplicationResultsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationResultsServiceMock.Setup(i => i.PostAidApplicationResultsAsync(_aidApplicationResultsDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationResultsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create aid-application-results."));
                var result = await _aidApplicationResultsController.PostAidApplicationResultsAsync(_aidApplicationResultsDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationResultsController_DeleteAidApplicationResultsAsync_Exception()
        {
            var expected = _aidApplicationResultsCollection.FirstOrDefault();
            await _aidApplicationResultsController.DeleteAidApplicationResultsAsync(expected.Id);
        }

        #endregion
    }
}
