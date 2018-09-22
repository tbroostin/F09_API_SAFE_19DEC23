// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps StudentPetition entity to StudentPetition DTO
    /// </summary>
    public class StudentPetitionEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StudentPetitionEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public StudentPetitionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
    }
}
