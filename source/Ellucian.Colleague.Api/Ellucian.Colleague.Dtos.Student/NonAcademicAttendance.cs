//Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A student's attendance for a nonacademic event
    /// </summary>
    public class NonAcademicAttendance
    {
        /// <summary>
        /// Unique identifier for the nonacademic event attendance
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier for the person who attended the nonacademic event
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Unique identifier for the nonacademic event that the person attended
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// The number of units that the person earned for attending the nonacademic event
        /// </summary>
        public decimal? UnitsEarned { get; set; }
    }
}
