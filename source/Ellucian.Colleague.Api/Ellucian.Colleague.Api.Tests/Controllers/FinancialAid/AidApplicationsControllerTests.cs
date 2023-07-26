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
    public class AidApplicationsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IAidApplicationsService> _aidApplicationsServiceMock;
        private Mock<ILogger> _loggerMock;

        private AidApplicationsController _aidApplicationsController;

        private List<AidApplications> _aidApplicationsCollection;
        private AidApplications _aidApplicationsDto;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string AidApplicationsId = "1";
        private const string PersonId = "0001000";
        private const string AidYear = "2020";
        private const string AidApplicationType = "CALISIR";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _aidApplicationsServiceMock = new Mock<IAidApplicationsService>();
            _loggerMock = new Mock<ILogger>();

            _aidApplicationsCollection = new List<AidApplications>();

            var aidApplications1 = new AidApplications
            {
                Id = AidApplicationsId,
                AppDemoID = AidApplicationsId,
                PersonId = PersonId,
                AidYear = AidYear,
                ApplicationType = AidApplicationType,
                ApplicantAssignedId = "987654321",
                StudentMarital = new Dtos.FinancialAid.StudentMaritalInfo()
                {
                    Status = AidApplicationsStudentMarital.Single,
                    Date = "202102"
                },
                StudentLegalResidence = new Dtos.FinancialAid.LegalResidence()
                {
                    State = "TX",
                    ResidentBefore = true,
                    Date = "199002"
                },
                Parents = new Dtos.FinancialAid.ParentsInfo()
                {
                    FirstParent = new Dtos.FinancialAid.ParentDetails()
                    {
                        EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond,
                        SsnOrItin = 123456789,
                        LastName = "Jonas",
                        FirstInitial = "A",
                        BirthDate = new DateTime(1980, 02, 03)
                    },
                    SecondParent = new Dtos.FinancialAid.ParentDetails()
                    {
                        EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond,
                        SsnOrItin = 123456780,
                        LastName = "Smith",
                        FirstInitial = "P",
                        BirthDate = new DateTime(1982, 03, 02)
                    },
                    ParentMarital = new Dtos.FinancialAid.ParentMaritalInfo()
                    {
                        Status = AidApplicationsParentMarital.NeverMarried,
                        Date = "200203"
                    },
                    ParentLegalResidence = new Dtos.FinancialAid.LegalResidence()
                    {
                        State = "TX",
                        ResidentBefore = true,
                        Date = "199002"
                    },
                    NumberInCollege = 2,
                    NumberInFamily = 10,
                    EmailAddress = "jonas.smith@gmail.com",
                    Income = new Dtos.FinancialAid.ParentsIncome()
                    {
                        SsiBenefits = true,
                        FoodStamps = true,
                        LunchBenefits = true,
                        TanfBenefits = true,
                        WicBenefits = true,
                        TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                        TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                        TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledSeparateReturn,
                        Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                        DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                        AdjustedGrossIncome = -100,
                        UsTaxPaid = 500,
                        FirstParentWorkEarnings = 1000,
                        SecondParentworkEarnings = 2000,
                        CashSavingsChecking = 400,
                        InvestmentNetWorth = 300,
                        BusinessOrFarmNetWorth = 400,
                        EducationalCredits = 100,
                        ChildSupportPaid = 100,
                        NeedBasedEmployment = 100,
                        GrantOrScholarshipAid = 100,
                        CombatPay = 100,
                        CoopEarnings = 150,
                        PensionPayments = 200,
                        IraPayments = 300,
                        ChildSupportReceived = 100,
                        TaxExemptInterstIncome = 100,
                        UntaxedIraAndPensions = 300,
                        MilitaryOrClergyAllowances = 500,
                        VeteranNonEdBenefits = 100,
                        OtherUntaxedIncome = 50
                    }
                },
                HighSchool = new Dtos.FinancialAid.HighSchoolDetails()
                {
                    GradType = AidApplicationsHSGradtype.GEDOrStateEquivalentTest,
                    Name = "El Paso Elementary School",
                    City = "El Paso",
                    State = "TX",
                    Code = "123"
                },
                DegreeBy = true,
                GradeLevelInCollege = AidApplicationsGradLvlInCollege.FirstYearAttendedCollegeBefore,
                DegreeOrCertificate = AidApplicationsDegreeOrCert.SecondBachelorsDegree,
                BornBefore = true,
                Married = true,
                GradOrProfProgram = false,
                ActiveDuty = true,
                USVeteran = true,
                DependentChildren = false,
                OtherDependents = true,
                OrphanWardFoster = true,
                EmancipatedMinor = true,
                LegalGuardianship = true,
                HomelessAtRisk = true,
                HomelessByHud = true,
                HomelessBySchool = false,
                StudentNumberInCollege = 2,
                StudentNumberInFamily = 5,
                StudentsIncome = new Dtos.FinancialAid.StudentIncome()
                {
                    MedicaidOrSSIBenefits = true,
                    FoodStamps = true,
                    LunchBenefits = true,
                    TanfBenefits = true,
                    WicBenefits = true,
                    TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                    TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledJointReturn,
                    TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                    Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                    DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                    AdjustedGrossIncome = 6000,
                    UsTaxPaid = 500,
                    WorkEarnings = 7890,
                    SpouseWorkEarnings = 4000,
                    CashSavingsChecking = 400,
                    InvestmentNetWorth = 400,
                    BusinessNetWorth = 800,
                    EducationalCredit = 765,
                    ChildSupportPaid = 400,
                    NeedBasedEmployment = 650,
                    GrantAndScholarshipAid = 450,
                    CombatPay = 50,
                    CoopEarnings = 10,
                    PensionPayments = 500,
                    IraPayments = 600,
                    ChildSupportReceived = 700,
                    InterestIncome = 500,
                    UntaxedIraPension = 600,
                    MilitaryClergyAllowance = 500,
                    VeteranNonEdBenefits = 600,
                    OtherNonReportedMoney = 450,
                    OtherUntaxedIncome = 700
                },
                SchoolCode1 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00001",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode2 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00002",
                    HousingPlan = AidApplicationHousingPlanDto.OffCampus
                },
                SchoolCode3 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00003",
                    HousingPlan = AidApplicationHousingPlanDto.WithParent
                },
                SchoolCode4 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00004",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode5 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00005",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode6 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00006",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode7 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00007",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode8 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00008",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode9 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00009",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode10 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B000010",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                ApplicationCompleteDate = new DateTime(2023, 01, 01),
                SignedFlag = "P",
                PreparerEin = 987678912,
                PreparerSigned = "Yes",
                PreparerSsn = 456670987

            };

            _aidApplicationsCollection.Add(aidApplications1);

            BuildData();

            _aidApplicationsController = new AidApplicationsController(_aidApplicationsServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _aidApplicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _aidApplicationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_aidApplicationsDto));
        }

        private void BuildData()
        {
            _aidApplicationsDto = new AidApplications
            {
                Id = AidApplicationsId,
                AppDemoID = AidApplicationsId,
                PersonId = PersonId,
                AidYear = AidYear,
                ApplicationType = AidApplicationType,
                ApplicantAssignedId = "987654321",
                StudentMarital = new Dtos.FinancialAid.StudentMaritalInfo()
                {
                    Status = AidApplicationsStudentMarital.Single,
                    Date = "202102"
                },
                StudentLegalResidence = new Dtos.FinancialAid.LegalResidence()
                {
                    State = "TX",
                    ResidentBefore = true,
                    Date = "199002"
                },
                Parents = new Dtos.FinancialAid.ParentsInfo()
                {
                    FirstParent = new Dtos.FinancialAid.ParentDetails()
                    {
                        EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond,
                        SsnOrItin = 123456789,
                        LastName = "Jonas",
                        FirstInitial = "A",
                        BirthDate = new DateTime(1980, 02, 03)
                    },
                    SecondParent = new Dtos.FinancialAid.ParentDetails()
                    {
                        EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond,
                        SsnOrItin = 123456780,
                        LastName = "Smith",
                        FirstInitial = "P",
                        BirthDate = new DateTime(1982, 03, 02)
                    },
                    ParentMarital = new Dtos.FinancialAid.ParentMaritalInfo()
                    {
                        Status = AidApplicationsParentMarital.NeverMarried,
                        Date = "200203"
                    },
                    ParentLegalResidence = new Dtos.FinancialAid.LegalResidence()
                    {
                        State = "TX",
                        ResidentBefore = true,
                        Date = "199002"
                    },
                    NumberInCollege = 2,
                    NumberInFamily = 10,
                    EmailAddress = "jonas.smith@gmail.com",
                    Income = new Dtos.FinancialAid.ParentsIncome()
                    {
                        SsiBenefits = true,
                        FoodStamps = true,
                        LunchBenefits = true,
                        TanfBenefits = true,
                        WicBenefits = true,
                        TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                        TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                        TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledSeparateReturn,
                        Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                        DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                        AdjustedGrossIncome = -100,
                        UsTaxPaid = 500,
                        FirstParentWorkEarnings = 1000,
                        SecondParentworkEarnings = 2000,
                        CashSavingsChecking = 400,
                        InvestmentNetWorth = 300,
                        BusinessOrFarmNetWorth = 400,
                        EducationalCredits = 100,
                        ChildSupportPaid = 100,
                        NeedBasedEmployment = 100,
                        GrantOrScholarshipAid = 100,
                        CombatPay = 100,
                        CoopEarnings = 150,
                        PensionPayments = 200,
                        IraPayments = 300,
                        ChildSupportReceived = 100,
                        TaxExemptInterstIncome = 100,
                        UntaxedIraAndPensions = 300,
                        MilitaryOrClergyAllowances = 500,
                        VeteranNonEdBenefits = 100,
                        OtherUntaxedIncome = 50
                    }
                },
                HighSchool = new Dtos.FinancialAid.HighSchoolDetails()
                {
                    GradType = AidApplicationsHSGradtype.GEDOrStateEquivalentTest,
                    Name = "El Paso Elementary School",
                    City = "El Paso",
                    State = "TX",
                    Code = "123"
                },
                DegreeBy = true,
                GradeLevelInCollege = AidApplicationsGradLvlInCollege.FirstYearAttendedCollegeBefore,
                DegreeOrCertificate = AidApplicationsDegreeOrCert.SecondBachelorsDegree,
                BornBefore = true,
                Married = true,
                GradOrProfProgram = false,
                ActiveDuty = true,
                USVeteran = true,
                DependentChildren = false,
                OtherDependents = true,
                OrphanWardFoster = true,
                EmancipatedMinor = true,
                LegalGuardianship = true,
                HomelessAtRisk = true,
                HomelessByHud = true,
                HomelessBySchool = false,
                StudentNumberInCollege = 2,
                StudentNumberInFamily = 5,
                StudentsIncome = new Dtos.FinancialAid.StudentIncome()
                {
                    MedicaidOrSSIBenefits = true,
                    FoodStamps = true,
                    LunchBenefits = true,
                    TanfBenefits = true,
                    WicBenefits = true,
                    TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                    TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledJointReturn,
                    TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                    Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                    DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                    AdjustedGrossIncome = 6000,
                    UsTaxPaid = 500,
                    WorkEarnings = 7890,
                    SpouseWorkEarnings = 4000,
                    CashSavingsChecking = 400,
                    InvestmentNetWorth = 400,
                    BusinessNetWorth = 800,
                    EducationalCredit = 765,
                    ChildSupportPaid = 400,
                    NeedBasedEmployment = 650,
                    GrantAndScholarshipAid = 450,
                    CombatPay = 50,
                    CoopEarnings = 10,
                    PensionPayments = 500,
                    IraPayments = 600,
                    ChildSupportReceived = 700,
                    InterestIncome = 500,
                    UntaxedIraPension = 600,
                    MilitaryClergyAllowance = 500,
                    VeteranNonEdBenefits = 600,
                    OtherNonReportedMoney = 450,
                    OtherUntaxedIncome = 700
                },
                SchoolCode1 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00001",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode2 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00002",
                    HousingPlan = AidApplicationHousingPlanDto.OffCampus
                },
                SchoolCode3 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00003",
                    HousingPlan = AidApplicationHousingPlanDto.WithParent
                },
                SchoolCode4 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00004",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode5 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00005",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode6 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00006",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode7 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00007",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode8 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00008",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode9 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00009",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                SchoolCode10 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B000010",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                },
                ApplicationCompleteDate = new DateTime(2023, 01, 01),
                SignedFlag = "P",
                PreparerEin = 987678912,
                PreparerSigned = "Yes",
                PreparerSsn = 456670987

            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aidApplicationsController = null;
            _aidApplicationsCollection = null;
            _loggerMock = null;
            _aidApplicationsServiceMock = null;
        }

        #region AidApplications

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplications()
        {
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);

            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplications>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplications>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.PersonId, actual.PersonId);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
                Assert.AreEqual(expected.ApplicationType, actual.ApplicationType);

            }
        }

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplications_PersonId()
        {
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplications() { PersonId = PersonId });

            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);

            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplications>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplications>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplications_status()
        {
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplications() { AidYear = "2010" });

            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);

            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplications>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplications>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplications_classifications()
        {
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplications() { ApplicationType = "CALISIR" });

            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);

            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationsRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplications>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplications>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_KeyNotFoundException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<KeyNotFoundException>();
            await _aidApplicationsController.GetAidApplicationsAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
            await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<ArgumentException>();
            await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<RepositoryException>();
            await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<IntegrationApiException>();
            await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplications_Exception()
        {
            var paging = new Paging(100, 0);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<Exception>();
            await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
        }

        //Successful
        //GetAidApplicationsAsync
        [TestMethod]
        public async Task AidApplicationsController_GetAidApplicationsAsync_Permissions()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);
            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplications));


        }

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplicationsAsync_UpdatePermissions()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplications>, int>(_aidApplicationsCollection, 1);

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock
                .Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplications>()))
                .ReturnsAsync(tuple);
            var aidApplicationsRecord = await _aidApplicationsController.GetAidApplicationsAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplications));


        }

        //Exception
        //GetAidApplicationsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
                _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-applications."));
                await _aidApplicationsController.GetAidApplicationsAsync(paging, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplications

        #region GetAidApplicationsByGuid

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplicationsById()
        {
            _aidApplicationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));

            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_NullException()
        {
            await _aidApplicationsController.GetAidApplicationsByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_KeyNotFoundException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_PermissionsException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_ArgumentException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_RepositoryException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_IntegrationApiException()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsById_Exception()
        {
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
        }

        //Successful
        //GetAidApplicationsByIdAsync
        [TestMethod]
        public async Task AidApplicationsController_GetAidApplicationsByIdAsync_Permissions()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplications));


        }

        [TestMethod]
        public async Task AidApplicationsController_GetAidApplicationsByIdAsync_UpdatePermissions()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplications));


        }

        //Exception
        //GetAidApplicationsByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_GetAidApplicationsByIdAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "GetAidApplicationsByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-applications."));
                await _aidApplicationsController.GetAidApplicationsByIdAsync(AidApplicationsId);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationsById

        #region Put

        [TestMethod]
        public async Task AidApplicationsController_PUT()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, It.IsAny<AidApplications>())).ReturnsAsync(_aidApplicationsDto);
            _aidApplicationsServiceMock.Setup(i => i.GetAidApplicationsByIdAsync(AidApplicationsId)).ReturnsAsync(_aidApplicationsDto);
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_aidApplicationsDto.Id, result.Id);
        }

        [TestMethod]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_RequestId_Null()
        {
            _aidApplicationsServiceMock.Setup(i => i.GetAidApplicationsByIdAsync(AidApplicationsId)).ReturnsAsync(_aidApplicationsDto);
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, It.IsAny<AidApplications>())).ReturnsAsync(_aidApplicationsDto);
            _aidApplicationsDto.Id = string.Empty;
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AidApplicationsController_POST()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ReturnsAsync(_aidApplicationsDto);
            _aidApplicationsDto.Id = string.Empty;
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_Null_Dto()
        {
            var result = await _aidApplicationsController.PostAidApplicationsAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_KeyNotFoundException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new KeyNotFoundException());
            _aidApplicationsDto.Id = string.Empty;
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_PermissionsException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new PermissionsException());
            _aidApplicationsDto.Id = string.Empty;
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_ArgumentException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new ArgumentException());
            _aidApplicationsDto.Id = string.Empty;
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_RepositoryException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_IntegrationApiException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_POST_Exception()
        {
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_PermissionsException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new PermissionsException());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_RepositoryException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_IntegrationApiException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_Exception1()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationsController_PutAidApplications_KeyNotFoundException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationsController_PutAidApplications_ArgumentException()
        {
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new ArgumentException());
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_Id_Null_Exception()
        {
            await _aidApplicationsController.PutAidApplicationsAsync(string.Empty, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_Request_Null_Exception()
        {
            await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_EmptyGuid_Null_Exception()
        {
            await _aidApplicationsController.PutAidApplicationsAsync("1", _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_RequestIdIsEmptyGuid_Null_Exception()
        {
            _aidApplicationsDto.Id = "";
            await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_ValidateUpdateRequest_GuidsNotMatching_Exception()
        {
            _aidApplicationsDto.Id = "2";
            await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplications_Exception()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault();
            await _aidApplicationsController.PutAidApplicationsAsync(expected.Id, expected);
        }

        //Successful
        //PutAidApplicationsAsync
        [TestMethod]
        public async Task AidApplicationsController_PutAidApplicationsAsync_Permissions()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationsId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "PutAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock.Setup(x => x.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(_aidApplicationsDto);
            _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, It.IsAny<AidApplications>())).ReturnsAsync(_aidApplicationsDto);
            var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplications));


        }

        //Exception
        //PutAidApplicationsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PutAidApplicationsAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "PutAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationsServiceMock.Setup(i => i.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update aid-applications."));
                var result = await _aidApplicationsController.PutAidApplicationsAsync(AidApplicationsId, _aidApplicationsDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Successful
        //PostAidApplicationsAsync
        [TestMethod]
        public async Task AidApplicationsController_PostAidApplicationsAsync_Permissions()
        {
            _aidApplicationsDto.Id = string.Empty;
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "PostAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);
            _aidApplicationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplications);

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ReturnsAsync(_aidApplicationsDto);
            var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);

            Object filterObject;
            _aidApplicationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplications));


        }

        //Exception
        //PostAidApplicationsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_PostAidApplicationsAsync_Invalid_Permissions()
        {
            _aidApplicationsDto.Id = string.Empty;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplications" },
                    { "action", "PostAidApplicationsAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationsController.ControllerContext;
            var actionDescriptor = _aidApplicationsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationsServiceMock.Setup(i => i.PostAidApplicationsAsync(_aidApplicationsDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create aid-applications."));
                var result = await _aidApplicationsController.PostAidApplicationsAsync(_aidApplicationsDto);
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
        public async Task AidApplicationsController_PostAidApplications_Exception()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault();
            await _aidApplicationsController.PostAidApplicationsAsync(expected);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationsController_DeleteAidApplicationsAsync_Exception()
        {
            var expected = _aidApplicationsCollection.FirstOrDefault();
            await _aidApplicationsController.DeleteAidApplicationsAsync(expected.Id);
        }

        #endregion
    }

}
