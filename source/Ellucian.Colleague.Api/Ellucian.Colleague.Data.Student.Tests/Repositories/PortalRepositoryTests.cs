// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Dmi.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class PortalRepositoryTests : BaseRepositorySetup
    {
        private PortalRepository repository;
        [TestInitialize]
        public void Initialize()
        {
            //mock cache provider, transaction, logger
            MockInitialize();
            //instantiate test repository
            repository = new PortalRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

        }
        [TestClass]
        public class PortalRepository_GetCoursesForDeletionAsync : PortalRepositoryTests
        {
            PortalGetCoursesForDeletionResponse deleteCoursesResponse;
            PortalGetCoursesForDeletionRequest deleteCoursesRequest;

            [TestInitialize]
            public void PortalRepository_GetCoursesForDeletionAsync_Initialize()
            {
                deleteCoursesRequest = new PortalGetCoursesForDeletionRequest();

            }
            //when response have total courses and courseids
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHasValidData()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = 2, CourseIds = new List<string>() { "course-1", "course-2" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(Convert.ToInt32(deleteCoursesResponse.TotalCourses), deletedCoursesResult.TotalCourses);
                Assert.AreEqual(deleteCoursesResponse.CourseIds.Count, deletedCoursesResult.CourseIds.Count);
                Assert.AreEqual(deleteCoursesResponse.CourseIds[0], deletedCoursesResult.CourseIds[0]);
                Assert.AreEqual(deleteCoursesResponse.CourseIds[1], deletedCoursesResult.CourseIds[1]);
            }

            //when transaction returns null response
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetCoursesForDeletionAsync_TransactionReturnsNullResponse()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(() => null);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();

            }
            //when transaction throws exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetCoursesForDeletionAsync_TransactionReturnsException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).Throws(new ColleagueTransactionException("something happened"));
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();

            }

            //when response returns empty course ids
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveEmptyCourseIds()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = 2, CourseIds = new List<string>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(deleteCoursesResponse.TotalCourses, deletedCoursesResult.TotalCourses);
                Assert.AreEqual(deleteCoursesResponse.CourseIds.Count, deletedCoursesResult.CourseIds.Count);
                Assert.AreEqual(0, deletedCoursesResult.CourseIds.Count);
            }
            //when response returns null course ids
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveNullCourseIds()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = 2, CourseIds = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(Convert.ToInt32(deleteCoursesResponse.TotalCourses), deletedCoursesResult.TotalCourses);
                Assert.IsNull(deletedCoursesResult.CourseIds);

            }

            //when response totalcourses is null
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveNullTotalCourses()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = null, CourseIds = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.IsNull(deletedCoursesResult.TotalCourses);
                Assert.IsNull(deletedCoursesResult.CourseIds);
            }

            //when response returns null total courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveEmptyTotalCourses()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = null, CourseIds = new List<string>() { "course-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(null, deletedCoursesResult.TotalCourses);
                Assert.AreEqual(1, deletedCoursesResult.CourseIds.Count);
                loggerMock.Verify(l => l.Info("Total Courses retrieved for deletion for Portal is null"));

            }
            //when reposnse total courses is 0
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveZeroTotalCourses()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = 0, CourseIds = new List<string>() { "course-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(0, deletedCoursesResult.TotalCourses);
                Assert.AreEqual(1, deletedCoursesResult.CourseIds.Count);
                loggerMock.Verify(l => l.Warn("Total Courses 0 retrieved for deletion for Portal is less than the actual count of list of courses retrieved 1"));
            }

            //when response total courses have negative
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_ResponseHaveNegativeTotalCourses()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = -1, CourseIds = new List<string>() { "course-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(-1, deletedCoursesResult.TotalCourses);
                Assert.AreEqual(1, deletedCoursesResult.CourseIds.Count);
                loggerMock.Verify(l => l.Warn("Total Courses -1 retrieved for deletion for Portal is less than 0"));
            }

            //when reposnse total courses is > than total courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForDeletionAsync_TotalCourses_greaterThan_ListOfCourses()
            {
                deleteCoursesResponse = new PortalGetCoursesForDeletionResponse() { TotalCourses = 5, CourseIds = new List<string>() { "course-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(It.IsAny<PortalGetCoursesForDeletionRequest>())).ReturnsAsync(deleteCoursesResponse);
                PortalDeletedCoursesResult deletedCoursesResult = await repository.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesResult);
                Assert.AreEqual(5, deletedCoursesResult.TotalCourses);
                Assert.AreEqual(1, deletedCoursesResult.CourseIds.Count);
                loggerMock.Verify(l => l.Info("Total Courses 5 retrieved for deletion for Portal is more than the actual count of list of courses retrieved 1, hence there could be more courses applicable for deletion for Portal"));
            }
        }

        [TestClass]
        public class PortalRepository_GetSectionsForUpdateAsync : PortalRepositoryTests
        {
            PortalGetSectionsForUpdateResponse response;

            [TestInitialize]
            public void PortalRepository_GetSectionsForUpdateAsync_Initialize()
            {
                colleagueTimeZone = apiSettings.ColleagueTimeZone;
            }

            //when response has total sections and valid list of sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHasValidData()
            {
                char _SM = Convert.ToChar(DynamicArray.SM);

                string[] bookDataSeparator = { "...;" };
                char bookCostSeparator = ';';
                var bookCostCultureInfo = new CultureInfo("en-US", false);

                response = SetPortalGetSectionsForUpdateResponseData();
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(expected: response.HostShortDateFormat, actual: result.ShortDateFormat);
                Assert.AreEqual(expected: response.TotalSections, actual: result.TotalSections);
                Assert.AreEqual(expected: response.PortalUpdatedSections.Count - 1, actual: result.Sections.Count);

                PortalUpdatedSections responseSection = null;
                PortalSection resultSection = null;

                for (var i = 1; i < response.PortalUpdatedSections.Count; i++)
                {
                    responseSection = (response.PortalUpdatedSections[i]);
                    resultSection = (result.Sections[i - 1]);

                    Assert.AreEqual(responseSection.SectionsId, resultSection.SectionId);
                    Assert.AreEqual(responseSection.SecShortTitle, resultSection.ShortTitle);
                    Assert.AreEqual(responseSection.CrsDesc, resultSection.Description);
                    Assert.AreEqual(responseSection.SecLocation, resultSection.Location);
                    Assert.AreEqual(responseSection.SecTerm, resultSection.Term);
                    Assert.AreEqual(responseSection.SecStartDate, resultSection.StartDate);
                    Assert.AreEqual(responseSection.SecEndDate, resultSection.EndDate);
                    Assert.AreEqual(responseSection.SecCapacity, resultSection.Capacity);
                    Assert.AreEqual(responseSection.SecSubject, resultSection.Subject);
                    Assert.AreEqual(responseSection.SecCourseNo, resultSection.CourseNumber);
                    Assert.AreEqual(responseSection.SecNo, resultSection.SectionNumber);
                    Assert.AreEqual(responseSection.SecAcadLevel, resultSection.AcademicLevel);
                    Assert.AreEqual(responseSection.SecSynonym, resultSection.Synonym);
                    Assert.AreEqual(responseSection.SecMinCred, resultSection.MinimumCredits);
                    Assert.AreEqual(responseSection.SecMaxCred, resultSection.MaximumCredits);
                    Assert.AreEqual(responseSection.SecName, resultSection.SectionName);
                    Assert.AreEqual(responseSection.SecCourse, resultSection.CourseId);
                    Assert.AreEqual(responseSection.CrsPrereqs, resultSection.PrerequisiteText);
                    Assert.AreEqual(responseSection.SecCeus, resultSection.ContinuingEducationUnits);
                    Assert.AreEqual(responseSection.SecPrintedComments, resultSection.PrintedComments);
                    Assert.AreEqual(responseSection.BookTotal ?? 0, resultSection.TotalBookCost);

                    var depts = !string.IsNullOrWhiteSpace(responseSection.SecDepts) ?
                        responseSection.SecDepts.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(depts.Count, resultSection.Departments.Count);

                    for (var j = 0; j < depts.Count; j++)
                    {
                        Assert.AreEqual(depts[j], resultSection.Departments[j]);
                    }

                    var faculty = !string.IsNullOrWhiteSpace(responseSection.SecFaculty) ?
                        responseSection.SecFaculty.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(faculty.Count, resultSection.Faculty.Count);
                    for (var j = 0; j < faculty.Count; j++)
                    {
                        Assert.AreEqual(faculty[j], resultSection.Faculty[j]);
                    }

                    var courseTypes = !string.IsNullOrWhiteSpace(responseSection.CrsType) ?
                        responseSection.CrsType.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(courseTypes.Count, resultSection.CourseTypes.Count);
                    for (var j = 0; j < courseTypes.Count; j++)
                    {
                        Assert.AreEqual(courseTypes[j], resultSection.CourseTypes[j]);
                    }

                    //book information
                    int blankBooksCount = 0;
                    var bookData = responseSection.BookData != null ?
                        responseSection.BookData.Split(bookDataSeparator, StringSplitOptions.None).ToList() : new List<string>();
                    var bookCost = responseSection.BookCost != null ?
                        responseSection.BookCost.Split(bookCostSeparator).ToList() : new List<string>();

                    for (var j = 0; j < bookData.Count; j++)
                    {
                        if (string.IsNullOrWhiteSpace(bookData[j]))
                        {
                            blankBooksCount++;
                            continue;
                        }

                        decimal cost;
                        if (!decimal.TryParse(bookCost[j], NumberStyles.Currency, bookCostCultureInfo.NumberFormat, out cost))
                            cost = 0;

                        Assert.AreEqual(bookData[j], resultSection.BookInformation[j].Information);
                        Assert.AreEqual(cost, resultSection.BookInformation[j].Cost);
                    }

                    //remove blank records before checking counts
                    Assert.AreEqual(bookData.Count - blankBooksCount, resultSection.BookInformation.Count);
                    Assert.AreEqual(bookCost.Count - blankBooksCount, resultSection.BookInformation.Count);

                    // meeting information
                    var buildings = responseSection.CsmBldg != null ? responseSection.CsmBldg.Split(_SM).ToList() : new List<string>();
                    var rooms = responseSection.CsmRoom != null ? responseSection.CsmRoom.Split(_SM).ToList() : new List<string>();
                    var instructionalMethod = responseSection.CsmInstrMethod != null ? responseSection.CsmInstrMethod.Split(_SM).ToList() : new List<string>();
                    var days = responseSection.CsmDays != null ? responseSection.CsmDays.Split(_SM).ToList() : new List<string>();
                    var starttime = responseSection.CsmStartTime != null ? responseSection.CsmStartTime.Split(_SM).ToList() : new List<string>();
                    var endtime = responseSection.CsmEndTime != null ? responseSection.CsmEndTime.Split(_SM).ToList() : new List<string>();

                    int blankMeetingsCount = 0;
                    for (int j = 0; j < instructionalMethod.Count(); j++)
                    {
                        if (string.IsNullOrWhiteSpace(instructionalMethod[j]))
                        {
                            blankMeetingsCount++;
                            continue;
                        }

                        //convert times
                        DateTime? stime = null;
                        DateTime? etime = null;
                        DateTimeOffset? stimeOffset = null;
                        DateTimeOffset? etimeOffset = null;

                        if (!string.IsNullOrWhiteSpace(starttime[j]))
                        {
                            stime = new DateTime(DmiString.PickTimeToDateTime(Int32.Parse(starttime[j])).Ticks);
                            stimeOffset = stime.ToPointInTimeDateTimeOffset(DateTime.MinValue, colleagueTimeZone);
                        }

                        if (!string.IsNullOrWhiteSpace(endtime[j]))
                        {
                            etime = new DateTime(DmiString.PickTimeToDateTime(Int32.Parse(endtime[j])).Ticks);
                            etimeOffset = etime.ToPointInTimeDateTimeOffset(DateTime.MinValue, colleagueTimeZone);
                        }

                        Assert.AreEqual(buildings[j], resultSection.MeetingInformation[j].Building);
                        Assert.AreEqual(rooms[j], resultSection.MeetingInformation[j].Room);
                        Assert.AreEqual(instructionalMethod[j], resultSection.MeetingInformation[j].InstructionalMethod);
                        Assert.AreEqual(stimeOffset, resultSection.MeetingInformation[j].StartTime);
                        Assert.AreEqual(etimeOffset, resultSection.MeetingInformation[j].EndTime);

                        //convert days of week
                        var daysofweek = !string.IsNullOrWhiteSpace(days[j]) ? ConvertSectionMeetingDaysStringToList(days[j], ' ') : new List<DayOfWeek>();
                        Assert.AreEqual(daysofweek.Count, resultSection.MeetingInformation[j].DaysOfWeek.Count);
                        for (int k = 0; k < daysofweek.Count; k++)
                        {
                            Assert.AreEqual(daysofweek[k], resultSection.MeetingInformation[j].DaysOfWeek[k]);
                        }
                    }

                    //remove blank records before checking counts
                    Assert.AreEqual(buildings.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                    Assert.AreEqual(rooms.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                    Assert.AreEqual(instructionalMethod.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                    Assert.AreEqual(days.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                    Assert.AreEqual(starttime.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                    Assert.AreEqual(endtime.Count - blankMeetingsCount, resultSection.MeetingInformation.Count);
                }
            }

            //when transaction returns null response
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetSectionsForUpdateAsync_TransactionReturnsNullResponse()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(() => null);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
            }

            //when transaction throws exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetSectionsForUpdateAsync_TransactionReturnsException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).Throws(new ColleagueTransactionException("something happened"));
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
            }

            //when response returns empty portal sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHasEmptySections()
            {
                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = 2, PortalUpdatedSections = new List<PortalUpdatedSections>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(response.HostShortDateFormat, result.ShortDateFormat);
                Assert.AreEqual(response.TotalSections, result.TotalSections);
                Assert.AreEqual(response.PortalUpdatedSections.Count, result.Sections.Count);
                Assert.AreEqual(0, result.Sections.Count);
            }

            //when response returns null portal sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHasNullSections()
            {
                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = 2, PortalUpdatedSections = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(response.HostShortDateFormat, result.ShortDateFormat);
                Assert.AreEqual(response.TotalSections, result.TotalSections);
                Assert.IsNull(response.PortalUpdatedSections);
                Assert.IsNotNull(result.Sections);
                Assert.AreEqual(0, result.Sections.Count);
            }

            //when response total sections is null
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHaveNullTotalSections()
            {
                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = null, PortalUpdatedSections = new List<PortalUpdatedSections>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.IsNull(result.TotalSections);
                Assert.IsNotNull(result.Sections);
                Assert.AreEqual(0, result.Sections.Count);
                loggerMock.Verify(l => l.Info("Total Sections retrieved for update for Portal is null"));
            }

            //when reposnse total sections is 0
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHaveZeroTotalSections()
            {
                var responseSection = new PortalUpdatedSections { SectionsId = "25310" };
                var responseSections = new List<PortalUpdatedSections>();
                responseSections.Add(responseSection);

                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = 0, PortalUpdatedSections = responseSections };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.TotalSections);
                Assert.AreEqual(1, result.Sections.Count);
                loggerMock.Verify(l => l.Warn("Total Sections 0 retrieved for update for Portal is less than the actual count of list of sections retrieved 1"));
            }

            //when response total section is a negative value
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_ResponseHaveNegativeTotalSections()
            {
                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = -1, PortalUpdatedSections = new List<PortalUpdatedSections>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(-1, result.TotalSections);
                Assert.AreEqual(0, result.Sections.Count);
                loggerMock.Verify(l => l.Warn("Total Sections -1 retrieved for update for Portal is less than 0"));
            }

            //when reposnse total sections is > than total sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForUpdateAsync_TotalSections_greaterThan_ListOfSections()
            {
                var responseSection = new PortalUpdatedSections { SectionsId = "25310" };
                var responseSections = new List<PortalUpdatedSections>();
                responseSections.Add(responseSection);

                response = new PortalGetSectionsForUpdateResponse() { HostShortDateFormat = "MDY", TotalSections = 5, PortalUpdatedSections = responseSections };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(It.IsAny<PortalGetSectionsForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedSectionsResult result = await repository.GetSectionsForUpdateAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(5, result.TotalSections);
                Assert.AreEqual(1, result.Sections.Count);
                loggerMock.Verify(l => l.Info("Total Sections 5 retrieved for update for Portal is more than the actual count of list of sections retrieved 1, hence there could be more sections applicable for update for Portal"));
            }

            private PortalGetSectionsForUpdateResponse SetPortalGetSectionsForUpdateResponseData()
            {
                var responseSections = new List<PortalUpdatedSections>();

                //null sections
                responseSections.Add(null);

                //empty section list
                responseSections.Add(new PortalUpdatedSections());

                //section with empty properties
                var responseSectionEmpty = new PortalUpdatedSections()
                {
                    SectionsId = string.Empty,
                    SecShortTitle = string.Empty,
                    CrsDesc = string.Empty,
                    SecLocation = string.Empty,
                    SecTerm = string.Empty,
                    SecStartDate = null,
                    SecEndDate = null,
                    SecCapacity = null,
                    SecSubject = string.Empty,
                    SecCourseNo = string.Empty,
                    SecNo = string.Empty,
                    SecAcadLevel = string.Empty,
                    SecSynonym = string.Empty,
                    SecMinCred = null,
                    SecMaxCred = null,
                    SecName = string.Empty,
                    SecCourse = string.Empty,
                    CrsPrereqs = string.Empty,
                    CsmBldg = string.Empty,
                    CsmRoom = string.Empty,
                    CsmInstrMethod = string.Empty,
                    CsmDays = string.Empty,
                    CsmStartTime = string.Empty,
                    CsmEndTime = string.Empty,
                    SecDepts = string.Empty,
                    SecFaculty = string.Empty,
                    CrsType = string.Empty,
                    SecCeus = null,
                    SecPrintedComments = string.Empty,
                    BookData = string.Empty,
                    BookCost = string.Empty,
                    BookTotal = 0
                };
                responseSections.Add(responseSectionEmpty);

                var responseSectionValid1 = new PortalUpdatedSections()
                {
                    SectionsId = "25400",
                    SecShortTitle = "Calculus III",
                    CrsDesc = "this is a course description.",
                    SecLocation = "Main Campus",
                    SecTerm = "2019 Fall",
                    SecStartDate = new DateTime(2019, 1, 1),
                    SecEndDate = new DateTime(2019, 12, 15),
                    SecCapacity = 25,
                    SecSubject = "Mathematics",
                    SecCourseNo = "200",
                    SecNo = "01",
                    SecAcadLevel = "Undergraduate",
                    SecSynonym = "18767",
                    SecMinCred = 3.00000m,
                    SecMaxCred = 12.00000m,
                    SecName = "MATH-200-01",
                    SecCourse = "46",
                    CrsPrereqs = "",
                    CsmBldg = "Carmichael HallüCarmichael HallüCarmichael HallüCarmichael Hall",
                    CsmRoom = "205ü210ü205ü205",
                    CsmInstrMethod = "LectureüLaboratoryüLectureüLecture",
                    CsmDays = "T W THüWüT W THüT W TH",
                    CsmStartTime = "54000ü64800ü57600ü61200",
                    CsmEndTime = "57000ü67800ü60000ü64200",
                    SecDepts = "Mathematics Bma",
                    SecFaculty = "T. SchmitüProfessor BhaumiküMs, J Wanda, Miles",
                    CrsType = "HonorsüCoop Work Experience",
                    SecCeus = null,
                    SecPrintedComments = "",
                    BookData = "\"Nuts and bolts\" by , (Pub date  by ); ISBN 12129812 (Required)...; \"\"Basic College Math, Student Support Edition\"\" by Richard N. Aufmann, (Pub date 2010 by Durham Publishers); ISBN 9781439046968 (Required)...; \"Newest Book on the Block\" by Newest Author on the Block, (Pub date 2010 by New Books on the Block); ISBN 5555566666 (Required)",
                    BookCost = "$599.99;$150.25;$10.00",
                    BookTotal = 760.24m
                };
                responseSections.Add(responseSectionValid1);

                var responseSectionValid2 = new PortalUpdatedSections()
                {
                    SectionsId = "section-2",
                    SecShortTitle = "section-2 title",
                    CrsDesc = "section-2 course description.",
                    SecLocation = "section-2 location",
                    SecTerm = "term-2",
                    SecStartDate = new DateTime(2019, 1, 1),
                    SecEndDate = new DateTime(2019, 12, 15),
                    SecCapacity = 25,
                    SecSubject = "section-2 subject",
                    SecCourseNo = "course-2",
                    SecNo = "02",
                    SecAcadLevel = "graduate",
                    SecSynonym = "2222",
                    SecMinCred = 1.12345m,
                    SecMaxCred = 15.25000m,
                    SecName = "SEC-200-02",
                    SecCourse = "2",
                    CrsPrereqs = "section-2 course prereq",
                    CsmBldg = "Carmichael Hallü",
                    CsmRoom = "205ü210",
                    CsmInstrMethod = "LectureüLaboratory",
                    CsmDays = "ü",
                    CsmStartTime = "54000ü",
                    CsmEndTime = "57000ü",
                    SecDepts = "dept-1üdept-2",
                    SecFaculty = "T. SchmitüMs, J Wanda, Miles",
                    CrsType = "Honors",
                    SecCeus = 3.25m,
                    SecPrintedComments = "section-2 course printed comments are here",
                    BookData = "\"Newest Book on the Block\" by Newest Author on the Block, (Pub date 2010 by New Books on the Block); ISBN 5555566666 (Required)",
                    BookCost = "#10.00",
                    BookTotal = 760.24m
                };
                responseSections.Add(responseSectionValid2);

                return new PortalGetSectionsForUpdateResponse()
                {
                    HostShortDateFormat = "MDY",
                    TotalSections = 154,
                    PortalUpdatedSections = responseSections
                };
            }

            private List<DayOfWeek> ConvertSectionMeetingDaysStringToList(string meetingDays, char separator = ' ')
            {
                var daysOfWeek = new List<DayOfWeek>();

                if (string.IsNullOrWhiteSpace(meetingDays))
                    return daysOfWeek;

                var days = meetingDays.Split(separator);
                foreach (string day in days)
                {
                    switch (day)
                    {
                        case "SU":
                            daysOfWeek.Add(DayOfWeek.Sunday);
                            break;
                        case "M":
                            daysOfWeek.Add(DayOfWeek.Monday);
                            break;
                        case "T":
                            daysOfWeek.Add(DayOfWeek.Tuesday);
                            break;
                        case "W":
                            daysOfWeek.Add(DayOfWeek.Wednesday);
                            break;
                        case "TH":
                            daysOfWeek.Add(DayOfWeek.Thursday);
                            break;
                        case "F":
                            daysOfWeek.Add(DayOfWeek.Friday);
                            break;
                        case "S":
                            daysOfWeek.Add(DayOfWeek.Saturday);
                            break;
                    }
                }
                return daysOfWeek;
            }
        }

        [TestClass]
        public class PortalRepository_GetSectionsForDeletionAsync : PortalRepositoryTests
        {
            PortalGetSectionsForDeletionResponse deleteSectionsResponse;

            [TestInitialize]
            public void PortalRepository_GetSectionsForDeletionAsync_Initialize()
            {
            }

            //when response has total sections and sectionids
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasValidData()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = 2, SectionIds = new List<string>() { "section-1", "section-2" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(Convert.ToInt32(deleteSectionsResponse.TotalSections), deletedSectionsResult.TotalSections);
                Assert.AreEqual(deleteSectionsResponse.SectionIds.Count, deletedSectionsResult.SectionIds.Count);
                Assert.AreEqual(deleteSectionsResponse.SectionIds[0], deletedSectionsResult.SectionIds[0]);
                Assert.AreEqual(deleteSectionsResponse.SectionIds[1], deletedSectionsResult.SectionIds[1]);
            }

            //when transaction returns null response
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetSectionsForDeletionAsync_TransactionReturnsNullResponse()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(() => null);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
            }

            //when transaction throws exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetSectionsForDeletionAsync_TransactionReturnsException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).Throws(new ColleagueTransactionException("something happened"));
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
            }

            //when response returns empty section ids
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasEmptySectionIds()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = 2, SectionIds = new List<string>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(deleteSectionsResponse.TotalSections, deletedSectionsResult.TotalSections);
                Assert.AreEqual(deleteSectionsResponse.SectionIds.Count, deletedSectionsResult.SectionIds.Count);
                Assert.AreEqual(0, deletedSectionsResult.SectionIds.Count);
            }

            //when response returns null section ids
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasNullSectionIds()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = 2, SectionIds = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(Convert.ToInt32(deleteSectionsResponse.TotalSections), deletedSectionsResult.TotalSections);
                Assert.IsNull(deletedSectionsResult.SectionIds);
            }

            //when response total sections is null
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasNullTotalSections()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = null, SectionIds = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.IsNull(deletedSectionsResult.TotalSections);
                Assert.IsNull(deletedSectionsResult.SectionIds);
            }

            //when response returns null total sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasEmptyTotalSections()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = null, SectionIds = new List<string>() { "section-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(null, deletedSectionsResult.TotalSections);
                Assert.AreEqual(1, deletedSectionsResult.SectionIds.Count);
                loggerMock.Verify(l => l.Info("Total Sections retrieved for deletion for Portal is null"));
            }

            //when reposnse total sections is 0
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasZeroTotalSections()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = 0, SectionIds = new List<string>() { "section-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(0, deletedSectionsResult.TotalSections);
                Assert.AreEqual(1, deletedSectionsResult.SectionIds.Count);
                loggerMock.Verify(l => l.Warn("Total Sections 0 retrieved for deletion for Portal is less than the actual count of list of sections retrieved 1"));
            }

            //when response total sections has negative
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_ResponseHasNegativeTotalSections()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = -1, SectionIds = new List<string>() { "section-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(-1, deletedSectionsResult.TotalSections);
                Assert.AreEqual(1, deletedSectionsResult.SectionIds.Count);
                loggerMock.Verify(l => l.Warn("Total Sections -1 retrieved for deletion for Portal is less than 0"));
            }

            //when reposnse total sections is > than total sections
            [TestMethod]
            public async Task PortalRepository_GetSectionsForDeletionAsync_TotalSections_GreaterThan_ListOfSections()
            {
                deleteSectionsResponse = new PortalGetSectionsForDeletionResponse() { TotalSections = 5, SectionIds = new List<string>() { "section-1" } };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(It.IsAny<PortalGetSectionsForDeletionRequest>())).ReturnsAsync(deleteSectionsResponse);
                PortalDeletedSectionsResult deletedSectionsResult = await repository.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsResult);
                Assert.AreEqual(5, deletedSectionsResult.TotalSections);
                Assert.AreEqual(1, deletedSectionsResult.SectionIds.Count);
                loggerMock.Verify(l => l.Info("Total Sections 5 retrieved for deletion for Portal is more than the actual count of list of sections retrieved 1, hence there could be more sections applicable for deletion for Portal"));
            }
        }

        [TestClass]
        public class PortalRepository_GetCoursesForUpdateAsync : PortalRepositoryTests
        {
            PortalGetCoursesForUpdateResponse response;

            [TestInitialize]
            public void PortalRepository_GetCoursesForUpdateAsync_Initialize()
            {
            }

            //when response have total courses and a valid list of courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHasValidData()
            {
                char _SM = Convert.ToChar(DynamicArray.SM);

                response = SetPortalGetCoursesForUpdateResponseData();
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(expected: response.TotalCourses, actual: result.TotalCourses);
                Assert.AreEqual(expected: response.PortalUpdatedCourses.Count - 1, actual: result.Courses.Count);

                PortalUpdatedCourses responseCourse = null;
                PortalCourse resultCourse = null;

                for (var i = 1; i < response.PortalUpdatedCourses.Count; i++)
                {
                    responseCourse = (response.PortalUpdatedCourses[i]);
                    resultCourse = (result.Courses[i - 1]);

                    Assert.AreEqual(responseCourse.CoursesId, resultCourse.CourseId);
                    Assert.AreEqual(responseCourse.CrsShortTitle, resultCourse.ShortTitle);
                    Assert.AreEqual(responseCourse.CrsTitle, resultCourse.Title);
                    Assert.AreEqual(responseCourse.CrsDesc, resultCourse.Description);
                    
                    Assert.AreEqual(responseCourse.CrsSubject, resultCourse.Subject);
                    Assert.AreEqual(responseCourse.CrsNo, resultCourse.CourseNumber);
                    Assert.AreEqual(responseCourse.CrsAcadLevel, resultCourse.AcademicLevel);
                    Assert.AreEqual(responseCourse.CrsName, resultCourse.CourseName);
                    Assert.AreEqual(responseCourse.CrsPrereqs, resultCourse.PrerequisiteText);

                    var depts = !string.IsNullOrWhiteSpace(responseCourse.CrsDepts) ?
                        responseCourse.CrsDepts.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(depts.Count, resultCourse.Departments.Count);

                    for (var j = 0; j < depts.Count; j++)
                    {
                        Assert.AreEqual(depts[j], resultCourse.Departments[j]);
                    }

                    var courseTypes = !string.IsNullOrWhiteSpace(responseCourse.CrsTypes) ?
                        responseCourse.CrsTypes.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(courseTypes.Count, resultCourse.CourseTypes.Count);
                    for (var j = 0; j < courseTypes.Count; j++)
                    {
                        Assert.AreEqual(courseTypes[j], resultCourse.CourseTypes[j]);
                    }

                    var locations = !string.IsNullOrWhiteSpace(responseCourse.Locations) ?
                        responseCourse.Locations.Split(_SM).ToList() : new List<string>();
                    Assert.AreEqual(locations.Count, resultCourse.Locations.Count);
                    for (var j = 0; j < locations.Count; j++)
                    {
                        Assert.AreEqual(locations[j], resultCourse.Locations[j]);
                    }
                }
            }

            //when transaction returns null response
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetCoursesForUpdateAsync_TransactionReturnsNullResponse()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(() => null);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
            }

            //when transaction throws exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetCoursesForUpdateAsync_TransactionReturnsException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).Throws(new ColleagueTransactionException("something happened"));
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
            }

            //when response returns empty portal courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHasEmptyCourses()
            {
                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = 2, PortalUpdatedCourses = new List<PortalUpdatedCourses>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(response.TotalCourses, result.TotalCourses);
                Assert.AreEqual(response.PortalUpdatedCourses.Count, result.Courses.Count);
                Assert.AreEqual(0, result.Courses.Count);
            }

            //when response returns null portal courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHasNullCourses()
            {
                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = 2, PortalUpdatedCourses = null };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(response.TotalCourses, result.TotalCourses);
                Assert.IsNull(response.PortalUpdatedCourses);
                Assert.IsNotNull(result.Courses);
                Assert.AreEqual(0, result.Courses.Count);
            }

            //when response total courses is null
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHaveNullTotalCourses()
            {
                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = null, PortalUpdatedCourses = new List<PortalUpdatedCourses>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.IsNull(result.TotalCourses);
                Assert.IsNotNull(result.Courses);
                Assert.AreEqual(0, result.Courses.Count);
                loggerMock.Verify(l => l.Info("Total Courses retrieved for update for Portal is null"));
            }

            //when reposnse total courses is 0
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHaveZeroTotalCourses()
            {
                var responseCourse = new PortalUpdatedCourses { CoursesId = "25310" };
                var responseCourses = new List<PortalUpdatedCourses>();
                responseCourses.Add(responseCourse);

                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = 0, PortalUpdatedCourses = responseCourses };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.TotalCourses);
                Assert.AreEqual(1, result.Courses.Count);
                loggerMock.Verify(l => l.Warn("Total Courses 0 retrieved for update for Portal is less than the actual count of list of courses retrieved 1"));
            }

            //when response total courses is a negative value
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_ResponseHaveNegativeTotalCourses()
            {
                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = -1, PortalUpdatedCourses = new List<PortalUpdatedCourses>() };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();
                Assert.IsNotNull(result);
                Assert.AreEqual(-1, result.TotalCourses);
                Assert.AreEqual(0, result.Courses.Count);
                loggerMock.Verify(l => l.Warn("Total Courses -1 retrieved for update for Portal is less than 0"));
            }

            //when reposnse total courses is > than total courses
            [TestMethod]
            public async Task PortalRepository_GetCoursesForUpdateAsync_TotalCourses_greaterThan_ListOfCourses()
            {
                var responseCourse = new PortalUpdatedCourses { CoursesId = "25310" };
                var responseCourses = new List<PortalUpdatedCourses>();
                responseCourses.Add(responseCourse);

                response = new PortalGetCoursesForUpdateResponse() { TotalCourses = 5, PortalUpdatedCourses = responseCourses };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(It.IsAny<PortalGetCoursesForUpdateRequest>())).ReturnsAsync(response);
                PortalUpdatedCoursesResult result = await repository.GetCoursesForUpdateAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(5, result.TotalCourses);
                Assert.AreEqual(1, result.Courses.Count);
                loggerMock.Verify(l => l.Info("Total Courses 5 retrieved for update for Portal is more than the actual count of list of courses retrieved 1, hence there could be more courses applicable for update for Portal"));
            }

            private PortalGetCoursesForUpdateResponse SetPortalGetCoursesForUpdateResponseData()
            {
                var responseCourses = new List<PortalUpdatedCourses>();

                //null course
                responseCourses.Add(null);

                //course with null properties
                responseCourses.Add(new PortalUpdatedCourses());

                //course with empty properties
                var responseCourseEmpty = new PortalUpdatedCourses()
                {
                    CoursesId = string.Empty,
                    CrsShortTitle = string.Empty,
                    CrsTitle = string.Empty,
                    CrsDesc = string.Empty,
                    CrsSubject = string.Empty,
                    CrsDepts = string.Empty,
                    CrsNo = string.Empty,
                    CrsAcadLevel = string.Empty,
                    CrsName = string.Empty,
                    CrsTypes = string.Empty,
                    CrsPrereqs = string.Empty,
                    Locations = string.Empty
                };
                responseCourses.Add(responseCourseEmpty);

                var responseCourseValid1 = new PortalUpdatedCourses()
                {
                    CoursesId = "10",
                    CrsShortTitle = "Studies in French",
                    CrsTitle = "Intermediate Studies in French",
                    CrsDesc = "Intermediate French Language and Literature",
                    CrsSubject = "French",
                    CrsDepts = "Modern Language and Literature",
                    CrsNo = "500",
                    CrsAcadLevel = "Graduate",
                    CrsName = "FREN-500",
                    CrsTypes = "Standard",
                    CrsPrereqs = "Take FREN-400",
                    Locations = "Main Campus"
                };
                responseCourses.Add(responseCourseValid1);

                var responseCourseValid2 = new PortalUpdatedCourses()
                {
                    CoursesId = "100",
                    CrsShortTitle = "Talmudic Studies",
                    CrsTitle = "Beginning Talmudic Studies",
                    CrsDesc = "Study of the Talmud",
                    CrsSubject = "Religious Studies",
                    CrsDepts = "Religious Studies",
                    CrsNo = "CE400",
                    CrsAcadLevel = "Continuing Education Level",
                    CrsName = "RELG-CE400",
                    CrsTypes = "Standard",
                    CrsPrereqs = "Take RELG-100 AND RELG-200",
                    Locations = ""
                };
                responseCourses.Add(responseCourseValid2);

                var responseCourseValid3 = new PortalUpdatedCourses()
                {
                    CoursesId = "980",
                    CrsShortTitle = "Comm * 1321",
                    CrsTitle = "The Art of Effective Communication",
                    CrsDesc = "The Art of Effective Communication Description",
                    CrsSubject = "Communications",
                    CrsDepts = "CommunicationsüHumanities",
                    CrsNo = "1321",
                    CrsAcadLevel = "Undergraduate",
                    CrsName = "COMM-1321",
                    CrsTypes = "StandardüBasic SkillsüHumanities",
                    CrsPrereqs = "Take RELG-100 AND RELG-200",
                    Locations = "Herndon College of MusicüCentral Campus SouthüFred Robinson CollegeüCentral District OfficeüMain Campus"
                };
                responseCourses.Add(responseCourseValid3);

                return new PortalGetCoursesForUpdateResponse()
                {
                    TotalCourses = 8,
                    PortalUpdatedCourses = responseCourses
                };
            }

        }

        [TestClass]
        public class PortalRepository_GetEventsAndRemindersAsync : PortalRepositoryTests
        {
            PortalGetEventsAndRemindersResponse ctxResponse;

            [TestInitialize]
            public void PortalRepository_GetEventsAndRemindersAsync_Initialize()
            {
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalRepository_GetEventsAndRemindersAsync_null_PersonId()
            {
                var entity = await repository.GetEventsAndRemindersAsync(null, new PortalEventsAndRemindersQueryCriteria());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetEventsAndRemindersAsync_CTX_returns_null()
            {
                ctxResponse = null;
                transManagerMock.Setup(ctx => ctx.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(It.IsAny<PortalGetEventsAndRemindersRequest>())).ReturnsAsync(ctxResponse);
                var entity = await repository.GetEventsAndRemindersAsync("0001234", new PortalEventsAndRemindersQueryCriteria());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_GetEventsAndRemindersAsync_throws_ColleagueTransactionException()
            {
                transManagerMock.Setup(ctx => ctx.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(It.IsAny<PortalGetEventsAndRemindersRequest>())).ThrowsAsync(new ColleagueTransactionException("message"));
                var entity = await repository.GetEventsAndRemindersAsync("0001234", new PortalEventsAndRemindersQueryCriteria());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalRepository_GetEventsAndRemindersAsync_throws_Exception()
            {
                transManagerMock.Setup(ctx => ctx.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(It.IsAny<PortalGetEventsAndRemindersRequest>())).ThrowsAsync(new Exception("message"));
                var entity = await repository.GetEventsAndRemindersAsync("0001234", new PortalEventsAndRemindersQueryCriteria());
            }

            [TestMethod]
            public async Task PortalRepository_GetEventsAndRemindersAsync_CTX_returns_valid_response()
            {
                ctxResponse = new PortalGetEventsAndRemindersResponse()
                {
                    HostShortDateFormat = "MDY",
                    Events = new List<Events>()
                    {
                        new Events()
                        {
                            BuildingInfo = "Building",
                            CalendarSchedulesId = "1",
                            CourseSectionCourseNumber = "HIST-100",
                            CourseSectionNumber = "01",
                            CourseSectionsId = "2",
                            CourseSectionSubject = "HIST",
                            Date = DateTime.Today,
                            Description = "Intro to World History",
                            EndTime = DateTime.Today.AddHours(10),
                            EventType = "CS Course Section Meeting",
                            Participants = "Participant, Participant 2",
                            RoomInfo = "100",
                            StartTime = DateTime.Today.AddHours(9)
                        }
                    },
                    Reminders = new List<Reminders>()
                    {
                        new Reminders()
                        {
                            ReminderActionTime = DateTime.Today.AddHours(17),
                            ReminderCity = "City",
                            ReminderEndDate = DateTime.Today.AddDays(1),
                            ReminderEndTime = DateTime.Today.AddDays(1).AddHours(18),
                            ReminderId = "1",
                            ReminderParticipants = "Participant, Participant 2",
                            ReminderRegions = "Region",
                            ReminderShortText = "Short Text",
                            ReminderStartDate = DateTime.Today,
                            ReminderType = "TH Thank You"
                        }
                    }
                };
                transManagerMock.Setup(ctx => ctx.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(It.IsAny<PortalGetEventsAndRemindersRequest>())).ReturnsAsync(ctxResponse);
                var entity = await repository.GetEventsAndRemindersAsync("0001234", new PortalEventsAndRemindersQueryCriteria());
                Assert.IsNotNull(entity);
                Assert.AreEqual(ctxResponse.HostShortDateFormat, entity.HostShortDateFormat);
                Assert.AreEqual(ctxResponse.Events.Count, entity.Events.Count);
                Assert.AreEqual(ctxResponse.Reminders.Count, entity.Reminders.Count);

            }

            [TestMethod]
            public async Task PortalRepository_GetEventsAndRemindersAsync_CTX_returns_valid_response_Bad_Data()
            {
                ctxResponse = new PortalGetEventsAndRemindersResponse()
                {
                    HostShortDateFormat = "MDY",
                    Events = new List<Events>()
                    {
                        new Events()
                        {
                            BuildingInfo = "Building",
                            CalendarSchedulesId = "1",
                            CourseSectionCourseNumber = "HIST-100",
                            CourseSectionNumber = "01",
                            CourseSectionsId = "2",
                            CourseSectionSubject = "HIST",
                            Date = DateTime.Today,
                            Description = "Intro to World History",
                            EndTime = DateTime.Today.AddHours(8),
                            EventType = "CS Course Section Meeting",
                            Participants = "Participant, Participant 2",
                            RoomInfo = "100",
                            StartTime = DateTime.Today.AddHours(9)
                        }
                    },
                    Reminders = new List<Reminders>()
                    {
                        new Reminders()
                        {
                            ReminderActionTime = DateTime.Today.AddHours(17),
                            ReminderCity = "City",
                            ReminderEndDate = DateTime.Today.AddDays(-1),
                            ReminderEndTime = DateTime.Today.AddDays(1).AddHours(18),
                            ReminderId = "1",
                            ReminderParticipants = "Participant, Participant 2",
                            ReminderRegions = "Region",
                            ReminderShortText = "Short Text",
                            ReminderStartDate = DateTime.Today,
                            ReminderType = "TH Thank You"
                        }
                    }
                };
                transManagerMock.Setup(ctx => ctx.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(It.IsAny<PortalGetEventsAndRemindersRequest>())).ReturnsAsync(ctxResponse);
                var entity = await repository.GetEventsAndRemindersAsync("0001234", new PortalEventsAndRemindersQueryCriteria());
                Assert.IsNotNull(entity);
                Assert.AreEqual(ctxResponse.HostShortDateFormat, entity.HostShortDateFormat);
                Assert.AreEqual(0, entity.Events.Count);
                Assert.AreEqual(0, entity.Reminders.Count);

            }

        }

        [TestClass]
        public class PortalRepository_UpdateStudentPreferredCourseSectionsAsync : PortalRepositoryTests
        {
            [TestInitialize]
            public void PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_null_StudentId()
            {
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync(null, new List<string>() { "12345" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_null_CourseSectionIds()
            {
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_empty_CourseSectionIds()
            {
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_CourseSectionIds_null_items()
            {
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { null, string.Empty });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Ctx_returns_null()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(() => null);
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", null });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Ctx_throws_ColleagueTransactionException()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ThrowsAsync(
                    new ColleagueTransactionException("Transaction error!"));
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Ctx_returns_null_results()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(
                    new PortalUpdatePreferredSectionsResponse()
                    {
                        Results = null
                    });
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Ctx_returns_empty_results()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(
                    new PortalUpdatePreferredSectionsResponse()
                    {
                        Results = new List<Results>()
                    });
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_Ctx_returns_missing_results()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(
                    new PortalUpdatePreferredSectionsResponse()
                    {
                        Results = new List<Results>()
                        {
                            new Results()
                            {
                                AddStatus = "OK",
                                CourseSectionId = "XYZ",
                                Message = ""
                            }
                        }
                    });
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_returns_results_with_data_error()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(
                    new PortalUpdatePreferredSectionsResponse()
                    {
                        Results = new List<Results>()
                        {
                            new Results()
                            {
                                AddStatus = "OK",
                                CourseSectionId = "123",
                                Message = ""
                            },
                            new Results()
                            {
                                AddStatus = "XYZ",
                                CourseSectionId = "456",
                                Message = "Could not add 456."
                            }
                        }
                    });
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
            }

            [TestMethod]
            public async Task PortalRepository_UpdateStudentPreferredCourseSectionsAsync_valid()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(It.IsAny<PortalUpdatePreferredSectionsRequest>())).ReturnsAsync(
                    new PortalUpdatePreferredSectionsResponse()
                    {
                        Results = new List<Results>()
                        {
                            new Results()
                            {
                                AddStatus = "OK",
                                CourseSectionId = "123",
                                Message = ""
                            },
                            new Results()
                            {
                                AddStatus = "ERROR",
                                CourseSectionId = "456",
                                Message = "Could not add 456."
                            }
                        }
                    });
                var entity = await repository.UpdateStudentPreferredCourseSectionsAsync("0012345", new List<string>() { "123", "456" });
                Assert.AreEqual(2, entity.Count());
            }
        }
    }
}
