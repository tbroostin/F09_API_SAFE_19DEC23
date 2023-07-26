/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// AidApplicationAdditionalInfoService class coordinates domain entities to interact with Financial Aid Application Additional Info. 
    /// </summary>
    [RegisterType]
    public class AidApplicationAdditionalInfoService : BaseCoordinationService, IAidApplicationAdditionalInfoService
    {
        private readonly IAidApplicationAdditionalInfoRepository _aidApplicationAdditionalInfoRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IFinancialAidReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly IAidApplicationDemographicsRepository _aidApplicationsDemoRepository;

        /// <summary>
        /// Constructor for AidApplicationAdditionalInfoService
        /// </summary>
        /// <param name="aidApplicationAdditionalInfoRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="configurationRepository"></param>
        public AidApplicationAdditionalInfoService(IAidApplicationAdditionalInfoRepository aidApplicationAdditionalInfoRepository,            
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAidApplicationDemographicsRepository aidApplicationsDemoRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
           ICurrentUserFactory currentUserFactory,
           IRoleRepository roleRepository,
           ILogger logger,           
           IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _aidApplicationAdditionalInfoRepository = aidApplicationAdditionalInfoRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _aidApplicationsDemoRepository = aidApplicationsDemoRepository;
        }

        /// <summary>
        /// Gets all aidApplicationAdditionalInfo matching the criteria
        /// </summary>
        /// <returns>Collection of AidApplicationAdditionalInfo DTO objects</returns>
        public async Task<Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>> GetAidApplicationAdditionalInfoAsync(int offset, int limit, AidApplicationAdditionalInfo criteriaFilter)
        {
            var aidApplicationAdditionalInfoDtos = new List<AidApplicationAdditionalInfo>();

            if (criteriaFilter == null)
            {
                criteriaFilter = new AidApplicationAdditionalInfo();
            }
            string appDemoId = criteriaFilter.AppDemoId;
            string personIdCriteria = criteriaFilter.PersonId;
            string aidApplicationType = criteriaFilter.ApplicationType;
            string aidYear = criteriaFilter.AidYear;
            string applicantAssignedId = criteriaFilter.ApplicantAssignedId;

            var aidApplicationAdditionalInfoEntities = await _aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoAsync(offset, limit, appDemoId, personIdCriteria, aidApplicationType, aidYear, applicantAssignedId);
            int totalRecords = 0;
            if(aidApplicationAdditionalInfoEntities != null)
            {
                totalRecords = aidApplicationAdditionalInfoEntities.Item2;
                if (aidApplicationAdditionalInfoEntities != null && aidApplicationAdditionalInfoEntities.Item1.Any())
                {
                    foreach (var additionalInfo in aidApplicationAdditionalInfoEntities.Item1)
                    {
                        aidApplicationAdditionalInfoDtos.Add(ConvertAidApplicationAdditionalInfoEntityToDto(additionalInfo));
                    }
                }
            }            
            return new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(aidApplicationAdditionalInfoDtos, totalRecords);
        }

        /// <summary>
        /// Get a AidApplicationAdditionalInfo from its Id
        /// </summary>
        /// <returns>AidApplicationAdditionalInfo DTO object</returns>
        public async Task<AidApplicationAdditionalInfo> GetAidApplicationAdditionalInfoByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id required to get an aid application additional info");
            }

            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity;
            try
            {
                aidApplicationAdditionalInfoEntity = await _aidApplicationAdditionalInfoRepository.GetAidApplicationAdditionalInfoByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (aidApplicationAdditionalInfoEntity == null)
            {
                throw new KeyNotFoundException("No Aid application additional info was found for ID " + id);
            }

            return ConvertAidApplicationAdditionalInfoEntityToDto(aidApplicationAdditionalInfoEntity);
        }

        /// <summary>
        /// Create a new AidApplicationAdditionalInfo record
        /// </summary>
        /// <param name="aidApplicationAdditionalInfoDto">aidApplicationAdditionalInfo DTO</param>
        /// <returns>AidApplicationAdditionalInfo domain entity</returns>
        public async Task<AidApplicationAdditionalInfo> PostAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto)
        {
            _aidApplicationAdditionalInfoRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo createdAidApplicationAdditionalInfo = null;
            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfo = null;
            try
            {
                ValidateAidApplicationAdditionalInfo(aidApplicationAdditionalInfoDto);
                aidApplicationAdditionalInfoDto.Id = "new";
                aidApplicationAdditionalInfo = await ConvertAidApplicationAdditionalInfoDtoToEntity(aidApplicationAdditionalInfoDto);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationAdditionalInfo != null && !string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId) ? aidApplicationAdditionalInfo.AppDemoId : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            try
            {                
                // create a AidApplicationDemographics entity in the database
                createdAidApplicationAdditionalInfo = await _aidApplicationAdditionalInfoRepository.CreateAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfo);

            }                     
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationAdditionalInfo != null && !string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId) ? aidApplicationAdditionalInfo.AppDemoId : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            if (createdAidApplicationAdditionalInfo != null)
            {
                return ConvertAidApplicationAdditionalInfoEntityToDto(createdAidApplicationAdditionalInfo);
            }
            else
            {
                IntegrationApiExceptionAddError("applicationAdditionalInfoEntity entity cannot be null");
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return null;           

        }

        /// <summary>
        /// Update an existing AidApplicationAdditionalInfo record
        /// </summary>
        /// <param name="aidApplicationAdditionalInfoDto">AidApplicationAdditionalInfo DTO</param>
        /// <returns>AidApplicationAdditionalInfo domain entity</returns>
        public async Task<AidApplicationAdditionalInfo> PutAidApplicationAdditionalInfoAsync(string id, AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("id required to update an aid application additional info");
            }

            
            _aidApplicationAdditionalInfoRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfo = null;
            try
            {
                ValidateAidApplicationAdditionalInfo(aidApplicationAdditionalInfoDto);
                aidApplicationAdditionalInfo = await ConvertAidApplicationAdditionalInfoDtoToEntity(aidApplicationAdditionalInfoDto, true);
            }            
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationAdditionalInfo != null && !string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId) ? aidApplicationAdditionalInfo.AppDemoId : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo updatedAidApplicationAdditionalInfo = null;

            try
            {
                // create a AidApplicationAdditionalInfo entity in the database
                updatedAidApplicationAdditionalInfo = await _aidApplicationAdditionalInfoRepository.UpdateAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfo);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationAdditionalInfo != null && !string.IsNullOrEmpty(aidApplicationAdditionalInfo.AppDemoId) ? aidApplicationAdditionalInfo.AppDemoId : null);

            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }


            if (updatedAidApplicationAdditionalInfo != null)
            {
                return ConvertAidApplicationAdditionalInfoEntityToDto(updatedAidApplicationAdditionalInfo);
            }
            else
            {
                IntegrationApiExceptionAddError("applicationAdditionalInfoEntity entity cannot be null");
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return null;
            
        }

        /// <summary>
        /// Converts a AidApplicationAdditionalInfo domain entity to its corresponding AidApplicationAdditionalInfo DTO
        /// </summary>
        /// <returns>AidApplicationAdditionalInfo DTO</returns>
        private AidApplicationAdditionalInfo ConvertAidApplicationAdditionalInfoEntityToDto(Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity)
        {
            var aidApplicationAdditionalInfoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo, AidApplicationAdditionalInfo>();
            return aidApplicationAdditionalInfoAdapter.MapToType(aidApplicationAdditionalInfoEntity);
            
        }

        private async Task<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo> ConvertAidApplicationAdditionalInfoDtoToEntity(AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto, bool updateRecord = false)
        {
            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographics;
            aidApplicationDemographics = await _aidApplicationsDemoRepository.GetAidApplicationDemographicsByIdAsync(aidApplicationAdditionalInfoDto.AppDemoId);

            Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo(aidApplicationAdditionalInfoDto.Id, aidApplicationAdditionalInfoDto.AppDemoId);

            var finAidYears = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(false);
            if (updateRecord && aidApplicationAdditionalInfoDto.Id != aidApplicationAdditionalInfoDto.AppDemoId)
            {
                IntegrationApiExceptionAddError(string.Format("Value of id and appDemoId does not match"));
                return null;
            }
            if (aidApplicationAdditionalInfoDto.AidYear != null && aidApplicationAdditionalInfoDto.AidYear != aidApplicationDemographics.AidYear)
            {
                IntegrationApiExceptionAddError(string.Format("The aidYear associated with appDemoId does not match the input aidYear"));
            }
            if (aidApplicationAdditionalInfoDto.PersonId != null && aidApplicationAdditionalInfoDto.PersonId != aidApplicationDemographics.PersonId)
            {
                IntegrationApiExceptionAddError(string.Format("The personId associated with appDemoId does not match the input personId"));
            }
            if (aidApplicationAdditionalInfoDto.ApplicationType != null && aidApplicationAdditionalInfoDto.ApplicationType != aidApplicationDemographics.ApplicationType)
            {
                IntegrationApiExceptionAddError(string.Format("The applicationType associated with appDemoId does not match the input applicationType"));
            }
            aidApplicationAdditionalInfoDto.AidYear = aidApplicationDemographics.AidYear;
            aidApplicationAdditionalInfoDto.PersonId = aidApplicationDemographics.PersonId;
            aidApplicationAdditionalInfoDto.ApplicationType = aidApplicationDemographics.ApplicationType;
            if (finAidYears == null || !finAidYears.Any(x => x.Code.ToLower() == aidApplicationAdditionalInfoDto.AidYear))
            {
                IntegrationApiExceptionAddError(string.Format("aid year {0} not found in FA.SUITES.YEAR", aidApplicationAdditionalInfoDto.AidYear));
            }
            var aidApplicationTypes = await _studentReferenceDataRepository.GetAidApplicationTypesAsync();
            if (aidApplicationTypes == null || !aidApplicationTypes.Any(x => x.Code.ToLower() == aidApplicationAdditionalInfoDto.ApplicationType.ToLower()))
            {
                IntegrationApiExceptionAddError(string.Format("aid application type {0} is not found in ST.VALCODES - FA.APPLN.TYPES", aidApplicationAdditionalInfoDto.ApplicationType));
            }
            

            var aidApplicationAdditionalInfoAdapter = _adapterRegistry.GetAdapter<AidApplicationAdditionalInfo, Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>();
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return aidApplicationAdditionalInfoAdapter.MapToType(aidApplicationAdditionalInfoDto);
        }

        private void ValidateAidApplicationAdditionalInfo(AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto)
        {

            if (aidApplicationAdditionalInfoDto == null)
                IntegrationApiExceptionAddError("aidApplicationAdditionalInfoDto", string.Format("Must provide a aidApplicationAdditionalInfo"));

            if (string.IsNullOrEmpty(aidApplicationAdditionalInfoDto.AppDemoId))
                IntegrationApiExceptionAddError("aidApplicationAdditionalInfoDto", string.Format("Must provide a value for appDemoId"));

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
        }
    }
}
