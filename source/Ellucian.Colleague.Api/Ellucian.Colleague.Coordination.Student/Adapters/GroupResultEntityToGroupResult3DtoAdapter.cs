// Copyright 2019 Ellucian Company L.P. and its affiliates.
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
            //Get the related credits as well as planned courses for the group. 
            if (Source.GetRelated() != null)
            {
                allRelated = Source.GetRelated().ToList();
            }

            var acadResultMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.AcadResultExplanation, Dtos.Student.Requirements.AcadResultExplanation>(adapterRegistry, logger);
            var acadResultReplacedStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.ReplacedStatus, Dtos.Student.ReplacedStatus>(adapterRegistry, logger);
            var acadResultReplacementStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.ReplacementStatus, Dtos.Student.ReplacementStatus>(adapterRegistry, logger);

            // init
            groupResult.AppliedAcademicCredits = new List<CreditResult>();
            groupResult.AppliedPlannedCourses = new List<CourseResult>();
            groupResult.ForceAppliedAcademicCreditIds = new List<string>();
            groupResult.ForceDeniedAcademicCreditIds = new List<string>();
            groupResult.AcademicCreditIdsIncludedInGPA = new List<string>();
            //Applied Credits
            foreach(Domain.Student.Entities.Requirements.CreditResult acadResult in appliedCredits)
            {
                Domain.Student.Entities.AcademicCredit academicCredit = acadResult.GetAcadCred();
                groupResult.AppliedAcademicCredits.Add(new CreditResult()
                {
                    AcademicCreditId = acadResult.GetAcadCredId(),
                    Explanation = acadResultMapper.MapToType(acadResult.Explanation),
                    ReplacedStatus = acadResultReplacedStatusMapper.MapToType(academicCredit.ReplacedStatus),
                    ReplacementStatus = acadResultReplacementStatusMapper.MapToType(academicCredit.ReplacementStatus)
                });
            }
            //Applied planned course result
            foreach (Domain.Student.Entities.Requirements.CourseResult acadResult in appliedCourses)
            {
                Domain.Student.Entities.Requirements.PlannedCredit course = acadResult.PlannedCourse;
                groupResult.AppliedPlannedCourses.Add(new CourseResult()
                {
                    CourseId = acadResult.GetCourse().Id,
                    Explanation = acadResultMapper.MapToType(acadResult.Explanation),
                    ReplacedStatus = acadResultReplacedStatusMapper.MapToType(course.ReplacedStatus),
                    ReplacementStatus = acadResultReplacementStatusMapper.MapToType(course.ReplacementStatus)
                });
            }
            //applied planned credits

            groupResult.AppliedPlannedCredits = new List<PlannedCredit>();
            foreach (Domain.Student.Entities.Requirements.CourseResult courseResult in appliedCourses)
            {
                Domain.Student.Entities.Requirements.PlannedCredit course = courseResult.PlannedCourse;
                groupResult.AppliedPlannedCredits.Add(new PlannedCredit()
                {
                    CourseId = courseResult.GetCourse().Id,
                    TermCode = courseResult.GetTermCode(),
                    ReplacedStatus = acadResultReplacedStatusMapper.MapToType(course.ReplacedStatus),
                    ReplacementStatus = acadResultReplacementStatusMapper.MapToType(course.ReplacementStatus)
                });
            }

            groupResult.ForceAppliedAcademicCreditIds.AddRange(Source.ForceAppliedAcademicCreditIds);
            groupResult.ForceDeniedAcademicCreditIds.AddRange(Source.ForceDeniedAcademicCreditIds);
            groupResult.AcademicCreditIdsIncludedInGPA.AddRange(creditsIncludedInGPA.Select(ac => ac.GetAcadCredId()));
            if (allRelated != null)
            {
                groupResult.RelatedAcademicCredits = new List<CreditResult>();
                groupResult.RelatedPlannedCredits = new List<PlannedCredit>();

                foreach (Domain.Student.Entities.Requirements.AcadResult acadResult in allRelated)
                {
                    //if it is academic credit
                    if (acadResult.GetAcadCred()!=null)
                    {
                        Domain.Student.Entities.AcademicCredit academicCredit = acadResult.GetAcadCred();
                        groupResult.RelatedAcademicCredits.Add(new CreditResult()
                        {
                            AcademicCreditId = acadResult.GetAcadCredId(),

                            Explanation = (acadResult.Result == Domain.Student.Entities.Requirements.Result.MinGrade ? AcadResultExplanation.MinGrade : acadResultMapper.MapToType(acadResult.Explanation)),
                            ReplacedStatus = acadResultReplacedStatusMapper.MapToType(academicCredit.ReplacedStatus),
                            ReplacementStatus = acadResultReplacementStatusMapper.MapToType(academicCredit.ReplacementStatus)
                        });
                    }
                    else //if it is planned credit
                    {
                        Domain.Student.Entities.Requirements.PlannedCredit course = (acadResult as Domain.Student.Entities.Requirements.CourseResult).PlannedCourse ;
                        groupResult.RelatedPlannedCredits.Add(new PlannedCredit()
                        {
                            CourseId = acadResult.GetCourse().Id,
                            TermCode = acadResult.GetTermCode(),
                            ReplacedStatus = acadResultReplacedStatusMapper.MapToType(course.ReplacedStatus),
                            ReplacementStatus = acadResultReplacementStatusMapper.MapToType(course.ReplacementStatus)
                        });
                    }
                }
            }


            var completionStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.CompletionStatus, Dtos.Student.Requirements.CompletionStatus>(adapterRegistry, logger);
            var planningStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.PlanningStatus, Dtos.Student.Requirements.PlanningStatus>(adapterRegistry, logger);
            groupResult.CompletionStatus = completionStatusMapper.MapToType(Source.CompletionStatus);
            groupResult.PlanningStatus = planningStatusMapper.MapToType(Source.PlanningStatus);

            var GroupResultMinGroupStatusMapper = new AutoMapperAdapter<Domain.Student.Entities.Requirements.GroupResultMinGroupStatus, Dtos.Student.Requirements.GroupResultMinGroupStatus>(adapterRegistry, logger);
            groupResult.MinGroupStatus = GroupResultMinGroupStatusMapper.MapToType(Source.MinGroupStatus);
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
