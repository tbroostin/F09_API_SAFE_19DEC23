// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class GroupResultDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult, Ellucian.Colleague.Dtos.Student.Requirements.GroupResult>
    {
        public GroupResultDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Ellucian.Colleague.Dtos.Student.Requirements.GroupResult MapToType(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult Source)
        {
            Ellucian.Colleague.Dtos.Student.Requirements.GroupResult groupResult = new Ellucian.Colleague.Dtos.Student.Requirements.GroupResult();

            groupResult.GroupId = Source.Group.Id;
            groupResult.Gpa = Source.Gpa ?? 0;
            groupResult.ModificationMessages = Source.Group.ModificationMessages;

            List<Domain.Student.Entities.Requirements.AcadResult> allApplied = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> appliedCredits = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> appliedCourses = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> creditsIncludedInGPA = new List<Domain.Student.Entities.Requirements.AcadResult>();
            
            // here are all the courses and credits that eval applied to this group
            allApplied = Source.GetApplied().ToList();

            // if there is an acad cred id, it is a credit
            appliedCredits = allApplied.Where(ac => ac.GetAcadCredId() != null).ToList();

            // Get planned applied, making double-sure there are no academic credits in there (no reason there should be)
            appliedCourses = Source.GetPlannedApplied().Where(pc => pc.GetAcadCred() == null).ToList();

            // Additional academic credits that will display with the requirement because they are included in the gpa calculation for the group
            creditsIncludedInGPA = Source.GetCreditsToIncludeInGpa().Except(allApplied).ToList();

            // init
            groupResult.AppliedAcademicCreditIds = new List<string>();
            groupResult.AppliedPlannedCourseIds = new List<string>();
            groupResult.ForceAppliedAcademicCreditIds = new List<string>();
            groupResult.ForceDeniedAcademicCreditIds = new List<string>();
            groupResult.AcademicCreditIdsIncludedInGPA = new List<string>();
            
            groupResult.AppliedAcademicCreditIds.AddRange(appliedCredits.Select(ac => ac.GetAcadCredId()));
            groupResult.AppliedPlannedCourseIds.AddRange(appliedCourses.Select(ac => ac.GetCourse().Id));
            groupResult.ForceAppliedAcademicCreditIds.AddRange(Source.ForceAppliedAcademicCreditIds);
            groupResult.ForceDeniedAcademicCreditIds.AddRange(Source.ForceDeniedAcademicCreditIds);
            groupResult.AcademicCreditIdsIncludedInGPA.AddRange(creditsIncludedInGPA.Select(ac => ac.GetAcadCredId()));

            var completionStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.CompletionStatus, Dtos.Student.Requirements.CompletionStatus>(adapterRegistry, logger);
            var planningStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.PlanningStatus, Dtos.Student.Requirements.PlanningStatus>(adapterRegistry, logger);
            groupResult.CompletionStatus = completionStatusMapper.MapToType(Source.CompletionStatus);
            groupResult.PlanningStatus = planningStatusMapper.MapToType(Source.PlanningStatus);

            groupResult.AppliedPlannedCredits = new List<PlannedCredit>();
            foreach (var courseResult in Source.GetPlannedApplied())
            {
                groupResult.AppliedPlannedCredits.Add(new PlannedCredit()
                {
                    CourseId = courseResult.GetCourse().Id,
                    TermCode = courseResult.GetTermCode()
                });
            }

            groupResult.InstitutionalCredits = Source.GetAppliedInstCredits();
            groupResult.MinInstitutionalCreditsIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.GroupExplanation.MinInstCredits) ? true : false;
            groupResult.MinGpaIsNotMet = Source.Explanations.Contains(Domain.Student.Entities.Requirements.GroupExplanation.MinGpa) ? true : false;

            return groupResult;
        }
    }
}
