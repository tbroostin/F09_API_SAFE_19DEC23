// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Planning.Tests.Repositories
{
    [TestClass]
    public class SampleDegreePlanRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ObjectCache> localCacheMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        SampleDegreePlan sample;
        CurriculumTracks currTrackResponseData;
        Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks> courseBlockResponseData;

        SampleDegreePlanRepository currTrackRepo;

        [TestInitialize]
        public async void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            // Build curriculum track repository
            currTrackRepo = BuildValidCurrTrackRepository();

            // Build Curriculum Track response used for mocking
            sample = await new TestSampleDegreePlanRepository().GetAsync("TRACK1");
            // Build standard repository response items
            currTrackResponseData = BuildCurriculumTracksResponse(sample);
            courseBlockResponseData = BuildCourseBlocksResponse(sample);
            // Set up standard repository response
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).Returns(Task.FromResult(currTrackResponseData));
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>("COURSE.BLOCKS", It.Is<string[]>(s => s.Contains("1")), true)).Returns(Task.FromResult(courseBlockResponseData));

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                       x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                       .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            currTrackResponseData = null;
            courseBlockResponseData = null;
            currTrackRepo = null;
        }

        [TestMethod]
        public async Task CurriculumTrack_Code()
        {
            var currTrack = await currTrackRepo.GetAsync("TRACK1");
            Assert.AreEqual("TRACK1", currTrack.Code);
        }

        [TestMethod]
        public async Task CurriculumTrack_Description()
        {
            var currTrack = await currTrackRepo.GetAsync("TRACK1");
            Assert.AreEqual("BA MATH 2009", currTrack.Description);
        }

        [TestMethod]
        public async Task CurriculumTrack_CurriculumBlocks_Description()
        {
            var currTrack = await currTrackRepo.GetAsync("TRACK1");
            Assert.AreEqual("block 1", currTrack.CourseBlocks.ElementAt(0).Description);
            Assert.AreEqual("block 2", currTrack.CourseBlocks.ElementAt(1).Description);
        }

        [TestMethod]
        public async Task CurriculumTrack_CurriculumBlocks_CourseIds()
        {
            var currTrack = await currTrackRepo.GetAsync("TRACK1");
            Assert.AreEqual("139", currTrack.CourseBlocks.ElementAt(0).CourseIds.ElementAt(0));
            Assert.AreEqual("142", currTrack.CourseBlocks.ElementAt(0).CourseIds.ElementAt(1));
            Assert.AreEqual("110", currTrack.CourseBlocks.ElementAt(1).CourseIds.ElementAt(0));
            Assert.AreEqual("21", currTrack.CourseBlocks.ElementAt(1).CourseIds.ElementAt(1));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InvalidRepositoryThrowsError()
        {
            currTrackRepo = BuildInvalidCurrTrackRepository();
            var currTrack = await currTrackRepo.GetAsync("TRACK2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ThrowsErrorIfRequestedCurrTrackDoesNotExistRequestReturnsNull()
        {
            var response = new CurriculumTracks();
            response = null;
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).ReturnsAsync(response);
            var currTrack = await currTrackRepo.GetAsync("TRACK3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ThrowsErrorIfRequestedCurrTrackDoesNotExistRequestReturnsEmptyObject()
        {
            var response = new CurriculumTracks();
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).ReturnsAsync(response);
            var currTrack = await currTrackRepo.GetAsync("TRACK3");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ThrowsErrorIfNoCourseBlocks()
        {
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).ReturnsAsync(currTrackResponseData);
            courseBlockResponseData = null;
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>("COURSE.BLOCKS", It.Is<string[]>(s => s.Contains("1")), true)).ReturnsAsync(courseBlockResponseData);
            var currTrack = await currTrackRepo.GetAsync("TRACK1");
        }

        [TestMethod]
        public async Task Get_WritesToCache()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "false" to indicate item is not in cache
            //  -to cache "Get" request, return null so we know it's getting data from "repository"
            var recordKey = "TRACK1";
            string cacheKey = currTrackRepo.BuildFullCacheKey("SampleDegreePlan*" + recordKey);
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).ReturnsAsync(currTrackResponseData);
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>("COURSE.BLOCKS", It.Is<string[]>(s => s.Contains("1")), true)).ReturnsAsync(courseBlockResponseData);

            // But after data accessor read, set up mocking so we can verify the list of courses was written to the cache
            cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<SampleDegreePlan>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            // Verify that curriculum track was returned
            var currTrack = await currTrackRepo.GetAsync(recordKey);
            Assert.AreEqual(recordKey, currTrack.Code);

            // Verify that the curr track item was added to the cache after it was read from the repository
            cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, It.IsAny<SampleDegreePlan>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
        }

        [TestMethod]
        public async Task Get_All_GetsCachedItem()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item 
            var recordKey = "TRACK1";
            string cacheKey = currTrackRepo.BuildFullCacheKey("SampleDegreePlan*" + recordKey);
            sample = await new TestSampleDegreePlanRepository().GetAsync(recordKey);
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(sample).Verifiable();

            // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            dataAccessorMock.Setup<CurriculumTracks>(acc => acc.ReadRecord<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).Returns(new CurriculumTracks());
            dataAccessorMock.Setup<Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>>(acc => acc.BulkReadRecord<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>("COURSE.BLOCKS", It.Is<string[]>(s => s.Contains("1")), true)).Returns(new Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>());

            // Assert that proper course was returned
            var curriculumTrack = await currTrackRepo.GetAsync(recordKey);
            Assert.AreEqual(recordKey, curriculumTrack.Code);

            // Verify that Get was called to get the curriculum track from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenProgramCodeIsNull_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync(null, "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenProgramCodeIsEmpty_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync(string.Empty, "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenCatalogCodeIsNull_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", null);
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenCatalogCodeIsEmpty_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", string.Empty);
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateNullEndDateNull_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = null;
            track4ResponseData.CtkEndDate = null;
            track5ResponseData.CtkStartDate = null;
            track5ResponseData.CtkEndDate = null;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateNullEndDatePast_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = null;
            track4ResponseData.CtkEndDate = new DateTime(2019, 12, 31);
            track5ResponseData.CtkStartDate = null;
            track5ResponseData.CtkEndDate = new DateTime(2021, 08, 23);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateNullEndDateCurrent_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = null;
            track4ResponseData.CtkEndDate = DateTime.Today;
            track5ResponseData.CtkStartDate = null;
            track5ResponseData.CtkEndDate = DateTime.Today;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateNullEndDateFuture_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = null;
            track4ResponseData.CtkEndDate = DateTime.Today.AddYears(2);
            track5ResponseData.CtkStartDate = null;
            track5ResponseData.CtkEndDate = DateTime.Today.AddYears(10);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDatePastEndDateNull_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddYears(-2);
            track4ResponseData.CtkEndDate = null;
            track5ResponseData.CtkStartDate = DateTime.Today.AddDays(-1);
            track5ResponseData.CtkEndDate = null;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDatePastEndDatePast_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddYears(-5);
            track4ResponseData.CtkEndDate = DateTime.Today.AddYears(-2);
            track5ResponseData.CtkStartDate = DateTime.Today.AddYears(-5);
            track5ResponseData.CtkEndDate = DateTime.Today.AddDays(-1);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDatePastEndDateCurrent_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddYears(-2);
            track4ResponseData.CtkEndDate = DateTime.Today;
            track5ResponseData.CtkStartDate = DateTime.Today.AddDays(-1);
            track5ResponseData.CtkEndDate = DateTime.Today;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDatePastEndDateFuture_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddYears(-2);
            track4ResponseData.CtkEndDate = DateTime.Today.AddDays(1);
            track5ResponseData.CtkStartDate = DateTime.Today.AddDays(-1);
            track5ResponseData.CtkEndDate = DateTime.Today.AddYears(10);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateCurrentEndDateNull_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today;
            track4ResponseData.CtkEndDate = null;
            track5ResponseData.CtkStartDate = DateTime.Today;
            track5ResponseData.CtkEndDate = null;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateCurrentEndDateFuture_ReturnsCurriculumTracks()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today;
            track4ResponseData.CtkEndDate = DateTime.Today.AddDays(1);
            track5ResponseData.CtkStartDate = DateTime.Today;
            track5ResponseData.CtkEndDate = DateTime.Today.AddYears(10);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateFutureEndDateNull_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddDays(1);
            track4ResponseData.CtkEndDate = null;
            track5ResponseData.CtkStartDate = DateTime.Today.AddYears(1);
            track5ResponseData.CtkEndDate = null;

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenStartDateFutureEndDateFuture_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            track4ResponseData.CtkStartDate = DateTime.Today.AddDays(1);
            track4ResponseData.CtkEndDate = DateTime.Today.AddYears(3);
            track5ResponseData.CtkStartDate = DateTime.Today.AddYears(1);
            track5ResponseData.CtkEndDate = DateTime.Today.AddYears(10);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenAcprCurriculumTrackIdsIsNull_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);
            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = null,
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenAcprCurriculumTrackIdsIsEmpty_ReturnsEmptyCollection()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>(),
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenCurriculumTracksNull_ReturnsEmptyCollection()
        {
            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            Collection<CurriculumTracks> curriculumTracksResponseData = null;

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WhenCurriculumTracksEmpty_ReturnsEmptyCollection()
        {
            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            Collection<CurriculumTracks> curriculumTracksResponseData = new Collection<CurriculumTracks>();

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", "MUSC.BA*2019", true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(0, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_ReturnsValidResponse()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);


            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", It.IsAny<string>(), true)).ReturnsAsync(acadProgramReqmtsResponseData);
            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = null, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        [TestMethod]
        public async Task GetProgramCatalogCurriculumTracksAsync_WithInvalidKeys_ReturnsValidResponse()
        {
            var track4 = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
            var track5 = await new TestSampleDegreePlanRepository().GetAsync("TRACK5");

            var track4ResponseData = BuildCurriculumTracksResponse(track4);
            var track5ResponseData = BuildCurriculumTracksResponse(track5);

            var acadProgramReqmtsResponseData = new AcadProgramReqmts()
            {
                Recordkey = "MUSC.BA*2019",
                AcprMaxCred = 120m,
                AcprCred = 100m,
                AcprCurriculumTrackIds = new List<string>()
                {
                    "TRACK4",
                    "TRACK5",
                    "TRACK6",
                    "TRACK7",
                },
            };

            var curriculumTracksResponseData = new Collection<CurriculumTracks>();
            curriculumTracksResponseData.Add(track4ResponseData);
            curriculumTracksResponseData.Add(track5ResponseData);

            // return response to data accessor request.
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", It.IsAny<string>(), true)).ReturnsAsync(acadProgramReqmtsResponseData);

            dataAccessorMock.Setup<Task<BulkReadOutput<CurriculumTracks>>>(acc => acc.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string[]>(), true))
                .ReturnsAsync(new BulkReadOutput<CurriculumTracks>() { BulkRecordsRead = curriculumTracksResponseData, InvalidKeys = new string[] { "TRACK6", "TRACK7" }, InvalidRecords = null });

            var tracks = await currTrackRepo.GetProgramCatalogCurriculumTracksAsync("MUSC.BA", "2019");
            Assert.AreEqual(2, tracks.Count());
        }

        private SampleDegreePlanRepository BuildValidCurrTrackRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Mock
            localCacheMock = new Mock<ObjectCache>();
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();
            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
            // Construct course repository
            currTrackRepo = new SampleDegreePlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return currTrackRepo;
        }

        private SampleDegreePlanRepository BuildInvalidCurrTrackRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up repo response for "all" courses requests
            Exception expectedFailure = new Exception("fail");
            dataAccessorMock.Setup<Task<CurriculumTracks>>(acc => acc.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", It.IsAny<string>(), true)).Throws(expectedFailure);

            // Cache Mock
            var localCacheMock = new Mock<ObjectCache>();
            // Cache Provider Mock
            var cacheProviderMock = new Mock<ICacheProvider>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Construct course repository
            currTrackRepo = new SampleDegreePlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                       x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                       .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            return currTrackRepo;
        }

        private CurriculumTracks BuildCurriculumTracksResponse(SampleDegreePlan currTrack)
        {
            var courseBlocksId = 0;
            var response = new CurriculumTracks();
            response.Recordkey = currTrack.Code;
            response.CtkDesc = currTrack.Description;
            var assoc = new List<CurriculumTracksCtkAcadPgmReqmts>();
            response.CtkAcadPgmReqmtsEntityAssociation = assoc;
            response.CtkCourseBlocks = new List<string>();
            foreach (var block in currTrack.CourseBlocks)
            {
                response.CtkCourseBlocks.Add(courseBlocksId++.ToString());
            }
            response.CtkStartDate = DateTime.Today.AddDays(-10);
            return response;
        }

        private Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks> BuildCourseBlocksResponse(SampleDegreePlan currTrack)
        {
            var courseBlockResponse = new Collection<Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks>();
            var courseBlocksId = 0;
            foreach (var blk in currTrack.CourseBlocks)
            {
                var courseBlk = new Ellucian.Colleague.Data.Student.DataContracts.CourseBlocks();
                courseBlk.Recordkey = courseBlocksId++.ToString();
                courseBlk.CblDesc = blk.Description;
                courseBlk.CblCourses = blk.CourseIds.ToList();
                courseBlk.CblStartDate = DateTime.Today.AddDays(-30);
                courseBlk.CblCoursePlaceholders = blk.CoursePlaceholderIds.ToList();
                courseBlockResponse.Add(courseBlk);
            }

            return courseBlockResponse;
        }
    }
}
