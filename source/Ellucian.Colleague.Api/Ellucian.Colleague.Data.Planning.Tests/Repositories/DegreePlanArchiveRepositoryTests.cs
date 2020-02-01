// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Planning.DataContracts;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Domain.Planning.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Data.Planning.Transactions;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Tests.Repositories
{
    [TestClass]
    public class DegreePlanArchiveRepositoryTests
    {
        /// <summary>
        /// Tests for getting information for a single degree plan archive item.
        /// </summary>
        [TestClass]
        public class GetDegreePlanArchivesTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            DegreePlanArchiveRepository degreePlanArchiveRepo;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive> testDegreePlanArchiveEntities;

            [TestInitialize]
            public async void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");
                degreePlanArchiveRepo = await BuildValidDegreePlanRepositoryAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanArchiveRepo = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                transFactoryMock = null;
            }

            private async Task<DegreePlanArchiveRepository> BuildValidDegreePlanRepositoryAsync()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup repo response for getting archives for a particular degree plan - PLAN 2
                testDegreePlanArchiveEntities = await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(2);

                var degreePlanArchiveResponse = BuildDegreePlanArchiveResponse(testDegreePlanArchiveEntities);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Planning.DataContracts.DegreePlanArchive>(It.IsAny<string>(), true)).Returns(Task.FromResult(degreePlanArchiveResponse));

                // Setup repo response for getting archive comments for degree plan archive  - This will return 2 comments that will be on archive 2.
                var degreePlanCommentArchiveResponse = BuildDegreePlanArchiveCommentResponse(testDegreePlanArchiveEntities);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Planning.DataContracts.DegreePlanCommentArchv>(It.IsAny<string>(), It.IsAny<string[]>(), true)).Returns(Task.FromResult(degreePlanCommentArchiveResponse));

                degreePlanArchiveRepo = new DegreePlanArchiveRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return degreePlanArchiveRepo;
            }

            //TODO
            [TestMethod]
            public void DegreePlanArchiveRepo_GetArchives()
            {
                IEnumerable<Domain.Planning.Entities.DegreePlanArchive> archives = degreePlanArchiveRepo.GetDegreePlanArchivesAsync(2).Result;
                Assert.AreEqual(2, archives.Count());
            }

            //TODO
            [TestMethod]
            public void DegreePlanArchiveRepo_VerifyArchive()
            {
                IEnumerable<Domain.Planning.Entities.DegreePlanArchive> archives = degreePlanArchiveRepo.GetDegreePlanArchivesAsync(2).Result;
                foreach (var testEntity in testDegreePlanArchiveEntities)
                {
                    var retrieved = archives.Where(a => a.Id == testEntity.Id).First();
                    Assert.AreEqual(testEntity.Id, retrieved.Id);
                    Assert.AreEqual(testEntity.StudentId, retrieved.StudentId);
                    Assert.AreEqual(testEntity.DegreePlanId, retrieved.DegreePlanId);
                    Assert.AreEqual(testEntity.Version, retrieved.Version);
                    Assert.AreEqual(testEntity.ReviewedBy, retrieved.ReviewedBy);
                    Assert.AreEqual(testEntity.ReviewedDate, retrieved.ReviewedDate);
                }
            }

            //TODO
            [TestMethod]
            public void DegreePlanArchiveRepo_VerifyArchiveCourses()
            {
                IEnumerable<Domain.Planning.Entities.DegreePlanArchive> archives = degreePlanArchiveRepo.GetDegreePlanArchivesAsync(2).Result;
                foreach (var testEntity in testDegreePlanArchiveEntities)
                {
                    var retrieved = archives.Where(a => a.Id == testEntity.Id).First();
                    // Archive #1 has zero courses. Archive #2 has more courses.
                    if (testEntity.Id == 1)
                    {
                        Assert.AreEqual(0, retrieved.ArchivedCourses.Count());
                    }
                    else
                    {
                        Assert.IsTrue(retrieved.ArchivedCourses.Count() > 0);
                    }
                    Assert.AreEqual(testEntity.ArchivedCourses.Count(), retrieved.ArchivedCourses.Count());
                    for (int i = 0; i < testEntity.ArchivedCourses.Count(); i++)
                    {
                        var testArchivedCourse = testEntity.ArchivedCourses.ElementAt(i);
                        var repoArchivedCourse = retrieved.ArchivedCourses.ElementAt(i);
                        Assert.AreEqual(testArchivedCourse.AddedBy, repoArchivedCourse.AddedBy);
                        Assert.AreEqual(testArchivedCourse.AddedOn, repoArchivedCourse.AddedOn);
                        Assert.AreEqual(testArchivedCourse.ApprovalDate, repoArchivedCourse.ApprovalDate);
                        Assert.AreEqual(testArchivedCourse.ApprovalStatus, repoArchivedCourse.ApprovalStatus);
                        Assert.AreEqual(testArchivedCourse.ApprovedBy, repoArchivedCourse.ApprovedBy);
                        Assert.AreEqual(testArchivedCourse.ContinuingEducationUnits, repoArchivedCourse.ContinuingEducationUnits);
                        Assert.AreEqual(testArchivedCourse.CourseId, repoArchivedCourse.CourseId);
                        Assert.AreEqual(testArchivedCourse.Credits, repoArchivedCourse.Credits);
                        Assert.AreEqual(testArchivedCourse.IsPlanned, repoArchivedCourse.IsPlanned);
                        Assert.AreEqual(testArchivedCourse.HasWithdrawGrade, repoArchivedCourse.HasWithdrawGrade);
                        Assert.AreEqual(testArchivedCourse.Name, repoArchivedCourse.Name);
                        Assert.AreEqual(testArchivedCourse.RegistrationStatus, repoArchivedCourse.RegistrationStatus);
                        Assert.AreEqual(testArchivedCourse.SectionId, repoArchivedCourse.SectionId);
                        Assert.AreEqual(testArchivedCourse.TermCode, repoArchivedCourse.TermCode);
                        Assert.AreEqual(testArchivedCourse.Title, repoArchivedCourse.Title);
                    }
                }
            }
        }

        [TestClass]
        public class DegreePlanArchiveAddTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            DegreePlanArchiveRepository degreePlanArchiveRepo;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            CreateDegreePlanArchiveRequest createRequest;

            [TestInitialize]
            public async void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");
                degreePlanArchiveRepo = await BuildValidDegreePlanRepositoryAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanArchiveRepo = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                transFactoryMock = null;
            }

            private async Task<DegreePlanArchiveRepository> BuildValidDegreePlanRepositoryAsync()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up transaction manager for mocking (needed for add)
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Setup repo response for getting a single archive by Id - Just return the first one.
                var archive3 = await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(3);
                var degreePlanArchiveResponse1 = BuildDegreePlanArchiveResponse(archive3).First();
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlanArchive>("DEGREE_PLAN_ARCHIVE", "15", true)).Returns(Task.FromResult(degreePlanArchiveResponse1));

                // Setup repo response for getting archive comments/notes for the above archived plan.
                var degreePlanArchiveCommentResponse3 = BuildDegreePlanArchiveCommentResponse(archive3);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanCommentArchv>("WITH DPCARCHV.DEGREE.PLAN.ARCHIVE EQ '15'", true)).Returns(Task.FromResult(degreePlanArchiveCommentResponse3));

                // Use Callback to Capture Request built during a successful Add 
                Transactions.CreateDegreePlanArchiveResponse createSuccessfulResponse = new Transactions.CreateDegreePlanArchiveResponse() { ADegreePlanArchiveId = "15", AErrorMessage = null };
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateDegreePlanArchiveRequest, CreateDegreePlanArchiveResponse>(It.Is<CreateDegreePlanArchiveRequest>(r => r.ADegreePlanId == "2"))).Returns(Task.FromResult(createSuccessfulResponse)).Callback<CreateDegreePlanArchiveRequest>(req => createRequest = req);

                // Use Callback to Capture Request built during an unsuccessful Add 
                Transactions.CreateDegreePlanArchiveResponse createFailedResponse = new Transactions.CreateDegreePlanArchiveResponse() { ADegreePlanArchiveId = "", AErrorMessage = "Failed" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateDegreePlanArchiveRequest, CreateDegreePlanArchiveResponse>(It.Is<CreateDegreePlanArchiveRequest>(r => r.ADegreePlanId == "3"))).Returns(Task.FromResult(createFailedResponse)).Callback<CreateDegreePlanArchiveRequest>(req => createRequest = req);

                degreePlanArchiveRepo = new DegreePlanArchiveRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return degreePlanArchiveRepo;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Add_DegreePlanArchiveNoArchiveThrowsException()
            {
                await degreePlanArchiveRepo.AddAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Add_DegreePlanArchiveFailsThrowsException()
            {
                var archive = (await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(3)).First();
                await degreePlanArchiveRepo.AddAsync(archive);
            }

            [TestMethod]
            public async Task Add_DegreePlanArchiveSuccess()
            {
                var archive = (await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(2)).First();
                await degreePlanArchiveRepo.AddAsync(archive);
            }

            [TestMethod]
            public async Task Add_DegreePlanArchiveRequestBuiltCorrectly()
            {
                // Returns the second test archive, which has courses in it.
                var archive = (await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(2)).ElementAt(1);
                await degreePlanArchiveRepo.AddAsync(archive);
                Assert.IsNotNull(createRequest);

                // Verify all the fields in the request are correctly built from the archive item
                Assert.AreEqual(archive.ArchivedCourses.Count(), createRequest.AlCourses.Count());
                for (int i = 0; i < archive.ArchivedCourses.Count(); i++)
                {
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).AddedBy, createRequest.AlCourses.ElementAt(i).AlCrsAddedBy);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).AddedOn,
                        createRequest.AlCourses.ElementAt(i).AlCrsAddedTime.ToPointInTimeDateTimeOffset(createRequest.AlCourses.ElementAt(i).AlCrsAddedTime, apiSettingsMock.ColleagueTimeZone));
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).ApprovedBy, createRequest.AlCourses.ElementAt(i).AlCrsStatusBy);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).ApprovalDate,
                        createRequest.AlCourses.ElementAt(i).AlCrsStatusTime.ToPointInTimeDateTimeOffset(createRequest.AlCourses.ElementAt(i).AlCrsStatusDate, apiSettingsMock.ColleagueTimeZone));
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).ContinuingEducationUnits, createRequest.AlCourses.ElementAt(i).AlCrsCeus);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).CourseId, createRequest.AlCourses.ElementAt(i).AlCourseId);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).Credits, createRequest.AlCourses.ElementAt(i).AlCredits);
                    Assert.IsTrue(archive.ArchivedCourses.ElementAt(i).IsPlanned == true ? createRequest.AlCourses.ElementAt(i).AlCrsIsPlanned == "Y" : createRequest.AlCourses.ElementAt(i).AlCrsIsPlanned == "N");
                    Assert.IsTrue(archive.ArchivedCourses.ElementAt(i).HasWithdrawGrade == true ? createRequest.AlCourses.ElementAt(i).AlCrsHasWithdrawGrd == "Y" : createRequest.AlCourses.ElementAt(i).AlCrsHasWithdrawGrd == "N");
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).RegistrationStatus, createRequest.AlCourses.ElementAt(i).AlCrsStcStatus);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).SectionId, createRequest.AlCourses.ElementAt(i).AlSectionId);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).TermCode, createRequest.AlCourses.ElementAt(i).AlTermId);
                    Assert.AreEqual(archive.ArchivedCourses.ElementAt(i).Title, createRequest.AlCourses.ElementAt(i).AlTitle);
                }
                Assert.AreEqual(archive.DegreePlanId.ToString(), createRequest.ADegreePlanId);
                Assert.AreEqual(archive.Notes.Count(), createRequest.AlComments.Count());
                for (int i = 0; i < archive.Notes.Count(); i++)
                {
                    Assert.AreEqual(archive.Notes.ElementAt(i).Date,
                        createRequest.AlComments.ElementAt(i).AlCommentAddedTime.ToPointInTimeDateTimeOffset(createRequest.AlComments.ElementAt(i).AlCommentAddedDate, apiSettingsMock.ColleagueTimeZone));
                    Assert.AreEqual(archive.Notes.ElementAt(i).Text, createRequest.AlComments.ElementAt(i).AlCommentText);
                    Assert.AreEqual(archive.Notes.ElementAt(i).PersonId, createRequest.AlComments.ElementAt(i).AlCommentAddedBy);
                }
                Assert.AreEqual(archive.ReviewedBy, createRequest.ADpLastReviewedBy);
                Assert.AreEqual(archive.ReviewedDate,
                    createRequest.ADpLastReviewedDate.ToPointInTimeDateTimeOffset(createRequest.ADpLastReviewedDate, apiSettingsMock.ColleagueTimeZone));
                Assert.AreEqual(archive.StudentId, createRequest.ADpStudentId);
                Assert.AreEqual(archive.StudentPrograms.Count(), createRequest.AlAcadPrograms.Count());
                for (int i = 0; i < archive.StudentPrograms.Count(); i++)
                {
                    Assert.AreEqual(archive.StudentPrograms.ElementAt(i).CatalogCode, createRequest.AlAcadPrograms.ElementAt(i).AlCatalog);
                    Assert.AreEqual(archive.StudentPrograms.ElementAt(i).ProgramCode, createRequest.AlAcadPrograms.ElementAt(i).AlProgram);
                }
                Assert.AreEqual(archive.Version.ToString(), createRequest.ADpVersionNumber);
            }
        }

        /// <summary>
        /// Tests for getting information for a single degree plan archive item.
        /// </summary>
        [TestClass]
        public class GetDegreePlanArchiveTests
        {
            Mock<IColleagueDataReader> dataAccessorMock;
            DegreePlanArchiveRepository degreePlanArchiveRepo;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;
            Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive degreePlanArchive2;
            Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive degreePlanArchive1;

            [TestInitialize]
            public async void Initialize()
            {
                // Build degree plan repository
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                apiSettingsMock = new ApiSettings("null");
                degreePlanArchiveRepo = await BuildValidDegreePlanRepositoryAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanArchiveRepo = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                transFactoryMock = null;
            }

            private async Task<DegreePlanArchiveRepository> BuildValidDegreePlanRepositoryAsync()
            {
                // Set up data accessor for mocking (needed for get)
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup repo response for getting degree plan archive for 2 different archive items.

                // First for degree plan archive 2 - which has courses and notes.
                var degreePlanArchives = await new TestDegreePlanArchiveRepository().GetDegreePlanArchivesAsync(2);
                var testArchives = degreePlanArchives.Where(da => da.Id == 2);
                degreePlanArchive2 = testArchives.First();
                
                var degreePlanArchiveResponse2 = BuildDegreePlanArchiveResponse(testArchives).First();
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlanArchive>("DEGREE_PLAN_ARCHIVE", "2", true)).Returns(Task.FromResult(degreePlanArchiveResponse2));

                var degreePlanArchiveCommentResponse2 = BuildDegreePlanArchiveCommentResponse(testArchives);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanCommentArchv>("WITH DPCARCHV.DEGREE.PLAN.ARCHIVE EQ '2'", true)).Returns(Task.FromResult(degreePlanArchiveCommentResponse2));
                
                // Now for degree plan archive 1 - which has no courses and no notes.
                testArchives = degreePlanArchives.Where(da => da.Id == 1);
                degreePlanArchive1 = testArchives.First();

                var degreePlanArchiveResponse1 = BuildDegreePlanArchiveResponse(testArchives).First();
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.DegreePlanArchive>("DEGREE_PLAN_ARCHIVE", "1", true)).Returns(Task.FromResult(degreePlanArchiveResponse1));
                
                var degreePlanArchiveCommentResponse1 = BuildDegreePlanArchiveCommentResponse(testArchives);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.DegreePlanCommentArchv>("WITH DPCARCHV.DEGREE.PLAN.ARCHIVE EQ '1'", true)).Returns(Task.FromResult(degreePlanArchiveCommentResponse1));

                degreePlanArchiveRepo = new DegreePlanArchiveRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return degreePlanArchiveRepo;
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifySimpleProperties()
            {
                Domain.Planning.Entities.DegreePlanArchive archive2 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(2);
                Assert.AreEqual(degreePlanArchive2.CreatedBy, archive2.CreatedBy);
                Assert.AreEqual(degreePlanArchive2.CreatedDate, archive2.CreatedDate);
                Assert.AreEqual(degreePlanArchive2.DegreePlanId, archive2.DegreePlanId);
                Assert.AreEqual(degreePlanArchive2.Id, archive2.Id);
                Assert.AreEqual(degreePlanArchive2.ReviewedBy, archive2.ReviewedBy);
                Assert.AreEqual(degreePlanArchive2.ReviewedDate, archive2.ReviewedDate);
                Assert.AreEqual(degreePlanArchive2.StudentId, archive2.StudentId);
                Assert.AreEqual(degreePlanArchive2.Version, archive2.Version);
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifyArchivedCourses()
            {
                Domain.Planning.Entities.DegreePlanArchive archive2 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(2);
                Assert.AreEqual(degreePlanArchive2.ArchivedCourses.Count(), archive2.ArchivedCourses.Count());
                for (int i = 0; i < archive2.ArchivedCourses.Count(); i++)
                {
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).AddedBy, degreePlanArchive2.ArchivedCourses.ElementAt(i).AddedBy);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).AddedOn, degreePlanArchive2.ArchivedCourses.ElementAt(i).AddedOn);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).ApprovalDate, degreePlanArchive2.ArchivedCourses.ElementAt(i).ApprovalDate);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).ApprovalStatus, degreePlanArchive2.ArchivedCourses.ElementAt(i).ApprovalStatus);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).ApprovedBy, degreePlanArchive2.ArchivedCourses.ElementAt(i).ApprovedBy);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).ContinuingEducationUnits, degreePlanArchive2.ArchivedCourses.ElementAt(i).ContinuingEducationUnits);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).CourseId, degreePlanArchive2.ArchivedCourses.ElementAt(i).CourseId);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).Credits, degreePlanArchive2.ArchivedCourses.ElementAt(i).Credits);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).HasWithdrawGrade, degreePlanArchive2.ArchivedCourses.ElementAt(i).HasWithdrawGrade);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).IsPlanned, degreePlanArchive2.ArchivedCourses.ElementAt(i).IsPlanned);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).Name, degreePlanArchive2.ArchivedCourses.ElementAt(i).Name);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).RegistrationStatus, degreePlanArchive2.ArchivedCourses.ElementAt(i).RegistrationStatus);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).SectionId, degreePlanArchive2.ArchivedCourses.ElementAt(i).SectionId);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).TermCode, degreePlanArchive2.ArchivedCourses.ElementAt(i).TermCode);
                    Assert.AreEqual(archive2.ArchivedCourses.ElementAt(i).Title, degreePlanArchive2.ArchivedCourses.ElementAt(i).Title);
                }
            }

            [TestMethod]
            public async Task GetSingleArchive_VerifyWhenNoArchivedCourses()
            {
                Domain.Planning.Entities.DegreePlanArchive archive1 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(1);
                Assert.AreEqual(0, archive1.ArchivedCourses.Count());
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifyArchivedNotes()
            {
                Domain.Planning.Entities.DegreePlanArchive archive2 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(2);
                Assert.AreEqual(degreePlanArchive2.Notes.Count(), archive2.Notes.Count());
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifyArchivedNote()
            {
                Domain.Planning.Entities.DegreePlanArchive archive2 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(2);
                var retrievedNote1 = archive2.Notes.OrderBy(n => n.Id).First();
                var expectedNote1 = degreePlanArchive2.Notes.OrderBy(n => n.Id).First();
                Assert.AreEqual(expectedNote1.Text, retrievedNote1.Text);
                Assert.AreEqual(expectedNote1.Date, retrievedNote1.Date);
                Assert.AreEqual(expectedNote1.PersonId, retrievedNote1.PersonId);
                Assert.AreEqual(expectedNote1.Id, retrievedNote1.Id);
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifyWhenNoArchivedNotes()
            {
                Domain.Planning.Entities.DegreePlanArchive archive1 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(1);
                Assert.AreEqual(0, archive1.Notes.Count());
            }

            [TestMethod]
            public async Task GetSingleDegreeArchive_VerifyStudentPrograms()
            {
                Domain.Planning.Entities.DegreePlanArchive archive2 = await degreePlanArchiveRepo.GetDegreePlanArchiveAsync(2);
                Assert.AreEqual(degreePlanArchive2.StudentPrograms.Count(), archive2.StudentPrograms.Count());
            }
        }

        // Shared methods to convert DegreePlanArchive and DegreePlanArchiveComment entities to Colleague Response objects
        public static Collection<DegreePlanArchive> BuildDegreePlanArchiveResponse(IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive> planArchives)
        {
            Collection<DegreePlanArchive> archives = new Collection<DegreePlanArchive>();
            foreach (var pa in planArchives)
            {
                DegreePlanArchive ac = new DegreePlanArchive();
                ac.Recordkey = pa.Id.ToString();
                ac.DparchvDegreePlanId = pa.DegreePlanId.ToString();
                ac.DparchvLastReviewedBy = pa.ReviewedBy;
                ac.DparchvLastReviewedDate = pa.ReviewedDate.HasValue ? pa.ReviewedDate.Value.Date : (DateTime?)null;
                ac.DparchvLastReviewedTime = pa.ReviewedDate.HasValue ? pa.ReviewedDate.Value.DateTime : (DateTime?)null;
                ac.DparchvStudentId = pa.StudentId;
                ac.DparchvVersionNumber = pa.Version.ToString();
                ac.DegreePlanArchiveAddtime = pa.CreatedDate.HasValue ? pa.CreatedDate.Value.DateTime : (DateTime?)null;
                ac.DegreePlanArchiveAdddate = pa.CreatedDate.HasValue ? pa.CreatedDate.Value.DateTime : (DateTime?)null;
                ac.DegreePlanArchiveAddopr = pa.CreatedBy;
                ac.DparchvProgramsEntityAssociation = new List<DegreePlanArchiveDparchvPrograms>();
                foreach (var sp in pa.StudentPrograms)
                {
                    var dpa = new DegreePlanArchiveDparchvPrograms();
                    dpa.DparchvPgmAcadProgramIdAssocMember = sp.ProgramCode;
                    dpa.DparchvPgmCatalogAssocMember = sp.CatalogCode;
                    ac.DparchvProgramsEntityAssociation.Add(dpa);
                }
                ac.DparchvCoursesEntityAssociation = new List<DegreePlanArchiveDparchvCourses>();
                foreach (var course in pa.ArchivedCourses)
                {
                    var dc = new DegreePlanArchiveDparchvCourses();
                    dc.DparchvCrsCourseIdAssocMember = course.CourseId;
                    dc.DparchvCrsCreditsAssocMember = course.Credits;
                    dc.DparchvCrsNameAssocMember = course.Name;
                    dc.DparchvCrsSectionIdAssocMember = course.SectionId;
                    dc.DparchvCrsStatusByAssocMember = course.ApprovedBy;
                    dc.DparchvCrsStatusDateAssocMember = course.ApprovalDate.HasValue ? course.ApprovalDate.Value.DateTime : (DateTime?)null;
                    dc.DparchvCrsStatusTimeAssocMember = course.ApprovalDate.HasValue ? course.ApprovalDate.Value.DateTime : (DateTime?)null;
                    dc.DparchvCrsTermIdAssocMember = course.TermCode;
                    dc.DparchvCrsTitleAssocMember = course.Title;
                    dc.DparchvCrsApprovalStatusAssocMember = course.ApprovalStatus;
                    dc.DparchvCrsCeusAssocMember = course.ContinuingEducationUnits;
                    dc.DparchvCrsAddedByAssocMember = course.AddedBy;
                    dc.DparchvCrsAddedOnDateAssocMember = course.AddedOn.HasValue ? course.AddedOn.Value.DateTime : (DateTime?)null;
                    dc.DparchvCrsAddedOnTimeAssocMember = course.AddedOn.HasValue ? course.AddedOn.Value.DateTime : (DateTime?)null;
                    dc.DparchvCrsIsPlannedAssocMember = course.IsPlanned ? "Y" : "N";
                    dc.DparchvCrsHasWithdrawGrdAssocMember = course.HasWithdrawGrade ? "Y" : "N";
                    dc.DparchvCrsStcStatusAssocMember = course.RegistrationStatus;
                    ac.DparchvCoursesEntityAssociation.Add(dc);
                }
                archives.Add(ac);
            }
            return archives;
        }

        public static Collection<DegreePlanCommentArchv> BuildDegreePlanArchiveCommentResponse(IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive> planArchives)
        {
            // Create 2 comments for degree plan archive 2.
            Collection<DegreePlanCommentArchv> archives = new Collection<DegreePlanCommentArchv>();
            foreach (var pa in planArchives)
            {
                foreach (var comment in pa.Notes)
                {
                    DegreePlanCommentArchv ac = new DegreePlanCommentArchv();
                    ac.DpcarchvText = comment.Text;
                    ac.DpcarchvAddedBy = comment.PersonId;
                    ac.DpcarchvAddedDate = comment.Date.HasValue ? comment.Date.Value.DateTime : (DateTime?)null;
                    ac.DpcarchvAddedTime = comment.Date.HasValue ? comment.Date.Value.DateTime : (DateTime?)null;
                    ac.Recordkey = comment.Id.ToString();
                    ac.DpcarchvDegreePlanArchive = "2";
                    archives.Add(ac);
                }
            }
            return archives;
        }
    }
}
