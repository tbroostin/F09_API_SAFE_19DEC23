// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Budget entity
    /// </summary>
    [Serializable]
    public class Budget
    {
        /// <summary>
        /// ID 
        /// </summary>
        public string RecordKey { get; set; }

        /// <summary>
        /// GUID 
        /// </summary>
        public string BudgetCodeGuid { get; set; }

        /// <summary>
        /// GUID 
        /// </summary>
        public string BudgetPhaseGuid { get; set; }

        /// <summary>
        /// Status 
        /// </summary>
        public string Status { get; set; }

        public string Title { get; set; }
        public string CurrentVersionDesc { get; set; }
        public string CurrentVersionName { get; set; }
        public List<string> Version { get; set; }
        public string BudgetCodesIntgIdx { get; set; }
        public string FiscalYear { get; set; }
    }
}
