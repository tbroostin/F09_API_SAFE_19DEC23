/* Copyright 2017-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Reports;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayStatementServiceTests : HumanResourcesServiceTestsSetup
    {

        public Mock<ILocalReportService> reportRenderServiceMock;
        public Mock<IPayStatementRepository> payStatementRepositoryMock;
        public Mock<IPayStatementDomainService> payStatementDomanServiceMock;
        public Mock<IEmployeeRepository> employeeRepositoryMock;
        public Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        public Mock<IPayrollRegisterRepository> payrollRegisterRepositoryMock;
        public Mock<IEarningsTypeRepository> earningsTypeRepositoryMock;
        public Mock<IPersonBenefitDeductionRepository> personBenefitDeductionRepositoryMock;
        public Mock<IPersonEmploymentStatusRepository> personEmploymentStatusRepositoryMock;
        public Mock<IPositionRepository> positionRepositoryMock;
        public ITypeAdapter<PayStatementSourceData, Dtos.HumanResources.PayStatementSummary> summaryEntityToDtoAdapter;
        public ITypeAdapter<PayStatementReport, Dtos.HumanResources.PayStatementReport> reportEntityToDtoAdapter;

        public PayStatementService serviceUnderTest;

        public TestPayStatementRepository testData;


        public TestBenefitDeductionTypeRepository testBenDedData;
        public TestEarningsTypeRepository testEarningsTypeData;
        public TestEarningsDifferentialRepository testEarningsDifferentialData;
        public TestTaxCodeRepository testTaxCodeData;
        public TestPersonBenefitDeductionRepository testPersonBenefitDeductionData;
        public TestPayrollRegisterRepository testPayrollRegisterData;
        public TestPersonEmploymentStatusRepository testPersonEmploymentStatusData;
        public TestPositionRepository testPositionData;

        public PayStatementConfiguration testPayStatementConfigurationData;
        public FunctionEqualityComparer<Dtos.HumanResources.PayStatementSummary> summaryEqualityComparer;

        public IEnumerable<ReportParameter> actualReportParameters;

        public void PayStatementServiceTestsInitialize()
        {
            MockInitialize();

            reportRenderServiceMock = new Mock<ILocalReportService>();
            payStatementRepositoryMock = new Mock<IPayStatementRepository>();
            payStatementDomanServiceMock = new Mock<IPayStatementDomainService>();
            hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            payrollRegisterRepositoryMock = new Mock<IPayrollRegisterRepository>();
            earningsTypeRepositoryMock = new Mock<IEarningsTypeRepository>();
            personBenefitDeductionRepositoryMock = new Mock<IPersonBenefitDeductionRepository>();
            personEmploymentStatusRepositoryMock = new Mock<IPersonEmploymentStatusRepository>();
            employeeRepositoryMock = new Mock<IEmployeeRepository>();
            positionRepositoryMock = new Mock<IPositionRepository>();
            testData = new TestPayStatementRepository();
            //testData.CreatePayStatementRecords(employeeCurrentUserFactory.CurrentUser.PersonId);
            testBenDedData = new TestBenefitDeductionTypeRepository();
            testEarningsTypeData = new TestEarningsTypeRepository();
            testEarningsDifferentialData = new TestEarningsDifferentialRepository();
            testTaxCodeData = new TestTaxCodeRepository();
            testPersonBenefitDeductionData = new TestPersonBenefitDeductionRepository();
            testPayrollRegisterData = new TestPayrollRegisterRepository();
            testPersonEmploymentStatusData = new TestPersonEmploymentStatusRepository();
            testPositionData = new TestPositionRepository();

            testPayStatementConfigurationData = new PayStatementConfiguration()
            {
                PreviousYearsCount = 10,
                OffsetDaysCount = 0,
                DisplayZeroAmountBenefitDeductions = true,
                DisplayWithholdingStatusFlag = true,
                SocialSecurityNumberDisplay = SSNDisplay.LastFour,
            };

            summaryEntityToDtoAdapter = new AutoMapperAdapter<PayStatementSourceData, Dtos.HumanResources.PayStatementSummary>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(r => r.GetAdapter<PayStatementSourceData, Dtos.HumanResources.PayStatementSummary>())
                .Returns(() => summaryEntityToDtoAdapter);

            reportEntityToDtoAdapter = new PayStatementReportEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(r => r.GetAdapter<PayStatementReport, Dtos.HumanResources.PayStatementReport>())
                .Returns(() => reportEntityToDtoAdapter);

            reportRenderServiceMock.Setup(r => r.SetParameters(It.IsAny<IEnumerable<ReportParameter>>()))
                .Callback<IEnumerable<ReportParameter>>(parameters => actualReportParameters = parameters);
            reportRenderServiceMock.Setup(r => r.RenderReport())
                .Returns(() =>
                {
                    var document = new PdfDocument();
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var font = new XFont("Verdana", 20, XFontStyle.Bold);
                    gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
                    document.AddPage(new PdfPage());
                    var outputStream = new MemoryStream();
                    document.Save(outputStream);
                    return outputStream.ToArray();
                });

            payStatementDomanServiceMock.Setup(p => p.SetContext(It.IsAny<IEnumerable<PayStatementSourceData>>(), It.IsAny<IEnumerable<PayrollRegisterEntry>>(), It.IsAny<IEnumerable<PersonBenefitDeduction>>(), It.IsAny<IEnumerable<PersonEmploymentStatus>>(), It.IsAny<PayStatementReferenceDataUtility>()));
            payStatementDomanServiceMock.Setup(p => p.BuildPayStatementReport(It.IsAny<PayStatementSourceData>()))
                .Returns<PayStatementSourceData>(src => new PayStatementReport(createReportDataContext(src), new List<PayStatementReportDataContext>() { createReportDataContext(src) }, createDataUtility()));

            employeeRepositoryMock.Setup(p => p.GetEmployeeKeysAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<IEnumerable<string>, bool?, bool, bool>((ids, hasConsent, includeNonEmployees, isActive) =>
                {
                    if (ids == null || !ids.Any())
                    {
                        return Task.FromResult(testData.payStatementRecords == null ? null : testData.payStatementRecords.Select(ps => ps.employeeId));

                    }
                    else
                    {
                        return Task.FromResult(ids);
                    }
                });
            employeeRepositoryMock.Setup(p => p.GetEmployeeKeysAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<IEnumerable<string>, bool?, bool, bool>((ids, hasConsent, includeNonEmployees, isActive) =>
                {
                    if (ids == null || !ids.Any())
                    {
                        return Task.FromResult(testData.payStatementRecords == null ? null : testData.payStatementRecords.Select(ps => ps.employeeId));

                    }
                    else
                    {
                        return Task.FromResult(ids);
                    }
                });
            employeeRepositoryMock.Setup(p => p.GetEmployeeKeysAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<IEnumerable<string>, bool?, bool, bool>((ids, hasConsent, includeNonEmployees, isActive) =>
                {
                    if (ids == null || !ids.Any())
                    {
                        return Task.FromResult(testData.payStatementRecords == null ? null : testData.payStatementRecords.Select(ps => ps.employeeId));

                    }
                    else
                    {
                        return Task.FromResult(ids);
                    }
                });
            hrReferenceDataRepositoryMock.Setup(r => r.GetBenefitDeductionTypesAsync())
                .Returns(() => testBenDedData.GetBenefitDeductionTypesAsync());
            hrReferenceDataRepositoryMock.Setup(r => r.GetEarningsDifferentialsAsync())
                .Returns(() => testEarningsDifferentialData.GetEarningsDifferentialsAsync());
            hrReferenceDataRepositoryMock.Setup(r => r.GetTaxCodesAsync())
                .Returns(() => testTaxCodeData.GetTaxCodesAsync());
            hrReferenceDataRepositoryMock.Setup(r => r.GetPayStatementConfigurationAsync())
                .Returns(() => Task.FromResult(testPayStatementConfigurationData));
            earningsTypeRepositoryMock.Setup(r => r.GetEarningsTypesAsync())
                .Returns(() => testEarningsTypeData.GetEarningsTypesAsync());
            positionRepositoryMock.Setup(r => r.GetPositionsAsync())
                .Returns(() => testPositionData.GetPositionsAsync());

            payStatementRepositoryMock.Setup(r => r.GetPayStatementSourceDataAsync(It.IsAny<string>()))
                .Returns<string>((id) =>
                    testData.GetPayStatementSourceDataAsync(id));


            payStatementRepositoryMock.Setup(r => r.GetPayStatementSourceDataByPersonIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, startDate, endDate) =>
                    testData.GetPayStatementSourceDataByPersonIdAsync(personIds, startDate, endDate));
            payStatementRepositoryMock.Setup(r => r.GetPayStatementSourceDataByPersonIdAsync(It.IsAny<IEnumerable<string>>(), null, null))
               .Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, start, end) =>
                   testData.GetPayStatementSourceDataByPersonIdAsync(personIds, start, end));


            payStatementRepositoryMock.Setup(r => r.GetPayStatementSourceDataAsync(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids =>
                    testData.GetPayStatementSourceDataAsync(ids));



            payrollRegisterRepositoryMock.Setup(r => r.GetPayrollRegisterByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, startDate, endDate) =>
                    testPayrollRegisterData.GetPayrollRegisterByEmployeeIdsAsync(personIds, startDate, endDate));
            payrollRegisterRepositoryMock.Setup(r => r.GetPayrollRegisterByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(), null, null))
                .Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, startDate, endDate) =>
                    testPayrollRegisterData.GetPayrollRegisterByEmployeeIdsAsync(personIds, startDate, endDate));

            personBenefitDeductionRepositoryMock.Setup(r => r.GetPersonBenefitDeductionsAsync(It.IsAny<string>()))
                .Returns<string>(personId => testPersonBenefitDeductionData.GetPersonBenefitDeductionsAsync(personId));
            personBenefitDeductionRepositoryMock.Setup(r => r.GetPersonBenefitDeductionsAsync(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(personIds => testPersonBenefitDeductionData.GetPersonBenefitDeductionsAsync(personIds));

            personEmploymentStatusRepositoryMock.Setup(r => r.GetPersonEmploymentStatusesAsync(It.IsAny<IEnumerable<string>>()))
                .Callback<IEnumerable<string>>(ids => testPersonEmploymentStatusData.personEmploymentStatusRecords.ForEach(r => r.personId = ids.First()))
                .Returns<IEnumerable<string>>(ids => testPersonEmploymentStatusData.GetPersonEmploymentStatusesAsync(ids));

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            serviceUnderTest = new PayStatementService(
                reportRenderServiceMock.Object,
                payStatementRepositoryMock.Object,
                payStatementDomanServiceMock.Object,
                employeeRepositoryMock.Object,
                hrReferenceDataRepositoryMock.Object,
                payrollRegisterRepositoryMock.Object,
                earningsTypeRepositoryMock.Object,
                personBenefitDeductionRepositoryMock.Object,
                personEmploymentStatusRepositoryMock.Object,
                positionRepositoryMock.Object,
                adapterRegistryMock.Object,
                employeeCurrentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);

            summaryEqualityComparer = new FunctionEqualityComparer<Dtos.HumanResources.PayStatementSummary>(
                (a, b) =>
                    a.Id == b.Id &&
                    a.PayDate == b.PayDate,
                (s) => s.Id.GetHashCode()
            );

        }

        private PayStatementReportDataContext createReportDataContext(PayStatementSourceData source)
        {
            return new PayStatementReportDataContext(source,
                new PayrollRegisterEntry(source.Id, source.EmployeeId, source.PeriodEndDate.AddMonths(-1).AddDays(1), source.PeriodEndDate, "BW", 1, source.PaycheckReferenceId, source.StatementReferenceId),
                testPersonBenefitDeductionData.GetPersonBenefitDeductions(source.EmployeeId),
                testPersonEmploymentStatusData.GetPersonEmploymentStatuses(new string[1] { source.EmployeeId }));
        }
        private PayStatementReferenceDataUtility createDataUtility()
        {
            return new PayStatementReferenceDataUtility(testEarningsTypeData.GetEarningsTypes(),
                testEarningsDifferentialData.GetEarningsDifferentials(),
                testTaxCodeData.GetTaxCodes(),
                testBenDedData.GetBenefitDeductionTypes(),
                new List<LeaveType>(),
                testPositionData.GetPositions(),
                testPayStatementConfigurationData);
        }

        [TestClass]
        public class GetPayStatementPdfTests : PayStatementServiceTests
        {
            public string inputId;
            public string inputPathToReport;
            public string inputPathToLogo;


            public async Task<Tuple<string, byte[]>> getActual()
            {
                return await serviceUnderTest.GetPayStatementPdf(inputId, inputPathToReport, inputPathToLogo);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayStatementServiceTestsInitialize();

                testData.CreatePayStatementRecords("0003914");
                inputId = testData.payStatementRecords[0].recordKey;
                inputPathToReport = "../../../Ellucian.Colleague.Coordination.HumanResources.Tests/Reports/foo.rdlc";
                inputPathToLogo = "";
                actualReportParameters = null;
            }

            [TestMethod]
            public async Task ExecuteNoErrorsTest()
            {
                await getActual();
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayStatementIdRequiredTest()
            {
                inputId = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PathToReportRequiredTest()
            {
                inputPathToReport = null;
                await getActual();
            }

            [TestMethod]
            public async Task ReportPropertiesSetTest()
            {
                await getActual();

                reportRenderServiceMock.Verify(r =>
                    r.SetPath(inputPathToReport));
                reportRenderServiceMock.Verify(r =>
                    r.EnableExternalImages(true));



            }


            [TestMethod]
            public async Task NoLogoPath_ParameterTest()
            {
                inputPathToLogo = null;
                await getActual();
                var logoPath = actualReportParameters.FirstOrDefault(p => p.Name == "LogoPath");
                Assert.IsTrue(logoPath.Values[0].Equals(string.Empty));
            }

            [TestMethod]
            public async Task LogoPath_ParameterTest()
            {
                inputPathToLogo = "foobar";
                await getActual();
                var logoPath = actualReportParameters.FirstOrDefault(p => p.Name == "LogoPath");
                Assert.IsTrue(logoPath.Values[0].Equals(inputPathToLogo));

            }

            [TestMethod]
            public async Task ShouldDisplayWithholding_ParameterTest()
            {
                testPayStatementConfigurationData.DisplayWithholdingStatusFlag = true;
                await getActual();
                var shouldDisplayWithholding = actualReportParameters.FirstOrDefault(p => p.Name == "ShouldDisplayWithholdingStatus");
                Assert.IsTrue(shouldDisplayWithholding.Values[0].Equals("true", StringComparison.InvariantCultureIgnoreCase));
            }

            [TestMethod]
            public async Task ShouldNotDisplayWithholding_ParameterTest()
            {
                testPayStatementConfigurationData.DisplayWithholdingStatusFlag = false;
                await getActual();
                var shouldDisplayWithholding = actualReportParameters.FirstOrDefault(p => p.Name == "ShouldDisplayWithholdingStatus");
                Assert.IsTrue(shouldDisplayWithholding.Values[0].Equals("false", StringComparison.InvariantCultureIgnoreCase));
            }

            [TestMethod]
            public async Task ShouldDisplayFullSSN_ParameterTest()
            {
                testPayStatementConfigurationData.SocialSecurityNumberDisplay = SSNDisplay.Full;
                await getActual();

                var shouldDisplaySSN = actualReportParameters.FirstOrDefault(p => p.Name == "ShouldDisplaySocialSecurityNumber");
                Assert.IsTrue(shouldDisplaySSN.Values[0].Equals("true", StringComparison.InvariantCultureIgnoreCase));

            }

            [TestMethod]
            public async Task ShouldDisplayLast4SSN_ParameterTest()
            {
                testPayStatementConfigurationData.SocialSecurityNumberDisplay = SSNDisplay.LastFour;
                await getActual();

                var shouldDisplaySSN = actualReportParameters.FirstOrDefault(p => p.Name == "ShouldDisplaySocialSecurityNumber");
                Assert.IsTrue(shouldDisplaySSN.Values[0].Equals("true", StringComparison.InvariantCultureIgnoreCase));
            }

            [TestMethod]
            public async Task ShouldNotDisplaySSN_ParameterTest()
            {
                testPayStatementConfigurationData.SocialSecurityNumberDisplay = SSNDisplay.Hidden;
                await getActual();

                var shouldDisplaySSN = actualReportParameters.FirstOrDefault(p => p.Name == "ShouldDisplaySocialSecurityNumber");
                Assert.IsTrue(shouldDisplaySSN.Values[0].Equals("false", StringComparison.InvariantCultureIgnoreCase));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorBuildingReportTest()
            {
                payStatementDomanServiceMock.Setup(d => d.BuildPayStatementReport(It.IsAny<PayStatementSourceData>()))
                    .Throws(new ArgumentNullException("ex"));

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullReportReturnedTest()
            {
                payStatementDomanServiceMock.Setup(d => d.BuildPayStatementReport(It.IsAny<PayStatementSourceData>()))
                    .Returns<PayStatementSourceData>(src => null);

                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InputIdNotFoundTest()
            {
                inputId = "foobar";
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NoPermissionForOtherEmployeeAccessTest()
            {
                testData.payStatementRecords.ForEach(p => p.employeeId = "foobar");
                await getActual();
            }

            [TestMethod]
            public async Task PermissionNeededForOtherEmployeeAccessTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
                    .Returns(() => employeeCurrentUserFactory.CurrentUser.Roles
                        .Select(r =>
                        {
                            var role = new Role(r.GetHashCode(), r);
                            role.AddPermission(new Permission(HumanResourcesPermissionCodes.ViewAllEarningsStatements));
                            return role;
                        }));

                testData.payStatementRecords.ForEach(p => p.employeeId = "foobar");
                var actual = await getActual();
            }
        }

        [TestClass]
        public class GetMultiplePayStatementPdfTests : PayStatementServiceTests
        {
            public List<string> inputIds;
            public string inputPathToReport;
            public string inputPathToLogo;

            public async Task<byte[]> getActual()
            {

                return await serviceUnderTest.GetPayStatementPdf(inputIds, inputPathToReport, inputPathToLogo);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayStatementServiceTestsInitialize();
                testData.CreatePayStatementRecords("0003914");
                inputIds = testData.payStatementRecords.Select(r => r.recordKey).ToList();
                inputPathToReport = "../../../Ellucian.Colleague.Coordination.HumanResources.Tests/Reports/TestReport.rdlc";
                inputPathToLogo = "";
            }

            [TestMethod]
            public async Task NoErrorsTest()
            {
                var actual = await getActual();
                Assert.IsInstanceOfType(actual, typeof(byte[]));
            }
        }

        [TestClass]
        public class GetPayStatementSummariesTests : PayStatementServiceTests
        {
            //public string inputPersonId;
            public List<string> inputPersonIds;
            public bool? inputHasOnlineConsent;
            public DateTime? inputPayDate;
            public string inputPayCycleId;
            public DateTime? inputStartDate;
            public DateTime? inputEndDate;
            public async Task<IEnumerable<Dtos.HumanResources.PayStatementSummary>> getActual()
            {
                return await serviceUnderTest.GetPayStatementSummariesAsync(inputPersonIds, inputHasOnlineConsent, inputPayDate, inputPayCycleId, inputStartDate, inputEndDate);
            }


            [TestInitialize]
            public void Initialize()
            {
                PayStatementServiceTestsInitialize();

                //inputPersonId = employeeCurrentUserFactory.CurrentUser.PersonId;
                inputPersonIds = new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId };
                inputHasOnlineConsent = null;
                inputPayDate = null;
                inputPayCycleId = null;
                inputStartDate = DateTime.Now.AddYears(-5);
                inputEndDate = DateTime.Now;
            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotAccessOtherUsersDataTest()
            {
                testData.CreatePayStatementRecords("foobar");
                testPayrollRegisterData.createPayToDateRecord("foobar", "BW");
                await serviceUnderTest.GetPayStatementSummariesAsync(new string[1] { "foobar" });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotAccessAllUserData_NullEmployeeIdsTest()
            {
                testData.CreatePayStatementRecords("foobar");
                testPayrollRegisterData.createPayToDateRecord("foobar", "BW");
                await serviceUnderTest.GetPayStatementSummariesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CannotAccessAllUserData_EmptyEmployeeIdsTest()
            {
                testData.CreatePayStatementRecords("foobar");
                testPayrollRegisterData.createPayToDateRecord("foobar", "BW");
                await serviceUnderTest.GetPayStatementSummariesAsync(new List<string>());
            }

            [TestMethod]
            public async Task PermissionRequiredToAccessOtherUsersDataTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
                    .Returns(() => employeeCurrentUserFactory.CurrentUser.Roles.Select(r =>
                        {
                            var role = new Role(r.GetHashCode(), r);
                            role.AddPermission(new Permission(HumanResourcesPermissionCodes.ViewAllEarningsStatements));
                            return role;
                        }));

                testData.CreatePayStatementRecords("foobar");

                inputPersonIds = new List<string>() { "foobar" };

                var actual = await getActual();

                Assert.IsTrue(actual.Any());

            }


            [TestMethod]
            public async Task EmptyListOfDtosTest()
            {
                testData.payStatementRecords = new List<TestPayStatementRepository.PayStatementRecord>();
                inputPersonIds = new List<string>();
                var actual = await getActual();
                Assert.AreEqual(0, actual.Count());
            }

            [TestMethod]
            public async Task StatementMustHaveAssociatedPayrollRegisterTest()
            {
                //payrollRegisterRepositoryMock.Setup(r => r.GetPayrollRegisterByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                ////.Callback<IEnumerable<string>>((personIds) =>
                ////    {
                ////        personIds.ToList().ForEach(p => testPayrollRegisterData.createPayToDateRecord(p, "BW"));
                ////        testPayrollRegisterData.payToDateRecords.ForEach(pt => pt.adviceNumber = "foo");
                ////    })
                //.Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, start, end) => testPayrollRegisterData.GetPayrollRegisterByEmployeeIdsAsync(personIds, start, end));

                testPayrollRegisterData.payToDateRecords.ForEach(ptd => ptd.adviceNumber = "foo");

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task PayDateFilterTest()
            {
                inputPayDate = new DateTime(2017, 8, 22);
                testData.payStatementRecords.ForEach(ps => ps.wpaDate = inputPayDate.Value.AddDays(-1));

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NullHasOnlineConsentTest()
            {
                inputHasOnlineConsent = null;
                var actual = await getActual();

                employeeRepositoryMock.Verify(r => r.GetEmployeeKeysAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            }

            [TestMethod]
            public async Task TrueHasOnlineConsentTest()
            {
                inputHasOnlineConsent = true;
                var actual = await getActual();
                employeeRepositoryMock.Verify(r => r.GetEmployeeKeysAsync(It.IsAny<IEnumerable<string>>(), true, It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            }

            [TestMethod]
            public async Task FalseHasOnlineConsentTest()
            {
                inputHasOnlineConsent = false;
                var actual = await getActual();
                employeeRepositoryMock.Verify(r => r.GetEmployeeKeysAsync(It.Is<IEnumerable<string>>(list => list.SequenceEqual(inputPersonIds)), false, It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            }

            [TestMethod]
            public async Task GetSummariesForMultiplePeopleTest()
            {
                roleRepositoryMock.Setup(r => r.Roles)
                    .Returns(() => employeeCurrentUserFactory.CurrentUser.Roles.Select(r =>
                    {
                        var role = new Role(r.GetHashCode(), r);
                        role.AddPermission(new Permission(HumanResourcesPermissionCodes.ViewAllEarningsStatements));
                        return role;
                    }));

                for (int i = 0; i < testData.payStatementRecords.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        testData.payStatementRecords[i].employeeId = "foo";
                    }
                    else
                    {
                        testData.payStatementRecords[i].employeeId = "bar";
                    }
                }

                inputPersonIds = new List<string>() { "foo", "bar" };


                var actual = await getActual();
            }

            [TestMethod]
            public async Task NoPayrollRegisterTest()
            {
                testPayrollRegisterData.payToDateRecords = new List<TestPayrollRegisterRepository.PayToDateRecord>();

                payrollRegisterRepositoryMock.Setup(r => r.GetPayrollRegisterByEmployeeIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                    .Returns<IEnumerable<string>, DateTime?, DateTime?>((personIds, start, end) => Task.FromResult<IEnumerable<PayrollRegisterEntry>>(new List<PayrollRegisterEntry>()));


                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task PayrollRegisterFilteredByPayCycleIdTest()
            {
                inputPayCycleId = "foo"; //no such pay cycle as foo. testing that all register entries are filtered out

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }
        }
    }
}
