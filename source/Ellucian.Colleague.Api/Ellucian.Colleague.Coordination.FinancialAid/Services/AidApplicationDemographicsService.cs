/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
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
    /// AidApplicationDemographicsService class coordinates domain entities to interact with Financial Aid Application Demographics. 
    /// </summary>
    [RegisterType]
    public class AidApplicationDemographicsService : BaseCoordinationService, IAidApplicationDemographicsService
    {
        private readonly IAidApplicationDemographicsRepository _aidApplicationDemographicsRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IFinancialAidReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly ICountryRepository _countryRepository;
        /// <summary>
        /// Constructor for AidApplicationDemographicsService
        /// </summary>
        /// <param name="aidApplicationDemographicsRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="logger"></param>
        public AidApplicationDemographicsService(

            IAidApplicationDemographicsRepository aidApplicationDemographicsRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            ICountryRepository countryRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _aidApplicationDemographicsRepository = aidApplicationDemographicsRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _countryRepository = countryRepository;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }

        /// <summary>
        /// Gets all aid application demographics matching the criteria
        /// </summary>
        /// <returns>Collection of AidApplicationDemographics DTO objects</returns>
        public async Task<Tuple<IEnumerable<AidApplicationDemographics>, int>> GetAidApplicationDemographicsAsync(int offset, int limit, AidApplicationDemographics criteriaFilter)
        {
            var aidApplicationDemographicsDtos = new List<AidApplicationDemographics>();

            if (criteriaFilter == null)
            {
                criteriaFilter = new AidApplicationDemographics();
            }
            string personIdCriteria = criteriaFilter.PersonId;
            string aidApplicationType = criteriaFilter.ApplicationType;
            string aidYear = criteriaFilter.AidYear;
            string applicantAssignedId = criteriaFilter.ApplicantAssignedId;

            var aidApplicantionDemographicsEntities = await _aidApplicationDemographicsRepository.GetAidApplicationDemographicsAsync(offset, limit, personIdCriteria, aidApplicationType, aidYear, applicantAssignedId);
            int totalRecords = 0;
            if (aidApplicantionDemographicsEntities != null)
            {
                totalRecords = aidApplicantionDemographicsEntities.Item2;
                if (aidApplicantionDemographicsEntities != null && aidApplicantionDemographicsEntities.Item1.Any())
                {
                    foreach (var demographics in aidApplicantionDemographicsEntities.Item1)
                    {
                        aidApplicationDemographicsDtos.Add(ConvertAidApplicationDemographicsEntityToDto(demographics));
                    }

                }
            }
            return new Tuple<IEnumerable<AidApplicationDemographics>, int>(aidApplicationDemographicsDtos, totalRecords);
        }

        /// <summary>
        /// Get a AidApplicationDemographics from its Id
        /// </summary>
        /// <returns>AidApplicationDemographics DTO object</returns>
        public async Task<AidApplicationDemographics> GetAidApplicationDemographicsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id required to get an aid-application-demographics");
            }

            Domain.FinancialAid.Entities.AidApplicationDemographics applicationDemographicsEntity;
            try
            {
                applicationDemographicsEntity = await _aidApplicationDemographicsRepository.GetAidApplicationDemographicsByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (applicationDemographicsEntity == null)
            {
                throw new KeyNotFoundException("No aid-application-demographics was found for ID " + id);
            }

            return ConvertAidApplicationDemographicsEntityToDto(applicationDemographicsEntity);
        }

        /// <summary>
        /// Create a new AidApplicationDemographics record
        /// </summary>
        /// <param name="aidApplicationDemographicsDto">AidApplicationDemographics DTO</param>
        /// <returns>AidApplicationDemographics domain entity</returns>
        public async Task<AidApplicationDemographics> PostAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographicsDto)
        {
            Domain.FinancialAid.Entities.AidApplicationDemographics createdAidApplicationDemo = null;
            _aidApplicationDemographicsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographics = null;
            try
            {
                ValidateAidApplicationDemographics(aidApplicationDemographicsDto);
                aidApplicationDemographicsDto.Id = "New";
                aidApplicationDemographics = await ConvertAidApplicationDemographicsDtoToEntity(aidApplicationDemographicsDto);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationDemographics != null && !string.IsNullOrEmpty(aidApplicationDemographics.Id) ? aidApplicationDemographics.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            try
            {
                // create a AidApplicationDemographics entity in the database
                createdAidApplicationDemo = await _aidApplicationDemographicsRepository.CreateAidApplicationDemographicsAsync(aidApplicationDemographics);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationDemographics != null && !string.IsNullOrEmpty(aidApplicationDemographics.Id) ? aidApplicationDemographics.Id : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            // return the created AidApplicationDemographics Dto
            if (createdAidApplicationDemo != null)
            {
                return ConvertAidApplicationDemographicsEntityToDto(createdAidApplicationDemo);
            }
            else
            {
                IntegrationApiExceptionAddError("applicationDemographicsEntity entity cannot be null");
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return null;
        }

        /// <summary>
        /// Update an existing AidApplicationDemographics record
        /// </summary>
        /// <param name="aidApplicationDemographicsDto">AidApplicationDemographics DTO</param>
        /// <returns>AidApplicationDemographics domain entity</returns>
        public async Task<AidApplicationDemographics> PutAidApplicationDemographicsAsync(string id, AidApplicationDemographics aidApplicationDemographicsDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("id required to update an aid application demographics");
            }

            _aidApplicationDemographicsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographics = null;
            try
            {
                ValidateAidApplicationDemographics(aidApplicationDemographicsDto, false);
                aidApplicationDemographics = await ConvertAidApplicationDemographicsDtoToEntity(aidApplicationDemographicsDto);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationDemographics != null && !string.IsNullOrEmpty(aidApplicationDemographics.Id) ? aidApplicationDemographics.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            Domain.FinancialAid.Entities.AidApplicationDemographics updatedAidApplicationDemo = null;
            try
            {
                // create a AidApplicationDemographics entity in the database
                updatedAidApplicationDemo = await _aidApplicationDemographicsRepository.UpdateAidApplicationDemographicsAsync(aidApplicationDemographics);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationDemographics != null && !string.IsNullOrEmpty(aidApplicationDemographics.Id) ? aidApplicationDemographics.Id : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            // return the newly updated AidApplicationDemographics Dto
            if (updatedAidApplicationDemo != null)
            {
                return ConvertAidApplicationDemographicsEntityToDto(updatedAidApplicationDemo);
            }
            else
            {
                IntegrationApiExceptionAddError("applicationDemographicsEntity entity cannot be null");
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return null;
        }

        private AidApplicationDemographics ConvertAidApplicationDemographicsEntityToDto(Domain.FinancialAid.Entities.AidApplicationDemographics applicationDemographicsEntity)
        {
            var aidApplicationDemographicsAdapter = new AidApplicationDemographicsEntityToDtoAdapter(_adapterRegistry, logger);
            return aidApplicationDemographicsAdapter.MapToType(applicationDemographicsEntity);
        }

        private async Task<Domain.FinancialAid.Entities.AidApplicationDemographics> ConvertAidApplicationDemographicsDtoToEntity(AidApplicationDemographics aidApplicationDemographicsDto)
        {
            var finAidYears = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(false);
            if (finAidYears == null || !finAidYears.Any(x => x.Code.ToLower() == aidApplicationDemographicsDto.AidYear))
            {
                IntegrationApiExceptionAddError(string.Format("aid year  {0} not found in FA.SUITES.YEAR", aidApplicationDemographicsDto.AidYear));
            }

            var aidApplicationTypes = await _studentReferenceDataRepository.GetAidApplicationTypesAsync();
            if (aidApplicationTypes == null || !aidApplicationTypes.Any(x => x.Code.ToLower() == aidApplicationDemographicsDto.ApplicationType.ToLower()))
            {
                IntegrationApiExceptionAddError(string.Format("application type {0} is not found in ST.VALCODES - FA.APPLN.TYPES", aidApplicationDemographicsDto.ApplicationType));
            }
            if (aidApplicationDemographicsDto.Address != null)
            {
                if (!string.IsNullOrEmpty(aidApplicationDemographicsDto.Address.Country))
                {
                    var country = aidApplicationDemographicsDto.Address.Country;
                    var countries = await _countryRepository.GetCountryCodesAsync(false);
                    if (countries == null || !countries.Any(x => x.Description.ToLower() == country.ToLower() || x.Code == country))
                    {
                        IntegrationApiExceptionAddError(string.Format("Country '{0}' cannot be found on the places table. ", country), "aidApplicationDemographicsDto.country");
                    }
                }
                if (!string.IsNullOrEmpty(aidApplicationDemographicsDto.Address.State))
                {

                    var state = aidApplicationDemographicsDto.Address.State;
                    var states = await _referenceDataRepository.GetStateCodesAsync(false);
                    if (states == null || !states.Any(x => x.Description.ToLower() == state.ToLower() || x.Code == state))
                    {
                        IntegrationApiExceptionAddError(string.Format("State '{0}' cannot be found on the places table. ", state), "aidApplicationDemographicsDto.state");
                    }
                }
            }


            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(aidApplicationDemographicsDto.Id, aidApplicationDemographicsDto.PersonId,
                aidApplicationDemographicsDto.AidYear, aidApplicationDemographicsDto.ApplicationType);

            aidApplicationDemographicsEntity.ApplicantAssignedId = aidApplicationDemographicsDto.ApplicantAssignedId;
            aidApplicationDemographicsEntity.LastName = aidApplicationDemographicsDto.LastName;
            aidApplicationDemographicsEntity.OrigName = aidApplicationDemographicsDto.OrigName;
            aidApplicationDemographicsEntity.FirstName = aidApplicationDemographicsDto.FirstName;
            aidApplicationDemographicsEntity.MiddleInitial = aidApplicationDemographicsDto.MiddleInitial;

            if (aidApplicationDemographicsDto.Address != null)
            {
                aidApplicationDemographicsEntity.Address = new Domain.FinancialAid.Entities.Address();
                aidApplicationDemographicsEntity.Address.AddressLine = aidApplicationDemographicsDto.Address.AddressLine;
                aidApplicationDemographicsEntity.Address.City = aidApplicationDemographicsDto.Address.City;
                aidApplicationDemographicsEntity.Address.State = aidApplicationDemographicsDto.Address.State;
                aidApplicationDemographicsEntity.Address.Country = aidApplicationDemographicsDto.Address.Country;
                aidApplicationDemographicsEntity.Address.ZipCode = aidApplicationDemographicsDto.Address.ZipCode;
            }

            aidApplicationDemographicsEntity.BirthDate = aidApplicationDemographicsDto.BirthDate;
            aidApplicationDemographicsEntity.PhoneNumber = aidApplicationDemographicsDto.PhoneNumber;
            aidApplicationDemographicsEntity.EmailAddress = aidApplicationDemographicsDto.EmailAddress;
            if (aidApplicationDemographicsDto.CitizenshipStatusType.HasValue)
            {
                aidApplicationDemographicsEntity.CitizenshipStatusType = GetCitizenshipStatus(aidApplicationDemographicsDto.CitizenshipStatusType.Value);
            }
            aidApplicationDemographicsEntity.AlternatePhoneNumber = aidApplicationDemographicsDto.AlternatePhoneNumber;
            aidApplicationDemographicsEntity.StudentTaxIdNumber = aidApplicationDemographicsDto.StudentTaxIdNumber;

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return aidApplicationDemographicsEntity;
        }

        private void ValidateAidApplicationDemographics(AidApplicationDemographics aidApplicationDemographicsDto, bool createRecord = true)
        {
            string createUpdateText = createRecord ? "create" : "update";

            if (aidApplicationDemographicsDto == null)
                IntegrationApiExceptionAddError("aidApplicationDemographicsDto", string.Format("Must provide a aidApplicationDemographics for {0}", createUpdateText));

            if (!createRecord && string.IsNullOrEmpty(aidApplicationDemographicsDto.Id))
                IntegrationApiExceptionAddError("aidApplicationDemographicsDto", string.Format("Must provide a id for aidApplicationDemographics for {0}", createUpdateText));

            if (string.IsNullOrEmpty(aidApplicationDemographicsDto.PersonId))
            {
                IntegrationApiExceptionAddError("aidApplicationDemographicsDto", string.Format("Must provide personId for aidApplicationDemographics for {0}", createUpdateText));
            }
            if (string.IsNullOrEmpty(aidApplicationDemographicsDto.ApplicationType))
            {
                IntegrationApiExceptionAddError("aidApplicationDemographicsDto", string.Format("Must provide applicationType for aidApplicationDemographics for {0}", createUpdateText));
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
        }

        private static Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus GetCitizenshipStatus(Dtos.FinancialAid.AidApplicationCitizenshipStatus citizenshipStatusTypeDto)
        {
            Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus citizenshipStatusType;
            switch (citizenshipStatusTypeDto)
            {
                case AidApplicationCitizenshipStatus.Citizen:
                    citizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.Citizen;
                    break;
                case AidApplicationCitizenshipStatus.NonCitizen:
                    citizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.NonCitizen;
                    break;
                case AidApplicationCitizenshipStatus.NotEligible:
                    citizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.NotEligible;
                    break;

                default:
                    throw new ApplicationException("Invalid Citizenship status: " + citizenshipStatusTypeDto.ToString());
            }
            return citizenshipStatusType;
        }
    }
}
