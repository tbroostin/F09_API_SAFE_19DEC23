﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class GroupResultEntityToGroupResult3DtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult, Ellucian.Colleague.Dtos.Student.Requirements.GroupResult3>
    {
        public GroupResultEntityToGroupResult3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Ellucian.Colleague.Dtos.Student.Requirements.GroupResult3 MapToType(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult Source)
        {
            Ellucian.Colleague.Dtos.Student.Requirements.GroupResult3 groupResult = new Ellucian.Colleague.Dtos.Student.Requirements.GroupResult3();

            groupResult.GroupId = Source.Group.Id;
            groupResult.Gpa = Source.Gpa;
            groupResult.ModificationMessages = Source.Group.ModificationMessages;

            List<Domain.Student.Entities.Requirements.AcadResult> allApplied = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> appliedCredits = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> appliedCourses = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> creditsIncludedInGPA = new List<Domain.Student.Entities.Requirements.AcadResult>();
            List<Domain.Student.Entities.Requirements.AcadResult> allRelated = null;

            // here are all the courses and credits that eval applied to this group
            allApplied = Source.GetApplied().ToList();

            // if there is an acad cred id, it is a credit
            appliedCredits = allApplied.Where(ac => ac.GetAcadCredId() != null).ToList();

            // Get planned applied, making double-sure there are no academic credits in there (no reason there should be)
            appliedCourses = Source.GetPlannedApplied().Where(pc => pc.GetAcadCred() == null).ToList();

            // Additional academic credits that will display with the requirement because they are included in the gpa calculation for the group
            creditsIncludedInGPA = Source.GetCreditsToIncludeInGpa().Except(allApplied).ToList();
            //Get the related credits for the group. This is only going to get credits and not planned courses.
            if (Source.GetRelated() != null)
            {
                allRelated = Source.GetRelated().Where(ac => ac.GetAcadCredId() != null).ToList();
            }

            var acadResultMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.AcadResultExplanation, Dtos.Student.Requirements.AcadResultExplanation>(adapterRegistry, logger);
            // init
            groupResult.AppliedAcademicCredits = new List<CreditResult>();
            groupResult.AppliedPlannedCourses = new List<CourseResult>();
            groupResult.ForceAppliedAcademicCreditIds = new List<string>();
            groupResult.ForceDeniedAcademicCreditIds = new List<string>();
            groupResult.AcademicCreditIdsIncludedInGPA = new List<string>();

            groupResult.AppliedAcademicCredits.AddRange(appliedCredits.Select(ac => new CreditResult() { AcademicCreditId = ac.GetAcadCredId(), Explanation = acadResultMapper.MapToType(ac.Explanation) }));
            groupResult.AppliedPlannedCourses.AddRange(appliedCourses.Select(ac => new CourseResult() { CourseId = ac.GetCourse().Id, Explanation = acadResultMapper.MapToType(ac.Explanation) }));
            groupResult.ForceAppliedAcademicCreditIds.AddRange(Source.ForceAppliedAcademicCreditIds);
            groupResult.ForceDeniedAcademicCreditIds.AddRange(Source.ForceDeniedAcademicCreditIds);
            groupResult.AcademicCreditIdsIncludedInGPA.AddRange(creditsIncludedInGPA.Select(ac => ac.GetAcadCredId()));
            if (allRelated != null)
            {
                groupResult.RelatedAcademicCredits = new List<CreditResult>();
                groupResult.RelatedAcademicCredits.AddRange(allRelated.Select(ac => new CreditResult() { AcademicCreditId = ac.GetAcadCredId(), Explanation = (ac.Result == Domain.Student.Entities.Requirements.Result.MinGrade ? AcadResultExplanation.MinGrade : acadResultMapper.MapToType(ac.Explanation)) }));
            }


            var completionStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.CompletionStatus, Dtos.Student.Requirements.CompletionStatus>(adapterRegistry, logger);
            var planningStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.PlanningStatus, Dtos.Student.Requirements.PlanningStatus>(adapterRegistry, logger);
            groupResult.CompletionStatus = completionStatusMapper.MapToType(Source.CompletionStatus);
            groupResult.PlanningStatus = planningStatusMapper.MapToType(Source.PlanningStatus);

            var GroupResultMinGroupStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.GroupResultMinGroupStatus, Dtos.Student.Requirements.GroupResultMinGroupStatus>(adapterRegistry, logger);
            groupResult.MinGroupStatus = GroupResultMinGroupStatusMapper.MapToType(Source.MinGroupStatus);


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
            groupResult.Explanations = new List<GroupExplanation>();
            foreach (var explanation in Source.Explanations)
            {
                switch (explanation)
                {
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.Satisfied:
                        groupResult.Explanations.Add(GroupExplanation.Satisfied);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.PlannedSatisfied:
                        groupResult.Explanations.Add(GroupExplanation.PlannedSatisfied);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.Courses:
                        groupResult.Explanations.Add(GroupExplanation.Courses);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCourses:
                        groupResult.Explanations.Add(GroupExplanation.MinCourses);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCredits:
                        groupResult.Explanations.Add(GroupExplanation.MinCredits);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinDepartments:
                        groupResult.Explanations.Add(GroupExplanation.MinDepartments);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinSubjects:
                        groupResult.Explanations.Add(GroupExplanation.MinSubjects);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinInstCredits:
                        groupResult.Explanations.Add(GroupExplanation.MinInstCredits);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCoursesPerSubject:
                        groupResult.Explanations.Add(GroupExplanation.MinCoursesPerSubject);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCreditsPerSubject:
                        groupResult.Explanations.Add(GroupExplanation.MinCreditsPerSubject);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCoursesPerDepartment:
                        groupResult.Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinCreditsPerDepartment:
                        groupResult.Explanations.Add(GroupExplanation.MinCreditsPerDepartment);
                        break;
                    case Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinGpa:
                        groupResult.Explanations.Add(GroupExplanation.MinGpa);
                        break;
                    default:
                        break;
                }
            }
            return groupResult;
        }
    }
}
