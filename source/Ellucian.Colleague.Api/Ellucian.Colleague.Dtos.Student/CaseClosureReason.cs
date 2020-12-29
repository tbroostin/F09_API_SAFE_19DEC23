// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of Case Closure Reason related to Retention Alert
    /// </summary>
    public class CaseClosureReason
    {
        /// <summary>
        /// The Closure reason Id
        /// </summary>
        public string ClosureReasonId { get; set; }

        /// <summary>
        /// Case Closure Reason code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Case Closure Reason description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of case categories which have this case closure reason
        /// </summary>
        public IEnumerable<string> CaseCategories { get; set; }
    }
}
