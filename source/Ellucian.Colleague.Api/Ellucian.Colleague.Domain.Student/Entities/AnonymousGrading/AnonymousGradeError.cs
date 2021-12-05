// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading
{
    /// <summary>
    /// Error related to preliminary anonymous grading of course sections
    /// </summary>
    [Serializable]
    public class AnonymousGradeError
    {
        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        public string StudentCourseSecId { get; private set; }

        /// <summary>
        /// ID of the associated student academic credit data
        /// </summary>
        public string StudentAcadCredId { get; private set; }

        /// <summary>
        /// ID of the associated preliminary student grade work data
        /// </summary>
        public string PrelimStuGrdWorkId { get; private set; }

        /// <summary>
        /// ID of the associated student terms data
        /// </summary>
        public string StudentTermsId { get; private set; }

        /// <summary>
        /// Explanation of the error
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AnonymousGradeError"/> object
        /// </summary>
        /// <param name="scsId">ID of the associated student course section data</param>
        /// <param name="stcId">ID of the associated student academic credit data</param>
        /// <param name="psgwId">ID of the associated preliminary student grade work data</param>
        /// <param name="sttrId">ID of the associated student terms data</param>
        /// <param name="message">Explanation of the error</param>
        /// <exception cref="ArgumentNullException">An explanatory message is required when building a preliminary anonymous grade error.</exception>
        public AnonymousGradeError(string scsId, string stcId, string psgwId, string sttrId, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "An explanatory message is required when building a preliminary anonymous grade error.");
            }

            StudentCourseSecId = scsId;
            StudentAcadCredId = stcId;
            PrelimStuGrdWorkId = psgwId;
            StudentTermsId = sttrId;
            Message = message;
        }
    }
}
