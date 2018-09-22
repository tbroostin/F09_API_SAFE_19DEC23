// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Exposes the appeal code and its description
    /// </summary>
    public class AcademicProgressAppealCode
    {
        /// <summary>
        /// The Appeal Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A short description of the appeal code
        /// </summary>
        public string Description { get; set; }

    }
}
