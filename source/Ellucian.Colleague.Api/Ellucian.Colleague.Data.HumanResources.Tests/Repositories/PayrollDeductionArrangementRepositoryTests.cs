using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayrollDeductionArrangementRepositoryTests : BaseRepositorySetup
    {
        private PayrollDeductionArrangementRepository repositoryUnderTest;
        private List<Perben> perbenList;
        private Collection<Perben> perbenCollection;
        private Collection<Perbencs> perbenCsCollection;
            
        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            TestDataSetup();
            repositoryUnderTest = new PayrollDeductionArrangementRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            #region
            //TestDataSetup();

            //dataReaderMock.Setup(repo => repo.SelectAsync("EMPLOYES", It.IsAny<string>())).ReturnsAsync(employeeIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Employes>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(employesCollDataList);

            //dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>(),
            //            It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(perposCollDataList);

            //dataReaderMock.Setup(repo => repo.SelectAsync("PERPOSWG", It.IsAny<string>(),
            //            It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(perposwgCollDataList);

            //dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>(),
            //            It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(perstatCollDataList);

            //dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", It.IsAny<string>(),
            //            It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(perbenCollDataList);

            //dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()))
            //    .ReturnsAsync(hrperCollDataList);

            //string fileName = "CORE.PARMS";
            //string field = "LDM.DEFAULTS";
            //LdmDefaults ldmDefaults = new LdmDefaults() { LdmdExcludeBenefits = excludeBenefits, LdmdLeaveStatusCodes = leaveStatuses };
            //dataReaderMock.Setup(repo => repo.ReadRecord<LdmDefaults>(fileName, field, It.IsAny<bool>())).Returns(ldmDefaults);
            #endregion
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetHostCountryParams()
        {
            Data.Base.DataContracts.IntlParams intlParams = new Base.DataContracts.IntlParams();
            intlParams.HostCountry = "USA";
            intlParams.HostDateDelimiter = "/";
            intlParams.HostShortDateFormat = "MDY";
            dataReaderMock.Setup(d => d.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
            var actual = await repositoryUnderTest.GetHostCountryAsync();
            var actual1 = await repositoryUnderTest.GetHostCountryAsync();
            Assert.IsNotNull(actual);
            Assert.AreEqual("USA", actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetIdFromGuidAsync()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = "1", SecondaryKey = "2" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await repositoryUnderTest.GetIdFromGuidAsync("100");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetHostCountryParams_Params_Null()
        {
            dataReaderMock.Setup(d => d.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(null);
            var actual = await repositoryUnderTest.GetHostCountryAsync();
            Assert.IsNotNull(actual);
            Assert.AreEqual("USA", actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync()
        {
            string[] primaryKeys = new[] { "1", "2" };
            string[] perbenKeys = new[] { "1", "2" };
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(primaryKeys);
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(perbenKeys);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetAsync(0, 2, true, "Filter2", "Filter3", "Filter4", "Filter5");
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Item1.Count());
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync_PerbenNull_LogError()
        {
            string[] primaryKeys = new[] { "1", "2" };
            string[] perbenKeys = new[] { "1", "2" };
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(primaryKeys);
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(perbenKeys);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetAsync(0, 2, true, "Filter2", "Filter3", "Filter4", "Filter5");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync_NoCriteria()
        {
            string[] primaryKeys = new[] { "1", "2" };
            string[] perbenKeys = new[] { "1", "2" };
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(primaryKeys);
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(perbenKeys);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetAsync(0, 2);
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Item1.Count());
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync_Status_Cancelled()
        {
            string[] primaryKeys = new[] { "1", "2" };
            string[] perbenKeys = new[] { "1", "2" };
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(primaryKeys);
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(perbenKeys);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetAsync(0, 2, status: "Cancelled");
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Item1.Count());
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_GetById()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = "1", SecondaryKey = "2" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection[0]);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetByIdAsync("1");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_CreateAsync()
        {
            Domain.HumanResources.Entities.PayrollDeductionArrangements entity = new Domain.HumanResources.Entities.PayrollDeductionArrangements("00000000-0000-0000-0000-000000000000", "123")
            {
                Id = "1",
                CommitmentContributionId = "CommitmentContributionId",
                CommitmentType = "CommitmentType",
                DeductionTypeCode = "DeductionTypeCode",
                Status = "Status",
                AmountPerPayment = 10,
                TotalAmount = 20,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(30),
                Interval = 10,
                MonthlyPayPeriods = new List<int?>() { 1, 2 },
                ChangeReason = "ChangeReason"
            };
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = "1", SecondaryKey = "2" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);
            // dataReaderMock.Setup(reader => reader.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
            transManagerMock.Setup(tr => tr.ExecuteAsync<CreateUpdatePerbenRequest, CreateUpdatePerbenResponse>(It.IsAny<CreateUpdatePerbenRequest>())).ReturnsAsync(new CreateUpdatePerbenResponse() { Guid = "1" });
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection[0]);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.CreateAsync(new Domain.HumanResources.Entities.PayrollDeductionArrangements("00000000-0000-0000-0000-000000000000", "123")); 
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementRepositoryTests_UpdateAsync()
        {
            Domain.HumanResources.Entities.PayrollDeductionArrangements entity = new Domain.HumanResources.Entities.PayrollDeductionArrangements("a64aa33f-a173-4518-a758-f733c7462ad4", "123")
            {
                Id = "1",
                CommitmentContributionId = "CommitmentContributionId",
                CommitmentType = "CommitmentType",
                DeductionTypeCode = "DeductionTypeCode",
                Status = "Status",
                AmountPerPayment = 10,
                TotalAmount = 20,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(30),
                Interval = 10,
                MonthlyPayPeriods = new List<int?>() {1, 2},
                ChangeReason = "ChangeReason"
            };
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = "1", SecondaryKey = "2" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);
            transManagerMock.Setup(tr => tr.ExecuteAsync<CreateUpdatePerbenRequest, CreateUpdatePerbenResponse>(It.IsAny<CreateUpdatePerbenRequest>())).ReturnsAsync(new CreateUpdatePerbenResponse() { Guid = "1" });
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection[0]);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.UpdateAsync("a64aa33f-a173-4518-a758-f733c7462ad4", entity);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetById_RepositoryException()
        {
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
            //dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetByIdAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetById_PerbenIntgIntervalNUll_PerbenIntgMonPayPeriodsNull()
        {
            perbenCollection = new Collection<Perben>() 
            {
                new Perben(){ 
                    PerbenBdId = "1",
                    PerbenHrpId = "1",
                    AllBenefitCosts = new List<string>(){"1", "2"}, 
                    RecordGuid = "1",
                    Recordkey = "1",
                    PerbenIntgContribution = "PerbenIntgContribution",
                    PerbenIntgCommitmentType = "",
                    PerbenCancelDate = DateTime.Now.AddDays(-2),
                    PerbenEnrollDate = DateTime.Now.AddDays(-5),
                    PerbenIntgInterval = null,
                    PerbenIntgMonPayPeriods = new List<int?>(){},
                    PerbenChangeReasons = new List<string>(){"PerbenChangeReasons"}
                },
                new Perben(){ PerbenBdId = "2", AllBenefitCosts = new List<string>(){"1", "2"}, PerbenHrpId = "2" }
            };
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection[0]);
            //dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.GetByIdAsync("1");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync_ArgumentNullException()
        {
            string[] primaryKeys = new[] { "1", "2" };
            string[] perbenKeys = new[] { "1", "2" };
            var perbenCollection = new Collection<Perben>() 
            {
                new Perben(){ PerbenBdId = "1", AllBenefitCosts = new List<string>(){"1", "2"} }
            };
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(primaryKeys);
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(perbenKeys);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);

            var actual = await repositoryUnderTest.GetAsync(0, 2, status: "Cancelled");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetAsync_RepositoryException()
        {
            dataReaderMock.Setup(reader => reader.SelectAsync("PERBEN", It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            var actual = await repositoryUnderTest.GetAsync(0, 2, status: "Cancelled");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetIdFromGuidAsync_Id_Null_ArgumentNullException()
        {            
            var actual = await repositoryUnderTest.GetIdFromGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetIdFromGuidAsync_Dictionary_Value_Null_KeyNotFoundException()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", null);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await repositoryUnderTest.GetIdFromGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayrollDeductionArrangementRepositoryTests_GetIdFromGuidAsync_Not_PERBEN_RepositoryException()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("PERBEN", new GuidLookupResult() { Entity = "PERBENN", PrimaryKey = "1", SecondaryKey = "2" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await repositoryUnderTest.GetIdFromGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayrollDeductionArrangementRepositoryTests_UpdateAsync_ArgumentNullException()
        {
            await repositoryUnderTest.UpdateAsync("", It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task PayrollDeductionArrangementRepositoryTests_CreateAsync_InvalidOperationException()
        {
            Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
            result.Add("PERBEN", new GuidLookupResult());
            dataReaderMock.Setup(reader => reader.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);
            var actual = await repositoryUnderTest.CreateAsync(new Domain.HumanResources.Entities.PayrollDeductionArrangements("a64aa33f-a173-4518-a758-f733c7462ad4", "123"));
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayrollDeductionArrangementRepositoryTests_UpdateAsync_RepositoryException()
        {
            Domain.HumanResources.Entities.PayrollDeductionArrangements entity = new Domain.HumanResources.Entities.PayrollDeductionArrangements("a64aa33f-a173-4518-a758-f733c7462ad4", "123")
            {
                Id = "1",
                CommitmentContributionId = "CommitmentContributionId",
                CommitmentType = "CommitmentType",
                DeductionTypeCode = "DeductionTypeCode",
                Status = "Status",
                AmountPerPayment = 10,
                TotalAmount = 20,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(30),
                Interval = 10,
                MonthlyPayPeriods = new List<int?>() { 1, 2 },
                ChangeReason = "ChangeReason"
            };
            transManagerMock.Setup(tr => tr.ExecuteAsync<CreateUpdatePerbenRequest, CreateUpdatePerbenResponse>(It.IsAny<CreateUpdatePerbenRequest>()))
                .ReturnsAsync(new CreateUpdatePerbenResponse() 
                { 
                    PerbenId = "1", 
                    ErrorOccurred = "ErrorOccurred", 
                    PerbenUpdateErrors = new List<PerbenUpdateErrors>() 
                    {
                        new PerbenUpdateErrors(){ ErrorCodes = "Errror1", ErrorMessages = "Error occured while updating"}
                    } 
                });
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<Perben>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(perbenCollection[0]);
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Perbencs>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(perbenCsCollection);

            var actual = await repositoryUnderTest.UpdateAsync("a64aa33f-a173-4518-a758-f733c7462ad4", entity);
        }

        public void TestDataSetup()
        {
            perbenCollection = new Collection<Perben>() 
            {
                new Perben(){ 
                    PerbenBdId = "1",
                    PerbenHrpId = "1",
                    AllBenefitCosts = new List<string>(){"1", "2"}, 
                    RecordGuid = "1",
                    Recordkey = "1",
                    PerbenIntgContribution = "PerbenIntgContribution",
                    PerbenIntgCommitmentType = "",
                    PerbenCancelDate = DateTime.Now.AddDays(-2),
                    PerbenEnrollDate = DateTime.Now.AddDays(-5),
                    PerbenIntgInterval = 1,
                    PerbenIntgMonPayPeriods = new List<int?>(){1},
                    PerbenChangeReasons = new List<string>(){"PerbenChangeReasons"}
                },
                new Perben(){ PerbenBdId = "2", AllBenefitCosts = new List<string>(){"1", "2"}, PerbenHrpId = "2" }
            };
            perbenCsCollection = new Collection<Perbencs>() 
            {
                new Perbencs()
                { 
                    PbcBdId = "1", 
                    PbcHrpId = "1", 
                    PbcEndDate = DateTime.Today,
                    PbcEmplyePayCost = 100,
                    PbcEmplyeLimitAmt = 200
                },
                new Perbencs(){ PbcBdId = "2", PbcHrpId = "2", PbcEndDate = DateTime.Today }
            };
        }
    }
}
