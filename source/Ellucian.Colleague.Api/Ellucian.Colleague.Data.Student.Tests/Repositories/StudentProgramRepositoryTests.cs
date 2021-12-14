// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentProgramRepositoryTests
    {
        [TestClass]
        public class StudentProgramRepository_Get : BaseRepositorySetup
        {

            StudentProgramRepository studentProgramRepo;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                studentProgramRepo =await BuildValidStudentProgramRepository();
            }

            [TestMethod]
            public async Task Get_ReturnStudentProgramsOverrides()
            {
                var studentProgram = await studentProgramRepo.GetAsync("0000894", "ECON.BA");
                Assert.IsTrue(studentProgram != null);
                Assert.AreEqual(2, studentProgram.Overrides.Count());
                Assert.IsTrue(studentProgram.Overrides.First(so => so.GroupId == "100").AllowsCredit("1"));
                Assert.IsTrue(studentProgram.Overrides.First(so => so.GroupId == "100").AllowsCredit("2"));
                Assert.IsTrue(studentProgram.Overrides.First(so => so.GroupId == "100").DeniesCredit("3"));
                Assert.IsTrue(studentProgram.Overrides.First(so => so.GroupId == "100").DeniesCredit("4"));
                Assert.IsTrue(studentProgram.Overrides.First(so => so.GroupId == "200").AllowsCredit("5"));

            }

            [TestMethod]
            public async Task Get_ReturnStudentProgramsFailedRequirementModifications()
            {
                // Set up response for the exceptions request
                var StudentDaExcptsResponseData = BuildStudentDaExcptsResponse();
                dataReaderMock.Setup<Task<Collection<StudentDaExcpts>>>(acc => acc.BulkReadRecordAsync<StudentDaExcpts>("STUDENT.DA.EXCPTS", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);


                var studentProgram = await studentProgramRepo.GetAsync("0000894", "MATH.BS");
                Assert.IsTrue(studentProgram != null);
                Assert.AreEqual(0, studentProgram.RequirementModifications.Count());

            }

            [TestMethod]
            public async Task Get_ReturnStudentProgramsWithRequirementModifications()
            {
                // Set up response for the exceptions request
                var StudentDaExcptsResponseData = BuildStudentDaExcptsResponse();
                dataReaderMock.Setup<Task<Collection<StudentDaExcpts>>>(acc => acc.BulkReadRecordAsync<StudentDaExcpts>("STUDENT.DA.EXCPTS", It.IsAny<string[]>(), true)).ReturnsAsync(StudentDaExcptsResponseData);


                var studentProgram = await studentProgramRepo.GetAsync("0000894", "MATH.BS");
                Assert.IsTrue(studentProgram != null);
                Assert.AreEqual(4, studentProgram.RequirementModifications.Count());

                // First Modification should be one with Additional Course
                var mod1 = studentProgram.RequirementModifications.Where(rm => rm.blockId == "10000").FirstOrDefault();
                var expectedMod1 = StudentDaExcptsResponseData.Where(erm => erm.StexAcadReqmtBlock == "10000").FirstOrDefault();
                Assert.IsNotNull(mod1);
                Assert.AreEqual(expectedMod1.StexPrintedSpec, mod1.modificationMessage);
                Assert.IsInstanceOfType(mod1, typeof(CoursesAddition));
                CoursesAddition ca = (CoursesAddition)mod1;
                Assert.AreEqual(2, ca.AdditionalCourses.Count());

                // Second Modification should be a replacement block
                var mod2 = studentProgram.RequirementModifications.Where(rm => rm.blockId == "20000").FirstOrDefault();
                var expectedMod2 = StudentDaExcptsResponseData.Where(erm => erm.StexAcadReqmtBlock == "20000").FirstOrDefault();
                Assert.IsNotNull(mod2);
                Assert.AreEqual(expectedMod2.StexPrintedSpec, mod2.modificationMessage);
                Assert.IsInstanceOfType(mod2, typeof(BlockReplacement));
                BlockReplacement br = (BlockReplacement)mod2;
                Assert.IsNotNull(br.NewRequirement);
                Assert.IsNotNull(br.NewRequirement.SubRequirements);
                Assert.IsTrue(br.NewRequirement.SubRequirements.First().IsBlockReplacement);
                Assert.AreEqual(2, br.NewRequirement.SubRequirements.First().MinGroups);
                Assert.IsTrue(br.NewRequirement.SubRequirements.First().Groups.First().IsBlockReplacement);

                // Third Modification should be a waiver
                var mod3 = studentProgram.RequirementModifications.Where(rm => rm.blockId == "30000").FirstOrDefault();
                var expectedMod3 = StudentDaExcptsResponseData.Where(erm => erm.StexAcadReqmtBlock == "30000").FirstOrDefault();
                Assert.IsNotNull(mod3);
                Assert.AreEqual(expectedMod3.StexPrintedSpec, mod3.modificationMessage);
                Assert.IsInstanceOfType(mod3, typeof(BlockReplacement));
                BlockReplacement br2 = (BlockReplacement)mod3;
                Assert.IsNull(br2.NewRequirement);

                // Fourth Modification should be a GPA waiver
                var mod4 = studentProgram.RequirementModifications.Where(rm => rm.blockId == "40000").FirstOrDefault();
                var expectedMod4 = StudentDaExcptsResponseData.Where(erm => erm.StexAcadReqmtBlock == "40000").FirstOrDefault();
                Assert.IsNotNull(mod4);
                Assert.AreEqual(expectedMod4.StexPrintedSpec, mod4.modificationMessage);
                Assert.IsInstanceOfType(mod4, typeof(GpaModification));

            }

            [TestMethod]
            public async Task Get_ReturnAllStudentPrograms_IncludingInactive()
            {
                var studentPrograms = await studentProgramRepo.GetAsync("0000894");
                Assert.AreEqual(6, studentPrograms.Count());
            }

            [TestMethod]
            public async Task Get_All_VerifyStudentProgramStatus()
            {
                var studentPrograms = (await studentProgramRepo.GetAsync("0000894")).ToList();
                Assert.AreEqual(6, studentPrograms.Count());
                Assert.AreEqual(studentPrograms[0].ProgramCode, "MATH.BS");
                Assert.AreEqual(studentPrograms[0].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.Active);
                Assert.AreEqual(studentPrograms[1].ProgramCode, "ECON.BA");
                Assert.AreEqual(studentPrograms[1].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.Potential);
                Assert.AreEqual(studentPrograms[2].ProgramCode, "PROG1");
                Assert.AreEqual(studentPrograms[2].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.Graduated);
                Assert.AreEqual(studentPrograms[3].ProgramCode, "PROG2");
                Assert.AreEqual(studentPrograms[3].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.InActive);
                Assert.AreEqual(studentPrograms[4].ProgramCode, "PROG3");
                Assert.AreEqual(studentPrograms[4].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.Withdrawn);
                Assert.AreEqual(studentPrograms[5].ProgramCode, "PROG4");
                Assert.AreEqual(studentPrograms[5].ProgramStatusProcessingCode, StudentProgramStatusProcessingType.None);

            }

            [TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            public async Task Get_NoLongerThrowsIfTransactionReturnsEmpty()
            {
                var studentPrograms = await studentProgramRepo.GetAsync("BADRECORD");
                Assert.IsTrue(studentPrograms.Count() == 0);
            }


            [TestMethod]
            public async Task Get_All_GetsCachedStudentPrograms()
            {
                var studentprograms1 = await studentProgramRepo.GetAsync("0000894");
                var response = BuildStudentProgramsResponse(studentprograms1);
                // from here on out return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>());
                // set up cache moth to return true, item found in cache, and to return item from cache            
                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(It.IsAny<string>(), null)).Returns(response).Verifiable();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
           x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
           .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
               null,
               new SemaphoreSlim(1, 1)
               ));
                // Set up response for the overrides request
                dataReaderMock.Setup<Task<Collection<StudentDaOverrides>>>(acc => acc.BulkReadRecordAsync<StudentDaOverrides>("STUDENT.DA.OVERRIDES", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentDaOverrides>());
                // Set up response for the exceptions request
                dataReaderMock.Setup<Task<Collection<StudentDaExcpts>>>(acc => acc.BulkReadRecordAsync<StudentDaExcpts>("STUDENT.DA.EXCPTS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentDaExcpts>());

                // Now attempt get for student program again, verify that correct student programs returned
                var studentprograms2 = await studentProgramRepo.GetAsync("0000894");
                Assert.IsTrue(studentprograms2.Count() >= 2);

                // Call the single-program getter, should hit cache again
                var studentprogramindiv = await studentProgramRepo.GetAsync("0000894", "ECON.BA");
                Assert.IsNotNull(studentprogramindiv);
                Assert.AreEqual("ECON.BA", studentprogramindiv.ProgramCode);

            }

            [TestMethod]
            public async Task Get_All_GetsNonCachedStudentPrograms()
            {
                TestStudentProgramRepository tsp = new TestStudentProgramRepository();
                var progdata1 = BuildStudentProgramsResponse(await tsp.GetAsync("0000894", "ECON.BA"));
                var progdata2 = BuildStudentProgramsResponse(await tsp.GetAsync("0000894")); 

                string[] twoprogram = { "ECON.BA", "PROG1" };
                string[] threeprogram = { "ECON.BA", "MATH.BS", "PROG1"};

                string[] twoprogramkey = { "0000894*ECON.BA", "0000894*PROG1" };
                string[] threeprogramkey = { "0000894*ECON.BA", "0000894*MATH.BS", "0000894*PROG1" };

                Students studata1 = new Students() { Recordkey = "0000894", StuAcadPrograms = new List<string>() { twoprogram[0], twoprogram[1] } }; 
                Students studata2 = new Students() { Recordkey = "0000894", StuAcadPrograms = new List<string>() { threeprogram[0], threeprogram[1], threeprogram[2] } };

                string cacheKey0 = studentProgramRepo.BuildFullCacheKey("StudentPrograms0000894");                          //  before this change they looked like this
                string cacheKey1 = studentProgramRepo.BuildFullCacheKey("StudentPrograms0000894*ECON.BA.0000894*PROG1");
                string cacheKey2 = studentProgramRepo.BuildFullCacheKey("StudentPrograms0000894*ECON.BA.0000894*MATH.BS.0000894*PROG1");

                // Set up repo response for STUDENT.PROGRAMS reads
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", twoprogramkey, true)).ReturnsAsync(progdata1);
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", threeprogramkey, true)).ReturnsAsync(progdata2);

                // Set the first STUDENTS read to return the first program only
                dataReaderMock.Setup<Task<Students>>(stu => stu.ReadRecordAsync<Students>("STUDENTS", "0000894", true)).ReturnsAsync(studata1);

                // Mock AcadPrograms response
                var testPrograms = (await new TestProgramRepository().GetAsync()).Where(x => x.Code == "ECON.BA");
                var acadProgramsResponse = BuildProgramsResponse(testPrograms);
                dataReaderMock.Setup<Task<Collection<AcadPrograms>>>(acc => acc.BulkReadRecordAsync<AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(acadProgramsResponse);

                // This should set the cache with the first cachekey
                var studentprograms1 = await studentProgramRepo.GetAsync("0000894");
                Assert.IsTrue(studentprograms1.Count() == 5);

                // Set up cache mock to return true, item found in cache, and to return item from cache            
                cacheProviderMock.Setup(x => x.Contains(cacheKey0, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey0, null)).Returns(progdata1).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(cacheKey1, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey1, null)).Returns(progdata1).Verifiable();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
           x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
           .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
               null,
               new SemaphoreSlim(1, 1)
               ));
                // Set the first STUDENTS read to return two programs now (someone just added a program on SACP.)
                dataReaderMock.Setup<Task<Students>>(stu => stu.ReadRecordAsync<Students>("STUDENTS", "0000894", true)).ReturnsAsync(studata2);

                var studentprograms2 = await studentProgramRepo.GetAsync("0000894");
                Assert.IsTrue(studentprograms2.Count() >= 3);

            }


            private async Task<StudentProgramRepository> BuildValidStudentProgramRepository()
            {
                // Set up data accessor for mocking 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Set up response for student 
                Students strec = new Students() { Recordkey = "0000894", StuAcadPrograms = new List<string>() { "MATH.BS", "ECON.BA", "PROG1", "PROG2","PROG3","PROG4"} };
                dataReaderMock.Setup<Task<Students>>(acc => acc.ReadRecordAsync<Students>("STUDENTS", "0000894", true)).ReturnsAsync(strec);

                // Set up response for student programs
                IEnumerable<StudentProgram> studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                var StudentProgramResponseData = BuildStudentProgramsResponse(studentPrograms);
                // Student has four programs total which include 2 current programs from TestStudentProgramRepository (MATH.BS, ECON.BA)
                // and two more noncurrent programs tacked on by BuildStudentProgramsResponse (PROG1, PROG2)
                string[] prog = { "0000894*MATH.BS", "0000894*ECON.BA", "0000894*PROG1", "0000894*PROG2", "0000894*PROG3", "0000894*PROG4" };
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", prog, true)).ReturnsAsync(StudentProgramResponseData);

                // Set up response for the overrides request
                var StudentDaOverridesResponseData = BuildStudentDaOverridesResponse(studentPrograms);
                dataReaderMock.Setup<Task<Collection<StudentDaOverrides>>>(acc => acc.BulkReadRecordAsync<StudentDaOverrides>("STUDENT.DA.OVERRIDES", new string[] { "99991", "99992" }, true)).ReturnsAsync(StudentDaOverridesResponseData);

                // Set up response for the exceptions request
                var StudentDaExcptsResponseData = BuildStudentDaExcptsResponse();
                dataReaderMock.Setup<Task<Collection<StudentDaExcpts>>>(acc => acc.BulkReadRecordAsync<StudentDaExcpts>("STUDENT.DA.EXCPTS", new string[] { }, true)).ReturnsAsync(StudentDaExcptsResponseData);

                // mock data accessor STUDENT.PROGRAM.STATUSES
                dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                    a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "P","A","G","C","W" },
                        ValExternalRepresentation = new List<string>() { "Potential","Active","Graduated", "Changed" ,"Withdrawn"},
                        ValActionCode1 = new List<string>() { "1","2","3","4","5" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals() 
                            {
                                ValInternalCodeAssocMember = "P",
                                ValExternalRepresentationAssocMember = "Potential",
                                ValActionCode1AssocMember = "1"
                            },
                            new ApplValcodesVals() 
                            {
                                ValInternalCodeAssocMember = "A",
                                ValExternalRepresentationAssocMember = "Active",
                                ValActionCode1AssocMember = "2"
                            },
                            new ApplValcodesVals() 
                            {
                                ValInternalCodeAssocMember = "G",
                                ValExternalRepresentationAssocMember = "Graduated",
                                ValActionCode1AssocMember = "3"
                            },
                            new ApplValcodesVals() 
                            {
                                ValInternalCodeAssocMember = "C",
                                ValExternalRepresentationAssocMember = "Changed",
                                ValActionCode1AssocMember = "4"
                            },
                            new ApplValcodesVals() 
                            {
                                ValInternalCodeAssocMember = "W",
                                ValExternalRepresentationAssocMember = "Withdrawn",
                                ValActionCode1AssocMember = "5"
                            }
                        }
                    });

                // Set up response for a request for a program that doesn't exist
                Collection<StudentPrograms> badresponse = new Collection<StudentPrograms>() { null };
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", "@ID LIKE 'BADRECORD*...'", true)).ReturnsAsync(badresponse);

                // Construct repository
                studentProgramRepo = new StudentProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentProgramRepo;
            }

            private Collection<StudentPrograms> BuildStudentProgramsResponse(StudentProgram studentprograms1)
            {
                return BuildStudentProgramsResponse(new List<StudentProgram>() { studentprograms1 });
            }

            private Collection<StudentPrograms> BuildStudentProgramsResponse(IEnumerable<StudentProgram> studentPrograms)
            {
                string[] programStatus = new string[] {"A","P","G","C","W" };
                int programStatusOffset = 0;
                Collection<StudentPrograms> repoStudentPrograms = new Collection<StudentPrograms>();
                foreach (var studentProgram in studentPrograms)
                {
                    if(programStatusOffset > 4)
                    {
                        programStatusOffset = 0;
                    }
                    var studentProgramData = new StudentPrograms();

                    studentProgramData.Recordkey = studentProgram.StudentId + "*" + studentProgram.ProgramCode;
                    studentProgramData.StprAntCmplDate = studentProgram.AnticipatedCompletionDate;
                    studentProgramData.StprCatalog = studentProgram.CatalogCode;

                    studentProgramData.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                    studentProgramData.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                    studentProgramData.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                    studentProgramData.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                    // Removed this logic because the Majors, Minors, ccds, specializations cannot be mocked
                    // Set up start and end dates for additional majors/minors/ccd/specializations setup
                    //   set 1: start before current date, no end date (included)
                    //   set 2: start before current date, future end date (included)
                    //   set 3: start after current date, no end date (excluded)
                    //   set 4: start before current date, ends today (excluded)
                    //var startDates = new List<DateTime?>() { DateTime.Now.Subtract(new TimeSpan(72, 00, 00)), DateTime.Now.Subtract(new TimeSpan(72, 00, 00)), DateTime.Now.AddDays(2), new DateTime(2013, 12, 31) };
                    //var endDates = new List<DateTime?>() {null, DateTime.Now.AddDays(3), null, DateTime.Now};
                    //// Build additional majors, minors, ccds, specializations responses, each one has four items with each of the above dates to verify proper date checking and inclusion
                    //studentProgramData.StprMajorListEntityAssociation = BuildAdditionalMajorsResponse(startDates, endDates).ToList();
                    //studentProgramData.StprMinorListEntityAssociation = BuildAdditionalMinorsResponse(startDates, endDates).ToList();
                    //studentProgramData.StprCcdListEntityAssociation = BuildAdditionalCcdsResponse(startDates, endDates).ToList();
                    //studentProgramData.StprSpecialtiesEntityAssociation = BuildAdditionalSpecialtiesResponse(startDates, endDates).ToList();

                    studentProgramData.StprDaOverrides = new List<string>();
                    studentProgramData.StprDaExcpts = new List<string>();

                    studentProgramData.StprStatus = new List<string> {programStatus[programStatusOffset++], "A" };
                    studentProgramData.StprStartDate = new List<DateTime?>() { new DateTime() };

                    if (studentProgram.Overrides.Count() > 0)
                    {
                        // the record keys to this file are not actually in the Override
                        // domain object and aren't used in the domain.  They are only read
                        // here and used to select the data.  These are set to match the mocked data.
                        studentProgramData.StprDaOverrides = new List<string>() { "99991", "99992" };
                    }
                    // Used for testing Program exceptions
                    if (studentProgramData.Recordkey == "0000894*MATH.BS")
                    {
                        studentProgramData.StprDaExcpts = new List<string>() { "E1", "E2", "E3", "E4" };
                    } else
                    {
                        studentProgramData.StprDaExcpts = new List<string>();
                    }
                        


                    repoStudentPrograms.Add(studentProgramData);
                }

                // Add another repo response item with a previous end date
                var stuProgData = new StudentPrograms();
                stuProgData.Recordkey = "0000894*PROG1";
                stuProgData.StprCatalog = "2012";
                stuProgData.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData.StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-30) };
                stuProgData.StprDaOverrides = new List<string>();
                stuProgData.StprDaExcpts = new List<string>();
                stuProgData.StprStatus = new List<string>() {"G" };
                stuProgData.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData);
                // Add another repo response item with a noncurrent status
                var stuProgData1 = new StudentPrograms();
                stuProgData1.Recordkey = "0000894*PROG2";
                stuProgData1.StprCatalog = "2012";
                stuProgData1.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData1.StprStatus = new List<string>() { "C" };
                stuProgData1.StprDaOverrides = new List<string>();
                stuProgData1.StprDaExcpts = new List<string>();
                stuProgData1.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData1.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData1.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData1.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData1);

                // Add another repo response item with a withdrawn status
                var stuProgData2 = new StudentPrograms();
                stuProgData2.Recordkey = "0000894*PROG3";
                stuProgData2.StprCatalog = "2012";
                stuProgData2.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData2.StprStatus = new List<string>() { "W" };
                stuProgData2.StprDaOverrides = new List<string>();
                stuProgData2.StprDaExcpts = new List<string>();
                stuProgData2.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData2.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData2.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData2.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData2);

                // Add another repo response item with a no status
                var stuProgData3 = new StudentPrograms();
                stuProgData3.Recordkey = "0000894*PROG4";
                stuProgData3.StprCatalog = "2012";
                stuProgData3.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData3.StprStatus = new List<string>();
                stuProgData3.StprDaOverrides = new List<string>();
                stuProgData3.StprDaExcpts = new List<string>();
                stuProgData3.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData3.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData3.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData3.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData3);

                return repoStudentPrograms;
            }

            private async Task<Collection<StudentProgramsStprMajorList>> BuildAdditionalMajorsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
            {
                var addlMajors = new Collection<StudentProgramsStprMajorList>();
                var majors = await new TestStudentReferenceDataRepository().GetMajorsAsync();
                for (int i = 0; i < startDates.Count(); i++)
                {
                    var addlMajor = new StudentProgramsStprMajorList()
                    {
                        StprAddnlMajorsAssocMember = majors.ElementAt(i).Code,
                        StprAddnlMajorReqmtsAssocMember = "req" + i.ToString(),
                        StprAddnlMajorStartDateAssocMember = startDates.ElementAt(i),
                        StprAddnlMajorEndDateAssocMember = endDates.ElementAt(i)
                    };
                    addlMajors.Add(addlMajor);
                }
                return addlMajors;
            }

            private async Task<Collection<StudentProgramsStprMinorList>> BuildAdditionalMinorsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
            {
                var addlMinors = new Collection<StudentProgramsStprMinorList>();
                var minors = await new TestStudentReferenceDataRepository().GetMinorsAsync();
                for (int i = 0; i < startDates.Count(); i++)
                {
                    var addlMinor = new StudentProgramsStprMinorList()
                    {
                        StprMinorsAssocMember = minors.ElementAt(i).Code,
                        StprMinorReqmtsAssocMember = "req" + i.ToString(),
                        StprMinorStartDateAssocMember = startDates.ElementAt(i),
                        StprMinorEndDateAssocMember = endDates.ElementAt(i)
                    };
                    addlMinors.Add(addlMinor);
                }
                return addlMinors;
            }

            private async Task<Collection<StudentProgramsStprCcdList>> BuildAdditionalCcdsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
            {
                var addlCcds = new Collection<StudentProgramsStprCcdList>();
                var ccds = await new TestStudentReferenceDataRepository().GetCcdsAsync();
                for (int i = 0; i < startDates.Count(); i++)
                {
                    var addlCcd = new StudentProgramsStprCcdList()
                    {
                        StprCcdsAssocMember = ccds.ElementAt(i).Code,
                        StprCcdsReqmtsAssocMember = "req" + i.ToString(),
                        StprCcdsStartDateAssocMember = startDates.ElementAt(i),
                        StprCcdsEndDateAssocMember = endDates.ElementAt(i)
                    };
                    addlCcds.Add(addlCcd);
                }
                return addlCcds;
            }

            private async Task<Collection<StudentProgramsStprSpecialties>> BuildAdditionalSpecialtiesResponse(List<DateTime?> startDates, List<DateTime?> endDates)
            {
                var addlSpecialtizations = new Collection<StudentProgramsStprSpecialties>();
                var specializations = await new TestStudentReferenceDataRepository().GetSpecializationsAsync();
                for (int i = 0; i < startDates.Count(); i++)
                {
                    var addlSpecialty = new StudentProgramsStprSpecialties()
                    {
                        StprSpecializationsAssocMember = specializations.ElementAt(i).Code,
                        StprSpecializationReqmtsAssocMember = "eq" + i.ToString(),
                        StprSpecializationStartAssocMember = startDates.ElementAt(i),
                        StprSpecializationEndAssocMember = endDates.ElementAt(i)
                    };
                    addlSpecialtizations.Add(addlSpecialty);
                }
                return addlSpecialtizations;
            }

            private Collection<StudentDaOverrides> BuildStudentDaOverridesResponse(IEnumerable<StudentProgram> studentPrograms)
            {
                Collection<StudentDaOverrides> overrides = new Collection<StudentDaOverrides>();

                foreach (var sp in studentPrograms)
                {
                    foreach (var o in sp.Overrides)
                    {
                        StudentDaOverrides over = new StudentDaOverrides();

                        over.StovStudentProgram = sp.ProgramCode;
                        over.StovAcadReqmtBlock = o.GroupId;
                        over.StovInclStudentAcadCred = o.CreditsAllowed.ToList();
                        over.StovExclStudentAcadCred = o.CreditsDenied.ToList();

                        overrides.Add(over);
                    }

                }

                return overrides;
            }

            private Collection<StudentDaExcpts> BuildStudentDaExcptsResponse()
            {
                Collection<StudentDaExcpts> exceptions = new Collection<StudentDaExcpts>();
                // First is Additional Courses
                var exception1 = new StudentDaExcpts() { Recordkey = "E1", StexStudentProgram = "0000894*MATH.BS",  StexAcadReqmtBlock = "10000", StexType = "E", StexElement = "ADEL", StexAddnlCourses = new List<string>() { "100", "200" }, StexPrintedSpec = "Exception: Allowing Courses 100 and 200"};
                exceptions.Add(exception1);

                // Second is a Replacement Block - without min grades 
                var courseAssociation = new List<StudentDaExcptsBlockRepl>();
                var ca1 = new StudentDaExcptsBlockRepl() { StexBlockReplCoursesAssocMember = "87" };
                courseAssociation.Add(ca1);
                var ca2 = new StudentDaExcptsBlockRepl() { StexBlockReplCoursesAssocMember = "139" };
                courseAssociation.Add(ca2);
                var ca3 = new StudentDaExcptsBlockRepl();
                courseAssociation.Add(ca3);
                var exception2 = new StudentDaExcpts() { Recordkey = "E2", StexStudentProgram = "0000894*MATH.BS", StexAcadReqmtBlock = "20000", StexType = "R", StexElement = "BLK", StexPrintedSpec = "BlockReplacement: Take Courses 87 - min B and 139 no min grade", BlockReplEntityAssociation = courseAssociation};
                exceptions.Add(exception2);

                // Third is a Block Waiver
                var exception3 = new StudentDaExcpts() { Recordkey = "E3", StexStudentProgram = "0000894*MATH.BS", StexAcadReqmtBlock = "30000", StexType = "W", StexElement = "BLK", StexPrintedSpec = "BlockReplacement Waiver: Waiving this requirement" };
                exceptions.Add(exception3);

                // Fourth is invalid because it does not have StexElement so it will be skipped.
                var exception4 = new StudentDaExcpts() { Recordkey = "E99", StexStudentProgram = "0000894*MATH.BS", StexAcadReqmtBlock = "99000", StexType = "W", StexPrintedSpec = "Invalid Waiver: StexElement is null" };
                exceptions.Add(exception4);

                // Fifth is a GPA 
                var exception5 = new StudentDaExcpts() { Recordkey = "E4", StexStudentProgram = "0000894*MATH.BS", StexAcadReqmtBlock = "40000", StexType = "W", StexElement = "GPA", StexPrintedSpec = "GPA Waiver: Waiving GPA for this requirement" };
                exceptions.Add(exception5);

                // Sixth is an invalid element type
                var exception6 = new StudentDaExcpts() { Recordkey = "E98", StexStudentProgram = "0000894*MATH.BS", StexAcadReqmtBlock = "98000", StexType = "R", StexElement = "XXX", StexPrintedSpec = "Invalid element type" };
                exceptions.Add(exception6);

                // Seventh is an invalid block replacement because it has no block Id
                var exception7 = new StudentDaExcpts() { Recordkey = "E97", StexStudentProgram = "0000894*MATH.BS", StexType = "R", StexElement = "BLK", StexPrintedSpec = "Invalid block: No block Id.", BlockReplEntityAssociation = courseAssociation };
                exceptions.Add(exception7);


                //453               group change take 4 courses to 3
                //449               pr    change overall program inst cred requirement from 90 to 89
                //454               group take 12 credits change to 11
                //450               pr    chg from min cred from 120 to 119
                //451               req   replace subrequirement count - two to 1




                return exceptions;
            }
        
            private Collection<AcadPrograms> BuildProgramsResponse(IEnumerable<Domain.Student.Entities.Requirements.Program> Programs)
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
                    }
                    else
                    {
                        ProgramData.AcpgTranscriptGrouping = "UG";
                    }


                    ProgramData.AcpgDegree = "BS";
                    ProgramData.AcpgStudentSelectFlag = program.IsSelectable == true ? "Y" : "N";
                    repoPrograms.Add(ProgramData);
                }
                return repoPrograms;
            }
        }

        [TestClass]
        public class StudentProgramRepository_GetStudentProgramNotice : BaseRepositorySetup
        {

            StudentProgramRepository studentProgramRepo;
            string studentId;
            string programId;
            
            [TestInitialize]
            public void  Initialize()
            {
                MockInitialize();

                studentProgramRepo = BuildValidStudentProgramRepository();

                studentId = "0000123";
                programId = "XYZ";
            }

            [TestMethod]
            public async Task NoText_ReturnsEmptyTextList()
            {
                // arrange
                BuildDegreeAuditCustomTextResponse noticeResponse = new BuildDegreeAuditCustomTextResponse();
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<BuildDegreeAuditCustomTextRequest, BuildDegreeAuditCustomTextResponse>(It.IsAny<BuildDegreeAuditCustomTextRequest>())).ReturnsAsync(noticeResponse);

                // act 
                var studentProgramNotices = await studentProgramRepo.GetStudentProgramEvaluationNoticesAsync(studentId, programId);
                
                // assert
                Assert.AreEqual(0, studentProgramNotices.Count());
            }

            [TestMethod]
            public async Task ReturnsAllText()
            {
                // arrange
                BuildDegreeAuditCustomTextResponse noticeResponse = new BuildDegreeAuditCustomTextResponse()
                    {
                        AlAcademicProgramText = new List<string>() { "AcademicProgramText Line1", "AcademicProgramText Line2" },
                        AlStudentProgramText = new List<string>() { "StudentProgramText Line1", "StudentProgramText Line2" },
                        AlStartText = new List<string>() { "StartText Line1", "StartText Line2" },
                        AlEndText = new List<string>() { "EndText Line1", "EndText Line2" }
                    };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<BuildDegreeAuditCustomTextRequest, BuildDegreeAuditCustomTextResponse>(It.IsAny<BuildDegreeAuditCustomTextRequest>())).ReturnsAsync(noticeResponse);

                // act 
                var studentProgramNotices = await studentProgramRepo.GetStudentProgramEvaluationNoticesAsync(studentId, programId);

                // assert
                Assert.AreEqual(4, studentProgramNotices.Count());
                Assert.AreEqual(studentId, studentProgramNotices.ElementAt(0).StudentId);
                Assert.AreEqual(programId, studentProgramNotices.ElementAt(1).ProgramCode);
                // Check academicprogram notice
                var notice = studentProgramNotices.Where(n => n.Type == EvaluationNoticeType.StudentProgram).First();
                foreach (var item in noticeResponse.AlStudentProgramText)
                {
                    Assert.IsTrue(notice.Text.Contains(item));
                }
                notice = studentProgramNotices.Where(n => n.Type == EvaluationNoticeType.Program).First();
                foreach (var item in noticeResponse.AlAcademicProgramText)
                {
                    Assert.IsTrue(notice.Text.Contains(item));
                }
                notice = studentProgramNotices.Where(n => n.Type == EvaluationNoticeType.Start).First();
                foreach (var item in noticeResponse.AlStartText)
                {
                    Assert.IsTrue(notice.Text.Contains(item));
                }
                notice = studentProgramNotices.Where(n => n.Type == EvaluationNoticeType.End).First();
                foreach (var item in noticeResponse.AlEndText)
                {
                    Assert.IsTrue(notice.Text.Contains(item));
                }
            }

            [TestMethod]
            public async Task ReturnsEmptyListWhenNoTextReturned()
            {
                // arrange
                BuildDegreeAuditCustomTextResponse noticeResponse = new BuildDegreeAuditCustomTextResponse()
                {
                    AlAcademicProgramText = new List<string>(),
                    AlStudentProgramText = new List<string>(),
                    AlStartText = new List<string>(),
                    AlEndText = new List<string>()
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<BuildDegreeAuditCustomTextRequest, BuildDegreeAuditCustomTextResponse>(It.IsAny<BuildDegreeAuditCustomTextRequest>())).ReturnsAsync(noticeResponse);

                // act 
                var studentProgramNotices = await studentProgramRepo.GetStudentProgramEvaluationNoticesAsync(studentId, programId);

                // assert
                Assert.AreEqual(0, studentProgramNotices.Count());
            }

            [TestMethod]
            public async Task ReturnsEmptyListForTransactionFailure()
            {
                // arrange
                BuildDegreeAuditCustomTextResponse noticeResponse = new BuildDegreeAuditCustomTextResponse();
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<BuildDegreeAuditCustomTextRequest, BuildDegreeAuditCustomTextResponse>(It.IsAny<BuildDegreeAuditCustomTextRequest>())).Throws(new Exception());

                // act 
                var studentProgramNotices = await studentProgramRepo.GetStudentProgramEvaluationNoticesAsync(studentId, programId);
                
                // assert
                Assert.AreEqual(0, studentProgramNotices.Count());
            }

            private StudentProgramRepository BuildValidStudentProgramRepository()
            {
                // Set up data accessor for mocking 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Construct repository
                studentProgramRepo = new StudentProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentProgramRepo;
            }

        }

        //#region HEDMGet

        //[TestClass]
        //public class StudentProgramRepository_GetHeDM : BaseRepositorySetup
        //{
        //    private ApiSettings apiSettingsMock;
        //    private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        //    IEnumerable<StudentProgram> allStuProgs;
        //    IEnumerable<AcademicProgram> allAcadProgs;
        //    Collection<AcadPrograms> acadProgResponseData;
        //    Collection<Person> studentResponseData;
        //    Collection<StudentProgramsStprMajorList> majors;
        //    Collection<StudentProgramsStprMinorList> minors;
        //    Collection<StudentProgramsStprCcdList> ccds;
        //    Collection<StudentProgramsStprSpecialties> sps;
        //    Collection<StudentPrograms> stuProgResponseData;
        //    Dictionary<string, StudentProgram> allStuProgDict;
        //    private IStudentProgramRepository stuProgRepo;
        //    private IStudentReferenceDataRepository stuRefData;

        //    [TestInitialize]
        //    public async void Initialize()
        //    {
        //        MockInitialize();
        //        studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
        //        allStuProgs = await new TestStudentProgramRepository().GetAcademicProgramEnrollmentsAsync(false);
        //        allAcadProgs = await new TestAcademicProgramRepository().GetAsync();
        //        // Build studentprograms dict, response from cache
        //        allStuProgDict = new Dictionary<string, StudentProgram>();
        //        foreach (var prog in allStuProgs)
        //        {
        //            allStuProgDict[prog.Guid] = prog;
        //        }


        //        // Repository response data
        //        var startDates = new List<DateTime?>() { DateTime.Now.Subtract(new TimeSpan(72, 00, 00)), DateTime.Now.Subtract(new TimeSpan(72, 00, 00)), DateTime.Now.AddDays(2), new DateTime(2013, 12, 31) };
        //        var endDates = new List<DateTime?>() { null, DateTime.Now.AddDays(3), null, DateTime.Now };
        //        majors = BuildAdditionalMajorsResponse(startDates, endDates);
        //        minors = BuildAdditionalMinorsResponse(startDates, endDates);
        //        ccds = BuildAdditionalCcdsResponse(startDates, endDates);
        //        sps = BuildAdditionalSpecialtiesResponse(startDates, endDates);
        //        stuProgResponseData = BuildStudentProgramResponse(allStuProgs);
        //        acadProgResponseData = BuildAcadProgResponse(allAcadProgs);
        //        studentResponseData = BuildStudentResponse();
        //        stuProgRepo = BuildValidStudentProgramRepository();
        //        stuRefData = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                
                              
        //    }


        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        transFactoryMock = null;
        //        cacheProviderMock = null;
        //        stuProgResponseData = null;
        //        allStuProgs = null;
        //        stuProgRepo = null;
        //    }

        //    private Collection<StudentPrograms> BuildStudentProgramResponse(IEnumerable<StudentProgram> stuProg)
        //    {

        //        Collection<StudentPrograms> repoStuProgs= new Collection<StudentPrograms>();
        //        foreach (var prog in stuProg)
        //        {
        //            var repoStuProg = new StudentPrograms();
        //            repoStuProg.Recordkey = string.Concat(prog.StudentId.ToString(), "*", prog.ProgramCode);
        //            repoStuProg.RecordGuid = prog.Guid;
        //            repoStuProg.StprCatalog = prog.CatalogCode;
        //            repoStuProg.StprStatus = new List<string>() { prog.Status };
        //            repoStuProg.StprStartDate = new List<DateTime?>() { prog.StartDate };
        //            repoStuProg.StprEndDate = new List<DateTime?>() { prog.EndDate };
                    
        //            // Build additional majors, minors, ccds, specializations responses, each one has four items with each of the above dates to verify proper date checking and inclusion
        //            repoStuProg.StprMajorListEntityAssociation = majors.ToList();
        //            repoStuProg.StprMinorListEntityAssociation = minors.ToList();
        //            repoStuProg.StprCcdListEntityAssociation = ccds.ToList();
        //            repoStuProg.StprSpecialtiesEntityAssociation = sps.ToList();

        //            repoStuProgs.Add(repoStuProg);
        //            //repoStuProg.StprAddnlMajors = prog.StudentProgramMajors;
        //        }
                    
        //        return repoStuProgs ;
        //    }

        //    private Collection<AcadPrograms> BuildAcadProgResponse(IEnumerable<AcademicProgram> acadProg)
        //    {
        //        Collection<AcadPrograms> repoAcadProgs = new Collection<AcadPrograms>();
        //        foreach (var prog in acadProg)
        //        {
        //            var repoStuProg = new AcadPrograms();
        //            repoStuProg.Recordkey = prog.Code;
        //            repoStuProg.RecordGuid = prog.Guid;
        //            repoStuProg.AcpgMajors = prog.MajorCodes;
        //            repoStuProg.AcpgMinors = prog.MinorCodes;
        //            repoStuProg.AcpgSpecializations = prog.SpecializationCodes;
        //            repoAcadProgs.Add(repoStuProg);                    
        //        }

        //        return repoAcadProgs;
        //    }

        //    private Collection<Person> BuildStudentResponse()
        //    {
        //        Collection<Person> stuCollection = new Collection<Person>();
        //        var stu = new Person();
        //        stu.Recordkey = "12345678";
        //        stuCollection.Add(stu);
        //        return stuCollection;
        //    }

        //    private Collection<StudentProgramsStprMajorList> BuildAdditionalMajorsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
        //    {
        //        var addlMajors = new Collection<StudentProgramsStprMajorList>();
        //        var majors = new TestAcademicDisciplineRepository().GetOtherMajors();
        //        for (int i = 0; i < majors.Count(); i++)
        //        {
        //            var addlMajor = new StudentProgramsStprMajorList()
        //            {
        //                StprAddnlMajorsAssocMember = majors.ElementAt(i).Code,
        //                StprAddnlMajorReqmtsAssocMember = "req" + i.ToString(),
        //                StprAddnlMajorStartDateAssocMember = startDates.ElementAt(i),
        //                StprAddnlMajorEndDateAssocMember = endDates.ElementAt(i)
        //            };
        //            addlMajors.Add(addlMajor);
        //        }
        //        return addlMajors;
        //    }

        //    private Collection<StudentProgramsStprMinorList> BuildAdditionalMinorsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
        //    {
        //        var addlMinors = new Collection<StudentProgramsStprMinorList>();
        //        var minors = new TestAcademicDisciplineRepository().GetOtherMinors();
        //        for (int i = 0; i < minors.Count(); i++)
        //        {
        //            var addlMinor = new StudentProgramsStprMinorList()
        //            {
        //                StprMinorsAssocMember = minors.ElementAt(i).Code,
        //                StprMinorReqmtsAssocMember = "req" + i.ToString(),
        //                StprMinorStartDateAssocMember = startDates.ElementAt(i),
        //                StprMinorEndDateAssocMember = endDates.ElementAt(i)
        //            };
        //            addlMinors.Add(addlMinor);
        //        }
        //        return addlMinors;
        //    }

        //    private Collection<StudentProgramsStprCcdList> BuildAdditionalCcdsResponse(List<DateTime?> startDates, List<DateTime?> endDates)
        //    {
        //        var addlCcds = new Collection<StudentProgramsStprCcdList>();
        //        var ccds = new TestAcademicCredentialsRepository().GetOtherCcds();
        //        for (int i = 0; i < ccds.Count(); i++)
        //        {
        //            var addlCcd = new StudentProgramsStprCcdList()
        //            {
        //                StprCcdsAssocMember = ccds.ElementAt(i).Code,
        //                StprCcdsReqmtsAssocMember = "req" + i.ToString(),
        //                StprCcdsStartDateAssocMember = startDates.ElementAt(i),
        //                StprCcdsEndDateAssocMember = endDates.ElementAt(i)
        //            };
        //            addlCcds.Add(addlCcd);
        //        }
        //        return addlCcds;
        //    }

        //    private Collection<StudentProgramsStprSpecialties> BuildAdditionalSpecialtiesResponse(List<DateTime?> startDates, List<DateTime?> endDates)
        //    {
        //        var addlSpecialtizations = new Collection<StudentProgramsStprSpecialties>();
        //        var specializations = new TestAcademicDisciplineRepository().GetOtherSpecials();
        //        for (int i = 0; i < specializations.Count(); i++)
        //        {
        //            var addlSpecialty = new StudentProgramsStprSpecialties()
        //            {
        //                StprSpecializationsAssocMember = specializations.ElementAt(i).Code,
        //                StprSpecializationReqmtsAssocMember = "eq" + i.ToString(),
        //                StprSpecializationStartAssocMember = startDates.ElementAt(i),
        //                StprSpecializationEndAssocMember = endDates.ElementAt(i)
        //            };
        //            addlSpecialtizations.Add(addlSpecialty);
        //        }
        //        return addlSpecialtizations;
        //    }

        //    public IStudentProgramRepository BuildValidStudentProgramRepository()
        //    {
        //        // Set up dataAccessorMock as the object for the DataAccessor
        //        transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

        //        // Set up response for grade Get request
        //        dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(acc => acc.BulkReadRecordAsync<StudentPrograms>("", true)).Returns(Task.FromResult(stuProgResponseData));

        //        //dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AcadPrograms>("ACAD.PROGRAMS", It.IsAny<string[]>(), true)).ReturnsAsync(acadProgResponseData);
        //        dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AcadPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(acadProgResponseData);

        //        dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(studentResponseData);
        //        //dataReaderMock.Setup<Task<Collection<AcadPrograms>>>(acc => acc.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS",It.IsAny<string[]>(), true)).Returns(Task.FromResult(acadProgResponseData));
        //        // get one student program
        //        dataReaderMock.Setup<Task<StudentPrograms>>(acc => acc.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(stuProgResponseData[0]));

        //        dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.PROGRAMS", "")).Returns(Task.FromResult(new string[] { "1", "2", "3" }));

        //        dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
        //        {
        //            var result = new Dictionary<string, GuidLookupResult>();
        //            foreach (var gl in gla)
        //            {
        //                var stuprog = stuProgResponseData.FirstOrDefault(x => x.RecordGuid == gl.Guid);
        //                result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = stuprog.Recordkey });
        //            }
        //            return Task.FromResult(result);
        //        });

        //        dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(
        //            (id, repl) => Task.FromResult(stuProgResponseData.FirstOrDefault(c => c.Recordkey == id)));
        //        dataReaderMock.Setup(acc => acc.ReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string>(), true)).Returns<string>(
        //            id => Task.FromResult(stuProgResponseData.FirstOrDefault(c => c.Recordkey == id)));
                
        //        // mock data accessor STUDENT.PROGRAM.STATUSES
        //        dataReaderMock.Setup<Task<ApplValcodes>>(a =>
        //            a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true))
        //            .ReturnsAsync(new ApplValcodes()
        //            {
        //                ValInternalCode = new List<string>() { "A", "C" },
        //                ValExternalRepresentation = new List<string>() { "Active", "Changed" },
        //                ValActionCode1 = new List<string>() { "2", "4" },
        //                ValsEntityAssociation = new List<ApplValcodesVals>()
        //                {
        //                    new ApplValcodesVals() 
        //                    {
        //                        ValInternalCodeAssocMember = "A",
        //                        ValExternalRepresentationAssocMember = "Active",
        //                        ValActionCode1AssocMember = "2"
        //                    },
        //                    new ApplValcodesVals() 
        //                    {
        //                        ValInternalCodeAssocMember = "C",
        //                        ValExternalRepresentationAssocMember = "Changed",
        //                        ValActionCode1AssocMember = "4"
        //                    }
        //                }
        //            });
        //        //mock for majors
        //        var majs = new List<Major>()
        //        {
        //            new Major("ENGL", "English"),
        //            new Major("MATH", "Mathematics")
        //        };
        //        studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetMajorsAsync()).ReturnsAsync(majs);

        //        //mock for minors
        //        var minors = new List<Minor>()
        //        {
        //            new Minor("HIST", "History"),
        //            new Minor("ACCT", "Accounting")
        //        };
        //        studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetMinorsAsync()).ReturnsAsync(minors);

        //        //mock for specializations
        //        var sps = new List<Specialization>()
        //        {
        //            new Specialization("CERT", "Certification"),
        //            new Specialization("SCIE", "Sciences")
        //        };
        //        studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetSpecializationsAsync()).ReturnsAsync(sps);

        //        //mock for ccds
        //        var ccd = new List<Ccd>()
        //        {
        //            new Ccd("ELE", "Elementary Education"),
        //            new Ccd( "DI", "Diploma")
        //        };
        //        studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetCcdsAsync()).ReturnsAsync(ccd);

        //        stuRefData = studentReferenceDataRepositoryMock.Object;

        //        // Construct student program repository
        //        stuProgRepo = new StudentProgramRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
      
                
        //        return stuProgRepo;
        //    }

        //    [TestMethod]
        //    public async Task StudentProgramRepository_GetAcademicProgramEnrollmentByGuidAsync()
        //    {
        //        string stuProgId = "12345678*BA-MATH";
        //        var stuprog = stuProgResponseData.FirstOrDefault(c => c.Recordkey == stuProgId);
        //        string guid = stuprog.RecordGuid;
        //        StudentProgram result = await stuProgRepo.GetAcademicProgramEnrollmentByGuidAsync(guid);
        //        Assert.AreEqual(stuprog.Recordkey, string.Concat(result.StudentId, "*", result.ProgramCode));
        //        Assert.AreEqual(stuprog.RecordGuid, result.Guid);
        //        Assert.AreEqual(stuprog.StprCatalog, result.CatalogCode);
        //        Assert.AreEqual(stuprog.StprStartDate.FirstOrDefault(), result.StartDate);
        //        Assert.AreEqual(stuprog.StprLocation, result.Location);
        //        Assert.AreEqual(stuprog.StprIntgStartTerm, result.StartTerm);
        //        Assert.AreEqual(stuprog.StprEndDate.FirstOrDefault(), result.EndDate);
        //        Assert.AreEqual(stuprog.StprStatus.FirstOrDefault(), result.Status);


        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public async Task StudentProgramRepository_GetAcademicProgramEnrollmentByGuidAsync_ArgumentNullException_NoGuid()
        //    {
        //        //Arrange
        //        StudentProgram result = await stuProgRepo.GetAcademicProgramEnrollmentByGuidAsync(null);

        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task StudentProgramRepository_GetAcademicProgramEnrollmentByGuidAsync_NullPrimaryKey()
        //    {
        //        var stuProg = allStuProgs.FirstOrDefault();
        //        StudentPrograms stProg = stuProgResponseData.FirstOrDefault();

        //        var guid = stProg.RecordGuid;
        //        var id = stProg.Recordkey;
        //        var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = null };
        //        var guidLookupDict = new Dictionary<string, GuidLookupResult>();
        //        dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
        //        {
        //            if (gla.Any(gl => gl.Guid == guid))
        //            {
        //                guidLookupDict.Add(guid, guidLookupResult);
        //            }
        //            return Task.FromResult(guidLookupDict);
        //        });

        //        dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentPrograms>(stProg.Recordkey,true)).ReturnsAsync(stProg);


        //        // Verify that the GetById returns a grade in the test repository
        //        // with fields properly initialized
        //        await stuProgRepo.GetAcademicProgramEnrollmentByGuidAsync(guid);
        //    }

        //    //[TestMethod]
        //    //public async Task StudentProgramRepository_CreateAcademicProgramEnrollmentAsync()
        //    //{
        //    //    var stuProgEntity = allStuProgs.FirstOrDefault();
        //    //    var result = await stuProgRepo.CreateAcademicProgramEnrollmentAsync(stuProgEntity);

        //    //    Assert.AreEqual(stuProgEntity.ProgramCode, result.ProgramCode);
        //    //    Assert.AreEqual(stuProgEntity.Guid, result.Guid);
        //    //    Assert.AreEqual(stuProgEntity.CatalogCode, result.CatalogCode);
        //    //    Assert.AreEqual(stuProgEntity.StartDate, result.StartDate);
        //    //    Assert.AreEqual(stuProgEntity.Location, result.Location);
        //    //    Assert.AreEqual(stuProgEntity.EndDate, result.EndDate);
        //    //    Assert.AreEqual(stuProgEntity.Status, result.Status);
        //    //    Assert.AreEqual(stuProgEntity.DegreeCode, result.DegreeCode);
        //    //     var dispCnt = 0;
        //    //    foreach (var dis in result.AdditionalRequirements)
        //    //    {
        //    //        Assert.AreEqual(dis.AwardCode, stuProgEntity.AdditionalRequirements[dispCnt].AwardCode);
        //    //        Assert.AreEqual(dis.AwardName, stuProgEntity.AdditionalRequirements[dispCnt].AwardName);
        //    //        Assert.AreEqual(dis.RequirementCode, stuProgEntity.AdditionalRequirements[dispCnt].RequirementCode);
        //    //        Assert.AreEqual(dis.Type, stuProgEntity.AdditionalRequirements[dispCnt].Type);
        //    //        dispCnt++;
        //    //    }
        //    //    var credCnt = 0;
        //    //    foreach (var dis in result.StudentProgramMajors)
        //    //    {
        //    //        Assert.AreEqual(dis.Code, stuProgEntity.StudentProgramMajors[credCnt].Code);
        //    //        credCnt++;
        //    //    }
        //    //    var minCnt = 0;
        //    //    foreach (var dis in result.StudentProgramMinors)
        //    //    {
        //    //        Assert.AreEqual(dis.Code, stuProgEntity.StudentProgramMinors[minCnt].Code);
        //    //        credCnt++;
        //    //    }

        //    //}


        //    //[TestMethod]
        //    //public async Task StudentProgramRepository_GetAcademicProgramEnrollmentsAsync()
        //    //{
        //    //    string stuProgId = "12345678*BA-MATH";
        //    //    var stuprog = stuProgResponseData.FirstOrDefault(c => c.Recordkey == stuProgId);
        //    //    string guid = stuprog.RecordGuid;
        //    //    string id = stuprog.Recordkey;

        //    //    var guidLookupResult = new GuidLookupResult() { Entity = "STUDENT.PROGRAMS", PrimaryKey = id };
        //    //    var guidLookupDict = new Dictionary<string, GuidLookupResult>();
        //    //    guidLookupDict.Add(guid, guidLookupResult);
        //    //    //dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
        //    //    //{
        //    //    //    if (gla.Any(gl => gl.Guid == guid))
        //    //    //    {
        //    //    //        guidLookupDict.Add(guid, guidLookupResult);
        //    //    //    }
        //    //    //    return Task.FromResult(guidLookupDict);
        //    //    //});

        //    //    //dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.StudentPrograms>(stuprog.Recordkey, true)).ReturnsAsync(stuprog); 


        //    //    //var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) })
        //    //    //.ConfigureAwait(false);
        //    //    //var foundEntry = idDict.FirstOrDefault();
        //    //    //return foundEntry.Value != null ? foundEntry.Value.PrimaryKey : null;

        //    //    dataReaderMock.Setup(x => x.SelectAsync(new GuidLookup[] { new GuidLookup(It.IsAny<string>()) })).Returns(Task.FromResult(guidLookupDict));

        //    //    StudentProgram result = await stuProgRepo.GetAcademicProgramEnrollmentByGuidAsync(guid);
        //    //    Assert.AreEqual(stuprog.Recordkey, string.Concat(result.StudentId, "*", result.ProgramCode));
        //    //    Assert.AreEqual(stuprog.RecordGuid, result.Guid);
        //    //    Assert.AreEqual(stuprog.StprCatalog, result.CatalogCode);
        //    //    Assert.AreEqual(stuprog.StprStartDate.FirstOrDefault(), result.StartDate);
        //    //    Assert.AreEqual(stuprog.StprLocation, result.Location);
        //    //    Assert.AreEqual(stuprog.StprEndDate.FirstOrDefault(), result.EndDate);
        //    //    Assert.AreEqual(stuprog.StprStatus.FirstOrDefault(), result.Status);


        //    //}
        //}
        //#endregion


    }
}