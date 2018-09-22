//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorPaymentTermsService : BaseCoordinationService, IVendorPaymentTermsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        private readonly IConfigurationRepository _configurationRepository;

        public VendorPaymentTermsService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,           
            ILogger logger,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Gets all vendor-payment-terms
        /// </summary>
        /// <returns>Collection of VendorPaymentTerms DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VendorPaymentTerms>> GetVendorPaymentTermsAsync(bool bypassCache = false)
        {
            var vendorPaymentTermsCollection = new List<Ellucian.Colleague.Dtos.VendorPaymentTerms>();

            var vendorPaymentTermsEntities = await _referenceDataRepository.GetVendorTermsAsync(bypassCache);
            if (vendorPaymentTermsEntities != null && vendorPaymentTermsEntities.Any())
            {
                foreach (var vendorPaymentTerms in vendorPaymentTermsEntities)
                {
                    vendorPaymentTermsCollection.Add(ConvertVendorTermsEntityToDto(vendorPaymentTerms));
                }
            }
            return vendorPaymentTermsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Get a VendorPaymentTerms from its GUID
        /// </summary>
        /// <returns>VendorPaymentTerms DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.VendorPaymentTerms> GetVendorPaymentTermsByGuidAsync(string guid)
        {
            try
            {
                return ConvertVendorTermsEntityToDto((await _referenceDataRepository.GetVendorTermsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("vendor-payment-terms not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a VendorTerms domain entity to its corresponding VendorPaymentTerms DTO
        /// </summary>
        /// <param name="source">VendorTerm domain entity</param>
        /// <returns>VendorPaymentTerms DTO</returns>
        private Ellucian.Colleague.Dtos.VendorPaymentTerms ConvertVendorTermsEntityToDto(VendorTerm source)
        {
            var vendorPaymentTerms = new Ellucian.Colleague.Dtos.VendorPaymentTerms();

            vendorPaymentTerms.Id = source.Guid;
            vendorPaymentTerms.Code = source.Code;
            vendorPaymentTerms.Title = source.Description;
            vendorPaymentTerms.Description = null;

            return vendorPaymentTerms;
        }
    }
}