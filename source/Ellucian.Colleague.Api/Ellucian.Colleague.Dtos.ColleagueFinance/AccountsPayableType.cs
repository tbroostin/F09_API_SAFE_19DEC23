// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Accounts Payable type DTO.
    /// </summary>
    public class AccountsPayableType
    {
        /// <summary>
        /// AP type code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// AP type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// AP type source
        /// </summary>
        public string Source { get; set; }
    }
}
