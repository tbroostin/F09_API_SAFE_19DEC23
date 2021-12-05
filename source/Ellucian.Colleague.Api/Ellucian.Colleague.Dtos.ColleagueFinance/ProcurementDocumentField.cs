// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A procurement document field
    /// </summary>    
    public class ProcurementDocumentField
    {
        /// <summary>
        /// Unique identifier for the document field
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the document field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Field requirement type
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// The type of field
        /// </summary>
        public string Type { get; set; }
    }
}
