// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
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
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Gets all commerce tax codes
        /// </summary>
        /// <returns>Collection of CommerceTaxCode DTO objects</returns>
        public async Task<IEnumerable<CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache = false)
        {
            var taxCodeCollection = new List<CommerceTaxCode>();

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
        public async Task<CommerceTaxCode> GetCommerceTaxCodeByGuidAsync(string guid)
        {
            try
            {
                var taxCodeCollection = new List<CommerceTaxCode>();

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
                throw new KeyNotFoundException("Commerce Tax Code not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Converts an CommerceTaxCode domain entity to its corresponding CommerceTaxCode DTO
        /// </summary>
        /// <param name="source">OtherSpecial domain entity</param>
        /// <returns>TaxCode DTO</returns>
        private CommerceTaxCode ConvertCommerceTaxCodeEntityToCommerceTaxCodeDto(Domain.Base.Entities.CommerceTaxCode source)
        {
            if (source == null)
                throw new ArgumentNullException("Commerce Tax Code is a required field");

            var taxCode = new CommerceTaxCode
            {
                Id = source.Guid,
                Code = source.Code,
                Title = source.Description,
                Description = null,
            };

            return taxCode;
        }
    }
}