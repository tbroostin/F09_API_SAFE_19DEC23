// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Text;
using slf4net;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class StaffRepositoryTests : BaseRepositorySetup
    {
        protected StaffRepository staffRepository;
        protected TestStaffRepository expectedRepository;
        protected Staff staffDataContract;

        [TestClass]
        public class GetSingleStaffTests : StaffRepositoryTests
        {
            private string staffId;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestStaffRepository();
                staffRepository = BuildMockStaffRepository();
                staffId = "0000001";
            }

            [TestCleanup]
            public void TestCleanup()
            {
                staffRepository = null;
                expectedRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoStaffIdPassed_ExceptionThrownTest()
            {
                staffRepository.Get("");
            }

            [TestMethod]
            public void CurrentStaff_IsActiveSetToTrue()
            {
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = staffRepository.Get(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(true, staff.IsActive);
            }

            [TestMethod]
            public void NonCurrentStaff_IsActiveSetToFalse()
            {
                //Staff response -- Type = S, Status = F (Not Active)
                staffId = "0000002";
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = staffRepository.Get(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NotStaffType_ThrowsError()
            {
                staffId = "0000003";
                var staff = staffRepository.Get(staffId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void EmptyStaffTypesValidationTable_ThrowsException()
            {
                // mock data accessor - empty valcode response
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).Returns(new ApplValcodes());
                var staff = staffRepository.Get(staffId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NullStaffTypesValidationTableResponse_ThrowsException()
            {
                // mock data accessor - null response
                var nullValTable = new ApplValcodes();
                nullValTable = null;
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).Returns(nullValTable);
                var staff = staffRepository.Get(staffId);
            }

            [TestMethod]
            public void ErrorRetrievingStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - thrown error response
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Throws(new SystemException());
                var staff = staffRepository.Get(staffId);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            public void NullStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - null response
                var nullValTable = new ApplValcodes();
                nullValTable = null;
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Returns(nullValTable);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = staffRepository.Get(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            public void EmptyStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - empty valcode
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Returns(new ApplValcodes());
                var staff = staffRepository.Get(staffId);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

        }

        [TestClass]
        public class GetAsyncSingleStaffTests : StaffRepositoryTests
        {
            private string staffId;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestStaffRepository();
                staffRepository = BuildMockStaffRepository();
                staffId = "0000001";
            }

            [TestCleanup]
            public void TestCleanup()
            {
                staffRepository = null;
                expectedRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NoStaffIdPassed_ExceptionThrownTest()
            {
                await staffRepository.GetAsync("");
            }

            [TestMethod]
            public async Task CurrentStaff_IsActiveSetToTrue()
            {
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = await staffRepository.GetAsync(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(true, staff.IsActive);
            }

            [TestMethod]
            public async Task NonCurrentStaff_IsActiveSetToFalse()
            {
                //Staff response -- Type = S, Status = F (Not Active)
                staffId = "0000002";
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = await staffRepository.GetAsync(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NotStaffType_ThrowsError()
            {
                staffId = "0000003";
                var staff = await staffRepository.GetAsync(staffId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmptyStaffTypesValidationTable_ThrowsException()
            {
                // mock data accessor - empty valcode response
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).Returns(new ApplValcodes());
                var staff = await staffRepository.GetAsync(staffId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullStaffTypesValidationTableResponse_ThrowsException()
            {
                // mock data accessor - null response
                var nullValTable = new ApplValcodes();
                nullValTable = null;
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).Returns(nullValTable);
                var staff = await staffRepository.GetAsync(staffId);
            }

            [TestMethod]
            public async Task ErrorRetrievingStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - thrown error response
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Throws(new SystemException());
                var staff = await staffRepository.GetAsync(staffId);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            public async Task NullStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - null response
                var nullValTable = new ApplValcodes();
                nullValTable = null;
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Returns(nullValTable);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                var staff = await staffRepository.GetAsync(staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

            [TestMethod]
            public async Task EmptyStaffStatusesValidationTable_CreatesStaffWithInactiveStatus()
            {
                // mock data accessor - empty valcode
                dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Returns(new ApplValcodes());
                var staff = await staffRepository.GetAsync(staffId);
                var person = expectedRepository.personData.First(p => p.recordKey == staffId);
                Assert.AreEqual(person.lastName, staff.LastName);
                Assert.AreEqual(false, staff.IsActive);
            }

        }

        [TestClass]
        public class GetMultipleStaffTests : StaffRepositoryTests
        {
            private List<string> staffIds;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestStaffRepository();
                staffIds = new List<string>() { "0000001", "0000002", "0000003" };
                //Set all staff members to be active initially
                foreach (var staff in expectedRepository.staffData)
                {
                    staff.status = "C";
                    staff.type = "S";
                }
                staffRepository = BuildMockStaffRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                staffRepository = null;
                expectedRepository = null;
                staffIds = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoIdsPassed_ArgumentNullExceptionIsThrownTest()
            {
                staffRepository.Get(new List<string>());
            }

            [TestMethod]
            public void NumberOfEntitiesReturned_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedRepository.staffData.Count, staffRepository.Get(staffIds).Count());
            }

            [TestMethod]
            public void DuplicateIds_OnlyDistinctRecordsReturnedTest()
            {
                staffIds.AddRange(new List<string>() { "0000001", "0000002", "0000003" });
                var staffCount = staffRepository.Get(staffIds).Count();
                Assert.AreNotEqual(staffIds.Count, staffCount);
                Assert.AreEqual(expectedRepository.staffData.Count, staffCount);
            }

            [TestMethod]
            public void NoStaffDataContractsReturned_ErrorIsLoggedTest()
            {
                dataReaderMock.Setup<IEnumerable<DataContracts.Staff>>(a => a.BulkReadRecord<DataContracts.Staff>(It.IsAny<string[]>(), true)).Returns(new Collection<Staff>());
                staffRepository.Get(staffIds);
                loggerMock.Verify(l => l.Info("Unable to get Staff information for specified id(s)"));
            }

            [TestMethod]
            public void WrongIdPassed_StaffListReturnedNoExceptionTest()
            {
                staffIds.Add("0000000");
                expectedRepository.personData.Add(new TestStaffRepository.PersonRecord() { recordKey = "0000000", lastName = "lastName", firstName = "firstName" });
                BuildMockStaffRepository();
                var actualStaff = staffRepository.Get(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }

            [TestMethod]
            public void EmptyIdPassed_StaffListReturnedNoExceptionsTest()
            {
                staffIds.Add("");
                BuildMockStaffRepository();
                var actualStaff = staffRepository.Get(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }

            [TestMethod]
            public void NullIdPassed_StaffListReturnedNoExceptionsTest()
            {
                staffIds.Add(null);
                BuildMockStaffRepository();
                var actualStaff = staffRepository.Get(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }
        }

        [TestClass]
        public class GetAsyncMultipleStaffTests : StaffRepositoryTests
        {
            private List<string> staffIds;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestStaffRepository();
                staffIds = new List<string>() { "0000001", "0000002", "0000003" };
                //Set all staff members to be active initially
                foreach (var staff in expectedRepository.staffData)
                {
                    staff.status = "C";
                    staff.type = "S";
                }
                staffRepository = BuildMockStaffRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                staffRepository = null;
                expectedRepository = null;
                staffIds = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NoIdsPassed_ArgumentNullExceptionIsThrownTest()
            {
                await staffRepository.GetAsync(new List<string>());
            }

            [TestMethod]
            public async Task NumberOfEntitiesReturned_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedRepository.staffData.Count, (await staffRepository.GetAsync(staffIds)).Count());
            }

            [TestMethod]
            public async Task DuplicateIds_OnlyDistinctRecordsReturnedTest()
            {
                staffIds.AddRange(new List<string>() { "0000001", "0000002", "0000003" });
                var staffCount = (await staffRepository.GetAsync(staffIds)).Count();
                Assert.AreNotEqual(staffIds.Count, staffCount);
                Assert.AreEqual(expectedRepository.staffData.Count, staffCount);
            }

            [TestMethod]
            public async Task NoStaffDataContractsReturned_ErrorIsLoggedTest()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.Staff>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Staff>());
                await staffRepository.GetAsync(staffIds);
                loggerMock.Verify(l => l.Info("Unable to get Staff information for specified id(s)"));
            }

            [TestMethod]
            public async Task WrongIdPassed_StaffListReturnedNoExceptionTest()
            {
                staffIds.Add("0000000");
                expectedRepository.personData.Add(new TestStaffRepository.PersonRecord() { recordKey = "0000000", lastName = "lastName", firstName = "firstName" });
                BuildMockStaffRepository();
                var actualStaff = await staffRepository.GetAsync(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }

            [TestMethod]
            public async Task EmptyIdPassed_StaffListReturnedNoExceptionsTest()
            {
                staffIds.Add("");
                BuildMockStaffRepository();
                var actualStaff = await staffRepository.GetAsync(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }

            [TestMethod]
            public async Task NullIdPassed_StaffListReturnedNoExceptionsTest()
            {
                staffIds.Add(null);
                BuildMockStaffRepository();
                var actualStaff = await staffRepository.GetAsync(staffIds);
                Assert.IsTrue(actualStaff.Count() > 0);
                Assert.AreEqual(expectedRepository.staffData.Count, actualStaff.Count());
            }
        }

        [TestClass]
        public class GetStaffLoginIdForPerson : StaffRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestStaffRepository();
                
                staffRepository = BuildMockStaffRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                staffRepository = null;
                expectedRepository = null;
                staffDataContract = null;
            }

            [TestMethod]
            public async Task GetStaffLoginIdForPerson_Success()
            {
                staffDataContract = new Staff()
                {
                    StaffLoginId = "TEST"
                };

                var actual = await staffRepository.GetStaffLoginIdForPersonAsync("0000001");

                Assert.AreEqual(staffDataContract.StaffLoginId, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetStaffLoginIdForPerson_NullStaffRecord()
            {
                staffDataContract = null;

                var actual = await staffRepository.GetStaffLoginIdForPersonAsync("0000001");
            }
        }

        private StaffRepository BuildMockStaffRepository()
        {
            dataReaderMock.Setup<IEnumerable<Staff>>(a => a.BulkReadRecord<Staff>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    (expectedRepository.staffData == null) ? null :
                    new Collection<DataContracts.Staff>(
                        expectedRepository.staffData
                        .Where(i => ids.Contains(i.recordKey))
                        .Select(i =>
                        new DataContracts.Staff()
                        {
                            Recordkey = i.recordKey,
                            StaffStatus = i.status,
                            StaffType = i.type
                        }).ToList()));

            dataReaderMock.Setup(a => a.BulkReadRecordAsync<Staff>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult((expectedRepository.staffData == null) ? null :
                    new Collection<DataContracts.Staff>(
                        expectedRepository.staffData
                        .Where(i => ids.Contains(i.recordKey))
                        .Select(i =>
                        new DataContracts.Staff()
                        {
                            Recordkey = i.recordKey,
                            StaffStatus = i.status,
                            StaffType = i.type
                        }).ToList())
                     ));

            dataReaderMock.Setup<Task<Collection<Person>>>(accessor => accessor.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true))
                 .Returns<string[], bool>((ids, b) =>
                     {
                         return Task.FromResult(
                             new Collection<DataContracts.Person>(
                                 expectedRepository.personData
                             .Where(p => ids.Contains(p.recordKey))
                             .Select(p =>
                             new Person()
                             {
                                 Recordkey = p.recordKey,
                                 LastName = p.lastName,
                                 FirstName = p.firstName
                             }).ToList()));
                     });

            staffDataContract = new Staff();

            dataReaderMock.Setup(a => a.ReadRecordAsync<Staff>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(() =>
                {
                    return Task.FromResult(staffDataContract);
                });


            // mock data access for preferred address - all will return null...
            TxGetHierarchyAddressResponse txAddressResponse = new TxGetHierarchyAddressResponse() { IoPersonId = "0000001" };
            transManagerMock.Setup<Task<TxGetHierarchyAddressResponse>>(manager => manager.ExecuteAsync<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(It.IsAny<TxGetHierarchyAddressRequest>())).ReturnsAsync(txAddressResponse);

            // mock data access for email addresses
            TxGetHierarchyEmailResponse txEmailResponse = new TxGetHierarchyEmailResponse() { IoPersonId = "0000001" };
            transManagerMock.Setup<Task<TxGetHierarchyEmailResponse>>(manager => manager.ExecuteAsync<TxGetHierarchyEmailRequest, TxGetHierarchyEmailResponse>(It.IsAny<TxGetHierarchyEmailRequest>())).ReturnsAsync(txEmailResponse);

            // mock STAFF.TYPES valcode response
            ApplValcodes staffTypesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "S", ValActionCode1AssocMember = "S" },
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "V", ValActionCode1AssocMember = ""},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "N"}
                }
            };
            dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).Returns(staffTypesResponse);

            // mock STAFF.STATUSES valcode response
            ApplValcodes staffStatusesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValActionCode1AssocMember = "A" },
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "F", ValActionCode1AssocMember = ""},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "F"}
                }
            };
            dataReaderMock.Setup<ApplValcodes>(accessor => accessor.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).Returns(staffStatusesResponse);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);


            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });
            StaffRepository repository = new StaffRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return repository;
        }
    }
}
