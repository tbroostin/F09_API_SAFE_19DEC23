/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentChecklistRepositoryTests : BaseRepositorySetup
    {
        public string studentId;

        public TestStudentChecklistRepository expectedRepository;

        public StudentChecklistRepository actualRepository;

        public CreateStudentChecklistRequest actualCreateStudentChecklistRequestCtx;
      
         //helper to build repository
        private StudentChecklistRepository BuildStudentChecklistRepository()
        {
            dataReaderMock.Setup(d => d.ReadRecordAsync<YsAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns<string, string, bool>((acyrFile, studentId, b) =>
                {
                    var ysRecord = expectedRepository.ysStudentRecords.FirstOrDefault(ys => ys.awardYear == acyrFile.Split('.')[1]);
                    return Task.FromResult((ysRecord == null) ? null :
                        new YsAcyr()
                        {
                            Recordkey = studentId,
                            ChecklistItemsEntityAssociation = (ysRecord.checklistItems == null) ? null :
                                ysRecord.checklistItems.Select(checklistRecord =>
                                new YsAcyrChecklistItems()
                                {
                                    YsChecklistItemsAssocMember = checklistRecord.checklistItem,
                                    YsDisplayActionAssocMember = checklistRecord.displayAction
                                }).ToList()
                        });
                });

            transManagerMock.Setup(t => t.ExecuteAsync<CreateStudentChecklistRequest, CreateStudentChecklistResponse>(It.IsAny<CreateStudentChecklistRequest>()))
                .Callback<CreateStudentChecklistRequest>((req) =>
                    {
                        expectedRepository.createChecklistHelper(new TestStudentChecklistRepository.CreateTransactionRequestRecord()
                        {
                            awardYear = req.Year,
                            studentId = req.StudentId,
                            checklistItems = req.Items.Select(i => new TestStudentChecklistRepository.ChecklistItemsRecord()
                            {
                                checklistItem = i.ChecklistItems,
                                displayAction = i.DisplayActions,
                            }).ToList()
                        });
                        actualCreateStudentChecklistRequestCtx = req;
                    })

                .Returns<CreateStudentChecklistRequest>((req) =>
                    Task.FromResult(expectedRepository.createTransactionResponseData == null ? null : new CreateStudentChecklistResponse()
                    {
                        ErrorMessage = expectedRepository.createTransactionResponseData.errorMessage
                    }
                ))
                .Verifiable();

            return new StudentChecklistRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        public void StudentChecklistRepositoryTestsInitialize()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestStudentChecklistRepository();
            actualRepository = BuildStudentChecklistRepository();

            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);
            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
        }

        [TestClass]
        public class GetStudentChecklistsTests : StudentChecklistRepositoryTests
        {
            public List<string> awardYears;
           
            public List<StudentFinancialAidChecklist> expectedChecklists
            { get { return expectedRepository.GetStudentChecklistsAsync(studentId, awardYears).Result.ToList(); } }

            public List<StudentFinancialAidChecklist> actualChecklists;
            

            [TestInitialize]
            public async void Initialize()
            {
                StudentChecklistRepositoryTestsInitialize();
                awardYears = expectedRepository.ysStudentRecords.Select(y => y.awardYear).ToList();
                actualChecklists = (await actualRepository.GetStudentChecklistsAsync(studentId, awardYears)).ToList();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedChecklists, actualChecklists);
            }

            [TestMethod]
            public async Task CatchExceptionGettingChecklistTest()
            {
                //arrange
                var year = "1937";
                awardYears.Add(year);
                //act
                var test = await actualRepository.GetStudentChecklistsAsync(studentId, awardYears);
                //assert
                Assert.IsNull(test.FirstOrDefault(c => c.AwardYear == year));
                loggerMock.Verify(l => l.Warn(It.IsAny<Exception>(), string.Format("Unable to get student checklist for studentId {0}, awardYear {1}", studentId, year)));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualRepository.GetStudentChecklistsAsync(studentId, awardYears);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearsRequiredTest()
            {
                awardYears = null;
                await actualRepository.GetStudentChecklistsAsync(studentId, awardYears);
            }
        }

        [TestClass]
        public class GetStudentChecklistTests : StudentChecklistRepositoryTests
        {
            public string awardYear;

            public StudentFinancialAidChecklist expectedChecklist
            { get { return expectedRepository.GetStudentChecklistAsync(studentId, awardYear).Result; } }

            public StudentFinancialAidChecklist actualChecklist;
            
            [TestInitialize]
            public async void Initialize()
            {
                StudentChecklistRepositoryTestsInitialize();
                awardYear = "2015";
                actualChecklist = await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.AreEqual(expectedChecklist, actualChecklist);
                CollectionAssert.AreEqual(expectedChecklist.ChecklistItems, actualChecklist.ChecklistItems);
            }            

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = "";
                await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearRequiredTest()
            {
                awardYear = "";
                await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullYsRecordThrowsExceptionTest()
            {
                awardYear = "1951";
                try
                {
                    await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no checklist items for year {1}", studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullChecklistThrowsExceptionTest()
            {
                expectedRepository.ysStudentRecords.ForEach(ys => ys.checklistItems = null);
                try
                {
                    await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no checklist items for year {1}", studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmptyChecklistThrowsExceptionTest()
            {
                expectedRepository.ysStudentRecords.ForEach(ys => ys.checklistItems = new List<TestStudentChecklistRepository.ChecklistItemsRecord>());
                try
                {
                    await actualRepository.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no checklist items for year {1}", studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            public async Task CatchExceptionCreatingStudentChecklistItemAndLogErrorTest()
            {
                expectedRepository.ysStudentRecords.First(r => r.awardYear == "2015").checklistItems.Add(new TestStudentChecklistRepository.ChecklistItemsRecord()
                    {
                        checklistItem = "",
                        displayAction = "",
                    });
                actualChecklist = await actualRepository.GetStudentChecklistAsync(studentId, "2015");
                Assert.IsNull(actualChecklist.ChecklistItems.FirstOrDefault(c => string.IsNullOrEmpty(c.Code)));
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeast(2));
            }
        }

        [TestClass]
        public class CreateStudentChecklistTests : StudentChecklistRepositoryTests
        {
            public StudentFinancialAidChecklist inputChecklist;
            public string awardYear;

            public StudentFinancialAidChecklist actualChecklist;
            
            [TestInitialize]
            public async void Initialize()
            {
                StudentChecklistRepositoryTestsInitialize();
                awardYear = "1986";
                inputChecklist = new StudentFinancialAidChecklist(studentId, awardYear)
                {
                    ChecklistItems = new List<StudentChecklistItem>()
                    {
                        new StudentChecklistItem("FAFSA", ChecklistItemControlStatus.CompletionRequired),
                        new StudentChecklistItem("CMPLREQDOC", ChecklistItemControlStatus.CompletionRequiredLater),
                        new StudentChecklistItem("SIGNAWDLTR", ChecklistItemControlStatus.RemovedFromChecklist)
                    }
                };
                actualChecklist = await actualRepository.CreateStudentChecklistAsync(inputChecklist);
            }

            [TestMethod]
            public void SuccessCreatingInputChecklist()
            {
                Assert.AreEqual(inputChecklist, actualChecklist);
                CollectionAssert.AreEqual(inputChecklist.ChecklistItems, actualChecklist.ChecklistItems);
            }

            [TestMethod]
            public async Task ActualRequestCtxTest()
            {
                await actualRepository.CreateStudentChecklistAsync(inputChecklist);
                transManagerMock.Verify(t => t.ExecuteAsync<CreateStudentChecklistRequest, CreateStudentChecklistResponse>(It.IsAny<CreateStudentChecklistRequest>()));
                Assert.AreEqual(inputChecklist.StudentId, actualCreateStudentChecklistRequestCtx.StudentId);
                Assert.AreEqual(inputChecklist.AwardYear, actualCreateStudentChecklistRequestCtx.Year);
                Assert.AreEqual(inputChecklist.ChecklistItems.Count(), actualCreateStudentChecklistRequestCtx.Items.Count());
                CollectionAssert.AreEqual(inputChecklist.ChecklistItems.Select(c => c.Code).ToList(), actualCreateStudentChecklistRequestCtx.Items.Select(c => c.ChecklistItems).ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChecklistRequiredTest()
            {
                inputChecklist = null;
                await actualRepository.CreateStudentChecklistAsync(inputChecklist);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ChecklistItemsNullTest()
            {
                inputChecklist.ChecklistItems = null;
                await actualRepository.CreateStudentChecklistAsync(inputChecklist);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ChecklistItemsRequiredTest()
            {
                inputChecklist.ChecklistItems = new List<StudentChecklistItem>();
                try
                {
                    await actualRepository.CreateStudentChecklistAsync(inputChecklist);
                }
                catch(Exception)
                {
                    loggerMock.Verify(l => l.Error("StudentChecklistItems are required to create a StudentChecklist"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task TransactionResponseNullTest()
            {
                expectedRepository.createTransactionResponseData = null;
                try
                {
                    await actualRepository.CreateStudentChecklistAsync(inputChecklist);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error("Error getting CreateStudentChecklist transaction response from Colleague"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorMessageInResponseTest()
            {
                expectedRepository.createTransactionResponseData.errorMessage = "Error Message";

                try
                {
                    await actualRepository.CreateStudentChecklistAsync(inputChecklist);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains(expectedRepository.createTransactionResponseData.errorMessage))));
                    throw;
                }
            }
        }

    }
}
