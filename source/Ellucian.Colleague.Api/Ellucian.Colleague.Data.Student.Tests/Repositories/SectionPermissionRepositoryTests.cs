// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class SectionPermissionRepositoryTests
    {
        [TestClass]
        public class StudentPermissionRepository_GetSectionPermissionAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;

            Collection<StudentPetitions> studentPetitionsResponseData;
            Collection<StuPetitionCmnts> stuPetitionCmntsResponseData;
            SectionPermissionRepository sectionPermissionRepository;
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

                sectionPermissionRepository = BuildValiSectionPermissionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentPetitionsResponseData = null;
                stuPetitionCmntsResponseData = null;
                sectionPermissionRepository = null;
            }

            [TestMethod]
            public async Task GetsAllSectionPermission()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                Assert.AreEqual(5, sectionPermissions.StudentPetitions.Count());
                Assert.AreEqual(4, sectionPermissions.FacultyConsents.Count());
            }

            [TestMethod]
            public async Task SectionPermission_Initialized()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                foreach (var response in studentPetitionsResponseData)
                {
                    var responsePetition = response.PetitionsEntityAssociation.Where(rp => rp.StpeSectionAssocMember == "SEC1").FirstOrDefault();
                    if (!string.IsNullOrEmpty(responsePetition.StpePetitionStatusAssocMember) || responsePetition.StpeSectionAssocMember != "SEC1")
                    {
                        var studentPetition = sectionPermissions.StudentPetitions.Where(sp => sp.Id == response.Recordkey).FirstOrDefault();
                        if (studentPetition != null)
                        {
                            Assert.AreEqual(response.Recordkey, studentPetition.Id);
                            Assert.AreEqual(response.StpeStudent, studentPetition.StudentId);
                            Assert.AreEqual(response.StpeTerm, studentPetition.TermCode);
                            Assert.AreEqual(response.StpeStartDate, studentPetition.StartDate);
                            Assert.AreEqual(response.StpeEndDate, studentPetition.EndDate);
                            Assert.AreEqual(responsePetition.StpePetitionStatusSetByAssocMember, studentPetition.SetBy);
                            Assert.AreEqual(response.StudentPetitionsChgopr, studentPetition.UpdatedBy);
                            Assert.AreEqual(responsePetition.StpePetitionStatusDateAssocMember, new DateTime(studentPetition.DateTimeChanged.Year, studentPetition.DateTimeChanged.Month, studentPetition.DateTimeChanged.Day));
                            Assert.AreEqual(new TimeSpan(responsePetition.StpePetitionStatusTimeAssocMember.Value.Hour, responsePetition.StpePetitionStatusTimeAssocMember.Value.Minute, responsePetition.StpePetitionStatusTimeAssocMember.Value.Second),
                                new TimeSpan(studentPetition.DateTimeChanged.Hour, studentPetition.DateTimeChanged.Minute, studentPetition.DateTimeChanged.Second));
                            Assert.AreEqual(responsePetition.StpeCoursesAssocMember, studentPetition.CourseId);
                            Assert.AreEqual(responsePetition.StpeSectionAssocMember, studentPetition.SectionId);
                            Assert.AreEqual(responsePetition.StpePetitionStatusAssocMember, studentPetition.StatusCode);
                            Assert.AreEqual(responsePetition.StpePetitionReasonCodeAssocMember, studentPetition.ReasonCode);
                        }
                    }

                    if (!string.IsNullOrEmpty(responsePetition.StpeFacultyConsentAssocMember) || responsePetition.StpeSectionAssocMember != "SEC1")
                    {
                        var facultyConsent = sectionPermissions.FacultyConsents.Where(sp => sp.Id == response.Recordkey).FirstOrDefault();
                        // One of the responses will not generate a petition because it is lacking either a comment or reason. Need to do a null check. 
                        if (facultyConsent != null)
                        {
                            Assert.AreEqual(response.Recordkey, facultyConsent.Id);
                            Assert.AreEqual(response.StpeStudent, facultyConsent.StudentId);
                            Assert.AreEqual(response.StpeTerm, facultyConsent.TermCode);
                            Assert.AreEqual(response.StpeStartDate, facultyConsent.StartDate);
                            Assert.AreEqual(response.StpeEndDate, facultyConsent.EndDate);
                            Assert.AreEqual(responsePetition.StpeFacultyConsentSetByAssocMember, facultyConsent.SetBy);
                            Assert.AreEqual(response.StudentPetitionsChgopr, facultyConsent.UpdatedBy);
                            Assert.AreEqual(responsePetition.StpeFacultyConsentDateAssocMember, new DateTime(facultyConsent.DateTimeChanged.Year, facultyConsent.DateTimeChanged.Month, facultyConsent.DateTimeChanged.Day));
                            Assert.AreEqual(new TimeSpan(responsePetition.StpeFacultyConsentTimeAssocMember.Value.Hour, responsePetition.StpeFacultyConsentTimeAssocMember.Value.Minute, responsePetition.StpeFacultyConsentTimeAssocMember.Value.Second),
                                new TimeSpan(facultyConsent.DateTimeChanged.Hour, facultyConsent.DateTimeChanged.Minute, facultyConsent.DateTimeChanged.Second));
                            Assert.AreEqual(responsePetition.StpeCoursesAssocMember, facultyConsent.CourseId);
                            Assert.AreEqual(responsePetition.StpeSectionAssocMember, facultyConsent.SectionId);
                            Assert.AreEqual(responsePetition.StpeFacultyConsentAssocMember, facultyConsent.StatusCode);
                            Assert.AreEqual(responsePetition.StpeConsentReasonCodeAssocMember, facultyConsent.ReasonCode);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task SectionPermissionComments_Initialized()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                foreach (var response in stuPetitionCmntsResponseData)
                {
                    var expectedPetitionCommentLines = response.StpcPetitionComments != null ? response.StpcPetitionComments.Replace(DmiString._VM, '\n') : null;
                    var expectedConsentCommentLines = response.StpcConsentComments != null ? response.StpcConsentComments.Replace(DmiString._VM, '\n') : null;

                    var studentPetitionComment = sectionPermissions.StudentPetitions.Where(sp => sp.Comment == expectedPetitionCommentLines).Select(sp => sp.Comment).FirstOrDefault();
                    var facultyConsentComment = sectionPermissions.FacultyConsents.Where(fc => fc.Comment == expectedConsentCommentLines).Select(fc => fc.Comment).FirstOrDefault();

                    Assert.AreEqual(expectedPetitionCommentLines, studentPetitionComment);
                    Assert.AreEqual(expectedConsentCommentLines, facultyConsentComment);
                }
            }

            [TestMethod]
            public async Task ReplacesValueMarksWithLineBreaksInComment()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                var studentPetition = sectionPermissions.StudentPetitions.Where(sp => sp.Id == "2").FirstOrDefault();
                var facultyConsent = sectionPermissions.FacultyConsents.Where(sp => sp.Id == "2").FirstOrDefault();

                var expectedPetitionCommentLines = petitionMultiLineComment.Replace(DmiString._VM, '\n');
                var expectedConsentCommentLines = consentMultiLineComment.Replace(DmiString._VM, '\n');

                Assert.AreEqual(expectedPetitionCommentLines, studentPetition.Comment);
                Assert.AreEqual(expectedConsentCommentLines, facultyConsent.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfAccessReturnsException()
            {
                SectionPermissionRepository sectionPermissionRepository = BuildInvaliSectionPermissionRepository();
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentNull()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentEmpty()
            {
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync(string.Empty);
            }

            [TestMethod]
            public async Task EmptyRepositoryDataReturnsEmptyLists()
            {
                // Call Section Permission Repository with a non-existant section
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("9999");
                Assert.AreEqual(0, sectionPermissions.StudentPetitions.Count());
                Assert.AreEqual(0, sectionPermissions.FacultyConsents.Count());
            }

            [TestMethod]
            public async Task NullRepositoryDataReturnsEmptyLists()
            {
                var nullResponse = new Collection<StudentPetitions>();
                nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(nullResponse));
                var nullCommentResponse = new Collection<StuPetitionCmnts>();
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(nullCommentResponse));
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                Assert.AreEqual(0, sectionPermissions.StudentPetitions.Count());
                Assert.AreEqual(0, sectionPermissions.FacultyConsents.Count());
            }

            [TestMethod]
            public async Task NoPetitionsRepositoryDataReturnsEmptyLists()
            {
                var sectionPermissionRepository = BuildEmptyPetitionsExcludeCmntsRepository();
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                Assert.AreEqual(0, sectionPermissions.StudentPetitions.Count());
                Assert.AreEqual(0, sectionPermissions.FacultyConsents.Count());
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
                var sectionPermissions = await sectionPermissionRepository.GetSectionPermissionAsync("SEC1");
                Assert.AreEqual(5, sectionPermissions.StudentPetitions.Count());
                Assert.AreEqual(4, sectionPermissions.FacultyConsents.Count());
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
                studentPetitionsData2.StpeStudent = "0000456";
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
                studentPetitionsData3.StpeStudent = "0000789";
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
                studentPetitionsData5.StpeStudent = "0001111";
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

            private SectionPermissionRepository BuildValiSectionPermissionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for petition request
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<StudentPetitions>>(studentPetitionsResponseData));
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(stuPetitionCmntsResponseData));

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private SectionPermissionRepository BuildInvaliSectionPermissionRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up data accessor for mocking 
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                Exception expectedFailure = new Exception("fail");
                dataAccessorMock.Setup<Task<Collection<StudentPetitions>>>(acc => acc.BulkReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Throws(expectedFailure);

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return repository;
            }
            private SectionPermissionRepository BuildEmptyPetitionsExcludeCmntsRepository()
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

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

        }

        [TestClass]
        public class StudentPermissionRepository_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;


            StudentPetitions studentPetitionsResponseData;
            Collection<StuPetitionCmnts> stuPetitionCmntsResponseData;
            SectionPermissionRepository sectionPermissionRepository;
            string petitionMultiLineComment;
            string consentMultiLineComment;
            string studentPetitionId = "1";
            string sectionId = "SEC1";

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Collection of data accessor responses
                studentPetitionsResponseData = BuildStudentPetitionsResponse();
                stuPetitionCmntsResponseData = BuildStudentPetitionCommentResponse();

                sectionPermissionRepository = BuildValiSectionPermissionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentPetitionsResponseData = null;
                stuPetitionCmntsResponseData = null;
                sectionPermissionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentPetitionIdArgumentNull()
            {
                var sectionPermissions = await sectionPermissionRepository.GetAsync(null, sectionId, StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentNull()
            {
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, null, StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfStudentPetitionIdArgumentEmpty()
            {
                var sectionPermissions = await sectionPermissionRepository.GetAsync(string.Empty, sectionId, StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfSectionIdArgumentEmpty()
            {
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, string.Empty, StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsKeyNotFoundExceptionIfDataReaderReturnsNull()
            {
                // Set up repo response for petition request
                StudentPetitions nullResponse = null;
                dataAccessorMock.Setup<Task<StudentPetitions>>(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, "JUNK", StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsKeyNotFoundExceptionIfStudentPetitionDoesNotHaveConsent()
            {
                // Set up repo response for petition request
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, "SEC3", StudentPetitionType.FacultyConsent);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsKeyNotFoundExceptionIfStudentPetitionDoesNotHaveType()
            {
                // Set up repo response for petition request
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, "SEC4", StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ThrowsExceptionIfExceptionThrownByDataReader()
            {
                dataAccessorMock.Setup<Task<StudentPetitions>>(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Throws(new Exception());
                var sectionPermissions = await sectionPermissionRepository.GetAsync(studentPetitionId, "JUNK", StudentPetitionType.StudentPetition);
            }

            [TestMethod]
            public async Task Get_ReturnsStudentPetition()
            {
                var studentPetition = await sectionPermissionRepository.GetAsync(studentPetitionId, sectionId, StudentPetitionType.StudentPetition);
                Assert.AreEqual(studentPetitionsResponseData.Recordkey, studentPetition.Id);
                Assert.AreEqual(studentPetitionsResponseData.StpeStartDate, studentPetition.StartDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeEndDate, studentPetition.EndDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeStudent, studentPetition.StudentId);
                Assert.AreEqual(studentPetitionsResponseData.StpeTerm, studentPetition.TermCode);
                Assert.AreEqual(StudentPetitionType.StudentPetition, studentPetition.Type);
                var studentPetitionSec1 = studentPetitionsResponseData.PetitionsEntityAssociation.Where(s => s.StpeSectionAssocMember == sectionId).FirstOrDefault();
                // Compare using the student petition fields. 
                Assert.AreEqual(studentPetitionSec1.StpeCoursesAssocMember, studentPetition.CourseId);
                Assert.AreEqual(studentPetitionSec1.StpePetitionReasonCodeAssocMember, studentPetition.ReasonCode);
                Assert.AreEqual(studentPetitionSec1.StpePetitionStatusAssocMember, studentPetition.StatusCode);
                Assert.AreEqual(studentPetitionSec1.StpePetitionStatusSetByAssocMember, studentPetition.SetBy);
                Assert.AreEqual(studentPetitionsResponseData.StudentPetitionsChgopr, studentPetition.UpdatedBy);
                Assert.AreEqual(studentPetitionSec1.StpePetitionStatusDateAssocMember, new DateTime(studentPetition.DateTimeChanged.Year, studentPetition.DateTimeChanged.Month, studentPetition.DateTimeChanged.Day));
                Assert.AreEqual(new TimeSpan(studentPetitionSec1.StpePetitionStatusTimeAssocMember.Value.Hour, studentPetitionSec1.StpePetitionStatusTimeAssocMember.Value.Minute, studentPetitionSec1.StpePetitionStatusTimeAssocMember.Value.Second),
                    new TimeSpan(studentPetition.DateTimeChanged.Hour, studentPetition.DateTimeChanged.Minute, studentPetition.DateTimeChanged.Second));
            }

            [TestMethod]
            public async Task Get_ReturnsFacultyConsent()
            {

                var studentPetition = await sectionPermissionRepository.GetAsync(studentPetitionId, sectionId, StudentPetitionType.FacultyConsent);
                Assert.AreEqual(studentPetitionsResponseData.Recordkey, studentPetition.Id);
                Assert.AreEqual(studentPetitionsResponseData.StpeStartDate, studentPetition.StartDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeEndDate, studentPetition.EndDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeStudent, studentPetition.StudentId);
                Assert.AreEqual(studentPetitionsResponseData.StpeTerm, studentPetition.TermCode);
                Assert.AreEqual(StudentPetitionType.FacultyConsent, studentPetition.Type);
                var studentPetitionSec1 = studentPetitionsResponseData.PetitionsEntityAssociation.Where(s => s.StpeSectionAssocMember == sectionId).FirstOrDefault();
                // This time compare using the faculty consent fields
                Assert.AreEqual(studentPetitionSec1.StpeCoursesAssocMember, studentPetition.CourseId);
                Assert.AreEqual(studentPetitionSec1.StpeConsentReasonCodeAssocMember, studentPetition.ReasonCode);
                Assert.AreEqual(studentPetitionSec1.StpeFacultyConsentAssocMember, studentPetition.StatusCode);
                Assert.AreEqual(studentPetitionSec1.StpeFacultyConsentSetByAssocMember, studentPetition.SetBy);
                Assert.AreEqual(studentPetitionsResponseData.StudentPetitionsChgopr, studentPetition.UpdatedBy);
                Assert.AreEqual(studentPetitionSec1.StpeFacultyConsentDateAssocMember, new DateTime(studentPetition.DateTimeChanged.Year, studentPetition.DateTimeChanged.Month, studentPetition.DateTimeChanged.Day));
                Assert.AreEqual(new TimeSpan(studentPetitionSec1.StpeFacultyConsentTimeAssocMember.Value.Hour, studentPetitionSec1.StpeFacultyConsentTimeAssocMember.Value.Minute, studentPetitionSec1.StpeFacultyConsentTimeAssocMember.Value.Second),
                    new TimeSpan(studentPetition.DateTimeChanged.Hour, studentPetition.DateTimeChanged.Minute, studentPetition.DateTimeChanged.Second));
            }

            [TestMethod]
            public async Task Get_ReturnsStudentPetition_CorrectComment()
            {

                var studentPetition = await sectionPermissionRepository.GetAsync(studentPetitionId, sectionId, StudentPetitionType.StudentPetition);
                Assert.AreEqual("Student 0000123 ART-101 Petition comment. Line1\ncomment line2\ncomment line3 the end", studentPetition.Comment);
            }

            [TestMethod]
            public async Task Get_ReturnsFacultyConsent_CorrectComment()
            {

                var studentPetition = await sectionPermissionRepository.GetAsync(studentPetitionId, sectionId, StudentPetitionType.FacultyConsent);
                Assert.AreEqual("Student 0000123 ART-101 Consent comment. Line1\ncomment line2\ncomment line3 the end", studentPetition.Comment);
            }

            [TestMethod]
            public async Task Get_ReturnsStudentPetitionWithNoComment()
            {
                var studentPetition = await sectionPermissionRepository.GetAsync(studentPetitionId, "SEC2", StudentPetitionType.StudentPetition);
                Assert.AreEqual(studentPetitionsResponseData.Recordkey, studentPetition.Id);
                Assert.AreEqual(studentPetitionsResponseData.StpeStartDate, studentPetition.StartDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeEndDate, studentPetition.EndDate);
                Assert.AreEqual(studentPetitionsResponseData.StpeStudent, studentPetition.StudentId);
                Assert.AreEqual(studentPetitionsResponseData.StpeTerm, studentPetition.TermCode);
                Assert.AreEqual(StudentPetitionType.StudentPetition, studentPetition.Type);
                var studentPetitionSec2 = studentPetitionsResponseData.PetitionsEntityAssociation.Where(s => s.StpeSectionAssocMember == "SEC2").FirstOrDefault();
                // Compare using the student petition fields. 
                Assert.AreEqual(studentPetitionSec2.StpeCoursesAssocMember, studentPetition.CourseId);
                Assert.AreEqual(studentPetitionSec2.StpePetitionReasonCodeAssocMember, studentPetition.ReasonCode);
                Assert.AreEqual(studentPetitionSec2.StpePetitionStatusAssocMember, studentPetition.StatusCode);
                Assert.AreEqual(studentPetitionSec2.StpePetitionStatusSetByAssocMember, studentPetition.SetBy);
                Assert.AreEqual(studentPetitionsResponseData.StudentPetitionsChgopr, studentPetition.UpdatedBy);
                Assert.AreEqual(studentPetitionSec2.StpePetitionStatusDateAssocMember, new DateTime(studentPetition.DateTimeChanged.Year, studentPetition.DateTimeChanged.Month, studentPetition.DateTimeChanged.Day));
                Assert.AreEqual(new TimeSpan(studentPetitionSec2.StpePetitionStatusTimeAssocMember.Value.Hour, studentPetitionSec2.StpePetitionStatusTimeAssocMember.Value.Minute, studentPetitionSec2.StpePetitionStatusTimeAssocMember.Value.Second),
                    new TimeSpan(studentPetition.DateTimeChanged.Hour, studentPetition.DateTimeChanged.Minute, studentPetition.DateTimeChanged.Second));
                Assert.IsNull(studentPetition.Comment);
            }

            private Collection<StuPetitionCmnts> BuildStudentPetitionCommentResponse()
            {
                // These would be all the comments associated to the specific student petition and they will all have same student but there could be different sections.

                var repoStuPetitionCmntsData = new Collection<StuPetitionCmnts>();

                petitionMultiLineComment = "Student 0000123 ART-101 Petition comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";
                consentMultiLineComment = "Student 0000123 ART-101 Consent comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "1",
                    StpcPetitionComments = petitionMultiLineComment,
                    StpcConsentComments = consentMultiLineComment
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "2",
                    StpcPetitionComments = string.Empty,
                    StpcConsentComments = "Student 0000123 ART-102 Consent comment."
                });

                return repoStuPetitionCmntsData;
            }

            private StudentPetitions BuildStudentPetitionsResponse()
            {
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
                    StpeFacultyConsentAssocMember = "D", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "1",
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
                    StpePetitionStatusAssocMember = "C",//Petition status(D, A, NULL)
                    StpeFacultyConsentAssocMember = "D", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "2",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    // This one has petition but no consent
                    StpeCoursesAssocMember = "ART-103",
                    StpeSectionAssocMember = "SEC3",
                    StpePetitionStatusAssocMember = "C",//Petition status(D, A, NULL)
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "",
                    StpeStuPetitionCmntsIdAssocMember = "2",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                });
                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    // This one has consent but no petition
                    StpeCoursesAssocMember = "ART-104",
                    StpeSectionAssocMember = "SEC4",
                    StpeFacultyConsentAssocMember = "D", //Consent status
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "",
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });
                return studentPetitionsData1;
            }

            private SectionPermissionRepository BuildValiSectionPermissionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for petition request
                dataAccessorMock.Setup<Task<StudentPetitions>>(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(studentPetitionsResponseData));
                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(stuPetitionCmntsResponseData));

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }
        }

        [TestClass]
        public class SectionPermissionRepository_AddStudentPetition
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            StudentPetitions petitionResponseData;
            Collection<StuPetitionCmnts> commentResponseData;
            ApiSettings apiSettings;
            SectionPermissionRepository sectionPermissionRepo;
            string multiLineComment;
            StudentPetition petitionToAdd;
            CreateStudentPetitionRequest createRequest;
            string petitionId;
            string studentId;
            string sectionId;
            string reason;
            string statusCode;
            string termCode;
            string petitionMultiLineComment;
            string consentMultiLineComment;
            string consentAddedBy;
            DateTime consentChangedDate;
            DateTime consentChangedTime;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                petitionId = "2";
                studentId = "0000123";
                sectionId = "SEC1";
                reason = "OVHM";
                statusCode = "D";
                multiLineComment = "Student 0000123 ART-101 Consent comment. Line1\ncomment line2\ncomment line3 the end";
                termCode = "2016/SP";
                consentAddedBy = "FacultyConcentAddedBy";
                consentChangedDate = DateTime.Now.AddDays(-20);
                consentChangedTime = DateTime.Now.AddHours(1);
                petitionResponseData = BuildPetitionResponse();
                commentResponseData = BuildStudentPetitionCommentResponse();
                sectionPermissionRepo = BuildValidSectionPermissionRepository();

                petitionToAdd = new StudentPetition(null, null, sectionId, studentId, StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                petitionResponseData = null;
                sectionPermissionRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfIncomingPetitionIsNull()
            {
                StudentPetition nullPetition = null;
                await sectionPermissionRepo.AddStudentPetitionAsync(nullPetition);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_StudentId()
            {
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.AreEqual(petitionToAdd.StudentId, createRequest.StudentId);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_SectionId()
            {
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.AreEqual(petitionToAdd.SectionId, createRequest.SectionId);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_ReasonCode()
            {
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.AreEqual(petitionToAdd.ReasonCode, createRequest.ReasonCode);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_StatusCode()
            {
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.AreEqual(petitionToAdd.StatusCode, createRequest.StatusCode);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_Type()
            {
                // Creating a faculty request
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.IsTrue(createRequest.IsFacultyConsent);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_Comment()
            {
                // Verifies that various types of carriage returns and line feeds are all properly converted to value marks
                await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                char newLineCharacter = '\n';
                string alternateNewLineCharacter = "\r\n";
                string carriageReturnCharacter = "\r";
                string expectedCommentText = petitionToAdd.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
                expectedCommentText = expectedCommentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
                var expectedCommentLines = expectedCommentText.Split(newLineCharacter);
                Assert.AreEqual(3, expectedCommentLines.Count());
                for (int i = 0; i < expectedCommentLines.Count(); i++)
                {
                    Assert.IsTrue(expectedCommentLines[i].Length > 0);
                    Assert.AreEqual(expectedCommentLines[i], createRequest.Comment.ElementAt(i));
                }
            }

            [TestMethod]
            public async Task ReturnsPetitionWhenAddIsSuccessful()
            {
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
                Assert.AreEqual(petitionToAdd.StudentId, newPetition.StudentId);
                Assert.AreEqual(petitionToAdd.SectionId, newPetition.SectionId);
                Assert.AreEqual("ART-101", newPetition.CourseId);
                Assert.AreEqual(petitionToAdd.EndDate, newPetition.EndDate);
                Assert.AreEqual(petitionToAdd.StartDate, newPetition.StartDate);
                Assert.AreEqual(petitionToAdd.TermCode, newPetition.TermCode);
                Assert.AreEqual(petitionToAdd.ReasonCode, newPetition.ReasonCode);
                Assert.AreEqual(petitionToAdd.StatusCode, newPetition.StatusCode);
                Assert.AreEqual(petitionToAdd.Type, newPetition.Type);
                Assert.AreEqual(new DateTime(consentChangedDate.Year, consentChangedDate.Month, consentChangedDate.Day, consentChangedTime.Hour, consentChangedTime.Minute, consentChangedTime.Second), new DateTime(newPetition.DateTimeChanged.Year, newPetition.DateTimeChanged.Month, newPetition.DateTimeChanged.Day, newPetition.DateTimeChanged.Hour, newPetition.DateTimeChanged.Minute, newPetition.DateTimeChanged.Second));
                Assert.AreEqual(multiLineComment, newPetition.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnPetitionForGivenStudent()
            {
                petitionToAdd = new StudentPetition(null, null, sectionId, "99999", StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnPetitionForGivenSection()
            {
                petitionToAdd = new StudentPetition(null, null, "9999", studentId, StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsKeyNotFoundExceptionWhenGetReadRecordReturnsNull()
            {
                // Set up repo response for null Get request
                petitionResponseData = null;
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(petitionResponseData));
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingStudentPetitionException))]
            public async Task ThrowsSpecialExceptionWhenExistingStudentPetitionFound()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "Existing petition found";
                createResponse.ExistingPetitionId = "123";
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsSimpleExceptionForAllOtherErrors()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "Problem in Colleague";
                createResponse.ExistingPetitionId = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var newPetition = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenSuccessfulFlagWithNoIdReturned()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var newWaiver = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenTransactionRequestThrowsException()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Throws(new Exception());
                var newWaiver = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task ThrowsExceptionWhenTransactionResponseIsNull()
            {
                CreateStudentPetitionResponse createResponse = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var newWaiver = await sectionPermissionRepo.AddStudentPetitionAsync(petitionToAdd);
            }

            private SectionPermissionRepository BuildValidSectionPermissionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = null;
                createResponse.ExistingPetitionId = "";
                createResponse.StudentPetitionsId = petitionId;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);

                // Set up repo response for petition Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(petitionResponseData));

                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(commentResponseData));

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentPetitions BuildPetitionResponse()
            {

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
                    StpeFacultyConsentAssocMember = "D", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "1",
                    StpePetitionStatusSetByAssocMember = "FacultyId1",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-10),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId2",
                    StpeFacultyConsentDateAssocMember = consentChangedDate,
                    StpeFacultyConsentTimeAssocMember = consentChangedTime

                });
                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-102",
                    StpeSectionAssocMember = "SEC2",
                    StpePetitionStatusAssocMember = "C",//Petition status(D, A, NULL)
                    StpeFacultyConsentAssocMember = "D", //Consent status
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "2",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });

                return studentPetitionsData1;
            }

            private Collection<StuPetitionCmnts> BuildStudentPetitionCommentResponse()
            {
                // These would be all the comments associated to the specific student petition and they will all have same student but there could be different sections.

                var repoStuPetitionCmntsData = new Collection<StuPetitionCmnts>();

                petitionMultiLineComment = "Student 0000123 ART-101 Petition comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";
                consentMultiLineComment = "Student 0000123 ART-101 Consent comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "1",
                    StpcPetitionComments = petitionMultiLineComment,
                    StpcConsentComments = consentMultiLineComment
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "2",
                    StpcPetitionComments = string.Empty,
                    StpcConsentComments = "Student 0000123 ART-102 Consent comment."
                });

                return repoStuPetitionCmntsData;
            }
        }


        [TestClass]
        public class SectionPermissionRepository_UpdateStudentPetition
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ILogger> loggerMock;
            StudentPetitions petitionResponseData;
            Collection<StuPetitionCmnts> commentResponseData;
            ApiSettings apiSettings;
            SectionPermissionRepository sectionPermissionRepo;
            string multiLineComment;
            StudentPetition petitionToUpdate;
            CreateStudentPetitionRequest createRequest;
            string petitionId;
            string studentId;
            string sectionId;
            string reason;
            string statusCode;
            string termCode;
            string petitionMultiLineComment;
            string consentMultiLineComment;
            string consentAddedBy;
            DateTime consentChangedDate;
            DateTime consentChangedTime;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                petitionId = "2";
                studentId = "0000123";
                sectionId = "SEC1";
                reason = "OVHM";
                statusCode = "D";
                multiLineComment = "Student 0000123 ART-101 Consent comment. Line1\ncomment line2\ncomment line3 the end";
                termCode = "2016/SP";
                consentAddedBy = "FacultyConcentAddedBy";
                consentChangedDate = DateTime.Now.AddDays(-20);
                consentChangedTime = DateTime.Now.AddHours(1);
                petitionResponseData = BuildPetitionResponse();
                commentResponseData = BuildStudentPetitionCommentResponse();
                sectionPermissionRepo = BuildValidSectionPermissionRepository();

                petitionToUpdate = new StudentPetition(null, null, sectionId, studentId, StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                petitionResponseData = null;
                sectionPermissionRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfIncomingPetitionIsNull()
            {
                StudentPetition nullPetition = null;
                await sectionPermissionRepo.UpdateStudentPetitionAsync(nullPetition);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_Success()
            {
                await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
                Assert.AreEqual(petitionToUpdate.StudentId, createRequest.StudentId);
                Assert.AreEqual(petitionToUpdate.SectionId, createRequest.SectionId);
                Assert.AreEqual(petitionToUpdate.ReasonCode, createRequest.ReasonCode);
                Assert.AreEqual(petitionToUpdate.StatusCode, createRequest.StatusCode);
                Assert.IsTrue(createRequest.IsFacultyConsent);
            }

            [TestMethod]
            public async Task BuildsValidCreateRequest_Comment()
            {
                // Verifies that various types of carriage returns and line feeds are all properly converted to value marks
                await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
                char newLineCharacter = '\n';
                string alternateNewLineCharacter = "\r\n";
                string carriageReturnCharacter = "\r";
                string expectedCommentText = petitionToUpdate.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
                expectedCommentText = expectedCommentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
                var expectedCommentLines = expectedCommentText.Split(newLineCharacter);
                Assert.AreEqual(3, expectedCommentLines.Count());
                for (int i = 0; i < expectedCommentLines.Count(); i++)
                {
                    Assert.IsTrue(expectedCommentLines[i].Length > 0);
                    Assert.AreEqual(expectedCommentLines[i], createRequest.Comment.ElementAt(i));
                }
            }

            [TestMethod]
            public async Task ReturnsPetitionWhenAddIsSuccessful()
            {
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
                Assert.AreEqual(petitionToUpdate.StudentId, updatePetition.StudentId);
                Assert.AreEqual(petitionToUpdate.SectionId, updatePetition.SectionId);
                Assert.AreEqual("ART-101", updatePetition.CourseId);
                Assert.AreEqual(petitionToUpdate.EndDate, updatePetition.EndDate);
                Assert.AreEqual(petitionToUpdate.StartDate, updatePetition.StartDate);
                Assert.AreEqual(petitionToUpdate.TermCode, updatePetition.TermCode);
                Assert.AreEqual(petitionToUpdate.ReasonCode, updatePetition.ReasonCode);
                Assert.AreEqual(petitionToUpdate.StatusCode, updatePetition.StatusCode);
                Assert.AreEqual(petitionToUpdate.Type, updatePetition.Type);
                Assert.AreEqual(new DateTime(consentChangedDate.Year, consentChangedDate.Month, consentChangedDate.Day, consentChangedTime.Hour, consentChangedTime.Minute, consentChangedTime.Second), new DateTime(updatePetition.DateTimeChanged.Year, updatePetition.DateTimeChanged.Month, updatePetition.DateTimeChanged.Day, updatePetition.DateTimeChanged.Hour, updatePetition.DateTimeChanged.Minute, updatePetition.DateTimeChanged.Second));
                Assert.AreEqual(multiLineComment, updatePetition.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnPetitionForGivenStudent()
            {
                petitionToUpdate = new StudentPetition(null, null, sectionId, "99999", StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionWhenGetDoesNotReturnPetitionForGivenSection()
            {
                petitionToUpdate = new StudentPetition(null, null, "9999", studentId, StudentPetitionType.FacultyConsent, statusCode) { Comment = multiLineComment, ReasonCode = reason, TermCode = termCode };
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsKeyNotFoundExceptionWhenGetReadRecordReturnsNull()
            {
                // Set up repo response for null Get request
                petitionResponseData = null;
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(petitionResponseData));
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsSimpleExceptionForAllOtherErrors()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "Problem in Colleague";
                createResponse.ExistingPetitionId = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionWhenSuccessfulFlagWithNoIdReturned()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionWhenTransactionRequestThrowsException()
            {
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Throws(new Exception());
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsExceptionWhenTransactionResponseIsNull()
            {
                CreateStudentPetitionResponse createResponse = null;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);
                var updatePetition = await sectionPermissionRepo.UpdateStudentPetitionAsync(petitionToUpdate);
            }

            private SectionPermissionRepository BuildValidSectionPermissionRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                localCacheMock = new Mock<ObjectCache>();

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                CreateStudentPetitionResponse createResponse = new CreateStudentPetitionResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = null;
                createResponse.ExistingPetitionId = "";
                createResponse.StudentPetitionsId = petitionId;
                mockManager.Setup(mgr => mgr.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(It.Is<CreateStudentPetitionRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).Returns(Task.FromResult(createResponse)).Callback<CreateStudentPetitionRequest>(req => createRequest = req);

                // Set up repo response for petition Get request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<StudentPetitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(petitionResponseData));

                dataAccessorMock.Setup<Task<Collection<StuPetitionCmnts>>>(acc => acc.BulkReadRecordAsync<StuPetitionCmnts>(It.IsAny<string[]>(), true)).Returns(Task.FromResult<Collection<StuPetitionCmnts>>(commentResponseData));

                SectionPermissionRepository repository = new SectionPermissionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }

            private StudentPetitions BuildPetitionResponse()
            {
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
                    StpePetitionStatusAssocMember = "A",
                    StpeFacultyConsentAssocMember = "D",
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "1",
                    StpePetitionStatusSetByAssocMember = "FacultyId1",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-10),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId2",
                    StpeFacultyConsentDateAssocMember = consentChangedDate,
                    StpeFacultyConsentTimeAssocMember = consentChangedTime

                });
                studentPetitionsData1.PetitionsEntityAssociation.Add(new StudentPetitionsPetitions()
                {
                    StpeCoursesAssocMember = "ART-102",
                    StpeSectionAssocMember = "SEC2",
                    StpePetitionStatusAssocMember = "C",
                    StpeFacultyConsentAssocMember = "D",
                    StpePetitionReasonCodeAssocMember = "ICHI",
                    StpeConsentReasonCodeAssocMember = "OVHM",
                    StpeStuPetitionCmntsIdAssocMember = "2",
                    StpePetitionStatusSetByAssocMember = "FacultyId3",
                    StpePetitionStatusDateAssocMember = DateTime.Today.AddDays(-30),
                    StpePetitionStatusTimeAssocMember = DateTime.Now.AddHours(-1),
                    StpeFacultyConsentSetByAssocMember = "FacultyId4",
                    StpeFacultyConsentDateAssocMember = DateTime.Today.AddDays(-4),
                    StpeFacultyConsentTimeAssocMember = DateTime.Now.AddHours(1)
                });

                return studentPetitionsData1;
            }

            private Collection<StuPetitionCmnts> BuildStudentPetitionCommentResponse()
            {
                // These would be all the comments associated to the specific student petition and they will all have same student but there could be different sections.
                var repoStuPetitionCmntsData = new Collection<StuPetitionCmnts>();

                petitionMultiLineComment = "Student 0000123 ART-101 Petition comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";
                consentMultiLineComment = "Student 0000123 ART-101 Consent comment. Line1" + DmiString._VM + "comment line2" + DmiString._VM + "comment line3 the end";

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "1",
                    StpcPetitionComments = petitionMultiLineComment,
                    StpcConsentComments = consentMultiLineComment
                });

                repoStuPetitionCmntsData.Add(new StuPetitionCmnts()
                {
                    Recordkey = "2",
                    StpcPetitionComments = string.Empty,
                    StpcConsentComments = "Student 0000123 ART-102 Consent comment."
                });

                return repoStuPetitionCmntsData;
            }
        }
    }
}

