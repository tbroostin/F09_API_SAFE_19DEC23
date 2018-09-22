using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayrollRegisterRepositoryTests : BaseRepositorySetup
    {

        public PayrollRegisterRepository repositoryUnderTest;

        public TestPayrollRegisterRepository testData;

        public void PayrollRegisterRepositoryTestsInitialize()
        {
            MockInitialize();

            testData = new TestPayrollRegisterRepository();

            repositoryUnderTest = new PayrollRegisterRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            dataReaderMock.Setup(d => d.SelectAsync("PAYTODAT", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, v, p, b, i) => Task.FromResult(testData.payToDateRecords == null ? null :
                    testData.payToDateRecords.Select(r => r.id).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Paytodat>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.payToDateRecords == null ? null :
                    new Collection<Paytodat>(testData.payToDateRecords
                        .Where(r => ids.Contains(r.id))
                        .Select(r => createPaytodataDataContract(r))
                        .ToList())));

            dataReaderMock.Setup(d => d.SelectAsync("PAYCNTRL", ""))
                .Returns<string, string>((f, c) => Task.FromResult(testData.payControlRecords == null ? null :
                    testData.payControlRecords
                    .Select(pc => pc.id)
                    .ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Paycntrl>(It.IsAny<string[]>(), It.IsAny<bool>()))                
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.payControlRecords == null ? null :
                    new Collection<Paycntrl>(testData.payControlRecords
                        .Where(pc => ids.Contains(pc.id))
                        .Select(pc => new Paycntrl()
                        {
                            Recordkey = pc.id,
                            PclPeriodStartDate = pc.periodStartDate
                        })
                        .ToList())));
        }

        private Paytodat createPaytodataDataContract(TestPayrollRegisterRepository.PayToDateRecord record)
        {
            return new Paytodat()
            {
                Recordkey = record.id,
                PtdCheckNo = record.checkNumber,
                PtdAdviceNo = record.adviceNumber,
                PtdearnEntityAssociation = record.earnings.Select(earn => new PaytodatPtdearn()
                {
                    PtdEarnTypesAssocMember = earn.earningsCode,
                    PtdHSFlagsAssocMember = earn.hourlySalaryFlag,
                    PtdAmountsAssocMember = earn.totalAmount,
                    PtdBaseEarningsAssocMember = earn.baseAmount,
                    PtdEarnFactorEarningsAssocMember = earn.earningsFactorAmount,
                    PtdHoursAssocMember = earn.hours,
                    PtdRatesAssocMember = earn.rate,
                    PtdEarndiffIdAssocMember = earn.differentialId,
                    PtdEarnDiffEarningsAssocMember = earn.diffAmount,
                    PtdEarnDiffUnitsAssocMember = earn.diffHours,
                    PtdEarnDiffRatesAssocMember = earn.diffRate,
                    PtdStipendIdAssocMember = earn.stipendId
                }).ToList(),
                PtdtaxesEntityAssociation = record.taxes.Select(tax => new PaytodatPtdtaxes()
                {
                    PtdTaxCodesAssocMember = tax.taxCode,
                    PtdTaxFaterdCodesAssocMember = tax.processingCode,
                    PtdTaxFaterdAmtsAssocMember = tax.specialProcessingAmount,
                    PtdTaxExemptionsAssocMember = tax.exemptions,
                    PtdEmplyeTaxAmtsAssocMember = tax.employeeTaxAmount,
                    PtdEmplyeTaxableAmtsAssocMember = tax.employeeTaxableAmount,
                    PtdEmplyrTaxableAmtsAssocMember = tax.employerTaxableAmount,
                }).ToList(),
                PtdtaxexpEntityAssociation = record.expandedTaxes.Select(tax => new PaytodatPtdtaxexp()
                {
                    PtdTaxExpControllerAssocMember = tax.expandedId,
                    PtdTaxExpEmplyrTaxAmtsAssocMember = tax.employerTaxAmount
                }).ToList(),
                PtdbndedEntityAssociation = record.benefits.Select(bd => new PaytodatPtdbnded()
                {
                    PtdBdCodesAssocMember = bd.benefitCode,
                    PtdBdEmplyeCalcAmtsAssocMember = bd.employeeAmount,
                    PtdBdEmplyeBaseAmtsAssocMember = bd.employeeBaseAmount,
                    PtdBdEmplyrBaseAmtsAssocMember = bd.employerBaseAmount
                }).ToList(),
                PtdbdexpEntityAssociation = record.expandedBenefits.Select(bd => new PaytodatPtdbdexp()
                {
                    PtdBdExpControllerAssocMember = bd.expandedId,
                    PtdBdExpEmplyrCalcAmtsAssocMember = bd.employerAmount,
                }).ToList(),
                PtdleaveEntityAssociation = record.leave.Select(l => new PaytodatPtdleave()
                {
                    PtdLvCodesAssocMember = l.code,
                    PtdLvTypesAssocMember = l.leaveType,
                    PtdLvAccruedHoursAssocMember = l.accruedHours,
                    PtdLvPriorBalancesAssocMember = l.priorBalanceHours

                }).ToList(),
                PtdlvtknEntityAssociation = record.leaveTaken.Select(lt => new PaytodatPtdlvtkn()
                {
                    PtdLvTknControllerAssocMember = lt.controller,
                    PtdLvTknHoursAssocMember = lt.takenHours
                }).ToList(),
                PtdtxblbdEntityAssociation = record.expandedTaxableBenefits.Select(tb => new PaytodatPtdtxblbd()
                {
                    PtdTxblBdControllerAssocMember = tb.controller,
                    PtdTxblBdEmplyrAmtsAssocMember = tb.employerAmount
                }).ToList()
            };
        }


        [TestClass]
        public class GetPayrollRegisterTests : PayrollRegisterRepositoryTests
        {

            public List<string> inputEmployeeIds;

            public async Task<IEnumerable<PayrollRegisterEntry>> getActual()
            {
                return await repositoryUnderTest.GetPayrollRegisterByEmployeeIdsAsync(inputEmployeeIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayrollRegisterRepositoryTestsInitialize();
                testData.createPayControlRecords("BW");
                testData.createPayToDateRecord("12345", "BW");
                testData.createPayToDateRecord("54321", "BW");
                inputEmployeeIds = new List<string>() { "12345", "54321" };
            }

            [TestMethod]
            public async Task NoErrorsTest()
            {
                await getActual();
            }

            [TestMethod]
            public async Task SelectKeysWithEmployeeIdCriteriaTest()
            {
                var actual = await getActual();

                dataReaderMock.Verify(dr => dr.SelectAsync(It.IsAny<string>(), It.Is<string>(s => s == "WITH PTD.EMPLOYEE.ID EQ ? AND PTD.SEQ.NO NE \"\""), It.Is<string[]>(ids => ids.Count() == inputEmployeeIds.Count()), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()));
            }

            [TestMethod]
            public async Task NullEmployeeIdsReturnsEmptyListTest()
            {
                inputEmployeeIds = null;
                var actual = await getActual();
                Assert.IsTrue(!actual.Any());
            }

            [TestMethod]
            public async Task EmptyEmployeeIdsReturnsEmptyListTest()
            {
                inputEmployeeIds = new List<string>();
                var actual = await getActual();
                Assert.IsTrue(!actual.Any());
            }

            [TestMethod]
            public async Task PaytodatBulkRecordReadIsChunkedTest()
            {
                apiSettings.BulkReadSize = 1;
                repositoryUnderTest = new PayrollRegisterRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                testData.payToDateRecords.Add(testData.payToDateRecords[0]);

                await getActual();

                dataReaderMock.Verify(dr => dr.BulkReadRecordAsync<Paytodat>(It.IsAny<string[]>(), true), Times.Exactly(testData.payToDateRecords.Count));
            }

            [TestMethod]
            public async Task NoPaytodatRecordsSelectedResultsInEmptyRegisterTest()
            {
                testData.payToDateRecords = null;
                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task HandleNullResultOfBulkRecordReadTest()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Paytodat>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>((ids, b) => Task.FromResult<Collection<Paytodat>>(null));

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task InvalidPaytodatRecordIsNotAddedToRegisterTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.id = "foobar");

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task PayCycleIdTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.id = "12345*BW*0003914*1");
                testData.payControlRecords.ForEach(pc => pc.id = "12345*BW");

                var actual = await getActual();

                Assert.IsTrue(actual.All(p => p.PayCycleId == "BW"));
            }

            [TestMethod]
            public async Task InvalidSequenceNumberInPaytodatRecordIdTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.id = "12345*BW*0003914*foo");
                testData.payControlRecords.ForEach(pc => pc.id = "12345*BW");

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task InvalidEndDateInPaytodatRecordIdTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.id = "foo*BW*0003914*1");
                testData.payControlRecords.ForEach(pc => pc.id = "foo*BW");

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }   

            [TestMethod]
            public async Task InvalidStartDateInPayControlRecordTest()
            {
                testData.payControlRecords.ForEach(pc => pc.periodStartDate = null);

                var actual = await getActual();

                Assert.IsTrue(actual.Any()); //its actually valid for a pay control record to have no start date
            }

            [TestMethod]
            public async Task InvalidEarningsAmount_IsNotAddedToRegisterTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => earn.totalAmount = null));

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task RateForHourlyEarningsTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.hourlySalaryFlag = "H"; earn.rate = 5.00m; }));

                var actual = (await getActual()).SelectMany(reg => reg.EarningsEntries);

                Assert.IsTrue(actual.All(e => e.StandardRate == 5.00m));

            }

            [TestMethod]
            public async Task StipendForEarningsTest()
            {
                var hmmm = "stipendId";
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn =>  earn.stipendId = hmmm ));

                var actual = (await getActual()).SelectMany(reg => reg.EarningsEntries);


                Assert.IsTrue(actual.All(e => e.StipendId == hmmm));

            }

            [TestMethod]
            public async Task RateForSalaryEarningsMultiplierTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.hourlySalaryFlag = "S"; earn.rate = 5.00m; }));

                var actual = (await getActual()).SelectMany(reg => reg.EarningsEntries);

                Assert.IsTrue(actual.All(e => e.StandardRate == 500m));

            }

            [TestMethod]
            public async Task NullRateForHourlyTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.hourlySalaryFlag = "H"; earn.rate = null; }));

                var actual = (await getActual()).SelectMany(reg => reg.EarningsEntries);

                Assert.IsTrue(actual.All(e => !e.StandardRate.HasValue));
            }

            [TestMethod]
            public async Task NullRateForSalaryTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.hourlySalaryFlag = "S"; earn.rate = null; }));

                var actual = (await getActual()).SelectMany(reg => reg.EarningsEntries);

                Assert.IsTrue(actual.All(e => !e.StandardRate.HasValue));
            }

            [TestMethod]
            public async Task InvalidEarningsDifferntialAmount_IsNotAddedToRegisterTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.differentialId = "foo"; earn.diffAmount = null; earn.diffRate = 5m; }));

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task InvalidEarningsDifferntialRate_IsNotAddedToRegisterTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.differentialId = "foo"; earn.diffAmount = 500m; earn.diffRate = null; }));

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task DifferntialEarningsAreSetTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.earnings.ForEach(earn => { earn.stipendId = null; earn.differentialId = "foo"; earn.diffAmount = 500m; earn.diffRate = 5m; }));

                var actualEarnings = (await getActual()).SelectMany(r => r.EarningsEntries);

                Assert.IsTrue(actualEarnings.All(e => e.HasDifferentialEarnings));
            }

            [TestMethod]
            public async Task InvalidTax_IsNotAddedToRegisterTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.taxCode = null));

                var actual = await getActual();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task FixedAmountProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "F"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.FixedAmount));
            }

            [TestMethod]
            public async Task AdditionalTaxAmountProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "A"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxAmount));
            }

            [TestMethod]
            public async Task AdditionalTaxableAmountProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "T"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxableAmount));
            }

            [TestMethod]
            public async Task TaxExemptProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "E"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.TaxExempt));
            }

            [TestMethod]
            public async Task RegularProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "R"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.Regular));
            }

            [TestMethod]
            public async Task InactiveProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "D"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.Inactive));
            }

            [TestMethod]
            public async Task TaxableExemptProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "X"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.TaxableExempt));
            }

            [TestMethod]
            public async Task DefaultProcessingCodeTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.processingCode = "Z"));

                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.ProcessingCode == PayrollTaxProcessingCode.Regular));
            }

            [TestMethod]
            public async Task NullExemptionsTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.taxes.ForEach(t => t.exemptions = null));
                var actual = (await getActual()).SelectMany(p => p.TaxEntries);

                Assert.IsTrue(actual.All(t => t.Exemptions == 0));
            }

            [TestMethod]
            public async Task EmployerTaxAmountIsNullTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.expandedTaxes.Clear());

                var actualTaxes = (await getActual()).SelectMany(r => r.TaxEntries);

                Assert.IsTrue(actualTaxes.All(t => !t.EmployerTaxAmount.HasValue));
            }

            [TestMethod]
            public async Task EmployerTaxAmountIsSumOfEmployerTaxRecordsTest()
            {
                var id = testData.payToDateRecords[0].id;
                var taxId = testData.payToDateRecords[0].taxes[0].taxCode;

                var expectedAmount = testData.payToDateRecords[0].expandedTaxes.Where(t => t.expandedId.StartsWith(taxId)).Sum(t => t.employerTaxAmount);

                var actualTax = (await getActual()).First(r => r.Id == id).TaxEntries.First(t => t.TaxCode == taxId);

                Assert.AreEqual(expectedAmount, actualTax.EmployerTaxAmount);

            }

            //[TestMethod]
            //public async Task InvalidBended_IsNotAddedToRegisterTest()
            //{
            //    testData.payToDateRecords.ForEach(ptd => ptd.benefits.ForEach(b => b.benefitCode = null));

            //    var actual = await getActual();

            //    Assert.IsFalse(actual.Any());
            //}

            [TestMethod]
            public async Task EmployerBenefitAmountIsNullTest()
            {
                testData.payToDateRecords.ForEach(ptd => ptd.benefits.Clear());

                var actualBendeds = (await getActual()).SelectMany(r => r.BenefitDeductionEntries);

                Assert.IsTrue(actualBendeds.All(t => !t.EmployerAmount.HasValue));
            }

            [TestMethod]
            public async Task EmployerBenefitAmountIsSumOfEmployerBenefitRecordsTest()
            {
                var id = testData.payToDateRecords[0].id;
                var benId = testData.payToDateRecords[0].benefits[0].benefitCode;
              

                var expectedAmount = testData.payToDateRecords[0].expandedBenefits.Where(b => b.expandedId.StartsWith(benId)).Sum(t => t.employerAmount);

                var actualBenefits = (await getActual()).First(r => r.Id == id).BenefitDeductionEntries.First(b => b.BenefitDeductionId == benId);

                Assert.AreEqual(expectedAmount, actualBenefits.EmployerAmount);
            }
        }

        [TestClass]
        public class GetPayrollRegisterByEmployeeIdsTests : PayrollRegisterRepositoryTests
        {

            public List<string> inputEmployeeIds;

            public async Task<IEnumerable<PayrollRegisterEntry>> getActual()
            {
                return await repositoryUnderTest.GetPayrollRegisterByEmployeeIdsAsync(inputEmployeeIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayrollRegisterRepositoryTestsInitialize();
                
            }

            
        }
    }
}
