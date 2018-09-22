// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    public class AcademicProgressEvaluationEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation, Dtos.FinancialAid.AcademicProgressEvaluation>
    {
        public AcademicProgressEvaluationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressEvaluationDetail, Dtos.FinancialAid.AcademicProgressEvaluationDetail>();
            AddMappingDependency<Domain.Student.Entities.Requirements.ProgramRequirements, Dtos.Student.Requirements.ProgramRequirements>();
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressAppeal, Dtos.FinancialAid.AcademicProgressAppeal>();
        }
    }
}
