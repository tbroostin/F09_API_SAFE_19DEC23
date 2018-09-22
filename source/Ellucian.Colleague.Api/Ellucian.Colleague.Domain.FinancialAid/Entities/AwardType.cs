//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardType class defines a Financial Aid Award Type.
    /// </summary>
    [Serializable]
    public class AwardType : CodeItem
    {

        /// <summary>
        /// Type (Repeat of description for Colleague.  Used to satisfy 
        /// single Student Success CRM contract for fund sources used for 
        /// both Banner and Colleague.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Constructor for AwardType object requires a code and a description
        /// </summary>
        /// <param name="code">The unique award type code.</param>
        /// <param name="description">A description of the award type.</param>
        public AwardType(string code, string description)
            : base(code, description)
        {
            Type = description;
        }
    }
}
