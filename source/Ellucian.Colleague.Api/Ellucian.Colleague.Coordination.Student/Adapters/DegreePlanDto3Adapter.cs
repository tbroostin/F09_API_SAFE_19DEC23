// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// This class maps an inbound degree plan DTO to a degree plan entity
    /// </summary>
    public class DegreePlanDto3Adapter : BaseAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>
    {
        public DegreePlanDto3Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        // Since AutoMapper cannot be used here, inherit from BaseAdapter and override the MapToType method for custom mapping
        public override Domain.Student.Entities.DegreePlans.DegreePlan MapToType(Dtos.Student.DegreePlans.DegreePlan3 Source)
        {
            // You could add validation code here before passing the properties to the Degree Plan entity constructor
            var degreePlanEntity = new Domain.Student.Entities.DegreePlans.DegreePlan(Source.Id, Source.PersonId, Source.Version, Source.ReviewRequested);
            degreePlanEntity.LastReviewedAdvisorId = Source.LastReviewedAdvisorId;
            degreePlanEntity.LastReviewedDate = Source.LastReviewedDate;
            if (Source.Terms != null)
            {
                foreach (var tc in Source.Terms)
                {
                    if (!String.IsNullOrEmpty(tc.TermId))
                    {
                        degreePlanEntity.AddTerm(tc.TermId);
                        if (tc.PlannedCourses != null && tc.PlannedCourses.Count() > 0)
                        {
                            foreach (var plannedcourse in tc.PlannedCourses)
                            {
                                if (!string.IsNullOrEmpty(plannedcourse.CourseId))
                                {
                                    Domain.Student.Entities.GradingType gradingType = Domain.Student.Entities.GradingType.Graded;
                                    switch (plannedcourse.GradingType)
                                    {
                                        case Dtos.Student.GradingType.PassFail:
                                            gradingType = Domain.Student.Entities.GradingType.PassFail;
                                            break;
                                        case Dtos.Student.GradingType.Audit:
                                            gradingType = Domain.Student.Entities.GradingType.Audit;
                                            break;
                                        default:
                                            break;
                                    }
                                    Domain.Student.Entities.DegreePlans.WaitlistStatus waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                                    switch (plannedcourse.SectionWaitlistStatus)
                                    {
                                        case Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted:
                                            waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                                            break;
                                        case Dtos.Student.DegreePlans.WaitlistStatus.Active:
                                            waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.Active;
                                            break;
                                        case Dtos.Student.DegreePlans.WaitlistStatus.PermissionToRegister:
                                            waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister;
                                            break;
                                        default:
                                            break;
                                    }
                                    // planned course warnings are not mapped back into the domain.
                                    degreePlanEntity.AddCourse(new Domain.Student.Entities.DegreePlans.PlannedCourse(plannedcourse.CourseId, plannedcourse.SectionId, gradingType, waitStatus, plannedcourse.AddedBy, plannedcourse.AddedOn) { Credits = plannedcourse.Credits }, tc.TermId);
                                }
                            }
                        }
                    }
                }
            }
            if (Source.NonTermPlannedCourses != null)
            {
                foreach (var ntpc in Source.NonTermPlannedCourses)
                {
                    if (!string.IsNullOrEmpty(ntpc.CourseId))
                    {
                        Domain.Student.Entities.GradingType gradingType = Domain.Student.Entities.GradingType.Graded;
                        switch (ntpc.GradingType)
                        {
                            case Dtos.Student.GradingType.PassFail:
                                gradingType = Domain.Student.Entities.GradingType.PassFail;
                                break;
                            case Dtos.Student.GradingType.Audit:
                                gradingType = Domain.Student.Entities.GradingType.Audit;
                                break;
                            default:
                                break;
                        }
                        Domain.Student.Entities.DegreePlans.WaitlistStatus waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                        switch (ntpc.SectionWaitlistStatus)
                        {
                            case Dtos.Student.DegreePlans.WaitlistStatus.NotWaitlisted:
                                waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                                break;
                            case Dtos.Student.DegreePlans.WaitlistStatus.Active:
                                waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.Active;
                                break;
                            case Dtos.Student.DegreePlans.WaitlistStatus.PermissionToRegister:
                                waitStatus = Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister;
                                break;
                            default:
                                break;
                        }

                        degreePlanEntity.AddCourse(new Domain.Student.Entities.DegreePlans.PlannedCourse(ntpc.CourseId, ntpc.SectionId, gradingType, waitStatus, ntpc.AddedBy, ntpc.AddedOn) { Credits = ntpc.Credits}, null);
                    }
                }
            }
            // Map approvals back to entity
            var approvals = new List<Domain.Student.Entities.DegreePlans.DegreePlanApproval>();
            if (Source.Approvals != null && Source.Approvals.Count() > 0)
            {
                // Add each approval for this term to the degree plan entity term approvals
                foreach (var approval in Source.Approvals)
                {
                    if (approval.Date == null || approval.Date == default(DateTimeOffset))
                    {
                        approval.Date = DateTimeOffset.Now;
                    }
                    if (!string.IsNullOrEmpty(approval.PersonId) && !string.IsNullOrEmpty(approval.CourseId) && !string.IsNullOrEmpty(approval.TermCode))
                    {
                        try
                        {
                            approvals.Add(new Domain.Student.Entities.DegreePlans.DegreePlanApproval(approval.PersonId, ConvertStatus(approval.Status), approval.Date, approval.CourseId, approval.TermCode));
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Approval skipped.");
                        }
                    }
                }
            }
            degreePlanEntity.Approvals = approvals;

            // Move notes from the degree plan into the dto
            var notes = new List<Domain.Student.Entities.DegreePlans.DegreePlanNote>();
            if (Source.Notes != null && Source.Notes.Count() > 0)
            {
                foreach (var note in Source.Notes)
                {
                    if (note.Id == 0)
                    {
                        notes.Add(new Domain.Student.Entities.DegreePlans.DegreePlanNote(note.Text));
                    }
                    else
                    {
                        notes.Add(new Domain.Student.Entities.DegreePlans.DegreePlanNote(note.Id, note.PersonId, note.Date, note.Text));
                    }
                }
            }
            degreePlanEntity.Notes = notes;
            return degreePlanEntity;
        }

        private Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus ConvertStatus(Dtos.Student.DegreePlans.DegreePlanApprovalStatus dtoStatus)
        {
            var status = Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Approved;
            switch (dtoStatus)
            {
                case Dtos.Student.DegreePlans.DegreePlanApprovalStatus.Approved:
                    status = Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Approved;
                    break;
                case Dtos.Student.DegreePlans.DegreePlanApprovalStatus.Denied:
                    status = Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Denied;
                    break;
            }
            return status;
        }
    }
}
