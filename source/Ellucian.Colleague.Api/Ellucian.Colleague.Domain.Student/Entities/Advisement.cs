using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Advisement information for a student.
    /// </summary>
    [Serializable]
    public class Advisement
    {
        /// <summary>
        /// Advisor Id (required)
        /// </summary>
        public String AdvisorId { get; set; }
        /// <summary>
        /// Advisement start date
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Advisement end date
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Advisement type (academic, athletic, etc)
        /// </summary>
        public String AdvisorType { get; set; }

        public Advisement(string advisorId, DateTime? startDate)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "A Student Advisement must have an advisor Id.");
            }
            this.AdvisorId = advisorId;
            this.StartDate = startDate;
            this.EndDate = null;
            this.AdvisorType = null;
        }
    }
}