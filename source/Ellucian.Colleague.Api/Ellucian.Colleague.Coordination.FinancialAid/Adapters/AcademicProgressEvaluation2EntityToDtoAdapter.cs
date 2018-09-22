// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// AcademicProgressEvaluation2 entity to DTO adapter
    /// </summary>
    public class AcademicProgressEvaluation2EntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation2, Dtos.FinancialAid.AcademicProgressEvaluation2>
    {
        public AcademicProgressEvaluation2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressEvaluationDetail, Dtos.FinancialAid.AcademicProgressEvaluationDetail>();
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressProgramDetail, Dtos.FinancialAid.AcademicProgressProgramDetail>();
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressAppeal, Dtos.FinancialAid.AcademicProgressAppeal>();
        }
    }
}
