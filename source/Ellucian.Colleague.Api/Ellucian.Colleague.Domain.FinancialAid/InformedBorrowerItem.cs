//Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An Informed Borrower Item object that contains the associated status and award year
    /// </summary>
    [Serializable]
    public class InformedBorrowerItem
    {
        /// <summary>
        /// FA Year Associated with the InformedBorrowerItem
        /// </summary>
        public string FaYear { get; set; }

        /// <summary>
        /// Status associated with Informed Borrower.
        /// True if complete, False if incomplete
        /// </summary>
        public bool IsInformedBorrowerComplete { get; set; }
    }
}
