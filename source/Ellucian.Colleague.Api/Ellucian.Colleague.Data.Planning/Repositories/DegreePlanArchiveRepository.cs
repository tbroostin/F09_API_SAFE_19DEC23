// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using slf4net;
using Ellucian.Colleague.Data.Planning.Transactions;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class DegreePlanArchiveRepository : BaseColleagueRepository, IDegreePlanArchiveRepository
    {
        private readonly string colleagueTimeZone;
        public DegreePlanArchiveRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Create a new degree plan archive item 
        /// </summary>
        /// <param name="degreePlanArchive">The Degree Plan Archive to create</param>
        /// <returns>Degree Plan Archive that was created - includes generated ID</returns>
        public async Task<DegreePlanArchive> AddAsync(DegreePlanArchive degreePlanArchive)
        {
            // Get the degree plan from the database and fill out the request for the transaction.
            if (degreePlanArchive == null)
            {
                throw new ArgumentNullException("Degree Plan Archive must be provided");
            }

            CreateDegreePlanArchiveRequest request = new CreateDegreePlanArchiveRequest();
            request.ADegreePlanId = degreePlanArchive.DegreePlanId.ToString();
            request.ADpStudentId = degreePlanArchive.StudentId;
            request.ADpLastReviewedBy = degreePlanArchive.ReviewedBy;
            request.ADpLastReviewedDate = degreePlanArchive.ReviewedDate.ToLocalDateTime(colleagueTimeZone);
            request.ADpVersionNumber = degreePlanArchive.Version.ToString();
            request.AlCourses = new List<AlCourses>();
            foreach (var archiveCourse in degreePlanArchive.ArchivedCourses)
            {
                var ac = new AlCourses()
                {
                    AlCourseId = archiveCourse.CourseId,
                    AlCredits = archiveCourse.Credits,
                    AlName = archiveCourse.Name,
                    AlTermId = archiveCourse.TermCode,
                    AlSectionId = archiveCourse.SectionId,
                    AlTitle = archiveCourse.Title,
                    AlCrsApprovalStatus = archiveCourse.ApprovalStatus,
                    AlCrsStatusDate = archiveCourse.ApprovalDate.ToLocalDateTime(colleagueTimeZone),
                    AlCrsStatusTime = archiveCourse.ApprovalDate.ToLocalDateTime(colleagueTimeZone),
                    AlCrsStatusBy = archiveCourse.ApprovedBy,
                    AlCrsAddedBy = archiveCourse.AddedBy,
                    AlCrsAddedDate = archiveCourse.AddedOn.ToLocalDateTime(colleagueTimeZone),
                    AlCrsAddedTime = archiveCourse.AddedOn.ToLocalDateTime(colleagueTimeZone),
                    AlCrsStcStatus = archiveCourse.RegistrationStatus,
                    AlCrsCeus = archiveCourse.ContinuingEducationUnits,
                    AlCrsIsPlanned = (archiveCourse.IsPlanned ? "Y" : "N"),
                    AlCrsHasWithdrawGrd = (archiveCourse.HasWithdrawGrade ? "Y" : "N")
                };
                request.AlCourses.Add(ac);
            }

            request.AlAcadPrograms = new List<AlAcadPrograms>();
            foreach (var sp in degreePlanArchive.StudentPrograms)
            {
                var prog = new AlAcadPrograms();
                prog.AlProgram = sp.ProgramCode;
                prog.AlCatalog = sp.CatalogCode;
                request.AlAcadPrograms.Add(prog);
            }

            request.AlComments = new List<AlComments>();
            foreach (var note in degreePlanArchive.Notes)
            {
                var comment = new AlComments()
                {
                    AlCommentAddedBy = note.PersonId,
                    AlCommentAddedDate = note.Date.ToLocalDateTime(colleagueTimeZone),
                    AlCommentAddedTime = note.Date.ToLocalDateTime(colleagueTimeZone),
                    AlCommentText = note.Text
                };
                request.AlComments.Add(comment);
            }

            Transactions.CreateDegreePlanArchiveResponse updateResponse = await transactionInvoker.ExecuteAsync<Transactions.CreateDegreePlanArchiveRequest, Transactions.CreateDegreePlanArchiveResponse>(request);

            if (string.IsNullOrEmpty(updateResponse.AErrorMessage) && !string.IsNullOrEmpty(updateResponse.ADegreePlanArchiveId))
            {
                int newArchiveId = Convert.ToInt32(updateResponse.ADegreePlanArchiveId);
                var degreePlanArchived = await GetDegreePlanArchiveAsync(newArchiveId); 
                return degreePlanArchived;
            }
            else
            {
                // Got an error back and or no archive Id. Something went wrong so throw an error.
                throw new ArgumentException("Unresolved errors trying to archive Degree Plan Id " + degreePlanArchive.Id);
            }
        }

        /// <summary>
        /// Get degree plan archive entities for a specific degree plan 
        /// </summary>
        /// <param name="degreePlanId">Id of plan for which archives are requested</param>
        /// <returns>All degree plan archive entities for this plan.</returns>
        public async Task<IEnumerable<DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId)
        {
            if (degreePlanId <= 0)
            {
                throw new ArgumentException("Plan Id must be greater than 0");
            }
            // Select the Degree Plan Archive items 
            string selectCriteria = "WITH DPARCHV.DEGREE.PLAN.ID = '" + degreePlanId + "'";

            var planArchiveDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanArchive>(selectCriteria);

            IEnumerable<DegreePlanArchive> degreePlanArchiveEntities = BuildDegreePlanArchive(planArchiveDataContracts, new List<DataContracts.DegreePlanCommentArchv>());
            return degreePlanArchiveEntities;
        }

        /// <summary>
        /// Build degree plan archive entities from data contracts for the degree plan archives and the degree plan comment archives
        /// </summary>
        /// <param name="planArchiveDataContracts">Degree Plan Archive data contracts</param>
        /// <param name="planArchiveCommentsDataContracts">Degree Plan Comment Archive data contracts</param>
        /// <returns>List of degree plan archive entities</returns>
        private IEnumerable<DegreePlanArchive> BuildDegreePlanArchive(IEnumerable<DataContracts.DegreePlanArchive> planArchiveDataContracts, IEnumerable<DataContracts.DegreePlanCommentArchv> planArchiveCommentsDataContracts)
        {

            List<DegreePlanArchive> degreePlanArchiveEntities = new List<DegreePlanArchive>();

            foreach (var planArchiveDC in planArchiveDataContracts)
            {
                try
                {

                    DegreePlanArchive planArchiveEntity = new DegreePlanArchive(Int32.Parse(planArchiveDC.Recordkey), Int32.Parse(planArchiveDC.DparchvDegreePlanId), planArchiveDC.DparchvStudentId, Int32.Parse(planArchiveDC.DparchvVersionNumber));
                    planArchiveEntity.CreatedBy = planArchiveDC.DegreePlanArchiveAddopr;
                    planArchiveEntity.ReviewedBy = planArchiveDC.DparchvLastReviewedBy;
                    if (planArchiveDC.DparchvLastReviewedDate != null)
                    {
                        if (planArchiveDC.DparchvLastReviewedTime != null && planArchiveDC.DparchvLastReviewedDate != null)
                        {
                            planArchiveEntity.ReviewedDate = planArchiveDC.DparchvLastReviewedTime.ToPointInTimeDateTimeOffset(
                                planArchiveDC.DparchvLastReviewedDate, colleagueTimeZone);
                        }
                        else if (planArchiveDC.DparchvLastReviewedDate != null)
                        {
                            planArchiveEntity.ReviewedDate = new DateTimeOffset(planArchiveDC.DparchvLastReviewedDate.Value, new TimeSpan(0, 0, 0));
                        }
                        else
                        {
                            new DateTimeOffset();
                        }
                    }
                    if (planArchiveDC.DegreePlanArchiveAdddate != null)
                    {
                        planArchiveEntity.CreatedDate = planArchiveDC.DegreePlanArchiveAddtime.ToPointInTimeDateTimeOffset(
                            planArchiveDC.DegreePlanArchiveAdddate, colleagueTimeZone) ?? new DateTimeOffset();
                    }

                    var programs = new List<StudentProgram>();
                    if (planArchiveDC.DparchvProgramsEntityAssociation != null && planArchiveDC.DparchvProgramsEntityAssociation.Count() > 0)
                    {
                        foreach (var program in planArchiveDC.DparchvProgramsEntityAssociation)
                        {
                            programs.Add(new StudentProgram(planArchiveDC.DparchvStudentId, program.DparchvPgmAcadProgramIdAssocMember, program.DparchvPgmCatalogAssocMember));
                        }
                    }

                    planArchiveEntity.StudentPrograms = programs;

                    var courses = new List<ArchivedCourse>();
                    if (planArchiveDC.DparchvCoursesEntityAssociation != null && planArchiveDC.DparchvCoursesEntityAssociation.Count() > 0)
                    {
                        foreach (var course in planArchiveDC.DparchvCoursesEntityAssociation)
                        {
                            try
                            {
                                var archivedCourse = new ArchivedCourse(course.DparchvCrsCourseIdAssocMember);
                                if (course.DparchvCrsStatusDateAssocMember.HasValue && course.DparchvCrsStatusTimeAssocMember.HasValue)
                                {
                                    archivedCourse.ApprovalDate = course.DparchvCrsStatusTimeAssocMember.ToPointInTimeDateTimeOffset(
                                        course.DparchvCrsStatusDateAssocMember, colleagueTimeZone);
                                }

                                archivedCourse.ApprovalStatus = course.DparchvCrsApprovalStatusAssocMember;
                                archivedCourse.ApprovedBy = course.DparchvCrsStatusByAssocMember;
                                archivedCourse.Credits = course.DparchvCrsCreditsAssocMember;
                                archivedCourse.Name = course.DparchvCrsNameAssocMember;
                                archivedCourse.SectionId = course.DparchvCrsSectionIdAssocMember;
                                archivedCourse.TermCode = course.DparchvCrsTermIdAssocMember;
                                archivedCourse.Title = course.DparchvCrsTitleAssocMember;
                                archivedCourse.AddedBy = course.DparchvCrsAddedByAssocMember;
                                if (course.DparchvCrsAddedOnDateAssocMember.HasValue && course.DparchvCrsAddedOnTimeAssocMember.HasValue)
                                {
                                    archivedCourse.AddedOn = course.DparchvCrsAddedOnTimeAssocMember.ToPointInTimeDateTimeOffset(
                                        course.DparchvCrsAddedOnDateAssocMember, colleagueTimeZone);
                                }
                                archivedCourse.RegistrationStatus = course.DparchvCrsStcStatusAssocMember;
                                archivedCourse.ContinuingEducationUnits = course.DparchvCrsCeusAssocMember;
                                archivedCourse.IsPlanned = (course.DparchvCrsIsPlannedAssocMember == "Y");
                                archivedCourse.HasWithdrawGrade = (course.DparchvCrsHasWithdrawGrdAssocMember == "Y");
                                courses.Add(archivedCourse);
                            }
                            catch (Exception ex)
                            {
                                // For just a bad course on the archive - skip it and keep building the degree plan archive. But log it.
                                var archiveError = "DegreePlanArchive course corrupt for DegreePlanArchive " + planArchiveDC.Recordkey;
                                logger.Error(ex, archiveError);
                            }

                        }
                    }
                    planArchiveEntity.ArchivedCourses = courses;

                    var notes = new List<DegreePlanNote>();

                    // Now retrieve and add any comments that exist for this degree plan archive.
                    var comments = planArchiveCommentsDataContracts.Where(dc => dc.DpcarchvDegreePlanArchive == planArchiveEntity.Id.ToString()).ToList();
                    foreach (var comment in comments)
                    {
                        try
                        {
                            // The DegreePlanNote constructor requires a degree plan note added date.
                            DateTimeOffset? createdDate = comment.DpcarchvAddedTime.ToPointInTimeDateTimeOffset(comment.DpcarchvAddedDate, colleagueTimeZone);
                            notes.Add(new DegreePlanNote(int.Parse(comment.Recordkey), comment.DpcarchvAddedBy, createdDate, comment.DpcarchvText));
                        }
                        catch (Exception ex)
                        {
                            var archiveError = "DegreePlanCommentArchive record corrupt for DegreePlanArchive " + planArchiveDC.Recordkey;
                            logger.Error(ex, archiveError);
                        }

                    }

                    // Add the list of comments found to the degreeplan object
                    planArchiveEntity.Notes = notes;

                    degreePlanArchiveEntities.Add(planArchiveEntity);

                }
                catch (Exception ex)
                {
                    LogDataError("DegreePlanArchive", planArchiveDC.Recordkey, planArchiveDC, ex);
                }
            }

            return degreePlanArchiveEntities;
        }

        //TODO
        /// <summary>
        /// Get a degree plan archive entity with all of its information so that we can ultimately generate a pdf
        /// </summary>
        /// <param name="degreePlanArchiveId"></param>
        /// <returns></returns>
        public async Task<DegreePlanArchive> GetDegreePlanArchiveAsync(int degreePlanArchiveId)
        {
            if (degreePlanArchiveId <= 0)
            {
                throw new ArgumentException("Degree Plan Archive Id must be greater than 0");
            }
            DataContracts.DegreePlanArchive archive = await DataReader.ReadRecordAsync<DataContracts.DegreePlanArchive>("DEGREE_PLAN_ARCHIVE", degreePlanArchiveId.ToString());
            if (archive == null)
            {
                throw new KeyNotFoundException("No Degree Plan Archive found with Archive Id " + degreePlanArchiveId);
            }
            else
            {
                // Now get the associated degree plan comment archives for this degree plan archive.
                var degreePlanCommentQuery = "WITH DPCARCHV.DEGREE.PLAN.ARCHIVE EQ '" + degreePlanArchiveId + "'";
                var planCommentsDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanCommentArchv>(degreePlanCommentQuery);
                var planCommentsDataContractsList = planCommentsDataContracts.ToList();

                planCommentsDataContractsList.Select(c => {
                    c.DpcarchvText = System.Web.HttpUtility.HtmlDecode(c.DpcarchvText);
                    c.DpcarchvText = c.DpcarchvText.Replace("<br/>", "\n");
                    return c; }).ToList();
                
                var archiveEntities = BuildDegreePlanArchive(new List<DataContracts.DegreePlanArchive> { archive }, planCommentsDataContractsList);
                DegreePlanArchive planArchiveEntity = archiveEntities.Where(d => d.Id == degreePlanArchiveId).FirstOrDefault();
                return planArchiveEntity;
            }
        }

    }
}
