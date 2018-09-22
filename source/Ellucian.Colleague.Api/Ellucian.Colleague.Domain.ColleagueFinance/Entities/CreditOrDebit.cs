// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Enumeration of amount value expressed as either a credit or a debit.
    /// Used in GeneralLedgerTransaction for the Higher Education Data Model.
    /// </summary>
    [Serializable]
    public enum CreditOrDebit
    {
        /// <summary>
        /// Credit
        /// </summary>
        Credit,

        /// <summary>
        /// Debit
        /// </summary>
        Debit
    }
}