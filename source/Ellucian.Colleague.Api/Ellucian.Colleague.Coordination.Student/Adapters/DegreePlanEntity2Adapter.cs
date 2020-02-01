// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// This class maps a degree plan entity to an outbound degree plan DTO
    /// </summary>

    public class DegreePlanEntity2Adapter : BaseAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>
    {
        public DegreePlanEntity2Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Student.DegreePlans.DegreePlan2 MapToType(Domain.Student.Entities.DegreePlans.DegreePlan Source)
        {
            var degreePlanDto = new Dtos.Student.DegreePlans.DegreePlan2();
            degreePlanDto.Id = Source.Id;
            degreePlanDto.PersonId = Source.PersonId;
            degreePlanDto.Version = Source.Version;
            degreePlanDto.ReviewRequested = Source.ReviewRequested;
            degreePlanDto.LastReviewedAdvisorId = Source.LastReviewedAdvisorId;
            degreePlanDto.LastReviewedDate = Source.LastReviewedDate;

            degreePlanDto.Terms = new List<Dtos.Student.DegreePlans.DegreePlanTerm2>();
            degreePlanDto.NonTermPlannedCourses = new List<Dtos.Student.DegreePlans.PlannedCourse2>();
            // Move all approvals to the degree plan dto
            degreePlanDto.Approvals = new List<Dtos.Student.DegreePlans.DegreePlanApproval>();
            var approvalDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>();
            foreach (var approval in Source.Approvals)
            {
                degreePlanDto.Approvals.Add(approvalDtoAdapter.MapToType(approval));
            }
            foreach (var term in Source.TermIds)
            {
                Dtos.Student.DegreePlans.DegreePlanTerm2 planTerm = new Dtos.Student.DegreePlans.DegreePlanTerm2();
                planTerm.TermId = term;
                planTerm.PlannedCourses = new List<Dtos.Student.DegreePlans.PlannedCourse2>();
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

                        DateTime? plannedCourseAddedOn = plannedcourse.AddedOn.HasValue ? plannedcourse.AddedOn.Value.DateTime : (DateTime?)null;
                        var plannedCourseDto = new Dtos.Student.DegreePlans.PlannedCourse2()
                        {
                            CourseId = plannedcourse.CourseId,
                            SectionId = plannedcourse.SectionId,
                            GradingType = gradingType,
                            TermId = term,
                            Credits = plannedcourse.Credits,
                            SectionWaitlistStatus = waitStatus,
                            AddedBy = plannedcourse.AddedBy,
                            AddedOn = plannedCourseAddedOn
                        };
                        plannedCourseDto.Warnings = new List<Dtos.Student.DegreePlans.PlannedCourseWarning>();
                        foreach (var warning in plannedcourse.Warnings)
                        {
                            var plannedCourseWarningDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning>();
                            var warningDto = plannedCourseWarningDtoAdapter.MapToType(warning);
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

                DateTime? pcAddedOn = pc.AddedOn.HasValue ? pc.AddedOn.Value.DateTime : (DateTime?)null;
                var plannedCourseDto = new Dtos.Student.DegreePlans.PlannedCourse2() { CourseId = pc.CourseId, SectionId = pc.SectionId, GradingType = gradingType, Credits = pc.Credits, SectionWaitlistStatus = waitStatus, AddedBy = pc.AddedBy, AddedOn = pcAddedOn };
                // Add empty warning list
                plannedCourseDto.Warnings = new List<Dtos.Student.DegreePlans.PlannedCourseWarning>();
                // For each warning associated with this nonterm course
                foreach (var warning in pc.Warnings)
                {
                    var degreePlanWarningDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning>();
                    var warningDto = degreePlanWarningDtoAdapter.MapToType(warning);
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
