// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The response from Colleague after executing a rule.
    /// </summary>
    [Serializable]
    public class RuleResult
    {
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public object Context { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [passed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [passed]; otherwise, <c>false</c>.
        /// </value>
        public bool Passed { get; set; }
        /// <summary>
        /// Gets or sets the rule identifier.
        /// </summary>
        /// <value>
        /// The rule identifier.
        /// </value>
        public string RuleId { get; set; }
    }
}
