//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class TaxFormsBaseService : BaseCoordinationService, ITaxFormsBaseService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public TaxFormsBaseService(

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
        /// Gets all tax-forms
        /// </summary>
        /// <returns>Collection of TaxForms DTO objects</returns>
        public async Task<IEnumerable<Dtos.TaxForms>> GetTaxFormsAsync(bool bypassCache = false)
        {
            var taxFormsCollection = new List<Dtos.TaxForms>();

            var taxFormsEntities = await _referenceDataRepository.GetTaxFormsBaseAsync(bypassCache);
            if (taxFormsEntities != null && taxFormsEntities.Any())
            {
                foreach (var taxForms in taxFormsEntities)
                {
                    taxFormsCollection.Add(ConvertTaxFormsEntityToDto(taxForms));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return taxFormsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a TaxForms from its GUID
        /// </summary>
        /// <returns>TaxForms DTO object</returns>
        public async Task<Dtos.TaxForms> GetTaxFormsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertTaxFormsEntityToDto((await _referenceDataRepository.GetTaxFormsBaseAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No tax-forms was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No tax-forms was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a TaxForms domain entity to its corresponding TaxForms DTO
        /// </summary>
        /// <param name="source">TaxForms domain entity</param>
        /// <returns>TaxForms DTO</returns>
        private Dtos.TaxForms ConvertTaxFormsEntityToDto(TaxForms2 source)
        {
            var taxForms = new Dtos.TaxForms();

            taxForms.Id = source.Guid;
            taxForms.Code = source.Code;
            taxForms.Title = source.Description;
            taxForms.Description = null;

            return taxForms;
        }
    }
}