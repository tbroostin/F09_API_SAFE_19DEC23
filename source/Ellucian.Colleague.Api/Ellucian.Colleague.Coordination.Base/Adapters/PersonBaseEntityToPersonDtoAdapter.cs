/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using Person = Ellucian.Colleague.Dtos.Base.Person;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping a PersonBase Entity to Person Dto
    /// </summary>
    public class PersonBaseEntityToPersonDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.PersonBase, Dtos.Base.Person>
    {
        public PersonBaseEntityToPersonDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.EthnicOrigin, Dtos.Base.EthnicOrigin>();
            AddMappingDependency<Domain.Base.Entities.MaritalState, Dtos.Base.MaritalState>();
        }
    }
}
