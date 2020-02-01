// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanPreviewEntity4Adapter : BaseAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>
    {
        public DegreePlanPreviewEntity4Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Planning.DegreePlanPreview4 MapToType(Domain.Planning.Entities.DegreePlanPreview Source)
        {
            Dtos.Planning.DegreePlanPreview4 degreePlanDto = new Dtos.Planning.DegreePlanPreview4();
            var degreePlanEntityAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            degreePlanDto.Preview = degreePlanEntityAdapter.MapToType(Source.Preview);
            degreePlanDto.MergedDegreePlan = degreePlanEntityAdapter.MapToType(Source.MergedDegreePlan);
            return degreePlanDto;
        }
    }
}
