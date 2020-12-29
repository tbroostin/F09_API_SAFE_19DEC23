// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanReviewRequestedEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanReviewRequest, Ellucian.Colleague.Dtos.Planning.DegreePlanReviewRequest>
    {
        public DegreePlanReviewRequestedEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
        public override Dtos.Planning.DegreePlanReviewRequest MapToType(Domain.Planning.Entities.DegreePlanReviewRequest Source)
        {
            var dpReviewRequestedDto = new Dtos.Planning.DegreePlanReviewRequest();
            dpReviewRequestedDto.Id = Source.Id;
            dpReviewRequestedDto.PersonId = Source.PersonId;
            dpReviewRequestedDto.ReviewRequestedDate = Source.ReviewRequestedDate;
            dpReviewRequestedDto.AssignedReviewer = Source.AssignedReviewer;
            return dpReviewRequestedDto;
        }
    }
}
