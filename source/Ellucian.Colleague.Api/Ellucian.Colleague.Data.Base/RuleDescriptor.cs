// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Intermediate representation of a rule from Colleague.
    /// </summary>
    public class RuleDescriptor
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the primary view.
        /// </summary>
        /// <value>
        /// The primary view.
        /// </value>
        public string PrimaryView { get; set; }

        /// <summary>
        /// A collection of expressions
        /// </summary>
        private IList<RuleExpressionDescriptor> expressions = new List<RuleExpressionDescriptor>();
        public IList<RuleExpressionDescriptor> Expressions
        {
            get { return expressions; }
            set { expressions = value; }
        }

        /// <summary>
        /// Gets or sets the not supported message.
        /// </summary>
        /// <value>
        /// The not supported message.
        /// </value>
        public string NotSupportedMessage { get; set; }

        /// <summary>
        /// Database-specific options for converting data for rule evaluation, such as for date conversions.
        /// </summary>
        public RuleConversionOptions RuleConversionOptions { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Id;
        }
    }
}
