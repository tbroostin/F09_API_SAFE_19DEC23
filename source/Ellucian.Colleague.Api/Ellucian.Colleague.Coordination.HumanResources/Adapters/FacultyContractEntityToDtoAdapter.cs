/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class FacultyContractEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.FacultyContractPosition, Dtos.Base.FacultyContractPosition>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for Positions
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public FacultyContractEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.FacultyContractPosition, Dtos.Base.FacultyContractPosition>();
            AddMappingDependency<Domain.HumanResources.Entities.FacultyContractAssignment, Dtos.Base.FacultyContractAssignment>();
        }
    }
}
