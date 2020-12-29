// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanPreviewEntityAdapter : BaseAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview>
    {
        public DegreePlanPreviewEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }
        
        public override Dtos.Planning.DegreePlanPreview MapToType(Domain.Planning.Entities.DegreePlanPreview Source)
        {
            var degreePlanDto = new Dtos.Planning.DegreePlanPreview();
            var degreePlanEntityAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>();
            degreePlanDto.Preview = degreePlanEntityAdapter.MapToType(Source.Preview);
            degreePlanDto.MergedDegreePlan = degreePlanEntityAdapter.MapToType(Source.MergedDegreePlan);
            return degreePlanDto;
        }
    }
}



