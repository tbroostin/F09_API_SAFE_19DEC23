// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for converting a SectionPreliminaryAnonymousGrading entity to a SectionPreliminaryAnonymousGrading DTO
    /// </summary>
    public class SectionPreliminaryAnonymousGradingAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading.SectionPreliminaryAnonymousGrading, Ellucian.Colleague.Dtos.Student.AnonymousGrading.SectionPreliminaryAnonymousGrading>
    {
        public SectionPreliminaryAnonymousGradingAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade, Ellucian.Colleague.Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading.AnonymousGradeError, Ellucian.Colleague.Dtos.Student.AnonymousGrading.AnonymousGradeError>();
        }

    }
}
