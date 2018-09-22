// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Indicates the status of an academic progress evaluation
    /// </summary>
    public class AcademicProgressStatus
    {
        /// <summary>
        /// Identifying code of the status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Describes the AcademicProgressStatus
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Contains the category of the AcademicProgressStatus
        /// </summary>
        public AcademicProgressStatusCategory? Category { get; set; }
        /// <summary>
        /// Contains the explanation of the AcademicProgressStatus
        /// </summary>
        public string Explanation { get; set; }
    }
}
