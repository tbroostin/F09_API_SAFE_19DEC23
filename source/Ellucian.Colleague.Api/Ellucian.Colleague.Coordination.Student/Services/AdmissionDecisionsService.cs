//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
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
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionDecisionsService : BaseCoordinationService, IAdmissionDecisionsService
    {
        private readonly IApplicationStatusRepository _applicationStatusRepository;
        private readonly IAdmissionApplicationsRepository _admissionApplicationsRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private Dictionary<string, string> _admissionApplicationDict = new Dictionary<string, string>();

        public AdmissionDecisionsService(
            IApplicationStatusRepository applicationStatusRepository,
            IAdmissionApplicationsRepository admissionApplicationsRepository,
            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _applicationStatusRepository = applicationStatusRepository;
            _admissionApplicationsRepository = admissionApplicationsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all admission-decisions
        /// </summary>
        /// <returns>Collection of AdmissionDecisions DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>> GetAdmissionDecisionsAsync(int offset, int limit, string applicationId, bool bypassCache)
        {
            try
            {
                if (!await CheckViewAdmissionDecisionsPermissionAsync())
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view admission-decisions.");
                    throw new PermissionsException("User is not authorized to view admission-decisions.");
                }

                var admissionDecisionsCollection = new List<Dtos.AdmissionDecisions>();

                Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int> admissionDecisionsEntities = await _applicationStatusRepository.GetApplicationStatusesAsync(offset, limit, applicationId, bypassCache);
                if (admissionDecisionsEntities != null && admissionDecisionsEntities.Item1.Any())
                {
                    _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(admissionDecisionsEntities.Item1.Select(i => i.ApplicantRecordKey).Distinct());
                    foreach (var admissionDecisions in admissionDecisionsEntities.Item1)
                    {
                        admissionDecisionsCollection.Add(await ConvertEntityToDtoAsync(admissionDecisions, _admissionApplicationDict, bypassCache));
                    }
                }
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
        /// Get a AdmissionDecisions from its GUID
        /// </summary>
        /// <returns>AdmissionDecisions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionDecisions> GetAdmissionDecisionsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (!await CheckViewAdmissionDecisionsPermissionAsync())
                {
                    logger.Error("User " + CurrentUser.UserId + " does not have permission to view admission decisions.");
                    throw new PermissionsException("User is not authorized to view admission-decisions.");
                }
                var entity = await _applicationStatusRepository.GetApplicationStatusByGuidAsync(guid, bypassCache);
                if (entity == null)
                {
                    throw new KeyNotFoundException(string.Format("No admission decision was found for guid {0}.", guid));
                }
                _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(new List<string>() { entity.ApplicantRecordKey });                

                return await ConvertEntityToDtoAsync(entity, _admissionApplicationDict, bypassCache);
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
            var decisionType = (await AdmissionDecisionTypesAsync(bypassCache)).FirstOrDefault(dt => dt.Code.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (decisionType == null)
            {
                throw new KeyNotFoundException(string.Format("Admission decision type not found for code {0}.", source));
            }
            return new GuidObject2(decisionType.Guid);
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
                throw new KeyNotFoundException(string.Format("No application guid found for id: {0}", applicationId));
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
                throw new PermissionsException("User is not authorized to create admission-decisions.");
            }

            _applicationStatusRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            Domain.Student.Entities.ApplicationStatus2 appStatusEntity = await ConvertDtoToAppStatusEntityAsync(admissionDecisions);
            Domain.Student.Entities.ApplicationStatus2 entity = await _applicationStatusRepository.UpdateAdmissionDecisionAsync(appStatusEntity);

            _admissionApplicationDict = await _admissionApplicationsRepository.GetAdmissionApplicationGuidDictionary(new List<string>() { entity.ApplicantRecordKey });

            return await ConvertEntityToDtoAsync(entity, _admissionApplicationDict, true);
        }

        /// <summary>
        /// Converts dto to entity for create(POST).
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.ApplicationStatus2> ConvertDtoToAppStatusEntityAsync(Dtos.AdmissionDecisions source)
        {
            if(!source.Id.Equals(Guid.Empty.ToString()))
            {
                var recordTuple = await _applicationStatusRepository.GetApplicationStatusKey(source.Id);
                if(recordTuple != null)
                {
                    throw new ArgumentException("Changing the admission decision type is not permitted.", "id");
                }
            }

            var applicationTuple = await _applicationStatusRepository.GetApplicationStatusKey(source.Application.Id);
            if (applicationTuple == null || !applicationTuple.Item1.Equals("APPLICATIONS", StringComparison.OrdinalIgnoreCase))
            {
                throw new KeyNotFoundException(string.Format("Application not found for guid '{0}'.", source.Application.Id));
            }

            if (applicationTuple == null || !string.IsNullOrEmpty(applicationTuple.Item3))
            {
                throw new KeyNotFoundException(string.Format("Application not found for guid {0}.", source.Application.Id));
            }

            var decisionType = (await AdmissionDecisionTypesAsync(true)).FirstOrDefault(i => i.Guid.Equals(source.DecisionType.Id, StringComparison.OrdinalIgnoreCase));
            if (decisionType == null)
            {
                throw new KeyNotFoundException(string.Format("Decision type not found for guid {0}.", source.DecisionType.Id));
            }

            if(decisionType.SpecialProcessingCode != null && decisionType.SpecialProcessingCode.Equals("MS", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Admission decision type associated with 'move to students' is not permitted.");
            }

            if(string.IsNullOrEmpty(decisionType.SpecialProcessingCode))
            {
                throw new InvalidOperationException("Admission decision type is not valid for a submitted application. This admission decision type is associated with a prospect status.");
            }

            var decisionTypeCode = decisionType.Code;

            var decidedOnDate = new DateTime(source.DecidedOn.Year, source.DecidedOn.Month, source.DecidedOn.Day);
            var decidedOnTime = new DateTime(1900, 1, 1, source.DecidedOn.Hour, source.DecidedOn.Minute, source.DecidedOn.Second);

            return new Domain.Student.Entities.ApplicationStatus2(source.Id, applicationTuple.Item2, decisionTypeCode, source.DecidedOn, decidedOnTime);
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper method to determine if the user has permission to delete Student Aptitude Assessments.
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
                _decisionTypes = (await _referenceDataRepository.GetAdmissionDecisionTypesAsync(bypassCache)).ToList();
            }
            return _decisionTypes;
        }
        #endregion
    }
}