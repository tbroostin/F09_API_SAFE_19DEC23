//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Higher education institution admission applications.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AdmissionApplicationsRepository : BaseColleagueRepository, IAdmissionApplicationsRepository
    {
        private Dictionary<string, string> _admissionApplicationDict = new Dictionary<string, string>();

        /// <summary>
        /// ..ctor
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

        #region GET methods

        Collection<StudentPrograms> programData = null;

        /// <summary>
        /// Get admission applications
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>> GetAdmissionApplications2Async(int offset, int limit, bool bypassCache)
        {
            List<Domain.Student.Entities.AdmissionApplication> applicationEntities = new List<Domain.Student.Entities.AdmissionApplication>();
            int totalCount = 0;
            string[] subList = null;
            string[] ids = null;
            string[] applicationIds = null;
            string criteria = string.Empty;

            Collection<ApplicationStatuses> categoryTable = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            if (categoryTable != null && categoryTable.Any())
            {
                ids = categoryTable.Select(i => i.Recordkey).Distinct().ToArray();
                criteria = "WITH APPL.APPLICANT NE '' ";
                string currStatus = string.Empty;
                foreach (var id in ids)
                {
                    currStatus += string.Format("AND APPL.CURRENT.STATUS NE '{0}' ", id);
                }
                criteria = criteria + currStatus;
                applicationIds = await DataReader.SelectAsync("APPLICATIONS", criteria);
            }
            else
            {
                criteria = "WITH APPL.APPLICANT NE ''";
                applicationIds = await DataReader.SelectAsync("APPLICATIONS", criteria);
            }

            totalCount = applicationIds.Count();

            Array.Sort(applicationIds);

            subList = applicationIds.Skip(offset).Take(limit).ToArray();

            // Now we have criteria, so we can select and read the records
            Collection<DataContracts.Applications> applicationDataContracts = null;
            applicationDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Applications>("APPLICATIONS", subList);

            if (applicationDataContracts != null && applicationDataContracts.Any())
            {    
                var stprIds = new List<string>();
                stprIds.AddRange(applicationDataContracts.Where(i => !string.IsNullOrEmpty(i.ApplAcadProgram)).Select(i => string.Concat(i.ApplApplicant, "*", i.ApplAcadProgram)));
                programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.Distinct().ToArray());

                foreach (var applicationDataContract in applicationDataContracts)
                {
                    List<string> applicationKey = new List<string>();
                    applicationKey.Add(applicationDataContract.Recordkey);
                    _admissionApplicationDict = await GetAdmissionApplicationGuidDictionary(applicationKey);

                    Domain.Student.Entities.AdmissionApplication applicationEntity = BuildAdmissionApplication(applicationDataContract, _admissionApplicationDict);
                    applicationEntities.Add(applicationEntity);
                }
            }

            return applicationEntities.Any() ?
                new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>(applicationEntities, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>(new List<Domain.Student.Entities.AdmissionApplication>(), 0);
        }

        /// <summary>
        /// Get admission applications
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache)
        {
            List<Domain.Student.Entities.AdmissionApplication> applicationEntities = new List<Domain.Student.Entities.AdmissionApplication>();
            int totalCount = 0;
            string[] subList = null;
            string[] ids = null;
            string[] applicationIds = null;
            string criteria = string.Empty;

            Collection<ApplicationStatuses> categoryTable = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            if(categoryTable != null && categoryTable.Any())
            {
                ids = categoryTable.Select(i => i.Recordkey ).Distinct().ToArray();
                criteria = "WITH APPL.APPLICANT NE '' ";
                string currStatus = string.Empty;
                foreach (var id in ids)
                {
                    currStatus += string.Format("AND APPL.CURRENT.STATUS NE '{0}' ", id);
                }
                criteria = criteria + currStatus;
                applicationIds = await DataReader.SelectAsync("APPLICATIONS", criteria);
            }
            else
            {
                criteria = "WITH APPL.APPLICANT NE ''";
                applicationIds = await DataReader.SelectAsync("APPLICATIONS", criteria);
            }

            totalCount = applicationIds.Count();

            Array.Sort(applicationIds);

            subList = applicationIds.Skip(offset).Take(limit).ToArray();

            // Now we have criteria, so we can select and read the records
            Collection<DataContracts.Applications> applicationDataContracts = null;
            applicationDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Applications>("APPLICATIONS", subList);

            if (applicationDataContracts != null && applicationDataContracts.Any())
            {
                var stprIds = new List<string>();
                stprIds.AddRange(applicationDataContracts.Where(i => !string.IsNullOrEmpty(i.ApplAcadProgram)).Select(i => string.Concat(i.ApplApplicant, "*", i.ApplAcadProgram)));
                programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.Distinct().ToArray());

                foreach (var applicationDataContract in applicationDataContracts)
                {
                    List<string> applicationKey = new List<string>();
                    applicationKey.Add(applicationDataContract.Recordkey);
                    _admissionApplicationDict = await GetAdmissionApplicationGuidDictionary(applicationKey);

                    Domain.Student.Entities.AdmissionApplication applicationEntity = BuildAdmissionApplication(applicationDataContract, _admissionApplicationDict);
                    applicationEntities.Add(applicationEntity);
                }
            }

            return applicationEntities.Any() ? 
                new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>(applicationEntities, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplication>, int>(new List<Domain.Student.Entities.AdmissionApplication>(), 0);
        }
        
        /// <summary>
        /// Get admission application by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.Student.Entities.AdmissionApplication> GetAdmissionApplicationByIdAsync(string guid)
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

            Collection<ApplicationStatuses> categoryTable = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var admissionApplicationDataContract = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", admissionApplicationId);
            if (admissionApplicationDataContract == null)
            {
                throw new KeyNotFoundException("admission-applications data contract not found for Id " + admissionApplicationId);
            }

            if (categoryTable != null && categoryTable.Any())
            {
                var statuses = categoryTable.Select(rk => rk.Recordkey).Distinct().ToArray();

                if (admissionApplicationDataContract.ApplStatusesEntityAssociation != null && admissionApplicationDataContract.ApplStatusesEntityAssociation.Any())
                {
                    var admApplStatuses = admissionApplicationDataContract.ApplStatusesEntityAssociation.FirstOrDefault();

                    if (statuses.Contains(admApplStatuses.ApplStatusAssocMember))
                    {
                        throw new KeyNotFoundException("admission-applications not found for GUID " + guid);
                    }
                }
            }

            var stprIds = new List<string>();
            stprIds.Add(string.Concat(admissionApplicationDataContract.ApplApplicant, "*", admissionApplicationDataContract.ApplAcadProgram));
            programData = await DataReader.BulkReadRecordAsync<StudentPrograms>(stprIds.Distinct().ToArray());

            List<string> applicationKey = new List<string>();
            applicationKey.Add(admissionApplicationDataContract.Recordkey);
            _admissionApplicationDict = await GetAdmissionApplicationGuidDictionary(applicationKey);
            
            var admissionApplicationEntity = BuildAdmissionApplication(admissionApplicationDataContract, _admissionApplicationDict);
            return admissionApplicationEntity;
        }

        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys)
        {
            if (personRecordKeys != null && !personRecordKeys.Any())
            {
                return null;
            }

            var personGuids = new Dictionary<string, string>();

            if (personRecordKeys != null && personRecordKeys.Any())
            {
                // convert the person keys to person guids
                var personGuidLookup = personRecordKeys.ToList().ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    string[] splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!personGuids.ContainsKey(splitKeys[1]))
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            personGuids.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
            }
            return (personGuids != null && personGuids.Any()) ? personGuids : null;
        }


        public async Task<IDictionary<string, string>> GetStaffOperIdsAsync(List<string> ids)
        {
            var opersData = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", ids.ToArray(), true);
            var staffData = await DataReader.BulkReadRecordAsync<Staff>(ids.ToArray());
            IDictionary<string, string> combinedIds = new Dictionary<string, string>();

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
        /// Builds admission application entity
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private Domain.Student.Entities.AdmissionApplication BuildAdmissionApplication(Applications application, Dictionary<string, string> _admissionApplicationDict)
        {
            try
            {
                //
                // Workaroud because applicant.RecordGuid unreliable because APPLICATIONS file has multiple GUID sources.  We
                // always want the GUID tied to the Colleague application record with no secondary key values, previously stored in the incoming DICT
                //
                var applicationGuid = _admissionApplicationDict.FirstOrDefault(i => i.Key.Equals(application.Recordkey, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(applicationGuid.Value))
                {
                    throw new KeyNotFoundException(string.Format("No application guid found for id: {0}", application.Recordkey));
                }
                application.RecordGuid = applicationGuid.Value;

                Domain.Student.Entities.AdmissionApplication applicationEntity =
                                new Domain.Student.Entities.AdmissionApplication(application.RecordGuid, application.Recordkey);

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
                applicationEntity.ApplicationStprAcadPrograms = BuildProgramCodeList(application);
                applicationEntity.ApplicationWithdrawReason = application.ApplWithdrawReason;
                applicationEntity.ApplicationAttendedInstead = application.ApplAttendedInstead;
                applicationEntity.ApplicationComments = application.ApplComments;
                applicationEntity.ApplicationSchool = BuildSchool(string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram));

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
        /// STPR_MAJOR_LIST, STPR_MINOR_LIST, STPR_SPECIALTIES
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private List<string> BuildProgramCodeList(Applications application)
        {
            List<string> programs = new List<string>();

            var key = string.Concat(application.ApplApplicant, "*", application.ApplAcadProgram);

            if (programData == null || (programData != null && !programData.Any()))
            {
                return programs;
            }
            var program = programData.FirstOrDefault(i => i.Recordkey.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (program == null)
            {
                throw new KeyNotFoundException(string.Format("Student academic program not found for: {0}. ", key));
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
        private string BuildSchool(string acadProgramKey)
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
                    throw new KeyNotFoundException(string.Format("Student academic program not found for: {0}. ", acadProgramKey));
                }
                return program.StprSchool;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Builds application statuses.
        /// </summary>
        /// <param name="applStatusesAssiciation"></param>
        /// <returns></returns>
        private List<Domain.Student.Entities.AdmissionApplicationStatus> BuildAdmissionApplicationStatuses(List<ApplicationsApplStatuses> applStatusesAssiciation)
        {
            List<Domain.Student.Entities.AdmissionApplicationStatus> applStatuses = new List<Domain.Student.Entities.AdmissionApplicationStatus>();
            if (applStatusesAssiciation != null && applStatusesAssiciation.Any())
            {
                foreach (var item in applStatusesAssiciation)
                {
                    Domain.Student.Entities.AdmissionApplicationStatus admApplStatus = new Domain.Student.Entities.AdmissionApplicationStatus() 
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
            List<string> decisionBy = new List<string>();

            var items = applicationsApplStatuses.OrderByDescending(i => i.ApplStatusDateAssocMember);
            items.ToList().ForEach(i => 
            {
                decisionBy.Add(i.ApplDecisionByAssocMember);
            });
            return decisionBy.Any() ? decisionBy : null;
        }

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for APPLICATIONS.
        /// </summary>
        /// <param name="applicationIds"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetAdmissionApplicationGuidDictionary(IEnumerable<string> applicationIds)
        {
            if (applicationIds == null || !Enumerable.Any<string>(applicationIds))
            {
                throw new ArgumentNullException("Application id's are required.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var applicationId in applicationIds)
            {
                var criteria = string.Format("WITH LDM.GUID.SECONDARY.FLD EQ '' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.LDM.NAME EQ 'admission-applications' AND LDM.GUID.PRIMARY.KEY EQ '{0}'", applicationId);
                var guidRecords = await DataReader.SelectAsync("LDM.GUID", criteria);
                if (!dict.ContainsKey(applicationId))
                {
                    if (guidRecords != null && guidRecords.Any())
                    {
                        dict.Add(applicationId, guidRecords[0]);
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Gets the record key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetRecordKeyAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
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
                UpdateAdmApplicationRequest request = new UpdateAdmApplicationRequest()
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
    }
}