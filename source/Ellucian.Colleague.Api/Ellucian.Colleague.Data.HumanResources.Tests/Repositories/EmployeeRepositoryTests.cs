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
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System.Threading;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{

    [TestClass]
    public class EmployeeRepositoryTests : BaseRepositorySetup
    {
        private EmployeeRepository repositoryUnderTest;

        //test data objects
        private List<string> employeeIdList;
        private List<string> personIdList;
        private List<string> hrperIdList;
        private Collection<Employes> employesCollDataList;
        private Collection<Perpos> perposCollDataList;
        private Collection<Perposwg> perposwgCollDataList;
        private Collection<Perstat> perstatCollDataList;
        private Collection<Perben> perbenCollDataList;
        private Collection<Hrper> hrperCollDataList;
        private Collection<Hrper> hrperCollDataList2;
        private List<string> excludeBenefits = new List<string>() { "1EJR", "4JAN" };
        private List<string> leaveStatuses = new List<string>() { "PR", "SUSP" };
        IEnumerable<HrStatuses> allHrStatuses;
        ApplValcodes contractTypesValcodeResponse;
        string domainEntityNameName;

        Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        IHumanResourcesReferenceDataRepository hrReferenceDataRepository;
        HumanResourcesReferenceDataRepository hrReferenceDataRepo;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            allHrStatuses = new TestContractTypesReferenceDataRepository().GetHrStatusesAsync();
            contractTypesValcodeResponse = BuildValcodeResponse(allHrStatuses);
            var contractTypesValResponse = new List<string>() { "2" };
            contractTypesValcodeResponse.ValActionCode1 = contractTypesValResponse;

            hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            hrReferenceDataRepository = hrReferenceDataRepositoryMock.Object;

            hrReferenceDataRepo = BuildValidReferenceDataRepository();
            domainEntityNameName = hrReferenceDataRepo.BuildFullCacheKey("ST_HR.STATUSES_GUID");

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            repositoryUnderTest = new EmployeeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            TestDataSetup();

            dataReaderMock.Setup(repo => repo.SelectAsync("EMPLOYES", It.IsAny<string>())).ReturnsAsync(employeeIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("EMPLOYES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(employeeIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Hrper>(It.IsAny<GuidLookup>(), It.IsAny<bool>()))
                .ReturnsAsync(new Hrper() { RecordGuid = "guid", Recordkey = "key" });

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Employes>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(employesCollDataList));

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOSWG", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposwgCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perstatCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perbenCollDataList);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(hrperCollDataList);

            string fileName = "CORE.PARMS";
            string field = "LDM.DEFAULTS";
            LdmDefaults ldmDefaults = new LdmDefaults() { LdmdExcludeBenefits = excludeBenefits, LdmdLeaveStatusCodes = leaveStatuses };
            dataReaderMock.Setup(repo => repo.ReadRecord<LdmDefaults>(fileName, field, It.IsAny<bool>())).Returns(ldmDefaults);

        }


        [TestMethod]
        public async Task GetAllEmployees_V7()
        {
            var testData = await repositoryUnderTest.GetEmployeesAsync(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1");

            Assert.AreEqual(testData.Item2, 40);
        }

        [TestMethod]
        public async Task HasOnlineConsentTest()
        {
            var testData = await repositoryUnderTest.GetEmployeesAsync(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1");

            var employees = testData.Item1;

            Assert.IsTrue(employees.All(e => e.HasOnlineEarningsStatementConsent));
        }

        [TestMethod]
        public async Task NotHasOnlineConsentTest()
        {
            foreach (var item in employesCollDataList)
            {
                item.EmpViewChkAdvOnline = "N";
            }

            var testData = await repositoryUnderTest.GetEmployeesAsync(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1");

            var employees = testData.Item1;

            Assert.IsTrue(employees.All(e => !e.HasOnlineEarningsStatementConsent));
        }

        public async Task GetEmployeeKeysWhenActiveOnlyIsTrue()
        {
            var testData = await repositoryUnderTest.GetEmployeeKeysAsync(includeNonEmployees: false, activeOnly: true);


        }

        [TestMethod]
        public async Task GetNoEmployees_V7()
        {
            var hrperIdList2 = new List<string>();
            //dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList2.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(hrperIdList2.ToArray());
            var testData = await repositoryUnderTest.GetEmployeesAsync(0, 100, campus: Guid.NewGuid().ToString());

            Assert.AreEqual(testData.Item2, 0);
        }

        //[TestMethod]
        //public async Task GetAllEmployeesById_V7()
        //{
        //    var testData = await repositoryUnderTest.GetEmployeeByIdAsync("id");

        //    Assert.IsNotNull(testData);
        //}

        public void TestDataSetup()
        {
            employeeIdList = new List<string>()
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            hrperIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80",
                "91", "92", "93", "94", "95", "96", "97", "98", "99", "100", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110",
                "111", "112", "113", "114", "115", "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130",
                "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145", "146", "147", "148", "149", "150"
            };

            personIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            employesCollDataList = new Collection<Employes>();

            for (int i = 41; i < 81; i++)
            {
                employesCollDataList.Add(new Employes()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    RecordModelName = "employees",
                    Recordkey = i.ToString(),
                    EmpViewChkAdvOnline = "Y"
                });
            }

            for (int i = 41; i < 81; i++)
            {
                employesCollDataList.Add(new Employes()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    RecordModelName = "employees",
                    Recordkey = i.ToString(),
                    EmpViewChkAdvOnline = "Y",

                });
            }

            perposCollDataList = new Collection<Perpos>();

            for (int i = 41; i < 81; i++)
            {
                perposCollDataList.Add(new Perpos()
                {
                    PerposAltSupervisorId = i.ToString(),
                    PerposHrpId = i.ToString(),
                    PerposSupervisorHrpId = i.ToString(),
                    PerposPositionId = i.ToString(),
                    PerposStartDate = DateTime.Now.AddDays(-1),
                    PerposEndDate = DateTime.Now.AddYears(1),
                    Recordkey = i.ToString()
                });
            }

            perposwgCollDataList = new Collection<Perposwg>();

            for (int i = 41; i < 81; i++)
            {
                perposwgCollDataList.Add(new Perposwg()
                {
                    PpwgBaseEt = "et",
                    PpwgEndDate = DateTime.Now.AddYears(1),
                    PpwgStartDate = DateTime.Now.AddDays(-1),
                    PpwgHrpId = i.ToString(),
                    Recordkey = i.ToString()
                });
            }

            perstatCollDataList = new Collection<Perstat>();

            //for (int i = 41; i < 81; i++)
            for (int i = 41; i < 80; i++)
            {
                perstatCollDataList.Add(new Perstat()
                {
                    Recordkey = i.ToString(),
                    PerstatHrpId = i.ToString(),
                    PerstatEndDate = DateTime.Now.AddYears(1),
                    PerstatEndReason = "End Time",
                    PerstatPrimaryPerposId = i.ToString(),
                    PerstatPrimaryPosId = i.ToString(),
                    PerstatStartDate = DateTime.Now.AddDays(-1),
                    PerstatStatus = "FT"
                });
            }
            for (int i = 80; i < 81; i++)
            {
                perstatCollDataList.Add(new Perstat()
                {
                    Recordkey = i.ToString(),
                    PerstatHrpId = i.ToString(),
                    PerstatEndDate = DateTime.Now.AddYears(1),
                    PerstatEndReason = "End Time",
                    PerstatPrimaryPerposId = i.ToString(),
                    PerstatPrimaryPosId = i.ToString(),
                    PerstatStartDate = DateTime.Now.AddDays(-1),
                    PerstatStatus = "PR"
                });
            }

            perbenCollDataList = new Collection<Perben>();

            for (int i = 41; i < 81; i++)
            {
                perbenCollDataList.Add(new Perben()
                {
                    Recordkey = i.ToString(),
                    PerbenBdId = i.ToString(),
                    PerbenCancelDate = DateTime.Now,
                    PerbenHrpId = i.ToString()
                });
            }

            hrperCollDataList = new Collection<Hrper>();

            for (int i = 41; i < 81; i++)
            {
                hrperCollDataList.Add(new Hrper()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = i.ToString()
                });
            }

        }

        private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
        {

            // Setup response to contractTypes domainEntityName read
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>())).ReturnsAsync(contractTypesValcodeResponse);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var contractTypes = allHrStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "HR.VALCODES", "HR.STATUSES", contractTypes.Code }),
                        new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            hrReferenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return hrReferenceDataRepo;
        }

        private ApplValcodes BuildValcodeResponse(IEnumerable<HrStatuses> contractTypes)
        {
            ApplValcodes contractTypesResponse = new ApplValcodes();
            contractTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            foreach (var item in contractTypes)
            {
                contractTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "Y", item.Code, "3", "", ""));
            }
            return contractTypesResponse;
        }

    }

    #region GetEmployeeKeysAsync
    [TestClass]
    public class GetEmployeeKeysAsync : BaseRepositorySetup
    {

        EmployeeRepository mockRepository;

        List<string> activeList = new List<string> { "0000001", "0000003", "0000004", "0000005", "0000006" };

        private IList<Hrper> HrperCriteriaFilter(IDictionary<string, Hrper> keyedRecords, string[] requestedIds, string criteria)
        {
            //No Criteria
            if (string.IsNullOrEmpty(criteria))
            {
                // No Criteria and Filtered
                if (requestedIds != null && requestedIds.Length != 0)
                {
                    return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                }
                return keyedRecords.Select(kr => kr.Value).ToList();
            }
            else
            {
                //Active Only
                if (criteria == "WITH HRP.ACTIVE.STATUS NE ''")
                {
                    //Active only and Filtered
                    if (requestedIds != null && requestedIds.Length != 0)
                    {
                        return keyedRecords.Where(kr => activeList.Contains(kr.Key) && requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                    }
                    else
                    {
                        return keyedRecords.Where(kr => activeList.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                    }
                }

                return keyedRecords.Select(kr => kr.Value).ToList();
            }
        }

        private IList<Employes> EmployesCriteriaFilter(IDictionary<string, Employes> keyedRecords, string[] requestedIds, string criteria)
        {
            // No Criteria
            if (string.IsNullOrEmpty(criteria))
            {

                // No Criteria and Filtered
                if (requestedIds != null && requestedIds.Length != 0)
                {
                    return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                }

                return keyedRecords.Select(kr => kr.Value).ToList();
            }
            else
            {
                string active = "WITH EMP.ACTIVE.STATUS NE ''";
                string noCheck = "WITH EMP.VIEW.CHK.ADV.ONLINE NE 'Y'";
                string yesCheck = "WITH EMP.VIEW.CHK.ADV.ONLINE EQ 'Y'";
                // Active
                if (criteria.Contains(active))
                {
                    // Active and EmpViewChkAdvOnline = Y
                    if (criteria.Contains(yesCheck))
                    {
                        // Active, EmpViewChkAdvOnline = Y, Filtered
                        if (requestedIds != null && requestedIds.Length != 0)
                        {
                            return keyedRecords.Where(kr => activeList.Contains(kr.Key) && requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "Y").ToList();
                        }

                        return keyedRecords.Where(kr => activeList.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "Y").ToList();
                    }

                    // Active and EmpViewChkAdvOnline = N
                    else if (criteria.Contains(noCheck))
                    {
                        // Active, EmpViewChkAdvOnline = N, Filtered
                        if (requestedIds != null && requestedIds.Length != 0)
                        {
                            return keyedRecords.Where(kr => activeList.Contains(kr.Key) && requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "N").ToList();
                        }

                        return keyedRecords.Where(kr => activeList.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "N").ToList();
                    }

                    //Active and Filtered
                    if (requestedIds != null && requestedIds.Length != 0)
                    {
                        return keyedRecords.Where(kr => activeList.Contains(kr.Key) && requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                    }

                    return keyedRecords.Where(kr => activeList.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                }

                // EmpViewChkAdvOnline = Y
                else if (criteria.Contains(yesCheck))
                {
                    // EmpViewChkAdvOnline = Y and Filtered
                    if (requestedIds != null && requestedIds.Length != 0)
                    {
                        return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "Y").ToList();
                    }

                    return keyedRecords.Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "Y").ToList();
                }

                // EmpViewChkAdvOnline = N
                else if (criteria.Contains(noCheck))
                {
                    // EmpViewChkAdvOnline = N and Filtered
                    if (requestedIds != null && requestedIds.Length != 0)
                    {
                        return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "N").ToList();
                    }

                    return keyedRecords.Select(kr => kr.Value).Where(v => v.EmpViewChkAdvOnline == "N").ToList();
                }

                return keyedRecords.Select(kr => kr.Value).ToList();

            }
        }

        [TestInitialize]
        public void Initialize()
        {

            MockInitialize();
            var hrperRecords = (new List<Hrper>()
            {
                new Hrper()
                {
                    //Active Record, Employee
                    Recordkey = "0000001",
                    RecordGuid = GenerateGuid()
                },
                new Hrper()
                {
                    //Inactive Record, Employee
                    Recordkey = "0000002",
                    RecordGuid = GenerateGuid()
                },
                new Hrper()
                {
                    //Active Record, Employee
                    Recordkey = "0000003",
                    RecordGuid = GenerateGuid()
                },
                new Hrper()
                {
                    //Active Record, Employee
                    Recordkey = "0000004",
                    RecordGuid = GenerateGuid()
                },
                new Hrper()
                {
                    //Active Record, Employee
                    Recordkey = "0000005",
                    RecordGuid = GenerateGuid()
                },
                new Hrper()
                {
                    //Active Record, Non-employee
                    Recordkey = "0000006",
                    RecordGuid = GenerateGuid()
                }

            });

            var employesRecords = (new List<Employes>()
            {
                new Employes()
                {
                    //Active Record, Employee, Online Consent Given
                    Recordkey = "0000001",
                    RecordGuid = GenerateGuid(),
                    EmpViewChkAdvOnline = "Y"
                },
                new Employes()
                {
                    //Inactive Record, Employee, Online Consent Given
                    Recordkey = "0000002",
                    RecordGuid = GenerateGuid(),
                    EmpViewChkAdvOnline = "Y"
                },
                new Employes()
                {
                    //Active Record, Employee, Online Consent Not Given
                    Recordkey = "0000003",
                    RecordGuid = GenerateGuid(),
                    EmpViewChkAdvOnline = "N"
                },
                new Employes()
                {
                    //Active Record, Employee, Online Consent Not Given
                    Recordkey = "0000004",
                    RecordGuid = GenerateGuid(),
                    EmpViewChkAdvOnline = "N"
                },
                new Employes()
                {
                    //Active Record, Employee, Online Consent Not Given
                    Recordkey = "0000005",
                    RecordGuid = GenerateGuid(),
                    EmpViewChkAdvOnline = "Y"
                }

            });

            MockRecordsAsync("HRPER", hrperRecords.ToDictionary(rr => rr.Recordkey), HrperCriteriaFilter);
            MockRecordsAsync("EMPLOYES", employesRecords.ToDictionary(rr => rr.Recordkey), EmployesCriteriaFilter);
            mockRepository = new EmployeeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesAndNonEmployees()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(includeNonEmployees: true);
            Assert.AreEqual(6, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_SomeEmployeesAndNonEmployees()
        {
            var ids = new List<string>() {"0000001", "0000003", "0000004", "0000005"};
            var result = await mockRepository.GetEmployeeKeysAsync(includeNonEmployees: true, ids: ids);
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesAndNonEmployees()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(includeNonEmployees: true, activeOnly: true);
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_SomeActiveEmployeesAndNonEmployees()
        {
            var ids = new List<string>() { "0000003" };
            var result = await mockRepository.GetEmployeeKeysAsync(includeNonEmployees: true, ids: ids);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesOnly()
        {
            var result = await mockRepository.GetEmployeeKeysAsync();
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_SomeEmployeesOnly()
        {
            var ids = new List<string>() { "0000001", "0000003" };
            var result = await mockRepository.GetEmployeeKeysAsync(ids: ids);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesOnly()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(activeOnly: true);
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetEmployeeKeysAsync_EmployeesAndNonEmployeesOnlineConsentGiven_ThrowsArgumentException()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(hasOnlineEarningsStatementConsentFilter: true, includeNonEmployees: true);
        }


        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesOnlineConsentGiven()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(hasOnlineEarningsStatementConsentFilter: true);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesOnlineConsentGivenFiltered()
        {
            var ids = new List<string>() { "0000001" };
            var result = await mockRepository.GetEmployeeKeysAsync(hasOnlineEarningsStatementConsentFilter: true, ids: ids);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesOnlineConsentGiven()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(activeOnly: true, hasOnlineEarningsStatementConsentFilter: true);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesOnlineConsentGivenFiltered()
        {
            var ids = new List<string>() { "0000001" };
            var result = await mockRepository.GetEmployeeKeysAsync(activeOnly: true, hasOnlineEarningsStatementConsentFilter: true, ids: ids);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesOnlineConsentNotGiven()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(hasOnlineEarningsStatementConsentFilter: false);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_EmployeesOnlineConsentNotGivenFiltered()
        {
            var ids = new List<string>() { "0000004" };
            var result = await mockRepository.GetEmployeeKeysAsync(hasOnlineEarningsStatementConsentFilter: false, ids: ids);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesOnlineConsentNotGiven()
        {
            var result = await mockRepository.GetEmployeeKeysAsync(activeOnly: false, hasOnlineEarningsStatementConsentFilter: false);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetEmployeeKeysAsync_ActiveEmployeesOnlineConsentNotGivenFiltered()
        {
            var ids = new List<string>() { "0000004" };
            var result = await mockRepository.GetEmployeeKeysAsync(activeOnly: false, hasOnlineEarningsStatementConsentFilter: false, ids: ids);
            Assert.AreEqual(1, result.Count());
        }
    }

    #endregion GetEmployeeKeysAsync

    [TestClass]
    public class EmployeeRepositoryTests_V11
    {
        [TestClass]
        public class EmployeeRepositoryTests_POST_AND_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private EmployeeRepository employeeRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private Employee employee;

            private UpdateEmployeeResponse response;

            private ApplValcodes hrStatuses;

            private Hrper hrper;

            private string[] keys;

            private Collection<Perstat> perStats;

            private Collection<Perpos> perPos;

            private Collection<Perposwg> perposwgs;

            private Collection<Perben> perbens;

            private LdmDefaults ldmDefaults;

            private Dictionary<string, RecordKeyLookupResult> guidList;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                employeeRepository = new EmployeeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                ldmDefaults = new LdmDefaults() { Recordkey = "1", LdmdLeaveStatusCodes = new List<string>() { "1" } };

                perbens = new Collection<Perben>()
                {
                    new Perben() { Recordkey = "1", RecordGuid = guid }
                };

                perposwgs = new Collection<Perposwg>()
                {
                    new Perposwg() { Recordkey = "1" }
                };

                perPos = new Collection<Perpos>()
                {
                    new Perpos() { Recordkey = "1", RecordGuid = guid }
                };

                perStats = new Collection<Perstat>()
                {
                    new Perstat() { Recordkey = "1", PerstatHrpId = guid, PerstatStatus = "1" }
                };

                keys = new string[] { "1", "2", "3" };

                guidList = new Dictionary<string, RecordKeyLookupResult>()
                {
                    { "HR.VALCODES+HR.STATUSES+1", new RecordKeyLookupResult(){ Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e871" } },
                    { "HR.VALCODES+HR.STATUSES+2", new RecordKeyLookupResult(){ Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e872" } },
                    { "HR.VALCODES+HR.STATUSES+3", new RecordKeyLookupResult(){ Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e873" } }
                };

                hrStatuses = new ApplValcodes()
                {
                    Recordkey = "HR.STATUSES",
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals("", "", "AC1", "IC1", "", "", ""){ ValInternalCodeAssocMember = "1", ValActionCode1AssocMember = "Y" },
                        new ApplValcodesVals("", "", "AC2", "IC2", "", "", ""){ ValInternalCodeAssocMember = "2", ValActionCode1AssocMember = "Y" },
                        new ApplValcodesVals("", "", "AC3", "IC3", "", "", ""){ ValInternalCodeAssocMember = "3", ValActionCode1AssocMember = "Y" }
                    }
                };

                hrper = new Hrper() { RecordGuid = guid, Recordkey = "1", HrpEffectTermDate = DateTime.Today };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "HRPER", PrimaryKey = "1" } } };

                response = new UpdateEmployeeResponse() { EmployeeGuid = guid, EmployeeId = "1" };

                employee = new Employee(guid, "1")
                {
                    StatusCode = "1",
                    Location = "1",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(100),
                    RehireEligibilityCode = "1",
                    StatusEndReasonCode = "1",
                    BenefitsStatus = BenefitsStatus.WithBenefits,
                    PayStatus = PayStatus.WithPay,
                    PayPeriodHours = new List<decimal?>() { 2 }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", true)).ReturnsAsync(hrStatuses);
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(guidList);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Hrper>(It.IsAny<GuidLookup>(), true)).ReturnsAsync(hrper);
                dataReaderMock.Setup(r => r.SelectAsync("PERSTAT", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), true)).ReturnsAsync(perStats);
                dataReaderMock.Setup(r => r.SelectAsync("PERPOS", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), true)).ReturnsAsync(perPos);
                dataReaderMock.Setup(r => r.SelectAsync("PERPOSWG", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true)).ReturnsAsync(perposwgs);
                dataReaderMock.Setup(r => r.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), true)).ReturnsAsync(perbens);
                dataReaderMock.Setup(r => r.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true)).Returns(ldmDefaults);

                transManagerMock.Setup(t => t.ExecuteAsync<UpdateEmployeeRequest, UpdateEmployeeResponse>(It.IsAny<UpdateEmployeeRequest>())).ReturnsAsync(response);

            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeRepo_CreateEmployeeAsync_Entity_Null()
            {
                await employeeRepository.CreateEmployee2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmployeeRepo_CreateEmployeeAsync_RepositoryException()
            {
                response.UpdateEmployeeErrors = new List<UpdateEmployeeErrors>() { new UpdateEmployeeErrors() { ErrorCodes = "ERROR", ErrorMessages = "MESSAGE" } };

                await employeeRepository.CreateEmployee2Async(employee);
            }

            [TestMethod]
            public async Task EmployeeRepo_CreateEmployeeAsync()
            {
                var result = await employeeRepository.CreateEmployee2Async(employee);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeRepo_UpdateEmployeeAsync_Entity_Null()
            {
                await employeeRepository.UpdateEmployee2Async(null);
            }

            [TestMethod]
            public async Task EmployeeRepo_UpdateEmployeeAsync_Create_Employee_With_PUT_Request()
            {
                dicResult[guid].PrimaryKey = null;
                var result = await employeeRepository.UpdateEmployee2Async(employee);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmployeeRepo_UpdateEmployeeAsync_RepositoryException()
            {
                response.UpdateEmployeeErrors = new List<UpdateEmployeeErrors>() { new UpdateEmployeeErrors() { ErrorCodes = "ERROR", ErrorMessages = "MESSAGE" } };
                await employeeRepository.UpdateEmployee2Async(employee);
            }

            [TestMethod]
            public async Task EmployeeRepo_UpdateEmployeeAsync()
            {
                employee.PayPeriodHours = null;

                var result = await employeeRepository.UpdateEmployee2Async(employee);

                Assert.IsNotNull(result);
            }
        }
    }

    [TestClass]
    public class EmployeeRepositoryTests_V12 : BaseRepositorySetup
    {
        private EmployeeRepository repositoryUnderTest;

        //test data objects
        private List<string> employeeIdList;
        private List<string> personIdList;
        private List<string> hrperIdList;
        private Collection<Employes> employesCollDataList;
        private Collection<Perpos> perposCollDataList;
        private Collection<Perposwg> perposwgCollDataList;
        private Collection<Perstat> perstatCollDataList;
        private Collection<Perben> perbenCollDataList;
        private Collection<Hrper> hrperCollDataList;
        private Collection<Hrper> hrperCollDataList2;
        private List<string> excludeBenefits = new List<string>() { "1EJR", "4JAN" };
        private List<string> leaveStatuses = new List<string>() { "PR", "SUSP" };
        IEnumerable<HrStatuses> allHrStatuses;
        ApplValcodes contractTypesValcodeResponse;
        string domainEntityNameName;

        Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        IHumanResourcesReferenceDataRepository hrReferenceDataRepository;
        HumanResourcesReferenceDataRepository hrReferenceDataRepo;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            allHrStatuses = new TestContractTypesReferenceDataRepository().GetHrStatusesAsync();
            contractTypesValcodeResponse = BuildValcodeResponse(allHrStatuses);
            var contractTypesValResponse = new List<string>() { "2" };
            contractTypesValcodeResponse.ValActionCode1 = contractTypesValResponse;

            hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            hrReferenceDataRepository = hrReferenceDataRepositoryMock.Object;

            hrReferenceDataRepo = BuildValidReferenceDataRepository();
            domainEntityNameName = hrReferenceDataRepo.BuildFullCacheKey("ST_HR.STATUSES_GUID");

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            repositoryUnderTest = new EmployeeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            TestDataSetup();

            dataReaderMock.Setup(repo => repo.SelectAsync("EMPLOYES", It.IsAny<string>())).ReturnsAsync(employeeIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("EMPLOYES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(employeeIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Hrper>(It.IsAny<GuidLookup>(), It.IsAny<bool>()))
                .ReturnsAsync(new Hrper() { RecordGuid = "guid", Recordkey = "key" });

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Employes>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new Employes() { RecordGuid = "guid", Recordkey = "key" });

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Employes>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(employesCollDataList));

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOSWG", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposwgCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perstatCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perbenCollDataList);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(hrperCollDataList);

            string fileName = "CORE.PARMS";
            string field = "LDM.DEFAULTS";
            LdmDefaults ldmDefaults = new LdmDefaults() { LdmdExcludeBenefits = excludeBenefits, LdmdLeaveStatusCodes = leaveStatuses };
            dataReaderMock.Setup(repo => repo.ReadRecord<LdmDefaults>(fileName, field, It.IsAny<bool>())).Returns(ldmDefaults);
        }

        [TestMethod]
        public async Task GetAllEmployees_V12()
        {
            List<string> contractTypes = new List<string>() { "FT", "PT" };
            var testData = await repositoryUnderTest.GetEmployees2Async(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1", contractTypeCodes: contractTypes , contractDetailTypeCode: "PT");

            Assert.AreEqual(testData.Item2, 40);
        }

        [TestMethod]
        public async Task HasOnlineConsentTest()
        {
            var testData = await repositoryUnderTest.GetEmployees2Async(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1");

            var employees = testData.Item1;

            Assert.IsTrue(employees.All(e => e.HasOnlineEarningsStatementConsent));
        }

        [TestMethod]
        public async Task NotHasOnlineConsentTest()
        {
            foreach (var item in employesCollDataList)
            {
                item.EmpViewChkAdvOnline = "N";
            }

            var testData = await repositoryUnderTest.GetEmployees2Async(0, 100, person: Guid.NewGuid().ToString(), campus: Guid.NewGuid().ToString(), status: "leave", startOn: DateTime.Now.ToString(), endOn: DateTime.Now.ToString(), rehireableStatusEligibility: "eligible", rehireableStatusType: "1");

            var employees = testData.Item1;

            Assert.IsTrue(employees.All(e => !e.HasOnlineEarningsStatementConsent));
        }

        [TestMethod]
        public async Task GetNoEmployees_V12()
        {
            var hrperIdList2 = new List<string>();
            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList2.ToArray());
            var testData = await repositoryUnderTest.GetEmployees2Async(0, 100, campus: Guid.NewGuid().ToString());

            Assert.AreEqual(testData.Item2, 0);
        }

        //[TestMethod]
        //public async Task GetAllEmployeesById_V12()
        //{
        //    var testData = await repositoryUnderTest.GetEmployee2ByGuidAsync("id");

        //    Assert.IsNotNull(testData);
        //}

        public void TestDataSetup()
        {
            employeeIdList = new List<string>()
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            hrperIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80",
                "91", "92", "93", "94", "95", "96", "97", "98", "99", "100", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110",
                "111", "112", "113", "114", "115", "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130",
                "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145", "146", "147", "148", "149", "150"
            };

            personIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            employesCollDataList = new Collection<Employes>();

            for (int i = 41; i < 81; i++)
            {
                employesCollDataList.Add(new Employes()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    RecordModelName = "employees",
                    Recordkey = i.ToString(),
                    EmpViewChkAdvOnline = "Y"
                });
            }

            perposCollDataList = new Collection<Perpos>();

            for (int i = 41; i < 81; i++)
            {
                perposCollDataList.Add(new Perpos()
                {
                    PerposAltSupervisorId = i.ToString(),
                    PerposHrpId = i.ToString(),
                    PerposSupervisorHrpId = i.ToString(),
                    PerposPositionId = i.ToString(),
                    PerposStartDate = DateTime.Now.AddDays(-1),
                    PerposEndDate = DateTime.Now.AddYears(1),
                    Recordkey = i.ToString()
                });
            }

            perposwgCollDataList = new Collection<Perposwg>();

            for (int i = 41; i < 81; i++)
            {
                perposwgCollDataList.Add(new Perposwg()
                {
                    PpwgBaseEt = "et",
                    PpwgEndDate = DateTime.Now.AddYears(1),
                    PpwgStartDate = DateTime.Now.AddDays(-1),
                    PpwgHrpId = i.ToString(),
                    Recordkey = i.ToString()
                });
            }

            perstatCollDataList = new Collection<Perstat>();

            //for (int i = 41; i < 81; i++)
            for (int i = 41; i < 80; i++)
            {
                perstatCollDataList.Add(new Perstat()
                {
                    Recordkey = i.ToString(),
                    PerstatHrpId = i.ToString(),
                    PerstatEndDate = DateTime.Now.AddYears(1),
                    PerstatEndReason = "End Time",
                    PerstatPrimaryPerposId = i.ToString(),
                    PerstatPrimaryPosId = i.ToString(),
                    PerstatStartDate = DateTime.Now.AddDays(-1),
                    PerstatStatus = "FT"
                });
            }
            for (int i = 80; i < 81; i++)
            {
                perstatCollDataList.Add(new Perstat()
                {
                    Recordkey = i.ToString(),
                    PerstatHrpId = i.ToString(),
                    PerstatEndDate = DateTime.Now.AddYears(1),
                    PerstatEndReason = "End Time",
                    PerstatPrimaryPerposId = i.ToString(),
                    PerstatPrimaryPosId = i.ToString(),
                    PerstatStartDate = DateTime.Now.AddDays(-1),
                    PerstatStatus = "PR"
                });
            }

            perbenCollDataList = new Collection<Perben>();

            for (int i = 41; i < 81; i++)
            {
                perbenCollDataList.Add(new Perben()
                {
                    Recordkey = i.ToString(),
                    PerbenBdId = i.ToString(),
                    PerbenCancelDate = DateTime.Now,
                    PerbenHrpId = i.ToString()
                });
            }

            hrperCollDataList = new Collection<Hrper>();

            for (int i = 41; i < 81; i++)
            {
                hrperCollDataList.Add(new Hrper()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    Recordkey = i.ToString()
                });
            }

        }

        private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
        {

            // Setup response to contractTypes domainEntityName read
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>())).ReturnsAsync(contractTypesValcodeResponse);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var contractTypes = allHrStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "HR.VALCODES", "HR.STATUSES", contractTypes.Code }),
                        new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            hrReferenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return hrReferenceDataRepo;
        }

        private ApplValcodes BuildValcodeResponse(IEnumerable<HrStatuses> contractTypes)
        {
            ApplValcodes contractTypesResponse = new ApplValcodes();
            contractTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            foreach (var item in contractTypes)
            {
                contractTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "Y", item.Code, "3", "", ""));
            }
            return contractTypesResponse;
        }

    }

}
