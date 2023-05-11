/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
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
                AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.PersonHierarchyName, Dtos.Base.PersonHierarchyName>();
          }
     }
}
