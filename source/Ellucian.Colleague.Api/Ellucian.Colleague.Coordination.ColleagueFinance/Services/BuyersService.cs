//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class BuyersService : BaseCoordinationService, IBuyersService
    {

        private readonly IBuyerRepository _referenceDataRepository;

        public BuyersService(
            IBuyerRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger, 
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all buyers
        /// </summary>
        /// <returns>Collection of Buyers DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Buyers>,int>> GetBuyersAsync(int offset, int limit, bool bypassCache = false)
        {
            var buyersCollection = new List<Ellucian.Colleague.Dtos.Buyers>();

            var buyersTuple = await _referenceDataRepository.GetBuyersAsync(offset, limit, bypassCache);

            var buyersEntities = buyersTuple.Item1;

            if (buyersEntities != null && buyersEntities.Any())
            {
                foreach (var buyers in buyersEntities)
                {
                    buyersCollection.Add(ConvertBuyersEntityToDto(buyers));
                }
            }
            return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.Buyers>,int> (buyersCollection, buyersTuple.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Buyers from its GUID
        /// </summary>
        /// <returns>Buyers DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Buyers> GetBuyersByGuidAsync(string guid)
        {
            try
            {
                return ConvertBuyersEntityToDto((await _referenceDataRepository.GetBuyerAsync(guid)));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Buyer not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Buyer not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Buyers domain entity to its corresponding Buyers DTO
        /// </summary>
        /// <param name="source">Buyers domain entity</param>
        /// <returns>Buyers DTO</returns>
        private Ellucian.Colleague.Dtos.Buyers ConvertBuyersEntityToDto(Buyer source)
        {
            var buyer = new Ellucian.Colleague.Dtos.Buyers();

            buyer.Id = source.Guid;
            if (string.IsNullOrWhiteSpace(source.Name))
            {
                throw new ColleagueWebApiException("The name was not found for this Staff record, Entity: Staff, Record ID: " + source.RecordKey);
            }

            var buyerPersonName = new Dtos.DtoProperties.NamePersonDtoProperty();

            if (!string.IsNullOrEmpty(source.PersonGuid))
            {
                buyerPersonName.Person = new GuidObject2(source.PersonGuid);
            }
            else
            {
                buyerPersonName.Name = source.Name;
            }
            if ((buyerPersonName.Person != null) || (!(string.IsNullOrEmpty(buyerPersonName.Name))))
            {
                buyer.Buyer = buyerPersonName;
            }

            buyer.StartOn = source.StartOn;
            buyer.EndOn = source.EndOn;
            
            switch (source.Status)
            {
                case "active":
                    buyer.Status = BuyerStatus.Active;
                    break;
                case "inactive":
                    buyer.Status = BuyerStatus.Inactive;
                    break;
            }

            return buyer;
        }


    }
}

