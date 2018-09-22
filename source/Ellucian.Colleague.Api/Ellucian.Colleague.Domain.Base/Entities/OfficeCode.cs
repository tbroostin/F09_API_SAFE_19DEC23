// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Office codes
    /// </summary>
    [Serializable]
    public class OfficeCode : CodeItem
    {
        /// <summary>
        /// The type of office this code represents. Examples: FinancialAid, Admissions, etc.
        /// </summary>
        public OfficeCodeType Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficeCode"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public OfficeCode(string code, string description)
            : base(code, description)
        {
        }

        /// <summary>
        /// Override ToString method returns the Code of this OfficeCode
        /// </summary>
        /// <returns>Code attribute of this OfficeCode</returns>
        public override string ToString()
        {
            return Code;
        }
    }
}
