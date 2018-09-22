/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class BankingInformationConfiguration
    {
        #region PROPERTIES
        /// <summary>
        /// Institution provided service terms and conditions
        /// </summary>
        public string AddEditAccountTermsAndConditions { get; set; }

        /// <summary>
        /// Institution provided "blanket" payroll message
        /// </summary>
        public string PayrollMessage { get; set; }

        /// <summary>
        /// Institution provided effective date policy message
        /// </summary>
        public string PayrollEffectiveDateMessage { get; set; }

        /// <summary>
        /// Indication that client requires at least one active remainder account
        /// </summary>
        public bool IsRemainderAccountRequired { get; set; }

        /// <summary>
        /// Indication that client wants to use the federal reserve banks' routing directory
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
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Builds configuration object
        /// </summary>
        public BankingInformationConfiguration()
        {

        }

        #endregion

    }
}
