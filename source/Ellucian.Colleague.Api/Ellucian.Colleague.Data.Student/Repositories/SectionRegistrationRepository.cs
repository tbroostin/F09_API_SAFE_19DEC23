// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SectionRegistrationRepository : BaseColleagueRepository, ISectionRegistrationRepository
    {
        private RepositoryException exception;
        const string AllSectionRegistrationsCache = "section-registrations";
        const int AllSectionRegistrationsCacheTimeout = 20;
        public SectionRegistrationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }
        /// <summary>
        /// Register a student into a section using HeDM
        /// </summary>
        /// <param name="request">Registration Request transaction</param>
        /// <returns>Registration Response <see cref="RegistrationResponse"> object</returns>
        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request)
        {
            UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
            updateRequest.RegSections = new List<RegSections>();

            updateRequest.StudentId = request.StudentId;
            updateRequest.CreateStudentFlag = request.CreateStudentFlag;
            // For every section submitted, add a Sections object to the updateRequest
            foreach (var section in request.Sections)
            {

                updateRequest.RegSections.Add(new RegSections() { SectionIds = section.SectionId, SectionAction = section.Action.ToString(), SectionCredits = section.Credits, SectionDate = section.RegistrationDate });
                updateRequest.SecRegGuid = section.Guid;
            }

            // Submit the registration
            UpdateSectionRegistrationResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest);

            // If there is any error message - throw an exception 
            if (updateResponse.ErrorOccurred)
            {
                var errorMessage = "Error(s) occurred updating section-registrations '" + request.StudentId + request.Sections.ElementAt(0) + "':";
                errorMessage += string.Join(Environment.NewLine, updateResponse.ErrorMessage);
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException(updateResponse.ErrorMessage);
            }

            // Process the messages returned by colleague registration 
            var outputMessages = new List<RegistrationMessage>();
            if (updateResponse.RegMessages.Count > 0)
            {
                foreach (var message in updateResponse.RegMessages)
                {
                    outputMessages.Add(new RegistrationMessage() { Message = message.Message, SectionId = message.MessageSection });
                }
            }

            return new RegistrationResponse(outputMessages, updateResponse.IpcRegId, null);
        }
        /// <summary>
        /// Register a student into a section using HeDM
        /// </summary>
        /// <param name="request">Registration Request transaction</param>
        /// <returns>Registration Response <see cref="RegistrationResponse"> object</returns>
        public async Task<SectionRegistrationResponse> GetAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var stcKey = await GetSectionRegistrationIdFromGuidAsync(guid);

            // Read original STUDENT.ACAD.CRED to get the actual Status Code for the section.
            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", stcKey);
            if (studentAcadCred == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.ACAD.CRED’, Record ID: '", stcKey, "'"));
            }
            // Read original STUDENT.COURSE.SEC to get the registration mode for the student in this section.
            var scsKey = studentAcadCred.StcStudentCourseSec;
            if (string.IsNullOrEmpty(scsKey))
            {
                // No STC.STUDENT.COURSE.SEC record associated to this STUDENT.ACAD.CRED.  This is not valid.
                throw new KeyNotFoundException(string.Format("SectionRegistration GUID '{0}' not found.", guid));
            }
            var studentCourseSec = await DataReader.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", scsKey);
            if (studentCourseSec == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, Record ID: '", scsKey, "'"));
            }

            var studentId = studentAcadCred.StcPersonId;
            var sectionId = studentCourseSec.ScsCourseSection;
            var statusCode = studentAcadCred.StcStatus.ElementAt(0);
            var gradeScheme = studentAcadCred.StcGradeScheme;
            var passAudit = studentCourseSec.ScsPassAudit;
            //Get all grades, midterm, final & verified
            var midTermGrades = GetMidTermGrades(studentCourseSec);

            var verifiedGrade = string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ?
                                    null :
                                    new VerifiedTermGrade(studentAcadCred.StcVerifiedGrade, studentAcadCred.StcVerifiedGradeDate, studentAcadCred.StcVerifiedGradeChgopr);

            var finalGradeDate = studentAcadCred.StcVerifiedGradeDate != null ? studentAcadCred.StcVerifiedGradeDate : studentAcadCred.StudentAcadCredChgdate;

            var finalGrade = string.IsNullOrEmpty(studentAcadCred.StcFinalGrade) ?
                                    null :
                                    new TermGrade(studentAcadCred.StcFinalGrade, finalGradeDate, studentAcadCred.StudentAcadCredChgopr);

            var sectionRegistrationResponse = new SectionRegistrationResponse(studentAcadCred.RecordGuid, studentId, sectionId, statusCode, gradeScheme, passAudit, new List<RegistrationMessage>());
            sectionRegistrationResponse.MidTermGrades = midTermGrades;
            sectionRegistrationResponse.FinalTermGrade = finalGrade;
            sectionRegistrationResponse.VerifiedTermGrade = verifiedGrade;
            sectionRegistrationResponse.InvolvementStartOn = studentAcadCred.StcStartDate;
            sectionRegistrationResponse.InvolvementEndOn = studentAcadCred.StcEndDate;
            sectionRegistrationResponse.ReportingStatus = studentCourseSec.ScsNeverAttendedFlag;
            sectionRegistrationResponse.ReportingLastDayOdAttendance = studentCourseSec.ScsLastAttendDate;
            sectionRegistrationResponse.GradeExtentionExpDate = studentAcadCred.StcGradeExpireDate;
            sectionRegistrationResponse.TranscriptVerifiedGradeDate = studentAcadCred.StcVerifiedGradeDate;
            sectionRegistrationResponse.TranscriptVerifiedBy = studentAcadCred.StcVerifiedGradeChgopr;
            //V7 changes
            sectionRegistrationResponse.CreditType = studentAcadCred.StcCredType;
            sectionRegistrationResponse.AcademicLevel = studentAcadCred.StcAcadLevel;
            sectionRegistrationResponse.Ceus = studentAcadCred.StcAttCeus.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCeus : studentAcadCred.StcCeus;
            sectionRegistrationResponse.EarnedCeus = studentAcadCred.StcCmplCeus;
            sectionRegistrationResponse.Credit = studentAcadCred.StcAttCred.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCred : studentAcadCred.StcCred;
            sectionRegistrationResponse.EarnedCredit = studentAcadCred.StcCmplCred;
            sectionRegistrationResponse.GradePoint = studentAcadCred.StcGradePts;
            sectionRegistrationResponse.ReplCode = studentAcadCred.StcReplCode;
            sectionRegistrationResponse.RepeatedAcadCreds = studentAcadCred.StcRepeatedAcadCred;
            sectionRegistrationResponse.AltcumContribCmplCred = studentAcadCred.StcAltcumContribCmplCred;
            sectionRegistrationResponse.AltcumContribGpaCred = studentAcadCred.StcAltcumContribGpaCred;

            return sectionRegistrationResponse;
        }

        /// <summary>
        /// Filter for section registrations
        /// </summary>    
        /// <param name="sectionId">sectionId</param>
        /// <param name="personId">personId</param>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrationsAsync(int offset,
            int limit, string sectionId, string personId)
        {
            SectionRegistrationResponse sectReg = new SectionRegistrationResponse(string.Empty, personId, sectionId, string.Empty, new List<RegistrationMessage>());

            var results = await this.GetSectionRegistrations2Async(offset, limit, sectReg, string.Empty, string.Empty);
            return results;
        }

        #region V16.0.0
        /// <summary>
        /// Section registration with paging and filters.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="sectionId"></param>
        /// <param name="personId"></param>
        /// <param name="acadPeriod"></param>
        /// <param name="sectionInstructor"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrations2Async(int offset,
            int limit, SectionRegistrationResponse sectReg, string acadPeriod, string sectionInstructor)
        {
            var sectionRegistrations = new List<SectionRegistrationResponse>();
            var totalCount = 0;
            string[] limitingKeys = null;
            var criteria = string.Empty;

            //STC.PERSON.ID (criteria)
            if (sectReg != null && !string.IsNullOrEmpty(sectReg.StudentId))
            {
                criteria = "WITH PST.STUDENT.ACAD.CRED BY.EXP PST.STUDENT.ACAD.CRED SAVING PST.STUDENT.ACAD.CRED";
                var acadCredIds = await DataReader.SelectAsync("PERSON.ST", new string[] { sectReg.StudentId }, criteria);
                // IF the person is a student, but has never been registered for a section, acadCredIds will come back
                // empty, which the SelectAsync call will interpret as "no limiting list" which is wrong in this case.
                if (acadCredIds == null || !acadCredIds.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }

                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", acadCredIds, "WITH STC.STUDENT.COURSE.SEC NE ''");
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }
            }

            //STC.TERM (named query)
            if (!string.IsNullOrWhiteSpace(acadPeriod))
            {
                criteria = string.Format("WITH STC.TERM EQ '{0}' AND WITH STC.STUDENT.COURSE.SEC NE ''", acadPeriod);
                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }
            }

            //SCS.COURSE.SECTION (criteria)
            if (sectReg != null && !string.IsNullOrEmpty(sectReg.SectionId))
            {
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    criteria = string.Format("WITH SCS.COURSE.SECTION EQ '{0}' SAVING UNIQUE SCS.STUDENT.ACAD.CRED",
                        sectReg.SectionId);
                    limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", criteria);
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                    }
                }
                else
                {
                    criteria = string.Format("WITH STC.STUDENT.COURSE.SEC NE '' AND WITH SCS.COURSE.SECTION EQ '{0}'", sectReg.SectionId);
                    limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, criteria);
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                    }
                }
            }

            //COURSE.SECTIONS (named query)
            if (!string.IsNullOrWhiteSpace(sectionInstructor))
            {
                //get all the ids COURSE.SEC.FACULTY...
                criteria = string.Format("WITH CSF.FACULTY EQ '{0}'", sectionInstructor);
                var csfId = await DataReader.SelectAsync("COURSE.SEC.FACULTY", criteria);
                if (csfId == null || !csfId.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }

                var instructorCriteria = "SAVING UNIQUE CSF.COURSE.SECTION";
                var courseSecIds = await DataReader.SelectAsync("COURSE.SEC.FACULTY", csfId, instructorCriteria);
                if (courseSecIds == null || !courseSecIds.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }
                criteria = "WITH SCS.COURSE.SECTION EQ '?'";
                var stCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", criteria, courseSecIds.Distinct().ToArray());

                var stAcadCredLimitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", stCourseSecIds, "SAVING UNIQUE SCS.STUDENT.ACAD.CRED");
                if (stAcadCredLimitingKeys == null || !stAcadCredLimitingKeys.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }
                if (limitingKeys == null)
                {
                    limitingKeys = stAcadCredLimitingKeys;
                }
                else
                {
                    limitingKeys = limitingKeys.Intersect(stAcadCredLimitingKeys).ToArray();
                }
            }

            //This is for GET ALL
            if (string.IsNullOrEmpty(criteria))
            {
                var sectionRegistrationsCacheKey = "AllSectionRegistrationsKeys";
                if (offset == 0 && ContainsKey(BuildFullCacheKey(sectionRegistrationsCacheKey)))
                {
                    ClearCache(new List<string> { sectionRegistrationsCacheKey });
                }

                limitingKeys = await GetOrAddToCacheAsync<string[]>(sectionRegistrationsCacheKey,
                async () =>
                {
                    limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", "WITH STC.STUDENT.COURSE.SEC NE ''");
                    Array.Sort(limitingKeys);
                    return limitingKeys;
                }, 20);
            }
            else
            {
                // Execute sort from limitingKeys built from filtering
                Array.Sort(limitingKeys);
            }

            if (limitingKeys == null || !limitingKeys.Any())
            {
                return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
            }

            totalCount = limitingKeys.Count();

            //Array.Sort(limitingKeys);
            var sublist = limitingKeys.Skip(offset).Take(limit).ToArray();

            var studentAcadCredsPage =
                await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", sublist);

            var scsKey = studentAcadCredsPage.Select(s => s.StcStudentCourseSec).Distinct();
            var studentCourseSecs = await DataReader.BulkReadRecordAsync<StudentCourseSec>(scsKey.ToArray());

            if (studentCourseSecs.Count() != sublist.Count())
            {
                var courseSecIds = studentCourseSecs.Select(scs => scs.Recordkey);
                var missingIds = scsKey.Except(courseSecIds);
                StringBuilder sb = new StringBuilder();
                sb.Append("Unable to provide all section registrations.  Entity: 'STUDENT.ACAD.CRED' referencing an invalid STC.STUDENT.COURSE.SEC record");
                sb.Append(", ");
                missingIds.ToList().ForEach(id =>
                {
                    var stcRec = studentAcadCredsPage.Where(sc => sc.StcStudentCourseSec == id).FirstOrDefault();
                    sb.Append(string.Format("Entity 'STUDENT.ACAD.CRED', ID: '{0}', Guid '{1}' references invalid data. ", stcRec.Recordkey, stcRec.RecordGuid));
                    sb.Append(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, ID: '", id, "'"));
                    sb.Append(" ");
                });
                var repositoryError = new RepositoryException("Error(s) reading data from STUDENT.ACAD.CRED");
                repositoryError.AddError(new RepositoryError("sectionRegistrations.Id", sb.ToString()));
                throw repositoryError;
            }

            var crsSecIds = studentCourseSecs.Select(i => i.ScsCourseSection);

            var courseSections = await DataReader.BulkReadRecordAsync<CourseSections>(crsSecIds.Distinct().ToArray());

            RepositoryException exception = null;
            foreach (var studentAcadCred in studentAcadCredsPage)
            {
                try
                {
                    SectionRegistrationResponse sectionRegistrationResponse = BuildSectionRegistrationResponse(studentAcadCred, studentCourseSecs, courseSections);
                    sectionRegistrations.Add(sectionRegistrationResponse);
                }
                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException("Unexpected repository error");
                    exception.AddError(new RepositoryError("sectionRegistrations.id", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, ex.Message)));
                }
            }
            if (exception != null && exception.Errors.Any())
            {
                throw exception;
            }

            return sectionRegistrations.Any() ? new Tuple<IEnumerable<SectionRegistrationResponse>, int>(sectionRegistrations, totalCount) :
                new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
        }

        /// <summary>
        /// Section registration with paging and filters with performance changes.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="sectionId"></param>
        /// <param name="personId"></param>
        /// <param name="acadPeriod"></param>
        /// <param name="sectionInstructor"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrations3Async(int offset,
            int limit, SectionRegistrationResponse sectReg, string acadPeriod, string sectionInstructor, 
            Tuple<string, List<string>> registrationStatusesByAcademicPeriod = null)
        {
            var sectionRegistrations = new List<SectionRegistrationResponse>();
            var totalCount = 0;
            string[] limitingKeys = null;
            var criteria = string.Empty;

            try
            {

                string sectionRegistrationsKeys = CacheSupport.BuildCacheKey(AllSectionRegistrationsCache,
                    sectReg != null && !string.IsNullOrWhiteSpace(sectReg.StudentId) ? sectReg.StudentId : string.Empty,
                    !string.IsNullOrWhiteSpace(acadPeriod) ? acadPeriod : string.Empty,
                    sectReg != null && !string.IsNullOrWhiteSpace(sectReg.SectionId) ? sectReg.SectionId : string.Empty,
                    !string.IsNullOrWhiteSpace(sectionInstructor) ? sectionInstructor : string.Empty);

                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    sectionRegistrationsKeys,
                    "STUDENT.ACAD.CRED",
                    offset,
                    limit,
                    AllSectionRegistrationsCacheTimeout,
                    async () =>
                    {
                        var studentAcadCredCriteria = string.Empty;
                        // registrant filter
                        if (sectReg != null && !string.IsNullOrEmpty(sectReg.StudentId))
                        {
                            criteria = "WITH PST.STUDENT.ACAD.CRED BY.EXP PST.STUDENT.ACAD.CRED SAVING PST.STUDENT.ACAD.CRED";
                            limitingKeys = await DataReader.SelectAsync("PERSON.ST", new string[] { sectReg.StudentId }, criteria);
                            // IF the person is a student, but has never been registered for a section, acadCredIds will come back
                            // empty, which the SelectAsync call will interpret as "no limiting list" which is wrong in this case.
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                        }

                        // academicPeriod named query
                        if ((!string.IsNullOrWhiteSpace(acadPeriod))
                         || (registrationStatusesByAcademicPeriod != null))
                        {
                            if (!string.IsNullOrWhiteSpace(acadPeriod))
                            {
                                studentAcadCredCriteria = string.Format("WITH STC.TERM EQ '{0}'", acadPeriod);
                            }

                        }

                        if (registrationStatusesByAcademicPeriod != null)
                        {

                            if (!string.IsNullOrEmpty(registrationStatusesByAcademicPeriod.Item1))
                            {
                                if (!string.IsNullOrEmpty(studentAcadCredCriteria))
                                {
                                    studentAcadCredCriteria += " AND ";
                                }

                                studentAcadCredCriteria += string.Format("WITH STC.TERM EQ '{0}'", registrationStatusesByAcademicPeriod.Item1);
                            }

                            string studentAcadCredStatus = string.Empty;

                            if ((registrationStatusesByAcademicPeriod.Item2 != null) && (registrationStatusesByAcademicPeriod.Item2.Any()))
                            {        
                                foreach (var statusCode in registrationStatusesByAcademicPeriod.Item2)
                                {
                                    studentAcadCredStatus += string.Format("'{0}'", statusCode);
                                }

                                if (!string.IsNullOrEmpty(studentAcadCredStatus))
                                {
                                    studentAcadCredCriteria = string.Format("{0} AND WITH STC.CURRENT.STATUS EQ {1}", studentAcadCredCriteria, studentAcadCredStatus.Trim());
                                }
                            }
                            
                        }


                        // section filter
                        if (sectReg != null && !string.IsNullOrEmpty(sectReg.SectionId))
                        {
                            if (studentAcadCredCriteria == string.Empty)
                            {
                                studentAcadCredCriteria = string.Format("WITH SCS.COURSE.SECTION EQ '{0}'", sectReg.SectionId);
                            }
                            else
                            {
                                studentAcadCredCriteria = studentAcadCredCriteria + string.Format(" WITH SCS.COURSE.SECTION EQ '{0}'", sectReg.SectionId);
                            }
                            limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, studentAcadCredCriteria);
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                            studentAcadCredCriteria = string.Empty;
                        }

                        // section instructor named query
                        if (!string.IsNullOrWhiteSpace(sectionInstructor))
                        {
                            //get all the ids COURSE.SEC.FACULTY...
                            criteria = string.Format("WITH CSF.FACULTY EQ '{0}' SAVING UNIQUE CSF.COURSE.SECTION", sectionInstructor);
                            var courseSecIds = await DataReader.SelectAsync("COURSE.SEC.FACULTY", criteria);
                            if (courseSecIds == null || !courseSecIds.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            } 
                            string stcKeyList = string.Empty;
                            foreach (var stcKey in courseSecIds)
                            {
                                stcKeyList = stcKeyList + "'" + stcKey + "'";
                            }
                            if (studentAcadCredCriteria == string.Empty)
                            {
                                studentAcadCredCriteria = "WITH SCS.COURSE.SECTION EQ " + stcKeyList;
                            }
                            else
                            {
                                studentAcadCredCriteria = studentAcadCredCriteria + " WITH SCS.COURSE.SECTION EQ " + stcKeyList;
                            }
                            
                        }

                        if (studentAcadCredCriteria == string.Empty)
                        {
                            studentAcadCredCriteria = "WITH STC.STUDENT.COURSE.SEC NE ''";
                        }
                        else
                        {
                            studentAcadCredCriteria = "WITH STC.STUDENT.COURSE.SEC NE '' " + studentAcadCredCriteria;
                        }

                        return new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                            criteria = studentAcadCredCriteria
                        };
                    });

                if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }

                totalCount = keyCacheObject.TotalCount.Value;

                //Array.Sort(limitingKeys);
                var sublist = keyCacheObject.Sublist.ToArray();

                //var studentAcadCredsPage =
                //    await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", sublist);
                var studentAcadCredData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentAcadCred>(sublist.ToArray());

                Collection<StudentAcadCred> studentAcadCredsPage = null;
                List<string> scsKey = null;
                if (!studentAcadCredData.Equals(default(BulkReadOutput<DataContracts.StudentAcadCred>)))
                {

                    if ((studentAcadCredData.InvalidKeys != null && studentAcadCredData.InvalidKeys.Any())
                             || (studentAcadCredData.InvalidRecords != null && studentAcadCredData.InvalidRecords.Any()))
                    {
                        var repositoryException = new RepositoryException();

                        if (studentAcadCredData.InvalidKeys.Any())
                        {
                            repositoryException.AddErrors(studentAcadCredData.InvalidKeys
                                .Select(key => new RepositoryError("Bad.Data",
                                string.Format("Unable to locate the following key '{0}'. Entity: 'STUDENT.ACAD.CRED'.", key.ToString()))));
                        }
                        if (studentAcadCredData.InvalidRecords.Any())
                        {
                            repositoryException.AddErrors(studentAcadCredData.InvalidRecords
                               .Select(r => new RepositoryError("Bad.Data",
                               string.Format("Error: '{0}'. Entity: 'STUDENT.ACAD.CRED'.  RecordKey: '{1}'", r.Value, r.Key))
                               { SourceId = r.Key}));
                        }
                        throw repositoryException;
                    }

                    studentAcadCredsPage = studentAcadCredData.BulkRecordsRead;
                    if (studentAcadCredsPage != null)
                    {
                        scsKey = studentAcadCredsPage.Select(s => s.StcStudentCourseSec).Distinct().ToList();
                    }
                }
                if (scsKey == null || scsKey.Count() == 0)
                {
                    return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
                }

                var studentCourseSecData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<StudentCourseSec>(scsKey.ToArray());

                Collection<CourseSections> courseSections = null;
                Collection<StudentCourseSec> studentCourseSecs = null;

                if (!studentCourseSecData.Equals(default(BulkReadOutput<DataContracts.StudentCourseSec>)))
                {

                   if ((studentCourseSecData.InvalidKeys != null && studentCourseSecData.InvalidKeys.Any())
                            || (studentCourseSecData.InvalidRecords != null && studentCourseSecData.InvalidRecords.Any()))
                    {
                        var repositoryException = new RepositoryException();

                        if (studentCourseSecData.InvalidKeys.Any())
                        {
                            repositoryException.AddErrors(studentCourseSecData.InvalidKeys
                                .Select(key => new RepositoryError("Bad.Data",
                                string.Format("Unable to locate the following key '{0}'. Entity: 'STUDENT.COURSE.SEC'.", key.ToString()))));
                        }
                        if (studentCourseSecData.InvalidRecords.Any())
                        {
                            repositoryException.AddErrors(studentCourseSecData.InvalidRecords
                               .Select(r => new RepositoryError("Bad.Data",
                               string.Format("Error: '{0}'. Entity: 'STUDENT.COURSE.SEC.", r.Value))
                               { SourceId = r.Key }));
                        }
                        throw repositoryException;
                    }

                    studentCourseSecs = studentCourseSecData.BulkRecordsRead;

                    if (studentCourseSecs.Count() != sublist.Count())
                    {
                        var courseSecIds = studentCourseSecs.Select(scs => scs.Recordkey).ToList();
                        var missingIds = scsKey.Except(courseSecIds);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Unable to provide all section registrations.  Entity: 'STUDENT.ACAD.CRED' referencing an invalid STC.STUDENT.COURSE.SEC record");
                        sb.Append(", ");
                        missingIds.ToList().ForEach(id =>
                        {
                            var stcRec = studentAcadCredsPage.Where(sc => sc.StcStudentCourseSec == id).FirstOrDefault();
                            sb.Append(string.Format("Entity 'STUDENT.ACAD.CRED', ID: '{0}', Guid '{1}' references invalid data. ", stcRec.Recordkey, stcRec.RecordGuid));
                            sb.Append(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, ID: '", id, "'"));
                            sb.Append(" ");
                        });
                        var repositoryError = new RepositoryException("Error(s) reading data from STUDENT.ACAD.CRED");
                        repositoryError.AddError(new RepositoryError("Bad.Data", sb.ToString()));
                        throw repositoryError;
                    }

                    var crsSecIds = studentCourseSecs.Select(i => i.ScsCourseSection);
                    courseSections = await DataReader.BulkReadRecordAsync<CourseSections>(crsSecIds.Distinct().ToArray());
                }

                //var personStIds = studentAcadCredsPage.Select(i => i.StcPersonId);
                //var personStColumnValues = await DataReader.BatchReadRecordColumnsAsync("PERSON.ST", personStIds.Distinct().ToArray(), new string[] { "PST.STUDENT.ACAD.CRED" });

                foreach (var studentAcadCred in studentAcadCredsPage)
                {
                    try
                    {
                        // Validate the STUDENT.ACAD.CRED key is properly referenced in the PERSON.ST table
                        //var personStKey = studentAcadCred.StcPersonId;
                        //if (string.IsNullOrEmpty(personStKey))
                        //{
                        //    var message = "The STC.PERSON.ID is missing from the STUDENT.ACAD.CRED record.";
                        //    if (exception == null)
                        //        exception = new RepositoryException("Unexpected repository error");
                        //    exception.AddError(new RepositoryError("Bad.Data", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, message)));
                        //}
                        //else
                        //{
                        //    Dictionary<string, string> personStColumnValue = new Dictionary<string, string>();
                        //    if (personStColumnValues != null)
                        //    {
                        //        personStColumnValue = personStColumnValues[studentAcadCred.StcPersonId];
                        //    }
                        //    if (personStColumnValue == null || personStColumnValue.Keys == null || personStColumnValue.Values == null ||
                        //        !personStColumnValue.Keys.Any() || !personStColumnValue.Values.Any() || personStColumnValue["PST.STUDENT.ACAD.CRED"] == null ||
                        //        !personStColumnValue["PST.STUDENT.ACAD.CRED"].Contains(studentAcadCred.Recordkey))
                        //    {
                        //        var message = "The STUDENT.ACAD.CRED key is missing from PST.STUDENT.ACAD.CRED in the PERSON.ST record.";
                        //        if (exception == null)
                        //            exception = new RepositoryException("Unexpected repository error");
                        //        exception.AddError(new RepositoryError("Bad.Data", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, message)));
                        //    }
                        //}
                        SectionRegistrationResponse sectionRegistrationResponse = BuildSectionRegistrationResponse(studentAcadCred, studentCourseSecs, courseSections);
                        // Override credit/ceu values for v16
                        sectionRegistrationResponse.Ceus = studentAcadCred.StcCeus;
                        sectionRegistrationResponse.Credit = studentAcadCred.StcCred;

                        sectionRegistrations.Add(sectionRegistrationResponse);
                    }
                    catch (Exception ex)
                    {
                        if (exception == null)
                            exception = new RepositoryException("Unexpected repository error");
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, ex.Message)));
                    }
                }
                if (exception != null && exception.Errors.Any())
                {
                    throw exception;
                }

                return sectionRegistrations.Any() ? new Tuple<IEnumerable<SectionRegistrationResponse>, int>(sectionRegistrations, totalCount) :
                    new Tuple<IEnumerable<SectionRegistrationResponse>, int>(new List<SectionRegistrationResponse>(), 0);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message));
                throw exception;
            }
        }

        /// <summary>
        /// Gets section registration response by id for V16.0.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<SectionRegistrationResponse> GetSectionRegistrationByIdAsync(string id)
        {
            // Read original STUDENT.ACAD.CRED to get the actual Status Code for the section.
            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", id);
            if (studentAcadCred == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.ACAD.CRED’, Record ID: '", id, "'"));
            }
            // Read original STUDENT.COURSE.SEC to get the registration mode for the student in this section.
            var scsKey = studentAcadCred.StcStudentCourseSec;
            if (string.IsNullOrEmpty(scsKey))
            {
                // No STC.STUDENT.COURSE.SEC record associated to this STUDENT.ACAD.CRED.  This is not valid.
                throw new KeyNotFoundException(string.Format("Record not found, Entity: STUDENT.COURSE.SEC, STUDENT.ACAD.CRED Record ID: '{0}'", id));
            }
            var studentCourseSec = await DataReader.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", scsKey);
            if (studentCourseSec == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, Record ID: '", scsKey, "'"));
            }
            Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>() { studentCourseSec };
            var courseSections = await DataReader.BulkReadRecordAsync<CourseSections>(new string[] { studentCourseSec.ScsCourseSection });
            try
            {
                SectionRegistrationResponse sectionRegistrationResponse = BuildSectionRegistrationResponse(studentAcadCred, studentCourseSecs, courseSections);
                return sectionRegistrationResponse;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException("Unexpected repository error");
                exception.AddError(new RepositoryError("sectionRegistrations.id", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, ex.Message)));
                throw exception;
            }
        }

        /// <summary>
        /// Gets section registration response by id for V16.0.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<SectionRegistrationResponse> GetSectionRegistrationById2Async(string id)
        {
            // Read original STUDENT.ACAD.CRED to get the actual Status Code for the section.
            StudentAcadCred studentAcadCred = null;
            try
            {
                studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", id);
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(
                    new RepositoryError("Bad Data", string.Format("Error: {0}. Entity: 'STUDENT.ACAD.CRED'.  RecordKey: '{1}'", ex.Message, id))
                    { SourceId = id });
                throw exception;
            }
            if (studentAcadCred == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.ACAD.CRED’, Record ID: '", id, "'"));
            }
            // Read original STUDENT.COURSE.SEC to get the registration mode for the student in this section.
            var scsKey = studentAcadCred.StcStudentCourseSec;
            if (string.IsNullOrEmpty(scsKey))
            {
                // No STC.STUDENT.COURSE.SEC record associated to this STUDENT.ACAD.CRED.  This is not valid.
                throw new KeyNotFoundException(string.Format("Record not found, Entity: STUDENT.COURSE.SEC, STUDENT.ACAD.CRED Record ID: '{0}'", id));
            }
            var studentCourseSec = await DataReader.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", scsKey);
            if (studentCourseSec == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, Record ID: '", scsKey, "'"));
            }
            Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>() { studentCourseSec };
            var courseSections = await DataReader.BulkReadRecordAsync<CourseSections>(new string[] { studentCourseSec.ScsCourseSection });
            try
            {
                SectionRegistrationResponse sectionRegistrationResponse = BuildSectionRegistrationResponse(studentAcadCred, studentCourseSecs, courseSections);
                // Override credit/ceu values for v16
                sectionRegistrationResponse.Ceus = studentAcadCred.StcCeus;
                sectionRegistrationResponse.Credit = studentAcadCred.StcCred;

                return sectionRegistrationResponse;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}", studentAcadCred.Recordkey, studentAcadCred.RecordGuid, ex.Message)));
                throw exception;
            }
        }

        /// <summary>
        /// Builds section registration response entity.
        /// </summary>
        /// <param name="studentAcadCred"></param>
        /// <param name="studentCourseSecs"></param>
        /// <param name="courseSections"></param>
        /// <returns></returns>
        private SectionRegistrationResponse BuildSectionRegistrationResponse(StudentAcadCred studentAcadCred, Collection<StudentCourseSec> studentCourseSecs,
            Collection<CourseSections> courseSections)
        {
            var studentId = studentAcadCred.StcPersonId;
            var studentCourseSec =
                studentCourseSecs.FirstOrDefault(sc => sc.Recordkey == studentAcadCred.StcStudentCourseSec);

            CourseSections courseSec = null;
            if (courseSections != null && courseSections.Any())
            {
                courseSec = courseSections.FirstOrDefault(i => i.Recordkey.Equals(studentCourseSec.ScsCourseSection, StringComparison.OrdinalIgnoreCase));
            }

            var sectId = string.Empty;
            var passAudit = string.Empty;
            if (studentCourseSec != null)
            {
                sectId = studentCourseSec.ScsCourseSection;
                passAudit = studentCourseSec.ScsPassAudit;
            }
            var statusCode = string.Empty;
            DateTime? statusDate = null;
            if (studentAcadCred.StcStatus != null && studentAcadCred.StcStatus.Any())
            {
                statusCode = studentAcadCred.StcStatus.ElementAt(0);
            }

            if (studentAcadCred.StcStatusDate != null && studentAcadCred.StcStatusDate.Any())
            {
                statusDate = studentAcadCred.StcStatusDate.ElementAt(0);
            }


            var gradeScheme = studentAcadCred.StcGradeScheme;

            //Get all grades, midterm, final & verified
            var midTermGrades = GetMidTermGrades(studentCourseSec);

            var verifiedGrade = string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade)
                ? null
                : new VerifiedTermGrade(studentAcadCred.StcVerifiedGrade, studentAcadCred.StcVerifiedGradeDate,
                    studentAcadCred.StcVerifiedGradeChgopr);

            var finalGradeDate = studentAcadCred.StcVerifiedGradeDate != null ? studentAcadCred.StcVerifiedGradeDate : studentAcadCred.StudentAcadCredChgdate;
            var finalGrade = string.IsNullOrEmpty(studentAcadCred.StcFinalGrade) ?
                                    null :
                                    new TermGrade(studentAcadCred.StcFinalGrade, finalGradeDate, studentAcadCred.StudentAcadCredChgopr);

            var sectionRegistrationResponse = new SectionRegistrationResponse(studentAcadCred.RecordGuid,
                studentId, sectId, statusCode, gradeScheme, passAudit, new List<RegistrationMessage>());

            sectionRegistrationResponse.StudentAcadCredKey = studentAcadCred.Recordkey;
            sectionRegistrationResponse.StudentCourseSecKey = studentCourseSec.Recordkey;
            sectionRegistrationResponse.MidTermGrades = midTermGrades;
            sectionRegistrationResponse.FinalTermGrade = finalGrade;
            sectionRegistrationResponse.VerifiedTermGrade = verifiedGrade;
            sectionRegistrationResponse.InvolvementStartOn = studentAcadCred.StcStartDate;
            sectionRegistrationResponse.InvolvementEndOn = studentAcadCred.StcEndDate;
            if (studentCourseSec != null)
            {
                sectionRegistrationResponse.ReportingStatus = studentCourseSec.ScsNeverAttendedFlag;
                sectionRegistrationResponse.ReportingLastDayOdAttendance = studentCourseSec.ScsLastAttendDate;
            }
            sectionRegistrationResponse.GradeExtentionExpDate = studentAcadCred.StcGradeExpireDate;
            sectionRegistrationResponse.TranscriptVerifiedGradeDate = studentAcadCred.StcVerifiedGradeDate;
            sectionRegistrationResponse.TranscriptVerifiedBy = studentAcadCred.StcVerifiedGradeChgopr;
            //V7 changes
            sectionRegistrationResponse.CreditType = studentAcadCred.StcCredType;
            sectionRegistrationResponse.AcademicLevel = studentAcadCred.StcAcadLevel;
            sectionRegistrationResponse.Ceus = studentAcadCred.StcAttCeus.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ?
                studentAcadCred.StcAttCeus : studentAcadCred.StcCeus;
            sectionRegistrationResponse.EarnedCeus = studentAcadCred.StcCmplCeus; ;
            sectionRegistrationResponse.Credit = studentAcadCred.StcAttCred.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ?
                studentAcadCred.StcAttCred : studentAcadCred.StcCred;
            sectionRegistrationResponse.EarnedCredit = studentAcadCred.StcCmplCred;
            sectionRegistrationResponse.GradePoint = studentAcadCred.StcGradePts;
            sectionRegistrationResponse.ReplCode = studentAcadCred.StcReplCode;
            sectionRegistrationResponse.RepeatedAcadCreds = studentAcadCred.StcRepeatedAcadCred;
            sectionRegistrationResponse.AltcumContribCmplCred = studentAcadCred.StcAltcumContribCmplCred;
            sectionRegistrationResponse.AltcumContribGpaCred = studentAcadCred.StcAltcumContribGpaCred;
            //V16.0.0
            if (studentAcadCred.StcStatusesEntityAssociation != null && studentAcadCred.StcStatusesEntityAssociation.Any())
            {
                var statusDateList = new List<Tuple<string, DateTime?>>();
                studentAcadCred.StcStatusesEntityAssociation.ToList().ForEach(i =>
                {
                    if (i.StcStatusDateAssocMember != null && i.StcStatusDateAssocMember.HasValue)
                    {
                        Tuple<string, DateTime?> statusDateTuple = new Tuple<string, DateTime?>(i.StcStatusAssocMember, i.StcStatusDateAssocMember.Value);
                        statusDateList.Add(statusDateTuple);
                    }
                });
                sectionRegistrationResponse.StatusDateTuple = statusDateList;
            }
            sectionRegistrationResponse.StatusDate = statusDate;
            if (courseSec != null && !courseSec.SecTerm.Equals(studentAcadCred.StcTerm, StringComparison.OrdinalIgnoreCase))
            {
                sectionRegistrationResponse.OverrideAcadPeriod = studentAcadCred.StcTerm;
            }
            else
            {
                sectionRegistrationResponse.OverrideAcadPeriod = null;
            }

            if (courseSec != null && !courseSec.SecLocation.Equals(studentCourseSec.ScsLocation, StringComparison.OrdinalIgnoreCase))
            {
                sectionRegistrationResponse.OverrideSite = studentCourseSec.ScsLocation;
            }

            return sectionRegistrationResponse;
        }

        #endregion

        /// <summary>
        /// Gets midterm grades
        /// </summary>
        /// <param name="studentCourseSec"></param>
        /// <returns></returns>
        private static List<MidTermGrade> GetMidTermGrades(StudentCourseSec studentCourseSec)
        {
            var midTermGrades = new List<MidTermGrade>();

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade1))
                midTermGrades.Add(new MidTermGrade(1, studentCourseSec.ScsMidTermGrade1, studentCourseSec.ScsMidGradeDate1, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade2))
                midTermGrades.Add(new MidTermGrade(2, studentCourseSec.ScsMidTermGrade2, studentCourseSec.ScsMidGradeDate2, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade3))
                midTermGrades.Add(new MidTermGrade(3, studentCourseSec.ScsMidTermGrade3, studentCourseSec.ScsMidGradeDate3, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade4))
                midTermGrades.Add(new MidTermGrade(4, studentCourseSec.ScsMidTermGrade4, studentCourseSec.ScsMidGradeDate4, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade5))
                midTermGrades.Add(new MidTermGrade(5, studentCourseSec.ScsMidTermGrade5, studentCourseSec.ScsMidGradeDate5, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade6))
                midTermGrades.Add(new MidTermGrade(6, studentCourseSec.ScsMidTermGrade6, studentCourseSec.ScsMidGradeDate6, studentCourseSec.StudentCourseSecChgopr));

            return midTermGrades;
        }

        /// <summary>
        /// Updates Grades
        /// </summary>
        /// <param name="response"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SectionRegistrationResponse> UpdateGradesAsync(SectionRegistrationResponse response, SectionRegistrationRequest request)
        {
            ImportGradesRequest importGradesRequest = new ImportGradesRequest();
            importGradesRequest.Grades = new List<Transactions.Grades>();
            importGradesRequest.Guid = request.RegGuid;
            importGradesRequest.SectionRegId = request.StudentAcadCredId;

            #region Final Grade

            if (request.FinalTermGrade != null)
            {
                Transactions.Grades finalGrade = new Transactions.Grades
                {
                    GradeKey = request.FinalTermGrade.GradeKey,
                    GradeType = request.FinalTermGrade.GradeTypeCode,
                    Grade = request.FinalTermGrade.Grade,
                    NeverAttend = request.ReportingStatus,
                    LastDayAttendDate = request.ReportingLastDayOfAttendance.HasValue
                        ? request.ReportingLastDayOfAttendance.Value.ToString("yyyy/MM/dd")
                        : string.Empty,
                    GradeExpiry = request.GradeExtentionExpDate.HasValue
                        ? request.GradeExtentionExpDate.Value.ToString("yyyy/MM/dd")
                        : string.Empty,
                    GradeSubmitBy = request.FinalTermGrade.SubmittedBy,
                    GradeSubmitDate = request.FinalTermGrade.SubmittedOn.HasValue
                        ? request.FinalTermGrade.SubmittedOn.Value.ToString("yyyy/MM/dd")
                        : string.Empty,
                    InvStartOn = request.InvolvementStartOn.HasValue
                        ? request.InvolvementStartOn.Value.Date
                        : default(DateTime?),
                    InvEndOn = request.InvolvementEndOn.HasValue
                        ? request.InvolvementEndOn.Value.Date
                        : default(DateTime?),
                    GradeChangeReason = request.FinalTermGrade.GradeChangeReason
                };
                importGradesRequest.Grades.Add(finalGrade);
            }

            #endregion

            #region Verified Grade

            if (request.VerifiedTermGrade != null)
            {
                Transactions.Grades verifiedGrade = new Transactions.Grades
                {
                    GradeKey = request.VerifiedTermGrade.GradeKey,
                    GradeType = request.VerifiedTermGrade.GradeTypeCode,
                    Grade = request.VerifiedTermGrade.Grade,
                    NeverAttend = request.ReportingStatus,
                    LastDayAttendDate = request.ReportingLastDayOfAttendance.HasValue
                        ? request.ReportingLastDayOfAttendance.Value.ToString("yyyy/MM/dd")
                        : string.Empty,
                    GradeExpiry = string.Empty,
                    GradeSubmitBy = request.VerifiedTermGrade.SubmittedBy,
                    GradeSubmitDate = request.VerifiedTermGrade.SubmittedOn.HasValue
                        ? request.VerifiedTermGrade.SubmittedOn.Value.ToString("yyyy/MM/dd")
                        : string.Empty,
                    InvStartOn = request.InvolvementStartOn.HasValue
                        ? request.InvolvementStartOn.Value.Date
                        : default(DateTime?),
                    InvEndOn = request.InvolvementEndOn.HasValue
                        ? request.InvolvementEndOn.Value.Date
                        : default(DateTime?),
                    GradeChangeReason = request.VerifiedTermGrade.GradeChangeReason
                };
                //HED-3219
                importGradesRequest.Grades.Add(verifiedGrade);
            }

            #endregion

            #region Mid term grades
            if (request.MidTermGrades != null && request.MidTermGrades.Any())
            {
                foreach (var grade in request.MidTermGrades)
                {
                    Transactions.Grades midTermGrade = new Transactions.Grades
                    {
                        GradeKey = grade.GradeKey,
                        GradeType = grade.GradeTypeCode,
                        //GradeType = grade.Position.ToString(),
                        Grade = grade.Grade,
                        NeverAttend = request.ReportingStatus,
                        LastDayAttendDate = request.ReportingLastDayOfAttendance.HasValue
                            ? request.ReportingLastDayOfAttendance.Value.ToString("yyyy/MM/dd")
                            : string.Empty,
                        GradeExpiry = string.Empty,
                        GradeSubmitBy = grade.SubmittedBy,
                        GradeSubmitDate = grade.GradeTimestamp.HasValue
                            ? grade.GradeTimestamp.Value.ToString("yyyy/MM/dd")
                            : string.Empty,
                        InvStartOn = request.InvolvementStartOn.HasValue
                            ? request.InvolvementStartOn.Value.Date
                            : default(DateTime?),
                        InvEndOn = request.InvolvementEndOn.HasValue
                            ? request.InvolvementEndOn.Value.Date
                            : default(DateTime?),
                        GradeChangeReason = grade.GradeChangeReason
                    };
                    //HED-3219
                    importGradesRequest.Grades.Add(midTermGrade);
                }
            }
            #endregion

            #region LDA update only

            if (importGradesRequest.Grades.Count == 0)
                // Check for LDA update without grades           
                if (!string.IsNullOrEmpty(request.ReportingStatus) || request.ReportingLastDayOfAttendance.HasValue)
                {
                    Transactions.Grades dummyGrade = new Transactions.Grades
                    {

                        NeverAttend = request.ReportingStatus,
                        LastDayAttendDate = request.ReportingLastDayOfAttendance.HasValue
                            ? request.ReportingLastDayOfAttendance.Value.ToString("yyyy/MM/dd")
                            : string.Empty
                    };
                    importGradesRequest.Grades.Add(dummyGrade);
                    if (!string.IsNullOrEmpty(request.ReportingStatus)) response.ReportingStatus = request.ReportingStatus;
                    if (request.ReportingLastDayOfAttendance.HasValue) response.ReportingLastDayOdAttendance = request.ReportingLastDayOfAttendance;
                }

            #endregion

            ImportGradesResponse importGradesResponse = await transactionInvoker.ExecuteAsync<ImportGradesRequest, ImportGradesResponse>(importGradesRequest);

            // If there is any error message - throw an exception

            if (importGradesResponse.GradeMessages != null && importGradesResponse.GradeMessages.Any(m => m.StatusCode.Equals("FAILURE", StringComparison.OrdinalIgnoreCase)))
            {
                var errorMessage = string.Empty;
                var failureMessage = string.Empty;
                foreach (var message in importGradesResponse.GradeMessages)
                {
                    errorMessage = string.Format("Error occurred updating grade for Student: '{0}' and Section: '{1}': ", request.StudentId, request.Section.SectionId);
                    errorMessage += string.Join(Environment.NewLine, message.ErrorMessge);
                    logger.Error(errorMessage);

                    //collect all the failure messages
                    if (message != null && message.StatusCode != null && message.StatusCode.Equals("FAILURE", StringComparison.OrdinalIgnoreCase))
                    {
                        failureMessage += string.Concat(message.ErrorMessge, Environment.NewLine);
                    }
                }
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new InvalidOperationException(failureMessage);
                }
            }

            // Process the messages returned by colleague registration
            if (response.Messages == null)
                response.Messages = new List<RegistrationMessage>();
            if (importGradesResponse.GradeMessages != null && importGradesResponse.GradeMessages.Any())
            {
                foreach (var message in importGradesResponse.GradeMessages)
                {
                    response.Messages.Add(new RegistrationMessage() { Message = message.InfoMessage, SectionId = message.StatusCode });
                }
            }



            foreach (var grade in importGradesRequest.Grades)
            {
                string gradeType = string.IsNullOrEmpty(grade.GradeType) ? string.Empty : grade.GradeType.Substring(0, 1);
                DateTimeOffset? submittedOn = null;
                if (!string.IsNullOrEmpty(grade.GradeSubmitDate))
                {
                    submittedOn = DateTimeOffset.Parse(grade.GradeSubmitDate);
                }

                #region Final Grade
                if (gradeType.Equals("F", StringComparison.OrdinalIgnoreCase))
                {
                    var finalGrade = new TermGrade(grade.GradeKey, submittedOn, grade.GradeSubmitBy, "FINAL")
                    {
                        Grade = grade.Grade,
                        GradeChangeReason = grade.GradeChangeReason
                    };
                    response.FinalTermGrade = finalGrade;
                }
                #endregion

                #region Verified Grade
                else if (gradeType.Equals("V", StringComparison.OrdinalIgnoreCase))
                {
                    var verifiedGrade = new VerifiedTermGrade(grade.GradeKey, submittedOn, grade.GradeSubmitBy, "VERIFIED")
                    {
                        Grade = grade.Grade,
                        GradeChangeReason = grade.GradeChangeReason
                    };
                    response.VerifiedTermGrade = verifiedGrade;
                }

                #endregion

                #region Midterm Grade
                else if (gradeType.Equals("M", StringComparison.OrdinalIgnoreCase))
                {
                    if (grade.GradeType.Length >= 4)
                    {
                        int position;
                        var parsed = int.TryParse(grade.GradeType.Substring(3), out position);
                        if (!parsed)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", grade.GradeType));

                        if (position <= 0 || position > 6)
                            throw new FormatException(string.Format("The midterm value : {0} is invalid", grade.GradeType));

                        var midTermGrade = new MidTermGrade(position, grade.GradeKey, submittedOn, grade.GradeSubmitBy)
                        {
                            Grade = grade.Grade,
                            GradeChangeReason = grade.GradeChangeReason,
                            GradeTypeCode = grade.GradeType
                        };
                        if (response.MidTermGrades == null)
                            response.MidTermGrades = new List<MidTermGrade>();

                        response.MidTermGrades.Add(midTermGrade);
                    }
                    else
                    {
                        throw new FormatException(string.Format("The midterm value : {0} is invalid", grade.GradeType));
                    }
                }
                #endregion            
            }
            //Get the involvement dates from one of the grades since these dates are same across all the grades coming back in response
            if (importGradesResponse.Grades != null && importGradesResponse.Grades.Any())
            {
                var grade = importGradesResponse.Grades.FirstOrDefault();
                response.InvolvementStartOn = grade.InvStartOn;
                response.InvolvementEndOn = grade.InvEndOn;
            }
            return response;
        }

        /// <summary>
        /// Update or Create a student into a section using HeDM
        /// </summary>
        /// <param name="request">Registration Request transaction</param>
        /// <returns>Registration Response <see cref="RegistrationResponse"> object</returns>
        public async Task<SectionRegistrationResponse> UpdateAsync(SectionRegistrationRequest request, string guid, string studentId, string sectionId, string statusCode)
        {
            UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
            updateRequest.RegSections = new List<RegSections>();

            updateRequest.StudentId = request.StudentId;
            updateRequest.CreateStudentFlag = request.CreateStudentFlag;

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                updateRequest.SecRegGuid = string.Empty;
            }
            else
            {
                updateRequest.SecRegGuid = guid;
            }

            updateRequest.RegSections.Add(new RegSections() { SectionIds = request.Section.SectionId, SectionAction = request.Section.Action.ToString(), SectionCredits = request.Section.Credits, SectionDate = request.Section.RegistrationDate });

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // Submit the registration
            UpdateSectionRegistrationResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest);

            // If there is any error message - throw an exception 
            if (updateResponse.ErrorOccurred)
            {
                var errorMessage = string.Format("Error(s) occurred updating section-registrations for Student: '{0}' and Section: '{1}': ", request.StudentId, request.Section.SectionId);
                errorMessage += string.Join(Environment.NewLine, updateResponse.ErrorMessage);
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException(updateResponse.ErrorMessage);
            }

            // Process the messages returned by colleague registration 
            var outputMessages = new List<RegistrationMessage>();
            if (updateResponse.RegMessages.Count > 0)
            {
                foreach (var message in updateResponse.RegMessages)
                {
                    outputMessages.Add(new RegistrationMessage() { Message = message.Message, SectionId = message.MessageSection });
                }
            }

            var stcKey = string.Empty;
            try
            {
                stcKey = await GetSectionRegistrationIdFromGuidAsync(updateResponse.SecRegGuid);
                guid = updateResponse.SecRegGuid;
            }
            catch (KeyNotFoundException)
            {
                if (updateResponse.RegMessages.Any())
                {
                    var sb = new StringBuilder();
                    updateResponse.RegMessages.ForEach(m =>
                    {
                        sb.Append(m.Message);
                        sb.Append("     ");

                    });

                    throw new KeyNotFoundException(string.Concat("Registration failed with the following Registration Messages : ", sb.ToString()));
                }
                else
                {
                    throw new KeyNotFoundException(string.Concat("Registration failed for ", guid.ToString()));
                }
            }

            var academicCreditIds = new List<string>() { stcKey };

            // Read the updated STUDENT.ACAD.CRED to get the actual Status Code for the section.
            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", stcKey);
            if (studentAcadCred != null)
            {

                var scsKey = studentAcadCred.StcStudentCourseSec;
                statusCode = studentAcadCred.StcStatus.ElementAt(0);

                // Read the updated STUDENT.COURSE.SEC to get the section and registration mode for the student in this section.
                var studentCourseSec = await DataReader.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", scsKey);
                if (studentCourseSec == null)
                {
                    throw new ArgumentOutOfRangeException("studentCourseSecIds", "Failed to retrieve all credits from the database.");
                }

                studentId = studentAcadCred.StcPersonId;
                sectionId = studentCourseSec.ScsCourseSection;
                statusCode = studentAcadCred.StcStatus.ElementAt(0);
                var gradeScheme = studentAcadCred.StcGradeScheme;
                var passAudit = studentCourseSec.ScsPassAudit;


                var sectionRegistrationResponse = new SectionRegistrationResponse(guid, studentId, sectionId, statusCode,
                    gradeScheme, passAudit, outputMessages)
                {
                    ErrorOccured = updateResponse.ErrorOccurred,
                    InvolvementStartOn = studentAcadCred.StcStartDate,
                    InvolvementEndOn = studentAcadCred.StcEndDate,
                    ReportingStatus = studentCourseSec.ScsNeverAttendedFlag,
                    ReportingLastDayOdAttendance = studentCourseSec.ScsLastAttendDate,
                    GradeExtentionExpDate = studentAcadCred.StcGradeExpireDate,
                    TranscriptVerifiedGradeDate = studentAcadCred.StcVerifiedGradeDate,
                    TranscriptVerifiedBy = studentAcadCred.StcVerifiedGradeChgopr
                };

                return sectionRegistrationResponse;
            }
            else
            {
                return new SectionRegistrationResponse(guid, studentId, sectionId, statusCode, outputMessages);
            }
        }

        /// <summary>
        /// Update or Create a student into a section using HeDM
        /// </summary>
        /// <param name="request">Registration Request transaction</param>
        /// <returns>Registration Response <see cref="RegistrationResponse"> object</returns>
        public async Task<SectionRegistrationResponse> Update2Async(SectionRegistrationRequest request, string guid, string studentId, string sectionId, string statusCode)
        {
            UpdateSectionRegistrationRequest updateRequest = new UpdateSectionRegistrationRequest();
            updateRequest.RegSections = new List<RegSections>();

            updateRequest.StudentId = request.StudentId;
            updateRequest.CreateStudentFlag = request.CreateStudentFlag;

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(guid))
            {
                updateRequest.SecRegGuid = string.Empty;
            }
            else
            {
                updateRequest.SecRegGuid = guid;
            }
            if (string.IsNullOrEmpty(request.StudentAcadCredId))
            {
                await CheckForExistingRegistration(studentId, sectionId);
            }
            var requestedSection = new RegSections()
            {
                SectionIds = request.Section.SectionId,
                SectionAction = request.Section.Action.ToString(),
                SectionCredits = request.Section.Credits,
                SectionCeus = request.Section.Ceus,
                SectionDate = request.Section.RegistrationDate,
                SectionAcadLevel = request.Section.AcademicLevelCode
            };
            if (request.InvolvementStartOn != null && request.InvolvementStartOn.HasValue)
            {
                requestedSection.InvolvementStartOn = request.InvolvementStartOn.Value.Date;
            }
            if (request.InvolvementEndOn != null && request.InvolvementEndOn.HasValue)
            {
                requestedSection.InvolvementEndOn = request.InvolvementEndOn.Value.Date;
            }

            updateRequest.RegSections.Add(requestedSection);

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // Submit the registration
            UpdateSectionRegistrationResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateSectionRegistrationRequest, UpdateSectionRegistrationResponse>(updateRequest);

            // If there is any error message - throw an exception 
            if (updateResponse.ErrorOccurred)
            {
                var errorMessage = string.Format("Error(s) occurred updating section-registrations for Student: '{0}' and Section: '{1}': ", request.StudentId, request.Section.SectionId);
                var exception = new RepositoryException(errorMessage);

                exception.AddError(new RepositoryError("sectionRegistrations", updateResponse.ErrorMessage)
                {
                    SourceId = request.StudentAcadCredId,
                    Id = updateRequest.SecRegGuid
                });

                errorMessage += string.Join(Environment.NewLine, updateResponse.ErrorMessage);
                logger.Error(errorMessage.ToString());
                // throw new InvalidOperationException(updateResponse.ErrorMessage);
                throw exception;
            }

            // Process the messages returned by colleague registration 
            var outputMessages = new List<RegistrationMessage>();
            if (updateResponse.RegMessages.Count > 0)
            {
                foreach (var message in updateResponse.RegMessages)
                {
                    outputMessages.Add(new RegistrationMessage() { Message = message.Message, SectionId = message.MessageSection });
                }
            }

            var stcKey = string.Empty;
            try
            {
                stcKey = await GetSectionRegistrationIdFromGuidAsync(updateResponse.SecRegGuid);
                guid = updateResponse.SecRegGuid;
            }
            catch (KeyNotFoundException)
            {
                if (updateResponse.RegMessages.Any())
                {
                    var sb = new StringBuilder();
                    updateResponse.RegMessages.ForEach(m =>
                    {
                        sb.Append(m.Message);
                        sb.Append("     ");

                    });

                    throw new KeyNotFoundException(string.Concat("Registration failed with the following Registration Messages : ", sb.ToString()));
                }
                else
                {
                    throw new KeyNotFoundException(string.Concat("Registration failed for ", guid.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(stcKey))
            {
                // Read the updated STUDENT.ACAD.CRED to get the actual Status Code for the section.
                var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", stcKey);
                if (studentAcadCred != null)
                    return await GetSectionRegistrationByIdAsync(stcKey);
            }
            return new SectionRegistrationResponse(guid, studentId, sectionId, statusCode, outputMessages)
            {
                InvolvementStartOn = request.InvolvementStartOn,
                InvolvementEndOn = request.InvolvementEndOn,
                AcademicLevel = request.Section.AcademicLevelCode,
                Credit = request.Section.Credits,
                Ceus = request.Section.Ceus,
                StatusDate = request.Section.RegistrationDate
            };
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetSectionRegistrationIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("SectionRegistration GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("SectionRegistration GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.ACAD.CRED" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new RepositoryException("GUID '" + guid + "' is not valid for section-registrations.");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the GUID for a grade using its ID
        /// </summary>
        /// <param name="id">Grade ID</param>
        /// <returns>Grade GUID</returns>
        public async Task<string> GetGradeGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("GRADES", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Grade.Guid.NotFound", "guid not found for grade " + id));
                throw ex;
            }
        }

        private async Task CheckForExistingRegistration(string studentId, string sectionId)
        {
            var personST = await DataReader.ReadRecordAsync<Base.DataContracts.PersonSt>("PERSON.ST", studentId);
            if (personST != null)
            {
                var scsKeys = personST.PstStudentCourseSec.ToArray();
                var studentCourseSec = await DataReader.BulkReadRecordAsync<StudentCourseSec>(scsKeys);
                if (studentCourseSec != null && studentCourseSec.Any())
                {
                    var existing = studentCourseSec.Where(sc => sc.ScsCourseSection == sectionId).FirstOrDefault();
                    if (existing != null)
                    {
                        var exception = new RepositoryException();
                        var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", existing.ScsStudentAcadCred);
                        if (studentAcadCred != null && !string.IsNullOrEmpty(studentAcadCred.RecordGuid) && !string.IsNullOrEmpty(studentAcadCred.Recordkey))
                        {
                            exception.AddError(new RepositoryError(studentAcadCred.RecordGuid, existing.ScsStudentAcadCred, "sectionRegistration",
                                string.Format("Student '{0}' already has registration activity related to section '{1}' associated with guid '{2}'.",
                                studentId, studentAcadCred.StcCourseName, studentAcadCred.RecordGuid)));
                        }
                        else
                        {
                            exception.AddError(new RepositoryError("sectionRegistration",
                                string.Format("Student '{0}' already has registration activity related to section '{1}'.",
                                studentId, sectionId)));
                        }
                        throw exception;
                    }
                }
            }
        }

        /// <summary>
        /// Get the GUID for a grade using its ID
        /// </summary>
        /// <param name="id">Grade ID</param>
        /// <returns>Grade GUID</returns>
        public async Task<bool> CheckStuAcadCredRecord(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
                else
                {
                    var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", id);
                    if (studentAcadCred == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (ArgumentNullException)
            {
                return false;
            }

        }

        #region Section Registrations Grade Options

        /// <summary>
        /// Gets section registration grade options.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>> GetSectionRegistrationGradeOptionsAsync(int offset, int limit, StudentAcadCredCourseSecInfo request)
        {
            List<StudentAcadCredCourseSecInfo> entities = new List<StudentAcadCredCourseSecInfo>();
            int totalCount = 0;
            string[] limitingKeys = null;
            string criteria = string.Empty;

            if (request != null && !string.IsNullOrEmpty(request.SectionId))
            {
                criteria = string.Format("WITH SCS.COURSE.SECTION EQ '{0}' WITH SCS.STUDENT.ACAD.CRED SAVING UNIQUE SCS.STUDENT.ACAD.CRED",
                        request.SectionId);
                limitingKeys = await DataReader.SelectAsync("STUDENT.COURSE.SEC", criteria);
                // make sure any selected STUDENT.ACAD.CRED keys selected above actually exist in STUDENT.ACAD.CRED
                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", limitingKeys, null);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(new List<StudentAcadCredCourseSecInfo>(), 0);
                }
            }

            //This is for GET ALL
            if (string.IsNullOrEmpty(criteria))
            {
                //May be here get all the id's from PERSON.ST?
                limitingKeys = await DataReader.SelectAsync("STUDENT.ACAD.CRED", "WITH STC.STUDENT.COURSE.SEC NE ''");
            }

            if (limitingKeys == null || !limitingKeys.Any())
            {
                return new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(new List<StudentAcadCredCourseSecInfo>(), 0);
            }

            totalCount = limitingKeys.Count();

            Array.Sort(limitingKeys);
            var sublist = limitingKeys.Skip(offset).Take(limit).ToArray();

            var studentAcadCredsPage =
                await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", sublist);

            var scsKey = studentAcadCredsPage.Select(s => s.StcStudentCourseSec).Distinct().ToList();
            var studentCourseSecs = await DataReader.BulkReadRecordAsync<StudentCourseSec>(scsKey.ToArray());

            if (studentCourseSecs.Count() != sublist.Count())
            {
                var courseSecIds = studentCourseSecs.Select(scs => scs.Recordkey).ToList();
                var missingIds = scsKey.Except(courseSecIds);
                var repositoryException = new RepositoryException();
                missingIds.ToList().ForEach(id =>
                {
                    var stcRec = studentAcadCredsPage.Where(sc => sc.StcStudentCourseSec == id).FirstOrDefault();
                    var repoError = new RepositoryError("sectionRegistration")
                    {
                        Id = stcRec.RecordGuid,
                        SourceId = string.IsNullOrEmpty(stcRec.Recordkey) ? stcRec.Recordkey : string.Empty,
                        Message = string.Format("Record not found, Entity 'STUDENT.COURSE.SEC', ID: '{0}', Guid '{1}' references invalid data. ",
                                                stcRec.Recordkey, stcRec.RecordGuid)
                    };
                    repositoryException.AddError(repoError);
                });
                throw repositoryException;
            }

            RepositoryException repException = null;
            foreach (var studentAcadCred in studentAcadCredsPage)
            {
                try
                {
                    StudentAcadCredCourseSecInfo entity = BuildSectionRegistrationGradeOptionsResponse(studentAcadCred, studentCourseSecs);
                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    if (repException == null)
                    {
                        repException = new RepositoryException();
                    }

                    var repoError = new RepositoryError("Invalid.StudentAcadCred", ex.Message);
                    repException.AddError(repoError);
                }
            }
            if (repException != null && repException.Errors.Any())
            {
                throw repException;
            }

            return entities.Any() ? new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(entities, totalCount) :
                new Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>(new List<StudentAcadCredCourseSecInfo>(), 0);
        }

        /// <summary>
        /// Get section registrations grade option by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StudentAcadCredCourseSecInfo> GetSectionRegistrationGradeOptionsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required.");
            }

            // Read original STUDENT.ACAD.CRED to get the actual Status Code for the section.
            var studentAcadCred = await DataReader.ReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", id);
            if (studentAcadCred == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.ACAD.CRED’, Record ID: '", id, "'"));
            }
            // Read original STUDENT.COURSE.SEC to get the registration mode for the student in this section.
            var scsKey = studentAcadCred.StcStudentCourseSec;
            if (string.IsNullOrEmpty(scsKey))
            {
                // No STC.STUDENT.COURSE.SEC record associated to this STUDENT.ACAD.CRED.  This is not valid.
                throw new KeyNotFoundException(string.Format("Record not found, Entity: STUDENT.COURSE.SEC, STUDENT.ACAD.CRED Record ID: '{0}'", id));
            }
            var studentCourseSec = await DataReader.ReadRecordAsync<StudentCourseSec>("STUDENT.COURSE.SEC", scsKey);
            if (studentCourseSec == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, Entity: ‘STUDENT.COURSE.SEC’, Record ID: '", scsKey, "'"));
            }
            try
            {
                Collection<StudentCourseSec> studentCourseSecs = new Collection<StudentCourseSec>() { studentCourseSec };
                StudentAcadCredCourseSecInfo sectionRegistrationResponse = BuildSectionRegistrationGradeOptionsResponse(studentAcadCred, studentCourseSecs);
                return sectionRegistrationResponse;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException("Unexpected repository error");
                exception.AddError(new RepositoryError(studentAcadCred.RecordGuid, studentAcadCred.Recordkey, string.Format("Error(s) processing STUDENT.ACAD.CRED record '{0}' with guid '{1}'.  Error: {2}",
                    studentAcadCred.Recordkey, studentAcadCred.RecordGuid, ex.Message)));
                throw exception;
            }
        }

        private StudentAcadCredCourseSecInfo BuildSectionRegistrationGradeOptionsResponse(StudentAcadCred studentAcadCred, Collection<StudentCourseSec> studentCourseSecs)
        {
            var studentCourseSec = studentCourseSecs.FirstOrDefault(sc => sc.Recordkey == studentAcadCred.StcStudentCourseSec);
            var sectionId = string.Empty;
            if (studentCourseSec != null)
            {
                sectionId = studentCourseSec.ScsCourseSection;
            }

            StudentAcadCredCourseSecInfo response = new StudentAcadCredCourseSecInfo(studentAcadCred.RecordGuid, studentAcadCred.Recordkey, sectionId);
            response.GradeScheme = studentAcadCred.StcGradeScheme;
            response.Term = studentAcadCred.StcTerm;
            response.StartDate = studentAcadCred.StcStartDate;
            response.EndDate = studentAcadCred.StcEndDate;
            response.VerifiedGrade = studentAcadCred.StcVerifiedGrade;
            response.FinalGrade = studentAcadCred.StcFinalGrade;
            return response;
        }

        #endregion
    }
}