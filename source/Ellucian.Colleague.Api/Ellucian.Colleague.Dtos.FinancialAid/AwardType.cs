//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Award Type DTO
    /// </summary>
    public class AwardType
    {
        /// <summary>
        /// AwardType object's unique identifier
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Short description of Award Type, usually used for display purposes
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Type (Repeat of description for Colleague.  Used to satisfy 
        /// single Student Success CRM contract for fund sources used for 
        /// both Banner and Colleague.)
        /// </summary>
        public string Type { get; set; }
    }
}
