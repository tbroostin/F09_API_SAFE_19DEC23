// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Defines the code and description of an Academic Progress Appeal Code
    /// </summary>
    [Serializable]
    public class AcademicProgressAppealCode : CodeItem
    {
        /// <summary>
        /// The appeal code for a single academic progress evaluation
        /// </summary>
        public string Code;

        /// <summary>
        /// A short description of the appeal code
        /// </summary>
        public string Description;
    

        public AcademicProgressAppealCode(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
