// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PlanningStudentRepository : StudentRepository, IPlanningStudentRepository
    {

        private string PlanningStudentCache = "PlanningStudent";
        private readonly string _colleagueTimeZone;

        public PlanningStudentRepository(
            ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory,
            ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            CacheTimeout = 5;
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }


        private async Task<ApplValcodes> GetStudentProgramStatusesAsync()
        {
            return await GetOrAddToCacheAsync<ApplValcodes>("StudentProgramStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access STUDENT.PROGRAM.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);

        }

        private async Task<ApplValcodes> GetEducationalGoalsAsync()
        {
            return await GetOrAddToCacheAsync<ApplValcodes>("EducationGoals",
                async () =>
                {
                    ApplValcodes educationGoalsTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EDUCATION.GOALS");
                    if (educationGoalsTable == null)
                    {
                        var errorMessage = "Unable to access EDUCATION.GOALS valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return educationGoalsTable;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Retrieves a student from the database using the provided Colleague student id.
        /// </summary>
        /// <param name="id">Colleague Person (student) id.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached data; defaults to true</param>
        /// <returns>A PlanningStudent Entity with data from Colleague.</returns>
        /// <remarks>
        /// The data retrieved from this method is not cached.
        /// </remarks>
        new public async Task<Domain.Student.Entities.PlanningStudent> GetAsync(string id, bool useCache = true)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Student ID may not be null or empty");
            }
            else
            {
                var studentEntity = (await GetAsync(new List<string>() { id }, useCache)).FirstOrDefault();

                return studentEntity;
            }
        }

        /// <summary>
        /// Reads the student information from Colleague and returns an IEnumerable of Students Entity models.
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <param name="useCache">Flag indicating whether or not to use cached data; defaults to true</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list if none are found.</returns>
        new public async Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> GetAsync(IEnumerable<string> ids, bool useCache = true)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentException("ids", "You must specify at least 1 id to retrieve.");
            }
            else
            {
                var watch = new Stopwatch();
                if (logger.IsInfoEnabled) { watch.Start(); };

                var planningStudentEntities = new List<Domain.Student.Entities.PlanningStudent>();
                var planningStudentsNotInCache = new List<string>();

                // For each requested student, first check for an entry in the cache.
                // If found, get it and add to return set.
                // Otherwise, add student ID to list of non-cached records to build/cache/return
                foreach (var id in ids)
                {
                    if (useCache)
                    {
                        string cacheKey = BuildFullCacheKey(PlanningStudentCache + id);
                        if (ContainsKey(cacheKey))
                        {
                            var planningStudent = (Domain.Student.Entities.PlanningStudent)_cacheProvider.Get(cacheKey);
                            planningStudentEntities.Add(planningStudent);
                        }
                        else
                        {
                            planningStudentsNotInCache.Add(id);
                        }
                    }
                    else
                    {
                        planningStudentsNotInCache.Add(id);
                    }
                }

                // If any requested students not found in the cache, retrieve the data and build them now.
                if (planningStudentsNotInCache != null && planningStudentsNotInCache.Count() > 0)
                {
                    // Bulk read all the non-cached STUDENTS records before looping and getting other data
                    var studentsBulkReadOutput = await DataReader.BulkReadRecordWithInvalidRecordsAsync<Students>(planningStudentsNotInCache.ToArray());
                    if (studentsBulkReadOutput.InvalidRecords != null && studentsBulkReadOutput.InvalidRecords.Any())
                    {
                        logger.Error("Following Students records could not be read:");
                        foreach (var invalidRecord in studentsBulkReadOutput.InvalidRecords)
                        {
                            logger.Error(String.Format("Student Id {0} has bad data {1}", invalidRecord.Key, invalidRecord.Value));
                        }
                    }
                    var students = new Collection<Students>();
                    if (studentsBulkReadOutput.BulkRecordsRead != null)
                    {
                        students = studentsBulkReadOutput.BulkRecordsRead;
                    }
                    else
                    {
                        logger.Info(string.Format("There are no STUDENTS records for the given advisees list"));
                    }

                    // Bulk read all the PERSON records before looping and getting other data
                    var personRecordsBulkReadOutput = (await DataReader.BulkReadRecordWithInvalidRecordsAsync<Base.DataContracts.Person>(planningStudentsNotInCache.ToArray()));
                    if (personRecordsBulkReadOutput.InvalidRecords != null && personRecordsBulkReadOutput.InvalidRecords.Any())
                    {
                        logger.Error("Following Person records could not be read:");
                        foreach (var invalidRecord in studentsBulkReadOutput.InvalidRecords)
                        {
                            logger.Error(String.Format("Person Id {0} has bad data {1}", invalidRecord.Key, invalidRecord.Value));
                        }
                    }
                    var personRecords = new Collection<Base.DataContracts.Person>();
                    if (personRecordsBulkReadOutput.BulkRecordsRead != null)
                    {
                        personRecords = personRecordsBulkReadOutput.BulkRecordsRead;
                    }
                    else
                    {
                        logger.Info(string.Format("There are no PERSON records for the given advisees list"));
                    }

                    // Bulk read all the PERSON.ST records before looping and getting other data
                    var personStRecordsBulkReadOutput = (await DataReader.BulkReadRecordWithInvalidRecordsAsync<PersonSt>(planningStudentsNotInCache.ToArray()));
                    if (personStRecordsBulkReadOutput.InvalidRecords != null && personStRecordsBulkReadOutput.InvalidRecords.Any())
                    {
                        logger.Error("Following Person-ST records could not be read:");
                        foreach (var invalidRecord in studentsBulkReadOutput.InvalidRecords)
                        {
                            logger.Error(String.Format("Person Id {0} has bad data {1}", invalidRecord.Key, invalidRecord.Value));
                        }
                    }
                    var personStRecords = new Collection<PersonSt>();
                    if (personStRecordsBulkReadOutput.BulkRecordsRead != null)
                    {
                        personStRecords = personStRecordsBulkReadOutput.BulkRecordsRead;
                    }
                    else
                    {
                        logger.Info(string.Format("There are no PERSON.ST records for the given advisees list"));
                    }

                    //Reading STUDENT.ADVISEMENT records from the PERSON.ST pointers
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
                    logger.Info("Selected " + studentAdvisementIds.Count() + " PERSON.ST pointers on STUDENT.ADVISEMENT for the student(s)");

                    var studentAdvisementRecords = new Collection<StudentAdvisement>();
                    if (studentAdvisementIds != null && studentAdvisementIds.Count() > 0)
                    {
                        // Limit the advisements returned to current, exclude any that are ended.
                        var date = await GetUnidataFormatDateAsync(DateTime.Today);

                        string criteria = string.Format("WITH STAD.END.DATE GE '{0}' OR STAD.END.DATE EQ ''", date);
                        studentAdvisementIds = (await DataReader.SelectAsync("STUDENT.ADVISEMENT", studentAdvisementIds.ToArray(), criteria)).ToList();
                        if (studentAdvisementIds != null && studentAdvisementIds.Count() > 0)
                        {
                            var studentAdvisementRecordsBulkReadOutput = await DataReader.BulkReadRecordWithInvalidRecordsAsync<StudentAdvisement>(studentAdvisementIds.Distinct().ToArray());
                            if (studentAdvisementRecordsBulkReadOutput.InvalidRecords != null && studentAdvisementRecordsBulkReadOutput.InvalidRecords.Any())
                            {
                                logger.Error("Following Student advisement records could not be read:");
                                foreach (var invalidRecord in studentAdvisementRecordsBulkReadOutput.InvalidRecords)
                                {
                                    logger.Error(String.Format("Student Advisement Id {0} has bad data {1}", invalidRecord.Key, invalidRecord.Value));
                                }
                            }
                            studentAdvisementRecords = studentAdvisementRecordsBulkReadOutput.BulkRecordsRead;
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

                    //Reading STUDENT.PROGRAMS from STU.ACAD.PROGRAMS pointers
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
                    var studentProgramRecordsBulkReadOutput = await DataReader.BulkReadRecordWithInvalidRecordsAsync<StudentPrograms>(studentProgramIds.ToArray());
                    if (studentProgramRecordsBulkReadOutput.InvalidRecords != null && studentProgramRecordsBulkReadOutput.InvalidRecords.Any())
                    {
                        logger.Error("Following Student program records could not be read:");
                        foreach (var invalidRecord in studentProgramRecordsBulkReadOutput.InvalidRecords)
                        {
                            logger.Error(String.Format("Student Program Id {0} has bad data {1}", invalidRecord.Key, invalidRecord.Value));
                        }
                    }
                    var studentProgramRecords = studentProgramRecordsBulkReadOutput.BulkRecordsRead;

                    //Start building PlanningStudent entities for all the students records retrieved
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
                            var planningStudentEntity = await BuildPlanningStudentAsync(student.Recordkey, student, studentProgramData, personStData, studentAdvisementData, personContract);
                            if (planningStudentEntity != null)
                            {
                                // Add this entity to the list of items to be returned
                                planningStudentEntities.Add(planningStudentEntity);
                            }
                            else
                            {
                                logger.Error(String.Format("Planning Student entity is null for student id {0}", student.Recordkey));
                            }

                            // Add the planningstudent to the cache ONLY IF IT HAS A DEGREE PLAN ID
                            if (planningStudentEntity != null && planningStudentEntity.DegreePlanId != null)
                            {
                                await AddOrUpdateCacheAsync<Domain.Student.Entities.PlanningStudent>((PlanningStudentCache + student.Recordkey), planningStudentEntity, CacheTimeout);
                            }
                        }
                        catch (ColleagueSessionExpiredException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("Unable to build PlanningStudent object for student {0}", student.Recordkey));
                        }
                    }
                }

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("    STEPX.4.1 Building PlanningStudents completed in " + watch.ElapsedMilliseconds.ToString());
                }

                return planningStudentEntities;
            }
        }

        /// <summary>
        /// Reads the required data from Colleague and returns a Students Entity model.
        /// </summary>
        /// <param name="studentId">Colleague Person (student) id.</param>
        /// <returns>Student Entity if found in Colleague, null if the student does not exist in Colleague.</returns>
        private async Task<Domain.Student.Entities.PlanningStudent> BuildPlanningStudentAsync(
            string studentId, Students studentData, IEnumerable<StudentPrograms> studentProgramData,
            PersonSt personStData, IEnumerable<StudentAdvisement> studentAdvisementData,
            Base.DataContracts.Person personData)
        {
            Domain.Student.Entities.PlanningStudent planningStudent = null;

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
                                    logger.Info(String.Format("Student {0} for program {1} status is withdrawn or dropped or changed-mind", studentData.Recordkey, studentProgram.Recordkey));
                                    continue;
                                }
                            }

                            // If student program has ended, skip it.
                            if (studentProgram.StprEndDate != null && studentProgram.StprEndDate.Count > 0 && studentProgram.StprEndDate.ElementAt(0) < DateTime.Today)
                            {
                                logger.Info(String.Format("Student {0} for program {1} has ended", studentData.Recordkey, studentProgram.Recordkey));
                                continue;
                            }

                            // If the program doesn't have a start date, skip it.
                            if (studentProgram.StprStartDate != null && studentProgram.StprStartDate.Count == 0)
                            {
                                logger.Info(String.Format("Student {0} for program {1} doesn't have start date", studentData.Recordkey, studentProgram.Recordkey));
                                continue;
                            }

                            // STUDENT.PROGRAMS key is multi-part.  Only save the program portion (second part) to the Student domain entity
                            programIds.Add(studentProgram.Recordkey.Split('*')[1]);
                        }
                        else
                        {
                            logger.Error(String.Format("There is no corresponding STUDENT.PROGRAMS record for STU.ACAD.PROGRAM Id {0} pointer for student {1}", acadProgramId, studentData.Recordkey));
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
                else
                {
                    logger.Debug(String.Format("Student {0} does not have a degree plan", studentData.Recordkey));
                }

                // Construct the Student entity
                planningStudent = new Domain.Student.Entities.PlanningStudent(
                    studentData.Recordkey, personData.LastName, degreePlanId, programIds, personData.PrivacyFlag);
                planningStudent.MiddleName = personData.MiddleName;
                planningStudent.FirstName = personData.FirstName;
                planningStudent.Prefix = personData.Prefix;
                planningStudent.Suffix = personData.Suffix;
                planningStudent.ChosenFirstName = personData.PersonChosenFirstName;
                planningStudent.ChosenMiddleName = personData.PersonChosenMiddleName;
                planningStudent.ChosenLastName = personData.PersonChosenLastName;
                planningStudent.BirthNameFirst = personData.BirthNameFirst;
                planningStudent.BirthNameMiddle = personData.BirthNameMiddle;
                planningStudent.BirthNameLast = personData.BirthNameLast;
                planningStudent.PersonalPronounCode = personData.PersonalPronoun;

                // Take the mail label name or preferred name override values from the data contract (which could be either a name or a coded entry) and 
                // convert it into simply a name override - where the coded entries are convered into their actual results.
                // In case of mail label name, it defaults to the preferred name override information unless it has its own.
                string mailLabelOverride = personData.PersonMailLabel != null && personData.PersonMailLabel.Any() ? string.Join(" ", personData.PersonMailLabel.ToArray()) : personData.PreferredName;
                planningStudent.MailLabelNameOverride = FormalNameFormat(mailLabelOverride, personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);
                planningStudent.PreferredNameOverride = FormalNameFormat(personData.PreferredName, personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);

                planningStudent.PreferredName = PersonNameService.FormatName(personData.Prefix, personData.FirstName, personData.MiddleName, personData.LastName, personData.Suffix);
                if (personData.PFormatEntityAssociation != null && personData.PFormatEntityAssociation.Any())
                {
                    foreach (var pFormat in personData.PFormatEntityAssociation)
                    {
                        try
                        {
                            planningStudent.AddFormattedName(pFormat.PersonFormattedNameTypesAssocMember, pFormat.PersonFormattedNamesAssocMember);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Unable to add formatted name to person " + personData.Recordkey + " with type " + pFormat.PersonFormattedNameTypesAssocMember);
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
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Unable to find name address hierarchy with ID " + planningDefaults.StwebDisplayNameHierarchy + ". Not calculating hierarchy name.");

                    }
                    if (hierarchy != null)
                    {
                        planningStudent.PersonDisplayName = PersonNameService.GetHierarchyName(planningStudent, hierarchy);
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
                            planningStudent.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
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
                            planningStudent.AddRegistrationPriority(stuRegPriorityId);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Priority ID is null or is a duplicatel.");
                        }
                    }
                }

                //PhoneTypes Hierarchy
                planningStudent.PhoneTypesHierarchy = planningDefaults.StwebProfilePhoneType;

                // Educational Goal
                try
                {
                    if (personStData != null && personStData.EducGoalsEntityAssociation != null && personStData.EducGoalsEntityAssociation.Count() > 0)
                    {
                        // Get the goal with the latest associated date
                        var currGoal = personStData.EducGoalsEntityAssociation.OrderByDescending(g => g.PstEducGoalsChgdatesAssocMember).Select(g => g.PstEducGoalsAssocMember).FirstOrDefault();
                        // Translate to the external representation string
                        planningStudent.EducationalGoal = (await GetEducationalGoalsAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == currGoal).Select(v => v.ValExternalRepresentationAssocMember).FirstOrDefault();
                    }
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error(e, string.Format("Unable to determine the educational goal for '{0}' ", studentData.Recordkey));
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
                            planningStudent.AddEmailAddress(eAddress);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("Unable to process an Email Address for Student: '{0}'", studentData.Recordkey));
                        }
                    }
                }

                // Completed Advisements
                if (studentData.StuAdvisementsEntityAssociation != null && studentData.StuAdvisementsEntityAssociation.Any())
                {
                    foreach (var adv in studentData.StuAdvisementsEntityAssociation)
                    {
                        try
                        {
                            if (adv.StuAdviseCompleteDateAssocMember.HasValue && adv.StuAdviseCompleteTimeAssocMember.HasValue)
                            {
                                planningStudent.AddCompletedAdvisement(adv.StuAdviseCompleteAdvisorAssocMember,
                                    adv.StuAdviseCompleteDateAssocMember.Value,
                                    adv.StuAdviseCompleteTimeAssocMember.ToTimeOfDayDateTimeOffset(_colleagueTimeZone).Value);
                            }
                            else
                            {
                                throw new ApplicationException("A completed advisement for student " + studentData.Recordkey + " does not have an associated date and/or time.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("Unable to process a completed advisement for Student: '{0}'", studentData.Recordkey));
                        }
                    }
                }
            }

            return planningStudent;
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> GetStwebDefaultsAsync()
        {

            var result = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("PlanningStudentWebDefaults",
            async () =>
            {
                Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
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
    }
}