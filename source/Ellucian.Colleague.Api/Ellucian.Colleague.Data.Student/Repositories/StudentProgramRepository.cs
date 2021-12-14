// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentProgramRepository : BaseColleagueRepository, IStudentProgramRepository
    {
        private IGradeRepository gradeRepo;
        private ApplValcodes studentProgramStatuses;
        private Data.Base.DataContracts.IntlParams internationalParameters;
        private IStudentReferenceDataRepository studentReferenceRepo;
        private ITermRepository termRepository;
        private IEnumerable<Term> termList;

        public StudentProgramRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 5;
            gradeRepo = new GradeRepository(cacheProvider, transactionFactory, logger);
            studentReferenceRepo = new StudentReferenceDataRepository(cacheProvider, transactionFactory, logger, apiSettings);
            termRepository = new TermRepository(cacheProvider, transactionFactory, logger);
        }

        private async Task<ApplValcodes> GetStudentProgramStatusesAsync()
        {
            if (studentProgramStatuses != null)
            {
                return studentProgramStatuses;
            }

            // Overriding cache timeout to be 240.
            studentProgramStatuses = await GetOrAddToCacheAsync<ApplValcodes>("StudentProgramStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access STUDENT.PROGRAM.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return studentProgramStatuses;
        }

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        new private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new Exception(errorMessage);
                        Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        /// <summary>
        /// Get Student Programs for a student ID
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>StudentProgram Entity</returns>

        public async Task<IEnumerable<StudentProgram>> GetAsync(string studentId)
        {
            // Don't cache the STUDENTS read so we get the latest list of programs in case 
            // it was changed recently.
            Students students = await DataReader.ReadRecordAsync<Students>("STUDENTS", studentId);

            if (students == null)
            {
                var errorMessage = string.Format("Unable to access STUDENTS table, record not found for student {0}", studentId);
                logger.Error(errorMessage);
                return new List<StudentProgram>();
            }

            var stprIds = new List<string>();
            if (students.StuAcadPrograms != null)
            {
                foreach (var acadProgramId in students.StuAcadPrograms)
                {
                    stprIds.Add(studentId + "*" + acadProgramId);
                }
            }

            string cachekey = string.Join(".", stprIds);



            var programsdata = await GetOrAddToCacheAsync<Collection<StudentPrograms>>("StudentPrograms" + cachekey,
                async () =>
                {

                    Collection<StudentPrograms> programData = await DataReader.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", stprIds.ToArray());
                    if (programData == null)
                    {
                        var errorMessage = "Unable to access STUDENT.PROGRAMS table.";
                        logger.Info(errorMessage);
                        //throw new Exception(errorMessage);
                    }
                    if (programData.Count == 1 && programData.FirstOrDefault() == null)
                    {
                        var errorMessage = string.Format("Unable to access STUDENT.PROGRAMS table, record(s) not found for {0}", string.Join(",", stprIds));
                        logger.Error(errorMessage);
                    }
                    return programData;
                });

            var programs = await BuildStudentProgramsAsync(programsdata, new List<string>() { studentId }, true);
            return programs;
        }

        /// <summary>
        /// Wrapper around Async, used by FinancialAid branch for AcademicProgressService
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns>StudentProgram Entity</returns>
        public IEnumerable<StudentProgram> Get(string studentId)
        {
            var x = Task.Run(async () =>
            {
                return await GetAsync(studentId);
            }).GetAwaiter().GetResult();
            return x;
        }

        /// <summary>
        /// Retrieve student programs for a list of students
        /// </summary>
        /// <param name="studentIds">List of student Ids</param>
        /// <param name="includeInactivePrograms">Boolean indicating if inactive programs should be returned</param>
        /// <param name="term">Term </param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentProgram>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, Term term = null, bool includeHistory = false)
        {
            // var stprIds = DataReader.Select("STUDENT.PROGRAMS", "WITH STPR.STUDENT = '?' BY STPR.STUDENT", studentIds.ToArray());
            // Build a list of STUDENT.PROGRAMS keys from the STUDENTS record instead of from Select
            // because the Select times out too often and is working off a computed column which is very slow.
            // SRM - 08/28/2014
            Collection<Students> studentsData = await DataReader.BulkReadRecordAsync<Students>(studentIds.ToArray());
            var stprIds = new List<string>();

            foreach (var students in studentsData)
            {
                if (students != null)
                {
                    if (students.StuAcadPrograms != null)
                    {
                        foreach (var acadProgramId in students.StuAcadPrograms)
                        {
                            stprIds.Add(students.Recordkey + "*" + acadProgramId);
                        }
                    }
                }
            }
            IEnumerable<StudentProgram> programs = new List<StudentProgram>();
            if (stprIds.Count() > 0)
            {
                Collection<StudentPrograms> programsData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.ToArray());
                if (programsData.Count <= 0)
                {
                    var errorMessage = "Unable to access STUDENT.PROGRAMS table.";
                    logger.Info(errorMessage);
                    //throw new Exception(errorMessage);
                }
                if (programsData.Count == 1 && programsData.FirstOrDefault() == null)
                {
                    var errorMessage = string.Format("Unable to access STUDENT.PROGRAMS table, record(s) not found for {0}", string.Join(",", stprIds));
                    logger.Error(errorMessage);
                }
                programs = await BuildStudentProgramsAsync(programsData, studentIds.ToList(), includeInactivePrograms, term, includeHistory);
            }
            return programs;
        }

        /// <summary>
        /// Add new academic Program for a Student
        /// </summary>
        /// <param name="studentAcademicProgram">The Student Academic program information</param>
        /// <param name="activePrograms">The Student active program</param>
        /// <param name="endDates">End date of the active program</param>
        /// <returns>Newly created Program</returns>
        public async Task<StudentProgram> AddStudentProgram(StudentAcademicProgram studentAcademicProgram, List<string> activePrograms, List<string> endDates)
        {
            var addProgramRequest = new TxChangeOfProgramRequest()
            {
                APersonId = studentAcademicProgram.StudentId,
                AAcadProgram = studentAcademicProgram.ProgramCode,
                ACatalog = studentAcademicProgram.CatalogCode,
                AStartDate = studentAcademicProgram.StartDate.HasValue ? studentAcademicProgram.StartDate.Value.ToShortDateString() : "",
                AlEndDates = endDates,
                ADept = studentAcademicProgram.DepartmentCode,
                ALocation = studentAcademicProgram.Location,
                AActivePrograms = activePrograms,
                AAction = "C", 
                ACopyEdPlan = "N",
                ACreateApplFlag = "N"
            };
            try
            {
                var createResponse = await transactionInvoker.ExecuteAsync<TxChangeOfProgramRequest, TxChangeOfProgramResponse>(addProgramRequest);
                if (!string.IsNullOrEmpty(createResponse.AErrorMessage))
                {
                    logger.Info(createResponse.AErrorMessage);
                    throw new Exception(string.Format("Unable to create student academic program. Message: '{0}'", createResponse.AErrorMessage));
                }

                // If add is success, Clear Student Programs Cache....
                Students students = await DataReader.ReadRecordAsync<Students>("STUDENTS", studentAcademicProgram.StudentId);

                if (students != null)
                {
                    var stprIds = new List<string>();
                    if (students.StuAcadPrograms != null)
                    {
                        foreach (var acadProgramId in students.StuAcadPrograms)
                        {
                            stprIds.Add(studentAcademicProgram.StudentId + "*" + acadProgramId);
                        }
                    }
                    
                    string cachekey = string.Join(".", stprIds);
                    string planningStudentCacheKey = "PlanningStudent" + studentAcademicProgram.StudentId;
                    ClearCache(new List<string> { "StudentPrograms" + cachekey, planningStudentCacheKey });
                }

                StudentProgram studentProgram = null;
                var studentPrograms = await GetStudentProgramsByIdsAsync(new List<string>() { studentAcademicProgram.StudentId }, false);
                if (studentPrograms != null || studentPrograms.Any())
                    studentProgram = studentPrograms.FirstOrDefault(x => x.ProgramCode == studentAcademicProgram.ProgramCode);

                return studentProgram;
            }
            catch(Exception ex)
            {
                var errorMessage = string.Format("Unable to add academic program for student: '{0}' and program: '{1}'. Message: '{2}'", studentAcademicProgram.StudentId, studentAcademicProgram.ProgramCode, ex.Message);
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Update academic Program for a Student
        /// </summary>
        /// <param name="studentAcademicProgram">The Student Academic program information</param>
        /// <param name="activePrograms">The Student active program</param>
        /// <param name="endDates">End date of the active program</param>
        /// <returns>Update status</returns>
        public async Task<StudentProgram> UpdateStudentProgram(StudentAcademicProgram studentAcademicProgram, List<string> activePrograms, List<string> endDates)
        {
            var addProgramRequest = new TxChangeOfProgramRequest()
            {
                APersonId = studentAcademicProgram.StudentId,
                AAcadProgram = studentAcademicProgram.ProgramCode,
                ACatalog = studentAcademicProgram.CatalogCode,
                AStartDate = studentAcademicProgram.StartDate.HasValue ? studentAcademicProgram.StartDate.Value.ToShortDateString() : "",
                AlEndDates = endDates,
                ADept = null,
                ALocation = null,
                AActivePrograms = activePrograms,
                AAction = "U",
                ACopyEdPlan = "N",
                ACreateApplFlag = "N"
            };
            try
            {
                var updateResponse = await transactionInvoker.ExecuteAsync<TxChangeOfProgramRequest, TxChangeOfProgramResponse>(addProgramRequest);
                if (!string.IsNullOrEmpty(updateResponse.AErrorMessage))
                {
                    logger.Info(updateResponse.AErrorMessage);
                    throw new Exception(string.Format("Unable to update student academic program. Message: '{0}'", updateResponse.AErrorMessage));
                }

                // If update is success, clear Student Programs Cache....
                Students students = await DataReader.ReadRecordAsync<Students>("STUDENTS", studentAcademicProgram.StudentId);

                if (students != null)
                {
                    var stprIds = new List<string>();
                    if (students.StuAcadPrograms != null)
                    {
                        foreach (var acadProgramId in students.StuAcadPrograms)
                        {
                            stprIds.Add(studentAcademicProgram.StudentId + "*" + acadProgramId);
                        }
                    }
                    string cachekey = string.Join(".", stprIds);
                    string planningStudentCacheKey = "PlanningStudent" + studentAcademicProgram.StudentId;
                    ClearCache(new List<string> { "StudentPrograms" + cachekey, planningStudentCacheKey });
                }

                StudentProgram studentProgram = null;
                var studentPrograms = await GetStudentProgramsByIdsAsync(new List<string>() { studentAcademicProgram.StudentId }, false);
                if (studentPrograms != null || studentPrograms.Any())
                    studentProgram = studentPrograms.FirstOrDefault(x => x.ProgramCode == studentAcademicProgram.ProgramCode);

                return studentProgram;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to update academic program for student: '{0}' and program: '{1}'. Message: '{2}'", studentAcademicProgram.StudentId, studentAcademicProgram.ProgramCode, ex.Message);
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get specific program for the student
        /// </summary>
        /// <param name="studentid">Id of the student</param>
        /// <param name="programid">Program Code</param>
        /// <returns>A StudentProgram object. Returns an exception if not found.</returns>
        public async Task<StudentProgram> GetAsync(string studentid, string programid)
        {
            var allPrograms = await GetAsync(studentid);
            if (allPrograms.Where(ap => ap.ProgramCode == programid).Count() > 0)
            {
                return allPrograms.FirstOrDefault(pr => pr.ProgramCode == programid);
            }
            else
            {
                var errorMessage = string.Format("Unable to locate STUDENT.PROGRAMS record for student: '{0}' and program: '{1}'", studentid, programid);
                logger.Error(errorMessage);
                return null;
            }
        }

        /// <summary>
        /// Builds StudentProgram Entity
        /// </summary>
        /// <param name="programData">Student Programs Data contracts</param>
        /// <param name="includeInactivePrograms">include Inactive program flag</param>
        /// <param name="term">term for the student program</param>
        /// <param name="includeHistory">flag to include history</param>
        /// <returns>Returns StudentProgram</returns>

        private async Task<IEnumerable<StudentProgram>> BuildStudentProgramsAsync(Collection<StudentPrograms> programData, List<string> studentIds, bool includeInactivePrograms = false, Term term = null, bool includeHistory = false)
        {

            List<StudentProgram> programs = new List<StudentProgram>();
            Collection<StudentDaOverrides> overrideData = new Collection<StudentDaOverrides>();
            Collection<StudentDaExcpts> exceptionData = new Collection<StudentDaExcpts>();
            ICollection<string> overrideIds = new List<string>();
            ICollection<string> exceptionIds = new List<string>();
            IDictionary<string, ApplValcodesVals> studentProgramStatusesDict = null;
            // build association of program statuses with its special processing code 
            try
            {
                studentProgramStatusesDict = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.AsEnumerable<ApplValcodesVals>().ToDictionary(a => a.ValInternalCodeAssocMember);
                if (studentProgramStatusesDict == null)
                {
                    throw new Exception("Student Program Statuses is null");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to retrieve student program statuses");
                logger.Error(ex, ex.Message);
            }

            foreach (var prog in programData)
            {
                foreach (var over in prog.StprDaOverrides)
                {
                    if (!string.IsNullOrEmpty(over))
                    {
                        overrideIds.Add(over);
                    }
                }

                foreach (var excp in prog.StprDaExcpts)
                {
                    if (!string.IsNullOrEmpty(excp))
                    {
                        exceptionIds.Add(excp);
                    }
                }
            }

            if (overrideIds.Count() > 0)
            {
                overrideData = await DataReader.BulkReadRecordAsync<StudentDaOverrides>("STUDENT.DA.OVERRIDES", overrideIds.ToArray());
            }

            if (exceptionIds.Count() > 0)
            {
                exceptionData = await DataReader.BulkReadRecordAsync<StudentDaExcpts>("STUDENT.DA.EXCPTS", exceptionIds.ToArray());
            }

            string[] progCodes = programData.Select(p => p.Recordkey).Where(x => !string.IsNullOrEmpty(x) && x.Contains("*")).Select(x => x.Split('*')[1]).Where(y => !string.IsNullOrEmpty(y)).ToArray();
            Collection<AcadPrograms> acadProgramCollection = new Collection<AcadPrograms>();
            Collection<StudentAcadLevels> studentAcadLevelsCollection = new Collection<StudentAcadLevels>();
            if (progCodes != null && progCodes.Count() > 0)
            {
                acadProgramCollection = await DataReader.BulkReadRecordAsync<AcadPrograms>(progCodes);

                var students = await DataReader.BulkReadRecordAsync<Students>(studentIds.ToArray());
                var studentAcadLevelIds = new List<string>();

                if (students != null)
                {
                    foreach (var student in students)
                    {
                        if (student.StuAcadLevels != null)
                        {
                            studentAcadLevelIds.AddRange(student.StuAcadLevels.Select(acadLevelId => student.Recordkey + "*" + acadLevelId));
                        }
                    }
                }

                studentAcadLevelsCollection = await DataReader.BulkReadRecordAsync<StudentAcadLevels>(studentAcadLevelIds.Distinct().ToArray());
            }
            if (acadProgramCollection == null || acadProgramCollection.Count() == 0)
            {
                var errorMessage = "Unable to get data from ACAD.PROGRAMS table for program codes " + progCodes.ToString();
                logger.Info(errorMessage);
                //throw new Exception(errorMessage);
            }
            bool error = false;
            foreach (var prog in programData)
            {
                StudentProgramStatusProcessingType programStatus = StudentProgramStatusProcessingType.None;
                if (studentProgramStatusesDict != null && prog.StprStatus.Count > 0)
                {
                    var isParsed = Enum.TryParse(studentProgramStatusesDict[prog.StprStatus.ElementAt(0)].ValActionCode1AssocMember, out programStatus);
                    if (!isParsed || !Enum.IsDefined(typeof(StudentProgramStatusProcessingType), programStatus))
                    {
                        programStatus = StudentProgramStatusProcessingType.None;
                    }
                }
                try
                {

                    // If the program has an end date (first position) before today, skip it
                    if (prog.StprEndDate != null && prog.StprEndDate.Count > 0 && prog.StprEndDate.ElementAt(0) < DateTime.Today)
                    {
                        // If we are including inactive programs, then don't exclude
                        if (includeInactivePrograms == false && includeHistory == false)
                        {
                            continue;
                        }
                    }

                    // If the program is withdrawn or dropped/changed-mind, skip it.
                    if (programStatus == StudentProgramStatusProcessingType.InActive || programStatus == StudentProgramStatusProcessingType.Withdrawn)
                    {
                        // If we are including inactive programs, then don't exclude
                        if (includeInactivePrograms == false && includeHistory == false)
                        {
                            continue;
                        }
                    }

                    // If the program doesn't have a start date, skip it.
                    if (prog.StprStartDate == null || prog.StprStartDate.Count == 0)
                    {
                        continue;
                    }

                    // If we're including historical data we want to return only programs where the start date is before the end of the parameter term if there is one.
                    if (includeHistory && term != null && (!prog.StprStartDate.Any(d => d < term.EndDate)))
                    {
                        continue;
                    }

                    string catcode = prog.StprCatalog;
                    string studentid = prog.Recordkey.Split('*')[0];
                    string progcode = prog.Recordkey.Split('*')[1];

                    StudentProgram stpr = new StudentProgram(studentid, progcode, catcode);

                    stpr.AnticipatedCompletionDate = prog.StprAntCmplDate;
                    if (prog.StprStartDate != null && prog.StprStartDate.Count() > 0)
                    {
                        var studentProgramStartDate = prog.StprStartDate.ElementAt(0);
                        if (studentProgramStartDate != null && studentProgramStartDate != DateTime.MinValue)
                        {
                            stpr.StartDate = studentProgramStartDate;
                        }
                    }
                    // Added logic to include inactive programs for comparisons in ESS, therefore, we need
                    // End Date from the student program.
                    // srm - 05/09/2014
                    if (prog.StprEndDate != null && prog.StprEndDate.Count() > 0)
                    {
                        var studentProgramEndDate = prog.StprEndDate.ElementAt(0);
                        if (studentProgramEndDate != null && studentProgramEndDate != DateTime.MinValue)
                        {
                            stpr.EndDate = studentProgramEndDate;
                        }
                    }
                    // Add additional data needed by the ESS project
                    // srm -5/09/2014
                    stpr.AdmitStatusCode = prog.StprAdmitStatus;
                    stpr.DepartmentCode = prog.StprDept;
                    stpr.Location = prog.StprLocation;
                    stpr.ProgramName = prog.StprTitle;
                    stpr.ProgramStatusProcessingCode = programStatus;
                    stpr.HasGraduated = (programStatus == StudentProgramStatusProcessingType.Graduated) ? true : false;
                    AcadPrograms acadProgramData = null;
                    StudentAcadLevels studentAcadLevelsData = null;
            
                    if (acadProgramCollection != null && acadProgramCollection.Any())
                    {
                        acadProgramData = acadProgramCollection.Where(a => a.Recordkey == progcode).FirstOrDefault();

                        if (studentAcadLevelsCollection != null && acadProgramData != null)
                        {
                            studentAcadLevelsData = studentAcadLevelsCollection.Where(a => a.Recordkey.Contains(studentid + "*"  + acadProgramData.AcpgAcadLevel)).FirstOrDefault();
                        }
                    }

                    if (studentAcadLevelsData != null)
                    {
                        stpr.AcadStartDate = studentAcadLevelsData.StaStartDate;
                        stpr.AcadEndDate = studentAcadLevelsData.StaEndDate;
                    }

                    if (acadProgramData != null)
                    {
                        // Add data from the Academic Program.
                        if (stpr.ProgramName == null || string.IsNullOrEmpty(stpr.ProgramName))
                        {
                            stpr.ProgramName = acadProgramData.AcpgTitle ?? progcode;
                        }
                        stpr.AcademicLevelCode = acadProgramData.AcpgAcadLevel;
                        stpr.DegreeCode = acadProgramData.AcpgDegree;

                        // Add majors from the Academic Program
                        foreach (var major in acadProgramData.AcpgMajors)
                        {
                            try
                            {
                                string name = (await studentReferenceRepo.GetMajorsAsync()).First(maj => maj.Code == major).Description ?? "";
                                if (!string.IsNullOrEmpty(major) && !string.IsNullOrEmpty(name))
                                {
                                    stpr.AddMajors(new StudentMajors(major, name, stpr.StartDate, stpr.EndDate));
                                }
                            }
                            catch { }
                        }

                        // Add minors from the Academic Program
                        foreach (var minor in acadProgramData.AcpgMinors)
                        {
                            try
                            {
                                string name = (await studentReferenceRepo.GetMinorsAsync()).First(min => min.Code == minor).Description ?? "";
                                if (!string.IsNullOrEmpty(minor) && !string.IsNullOrEmpty(name))
                                {
                                    stpr.AddMinors(new StudentMinors(minor, name, stpr.StartDate, stpr.EndDate));
                                }
                            }
                            catch { }
                        }
                    }
                    // Additional Requirements

                    // Major
                    foreach (var ar in prog.StprMajorListEntityAssociation)
                    {
                        bool addMajor = false;
                        if (includeHistory)
                        {
                            if (term == null)
                                addMajor = true; // No parameter term, no filtering.
                            else if (ar.StprAddnlMajorStartDateAssocMember != null && ar.StprAddnlMajorStartDateAssocMember < term.EndDate)
                                addMajor = true; // We want any major that began before the parameter term ended, even if it has ended (or hasn't started - it's term based so "today" doesn't matter).
                        }
                        else if ((ar.StprAddnlMajorStartDateAssocMember != null) && (ar.StprAddnlMajorEndDateAssocMember == null || ar.StprAddnlMajorEndDateAssocMember >= DateTime.Today))
                            addMajor = true; // If not including history, make sure the start date is on or before the current date AND the end date is null or after the current date
                        if (addMajor)
                        {
                            try
                            {
                                string awardName = (await studentReferenceRepo.GetMajorsAsync()).First(maj => maj.Code == ar.StprAddnlMajorsAssocMember).Description ?? "";
                                if (!string.IsNullOrEmpty(awardName))
                                {
                                    stpr.AddAddlRequirement(new AdditionalRequirement(ar.StprAddnlMajorsAssocMember, ar.StprAddnlMajorReqmtsAssocMember, AwardType.Major, awardName));
                                    // Add new Majors object which contains all majors from Program and Additional Requirements
                                    // srm - 05/09/2014
                                    var majorStartDate = ar.StprAddnlMajorStartDateAssocMember;
                                    if (majorStartDate == null || majorStartDate == DateTime.MinValue)
                                    {
                                        majorStartDate = stpr.StartDate;
                                    }
                                    if (!string.IsNullOrEmpty(ar.StprAddnlMajorsAssocMember) && !string.IsNullOrEmpty(awardName))
                                    {
                                        stpr.AddMajors(new StudentMajors(ar.StprAddnlMajorsAssocMember, awardName, majorStartDate, ar.StprAddnlMajorEndDateAssocMember));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var errorMessage = string.Format("Unable to lookup Major Code: '{0}', Student ID: '{1}'", ar.StprAddnlMajorsAssocMember, studentid);
                                logger.Error(ex, errorMessage);
                            }
                        }
                    }
                    // Minor
                    foreach (var ar in prog.StprMinorListEntityAssociation)
                    {
                        bool addMinor = false;
                        if (includeHistory)
                        {
                            if (term == null)
                                addMinor = true; // No parameter term, no filtering.
                            else if (ar.StprMinorStartDateAssocMember != null && ar.StprMinorStartDateAssocMember < term.EndDate)
                                addMinor = true; // We want any major that began before the parameter term ended, even if it has ended (or hasn't started - it's term based so "today" doesn't matter).
                        }
                        else if ((ar.StprMinorStartDateAssocMember != null ) && (ar.StprMinorEndDateAssocMember == null || ar.StprMinorEndDateAssocMember >= DateTime.Today))
                            addMinor = true; // If not including history, make sure the start date is on or before the current date AND the end date is null or after the current date
                        if (addMinor)
                        {
                            try
                            {
                                string awardName = (await studentReferenceRepo.GetMinorsAsync()).First(min => min.Code == ar.StprMinorsAssocMember).Description ?? "";
                                if (!string.IsNullOrEmpty(awardName))
                                {
                                    stpr.AddAddlRequirement(new AdditionalRequirement(ar.StprMinorsAssocMember, ar.StprMinorReqmtsAssocMember, AwardType.Minor, awardName));
                                    // Add new Minors object which contains all minors from Program and Additional Requirements
                                    var minorStartDate = ar.StprMinorStartDateAssocMember;
                                    if (!minorStartDate.HasValue)
                                    {
                                        minorStartDate = stpr.StartDate;
                                    }
                                    if (!string.IsNullOrEmpty(ar.StprMinorsAssocMember) && !string.IsNullOrEmpty(awardName))
                                    {
                                        stpr.AddMinors(new StudentMinors(ar.StprMinorsAssocMember, awardName, minorStartDate, ar.StprMinorEndDateAssocMember));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var errorMessage = string.Format("Unable to lookup Minor Code: '{0}', Student ID: '{1}'", ar.StprMinorsAssocMember, studentid);
                                logger.Error(ex, errorMessage);
                            }
                        }
                    }
                    // Ccd
                    foreach (var ar in prog.StprCcdListEntityAssociation)
                    {
                        // Make sure the start date is on or before the current date AND the end date is null or after the current date
                        if ((ar.StprCcdsStartDateAssocMember != null) && (ar.StprCcdsEndDateAssocMember == null || ar.StprCcdsEndDateAssocMember >= DateTime.Today))
                        {
                            try
                            {
                                string awardName = (await studentReferenceRepo.GetCcdsAsync()).First(ccd => ccd.Code == ar.StprCcdsAssocMember).Description ?? "";
                                if (!string.IsNullOrEmpty(awardName))
                                {
                                    stpr.AddAddlRequirement(new AdditionalRequirement(ar.StprCcdsAssocMember, ar.StprCcdsReqmtsAssocMember, AwardType.Ccd, awardName));
                                }
                            }
                            catch (Exception ex)
                            {
                                var errorMessage = string.Format("Unable to lookup CCD Code: '{0}', Student ID: '{1}'", ar.StprCcdsAssocMember, studentid);
                                logger.Error(ex, errorMessage);
                            }
                        }
                    }
                    // Specialization
                    foreach (var ar in prog.StprSpecialtiesEntityAssociation)
                    {
                        // Make sure the start date is on or before the current date AND the end date is null or after the current date
                        if ((ar.StprSpecializationStartAssocMember != null) && (ar.StprSpecializationEndAssocMember == null || ar.StprSpecializationEndAssocMember >= DateTime.Today))
                        {
                            try
                            {
                                string awardName = (await studentReferenceRepo.GetSpecializationsAsync()).First(spc => spc.Code == ar.StprSpecializationsAssocMember).Description ?? "";
                                if (!string.IsNullOrEmpty(awardName))
                                {
                                    stpr.AddAddlRequirement(new AdditionalRequirement(ar.StprSpecializationsAssocMember, ar.StprSpecializationReqmtsAssocMember, AwardType.Specialization, awardName));
                                }
                            }
                            catch (Exception ex)
                            {
                                var errorMessage = string.Format("Unable to lookup Specialization Code: '{0}', Student ID: '{1}'", ar.StprSpecializationsAssocMember, studentid);
                                logger.Error(ex, errorMessage);
                            }
                        }
                    }

                    // Overrides

                    if (overrideData.Count() > 0)
                    {
                        foreach (var over in overrideData)
                        {
                            if (over.StovAcadReqmtBlock != "")
                            {
                                //Make sure the data accessor didn't leave blanks in these
                                over.StovInclStudentAcadCred.RemoveAll(delegate (string s) { return s.Trim() == ""; });
                                over.StovExclStudentAcadCred.RemoveAll(delegate (string s) { return s.Trim() == ""; });

                                IEnumerable<string> includeCredits = over.StovInclStudentAcadCred;
                                IEnumerable<string> excludeCredits = over.StovExclStudentAcadCred;
                                try
                                {
                                    stpr.AddOverride(new Override(over.StovAcadReqmtBlock, includeCredits, excludeCredits));
                                }
                                catch (Exception ex)
                                {
                                    var errorMessage = string.Format("Unable to add override: '{0}', Student ID: '{1}'", over.Recordkey, studentid);
                                    logger.Error(ex, errorMessage);
                                }
                            }
                        }
                    }

                    // Exceptions

                    await BuildExceptionsAsync(prog.StprDaExcpts, exceptionData, stpr);

                    var allStudentProgramStatuses = new List<StudentProgramStatus>();
                    try
                    {
                        foreach (var studentProgramStatus in prog.StprStatusesEntityAssociation)
                        {
                            allStudentProgramStatuses.Add(new StudentProgramStatus(studentProgramStatus.StprStatusAssocMember, studentProgramStatus.StprStatusDateAssocMember));
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Concat("Unable to add all student statuses. Student ID: ", studentid);
                        logger.Error(ex, errorMessage);
                    }
                    stpr.StudentProgramStatuses = allStudentProgramStatuses;


                    programs.Add(stpr);
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Could not build program {0} for student {1}", prog.Recordkey.Split('*')[0], prog.Recordkey.Split('*')[1]));
                    logger.Error(e.GetBaseException().Message);
                    logger.Error(e.GetBaseException().StackTrace);
                    error = true;
                }
            }
            return programs;

        }

        private async Task BuildExceptionsAsync(List<string> exceptionids, Collection<StudentDaExcpts> exceptionData, StudentProgram stpr)
        {
            char _VM = Convert.ToChar(DynamicArray.VM);

            if (exceptionids != null && exceptionData != null && stpr != null && exceptionids.Count > 0 && exceptionData.Count() > 0)
            {
                foreach (var excp in exceptionData.Where(ex => exceptionids.Contains(ex.Recordkey)))
                {

                    string blockid = null;
                    if (!string.IsNullOrEmpty(excp.StexAcadReqmtBlock))
                    {
                        blockid = excp.StexAcadReqmtBlock;
                    }
                    string message = excp.StexPrintedSpec;

                    // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
                    message = message.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
                    // If there is a single-VM, replace it with a space.
                    message = message.Replace(_VM, ' ');

                    // Minimum Credit (Program and group level only) or Minimum Institutional Credit(All levels)
                    // Min GPA(All levels), Min Inst GPA (Program level only)
                    // waiver or replacement
                    if (!string.IsNullOrEmpty(excp.StexElement) && (excp.StexElement == "CRED" || excp.StexElement == "ICRED" ||
                                                                    excp.StexElement == "GPA" || excp.StexElement == "IGPA" || excp.StexElement == "CNT"))
                    {
                        decimal? newvalue = null;
                        decimal? newgpa = null;
                        int? newcount = null;


                        if (excp.StexType == "W")
                        {
                            //waiver, no-op, leave new value null
                        }
                        else if (excp.StexType == "R")
                        {
                            // only one of these will be populated
                            newvalue = excp.StexReplCred;
                            newcount = excp.StexReplCount;
                            newgpa = excp.StexReplGpa;
                        }
                        else
                        {
                            var errorMessage = string.Format("{0} can be (W)aived or (R)eplaced.  Code '{1}' is invalid within this context.  Student ID: '{2}', Program: '{3}'", excp.StexElement, excp.StexType, stpr.StudentId, stpr.ProgramCode);
                            logger.Error(errorMessage);
                            //throw new NotSupportedException(errorMessage);
                        }

                        switch (excp.StexElement)
                        {
                            case "ICRED":
                                {
                                    InstitutionalCreditModification institutionalCreditModification = new InstitutionalCreditModification(blockid, newvalue, message);
                                    stpr.AddRequirementModification(institutionalCreditModification);
                                    break;
                                }
                            case "CRED":
                                {
                                    CreditModification creditModification = new CreditModification(blockid, newvalue, message);
                                    stpr.AddRequirementModification(creditModification);
                                    break;
                                }
                            case "IGPA":
                                {
                                    InstitutionalGpaModification institutionalGpaModification = new InstitutionalGpaModification(blockid, newgpa, message);
                                    stpr.AddRequirementModification(institutionalGpaModification);
                                    break;
                                }
                            case "GPA":
                                {
                                    GpaModification gpaModification = new GpaModification(blockid, newgpa, message);
                                    stpr.AddRequirementModification(gpaModification);
                                    break;
                                }
                            case "CNT":
                                {
                                    CountModification countModification = new CountModification(blockid, newcount, message);
                                    stpr.AddRequirementModification(countModification);
                                    break;
                                }

                            default:
                                {
                                    var errorMessage = string.Format("Unexpected condition encountered in Colleague exception processing.  Student ID: '{0}, Program: '{1}'", stpr.StudentId, stpr.ProgramCode);
                                    logger.Error(errorMessage);
                                    break;
                                    //throw new NotSupportedException(errorMessage);
                                }
                        }
                    }
                    else if (!string.IsNullOrEmpty(excp.StexElement) && (excp.StexElement == "CRSL" || excp.StexElement == "ADEL"))
                    {
                        List<string> waivedcourselist = new List<string>();
                        List<string> additionalcourselist = new List<string>();
                        List<string> noweligiblecourselist = new List<string>();

                        // Waived courses and additional courses can exist in the same 
                        // STUDENT.DA.EXCPTS record in Colleague.  If both are in one record, 
                        // put them into a special Modification of their own
                        try
                        {
                            if ((excp.StexAddnlCourses != null && excp.StexAddnlCourses.Count > 0) && (excp.StexWaivedCourses != null && excp.StexWaivedCourses.Count > 0))
                            {

                                foreach (var waived in excp.StexWaivedCourses)
                                {
                                    if (!string.IsNullOrEmpty(waived))
                                    {
                                        waivedcourselist.Add(waived);
                                    }
                                }
                                foreach (var addl in excp.StexAddnlCourses)
                                {
                                    if (!string.IsNullOrEmpty(addl))
                                    {
                                        additionalcourselist.Add(addl);
                                    }
                                }
                                CourseWaiverAndCoursesAddition courseWaiverAndAddition = new CourseWaiverAndCoursesAddition(blockid, additionalcourselist, waivedcourselist, message);
                                stpr.AddRequirementModification(courseWaiverAndAddition);
                            }
                            else
                            {

                                // Course waiver
                                if (excp.StexWaivedCourses != null && excp.StexWaivedCourses.Count > 0)
                                {
                                    foreach (var waived in excp.StexWaivedCourses)
                                    {
                                        if (!string.IsNullOrEmpty(waived))
                                        {
                                            waivedcourselist.Add(waived);
                                        }
                                    }
                                    CourseWaiver courseWaiver = new CourseWaiver(blockid, waivedcourselist, message);
                                    stpr.AddRequirementModification(courseWaiver);
                                }

                                // Add required course
                                if (excp.StexAddnlCourses != null && excp.StexAddnlCourses.Count > 0)
                                {
                                    foreach (var addl in excp.StexAddnlCourses)
                                    {
                                        if (!string.IsNullOrEmpty(addl))
                                        {
                                            additionalcourselist.Add(addl);
                                        }
                                    }
                                    CoursesAddition courseAddition = new CoursesAddition(blockid, additionalcourselist, message);
                                    stpr.AddRequirementModification(courseAddition);
                                }

                                // Add eligible course to "from" list
                                if (excp.StexNowEligibleCourses != null && excp.StexNowEligibleCourses.Count > 0)
                                {
                                    foreach (var nowelg in excp.StexNowEligibleCourses)
                                    {
                                        if (!string.IsNullOrEmpty(nowelg))
                                        {
                                            noweligiblecourselist.Add(nowelg);
                                        }
                                    }
                                    FromCoursesAddition fromCourseAddition = new FromCoursesAddition(blockid, noweligiblecourselist, message);
                                    stpr.AddRequirementModification(fromCourseAddition);
                                }
                            }
                        }
                        catch (NotSupportedException ex)
                        {
                            var errorMessage = string.Format("Unable to add Exception: {0} for student id: {1} and program code: {2}", excp.Recordkey, stpr.StudentId, stpr.ProgramCode);
                            logger.Error(ex, string.Concat(errorMessage, Environment.NewLine, "Exception record is missing pointer to requirement block"));
                        }

                    }
                    else if (!string.IsNullOrEmpty(excp.StexElement) && (excp.StexElement == "BLK"))
                    {
                        // block replacement or waiver
                        Requirement newRequirement = null;
                        if (excp.StexType == "R")
                        {
                            // Replacement.  Construct the new requirement block.  Since each course
                            // can have its own minimum grade (making it its own group) - in the interest
                            // of simplicity we will just put each in its own group anyway.

                            Requirement req = new Requirement("", "", "", "", null) { IsBlockReplacement = true };
                            Subrequirement sub = new Subrequirement("", "") { IsBlockReplacement = true };

                            sub.Requirement = req;
                            req.MinSubRequirements = 1;
                            req.SubRequirements.Add(sub);

                            int groupcount = 0;
                            if (excp.BlockReplEntityAssociation != null && excp.BlockReplEntityAssociation.Any())
                            {
                                foreach (var blk in excp.BlockReplEntityAssociation)
                                {
                                    string courseid = blk.StexBlockReplCoursesAssocMember;
                                    if (!string.IsNullOrEmpty(courseid))
                                    {
                                        string gradeid = blk.StexBlockReplMinGradeAssocMember;
                                        string groupid = "Group " + (++groupcount).ToString();

                                        Group g = new Group(groupid, groupid, sub);
                                        g.Courses.Add(courseid);
                                        if (!string.IsNullOrEmpty(gradeid))
                                        {
                                            g.MinGrade = (await gradeRepo.GetAsync()).ToList().FirstOrDefault(grd => grd.Id == gradeid);
                                        }
                                        g.MinCourses = 1;
                                        g.GroupType = GroupType.TakeAll;
                                        g.SubRequirement = sub;
                                        g.IsBlockReplacement = true;
                                        sub.Groups.Add(g);

                                    }
                                }
                                sub.MinGroups = groupcount;
                                newRequirement = req;
                            }
                        }
                        try
                        {
                            BlockReplacement blockReplacement = new BlockReplacement(excp.StexAcadReqmtBlock, newRequirement, message);
                            stpr.AddRequirementModification(blockReplacement);
                        }
                        catch (Exception)
                        {
                            var errorMessage = string.Format("Unable to create block replacement for Student ID: '{0}, Program: '{1}', Exception: '{2}'", stpr.StudentId, stpr.ProgramCode, excp.Recordkey);
                            logger.Error(errorMessage);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// returns student program entities with just status information for student academic period profile. 
        /// </summary>
        /// <param name="stuProgIds">student program Ids</param>
        /// <returns>An IEnumerable list of StudentProgram Entities .</returns>
        public async Task<List<StudentProgram>> GetStudentAcademicPeriodProfileStudentProgramInfoAsync(List<string> stuProgIds)
        {
            var studentProgramCollection = await DataReader.BulkReadRecordAsync<StudentPrograms>(stuProgIds.ToArray());
            var studentProgramEntities = new List<Ellucian.Colleague.Domain.Student.Entities.StudentProgram>();

            if (studentProgramCollection != null & studentProgramCollection.Any())
            {
                foreach (var student in studentProgramCollection)
                {
                    string catcode = student.StprCatalog;
                    string studentid = student.Recordkey.Split('*')[0];
                    string progcode = student.Recordkey.Split('*')[1];
                    StudentProgram studentEntity = new StudentProgram(studentid, progcode, catcode);
                    if (student.StprStatusesEntityAssociation != null && student.StprStatusesEntityAssociation.Any())
                    {
                        var allStudentProgramStatuses = new List<StudentProgramStatus>();
                        foreach (var studentProgramStatus in student.StprStatusesEntityAssociation)
                        {
                            allStudentProgramStatuses.Add(new StudentProgramStatus(studentProgramStatus.StprStatusAssocMember, studentProgramStatus.StprStatusDateAssocMember));
                        }
                        studentEntity.StudentProgramStatuses = allStudentProgramStatuses;
                    }

                    studentProgramEntities.Add(studentEntity);
                }
            }
            return studentProgramEntities;
        }

        /// <summary>
        /// Call a colleague transaction to calculate and return any notices relevant to this student and program.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="programCode"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EvaluationNotice>> GetStudentProgramEvaluationNoticesAsync(string studentId, string programCode)
        {
            var studentProgramNotices = new List<EvaluationNotice>();

            var request = new BuildDegreeAuditCustomTextRequest() { AStudentId = studentId, AProgramCode = programCode };
            var response = new BuildDegreeAuditCustomTextResponse();
            try
            {
                response = await transactionInvoker.ExecuteAsync<BuildDegreeAuditCustomTextRequest, BuildDegreeAuditCustomTextResponse>(request);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                response = null;
            }
            if (response == null)
            {
                var errorMessage = string.Format("Unable to get DegreeAuditCustomText for student: '{0}', program: '{1}'", studentId, programCode);
                logger.Error(errorMessage);
            }

            if (response != null)
            {
                try
                {
                    if (response.AlStudentProgramText != null && response.AlStudentProgramText.Count() > 0)
                    {
                        studentProgramNotices.Add(new EvaluationNotice(studentId, programCode, response.AlStudentProgramText, EvaluationNoticeType.StudentProgram));
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }

                try
                {
                    if (response.AlAcademicProgramText != null && response.AlAcademicProgramText.Count() > 0)
                    {
                        studentProgramNotices.Add(new EvaluationNotice(studentId, programCode, response.AlAcademicProgramText, EvaluationNoticeType.Program));
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }

                try
                {
                    if (response.AlStartText != null && response.AlStartText.Count() > 0)
                    {
                        studentProgramNotices.Add(new EvaluationNotice(studentId, programCode, response.AlStartText, EvaluationNoticeType.Start));
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }

                try
                {
                    if (response.AlEndText != null && response.AlEndText.Count() > 0)
                    {
                        studentProgramNotices.Add(new EvaluationNotice(studentId, programCode, response.AlEndText, EvaluationNoticeType.End));
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }
            }
            return studentProgramNotices;
        }
    }
}
