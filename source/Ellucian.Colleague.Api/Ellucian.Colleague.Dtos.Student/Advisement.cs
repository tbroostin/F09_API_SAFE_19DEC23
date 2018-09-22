using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains details of student advisement records
    /// </summary>
    public class Advisement
    {
        /// <summary>
        /// Advisor
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
    }
}