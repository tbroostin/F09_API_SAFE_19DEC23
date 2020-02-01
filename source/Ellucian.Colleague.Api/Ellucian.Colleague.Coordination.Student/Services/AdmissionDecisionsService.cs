//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionDecisionsService : BaseCoordinationService, IAdmissionDecisionsService
    {
        private readonly IApplicationStatusRepository _applicationStatusRepository;
        private readonly IAdmissionApplicationsRepository _admissionApplicationsRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private Dictionary<string, string> _admissionApplicationDict = new Dictionary<string, string>();

        public AdmissionDecisionsService(
            IApplicationStatusRepository applicationStatusRepository,
            IAdmissionApplicationsRepository admissionApplicationsRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _applicationStatusRepository = applicationStatusRepository;
            _admissionApplicationsRepository = admissionApplicationsRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET
        /// <summary>
        /// Gets all admission-decisions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="applicationId">The admission application, on which this decision was made.</param>
        /// <param name="decidedOn">The date of the decision on the admission application.</param>
        /// <param name="filterQualifiers">KeyValuePair of advanced filter criteria.</param>
        /// <param name="personFilterValue">Person filter criteria.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdmissionDecisions">admissionDecisions</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>> GetAdmissionDecisionsAsync(int offset, int limit,
            string applicationId, DateTimeOffset? decidedOn, Dictionary<string, string> filterQualifiers, string personFilterValue, bool bypassCache)
        {
            try
            {
                // access is ok if the current user has the view, or create, permission
                if ((!await CheckViewAdmissionDecisionsPermissionAsync()) && (!await CheckCreateAdmissionDecisionPermissionAsync()))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-decisions.");
                    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to view admission-decisions");
                }

                var admissionDecisionsCollection = new List<Dtos.AdmissionDecisions>();
                var newPersonFilter = string.Empty;
                Tuple<IEnumerable<ApplicationStatus2>, int> admissionDecisionsEntities = null;

                string[] filterPersonIds = new List<string>().ToArray();

                if (!string.IsNullOrEmpty(personFilterValue))
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilterValue));
                    if (personFilterKeys != null)
                    {
                        filterPersonIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(new List<Dtos.AdmissionDecisions>(), 0);
                    }
                }

                try
                {
                    admissionDecisionsEntities = await _applicationStatusRepository.GetApplicationStatusesAsync(offset, limit, applicationId, filterPersonIds, decidedOn, filterQualifiers, bypassCache);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                if (admissionDecisionsEntities != null && admissionDecisionsEntities.Item1.Any())
                {
                    _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(admissionDecisionsEntities.Item1.Select(i => i.ApplicantRecordKey).Distinct());
                    foreach (var admissionDecisions in admissionDecisionsEntities.Item1)
                    {
                        admissionDecisionsCollection.Add(await ConvertEntityToDtoAsync(admissionDecisions, _admissionApplicationDict, bypassCache));
                    }
                }
                if (IntegrationApiException != null)
                    throw IntegrationApiException;

                return admissionDecisionsCollection.Any() ? new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsCollection, admissionDecisionsEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(new List<Dtos.AdmissionDecisions>(), 0);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a admissionDecisions by guid.
        /// </summary>
        /// <param name="guid">Guid of the admissionDecisions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AdmissionDecisions">admissionDecisions</see></returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionDecisions> GetAdmissionDecisionsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                // access is ok if the current user has the view, or create, permission
                if ((!await CheckViewAdmissionDecisionsPermissionAsync()) && (!await CheckCreateAdmissionDecisionPermissionAsync()))
                {
                    logger.Error("User " + CurrentUser.UserId + " does not have permission to view admission decisions.");
                    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to view admission-decisions");

                }
                ApplicationStatus2 entity = null;
                try
                {
                    entity = await _applicationStatusRepository.GetApplicationStatusByGuidAsync(guid, bypassCache);

                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }

                if (entity == null)
                {
                    throw new KeyNotFoundException(string.Format("No admission-decisions was found for guid '{0}'.", guid));
                }
                _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(new List<string>() { entity.ApplicantRecordKey });

                var retval = await ConvertEntityToDtoAsync(entity, _admissionApplicationDict, bypassCache);

                if (IntegrationApiException != null)
                    throw IntegrationApiException;

                return retval;
            }          
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-decisions was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-decisions was found for guid '{0}'", guid), ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

      
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ApplStatuses domain entity to its corresponding AdmissionDecisions DTO
        /// </summary>
        /// <param name="source">ApplStatuses domain entity</param>
        /// <returns>AdmissionDecisions DTO</returns>
        private async Task<Dtos.AdmissionDecisions> ConvertEntityToDtoAsync(Domain.Student.Entities.ApplicationStatus2 source, Dictionary<string, string> _admissionApplicationDict, bool bypassCache)
        {
            var admissionDecisions = new Ellucian.Colleague.Dtos.AdmissionDecisions();

            admissionDecisions.Id = source.Guid;
            admissionDecisions.Application = ConvertEntityToGuidObject(source.ApplicantRecordKey, _admissionApplicationDict);
            admissionDecisions.DecisionType = await ConvertEntityToDecisionTypeGuidObjectAsync(source.DecisionType, bypassCache);
            admissionDecisions.DecidedOn = ConvertEntityToDateTimeDto(source.DecidedOnDate, source.DecidedOnTime);

            return admissionDecisions;
        }

        /// <summary>
        /// Convert entity date & time to date time dto.
        /// </summary>
        /// <param name="decidedOnDate"></param>
        /// <param name="decidedOnTime"></param>
        /// <returns></returns>
        private DateTime ConvertEntityToDateTimeDto(DateTime decidedOnDate, DateTime decidedOnTime)
        {
            return new DateTime(decidedOnDate.Year, decidedOnDate.Month, decidedOnDate.Day, decidedOnTime.Hour, decidedOnTime.Minute, decidedOnTime.Second);
        }

        /// <summary>
        /// Converts entity to decision type dto guid object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToDecisionTypeGuidObjectAsync(string source, bool bypassCache)
        {
            var decisionTypeGuid = await _studentReferenceDataRepository.GetAdmissionDecisionTypesGuidAsync(source);
            if (decisionTypeGuid == null)
            {
                IntegrationApiExceptionAddError(string.Format("Admission decision type not found for code {0}.", source));
                return null;
            }
            return new GuidObject2(decisionTypeGuid);
        }

        /// <summary>
        /// Converts entity to dto guid object.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private GuidObject2 ConvertEntityToGuidObject(string applicationId, Dictionary<string, string> source)
        {
            var applicationGuid = source.FirstOrDefault(i => i.Key.Equals(applicationId, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(applicationGuid.Value))
            {
                IntegrationApiExceptionAddError(string.Format("No application guid found for id: {0}", applicationId));
                return null;
            }
            return new GuidObject2(applicationGuid.Value);
        }

        #endregion

        #region POST

        /// <summary>
        /// 
        /// </summary>
        /// <param name="admissionDecisions"></param>
        /// <returns></returns>
        public async Task<AdmissionDecisions> CreateAdmissionDecisionAsync(AdmissionDecisions admissionDecisions)
        {
            if (!await CheckCreateAdmissionDecisionPermissionAsync())
            {
                logger.Error(string.Format("User '{0}' is not authorized to create admission-decisions.", CurrentUser.UserId));
                throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to create admission-decisions");

            }

            _applicationStatusRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

           ApplicationStatus2 appStatusEntity = await ConvertDtoToAppStatusEntityAsync(admissionDecisions);
            if (IntegrationApiException != null)
                throw IntegrationApiException;
            ApplicationStatus2 entity = null;
            try
            {
                entity = await _applicationStatusRepository.UpdateAdmissionDecisionAsync(appStatusEntity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(new List<string>() { entity.ApplicantRecordKey });

            var retval = await ConvertEntityToDtoAsync(entity, _admissionApplicationDict, true);
            if (IntegrationApiException != null)
                throw IntegrationApiException;
            return retval;
        }

        /// <summary>
        /// Converts dto to entity for create(POST).
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.ApplicationStatus2> ConvertDtoToAppStatusEntityAsync(Dtos.AdmissionDecisions source)
        {
            ApplicationStatus2 retval = null;

            if (source == null)
            {
                IntegrationApiExceptionAddError("Source is a required object.");
                return retval;
            }

            if (!source.Id.Equals(Guid.Empty.ToString()))
            {
                var recordTuple = await _applicationStatusRepository.GetApplicationStatusKey(source.Id);
                if (recordTuple != null)
                {
                    IntegrationApiExceptionAddError("Changing the admission decision type is not permitted.", "id");
                }
            }

            var applicationTuple = await _applicationStatusRepository.GetApplicationStatusKey(source.Application.Id);
            if (applicationTuple == null || !applicationTuple.Item1.Equals("APPLICATIONS", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(applicationTuple.Item3))
            {
                IntegrationApiExceptionAddError(string.Format("Application not found for guid '{0}'.", source.Application.Id));
            }

            var decisionType = (await AdmissionDecisionTypesAsync(true)).FirstOrDefault(i => i.Guid.Equals(source.DecisionType.Id, StringComparison.OrdinalIgnoreCase));
            string decisionTypeCode = string.Empty;
            if (decisionType == null)
            {
                IntegrationApiExceptionAddError(string.Format("Decision type not found for guid {0}.", source.DecisionType.Id));
            }
            else
            {
                if (decisionType.SpecialProcessingCode != null && decisionType.SpecialProcessingCode.Equals("MS", StringComparison.OrdinalIgnoreCase))
                {
                    IntegrationApiExceptionAddError("Admission decision type associated with 'move to students' is not permitted.");
                }

                if (string.IsNullOrEmpty(decisionType.SpecialProcessingCode))
                {
                    IntegrationApiExceptionAddError("Admission decision type is not valid for a submitted application. This admission decision type is associated with a prospect status.");
                }

                decisionTypeCode = decisionType.Code;
            }
           
            try
            {

                var decidedOnDate = new DateTime(source.DecidedOn.Year, source.DecidedOn.Month, source.DecidedOn.Day);
                var decidedOnTime = new DateTime(1900, 1, 1, source.DecidedOn.Hour, source.DecidedOn.Minute, source.DecidedOn.Second);

                if (applicationTuple != null)
                {
                    retval = new ApplicationStatus2(source.Id, applicationTuple.Item2, decisionTypeCode, decidedOnDate, decidedOnTime)
                    {
                        DecidedOn = source.DecidedOn
                    };
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }
            return retval;
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Permissions code that allows an external system to view admission applications
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<bool> CheckViewAdmissionDecisionsPermissionAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.ViewAdmissionDecisions))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Permissions code that allows an external system to create admission applications. Update is not allowed for admission decision.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckCreateAdmissionDecisionPermissionAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.UpdateAdmissionDecisions))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all decision types.
        /// </summary>
        IEnumerable<Domain.Student.Entities.AdmissionDecisionType> _decisionTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.AdmissionDecisionType>> AdmissionDecisionTypesAsync(bool bypassCache)
        {
            if (_decisionTypes == null)
            {
                _decisionTypes = (await _studentReferenceDataRepository.GetAdmissionDecisionTypesAsync(bypassCache)).ToList();
            }
            return _decisionTypes;
        }
        #endregion
    }
}