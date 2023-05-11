//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Services;
namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Higher education institution admission applications.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AdmissionApplicationsRepository : BaseColleagueRepository, IAdmissionApplicationsRepository
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AdmissionApplicationsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        RepositoryException exception = new RepositoryException();

        const int AllAdmissionApplicationsCacheTimeout = 20; // Clear from cache every 20 minutes
        const string AllAdmissionApplicationsCache = "AllAdmissionApplications";

        #region GET methods

        /// <summary>
        /// Get AdmissionApplications domain entity collection
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>A tuple consisting of 1) collection of AdmissionApplication doamin entities, 2) count</returns>
        public async Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache)
        {
            var applicationEntities = new List<AdmissionApplication>();
            int totalCount = 0;
            string[] subList = null;
            string[] applicationIds = null;
            var criteria = "WITH APPL.APPLICANT NE ''";
                     
            var applStatusesSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var currStatus = string.Empty;
            foreach (var applStatusesSpCodeId in applStatusesSpCodeIds)
            {
                currStatus += string.Format("AND APPL.CURRENT.STATUS NE '{0}' ", applStatusesSpCodeId);
            }

            applicationIds = (applStatusesSpCodeIds != null && applStatusesSpCodeIds.Any())
                ? await DataReader.SelectAsync("APPLICATIONS", string.Concat(criteria, " ", currStatus))
                : await DataReader.SelectAsync("APPLICATIONS", criteria);

            totalCount = applicationIds.Count();
            Array.Sort(applicationIds);
            subList = applicationIds.Skip(offset).Take(limit).ToArray();
            var applicationDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Applications>("APPLICATIONS", subList);

            if (applicationDataContracts != null && applicationDataContracts.Any())
            {
                var studentProgramIds = new List<string>();
                studentProgramIds.AddRange(applicationDataContracts.Where(i => !string.IsNullOrEmpty(i.ApplAcadProgram)).Select(i => string.Concat(i.ApplApplicant, "*", i.ApplAcadProgram)));
                var programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.Distinct().ToArray());

                foreach (var applicationDataContract in applicationDataContracts)
                {
                    applicationEntities.Add(BuildAdmissionApplication(applicationDataContract, programData));
                }
            }

            return applicationEntities.Any() ?
                new Tuple<IEnumerable<AdmissionApplication>, int>(applicationEntities, totalCount) :
                new Tuple<IEnumerable<AdmissionApplication>, int>(new List<AdmissionApplication>(), 0);
        }

        /// <summary>
        /// Get AdmissionApplications domain entity collection with filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>A tuple consisting of 1) collection of AdmissionApplication doamin entities, 2) count</returns>
        public async Task<Tuple<IEnumerable<AdmissionApplication>, int>> GetAdmissionApplications2Async(int offset, int limit, string applicant = "", string academicPeriod = "", string personFilter = "", string[] filterPersonIds = null, bool bypassCache = false)
        {
            var applicationEntities = new List<AdmissionApplication>();
            int totalCount = 0;
            string[] subList = null;
            string[] applicationIds = null;

            string allAdmissionApplicationsCacheKey = CacheSupport.BuildCacheKey(AllAdmissionApplicationsCache, applicant, academicPeriod, personFilter,
                    filterPersonIds);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
               this,
               ContainsKey,
               GetOrAddToCacheAsync,
               AddOrUpdateCacheAsync,
               transactionInvoker,
               allAdmissionApplicationsCacheKey,
               "APPLICATIONS",
               offset,
               limit,
               AllAdmissionApplicationsCacheTimeout,
               async () =>
               {
                   string[] limitingKeys = new string[] { };
                   var criteria = "WITH APPL.APPLICANT NE ''";
                   // Applicant Filter
                   if (!string.IsNullOrEmpty(applicant))
                   {
                       criteria = string.Format("WITH APPL.APPLICANT = '{0}'", applicant);
                   }
                   // Academic Period Filter
                   if (!string.IsNullOrEmpty(academicPeriod))
                   {
                       criteria = string.Concat(criteria, " AND APPL.START.TERM EQ '", academicPeriod, "'");
                   }

                   var applStatusesSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

                   var currStatus = string.Empty;
                   foreach (var applStatusesSpCodeId in applStatusesSpCodeIds)
                   {
                       currStatus += string.Format("AND APPL.CURRENT.STATUS NE '{0}' ", applStatusesSpCodeId);
                   }

                   #region  Named query person filter
                   if (filterPersonIds != null && filterPersonIds.Any())
                   {
                       // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                       limitingKeys = filterPersonIds;
                       var applicantApplicationId = await DataReader.SelectAsync("APPLICANTS", limitingKeys, "WITH APP.APPLICATIONS NE '' BY.EXP APP.APPLICATIONS SAVING APP.APPLICATIONS");
                       if (applicantApplicationId == null || !applicantApplicationId.Any())
                       {
                           return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                       }
                       limitingKeys = applicantApplicationId;
                   }
                   #endregion

                   applicationIds = (applStatusesSpCodeIds != null && applStatusesSpCodeIds.Any())
                       ? await DataReader.SelectAsync("APPLICATIONS", limitingKeys, string.Concat(criteria, " ", currStatus))
                       : await DataReader.SelectAsync("APPLICATIONS", limitingKeys, criteria);
                   
                   if (applicationIds == null || !applicationIds.Any())
                   {
                       return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                   }

                   CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                   {
                       limitingKeys = applicationIds.ToList()
                   };

                   return requirements;

               });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<AdmissionApplication>, int>(new List<AdmissionApplication>(), 0);
            }

            totalCount = keyCache.TotalCount.Value;
            subList = keyCache.Sublist.ToArray();
            var applicationDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Applications>("APPLICATIONS", subList);                       

            if (applicationDataContracts != null && applicationDataContracts.Any())
            {
                // Bulk read the applicants for the applications
                Collection<Applicants> applicantDataContracts = null;
                var applicantIds = applicationDataContracts.Where(ap => (!string.IsNullOrWhiteSpace(ap.ApplApplicant)))
                          .Select(ap => ap.ApplApplicant).Distinct().ToArray();
                if (applicantIds != null && applicantIds.Any())
                {
                    var applicants = await DataReader.SelectAsync("APPLICANTS", applicantIds, "");
                    if (applicants != null && applicants.Any())
                    {
                        applicantDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Applicants>(applicants);
                    }
                }

                var studentProgramIds = new List<string>();
                studentProgramIds.AddRange(applicationDataContracts.Where(i => !string.IsNullOrEmpty(i.ApplAcadProgram)).Select(i => string.Concat(i.ApplApplicant, "*", i.ApplAcadProgram)));
                var programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.Distinct().ToArray());

                foreach (var applicationDataContract in applicationDataContracts)
                {
                    Applicants applicantDataContract = null;
                    if (applicantDataContracts != null)
                    {
                        //applicantDataContract = applicantDataContracts.Where(app => app.Recordkey == applicationDataContract.ApplApplicant).FirstOrDefault();
                        applicantDataContract = applicantDataContracts.FirstOrDefault(app => app.Recordkey == applicationDataContract.ApplApplicant);
                    }
                    // passing in a null for personOriginCodes in 3rd argument since it is not returned in the admission applications DTO. 
                    // Its only in the build for get guid so that partial put merge logic never loses it.
                    applicationEntities.Add(BuildAdmissionApplication2(applicationDataContract, applicantDataContract, programData, null));
                }
            }

            return applicationEntities.Any() ?
                new Tuple<IEnumerable<AdmissionApplication>, int>(applicationEntities, totalCount) :
                new Tuple<IEnumerable<AdmissionApplication>, int>(new List<AdmissionApplication>(), 0);
        }

        /// <summary>
        /// Get admission application by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> GetAdmissionApplicationByIdAsync(string guid)
        {
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });

            if (idDict == null || !idDict.Any())
            {
                throw new KeyNotFoundException("APPLICATIONS GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("APPLICATIONS GUID " + guid + " not found.");
            }

            if (foundEntry.Value != null && !string.IsNullOrWhiteSpace(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException("APPLICATIONS GUID " + guid + " not found.");
            }

            var admissionApplicationId = foundEntry.Value.PrimaryKey;
            if (string.IsNullOrEmpty(admissionApplicationId))
            {
                throw new KeyNotFoundException("admission-applications key not found for GUID " + guid);
            }

            var applicationStatuses = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var admissionApplicationDataContract = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", admissionApplicationId);
            if (admissionApplicationDataContract == null)
            {
                throw new KeyNotFoundException("admission-applications data contract not found for Id " + admissionApplicationId);
            }

            if ((applicationStatuses != null) && (applicationStatuses.Any())
                    && (admissionApplicationDataContract.ApplStatusesEntityAssociation != null && admissionApplicationDataContract.ApplStatusesEntityAssociation.Any()))
            {
                var statuses = applicationStatuses.Select(rk => rk.Recordkey).Distinct().ToArray();

                var admApplStatuses = admissionApplicationDataContract.ApplStatusesEntityAssociation.FirstOrDefault();

                if (statuses.Contains(admApplStatuses.ApplStatusAssocMember))
                {
                    throw new KeyNotFoundException("admission-applications not found for GUID " + guid);
                }
            }
            var studentProgramIds = new List<string>();
            studentProgramIds.Add(string.Concat(admissionApplicationDataContract.ApplApplicant, "*", admissionApplicationDataContract.ApplAcadProgram));
            var programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.Distinct().ToArray());

            return BuildAdmissionApplication(admissionApplicationDataContract, programData);
        }

        /// <summary>
        /// Get admission application by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> GetAdmissionApplicationById2Async(string guid)
        {
            var applicationStatuses = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var admissionApplicationDataContract = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", new GuidLookup(guid));
            if (admissionApplicationDataContract == null)
            {
                throw new KeyNotFoundException("No admission-applications was found for guid " + guid);
            }

            if ((applicationStatuses != null) && (applicationStatuses.Any())
                    && (admissionApplicationDataContract.ApplStatusesEntityAssociation != null && admissionApplicationDataContract.ApplStatusesEntityAssociation.Any()))
            {
                var statuses = applicationStatuses.Select(rk => rk.Recordkey).Distinct().ToArray();

                var admApplStatuses = admissionApplicationDataContract.ApplStatusesEntityAssociation.FirstOrDefault();

                if (statuses.Contains(admApplStatuses.ApplStatusAssocMember))
                {
                    throw new KeyNotFoundException("admission-applications not found for guid " + guid);
                }
            }

            DataContracts.Applicants applicantDataContract = null;            
            if (admissionApplicationDataContract != null && (!string.IsNullOrEmpty(admissionApplicationDataContract.ApplApplicant)))
            {
                applicantDataContract = await DataReader.ReadRecordAsync<Applicants>("APPLICANTS", admissionApplicationDataContract.ApplApplicant);
            }

            var studentProgramIds = new List<string>();
            studentProgramIds.Add(string.Concat(admissionApplicationDataContract.ApplApplicant, "*", admissionApplicationDataContract.ApplAcadProgram));
            var programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.Distinct().ToArray());

            string personOriginCode = string.Empty;
            string[] columns = new string[] { "PERSON.ORIGIN.CODE" };
            try
            {
                var personOriginCodes = await DataReader.ReadRecordColumnsAsync("PERSON", admissionApplicationDataContract.ApplApplicant, columns);
                if (personOriginCodes != null && personOriginCodes.Any() && !string.IsNullOrEmpty(personOriginCodes.FirstOrDefault().Value))
                {
                    personOriginCode = personOriginCodes.FirstOrDefault().Value;
                }
            }
            catch
            {
                exception.AddError(new RepositoryError("Bad.Data", string.Format("PERSON record missing or invalid for applicant {0}.", admissionApplicationDataContract.ApplApplicant))
                {
                    SourceId = admissionApplicationDataContract.Recordkey,
                    Id = guid
                });
                throw exception;
            }

            return BuildAdmissionApplication2(admissionApplicationDataContract, applicantDataContract, programData, personOriginCode);
        }


        public async Task<Dictionary<string, string>> GetStaffOperIdsAsync(List<string> ids)
        {
            var opersData = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", ids.ToArray(), true);
            var staffData = await DataReader.BulkReadRecordAsync<Staff>(ids.ToArray());
            var combinedIds = new Dictionary<string, string>();

            if (opersData != null && opersData.Any())
            {
                opersData.ToList().ForEach(i =>
                {
                    if (!string.IsNullOrEmpty(i.SysPersonId) && !combinedIds.ContainsKey(i.Recordkey))
                    {
                        combinedIds.Add(i.Recordkey, i.SysPersonId);
                    }
                });
            }

            if (staffData != null && staffData.Any())
            {
                staffData.ToList().ForEach(i =>
                {
                    if (!string.IsNullOrEmpty(i.Recordkey) && !string.IsNullOrEmpty(i.StaffLoginId) && !combinedIds.ContainsKey(i.StaffLoginId))
                    {
                        combinedIds.Add(i.StaffLoginId, i.Recordkey);
                    }
                });
            }
            return combinedIds;
        }

        #endregion

        #region Build method

        /// <summary>
        /// BuildAdmissionApplication
        /// </summary>
        /// <param name="application"></param>
        /// <param name="programData"></param>
        /// <returns>AdmissionApplication domain entity</returns>
        private AdmissionApplication BuildAdmissionApplication(Applications application,
            Collection<StudentPrograms> programData)   
        {
            try
            {
                var applicationEntity = new AdmissionApplication(application.RecordGuid, application.Recordkey);

                applicationEntity.ApplicantPersonId = application.ApplApplicant;
                applicationEntity.ApplicationNo = application.ApplNo;
                applicationEntity.ApplicationStartTerm = application.ApplStartTerm;
                applicationEntity.AdmissionApplicationStatuses = BuildAdmissionApplicationStatuses(application.ApplStatusesEntityAssociation);
                applicationEntity.ApplicationSource = application.ApplSource;
                applicationEntity.ApplicationAdmissionsRep = application.ApplAdmissionsRep;
                applicationEntity.ApplicationAdmitStatus = application.ApplAdmitStatus;
                applicationEntity.ApplicationLocations = application.ApplLocations;
                applicationEntity.ApplicationResidencyStatus = application.ApplResidencyStatus;
                applicationEntity.ApplicationStudentLoadIntent = application.ApplStudentLoadIntent;
                applicationEntity.ApplicationAcadProgram = application.ApplAcadProgram;
                applicationEntity.ApplicationStprAcadPrograms = BuildProgramCodeList(application, programData);
                applicationEntity.ApplicationWithdrawReason = application.ApplWithdrawReason;
                applicationEntity.ApplicationAttendedInstead = application.ApplAttendedInstead;
                applicationEntity.ApplicationComments = application.ApplComments;
                applicationEntity.ApplicationSchool = BuildSchool(string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram), programData);

                //V11 changes
                applicationEntity.ApplicationWithdrawDate = application.ApplWithdrawDate;

                return applicationEntity;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
        }

        /// <summary>
        /// BuildAdmissionApplication
        /// </summary>
        /// <param name="application"></param>
        /// <param name="applicant"></param>
        /// <param name="programData"></param>
        /// <param name="personOriginCode"></param>
        /// <returns>AdmissionApplication domain entity</returns>
        private AdmissionApplication BuildAdmissionApplication2(Applications application, Applicants applicant,
            Collection<StudentPrograms> programData, string personOriginCode)
        {
            try
            {
                var applicationEntity = new AdmissionApplication(application.RecordGuid, application.Recordkey);

                applicationEntity.ApplicantPersonId = application.ApplApplicant;
                applicationEntity.ApplicationNo = application.ApplNo;
                applicationEntity.ApplicationStartTerm = application.ApplStartTerm;
                applicationEntity.AdmissionApplicationStatuses = BuildAdmissionApplicationStatuses(application.ApplStatusesEntityAssociation);
                applicationEntity.ApplicationSource = application.ApplSource;
                applicationEntity.ApplicationAdmissionsRep = application.ApplAdmissionsRep;
                applicationEntity.ApplicationAdmitStatus = application.ApplAdmitStatus;
                applicationEntity.ApplicationLocations = application.ApplLocations;
                applicationEntity.ApplicationResidencyStatus = application.ApplResidencyStatus;
                applicationEntity.ApplicationStudentLoadIntent = application.ApplStudentLoadIntent;
                applicationEntity.ApplicationAcadProgram = application.ApplAcadProgram;                
                applicationEntity.CareerGoals = application.ApplIntgCareerGoals;
                applicationEntity.Influences = application.ApplInfluencedToApply;

                if (applicant != null)
                {
                    applicationEntity.EducationalGoal = applicant.AppOrigEducGoal;
                }

                var key = string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram);
                if (programData == null || !programData.Any())
                {
                    throw new RepositoryException(string.Format("Student program not found for: {0}. ", key));
                }
                var program = programData.FirstOrDefault(i => i.Recordkey.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (program == null || string.IsNullOrEmpty(program.RecordGuid))
                {
                    throw new RepositoryException(string.Format("Student program not found for: {0}. ", key));
                }
                else
                {
                    applicationEntity.ApplicationAcadProgramGuid = program.RecordGuid;
                    applicationEntity.ApplicationStprAcadPrograms = BuildProgramCodeList(application, programData);
                }
                applicationEntity.ApplicationWithdrawReason = application.ApplWithdrawReason;
                applicationEntity.ApplicationAttendedInstead = application.ApplAttendedInstead;
                applicationEntity.ApplicationComments = application.ApplComments;
                applicationEntity.ApplicationWithdrawDate = application.ApplWithdrawDate;
                applicationEntity.PersonSource = personOriginCode;

                return applicationEntity;
            }
            catch (KeyNotFoundException ex)
            {
                throw new RepositoryException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
        }

        /// <summary>
        /// STPR_MAJOR_LIST, STPR_MINOR_LIST, STPR_SPECIALTIES
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private List<string> BuildProgramCodeList(Applications application, Collection<StudentPrograms> programData)
        {
            var programs = new List<string>();

            if (programData == null || (programData != null && !programData.Any()))
            {
                return programs;
            }
            var key = string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram);

            var program = programData.FirstOrDefault(i => i.Recordkey.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (program == null)
            {
                throw new KeyNotFoundException(string.Format("Student program not found for: {0}. ", key));
            }

            //STPR_MAJOR_LIST
            if (program.StprMajorListEntityAssociation != null && program.StprMajorListEntityAssociation.Any())
            {
                foreach (var major in program.StprMajorListEntityAssociation)
                {
                    if (!programs.Contains(major.StprAddnlMajorsAssocMember))
                    {
                        programs.Add(major.StprAddnlMajorsAssocMember);
                    }
                }
            }

            //STPR_MINOR_LIST
            if (program.StprMinorListEntityAssociation != null && program.StprMinorListEntityAssociation.Any())
            {
                foreach (var minor in program.StprMinorListEntityAssociation)
                {
                    if (!programs.Contains(minor.StprMinorsAssocMember))
                    {
                        programs.Add(minor.StprMinorsAssocMember);
                    }
                }
            }

            //STPR_SPECIALTIES
            if (program.StprSpecialtiesEntityAssociation != null && program.StprSpecialtiesEntityAssociation.Any())
            {
                foreach (var specialty in program.StprSpecialtiesEntityAssociation)
                {
                    if (!programs.Contains(specialty.StprSpecializationsAssocMember))
                    {
                        programs.Add(specialty.StprSpecializationsAssocMember);
                    }
                }
            }
            return programs.Any() ? programs : null;
        }

        /// <summary>
        /// Gets school guid
        /// </summary>
        /// <param name="acadProgramKey"></param>
        /// <returns></returns>
        private string BuildSchool(string acadProgramKey, Collection<StudentPrograms> programData)
        {
            try
            {
                if (programData == null || (programData != null && !programData.Any()))
                {
                    return string.Empty;
                }
                var program = programData.FirstOrDefault(i => i.Recordkey.Equals(acadProgramKey, StringComparison.OrdinalIgnoreCase));
                if (program == null)
                {
                    throw new KeyNotFoundException(string.Format("Student program not found for: {0}. ", acadProgramKey));
                }
                return program.StprSchool;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Builds application statuses.
        /// </summary>
        /// <param name="applStatusesAssociation"></param>
        /// <returns></returns>
        private List<Domain.Student.Entities.AdmissionApplicationStatus> BuildAdmissionApplicationStatuses(List<ApplicationsApplStatuses> applStatusesAssociation)
        {
            var applStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>();
            if (applStatusesAssociation != null && applStatusesAssociation.Any())
            {
                foreach (var item in applStatusesAssociation)
                {
                    var admApplStatus = new Domain.Student.Entities.AdmissionApplicationStatus()
                    {
                        ApplicationDecisionBy = item.ApplDecisionByAssocMember,
                        ApplicationStatus = item.ApplStatusAssocMember,
                        ApplicationStatusDate = item.ApplStatusDateAssocMember,
                        ApplicationStatusTime = item.ApplStatusTimeAssocMember
                    };
                    applStatuses.Add(admApplStatus);
                }
            }
            return applStatuses;
        }

        /// <summary>
        /// Builds decision by.
        /// </summary>
        /// <param name="applicationsApplStatuses"></param>
        /// <returns></returns>
        private List<string> BuildApplicationDecisionBy(List<ApplicationsApplStatuses> applicationsApplStatuses)
        {
            var decisionBy = new List<string>();

            var items = applicationsApplStatuses.OrderByDescending(i => i.ApplStatusDateAssocMember);
            items.ToList().ForEach(i => decisionBy.Add(i.ApplDecisionByAssocMember));

            return decisionBy.Any() ? decisionBy : null;
        }

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for APPLICATIONS.
        /// </summary>
        /// <param name="applicationIds"></param>
        /// <returns>Dictionary containing entity PK (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetAdmissionApplicationGuidDictionary(IEnumerable<string> applicationIds)
        {
            var ldmGuidCollection = new Dictionary<string, string>();

            if ((applicationIds == null) || (!applicationIds.Any()))
            {
                return ldmGuidCollection;
            }

            var lookups = applicationIds.Select(i => new RecordKeyLookup( "APPLICATIONS", i, string.Empty, string.Empty, false ));
            var ldmGuidRecords = await DataReader.SelectAsync( lookups.ToArray() );

            var exception = new RepositoryException();

            foreach (var e in ldmGuidRecords)
            {
                var output = string.Empty;
                var key = e.Key.Split( new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries );
                if (ldmGuidCollection.TryGetValue(e.Key[1].ToString(), out output))
                {
                    var errorMessage = string.Format("Duplicate key found in LDM.GUID '{0}':", e.Key);
                    exception.AddError(new RepositoryError("invalid.key", errorMessage));
                }
                else
                    ldmGuidCollection.Add( key[1], e.Value.Guid);
            }

            if (exception.Errors.Any())
            {
                throw exception;
            }
            return ldmGuidCollection;
        }


        /// <summary>
        /// Gets the record key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetRecordKeyAsync(string guid)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "APPLICATIONS")
            {
                throw new KeyNotFoundException(string.Format("Applications record {0} does not exist.", guid));
            }
            return recordInfo.PrimaryKey;
        }

        /// <summary>
        /// Updates an exiting admission application.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> UpdateAdmissionApplicationAsync(AdmissionApplication entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("admissionApplication", "Must provide an admission application to update.");
            }

            if (string.IsNullOrEmpty(entity.Guid))
            {
                throw new ArgumentNullException("admissionApplicationEntity", "Must provide the guid for admission application to update.");
            }

            var recordKey = await GetRecordKeyAsync(entity.Guid);

            if (!string.IsNullOrEmpty(recordKey))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();

                UpdateAdmApplicationRequest request = BuildAdmissionApplicationRequest(entity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    request.ExtendedNames = extendedDataTuple.Item1;
                    request.ExtendedValues = extendedDataTuple.Item2;
                }
                var response = await transactionInvoker.ExecuteAsync<UpdateAdmApplicationRequest, UpdateAdmApplicationResponse>(request);

                if ((response != null && !string.IsNullOrEmpty(response.Error)) || (response != null && response.UpdateAdmApplicationErrors != null && response.UpdateAdmApplicationErrors.Any()))
                {
                    var errorMessage = string.Format("Error(s) occurred updating admission application, Guid: '{0}':", entity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    response.UpdateAdmApplicationErrors.ForEach(e => exception.AddError(new RepositoryError("admissionApplication", e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }
                return await GetAdmissionApplicationByIdAsync(response.Guid);
            }

            return await CreateAdmissionApplicationAsync(entity);
        }

        /// <summary>
        /// Creates an admission application.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> CreateAdmissionApplicationAsync(AdmissionApplication entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("admissionApplicationEntity", "Must provide an admission application to create.");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            UpdateAdmApplicationRequest request = BuildAdmissionApplicationRequest(entity);
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateAdmApplicationRequest, UpdateAdmApplicationResponse>(request);

            if ((response != null && !string.IsNullOrEmpty(response.Error)) || (response != null && response.UpdateAdmApplicationErrors != null && response.UpdateAdmApplicationErrors.Any()))
            {
                var errorMessage = string.Format("Error(s) occurred updating admission application, Guid: '{0}':", entity.Guid);
                var exception = new RepositoryException(errorMessage);
                response.UpdateAdmApplicationErrors.ForEach(e => exception.AddError(new RepositoryError("admissionApplication", e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }
            return await GetAdmissionApplicationByIdAsync(response.Guid);
        }

        /// <summary>
        /// Builds an  update admApplication request.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private UpdateAdmApplicationRequest BuildAdmissionApplicationRequest(AdmissionApplication entity)
        {
            try
            {
                var request = new UpdateAdmApplicationRequest()
                {
                    Guid = entity.Guid,
                    Application = entity.ApplicationRecordKey
                };
                request.AcademicLoad = string.IsNullOrEmpty(entity.ApplicationStudentLoadIntent) ? string.Empty : entity.ApplicationStudentLoadIntent;
                request.AcademicPeriod = string.IsNullOrEmpty(entity.ApplicationStartTerm) ? string.Empty : entity.ApplicationStartTerm;
                request.AdmissionPopulation = string.IsNullOrEmpty(entity.ApplicationAdmitStatus) ? string.Empty : entity.ApplicationAdmitStatus;
                request.Owner = string.IsNullOrEmpty(entity.ApplicationOwnerId) ? string.Empty : entity.ApplicationOwnerId;
                request.School = string.IsNullOrEmpty(entity.ApplicationSchool) ? string.Empty : entity.ApplicationSchool;
                request.Type = string.IsNullOrEmpty(entity.ApplicationIntgType) ? string.Empty : entity.ApplicationIntgType;
                request.AcademicProgram = string.IsNullOrEmpty(entity.ApplicationAcadProgram) ? null : entity.ApplicationAcadProgram;
                request.AdmittedOn = entity.AdmittedOn.HasValue ? entity.AdmittedOn.Value.Date.ToString() : string.Empty;
                request.Applicant = string.IsNullOrEmpty(entity.ApplicantPersonId) ? string.Empty : entity.ApplicantPersonId;
                request.AppliedOn = entity.AppliedOn.HasValue ? entity.AppliedOn.Value.Date.ToString() : string.Empty;
                request.Comment = string.IsNullOrEmpty(entity.ApplicationComments) ? string.Empty : entity.ApplicationComments;
                request.MatriculatedOn = entity.MatriculatedOn.HasValue ? entity.MatriculatedOn.Value.Date.ToString() : string.Empty;
                request.Reference = string.IsNullOrEmpty(entity.ApplicationNo) ? string.Empty : entity.ApplicationNo;
                request.ResidencyType = string.IsNullOrEmpty(entity.ApplicationResidencyStatus) ? string.Empty : entity.ApplicationResidencyStatus;
                request.Site = entity.ApplicationLocations != null && entity.ApplicationLocations.Any() ? entity.ApplicationLocations.FirstOrDefault() : string.Empty;
                request.Source = string.IsNullOrEmpty(entity.ApplicationSource) ? string.Empty : entity.ApplicationSource;
                request.WithdrawalInstitutionAttended = string.IsNullOrEmpty(entity.ApplicationAttendedInstead) ? string.Empty : entity.ApplicationAttendedInstead;
                request.WithdrawalReason = string.IsNullOrEmpty(entity.ApplicationWithdrawReason) ? string.Empty : entity.ApplicationWithdrawReason;
                request.WithdrawnOn = entity.WithdrawnOn.HasValue ? entity.WithdrawnOn.Value.Date.ToString() : string.Empty;                


                return request;
            }
            catch (Exception e)
            {
                throw new RepositoryException(e.Message);
            }
        }

        #endregion

        #region Admission Application Submission

        /// <summary>
        /// Get admission application by Id for submission.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> GetAdmissionApplicationSubmissionByIdAsync(string guid, bool bypassCache = false)
        {
            var applicationStatuses = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var admissionApplicationDataContract = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", new GuidLookup(guid));
            if (admissionApplicationDataContract == null)
            {
                throw new KeyNotFoundException("No admission-applications was found for guid " + guid);
            }

            if ((applicationStatuses != null) && (applicationStatuses.Any())
                    && (admissionApplicationDataContract.ApplStatusesEntityAssociation != null && admissionApplicationDataContract.ApplStatusesEntityAssociation.Any()))
            {
                var statuses = applicationStatuses.Select(rk => rk.Recordkey).Distinct().ToArray();

                var admApplStatuses = admissionApplicationDataContract.ApplStatusesEntityAssociation.FirstOrDefault();

                if (statuses.Contains(admApplStatuses.ApplStatusAssocMember))
                {
                    throw new KeyNotFoundException("admission-applications not found for guid " + guid);
                }
            }
            var studentProgramIds = new List<string>();
            studentProgramIds.Add(string.Concat(admissionApplicationDataContract.ApplApplicant, "*", admissionApplicationDataContract.ApplAcadProgram));
            var programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(studentProgramIds.Distinct().ToArray());

            var applicantData = new Applicants();
            if (!string.IsNullOrEmpty(admissionApplicationDataContract.ApplApplicant))
            {
                applicantData = await DataReader.ReadRecordAsync<Applicants>("APPLICANTS", admissionApplicationDataContract.ApplApplicant);
            }
            return BuildAdmissionApplicationSubmission(admissionApplicationDataContract, programData, applicantData);
        }


        /// <summary>
        /// Updates an exiting admission application.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> UpdateAdmissionApplicationSubmissionAsync(AdmissionApplication entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("admissionApplication", "Must provide an admission application to update.");
            }

            if (string.IsNullOrWhiteSpace(entity.Guid))
            {
                throw new ArgumentNullException("admissionApplicationEntity", "Must provide the guid for admission application to update.");
            }

            var recordKey = await GetRecordKeyAsync(entity.Guid);

            if (!string.IsNullOrEmpty(recordKey))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();

                UpdateProspectOrApplicantRequest request = BuildAdmissionApplicationSubmissionRequest(entity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    request.ExtendedNames = extendedDataTuple.Item1;
                    request.ExtendedValues = extendedDataTuple.Item2;
                }
                var response = await transactionInvoker.ExecuteAsync<UpdateProspectOrApplicantRequest, UpdateProspectOrApplicantResponse>(request);

                // If there is any error message - throw an exception
                if (response != null && response.UpdateProspectApplicantErrors != null && response.UpdateProspectApplicantErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating admission application, Guid: '{0}':", entity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    response.UpdateProspectApplicantErrors.ForEach(e => exception.AddError(new RepositoryError("admissionApplication", e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }
                return await GetAdmissionApplicationById2Async(response.Guid);
            }

            return await CreateAdmissionApplicationSubmissionAsync(entity);
        }

        /// <summary>
        /// Creates an admission application.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<AdmissionApplication> CreateAdmissionApplicationSubmissionAsync(AdmissionApplication entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("admissionApplicationEntity", "Must provide an admission application to create.");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            UpdateProspectOrApplicantRequest request = BuildAdmissionApplicationSubmissionRequest(entity);
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateProspectOrApplicantRequest, UpdateProspectOrApplicantResponse>(request);

            if ((response != null && !string.IsNullOrEmpty(response.Error)) || (response != null && response.UpdateProspectApplicantErrors != null && 
                response.UpdateProspectApplicantErrors.Any()))
            {
                var errorMessage = string.Format("Error(s) occurred updating admission application, Guid: '{0}':", entity.Guid);
                var exception = new RepositoryException(errorMessage);
                response.UpdateProspectApplicantErrors.ForEach(e => exception.AddError(new RepositoryError("admissionApplication", e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }
            return await GetAdmissionApplicationById2Async(response.Guid);
        }

        /// <summary>
        /// Builds an  update admApplication request.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private UpdateProspectOrApplicantRequest BuildAdmissionApplicationSubmissionRequest(AdmissionApplication entity)
        {
            try
            {
                var request = new UpdateProspectOrApplicantRequest()
                {
                    Guid = entity.Guid,
                    Application = entity.ApplicationRecordKey,
                    IsProspect = false
                };
                request.AcademicLoad = string.IsNullOrEmpty(entity.ApplicationStudentLoadIntent) ? string.Empty : entity.ApplicationStudentLoadIntent;
                request.AcademicPeriod = string.IsNullOrEmpty(entity.ApplicationStartTerm) ? string.Empty : entity.ApplicationStartTerm;
                request.AcademicLevel = string.IsNullOrEmpty(entity.ApplicationAcadLevel) ? string.Empty : entity.ApplicationAcadLevel;
                request.AdmissionPopulation = string.IsNullOrEmpty(entity.ApplicationAdmitStatus) ? string.Empty : entity.ApplicationAdmitStatus;
                request.ProgramOwner = string.IsNullOrEmpty(entity.ApplicationProgramOwner) ? string.Empty : entity.ApplicationProgramOwner;
                request.Owner = string.IsNullOrEmpty(entity.ApplicationAdmissionsRep) ? string.Empty : entity.ApplicationAdmissionsRep;
                request.Type = string.IsNullOrEmpty(entity.ApplicationIntgType) ? string.Empty : entity.ApplicationIntgType;
                request.AcademicProgram = string.IsNullOrEmpty(entity.ApplicationAcadProgram) ? null : entity.ApplicationAcadProgram;
                request.Applicant = string.IsNullOrEmpty(entity.ApplicantPersonId) ? string.Empty : entity.ApplicantPersonId;
                request.AppliedOn = entity.AppliedOn;
                request.Comment = string.IsNullOrEmpty(entity.ApplicationComments) ? string.Empty : entity.ApplicationComments;
                request.Reference = string.IsNullOrEmpty(entity.ApplicationNo) ? string.Empty : entity.ApplicationNo;
                request.ResidencyType = string.IsNullOrEmpty(entity.ApplicationResidencyStatus) ? string.Empty : entity.ApplicationResidencyStatus;
                request.Site = entity.ApplicationLocations != null && entity.ApplicationLocations.Any() ? entity.ApplicationLocations.FirstOrDefault() : string.Empty;
                request.ApplicationSource = string.IsNullOrEmpty(entity.ApplicationSource) ? string.Empty : entity.ApplicationSource;
                request.PersonSource = string.IsNullOrEmpty(entity.PersonSource) ? string.Empty : entity.PersonSource;
                request.WithdrawalInstitutionAttended = string.IsNullOrEmpty(entity.ApplicationAttendedInstead) ? string.Empty : entity.ApplicationAttendedInstead;
                request.WithdrawalReason = string.IsNullOrEmpty(entity.ApplicationWithdrawReason) ? string.Empty : entity.ApplicationWithdrawReason;
                request.WithdrawnOn = entity.WithdrawnOn;
                request.ccds = entity.ApplicationCredentials;
                request.EducGoal = entity.EducationalGoal;
                request.CareerGoals = entity.CareerGoals;
                request.Influences = entity.Influences;

                // Majors, Minors, and Concentrations (specializations)
                if (entity.ApplicationDisciplines != null && entity.ApplicationDisciplines.Any())
                {
                    foreach (var discipline in entity.ApplicationDisciplines)
                    {
                        switch (discipline.DisciplineType)
                        {
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                {
                                    request.UpdateProspectApplicantMajors.Add(new UpdateProspectApplicantMajors()
                                    {
                                        Majors = discipline.Code,
                                        MajorDepts = discipline.AdministeringInstitutionUnit,
                                        MajorStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                {
                                    request.UpdateProspectApplicantMinors.Add(new UpdateProspectApplicantMinors()
                                    {
                                        Minors = discipline.Code,
                                        MinorDepts = discipline.AdministeringInstitutionUnit,
                                        MinorStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                {
                                    request.UpdateProspectApplicantSpecials.Add(new UpdateProspectApplicantSpecials()
                                    {
                                        Specials = discipline.Code,
                                        SpecialDepts = discipline.AdministeringInstitutionUnit,
                                        SpecialStartDates = discipline.StartOn
                                    });
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }

                return request;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("data.access", e.Message) { Id = entity.Guid, SourceId = entity.ApplicationRecordKey });
                throw exception;
            }
        }

        /// <summary>
        /// BuildAdmissionApplication
        /// </summary>
        /// <param name="application"></param>
        /// <param name="programData"></param>
        /// <returns>AdmissionApplication domain entity</returns>
        private AdmissionApplication BuildAdmissionApplicationSubmission(Applications application,
            Collection<StudentPrograms> programData, Applicants applicantData)
        {
            try
            {
                var applicationEntity = new AdmissionApplication(application.RecordGuid, application.Recordkey);

                applicationEntity.ApplicantPersonId = application.ApplApplicant;
                applicationEntity.ApplicationNo = application.ApplNo;
                applicationEntity.ApplicationStartTerm = application.ApplStartTerm;
                applicationEntity.AdmissionApplicationStatuses = BuildAdmissionApplicationStatuses(application.ApplStatusesEntityAssociation);
                applicationEntity.ApplicationSource = application.ApplSource;
                applicationEntity.ApplicationAdmissionsRep = application.ApplAdmissionsRep;
                applicationEntity.ApplicationAdmitStatus = application.ApplAdmitStatus;
                applicationEntity.ApplicationLocations = application.ApplLocations;
                applicationEntity.ApplicationResidencyStatus = application.ApplResidencyStatus;
                applicationEntity.ApplicationStudentLoadIntent = application.ApplStudentLoadIntent;
                applicationEntity.ApplicationAcadProgram = application.ApplAcadProgram;

                var key = string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram);
                if (programData == null || !programData.Any())
                {
                    throw new KeyNotFoundException(string.Format("Student program not found for: {0}. ", key));
                }
                var program = programData.FirstOrDefault(i => i.Recordkey.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (program == null || string.IsNullOrEmpty(program.RecordGuid))
                {
                    throw new KeyNotFoundException(string.Format("Student program not found for: {0}. ", key));
                }
                else
                {
                    applicationEntity.ApplicationAcadProgramGuid = program.RecordGuid;
                    applicationEntity.ApplicationStprAcadPrograms = BuildProgramCodeList(application, programData);
                }
                //Application Admission Submission V1.0.0 changes
                applicationEntity.ApplicationDisciplines = BuildAcademicDiscipline(application, program);
                applicationEntity.ApplicationCredentials = program.StprCcds;
                applicationEntity.ApplicationWithdrawDate = application.ApplWithdrawDate;
                applicationEntity.ApplicationSchool = program.StprSchool;
                applicationEntity.ApplicationProgramOwner = program.StprDept;

                applicationEntity.ApplicationWithdrawReason = application.ApplWithdrawReason;
                applicationEntity.ApplicationAttendedInstead = application.ApplAttendedInstead;
                applicationEntity.ApplicationComments = application.ApplComments;
                applicationEntity.ApplicationWithdrawDate = application.ApplWithdrawDate;
                applicationEntity.CareerGoals = application.ApplIntgCareerGoals;
                applicationEntity.Influences = application.ApplInfluencedToApply;
                if (applicantData != null && !string.IsNullOrEmpty(applicantData.AppOrigEducGoal))
                {
                    applicationEntity.EducationalGoal = applicantData.AppOrigEducGoal;
                }

                return applicationEntity;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(string.Format(ex.Message, "admission application guid: {0}.", application.RecordGuid), ex);
            }
        }

        /// <summary>
        /// STPR_MAJOR_LIST, STPR_MINOR_LIST, STPR_SPECIALTIES
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private List<ApplicationDiscipline> BuildAcademicDiscipline(Applications application, StudentPrograms programData)
        {
            var discipline = new List<ApplicationDiscipline>();

            if (programData == null)
            {
                return discipline;
            }

            //STPR_MAJOR_LIST
            if (programData.StprMajorListEntityAssociation != null && programData.StprMajorListEntityAssociation.Any())
            {
                foreach (var major in programData.StprMajorListEntityAssociation)
                {
                    var existingMajor = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Major && i.Code == major.StprAddnlMajorsAssocMember);
                    if (existingMajor == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = major.StprAddnlMajorsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Major,
                            StartOn = major.StprAddnlMajorStartDateAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }

            //STPR_MINOR_LIST
            if (programData.StprMinorListEntityAssociation != null && programData.StprMinorListEntityAssociation.Any())
            {
                foreach (var minor in programData.StprMinorListEntityAssociation)
                {
                    var existingMinor = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Minor && i.Code == minor.StprMinorsAssocMember);
                    if (existingMinor == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = minor.StprMinorsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Minor,
                            StartOn = minor.StprMinorStartDateAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }

            //STPR_SPECIALTIES
            if (programData.StprSpecialtiesEntityAssociation != null && programData.StprSpecialtiesEntityAssociation.Any())
            {
                foreach (var specialty in programData.StprSpecialtiesEntityAssociation)
                {
                    var existingSpecialty = discipline.FirstOrDefault(i => i.DisciplineType == Domain.Base.Entities.AcademicDisciplineType.Concentration && i.Code == specialty.StprSpecializationsAssocMember);
                    if (existingSpecialty == null)
                    {
                        discipline.Add(new ApplicationDiscipline()
                        {
                            Code = specialty.StprSpecializationsAssocMember,
                            DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Concentration,
                            StartOn = specialty.StprSpecializationStartAssocMember,
                            AdministeringInstitutionUnit = programData.StprDept
                        });
                    }
                }
            }
            return discipline.Any() ? discipline : null;
        }

        public async Task<string> GetDefaultApplicationStatus()
        {            
            var ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            return ldmDefaults.LdmdDefaultApplStatus;
        }

        #endregion
    }
}
 