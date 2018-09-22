// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for Approver validation response entity to Dto mapping.
    /// </summary>
    public class NextAppoverValidationResponseEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.ApproverValidationResponse, Ellucian.Colleague.Dtos.ColleagueFinance.NextApproverValidationResponse>
    {
        public NextAppoverValidationResponseEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override NextApproverValidationResponse MapToType(Domain.ColleagueFinance.Entities.ApproverValidationResponse Source)
        {
            var nextApproverValidationResponeDto = new Dtos.ColleagueFinance.NextApproverValidationResponse();
            nextApproverValidationResponeDto.Id = Source.Id;
            nextApproverValidationResponeDto.NextApproverName = Source.ApproverName;
            nextApproverValidationResponeDto.IsValid = Source.IsValid;
            nextApproverValidationResponeDto.ErrorMessage = Source.ErrorMessage;

            return nextApproverValidationResponeDto;
        }
    }
}
