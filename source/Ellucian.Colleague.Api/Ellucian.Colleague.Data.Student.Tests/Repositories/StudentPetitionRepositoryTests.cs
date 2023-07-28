// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentPetitionRepositoryTests
    {
        [TestClass]
        public class StudentPetitionRepository_GetStudentPetitionsAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;

            Collection<StudentPetitions> studentPetitionsResponseData;
            Collection<StuPetitionCmnts> stuPetitionCmntsResponseData;
            StudentPetitionRepository studentPetitionRepository;
            string petitionMultiLineComment;
            string consentMultiLineComment;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                studentPetitionsResponseData = BuildStudentPetitionsResponse();
                stuPetitionCmntsResponseData = BuildStudentPetitionCommentResponse();

                studentPetitionRepository = BuildValiStudentPetitionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentPetitionsResponseData = null;
                stuPetitionCmntsResponseData = null;
                studentPetitionRepository = null;
            }

            [TestMethod]
            public async Task GetsAllStudentPetitions()
            {
                var studentPetitions = await studentPetitionRepository.GetStudentPetitionsAsync("0000123");
                Assert.AreEqual(5, studentPetitions.Count(s => s.Type == StudentPetitionType.StudentPetition));
                Assert.AreEqual(5, studentPetitions.Count(s => s.Type == StudentPetitionType.FacultyConsent));
            }



            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentPetitionRepository studentPetitionRepository = BuildInvalidStudentPetitionRepository();
                var studentPetitions = await studentPetitionRepository.GetStudentPetitionsAsync("0000123");
            }


            [TestMethod]
            public async Task EmptyRepositoryDataReturnsEmptyLists()
            {
                var sectionPermissions = await studentPetitionRepository.GetStudentPetitionsAsync("0000999");
                Assert.AreEqual(0, sectionPermissions.Where(s => s.Type == StudentPetitionType.StudentPetition).Count());
                Assert.AreEqual(0, sectionPermissions.Where(s => s.Type == StudentPetitionType.FacultyConsent).Count());
            }

            [TestMethod]
            public async Task NullRepositoryDataReturnsEmptyLists()
            {
                var nullResponse = new Collection<StudentPetitions>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(nullResponse));
                var nullCommentResponse = new Collection<StuPetitionCmnts>();
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(nullCommentResponse));
                var sectionPermissions = await studentPetitionRepository.GetStudentPetitionsAsync("0000123");
                Assert.AreEqual(0, sectionPermissions.Count(s => s.Type == StudentPetitionType.StudentPetition));
                Assert.AreEqual(0, sectionPermissions.Count(s => s.Type == StudentPetitionType.FacultyConsent));
            }

            [TestMethod]
            public async Task NoPetitionsRepositoryDataReturnsEmptyLists()
            {
                var studentPetitionRepository = BuildEmptyPetitionsExcludeCmntsRepository();
                var studentPetitions = await studentPetitionRepository.GetStudentPetitionsAsync("0000123");
                Assert.AreEqual(0, studentPetitions.Count());
            }

            [TestMethod]
            public async Task ReturnsOnlyValidItems()
            {
                var mixedResponseData = studentPetitionsResponseData;

                // Add an  invalid student petition to the normal response
                var invalidStudentPetitionsData = new StudentPetitions();
                invalidStudentPetitionsData.Recordkey = "6";
                invalidStudentPetitionsData.StpeStudent = "0001111";
                invalidStudentPetitionsData.StpeTerm = "2016/SP";
                invalidStudentPetitionsData.StudentPetitionsChgdate = new DateTime(2015, 01, 13);
                var chTime = DateTime.MinValue;
                chTime.AddHours(13);
                chTime.AddMinutes(01);
                chTime.AddSeconds(15);
                invalidStudentPetitionsData.StudentPetitionsChgtime = chTime;
                invalidStudentPetitionsData.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();
                invalidStudentPetitionsData.StudentPetitionsChgopr = "AAA";
                invalidStudentPetitionsData.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "MATH-101",
                    StpeSectionAssocMember = "SEC3",
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "D",
                    StpePetitionReasonCodeAssocMember = null,
                    StpeConsentReasonCodeAssocMember = null,
                    StpeStuPetitionCmntsIdAssocMember = null,
                    StpePetitionStatusSetByAssocMember = "FacultyId5",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-1),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId6",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-2),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });

                mixedResponseData.Add(invalidStudentPetitionsData);
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(mixedResponseData));

                // Verify that the original section permission response items are returned even though an additional item has invalid data
                var sectionPermissions = await studentPetitionRepository.GetStudentPetitionsAsync("0000123");
                Assert.AreEqual(5, sectionPermissions.Where(s => s.Type == StudentPetitionType.StudentPetition).Count());
                Assert.AreEqual(5, sectionPermissions.Where(s => s.Type == StudentPetitionType.FacultyConsent).Count());
            }



            private Collection<StudentPetitions> BuildStudentPetitionsResponse()
            {
                Collection<StudentPetitions> repoStudentPetitions = new Collection<StudentPetitions>();

                // This student has reason codes but does not have a petition comment or consent comment
                var studentPetitionsData1 = new StudentPetitions();
                studentPetitionsData1.Recordkey = "1";
                studentPetitionsData1.StpeStudent = "0000123";
                studentPetitionsData1.StpeTerm = "2016/SP";
                studentPetitionsData1.StudentPetitionsChgdate = new DateTime(2015, 01, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(01);
                chTime1.AddSeconds(15);
                studentPetitionsData1.StudentPetitionsChgtime = chTime1;
                studentPetitionsData1.StudentPetitionsChgopr = "SSS";
                studentPetitionsData1.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();

                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-101",
                    StpeSectionAssocMember = "SEC1",
                    StpePetitionStatusAssocMember = "A",//Petition status(D, A, NULL)
                    StpeFacultyConsentAssocMember = "A", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = null,
                    StpePetitionStatusSetByAssocMember = "FacultyId1",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-10),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId2",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-20),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)

                });
                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-102",
                    StpeSectionAssocMember = "SEC2",
                    StpePetitionStatusAssocMember = "A",//Petition status(D, A, NULL)
                    StpeFacultyConsentAssocMember = "A", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = null,
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                repoStudentPetitions.Add(studentPetitionsData1);

                // This student has a petition comment and consent comment and reason codes
                var studentPetitionsData2 = new StudentPetitions();
                studentPetitionsData2.Recordkey = "2";
                studentPetitionsData2.StpeStudent = "0000123";
                studentPetitionsData2.StpeTerm = "2016/SP";
                studentPetitionsData2.StudentPetitionsChgdate = new DateTime(2015, 01, 21);
                var chTime2 = DateTime.MinValue;
                chTime2.AddHours(21);
                chTime2.AddMinutes(01);
                chTime2.AddSeconds(15);
                studentPetitionsData2.StudentPetitionsChgtime = chTime2;
                studentPetitionsData2.StudentPetitionsChgopr = "LDG";
                studentPetitionsData2.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();

                studentPetitionsData2.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-101",
                    StpeSectionAssocMember = "SEC1",
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "D",
                    StpePetitionReasonCodeAssocMember = "OVHM",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "1",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                repoStudentPetitions.Add(studentPetitionsData2);

                //student has a consent comment and petition comment but no reason codes
                var studentPetitionsData3 = new StudentPetitions();
                studentPetitionsData3.Recordkey = "3";
                studentPetitionsData3.StpeStudent = "0000123";
                studentPetitionsData3.StpeTerm = "2016/SP";
                studentPetitionsData3.StudentPetitionsChgdate = new DateTime(2015, 01, 22);
                var chTime3 = DateTime.MinValue;
                chTime3.AddHours(22);
                chTime3.AddMinutes(01);
                chTime3.AddSeconds(15);
                studentPetitionsData3.StudentPetitionsChgtime = chTime3;
                studentPetitionsData3.StudentPetitionsChgopr = "CRS";
                studentPetitionsData3.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();

                studentPetitionsData3.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-101",
                    StpeSectionAssocMember = "SEC1",
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "A",
                    StpePetitionReasonCodeAssocMember = null,
                    StpeConsentReasonCodeAssocMember = null,
                    StpeStuPetitionCmntsIdAssocMember = "2",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                repoStudentPetitions.Add(studentPetitionsData3);

                // This student has a petition and petition comment but no reason codes, no faculty consent data but no faculty consent status so no load.
                var studentPetitionsData4 = new StudentPetitions();
                studentPetitionsData4.Recordkey = "4";
                studentPetitionsData4.StpeStudent = "0003964";
                studentPetitionsData4.StpeTerm = "2016/SP";
                studentPetitionsData4.StudentPetitionsChgdate = new DateTime(2015, 01, 12);
                var chTime4 = DateTime.MinValue;
                chTime4.AddHours(12);
                chTime4.AddMinutes(01);
                chTime4.AddSeconds(15);
                studentPetitionsData4.StudentPetitionsChgtime = chTime4;
                studentPetitionsData4.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();
                studentPetitionsData4.StudentPetitionsChgopr = "SSS";
                studentPetitionsData4.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-101",
                    StpeSectionAssocMember = "SEC1",
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "",
                    StpePetitionReasonCodeAssocMember = null,
                    StpeConsentReasonCodeAssocMember = null,
                    StpeStuPetitionCmntsIdAssocMember = "3",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                repoStudentPetitions.Add(studentPetitionsData4);

                // This student has a petition with a reason code but no petition comment, and faculty consent reson code and a consent comment
                var studentPetitionsData5 = new StudentPetitions();
                studentPetitionsData5.Recordkey = "5";
                studentPetitionsData5.StpeStudent = "0000123";
                studentPetitionsData5.StpeTerm = "2016/SP";
                studentPetitionsData5.StudentPetitionsChgdate = new DateTime(2015, 01, 11);
                var chTime5 = DateTime.MinValue;
                chTime5.AddHours(11);
                chTime5.AddMinutes(01);
                chTime5.AddSeconds(15);
                studentPetitionsData5.StudentPetitionsChgtime = chTime5;
                studentPetitionsData5.PetitionsEntityAssociation = new List<StudentPetitionsPetitions>();
                studentPetitionsData5.StudentPetitionsChgopr = "AAA";
                studentPetitionsData5.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-101",
                    StpeSectionAssocMember = "SEC1",
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "D",
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = null,
                    StpeStuPetitionCmntsIdAssocMember = "4",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                repoStudentPetitions.Add(studentPetitionsData5);

                return repoStudentPetitions;
            }

            private Collection<StuPetitionCmnts> BuildStudentPetitionCommentResponse()
            {
                var repoStuPetitionCmntsData = new Collection<StuPetitionCmnts>();

                petitionMultiLineComment = "Student 456 ART-101 Petition comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";
                consentMultiLineComment = "Student 456 ART-101 Consent comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "1",
                    StpcPetitionComments = petitionMultiLineComment,
                    StpcConsentComments = consentMultiLineComment
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "2",
                    StpcPetitionComments = "Student 789 ART-101 Petition comment.",
                    StpcConsentComments = "Student 789 ART-101 Consent comment."
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "3",
                    StpcPetitionComments = "Student 3964 ART-101 Petition comment.",
                    StpcConsentComments = null
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "4",
                    StpcPetitionComments = null,
                    StpcConsentComments = "Student 1111 ART-101 Consent comment."
                });

                return repoStuPetitionCmntsData;
            }

            private StudentPetitionRepository BuildValiStudentPetitionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for petition request
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(studentPetitionsResponseData));
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(stuPetitionCmntsResponseData));

                StudentPetitionRepository repository = new StudentPetitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentPetitionRepository BuildInvalidStudentPetitionRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Throws(expectedFailure);

                StudentPetitionRepository repository = new StudentPetitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
            private StudentPetitionRepository BuildEmptyPetitionsExcludeCmntsRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                var noPetitionsResponse = new Collection<StudentPetitions>();
                var noCommentsResponse = new Collection<StuPetitionCmnts>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for petition comments empty request
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(noPetitionsResponse));
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(noCommentsResponse));

                StudentPetitionRepository repository = new StudentPetitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
        }

        [TestClass]
        public class StudentPetitionRepository_GetStudentOverloadPetitionsAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;

            Collection<StudentPetitions> studentPetitionsResponseData;
            StudentPetitionRepository studentPetitionRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                studentPetitionsResponseData = BuildStudentOverloadPetitionsResponse();
                studentPetitionRepository = BuildValidStudentPetitionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentPetitionsResponseData = null;
                studentPetitionRepository = null;
            }

            [TestMethod]
            public async Task GetsAllStudentPetitions()
            {
                var studentOverloadPetitions = await studentPetitionRepository.GetStudentOverloadPetitionsAsync("0000123");
                Assert.AreEqual(2, studentOverloadPetitions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                StudentPetitionRepository studentPetitionRepository = BuildInvalidStudentPetitionRepository();
                var studentOverloadPetitions = await studentPetitionRepository.GetStudentOverloadPetitionsAsync("0000123");
            }

            [TestMethod]
            public async Task EmptyRepositoryDataReturnsEmptyLists()
            {
                var studentOverloadPetitions = await studentPetitionRepository.GetStudentOverloadPetitionsAsync("0000999");
                Assert.AreEqual(0, studentOverloadPetitions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentStringNull()
            {
                var studentOverloadPetitions = await studentPetitionRepository.GetStudentOverloadPetitionsAsync(null);
            }

            [TestMethod]
            public async Task NullRepositoryDataReturnsEmptyLists()
            {
                var nullResponse = new Collection<StudentPetitions>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(nullResponse));
                var studentOverloadPetitions = await studentPetitionRepository.GetStudentOverloadPetitionsAsync("0000123");
                Assert.AreEqual(0, studentOverloadPetitions.Count());
            }

            private Collection<StudentPetitions> BuildStudentOverloadPetitionsResponse()
            {
                Collection<StudentPetitions> repoStudentPetitions = new Collection<StudentPetitions>();

                // This student has reason codes but does not have a petition comment or consent comment
                var studentPetitionsData1 = new StudentPetitions();
                studentPetitionsData1.Recordkey = "1";
                studentPetitionsData1.StpeStudent = "0000123";
                studentPetitionsData1.StpeTerm = "2016/SP";
                studentPetitionsData1.StpeOverloadPetition = "A";
                studentPetitionsData1.StudentPetitionsChgdate = new DateTime(2015, 01, 14);
                var chTime1 = DateTime.MinValue;
                chTime1.AddHours(14);
                chTime1.AddMinutes(01);
                chTime1.AddSeconds(15);
                studentPetitionsData1.StudentPetitionsChgtime = chTime1;
                studentPetitionsData1.StudentPetitionsChgopr = "SSS";
                repoStudentPetitions.Add(studentPetitionsData1);

                // This student has a petition comment and consent comment and reason codes
                var studentPetitionsData2 = new StudentPetitions();
                studentPetitionsData2.Recordkey = "2";
                studentPetitionsData2.StpeStudent = "0000123";
                studentPetitionsData2.StpeTerm = "2016/SP";
                studentPetitionsData2.StpeOverloadPetition = "D";
                studentPetitionsData2.StudentPetitionsChgdate = new DateTime(2015, 01, 21);
                var chTime2 = DateTime.MinValue;
                chTime2.AddHours(21);
                chTime2.AddMinutes(01);
                chTime2.AddSeconds(15);
                studentPetitionsData2.StudentPetitionsChgtime = chTime2;
                studentPetitionsData2.StudentPetitionsChgopr = "LDG";
                repoStudentPetitions.Add(studentPetitionsData2);
                return repoStudentPetitions;
            }

            private StudentPetitionRepository BuildValidStudentPetitionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for petition request
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(studentPetitionsResponseData));

                StudentPetitionRepository repository = new StudentPetitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentPetitionRepository BuildInvalidStudentPetitionRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Throws(expectedFailure);

                StudentPetitionRepository repository = new StudentPetitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
        }
    }
}

