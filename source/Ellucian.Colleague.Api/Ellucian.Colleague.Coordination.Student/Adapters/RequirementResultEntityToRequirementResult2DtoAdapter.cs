// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class RequirementResultEntityToRequirementResult2DtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.RequirementResult, Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2>
    {
        public RequirementResultEntityToRequirementResult2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2 MapToType(Ellucian.Colleague.Domain.Student.Entities.Requirements.RequirementResult Source)
        {
            Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2 requirementResult = new Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2();

            requirementResult.RequirementId = Source.Requirement.Id;
            requirementResult.Gpa = Source.Gpa;
            requirementResult.ModificationMessages = Source.Requirement.ModificationMessages;
            var completionStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.CompletionStatus, Dtos.Student.Requirements.CompletionStatus>(adapterRegistry, logger);
            var planningStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.PlanningStatus, Dtos.Student.Requirements.PlanningStatus>(adapterRegistry, logger);
            requirementResult.CompletionStatus = completionStatusMapper.MapToType(Source.CompletionStatus);
            requirementResult.PlanningStatus = planningStatusMapper.MapToType(Source.PlanningStatus);

            requirementResult.SubrequirementResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.SubrequirementResult2>();
            var SubrequirementResult2DtoAdapter = new SubRequirementResultEntityToSubRequirementResult2DtoAdapter(adapterRegistry, logger);

            if (Source.SubRequirementResults.Count > 0)
            {
                requirementResult.SubrequirementResults.AddRange(Source.SubRequirementResults.Select(sr => SubrequirementResult2DtoAdapter.MapToType(sr)).ToList());
            }

            requirementResult.MinGpaIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.RequirementExplanation.MinGpa) ? true : false;

            requirementResult.InstitutionalCredits = Source.GetAppliedInstitutionalCredits();
            requirementResult.MinInstitutionalCreditsIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.RequirementExplanation.MinInstitutionalCredits) ? true : false;

            return requirementResult;
        }
    }
}
