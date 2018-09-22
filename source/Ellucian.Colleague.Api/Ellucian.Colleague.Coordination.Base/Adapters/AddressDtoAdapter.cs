// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping Address Dto to Entity
    /// </summary>
    public class AddressDtoAdapter : AutoMapperAdapter<Dtos.Base.Address, Domain.Base.Entities.Address>
    {
        /// <summary>
        /// Initializes a new instance of the AddressDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public AddressDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
        }

        /// <summary>
        /// Override automapper because some addresses may come in without an id.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Domain.Base.Entities.Address MapToType(Dtos.Base.Address source)
        {
            Domain.Base.Entities.Address addressEntity = null;
            if (string.IsNullOrEmpty(source.AddressId))
            {
                addressEntity = new Domain.Base.Entities.Address(source.PersonId);
            }
            else
            {
                addressEntity = new Domain.Base.Entities.Address(source.AddressId, source.PersonId);
            }
            addressEntity.AddressLines = source.AddressLines;
            addressEntity.AddressModifier = source.AddressModifier;
            addressEntity.City = source.City;
            addressEntity.CountryCode = source.CountryCode;
            addressEntity.County = source.County;
            addressEntity.Country = source.Country;
            addressEntity.EffectiveEndDate = source.EffectiveEndDate;
            addressEntity.EffectiveStartDate = source.EffectiveStartDate;
            addressEntity.IsPreferredAddress = source.IsPreferredAddress;
            addressEntity.IsPreferredResidence = source.IsPreferredResidence;
            addressEntity.PostalCode = source.PostalCode;
            addressEntity.RouteCode = source.RouteCode;
            addressEntity.State = source.State;
            addressEntity.Type = source.Type;
            addressEntity.TypeCode = source.TypeCode;
            var phoneDtoAdapter = adapterRegistry.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
            if (source.PhoneNumbers != null)
            {   
                foreach (var phoneDto in source.PhoneNumbers)
                {
                    addressEntity.AddPhone(phoneDtoAdapter.MapToType(phoneDto));
                }
            }
            return addressEntity;
        }
    }
}
