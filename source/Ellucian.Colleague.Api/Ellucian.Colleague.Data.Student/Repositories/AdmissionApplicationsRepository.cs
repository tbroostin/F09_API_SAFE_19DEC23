//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
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
                throw ex;
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

            //The data contract can sometimes get the wrong guid
            var guidList = await DataReader.SelectAsync("LDM.GUID", "WITH LDM.GUID.PRIMARY.KEY EQ '?' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.LDM.NAME EQ 'admission-applications' AND LDM.GUID.REPLACED.BY EQ ''", applicationIds.ToArray());
            var ldmGuidRecords = await DataReader.BulkReadRecordAsync<LdmGuid>(guidList);
           

            ldmGuidRecords.ToList().ForEach(e => ldmGuidCollection.Add(e.LdmGuidPrimaryKey, e.Recordkey));

            return ldmGuidCollection;
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
    }
}
 