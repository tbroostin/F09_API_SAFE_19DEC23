// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.AnonymousGrading
{
    /// <summary>
    /// Error related to preliminary anonymous grading of course sections
    /// </summary>
    public class AnonymousGradeError
    {
        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        public string StudentCourseSecId { get; set; }

        /// <summary>
        /// ID of the associated student academic credit data
        /// </summary>
        public string StudentAcadCredId { get; set; }

        /// <summary>
        /// ID of the associated preliminary student grade work data
        /// </summary>
        public string PrelimStuGrdWorkId { get; set; }

        /// <summary>
        /// ID of the associated student terms data
        /// </summary>
        public string StudentTermsId { get; set; }

        /// <summary>
        /// Explanation of the error
        /// </summary>
        public string Message { get; set; }
    }
}
