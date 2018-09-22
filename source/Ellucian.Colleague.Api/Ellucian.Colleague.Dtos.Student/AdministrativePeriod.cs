using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains details of administrative period
    /// </summary>
    public class AdministrativePeriod
    {
        /// <summary>
        /// Administrative time period
        /// </summary>
        public String AdministrativePeriodId { get; set; }

        /// <summary>
        /// A code that identifies an administrative time period.
        /// </summary>
        public String Code { get; set; }

        /// <summary>
        /// Title of the administrative time period.
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Description of the administrative time period.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Administrative Period Start Date
        /// /// </summary>
        public DateTime? PeriodStartDate { get; set; }

        /// <summary>
        /// Administrative Period End Date
        /// </summary>
        /// 
        public DateTime? PeriodEndDate { get; set; }

        /// <summary>
        /// Administrative period type
        /// </summary>
        public String AcademicPeriodsType { get; set; }
    }
}