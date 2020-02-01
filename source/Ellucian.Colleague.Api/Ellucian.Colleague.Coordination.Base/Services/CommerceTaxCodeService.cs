// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CommerceTaxCodeService : BaseCoordinationService, ICommerceTaxCodeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public CommerceTaxCodeService(IAdapterRegistry adapterRegistry,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Gets all commerce tax codes
        /// </summary>
        /// <returns>Collection of CommerceTaxCode DTO objects</returns>
        public async Task<IEnumerable<Dtos.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache = false)
        {
            var taxCodeCollection = new List<Dtos.CommerceTaxCode>();

            var taxCodesEntities = await _referenceDataRepository.GetCommerceTaxCodesAsync(bypassCache);
            if (taxCodesEntities != null && taxCodesEntities.Any())
            {
                foreach (var taxCode in taxCodesEntities)
                {
                    taxCodeCollection.Add(ConvertCommerceTaxCodeEntityToCommerceTaxCodeDto(taxCode));
                }
            }

            return taxCodeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Get an commerce tax code from its GUID
        /// </summary>
        /// <returns>CommerceTaxCode DTO object</returns>
        public async Task<Dtos.CommerceTaxCode> GetCommerceTaxCodeByGuidAsync(string guid)
        {
            try
            {
                var taxCodeCollection = new List<Dtos.CommerceTaxCode>();

                var taxCodesEntities = await _referenceDataRepository.GetCommerceTaxCodesAsync(true);
                if (taxCodesEntities != null && taxCodesEntities.Any())
                {
                    foreach (var taxCode in taxCodesEntities)
                    {
                        taxCodeCollection.Add(ConvertCommerceTaxCodeEntityToCommerceTaxCodeDto(taxCode));
                    }
                }

                return taxCodeCollection.Where(om => om.Id == guid).First();
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No commerce-tax-codes was found for '{0}'.", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all commerce-tax-code-rates
        /// </summary>
        /// <returns>Collection of CommerceTaxCodeRates DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommerceTaxCodeRates>> GetCommerceTaxCodeRatesAsync(bool bypassCache = false)
        {
            var commerceTaxCodeRatesCollection = new List<Ellucian.Colleague.Dtos.CommerceTaxCodeRates>();

            IEnumerable<CommerceTaxCodeRate> commerceTaxCodeRatesEntities = null;
            try
            {
                commerceTaxCodeRatesEntities = await _referenceDataRepository.GetCommerceTaxCodeRatesAsync(bypassCache);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            if (commerceTaxCodeRatesEntities == null)
            {
                return commerceTaxCodeRatesCollection;
            }

            foreach (var commerceTaxCodeRates in commerceTaxCodeRatesEntities)
            {
                commerceTaxCodeRatesCollection.Add(await ConvertCommerceTaxCodeRatesEntityToDtoAsync(commerceTaxCodeRates));
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return commerceTaxCodeRatesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CommerceTaxCodeRates from its GUID
        /// </summary>
        /// <returns>CommerceTaxCodeRates DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CommerceTaxCodeRates> GetCommerceTaxCodeRatesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                var commerceTaxCodeRate = (await _referenceDataRepository.GetCommerceTaxCodeRatesAsync(bypassCache)).Where(r => r.Guid == guid).First();
                if (commerceTaxCodeRate == null)
                {
                    throw new KeyNotFoundException(string.Format("No commerce-tax-code-rates was found for guid '{0}'.", guid));
                }
                var retVal = await ConvertCommerceTaxCodeRatesEntityToDtoAsync(commerceTaxCodeRate);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return retVal;

            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No commerce-tax-code-rates was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No commerce-tax-code-rates was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Converts an CommerceTaxCode domain entity to its corresponding CommerceTaxCode DTO
        /// </summary>
        /// <param name="source">OtherSpecial domain entity</param>
        /// <returns>TaxCode DTO</returns>
        private Dtos.CommerceTaxCode ConvertCommerceTaxCodeEntityToCommerceTaxCodeDto(Domain.Base.Entities.CommerceTaxCode source)
        {
            if (source == null)
                throw new ArgumentNullException("Commerce Tax Code is a required field");
            DateTime? startOn = default(DateTime?);
            if (source.ApTaxEffectiveDates != null && source.ApTaxEffectiveDates.Any())
            {
                startOn = source.ApTaxEffectiveDates.OrderBy(d => d).FirstOrDefault();
            }

            var taxCode = new Dtos.CommerceTaxCode
            {
                Id = source.Guid,
                Code = source.Code,
                Title = source.Description,
                Description = null,
                StartOn = startOn
            };

            return taxCode;
        }



        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ApTaxes domain entity to its corresponding CommerceTaxCodeRates DTO
        /// </summary>
        /// <param name="source">ApTaxes domain entity</param>
        /// <returns>CommerceTaxCodeRates DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.CommerceTaxCodeRates> ConvertCommerceTaxCodeRatesEntityToDtoAsync(CommerceTaxCodeRate source)
        {

            var commerceTaxCodeRate = new Ellucian.Colleague.Dtos.CommerceTaxCodeRates();

            commerceTaxCodeRate.Id = source.Guid;
            commerceTaxCodeRate.Code = source.Code;
            commerceTaxCodeRate.Title = source.Description;
            commerceTaxCodeRate.Description = null;

            if (source.ApTaxCompoundingSequence.HasValue)
            {
                commerceTaxCodeRate.DefaultCompoundingSequence = Convert.ToInt16(source.ApTaxCompoundingSequence);
            }
      
            if (source.ApTaxPercent.HasValue)
            {
                commerceTaxCodeRate.TaxPercentage = Convert.ToDecimal(source.ApTaxPercent);
            }
           

            if (source.ApTaxRebatePercent.HasValue)
            {
                commerceTaxCodeRate.TaxRebatePercentage = source.ApTaxRebatePercent;
            }
            //required
            if (source.ApTaxEffectiveDate.HasValue)
            {
                commerceTaxCodeRate.StartOn = Convert.ToDateTime(source.ApTaxEffectiveDate);
            }
            else
            {
                IntegrationApiExceptionAddError("Effective Date is a required field.", "commerce-tax-code-rates.startOn", source.Guid, source.Code);
            }
            var apTaxGuid = await _referenceDataRepository.GetCommerceTaxCodeGuidAsync(source.Code);

            if (!string.IsNullOrEmpty(apTaxGuid))
                commerceTaxCodeRate.TaxCodes = new List<GuidObject2>()
            {
                new GuidObject2(apTaxGuid)
            };
            return commerceTaxCodeRate;
        }
    }
}