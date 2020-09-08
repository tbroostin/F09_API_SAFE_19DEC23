// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Attributes of Case Closure Reason related to Retention Alert
    /// </summary>
    [Serializable]
    public class CaseClosureReason : CodeItem
    {
        /// <summary>
        /// The Closure reason Id
        /// </summary>
        public string ClosureReasonId { get; set; }       

        /// <summary>
        /// A list of case categories which have this case closure reason
        /// </summary>
        public List<string> CaseCategories { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="closureReasonId"></param>
        /// <param name="caseCategories"></param>
        public CaseClosureReason(string code, string description, string closureReasonId, List<string> caseCategories)
            : base(code, description)
        {
            ClosureReasonId = closureReasonId;
            CaseCategories = caseCategories;           
        }
    }
}


