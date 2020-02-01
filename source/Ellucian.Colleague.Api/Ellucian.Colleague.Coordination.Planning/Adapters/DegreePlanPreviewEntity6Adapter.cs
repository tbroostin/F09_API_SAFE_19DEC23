// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanPreviewEntity6Adapter : BaseAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview6>
    {
        public DegreePlanPreviewEntity6Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Planning.DegreePlanPreview6 MapToType(Domain.Planning.Entities.DegreePlanPreview Source)
        {
            var degreePlanDto = new Dtos.Planning.DegreePlanPreview6();
            var degreePlanEntityAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            degreePlanDto.Preview = degreePlanEntityAdapter.MapToType(Source.Preview);
            degreePlanDto.MergedDegreePlan = degreePlanEntityAdapter.MapToType(Source.MergedDegreePlan);
            return degreePlanDto;
        }
    }
}
