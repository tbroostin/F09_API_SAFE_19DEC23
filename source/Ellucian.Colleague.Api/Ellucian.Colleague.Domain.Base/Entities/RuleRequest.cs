// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A request for Colleague to execute a rule against a given record,
    /// as represented by a context object on the .NET side.
    /// </summary>
    /// <typeparam name="T">the type of the context object</typeparam>
    [Serializable]
    public class RuleRequest<T>
    {
        private readonly Rule<T> rule;
        private readonly T context;

        /// <summary>
        /// Gets the rule.
        /// </summary>
        /// <value>
        /// The rule.
        /// </value>
        public Rule<T> Rule { get { return this.rule; } }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public T Context { get { return this.context; } }

        /// <summary>
        /// Initializes a new instance of the RuleRequest class.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="context"></param>
        public RuleRequest(Rule<T> rule, T context)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.rule = rule;
            this.context = context;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RuleRequest<T>))
            {
                return false;
            }
            var other = (RuleRequest<T>)obj;
            return other.rule.Equals(this.rule) && other.context.Equals(this.context);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return rule.GetHashCode() + context.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.context.ToString() + " against rule " + rule.Id;
        }
    }
}
