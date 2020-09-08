//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class TaxFormComponentsService : BaseCoordinationService, ITaxFormComponentsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public TaxFormComponentsService(

            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all tax-form-components
        /// </summary>
        /// <returns>Collection of TaxFormComponents DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.TaxFormComponents>> GetTaxFormComponentsAsync(bool bypassCache = false)
        {
            var taxFormComponentsCollection = new List<Ellucian.Colleague.Dtos.TaxFormComponents>();

            var taxFormComponentsEntities = await _referenceDataRepository.GetAllBoxCodesAsync(bypassCache);
            if (taxFormComponentsEntities != null && taxFormComponentsEntities.Any())
            {
                foreach (var taxFormComponents in taxFormComponentsEntities)
                {
                    taxFormComponentsCollection.Add(await ConvertTaxFormComponentsEntityToDto(taxFormComponents));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return taxFormComponentsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a TaxFormComponents from its GUID
        /// </summary>
        /// <returns>TaxFormComponents DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.TaxFormComponents> GetTaxFormComponentsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                var dto = await ConvertTaxFormComponentsEntityToDto((await _referenceDataRepository.GetAllBoxCodesAsync(bypassCache)).Where(r => r.Guid == guid).First());
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No tax-form-components was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No tax-form-components was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BoxCodes domain entity to its corresponding TaxFormComponents DTO
        /// </summary>
        /// <param name="source">BoxCodes domain entity</param>
        /// <returns>TaxFormComponents DTO</returns>
        private async Task<Dtos.TaxFormComponents> ConvertTaxFormComponentsEntityToDto(BoxCodes source)
        {
            var dto = new Dtos.TaxFormComponents();

            dto.Id = source.Guid;
            dto.Code = source.Code;
            dto.Title = source.Description;
            dto.Description = null;
            try
            {
                var code = await _referenceDataRepository.GetTaxFormsGuidAsync(source.TaxCode);
                if(string.IsNullOrWhiteSpace(code))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate GUID for tax-forms of '{0}'", source.TaxCode), "GUID.Not.Found", 
                        source.Guid, source.Code);
                }
                dto.TaxForm = new GuidObject2(code);

            }
            catch (RepositoryException)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate GUID for tax-forms of '{0}'", source.TaxCode), "GUID.Not.Found", 
                    source.Guid, source.Code);
            }

            return dto;
        }
    }
}