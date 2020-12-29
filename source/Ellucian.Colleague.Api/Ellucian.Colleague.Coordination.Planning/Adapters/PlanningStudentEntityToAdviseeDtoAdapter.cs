// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    /// <summary>
    /// Maps a PlanningStudent Entity to a PlanningStudent Dto.
    /// </summary>
    public class PlanningStudentEntityToAdviseeDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>
    {
        /// <summary>
        /// Initializes a new instance of the PlanningStudentEntityToDtoAdapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PlanningStudentEntityToAdviseeDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Base.Entities.PersonHierarchyName, Dtos.Base.PersonHierarchyName>();
            AddMappingDependency<Domain.Student.Entities.CompletedAdvisement, Dtos.Planning.CompletedAdvisement>();
        }
    }
}
