﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Tax Code DTO.
    /// </summary>
    public class AccountsPayableTax
    {
        /// <summary>
        /// This is the tax code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// This is the tax code description.
        /// </summary>
        public string Description { get; set; }
    }
}
