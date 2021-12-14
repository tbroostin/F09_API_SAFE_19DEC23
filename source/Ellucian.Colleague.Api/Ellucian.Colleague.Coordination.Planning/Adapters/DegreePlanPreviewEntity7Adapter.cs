// Copyright 2021 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanPreviewEntity7Adapter : BaseAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview7>
    {
        public DegreePlanPreviewEntity7Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Planning.DegreePlanPreview7 MapToType(Domain.Planning.Entities.DegreePlanPreview Source)
        {
            var degreePlanDto = new Dtos.Planning.DegreePlanPreview7();
            var degreePlanEntityAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            var degreePlanCourseResultTypeAdapter = adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreviewCourseEvaluationStatusType, Dtos.Planning.DegreePlanPreviewCourseEvaluationStatusType>();
            var degreePlanCourseResultAdapter = adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreviewCourseEvaluationResult, Dtos.Planning.DegreePlanPreviewCourseEvaluationResult>();
            degreePlanDto.Preview = degreePlanEntityAdapter.MapToType(Source.Preview);
            degreePlanDto.MergedDegreePlan = degreePlanEntityAdapter.MapToType(Source.MergedDegreePlan);
            degreePlanDto.DegreePlanPreviewCoursesEvaluation = new List<Dtos.Planning.DegreePlanPreviewCourseEvaluationResult>();
            if (Source.DegreePlanPreviewCoursesEvaluation != null)
            {
                foreach (Domain.Planning.Entities.DegreePlanPreviewCourseEvaluationResult courseResult in Source.DegreePlanPreviewCoursesEvaluation)
                {

                    Dtos.Planning.DegreePlanPreviewCourseEvaluationResult courseResultDto = new Dtos.Planning.DegreePlanPreviewCourseEvaluationResult();
                    courseResultDto = degreePlanCourseResultAdapter.MapToType(courseResult);
                    courseResultDto.EvaluationStatus = degreePlanCourseResultTypeAdapter.MapToType(courseResult.EvaluationStatus);
                    degreePlanDto.DegreePlanPreviewCoursesEvaluation.Add(courseResultDto);
                }
            }
            return degreePlanDto;
        }
    }
}
