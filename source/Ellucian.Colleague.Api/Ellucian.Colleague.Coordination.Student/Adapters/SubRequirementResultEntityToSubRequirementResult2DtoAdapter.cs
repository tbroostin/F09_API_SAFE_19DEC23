// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SubRequirementResultEntityToSubRequirementResult2DtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.SubrequirementResult, Ellucian.Colleague.Dtos.Student.Requirements.SubrequirementResult2>
    {
        public SubRequirementResultEntityToSubRequirementResult2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Ellucian.Colleague.Dtos.Student.Requirements.SubrequirementResult2 MapToType(Ellucian.Colleague.Domain.Student.Entities.Requirements.SubrequirementResult Source)
        {
            var subrequirementResult = new Ellucian.Colleague.Dtos.Student.Requirements.SubrequirementResult2();

            subrequirementResult.SubrequirementId = Source.SubRequirement.Id;
            subrequirementResult.Gpa = Source.Gpa;
            subrequirementResult.ModificationMessages = Source.SubRequirement.ModificationMessages; 

            var completionStatusMapper = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.CompletionStatus, Ellucian.Colleague.Dtos.Student.Requirements.CompletionStatus>(adapterRegistry, logger);
            var planningStatusMapper = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.PlanningStatus, Ellucian.Colleague.Dtos.Student.Requirements.PlanningStatus>(adapterRegistry, logger);
            subrequirementResult.CompletionStatus = completionStatusMapper.MapToType(Source.CompletionStatus);
            subrequirementResult.PlanningStatus = planningStatusMapper.MapToType(Source.PlanningStatus);

            subrequirementResult.GroupResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.GroupResult2>();

            var groupResultDtoAdapter = new GroupResultEntityToGroupResult2DtoAdapter(adapterRegistry, logger);

            if (Source.GroupResults.Count > 0)
            {
                subrequirementResult.GroupResults.AddRange(Source.GroupResults.Select(gr => groupResultDtoAdapter.MapToType(gr)).ToList());
            }

            subrequirementResult.MinGpaIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.SubrequirementExplanation.MinGpa) ? true : false;

            subrequirementResult.InstitutionalCredits = Source.GetAppliedInstitutionalCredits();
            subrequirementResult.MinInstitutionalCreditsIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.SubrequirementExplanation.MinInstitutionalCredits) ? true : false;
            
            return subrequirementResult;
        }
    }
}
