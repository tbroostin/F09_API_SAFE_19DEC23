// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of rule configurations
    /// </summary>
    [Serializable]
    public class RuleConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether [execute all rules in colleague].
        /// </summary>
        /// <value>
        /// <c>true</c> if [execute all rules in colleague]; otherwise, <c>false</c>.
        /// </value>
        public bool ExecuteAllRulesInColleague { get; set; }
    }
}
