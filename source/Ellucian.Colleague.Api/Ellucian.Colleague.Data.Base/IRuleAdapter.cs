using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Maps between Colleague metadata about rules and executable versions in .NET.
    /// You'd have one of these for every "primary view" or "context type" that the .NET side cares about.
    /// </summary>
    public interface IRuleAdapter
    {
        /// <summary>
        /// Creates an object to represent the specified rule. If the rule can be executed in .NET, 
        /// the rule's expression shall be populated.
        /// </summary>
        /// <param name="descriptor">the rule data from Colleague</param>
        /// <returns>a rule</returns>
        Rule Create(RuleDescriptor descriptor);

        /// <summary>
        /// Returns the record ID for the specified context object. This is used when mapping
        /// from a rule back to Colleague for execution in Envision.
        /// </summary>
        /// <param name="context">must be object of the context type that this adapter deals with</param>
        /// <returns></returns>
        string GetRecordId(object context);

        /// <summary>
        /// Returns the file suite instance of the specified context object. This is used for file suite specific objects when 
        /// mapping from a rule back to Colleague for execution in Envision. For non file suite objects, this
        /// method should return an empty string.
        /// </summary>
        /// <param name="context">must be an object of the context type that this adapter deals with</param>
        /// <returns></returns>
        string GetFileSuiteInstance(object context);

        /// <summary>
        /// Answers the type of object on the .NET side that the rule executes against.
        /// </summary>
        Type ContextType { get; }

        /// <summary>
        /// Answers the primary view on the Colleague side that this adapter is expecting to simulate.
        /// </summary>
        string ExpectedPrimaryView { get; }
    }
}
