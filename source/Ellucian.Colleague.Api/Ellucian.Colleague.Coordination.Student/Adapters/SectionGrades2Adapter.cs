// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a SectionGrades2 DTO to a SectionGrades domain entity.
    /// </summary>
    public class SectionGrades2Adapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades2, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>
    {
        /// <summary>
        /// Initializes a new instance of the SectionGradesAdapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionGrades2Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Dtos.Student.StudentGrade2, Ellucian.Colleague.Domain.Student.Entities.StudentGrade>();
        }
    }
}
