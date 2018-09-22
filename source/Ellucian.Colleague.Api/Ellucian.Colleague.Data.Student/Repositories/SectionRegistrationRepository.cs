// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SectionRegistrationRepository : BaseColleagueRepository, ISectionRegistrationRepository
    {
        public SectionRegistrationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
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

            return new RegistrationResponse(outputMessages, updateResponse.IpcRegId);
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

            var sectionRegistrationResponse = new SectionRegistrationResponse(guid, studentId, sectionId, statusCode, gradeScheme, passAudit, new List<RegistrationMessage>());
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
            sectionRegistrationResponse.EarnedCeus = studentAcadCred.StcCmplCeus; ;
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
            var sectionRegistrations = new List<SectionRegistrationResponse>();
            var totalCount = 0;
            var criteria = string.Empty;

            if (!string.IsNullOrEmpty(sectionId))
            {
                criteria += "WITH SCS.COURSE.SECTION EQ '" + sectionId + "'";
            }
            if (!string.IsNullOrEmpty(personId))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH STC.PERSON.ID EQ '" + personId + "'";
            }

            if (!string.IsNullOrEmpty(sectionId) || !string.IsNullOrEmpty(personId))
            {
                criteria += " AND WITH STC.STUDENT.COURSE.SEC NE ''";
            }
            else
            {
                criteria = "WITH STC.STUDENT.COURSE.SEC NE ''";
            }

            var studentAcadCreds = await DataReader.SelectAsync("STUDENT.ACAD.CRED", criteria);
            totalCount = studentAcadCreds.Count();

            if (totalCount == 0)
                return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(sectionRegistrations, totalCount);

            Array.Sort(studentAcadCreds);
            var sublist = studentAcadCreds.Skip(offset).Take(limit).ToArray();

            var studentAcadCredsPage =
                await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED", sublist);

            var scsKey = studentAcadCredsPage.Select(s => s.StcStudentCourseSec).Distinct();
            var studentCourseSecs = await DataReader.BulkReadRecordAsync<StudentCourseSec>(scsKey.ToArray());

            if (studentCourseSecs.Count() != sublist.Count())
            {
                throw new ArgumentOutOfRangeException("academicCreditIds",
                    "Failed to retrieve all credits from the database.");
            }

            foreach (var studentAcadCred in studentAcadCredsPage)
            {
                var studentId = studentAcadCred.StcPersonId;
                var studentCourseSec =
                    studentCourseSecs.FirstOrDefault(sc => sc.Recordkey == studentAcadCred.StcStudentCourseSec);
                var sectId = string.Empty;
                var passAudit = string.Empty;
                if (studentCourseSec != null)
                {
                    sectId = studentCourseSec.ScsCourseSection;
                    passAudit = studentCourseSec.ScsPassAudit;
                }
                var statusCode = string.Empty;
                if (studentAcadCred.StcStatus != null && studentAcadCred.StcStatus.Any())
                    statusCode = studentAcadCred.StcStatus.ElementAt(0);
                var gradeScheme = studentAcadCred.StcGradeScheme;

                //Get all grades, midterm, final & verified
                var midTermGrades = GetMidTermGrades(studentCourseSec);

                var verifiedGrade = string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade)
                    ? null
                    : new VerifiedTermGrade(studentAcadCred.StcVerifiedGrade, studentAcadCred.StcVerifiedGradeDate,
                        studentAcadCred.StcVerifiedGradeChgopr);

                var finalGradeDate = studentAcadCred.StcVerifiedGradeDate ?? studentAcadCred.StudentAcadCredChgdate;

                var finalGrade = string.IsNullOrEmpty(studentAcadCred.StcFinalGrade)
                    ? null
                    : new TermGrade(studentAcadCred.StcFinalGrade, finalGradeDate,
                        studentAcadCred.StudentAcadCredChgopr);

                var sectionRegistrationResponse = new SectionRegistrationResponse(studentAcadCred.RecordGuid,
                    studentId, sectId, statusCode, gradeScheme, passAudit, new List<RegistrationMessage>());
                sectionRegistrationResponse.MidTermGrades = midTermGrades;
                sectionRegistrationResponse.FinalTermGrade = finalGrade;
                sectionRegistrationResponse.VerifiedTermGrade = verifiedGrade;
                sectionRegistrationResponse.InvolvementStartOn = studentAcadCred.StcStartDate;
                sectionRegistrationResponse.InvolvementEndOn = studentAcadCred.StcEndDate;
                if (studentCourseSec != null)
                {
                    sectionRegistrationResponse.ReportingStatus = studentCourseSec.ScsNeverAttendedFlag;
                    sectionRegistrationResponse.ReportingLastDayOdAttendance =
                        studentCourseSec.ScsLastAttendDate;
                }
                sectionRegistrationResponse.GradeExtentionExpDate = studentAcadCred.StcGradeExpireDate;
                sectionRegistrationResponse.TranscriptVerifiedGradeDate = studentAcadCred.StcVerifiedGradeDate;
                sectionRegistrationResponse.TranscriptVerifiedBy = studentAcadCred.StcVerifiedGradeChgopr;
                //V7 changes
                sectionRegistrationResponse.CreditType = studentAcadCred.StcCredType;
                sectionRegistrationResponse.AcademicLevel = studentAcadCred.StcAcadLevel;
                sectionRegistrationResponse.Ceus = studentAcadCred.StcAttCeus.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCeus : studentAcadCred.StcCeus;
                sectionRegistrationResponse.EarnedCeus = studentAcadCred.StcCmplCeus; ;
                sectionRegistrationResponse.Credit = studentAcadCred.StcAttCred.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCred : studentAcadCred.StcCred;
                sectionRegistrationResponse.EarnedCredit = studentAcadCred.StcCmplCred;
                sectionRegistrationResponse.GradePoint = studentAcadCred.StcGradePts;
                sectionRegistrationResponse.ReplCode = studentAcadCred.StcReplCode;
                sectionRegistrationResponse.RepeatedAcadCreds = studentAcadCred.StcRepeatedAcadCred;
                sectionRegistrationResponse.AltcumContribCmplCred = studentAcadCred.StcAltcumContribCmplCred;
                sectionRegistrationResponse.AltcumContribGpaCred = studentAcadCred.StcAltcumContribGpaCred;

                sectionRegistrations.Add(sectionRegistrationResponse);
            }

            return new Tuple<IEnumerable<SectionRegistrationResponse>, int>(sectionRegistrations, totalCount);
        }

        /// <summary>
        /// Gets midterm grades
        /// </summary>
        /// <param name="studentCourseSec"></param>
        /// <returns></returns>
        private static List<MidTermGrade> GetMidTermGrades(StudentCourseSec studentCourseSec)
        {
            var midTermGrades = new List<MidTermGrade>();

            if(!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade1))
                midTermGrades.Add(new MidTermGrade(1, studentCourseSec.ScsMidTermGrade1, studentCourseSec.ScsMidGradeDate1, studentCourseSec.StudentCourseSecChgopr));

            if (!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade2))
                midTermGrades.Add(new MidTermGrade(2, studentCourseSec.ScsMidTermGrade2, studentCourseSec.ScsMidGradeDate2, studentCourseSec.StudentCourseSecChgopr));

            if(!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade3))
                midTermGrades.Add(new MidTermGrade(3, studentCourseSec.ScsMidTermGrade3, studentCourseSec.ScsMidGradeDate3, studentCourseSec.StudentCourseSecChgopr));

            if(!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade4))
                midTermGrades.Add(new MidTermGrade(4, studentCourseSec.ScsMidTermGrade4, studentCourseSec.ScsMidGradeDate4, studentCourseSec.StudentCourseSecChgopr));

            if(!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade5))
                midTermGrades.Add(new MidTermGrade(5, studentCourseSec.ScsMidTermGrade5, studentCourseSec.ScsMidGradeDate5, studentCourseSec.StudentCourseSecChgopr));

            if(!string.IsNullOrEmpty(studentCourseSec.ScsMidTermGrade6))
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
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                updateRequest.SecRegGuid = string.Empty;
            }
            else
            {
                updateRequest.SecRegGuid = guid;
            }

            updateRequest.RegSections.Add(new RegSections() 
            { 
                SectionIds = request.Section.SectionId, 
                SectionAction = request.Section.Action.ToString(), 
                SectionCredits = request.Section.Credits,
                SectionCeus = request.Section.Ceus,
                SectionDate = request.Section.RegistrationDate,
                SectionAcadLevel = request.Section.AcademicLevelCode
            });

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
                //V7 changes
                sectionRegistrationResponse.CreditType = studentAcadCred.StcCredType;
                sectionRegistrationResponse.AcademicLevel = studentAcadCred.StcAcadLevel;
                sectionRegistrationResponse.Ceus = studentAcadCred.StcAttCeus.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCeus : studentAcadCred.StcCeus;
                sectionRegistrationResponse.EarnedCeus = studentAcadCred.StcCmplCeus; ;
                sectionRegistrationResponse.Credit = studentAcadCred.StcAttCred.HasValue || !string.IsNullOrEmpty(studentAcadCred.StcVerifiedGrade) ? studentAcadCred.StcAttCred : studentAcadCred.StcCred;
                sectionRegistrationResponse.EarnedCredit = studentAcadCred.StcCmplCred;
                sectionRegistrationResponse.GradePoint = studentAcadCred.StcGradePts;
                sectionRegistrationResponse.ReplCode = studentAcadCred.StcReplCode;
                sectionRegistrationResponse.RepeatedAcadCreds = studentAcadCred.StcRepeatedAcadCred;
                sectionRegistrationResponse.AltcumContribCmplCred = studentAcadCred.StcAltcumContribCmplCred;
                sectionRegistrationResponse.AltcumContribGpaCred = studentAcadCred.StcAltcumContribGpaCred;

                return sectionRegistrationResponse;
            }
            else
            {
                return new SectionRegistrationResponse(guid, studentId, sectionId, statusCode, outputMessages);
            }
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

            if (foundEntry.Value.Entity != "STUDENT.ACAD.CRED")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, STUDENT.ACAD.CRED");
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
            catch(ArgumentNullException)
            {
                throw;
            }
            catch(RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Grade.Guid.NotFound", "guid not found for grade " + id));
                throw ex;
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

    }
}