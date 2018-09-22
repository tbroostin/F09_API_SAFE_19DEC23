/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{

     /// <summary>
     /// Custom Adapter for PersonBase Entity to HumanResourceDemographics Dto
     /// </summary>
     public class PersonBaseEntityToHumanResourceDemographicsDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.PersonBase, HumanResourceDemographics>
     {

          public PersonBaseEntityToHumanResourceDemographicsDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
               : base(adapterRegistry, logger)
          {
          }
     }
}
