// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class ProgramRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<ObjectCache> localCacheMock;
        Mock<ILogger> loggerMock;
        Collection<AcadPrograms> progsResponseData;
        Collection<TranscriptGroupings> trgrData;
        Collection<TranscriptGroupings> badTrgrData;
        List<Program> allPrograms;
        ApiSettings apiSettings;
        ProgramRepository programRepo;

        [TestInitialize]
        public async void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");
            // Enumerable list of all programs
            allPrograms = (await new TestProgramRepository().GetAsync()).ToList();
            // Collection of data accessor responses
            progsResponseData = BuildProgramsResponse(allPrograms);

            trgrData = new Collection<TranscriptGroupings>();
            trgrData.Add(new TranscriptGroupings()
            {
                Recordkey = "UG",
                TrgpAcadLevels = new List<string> { "UG" },
                TrgpCourseLevels = new List<string>(),
                TrgpCredTypes = new List<string>(),
                TrgpDepts = new List<string>(),
                TrgpSubjects = new List<string>(),
                TrgpAcadCredMarks = new List<string>(),
                TrgpInclNoGradesFlag = "Y"
            });
            trgrData.Add(new TranscriptGroupings()
            {
                Recordkey = "DA",
                TrgpAcadLevels = new List<string>(),
                TrgpCourseLevels = new List<string>(),
                TrgpCredTypes = new List<string>(),
                TrgpDepts = new List<string>(),
                TrgpSubjects = new List<string>(),
                TrgpAcadCredMarks = new List<string>(),
                TrgpInclNoGradesFlag = "N"
            });
            badTrgrData = new Collection<TranscriptGroupings>();
            badTrgrData.Add(new TranscriptGroupings()
            {
                Recordkey = "UG",
                //TrgpAcadLevels = null;
                TrgpCourseLevels = new List<string>(),
                TrgpCredTypes = new List<string>(),
                TrgpDepts = new List<string>(),
                TrgpSubjects = new List<string>(),
                TrgpAcadCredMarks = new List<string>()
            });


            programRepo = BuildValidProgramRepository();

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
            progsResponseData = null;
            allPrograms = null;
            programRepo = null;
        }

        [TestMethod]
        public async Task Get_All_Programs()
        {
            var programs = await programRepo.GetAsync();
            Assert.IsTrue(programs.Count() >= 25);
        }

        [TestMethod]
        public async Task Description()
        {
            var programs = await programRepo.GetAsync();
            foreach (var prog in programs)
            {
                Assert.AreEqual(prog.Description, allPrograms.Where(s => s.Code == prog.Code).First().Description);
            }
        }

        [TestMethod]
        public async Task Title()
        {
            var programs = await programRepo.GetAsync();
            foreach (var prog in programs)
            {
                Assert.AreEqual(prog.Title, allPrograms.Where(p => p.Code == prog.Code).First().Title);
            }
        }

        [TestMethod]
        public async Task Status()
        {
            var programs = (await programRepo.GetAsync()).Where(p => p.IsActive == true);
            Assert.IsTrue(programs.Count() >= 25);
            programs = (await programRepo.GetAsync()).Where(p => p.IsActive == false);
            Assert.IsTrue(programs.Count() == 1);
        }

        [TestMethod]
        public async Task Catalogs()
        {
            var programs = await programRepo.GetAsync();
            Assert.AreEqual("2012", programs.ElementAt(0).Catalogs.ElementAt(0));
            Assert.AreEqual(4, programs.ElementAt(0).Catalogs.Count());
        }

        [TestMethod]
        public async Task IsGraduationAllowed()
        {
            var programs = await programRepo.GetAsync();
            Assert.IsTrue(programs.Where(p => p.IsGraduationAllowed).Count() >= 23);
            Assert.IsTrue(programs.Where(p => !p.IsGraduationAllowed).Count() == 9);
        }

        [TestMethod]
        public async Task RelatedPrograms()
        {
            var programs = await programRepo.GetAsync();
            var prog = programs.Where(p => p.Code == "MATH.BS").First();
            Assert.AreEqual(2, prog.RelatedPrograms.Count());
            Assert.AreEqual("PHYS.BS", prog.RelatedPrograms.ElementAt(1));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ThrowsExceptionIfAccessReturnsException()
        {
            ProgramRepository programRepo = BuildInvalidProgramRepository();
            var allPrograms = await programRepo.GetAsync();
        }

        [TestMethod]
        public async Task Get_WritesToCache()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "false" to indicate item is not in cache
            //  -to "Get" request, null (to ensure we are not getting it from cache)
            string cacheKey = programRepo.BuildFullCacheKey("AllPrograms");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
           // cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
           //x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
           //.Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
           //    null,
           //    new SemaphoreSlim(1, 1)
           // )));
            // Set up repo response for transcript grouping
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<TranscriptGroupings>("TRANSCRIPT.GROUPINGS", "", true)).Returns(Task.FromResult(trgrData));

            // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "", true)).Returns(Task.FromResult(progsResponseData));

            // But after data accessor read, set up so we can verify that the cache "add" method was used to add programs to cache
            cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<Task<List<Program>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
            //cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(Task.FromResult(allPrograms)).Verifiable();

            // Verify that program was returned, which means it came from cache.
            var programs = await programRepo.GetAsync();
            Assert.IsTrue(programs.Count() >= 25);

            // Verify that the "AllPrograms" list was added to the cache after it was read from the repository
            cacheProviderMock.Verify(m => m.Add(cacheKey, It.IsAny<Task<List<Program>>>(), It.IsAny<CacheItemPolicy>(), null),Times.Never);
        }

        [TestMethod]
        public async Task Get_GetsCachedPrograms()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item (in this case the "AllPrograms" cache item)
            string cacheKey = programRepo.BuildFullCacheKey("AllPrograms");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(allPrograms).Verifiable();

            // return null for the data accessor request, so that if we have a result, we know it wasn't the data accessor that returned it.
            //create an empty completed task 
            var task = Task.FromResult<Collection<AcadPrograms>>(new Collection<AcadPrograms>());
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "ACPG.CURRENT.STATUS.ACTION1 NE '2'", true)).Returns(task);

            // Assert that all programs were returned
            var programs = await programRepo.GetAsync();
            Assert.IsTrue(programs.Count() > 3);
            // Verify that Get was called to get the list of programs from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }

        [TestMethod]
        public async Task Get_Single_ReturnSinglePrograms()
        {
            // for each program that exists in the test repository...
            foreach (var prog in allPrograms)
            {
                Program Program = await programRepo.GetAsync(prog.Code);
                Assert.AreEqual(Program.Code, prog.Code);
                Assert.AreEqual(Program.Title, prog.Title);
                Assert.AreEqual(Program.Description, prog.Description);
                Assert.AreEqual(Program.AcademicLevelCode, prog.AcademicLevelCode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task Get_Single_ProgramNotFoundReturnsException()
        {
            Program Program = await programRepo.GetAsync("Junk");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get_Empty_ProgramReturnsException()
        {
            Program Program = await programRepo.GetAsync("");
        }

        [TestMethod]
        public async Task Get_withBadTranscriptGroupingNoThrow()
        {
            Program GoodProgram = (await programRepo.GetAsync()).First();
            Program BadProgram = (await BuildValidProgramRepositoryWithBadTranscriptGrouping().GetAsync()).First();
            Assert.AreEqual(1, GoodProgram.CreditFilter.AcademicLevels.Count);
            Assert.AreEqual(0, BadProgram.CreditFilter.AcademicLevels.Count);
        }

        [TestMethod]
        public async Task Get_ExpiredProgramIsActiveIsFalse()
        {
            var programs = await programRepo.GetAsync();
            Assert.AreEqual(false, programs.Where(p => p.Code == "EXP.Ccd").First().IsActive);
        }

        [TestMethod]
        public async Task Get_AlmostExpiredProgramIsActiveIsTrue()
        {
            var programs = await programRepo.GetAsync();
            Assert.AreEqual(true, programs.Where(p => p.Code == "ALMOSTEXP.Ccd").First().IsActive);
        }

        [TestMethod]
        public async Task Get_IsSelectableFlag()
        {
            var programs = await programRepo.GetAsync();
            foreach (var pgm in programs)
            {
                if (pgm.Code == "SIMPLE")
                {
                    Assert.IsFalse(pgm.IsSelectable);
                }
                else
                {
                    Assert.IsTrue(pgm.IsSelectable);
                }
            }
        }

        [TestMethod]
        public async Task Get_CreditFilter_IncludeNeverGradedCredits()
        {
            var programs = await programRepo.GetAsync();
            foreach (var pgm in programs)
            {
                if (pgm.Code == "ENGL.CERT")
                {
                    Assert.IsFalse(pgm.CreditFilter.IncludeNeverGradedCredits);
                }
                else
                {
                    Assert.IsTrue(pgm.CreditFilter.IncludeNeverGradedCredits);
                }
            }
        }

        [TestMethod]
        public async Task Get_InitializesTranscriptGroupings()
        {
            var programs = await programRepo.GetAsync();
            foreach (var pgm in progsResponseData)
            {
                var repoProgram = programs.Where(p => p.Code == pgm.Recordkey).FirstOrDefault();
                Assert.IsNotNull(repoProgram);
                Assert.AreEqual(pgm.AcpgTranscriptGrouping, repoProgram.TranscriptGrouping);
                Assert.AreEqual(pgm.AcpgUnoffTransGrouping, repoProgram.UnofficialTranscriptGrouping);
            }
        }

        private ProgramRepository BuildValidProgramRepository()
        {
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            cacheProviderMock = new Mock<ICacheProvider>();
            localCacheMock = new Mock<ObjectCache>();

            // Set up data accessor for the transaction factory 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up repo response for "all" programs requests
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "", true)).Returns(Task.FromResult(progsResponseData));

            // Set up repo response for transcript grouping
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<TranscriptGroupings>("TRANSCRIPT.GROUPINGS", "", true)).Returns(Task.FromResult(trgrData));

            // Set up repo response for program statuses valcode request
            var programStatuses = BuildProgramStatusesResponse();
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "PROGRAM.STATUSES", true)).ReturnsAsync(programStatuses);

            // List of programs eligible for selection -- Removed because we are using the StuSelectFlag on programs
            //var stwebDefaults = new StwebDefaults();
            //stwebDefaults.StwebAcadPrograms = allPrograms.Where(p => p.IsSelectable == true).Select(p => p.Code).ToList();
            //dataAccessorMock.Setup<StwebDefaults>(acc => acc.ReadRecord<StwebDefaults>(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<bool>())).Returns(stwebDefaults);

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
        )));

            ProgramRepository repository = new ProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return repository;
        }

        private ProgramRepository BuildInvalidProgramRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up repo response for transcript grouping
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<TranscriptGroupings>("TRANSCRIPT.GROUPINGS", "", true)).Returns(Task.FromResult(trgrData));
            
            //Exception expectedFailure = new Exception("fail");
            dataAccessorMock.Setup<Task<Collection<AcadPrograms>>>(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "", true)).Throws(new Exception("fail"));
            //dataAccessorMock.Setup<Task<Collection<AcadPrograms>>>(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "", true)).Throws(expectedFailure);


            //dataAccessorMock.Setup<StudentPetitions>(acc => acc.ReadRecord<StudentPetitions>(It.IsAny<string>(), true)).Throws(new Exception());
            //var sectionPermissions = sectionPermissionRepository.Get(studentPetitionId, "JUNK", StudentPetitionType.StudentPetition);



            // List of programs eligible for selection -- Removed because we are using the StuSelectFlag on programs
            //var stwebDefaults = new StwebDefaults();
            //stwebDefaults.StwebAcadPrograms = allPrograms.Where(p => p.IsSelectable == true).Select(p => p.Code).ToList();
            //dataAccessorMock.Setup<StwebDefaults>(acc => acc.ReadRecord<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(stwebDefaults);

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            ProgramRepository repository = new ProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return repository;
        }

        private ProgramRepository BuildValidProgramRepositoryWithBadTranscriptGrouping()
        {
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            cacheProviderMock = new Mock<ICacheProvider>();
            localCacheMock = new Mock<ObjectCache>();

            // Set up data accessor for the transaction factory 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up repo response for "all" programs requests
            dataAccessorMock.Setup<Task<Collection<AcadPrograms>>>(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "", true)).Returns(Task.FromResult(progsResponseData));

            // Set up repo response for transcript grouping
            dataAccessorMock.Setup<Task<Collection<TranscriptGroupings>>>(acc => acc.BulkReadRecordAsync<TranscriptGroupings>("TRANSCRIPT.GROUPINGS", "", true)).Returns(Task.FromResult(badTrgrData));

            // Set up repo response for program statuses valcode request
            var programStatuses = BuildProgramStatusesResponse();
            dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "PROGRAM.STATUSES", true)).ReturnsAsync(programStatuses);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                   null,
                   new SemaphoreSlim(1, 1)
             )));
            // List of programs eligible for selection -- Removed because we are using the StuSelectFlag on programs
            //var stwebDefaults = new StwebDefaults();
            //stwebDefaults.StwebAcadPrograms = allPrograms.Where(p => p.IsSelectable == true).Select(p => p.Code).ToList();
            //dataAccessorMock.Setup<StwebDefaults>(acc => acc.ReadRecord<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(stwebDefaults);

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            ProgramRepository repository = new ProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return repository;
        }

        private Collection<AcadPrograms> BuildProgramsResponse(IEnumerable<Program> Programs)
        {
            Collection<AcadPrograms> repoPrograms = new Collection<AcadPrograms>();
            foreach (var program in Programs)
            {
                var ProgramData = new AcadPrograms();
                ProgramData.Recordkey = program.Code;
                ProgramData.AcpgDesc = program.Description;
                ProgramData.AcpgTitle = program.Title;
                ProgramData.AcpgDepts = program.Departments.ToList();
                ProgramData.AcpgAcadLevel = program.AcademicLevelCode;
                var status = (program.IsActive == true) ? "A" : "X";
                var statusDate = DateTime.Today.AddDays(-30);
                ProgramData.AcpgStatus = new List<string>() { status };
                ProgramData.AcpgStatusDate = new List<DateTime?>() { statusDate };
                if (program.Code == "ALMOSTEXP.Ccd")
                {
                    // contrive a test of an acad program that has an expired status, but end date has not occurred yet
                    ProgramData.AcpgEndDate = DateTime.Today.AddDays(30);
                }
                else
                {
                    ProgramData.AcpgEndDate = statusDate;
                }
                ProgramData.ProgramStatusEntityAssociation = new List<AcadProgramsProgramStatus>();
                ProgramData.AcpgMajors = new List<string>();
                ProgramData.AcpgMinors = new List<string>();
                ProgramData.AcpgSpecializations = new List<string>();
                ProgramData.AcpgCcds = new List<string>();
                ProgramData.ProgramStatusEntityAssociation.Add(new AcadProgramsProgramStatus()
                        {
                            // If IsActive is true, set code to A. If false, set to X (for obsolete)
                            AcpgStatusAssocMember = status,
                            AcpgApprovalDatesAssocMember = statusDate
                        }
                    );
                ProgramData.AcpgCatalogs = program.Catalogs.ToList();
                ProgramData.AcpgRelatedPrograms = program.RelatedPrograms.ToList();
                if (program.Code == "ENGL.CERT")
                {
                    ProgramData.AcpgTranscriptGrouping = "DA";
                    ProgramData.AcpgUnoffTransGrouping = "AB";
                }
                else
                {
                    ProgramData.AcpgTranscriptGrouping = "UG";
                    ProgramData.AcpgUnoffTransGrouping = "BA";
                }


                ProgramData.AcpgDegree = "BS";
                ProgramData.AcpgStudentSelectFlag = program.IsSelectable ? "Y" : "N";
                ProgramData.AcpgAllowGraduationFlag = program.IsGraduationAllowed ? "Y" : "N";
                repoPrograms.Add(ProgramData);
            }
            return repoPrograms;
        }

        private ApplValcodes BuildProgramStatusesResponse()
        {
            var programStatuses = new ApplValcodes();
            programStatuses.ValsEntityAssociation = new List<ApplValcodesVals>();
            var status1 = new ApplValcodesVals()
            {
                ValInternalCodeAssocMember = "A",
                ValActionCode1AssocMember = ""
            };
            programStatuses.ValsEntityAssociation.Add(status1);
            var status2 = new ApplValcodesVals()
            {
                ValInternalCodeAssocMember = "X",
                ValActionCode1AssocMember = "2"
            };
            programStatuses.ValsEntityAssociation.Add(status2);
            return programStatuses;
        }
    }
}
