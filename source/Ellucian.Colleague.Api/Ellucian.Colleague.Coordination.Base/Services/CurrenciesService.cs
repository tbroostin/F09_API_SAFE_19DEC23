// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CurrenciesService : BaseCoordinationService, ICurrenciesService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public CurrenciesService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ICurrencyRepository currencyRepository,
            IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _currencyRepository = currencyRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Currencies">currencies</see> objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Currencies>> GetCurrenciesAsync(bool bypassCache = false)
        {
            var currenciesCollection = new List<Ellucian.Colleague.Dtos.Currencies>();

            List<Domain.Base.Entities.CurrencyConv> currencyEntities = null;

            try
            {
                currencyEntities = (await _currencyRepository.GetCurrencyConversionAsync(bypassCache)).ToList();
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            foreach (var currencyEntity in currencyEntities)
            {
                if (currencyEntity.Guid != null)
                {
                    currenciesCollection.Add(ConvertCurrencyEntityToCurrenciesDto(currencyEntity, bypassCache));
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return currenciesCollection;
        }

        /// <summary>
        /// Get a currencies by guid.
        /// </summary>
        /// <param name="guid">Guid of the currencies in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Currencies">currencies</see></returns>
        public async Task<Dtos.Currencies> GetCurrenciesByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a currency.");
            }

            Domain.Base.Entities.CurrencyConv currencyEntity = null;
            try
            {
                currencyEntity = await _currencyRepository.GetCurrencyConversionByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (currencyEntity == null)
            {
                throw new KeyNotFoundException("No currency was found for guid " + guid);
            }

            var retval = ConvertCurrencyEntityToCurrenciesDto(currencyEntity);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return retval;
        }

        /// <summary>
        /// Get a currencies by guid.
        /// </summary>
        /// <param name="guid">Guid of the currencies in Colleague.</param>
        /// <param name="currencies">currencies dto to update</param>
        /// <returns>The <see cref="Currencies">updated currencies</see></returns>
        public async Task<Dtos.Currencies> PutCurrenciesAsync(string guid, Dtos.Currencies currencies)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to update a Currency.");
            }

            if (currencies == null)
            {
                throw new ArgumentNullException("currencies", "Message body required to update a Currency");
            }

            CheckCurrencyCreateUpdatePermissions();


            var existingCurrencies = await this.GetCurrenciesByGuidAsync(guid);

            Validate(guid, currencies, existingCurrencies);

            _currencyRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            var entity = this.ConvertCurrenciesDtoToCurrencyEntity(currencies);

            if (IntegrationApiException != null)
                throw IntegrationApiException;
     
            Domain.Base.Entities.CurrencyConv newEntity = null;
            try
            {
                newEntity = await _currencyRepository.UpdateCurrencyConversionAsync(entity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            var newDto = ConvertCurrencyEntityToCurrenciesDto(newEntity);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return newDto;
        }

        /// <summary>
        /// Validate currency dto for update
        /// </summary>
        /// <param name="guid">currency guid</param>
        /// <param name="currencies">currency dto</param>
        /// <param name="existingCurrencyDto">existing currency record</param>
        private void Validate(string guid, Currencies currencies, Currencies existingCurrencyDto)
        {
            if (existingCurrencyDto == null)
            {
                throw new KeyNotFoundException("No currency was found for guid " + guid);
            }

            //If the code is included in the request as an empty string, then issue an error 
            if (string.IsNullOrEmpty(currencies.Code))
            {
                IntegrationApiExceptionAddError("The code cannot be removed for an existing currency.");
            }
            if (string.IsNullOrEmpty(currencies.Title))
            {
                IntegrationApiExceptionAddError("The title is required.");
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            //The code cannot be changed to a different code than the original. 
            if (!existingCurrencyDto.Code.Equals(currencies.Code))
            {
                IntegrationApiExceptionAddError("The code cannot be changed for an existing currency.", guid: guid);
            }

            if (!existingCurrencyDto.Title.Equals(currencies.Title))
            {
                IntegrationApiExceptionAddError("The title cannot be changed for an existing currency.", guid: guid);
            }

            if ((!string.IsNullOrEmpty(currencies.ISOCode)) && (currencies.ISOCode.Length > 3))
            {
                IntegrationApiExceptionAddError("The ISO Code can not be greater than 3 characters.", guid: guid);
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all currency-iso-codes
        /// </summary>
        /// <returns>Collection of CurrencyIsoCodes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CurrencyIsoCodes>> GetCurrencyIsoCodesAsync(bool bypassCache = false)
        {
            var currencyIsoCodesCollection = new List<Ellucian.Colleague.Dtos.CurrencyIsoCodes>();

            var currencyIsoCodesEntities = await _currencyRepository.GetIntgIsoCurrencyCodesAsync(bypassCache);
            if (currencyIsoCodesEntities != null && currencyIsoCodesEntities.Any())
            {
                foreach (var currencyIsoCodes in currencyIsoCodesEntities)
                {
                    currencyIsoCodesCollection.Add(ConvertCurrencyIsoCodesEntityToDto(currencyIsoCodes));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return currencyIsoCodesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CurrencyIsoCodes from its GUID
        /// </summary>
        /// <returns>CurrencyIsoCodes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CurrencyIsoCodes> GetCurrencyIsoCodesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCurrencyIsoCodesEntityToDto((await _currencyRepository.GetIntgIsoCurrencyCodesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No currency-iso-codes was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No currency-iso-codes was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Convert currency domain entity to a currencies DTO
        /// </summary>
        /// <param name="source">currency domain entity</param>
        /// <returns>currencies DTO</returns>
        private Dtos.Currencies ConvertCurrencyEntityToCurrenciesDto(Domain.Base.Entities.CurrencyConv source, bool bypassCache = true)
        {
            var currency = new Ellucian.Colleague.Dtos.Currencies();

            if (source == null)
            {
                IntegrationApiExceptionAddError("Currency body is required.");
                return null;
            }

            if (source.Guid == null)
            {
                IntegrationApiExceptionAddError("Currency.Id is required.");
                return null;
            }
            currency.Id = source.Guid;
            currency.Code = source.Code;
            currency.Title = source.Description;
            currency.ISOCode = string.IsNullOrEmpty(source.IsoCode) ? null:  source.IsoCode;

            return currency;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert a currencies dto to a currency entity
        /// </summary>
        /// <param name="source">Currencies DTO</param>
        /// <returns>Currency domain entity</returns>
        private Domain.Base.Entities.CurrencyConv ConvertCurrenciesDtoToCurrencyEntity(Dtos.Currencies source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("Currency body is required.");
                throw IntegrationApiException;
            }
            if (source.Id == null)
            {
                IntegrationApiExceptionAddError("Currency.Id is required.");
                throw IntegrationApiException;
            }
            Domain.Base.Entities.CurrencyConv currency = null;

            try
            {
                currency = new Domain.Base.Entities.CurrencyConv(source.Id, source.Code, source.Title,
                    (!string.IsNullOrEmpty(source.ISOCode)) ?  source.ISOCode.ToUpper() : string.Empty);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An error occurred building the currency: " + ex.Message, "currencies",  guid: source.Id);
            }
            return currency;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgIsoCurrencyCodes domain entity to its corresponding CurrencyIsoCodes DTO
        /// </summary>
        /// <param name="source">IntgIsoCurrencyCodes domain entity</param>
        /// <returns>CurrencyIsoCodes DTO</returns>
        private Ellucian.Colleague.Dtos.CurrencyIsoCodes ConvertCurrencyIsoCodesEntityToDto(IntgIsoCurrencyCodes source)
        {
            var currencyIsoCodes = new Ellucian.Colleague.Dtos.CurrencyIsoCodes();

            currencyIsoCodes.Id = source.Guid;
            currencyIsoCodes.ISOCode = string.IsNullOrEmpty(source.Code) ? null : source.Code;
            currencyIsoCodes.Title = source.Description;
          
            currencyIsoCodes.Status = (!string.IsNullOrEmpty(source.Status) && source.Status.Equals("I", StringComparison.OrdinalIgnoreCase))
                ? Status.Inactive : Status.Active;
            return currencyIsoCodes;
        }

        /// <summary>
        /// Provides an integration user permission to update Currencies
        /// </summary>
        private void CheckCurrencyCreateUpdatePermissions()
        {
            // access is ok if the current user has the create/update currency permission
            if (!HasPermission(BasePermissionCodes.UpdateCurrency))
            {
                logger.Error("User '" + CurrentUser.UserId + "' does not have permission to update currencies.");
                throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to update currencies.");
            }
        }    
    }
}