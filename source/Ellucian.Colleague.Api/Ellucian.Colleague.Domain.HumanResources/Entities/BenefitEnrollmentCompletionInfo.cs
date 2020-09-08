/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// BenefitEnrollmentCompletionInfo Domain Entity
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Constructor to build a BenefitEnrollmentCompletionInfo object
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>
        /// <param name="enrollmentConfirmationDate">Date when the user submits the benefits for completion</param>
        /// <param name="errorMessages">List of error message from CTX</param>
        public BenefitEnrollmentCompletionInfo(string employeeId, string enrollmentPeriodId, DateTime? enrollmentConfirmationDate, IEnumerable<string> errorMessages)
        {
            EmployeeId = employeeId;
            EnrollmentPeriodId = enrollmentPeriodId;
            EnrollmentConfirmationDate = enrollmentConfirmationDate;
            ErrorMessages = errorMessages;
        }
    }
}
