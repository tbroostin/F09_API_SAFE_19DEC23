// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentChargeRepositoryTests
    {
        [TestClass]
        public class GetStudentChargesLoads : BasePersonSetup
        {
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private IStudentChargeRepository studentChargeRepo;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;

            private string knownStudentId1;
            private string knownStudentId2;
            private string knownStudentId3;
            private IEnumerable<string> ids;

            private string knownArInvItemsIntgId1;
            private string knownArInvItemsIntgId2;
            private string knownArInvItemsIntgId3;
            private string knownArInvItemsIntgId4;
            private string knownArInvItemsIntgId5;
            private IEnumerable<string> studentChargeIds;

            private Collection<DataContracts.Students> students;
            private Collection<DataContracts.ArInvItemsIntg> arInvItemsIntg;
            private Dictionary<string, GuidLookupResult> dicResult;
            string guid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                // Setup mock repositories
                base.MockInitialize();                
                studentRepoMock = new Mock<IStudentRepository>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                studentRepo = studentRepoMock.Object;
                studentChargeRepo = BuildValidStudentChargeRepository();
               
                Ellucian.Colleague.Domain.Base.Entities.Person person1 = GetTestPersonEntity();

                
                arInvItemsIntg = new Collection<DataContracts.ArInvItemsIntg>();
                knownArInvItemsIntgId1 = "18";
                arInvItemsIntg.Add(new DataContracts.ArInvItemsIntg() 
                { Recordkey = knownArInvItemsIntgId1, 
                     
                  RecordGuid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156", 
                  InviIntgAmt = 300m, InviIntgAmtCurrency = "USD",
                  InviIntgArCode = "TUI",
                  InviIntgArInvItem = "168969",
                  InviIntgArType = "01",
                  InviIntgChargeType = "tuition",
                  InviIntgComments = "test",
                  InviIntgDueDate = Convert.ToDateTime("2016-10-13"),
                  InviIntgPersonId = person1.Id,
                  InviIntgTerm = "2015/SP",
                  InviIntgUnitCost = 300,
                  InviIntgUnitCurrency = "USD",
                  InviIntgUnitQty = 3
                });

                knownArInvItemsIntgId2 = "19";
                arInvItemsIntg.Add(new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId2,

                    RecordGuid = "cc2cb4bf-9531-4d38-8bcf-a96bc8628156",
                    InviIntgAmt = 300m,
                    InviIntgAmtCurrency = "USD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168970",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-13"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2015/SP",
                    InviIntgUnitCost = 300,
                    InviIntgUnitCurrency = "USD",
                    InviIntgUnitQty = 3
                });

                knownArInvItemsIntgId3 = "20";
                arInvItemsIntg.Add(new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId3,

                    RecordGuid = "kk2cb4bf-9531-4d38-8bcf-a96bc8628156",
                    InviIntgAmt = 300m,
                    InviIntgAmtCurrency = "USD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168971",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-13"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2015/SP",
                    InviIntgUnitCost = 300,
                    InviIntgUnitCurrency = "USD",
                    InviIntgUnitQty = 3
                });


                knownArInvItemsIntgId4 = "21";
                arInvItemsIntg.Add(new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId4,

                    RecordGuid = "gg2cb4bf-9531-4d38-8bcf-a96bc8628156",
                    InviIntgAmt = 300m,
                    InviIntgAmtCurrency = "USD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168972",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-13"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2015/SP",
                    InviIntgUnitCost = 300,
                    InviIntgUnitCurrency = "USD",
                    InviIntgUnitQty = 3
                });

                knownArInvItemsIntgId5 = "22";
                arInvItemsIntg.Add(new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId5,

                    RecordGuid = "jj2cb4bf-9531-4d38-8bcf-a96bc8628156",
                    InviIntgAmt = 300m,
                    InviIntgAmtCurrency = "USD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168973",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-13"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2015/SP",
                    InviIntgUnitCost = 300,
                    InviIntgUnitCurrency = "USD",
                    InviIntgUnitQty = 3
                });
                // mock data accessor STUDENTS response 
                ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };    
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Students>(ids.ToArray(), true)).ReturnsAsync(students);

               
                studentChargeIds = new List<string>() { knownArInvItemsIntgId1, knownArInvItemsIntgId2, knownArInvItemsIntgId3, knownArInvItemsIntgId4, knownArInvItemsIntgId5 };

                dataReaderMock.Setup(a => a.SelectAsync("AR.INV.ITEMS.INTG", It.IsAny<string>())).ReturnsAsync(studentChargeIds.ToArray());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", studentChargeIds.ToArray(), true)).ReturnsAsync(arInvItemsIntg);
                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "AR.INV.ITEMS.INTG", PrimaryKey = "1" } }
                };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            }

            private IStudentChargeRepository BuildValidStudentChargeRepository()
            {
                // Set up data accessor for mocking 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                // Construct repository
                studentChargeRepo = new StudentChargeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return studentChargeRepo;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                studentChargeRepo = null;
            }

          
            [TestMethod]
            public async Task StudentChargeRepository_GetStudentCharges_NoFilter()
            {
                 dataReaderMock.Setup(a => a.SelectAsync("AR.INV.ITEMS.INTG", It.IsAny<string>())).ReturnsAsync(studentChargeIds.ToArray());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string[]>(), It.IsAny<bool>()))
                   .ReturnsAsync(arInvItemsIntg);
           
                var actuals = await studentChargeRepo.GetAsync(0, 5, false);
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = arInvItemsIntg.FirstOrDefault(i => i.RecordGuid.Equals(actual.Guid));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.InviIntgArCode, actual.AccountsReceivableCode);
                    Assert.AreEqual(expected.InviIntgArType, actual.AccountsReceivableTypeCode);
                    Assert.AreEqual(expected.InviIntgAmt, actual.ChargeAmount);
                    Assert.AreEqual(expected.InviIntgAmtCurrency, actual.ChargeCurrency);
                    Assert.AreEqual(expected.InviIntgDueDate, actual.ChargeDate);
                    Assert.AreEqual(expected.InviIntgChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.InviIntgComments, actual.Comments[0]);
                    Assert.AreEqual(expected.InviIntgArInvItem, actual.InvoiceItemID);
                    Assert.AreEqual(expected.InviIntgPersonId, actual.PersonId);
                    Assert.AreEqual(expected.InviIntgTerm, actual.Term);
                  }
            }


            [TestMethod]
            public async Task StudentChargeRepository_GetStudentCharges_WithFilter()
            {
                dataReaderMock.Setup(a => a.SelectAsync("AR.INV.ITEMS.INTG", It.IsAny<string>())).ReturnsAsync(studentChargeIds.ToArray());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string[]>(), It.IsAny<bool>()))
                   .ReturnsAsync(arInvItemsIntg);

                var actuals = await studentChargeRepo.GetAsync(0, 5, false, "0000011", "2015/SP", "TUI", "tuition", "charge Type", "usage");
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = arInvItemsIntg.FirstOrDefault(i => i.RecordGuid.Equals(actual.Guid));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.InviIntgArCode, actual.AccountsReceivableCode);
                    Assert.AreEqual(expected.InviIntgArType, actual.AccountsReceivableTypeCode);
                    Assert.AreEqual(expected.InviIntgAmt, actual.ChargeAmount);
                    Assert.AreEqual(expected.InviIntgAmtCurrency, actual.ChargeCurrency);
                    Assert.AreEqual(expected.InviIntgDueDate, actual.ChargeDate);
                    Assert.AreEqual(expected.InviIntgChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.InviIntgComments, actual.Comments[0]);
                    Assert.AreEqual(expected.InviIntgArInvItem, actual.InvoiceItemID);
                    Assert.AreEqual(expected.InviIntgPersonId, actual.PersonId);
                    Assert.AreEqual(expected.InviIntgTerm, actual.Term);
                }
            }

            [TestMethod]
            public async Task StudentChargeRepository_GetStudentCharges_WithFilter_EmptyResponse()
            {
                dataReaderMock.Setup(a => a.SelectAsync("AR.INV.ITEMS.INTG", It.IsAny<string>())).ReturnsAsync(studentChargeIds.ToArray());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string[]>(), It.IsAny<bool>()))
                   .ReturnsAsync(() => null);

                var actuals = await studentChargeRepo.GetAsync(0, 5, false, "0000011", "2015/SP", "TUI", "tuition", "charge Type", "usage");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task StudentChargeRepository_GetById()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "AR.INV.ITEMS.INTG", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG",  true)).ReturnsAsync(arInvItemsIntg.First());
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string>(), true)).ReturnsAsync(arInvItemsIntg.First());
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), true)).ReturnsAsync(arInvItemsIntg.First());

                var actual = await studentChargeRepo.GetByIdAsync(guid);
                Assert.IsNotNull(actual);

                var expected = arInvItemsIntg.First();
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
               
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentChargeRepository_Get_RepositoryException()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string[]>(), true)).ThrowsAsync(new RepositoryException());

                var actuals = await studentChargeRepo.GetAsync(0, 3, false, "", "");               
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeRepository_GetById_ArgumentNullException()
            {
               var actual = await studentChargeRepo.GetByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentChargeRepository_GetRecordInfoFromGuidAsync_KeyNotFoundException()
            {
                dicResult[guid] = null;
                var actual = await studentChargeRepo.GetByIdAsync("Bad Id");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentChargeRepository_GetById_Exception()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "AR.INV.ITEMS.INTG", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                var actual = await studentChargeRepo.GetByIdAsync(guid);                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeRepository_GetById_KeyNotFoundException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "AR.INV.ITEMS.INTG", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(() => null);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), true)).ReturnsAsync(arInvItemsIntg.First());

                var actual = await studentChargeRepo.GetByIdAsync(guid);
               
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentChargeRepository_GetById_NullDictionary()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", null);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), true)).ReturnsAsync(arInvItemsIntg.First());

                var actual = await studentChargeRepo.GetByIdAsync(guid);               
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentChargeRepository_GetById_EntityMismatch_RepositoryException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "STUDENT", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), true)).ReturnsAsync(arInvItemsIntg.First());
                
                var actual = await studentChargeRepo.GetByIdAsync(guid);               
            }
      
            [TestMethod]
            public async Task StudentChargeRepository_UpdateAsync()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                Ellucian.Colleague.Domain.Base.Entities.Person person1 = GetTestPersonEntity();

                var studentCharge = new Domain.Student.Entities.StudentCharge(person1.Id, "tuition", Convert.ToDateTime("2016-10-17"))
                {
                    AccountsReceivableTypeCode = "01",
                    AccountsReceivableCode = "TUI",
                    ChargeAmount = 400m,
                    ChargeCurrency = "CAD",
                    Comments = new List<string> { "test" },
                    InvoiceItemID = "168999",
                    Guid = Guid.Empty.ToString(),
                    Term = "2016/SP"                 
                };

                var response = new PostStudentChargesResponse()
                {
                    InviIntgGuid = guid                        
                };

                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentChargesRequest, PostStudentChargesResponse>(It.IsAny<PostStudentChargesRequest>())).ReturnsAsync(response);
                 
                var expected = new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId1,

                    RecordGuid = guid,
                    InviIntgAmt = 400m,
                    InviIntgAmtCurrency = "CAD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168999",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-17"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2016/SP",
                    InviIntgUnitCost = 400,
                    InviIntgUnitCurrency = "CAD",
                    InviIntgUnitQty = 3
                };
                                   
               var lookup = new GuidLookup[] {new GuidLookup(guid)};
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() {Entity = "AR.INV.ITEMS.INTG", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey"});
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
              
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", true)).ReturnsAsync(expected);
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string>(), true)).ReturnsAsync(expected);
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);

                var actual = await studentChargeRepo.UpdateAsync(knownArInvItemsIntgId1, studentCharge);
                Assert.IsNotNull(actual);

                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(studentCharge.AccountsReceivableCode, actual.AccountsReceivableCode);
                Assert.AreEqual(studentCharge.AccountsReceivableTypeCode, actual.AccountsReceivableTypeCode);
                Assert.AreEqual(studentCharge.ChargeAmount, actual.ChargeAmount);
                Assert.AreEqual(studentCharge.ChargeCurrency, actual.ChargeCurrency);
                Assert.AreEqual(studentCharge.ChargeDate, actual.ChargeDate);
                Assert.AreEqual(studentCharge.ChargeType, actual.ChargeType);
                Assert.AreEqual(studentCharge.Comments[0], actual.Comments[0]);
                Assert.AreEqual(studentCharge.InvoiceItemID, actual.InvoiceItemID);
                Assert.AreEqual(studentCharge.PersonId, actual.PersonId);
                Assert.AreEqual(studentCharge.Term, actual.Term);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentChargeRepository_UpdateAsync_RepositoryException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                Ellucian.Colleague.Domain.Base.Entities.Person person1 = GetTestPersonEntity();

                var studentCharge = new Domain.Student.Entities.StudentCharge(person1.Id, "tuition", Convert.ToDateTime("2016-10-17"))
                {
                    AccountsReceivableTypeCode = "01",
                    AccountsReceivableCode = "TUI",
                    ChargeAmount = 400m,
                    ChargeCurrency = "CAD",
                    Comments = new List<string> { "test" },
                    InvoiceItemID = "168999",
                    Guid = Guid.Empty.ToString(),
                    Term = "2016/SP"
                };

                var response = new PostStudentChargesResponse()
                {
                    Error = "Error",
                    StudentChargeErrors = new List<StudentChargeErrors>()
                    {
                       new StudentChargeErrors()
                       {
                           ErrorCodes = "1",
                           ErrorMessages = "Error"
                       }
                    }
                };

                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentChargesRequest, PostStudentChargesResponse>(It.IsAny<PostStudentChargesRequest>()))
                    .ReturnsAsync(response);
                var actual = await studentChargeRepo.UpdateAsync(knownArInvItemsIntgId1, studentCharge);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAsync_ArgumentNullException()
            {
                var actual = await studentChargeRepo.UpdateAsync("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateAsync_InvalidOperationException()
            {
                StudentCharge studentCharge = new StudentCharge("1", "chargeType", null)
                {
                    Guid = Guid.NewGuid().ToString()
                };
                var actual = await studentChargeRepo.UpdateAsync("Bad_Id", studentCharge);
            }

            [TestMethod]
            public async Task StudentChargeRepository_CreateAsync()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                Ellucian.Colleague.Domain.Base.Entities.Person person1 = GetTestPersonEntity();

                var studentCharge = new Domain.Student.Entities.StudentCharge(person1.Id, "tuition", Convert.ToDateTime("2016-10-17"))
                {
                    AccountsReceivableTypeCode = "01",
                    AccountsReceivableCode = "TUI",
                    ChargeAmount = 400m,
                    ChargeCurrency = "CAD",
                    Comments = new List<string> { "test" },
                    InvoiceItemID = "168999",
                    Guid = Guid.Empty.ToString(),
                    Term = "2016/SP"
                };

                var response = new PostStudentChargesResponse()
                {
                    InviIntgGuid = guid
                };

                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentChargesRequest, PostStudentChargesResponse>(It.IsAny<PostStudentChargesRequest>())).ReturnsAsync(response);

                var expected = new DataContracts.ArInvItemsIntg()
                {
                    Recordkey = knownArInvItemsIntgId1,

                    RecordGuid = guid,
                    InviIntgAmt = 400m,
                    InviIntgAmtCurrency = "CAD",
                    InviIntgArCode = "TUI",
                    InviIntgArInvItem = "168999",
                    InviIntgArType = "01",
                    InviIntgChargeType = "tuition",
                    InviIntgComments = "test",
                    InviIntgDueDate = Convert.ToDateTime("2016-10-17"),
                    InviIntgPersonId = person1.Id,
                    InviIntgTerm = "2016/SP",
                    InviIntgUnitCost = 400,
                    InviIntgUnitCurrency = "CAD",
                    InviIntgUnitQty = 3
                };

                var lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "AR.INV.ITEMS.INTG", PrimaryKey = knownArInvItemsIntgId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", true)).ReturnsAsync(expected);
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", It.IsAny<string>(), true)).ReturnsAsync(expected);
                dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvItemsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);

                var actual = await studentChargeRepo.CreateAsync(studentCharge);
                Assert.IsNotNull(actual);

                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(studentCharge.AccountsReceivableCode, actual.AccountsReceivableCode);
                Assert.AreEqual(studentCharge.AccountsReceivableTypeCode, actual.AccountsReceivableTypeCode);
                Assert.AreEqual(studentCharge.ChargeAmount, actual.ChargeAmount);
                Assert.AreEqual(studentCharge.ChargeCurrency, actual.ChargeCurrency);
                Assert.AreEqual(studentCharge.ChargeDate, actual.ChargeDate);
                Assert.AreEqual(studentCharge.ChargeType, actual.ChargeType);
                Assert.AreEqual(studentCharge.Comments[0], actual.Comments[0]);
                Assert.AreEqual(studentCharge.InvoiceItemID, actual.InvoiceItemID);
                Assert.AreEqual(studentCharge.PersonId, actual.PersonId);
                Assert.AreEqual(studentCharge.Term, actual.Term);

            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateAsync_InvalidOperationException()
            {
                StudentCharge studentCharge = new StudentCharge("1", "chargeType", null)
                {
                    Guid = Guid.NewGuid().ToString()
                };
                var actual = await studentChargeRepo.CreateAsync(studentCharge);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DeleteAsync_GetRecordInfoFromGuidAsync_KeyNotFoundException()
            {
                dicResult[guid] = null;
                var actual = await studentChargeRepo.DeleteAsync("Bad Id");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task DeleteAsync_KeyNotFoundException()
            {
                iColleagueTransactionInvokerMock.Setup(repo =>
                repo.ExecuteAsync<DeleteStudentChargeRequest, DeleteStudentChargeResponse>(It.IsAny<DeleteStudentChargeRequest>()))
                    .ReturnsAsync(new DeleteStudentChargeResponse()
                    {
                        DeleteIntgGlPostingErrors = new List<DeleteIntgGlPostingErrors>()
                        {
                            new DeleteIntgGlPostingErrors()
                            {
                                ErrorCode = "1",
                                ErrorMsg = "Error Message"
                            }
                        }
                    });
                var actual = await studentChargeRepo.DeleteAsync(guid);
            }
        }
    }
}
