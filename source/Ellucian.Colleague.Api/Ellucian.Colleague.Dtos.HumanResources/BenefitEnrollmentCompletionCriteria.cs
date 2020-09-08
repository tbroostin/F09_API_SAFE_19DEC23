/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Class containing all input parameters for Benefit Enrollment Completion
    /// </summary>
   public class BenefitEnrollmentCompletionCriteria
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
        /// Id of the benefits package associated with the enrollment period
        /// </summary>
        public string BenefitsPackageId { get; set; }

        /// <summary>
        /// Flag that indicates whether to submit or re-open the benefit elections.
        /// True: Submit benefit elections
        /// False: Reopen benefit elections
        /// </summary>
        public bool SubmitBenefitElections { get; set; }
    }
}
