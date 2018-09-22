﻿// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a SectionGradeResponse domain entity to a SectionGradeResponse DTO.
    /// </summary>
    public class SectionGradeResponseAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse,  Ellucian.Colleague.Dtos.Student.SectionGradeResponse>
    {
        /// <summary>
        /// Initializes a new instance of the SectionGradeResponseAdapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionGradeResponseAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponseError, Ellucian.Colleague.Dtos.Student.SectionGradeResponseError>();
        }
    }
}
