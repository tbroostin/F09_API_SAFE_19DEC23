// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps StudentProfileConfiguration entity to StudentProfileConfiguration DTO
    /// </summary>
    public class StudentConfigurationEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration, Ellucian.Colleague.Dtos.Student.StudentProfileConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StudentConfigurationEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public StudentConfigurationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.StudentProfileConfiguration, Dtos.Student.StudentProfileConfiguration>();
            AddMappingDependency<Domain.Student.Entities.StudentProfilePersonConfiguration, Dtos.Student.StudentProfilePersonConfiguration>();
        }
    }
}