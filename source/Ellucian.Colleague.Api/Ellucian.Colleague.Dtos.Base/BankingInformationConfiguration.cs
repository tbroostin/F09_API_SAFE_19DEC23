/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration parameters for Colleague Self Service Banking Information
    /// </summary>
    public class BankingInformationConfiguration
    {
        /// <summary>
        /// The terms and conditions displayed to the user when adding or editing an account.
        /// </summary>
        public string AddEditAccountTermsAndConditions { get; set; }

        /// <summary>
        /// The payroll message is displayed to a payroll user at the top of the banking information summary page.
        /// </summary>
        public string PayrollMessage { get; set; }

        /// <summary>
        /// The payroll effective date message is displayed to a payroll under the effective date.
        /// </summary>
        public string PayrollEffectiveDateMessage { get; set; }

        /// <summary>
        /// Indicates that the user must have at least one remainder deposit active for banking information.
        /// </summary>
        public bool IsRemainderAccountRequired { get; set; }

        /// <summary>
        /// Indicates that the client agrees to the terms and conditions for using the federal reserve banks' E-Payments routing directory.
        /// </summary>
        public bool UseFederalRoutingDirectory { get; set; }

        /// <summary>
        /// Indicates that the client does or does not want to use direct deposits in banking information.
        /// </summary>
        public bool IsDirectDepositEnabled { get; set; }

        /// <summary>
        /// Indicates that the client does or does not want to use account payable in banking information.
        /// </summary>
        public bool IsPayableDepositEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not additional account step-up authentication should be disabled.
        /// </summary>
        public bool IsAccountAuthenticationDisabled { get; set; }
    }
}
