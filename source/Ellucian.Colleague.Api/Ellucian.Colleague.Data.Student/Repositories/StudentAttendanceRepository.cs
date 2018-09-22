// Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAttendanceRepository : BaseColleagueRepository, IStudentAttendanceRepository
    {

        private string colleagueTimeZone;
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public StudentAttendanceRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using 24 hours for the RegistrationSections cache timeout - otherwise not caching section info.
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }
        /// <summary>
        /// Query Student Attendance Information
        /// </summary>
        /// <param name="querySectionIds"></param>
        /// <param name="includeDroppedWithdrawn"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentAttendance>> GetStudentAttendancesAsync(List<string> querySectionIds, DateTime? date)
        {
            if (querySectionIds == null || !querySectionIds.Any())
            {
                throw new ArgumentNullException("querySectionIds", "At list one section IdO must be provided. ");
            }
            var studentAttendances = new List<StudentAttendance>();
            var queryStringSectionIds = querySectionIds.ToArray();
            string selectCriteria = "WITH SCS.COURSE.SECTION EQ '?'";
            string[] studentCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", selectCriteria, queryStringSectionIds);
            var studentCourseSecData = await RetrieveBulkDataInBatchAsync<StudentCourseSec>(studentCourseSecIds, "STUDENT.COURSE.SEC");
            studentAttendances = BuildStudentAttendances(studentCourseSecData, date);
            return studentAttendances;
        }


        /// <summary>
        /// Query Student Section Attendances Information.
        /// StudentId must be provided.
        /// If sectionsIds is not provided then attendances from all the sections for the given studentId is retrieved.
        /// If sectionIds is provided then attendances from only those sections which are in list and belongs to given student are retrieved.
        /// </summary>
        /// <param name="sectionIds">list of section Ids(optional)</param>
        /// <param name="studentId">student Id </param>
        /// <returns ><see cref="StudentSectionsAttendances"/> SectionWise Student Attendances </returns>
        public async Task<StudentSectionsAttendances> GetStudentSectionAttendancesAsync(string studentId, IEnumerable<string> sectionIds)
        {
            string selectCriteria = string.Empty;
            string[] studentCourseSecIds;
            StudentSectionsAttendances StudentSectionsAttendances = new StudentSectionsAttendances(studentId);
            if (studentId == null )
            {
                throw new ArgumentNullException("studentId", "Student Id must be provided. ");
            }
           
            var studentAttendances = new List<StudentAttendance>();
            if(sectionIds!=null && sectionIds.Any())
            {
                var queryStringSectionIds = sectionIds.ToArray();
                selectCriteria = "WITH SCS.COURSE.SECTION EQ '?'";
                selectCriteria += "AND SCS.STUDENT EQ " + "'" + studentId + "'";
                studentCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", selectCriteria, queryStringSectionIds);
            }
            else
            {
                selectCriteria = "WITH SCS.STUDENT EQ " + "'" + studentId + "'";
                studentCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", selectCriteria);
            }
           
            var studentCourseSecData = await RetrieveBulkDataInBatchAsync<StudentCourseSec>(studentCourseSecIds, "STUDENT.COURSE.SEC");
             studentAttendances = BuildStudentAttendances(studentCourseSecData,null);
            StudentSectionsAttendances.AddStudentAttendances(studentAttendances);
            return StudentSectionsAttendances;
        }
        /// <summary>
        /// Updates student attendance information for a student in a section at a specific meeting instance
        /// </summary>
        /// <param name="StudentAttendanceToUpdate">StudentAttendance to update</param>
        /// <returns>Updated StudentAttendance entity</returns>
        public async Task<StudentAttendance> UpdateStudentAttendanceAsync(StudentAttendance studentAttendanceToUpdate, IEnumerable<SectionMeetingInstance> sectionMeetingInstances)
        {
            if (studentAttendanceToUpdate == null)
            {
                throw new ArgumentNullException("studentAttendanceToUpdate", "StudentAttendance to update can not be null.");
            }
            if (sectionMeetingInstances == null || !sectionMeetingInstances.Any())
            {
                throw new ApplicationException("No SectionMeetingInstances for section for which attendance is being updated.");
            }

            // If the incoming StudentAttendance entity does not have a student course sec ID - see if we can find it. Otherwise error.
            string studentCourseSecId = string.Empty;
            if (!string.IsNullOrEmpty(studentAttendanceToUpdate.StudentCourseSectionId))
            {
                studentCourseSecId = studentAttendanceToUpdate.StudentCourseSectionId;
            }
            else
            {
                string selectCriteria = "WITH SCS.COURSE.SECTION EQ '" + studentAttendanceToUpdate.SectionId + "' AND SCS.STUDENT EQ '" + studentAttendanceToUpdate.StudentId + "'";
                string[] studentCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", selectCriteria);
                // There should be 1 and only 1 ID returned.  Any other situation should generate an error and not continue.
                if (studentCourseSecIds == null || !studentCourseSecIds.Any() || studentCourseSecIds.Count() > 1)
                {
                    string err = "Update Attendance: Unable to find STUDENT.COURSE.SEC FOR STUDENT  " + studentAttendanceToUpdate.StudentId + " in section " + studentAttendanceToUpdate.SectionId;
                    logger.Error(err);
                    throw new ApplicationException(err);
                }
                studentCourseSecId = studentCourseSecIds.First();
            }

            // Find the id of the appropriate calendar schedule being updated so we can get its id.
            var calendarSchedule = sectionMeetingInstances.Where(s => s.BelongsToStudentAttendance(studentAttendanceToUpdate)).FirstOrDefault();
            if (calendarSchedule == null)
            {
                string err = "Update Attendance: Section does not have a meeting instance cooresponding to the student attendance being updated. ";
                logger.Error(err);
                throw new Exception(err);
            }
            // For now we are only supplying one student attendance item to update.
            var request = new UpdateStudentAttendanceRequest();
            request.SectionId = studentAttendanceToUpdate.SectionId;
            request.CalendarSchedulesId = calendarSchedule.Id;
            request.StudentAttendances = new List<StudentAttendances>() {
                new StudentAttendances() {
                    StudentCourseSecIds = studentCourseSecId,
                    AttendanceTypes = studentAttendanceToUpdate.AttendanceCategoryCode,
                    MinutesAttended = studentAttendanceToUpdate.MinutesAttended,
                    Reasons = studentAttendanceToUpdate.Comment
                }
            };

            // EXECUTE the COLLEAGUE TRANSACTION
            UpdateStudentAttendanceResponse response;
            try
            {
                response = await transactionInvoker.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(request);
            }
            catch (Exception)
            {
                string err = "Update Attendance: Student Attendance not updated for STUDENT.COURSE.SEC " + studentAttendanceToUpdate.StudentCourseSectionId;
                logger.Error(err);
                throw new ApplicationException(err);
            }

            // VALIDATE RESULTS OF THE UPDATE
            if (response.ErrorFlag)
            {
                // likely situation is that there were no items to update.
                string err = response.ErrorMessage + ". Student Attendance information not updated.";
                logger.Error(err);
                throw new ApplicationException(err);
            }
            // Report all individual errors.
            if (response.ErroredStudentAttendances != null && response.ErroredStudentAttendances.Any())
            {
                string finalString = string.Join("|", response.ErroredStudentAttendances.Select(x => x.ErrorMessages));
                logger.Error(finalString);
                if (finalString.Contains("is locked and cannot be updated"))
                {
                    throw new RecordLockException(finalString, "StudentCourseSec", studentCourseSecId);
                }
                throw new ApplicationException(finalString);
            }
            // Since we are only updating one make sure it is in the updated list.
            StudentAttendance updatedStudentAttendance = null;
            if (!response.UpdatedStudentCourseSecId.Contains(studentCourseSecId))
            {
                string err = "Update Attendance: Student Attendance not updated for STUDENT.COURSE.SEC " + studentAttendanceToUpdate.StudentCourseSectionId;
                logger.Error(err);
                throw new ApplicationException(err);
            }
            // No obvious errors - get & return updated value.
            var sectionids = new List<string>() { studentAttendanceToUpdate.SectionId };
            var studentAttendances = await GetStudentAttendancesAsync(sectionids, studentAttendanceToUpdate.MeetingDate);
            // Since there could be multiple meeting times for a section on a specific day, need to make sure we get the proper attendence item to return
            updatedStudentAttendance = studentAttendances.Where(sa => sa.Equals(studentAttendanceToUpdate)).FirstOrDefault();

            if (updatedStudentAttendance == null)
            {
                // This would mean the Colleague TX thought it was updated but we could not find it (get it) afterwards.
                string err = "Update Attendance: Student Attendance not found for STUDENT.COURSE.SEC " + studentAttendanceToUpdate.StudentCourseSectionId;
                logger.Error(err);
                throw new ApplicationException(err);
            }
            return updatedStudentAttendance;




        }

        /// <summary>
        /// Updates student attendance information for students in a section at a specific meeting instance
        /// </summary>
        /// <param name="SectionAttendanceToUpdate">SectionAttendance to update</param>
        /// <param name="crosslistSectionIds">The section Ids of cross-listed sections for the section being updated.</param>
        /// <returns>SectionAttendanceResponse that included what was updated and what wasn't</returns>
        public async Task<SectionAttendanceResponse> UpdateSectionAttendanceAsync(SectionAttendance sectionAttendanceToUpdate, List<string> crosslistSectionIds)
        {
            if (sectionAttendanceToUpdate == null)
            {
                throw new ArgumentNullException("sectionAttendanceToUpdate", "SectionAttendance to update can not be null.");
            }
            if (sectionAttendanceToUpdate.StudentAttendances == null || !sectionAttendanceToUpdate.StudentAttendances.Any())
            {
                throw new ArgumentException("Must provide at least one student attendance to be updated.");
            }

            // BUILD the request

            var request = new UpdateStudentAttendanceRequest();
            request.SectionId = sectionAttendanceToUpdate.SectionId;
            request.CalendarSchedulesId = sectionAttendanceToUpdate.MeetingInstance.Id;
            request.StudentAttendances = new List<StudentAttendances>();

            foreach (var attendance in sectionAttendanceToUpdate.StudentAttendances)
            {
                var studentAttendancesContract = new StudentAttendances()
                {
                    StudentCourseSecIds = attendance.StudentCourseSectionId,
                    AttendanceTypes = attendance.AttendanceCategoryCode,
                    Reasons = attendance.Comment,
                    MinutesAttended = attendance.MinutesAttended

                };
                request.StudentAttendances.Add(studentAttendancesContract);

            }

            // EXECUTE the COLLEAGUE TRANSACTION
            UpdateStudentAttendanceResponse response;
            try
            {
                response = await transactionInvoker.ExecuteAsync<UpdateStudentAttendanceRequest, UpdateStudentAttendanceResponse>(request);
            }
            catch (Exception)
            {
                string err = "Update Attendance: Attendance Information not updated for STUDENT.COURSE.SEC " + sectionAttendanceToUpdate.SectionId;
                logger.Error(err);
                throw new ApplicationException(err);
            }

            // VALIDATE RESULTS OF THE UPDATE
            if (response.ErrorFlag || ((response.UpdatedStudentCourseSecId == null || !response.UpdatedStudentCourseSecId.Any()) && (response.ErroredStudentAttendances == null || !response.ErroredStudentAttendances.Any())))
            {
                // If the overall flag is set, or if there are no successfully updated student course sec items but no errors either then throw and overall application exception.
                // Something went horribly wrong.
                string err = response.ErrorMessage + ". Student Attendance information not updated for section " + sectionAttendanceToUpdate.SectionId;
                logger.Error(err);
                throw new ApplicationException(err);
            }

            // If there are successes and/or failures, BUILD the return object with the successes and failures.
            SectionAttendanceResponse sectionAttendanceResponse = new SectionAttendanceResponse(sectionAttendanceToUpdate.SectionId, sectionAttendanceToUpdate.MeetingInstance);
            // First update and Log any individual errors.
            if (response.ErroredStudentAttendances != null && response.ErroredStudentAttendances.Any())
            {
                foreach (var errorItem in response.ErroredStudentAttendances)
                {
                    if (errorItem != null)
                    {
                        // Skip anything without a course sec Id.
                        if (!string.IsNullOrEmpty(errorItem.ErroredStudentCourseSecIds))
                        {
                            sectionAttendanceResponse.AddStudentAttendanceError(new StudentSectionAttendanceError(errorItem.ErroredStudentCourseSecIds) { ErrorMessage = errorItem.ErrorMessages });
                            logger.Error("Update Attendance: Unable to update Student Course Sec Id" + errorItem.ErroredStudentCourseSecIds + ": " + errorItem.ErrorMessages);
                        }
                    }
                }

            }
            // Get a consolidated list of the section Ids involved in this update. The section Ids and all the crosslisted sections.
            var sectionIds = new List<string>();
            if (crosslistSectionIds != null && crosslistSectionIds.Any())
            {
                sectionIds = crosslistSectionIds;
                if (!sectionIds.Contains(sectionAttendanceToUpdate.SectionId))
                {
                    sectionIds.Add(sectionAttendanceToUpdate.SectionId);
                }
            }
            else
            {
                sectionIds.Add(sectionAttendanceToUpdate.SectionId);
            }
            var studentAttendances = await GetStudentAttendancesAsync(sectionIds, sectionAttendanceToUpdate.MeetingInstance.MeetingDate);
            if (response.UpdatedStudentCourseSecId != null && response.UpdatedStudentCourseSecId.Any())
            {
                foreach (var courseSecId in response.UpdatedStudentCourseSecId)
                {
                    if (!string.IsNullOrEmpty(courseSecId))
                    {
                        var updatedAttendance = studentAttendances.Where(sa => sa.StudentCourseSectionId == courseSecId && sectionAttendanceToUpdate.MeetingInstance.BelongsToStudentAttendance(sa)).FirstOrDefault();
                        if (updatedAttendance != null)
                        {
                            sectionAttendanceResponse.AddUpdatedStudentAttendance(updatedAttendance);
                        }
                        else
                        {
                            sectionAttendanceResponse.AddStudentCourseSectionsWithDeletedAttendance(courseSecId);
                            logger.Info(string.Format("Update Attendance: Attendance updated for Student Course Sec {0} for {1} {2}-{3}." + Environment.NewLine +
                                "Unable to build student attendance for this section meeting for one of the following reasons:" + Environment.NewLine +
                                "- student does not have any attendance data for {0} for {1} {2}-{3}, meaning it is not stored in the database" + Environment.NewLine +
                                "- there was an error retrieving attendance data for Student Course Sec {0} for {1} {2}-{3}.",
                                courseSecId,
                                sectionAttendanceToUpdate.MeetingInstance.MeetingDate.ToShortDateString(),
                                sectionAttendanceToUpdate.MeetingInstance.StartTime.HasValue ? sectionAttendanceToUpdate.MeetingInstance.StartTime.Value.ToLocalDateTime(colleagueTimeZone).ToShortTimeString() : null,
                                sectionAttendanceToUpdate.MeetingInstance.EndTime.HasValue ? sectionAttendanceToUpdate.MeetingInstance.EndTime.Value.ToLocalDateTime(colleagueTimeZone).ToShortTimeString() : null));
                        }
                    }
                }
            }

            return sectionAttendanceResponse;
        }

        /// Builds student attendance entities from StudentCourseSec data contracts.
        /// </summary>
        /// <param name="studentCourseSecData">Collection of StudentCourseSec records</param>
        /// <param name="date">optional date if only one date of attendances is requested</param>
        /// <returns></returns>
        private List<StudentAttendance> BuildStudentAttendances(List<StudentCourseSec> studentCourseSecData, DateTime? date)
        {
            var studentAttendances = new List<StudentAttendance>();
            if (studentCourseSecData != null && studentCourseSecData.Any())
            {
                // Build a dictionary of STUDENT.COURSE.SEC data keyed by student for easy retrieval later
                Dictionary<string, List<StudentCourseSec>> studentCourseSecDict = new Dictionary<string, List<StudentCourseSec>>();
                foreach(var scs in studentCourseSecData)
                {
                    if (studentCourseSecDict.ContainsKey(scs.ScsStudent))
                    {
                        studentCourseSecDict[scs.ScsStudent].Add(scs);
                    } else
                    {
                        studentCourseSecDict.Add(scs.ScsStudent, new List<StudentCourseSec>() { scs });
                    }
                }

                foreach (var studentCourseSec in studentCourseSecData)
                {
                    foreach (var att in studentCourseSec.ScsAttendanceEntityAssociation)
                    {
                        if (att.ScsAbsentDatesAssocMember.HasValue)
                        {
                            if (!date.HasValue || Nullable.Compare<DateTime>(att.ScsAbsentDatesAssocMember, date) == 0)
                            {
                                try
                                {
                                    DateTimeOffset? meetingStartTime = att.ScsAttendanceStartTimesAssocMember.HasValue ? att.ScsAttendanceStartTimesAssocMember.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                                    DateTimeOffset? meetingEndTime = att.ScsAttendanceEndTimesAssocMember.HasValue ? att.ScsAttendanceEndTimesAssocMember.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                                    var attendances = studentCourseSecDict[studentCourseSec.ScsStudent].Where(s=>s.ScsCourseSection==studentCourseSec.ScsCourseSection).SelectMany(scs => scs.ScsAttendanceEntityAssociation).ToList();
                                    int cumulativeMinutesAttended = attendances.Sum(scsa => scsa.ScsAttendanceMinutesAssocMember ?? 0);
                                    // If no date specified, use the current item's date as reference point for minutes attended to date
                                    DateTime? dateForComparison = date.HasValue ? date.Value : att.ScsAbsentDatesAssocMember ?? null;
                                    int minutesAttendedToDate = 0;
                                    if (dateForComparison.HasValue)
                                    {
                                        minutesAttendedToDate = attendances.Where(scs => scs.ScsAbsentDatesAssocMember.HasValue && scs.ScsAbsentDatesAssocMember.Value.CompareTo(dateForComparison.Value) <= 0).Sum(scsa => scsa.ScsAttendanceMinutesAssocMember ?? 0);
                                    }
                                    var studentAttendance = new StudentAttendance(studentCourseSec.ScsStudent, studentCourseSec.ScsCourseSection, att.ScsAbsentDatesAssocMember.Value, att.ScsAttendanceTypesAssocMember,
                                        att.ScsAttendanceMinutesAssocMember, att.ScsAttendanceReasonAssocMember)
                                    {
                                        StudentCourseSectionId = studentCourseSec.Recordkey,
                                        StartTime = meetingStartTime,
                                        EndTime = meetingEndTime,
                                        InstructionalMethod = string.IsNullOrEmpty(att.ScsAttendanceInstrMethodsAssocMember) ? null : att.ScsAttendanceInstrMethodsAssocMember.ToUpper(),
                                        MinutesAttendedToDate = minutesAttendedToDate,
                                        CumulativeMinutesAttended = cumulativeMinutesAttended
                                    };
                                    studentAttendances.Add(studentAttendance);
                                }
                                catch (Exception ex)
                                {
                                    var attendanceError = "Unable to update attendance info for item in STUDENT.COURSE.SEC " + studentCourseSec.Recordkey;
                                    LogDataError("Student Attendance Error", studentCourseSec.Recordkey, studentCourseSec, ex, attendanceError);
                                }
                            }
                        }
                        else
                        {
                            var attendanceError = "Unable to update attendance info for item in STUDENT.COURSE.SEC " + studentCourseSec.Recordkey + ". Missing date";
                            logger.Error(attendanceError);
                            LogDataError("Student Attendance", studentCourseSec.Recordkey, studentCourseSec);
                        }
                    }


                }
            }
            return studentAttendances;
        }


        private async Task<List<T>> RetrieveBulkDataInBatchAsync<T>(IEnumerable<string> Ids, string tableToRead, int batchCount = 5000) where T : class, IColleagueEntity
        {
            // to read in batch of  less than 5000 

            List<T> sectionsToBuild = new List<T>();
            if (Ids != null)
            {
                List<string> IdsToSearch = Ids.ToList();
                int howManyTimes = (IdsToSearch.Count / batchCount);

                for (int i = 0; i <= howManyTimes; i++)
                {
                    var idSubList = IdsToSearch.Skip(i * batchCount).Take(batchCount);
                    if (idSubList != null && idSubList.Count() > 0)
                    {
                        List<T> sectionsRetrieved = (await DataReader.BulkReadRecordAsync<T>(tableToRead, idSubList.ToArray())).ToList();
                        sectionsToBuild.AddRange(sectionsRetrieved);
                    }
                }
            }
            return sectionsToBuild;
        }
    }
}
