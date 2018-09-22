/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Identifies how Awards are grouped on the financial aid shopping sheet
    /// </summary>
    [Serializable]
    public enum ShoppingSheetAwardGroup
    {
        /// <summary>
        /// Grants given to students by the school
        /// </summary>
        SchoolGrants,

        /// <summary>
        /// Federal Pell Grants
        /// </summary>
        PellGrants,

        /// <summary>
        /// Grants given to students by the state
        /// </summary>
        StateGrants,

        /// <summary>
        /// Any other grants given to students
        /// </summary>
        OtherGrants,

        /// <summary>
        /// Awards given to students who will work for the money
        /// </summary>
        WorkStudy,

        /// <summary>
        /// Perkins Loans
        /// </summary>
        PerkinsLoans,

        /// <summary>
        /// Subsidized Federal Direct Stafford Loans
        /// </summary>
        SubsidizedLoans,

        /// <summary>
        /// Unsubsidized Federal Direct Stafford Loans
        /// </summary>
        UnsubsidizedLoans
    }
}
