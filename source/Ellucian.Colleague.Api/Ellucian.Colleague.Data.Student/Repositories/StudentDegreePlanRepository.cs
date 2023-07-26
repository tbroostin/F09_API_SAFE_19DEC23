// Copyright 2021-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentDegreePlanRepository : BaseColleagueRepository, IStudentDegreePlanRepository
    {
        private ApplValcodes waitlistStatuses;
        private readonly string colleagueTimeZone;

        public StudentDegreePlanRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Caching degree plans only for verification purposes
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        public async Task<DegreePlan> AddAsync(DegreePlan newPlan)
        {
            var updateReq = new Transactions.AddDegreePlanRequest();

            updateReq.StudentId = newPlan.PersonId;
            updateReq.TermIds = newPlan.TermIds.ToList();

            // Add the degree plan
            AddDegreePlanResponse updateResponse = await transactionInvoker.ExecuteAsync<AddDegreePlanRequest, AddDegreePlanResponse>(updateReq);

            if (String.IsNullOrEmpty(updateResponse.AErrorMessage))
            {
                // Get the updated plan from Colleague using returned ID
                int newPlanId = Convert.ToInt32(updateResponse.DegreePlanId);
                var updatedDegreePlan = await GetAsync(newPlanId);
                return updatedDegreePlan;
            }
            else
            {
                if (!String.IsNullOrEmpty(updateResponse.ExistingDegreePlanId))
                {
                    throw new ExistingDegreePlanException("Student already has a plan. Cannot create a new one.", Convert.ToInt32(updateResponse.ExistingDegreePlanId));
                }
                else if (updateResponse.AErrorMessage.Contains("lock"))
                {
                    throw new RecordLockException("Student record locked on add. Unable to add new plan.", "STUDENTS", newPlan.PersonId);
                }
                else
                {
                    // for all other errors.
                    throw new ArgumentException("Unable to create a new degree plan for this student.");
                }
            }
        }

        public async Task<DegreePlan> UpdateAsync(DegreePlan newPlan)
        {
            var updateReq = new Transactions.UpdateDegreePlanRequest();
            updateReq.DegreePlanId = newPlan.Id.ToString();
            updateReq.Version = newPlan.Version.ToString();
            updateReq.ReviewRequested = newPlan.ReviewRequested ? "Y" : "";
            updateReq.LastReviewedDate = newPlan.LastReviewedDate;
            updateReq.LastReviewedBy = newPlan.LastReviewedAdvisorId;
            updateReq.ReviewRequestedDate = newPlan.ReviewRequestedDate;
            updateReq.ReviewRequestedTime = newPlan.ReviewRequestedTime;
            updateReq.AArchiveNotificationDate = newPlan.ArchiveNotificationDate;
            updateReq.IsAdvisorNotes = "Y";
            updateReq.AAdvisingOfficeEmailId = newPlan.AdvisingOfficeEmailId;
            updateReq.CourseApprovals = new List<Transactions.CourseApprovals>();
            foreach (var item in newPlan.Approvals)
            {
                var approval = new Transactions.CourseApprovals();
                var itemApprovalDate = item.Date.ToLocalDateTime(colleagueTimeZone);
                approval.ApprovalDate = itemApprovalDate;
                approval.ApprovalPersonId = item.PersonId;
                approval.ApprovalStatus = item.Status.ToString();
                approval.ApprovalTermId = item.TermCode;
                approval.ApprovalTime = itemApprovalDate;
                approval.ApprovalCourseId = item.CourseId;
                updateReq.CourseApprovals.Add(approval);
            }
            // This list contains the term Ids for real terms and does not include a null term if there are nonterm sections.
            updateReq.TermList = newPlan.TermIds.ToList();
            updateReq.TermCourses = new List<Transactions.TermCourses>();
            foreach (var term in newPlan.TermIds)
            {
                var plannedCourses = newPlan.GetPlannedCourses(term);
                if (plannedCourses != null)
                {
                    foreach (var plannedCourse in plannedCourses)
                    {
                        var tc = new Transactions.TermCourses();
                        tc.TermIds = term;
                        tc.CourseIds = plannedCourse.CourseId;
                        tc.SectionIds = plannedCourse.SectionId;
                        tc.AlCoursePlaceholders = plannedCourse.CoursePlaceholderId;
                        tc.Credits = plannedCourse.Credits;
                        tc.AddedBy = plannedCourse.AddedBy;
                        DateTime? addedOnDateTime = null;
                        if (plannedCourse.AddedOn.HasValue)
                        {
                            addedOnDateTime = plannedCourse.AddedOn.Value.ToLocalDateTime(colleagueTimeZone);
                        }
                        tc.AddedOnDate = addedOnDateTime;
                        tc.AddedOnTime = addedOnDateTime;
                        //Grading type is blank unless there is a section and the associated grading type is either Pass/Fail or Audit.
                        string gradingType = null;
                        if (!string.IsNullOrEmpty(plannedCourse.SectionId) && plannedCourse.GradingType == GradingType.Audit) { gradingType = "A"; }
                        if (!string.IsNullOrEmpty(plannedCourse.SectionId) && plannedCourse.GradingType == GradingType.PassFail) { gradingType = "P"; }
                        tc.GradingType = gradingType;
                        if (plannedCourse.IsProtected == true)
                        {
                            tc.Protected = "Y";
                        }
                        else if (plannedCourse.IsProtected == null)
                        {
                            tc.Protected = "";
                        }
                        else
                        {
                            tc.Protected = "N";
                        }
                        updateReq.TermCourses.Add(tc);
                    }
                }
            }
            if (newPlan.NonTermPlannedCourses.Count() > 0)
            {
                foreach (var plannedCourse in newPlan.NonTermPlannedCourses)
                {
                    // Add these into the TermCourses list.
                    var tc = new Transactions.TermCourses();
                    tc.CourseIds = plannedCourse.CourseId;
                    tc.SectionIds = plannedCourse.SectionId;
                    tc.Credits = plannedCourse.Credits;
                    tc.AddedBy = plannedCourse.AddedBy;
                    DateTime? addedOnDateTime = null;
                    if (plannedCourse.AddedOn.HasValue)
                    {
                        addedOnDateTime = plannedCourse.AddedOn.ToLocalDateTime(colleagueTimeZone);
                    }
                    tc.AddedOnDate = addedOnDateTime;
                    tc.AddedOnTime = addedOnDateTime;
                    if (plannedCourse.IsProtected == true)
                    {
                        tc.Protected = "Y";
                    }
                    else if (plannedCourse.IsProtected == null)
                    {
                        tc.Protected = "";
                    }
                    else
                    {
                        tc.Protected = "N";
                    }
                    updateReq.TermCourses.Add(tc);
                }
            }

            // Add new public notes to update request.
            foreach (var note in newPlan.Notes)
            {
                if (note.Id == 0 && !(string.IsNullOrEmpty(note.Text)))
                {
                    updateReq.CommentText.Add(note.Text);
                    if (note.PersonType == Dtos.Student.PersonType.Student)
                    {
                        updateReq.IsAdvisorNotes = "N";
                    }
                    else
                    {
                        //when the notes are from advisor, 
                        //the advisor id needs to be set so that the email notifications are sent to the student appropriately
                        updateReq.AdvisorId = newPlan.CurrentUserId;
                    }
                }
            }

            // Add new restricted notes to update request.
            foreach (var restrictedNote in newPlan.RestrictedNotes)
            {
                if (restrictedNote.Id == 0 && !(string.IsNullOrEmpty(restrictedNote.Text)))
                {
                    updateReq.RestrictedCommentText.Add(restrictedNote.Text);
                }
            }

            try
            {
                // Update the plan
                var updateResponse = await transactionInvoker.ExecuteAsync<Transactions.UpdateDegreePlanRequest, Transactions.UpdateDegreePlanResponse>(updateReq);

                // CTX returns null object
                if (updateResponse == null)
                {
                    throw new ApplicationException(String.Format("The request to update the degree plan {0} returned a null response.", updateReq.DegreePlanId));
                }

                // Log any warnings
                if (updateResponse.AlWarningMessages != null && updateResponse.AlWarningMessages.Any())
                {
                    var degreePlanUpdateWarnings = new StringBuilder(String.Format("One or more warnings were generated when updating degree plan {0}:" + Environment.NewLine, updateResponse.DegreePlanId));
                    foreach (var message in updateResponse.AlWarningMessages)
                    {
                        degreePlanUpdateWarnings.AppendLine(message);
                    }
                    logger.Info(degreePlanUpdateWarnings.ToString());
                }

                if (String.IsNullOrEmpty(updateResponse.AErrorMessage))
                {
                    // Degree Plan update is successful. Remove the record from DEGREE_PLAN_RVW_ASGN table to remove the relation when Advising by Office is true                
                    if (!string.IsNullOrEmpty(newPlan.LastReviewedAdvisorId))
                    {
                        var updateRevReq = new Transactions.MaintDegreePlanRvwAsgnRequest();
                        updateRevReq.ADegreePlanId = newPlan.Id.ToString();
                        updateRevReq.AReviewerId = newPlan.LastReviewedAdvisorId;
                        updateRevReq.AAction = "D";
                        Transactions.MaintDegreePlanRvwAsgnResponse deleteResponse = await transactionInvoker.ExecuteAsync<Transactions.MaintDegreePlanRvwAsgnRequest, Transactions.MaintDegreePlanRvwAsgnResponse>(updateRevReq);
                        if (!string.IsNullOrEmpty(deleteResponse.AErrorMessage))
                        {
                            // Dont have to through the exception. Log it
                            logger.Error(deleteResponse.AErrorMessage);
                        }
                    }

                    // Get the updated plan from Colleague using returned ID
                    int newPlanId = Convert.ToInt32(updateResponse.DegreePlanId);
                    var updatedDegreePlan = await GetAsync(newPlanId);
                    return updatedDegreePlan;
                }

                // Whatever the error was, log it.
                logger.Error(updateResponse.AErrorMessage);
                // Throw a different exception depending on the error
                if (updateResponse.AErrorMessage == "Degree Plan update had version incompatibility. Update not performed.")
                {
                    throw new InvalidOperationException("Version number mismatch.");
                }
                else
                {
                    // If record wasn't found - meaning the id was bad - produce a different exception 
                    if (updateResponse.AErrorMessage.Contains("No Degree Plan found"))
                    {
                        throw new KeyNotFoundException("Plan not found. Unable to update.");
                    }
                    else
                    {
                        // Record was locked, or other invalid data condition such as a bad term or course or section provided, etc.
                        throw new ArgumentException("Unresolved errors on update for Plan Id " + newPlan.Id);
                    }
                }
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = string.Format("Colleague transaction error occurred while updating degree plan {0}", updateReq.DegreePlanId);
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to update the degree plan {0}.", updateReq.DegreePlanId);
                logger.Error(ex, exceptionMsg);
                throw;
            }
        }

        /// <summary>
        /// Get the specified degree plan from the database
        /// </summary>
        /// <param name="planId">ID of the degree plan</param>
        /// <returns>Degree Plan Entity</returns>
        public async Task<DegreePlan> GetAsync(int planId)
        {
            if (planId <= 0)
            {
                throw new ArgumentException("Plan Id must be greater than 0");
            }

            DataContracts.DegreePlan plan = await DataReader.ReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", planId.ToString());
            if (plan == null)
            {
                throw new ArgumentException("No Degree Plan found with Plan Id " + planId);
            }
            else
            {
                // Gather all other necessary data to construct the degree plan.
                var degreePlanTermsQuery = "WITH DPT.DEGREE.PLAN EQ '" + plan.Recordkey + "'";
                var degreePlanTermData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(degreePlanTermsQuery);
                var waitlistQuery = "WITH WAIT.STUDENT EQ '" + plan.DpStudentId + "'";
                var waitlistData = await DataReader.BulkReadRecordAsync<DataContracts.WaitList>(waitlistQuery);
                var degreePlanCommentQuery = "WITH DPC.DEGREE.PLAN EQ '" + plan.Recordkey + "'";
                var degreePlanCommentData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanComment>(degreePlanCommentQuery, false);
                var degreePlanRstrCmtQuery = "WITH DPRC.DEGREE.PLAN EQ '" + plan.Recordkey + "'";
                var degreePlanRstrCmtData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(degreePlanRstrCmtQuery, false);
                var degreePlan = (await BuildDegreePlansAsync(new Collection<DataContracts.DegreePlan>() { plan }, degreePlanTermData, waitlistData, degreePlanCommentData, degreePlanRstrCmtData)).First();
                return degreePlan;
            }
        }

        public async Task<IEnumerable<DegreePlan>> GetAsync(IEnumerable<string> studentIds)
        {
            var degreePlans = new List<DegreePlan>();
            if (studentIds != null && studentIds.Count() > 0)
            {
                // Use Select statements to obtain record keys using criteria; 
                // Bulk reads with criteria break when the query string is too long.
                string searchString = "DP.STUDENT.ID EQ '?'";
                var planIds = await DataReader.SelectAsync("DEGREE_PLAN", searchString, studentIds.ToArray());
                var planRepoData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlan>("DEGREE_PLAN", planIds);
                if (planRepoData != null && planRepoData.Count() > 0)
                {
                    // select all degree plan term items needed in bulk
                    var degreePlanTermQuery = "DPT.DEGREE.PLAN EQ '?'";
                    var dptIds = await DataReader.SelectAsync("DEGREE_PLAN_TERMS", degreePlanTermQuery, planIds);
                    var degreePlanTermData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanTerms>(dptIds);
                    // also select waitlist records for thes students
                    var degreePlanWaitlistQuery = "WAIT.STUDENT EQ '?'";
                    var waitlistIds = await DataReader.SelectAsync("WAIT.LIST", degreePlanWaitlistQuery, studentIds.ToArray());
                    var waitlistData = await DataReader.BulkReadRecordAsync<DataContracts.WaitList>(waitlistIds);
                    // select all the degree plan comment items for the selected plans
                    var degreePlanCommentQuery = "DPC.DEGREE.PLAN EQ '?'";
                    var dpCommentIds = await DataReader.SelectAsync("DEGREE_PLAN_COMMENT", degreePlanCommentQuery, planIds);
                    var degreePlanCommentData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanComment>(dpCommentIds);
                    var degreePlanRstrCmtQuery = "DPRC.DEGREE.PLAN EQ '?'";
                    var dpRstrCommentIds = await DataReader.SelectAsync("DEGREE_PLAN_RSTR_CMT", degreePlanRstrCmtQuery, planIds);
                    var degreePlanRstrCmtData = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanRstrCmt>(dpCommentIds);
                    degreePlans = (await BuildDegreePlansAsync(planRepoData, degreePlanTermData, waitlistData, degreePlanCommentData, degreePlanRstrCmtData)).ToList();
                }
            }
            return degreePlans;
        }

        private async Task<IEnumerable<DegreePlan>> BuildDegreePlansAsync(ICollection<DataContracts.DegreePlan> plans, ICollection<DataContracts.DegreePlanTerms> degreePlanTermData, ICollection<DataContracts.WaitList> waitlists, ICollection<DataContracts.DegreePlanComment> degreePlanCommentData, ICollection<DataContracts.DegreePlanRstrCmt> degreePlanRstrCmtData)
        {
            var degreePlans = new List<DegreePlan>();
            foreach (var plan in plans)
            {
                bool planReviewRequested = false;
                if (plan.DpReviewRequested == "Y")
                {
                    planReviewRequested = true;
                }
                var degreePlan = new DegreePlan(Int32.Parse(plan.Recordkey), plan.DpStudentId, Int32.Parse(plan.DpVersionNumber), planReviewRequested);
                degreePlan.LastReviewedAdvisorId = plan.DpLastReviewedBy;
                degreePlan.LastReviewedDate = plan.DpLastReviewedDate;
                degreePlan.ReviewRequestedDate = plan.DpReviewRequestedDate;
                degreePlan.ReviewRequestedTime = plan.DpReviewRequestedTime;
                degreePlan.ArchiveNotificationDate = plan.DpArchiveNotificationDate;

                // Add approval association
                // As of Colleague SU 61699.11, degree plan approvals  returned by Colleague will only carry the current status of each course/term combination. 
                // It was determined that the approval audit history information was not needed in the domain.
                var approvals = new List<DegreePlanApproval>();
                if (plan.DpApprovalsEntityAssociation != null && plan.DpApprovalsEntityAssociation.Count() > 0)
                {
                    var validDpApprovals = plan.DpApprovalsEntityAssociation.Where(a => (!string.IsNullOrEmpty(a.DpApprovalTermIdAssocMember) && (!string.IsNullOrEmpty(a.DpApprovalCourseIdAssocMember)))).ToList();
                    foreach (var item in validDpApprovals)
                    {
                        // Only create the approval in the domain if it has a status of Approved or Denied. In version 1 there could be a status of 
                        // requested but that status is not longer meaningful and should just act like a not approved item. 
                        if (item.DpApprovalStatusAssocMember == "Approved" || item.DpApprovalStatusAssocMember == "Denied")
                        {
                            DateTimeOffset approvalDate = item.DpApprovalTimeAssocMember.ToPointInTimeDateTimeOffset(
                                item.DpApprovalDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                            // Convert item status to DegreePlanApprovalStatus enum value
                            DegreePlanApprovalStatus status = DegreePlanApprovalStatus.Approved;
                            if (item.DpApprovalStatusAssocMember == "Denied")
                            {
                                status = DegreePlanApprovalStatus.Denied;
                            }
                            approvals.Add(new DegreePlanApproval(item.DpApprovalPersonIdAssocMember, status, approvalDate, item.DpApprovalCourseIdAssocMember, item.DpApprovalTermIdAssocMember));
                        }
                    }
                }
                degreePlan.Approvals = approvals;

                // Determine if this plan has any terms.
                IEnumerable<DataContracts.DegreePlanTerms> planTerms = degreePlanTermData.Where(pt => pt.DptDegreePlan == plan.Recordkey).ToList();
                if (planTerms != null && planTerms.Count() > 0)
                {
                    foreach (var t in planTerms.OrderBy(pt => pt.Recordkey))
                    {
                        if (!String.IsNullOrEmpty(t.DptTerm))
                        {
                            if (!(degreePlan.TermIds.Contains(t.DptTerm)))
                            {
                                degreePlan.AddTerm(t.DptTerm);
                            }
                            else
                            {
                                // Log the plan and then log the duplicate planning term but keep going.
                                LogDataError("DegreePlan", degreePlan.Id.ToString(), plan);
                                LogDataError("Duplicate DegreePlanTerm", t.Recordkey, t);
                            }
                        }
                        if (t.PlannedCoursesEntityAssociation != null)
                        {
                            foreach (var plannedcourse in t.PlannedCoursesEntityAssociation)
                            {
                                if (!string.IsNullOrEmpty(plannedcourse.DptCoursesAssocMember))
                                {
                                    string course = string.IsNullOrWhiteSpace(plannedcourse.DptCoursesAssocMember) ? null : plannedcourse.DptCoursesAssocMember;
                                    string section = string.IsNullOrWhiteSpace(plannedcourse.DptSectionsAssocMember) ? null : plannedcourse.DptSectionsAssocMember;
                                    decimal? credits = plannedcourse.DptCreditsAssocMember;
                                    string coursePlaceholder = null; // course placeholder will always be empty when course has a vaule
                                    string addedBy = plannedcourse.DptAddedByAssocMember;
                                    DateTimeOffset? addedOn = null;
                                    if (plannedcourse.DptAddedOnDateAssocMember != null && plannedcourse.DptAddedOnTimeAssocMember != null)
                                    {
                                        addedOn = plannedcourse.DptAddedOnTimeAssocMember.ToPointInTimeDateTimeOffset(
                                            plannedcourse.DptAddedOnDateAssocMember, colleagueTimeZone);
                                    }

                                    GradingType gradingType = GradingType.Graded;
                                    if (plannedcourse.DptGradingTypeAssocMember == "P")
                                    {
                                        gradingType = GradingType.PassFail;
                                    }
                                    if (plannedcourse.DptGradingTypeAssocMember == "A")
                                    {
                                        gradingType = GradingType.Audit;
                                    }
                                    // Determine waitlist status of any associated section here.
                                    var status = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                                    var studentSectionWaitlists = new List<DataContracts.WaitList>();
                                    if (!string.IsNullOrEmpty(section))
                                    {
                                        studentSectionWaitlists = waitlists.Where(wl => wl.WaitStudent == degreePlan.PersonId && wl.WaitCourseSection == section).ToList();
                                    }
                                    else
                                    {
                                        // If planned course has no section, see if there are any sections waitlisted for this student/term/course
                                        var studentCourseWaitlists = waitlists.Where(wl => wl.WaitStatus != null && wl.WaitStudent == degreePlan.PersonId && wl.WaitCourse == course && wl.WaitTerm == t.DptTerm).ToList();
                                        foreach (var item in studentCourseWaitlists)
                                        {
                                            // Make sure the section in this waitlist item is not claimed by another planned course, either in the source data or in the
                                            // planned courses already built.
                                            if (!t.PlannedCoursesEntityAssociation.Select(pc => pc.DptSectionsAssocMember).Contains(item.WaitCourseSection)
                                                && !degreePlan.SectionsInPlan.Contains(item.WaitCourseSection))
                                            {
                                                studentSectionWaitlists.Add(item);
                                            }
                                        }
                                    }
                                    if (studentSectionWaitlists != null)
                                    {
                                        foreach (var waitlistItem in studentSectionWaitlists)
                                        {
                                            // Make sure this section does not conflict with the section already associated with this planned course.
                                            // This handles the case where there are multiple sections waitlisted for the same course (if that's even possible)
                                            if (string.IsNullOrEmpty(section) || waitlistItem.WaitCourseSection == section)
                                            {
                                                // while the student should only have 1 waitlist record per section, will have active statuses take priority in case of bad data.
                                                if (!String.IsNullOrEmpty(waitlistItem.WaitStatus))
                                                {
                                                    if ((await GetWaitlistStatusActionCodeAsync(waitlistItem.WaitStatus)) == "4")
                                                    {
                                                        status = Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister;
                                                        // Associate this section to this planned course
                                                        section = waitlistItem.WaitCourseSection;
                                                    }
                                                    if ((await GetWaitlistStatusActionCodeAsync(waitlistItem.WaitStatus)) == "1")
                                                    {
                                                        if (status != Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister)
                                                        {
                                                            status = Domain.Student.Entities.DegreePlans.WaitlistStatus.Active;
                                                            // Associate this section to this planned course
                                                            section = waitlistItem.WaitCourseSection;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Preserving the Y, N, null values that may come from the database.
                                    bool? isProtected = null;
                                    if (!string.IsNullOrEmpty(plannedcourse.DptProtectedAssocMember) && plannedcourse.DptProtectedAssocMember.ToUpper() == "Y")
                                    {
                                        isProtected = true;
                                    }
                                    else if (!string.IsNullOrEmpty(plannedcourse.DptProtectedAssocMember) && plannedcourse.DptProtectedAssocMember.ToUpper() == "N")
                                    {
                                        isProtected = false;
                                    }
                                    degreePlan.AddCourse(new PlannedCourse(course: course, section: section, gradingType: gradingType, status: status,
                                        addedBy: addedBy, addedOn: addedOn, coursePlaceholder: coursePlaceholder)
                                    { Credits = credits, IsProtected = isProtected }, t.DptTerm);
                                }
                                else if (!string.IsNullOrEmpty(plannedcourse.DptCoursePlaceholdersAssocMember))
                                {
                                    string coursePlaceholder = string.IsNullOrWhiteSpace(plannedcourse.DptCoursePlaceholdersAssocMember) ? null : plannedcourse.DptCoursePlaceholdersAssocMember;
                                    string addedBy = plannedcourse.DptAddedByAssocMember;
                                    DateTimeOffset? addedOn = null;
                                    if (plannedcourse.DptAddedOnDateAssocMember != null && plannedcourse.DptAddedOnTimeAssocMember != null)
                                    {
                                        addedOn = plannedcourse.DptAddedOnTimeAssocMember.ToPointInTimeDateTimeOffset(
                                            plannedcourse.DptAddedOnDateAssocMember, colleagueTimeZone);
                                    }
                                    degreePlan.AddCourse(new PlannedCourse(course: null, section: null, gradingType: GradingType.Graded,
                                        status: Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: addedBy, addedOn: addedOn,
                                        coursePlaceholder: coursePlaceholder)
                                    { IsProtected = false }, t.DptTerm);
                                }
                            }
                        }

                        // Gather all waitlists for this student in this term in case they are on a waitlist for a section that is not already on the plan.
                        // (This can happen if they are waitlisted via another Colleague process RGN, webadvisor, etc. )
                        // Excluding non-term waitlist items for now based on possibilities of bad data in the WAIT.LIST file.
                        if (t.DptTerm != null)
                        {
                            var studentTermWaitlists = waitlists.Where(swl => swl.WaitStudent == degreePlan.PersonId).ToList();
                            IEnumerable<string> sectionsOnPlan = degreePlan.SectionsInPlan;
                            foreach (var wl in studentTermWaitlists)
                            {
                                if (!String.IsNullOrEmpty(wl.WaitStatus) && !string.IsNullOrEmpty(wl.WaitCourseSection) && !string.IsNullOrEmpty(wl.WaitCourse))
                                {
                                    if (!sectionsOnPlan.Contains(wl.WaitCourseSection))
                                    {
                                        if ((await GetWaitlistStatusActionCodeAsync(wl.WaitStatus)) == "4")
                                        {
                                            degreePlan.AddCourse(new PlannedCourse(course: wl.WaitCourse, section: wl.WaitCourseSection,
                                                gradingType: GradingType.Graded, status: Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister,
                                                coursePlaceholder: null)
                                            { Credits = wl.WaitCred }, wl.WaitTerm);
                                        }
                                        if ((await GetWaitlistStatusActionCodeAsync(wl.WaitStatus)) == "1")
                                        {
                                            degreePlan.AddCourse(new PlannedCourse(course: wl.WaitCourse, section: wl.WaitCourseSection,
                                                gradingType: GradingType.Graded, status: Domain.Student.Entities.DegreePlans.WaitlistStatus.Active,
                                                coursePlaceholder: null)
                                            { Credits = wl.WaitCred }, wl.WaitTerm);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Now retrieve and add any comments that exist for this degree plan.
                if (degreePlanCommentData != null)
                {
                    IEnumerable<DataContracts.DegreePlanComment> comments = degreePlanCommentData.Where(dc => dc.DpcDegreePlan == plan.Recordkey);
                    var dpComments = new List<DegreePlanNote>();
                    if (comments != null && comments.Any())
                    {
                        foreach (var comment in comments)
                        {
                            DateTimeOffset? date = comment.DegreePlanCommentAddtime.ToPointInTimeDateTimeOffset(
                                comment.DegreePlanCommentAdddate, colleagueTimeZone);
                            // Convert value marks to new line characters because we want to maintain any formatting (line-to-line) that the user may
                            // have entered.
                            var commentText = !string.IsNullOrEmpty(comment.DpcText) ? comment.DpcText.Replace(DmiString._VM, '\n') : comment.DpcText;
                            dpComments.Add(new DegreePlanNote(
                                int.Parse(comment.Recordkey), comment.DegreePlanCommentAddopr, date, commentText));
                        }
                    }
                    // Add the list of comments found to the degreeplan object
                    degreePlan.Notes = dpComments;
                }
                // Now retrieve and add any rstricted comments that exist for this degree plan.
                if (degreePlanRstrCmtData != null)
                {
                    IEnumerable<DataContracts.DegreePlanRstrCmt> restrictedComments = degreePlanRstrCmtData.Where(dc => dc.DprcDegreePlan == plan.Recordkey);
                    var restrictedNotes = new List<DegreePlanNote>();
                    if (restrictedComments != null && restrictedComments.Any())
                    {
                        foreach (var note in restrictedComments)
                        {
                            DateTimeOffset? adddate = note.DegreePlanRstrCmtAddtime.ToPointInTimeDateTimeOffset(
                                note.DegreePlanRstrCmtAdddate, colleagueTimeZone);
                            // Convert value marks to new line characters because we want to maintain any formatting (line-to-line) that the user may
                            // have entered.
                            var rcommentText = !string.IsNullOrEmpty(note.DprcText) ? note.DprcText.Replace(DmiString._VM, '\n') : note.DprcText;
                            restrictedNotes.Add(new DegreePlanNote(
                                int.Parse(note.Recordkey), note.DegreePlanRstrCmtAddopr, adddate, rcommentText));
                        }
                    }
                    // Add the list of restricted comments found to the degreeplan object
                    degreePlan.RestrictedNotes = restrictedNotes;
                }
                // Add this degree plan to the list of degree plans to return.
                if (degreePlan != null)
                {
                    degreePlans.Add(degreePlan);
                }
            }
            return degreePlans.AsEnumerable();
        }

        private async Task<ApplValcodes> GetWaitlistStatusesAsync()
        {
            if (waitlistStatuses != null)
            {
                return waitlistStatuses;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            waitlistStatuses = await GetOrAddToCacheAsync<ApplValcodes>("WaitlistStatuses",
                async () =>
                {
                    ApplValcodes waitlistStatusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES");
                    if (waitlistStatusesTable == null)
                    {
                        // log this but don't throw exception because not all clients use wait lists.
                        var errorMessage = "Unable to access WAIT.LIST.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        waitlistStatusesTable = new ApplValcodes() { ValsEntityAssociation = new List<ApplValcodesVals>() };
                    }
                    return waitlistStatusesTable;
                }, Level1CacheTimeoutValue);
            return waitlistStatuses;
        }

        private async Task<string> GetWaitlistStatusActionCodeAsync(string waitlistStatusCode)
        {
            if (!String.IsNullOrEmpty(waitlistStatusCode))
            {
                var codeAssoc = (await GetWaitlistStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == waitlistStatusCode).FirstOrDefault();
                if (codeAssoc != null)
                {
                    return codeAssoc.ValActionCode1AssocMember;
                }
            }
            return null;
        }

    }
}
