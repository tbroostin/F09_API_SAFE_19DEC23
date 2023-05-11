// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Representation of an E-Commerce service
    /// </summary>
    [RegisterType]
    public class ECommerceService : BaseCoordinationService, IECommerceService
    {
        private IECommerceRepository _ecommerceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ECommerceService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="ecommerceRepository">The ecommerce repository.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public ECommerceService(IAdapterRegistry adapterRegistry, IECommerceRepository ecommerceRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _ecommerceRepository = ecommerceRepository;
        }

        /// <summary>
        /// Get all the defined convenience fees
        /// </summary>
        /// <returns>List of ReceivableType DTOs</returns>
        public IEnumerable<ConvenienceFee> GetConvenienceFees()
        {
            try
            {
                var entityCollection = _ecommerceRepository.ConvenienceFees;
                var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ConvenienceFee, ConvenienceFee>();

                var dtoCollection = new List<ConvenienceFee>();
                foreach (var entity in entityCollection)
                {
                    dtoCollection.Add(adapter.MapToType(entity));
                }

                return dtoCollection;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }
    }
}
