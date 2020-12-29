// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class AcademicCreditRepositoryTests
    {

        const string _CacheName = "Ellucian.Colleague.Data.Student.Repositories.AcademicCreditRepository";
        ObjectCache localCache = new MemoryCache(_CacheName);
        TestAcademicCreditRepository testAcademicCreditRepository;
        AcademicCreditRepository academicCreditRepository;
        ApiSettings apiSettingsRepository = new ApiSettings("MockSettings");
        private string colleagueTimeZone;
        private List<AcademicCredit> _acadCreditsToSort;
        private List<string> _sortSpecificationIds = new List<string>() { "SORT1", "SORT2" };
        protected Mock<IColleagueDataReader> dataReaderMock = new Mock<IColleagueDataReader>();


        [TestInitialize]
        public async void Initialize()
        {
            testAcademicCreditRepository = new TestAcademicCreditRepository();
            _acadCreditsToSort = (await testAcademicCreditRepository.GetAsync()).Take(5).ToList();
            academicCreditRepository = await BuildValidAcademicCreditRepository();
            colleagueTimeZone = apiSettingsRepository.ColleagueTimeZone;
        }

        [TestClass]
        public class StudentCourseTransfer_GetAll_GetById : AcademicCreditRepositoryTests
        {
            [TestInitialize]
            public void StudentCourseTransfer_Initialize()
            {
                base.Initialize();

            }

            [TestMethod]
            public async Task AcademicCreditRepository_GetStudentCourseTransfersAsync()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.EQUIV.EVALS", It.IsAny<string>())).ReturnsAsync(new string[] { "1" });

                Collection<StudentAcadCred> studentAcadCredColl = new Collection<StudentAcadCred>()
                {
                    new StudentAcadCred()
                    {
                        Recordkey = "1",
                        StcStudentEquivEval = "1",
                        StcPersonId = "0003977"
                    }
                };
                BulkReadOutput<StudentAcadCred> bro = new Ellucian.Data.Colleague.BulkReadOutput<StudentAcadCred>()
                {
                    BulkRecordsRead = studentAcadCredColl
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
                  .ReturnsAsync(bro);

                Collection<StudentEquivEvals> studentEquivEvalsColl = new Collection<StudentEquivEvals>()
                {
                    new StudentEquivEvals()
                    {
                        Recordkey = "1",
                        SteAcadPrograms = new List<string>() { "1", "2" },
                        SteCourseAcadCred = new List<string>() { "1" },
                        SteInstitution = "SteInst1",
                        SteStudentNonCourse = "Math 101"
                    }
                };
                BulkReadOutput<StudentEquivEvals> broSev = new Ellucian.Data.Colleague.BulkReadOutput<StudentEquivEvals>()
                {
                    BulkRecordsRead = studentEquivEvalsColl
                };

                dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(broSev);


                var results = await academicCreditRepository.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());

                Assert.IsNotNull(results);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AcademicCreditRepository_GetStudentCourseTransfersAsync_StAcadCred_Error_RepoException()
        {
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.EQUIV.EVALS", It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "1");
            Collection<StudentAcadCred> studentAcadCredColl = new Collection<StudentAcadCred>()
            {
                    new StudentAcadCred()
                    {
                        Recordkey = "1",
                        StcStudentEquivEval = "1",
                        StcPersonId = "0003977"
                    }
            };
            BulkReadOutput<StudentAcadCred> bro = new Ellucian.Data.Colleague.BulkReadOutput<StudentAcadCred>()
            {
                BulkRecordsRead = studentAcadCredColl,
                InvalidKeys = new string[] { "1" },
                InvalidRecords = dict
            };
            dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
              .ReturnsAsync(bro);
            var results = await academicCreditRepository.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AcademicCreditRepository_GetStudentCourseTransfersAsync_StEquivLvl_Error_RepoException()
        {
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.EQUIV.EVALS", It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "1");
            Collection<StudentAcadCred> studentAcadCredColl = new Collection<StudentAcadCred>()
            {
                    new StudentAcadCred()
                    {
                        Recordkey = "1",
                        StcStudentEquivEval = "1",
                        StcPersonId = "0003977"
                    }
            };
            BulkReadOutput<StudentAcadCred> bro = new Ellucian.Data.Colleague.BulkReadOutput<StudentAcadCred>()
            {
                BulkRecordsRead = studentAcadCredColl
            };
            dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
              .ReturnsAsync(bro);

            Collection<StudentEquivEvals> studentEquivEvalsColl = new Collection<StudentEquivEvals>()
                {
                    new StudentEquivEvals()
                    {
                        Recordkey = "1",
                        SteAcadPrograms = new List<string>() { "1", "2" },
                        SteCourseAcadCred = new List<string>() { "1" },
                        SteInstitution = "SteInst1",
                        SteStudentNonCourse = "Math 101"
                    }
                };
            BulkReadOutput<StudentEquivEvals> broSev = new Ellucian.Data.Colleague.BulkReadOutput<StudentEquivEvals>()
            {
                BulkRecordsRead = studentEquivEvalsColl,
                InvalidKeys = new string[] { "1" },
                InvalidRecords = dict
            };

            dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(broSev);


            var results = await academicCreditRepository.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicCreditRepository_GetStudentCourseTransfersAsync_Invalid_StcStudentEquivEval_RepoException()
        {
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.EQUIV.EVALS", It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "1");
            Collection<StudentAcadCred> studentAcadCredColl = new Collection<StudentAcadCred>()
            {
                    new StudentAcadCred()
                    {
                        Recordkey = "1",
                        StcStudentEquivEval = "BAD_KEY",
                        StcPersonId = "0003977"
                    }
            };
            BulkReadOutput<StudentAcadCred> bro = new Ellucian.Data.Colleague.BulkReadOutput<StudentAcadCred>()
            {
                BulkRecordsRead = studentAcadCredColl
            };
            dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
              .ReturnsAsync(bro);

            Collection<StudentEquivEvals> studentEquivEvalsColl = new Collection<StudentEquivEvals>()
                {
                    new StudentEquivEvals()
                    {
                        Recordkey = "1",
                        SteAcadPrograms = new List<string>() { "1", "2" },
                        SteCourseAcadCred = new List<string>() { "1" },
                        SteInstitution = "SteInst1",
                        SteStudentNonCourse = "Math 101"
                    }
                };
            BulkReadOutput<StudentEquivEvals> broSev = new Ellucian.Data.Colleague.BulkReadOutput<StudentEquivEvals>()
            {
                BulkRecordsRead = studentEquivEvalsColl
            };

            dataReaderMock.Setup(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(broSev);


            var results = await academicCreditRepository.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
        }

        [TestClass]
        public class Get : AcademicCreditRepositoryTests
        {

            [TestMethod]
            public async Task ReturnsSpecifiedAcademicCredits()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.Count());
            }

            [TestMethod]
            public async Task ReturnsSpecifiedAcademicCredits_StudentCourseSectionIds_Correct()
            {
                var allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] allCourseIds = allCredits.Where(ai => ai.Course != null).Select(ai => ai.Id).ToArray();
                string[] allNonCourseIds = allCredits.Where(ai => ai.Course == null).Select(ai => ai.Id).ToArray();
                List<AcademicCredit> credits = (await academicCreditRepository.GetAsync(allIds, false, false)).ToList();
                List<AcademicCredit> courseCredits = credits.Where(c => c.Course != null).ToList();
                List<AcademicCredit> nonCourseCredits = credits.Where(c => c.Course == null).ToList();

                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.Count());
                Assert.AreEqual(allCourseIds.Count(), courseCredits.Count());
                Assert.AreEqual(allNonCourseIds.Count(), nonCourseCredits.Count());

                for(int i = 0; i > allCourseIds.Count(); i++)
                {
                    Assert.AreEqual(allCourseIds[i], courseCredits[i].StudentCourseSectionId);
                }
                for (int j = 0; j > allNonCourseIds.Count(); j++)
                {
                    Assert.AreEqual(allNonCourseIds[j], nonCourseCredits[j].StudentCourseSectionId);
                }
            }

            [TestMethod]
            public async Task AcademicCredit_AdjustedCredits_is_0_when_StcAltcumContribCmplCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.Count());
                for(int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.ElementAt(i).AdjustedCredit);
                    } else
                    {
                        Assert.IsNotNull(credits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.ElementAt(i).AdjustedCredit);
                    }
                }
            }

            [TestMethod]
            public async Task Exclude_Drops_when_includeDrops_flag_is_False()
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse
                };
                List<AcademicCredit> allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).Where(ac => filteredStatuses.Contains(ac.Status)).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] expectedIds = expected.Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, true, false);
                // All should be returned
                Assert.AreEqual(expectedIds.Count(), credits.Count());
                for (int i = 0; i < expectedIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.ElementAt(i).AdjustedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.ElementAt(i).AdjustedCredit);
                    }
                }
            }


            [TestMethod]
            public async Task Return_Drops_when_includeDrops_flag_is_True()
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse,
                    CreditStatus.Dropped
                };
                List<AcademicCredit> allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).Where(ac => filteredStatuses.Contains(ac.Status)).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] expectedIds = expected.Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, true, true);
                // All should be returned
                Assert.AreEqual(expectedIds.Count(), credits.Count());
                for (int i = 0; i < expectedIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.ElementAt(i).AdjustedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.ElementAt(i).AdjustedCredit);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task MissingRecordThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "InvalidId", "3" });
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BlankIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "", "3" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task NullIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", null, "3" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task InvalidIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "INVALID" });
            }

            [TestMethod]
            public async Task EmptyListReturnsEmptyList()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { });
                Assert.AreEqual(0, credits.Count());
            }

            [TestMethod]
            public async Task AcademicCredit_CompletedCredit_is_null_when_StcCmplCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.Count());
                for (int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].CompletedCredit == null)
                    {
                        Assert.IsNull(credits.ElementAt(i).CompletedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.ElementAt(i).CompletedCredit);
                        Assert.AreEqual(expected[i].CompletedCredit, credits.ElementAt(i).CompletedCredit);
                    }
                }
            }
            [TestMethod]
            public async Task AcademicCredit_GPACredit_is_null_when_StcGpaCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.Count());
                for (int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].GpaCredit == null)
                    {
                        Assert.IsNull(credits.ElementAt(i).GpaCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.ElementAt(i).GpaCredit);
                        Assert.AreEqual(expected[i].GpaCredit, credits.ElementAt(i).GpaCredit);
                    }
                }
            }
          

        }

        [TestClass]
        public class GetBestFit : AcademicCreditRepositoryTests
        {
            [TestMethod]
            public async Task BestFitVerifyCount()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> bestFitCredits = await academicCreditRepository.GetAsync(allIds, true, false);
                Assert.AreEqual(allIds.Count(), bestFitCredits.Count());
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitFalseMissingRecordThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "InvalidId", "3" }, false);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitTrueMissingRecordThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "InvalidId", "3" }, true);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitFalseBlankIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "", "3" }, false);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitTrueBlankIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", "", "3" }, true);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitFalseNullIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", null, "3" }, false);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task BestFitTrueNullIdThrows()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { "1", null, "3" }, true);
            }
            [TestMethod]
            public async Task BestFitFalseEmptyListReturnsEmptyList()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { }, false);
                Assert.AreEqual(0, credits.Count());
            }
            [TestMethod]
            public async Task BestFitTrueEmptyListReturnsEmptyList()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(new List<string>() { }, true);
                Assert.AreEqual(0, credits.Count());
            }
            [TestMethod]
            public async Task BestFitTrueCheckCounts()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                // ignoring non-course credits, 2 null term credits mapped to 2009/WI
                IEnumerable<AcademicCredit> bestFitCredits = await academicCreditRepository.GetAsync(allIds, true);
                IEnumerable<AcademicCredit> bestFit2009WICredits = bestFitCredits.Where(ac => ac.TermCode == "2009/WI");
                Assert.AreEqual(2, bestFit2009WICredits.Count());
            }
        }

        [TestClass]
        public class BuildCredits : AcademicCreditRepositoryTests
        {

            IEnumerable<AcademicCredit> testcreds = new List<AcademicCredit>();
            IEnumerable<AcademicCredit> testcredsorig = new List<AcademicCredit>();
            IEnumerable<AcademicCredit> repocreds = new List<AcademicCredit>();
            int credcount;

            [TestInitialize]
            public async void InitializeBuildCredits()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(tc => tc.Id).ToArray();

                // The repository now returns everything, regardless of status, so this filter had to be removed.

                //// This is a test or tests in and of itself.  
                //// The repo will not return the credit at id 55,56 because it is ^H^H^H^H they are not active.  
                //// If the repo starts returning them, most of the tests below will fail.
                ////testcredsorig = tacr.Get();
                ////testcreds = testcredsorig.Where(tc => tc.Id != "55" && tc.Id != "56");

                testcreds = await testAcademicCreditRepository.GetAsync();

                repocreds = await academicCreditRepository.GetAsync(allIds, false, false);
                credcount = testcreds.Count();

            }

            [TestMethod]
            public void AcademicCredit_Id()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).Id, repocreds.ElementAt(i).Id, "Index=" + i);
                }
            }
            [TestMethod]
            public void AcademicCredit_Course()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (testcreds.ElementAt(i).Course != null)
                    {
                        Assert.AreEqual(testcreds.ElementAt(i).Course.Id, repocreds.ElementAt(i).Course.Id);
                    }
                    else
                    {
                        Assert.IsNull(repocreds.ElementAt(i).Course);
                    }
                }
            }
            [TestMethod]
            public void AcademicCredit_VerifiedGrade()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (testcreds.ElementAt(i).VerifiedGrade != null)
                    {
                        Assert.AreEqual(testcreds.ElementAt(i).VerifiedGrade.Id, repocreds.ElementAt(i).VerifiedGrade.Id);
                    }
                    else
                    {
                        Assert.IsNull(repocreds.ElementAt(i).VerifiedGrade);
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_VerifiedGradeTimestamp()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (testcreds.ElementAt(i).VerifiedGrade != null)
                    {
                        Assert.AreEqual(testcreds.ElementAt(i).VerifiedGradeTimestamp.ToLocalDateTime(colleagueTimeZone), repocreds.ElementAt(i).VerifiedGradeTimestamp.ToLocalDateTime(colleagueTimeZone));
                    }
                    else
                    {
                        Assert.IsNull(repocreds.ElementAt(i).VerifiedGradeTimestamp);
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_MidTermGrades()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).MidTermGrades.Count(), repocreds.ElementAt(i).MidTermGrades.Count());
                    int mtcount = testcreds.ElementAt(i).MidTermGrades.Count();
                    // is this level of check necessary/valid? does Add add in a stable order
                    for (int j = 0; j < mtcount; j++)
                    {
                        Assert.AreEqual(testcreds.ElementAt(i).MidTermGrades.ElementAt(j).Position, repocreds.ElementAt(i).MidTermGrades.ElementAt(j).Position);
                        Assert.AreEqual(testcreds.ElementAt(i).MidTermGrades.ElementAt(j).GradeId, repocreds.ElementAt(i).MidTermGrades.ElementAt(j).GradeId);
                        Assert.AreEqual(testcreds.ElementAt(i).MidTermGrades.ElementAt(j).GradeTimestamp, repocreds.ElementAt(i).MidTermGrades.ElementAt(j).GradeTimestamp);
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_SectionId()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (testcreds.ElementAt(i).SectionId != null)
                    {
                        Assert.AreEqual(testcreds.ElementAt(i).SectionId, repocreds.ElementAt(i).SectionId);
                    }
                    else
                    {
                        Assert.IsNull(repocreds.ElementAt(i).SectionId);
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_AdjustedCredit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).AdjustedCredit, repocreds.ElementAt(i).AdjustedCredit);
                }
            }
            [TestMethod]
            public void AcademicCredit_AdjustedGpaCredit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).AdjustedGpaCredit, repocreds.ElementAt(i).AdjustedGpaCredit);
                }
            }
            [TestMethod]
            public void AcademicCredit_CanBeReplaced()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).CanBeReplaced, repocreds.ElementAt(i).CanBeReplaced);
                }
            }
            [TestMethod]
            public void AcademicCredit_ContinuingEducationUnits()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).ContinuingEducationUnits, repocreds.ElementAt(i).ContinuingEducationUnits);
                }
            }
            [TestMethod]
            public void AcademicCredit_CourseName()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).CourseName, repocreds.ElementAt(i).CourseName);
                }
            }
            [TestMethod]
            public void AcademicCredit_Title()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).Title, repocreds.ElementAt(i).Title);
                }
            }
            [TestMethod]
            public void AcademicCredit_Credit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).Credit, repocreds.ElementAt(i).Credit);
                }
            }
            [TestMethod]
            public void AcademicCredit_DepartmentCodes()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).DepartmentCodes.Count(), repocreds.ElementAt(i).DepartmentCodes.Count());
                }
            }
            [TestMethod]
            public void AcademicCredit_DifferingDepartmentCodes()
            {
                // Be sure the departments on the academic credit's course are not being defaulted to the academic credit.
                // Specifically look at COMP-200 where the academic credit created should have a department of COMM only but the course
                // associated to COMP-200 has a department of ENGL.  ENGL should not end up in the list.
                var testComm200 = testcreds.Where(a => a.Id == "51").FirstOrDefault();
                Assert.AreEqual(1, testComm200.DepartmentCodes.Count());
                Assert.AreEqual("COMM", testComm200.DepartmentCodes.ElementAt(0));
            }
            [TestMethod]
            public void AcademicCredit_DefaultsDepartmentFromCourse()
            {
                // Test to be sure when the incoming academic credit has no departments that the departments on the associated course are defaulting.
                var testHist100 = testcreds.Where(a => a.Id == "1").FirstOrDefault();
                Assert.AreEqual(2, testHist100.DepartmentCodes.Count());
                Assert.AreEqual("POLI", testHist100.DepartmentCodes.ElementAt(0));
            }
            [TestMethod]
            public void AcademicCredit_GpaCredit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).GpaCredit, repocreds.ElementAt(i).GpaCredit);
                }
            }
            [TestMethod]
            public void AcademicCredit_GradePoints()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).GradePoints, repocreds.ElementAt(i).GradePoints);
                }
            }
            [TestMethod]
            public void AcademicCredit_GradeSchemeCode()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).GradeSchemeCode, repocreds.ElementAt(i).GradeSchemeCode);
                }
            }
            [TestMethod]
            public void AcademicCredit_CourseLevelCode()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).CourseLevelCode, repocreds.ElementAt(i).CourseLevelCode);
                }
            }
            [TestMethod]
            public void AcademicCredit_AcadLevelCode()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).AcademicLevelCode, repocreds.ElementAt(i).AcademicLevelCode);
                }
            }

            [TestMethod]
            public void AcademicCredit_SubjectCode()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).SubjectCode, repocreds.ElementAt(i).SubjectCode);
                }
            }
            [TestMethod]
            public void AcademicCredit_TermCode()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).TermCode, repocreds.ElementAt(i).TermCode);
                }
            }
            [TestMethod]
            public void AcademicCredit_Type()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).Type, repocreds.ElementAt(i).Type);
                }
            }
            [TestMethod]
            public void AcademicCredit_LocalType()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).LocalType, repocreds.ElementAt(i).LocalType);
                }
            }
            [TestMethod]
            public void AcademicCredit_CompletedCredit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).CompletedCredit, repocreds.ElementAt(i).CompletedCredit);
                }
            }
            [TestMethod]
            public void AcademicCredit_AttemptedCredit()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).AttemptedCredit, repocreds.ElementAt(i).AttemptedCredit);
                }
            }
            [TestMethod]
            public void AcademicCredit_Status()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).Status, repocreds.ElementAt(i).Status);
                }
            }

            [TestMethod]
            public void AcademicCredit_EndDate()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (repocreds.ElementAt(i).TermCode == "2009/SP")
                    {
                        Assert.AreEqual(new DateTime(2009, 5, 11), repocreds.ElementAt(i).EndDate);
                    }
                    else
                    {
                        if (repocreds.ElementAt(i).Id == "39" || repocreds.ElementAt(i).Id == "40")
                        {
                            Assert.IsNull(repocreds.ElementAt(i).TermCode);
                        }
                        else
                        {
                            if (repocreds.ElementAt(i).TermCode == "2011/SP")
                            {
                                Assert.AreEqual(null, repocreds.ElementAt(i).EndDate);
                            }
                        }
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_StartDate()
            {
                for (int i = 0; i < credcount; i++)
                {
                    if (repocreds.ElementAt(i).TermCode == "2009/SP")
                    {
                        Assert.AreEqual(new DateTime(2009, 1, 20), repocreds.ElementAt(i).StartDate);
                    }
                    else
                    {
                        // These are special cases where there is no term specified
                        if (repocreds.ElementAt(i).Id == "39" || repocreds.ElementAt(i).Id == "40" || repocreds.ElementAt(i).Id == "61")
                        {
                            Assert.IsNull(repocreds.ElementAt(i).TermCode);
                        }
                        else
                        {
                            //Assert.AreEqual(1, 1);
                            if (repocreds.ElementAt(i).TermCode == "2011/SP")
                            {
                                Assert.AreEqual(null, repocreds.ElementAt(i).StartDate);
                            }
                        }
                    }
                }
            }

            [TestMethod]
            public void AcademicCredit_SectionNumber()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).SectionNumber, repocreds.ElementAt(i).SectionNumber);
                }
            }

            [TestMethod]
            public void AcademicCredit_ReplacedStatus()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).ReplacedStatus, repocreds.ElementAt(i).ReplacedStatus);
                }
            }

            [TestMethod]
            public void AcademicCredit_RepeatList()
            {
                for (int i = 0; i < credcount; i++)
                {
                    Assert.AreEqual(testcreds.ElementAt(i).RepeatAcademicCreditIds, repocreds.ElementAt(i).RepeatAcademicCreditIds);
                    if (repocreds.ElementAt(i).Id == "65" || repocreds.ElementAt(i).Id == "66")
                    {
                        Assert.IsTrue(repocreds.ElementAt(i).RepeatAcademicCreditIds.Contains("65"));
                        Assert.IsTrue(repocreds.ElementAt(i).RepeatAcademicCreditIds.Contains("65"));
                    }
                }
            }
        }

        [TestClass]
        public class GetAll : AcademicCreditRepositoryTests
        {
            [TestMethod]
            public async Task GetsAllAcademicCreditsRegardlessOfStatus()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, false);
                // All should be returned that included dropped ones too
                Assert.AreEqual(allIds.Count(), credits.Count());
                Assert.IsTrue(credits.Where(c => c.Id == "100" && c.Status==CreditStatus.Dropped).Any());
                Assert.IsTrue(credits.Where(c => c.Id == "101" && c.Status==CreditStatus.Dropped).Any());
            }

            [TestMethod]
            public async Task GetsAllAcademicCreditsRegardlessOfStatus_Get2Async()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCreditMinimum> credits = await academicCreditRepository.GetAcademicCreditMinimumAsync(allIds, false, false);
                // All should be returned that included dropped ones too
                Assert.AreEqual(allIds.Count(), credits.Count());
                Assert.IsTrue(credits.Where(c => c.Id == "100" && c.Status == CreditStatus.Dropped).Any());
                Assert.IsTrue(credits.Where(c => c.Id == "101" && c.Status == CreditStatus.Dropped).Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetsAllAcademicCreditsRegardlessOfStatus_Get2Async_ArgumentOutOfRangeException()
            {
                string[] allIds = new[] { "abcd" };
                IEnumerable<AcademicCreditMinimum> credits = await academicCreditRepository.GetAcademicCreditMinimumAsync(allIds, false, false);
            }

            [TestMethod]
            public async Task GetsAllAcademicCreditsRegardlessOfStatus_Get2Async_FilterTrue()
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse,
                    CreditStatus.Dropped
                };
                var acadCredits = await testAcademicCreditRepository.GetAsync();
                string[] allIds = (acadCredits).Select(ai => ai.Id).ToArray();
                int count = acadCredits.Where(ac => filteredStatuses.Contains(ac.Status)).Count();
                IEnumerable<AcademicCreditMinimum> credits = await academicCreditRepository.GetAcademicCreditMinimumAsync(allIds, true, true);
                // All should be returned that included dropped ones too
                Assert.AreEqual(count, credits.Count());
            }

            [TestMethod]
            public async Task GetsFilteredAcademicCreditsWhenFilterArgNull_IncludeDrops_False()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false);
                // Credits are returned, but fewer than the total number but includes dropped ones
                Assert.IsTrue(credits.Count() > 0);
                Assert.IsTrue(credits.Count() < allIds.Count());
                Assert.IsFalse(credits.Where(c => c.Id == "100" && c.Status == CreditStatus.Dropped).Any());
                Assert.IsFalse(credits.Where(c => c.Id == "101" && c.Status == CreditStatus.Dropped).Any());
            }

            [TestMethod]
            public async Task GetsFilteredAcademicCreditsWhenFilterArgTrue_IncludeDrops_False()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds);
                // Credits are returned, but fewer than the total number but includes dropped ones
                Assert.IsTrue(credits.Count() > 0);
                Assert.IsTrue(credits.Count() < allIds.Count());
                Assert.IsFalse(credits.Where(c => c.Id == "100" && c.Status == CreditStatus.Dropped).Any());
                Assert.IsFalse(credits.Where(c => c.Id == "101" && c.Status == CreditStatus.Dropped).Any());
            }

            [TestMethod]
            public async Task GetsFilteredAcademicCreditsWhenFilterArgNull_IncludeDrops_True()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAsync(allIds, false, true, true);
                // Credits are returned, but fewer than the total number but includes dropped ones
                Assert.IsTrue(credits.Count() > 0);
                Assert.IsTrue(credits.Count() < allIds.Count());
                Assert.IsTrue(credits.Where(c => c.Id == "100" && c.Status == CreditStatus.Dropped).Any());
                Assert.IsTrue(credits.Where(c => c.Id == "101" && c.Status == CreditStatus.Dropped).Any());
            }
        }

        [TestClass]
        public class Pilot : AcademicCreditRepositoryTests
        {
            IEnumerable<PilotAcademicCredit> academicCredits;
            AcademicCreditRepository pilotAcademicCreditRepository;

            [TestInitialize]
            public async void Initialize()
            {
                base.Initialize();
                var transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                var loggerMock = new Mock<ILogger>();
                var apiSettingsMock = new ApiSettings("null");
                // Set up data reader for mocking 
                var dataReaderMock = new Mock<IColleagueDataReader>();
                transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Cache mocking
                var cacheProviderMock = new Mock<ICacheProvider>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                //Mock DataReader
                var creditIds = new List<string>() { "1", "2", "3", "108","109" };
                dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).Returns(Task.FromResult(creditIds.ToArray()));
                var creds = new List<StudentAcadCred>();
                var academics = await testAcademicCreditRepository.GetAsync();
                foreach (var testCredit in academics.Where(a => creditIds.Contains(a.Id)))
                {
                    var sac = BuildValidStcResponse(testCredit);
                    creds.AddRange(sac);
                }
                foreach(var cred in creds)
                {
                    cred.StcPersonId = cred.Recordkey;
                }
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<StudentAcadCred>(creds)));
                // Credit Types transaction mockery
                Collection<CredTypes> credTypeResponse = new Collection<CredTypes>(){ new CredTypes() { Recordkey = "IN", CrtpCategory = "I"},
                                                                                  new CredTypes() { Recordkey = "TRN", CrtpCategory = "T"},
                                                                                  new CredTypes() { Recordkey = "CE", CrtpCategory = "C"},
                                                                                  new CredTypes() {Recordkey = "OTH", CrtpCategory = "O"}};
                dataReaderMock.Setup<Task<Collection<CredTypes>>>(acc => acc.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "", true)).Returns(Task.FromResult(credTypeResponse));
                // StudentAcadCredStatus mock
                ApplValcodes statusCodeResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "N", ValActionCode1AssocMember = "1" },
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "2"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "W", ValActionCode1AssocMember = "4"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValActionCode1AssocMember = "6"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "TR", ValActionCode1AssocMember = "7"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "NC", ValActionCode1AssocMember = "7"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "PR", ValActionCode1AssocMember = "8"},}
                };
                dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", true)).Returns(Task.FromResult(statusCodeResponse));

                pilotAcademicCreditRepository = new AcademicCreditRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object, new TestCourseRepository(), new TestGradeRepository(), new TestTermRepository(), apiSettingsMock);
            }

            [TestMethod]
            public async Task GetPilotAcademicCreditsByStudentIdsAsync()
            {
                var students = new List<string>() { "1", "2", "3" };
                
                var result = await pilotAcademicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(students, AcademicCreditDataSubset.None);
                Assert.AreEqual(5, result.Count());
                Assert.AreEqual(0m,result["108"][0].GpaCredit);
                Assert.AreEqual(3.0m,result["108"][0].CompletedCredit);
                Assert.AreEqual(1.0m,result["109"][0].GpaCredit);
                Assert.AreEqual(0m,result["109"][0].CompletedCredit);
            }
        }

        [TestClass]
        public class GetAcademicCreditsBySectionIdsAsync : AcademicCreditRepositoryTests
        {

            [TestMethod]
            public async Task Success_GetAcademicCreditsBySectionIdsAsync()
            {
                // This method should return all academic credits (unfiltered) for a section - of all statuses
                // including drops, withdrawns, etc.
                string[] allAcademicCreditIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                // Which section Ids I use here is irrelevant - all academic credits will be returned by the data reader.
                IEnumerable<string> sectionIds = new List<string>() { "1", "2" };
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsAsync(sectionIds);
                // All should be returned
                Assert.AreEqual(allAcademicCreditIds.Count(), credits.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionWhenEmptySectionIds_GetAcademicCreditsBySectionIdsAsync()
            {
                IEnumerable<string> sectionIds = new List<string>();
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsAsync(sectionIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionWhenNullSectionIds_GetAcademicCreditsBySectionIdsAsync()
            {
                IEnumerable<AcademicCredit> credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsAsync(null);
            }
        }

        [TestClass]
        public class GetAcademicCreditsBySectionIdsWithInvalidKeysAsync : AcademicCreditRepositoryTests
        {

            [TestMethod]
            public async Task Success_GetAcademicCreditsBySectionIdsAsync()
            {
                // This method should return all academic credits (unfiltered) for a section - of all statuses
                // including drops, withdrawns, etc.
                string[] allAcademicCreditIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                // Which section Ids I use here is irrelevant - all academic credits will be returned by the data reader.
                IEnumerable<string> sectionIds = new List<string>() { "1", "2" };
               AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIds);
                // All should be returned
                Assert.AreEqual(allAcademicCreditIds.Count(), credits.AcademicCredits.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionWhenEmptySectionIds_GetAcademicCreditsBySectionIdsAsync()
            {
                IEnumerable<string> sectionIds = new List<string>();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionWhenNullSectionIds_GetAcademicCreditsBySectionIdsAsync()
            {
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(null);
            }
            //validate DatReader.SelectAsync is called with distinct values
            [TestMethod]
            public async Task SectionIds_have_duplicates()
            {
                // This method should return all academic credits (unfiltered) for a section - of all statuses
                // including drops, withdrawns, etc.
                string[] allAcademicCreditIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                // Which section Ids I use here is irrelevant - all academic credits will be returned by the data reader.
                IEnumerable<string> sectionIds = new List<string>() { "1", "2","1","2" };
                string[] nonDuplicateSectionIds = new string[] { "1", "2" };
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIds);
                // All should be returned
                Assert.AreEqual(allAcademicCreditIds.Count(), credits.AcademicCredits.Count());
                dataReaderMock.Verify(d => d.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '?' SAVING SCS.STUDENT.ACAD.CRED", nonDuplicateSectionIds,"?",true,It.IsAny<int>()));
                Assert.AreEqual(allAcademicCreditIds.Count(), credits.AcademicCredits.Count());


            }

        }


        [TestClass]
        public class GetWithInvalidKeysAsync : AcademicCreditRepositoryTests
        {

            [TestMethod]
            public async Task ReturnsSpecifiedAcademicCredits()
            {
                string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.AcademicCredits.Count());
            }

            [TestMethod]
            public async Task ReturnsSpecifiedAcademicCredits_StudentCourseSectionIds_Correct()
            {
                var allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] allCourseIds = allCredits.Where(ai => ai.Course != null).Select(ai => ai.Id).ToArray();
                string[] allNonCourseIds = allCredits.Where(ai => ai.Course == null).Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = (await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, false));
                List<AcademicCredit> courseCredits = credits.AcademicCredits.Where(c => c.Course != null).ToList();
                List<AcademicCredit> nonCourseCredits = credits.AcademicCredits.Where(c => c.Course == null).ToList();

                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.AcademicCredits.Count());
                Assert.AreEqual(allCourseIds.Count(), courseCredits.Count());
                Assert.AreEqual(allNonCourseIds.Count(), nonCourseCredits.Count());

                for (int i = 0; i > allCourseIds.Count(); i++)
                {
                    Assert.AreEqual(allCourseIds[i], courseCredits[i].StudentCourseSectionId);
                }
                for (int j = 0; j > allNonCourseIds.Count(); j++)
                {
                    Assert.AreEqual(allNonCourseIds[j], nonCourseCredits[j].StudentCourseSectionId);
                }
            }

            [TestMethod]
            public async Task AcademicCredit_AdjustedCredits_is_0_when_StcAltcumContribCmplCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.AcademicCredits.Count());
                for (int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                }
            }

            [TestMethod]
            public async Task Exclude_Drops_when_includeDrops_flag_is_False()
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse
                };
                List<AcademicCredit> allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).Where(ac => filteredStatuses.Contains(ac.Status)).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] expectedIds = expected.Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, true, false);
                // All should be returned
                Assert.AreEqual(expectedIds.Count(), credits.AcademicCredits.Count());
                for (int i = 0; i < expectedIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                }
            }


            [TestMethod]
            public async Task Return_Drops_when_includeDrops_flag_is_True()
            {
                List<CreditStatus> filteredStatuses = new List<CreditStatus>()
                {
                    CreditStatus.Add,
                    CreditStatus.New,
                    CreditStatus.Preliminary,
                    CreditStatus.Withdrawn,
                    CreditStatus.TransferOrNonCourse,
                    CreditStatus.Dropped
                };
                List<AcademicCredit> allCredits = (await testAcademicCreditRepository.GetAsync()).ToList();
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).Where(ac => filteredStatuses.Contains(ac.Status)).ToList();
                string[] allIds = allCredits.Select(ai => ai.Id).ToArray();
                string[] expectedIds = expected.Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, true, true);
                // All should be returned
                Assert.AreEqual(expectedIds.Count(), credits.AcademicCredits.Count());
                for (int i = 0; i < expectedIds.Count(); i++)
                {
                    if (expected[i].AdjustedCredit == null)
                    {
                        Assert.IsNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                        Assert.AreEqual(expected[i].AdjustedCredit, credits.AcademicCredits.ElementAt(i).AdjustedCredit);
                    }
                }
            }

            [TestMethod]
            public async Task MissingRecordThrows()
            {
                Collection<StudentAcadCred> stcEmptyResponse = new Collection<StudentAcadCred>();
                string[] requestedIds3 = { "1", "missng", "3" };
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds3, true)).Returns(Task.FromResult(stcEmptyResponse));

                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(requestedIds3);
                Assert.AreEqual(0, credits.AcademicCredits.Count());
                Assert.AreEqual(3, credits.InvalidAcademicCreditIds.Count());
            }
            [TestMethod]
            public async Task BlankIdThrows()
            {
                Collection<StudentAcadCred> stcEmptyResponse = new Collection<StudentAcadCred>();
                string[] requestedIds3 = { "1", "", "3" };
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds3, true)).Returns(Task.FromResult(stcEmptyResponse));

                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(new List<string>() { "1", "", "3" });
                Assert.AreEqual(0, credits.AcademicCredits.Count());
                Assert.AreEqual(3, credits.InvalidAcademicCreditIds.Count());

            }

            [TestMethod]
            public async Task NullIdThrows()
            {
                Collection<StudentAcadCred> stcEmptyResponse = new Collection<StudentAcadCred>();

                string[] requestedIds3 = { "1", null, "3" };
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds3, true)).Returns(Task.FromResult(stcEmptyResponse));

                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(requestedIds3);
                Assert.AreEqual(0, credits.AcademicCredits.Count());
                Assert.AreEqual(3, credits.InvalidAcademicCreditIds.Count());

            }

            [TestMethod]
            public async Task InvalidIdThrows()
            {
                Collection<StudentAcadCred> stcEmptyResponse = new Collection<StudentAcadCred>();

                string[] requestedIds3 = { "1", "Invalid", "3" };
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds3, true)).Returns(Task.FromResult(stcEmptyResponse));

                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(requestedIds3);
                Assert.AreEqual(0, credits.AcademicCredits.Count());
                Assert.AreEqual(3, credits.InvalidAcademicCreditIds.Count());

            }

            [TestMethod]
            public async Task EmptyListReturnsEmptyList()
            {
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(new List<string>() { });
                Assert.AreEqual(0, credits.AcademicCredits.Count());
            }

            [TestMethod]
            public async Task AcademicCredit_CompletedCredit_is_null_when_StcCmplCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.AcademicCredits.Count());
                for (int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].CompletedCredit == null)
                    {
                        Assert.IsNull(credits.AcademicCredits.ElementAt(i).CompletedCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.AcademicCredits.ElementAt(i).CompletedCredit);
                        Assert.AreEqual(expected[i].CompletedCredit, credits.AcademicCredits.ElementAt(i).CompletedCredit);
                    }
                }
            }
            [TestMethod]
            public async Task AcademicCredit_GPACredit_is_null_when_StcGpaCred_is_null()
            {
                List<AcademicCredit> expected = (await testAcademicCreditRepository.GetAsync()).ToList();
                string[] allIds = expected.Select(ai => ai.Id).ToArray();
                AcademicCreditsWithInvalidKeys credits = await academicCreditRepository.GetWithInvalidKeysAsync(allIds, false, false);
                // All should be returned
                Assert.AreEqual(allIds.Count(), credits.AcademicCredits.Count());
                for (int i = 0; i < allIds.Count(); i++)
                {
                    if (expected[i].GpaCredit == null)
                    {
                        Assert.IsNull(credits.AcademicCredits.ElementAt(i).GpaCredit);
                    }
                    else
                    {
                        Assert.IsNotNull(credits.AcademicCredits.ElementAt(i).GpaCredit);
                        Assert.AreEqual(expected[i].GpaCredit, credits.AcademicCredits.ElementAt(i).GpaCredit);
                    }
                }
            }


        }
        [TestClass]
        public class GetSortedAcademicCreditsBySortSpecificationIdAsync : AcademicCreditRepositoryTests
        {
            Dictionary<string, Transactions.SortStudentAcadCredsResponse> responseDict;
            Mock<IColleagueTransactionFactory> transactionFactoryMock;
            Mock<IColleagueTransactionInvoker> transactionInvokerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;

            // Return mocked up repo
            AcademicCreditRepository acr;

            [TestInitialize]
            public async void Initialize_GetSortedAcademicCreditsBySortSpecificationIdAsync()
            {
                responseDict = await BuildSortStudentAcadCredsResponses(_acadCreditsToSort, _sortSpecificationIds);
                transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                transactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                transactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvokerMock.Object);
                cacheProviderMock = new Mock<ICacheProvider>();
                loggerMock = new Mock<ILogger>();
                apiSettingsMock = new ApiSettings("null");

                // Return mocked up repo
                acr = new AcademicCreditRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object, new TestCourseRepository(), new TestGradeRepository(), new TestTermRepository(), apiSettingsMock);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Null_AcademicCredits()
            {
                var sortedIds = await academicCreditRepository.GetSortedAcademicCreditsBySortSpecificationIdAsync(null, _sortSpecificationIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Empty_AcademicCredits()
            {
                var sortedIds = await academicCreditRepository.GetSortedAcademicCreditsBySortSpecificationIdAsync(new List<AcademicCredit>(), _sortSpecificationIds);

            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task Null_Response()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["null"]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, _sortSpecificationIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task Error_Response()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["error"]);
                            
                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, _sortSpecificationIds);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SortedAcademicCreditIds_NullAcademicCredits()
            {

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(null, _sortSpecificationIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SortedAcademicCreditIds_EmptyListAcademicCredits()
            {
                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(new List<AcademicCredit>(), _sortSpecificationIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task Null_SortedAcademicCreditIds()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["nullSortedIds"]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, _sortSpecificationIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task Empty_SortedAcademicCreditIds()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["emptySortedIds"]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, _sortSpecificationIds);
            }

            [TestMethod]
            public async Task Valid()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["valid"]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, _sortSpecificationIds);
                Assert.AreEqual(sortedIds.Count, _sortSpecificationIds.Distinct().Count());
            }

            [TestMethod]
            public async Task Valid_NullSortSpecificationIds()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["valid"]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, null);
                Assert.AreEqual(sortedIds.Count, _sortSpecificationIds.Distinct().Count());
            }

            [TestMethod]
            public async Task Valid_DuplicateSortSpecificationId()
            {
                transactionInvokerMock.Setup(manager => manager
                    .ExecuteAsync<Transactions.SortStudentAcadCredsRequest, Transactions.SortStudentAcadCredsResponse>(It.IsAny<Transactions.SortStudentAcadCredsRequest>()))
                    .ReturnsAsync(responseDict["validDuplicates"]);

                var sortSpecificationIdsWithDuplicate = new List<string>();
                sortSpecificationIdsWithDuplicate.AddRange(_sortSpecificationIds);
                sortSpecificationIdsWithDuplicate.Add(_sortSpecificationIds[0]);

                var sortedIds = await acr.GetSortedAcademicCreditsBySortSpecificationIdAsync(_acadCreditsToSort, sortSpecificationIdsWithDuplicate);
                Assert.AreEqual(sortedIds.Count, _sortSpecificationIds.Distinct().Count());
            }
        }

        [TestClass]
        public class FilterAcademicCreditsAsync : AcademicCreditRepositoryTests
        {
            string criteria = "WITH STC.COURSE.LEVEL EQ '100' ";
            [TestInitialize]
            public  void TestInitialize()
            {
                criteria = "WITH STC.COURSE.LEVEL EQ '100' ";
            }
            //test with null parameters
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task FilterAcademicCredits_AcademicCredits_Is_Null()
            {
                IEnumerable<AcademicCredit> filteredCredits;
               filteredCredits= await academicCreditRepository.FilterAcademicCreditsAsync(null, criteria);
            }

            [TestMethod]
            public async Task FilterAcademicCredits_Criteria_Is_Null()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, null);
                Assert.AreEqual(5, filteredCredits.Count());
                Assert.AreEqual(_acadCreditsToSort[0], filteredCredits.ToList()[0]);
                Assert.AreEqual(_acadCreditsToSort[1], filteredCredits.ToList()[1]);
                Assert.AreEqual(_acadCreditsToSort[2], filteredCredits.ToList()[2]);
                Assert.AreEqual(_acadCreditsToSort[3], filteredCredits.ToList()[3]);
                Assert.AreEqual(_acadCreditsToSort[4], filteredCredits.ToList()[4]);

            }
            //test with empty parameters
            [TestMethod]
            public async Task FilterAcademicCredits_AcademicCredits_Is_Empty()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(new List<AcademicCredit>(), criteria);
                Assert.AreEqual(0, filteredCredits.Count());
            }

            [TestMethod]
            public async Task FilterAcademicCredits_Criteria_Is_Empty()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, string.Empty);
                Assert.AreEqual(_acadCreditsToSort, filteredCredits);
            }
            //test when datareader throws exception
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Throws_Exception()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                dataReaderMock.Setup(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Throws<Exception>();
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
            }
            //test when datareader return empty filter ids- it means none of the academic credits met the critria hence retuen empty list
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_Empty_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds = new string[] { };
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(0, filteredCredits.Count());
            }
            //test when datareader returns null filter ids- it means none of the academic credits met the criteria hence returns empty list
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_Null_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds =null;
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(0, filteredCredits.Count());
            }
            //test when datareader returns subset
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_Subset_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds = new string[] { "2", "4" };
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(2,filteredCredits.Count());
            }
            //test when datareader returns all
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_All_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds = new string[] {"1", "2","3", "4" ,"5"};
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(5, filteredCredits.Count());
                Assert.AreEqual(_acadCreditsToSort[0], filteredCredits.ToList()[0]);
                Assert.AreEqual(_acadCreditsToSort[1], filteredCredits.ToList()[1]);
                Assert.AreEqual(_acadCreditsToSort[2], filteredCredits.ToList()[2]);
                Assert.AreEqual(_acadCreditsToSort[3], filteredCredits.ToList()[3]);
                Assert.AreEqual(_acadCreditsToSort[4], filteredCredits.ToList()[4]);
            }
            //test when datareader returns filteredId which is not in list of academic credits //not possible but in case
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_NotInList_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds = new string[] { "7","8" };
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(0, filteredCredits.Count());
            }
            //test when datareader returns filteredId which is not in list of academic credits //not possible but in case
            [TestMethod]
            public async Task FilterAcademicCredits_DataReader_Returns_Few_NotInList_FilteredIds()
            {
                IEnumerable<AcademicCredit> filteredCredits;
                string[] creditIds = new string[] { "1", "8", "2" };
                dataReaderMock.Setup<Task<string[]>>(d => d.SelectAsync("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(creditIds));
                filteredCredits = await academicCreditRepository.FilterAcademicCreditsAsync(_acadCreditsToSort, criteria);
                Assert.AreEqual(2, filteredCredits.Count());
            }
        }

        private async Task<AcademicCreditRepository> BuildValidAcademicCreditRepository()
        {

            var transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();
            var apiSettingsMock = new ApiSettings("null");
            // Set up data reader for mocking 
          //  var dataReaderMock = new Mock<IColleagueDataReader>();
            var transactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvokerMock.Object);

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
            null,
            new SemaphoreSlim(1, 1)
            )));


            // Mock up response for grade repository
            Collection<Grades> gradeResponse = await BuildValidGradeResponse();
            dataReaderMock.Setup<Collection<Grades>>(grades => grades.BulkReadRecord<Grades>("GRADES", "", true)).Returns(gradeResponse);

            // Response for multiple-record inquiry
            Collection<StudentAcadCred> stcMultiResponse = new Collection<StudentAcadCred>();

            // Response for bad requests.  Currently the accessor just returns an empty object.
            Collection<StudentAcadCred> stcEmptyResponse = new Collection<StudentAcadCred>();

            // Response for all records
            Collection<StudentAcadCred> academicCreditAllResponse = new Collection<StudentAcadCred>();
            Collection<StudentAcadCredCc> academicCreditCcAllResponse = new Collection<StudentAcadCredCc>();
            Collection<StudentCourseSec> studentCourseSecAllResponse = new Collection<StudentCourseSec>();
            Collection<StudentCourseSecCc> studentCourseSecCcAllResponse = new Collection<StudentCourseSecCc>();
            var academics = await testAcademicCreditRepository.GetAsync();
            foreach (var testCredit in academics)
            {
                // Build STUDENT.ACAD.CRED responses and mock
                Collection<StudentAcadCred> studentAcadCredResponse = BuildValidStcResponse(testCredit);
                string[] idArray = new List<string>() { testCredit.Id }.ToArray();
                dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", idArray, true)).Returns(Task.FromResult(studentAcadCredResponse));

                // test credit 54 to simulate no STUDENT.ACAD.CRED.CC record
                if (testCredit.Id != "54")
                {
                    StudentAcadCredCc stcccResponse = BuildValidStcccResponse(testCredit);
                    dataReaderMock.Setup<Task<StudentAcadCredCc>>(acc => acc.ReadRecordAsync<StudentAcadCredCc>("STUDENT.ACAD.CRED.CC", testCredit.Id, true)).Returns(Task.FromResult(stcccResponse));
                    academicCreditCcAllResponse.Add(stcccResponse);
                }

                // Build STUDENT.COURSE.SEC responses and mock
                // (Mentioned below in BuildValid... but bears repeating.  The SCS and STC do NOT normally share an ID!!)
                if (testCredit.Course != null)
                {
                    StudentCourseSec scsResponse = BuildValidSCSResponse(testCredit);
                    dataReaderMock.Setup<Task<StudentCourseSec>>(acc => acc.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", testCredit.Id, true)).Returns(Task.FromResult(scsResponse));
                    studentCourseSecAllResponse.Add(scsResponse);

                    // added for mobile
                    // test credit 54 simulate no STUDENT.COURSE.SEC.CC record
                    if (testCredit.Id != "54")
                    {
                        StudentCourseSecCc scsccResponse = BuildValidScsccResponse(testCredit);
                        dataReaderMock.Setup<Task<StudentCourseSecCc>>(acc => acc.ReadRecordAsync<StudentCourseSecCc>("STUDENT.COURSE.SEC.CC", testCredit.Id, true)).Returns(Task.FromResult(scsccResponse));
                        studentCourseSecCcAllResponse.Add(scsccResponse);
                    }
                    // end added for mobile
                }

                // Aggregate responses 1,2,3 for a multiple-record get
                if (testCredit.Id == "1" || testCredit.Id == "2" || testCredit.Id == "3")
                {
                    stcMultiResponse.Add(studentAcadCredResponse.First());
                }

                // Aggregate all responses for tests that want to test all credits
                academicCreditAllResponse.Add(studentAcadCredResponse.First());

            }


            // Multi-record request responses
            string[] requestedIds1 = { "1", "2", "3" };
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds1, true)).Returns(Task.FromResult(stcMultiResponse));
            string[] requestedIds2 = { "1", "InvalidId", "3" };
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds2, true)).Returns(Task.FromResult(stcEmptyResponse));
            string[] requestedIds3 = { "1", "", "3" };
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds3, true)).Returns(Task.FromResult(stcEmptyResponse));
            string[] requestedIds4 = { "1", null, "3" };
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds4, true)).Returns(Task.FromResult(stcEmptyResponse));
            string[] requestedIds5 = { "INVALID" };
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", requestedIds5, true)).Returns(Task.FromResult(stcEmptyResponse));

            // All record response
            string[] allIds = (await testAcademicCreditRepository.GetAsync()).Select(ai => ai.Id).ToArray();
            dataReaderMock.Setup<Task<Collection<StudentAcadCred>>>(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", allIds, true)).Returns(Task.FromResult(academicCreditAllResponse));
            dataReaderMock.Setup<Task<Collection<StudentAcadCredCc>>>(acc => acc.BulkReadRecordAsync<StudentAcadCredCc>("STUDENT.ACAD.CRED.CC", allIds, true)).Returns(Task.FromResult(academicCreditCcAllResponse));
            dataReaderMock.Setup<Task<Collection<StudentCourseSec>>>(acc => acc.BulkReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", allIds, true)).Returns(Task.FromResult(studentCourseSecAllResponse));
            dataReaderMock.Setup<Task<Collection<StudentCourseSecCc>>>(acc => acc.BulkReadRecordAsync<StudentCourseSecCc>("STUDENT.COURSE.SEC.CC", allIds, true)).Returns(Task.FromResult(studentCourseSecCcAllResponse));
            // Credit Types transaction mockery
            Collection<CredTypes> credTypeResponse = new Collection<CredTypes>(){ new CredTypes() { Recordkey = "IN", CrtpCategory = "I"},
                                                                                  new CredTypes() { Recordkey = "TRN", CrtpCategory = "T"},
                                                                                  new CredTypes() { Recordkey = "CE", CrtpCategory = "C"},
                                                                                  new CredTypes() {Recordkey = "OTH", CrtpCategory = "O"}};
            dataReaderMock.Setup<Task<Collection<CredTypes>>>(acc => acc.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "", true)).Returns(Task.FromResult(credTypeResponse));

            Collection<StudentEquivEvals> studentEquivEvalsColl = new Collection<StudentEquivEvals>()
            {
                new StudentEquivEvals()
                {
                    Recordkey = "1",
                    SteAcadPrograms = new List<string>() { "1", "2" },
                    SteCourseAcadCred = new List<string>() { "1" },
                    SteInstitution = "SteInst1",
                    SteStudentNonCourse = "Math 101"
                }
            };
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<StudentEquivEvals>("STUDENT.EQUIV.EVALS", It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(studentEquivEvalsColl));

            academicCreditAllResponse.FirstOrDefault(i => i.Recordkey.Equals("1", StringComparison.OrdinalIgnoreCase)).StcPersonId = "1";
            academicCreditAllResponse.FirstOrDefault(i => i.Recordkey.Equals("2", StringComparison.OrdinalIgnoreCase)).StcPersonId = "2";

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(academicCreditAllResponse);

            RecordKeyLookupResult stdEquivRecordLookupResult = new RecordKeyLookupResult() { Guid = "71eebbae-eff5-43e9-9f66-a267258812d3", ModelName = "STUDENT.EQUIV.EVALS" };
            RecordKeyLookupResult aadLevelRecordLookupResult = new RecordKeyLookupResult() { Guid = "61eebbae-eff5-43e9-9f66-a267258812d4", ModelName = "ACAD.LEVELS" };
            RecordKeyLookupResult personRecordLookupResult = new RecordKeyLookupResult() { Guid = "51eebbae-eff5-43e9-9f66-a267258812d5", ModelName = "PERSON" };
            RecordKeyLookupResult stcPersonId = new RecordKeyLookupResult() { Guid = "11eebbae-eff5-43e9-9f66-a267258812d1", ModelName = "PERSON" };
            RecordKeyLookupResult stcCourse = new RecordKeyLookupResult() { Guid = "21eebbae-eff5-43e9-9f66-a267258812d2", ModelName = "COURSES" };
            RecordKeyLookupResult steAcadPrograms1 = new RecordKeyLookupResult() { Guid = "31eebbae-eff5-43e9-9f66-a267258812d3", ModelName = "ACAD.PROGRAMS" };
            RecordKeyLookupResult steAcadPrograms2 = new RecordKeyLookupResult() { Guid = "41eebbae-eff5-43e9-9f66-a267258812d4", ModelName = "ACAD.PROGRAMS" };
            RecordKeyLookupResult stcTerm = new RecordKeyLookupResult() { Guid = "52eebbae-eff5-43e9-9f66-a267258812c5", ModelName = "TERMS" };
            RecordKeyLookupResult stcGradeScheme = new RecordKeyLookupResult() { Guid = "61eebbae-eff5-43e9-9f66-a267258812d6", ModelName = "GRADE.SCHEMES" };
            RecordKeyLookupResult stcVerifiedGrade = new RecordKeyLookupResult() { Guid = "71eebbae-eff5-43e9-9f66-a267258812d7", ModelName = "GRADES" };
            RecordKeyLookupResult stcCredType = new RecordKeyLookupResult() { Guid = "81eebbae-eff5-43e9-9f66-a267258812d8", ModelName = "CRED.TYPES" };


            RecordKeyLookup stdEquivRecordLookup = new RecordKeyLookup("STUDENT.EQUIV.EVALS", "1", "STE.COURSE.ACAD.CRED", "1", It.IsAny<bool>());
            RecordKeyLookup acadLevelRecordLookup = new RecordKeyLookup("ACAD.LEVELS", "UG", It.IsAny<bool>());
            RecordKeyLookup personRecordLookup = new RecordKeyLookup("PERSON", "SteInst1", It.IsAny<bool>());
            RecordKeyLookup person2RecordLookup = new RecordKeyLookup("PERSON", "1", It.IsAny<bool>());
            RecordKeyLookup courseRecordLookup = new RecordKeyLookup("COURSES", "139", It.IsAny<bool>());
            RecordKeyLookup acadProg1RecordLookup = new RecordKeyLookup("ACAD.PROGRAMS", "1", It.IsAny<bool>());
            RecordKeyLookup acadProg2RecordLookup = new RecordKeyLookup("ACAD.PROGRAMS", "2", It.IsAny<bool>());
            RecordKeyLookup termRecordLookup = new RecordKeyLookup("TERMS", "2009/SP", It.IsAny<bool>());
            RecordKeyLookup gradeSchemeRecordLookup = new RecordKeyLookup("GRADE.SCHEMES", "UG", It.IsAny<bool>());
            RecordKeyLookup verGradeRecordLookup = new RecordKeyLookup("GRADES", "A", It.IsAny<bool>());
            RecordKeyLookup cradeTypeRecordLookup = new RecordKeyLookup("CRED.TYPES", "IN", It.IsAny<bool>());


            var stdEquivRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            stdEquivRecordLookupDict.Add(stdEquivRecordLookup.ResultKey, stdEquivRecordLookupResult);

            var acadLevelRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            acadLevelRecordLookupDict.Add(acadLevelRecordLookup.ResultKey, aadLevelRecordLookupResult);

            var personRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            personRecordLookupDict.Add(personRecordLookup.ResultKey, personRecordLookupResult);

            var stcPersonRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            stcPersonRecordLookupDict.Add(person2RecordLookup.ResultKey, stcPersonId);

            var StcCourseRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            StcCourseRecordLookupDict.Add(courseRecordLookup.ResultKey, stcCourse);

            var SteAcadProgramsRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            SteAcadProgramsRecordLookupDict.Add(acadProg1RecordLookup.ResultKey, steAcadPrograms1);

            var SteAcadProgramsRecordLookupDict2 = new Dictionary<string, RecordKeyLookupResult>();
            SteAcadProgramsRecordLookupDict2.Add(acadProg2RecordLookup.ResultKey, steAcadPrograms2);

            var StcTermRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            StcTermRecordLookupDict.Add(termRecordLookup.ResultKey, stcTerm);

            var StcGradeSchemeRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            StcGradeSchemeRecordLookupDict.Add(gradeSchemeRecordLookup.ResultKey, stcGradeScheme);

            var StcVerifiedGradeRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            StcVerifiedGradeRecordLookupDict.Add(verGradeRecordLookup.ResultKey, stcVerifiedGrade);

            var StcCredTypeRecordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
            StcCredTypeRecordLookupDict.Add(cradeTypeRecordLookup.ResultKey, stcCredType);

            dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                .Returns(Task.FromResult(stdEquivRecordLookupDict))
                .Returns(Task.FromResult(acadLevelRecordLookupDict))
                .Returns(Task.FromResult(personRecordLookupDict))
                .Returns(Task.FromResult(stcPersonRecordLookupDict))
                .Returns(Task.FromResult(StcCourseRecordLookupDict))
                .Returns(Task.FromResult(SteAcadProgramsRecordLookupDict))
                .Returns(Task.FromResult(SteAcadProgramsRecordLookupDict2))
                .Returns(Task.FromResult(StcTermRecordLookupDict))
                .Returns(Task.FromResult(StcGradeSchemeRecordLookupDict))
                .Returns(Task.FromResult(StcVerifiedGradeRecordLookupDict))
                .Returns(Task.FromResult(StcCredTypeRecordLookupDict));

            // StudentAcadCredStatus mock
            ApplValcodes statusCodeResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "N", ValActionCode1AssocMember = "1" },
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "2"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "W", ValActionCode1AssocMember = "4"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValActionCode1AssocMember = "6"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "TR", ValActionCode1AssocMember = "7"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "NC", ValActionCode1AssocMember = "7"},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "PR", ValActionCode1AssocMember = "8"}
                }
            };
            dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", true)).Returns(Task.FromResult(statusCodeResponse));
            
            // Select of Student Course Sec mock
            dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.COURSE.SEC", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(allIds);            
            
            // Return mocked up repo
            AcademicCreditRepository acr = new AcademicCreditRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object, new TestCourseRepository(), new TestGradeRepository(), new TestTermRepository(), apiSettingsMock);
            return acr;

        }
        private Collection<StudentAcadCred> BuildValidStcResponse(AcademicCredit ac)
        {
            Collection<StudentAcadCred> responseData = new Collection<StudentAcadCred>();
            StudentAcadCred stc = new StudentAcadCred();

            stc.Recordkey = ac.Id;
            stc.StcAcadLevel = ac.AcademicLevelCode;
            stc.StcAllowReplFlag = ac.CanBeReplaced ? "Y" : "";
            stc.StcAltcumContribCmplCred = ac.AdjustedCredit;
            stc.StcAltcumContribGpaCred = ac.AdjustedGpaCredit;
            stc.StcAltcumContribGradePts = ac.AdjustedGradePoints;
            stc.StcCeus = ac.ContinuingEducationUnits;
            if (ac.Course != null)
            {
                stc.StcCourse = ac.Course.Id;
                stc.StcCourseLevel = ac.Course.CourseLevelCodes.First();
            }
            stc.StcCourseName = ac.CourseName;
            stc.StcCred = ac.Credit;
            string typecode;
            switch (ac.Type)
            {
                case CreditType.ContinuingEducation:
                    typecode = "CE";
                    break;
                case CreditType.Institutional:
                    typecode = "IN";
                    break;
                case CreditType.Transfer:
                    typecode = "TRN";
                    break;
                default:
                    typecode = "OTH";
                    break;
            }
            stc.StcCredType = typecode;
            // For one academic credit leave the departments blank and be sure they get defaulted from the course
            if (ac.Id == "1")
            {
                stc.StcDepts = new List<string>();
            }
            else
            {
                stc.StcDepts = ac.DepartmentCodes.ToList();
            }
            stc.StcStartDate = ac.StartDate;
            stc.StcEndDate = ac.EndDate;
            stc.StcGpaCred = ac.GpaCredit;
            stc.StcGradePts = ac.GradePoints;
            stc.StcGradeScheme = ac.GradeSchemeCode;
            stc.StcCmplCred = ac.CompletedCredit;
            stc.StcAttCred = ac.AttemptedCredit;
            stc.StcSectionNo = ac.SectionNumber;
            stc.StcStudentEquivEval = ac.CourseName;
            //stc.StcMark 
            stc.StcReplCode = ac.ReplacedStatus == ReplacedStatus.Replaced ? "R" : null;
            stc.StcRepeatedAcadCred = ac.RepeatAcademicCreditIds;

            // Status 
            string stat = "";
            switch (ac.Status)
            {
                case CreditStatus.Add: { stat = "A"; break; }
                case CreditStatus.Cancelled: { stat = "C"; break; }
                case CreditStatus.Deleted: { stat = "X"; break; }
                case CreditStatus.Dropped: { stat = "D"; break; }
                case CreditStatus.New: { stat = "N"; break; }
                case CreditStatus.Preliminary: { stat = "PR"; break; }
                case CreditStatus.TransferOrNonCourse: { stat = "TR"; break; }
                case CreditStatus.Withdrawn: { stat = "W"; break; }
                default: { stat = ""; break; }
            }

            stc.StcStatus = new List<string>() { stat };

            stc.StcStatusesEntityAssociation = new List<StudentAcadCredStcStatuses>();
            StudentAcadCredStcStatuses statusitem = new StudentAcadCredStcStatuses(
                                                            DateTime.Now,
                                                            ac.Status.ToString()[0].ToString(),
                                                            DateTime.Now,
                                                            "");
            stc.StcStatusesEntityAssociation.Add(statusitem);
            stc.StcStudentCourseSec = ac.Id; // Not real life example here.  The SCS record would have a diff-
            // erent ID in real life, but we don't keep it here, so for mocking
            // we will pretend it is the same as the STC record ID.
            stc.StcSubject = ac.SubjectCode;
            stc.StcTerm = ac.TermCode;
            //stc.StcTitle I don't think we use this
            if (ac.VerifiedGrade != null)
            {
                stc.StcVerifiedGrade = ac.VerifiedGrade.Id;
                // added for mobile
                stc.StcVerifiedGradeDate = new DateTime(ac.VerifiedGradeTimestamp.Value.Year, ac.VerifiedGradeTimestamp.Value.Month, ac.VerifiedGradeTimestamp.Value.Day, 0, 0, 0);
                // end added for mobile
            }
            responseData.Add(stc);
            return responseData;
        }

        private StudentAcadCredCc BuildValidStcccResponse(AcademicCredit ac)
        {
            StudentAcadCredCc studentAcadCredCc = new StudentAcadCredCc();
            studentAcadCredCc.Recordkey = ac.Id;
            if (ac.VerifiedGrade != null)
            {
                // mock the date portion on 12.31.1967, which we shouldn't see in the aggregate value
                // since the date comes from STC
                studentAcadCredCc.StcccVerifiedGradeTime = new DateTime(1967, 12, 31, ac.VerifiedGradeTimestamp.Value.Hour, ac.VerifiedGradeTimestamp.Value.Minute, ac.VerifiedGradeTimestamp.Value.Second);
            }
            return studentAcadCredCc;
        }

        private StudentCourseSec BuildValidSCSResponse(AcademicCredit ac)
        {
            StudentCourseSec scs = new StudentCourseSec();
            scs.Recordkey = ac.Id;
            scs.ScsCourseSection = ac.SectionId;
            switch (ac.GradingType)
            {
                case GradingType.Graded:
                    scs.ScsPassAudit = "";
                    break;
                case GradingType.PassFail:
                    scs.ScsPassAudit = "P";
                    break;
                case GradingType.Audit:
                    scs.ScsPassAudit = "A";
                    break;
                default:
                    break;
            }
            // added for mobile
            // for each midtermgrade in the list, populate the correct field 
            // Not necessarily sequential starting from 1!!! Mock the time as
            // midnight, which we won't see if we get a time from the SCSCC
            foreach (MidTermGrade mtg in ac.MidTermGrades)
            {
                switch (mtg.Position)
                {
                    case 1:
                        scs.ScsMidTermGrade1 = mtg.GradeId;
                        scs.ScsMidGradeDate1 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                    case 2:
                        scs.ScsMidTermGrade2 = mtg.GradeId;
                        scs.ScsMidGradeDate2 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                    case 3:
                        scs.ScsMidTermGrade3 = mtg.GradeId;
                        scs.ScsMidGradeDate3 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                    case 4:
                        scs.ScsMidTermGrade4 = mtg.GradeId;
                        scs.ScsMidGradeDate4 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                    case 5:
                        scs.ScsMidTermGrade5 = mtg.GradeId;
                        scs.ScsMidGradeDate5 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                    case 6:
                        scs.ScsMidTermGrade6 = mtg.GradeId;
                        scs.ScsMidGradeDate6 = new DateTime(mtg.GradeTimestamp.Value.Year, mtg.GradeTimestamp.Value.Month, mtg.GradeTimestamp.Value.Day, 0, 0, 0);
                        break;
                }
            }
            // end added for mobile
            return scs;
        }

        private StudentCourseSecCc BuildValidScsccResponse(AcademicCredit ac)
        {
            StudentCourseSecCc scscc = new StudentCourseSecCc();
            scscc.Recordkey = ac.Id;
            // mock the times on 12.31.1967 (we shouldn't ever see that date
            // in the aggregate Timestamp for a midterm grade, the date portion comes from the SCS.
            foreach (MidTermGrade mtg in ac.MidTermGrades)
            {
                switch (mtg.Position)
                {
                    case 1:
                        scscc.ScsccMidGradeTime1 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                    case 2:
                        scscc.ScsccMidGradeTime2 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                    case 3:
                        scscc.ScsccMidGradeTime3 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                    case 4:
                        scscc.ScsccMidGradeTime4 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                    case 5:
                        scscc.ScsccMidGradeTime5 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                    case 6:
                        scscc.ScsccMidGradeTime6 = new DateTime(1967, 12, 31, mtg.GradeTimestamp.Value.Hour, mtg.GradeTimestamp.Value.Minute, mtg.GradeTimestamp.Value.Second);
                        break;
                }
            }
            return scscc;
        }

        private async Task<Collection<Grades>> BuildValidGradeResponse()
        {
            TestGradeRepository testGradeRepository = new TestGradeRepository();
            Collection<Grades> grades = new Collection<Grades>();
            foreach (var gradeDomain in (await testGradeRepository.GetAsync()))
            {
                Grades grade = new Grades();
                grade.Recordkey = gradeDomain.Id;
                grade.GrdGrade = gradeDomain.LetterGrade;
                grade.GrdGradeScheme = gradeDomain.GradeSchemeCode;
                grade.GrdLegend = gradeDomain.Description;
                grade.GrdValue = gradeDomain.GradeValue;

                grades.Add(grade);
            }
            return grades;
        }

        private async Task<Dictionary<string, Transactions.SortStudentAcadCredsResponse>> BuildSortStudentAcadCredsResponses(IEnumerable<AcademicCredit> acadCredits, IEnumerable<string> sortSpecificationIds)
        {
            char _SM = Convert.ToChar(Ellucian.Dmi.Runtime.DynamicArray.SM);
            var academicCreditIds = acadCredits.Select(ac => ac.Id).Where(ac => !string.IsNullOrEmpty(ac)).Distinct().ToList();

            Dictionary<string, Transactions.SortStudentAcadCredsResponse> dictionary = new Dictionary<string, Transactions.SortStudentAcadCredsResponse>();

            var errorResponse = new Transactions.SortStudentAcadCredsResponse()
            {
                OutMessages = new List<string>() { "Sort spec ERROR-RESPONSE does not exist." },
                SortedStudentAcadCreditsBySortSpecId = new List<Transactions.SortedStudentAcadCreditsBySortSpecId>()
            };

            var nullSortedIdsResponse = new Transactions.SortStudentAcadCredsResponse()
            {
                OutMessages = new List<string>(),
                SortedStudentAcadCreditsBySortSpecId = null
            };

            var emptySortedIdsResponse = new Transactions.SortStudentAcadCredsResponse()
            {
                OutMessages = new List<string>(),
                SortedStudentAcadCreditsBySortSpecId = new List<Transactions.SortedStudentAcadCreditsBySortSpecId>()
            };

            var validResponse = new Transactions.SortStudentAcadCredsResponse()
            {
                OutMessages = new List<string>(),
                SortedStudentAcadCreditsBySortSpecId = new List<Transactions.SortedStudentAcadCreditsBySortSpecId>()
            };
            if (sortSpecificationIds != null)
            {
                foreach(var id in sortSpecificationIds)
                {
                    validResponse.SortedStudentAcadCreditsBySortSpecId.Add(new Transactions.SortedStudentAcadCreditsBySortSpecId()
                    {
                        OutDaSortSpecsIds = id,
                        OutSortedStudentAcadCredIds = String.Join(_SM.ToString(), academicCreditIds)
                    });
                }
            }

            var duplicatesResponse = new Transactions.SortStudentAcadCredsResponse()
            {
                OutMessages = null,
                SortedStudentAcadCreditsBySortSpecId = new List<Transactions.SortedStudentAcadCreditsBySortSpecId>()
            };
            if (sortSpecificationIds != null)
            {
                foreach(var id in sortSpecificationIds)
                {
                    duplicatesResponse.SortedStudentAcadCreditsBySortSpecId.Add(new Transactions.SortedStudentAcadCreditsBySortSpecId()
                    {
                        OutDaSortSpecsIds = id,
                        OutSortedStudentAcadCredIds = String.Join(_SM.ToString(), academicCreditIds)
                    });
                }
                duplicatesResponse.SortedStudentAcadCreditsBySortSpecId.Add(new Transactions.SortedStudentAcadCreditsBySortSpecId()
                {
                    OutDaSortSpecsIds = sortSpecificationIds.ToList()[0],
                    OutSortedStudentAcadCredIds = String.Join(_SM.ToString(), academicCreditIds)
                });
            }

            dictionary.Add("null", null);
            dictionary.Add("error", errorResponse);
            dictionary.Add("nullSortedIds", nullSortedIdsResponse);
            dictionary.Add("emptySortedIds", emptySortedIdsResponse);
            dictionary.Add("valid", validResponse);
            dictionary.Add("validDuplicates", duplicatesResponse);

            return dictionary;
        }
    }
}