//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;


namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorHoldReasonsService : BaseCoordinationService, IVendorHoldReasonsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public VendorHoldReasonsService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Gets all vendor-hold-reasons
        /// </summary>
        /// <returns>Collection of VendorHoldReasons DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VendorHoldReasons>> GetVendorHoldReasonsAsync(bool bypassCache = false)
        {
            var VendorHoldReasonsCollection = new List<Ellucian.Colleague.Dtos.VendorHoldReasons>();

            var vendorHoldReasonsEntities = await _referenceDataRepository.GetVendorHoldReasonsAsync(bypassCache);
            if (vendorHoldReasonsEntities != null && vendorHoldReasonsEntities.Any())
            {
                foreach (var VendorHoldReasons in vendorHoldReasonsEntities)
                {
                    VendorHoldReasonsCollection.Add(ConvertVendorHoldReasonsEntityToDto(VendorHoldReasons));
                }
            }
            return VendorHoldReasonsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Get a VendorHoldReasons from its GUID
        /// </summary>
        /// <returns>VendorHoldReasons DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.VendorHoldReasons> GetVendorHoldReasonsByGuidAsync(string guid)
        {
            try
            {
                return ConvertVendorHoldReasonsEntityToDto((await _referenceDataRepository.GetVendorHoldReasonsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("vendor-hold-reasons not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a VendorHoldReasons domain entity to its corresponding VendorHoldReasons DTO
        /// </summary>
        /// <param name="source">VendorHoldReasons domain entity</param>
        /// <returns>VendorHoldReasons DTO</returns>
        private Ellucian.Colleague.Dtos.VendorHoldReasons ConvertVendorHoldReasonsEntityToDto(Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorHoldReasons source)
        {
            var vendorHoldReasons = new Ellucian.Colleague.Dtos.VendorHoldReasons();

            vendorHoldReasons.Id = source.Guid;
            vendorHoldReasons.Code = source.Code;
            vendorHoldReasons.Title = source.Description;
            vendorHoldReasons.Description = null;
            return vendorHoldReasons;
        }


    }
}
