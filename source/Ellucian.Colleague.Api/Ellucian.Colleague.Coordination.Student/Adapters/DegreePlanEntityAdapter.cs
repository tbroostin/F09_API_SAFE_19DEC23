// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// This class maps a degree plan entity to an outbound degree plan DTO
    /// </summary>

    public class DegreePlanEntityAdapter : BaseAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>
    {
        public DegreePlanEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Student.DegreePlans.DegreePlan MapToType(Domain.Student.Entities.DegreePlans.DegreePlan Source)
        {
            var degreePlanDto = new Dtos.Student.DegreePlans.DegreePlan();
            degreePlanDto.Id = Source.Id;
            degreePlanDto.PersonId = Source.PersonId;
            degreePlanDto.Version = Source.Version;
            degreePlanDto.ReviewRequested = Source.ReviewRequested;
            degreePlanDto.LastReviewedAdvisorId = Source.LastReviewedAdvisorId;
            degreePlanDto.LastReviewedDate = Source.LastReviewedDate;

            degreePlanDto.Terms = new List<Dtos.Student.DegreePlans.DegreePlanTerm>();
            degreePlanDto.NonTermPlannedCourses = new List<Dtos.Student.DegreePlans.PlannedCourse>();
            // Move all approvals to the degree plan dto
            degreePlanDto.Approvals = new List<Dtos.Student.DegreePlans.DegreePlanApproval>();
            var approvalDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>();
            foreach (var approval in Source.Approvals)
            {
                degreePlanDto.Approvals.Add(approvalDtoAdapter.MapToType(approval));
            }
            foreach (var term in Source.TermIds)
            {
                Dtos.Student.DegreePlans.DegreePlanTerm planTerm = new Dtos.Student.DegreePlans.DegreePlanTerm();
                planTerm.TermId = term;
                planTerm.PlannedCourses = new List<Dtos.Student.DegreePlans.PlannedCourse>();
                var plannedcourses = Source.GetPlannedCourses(term);
                if (plannedcourses != null)
                {
                    foreach (var plannedcourse in plannedcourses)
                    {
                        // Grading Type defaults to graded.
                        Dtos.Student.GradingType gradingType = Dtos.Student.GradingType.Graded;
                        switch (plannedcourse.GradingType)
                        {
                            case Domain.Student.Entities.GradingType.Graded:
                                gradingType = Dtos.Student.GradingType.Graded;
                                break;
                            case Domain.Student.Entities.GradingType.PassFail:
                                gradingType = Dtos.Student.GradingType.PassFail;
                                break;
                            case Domain.Student.Entities.GradingType.Audit:
                                gradingType = Dtos.Student.GradingType.Audit;
                                break;
                            default:
                                break;
                        }
                        // Convert the waitlist status
                        Dtos.Student.DegreePlans.WaitlistStatus waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted;
                        switch (plannedcourse.WaitlistedStatus)
                        {
                            case Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted:
                                waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted;
                                break;
                            case Domain.Student.Entities.DegreePlans.WaitlistStatus.Active:
                                waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.Active;
                                break;
                            case Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister:
                                waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.PermissionToRegister;
                                break;
                            default:
                                break;
                        }
                        var plannedCourseDto = new Dtos.Student.DegreePlans.PlannedCourse() { CourseId = plannedcourse.CourseId, SectionId = plannedcourse.SectionId, GradingType = gradingType, TermId = term, Credits = plannedcourse.Credits, SectionWaitlistStatus = waitStatus };
                        plannedCourseDto.Warnings = new List<Dtos.Student.DegreePlans.DegreePlanWarning>();
                        foreach (var warning in plannedcourse.Warnings)
                        {
                            Dtos.Student.DegreePlans.DegreePlanWarning warningDto = new Dtos.Student.DegreePlans.DegreePlanWarning();

                            // Do all the other types of warnings.
                            switch (warning.Type)
                            {
                                case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.InvalidPlannedCredits:
                                    warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.InvalidPlannedCredits;
                                    break;
                                case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.NegativePlannedCredits:
                                    warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.NegativePlannedCredits;
                                    break;
                                case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.TimeConflict:
                                    warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.TimeConflict;
                                    break;
                                case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite:
                                    if (warning.Requisite != null)
                                    {
                                        if (warning.Requisite.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous)
                                        {
                                            // this is an unmet prereq
                                            warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.PrerequisiteUnsatisfied;
                                            warningDto.RequirementCode = warning.Requisite.RequirementCode;
                                        }
                                        else
                                        {
                                            warningDto.Type = warning.Requisite.IsRequired == true ? Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteRequiredCourse : Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteOptionalCourse;
                                            // Pre-conversion data should have the coreq course filled in.
                                            if (!string.IsNullOrEmpty(warning.Requisite.CorequisiteCourseId))
                                            {
                                                warningDto.CourseId = warning.Requisite.CorequisiteCourseId;
                                                
                                            }
                                            else
                                            {
                                                // If there is no course will send the requirement, but this won't show up in Self-Service 2.2
                                                warningDto.RequirementCode = warning.Requisite.RequirementCode;
                                            }
                                        }
                                        
                                    }
                                    if (warning.SectionRequisite != null)
                                    {
                                            warningDto.SectionId = warning.SectionId;
                                            warningDto.Type = warning.SectionRequisite.IsRequired == true ? Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteRequiredSection : Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteOptionalSection;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            warningDto.SectionId = warning.SectionId;
                            plannedCourseDto.Warnings.Add(warningDto);
                        }
                        planTerm.PlannedCourses.Add(plannedCourseDto);
                    }
                }

                degreePlanDto.Terms.Add(planTerm);
            }
            foreach (var pc in Source.NonTermPlannedCourses)
            {
                // Create planned course dto for each nonterm planned course
                // Grading type defaults to graded.
                Dtos.Student.GradingType gradingType = Dtos.Student.GradingType.Graded;
                switch (pc.GradingType)
                {
                    case Domain.Student.Entities.GradingType.Graded:
                        gradingType = Dtos.Student.GradingType.Graded;
                        break;
                    case Domain.Student.Entities.GradingType.PassFail:
                        gradingType = Dtos.Student.GradingType.PassFail;
                        break;
                    case Domain.Student.Entities.GradingType.Audit:
                        gradingType = Dtos.Student.GradingType.Audit;
                        break;
                    default:
                        break;
                }
                // Convert the waitlist status
                Dtos.Student.DegreePlans.WaitlistStatus waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted;
                switch (pc.WaitlistedStatus)
                {
                    case Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted:
                        waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted;
                        break;
                    case Domain.Student.Entities.DegreePlans.WaitlistStatus.Active:
                        waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.Active;
                        break;
                    case Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister:
                        waitStatus = Dtos.Student.DegreePlans.WaitlistStatus.PermissionToRegister;
                        break;
                    default:
                        break;
                }
                var plannedCourseDto = new Dtos.Student.DegreePlans.PlannedCourse() { CourseId = pc.CourseId, SectionId = pc.SectionId, GradingType = gradingType, Credits = pc.Credits, SectionWaitlistStatus = waitStatus };
                // Add empty warning list
                plannedCourseDto.Warnings = new List<Dtos.Student.DegreePlans.DegreePlanWarning>();
                // For each warning associated with this nonterm course
                foreach (var warning in pc.Warnings)
                {
                    Dtos.Student.DegreePlans.DegreePlanWarning warningDto = new Dtos.Student.DegreePlans.DegreePlanWarning();

                    // Do all the other types of warnings.
                    switch (warning.Type)
                    {
                        case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.InvalidPlannedCredits:
                            warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.InvalidPlannedCredits;
                            break;
                        case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.NegativePlannedCredits:
                            warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.NegativePlannedCredits;
                            break;
                        case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.TimeConflict:
                            warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.TimeConflict;
                            break;
                        case Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite:
                            if (warning.Requisite != null)
                            {
                                if (warning.Requisite.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous)
                                {
                                    // this is an unmet prereq
                                    warningDto.Type = Dtos.Student.DegreePlans.DegreePlanWarningType.PrerequisiteUnsatisfied;
                                    warningDto.RequirementCode = warning.Requisite.RequirementCode;
                                }
                                // Now for anything with a coreq course - convert it into appropriate coreq.
                                if (!string.IsNullOrEmpty(warning.Requisite.CorequisiteCourseId))
                                {
                                    warningDto.CourseId = warning.Requisite.CorequisiteCourseId;
                                    warningDto.Type = warning.Requisite.IsRequired == true ? Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteRequiredCourse : Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteOptionalCourse;
                                }
                            }
                            if (warning.SectionRequisite != null)
                            {
                                warningDto.SectionId = warning.SectionId;
                                warningDto.Type = warning.SectionRequisite.IsRequired == true ? Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteRequiredSection : Dtos.Student.DegreePlans.DegreePlanWarningType.CorequisiteOptionalSection;
                            }
                            break;
                        default:
                            break;
                    }
                    warningDto.SectionId = warning.SectionId;
                    plannedCourseDto.Warnings.Add(warningDto);
                }
                // Add planned course dto to the list of nonterm planned courses
                degreePlanDto.NonTermPlannedCourses.Add(plannedCourseDto);
            }
            // Move notes into dto
            degreePlanDto.Notes = new List<Dtos.Student.DegreePlans.DegreePlanNote>();
            foreach (var note in Source.Notes)
            {
                degreePlanDto.Notes.Add(new Dtos.Student.DegreePlans.DegreePlanNote()
                {
                    Id = note.Id,
                    PersonId = note.PersonId,
                    Date = note.Date.GetValueOrDefault().DateTime,
                    Text = note.Text,
                    PersonType = (Source.PersonId == note.PersonId ? Dtos.Student.PersonType.Student : Dtos.Student.PersonType.Advisor)
                });
            }
            return degreePlanDto;
        }

    }
}
