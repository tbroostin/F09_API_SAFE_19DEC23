/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    /// <summary>
    /// Represents specific exception codes for handling and reporting errors associated with TotalCompensation Transaction execution.
    /// </summary>
    public enum TotalCompensationExceptionCodes
    {
        /// <summary>
        /// Occurs when employee restriction rules are configured.
        /// </summary>
        RestrictedbyRules,

        /// <summary>
        /// Occurs when the employee involved in total compensation calcuation , has no active employment status.
        /// </summary>
        NoActiveEmploymentStatus,

        /// <summary>
        /// Occurs when the employee involved in total compensation calcuation is terminated.
        /// </summary>
        EmployeeTerminated,

        /// <summary>
        /// Occurs when the employee involved in total compensation calcuation , has no active person wage records.
        /// </summary>
        NoActiveWage,

        /// <summary>
        /// Used to trap any other generic exception while total compensation calculation
        /// </summary>
        OtherMessage
    }
}
