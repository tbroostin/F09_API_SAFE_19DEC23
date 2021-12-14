/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementReportTests
    {
        PayStatementReport report;
        PayStatementReportDataContext payStatementReportDatacontext;
        PayStatementReportDataContext payStatementReportDatacontext_0;
        List<PayStatementReportDataContext> yearToDateDataContext;
        PayrollRegisterEntry payrollRegisterEntry;
        PayrollRegisterEntry payrollRegisterEntry_0;
        PayStatementConfiguration payStatementConfiguration;
        PayStatementReferenceDataUtility referenceUtility;

        string id;
        string employeeId;
        string employeeName;
        string employeeSSN;
        List<PayStatementAddress> mailingLabel;
        string checkReferenceId;
        string statementReferenceId;
        DateTime payDate;
        DateTime periodEndDate;
        decimal periodGrossPay;
        decimal periodNetPay;
        decimal ytdGrossPay;
        decimal ytdNetPay;
        string comments;
        PayStatementSourceData sourceData;
        PayStatementSourceData sourceData_0;
        IEnumerable<PayStatementSourceData> yearToDateSourceEntities;

        string federalWithholdingSingleTaxCodeId;
        string stateWithholdingSingleTaxCodeId;

        string benefitDeductionId;
        DateTime enrollmentDate;
        DateTime? cancelDate;
        DateTime? lastPayDate;
        List<PersonBenefitDeduction> benefitDeductions;

        string personEmploymentStatusId;
        string primaryPositionId;
        string personPositionId;
        DateTime? startDate;
        DateTime? endDate;
        List<PersonEmploymentStatus> personEmploymentStatuses;

        List<BenefitDeductionType> benefitDeductionTypes;
        List<TaxCode> taxCodes;
        IEnumerable<TaxCode> federalWithholdingTaxCodes;
        IEnumerable<TaxCode> stateWithholdingTaxCodes;
        List<EarningsType> earningsTypes;
        List<EarningsDifferential> earningsDifferentials;
        List<LeaveType> leaveTypes;
        public List<Position> positions;

        public bool isAdj;

        [TestInitialize]
        public void Initialize()
        {
            id = "14300";
            employeeId = "24601";
            employeeName = "Matt Dediana";
            employeeSSN = "123-45-6789";
            mailingLabel = new List<PayStatementAddress>() { new PayStatementAddress("foo, bar st."), new PayStatementAddress("foo, bar 12345") };
            checkReferenceId = "ref";
            statementReferenceId = "ref-008";
            payDate = new DateTime(2017, 8, 1);
            periodEndDate = new DateTime(2017, 7, 31);
            periodGrossPay = 1114.14M;
            periodNetPay = 1023.11M;
            ytdGrossPay = 10010.28M;
            ytdNetPay = 9010.19M;
            comments = "some comments";
            sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSSN, mailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            sourceData_0 = new PayStatementSourceData("14301", employeeId, employeeName, employeeSSN, mailingLabel, "12010", "ref-009", payDate.AddMonths(-1), periodEndDate.AddMonths(-1), periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            yearToDateSourceEntities = new List<PayStatementSourceData>() { sourceData_0, sourceData };

            payrollRegisterEntry = new PayrollRegisterEntry("54321", employeeId, periodEndDate.AddMonths(-1).AddDays(1), periodEndDate, "PC", 1, "ref", "ref-008", false, null);
            payrollRegisterEntry_0 = new PayrollRegisterEntry("54322", employeeId, periodEndDate.AddMonths(-2).AddDays(1), periodEndDate.AddMonths(-1), "PC", 1, "12010", "ref-009", true, null);

            benefitDeductionId = "401k";
            enrollmentDate = new DateTime(2017, 1, 1);
            cancelDate = null;
            lastPayDate = null;
            benefitDeductions = new List<PersonBenefitDeduction>();
            benefitDeductions.Add(new PersonBenefitDeduction(employeeId, benefitDeductionId, enrollmentDate, cancelDate, lastPayDate));
            benefitDeductionId = "MEDF";
            benefitDeductions.Add(new PersonBenefitDeduction(employeeId, benefitDeductionId, enrollmentDate, cancelDate, lastPayDate));
            benefitDeductionId = "UNWA";
            benefitDeductions.Add(new PersonBenefitDeduction(employeeId, benefitDeductionId, enrollmentDate, cancelDate, lastPayDate));

            personEmploymentStatusId = "001";
            primaryPositionId = "ABC";
            personPositionId = "999";
            startDate = new DateTime(2017, 1, 1);
            endDate = null;
            personEmploymentStatuses = new List<PersonEmploymentStatus>();
            personEmploymentStatuses.Add(new PersonEmploymentStatus(personEmploymentStatusId, employeeId, primaryPositionId, personPositionId, startDate, endDate));

            payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
            payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);

            yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };

            benefitDeductionTypes = new List<BenefitDeductionType>();
            benefitDeductionTypes.Add(new BenefitDeductionType("401k", "401K Retirement", "Self-Service 401K Retirement Plan", BenefitDeductionTypeCategory.Benefit));
            benefitDeductionTypes.Add(new BenefitDeductionType("LIFE", "Life Insurance", "Self-Service Life Insurance", BenefitDeductionTypeCategory.Benefit));
            benefitDeductionTypes.Add(new BenefitDeductionType("MEDF", "Medical - Family", null, BenefitDeductionTypeCategory.Benefit));
            benefitDeductionTypes.Add(new BenefitDeductionType("UNWA", "United Way", "Self-Service United Way Deduction", BenefitDeductionTypeCategory.Deduction));

            taxCodes = new List<TaxCode>();
            federalWithholdingSingleTaxCodeId = "FWHS";
            var fedWithholdingSingleTaxCode = new TaxCode(federalWithholdingSingleTaxCodeId, "Federal Withholding Single", TaxCodeType.FederalWithholding) { FilingStatus = new TaxCodeFilingStatus("S", "Single") };
            taxCodes.Add(fedWithholdingSingleTaxCode);

            stateWithholdingSingleTaxCodeId = "VASM";
            var stateWithholdingMarriedTaxCode = new TaxCode(stateWithholdingSingleTaxCodeId, "Virginia Withholding Married", TaxCodeType.StateWithholding) { FilingStatus = new TaxCodeFilingStatus("M", "Married") };
            taxCodes.Add(stateWithholdingMarriedTaxCode);

            leaveTypes = new List<LeaveType>();
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "VAC", "Vacation") { TimeType = LeaveTypeCategory.Vacation });
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "SIC", "Sick Leave") { TimeType = LeaveTypeCategory.Sick });
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "JURY", "Jury Duty") { TimeType = LeaveTypeCategory.None });
            var compLeaveType = new LeaveType(Guid.NewGuid().ToString(), "COMP", "Comp Time") { TimeType = LeaveTypeCategory.Compensatory };
            leaveTypes.Add(compLeaveType);

            earningsTypes = new List<EarningsType>();
            earningsTypes.Add(new EarningsType("REG", "Regular", true, EarningsCategory.Regular, EarningsMethod.None, null));
            earningsTypes.Add(new EarningsType("ADJ", "Adjunct", true, EarningsCategory.Regular, EarningsMethod.None, null));
            earningsTypes.Add(new EarningsType("OVT", "Overtime", true, EarningsCategory.Overtime, EarningsMethod.None, 1.5m));
            earningsTypes.Add(new EarningsType("CMPE", "Comp Time Earned", true, EarningsCategory.Leave, EarningsMethod.Accrued, null, compLeaveType));
            earningsTypes.Add(new EarningsType("CMPT", "Comp Time Taken", true, EarningsCategory.Leave, EarningsMethod.Taken, null, compLeaveType));

            earningsDifferentials = new List<EarningsDifferential>();
            earningsDifferentials.Add(new EarningsDifferential("1ST", "First Shift"));
            earningsDifferentials.Add(new EarningsDifferential("2ND", "Second Shift"));

            positions = new List<Position>();
            positions.Add(new Position("ABC", "Alphabet", "Alpha", "ENG", new DateTime(2015, 1, 1), true));
            positions.Add(new Position("123", "Numeric", "Num", "MATH", new DateTime(2016, 1, 1), false));

            payStatementConfiguration = new PayStatementConfiguration()
            {
                DisplayZeroAmountBenefitDeductions = true,
                OffsetDaysCount = -5,
                PreviousYearsCount = 5,
                SocialSecurityNumberDisplay = SSNDisplay.Full,
                DisplayWithholdingStatusFlag = true,
                Message = "pay statement text",
                InstitutionName = "Ellucian U",
                InstitutionMailingLabel = mailingLabel
            };
            referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
            federalWithholdingTaxCodes = referenceUtility.GetTaxCodesForType(TaxCodeType.FederalWithholding);
            stateWithholdingTaxCodes = referenceUtility.GetTaxCodesForType(TaxCodeType.StateWithholding);
        }

        [TestClass]
        public class GeneralPayStatementReportConstructorTests : PayStatementReportTests
        {
            [TestMethod]
            public void BasePropertiesAreSet()
            {
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(id, report.Id);
                Assert.AreEqual(employeeId, report.EmployeeId);
                Assert.AreEqual(employeeName, report.EmployeeName);
                Assert.AreEqual(employeeSSN, report.EmployeeSSN);
                Assert.AreEqual(payStatementConfiguration.InstitutionName, report.InstitutionName);
                Assert.AreEqual(payStatementConfiguration.InstitutionMailingLabel, report.InstitutionMailingLabel);
                CollectionAssert.AreEqual(mailingLabel, report.EmployeeMailingLabel.ToList());
                Assert.AreEqual(checkReferenceId, report.PaycheckReferenceId);
                Assert.AreEqual(statementReferenceId, report.StatementReferenceId);
                Assert.AreEqual(payDate, report.PayDate);
                Assert.AreEqual(payrollRegisterEntry.PayPeriodStartDate, report.PeriodStartDate);
                Assert.AreEqual(periodEndDate, report.PeriodEndDate);
                Assert.AreEqual(periodGrossPay, report.PeriodGrossPay);
                Assert.AreEqual(periodNetPay, report.PeriodNetPay);
                Assert.AreEqual(ytdGrossPay, report.YearToDateGrossPay);
                Assert.AreEqual(ytdNetPay, report.YearToDateNetPay);
                Assert.AreEqual(comments, report.Comments);
            }
        }

        [TestClass]
        public class SsnTests : PayStatementReportTests
        {
            [TestMethod]
            public void EmptySSNTest()
            {
                sourceData = new PayStatementSourceData(id, employeeId, employeeName, null, mailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(string.Empty, report.EmployeeSSN);
            }

            [TestMethod]
            public void SSNDisplayFullTest()
            {
                payStatementConfiguration.SocialSecurityNumberDisplay = SSNDisplay.Full;
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(employeeSSN, report.EmployeeSSN);
            }

            [TestMethod]
            public void SSNDisplayLastFourTest()
            {
                payStatementConfiguration.SocialSecurityNumberDisplay = SSNDisplay.LastFour;
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                var employeeSSN = payStatementReportDatacontext.sourceData.EmployeeSSN;
                var lastFour = employeeSSN.Substring(employeeSSN.Length - 4);
                var firstPart = employeeSSN.Substring(0, employeeSSN.Length - 4);
                var maskedSSN = Regex.Replace(firstPart, "[0-9]", "X") + lastFour;
                Assert.AreEqual(maskedSSN, report.EmployeeSSN);
            }

            [TestMethod]
            public void SSNDisplayLastFourForShortSSNTest()
            {
                var shortSSN = "123";
                sourceData = new PayStatementSourceData(id, employeeId, employeeName, shortSSN, mailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementConfiguration.SocialSecurityNumberDisplay = SSNDisplay.LastFour;
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(shortSSN, report.EmployeeSSN);
            }
            [TestMethod]
            public void SSNDisplayHiddenTest()
            {
                payStatementConfiguration.SocialSecurityNumberDisplay = SSNDisplay.Hidden;
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(string.Empty, report.EmployeeSSN);
            }
        }

        [TestClass]
        public class PrimaryPositionTests : PayStatementReportTests
        {
            [TestMethod]
            public void PrimaryPositionTest()
            {
                var expectedPosition = referenceUtility.GetPosition(payStatementReportDatacontext.personEmploymentStatus.PrimaryPositionId);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(expectedPosition.ShortTitle, report.PrimaryPosition);
            }

            [TestMethod]
            public void PrimaryPositionIsEmptyWhenNoEmploymentStatusTest()
            {
                personEmploymentStatuses = new List<PersonEmploymentStatus>();
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(string.Empty, report.PrimaryPosition);
            }
        }

        [TestClass]
        public class WithholdingStatusAndExemptionsTests : PayStatementReportTests
        {
            PayrollRegisterTaxEntry federalWithholdingTaxEntry;
            PayrollRegisterTaxEntry stateWithholdingTaxEntry;

            [TestInitialize]
            public void WithholdingInitialize()
            {
                federalWithholdingTaxEntry = new PayrollRegisterTaxEntry(federalWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.Regular)
                {
                    Exemptions = 2,
                };

                stateWithholdingTaxEntry = new PayrollRegisterTaxEntry(stateWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.Regular)
                {
                    Exemptions = 1,
                };
            }

            [TestMethod]
            public void FederalWithholdingStatusAndExemptionsTest()
            {
                // add a federal withholding tax entry
                payrollRegisterEntry.TaxEntries.Add(federalWithholdingTaxEntry);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                var federalWithholdingPayrollRegisterTaxEntry = payStatementReportDatacontext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode != PayrollTaxProcessingCode.Inactive && te.ProcessingCode != PayrollTaxProcessingCode.TaxableExempt)
                    .FirstOrDefault(te => federalWithholdingTaxCodes.Any(t => t.Code == te.TaxCode));

                var federalWithholdingPayrollRegisterTaxCode = referenceUtility.GetTaxCode(federalWithholdingPayrollRegisterTaxEntry.TaxCode);
                var expectedFederalFilingStatus = federalWithholdingPayrollRegisterTaxCode.FilingStatus.Description;
                var expectedFederalExemptions = federalWithholdingPayrollRegisterTaxEntry.Exemptions;

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                var actualFederalFilingStatus = report.FederalWithholdingStatus;
                var actualFederalExemptions = report.FederalExemptions;

                Assert.AreEqual(expectedFederalFilingStatus, actualFederalFilingStatus);
                Assert.AreEqual(expectedFederalExemptions, actualFederalExemptions);
            }

            [TestMethod]
            public void NoFederalWithholdingStatusAndExemptionsTest()
            {
                // add only a state withholding tax entry, no federal
                payrollRegisterEntry.TaxEntries.Add(stateWithholdingTaxEntry);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(string.Empty, report.FederalWithholdingStatus);
                Assert.AreEqual(0, report.FederalExemptions);
            }

            [TestMethod]
            public void StateWithholdingStatusAndExemptionsTest()
            {
                // add a state withholding tax entry
                payrollRegisterEntry.TaxEntries.Add(stateWithholdingTaxEntry);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                var stateWithholdingtaxPayrollRegisterEntry = payStatementReportDatacontext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode != PayrollTaxProcessingCode.Inactive && te.ProcessingCode != PayrollTaxProcessingCode.TaxableExempt)
                    .FirstOrDefault(te => stateWithholdingTaxCodes.Any(t => t.Code == te.TaxCode));

                var stateWithholdingTaxCode = referenceUtility.GetTaxCode(stateWithholdingtaxPayrollRegisterEntry.TaxCode);
                var expectedStateFilingStatus = stateWithholdingTaxCode.FilingStatus.Description;
                var expectedStateExemptions = stateWithholdingtaxPayrollRegisterEntry.Exemptions;

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                var actualStateFilingStatus = report.StateWithholdingStatus;
                var actualStateExemptions = report.StateExemptions;

                Assert.AreEqual(expectedStateFilingStatus, report.StateWithholdingStatus);
                Assert.AreEqual(expectedStateExemptions, report.StateExemptions);
            }

            [TestMethod]
            public void NoStateWithholdingStatusAndExemptionsTest()
            {
                // add only a federal withholding tax, no state
                payrollRegisterEntry.TaxEntries.Add(federalWithholdingTaxEntry);
                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                Assert.AreEqual(string.Empty, report.StateWithholdingStatus);
                Assert.AreEqual(0, report.StateExemptions);
            }

            [TestClass]
            public class AdditionalWithholdingTests : WithholdingStatusAndExemptionsTests
            {
                [TestMethod]
                public void AdditionalFederalWithholdingTest()
                {
                    var federalWithholdingAdditionalTaxEntry = new PayrollRegisterTaxEntry(federalWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.AdditionalTaxAmount) { SpecialProcessingAmount = 100 };
                    payrollRegisterEntry.TaxEntries.Add(federalWithholdingAdditionalTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    var expectedFederalWithholdingTaxEntry = payStatementReportDatacontext.payrollRegisterEntry.TaxEntries
                            .Where(te => te.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxAmount)
                        .FirstOrDefault(te => federalWithholdingTaxCodes.Any(t => t.Code == te.TaxCode));
                    var expectedAdditionalAmount = expectedFederalWithholdingTaxEntry.SpecialProcessingAmount;

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    var actualAdditionalAmount = report.AdditionalFederalWithholding;

                    Assert.AreEqual(expectedAdditionalAmount, actualAdditionalAmount);
                }

                [TestMethod]
                public void NoAdditionalFederalWithholdingTest()
                {
                    payrollRegisterEntry.TaxEntries.Add(federalWithholdingTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    Assert.AreEqual(0, report.AdditionalFederalWithholding);
                }

                [TestMethod]
                public void NoAdditionalFederalWithholdingAmountTest()
                {
                    // add a tax entry with the additional withholding processing code but no additional amount
                    var federalWithholdingAdditionalTaxEntry = new PayrollRegisterTaxEntry(federalWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.AdditionalTaxAmount);
                    payrollRegisterEntry.TaxEntries.Add(federalWithholdingAdditionalTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    Assert.AreEqual(0, report.AdditionalFederalWithholding);
                }

                [TestMethod]
                public void AdditionalStateWithholdingTest()
                {
                    var stateWithholdingAdditionalTaxEntry = new PayrollRegisterTaxEntry(stateWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.AdditionalTaxAmount) { SpecialProcessingAmount = 55 };
                    payrollRegisterEntry.TaxEntries.Add(stateWithholdingAdditionalTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    var expectedStateWithholdingTaxEntry = payStatementReportDatacontext.payrollRegisterEntry.TaxEntries
                            .Where(te => te.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxAmount)
                        .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));
                    var expectedAdditionalAmount = expectedStateWithholdingTaxEntry.SpecialProcessingAmount;

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    var actualAdditionalAmount = report.AdditionalStateWithholding;

                    Assert.AreEqual(expectedAdditionalAmount, actualAdditionalAmount);
                }

                [TestMethod]
                public void NoAdditionalStateWithholdingTest()
                {
                    payrollRegisterEntry.TaxEntries.Add(stateWithholdingTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    Assert.AreEqual(0, report.AdditionalStateWithholding);
                }

                [TestMethod]
                public void NoAdditionalStateWithholdingAmountTest()
                {
                    // add a tax entry with the additional withholding processing code but no additional amount
                    var stateWithholdingAdditionalTaxEntry = new PayrollRegisterTaxEntry(stateWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.AdditionalTaxAmount);
                    payrollRegisterEntry.TaxEntries.Add(stateWithholdingAdditionalTaxEntry);
                    payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);

                    report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                    Assert.AreEqual(0, report.AdditionalStateWithholding);
                }
            }
        }

        [TestClass]
        public class LeaveTests : PayStatementReportTests
        {
            PayrollRegisterLeaveEntry sickLeaveEntry;
            PayrollRegisterLeaveEntry annualVacationLeaveEntry;
            PayrollRegisterLeaveEntry personalVacationLeaveEntry;
            PayrollRegisterLeaveEntry compTimeTakenLeaveEntry;
            PayrollRegisterLeaveEntry juryDutyLeaveEntry;

            [TestInitialize]
            public void LeaveInitialize()
            {
                sickLeaveEntry = new PayrollRegisterLeaveEntry("SICK", "SIC", 0, 20);
                annualVacationLeaveEntry = new PayrollRegisterLeaveEntry("VACA", "VAC", 0, 36);
                personalVacationLeaveEntry = new PayrollRegisterLeaveEntry("VACP", "VAC", 8, 29);
                compTimeTakenLeaveEntry = new PayrollRegisterLeaveEntry("CMPT", "COMP", 2, 10);
                juryDutyLeaveEntry = new PayrollRegisterLeaveEntry("JURY", "JURY", 5, 0);
            }

            [TestMethod]
            public void LeaveTest()
            {
                payrollRegisterEntry.LeaveEntries.Add(sickLeaveEntry);
                payrollRegisterEntry.LeaveEntries.Add(annualVacationLeaveEntry);
                payrollRegisterEntry.LeaveEntries.Add(personalVacationLeaveEntry);
                payrollRegisterEntry.LeaveEntries.Add(compTimeTakenLeaveEntry);
                payrollRegisterEntry.LeaveEntries.Add(juryDutyLeaveEntry);

                var leaveItems = new List<PayStatementLeave>();
                var leaveEntryGroups = payrollRegisterEntry.LeaveEntries.GroupBy(le => le.LeaveType);
                foreach (var leaveEntryGroup in leaveEntryGroups)
                {
                    var leaveType = referenceUtility.GetLeaveType(leaveEntryGroup.Key);
                    if (leaveType.TimeType == LeaveTypeCategory.Sick ||
                        leaveType.TimeType == LeaveTypeCategory.Compensatory ||
                        leaveType.TimeType == LeaveTypeCategory.Vacation)
                    {
                        var payStatementLeave = new PayStatementLeave(leaveType.Code,
                            leaveType.Description,
                            leaveEntryGroup.Sum(entry => entry.LeaveTaken),
                            leaveEntryGroup.Sum(entry => entry.LeaveRemaining));

                        leaveItems.Add(payStatementLeave);
                    }
                }
                var expectedLeaveItems = leaveItems.OrderByDescending(l => l.Description).ToList();

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);
                var actualLeaveItems = report.Leave.ToList();

                Assert.AreEqual(expectedLeaveItems.Count, actualLeaveItems.Count);
                for (int i = 0; i < leaveItems.Count; i++)
                {
                    Assert.AreEqual(expectedLeaveItems[i].LeaveTypeId, actualLeaveItems[i].LeaveTypeId);
                    Assert.AreEqual(expectedLeaveItems[i].Description, actualLeaveItems[i].Description);
                    Assert.AreEqual(expectedLeaveItems[i].LeaveTaken, actualLeaveItems[i].LeaveTaken);
                    Assert.AreEqual(expectedLeaveItems[i].LeaveRemaining, actualLeaveItems[i].LeaveRemaining);
                }
            }
        }

        [TestClass]
        public class TaxableBenefitTests : PayStatementReportTests
        {
            string cellBenefitId; string cellBenefitDescription;
            string gtrmBenefitId; string gtrmBenefitDescription;
            string tuitBenefitId; string tuitBenefitDescription;

            [TestInitialize]
            public void TaxableBenefitInitialze()
            {
                cellBenefitId = "CELL";
                gtrmBenefitId = "GTL";
                tuitBenefitId = "TUIT";

                benefitDeductionTypes.Add(new BenefitDeductionType(cellBenefitId, "Cell Phone Reimbursement", null, BenefitDeductionTypeCategory.Benefit));
                benefitDeductionTypes.Add(new BenefitDeductionType(gtrmBenefitId, "Group Term Life", "Self-Service Group Term Life Insurance", BenefitDeductionTypeCategory.Benefit));
                benefitDeductionTypes.Add(new BenefitDeductionType(tuitBenefitId, "Tuit Reimb", "Self-Service Tuition Reimbursement", BenefitDeductionTypeCategory.Benefit));
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);

                cellBenefitDescription = referenceUtility.GetBenefitDeductionType(cellBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(cellBenefitId).Description;
                gtrmBenefitDescription = referenceUtility.GetBenefitDeductionType(gtrmBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(gtrmBenefitId).Description;
                tuitBenefitDescription = referenceUtility.GetBenefitDeductionType(tuitBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(tuitBenefitId).Description;

            }

            [TestMethod]
            public void TaxableBenefitTest()
            {
                // add some taxable benefit entries for the current period
                payrollRegisterEntry.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(gtrmBenefitId, federalWithholdingSingleTaxCodeId, 5.00m));
                payrollRegisterEntry.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(gtrmBenefitId, "FICA", 5.00m));
                payrollRegisterEntry.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(tuitBenefitId, federalWithholdingSingleTaxCodeId, 100.00m));
                payrollRegisterEntry.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(tuitBenefitId, "FICA", 100.00m));

                // add some taxable benefit entries for the prior period
                payrollRegisterEntry_0.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(gtrmBenefitId, federalWithholdingSingleTaxCodeId, 5.00m));
                payrollRegisterEntry_0.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(gtrmBenefitId, "FICA", 5.00m));
                payrollRegisterEntry_0.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(tuitBenefitId, federalWithholdingSingleTaxCodeId, 500.00m));
                payrollRegisterEntry_0.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(tuitBenefitId, "FICA", 500.00m));
                payrollRegisterEntry_0.TaxableBenefitEntries.Add(new PayrollRegisterTaxableBenefitEntry(cellBenefitId, federalWithholdingSingleTaxCodeId, 45.00m));

                List<PayStatementTaxableBenefit> expectedTaxableBenefitItems;
                expectedTaxableBenefitItems = new List<PayStatementTaxableBenefit>()
                {
                    new PayStatementTaxableBenefit(cellBenefitId, cellBenefitDescription, 0, 45m),
                    new PayStatementTaxableBenefit(gtrmBenefitId, gtrmBenefitDescription, 5, 10m),
                    new PayStatementTaxableBenefit(tuitBenefitId, tuitBenefitDescription, 100,600m)
                };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualTaxableBenefitItems = report.TaxableBenefits.ToList();

                Assert.AreEqual(expectedTaxableBenefitItems.Count, actualTaxableBenefitItems.Count);
                for (int i = 0; i < expectedTaxableBenefitItems.Count; i++)
                {
                    Assert.AreEqual(expectedTaxableBenefitItems[i].TaxableBenefitId, actualTaxableBenefitItems[i].TaxableBenefitId);
                    Assert.AreEqual(expectedTaxableBenefitItems[i].TaxableBenefitDescription, actualTaxableBenefitItems[i].TaxableBenefitDescription);
                    Assert.AreEqual(expectedTaxableBenefitItems[i].TaxableBenefitAmt, actualTaxableBenefitItems[i].TaxableBenefitAmt);
                    Assert.AreEqual(expectedTaxableBenefitItems[i].TaxableBenefitYtdAmt, actualTaxableBenefitItems[i].TaxableBenefitYtdAmt);
                }
            }
        }

        [TestClass]
        public class BankDepositTests : PayStatementReportTests
        {
            PayStatementSourceBankDeposit bankDeposit1;
            PayStatementSourceBankDeposit bankDeposit2;

            [TestInitialize]
            public void BankDepositInitialize()
            {
                bankDeposit1 = new PayStatementSourceBankDeposit("Great Big Bank", Base.Entities.BankAccountType.Checking, "4321", 43.21m);
                sourceData.SourceBankDeposits.Add(bankDeposit1);
                bankDeposit2 = new PayStatementSourceBankDeposit("Teeny Tiny Bank", Base.Entities.BankAccountType.Savings, "0001", 1.00m);
                sourceData.SourceBankDeposits.Add(bankDeposit2);

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
            }

            [TestMethod]
            public void BankDepositTest()
            {
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var expectedBankDeposits = sourceData.SourceBankDeposits.ToList();
                var actualBankDeposits = report.Deposits.ToList();

                Assert.AreEqual(expectedBankDeposits.Count, actualBankDeposits.Count);
                for (int i = 0; i < expectedBankDeposits.Count; i++)
                {
                    Assert.AreEqual(expectedBankDeposits[i].BankName, actualBankDeposits[i].BankName);
                    Assert.AreEqual(expectedBankDeposits[i].BankAccountType, actualBankDeposits[i].BankAccountType);
                    Assert.AreEqual(expectedBankDeposits[i].AccountIdLastFour, actualBankDeposits[i].AccountIdLastFour);
                    Assert.AreEqual(expectedBankDeposits[i].DepositAmount, actualBankDeposits[i].DepositAmount);
                }
            }
        }

        [TestClass]
        public class DeductionsTests : PayStatementReportTests
        {
            //string federalWithholdingSingleTaxCodeId; already declared
            PayrollRegisterTaxEntry federalWithholdingTaxEntry;

            //string stateWithholdingSingleTaxCodeId;  already declared
            PayrollRegisterTaxEntry stateWithholdingTaxEntry;

            string xtaxCodeId;
            TaxCode xtaxCode;
            PayrollRegisterTaxEntry xtaxEntry;

            string r401kBenefitId;
            PayrollRegisterBenefitDeductionEntry r401kBenefitEntry;

            string medfBenefitId;
            PayrollRegisterBenefitDeductionEntry medfBenefitEntry;

            string unwaBenefitId;

            PayrollRegisterBenefitDeductionEntry zeroAmountBenefitEntry;

            public class PayStatementReportDeduction
            {
                public string code;
                public string description;
                public PayStatementDeductionType type;
                public decimal? employeePeriodAmount;
                public decimal? employerPeriodAmount;
                public decimal? applicableGrossPeriodAmount;
                public decimal? employeeYearToDateAmount;
                public decimal? employerYearToDateAmount;
                public decimal? applicableGrossYearToDateAmount;
            }

            [TestInitialize]
            public void DeductionsInitialize()
            {
                // this tax wasn't on the original taxCodes list established earlier
                xtaxCodeId = "XTAX";
                xtaxCode = new TaxCode(xtaxCodeId, "Mystery Tax", TaxCodeType.LocalWithholding);
                taxCodes.Add(xtaxCode);
                referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);

                // set up some test deduction entries for taxes, benefits, and other deductions
                xtaxEntry = new PayrollRegisterTaxEntry(xtaxCodeId, PayrollTaxProcessingCode.Regular)
                {
                    Exemptions = 0,
                    EmployeeTaxAmount = 15m,
                    EmployeeTaxableAmount = 1500m,
                    EmployerTaxAmount = 16m,
                    EmployerTaxableAmount = 1600m
                };

                federalWithholdingTaxEntry = new PayrollRegisterTaxEntry(federalWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.Regular)
                {
                    Exemptions = 2,
                    EmployeeTaxAmount = 5m,
                    EmployeeTaxableAmount = 100m,
                    EmployerTaxAmount = 10m,
                    EmployerTaxableAmount = 200m
                };

                stateWithholdingTaxEntry = new PayrollRegisterTaxEntry(stateWithholdingSingleTaxCodeId, PayrollTaxProcessingCode.Regular)
                {
                    Exemptions = 1,
                    EmployeeTaxAmount = 2m,
                    EmployeeTaxableAmount = 50m,
                    EmployerTaxAmount = 4m,
                    EmployerTaxableAmount = 100m
                };

                r401kBenefitId = "401k";
                r401kBenefitEntry = new PayrollRegisterBenefitDeductionEntry(r401kBenefitId)
                {
                    EmployeeAmount = 125m,
                    EmployeeBasisAmount = 1500m,
                    EmployerAmount = 125m,
                    EmployerBasisAmount = 1500m
                };

                medfBenefitId = "MEDF";
                medfBenefitEntry = new PayrollRegisterBenefitDeductionEntry(medfBenefitId)
                {
                    EmployeeAmount = 85m,
                    EmployeeBasisAmount = 1500m,
                    EmployerAmount = 300m,
                    EmployerBasisAmount = 1500m
                };

                unwaBenefitId = "UNWA";

                zeroAmountBenefitEntry = new PayrollRegisterBenefitDeductionEntry(r401kBenefitId)
                {
                    EmployeeAmount = null,
                    EmployeeBasisAmount = 1500m,
                    EmployerAmount = null,
                    EmployerBasisAmount = 1500m,
                };
            }

            [TestMethod]
            public void DeductionsTest()
            {
                // add withholding taxes to payroll register entries for current and prior periods
                payrollRegisterEntry.TaxEntries.Add(federalWithholdingTaxEntry);
                payrollRegisterEntry_0.TaxEntries.Add(federalWithholdingTaxEntry);
                payrollRegisterEntry.TaxEntries.Add(stateWithholdingTaxEntry);
                payrollRegisterEntry_0.TaxEntries.Add(stateWithholdingTaxEntry);

                // add a tax deduction for prior period only 
                payrollRegisterEntry_0.TaxEntries.Add(xtaxEntry);

                // add a benefit deduction for current and prior period
                payrollRegisterEntry.BenefitDeductionEntries.Add(medfBenefitEntry);
                payrollRegisterEntry_0.BenefitDeductionEntries.Add(medfBenefitEntry);

                // add a benefit deduction for prior period only
                payrollRegisterEntry_0.BenefitDeductionEntries.Add(r401kBenefitEntry);

                List<PayStatementReportDeduction> expectedDeductionItems;
                expectedDeductionItems = new List<PayStatementReportDeduction>()
                {
                    new PayStatementReportDeduction
                    {
                        code = federalWithholdingSingleTaxCodeId,
                        description = referenceUtility.GetTaxCode(federalWithholdingSingleTaxCodeId).Description,
                        type = PayStatementDeductionType.Tax,
                        employeePeriodAmount = 5m,
                        employeeYearToDateAmount = 10m,
                        employerPeriodAmount = 10m,
                        employerYearToDateAmount = 20m,
                        applicableGrossPeriodAmount = 100m,
                        applicableGrossYearToDateAmount = 200m
                    },
                    new PayStatementReportDeduction
                    {
                        code = stateWithholdingSingleTaxCodeId,
                        description = referenceUtility.GetTaxCode(stateWithholdingSingleTaxCodeId).Description,
                        type = PayStatementDeductionType.Tax,
                        employeePeriodAmount = 2m,
                        employeeYearToDateAmount = 4m,
                        employerPeriodAmount = 4m,
                        employerYearToDateAmount = 8m,
                        applicableGrossPeriodAmount = 50m,
                        applicableGrossYearToDateAmount = 100m
                    },
                    new PayStatementReportDeduction
                    {
                        code = xtaxCodeId,
                        description = referenceUtility.GetTaxCode(xtaxCodeId).Description,
                        type = PayStatementDeductionType.Tax,
                        employeePeriodAmount = null,
                        employeeYearToDateAmount = 15m,
                        employerPeriodAmount = null,
                        employerYearToDateAmount = 16m,
                        applicableGrossPeriodAmount = null,
                        applicableGrossYearToDateAmount = 1500m
                    },
                    new PayStatementReportDeduction
                    {
                        code = medfBenefitId,
                        description = referenceUtility.GetBenefitDeductionType(medfBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(medfBenefitId).Description,
                        type = PayStatementDeductionType.Benefit,
                        employeePeriodAmount = 85m,
                        employeeYearToDateAmount = 170m,
                        employerPeriodAmount = 300,
                        employerYearToDateAmount = 600m,
                        applicableGrossPeriodAmount = 1500m,
                        applicableGrossYearToDateAmount = 3000m
                    },
                    new PayStatementReportDeduction
                    {
                        code = r401kBenefitId,
                        description = referenceUtility.GetBenefitDeductionType(r401kBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(r401kBenefitId).Description,
                        type = PayStatementDeductionType.Benefit,
                        employeePeriodAmount = null,
                        employeeYearToDateAmount = 125m,
                        employerPeriodAmount = null,
                        employerYearToDateAmount = 125m,
                        applicableGrossPeriodAmount = null,
                        applicableGrossYearToDateAmount = 1500m
                    },
                    new PayStatementReportDeduction
                    {
                        code = unwaBenefitId,
                        description = referenceUtility.GetBenefitDeductionType(unwaBenefitId).SelfServiceDescription ?? referenceUtility.GetBenefitDeductionType(unwaBenefitId).Description,
                        type = PayStatementDeductionType.Deduction,
                        employeePeriodAmount = null,
                        employeeYearToDateAmount = 0,
                        employerPeriodAmount = null,
                        employerYearToDateAmount = 0,
                        applicableGrossPeriodAmount = null,
                        applicableGrossYearToDateAmount = 0
                    }
                };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualDeductionItems = report.Deductions.ToList();

                Assert.AreEqual(expectedDeductionItems.Count, actualDeductionItems.Count);
                for (int i = 0; i < expectedDeductionItems.Count; i++)
                {
                    Assert.AreEqual(expectedDeductionItems[i].code, actualDeductionItems[i].Code);
                    Assert.AreEqual(expectedDeductionItems[i].description, actualDeductionItems[i].Description);
                    Assert.AreEqual(expectedDeductionItems[i].type, actualDeductionItems[i].Type);
                    Assert.AreEqual(expectedDeductionItems[i].employeePeriodAmount, actualDeductionItems[i].EmployeePeriodAmount);
                    Assert.AreEqual(expectedDeductionItems[i].employeeYearToDateAmount, actualDeductionItems[i].EmployeeYearToDateAmount);
                    Assert.AreEqual(expectedDeductionItems[i].employerPeriodAmount, actualDeductionItems[i].EmployerPeriodAmount);
                    Assert.AreEqual(expectedDeductionItems[i].employerYearToDateAmount, actualDeductionItems[i].EmployerYearToDateAmount);
                    Assert.AreEqual(expectedDeductionItems[i].applicableGrossPeriodAmount, actualDeductionItems[i].ApplicableGrossPeriodAmount);
                    Assert.AreEqual(expectedDeductionItems[i].applicableGrossYearToDateAmount, actualDeductionItems[i].ApplicableGrossYearToDateAmount);
                }
            }

        }

        [TestClass]
        public class EarningsTests : PayStatementReportTests
        {
            string regEarningsTypeId;
            string adjEarningsTypeId;
            string ovtEarningsTypeId;
            string firstShiftId;
            string secondShiftId;
            PayrollRegisterEarningsEntry earningsEntryWith1stShiftDiff;
            PayrollRegisterEarningsEntry earningsEntryWith2ndShiftDiff;

            string compTimeAccruedEarningsTypeId;
            string compTimeTakenEarningsTypeId;

            public class PayStatementReportEarnings
            {
                public string earningsTypeId;
                public string earningsTypeDescription;
                public decimal? unitsWorked;
                public decimal? rate;
                public decimal? periodPaymentAmount;
                public decimal yearToDatePaymentAmount;

            }

            [TestInitialize]
            public void EarningsInitialize()
            {
                regEarningsTypeId = "REG";
                adjEarningsTypeId = "ADJ";
                ovtEarningsTypeId = "OVT";
                firstShiftId = "1ST";
                secondShiftId = "2ND";

                compTimeAccruedEarningsTypeId = "CMPE";
                compTimeTakenEarningsTypeId = "CMPT";
            }

            [TestMethod]
            public void EarningsTest()
            {
                // add some earnings for current period
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(ovtEarningsTypeId, 112.5m, 70, 42.5m, 5, 15, HourlySalaryIndicator.Hourly, isAdj));

                // add some earnings for prior period
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Salary, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 300m, 300, 0, 30, 10m, HourlySalaryIndicator.Salary, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(adjEarningsTypeId, 1000m, 1000, 0, 10, null, HourlySalaryIndicator.Salary, isAdj));

                List<PayStatementReportEarnings> expectedPayStatementEarningsItems;
                expectedPayStatementEarningsItems = new List<PayStatementReportEarnings>()
                {
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = adjEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(adjEarningsTypeId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = null,
                        yearToDatePaymentAmount = 1000m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = regEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(regEarningsTypeId).Description,
                        unitsWorked = 80,
                        rate = 10m,
                        periodPaymentAmount = 800m,
                        yearToDatePaymentAmount = 1500m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = ovtEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(ovtEarningsTypeId).Description,
                        unitsWorked = 5,
                        rate = 22.5m, //rate on the payroll register entry * OVT earnings type factor
                        periodPaymentAmount = 112.5m,
                        yearToDatePaymentAmount = 112.5m
                    }
                };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();

                Assert.AreEqual(expectedPayStatementEarningsItems.Count, actualPayStatementEarningsItems.Count);
                for (int i = 0; i < expectedPayStatementEarningsItems.Count; i++)
                {
                    Assert.AreEqual(expectedPayStatementEarningsItems.Count, actualPayStatementEarningsItems.Count);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeId, actualPayStatementEarningsItems[i].EarningsTypeId);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeDescription, actualPayStatementEarningsItems[i].EarningsTypeDescription);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].unitsWorked, actualPayStatementEarningsItems[i].UnitsWorked);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].rate, actualPayStatementEarningsItems[i].Rate);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].periodPaymentAmount, actualPayStatementEarningsItems[i].PeriodPaymentAmount);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].yearToDatePaymentAmount, actualPayStatementEarningsItems[i].YearToDatePaymentAmount);
                }
            }

            [TestMethod]
            public void EarningsRateIsNullWhenRatesAreDifferentTest()
            {
                // add some earnings for same earnings type but different pay rates
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 300m, 300, 0, 20, 15m, HourlySalaryIndicator.Hourly, isAdj));

                PayStatementReportEarnings expectedPayStatementEarningsItem;
                expectedPayStatementEarningsItem =
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = regEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(regEarningsTypeId).Description,
                        unitsWorked = 60,
                        rate = null,
                        periodPaymentAmount = 700m,
                        yearToDatePaymentAmount = 700m
                    };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();

                Assert.AreEqual(expectedPayStatementEarningsItem.earningsTypeId, actualPayStatementEarningsItems[0].EarningsTypeId);
                Assert.AreEqual(expectedPayStatementEarningsItem.earningsTypeDescription, actualPayStatementEarningsItems[0].EarningsTypeDescription);
                Assert.AreEqual(expectedPayStatementEarningsItem.unitsWorked, actualPayStatementEarningsItems[0].UnitsWorked);
                Assert.AreEqual(expectedPayStatementEarningsItem.rate, actualPayStatementEarningsItems[0].Rate);
                Assert.AreEqual(expectedPayStatementEarningsItem.periodPaymentAmount, actualPayStatementEarningsItems[0].PeriodPaymentAmount);
                Assert.AreEqual(expectedPayStatementEarningsItem.yearToDatePaymentAmount, actualPayStatementEarningsItems[0].YearToDatePaymentAmount);
            }

            [TestMethod]
            public void ExcludeAccruedCompTimeEarningsTest()
            {
                // add some earnings including comp time earned and taken for current period
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(compTimeAccruedEarningsTypeId, 90, 90, 0, 6, 15m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(compTimeTakenEarningsTypeId, 45m, 45, 0, 3, 15m, HourlySalaryIndicator.Hourly, isAdj));

                // add some earnings including comp time  earned for prior period
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(compTimeAccruedEarningsTypeId, 300, 300, 0, 20, 15m, HourlySalaryIndicator.Hourly, isAdj));

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualAccruedCompTime = report.Earnings.Where(e => e.EarningsTypeId == compTimeAccruedEarningsTypeId);

                Assert.AreEqual(0, actualAccruedCompTime.ToList().Count);
            }

            [TestMethod]
            public void StipendEarningsTest()
            {
                // add some earnings for current period including stipend earnings
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 600m, 600, 0, 60, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, "STIP001", 200m, 200, 0, 20, 10m, HourlySalaryIndicator.Hourly, isAdj));
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(ovtEarningsTypeId, 112.5m, 75m, 37.5m, 5, 15, HourlySalaryIndicator.Hourly, isAdj));

                // add some earnings for prior period including stipend earnings
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Salary, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(regEarningsTypeId, "STIP001", 300m, 300, 0, 30, 10m, HourlySalaryIndicator.Salary, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(adjEarningsTypeId, 1000m, 1000, 0, 10, null, HourlySalaryIndicator.Salary, isAdj));
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(adjEarningsTypeId, "STIP002", 750m, 750, 0, 10, null, HourlySalaryIndicator.Salary, isAdj));

                List<PayStatementReportEarnings> expectedPayStatementEarningsItems;
                expectedPayStatementEarningsItems = new List<PayStatementReportEarnings>()
                {
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = adjEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(adjEarningsTypeId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = null,
                        yearToDatePaymentAmount = 1000m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = adjEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsTypeAsStipend(adjEarningsTypeId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = null,
                        yearToDatePaymentAmount = 750m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = regEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(regEarningsTypeId).Description,
                        unitsWorked = 60,
                        rate = 10m,
                        periodPaymentAmount = 600m,
                        yearToDatePaymentAmount = 1000m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = regEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsTypeAsStipend(regEarningsTypeId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = 200m,
                        yearToDatePaymentAmount = 500m
                    },
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = ovtEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(ovtEarningsTypeId).Description,
                        unitsWorked = 5,
                        rate = 22.5m, //rate on the payroll register entry * OVT earnings type factor
                        periodPaymentAmount = 112.5m,
                        yearToDatePaymentAmount = 112.5m
                    }
                };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();

                Assert.AreEqual(expectedPayStatementEarningsItems.Count, actualPayStatementEarningsItems.Count);
                for (int i = 0; i < expectedPayStatementEarningsItems.Count; i++)
                {
                    Assert.AreEqual(expectedPayStatementEarningsItems.Count, actualPayStatementEarningsItems.Count);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeId, actualPayStatementEarningsItems[i].EarningsTypeId);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeDescription, actualPayStatementEarningsItems[i].EarningsTypeDescription);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].unitsWorked, actualPayStatementEarningsItems[i].UnitsWorked);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].rate, actualPayStatementEarningsItems[i].Rate);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].periodPaymentAmount, actualPayStatementEarningsItems[i].PeriodPaymentAmount);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].yearToDatePaymentAmount, actualPayStatementEarningsItems[i].YearToDatePaymentAmount);
                }
            }

            [TestMethod]
            public void DifferentialEarningsTest()
            {
                // add some earnings for current period including differential earnings
                earningsEntryWith1stShiftDiff = new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj);
                earningsEntryWith1stShiftDiff.SetEarningsDifferential(firstShiftId, 110m, 10, 11m);
                payrollRegisterEntry.EarningsEntries.Add(earningsEntryWith1stShiftDiff);
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(ovtEarningsTypeId, 112.5m, 50, 62.5m, 5, 15, HourlySalaryIndicator.Hourly, isAdj));

                // add some earnings for prior period including differential earnings
                payrollRegisterEntry_0.EarningsEntries.Add(earningsEntryWith1stShiftDiff);
                earningsEntryWith2ndShiftDiff = new PayrollRegisterEarningsEntry(regEarningsTypeId, 300m, 300, 0, 30, 10m, HourlySalaryIndicator.Hourly, isAdj);
                earningsEntryWith2ndShiftDiff.SetEarningsDifferential(secondShiftId, 60m, 5, 12m);
                payrollRegisterEntry_0.EarningsEntries.Add(earningsEntryWith2ndShiftDiff);
                payrollRegisterEntry_0.EarningsEntries.Add(new PayrollRegisterEarningsEntry(adjEarningsTypeId, 1000m, 1000, 0, 10, null, HourlySalaryIndicator.Salary, isAdj));

                List<PayStatementReportEarnings> expectedPayStatementEarningsItems;
                expectedPayStatementEarningsItems = new List<PayStatementReportEarnings>()
                {
                new PayStatementReportEarnings
                    {
                        earningsTypeId = adjEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(adjEarningsTypeId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = null,
                        yearToDatePaymentAmount = 1000m
                    },
                new PayStatementReportEarnings
                    {
                        earningsTypeId = regEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(regEarningsTypeId).Description,
                        unitsWorked = 40,
                        rate = 10m,
                        periodPaymentAmount = 400m,
                        yearToDatePaymentAmount = 1100m
                    },
                new PayStatementReportEarnings
                    {
                        earningsTypeId = ovtEarningsTypeId,
                        earningsTypeDescription = referenceUtility.GetEarningsType(ovtEarningsTypeId).Description,
                        unitsWorked = 5,
                        rate = 22.5m, //rate on the payroll register entry * OVT earnings type factor
                        periodPaymentAmount = 112.5m,
                        yearToDatePaymentAmount = 112.5m
                    },
                new PayStatementReportEarnings
                    {
                        earningsTypeId = firstShiftId,
                        earningsTypeDescription = referenceUtility.GetEarningsDifferential(firstShiftId).Description,
                        unitsWorked = 10,
                        rate = 11m,
                        periodPaymentAmount = 110m,
                        yearToDatePaymentAmount = 220m
                    },
                new PayStatementReportEarnings
                    {
                        earningsTypeId = secondShiftId,
                        earningsTypeDescription = referenceUtility.GetEarningsDifferential(secondShiftId).Description,
                        unitsWorked = null,
                        rate = null,
                        periodPaymentAmount = null,
                        yearToDatePaymentAmount = 60m
                    }
                };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();

                Assert.AreEqual(expectedPayStatementEarningsItems.Count, actualPayStatementEarningsItems.Count);
                for (int i = 0; i < expectedPayStatementEarningsItems.Count; i++)
                {
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeId, actualPayStatementEarningsItems[i].EarningsTypeId);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].earningsTypeDescription, actualPayStatementEarningsItems[i].EarningsTypeDescription);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].unitsWorked, actualPayStatementEarningsItems[i].UnitsWorked);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].rate, actualPayStatementEarningsItems[i].Rate);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].periodPaymentAmount, actualPayStatementEarningsItems[i].PeriodPaymentAmount);
                    Assert.AreEqual(expectedPayStatementEarningsItems[i].yearToDatePaymentAmount, actualPayStatementEarningsItems[i].YearToDatePaymentAmount);
                }
            }

            [TestMethod]
            public void DifferentialRateIsNullWhenRatesAreDifferentTest()
            {
                // add some earnings for same differential code but different rates
                earningsEntryWith1stShiftDiff = new PayrollRegisterEarningsEntry(regEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj);
                earningsEntryWith1stShiftDiff.SetEarningsDifferential(firstShiftId, 110m, 10, 11m);
                payrollRegisterEntry.EarningsEntries.Add(earningsEntryWith1stShiftDiff);
                earningsEntryWith1stShiftDiff = new PayrollRegisterEarningsEntry(adjEarningsTypeId, 400m, 400, 0, 40, 10m, HourlySalaryIndicator.Hourly, isAdj);
                earningsEntryWith1stShiftDiff.SetEarningsDifferential(firstShiftId, 120m, 10, 12m);
                payrollRegisterEntry.EarningsEntries.Add(earningsEntryWith1stShiftDiff);

                PayStatementReportEarnings expectedPayStatementEarningsItem;
                expectedPayStatementEarningsItem =
                    new PayStatementReportEarnings
                    {
                        earningsTypeId = firstShiftId,
                        earningsTypeDescription = referenceUtility.GetEarningsDifferential(firstShiftId).Description,
                        unitsWorked = 20,
                        rate = null,
                        periodPaymentAmount = 230m,
                        yearToDatePaymentAmount = 230m
                    };

                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext };
                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();
                Assert.AreEqual(expectedPayStatementEarningsItem.earningsTypeId, actualPayStatementEarningsItems[2].EarningsTypeId);
                Assert.AreEqual(expectedPayStatementEarningsItem.earningsTypeDescription, actualPayStatementEarningsItems[2].EarningsTypeDescription);
                Assert.AreEqual(expectedPayStatementEarningsItem.unitsWorked, actualPayStatementEarningsItems[2].UnitsWorked);
                Assert.AreEqual(expectedPayStatementEarningsItem.rate, actualPayStatementEarningsItems[2].Rate);
                Assert.AreEqual(expectedPayStatementEarningsItem.periodPaymentAmount, actualPayStatementEarningsItems[2].PeriodPaymentAmount);
                Assert.AreEqual(expectedPayStatementEarningsItem.yearToDatePaymentAmount, actualPayStatementEarningsItems[2].YearToDatePaymentAmount);
            }

            [TestMethod]
            public void OvertimeRateIsNullWhenMathIsInvalidTest()
            {
                //Period Amount = 112.5
                //Rate = 10 * 1.5(factor) = 15
                //Units Worked = 5
                //Math is invalid because Units Worked * Rate != Period Amount
                // 5 * 15 == 75 != 112.5
                payrollRegisterEntry.EarningsEntries.Add(new PayrollRegisterEarningsEntry(ovtEarningsTypeId, 112.5m, 50, 62.5m, 5, 10, HourlySalaryIndicator.Hourly, false));



                payStatementReportDatacontext = new PayStatementReportDataContext(sourceData, payrollRegisterEntry, benefitDeductions, personEmploymentStatuses);
                payStatementReportDatacontext_0 = new PayStatementReportDataContext(sourceData_0, payrollRegisterEntry_0, benefitDeductions, personEmploymentStatuses);
                yearToDateDataContext = new List<PayStatementReportDataContext>() { payStatementReportDatacontext_0, payStatementReportDatacontext };

                report = new PayStatementReport(payStatementReportDatacontext, yearToDateDataContext, referenceUtility);

                var actualPayStatementEarningsItems = report.Earnings.ToList();

                Assert.AreEqual(1, actualPayStatementEarningsItems.Count);
                Assert.IsNull(actualPayStatementEarningsItems[0].Rate);
            }
        }
    }
}

