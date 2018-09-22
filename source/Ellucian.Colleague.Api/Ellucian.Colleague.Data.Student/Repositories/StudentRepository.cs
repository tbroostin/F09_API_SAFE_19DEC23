// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Transcripts;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Security;
using Ellucian.Web.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentRepository : PersonRepository, IStudentRepository
    {
        private ApplValcodes StudentProgramStatuses;
        private ApplValcodes EducationalGoals;
        private ApplValcodes InstitutionTypes;
        private ApplValcodes PersonEthnics;
        private ApplValcodes PersonRaces;
        private ApplValcodes MaritalStatuses;
        private ApplValcodes ParentEducationLevel;
        private IEnumerable<ResidencyStatus> ResidencyStatuses;
        private IEnumerable<AdmittedStatus> AdmittedStatuses;
        private Base.DataContracts.Defaults CoreDefaults;
        private Data.Base.DataContracts.IntlParams internationalParameters;
        private readonly string colleagueTimeZone;
        private string studentCache = "Student";

        public StudentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        #region Validation Tables

        private async Task<ApplValcodes> GetStudentProgramStatusesAsync()
        {
            if (StudentProgramStatuses != null)
            {
                return StudentProgramStatuses;
            }

            StudentProgramStatuses = await GetOrAddToCacheAsync<ApplValcodes>("StudentProgramStatuses",
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
            return StudentProgramStatuses;
        }

        /// <summary>
        /// Return the Validation Table InstTypes for determination of High School or College
        /// within the Institutions Attended data.
        /// </summary>
        /// <returns></returns>
        private async Task<ApplValcodes> GetInstitutionTypesAsync()
        {
            if (InstitutionTypes != null)
            {
                return InstitutionTypes;
            }

            InstitutionTypes = await GetOrAddToCacheAsync<ApplValcodes>("InstitutionTypes",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access INST.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return InstitutionTypes;
        }

        private async Task<ApplValcodes> GetEducationalGoalsAsync()
        {
            if (EducationalGoals != null)
            {
                return EducationalGoals;
            }

            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            EducationalGoals = await GetOrAddToCacheAsync<ApplValcodes>("EducationGoals",
                async () =>
                {
                    ApplValcodes educationGoalsTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EDUCATION.GOALS");
                    if (educationGoalsTable == null)
                    {
                        var errorMessage = "Unable to access EDUCATION.GOALS valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return educationGoalsTable;
                }, Level1CacheTimeoutValue);
            return EducationalGoals;
        }

        /// <summary>
        /// Get Residency Status for translate of code to description
        /// </summary>
        /// <returns>Residency Statuses Validation table</returns>
        public async Task<IEnumerable<ResidencyStatus>> GetResidencyStatusesAsync(bool ignoreCache = false)
        {
            if (ResidencyStatuses != null)
            {
                return ResidencyStatuses;
            }

            ResidencyStatuses = await GetGuidCodeItemAsync<ResidencyStatuses, ResidencyStatus>("AllResidencyStatuses", "RESIDENCY.STATUSES",
                (r, g) => new ResidencyStatus(g, r.Recordkey, (String.IsNullOrEmpty(r.ResDesc)? r.Recordkey : r.ResDesc)), bypassCache: ignoreCache);
            if (ResidencyStatuses == null)
            {
                var errorMessage = "Unable to access RESIDENCY.STATUSES code table.";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }
            return ResidencyStatuses;
        }

        /// <summary>
        ///  Get Admit status for code/description/transfer flag
        /// </summary>
        /// <returns>Admitted Status Validation table</returns>
        private async Task<IEnumerable<AdmittedStatus>> GetAdmitStatusesAsync()
        {
            if (AdmittedStatuses != null)
            {
                return AdmittedStatuses;
            }
            AdmittedStatuses = await GetCodeItemAsync<AdmitStatuses, AdmittedStatus>("AllAdmitStatuses", "ADMIT.STATUSES",
                a => new AdmittedStatus(a.Recordkey, a.AdmsDesc, a.AdmsTransferFlag));
            if (AdmittedStatuses == null)
            {
                var errorMessage = "Unable to access ADMIT.STATUSES code table.";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }
            return AdmittedStatuses;
        }

        private async Task<ApplValcodes> GetEthnicitiesAsync()
        {
            if (PersonEthnics != null)
            {
                return PersonEthnics;
            }

            PersonEthnics = await GetOrAddToCacheAsync<ApplValcodes>("PersonEthnics",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access PERSON.ETHNICS valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return PersonEthnics;
        }

        private async Task<ApplValcodes> GetRacesAsync()
        {
            if (PersonRaces != null)
            {
                return PersonRaces;
            }

            PersonRaces = await GetOrAddToCacheAsync<ApplValcodes>("PersonRaces",
                async () =>
                {
                    ApplValcodes racesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES");
                    if (racesTable == null)
                    {
                        var errorMessage = "Unable to access PERSON.RACES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return racesTable;
                }, Level1CacheTimeoutValue);
            return PersonRaces;
        }

        private async Task<ApplValcodes> GetMaritalStatusesAsync()
        {
            if (MaritalStatuses != null)
            {
                return MaritalStatuses;
            }

            MaritalStatuses = await GetOrAddToCacheAsync<ApplValcodes>("MaritalStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access MARITAL.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return MaritalStatuses;
        }

        private async Task<ApplValcodes> GetParentEducationLevelAsync()
        {
            if (ParentEducationLevel != null)
            {
                return ParentEducationLevel;
            }

            ParentEducationLevel = await GetOrAddToCacheAsync<ApplValcodes>("ParentEducationLevel",
                async () =>
                {
                    ApplValcodes educationLevel = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "PARENT.EDUCATION.LEVEL");
                    if (educationLevel == null)
                    {
                        var errorMessage = "Unable to access PARENT.EDUCATION.LEVEL valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return educationLevel;
                }, Level1CacheTimeoutValue);
            return ParentEducationLevel;
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private async Task<Base.DataContracts.Defaults> GetDefaultsAsync()
        {
            if (CoreDefaults != null)
            {
                return CoreDefaults;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            CoreDefaults = await GetOrAddToCacheAsync<Data.Base.DataContracts.Defaults>("CoreDefaults",
                async () =>
                {
                    Data.Base.DataContracts.Defaults coreDefaults = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        var errorMessage = "Unable to access DEFAULTS from CORE.PARMS table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
            return CoreDefaults;
        }

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
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

        #endregion

        #region IStudentRepository

        /// <summary>
        /// get students w/o caching
        /// Include citizenship statuses
        /// </summary>
        /// <param name="studentIds">List of Student IDs</param>
        /// <returns>Student Object</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetStudentsByIdAsync(IEnumerable<string> studentIds, Term termData, IEnumerable<CitizenshipStatus> citizenshipStatusData, bool inheritFromPerson = true, bool getDegreePlan = true, bool filterAdvisorsByTerm = false, bool filterEndedAdvisements = false)
        {
            IEnumerable<Domain.Student.Entities.Student> studentEntities = new List<Domain.Student.Entities.Student>();

            if (studentIds != null && studentIds.Count() > 0)
            {
                // get students data
                Collection<Ellucian.Colleague.Data.Student.DataContracts.Students> students = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Students>(studentIds.ToArray());
                if (students != null && students.Count > 0)
                {
                    // get student programs and student acad levels keys
                    var stprIds = new List<string>();
                    var acadLevelIds = new List<string>();
                    foreach (var student in students)
                    {
                        if (student.StuAcadPrograms != null)
                        {
                            foreach (var acadProgramId in student.StuAcadPrograms)
                            {
                                stprIds.Add(student.Recordkey + "*" + acadProgramId);
                            }
                        }
                        if (student.StuAcadLevels != null)
                        {
                            foreach (var acadLevelId in student.StuAcadLevels)
                            {
                                acadLevelIds.Add(student.Recordkey + "*" + acadLevelId);
                            }
                        }
                    }
                    Collection<StudentPrograms> programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.Distinct().ToArray());

                    // Get PERSON.ST data
                    Collection<PersonSt> personSt = await DataReader.BulkReadRecordAsync<PersonSt>(studentIds.ToArray());

                    // Get STUDENT.ACAD.LEVELS data contract
                    // string[] acadLevelIds = DataReader.Select("STUDENT.ACAD.LEVELS", "WITH STA.STUDENT = '?'", studentIds.ToArray());

                    Collection<StudentAcadLevels> studentAcadLevelsData = new Collection<StudentAcadLevels>();
                    if (acadLevelIds != null && acadLevelIds.Count() > 0)
                    {
                        studentAcadLevelsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StudentAcadLevels>(acadLevelIds.Distinct().ToArray());
                    }

                    // Get StudentAdvisement data
                    Collection<Student.DataContracts.StudentAdvisement> studentAdvisementData = await DataReader.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(personSt.SelectMany(a => a.PstAdvisement).Distinct().ToArray());

                    // Get PERSON data
                    Collection<Base.DataContracts.Person> personContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", studentIds.ToArray());
                    // Get FOREIGN.PERSON data
                    Collection<Base.DataContracts.ForeignPerson> foreignPersonContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(studentIds.ToArray());
                    // Get APPLICANTS data
                    Collection<Student.DataContracts.Applicants> applicantsContract = await DataReader.BulkReadRecordAsync<Student.DataContracts.Applicants>(studentIds.ToArray());

                    // Get INSTITUTIONS and INSTITUTIONS.ATTEND data
                    Collection<Base.DataContracts.Institutions> institutionData = new Collection<Base.DataContracts.Institutions>();
                    Collection<Base.DataContracts.InstitutionsAttend> instAttendData = new Collection<Base.DataContracts.InstitutionsAttend>();
                    var instAttendIds = new List<string>();
                    var institutionIds = new List<string>();
                    if (personContract != null && personContract.Count > 0)
                    {
                        foreach (var person in personContract)
                        {
                            if (person.PersonInstitutionsAttend != null)
                            {
                                foreach (var instId in person.PersonInstitutionsAttend)
                                {
                                    instAttendIds.Add(person.Recordkey + "*" + instId);
                                    institutionIds.Add(instId);
                                }
                            }
                        }
                        if (institutionIds.Count > 0)
                        {
                            institutionData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Institutions>(institutionIds.Distinct().ToArray());
                            instAttendData = await DataReader.BulkReadRecordAsync<Base.DataContracts.InstitutionsAttend>(instAttendIds.ToArray());
                        }

                        // Get Parents PERSON data                    
                        var otherIds = new List<string>();
                        foreach (var person in personContract)
                        {
                            if (person.Parents != null)
                            {
                                foreach (var parent in person.Parents)
                                {
                                    otherIds.Add(parent);
                                }
                            }
                        }
                        Collection<Base.DataContracts.Person> otherContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", otherIds.ToArray());
                        if (otherContract != null && otherContract.Count > 0)
                        {
                            foreach (var other in otherContract)
                            {
                                if (other != null)
                                {
                                    personContract.Add(other);
                                }
                            }
                        }
                    }

                    //Get the FIN.AID datacontract
                    var financialAidData = await DataReader.BulkReadRecordAsync<Student.DataContracts.FinAid>(studentIds.ToArray());

                    // Now build domain objects
                    studentEntities = await BuildStudentsAsync(studentIds, students, programData,
                        personSt, studentAdvisementData, personContract, financialAidData, foreignPersonContract,
                        applicantsContract, instAttendData, institutionData, instAttendData, studentAcadLevelsData,
                        inheritFromPerson, getDegreePlan, termData, filterAdvisorsByTerm, citizenshipStatusData, filterEndedAdvisements);
                }
            }
            return studentEntities;
        }
        
        /// <summary>
        /// Retrieves a list of student IDs for a given term
        /// </summary>
        /// <param name="id">Term ID</param>
        /// <returns>A list of Student IDs for the term.</returns>
        public async Task<IEnumerable<string>> SearchIdsAsync(string termId)
        {
            var studentIds = new string[] {};
            var selectCriteria = "WITH STU.TERMS = '" + termId + "'";
            studentIds = await DataReader.SelectAsync("STUDENTS", selectCriteria);
            return studentIds;
        }

        public async Task<GradeRestriction> GetGradeRestrictionsAsync(string id)
        {
            GetGradeViewRestrictionsRequest gradeViewRequest = new GetGradeViewRestrictionsRequest();
            gradeViewRequest.PersonId = id;
            GetGradeViewRestrictionsResponse gradeViewResponse = await transactionInvoker.ExecuteAsync<GetGradeViewRestrictionsRequest, GetGradeViewRestrictionsResponse>(gradeViewRequest);

            if (gradeViewResponse.IsRestricted == "N")
            {
                return new GradeRestriction(false);
            }
            else
            {
                GradeRestriction gradeRestriction = new GradeRestriction(true);
                if (gradeViewResponse.Reasons != null && gradeViewResponse.Reasons.Count > 0)
                {
                    foreach (var reason in gradeViewResponse.Reasons)
                    {
                        gradeRestriction.AddReason(reason);
                    }
                }
                return gradeRestriction;
            }
        }

        public Task<IEnumerable<StudentCohort>> GetAllStudentCohortAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }

        public async Task<RegistrationEligibility> CheckRegistrationEligibilityAsync(string id)
        {

            var messages = new List<RegistrationMessage>();
            var eligibilityRequest = new CheckRegistrationEligibilityRequest();
            eligibilityRequest.StudentId = id;

            var eligibilityReponse = await transactionInvoker.ExecuteAsync<CheckRegistrationEligibilityRequest, CheckRegistrationEligibilityResponse>(eligibilityRequest);

            // Return messages regardless of eligibility.
            if (eligibilityReponse.Messages.Count() > 0)
            {
                foreach (var message in eligibilityReponse.Messages)
                {
                    messages.Add(new RegistrationMessage() {Message = message});
                }
            }

            var registrationEligibility = new RegistrationEligibility(messages, eligibilityReponse.Eligible, eligibilityReponse.HasOverride);

            // Add in the additional eligibility information for each term as appropriate.
            if (eligibilityReponse.Terms != null && eligibilityReponse.Terms.Count > 0)
            {
                // Ellucian.Colleague.Domain.Student.Entities.Student student = Get(id);
                foreach (var term in eligibilityReponse.Terms)
                {

                    RegistrationEligibilityTermStatus status = RegistrationEligibilityTermStatus.Open;
                    DateTimeOffset? anticipatedRegistrationDate = term.TermAddCheckDate.ToPointInTimeDateTimeOffset(term.TermAddCheckDate, colleagueTimeZone);

                    // Setting the term Registration Eligibility Status:
                    // 1) If that term has a Term Priority Override set status to override and move on.
                    // 2) If the student is not eligible overall then he isn't eligibile in any term.
                    // 3) If the student is eligible overall 
                    //     a) see if he has Term add allowed and if so registration is open for him.
                    //     b) see if he has a future term add check date. If so he is "future".  This would come from the reg period OR reg rules
                    //     c) otherwise he is just not eligible because it is either past or some other issue.

                    if (term.TermPriorityOverride)
                    {
                        status = RegistrationEligibilityTermStatus.HasOverride;
                    }
                    else if (eligibilityReponse.Eligible)
                    {
                        if (term.TermAddAllowed)
                        {
                            status = RegistrationEligibilityTermStatus.Open;
                        }
                        else if (anticipatedRegistrationDate != null && anticipatedRegistrationDate > DateTimeOffset.Now)
                        {
                            status = RegistrationEligibilityTermStatus.Future;
                        }
                        else
                        {
                            status = RegistrationEligibilityTermStatus.NotEligible;
                        }
                    }
                    else
                    {
                        status = RegistrationEligibilityTermStatus.NotEligible;
                    }

                    try
                    {
                        RegistrationEligibilityTerm regTerm = new RegistrationEligibilityTerm(term.TermCode, term.TermCheckPriority, term.TermPriorityOverride);
                        regTerm.AnticipatedTimeForAdds = anticipatedRegistrationDate;
                        regTerm.Status = status;
                        regTerm.Message = term.TermAddMessages;
                        regTerm.SkipWaitlistAllowed = term.TermSkipWaitlistAllowed;
                        regTerm.FailedRegistrationTermRules = term.TermRegRulesFailed;
                        registrationEligibility.AddRegistrationEligibilityTerm(regTerm);
                    }
                    catch (Exception)
                    {
                        if (logger.IsInfoEnabled)
                        {
                            var inError = "Registration Eligibility Term corrupt";
                            var inString = string.Format("Registration Term Code: '{0}', Student Id: '{1}'", term.TermCode, id);
                            var formattedterm = ObjectFormatter.FormatAsXml(inString);
                            var errorMessage = string.Format("{0}" + Environment.NewLine + "{1}", inError, formattedterm);
                            logger.Info(errorMessage);
                        }
                    }
                }
            }

            return registrationEligibility;
        }
        
        /// <summary>
        /// Call the eligibility transaction to see if this student is eligible to register
        /// for classes.  This version calls the Ethos version of the Colleague transaction.
        /// Used in Ethos API student-registration-eligibilities V9.
        /// </summary>
        /// <param name="id">Key to Colleague's Student</param>
        /// <returns>RegistrationEligibility Entity containing data about eligibility.</returns>
        public async Task<RegistrationEligibility> CheckRegistrationEligibilityEthosAsync(string id, List<string> termCodes)
        {
            var messages = new List<RegistrationMessage>();
            var eligibilityRequest = new CheckRegEligibilityEthosRequest();
            eligibilityRequest.StudentId = id;
            var termsList = new List<EthosTerms>();
            foreach (var termCode in termCodes)
            {
                var term = new EthosTerms() { TermCode = termCode };
                termsList.Add(term);
            }
            eligibilityRequest.EthosTerms = termsList;

            var eligibilityReponse = await transactionInvoker.ExecuteAsync<CheckRegEligibilityEthosRequest, CheckRegEligibilityEthosResponse>(eligibilityRequest);

            // Return messages regardless of eligibility.
            if (eligibilityReponse.Messages.Count() > 0)
            {
                foreach (var message in eligibilityReponse.Messages)
                {
                    messages.Add(new RegistrationMessage() { Message = message });
                }
            }

            var registrationEligibility = new RegistrationEligibility(messages, eligibilityReponse.Eligible, eligibilityReponse.HasOverride);

            // Add in the additional eligibility information for each term as appropriate.
            if (eligibilityReponse.EthosTerms != null && eligibilityReponse.EthosTerms.Count > 0)
            {
                // Ellucian.Colleague.Domain.Student.Entities.Student student = Get(id);
                foreach (var term in eligibilityReponse.EthosTerms)
                {

                    RegistrationEligibilityTermStatus status = RegistrationEligibilityTermStatus.Open;
                    DateTimeOffset? anticipatedRegistrationDate = term.TermAddCheckDate.ToPointInTimeDateTimeOffset(term.TermAddCheckDate, colleagueTimeZone);

                    // Setting the term Registration Eligibility Status:
                    // 1) If that term has a Term Priority Override set status to override and move on.
                    // 2) If the student is not eligible overall then he isn't eligibile in any term.
                    // 3) If the student is eligible overall 
                    //     a) see if he has Term add allowed and if so registration is open for him.
                    //     b) see if he has a future term add check date. If so he is "future".  This would come from the reg period OR reg rules
                    //     c) otherwise he is just not eligible because it is either past or some other issue.

                    if (term.TermPriorityOverride)
                    {
                        status = RegistrationEligibilityTermStatus.HasOverride;
                    }
                    else if (eligibilityReponse.Eligible)
                    {
                        if (term.TermAddAllowed)
                        {
                            status = RegistrationEligibilityTermStatus.Open;
                        }
                        else if (anticipatedRegistrationDate != null && anticipatedRegistrationDate > DateTimeOffset.Now)
                        {
                            status = RegistrationEligibilityTermStatus.Future;
                        }
                        else
                        {
                            status = RegistrationEligibilityTermStatus.NotEligible;
                        }
                    }
                    else
                    {
                        status = RegistrationEligibilityTermStatus.NotEligible;
                    }

                    try
                    {
                        RegistrationEligibilityTerm regTerm = new RegistrationEligibilityTerm(term.TermCode, term.TermCheckPriority, term.TermPriorityOverride);
                        regTerm.AnticipatedTimeForAdds = anticipatedRegistrationDate;
                        regTerm.Status = status;
                        regTerm.Message = term.TermAddMessages;
                        regTerm.SkipWaitlistAllowed = term.TermSkipWaitlistAllowed;
                        regTerm.FailedRegistrationTermRules = term.TermRegRulesFailed;
                        registrationEligibility.AddRegistrationEligibilityTerm(regTerm);
                    }
                    catch (Exception)
                    {
                        if (logger.IsInfoEnabled)
                        {
                            var inError = "Registration Eligibility Term corrupt";
                            var inString = string.Format("Registration Term Code: '{0}', Student Id: '{1}'", term.TermCode, id);
                            var formattedterm = ObjectFormatter.FormatAsXml(inString);
                            var errorMessage = string.Format("{0}" + Environment.NewLine + "{1}", inError, formattedterm);
                            logger.Info(errorMessage);
                        }
                    }
                }
            }

            return registrationEligibility;
        }

        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request)
        {
            RegisterForSectionsRequest updateRequest = new RegisterForSectionsRequest();
            updateRequest.Sections = new List<Sections>();

            updateRequest.StudentId = request.StudentId;
            // For every section submitted, add a Sections object to the updateRequest
            foreach (var section in request.Sections)
            {

                updateRequest.Sections.Add(new Sections() {SectionIds = section.SectionId, SectionAction = section.Action.ToString(), SectionCredits = section.Credits, SectionDropReasonCode = section.DropReasonCode});
            }

            // Submit the registration
            RegisterForSectionsResponse updateResponse = await transactionInvoker.ExecuteAsync<RegisterForSectionsRequest, RegisterForSectionsResponse>(updateRequest);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.ErrorMessage))
            {
                throw new InvalidOperationException(updateResponse.ErrorMessage);
            }

            // Process the messages returned by colleague registration 
            var outputMessages = new List<RegistrationMessage>();
            if (updateResponse.Messages.Count > 0)
            {
                foreach (var message in updateResponse.Messages)
                {
                    outputMessages.Add(new RegistrationMessage() {Message = message.Message, SectionId = message.MessageSection});
                }
            }

            return new RegistrationResponse(outputMessages, updateResponse.IpcRegId);
        }


        // <summary>
        /// Gets the transcript restrictions for the indicated student.
        /// </summary>
        /// <param name="id">Id of the student</param>
        /// <returns>The list of <see cref="TranscriptRestriction">TranscriptRestrictions</see> found for this student</returns>
        public async Task<IEnumerable<TranscriptRestriction>> GetTranscriptRestrictionsAsync(string id)
        {
            var returnval = new List<TranscriptRestriction>();

            GetTranscriptHoldsRequest req;
            GetTranscriptHoldsResponse resp = null;
            try
            {
                req = new GetTranscriptHoldsRequest() {APersonId = id};
                resp = await transactionInvoker.ExecuteAsync<GetTranscriptHoldsRequest, GetTranscriptHoldsResponse>(req);
            }
            catch (Exception e)
            {
                logger.Error("Error thrown in GetTranscriptHolds transaction");
                logger.Error(e.Message);
                throw e;
            }

            if (resp != null && resp.HoldGroup != null && resp.HoldGroup.Count() > 0)
            {
                foreach (var holdgroup in resp.HoldGroup)
                {
                    if (!string.IsNullOrEmpty(holdgroup.FailRuleIds))
                    {
                        if (holdgroup.FailRuleTypes == "X")
                        {
                            // Transaction failed to identify person ID given as belonging to a student.
                            logger.Error(string.Format("In GetTranscriptRestrictions(): '{0}' is not a valid student ID", id));
                            throw new KeyNotFoundException();
                        }
                        returnval.Add(new TranscriptRestriction() {Code = holdgroup.FailRuleIds, Description = holdgroup.FailRuleMsgs});
                    }

                }
            }

            return returnval;
        }


        /// <summary>
        /// Reads the student information from Colleague and returns an IEnumerable of Students Entity models.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list if none are found.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetAsync(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentException("ids", "You must specify at least 1 id to retrieve.");
            }
            else
            {
                var watch = new Stopwatch();
                watch.Start();

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student> studentEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Student>();

                Collection<Students> students = await DataReader.BulkReadRecordAsync<Students>(ids.ToArray());

                // get student programs and student acad levels keys
                var studentProgramIds = new List<string>();
                var acadLevelIds = new List<string>();
                foreach (var student in students)
                {
                    if (student.StuAcadPrograms != null)
                    {
                        foreach (var acadProgramId in student.StuAcadPrograms)
                        {
                            studentProgramIds.Add(student.Recordkey + "*" + acadProgramId);
                        }
                    }
                    if (student.StuAcadLevels != null)
                    {
                        foreach (var acadLevelId in student.StuAcadLevels)
                        {
                            acadLevelIds.Add(student.Recordkey + "*" + acadLevelId);
                        }
                    }
                }
                Collection<StudentPrograms> programData = new Collection<StudentPrograms>();
                if (studentProgramIds != null && studentProgramIds.Count > 0)
                {
                    programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.ToArray());
                }

                // Get PERSON.ST data contract
                Collection<Ellucian.Colleague.Data.Base.DataContracts.PersonSt> personStData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonSt>(ids.ToArray());

                // Get STUDENT.ACAD.LEVELS data contract
                // string[] acadLevelIds = DataReader.Select("STUDENT.ACAD.LEVELS", "WITH STA.STUDENT = '?'", ids.ToArray());
                Collection<StudentAcadLevels> studentAcadLevelsData = new Collection<StudentAcadLevels>();
                if (acadLevelIds != null && acadLevelIds.Count() > 0)
                {
                    studentAcadLevelsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StudentAcadLevels>(acadLevelIds.Distinct().ToArray());
                }

                // Get StudentAdvisement data
                Collection<Student.DataContracts.StudentAdvisement> studentAdvisementData = new Collection<Student.DataContracts.StudentAdvisement>();
                if (personStData != null && personStData.Count > 0)
                {
                    var studentAdvisementIds = new List<string>();
                    foreach (var personSt in personStData)
                    {
                        foreach (var advisementId in personSt.PstAdvisement)
                        {
                            studentAdvisementIds.Add(advisementId);
                        }
                    }
                    studentAdvisementData = await DataReader.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(studentAdvisementIds.Distinct().ToArray());

                }

                // Get PERSON data
                Collection<Base.DataContracts.Person> personContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", ids.ToArray());

                // Get FOREIGN.PERSON data
                Collection<Base.DataContracts.ForeignPerson> foreignPersonContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(ids.ToArray());

                // Get APPLICANTS data
                Collection<Student.DataContracts.Applicants> applicantsContract = await DataReader.BulkReadRecordAsync<Student.DataContracts.Applicants>(ids.ToArray());

                // Get INSTITUTIONS and INSTITUTIONS.ATTEND data
                Collection<Base.DataContracts.Institutions> institutionData = new Collection<Base.DataContracts.Institutions>();
                Collection<Base.DataContracts.InstitutionsAttend> instAttendData = new Collection<Base.DataContracts.InstitutionsAttend>();
                var instAttendIds = new List<string>();
                var institutionIds = new List<string>();

                foreach (var person in personContract)
                {
                    if (person.PersonInstitutionsAttend != null)
                    {
                        foreach (var instId in person.PersonInstitutionsAttend)
                        {
                            instAttendIds.Add(person.Recordkey + "*" + instId);
                            institutionIds.Add(instId);
                        }
                    }
                }
                if (institutionIds.Count > 0)
                {
                    institutionData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Institutions>(institutionIds.Distinct().ToArray());
                    instAttendData = await DataReader.BulkReadRecordAsync<Base.DataContracts.InstitutionsAttend>(instAttendIds.ToArray());
                }

                // Get Parents PERSON data                    
                if (personContract != null && personContract.Count > 0)
                {
                    var otherIds = new List<string>();

                    foreach (var person in personContract)
                    {
                        if (person.Parents != null)
                        {
                            foreach (var parent in person.Parents)
                            {
                                otherIds.Add(parent);
                            }
                        }
                    }
                    if (otherIds.Count > 0)
                    {
                        Collection<Base.DataContracts.Person> otherContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", otherIds.ToArray());

                        if (otherContract != null && otherContract.Count > 0)
                        {
                            foreach (var other in otherContract)
                            {
                                if (other != null)
                                {
                                    personContract.Add(other);
                                }
                            }
                        }
                    }
                }

                // Get FIN.AID data contracts
                var financialAidData = await DataReader.BulkReadRecordAsync<Student.DataContracts.FinAid>(ids.ToArray());

                watch.Stop();
                logger.Info("    STEPX.4.1 Bulk Read Student Data... completed in " + watch.ElapsedMilliseconds.ToString());

                Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
                IEnumerable<CitizenshipStatus> citizenshipStatusData = null;
                // Now that we have all the data, assemble the entities.
                studentEntities = await BuildStudentsAsync(ids, students, programData,
                    personStData, studentAdvisementData, personContract, financialAidData,
                    foreignPersonContract, applicantsContract,
                    instAttendData, institutionData, instAttendData, studentAcadLevelsData,
                    true, true, termData, false, citizenshipStatusData, false);

                return studentEntities;
            }
        }

        /// <summary>
        /// Gets RosterStudents for each Colleague Id provided.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>IEnumerable of RosterStudent entities</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.RosterStudent>> GetRosterStudentsAsync(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentException("You must specify at least 1 id to retrieve.", "ids");
            }
            else
            {
                List<Ellucian.Colleague.Domain.Student.Entities.RosterStudent> rosterStudentEntities = new List<RosterStudent>();
                Collection<Base.DataContracts.Person> personContracts = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", ids.ToArray());
                if (personContracts != null && personContracts.Count > 0)
                {
                    foreach (Base.DataContracts.Person person in personContracts)
                    {
                        try
                        {
                            rosterStudentEntities.Add(new RosterStudent(person.Recordkey, person.LastName) {FirstName = person.FirstName, MiddleName = person.MiddleName});
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, string.Format("Unable to create RosterStudent entity"));
                        }
                    }
                }

                return rosterStudentEntities;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="dateOfBirth"></param>
        /// <param name="formerName"></param>
        /// <param name="studentId"></param>
        /// <param name="governmentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.Student.Entities.Student>> SearchAsync(string lastName, string firstName, DateTime? dateOfBirth, string formerName, string studentId, string governmentId)
        {
            // Trim spaces
            lastName = (lastName != null) ? lastName.Trim() : null;
            firstName = (firstName != null) ? firstName.Trim() : null;
            formerName = (formerName != null) ? formerName.Trim() : null;
            studentId = (studentId != null) ? studentId.Trim() : null;
            governmentId = (governmentId != null) ? governmentId.Trim() : null;

            
            Ellucian.Data.Colleague.DataContracts.IntlParams intlParams = await base.GetInternationalParametersAsync();
            

            string searchString = "";
            string Quote = '"'.ToString();

            // Use the new PersonRepository.Search() for the name.  It returns an array of IDs that 
            // match the name.  (EQ XXX YYY ZZZ in Envision basic means "matches either XXX YYY OR ZZZ")

            IEnumerable<string> idList = await base.SearchByNameAsync(lastName, firstName, null);


            // 20160527 MBS SCR 142872 CR-00014050 

            // Do not put the IDs into the search string.  There is a datareader method now
            // that will put the keys from above into a select list, then subselect against 
            // that with the string.

            // If no hits, return empty
            if (idList.Count() == 0)
            {
                return new List<Domain.Student.Entities.Student>();
            }

            // If we were given a student ID, make sure it's in the list 
            if (!string.IsNullOrEmpty(studentId))
            {
                if (!idList.Contains(studentId) && !ContainsNumericEquivalent(idList, studentId))
                {
                    return new List<Domain.Student.Entities.Student>();
                }
            }

            // If given a government ID, we'll make sure it matches
            if (!string.IsNullOrEmpty(governmentId))
            {
                if (searchString == "")
                {
                    searchString += "WITH ";
                }
                else
                {
                    searchString += " AND ";
                }
                


                // if the ID came in with dashes properly
                if (governmentId.Contains('-'))
                {
                    searchString += "SSN EQ " + Quote + governmentId + Quote;
                }
                else
                {
                    string usFormat = "";
                    string canadaFormat = "";
                    try
                    {
                        // if this throws, it doesn't have a dash and isn't numeric, so give up.
                        usFormat = String.Format("{0:000-00-0000}", Int32.Parse(governmentId));
                        canadaFormat = String.Format("{0:000-000-000}", Int32.Parse(governmentId));
                    }
                    catch (Exception)
                    {
                    }

                    searchString += " SSN EQ " + Quote + usFormat + Quote + " OR SSN EQ " + Quote + canadaFormat + Quote;
                }

            }

            // Birth date must match

            if (dateOfBirth != null)
            {
                if (searchString == "")
                {
                    searchString += "WITH ";
                }
                else
                {
                    searchString += " AND ";
                }

                DateTime dt = (DateTime) dateOfBirth;
                String dateOfBirthUnidata = UniDataFormatter.UnidataFormatDate(dt, intlParams.HostShortDateFormat, intlParams.HostDateDelimiter);
                searchString += "BIRTH.DATE EQ '" + dateOfBirthUnidata + "'";
            }

            // Former name will be ignored for now, it will come in with the request if research is required
            var results = await ExecuteSearchAsync(searchString, idList);

            // We didn't check the student ID for an exact match yet

            Int32 idNumeric;

            if (!string.IsNullOrEmpty(studentId) && Int32.TryParse(studentId, out idNumeric))
            {
                foreach (var s in results)
                {
                    Int32 studentIdNumeric;
                    if (Int32.TryParse(s.Id, out studentIdNumeric))
                    {
                        if (idNumeric == studentIdNumeric)
                        {
                            return new List<Domain.Student.Entities.Student>() {s};
                        }
                    }
                }
                return new List<Domain.Student.Entities.Student>();
            }

            return results;


        }

        private bool ContainsNumericEquivalent(IEnumerable<string> idList, string studentId)
        {
            int numericId;
            if (Int32.TryParse(studentId, out numericId))
            {
                foreach (var id in idList)
                {
                    int idFromList;
                    if (Int32.TryParse(id, out idFromList))
                    {
                        if (numericId == idFromList)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private async Task<IEnumerable<Domain.Student.Entities.Student>> ExecuteSearchAsync(string searchString, IEnumerable<string> limitingList)
        {
            string[] personhits;

            try
            {
                personhits = await DataReader.SelectAsync("PERSON", limitingList.ToArray(), searchString);
            }
            catch (PermissionsException pex)
            {
                throw pex;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("SECURITY"))
                {
                    logger.Error(e.Message);
                    throw new PermissionsException(e.Message);
                }
                else
                {
                    logger.Error(e.Message);
                    throw e;
                }
            }

            if (personhits == null || personhits.Length == 0)
            {
                throw new KeyNotFoundException("No student found matching these criteria");
            }

            // Then try to get STUDENT(s) with returned ID(s)
            try
            {
                return await GetAsync(personhits.AsEnumerable());
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Person found matching the supplied criteria has no student record");
            }


        }

        #endregion

        /// <summary>
        /// Reads the required data from Colleague and returns a Students Entity model.
        /// </summary>
        /// <param name="studentId">Colleague Person (student) id.</param>
        /// <returns>Student Entity if found in Colleague, null if the student does not exist in Colleague.</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> BuildStudentsAsync(IEnumerable<string> studentIds,
            Collection<Student.DataContracts.Students> studentData, Collection<Student.DataContracts.StudentPrograms> studentProgramData,
            Collection<Base.DataContracts.PersonSt> personStData, Collection<Student.DataContracts.StudentAdvisement> studentAdvisementData,
            Collection<Base.DataContracts.Person> personData, Collection<Student.DataContracts.FinAid> financialAidData,
            Collection<Base.DataContracts.ForeignPerson> foreignPersonData,
            Collection<Student.DataContracts.Applicants> applicantsData,
            Collection<Base.DataContracts.InstitutionsAttend> instAttendData,
            Collection<Base.DataContracts.Institutions> institutionData,
            Collection<Base.DataContracts.InstitutionsAttend> institutionAttendData,
            Collection<Student.DataContracts.StudentAcadLevels> studentAcadLevelsData,
            bool inheritFromPerson, bool getDegreePlan, Term termData, bool filterAdvisorsByTerm, IEnumerable<CitizenshipStatus> citizenshipStatusData, bool filterEndedAdvisements)
        {
            var studentResults = new List<Ellucian.Colleague.Domain.Student.Entities.Student>();
            bool error = false;
            List<string> studentIdsNotFound = new List<string>();
            foreach (var studentId in studentIds)
            {
                try
                {
                    Ellucian.Colleague.Domain.Student.Entities.Student studentEntity = null;

                    // Get students data contract
                    Students students = studentData.Where(s => s.Recordkey == studentId).FirstOrDefault();
                    // Get person data contract
                    Base.DataContracts.Person personContract = personData.Where(p => p.Recordkey == studentId).FirstOrDefault();
                    if (students != null)
                    {
                        // Get student programs
                        // Also evaluate student program admit status to see if student is a transfer student
                        var studentIsTransfer = false;

                        List<string> programIds = new List<string>();
                        if (students.StuAcadPrograms != null)
                        {
                            foreach (var acadProgramId in students.StuAcadPrograms)
                            {
                                Student.DataContracts.StudentPrograms studentProgram = studentProgramData.Where(sp => sp.Recordkey == (studentId + "*" + acadProgramId)).FirstOrDefault();
                                if (studentProgram != null)
                                {
                                    // If the program is withdrawn or dropped/changed-mind, skip it.
                                    if (studentProgram.StprStatus.Count > 0)
                                    {
                                        var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == studentProgram.StprStatus.ElementAt(0)).FirstOrDefault();
                                        if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "4" || codeAssoc.ValActionCode1AssocMember == "5"))
                                        {
                                            continue;
                                        }
                                    }

                                    // If student program has ended, skip it.
                                    if (studentProgram.StprEndDate != null && studentProgram.StprEndDate.Count > 0 && studentProgram.StprEndDate.ElementAt(0) < DateTime.Today)
                                    {
                                        continue;
                                    }

                                    // If the program doesn't have a start date, skip it.
                                    if (studentProgram.StprStartDate != null && studentProgram.StprStartDate.Count == 0)
                                    {
                                        continue;
                                    }

                                    // STUDENT.PROGRAMS key is multi-part.  Only save the program portion (second part) to the Student domain entity
                                    programIds.Add(studentProgram.Recordkey.Split('*')[1]);

                                    //// If student program has an admit status, check if student is a
                                    //// transfer student.
                                    if (!string.IsNullOrEmpty(studentProgram.StprAdmitStatus))
                                    {
                                        // Check the transfer flag on the admit status.  If set to "Y", tag
                                        // the student as a transfer student.
                                        var tempAdmitStatusTransferFlag = (await GetAdmitStatusesAsync()).Where(ast => ast.Code == studentProgram.StprAdmitStatus).Select(ast => ast.TransferFlag.ToUpper()).FirstOrDefault();
                                        if (tempAdmitStatusTransferFlag == "Y")
                                        {
                                            studentIsTransfer = true;
                                        }
                                    }
                                }
                            }
                        }

                        // from PERSON.ST record

                        List<string> academicCreditIds = new List<string>();
                        Ellucian.Colleague.Data.Base.DataContracts.PersonSt personSt = personStData.Where(pst => pst.Recordkey == studentId).FirstOrDefault();
                        // get the student acad cred list
                        if (personSt != null && personSt.PstStudentAcadCred != null && personSt.PstStudentAcadCred.Count > 0)
                        {
                            foreach (string pstStudentAcadCred in personSt.PstStudentAcadCred)
                            {
                                if (!string.IsNullOrEmpty(pstStudentAcadCred))
                                {
                                    academicCreditIds.Add(pstStudentAcadCred);
                                }
                            }
                        }

                        // Use select to get the student's degree plan id - if present. 
                        // (Can't bulk read degree plan data because it is in the planning module not student module.)
                        int? degreePlanId = null;
                        if (getDegreePlan)
                        {
                            string searchString = "DP.STUDENT.ID EQ '" + students.Recordkey + "'";
                            string[] studentPlans = await DataReader.SelectAsync("DEGREE_PLAN", searchString);
                            if (studentPlans.Count() > 0)
                            {
                                // Sorting results in the event multiple plans are returned so that we always get the plan with the  smallest Id.
                                IEnumerable<int> studentPlanIds = studentPlans.Select(int.Parse);
                                degreePlanId = studentPlanIds.OrderBy(s => s).FirstOrDefault();
                            }
                        }
                        StwebDefaults stWebDefaults = await GetStwebDefaultsAsync();
                        NameAddressHierarchy hierarchy = null;
                        if (stWebDefaults != null && !string.IsNullOrEmpty(stWebDefaults.StwebDisplayNameHierarchy))
                        {
                            try
                            {
                                hierarchy = await GetCachedNameAddressHierarchyAsync(stWebDefaults.StwebDisplayNameHierarchy);
                            }
                            catch (Exception)
                            {
                                logger.Info("Unable to find name address hierarchy with ID " + stWebDefaults.StwebDisplayNameHierarchy + ". Not calculating hierarchy name.");

                            }

                        }

                        if (inheritFromPerson)
                        {
                            // Now that we have the needed data, create students entity model (pulling it from cache, if available)
                            var cachedStudentEntity = await GetAsync<Ellucian.Colleague.Domain.Student.Entities.Student>(students.Recordkey,
                                person => new Ellucian.Colleague.Domain.Student.Entities.Student(person.Recordkey, person.LastName,
                                    degreePlanId, programIds, academicCreditIds));

                            // create a new student entity, using a combo of the cached data and any new data that might have read above
                            // (acad creds is an example - these can change faster than the cache expires)
                            studentEntity = new Domain.Student.Entities.Student(students.RecordGuid,
                                students.Recordkey, cachedStudentEntity.LastName,
                                degreePlanId, programIds, academicCreditIds, personContract.PrivacyFlag);
                            studentEntity.MiddleName = cachedStudentEntity.MiddleName;
                            studentEntity.FirstName = cachedStudentEntity.FirstName;
                            studentEntity.PreferredAddress = cachedStudentEntity.PreferredAddress;
                            studentEntity.PreferredName = cachedStudentEntity.PreferredName;
                            studentEntity.Gender = cachedStudentEntity.Gender;
                            studentEntity.BirthDate = cachedStudentEntity.BirthDate;
                            studentEntity.Prefix = cachedStudentEntity.Prefix;
                            studentEntity.Suffix = cachedStudentEntity.Suffix;
                            studentEntity.ChosenFirstName = cachedStudentEntity.ChosenFirstName;
                            studentEntity.ChosenMiddleName = cachedStudentEntity.ChosenMiddleName;
                            studentEntity.ChosenLastName = cachedStudentEntity.ChosenLastName;
                            studentEntity.BirthNameFirst = cachedStudentEntity.BirthNameFirst;
                            studentEntity.BirthNameMiddle = cachedStudentEntity.BirthNameMiddle;
                            studentEntity.BirthNameLast = cachedStudentEntity.BirthNameLast;
                            studentEntity.MailLabelNameOverride = cachedStudentEntity.MailLabelNameOverride;
                            studentEntity.PreferredNameOverride = cachedStudentEntity.PreferredNameOverride;
                            studentEntity.PersonalPronounCode = cachedStudentEntity.PersonalPronounCode;
                            if (cachedStudentEntity.FormattedNames != null && cachedStudentEntity.FormattedNames.Any())
                            {
                                foreach (var fname in cachedStudentEntity.FormattedNames)
                                {
                                    studentEntity.AddFormattedName(fname.Type, fname.Name);
                                }
                            }
                            foreach (var email in cachedStudentEntity.EmailAddresses)
                            {
                                studentEntity.AddEmailAddress(email);
                            }
                            if (hierarchy != null)
                            {
                                // Student Display Name Hierarchy parameters from the SPWP form are used to
                                // calculate the PersonDisplay name for a student entity.
                                // MUST PASS THE cached entity which has the fully filled out PersonBase with all the name fields.
                                studentEntity.PersonDisplayName = PersonNameService.GetHierarchyName(cachedStudentEntity, hierarchy);
                            }                         
                        }
                        else
                        {
                            studentEntity = new Domain.Student.Entities.Student(students.RecordGuid, students.Recordkey, personContract.LastName,
                                degreePlanId, programIds, academicCreditIds, personContract.PrivacyFlag);
                            studentEntity.MiddleName = personContract.MiddleName;
                            studentEntity.FirstName = personContract.FirstName;
                            studentEntity.Prefix = personContract.Prefix;
                            studentEntity.Suffix = personContract.Suffix;
                            studentEntity.ChosenFirstName = personContract.PersonChosenFirstName;
                            studentEntity.ChosenMiddleName = personContract.PersonChosenMiddleName;
                            studentEntity.ChosenLastName = personContract.PersonChosenLastName;
                            studentEntity.BirthNameFirst = personContract.BirthNameFirst;
                            studentEntity.BirthNameMiddle = personContract.BirthNameMiddle;
                            studentEntity.BirthNameLast = personContract.BirthNameLast;
                            studentEntity.PersonalPronounCode = personContract.PersonalPronoun;

                            // Take the mail label name or preferred name override values from the data contract (which could be either a name or a coded entry) and 
                            // convert it into simply a name override - where the coded entries are convered into their actual results.
                            // In case of mail label name, it defaults to the preferred name override information unless it has its own.
                            string mailLabelOverride = personContract.PersonMailLabel != null && personContract.PersonMailLabel.Any() ? string.Join(" ", personContract.PersonMailLabel.ToArray()) : personContract.PreferredName;
                            studentEntity.MailLabelNameOverride = FormalNameFormat(mailLabelOverride, personContract.Prefix, personContract.FirstName, personContract.MiddleName, personContract.LastName, personContract.Suffix);
                            studentEntity.PreferredNameOverride = FormalNameFormat(personContract.PreferredName, personContract.Prefix, personContract.FirstName, personContract.MiddleName, personContract.LastName, personContract.Suffix);
                            if (personContract.PFormatEntityAssociation != null && personContract.PFormatEntityAssociation.Any())
                            {
                                foreach (var pFormat in personContract.PFormatEntityAssociation)
                                {
                                    try
                                    {
                                        studentEntity.AddFormattedName(pFormat.PersonFormattedNameTypesAssocMember, pFormat.PersonFormattedNamesAssocMember);
                                    }
                                    catch (Exception)
                                    {
                                        logger.Info("Unable to add formatted name to person " + personContract.Recordkey + " with type " + pFormat.PersonFormattedNameTypesAssocMember + " and name " + pFormat.PersonFormattedNamesAssocMember);
                                    }
                                }
                            }
                            if (hierarchy != null)
                            {
                                // Student Display Name Hierarchy parameters from the SPWP form are used to
                                // calculate the PersonDisplay name for a student entity.   
                                studentEntity.PersonDisplayName = PersonNameService.GetHierarchyName(studentEntity, hierarchy);
                            }   
                        }

                        // Add any student advisors
                        if (personSt != null && personSt.PstAdvisement != null && personSt.PstAdvisement.Count > 0)
                        {
                            // Gather this student's advisement items.
                            if (studentAdvisementData != null)
                            {
                                IEnumerable<StudentAdvisement> advisorAssignments = studentAdvisementData.Where(sa => sa.StadStudent == students.Recordkey);

                                if (advisorAssignments != null && advisorAssignments.Count() > 0)
                                {
                                    foreach (var studentAdvisement in advisorAssignments)
                                    {
                                        var startDate = studentAdvisement.StadStartDate;
                                        var endDate = studentAdvisement.StadEndDate;
                                        bool addAdvisorAssignment = false;
                                        if (termData != null && filterAdvisorsByTerm == true)
                                        {
                                            var termStartDate = termData.StartDate;
                                            var termEndDate = termData.EndDate;
                                            //
                                            // Add advisor assignment if it intersects with term:
                                            // - advisor assignment start date is within term start and end
                                            // - advisor assignment end date exists and 
                                            //      term start is within advisor assignment start and end
                                            // - advisor assignment end date does not exist and 
                                            //      term starts after advisor assignment starts
                                            //
                                            if ((termStartDate.CompareTo(startDate) <= 0 && termEndDate.CompareTo(startDate) >= 0) ||
                                            (termStartDate.CompareTo(startDate) >= 0 && (endDate != null && termStartDate.CompareTo(endDate) <= 0)) ||
                                            (termStartDate.CompareTo(startDate) >= 0 && (endDate == null)))
                                            {
                                                if (filterEndedAdvisements == true)
                                                {
                                                    if ((startDate == null || startDate <= DateTime.Now) && (endDate == null || endDate > DateTime.Now))
                                                    {
                                                        addAdvisorAssignment = true;
                                                    }
                                                }
                                                else
                                                {
                                                addAdvisorAssignment = true;
                                            }
                                        }
                                        }
                                        else
                                        {
                                            // Add active advisor assignments
                                            if ((startDate == null || startDate <= DateTime.Now) && (endDate == null || endDate > DateTime.Now))
                                            {
                                                addAdvisorAssignment = true;
                                            }
                                        }
                                        if (addAdvisorAssignment == true)
                                        {
                                            try
                                            {
                                                studentEntity.AddAdvisor(studentAdvisement.StadFaculty);
                                            }
                                            catch (Exception e)
                                            {
                                                var message = "Unable to add advisor '" + studentAdvisement.StadFaculty
                                                    + "' for student '" + studentAdvisement.StadStudent + "' for type '"
                                                    + studentAdvisement.StadType + "'";
                                                logger.Warn(e, message);
                                            }
                                            try
                                            {
                                                studentEntity.AddAdvisement(studentAdvisement.StadFaculty, studentAdvisement.StadStartDate, studentAdvisement.StadEndDate, studentAdvisement.StadType);
                                            }
                                            catch (Exception e)
                                            {
                                                var message = "Unable to add advisement for advisor'" + studentAdvisement.StadFaculty
                                                    + "' for student '" + studentAdvisement.StadStudent + "' for type '"
                                                    + studentAdvisement.StadType + "'";
                                                logger.Warn(e, message);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Add any Registration Priority Ids now that studentEntity has been created
                        if (students.StuRegPriorities != null && students.StuRegPriorities.Count > 0)
                        {
                            foreach (string stuRegPriorityId in students.StuRegPriorities)
                            {
                                try
                                {
                                    studentEntity.AddRegistrationPriority(stuRegPriorityId);
                                }
                                catch
                                {
                                    // Don't bother logging if priority ID is null or this is a duplicate
                                }
                            }
                        }

                        // Add student home locations

                        if (students.StuHomeLocation != null && students.StuHomeLocation.Count > 0)
                        {
                            bool foundPrimary = false;
                            foreach (var sl in students.StuHomeLocationsEntityAssociation)
                            {
                                try
                                {
                                    var location = sl.StuHomeLocationAssocMember;
                                    var startDate = sl.StuHomeLocationStartDateAssocMember;
                                    var endDate = sl.StuHomeLocationEndDateAssocMember;
                                    bool isPrimary = false;
                                    if (foundPrimary == false)
                                    {
                                        if (termData != null)
                                        {
                                            var termStartDate = termData.StartDate;
                                            var termEndDate = termData.EndDate;
                                            //
                                            // Primary home location for first record found where any
                                            // of the following are true to indicate home location
                                            // intersection with term dates:
                                            // - home location start date is within term start and end
                                            // - home location end date exists and term start is within home location start and end
                                            // - home location end date does not exist and term starts after home location starts
                                            //
                                            if ((termStartDate.CompareTo(startDate) <= 0 && termEndDate.CompareTo(startDate) >= 0) ||
                                            (termStartDate.CompareTo(startDate) >= 0 && (endDate != null && termStartDate.CompareTo(endDate) <= 0)) ||
                                            (termStartDate.CompareTo(startDate) >= 0 && (endDate == null)))
                                            {
                                                isPrimary = true;
                                                foundPrimary = true;
                                            }
                                        }
                                    }
                                    studentEntity.AddLocation(location, startDate, endDate, isPrimary);
                                }
                                catch (Exception e)
                                {
                                    LogDataError("Students", students.Recordkey, sl, e);
                                }
                            }
                        }

                        // Add any Student Restriction Ids
                        if (personSt != null && personSt.PstRestrictions != null && personSt.PstRestrictions.Count > 0)
                        {
                            foreach (string stuRestrId in personSt.PstRestrictions)
                            {
                                try
                                {
                                    studentEntity.AddStudentRestriction(stuRestrId);
                                }
                                catch
                                {
                                    // Don't bother logging if student Id is null or this is a duplicate
                                }
                            }
                        }

                        // Add educational Goal
                        try
                        {
                            if (personSt != null && personSt.EducGoalsEntityAssociation != null && personSt.EducGoalsEntityAssociation.Count() > 0)
                            {
                                // Get the goal with the latest associated date
                                var currGoal = personSt.EducGoalsEntityAssociation.OrderByDescending(g => g.PstEducGoalsChgdatesAssocMember).Select(g => g.PstEducGoalsAssocMember).FirstOrDefault();
                                // Translate to the external representation string
                                studentEntity.EducationalGoal = (await GetEducationalGoalsAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == currGoal).Select(v => v.ValExternalRepresentationAssocMember).FirstOrDefault();
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn(e, string.Format("Unable to determine the educational goal for '{0}' ", students.Recordkey));
                        }


                        // Since preferred email is not currently part of PERSON, use the person data contract to get emails.
                        if (personContract != null)
                        {
                            if (personContract.PeopleEmailEntityAssociation != null && personContract.PeopleEmailEntityAssociation.Count > 0)
                            {

                                foreach (var emailData in personContract.PeopleEmailEntityAssociation)
                                {
                                    try
                                    {
                                        EmailAddress eAddress = new EmailAddress(emailData.PersonEmailAddressesAssocMember, emailData.PersonEmailTypesAssocMember) { IsPreferred = !string.IsNullOrEmpty(emailData.PersonPreferredEmailAssocMember) && emailData.PersonPreferredEmailAssocMember.ToUpper() == "Y" };

                                        // The first email found will become the student's preferred email address unless any subsequent emails in the list
                                        // have the preferred email address flag set to Y - then the last one with that flag becomes the preferred email address.
                                        // In Colleague - only one is supposed to be flagged as "preferred" - if none are flagged the first one becomes the preferred.
                                        if (studentEntity.PreferredEmailAddress == null || emailData.PersonPreferredEmailAssocMember == "Y")
                                        {
                                            studentEntity.AddEmailAddress(eAddress);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn(e, string.Format("Unable to determine the Email Address for Student: '{0}'", students.Recordkey));
                                    }
                                }
                            }
                            // Add data for ESS Student updates
                            // srm - 11/04/2013
                            studentEntity.Prefix = personContract.Prefix;
                            studentEntity.Suffix = personContract.Suffix;
                            studentEntity.BirthDate = personContract.BirthDate;
                            studentEntity.Gender = personContract.Gender;
                            studentEntity.GovernmentId = personContract.Ssn;
                            studentEntity.RaceCodes = personContract.PerRaces;
                            studentEntity.EthnicCodes = personContract.PerEthnics;

                            // Add isConfidential flag based on Privacy Code;
                            studentEntity.IsConfidential = false;
                            if (!string.IsNullOrEmpty(personContract.PrivacyFlag))
                            {
                                studentEntity.IsConfidential = true;
                            }

                            // Sort Student Types by Date and take the newest
                            if (students.StuTypes != null && students.StuTypes.Count > 0)
                            {
                                studentEntity.StudentTypeCode = students.StuTypeInfoEntityAssociation.OrderByDescending(g => g.StuTypeDatesAssocMember).Select(g => g.StuTypesAssocMember).FirstOrDefault();
                            }
                            var studentTypeInfo = new List<StudentTypeInfo>();
                            if (students.StuTypeInfoEntityAssociation != null && students.StuTypeInfoEntityAssociation.Any())
                            {
                                foreach (var studentType in students.StuTypeInfoEntityAssociation)
                                {
                                    var type = new StudentTypeInfo(studentType.StuTypesAssocMember, studentType.StuTypeDatesAssocMember);
                                    studentTypeInfo.Add(type);
                                }
                            }
                            studentEntity.StudentTypeInfo = studentTypeInfo;


                            // Build Ethnicities Data element from Ethnic Codes and Race Codes
                            var ethnicOrigins = new List<EthnicOrigin>();
                            if (personContract.PerEthnics != null && personContract.PerEthnics.Count > 0)
                            {
                                foreach (var ethnicCode in personContract.PerEthnics)
                                {
                                    var ethnicOrigin = EthnicOrigin.Unknown;
                                    var codeAssoc = (await GetEthnicitiesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == ethnicCode).FirstOrDefault();
                                    if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "H")
                                    {
                                        ethnicOrigin = EthnicOrigin.HispanicOrLatino;
                                        ethnicOrigins.Add(ethnicOrigin);
                                    }
                                }
                                foreach (var raceCode in personContract.PerRaces)
                                {
                                    var ethnicOrigin = EthnicOrigin.Unknown;
                                    var codeAssoc = (await GetRacesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == raceCode).FirstOrDefault();
                                    if (codeAssoc != null)
                                    {
                                        switch (codeAssoc.ValActionCode1AssocMember)
                                        {
                                            case ("1"):
                                            {
                                                ethnicOrigin = EthnicOrigin.AmericanIndianOrAlaskanNative;
                                                break;
                                        }
                                            case ("2"):
                                            {
                                                ethnicOrigin = EthnicOrigin.Asian;
                                                break;
                                            }
                                            case ("3"):
                                            {
                                                ethnicOrigin = EthnicOrigin.BlackOrAfricanAmerican;
                                                break;
                                            }
                                            case ("4"):
                                            {
                                                ethnicOrigin = EthnicOrigin.NativeHawaiianOrOtherPacificIslander;
                                                break;
                                            }
                                            case ("5"):
                                            {
                                                ethnicOrigin = EthnicOrigin.White;
                                                break;
                                            }
                                        }
                                        if (ethnicOrigin != EthnicOrigin.Unknown)
                                        {
                                            ethnicOrigins.Add(ethnicOrigin);
                                        }
                                    }
                                    else
                                    {
                                        logger.Error(string.Format("Race code {0} not found for student {1}", raceCode, students.Recordkey));
                                    }
                                }
                            }
                            studentEntity.Ethnicities = ethnicOrigins;

                            // Build Marital Status field       
                            if (!string.IsNullOrEmpty(personContract.MaritalStatus))
                            {
                                var maritalStatusCode = (await GetMaritalStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == personContract.MaritalStatus).FirstOrDefault();
                                if (maritalStatusCode != null)
                                {
                                    switch (maritalStatusCode.ValActionCode1AssocMember)
                                    {
                                        case ("1"):
                                        {
                                            studentEntity.MaritalStatus = MaritalState.Single;
                                            break;
                                        }
                                        case ("2"):
                                        {
                                            studentEntity.MaritalStatus = MaritalState.Married;
                                            break;
                                        }
                                        case ("3"):
                                        {
                                            studentEntity.MaritalStatus = MaritalState.Divorced;
                                            break;
                                        }
                                        case ("4"):
                                        {
                                            studentEntity.MaritalStatus = MaritalState.Widowed;
                                            break;
                                        }
                                    }
                                }
                            }

                            studentEntity.AcademicLevelCodes = students.StuAcadLevels;
                            studentEntity.ClassLevelCodes = new List<string>();
                            studentEntity.AdmitTerms = new List<string>();
                            studentEntity.IsLegacyStudent = await IsStudentLegacyAsync(personContract, personData);
                            studentEntity.IsInternationalStudent = IsStudentInternational(studentId, foreignPersonData, personData, citizenshipStatusData);
                            studentEntity.IsFirstGenerationStudent = await IsStudentFirstGenerationAsync(studentId, applicantsData);
                            // We're now reading Student Acad Levels for other information.
                            // Populate Admit Terms later in the process when dealing with Student Acad Levels.
                            // studentEntity.AdmitTerms = GetAdmitTerm(studentId, students.StuAcadLevels);

                            // Cannot be a first generation student and have had your parent attend this institution
                            if (studentEntity.IsLegacyStudent == true && studentEntity.IsFirstGenerationStudent == true)
                            {
                                studentEntity.IsFirstGenerationStudent = false;
                            }

                            // Get the Insitution Types for High School and update High School GPA
                            if (personContract.PersonInstitutionsAttend != null && personContract.PersonInstitutionsAttend.Count > 0)
                            {
                                foreach (var instId in personContract.PersonInstitutionsAttend)
                                {
                                    Base.DataContracts.Institutions institutionContract = institutionData.Where(p => p.Recordkey == instId).FirstOrDefault();

                                    if (institutionContract != null && institutionContract.InstType != null)
                                    {
                                        var codeAssoc = (await GetInstitutionTypesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == institutionContract.InstType).FirstOrDefault();
                                        if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "H"))
                                        {
                                            var instAttendId = studentId + "*" + instId;
                                            Base.DataContracts.InstitutionsAttend instAttend = institutionAttendData.Where(p => p.Recordkey == instAttendId).FirstOrDefault();

                                            if (instAttend != null)
                                            {
                                                var lastYearAttend = instAttend.YearsAttendedEntityAssociation.OrderByDescending(i => i.InstaYearAttendEndAssocMember).Select(i => i.InstaYearAttendEndAssocMember).FirstOrDefault();
                                                studentEntity.AddHighSchoolGpa(instId, instAttend.InstaExtGpa, lastYearAttend);
                                            }
                                        }
                                    }
                                }
                            }

                            // Add Residency Status
                            try
                            {
                                if (students != null && students.StuResidenciesEntityAssociation != null && students.StuResidenciesEntityAssociation.Count() > 0)
                                {
                                    // Get the goal with the latest associated date
                                    var currStatus = students.StuResidenciesEntityAssociation.OrderByDescending(r => r.StuResidencyStatusDateAssocMember).Select(r => r.StuResidencyStatusAssocMember).FirstOrDefault();
                                    // Translate to the external representation string
                                    if (!string.IsNullOrEmpty(currStatus))
                                    {
                                        studentEntity.ResidencyStatus = (await GetResidencyStatusesAsync()).Where(r => r.Code == currStatus).Select(r => r.Description).FirstOrDefault();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e, string.Format("Unable to determine the Residency Status for '{0}'", students.Recordkey));
                            }

                            // Add (all) student residencies

                            if (students != null && students.StuResidenciesEntityAssociation != null && students.StuResidenciesEntityAssociation.Count() > 0)
                            {
                                foreach (var sr in students.StuResidenciesEntityAssociation)
                                {
                                    try
                                    {
                                        var residency = sr.StuResidencyStatusAssocMember;
                                        var date = sr.StuResidencyStatusDateAssocMember;
                                        studentEntity.AddResidency(residency, date);
                                    }
                                    catch (Exception e)
                                    {
                                        LogDataError("Students", students.Recordkey, sr, e);
                                    }
                                }
                            }

                            // Add Class Levels from Student Acad Levels
                            // Also evaluate student academic level admit statuses to see if student
                            // is a transfer student                                            
                            if (students != null && students.StuAcadLevels != null && students.StuAcadLevels.Count > 0)
                            {
                                // Gather this student's academic level data.
                                if (studentAcadLevelsData != null)
                                {
                                    foreach (var acadLevel in students.StuAcadLevels)
                                    {
                                        string studentAcadLevelKey = studentId + "*" + acadLevel;
                                        StudentAcadLevels studentAcadLevel = studentAcadLevelsData.Where(sa => sa.Recordkey == studentAcadLevelKey).FirstOrDefault();
                                        if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaClass))
                                        {
                                            studentEntity.ClassLevelCodes.Add(studentAcadLevel.StaClass);
                                        }
                                        if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaStartTerm))
                                        {
                                            studentEntity.AdmitTerms.Add(studentAcadLevel.StaStartTerm);
                                        }
                                        if (studentAcadLevel != null && studentEntity.StudentAcademicLevels != null)
                                        {
                                            bool isActive = false;

                                            if (termData != null)
                                            {
                                                var termStartDate = termData.StartDate;
                                                var termEndDate = termData.EndDate;
                                                //
                                                // Set a StudentAcademicLevel's IsActive to true if it's dates
                                                // intersect with the dates of an optional incoming term:
                                                // - student acad level start date is within term start and end
                                                // - student acad level end date exists and term start is within student acad level start and end
                                                // - student acad level end date does not exist and term starts after student aad level starts
                                                //
                                                if ((termStartDate.CompareTo(studentAcadLevel.StaStartDate) <= 0 && termEndDate.CompareTo(studentAcadLevel.StaStartDate) >= 0) ||
                                                (termStartDate.CompareTo(studentAcadLevel.StaStartDate) >= 0 && (studentAcadLevel.StaEndDate != null && termStartDate.CompareTo(studentAcadLevel.StaEndDate) <= 0)) ||
                                                (termStartDate.CompareTo(studentAcadLevel.StaStartDate) >= 0 && (studentAcadLevel.StaEndDate == null)))
                                                {
                                                    isActive = true;
                                                }
                                            }

                                            StudentAcademicLevel levelDomain = new StudentAcademicLevel(acadLevel, studentAcadLevel.StaAdmitStatus, studentAcadLevel.StaClass, studentAcadLevel.StaStartTerm, studentAcadLevel.StaStudentAcadCred, isActive);
                                            studentEntity.StudentAcademicLevels.Add(levelDomain);
                                        }
                                        // If we haven't already identified this student as a transfer student via
                                        // their student program admit statuses, check their student academic level
                                        // admit statuses
                                        if (studentIsTransfer != true)
                                        {
                                            if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaAdmitStatus))
                                            {
                                                {
                                                    // Check the transfer flag on the admit status.  If set to "Y", tag
                                                    // the student as a transfer student.        
                                                    var tempAdmitStatusTransferFlag = (await GetAdmitStatusesAsync()).Where(ast => ast.Code == studentAcadLevel.StaAdmitStatus).Select(ast => ast.TransferFlag.ToUpper()).FirstOrDefault();
                                                    if (tempAdmitStatusTransferFlag == "Y")
                                                    {
                                                        studentIsTransfer = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // If studentIsTransfer set from academic program or academic level, set it for
                            // for the student Entity.
                            studentEntity.IsTransfer = studentIsTransfer;


                            //Add the financial aid counselor id if it exists
                            if (financialAidData != null && financialAidData.Count() > 0)
                            {
                                var financialAidStudentRecord = financialAidData.FirstOrDefault(f => f != null && f.Recordkey == studentEntity.Id);
                                if (financialAidStudentRecord != null &&
                                    financialAidStudentRecord.FaCounselorsEntityAssociation != null &&
                                    financialAidStudentRecord.FaCounselorsEntityAssociation.Count() > 0)
                                {
                                    foreach (var faCounselorEntity in financialAidStudentRecord.FaCounselorsEntityAssociation)
                                    {
                                        if (
                                            (!faCounselorEntity.FaCounselorEndDateAssocMember.HasValue ||
                                            DateTime.Today <= faCounselorEntity.FaCounselorEndDateAssocMember.Value) &&
                                            (!faCounselorEntity.FaCounselorStartDateAssocMember.HasValue ||
                                            DateTime.Today >= faCounselorEntity.FaCounselorStartDateAssocMember.Value)
                                           )
                                        {
                                            studentEntity.FinancialAidCounselorId = faCounselorEntity.FaCounselorAssocMember;
                                            break;
                                        }
                                    }
                                }
                            }
                            // Add the student entity to the results which will be returned
                            studentResults.Add(studentEntity);
                        }
                        else
                        {
                            studentIdsNotFound.Add(studentId);
                        }
                    }
                    else
                    {
                        studentIdsNotFound.Add(studentId);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Failed to build student {0}", studentId));
                    logger.Error(e.GetBaseException().Message);
                    logger.Error(e.GetBaseException().StackTrace);
                    error = true;
                }
            }
            if (studentIdsNotFound.Count() > 0)
            {
                // log any ids that were not found.
                var errorMessage = string.Format("The following student Ids were requested but not found: '{0}'", string.Join(",", studentIdsNotFound.ToArray()));
                logger.Info(errorMessage);
            }
            return studentResults;
        }

        /// <summary>
        /// Parses the PESC transcript request into Colleague-friendly variables and calls the transaction to send in the request.  Receives a PESC XML transcript response
        /// </summary>
        /// <param name="order">PESC XML transcript request</param>
        /// <returns>XML response as a string</returns>
        public async Task<string> OrderTranscriptAsync(Domain.Student.Entities.Transcripts.TranscriptRequest order)
        {
            // Get all of the bits of the request out of the domain objects and feed them into the transaction objects.
            ProcessTranscriptOrderRequest request = new ProcessTranscriptOrderRequest()
            {
                AttachmentSpclInstrs = String.Empty,
                AttachmentUrl = String.Empty,
                AttendedSchools = new List<AttendedSchools>(),
                DeliveryMethod = String.Empty,
                ElectronicDelivery = String.Empty,
                HoldForDegree = String.Empty,
                HoldForTerm = String.Empty,
                OrderNo = String.Empty,
                OrderSource = String.Empty,
                ProcessCode = String.Empty,
                RcptAddressLines = new List<string>(),
                RcptAttentionLine = String.Empty,
                RcptCeebact = String.Empty,
                RcptCity = String.Empty,
                RcptCountryCode = String.Empty,
                RcptEmailAddress = String.Empty,
                RcptFaxNumber = String.Empty,
                RcptOpeid = String.Empty,
                RcptOrgName = String.Empty,
                RcptPhoneNumber = String.Empty,
                RcptPostalCode = String.Empty,
                RcptStateProvinceCode = String.Empty,
                SpecialInstrs = String.Empty,
                StampSealInd = false,
                StuAddressLines = new List<string>(),
                StuAltFirstName = String.Empty,
                StuAltLastName = String.Empty,
                StuAltMiddleName = String.Empty,
                StuBirthDate = DateTime.Parse("1/1/1900"),
                StuCity = String.Empty,
                StuCountryCode = String.Empty,
                Student = String.Empty,
                StuEmailAddress = String.Empty,
                StuFirstName = String.Empty,
                StuLastName = String.Empty,
                StuMiddleName = String.Empty,
                StuPhoneNumber = String.Empty,
                StuPostalCode = String.Empty,
                StuSsn = String.Empty,
                StuStateProvinceCode = String.Empty,
                StuUnverifiedId = String.Empty,
                TranscriptCopies = 0,
                TranscriptPurpose = String.Empty,
                TranscriptType = String.Empty,
                RushProcessingFlag = false
            };

            if (order.UserDefinedExtensions.AttachmentFlag != null && (order.UserDefinedExtensions.AttachmentFlag.ToUpper() == "T" || order.UserDefinedExtensions.AttachmentFlag.ToUpper() == "TRUE"))
            {
                request.AttachmentSpclInstrs = order.UserDefinedExtensions.AttachmentSpecialInstructions ?? "";
                request.AttachmentUrl = order.UserDefinedExtensions.AttachmentURL ?? "";
            }
            request.AttendedSchools = new List<AttendedSchools>();

            AttendedSchools atsc = new AttendedSchools()
            {
                StuSchAcadAwardDate = String.Empty,
                StuSchAcadAwardTitle = String.Empty,
                StuSchOrgName = String.Empty,
                StuSchExitDate = null,
                StuSchEnrollDate = null,
                StuSchDtlOrgName = String.Empty,
                StuSchDtlEndYr = String.Empty,
                StuSchDtlBeginYr = String.Empty,
                StuSchCurrEnrFlag = false,
                StuSchOpeid = String.Empty
            };

            string dates = "";
            string titles = "";

            try
            {
                if (order.Request.RequestedStudent.Attendance.AcademicAwardsReported.Count() > 0)
                {
                    foreach (var award in order.Request.RequestedStudent.Attendance.AcademicAwardsReported)
                    {
                        dates += award.AcademicAwardDate.ToString("d", CultureInfo.CreateSpecificCulture("en-US")) + "|";
                        titles += award.AcademicAwardTitle + "|";
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
            ;

            atsc.StuSchAcadAwardDate = dates;
            atsc.StuSchAcadAwardTitle = titles;
            try
            {
                atsc.StuSchCurrEnrFlag = (order.Request.RequestedStudent.Attendance.CurrentEnrollmentIndicator != null && order.Request.RequestedStudent.Attendance.CurrentEnrollmentIndicator.ToUpper() == "TRUE");
            }
            catch (NullReferenceException)
            {
            }
            ;
            try
            {
                atsc.StuSchOpeid = order.Request.RequestedStudent.Attendance.School.OPEID ?? "";
            }
            catch (NullReferenceException)
            {
            }
            ;



            string progname = "";
            string beginyear = "";
            string endyear = "";
            try
            {
                if (order.UserDefinedExtensions.EnrollmentDetail.Count() > 0)
                {
                    foreach (var det in order.UserDefinedExtensions.EnrollmentDetail)
                    {
                        beginyear += det.BeginYear + "|";
                        endyear += det.EndYear + "|";
                        progname += det.NameOfProgram + "|";
                    }
                }

            }
            catch (NullReferenceException)
            {
            }
            ;
            atsc.StuSchDtlBeginYr = beginyear;
            atsc.StuSchDtlEndYr = endyear;
            atsc.StuSchDtlOrgName = progname;
            atsc.StuSchEnrollDate = order.Request.RequestedStudent.Attendance.EnrollDate;
            atsc.StuSchExitDate = order.Request.RequestedStudent.Attendance.ExitDate;

            try
            {
                atsc.StuSchOrgName = order.Request.RequestedStudent.Attendance.School.OrganizationName ?? "";
            }
            catch (NullReferenceException)
            {
                atsc.StuSchOrgName = "";
            }

            request.AttendedSchools.Add(atsc);
            try
            {
                request.DeliveryMethod = order.Request.Recipient.DeliveryMethod.ToString();
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Missing Field DeliveryMethod");
            }

            try
            {
                request.ElectronicDelivery = order.Request.Recipient.ElectronicDelivery.ElectronicFormat;
            }
            catch (NullReferenceException)
            {
                if (!string.IsNullOrEmpty(request.DeliveryMethod) && order.Request.Recipient.DeliveryMethod == DeliveryMethod.Electronic)
                {
                    throw new NullReferenceException("DeliveryMethod set to electronic, but ElectronicDelivery value not set");
                }
                request.ElectronicDelivery = "";
            }

            request.HoldForDegree = order.UserDefinedExtensions.HoldForProgramId ?? "";

            if (!string.IsNullOrEmpty(request.HoldForDegree))
            {
                if (request.HoldForDegree.Length > 20)
                {
                    throw new ApplicationException(request.HoldForDegree + " is not a valid program code.  Program codes are limited to 20 bytes.");
                }
            }

            request.HoldForTerm = order.UserDefinedExtensions.HoldForTermId ?? "";
            if (!string.IsNullOrEmpty(request.HoldForTerm))
            {
                if (request.HoldForTerm.Length > 7)
                {
                    throw new ApplicationException(request.HoldForTerm + " is not a valid term.  Term codes are limited to 7 bytes.");
                }
            }
            request.OrderNo = order.TransmissionData.RequestTrackingID;
            request.OrderSource = order.TransmissionData.Source.Organization.DUNS ?? "";

            try
            {
                if (order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.AddressLine.Count > 0)
                {
                    request.RcptAddressLines = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.AddressLine;
                }
                else
                {
                    request.RcptAddressLines = new List<string>();
                }
            }
            catch (Exception)
            {
                request.RcptAddressLines = new List<string>();
            }

            try
            {
                request.RcptAttentionLine = String.Join(" ", order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.AttentionLine.Where(al => !string.IsNullOrEmpty(al))) ?? "";
            }
            catch (Exception)
            {
            }

            request.RcptCeebact = order.UserDefinedExtensions.ReceivingInstitutionCeebId ?? "";

            try
            {
                request.RcptCity = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.City ?? "";
            }
            catch (Exception)
            {
                request.RcptCity = String.Empty;
            }

            try
            {
                request.RcptCountryCode = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.CountryCode ?? "";
            }
            catch (Exception)
            {
                request.RcptCountryCode = String.Empty;
            }

            try
            {
                request.RcptEmailAddress = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Email.EmailAddress ?? "";
            }
            catch (Exception)
            {
                request.RcptEmailAddress = String.Empty;
            }

            // MBS 8/31/2016 adding in missing fax number
            try
            {
                request.RcptFaxNumber = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().FaxPhone.AreaCityCode +
                     order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().FaxPhone.PhoneNumber;
            }
            catch (Exception)
            {
            }


            try
            {
                request.RcptOpeid = order.Request.Recipient.Receiver.RequestorReceiverOrganization.OPEID ?? "";
            }
            catch (NullReferenceException)
            {
                request.RcptOpeid = String.Empty;
            }

            try
            {
                request.RcptOrgName = order.Request.Recipient.Receiver.RequestorReceiverOrganization.OrganizationName ?? "";
            }
            catch (NullReferenceException)
            {
                request.RcptOrgName = String.Empty;
            }

            try
            {

                Domain.Student.Entities.Transcripts.Phone tempPhone = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Phone;

                request.RcptPhoneNumber = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Phone.AreaCityCode +
                                          order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Phone.PhoneNumber;
            }
            catch (InvalidOperationException)
            {
                request.RcptPhoneNumber = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.RcptPhoneNumber = String.Empty;
            }

            try
            {
                request.RcptPostalCode = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.PostalCode ?? "";
            }
            catch (InvalidOperationException)
            {
                request.RcptPostalCode = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.RcptPostalCode = String.Empty;
            }

            try
            {
                request.RcptStateProvinceCode = order.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.FirstOrDefault().Address.StateProvinceCode ?? "";
            }
            catch (InvalidOperationException)
            {
                request.RcptStateProvinceCode = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.RcptStateProvinceCode = String.Empty;
            }

            try
            {
                request.SpecialInstrs = order.Request.Recipient.DeliveryInstruction ?? "";
            }
            catch (NullReferenceException)
            {
                request.SpecialInstrs = String.Empty;
            }
            ;

            try
            {
                request.StampSealInd = (order.Request.Recipient.StampSealEnvelopeIndicator != null && order.Request.Recipient.StampSealEnvelopeIndicator.ToUpper() == "TRUE");
            }
            catch (NullReferenceException)
            {
                request.StampSealInd = false;
            }

            try
            {
                request.StuAddressLines = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Address.AddressLine;
            }
            catch (InvalidOperationException)
            {
                request.StuAddressLines = new List<string>();
            }
            catch (NullReferenceException)
            {
                request.StuAddressLines = new List<string>();
            }
            ;

            try
            {
                request.StuAltFirstName = order.Request.RequestedStudent.Person.AlternateName.FirstName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuAltFirstName = String.Empty;
            }

            try
            {
                request.StuAltLastName = order.Request.RequestedStudent.Person.AlternateName.LastName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuAltLastName = String.Empty;
            }

            try
            {
                request.StuAltMiddleName = order.Request.RequestedStudent.Person.AlternateName.MiddleName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuAltMiddleName = String.Empty;
            }

            try
            {
                request.StuBirthDate = order.Request.RequestedStudent.Person.Birth.BirthDate;
            }
            catch (NullReferenceException)
            {
                request.StuBirthDate = DateTime.Parse("1/1/1900");
            }

            try
            {
                request.StuCity = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Address.City ?? "";
            }
            catch (InvalidOperationException)
            {
                request.StuCity = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuCity = String.Empty;
            }
            ;

            try
            {
                request.StuCountryCode = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Address.CountryCode ?? "";
            }
            catch (InvalidOperationException)
            {
                request.StuCountryCode = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuCountryCode = String.Empty;
            }
            ;

            try
            {
                request.Student = order.Request.RequestedStudent.Person.SchoolAssignedPersonID ?? "";
            }
            catch (NullReferenceException)
            {
                request.Student = String.Empty;
            }

            try
            {
                request.StuEmailAddress = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Email.EmailAddress ?? "";
            }
            catch (InvalidOperationException)
            {
                request.StuEmailAddress = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuEmailAddress = String.Empty;
            }
            ;

            try
            {
                request.StuFirstName = order.Request.RequestedStudent.Person.Name.FirstName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuFirstName = String.Empty;
            }

            try
            {
                request.StuLastName = order.Request.RequestedStudent.Person.Name.LastName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuLastName = String.Empty;
            }

            try
            {
                request.StuMiddleName = order.Request.RequestedStudent.Person.Name.MiddleName ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuMiddleName = String.Empty;
            }


            try
            {
                request.StuPhoneNumber = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Phone.AreaCityCode +
                                         order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Phone.PhoneNumber;
            }
            catch (InvalidOperationException)
            {
                request.StuPhoneNumber = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuPhoneNumber = String.Empty;
            }


            try
            {
                request.StuPostalCode = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Address.PostalCode ?? "";
            }
            catch (InvalidOperationException)
            {
                request.StuPostalCode = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuPostalCode = String.Empty;
            }


            try
            {
                request.StuSsn = order.Request.RequestedStudent.Person.SSN ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuSsn = String.Empty;
            }

            try
            {
                request.StuStateProvinceCode = order.Request.RequestedStudent.Person.Contacts.FirstOrDefault().Address.StateProvinceCode ?? "";
            }
            catch (InvalidOperationException)
            {
                request.StuStateProvinceCode = String.Empty;
            }
            catch (NullReferenceException)
            {
                request.StuStateProvinceCode = String.Empty;
            }

            try
            {
                request.StuUnverifiedId = order.UserDefinedExtensions.UnverifiedStudentId ?? "";
            }
            catch (NullReferenceException)
            {
                request.StuUnverifiedId = String.Empty;
            }

            try
            {
                request.TranscriptCopies = order.Request.Recipient.TranscriptCopies;
            }
            catch (Exception)
            {
                request.TranscriptCopies = 1;
            }

            try
            {
                request.TranscriptPurpose = order.Request.Recipient.TranscriptPurpose.ToString();
            }
            catch (Exception)
            {
                request.TranscriptPurpose = String.Empty;
            }

            try
            {
                request.TranscriptType = order.Request.Recipient.TranscriptType.ToString();
            }
            catch (Exception)
            {
                request.TranscriptType = TranscriptType.Complete.ToString();
            }

            try
            {
                request.ProcessCode = order.TransmissionData.DocumentProcessCode ?? "";
            }
            catch (Exception)
            {
                request.ProcessCode = String.Empty;
            }

            try
            {
                request.RushProcessingFlag = (!String.IsNullOrEmpty(order.Request.Recipient.RushProcessingRequested)) &&
                                                (order.Request.Recipient.RushProcessingRequested.ToUpper() == "TRUE" ||
                                                 order.Request.Recipient.RushProcessingRequested.ToUpper() == "T");

            }
            catch (Exception)
            {
            }





            // Execute Transaction

            ProcessTranscriptOrderResponse response = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<ProcessTranscriptOrderRequest, ProcessTranscriptOrderResponse>(request);
            }
            catch (ColleagueTransactionException ce)
            {
                logger.Error(ce.Message);
                if (ce.Message.Contains("SECURITY") || ce.Message.Contains("TOKEN"))
                {
                    logger.Error(ce.Message);
                    throw new PermissionsException("Login expired");
                }
                else
                {
                    logger.Error(ce.Message);
                    throw ce;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            if (response == null || string.IsNullOrEmpty(response.TranscriptResponse))
            {
                throw new ApplicationException("Colleague returned no Transcript Response");
            }


            return response.TranscriptResponse;
        }

        /// <summary>
        /// Method to GET the status of an existing order
        /// </summary>
        /// <param name="orderId">The third-party-generated order ID</param>
        /// <returns>JSON object containing Base-64 encoded PESC XML transcript response data</returns>
        public async Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode)
        {

            TranscriptOrderStatusRequest request = new TranscriptOrderStatusRequest() {OrderNo = orderId, ATranscriptOrderCloudStatus = currentStatusCode};
            TranscriptOrderStatusResponse response = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<TranscriptOrderStatusRequest, TranscriptOrderStatusResponse>(request);
            }
            catch (ColleagueTransactionException ce)
            {
                logger.Error(ce.Message);
                if (ce.Message.Contains("SECURITY") || ce.Message.Contains("TOKEN"))
                {
                    logger.Error(ce.Message);
                    throw new PermissionsException("Login expired");
                }
                else
                {
                    logger.Error(ce.Message);
                    throw ce;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            if (response == null || string.IsNullOrEmpty(response.TranscriptResponse))
            {
                throw new ApplicationException("Colleague returned no Transcript Response");
            }

            return response.TranscriptResponse;
        }

        /// <summary>
        /// Get a list of Admit Terms from the Student Academic Levels
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="acadLevels"></param>
        /// <returns>List of Admit Terms</returns>
        private async Task<List<string>> GetAdmitTermAsync(string studentId, List<string> acadLevels)
        {
            List<string> admitTerms = new List<string>();
            if (studentId != null && acadLevels != null)
            {
                foreach (var acadLevel in acadLevels)
                {
                    string stuAcadLevelsId = studentId + "*" + acadLevel;
                    StudentAcadLevels studentAcadLevel = await DataReader.ReadRecordAsync<StudentAcadLevels>(stuAcadLevelsId);
                    if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaStartTerm))
                    {
                        admitTerms.Add(studentAcadLevel.StaStartTerm);
                    }
                }
            }
            return admitTerms;
        }

        /// <summary>
        /// Calculates if the student's parent(s) went to this institution
        /// </summary>
        /// <param name="personContract"></param>
        /// <param name="personData"></param>
        /// <returns>True or False if Student is considered a Legacy Student</returns>
        private async Task<bool> IsStudentLegacyAsync(Base.DataContracts.Person personContract, Collection<Base.DataContracts.Person> personData)
        {
            bool legacyStudent = false;

            if (personContract == null)
            {
                var errorMessage = string.Format("No Person Data available for student '{0}'", personContract.Recordkey);
                logger.Warn(errorMessage);
            }
            List<string> parentIds = personContract.Parents;

            if (parentIds != null && parentIds.Count > 0)
            {
                Base.DataContracts.Defaults defaultData = await GetDefaultsAsync();
                foreach (var parentId in parentIds)
                {
                    Base.DataContracts.Person parentContract = personData.Where(p => p.Recordkey == parentId).FirstOrDefault();

                    if (parentContract != null && defaultData != null)
                    {
                        List<string> institutionsAttend = parentContract.PersonInstitutionsAttend;
                        foreach (var instAttend in institutionsAttend)
                        {
                            if (defaultData.DefaultHostCorpId == instAttend)
                            {
                                legacyStudent = true;
                            }
                        }
                    }
                }
            }
            return legacyStudent;
        }

        /// <summary>
        /// If we have Foreign Person data on file then this is considered an International Student
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="foreignPersonContract"></param>
        /// <returns>Boolean Yes or No for International Student</returns>
        private bool IsStudentInternational(string studentId, Collection<Base.DataContracts.ForeignPerson> foreignPersonContract, Collection<Base.DataContracts.Person> personContract, IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData)
        {
            bool internationalStudent = false;
            Base.DataContracts.Person personData = personContract.Where(p => p.Recordkey == studentId).FirstOrDefault();

            if (!string.IsNullOrEmpty(personData.AlienId))
            {
                internationalStudent = true;
            }
            else
            {
                if (foreignPersonContract == null)
                {
                    // do not log error, just return false;
                    return false;
                }
                Base.DataContracts.ForeignPerson foreignPersonData = foreignPersonContract.Where(p => p.Recordkey == studentId).FirstOrDefault();
                if (foreignPersonData != null)
                {
                    // Check if alien status exists.  Consider the student an alien based on that field only if it indicates a NonCitizen type.
                    bool hasAlienStatus = false;
                    if (!string.IsNullOrEmpty(foreignPersonData.FperAlienStatus))
                    {
                        hasAlienStatus = true;
                        if (citizenshipStatusData != null)
                        {
                            foreach (var citizenshipStatus in citizenshipStatusData)
                            {
                                if (citizenshipStatus.Code == foreignPersonData.FperAlienStatus && citizenshipStatus.CitizenshipStatusType == CitizenshipStatusType.Citizen)
                                {
                                    hasAlienStatus = false;
                                }
                            }
                        }
                    }
                    if ((hasAlienStatus == true) || !string.IsNullOrEmpty(foreignPersonData.FperVisaNo))
                    {
                        internationalStudent = true;
                    }
                }
            }
            return internationalStudent;
        }

        /// <summary>
        /// Determines if this student is a first generation college student.
        /// This means that neither of their parents have attended or enrolled
        /// into any college.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="applicantsContracts">APPLICANTS data contains parent and guardian education levels</param>
        /// <returns>Boolean to identify First Generation Student</returns>
        private async Task<bool?> IsStudentFirstGenerationAsync(string studentId, Collection<Student.DataContracts.Applicants> applicantsContracts)
        {
            bool? firstGenerationStudent = null;
            if (applicantsContracts == null)
            {
                // do not log error, just return false;
                return firstGenerationStudent;
            }
            Student.DataContracts.Applicants applicantsData = applicantsContracts.Where(p => p.Recordkey == studentId).FirstOrDefault();
            if (applicantsData != null)
            {
                if (string.IsNullOrEmpty(applicantsData.AppParent1EducLevel)
                    && string.IsNullOrEmpty(applicantsData.AppParent2EducLevel)
                    && string.IsNullOrEmpty(applicantsData.AppGuardian1EducLevel)
                    && string.IsNullOrEmpty(applicantsData.AppGuardian2EducLevel))
                {
                    firstGenerationStudent = null;
                }
                else
                {
                    firstGenerationStudent = true;
                }
                if (!string.IsNullOrEmpty(applicantsData.AppParent1EducLevel))
                {
                    var codeAssoc = (await GetParentEducationLevelAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == applicantsData.AppParent1EducLevel).FirstOrDefault();
                    if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "2")
                    {
                        firstGenerationStudent = false;
                    }
                }
                if (!string.IsNullOrEmpty(applicantsData.AppParent2EducLevel))
                {
                    var codeAssoc = (await GetParentEducationLevelAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == applicantsData.AppParent2EducLevel).FirstOrDefault();
                    if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "2")
                    {
                        firstGenerationStudent = false;
                    }
                }
                if (!string.IsNullOrEmpty(applicantsData.AppGuardian1EducLevel))
                {
                    var codeAssoc = (await GetParentEducationLevelAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == applicantsData.AppGuardian1EducLevel).FirstOrDefault();
                    if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "2")
                    {
                        firstGenerationStudent = false;
                    }
                }
                if (!string.IsNullOrEmpty(applicantsData.AppGuardian2EducLevel))
                {
                    var codeAssoc = (await GetParentEducationLevelAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == applicantsData.AppGuardian2EducLevel).FirstOrDefault();
                    if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "2")
                    {
                        firstGenerationStudent = false;
                    }
                }
            }
            return firstGenerationStudent;
        }

        /// <summary>
        /// Retrieves the student's transcript in a string format
        /// </summary>
        /// <param name="id">Id of the student for whom the transcript is requested</param>
        /// <param name="transcriptGrouping">Transcript Grouping code if a particular transcript is desired. If empty transcripts for all the student's transcript groupings will be returned</param>
        /// <returns>The student's transcript</returns>
        public async Task<string> GetTranscriptAsync(string studentId, string transcriptGrouping)
        {
            char _VM = Convert.ToChar(DynamicArray.VM);

            var request = new CreatePlainTextTranscriptRequest() {StudentId = studentId, TranscriptGrouping = transcriptGrouping};
            CreatePlainTextTranscriptResponse response = null;
            string transcript = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<CreatePlainTextTranscriptRequest, CreatePlainTextTranscriptResponse>(request);
                if (string.IsNullOrEmpty(response.TranscriptText))
                {
                    logger.Info(string.Format("Unable to generate transcript text for student: '{0}' and transcript grouping: '{1}'", studentId, transcriptGrouping));
                    transcript = "Unable to generate transcript at this time.";
                }
                else
                {
                    transcript = response.TranscriptText.Replace(_VM, '\n');
                    // Strip any leading newlines
                    while (transcript[0] == '\n')
                    {
                        transcript = transcript.Substring(1);
                    }
                }
            }
            catch (ColleagueTransactionException ce)
            {
                logger.Error(ce, string.Format("Unable to generate transcript text for student: '{0}' and transcript grouping: '{1}'", studentId, transcriptGrouping));
                throw new ApplicationException("Unable to generate transcript.", ce);
            }

            return transcript;

        }

        /// <summary>
        /// Returns access information for the given list of students
        /// </summary>
        /// <param name="ids">List of students</param>
        /// <returns>List of Student Access information</returns>
        public async Task<IEnumerable<Domain.Student.Entities.StudentAccess>> GetStudentAccessAsync(IEnumerable<string> ids)
        {
            // First attempt to get items from the short cache
            const string _StudentAccessCache = "StudentAccess";

            var studentAccessEntities = new Collection<StudentAccess>();
            if ((ids == null) || (ids.Count() == 0))
            {
                return studentAccessEntities;
            }
            List<string> idsNotFoundInCache = new List<string>();
            foreach (var id in ids)
            {
                string cacheKey = _StudentAccessCache + id;
                string fullCacheKey = BuildFullCacheKey(cacheKey);
                if (ContainsKey(fullCacheKey))
                {
                    var studentAccess = (StudentAccess) _cacheProvider.Get(fullCacheKey);
                    studentAccessEntities.Add(studentAccess);
                }
                else
                {
                    idsNotFoundInCache.Add(id);
                }
            }
            if (idsNotFoundInCache != null && idsNotFoundInCache.Count() > 0)
            {
                // Get source StudentAdvisement data
                string criteria = "STAD.STUDENT EQ '?'";
                string[] studentAdvisementIds = await DataReader.SelectAsync("STUDENT.ADVISEMENT", criteria, idsNotFoundInCache.Distinct().ToArray());
                Collection<Student.DataContracts.StudentAdvisement> studentAdvisementData = await DataReader.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(studentAdvisementIds);

                // Create a studentAccess item for each incoming student 
                foreach (var id in idsNotFoundInCache)
                {
                    string cacheKey = _StudentAccessCache + id;
                    studentAccessEntities.Add(GetOrAddToCache<StudentAccess>(cacheKey,
                        () =>
                        {
                            StudentAccess studentAccess = new StudentAccess(id);

                            var advisements = ProcessAdvisements(id, studentAdvisementData);

                            foreach (var advisement in advisements)
                            {
                                studentAccess.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
                            }
                            return studentAccess;
                        })
                    );
                }
            }
            return studentAccessEntities;
        }

        protected List<Advisement> ProcessAdvisements(string studentId, IEnumerable<StudentAdvisement> studentAdvisementData)
        {
            var advisements = new List<Advisement>();
            if (studentAdvisementData != null)
            {
                IEnumerable<StudentAdvisement> advisorAssignments = studentAdvisementData.Where(sa => sa.StadStudent == studentId);
                // Sift out the "current" advisors by date here in the repository. Always based on today's date.
                var currentAdvisors = advisorAssignments.Where(sa => (sa.StadStartDate == null || sa.StadStartDate <= DateTime.Now) && (sa.StadEndDate == null || sa.StadEndDate > DateTime.Now));
                if (currentAdvisors != null && currentAdvisors.Count() > 0)
                {
                    foreach (var studentAdvisement in currentAdvisors)
                    {
                        try
                        {
                            advisements.Add(new Advisement(studentAdvisement.StadFaculty, studentAdvisement.StadStartDate)
                                {
                                    EndDate = studentAdvisement.StadEndDate,
                                    AdvisorType = studentAdvisement.StadType
                                });
                        }
                        catch (Exception)
                        {
                            // Don't bother logging if student Id is null or this is a duplicate advisee.
                        }
                    }
                }
            }
            return advisements;
        }
  
        /// <summary>
        /// Retrieves a student from the database using the provided Colleague student id.
        /// </summary>
        /// <param name="id">Colleague Person (student) id.</param>
        /// <returns>A Student Entity with data from Colleague.</returns>
        /// <remarks>
        /// The data retrieved from this method is not cached.
        /// </remarks>
        public async Task<Ellucian.Colleague.Domain.Student.Entities.Student> GetAsync(string id)
        {
            Students students = null;
            PersonSt personSt = null;
            //to retrieve collection of ids to pass to datareader calls
            var stprIds = new List<string>();
            var acadLevelIds = new List<string>();
            var instAttendIds = new List<string>();
            var institutionIds = new List<string>();
            var otherIds = new List<string>();
            IEnumerable<string> studentAdvisementIds = null;
            //data contracts
            Base.DataContracts.Person personData = null;
            Base.DataContracts.ForeignPerson foreignPersonData = null;
            Student.DataContracts.Applicants applicantsData = null;
            Student.DataContracts.FinAid financialAidData = null;
            Collection<StudentPrograms> programData = null;
            Collection<StudentAcadLevels> studentAcadLevelsData = new Collection<StudentAcadLevels>();
            Collection<Base.DataContracts.Person> personContract = new Collection<Base.DataContracts.Person>();
            Collection<Base.DataContracts.ForeignPerson> foreignPersonContract = new Collection<Base.DataContracts.ForeignPerson>();
            Collection<Student.DataContracts.Applicants> applicantsContract = new Collection<Student.DataContracts.Applicants>();
            Collection<Base.DataContracts.Institutions> institutionData = new Collection<Base.DataContracts.Institutions>();
            Collection<Base.DataContracts.InstitutionsAttend> instAttendData = new Collection<Base.DataContracts.InstitutionsAttend>();
            Collection<Student.DataContracts.StudentAdvisement> studentAdvisementData = null;
            Collection<Base.DataContracts.Person> otherContract = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Student ID may not be null or empty");
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Ellucian.Colleague.Domain.Student.Entities.Student studentEntity = null;
            try
            {
                students = await DataReader.ReadRecordAsync<Students>(id);
                if (null == students)
                {
                    return null;
                }
                // get student programs and student acad levels keys
                if (students.StuAcadPrograms != null)
                {
                    foreach (var acadProgramId in students.StuAcadPrograms)
                    {
                        stprIds.Add(students.Recordkey + "*" + acadProgramId);
                    }
                }
                if (students.StuAcadLevels != null)
                {
                    foreach (var acadLevelId in students.StuAcadLevels)
                    {
                        acadLevelIds.Add(students.Recordkey + "*" + acadLevelId);
                    }
                }

                // Get PERSON.ST data contract
                personSt = await DataReader.ReadRecordAsync<PersonSt>(id);
                //"StudentPrograms"
                if (stprIds != null && stprIds.Count() > 0)
                {
                    programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.Distinct().ToArray());
                }

                //"StudentAcadLevels"
                if (acadLevelIds != null && acadLevelIds.Count() > 0)
                {
                    studentAcadLevelsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StudentAcadLevels>(acadLevelIds.Distinct().ToArray());
                }

                // Get PERSON data contract.
                personData = await DataReader.ReadRecordAsync<Base.DataContracts.Person>("PERSON", id);
                // Get FOREIGN.PERSON contract.
                foreignPersonData = await DataReader.ReadRecordAsync<Base.DataContracts.ForeignPerson>(id);
                // Get APPLICANTS contract.
                applicantsData = await DataReader.ReadRecordAsync<Student.DataContracts.Applicants>(id);
                //Get the FIN.AID datacontract
                financialAidData = await DataReader.ReadRecordAsync<Student.DataContracts.FinAid>(id);
                if (foreignPersonData != null)
                {
                    foreignPersonContract.Add(foreignPersonData);
                }
                if (applicantsData != null)
                {
                    applicantsContract.Add(applicantsData);
                }
                // Get INSTITUTIONS and INSTITUTIONS.ATTEND data
                if (personData != null)
                {
                    if (personData.PersonInstitutionsAttend != null)
                    {
                        foreach (var instId in personData.PersonInstitutionsAttend)
                        {
                            instAttendIds.Add(personData.Recordkey + "*" + instId);
                            institutionIds.Add(instId);
                        }
                    }
                    // Get Parent PERSON data.

                    if (personData.Parents != null)
                    {
                        foreach (var parent in personData.Parents)
                        {
                            otherIds.Add(parent);
                        }
                    }
                    if (institutionIds.Count > 0)
                    {
                        //"Institutions"
                        institutionData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Institutions>(institutionIds.Distinct().ToArray());
                        instAttendData = await DataReader.BulkReadRecordAsync<Base.DataContracts.InstitutionsAttend>(instAttendIds.ToArray());

                    }
                    if (otherIds != null && otherIds.Count > 0)
                    {
                        otherContract = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", otherIds.ToArray());
                    }

                }
                if (personSt != null)
                {
                    // Get StudentAdvisement data contracts
                    studentAdvisementIds = personSt.PstAdvisement;
                    studentAdvisementData = await DataReader.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(studentAdvisementIds.ToArray());
                }
                //collect result from tasks
                if (personData != null)
                {
                    personContract.Add(personData);
                }

                if (otherContract != null && otherContract.Count > 0)
                {
                    foreach (var other in otherContract)
                    {
                        if (other != null)
                        {
                            personContract.Add(other);
                        }
                    }
                }
                Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
                IEnumerable<CitizenshipStatus> citizenshipStatusData = null;
                // Now
                var studentEntities = await BuildStudentsAsync(new List<string>() {id},
                    new Collection<Student.DataContracts.Students>() {students},
                    programData, new Collection<PersonSt>() {personSt},
                    studentAdvisementData, personContract, new Collection<Student.DataContracts.FinAid>() {financialAidData}, foreignPersonContract,
                    applicantsContract, instAttendData, institutionData, instAttendData, studentAcadLevelsData,
                    true, true, termData, false, citizenshipStatusData, false);

                studentEntity = studentEntities.Where(s => s.Id == id).FirstOrDefault();
                stopWatch.Stop();
                long elapsedTime = stopWatch.ElapsedMilliseconds;
                Debug.WriteLine(string.Format("Sync Elapsed Time {0}", elapsedTime));
                return studentEntity;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Exception occured while retrieving student details for student with id {0}", id));
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves a student from the database using the provided Colleague student guid.
        /// </summary>
        /// <param name="guid">Colleague Person (student) guid.</param>
        /// <returns>A Student Entity with data from Colleague.</returns>
        public async Task<Ellucian.Colleague.Domain.Student.Entities.Student> GetDataModelStudentFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var id = await GetStudentIdFromGuidAsync(guid);

            if (id == null)
            {
                throw new KeyNotFoundException("Student GUID " + guid + " lookup failed.");
            }

            //return await GetDataModelStudentAsync(id);
            var student = await GetDataModelStudentsAsync(new List<string>() { id });
            return student.FirstOrDefault();
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks> 
        /// <summary>
        /// Returns Student Entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person"></param>
        /// <param name="type"></param>
        /// <param name="cohort"></param>
        /// <param name="residency"></param>
        /// <returns>List of Student Entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.Student>, int>> GetDataModelStudentsAsync(int offset, int limit,
            bool bypassCache = false, string person = "", string type = "", string cohort = "", string residency = "")
        {
            try
            {
                var criteria = "";
                if (!string.IsNullOrEmpty(person))
                {
                    criteria += "WITH STUDENTS.ID EQ '" + person + "'";
                }
                if (!string.IsNullOrEmpty(type))
                {
                    if (!string.IsNullOrEmpty(criteria))
                        criteria += " AND ";
                    criteria += "WITH STU.CURRENT.TYPE EQ '" + type + "'";
                }

                if (!string.IsNullOrEmpty(residency))
                {
                    if (!string.IsNullOrEmpty(criteria))
                        criteria += " AND ";
                    criteria += "WITH STU.CURRENT.RESIDENCY.STATUS EQ '" + residency + "'";
                }

                var studentsIds = await DataReader.SelectAsync("STUDENTS", criteria);

                var cohortStudentIds = await GetCohortStudentIds(cohort);

                if (cohortStudentIds.Any())
                {
                    studentsIds = studentsIds.Intersect(cohortStudentIds.ToArray()).ToArray();
                }

                var totalCount = studentsIds.Count();

                Array.Sort(studentsIds);
                var sublist = studentsIds.Skip(offset).Take(limit);

                if (sublist != null && !sublist.Any())
                {
                    return new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(new List<Domain.Student.Entities.Student>(), 0);
                }

                IEnumerable<Domain.Student.Entities.Student> studentsList = null;
                try
                {
                    studentsList = await GetDataModelStudentsAsync(sublist.ToArray());
                }
                catch (Exception ex)
                {
                    throw new RepositoryException(ex.Message);
                }
                return new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentsList, totalCount);

            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }

        private async Task<List<string>> GetCohortStudentIds(string cohort)
        {
            // need to get all cohorts from STUDENT.ACAD.LEVELS
            var cohortStudentIds = new List<string>();
            if (!string.IsNullOrEmpty(cohort))
            {
                var studentAcadLevelIds = await DataReader.SelectAsync("STUDENT.ACAD.LEVELS", "WITH STA.OTHER.COHORT.GROUPS EQ '" + cohort + "'");
                if (studentAcadLevelIds != null && studentAcadLevelIds.Any())
                {
                    var studentAcadLevels = await DataReader.BulkReadRecordAsync<StudentAcadLevels>("STUDENT.ACAD.LEVELS", studentAcadLevelIds);
                    if (studentAcadLevels != null && studentAcadLevels.Any())
                    {
                        DateTime current = DateTime.Now;
                        // Determine which cohorts are active and get the studentID
                        foreach (var studentAcadLevel in studentAcadLevels)
                        {
                            try
                            {
                                if ((studentAcadLevel.StaOtherCohortsEntityAssociation == null) || (!studentAcadLevel.StaOtherCohortsEntityAssociation.Any())) continue;
                                foreach (var otherCohort in studentAcadLevel.StaOtherCohortsEntityAssociation)
                                {
                                    var startDate = default(DateTime);
                                    var endDate = default(DateTime);
                                    if (otherCohort.StaOtherCohortStartDatesAssocMember != null)
                                        startDate = (DateTime)otherCohort.StaOtherCohortStartDatesAssocMember;
                                    if (otherCohort.StaOtherCohortEndDatesAssocMember != null)
                                        endDate = (DateTime)otherCohort.StaOtherCohortEndDatesAssocMember;

                                    if (startDate.CompareTo(current) <= 0 && endDate.CompareTo(current) <= 0)
                                    {
                                        var studentAcadLevelId = studentAcadLevel.Recordkey.Split('*');
                                        if (studentAcadLevelId.Length > 0)
                                        {
                                            cohortStudentIds.Add(studentAcadLevelId[0]);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e, string.Format("Unable to determine active Student Cohorts for studentAcadLevel '{0}'", studentAcadLevel.Recordkey));
                            }
                        }
                    }
                }
            }

            return cohortStudentIds;
        }

        /// <summary>
        /// Wrapper around Async, used by FinancialAid branch for AcademicProgressService
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="cat"></param>
        /// <returns></returns> 
        public Ellucian.Colleague.Domain.Student.Entities.Student Get(string id)
        {
            var x = Task.Run(async () =>
            {
                return await GetAsync(id);
            }).GetAwaiter().GetResult();
            return x;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetStudentIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] {new GuidLookup(guid)});
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Student GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Student GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENTS")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, STUDENTS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        
        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Reads the student information from Colleague and returns an IEnumerable of Students Entity models.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list if none are found.</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetDataModelStudentsAsync(IEnumerable<string> ids)
        {

            var students = await DataReader.BulkReadRecordAsync<Students>(ids.ToArray());
            var personSts = await DataReader.BulkReadRecordAsync<PersonSt>(ids.ToArray());
            var studentAcadProgramIds = new List<string>();
            var studentAcadLevelIds = new List<string>();
            var studentProgramData = new Collection<StudentPrograms>();
            var studentAcadLevelsData = new Collection<StudentAcadLevels>();
            var studentAcadCredentials = new Collection<StudentAcadCred>();
            var studentEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Student>();

            // get all the student academic programs at once
            foreach (var student in students)
            {
                if (student.StuAcadPrograms != null)
                {
                    studentAcadProgramIds.AddRange(student.StuAcadPrograms.Select(acadProgramId => student.Recordkey + "*" + acadProgramId));
                }

                if (student.StuAcadLevels != null)
                {
                    studentAcadLevelIds.AddRange(student.StuAcadLevels.Select(acadLevelId => student.Recordkey + "*" + acadLevelId));
                }
            }

            if (studentAcadProgramIds.Any())
            {
                studentProgramData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentAcadProgramIds.Distinct().ToArray());
            }

            if (studentAcadLevelIds.Any())
            {
                studentAcadLevelsData = await DataReader.BulkReadRecordAsync<StudentAcadLevels>(studentAcadLevelIds.Distinct().ToArray());
            }

            if (personSts.Any())
            {
                studentAcadCredentials = await DataReader.BulkReadRecordAsync<StudentAcadCred>("STUDENT.ACAD.CRED",
                personSts.SelectMany(x => x.PstStudentAcadCred).ToArray());
            }

            return (await BuildDataModelStudentsAsync(
                    ids,
                    students,
                    studentProgramData,
                    personSts,
                    studentAcadLevelsData,
                    studentAcadCredentials)).ToList();
        }


        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Reads the required data from Colleague and returns a Students Entity model used by Data Model.
        /// </summary>
        /// <param name="studentIds">Colleague Person (student) id.</param>
        /// <param name="studentData"></param>
        /// <param name="studentProgramData"></param>
        /// <param name="personStData"></param>
        /// <param name="studentAcadLevelsData"></param>
        /// <param name="studentAcadCredits"></param>
        /// <returns>Student Entity if found in Colleague, null if the student does not exist in Colleague.</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> BuildDataModelStudentsAsync(
            IEnumerable<string> studentIds,
            Collection<Student.DataContracts.Students> studentData,
            Collection<Student.DataContracts.StudentPrograms> studentProgramData,
            Collection<Base.DataContracts.PersonSt> personStData,
            Collection<Student.DataContracts.StudentAcadLevels> studentAcadLevelsData,
            Collection<Student.DataContracts.StudentAcadCred> studentAcadCredits
            )
        {
            var studentResults = new List<Ellucian.Colleague.Domain.Student.Entities.Student>();

            foreach (var studentId in studentIds)
            {
                try
                {
                    Domain.Student.Entities.Student studentEntity = null;

                    // Get students data contract
                    var students = studentData.FirstOrDefault(s => s.Recordkey == studentId);
                    // Get person data contract
                    //var personContract = personData.FirstOrDefault(p => p.Recordkey == studentId);
                    if (students != null)
                    {
                        var programIds = new List<string>();
                        if ((students.StuAcadPrograms != null) && (students.StuAcadPrograms.Any()))
                        {
                            foreach (var acadProgramId in students.StuAcadPrograms)
                            {
                                var studentProgram = studentProgramData.FirstOrDefault(sp => sp.Recordkey == (studentId + "*" + acadProgramId));
                                if (studentProgram != null)
                                {
                                    // If the program is withdrawn or dropped/changed-mind, skip it.
                                    if (studentProgram.StprStatus.Count > 0)
                                    {
                                        var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValInternalCodeAssocMember == studentProgram.StprStatus.ElementAt(0));
                                        if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "4" || codeAssoc.ValActionCode1AssocMember == "5"))
                                        {
                                            continue;
                                        }
                                    }
                                    // If student program has ended, skip it.
                                    if (studentProgram.StprEndDate != null && studentProgram.StprEndDate.Count > 0 && studentProgram.StprEndDate.ElementAt(0) < DateTime.Today)
                                    {
                                        continue;
                                    }
                                    // If the program doesn't have a start date, skip it.
                                    if (studentProgram.StprStartDate != null && studentProgram.StprStartDate.Count == 0)
                                    {
                                        continue;
                                    }
                                    // STUDENT.PROGRAMS key is multi-part.  Only save the program portion (second part) to the Student domain entity
                                    programIds.Add(studentProgram.Recordkey.Split('*')[1]);

                                }
                            }
                        }

                        // from PERSON.ST record get the student acad cred list
                        var academicCreditIds = new List<string>();
                        if (personStData != null && personStData.Any())
                        {
                            var personSt = personStData.FirstOrDefault(pst => pst.Recordkey == studentId);
                            if (personSt != null && personSt.PstStudentAcadCred != null && personSt.PstStudentAcadCred.Any())
                            {
                                academicCreditIds.AddRange(personSt.PstStudentAcadCred.Select(ids => ids));
                            }
                        }
                        studentEntity = new Domain.Student.Entities.Student(students.RecordGuid,
                            students.Recordkey,
                            programIds, academicCreditIds, "*", false);
                    }

                    if (studentAcadCredits != null && studentAcadCredits.Any())
                    {
                        var studentAcadCredit = studentAcadCredits.Where(sac => sac.StcPersonId == studentId);
                        if (studentAcadCredit != null && studentAcadCredit.Any())
                        {
                            var groups = studentAcadCredit
                                 .GroupBy(x => x.StcAcadLevel)
                                 .ToDictionary(x => x.Key, x => x.Select(g => g).Distinct().ToList());

                            var performanceMeasureDict = new Dictionary<string, string>();
                            foreach (var group in groups)
                            {
                                var acadeLevel = group.Key;
                                decimal totalGradePoints = 0;
                                decimal totalGpaCredit = 0;
                                var performanceMeasure = string.Empty;
                                foreach (var academicCredit in group.Value)
                                {
                                    totalGradePoints += Convert.ToDecimal(academicCredit.StcGradePts);
                                    totalGpaCredit += Convert.ToDecimal(academicCredit.StcGpaCred);
                                }
                                if (!(totalGradePoints == 0 || totalGpaCredit == 0))
                                {
                                    var gpa = totalGradePoints / totalGpaCredit;
                                    performanceMeasure = gpa != 0 ? gpa.ToString("#.##") : string.Empty;
                                }
                                performanceMeasureDict.Add(acadeLevel, performanceMeasure);
                            }
                            studentEntity.PerformanceMeasures = performanceMeasureDict.Any() ? performanceMeasureDict : null;
                        }
                    }
                    // Sort Student Types by Date and take the newest
                    if (students.StuTypes != null && students.StuTypes.Any())
                    {
                        studentEntity.StudentTypeCode = students.StuTypeInfoEntityAssociation
                            .OrderByDescending(g => g.StuTypeDatesAssocMember)
                            .Select(g => g.StuTypesAssocMember).FirstOrDefault();
                    }

                    var studentTypeInfo = new List<StudentTypeInfo>();
                    foreach (var studentType in students.StuTypeInfoEntityAssociation)
                    {
                        var type = new StudentTypeInfo(studentType.StuTypesAssocMember, studentType.StuTypeDatesAssocMember);
                        studentTypeInfo.Add(type);
                    }
                    studentEntity.StudentTypeInfo = studentTypeInfo;

                    // Add Residency Status
                    if (students != null && students.StuResidenciesEntityAssociation != null && students.StuResidenciesEntityAssociation.Count() > 0)
                    {
                        // Get the goal with the latest associated date
                        var currStatus = students.StuResidenciesEntityAssociation.OrderByDescending(r => r.StuResidencyStatusDateAssocMember).Select(r => r.StuResidencyStatusAssocMember).FirstOrDefault();
                        // Translate to the external representation string
                        if (!string.IsNullOrEmpty(currStatus))
                        {
                            studentEntity.ResidencyStatus = currStatus;
                        }
                    }

                    studentEntity.AcademicLevelCodes = students.StuAcadLevels;
                    studentEntity.ClassLevelCodes = new List<string>();
                    studentEntity.AdmitTerms = new List<string>();


                    // Add Class Levels from Student Acad Levels
                    if (students != null && students.StuAcadLevels != null && students.StuAcadLevels.Any())
                    {
                        // Gather this student's academic level data.
                        if (studentAcadLevelsData != null)
                        {
                            foreach (var acadLevel in students.StuAcadLevels)
                            {
                                string studentAcadLevelKey = studentId + "*" + acadLevel;
                                var studentAcadLevel = studentAcadLevelsData.FirstOrDefault(sa => sa.Recordkey == studentAcadLevelKey);
                                if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaClass))
                                {
                                    studentEntity.ClassLevelCodes.Add(studentAcadLevel.StaClass);
                                }
                                if (studentAcadLevel != null && !string.IsNullOrEmpty(studentAcadLevel.StaStartTerm))
                                {
                                    studentEntity.AdmitTerms.Add(studentAcadLevel.StaStartTerm);
                                }
                                if (studentAcadLevel != null && studentEntity.StudentAcademicLevels != null)
                                {
                                    var isActive = false;
                                    var levelDomain = new StudentAcademicLevel(acadLevel, studentAcadLevel.StaAdmitStatus, studentAcadLevel.StaClass, studentAcadLevel.StaStartTerm, studentAcadLevel.StaStudentAcadCred, isActive);

                                    try
                                    {
                                        var studentAcademicLevelCohorts = new List<StudentAcademicLevelCohort>();
                                        if ((studentAcadLevel.StaOtherCohortsEntityAssociation != null) && (studentAcadLevel.StaOtherCohortsEntityAssociation.Any()))
                                        {
                                            foreach (var otherCohort in studentAcadLevel.StaOtherCohortsEntityAssociation)
                                            {
                                                var startDate = DateTime.MinValue;
                                                var endDate = DateTime.MinValue;
                                                if (otherCohort.StaOtherCohortStartDatesAssocMember != null)
                                                    startDate = (DateTime)otherCohort.StaOtherCohortStartDatesAssocMember;
                                                if (otherCohort.StaOtherCohortEndDatesAssocMember != null)
                                                    endDate = (DateTime)otherCohort.StaOtherCohortEndDatesAssocMember;

                                                if (startDate.CompareTo(DateTime.Now) <= 0 && endDate.CompareTo(DateTime.Now) <= 0)
                                                {
                                                    var cohort = new StudentAcademicLevelCohort(otherCohort.StaOtherCohortGroupsAssocMember,
                                                        otherCohort.StaOtherCohortStartDatesAssocMember, otherCohort.StaOtherCohortEndDatesAssocMember);
                                                    if (cohort == null) continue;
                                                    studentAcademicLevelCohorts.Add(cohort);
                                                }
                                            }
                                        }
                                        levelDomain.StudentAcademicLevelCohorts = studentAcademicLevelCohorts;
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn(e, string.Format("Unable to determine the Student Cohorts for '{0}'", students.Recordkey));
                                    }
                                    studentEntity.StudentAcademicLevels.Add(levelDomain);
                                }

                            }
                        }
                    }

                    // Add the student entity to the results which will be returned
                    studentResults.Add(studentEntity);
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Failed to build student {0}", studentId));
                    logger.Error(e.GetBaseException().Message);
                    logger.Error(e.GetBaseException().StackTrace);
                }
            }

            return studentResults;
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> GetStwebDefaultsAsync()
        {

            var result = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                if (stwebDefaults == null)
                {
                    if (logger.IsInfoEnabled)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                    }
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);

            return result;

        }

        /// <summary>
        /// Finds students given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against STUDENTS.
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Student Ids</returns>
        public async Task<IEnumerable<Domain.Student.Entities.Student>> GetStudentSearchByNameAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<Domain.Student.Entities.Student>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var studentIds = await base.SearchByNameAsync(lastName, firstName, middleName);

            watch.Stop();
            logger.Info("  STEP5.1 SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();
            // Filter to only return students
            studentIds = await base.FilterByEntityAsync("STUDENTS", studentIds);
            watch.Stop();
            logger.Info("  STEP5.2 FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());
            if (studentIds != null)
            {
                logger.Info("  STEP5.2 Filtered PERSONS to " + studentIds.Count() + " STUDENTS.");
            }

            //// If there are assigned advisees (only set if the user can only view assigned advisees), limit the list
            //if (assignedAdvisees != null && assignedAdvisees.Count() > 0)
            //{
            //    adviseeIds = adviseeIds.Where(x => assignedAdvisees.Contains(x));
            //}

            watch.Restart();

            var currentPageStudents = await this.GetCurrentPageAsync(studentIds, pageSize, pageIndex);

            watch.Stop();
            logger.Info("  STEP5.3 GetCurrentPage... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();

            var studentsUnsorted = currentPageStudents.Count() > 0 ? (await GetStudentsSearchAsync(currentPageStudents)).ToList() : new List<Domain.Student.Entities.Student>();

            // GetCurrentPage returns the advisee IDs sorted.  base.Get does not.
            // So, sort advisee entities in the order supplied by GetCurrentPage
            var students = new List<Domain.Student.Entities.Student>();
            foreach (var id in currentPageStudents)
            {
                students.Add(studentsUnsorted.First(x => x.Id == id));
            }

            watch.Stop();
            logger.Info("  STEP5.4 Get(currentPageAdvisees)(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            return students;
        }

        private async Task<IEnumerable<string>> GetCurrentPageAsync(IEnumerable<string> adviseeIds, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            // Get the student IDs for any advisees that have requested review/approval,
            // then sort those IDs by PERSON-SORT.NAME
            // Advisee performance diagnostics
            logger.Info("Start AdviseeRepository GetCurrentPage ");

            // Returns empty list if no ids passed in
            if (adviseeIds == null || adviseeIds.Count() == 0)
            {
                return new List<string>();
            }

            var watch = new Stopwatch();
            watch.Start();

            string[] studentIdsReviewRequested = new string[] { };
            string searchString = "DP.REVIEW.REQUESTED EQ 'Y' AND WITH DP.STUDENT.ID EQ '?' SAVING DP.STUDENT.ID";
            studentIdsReviewRequested = await DataReader.SelectAsync("DEGREE_PLAN", searchString, adviseeIds.ToArray());

            watch.Stop();
            logger.Info("    STEPX.3.1 SELECT DEGREE_PLAN WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());

            string[] studentIdsReviewRequestedSorted = new string[] { };
            if (studentIdsReviewRequested != null && studentIdsReviewRequested.Count() > 0)
            {
                watch.Restart();

                searchString = "BY LAST.NAME BY FIRST.NAME BY MIDDLE.NAME";
                studentIdsReviewRequestedSorted = await DataReader.SelectAsync("PERSON", studentIdsReviewRequested, searchString);

                watch.Stop();
                logger.Info("    STEPX.3.2 SELECT PERSON WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());
            }

            watch.Restart();

            // Now get the IDs for all student advisees, sorted by name.
            // First, narrow the list to just students because applicants can also be assigned to advisors.
            string[] studentIds = await DataReader.SelectAsync("STUDENTS", adviseeIds.ToArray(), string.Empty);
            watch.Stop();
            logger.Info("    STEPX.3.3 SELECT STUDENTS " + "... completed in " + watch.ElapsedMilliseconds.ToString());

            // Next sort them by name
            watch.Restart();
            searchString = "BY LAST.NAME BY FIRST.NAME BY MIDDLE.NAME";
            string[] studentIdsSorted = await DataReader.SelectAsync("PERSON", studentIds, searchString);

            watch.Stop();
            logger.Info("    STEPX.3.3.1 SELECT PERSON WITH " + searchString + "... completed in " + watch.ElapsedMilliseconds.ToString());

            // Finally, merge the two lists of IDs, removing duplicates.
            var studentIdsMerged = studentIdsReviewRequestedSorted != null && studentIdsReviewRequestedSorted.Count() != 0 ? studentIdsReviewRequestedSorted.Concat(studentIdsSorted) : studentIdsSorted.ToList();
            var studentIdsMergedNoDupes = new List<string>();
            foreach (var studentId in studentIdsMerged)
            {
                if (!studentIdsMergedNoDupes.Contains(studentId))
                {
                    studentIdsMergedNoDupes.Add(studentId);
                }
            }

            var totalItems = studentIdsMergedNoDupes.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            return studentIdsMergedNoDupes.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        /// <summary>
        /// Reads the student information from Colleague and returns an IEnumerable of Students Entity models.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list if none are found.</returns>
        public async Task<IEnumerable<Domain.Student.Entities.Student>> GetStudentsSearchAsync(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentException("ids", "You must specify at least 1 id to retrieve.");
            }
            else
            {
                var watch = new Stopwatch();
                if (logger.IsInfoEnabled) { watch.Start(); };

                var studentEntities = new List<Domain.Student.Entities.Student>();
                var studentsNotInCache = new List<string>();

                // For each requested student, first check for an entry in the cache.
                // If found, get it and add to return set.
                // Otherwise, add student ID to list of non-cached records to build/cache/return
                foreach (var id in ids)
                {
                    studentsNotInCache.Add(id);
                }

                // If any requested students not found in the cache, retrieve the data and build them now.
                if (studentsNotInCache != null && studentsNotInCache.Count() > 0)
                {
                    // Bulk read all the non-cached student records before looping and getting other data
                    var students = await DataReader.BulkReadRecordAsync<Students>(studentsNotInCache.ToArray());
                    var personRecords = (await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>(studentsNotInCache.ToArray())).ToList();
                    var personStRecords = (await DataReader.BulkReadRecordAsync<PersonSt>(studentsNotInCache.ToArray())).ToList();
                    var studentAdvisementIds = new List<string>();
                    foreach (var person in personStRecords)
                    {
                        if (person.PstAdvisement != null)
                        {
                            foreach (var advisementId in person.PstAdvisement)
                            {
                                studentAdvisementIds.Add(advisementId);
                            }
                        }
                    }
                    var studentAdvisementRecords = new Collection<StudentAdvisement>();
                    if (studentAdvisementIds != null && studentAdvisementIds.Count() > 0)
                    {
                        // Limit the advisements returned to current, exclude any that are ended.
                        string criteria = string.Format("WITH STAD.END.DATE GE '{0}' OR STAD.END.DATE EQ ''", DateTime.Today.ToShortDateString());
                        studentAdvisementIds = (await DataReader.SelectAsync("STUDENT.ADVISEMENT", studentAdvisementIds.ToArray(), criteria)).ToList();
                        if (studentAdvisementIds != null && studentAdvisementIds.Count() > 0)
                        {
                            studentAdvisementRecords = await DataReader.BulkReadRecordAsync<StudentAdvisement>(studentAdvisementIds.Distinct().ToArray());
                        }
                        if (logger.IsInfoEnabled)
                        {
                            var message = "Selected " + studentAdvisementIds.Count() + " STUDENT.ADVISEMENTS for the student(s) ";
                            foreach (var tempId in ids)
                            {
                                message += " " + tempId;
                            }
                            logger.Info(message);
                        }
                    }
                    else
                    {
                        if (logger.IsInfoEnabled)
                        {
                            var message = "No StudentAdvisement IDs found, STUDENT.ADVISEMENT not selected for the student(s) ";
                            foreach (var tempId in ids)
                            {
                                message += " " + tempId;
                            }
                            logger.Info(message);
                        }
                    }
                    var studentProgramIds = new List<string>();
                    foreach (var student in students)
                    {
                        if (student.StuAcadPrograms != null)
                        {
                            foreach (var acadProgramId in student.StuAcadPrograms)
                            {
                                studentProgramIds.Add(student.Recordkey + "*" + acadProgramId);
                            }
                        }
                    }
                    var studentProgramRecords = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.ToArray());

                    foreach (var student in students)
                    {
                        try
                        {
                            var studentProgramData = new List<StudentPrograms>();
                            if (studentProgramRecords != null)
                            {
                                studentProgramData.AddRange(studentProgramRecords.Where(x => x.Recordkey.Contains(student.Recordkey + "*")));
                            }

                            // Get PERSON.ST data contract from the previously acquired list
                            var personStData = personStRecords.FirstOrDefault(x => x.Recordkey == student.Recordkey);

                            // Get StudentAdvisement data
                            var studentAdvisementData = new List<StudentAdvisement>();
                            if (studentAdvisementRecords != null)
                            {
                                studentAdvisementData.AddRange(studentAdvisementRecords.Where(x => x.StadStudent == student.Recordkey));
                            }

                            // Get PERSON data
                            var personContract = personRecords.FirstOrDefault(XmlSettingsRepository => XmlSettingsRepository.Recordkey == student.Recordkey);

                            // Now that we have all the data, assemble the entity
                            var studentEntity = await BuildStudentAsync(student.Recordkey, student, studentProgramData, personStData, studentAdvisementData, personContract);


                            // Add this entity to the list of items to be returned
                            studentEntities.Add(studentEntity);

                            // Add the planningstudent to the cache ONLY IF IT HAS A DEGREE PLAN ID
                            if (studentEntity != null && studentEntity.DegreePlanId != null)
                            {
                                await AddOrUpdateCacheAsync<Domain.Student.Entities.Student>((studentCache + student.Recordkey), studentEntity, CacheTimeout);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info("Unable to build Student object for student Id " + student.Recordkey + ". Error: " + ex.Message);
                        }
                    }
                }

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("    STEPX.4.1 Building PlanningStudents completed in " + watch.ElapsedMilliseconds.ToString());
                }

                return studentEntities;
            }
        }

        /// <summary>
        /// Reads the required data from Colleague and returns a Students Entity model.
        /// </summary>
        /// <param name="studentId">Colleague Person (student) id.</param>
        /// <returns>Student Entity if found in Colleague, null if the student does not exist in Colleague.</returns>
        private async Task<Domain.Student.Entities.Student> BuildStudentAsync(string studentId, Students studentData, IEnumerable<StudentPrograms> studentProgramData,
            PersonSt personStData, IEnumerable<StudentAdvisement> studentAdvisementData, Base.DataContracts.Person personData)
        {
            Domain.Student.Entities.Student student = null;

            // This is not a valid student if either student or person not found.
            if (studentData != null && personData != null)
            {
                // Student Programs
                List<string> programIds = new List<string>();
                if (studentData.StuAcadPrograms != null)
                {
                    foreach (var acadProgramId in studentData.StuAcadPrograms)
                    {
                        StudentPrograms studentProgram = studentProgramData.Where(sp => sp.Recordkey == (studentId + "*" + acadProgramId)).FirstOrDefault();
                        if (studentProgram != null)
                        {
                            // If the program is withdrawn or dropped/changed-mind, skip it.
                            if (studentProgram.StprStatus.Count > 0)
                            {
                                var codeAssoc = (await GetStudentProgramStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == studentProgram.StprStatus.ElementAt(0)).FirstOrDefault();
                                if (codeAssoc != null && (codeAssoc.ValActionCode1AssocMember == "4" || codeAssoc.ValActionCode1AssocMember == "5"))
                                {
                                    continue;
                                }
                            }

                            // If student program has ended, skip it.
                            if (studentProgram.StprEndDate != null && studentProgram.StprEndDate.Count > 0 && studentProgram.StprEndDate.ElementAt(0) < DateTime.Today)
                            {
                                continue;
                            }

                            // If the program doesn't have a start date, skip it.
                            if (studentProgram.StprStartDate != null && studentProgram.StprStartDate.Count == 0)
                            {
                                continue;
                            }

                            // STUDENT.PROGRAMS key is multi-part.  Only save the program portion (second part) to the Student domain entity
                            programIds.Add(studentProgram.Recordkey.Split('*')[1]);
                        }
                    }
                }

                // DegreePlan
                // (Cannot bulk read because of potential for duplicate plans. )
                int? degreePlanId = null;
                string searchString = "DP.STUDENT.ID EQ '" + studentData.Recordkey + "'";
                string[] studentPlans = await DataReader.SelectAsync("DEGREE_PLAN", searchString);
                if (studentPlans.Count() > 0)
                {
                    // Sorting results in the event multiple plans are returned so that we always get the plan with the  smallest Id.
                    IEnumerable<int> studentPlanIds = studentPlans.Select(int.Parse);
                    degreePlanId = studentPlanIds.OrderBy(s => s).FirstOrDefault();
                }

                // Construct the Student entity
                student = new Domain.Student.Entities.Student(studentData.Recordkey, personData.LastName,
                    degreePlanId, programIds, new List<string>() { personData.PrivacyFlag });

                student.MiddleName = personData.MiddleName;
                student.FirstName = personData.FirstName;
                student.Prefix = personData.Prefix;
                student.Suffix = personData.Suffix;
                student.ChosenFirstName = personData.PersonChosenFirstName;
                student.ChosenMiddleName = personData.PersonChosenMiddleName;
                student.ChosenLastName = personData.PersonChosenLastName;
                student.BirthNameFirst = personData.BirthNameFirst;
                student.BirthNameMiddle = personData.BirthNameMiddle;
                student.BirthNameLast = personData.BirthNameLast;
                student.PersonalPronounCode = personData.PersonalPronoun;

                // Take the mail label name or preferred name override values from the data contract (which could be either a name or a coded entry) and 
                // convert it into simply a name override - where the coded entries are convered into their actual results.
                // In case of mail label name, it defaults to the preferred name override information unless it has its own.
                string mailLabelOverride = personData.PersonMailLabel != null && personData.PersonMailLabel.Any() ? string.Join(" ", personData.PersonMailLabel.ToArray()) : personData.PreferredName;
                student.MailLabelNameOverride = FormalNameFormat(mailLabelOverride, personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);
                student.PreferredNameOverride = FormalNameFormat(personData.PreferredName, personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);

                student.PreferredName = PersonNameService.FormatName(personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);
                if (personData.PFormatEntityAssociation != null && personData.PFormatEntityAssociation.Any())
                {
                    foreach (var pFormat in personData.PFormatEntityAssociation)
                    {
                        try
                        {
                            student.AddFormattedName(pFormat.PersonFormattedNameTypesAssocMember, pFormat.PersonFormattedNamesAssocMember);
                        }
                        catch (Exception)
                        {
                            logger.Info("Unable to add formatted name to person " + personData.Recordkey + " with type " + pFormat.PersonFormattedNameTypesAssocMember + " and name " + pFormat.PersonFormattedNamesAssocMember);
                        }
                    }
                }
                // Calculate the planning Student display name to use based on Student Display Hierarchy parameter
                StwebDefaults planningDefaults = await GetStwebDefaultsAsync();
                if (planningDefaults != null && !string.IsNullOrEmpty(planningDefaults.StwebDisplayNameHierarchy))
                {
                    // Calculate the person display name
                    NameAddressHierarchy hierarchy = null;
                    try
                    {
                        hierarchy = await GetCachedNameAddressHierarchyAsync(planningDefaults.StwebDisplayNameHierarchy);
                    }
                    catch (Exception)
                    {
                        logger.Info("Unable to find name address hierarchy with ID " + planningDefaults.StwebDisplayNameHierarchy + ". Not calculating hierarchy name.");

                    }
                    if (hierarchy != null)
                    {
                        student.PersonDisplayName = PersonNameService.GetHierarchyName(student, hierarchy);
                    }

                }
                // Student Advisement
                if (personStData != null && personStData.PstAdvisement != null && personStData.PstAdvisement.Count > 0)
                {
                    if (studentAdvisementData != null)
                    {
                        var advisements = ProcessAdvisements(studentData.Recordkey, studentAdvisementData.ToList());

                        foreach (var advisement in advisements)
                        {
                            student.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
                        }
                    }
                }

                // Registration Priorities
                if (studentData.StuRegPriorities != null && studentData.StuRegPriorities.Count > 0)
                {
                    foreach (string stuRegPriorityId in studentData.StuRegPriorities)
                    {
                        try
                        {
                            student.AddRegistrationPriority(stuRegPriorityId);
                        }
                        catch
                        {
                            // Don't bother logging if priority ID is null or this is a duplicate
                        }
                    }
                }

                // Educational Goal
                try
                {
                    if (personStData != null && personStData.EducGoalsEntityAssociation != null && personStData.EducGoalsEntityAssociation.Count() > 0)
                    {
                        // Get the goal with the latest associated date
                        var currGoal = personStData.EducGoalsEntityAssociation.OrderByDescending(g => g.PstEducGoalsChgdatesAssocMember).Select(g => g.PstEducGoalsAssocMember).FirstOrDefault();
                        // Translate to the external representation string
                        student.EducationalGoal = (await GetEducationalGoalsAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == currGoal).Select(v => v.ValExternalRepresentationAssocMember).FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e, string.Format("Unable to determine the educational goal for '{0}' ", studentData.Recordkey));
                }


                // Emails
                if (personData.PeopleEmailEntityAssociation != null && personData.PeopleEmailEntityAssociation.Count > 0)
                {
                    foreach (var emailData in personData.PeopleEmailEntityAssociation)
                    {
                        try
                        {
                            EmailAddress eAddress = new EmailAddress(emailData.PersonEmailAddressesAssocMember, emailData.PersonEmailTypesAssocMember);
                            eAddress.IsPreferred = emailData.PersonPreferredEmailAssocMember == "Y";
                            student.AddEmailAddress(eAddress);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, string.Format("Unable to process an Email Address for Student: '{0}'", studentData.Recordkey));
                        }
                    }
                }
            }

            return student;
        }
    }
}
