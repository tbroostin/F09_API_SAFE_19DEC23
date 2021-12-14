// Copyright 2021 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Query request for CurriculumTrack
    /// </summary>
    public class CurriculumTrackQueryCriteria
    {
        /// <summary>
        /// Id of the student.
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Program Code for the Curriculum Tracks to retrieve.
        /// </summary>
        public string ProgramCode { get; set; }
    }
}
