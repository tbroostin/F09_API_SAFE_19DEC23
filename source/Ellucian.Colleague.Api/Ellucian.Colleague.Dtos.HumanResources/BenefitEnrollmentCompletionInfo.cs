/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO Class for Benefit Enrollment Completion
    /// </summary>
    public class BenefitEnrollmentCompletionInfo
    {

        /// <summary>
        /// Employee Id of the user
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Id of the current enrollment period
        /// </summary>
        public string EnrollmentPeriodId { get; set; }
        
        /// <summary>
        /// Date when the user submits the benefits for completion
        /// </summary>
        public DateTime? EnrollmentConfirmationDate { get; set; }

        /// <summary>
        /// List of error messages from transaction
        /// </summary>

        public IEnumerable<string> ErrorMessages { get; set; }
    }
}
