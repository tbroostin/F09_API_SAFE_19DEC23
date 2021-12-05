// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Coordination.Planning.Reports;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    public interface IDegreePlanService
    {
        Task<IEnumerable<CurriculumTrack>> QueryCurriculumTracksForStudentByProgramAsync(CurriculumTrackQueryCriteria criteria);
        Task<DegreePlanPreview> PreviewSampleDegreePlanAsync(int degreePlanId, string programCode);
        [Obsolete("Obsolete on version 1.5 of the API. Use PreviewSampleDegreePlan3Async going forward.")]
        Task<DegreePlanPreview2> PreviewSampleDegreePlan2Async(int degreePlanId, string programCode, string firstTermCode);
        [Obsolete("Obsolete on version 1.6 of the Api. Use PreviewSampleDegreePlan4Async going forward.")]
        Task<DegreePlanPreview3> PreviewSampleDegreePlan3Async(int degreePlanId, string programCode, string firstTermCode);
        [Obsolete("Obsolete on version 1.11 of the Api. Use PreviewSampleDegreePlan5Async going forward.")]
        Task<DegreePlanPreview4> PreviewSampleDegreePlan4Async(int degreePlanId, string programCode, string firstTermCode);
        [Obsolete("Obsolete on version 1.18 of the Api. Use PreviewSampleDegreePlan6Async going forward.")]
        Task<DegreePlanPreview5> PreviewSampleDegreePlan5Async(int degreePlanId, string programCode, string firstTermCode);
        [Obsolete("Obsolete on version 1.32 of the Api. Use PreviewSampleDegreePlan7Async going forward.")]
        Task<DegreePlanPreview6> PreviewSampleDegreePlan6Async(int degreePlanId, string programCode, string firstTermCode);

        [Obsolete("Obsolete on version 1.33 of the Api. Use PreviewSampleDegreePlan8Async going forward.")]
        Task<DegreePlanPreview6> PreviewSampleDegreePlan7Async(int degreePlanId, string curriculumTrackCode, string firstTermCode);

        /// <summary>
        /// CURRENT VERSION: Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <remarks>This method considers equated courses when deciding whether or not to add planned courses to terms; if the student is currently
        /// taking or previously completed a given course or one of its equated courses, that course will not be added to the degree plan preview.
        /// This method examines the student's academic history and considers which academic credits, if any, would be applied to the provided 
        /// academic program and catalog</remarks>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="curriculumTrackCode">Curriculum track code from which the sample plan will be obtained</param>
        /// <param name="firstTermCode">First academic term to which sample course plan will be applied</param>
        /// <param name="programCode">Academic program for which student's academic history will be considered</param>
        /// <returns>Sample course plan</returns>
        Task<DegreePlanPreview7> PreviewSampleDegreePlan8Async(int degreePlanId, string curriculumTrackCode, string firstTermCode, string programCode);

        Task<bool> CheckForSampleAsync(string programCode, string catalog);

        [Obsolete("Obsolete on version 1.5 of the Api. Use ArchiveDegreePlan2 going forward.")]
        Task<DegreePlanArchive> ArchiveDegreePlanAsync(DegreePlan2 degreePlan);
        [Obsolete("Obsolete on version 1.7 of the Api. Use ArchiveDegreePlan3 going forward.")]
        Task<DegreePlanArchive2> ArchiveDegreePlan2Async(DegreePlan3 degreePlan);
        Task<DegreePlanArchive2> ArchiveDegreePlan3Async(DegreePlan4 degreePlan);

        Task<IEnumerable<DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId);
        Task<IEnumerable<DegreePlanArchive2>> GetDegreePlanArchives2Async(int degreePlanId);

        Task<DegreePlanArchiveReport> GetDegreePlanArchiveReportAsync(int archiveId);
        Byte[] GenerateDegreePlanArchiveReport(DegreePlanArchiveReport archiveReport, string path, string reportLogoPath);


        [Obsolete("Deprecated on version 1.2 of the Api. Use PreviewSampleDegreePlan along with the UpdateDegreePlan methods going forward.")]
        Task<DegreePlan> ApplySampleDegreePlanAsync(string studentId, string programCode);

        Task<IEnumerable<DegreePlanReviewRequest>> GetReviewRequestedDegreePlans(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);

        Task<DegreePlanReviewRequest> UpdateAdvisorAssignment(DegreePlanReviewRequest degreePlanReviewRequest);
        Task<IEnumerable<DegreePlanReviewRequest>> SearchReviewRequestDegreePlans(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);

        Task<IEnumerable<DegreePlanReviewRequest>> SearchReviewRequestDegreePlansForExactMatchAsync(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);
    }
}
