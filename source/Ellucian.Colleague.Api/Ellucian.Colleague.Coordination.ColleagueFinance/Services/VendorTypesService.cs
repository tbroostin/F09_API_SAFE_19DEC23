// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorTypesService : BaseCoordinationService, IVendorTypesService
    {
        private readonly IColleagueFinanceReferenceDataRepository _cfReferenceDataRepository;
        private readonly ILogger _logger;
        private readonly IConfigurationRepository _configurationRepository;

        public VendorTypesService(IColleagueFinanceReferenceDataRepository cfReferenceDataRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _cfReferenceDataRepository = cfReferenceDataRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }
               

        /// <summary>
        /// Returns all Vendor types
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VendorType>> GetVendorTypesAsync(bool bypassCache)
        {
            var vendorTypeCollection = new List<Ellucian.Colleague.Dtos.VendorType>();

            var vendorTypes = await _cfReferenceDataRepository.GetVendorTypesAsync(bypassCache);
            if (vendorTypes != null && vendorTypes.Any())
            {
                foreach (var vendorType in vendorTypes)
                {
                    vendorTypeCollection.Add(ConvertVendorTypeEntityToDto(vendorType));
                }
            }
            return vendorTypeCollection;
        }

        /// <summary>
        /// Returns an vendor type from an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.VendorType> GetVendorTypeByIdAsync(string id)
        {
            var vendorTypeEntity = (await _cfReferenceDataRepository.GetVendorTypesAsync(true)).FirstOrDefault(ct => ct.Guid == id);
            if (vendorTypeEntity == null)
            {
                throw new KeyNotFoundException("Vendor Type is not found.");
            }

            var vendorType = ConvertVendorTypeEntityToDto(vendorTypeEntity);
            return vendorType;
        }

        #region Convert method(s)

        /// <summary>
        /// Converts from VendorType entity to VendorType dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.VendorType ConvertVendorTypeEntityToDto(VendorType source)
        {
            Dtos.VendorType vendorType = new Dtos.VendorType();
            vendorType.Id = source.Guid;
            vendorType.Code = source.Code;
            vendorType.Title = source.Description;
            vendorType.Description = string.Empty;
            return vendorType;
        }

        #endregion
    }
}
