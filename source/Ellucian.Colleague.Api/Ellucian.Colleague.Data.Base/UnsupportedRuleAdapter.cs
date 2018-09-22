// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// This adapter exists to accommodate rules whose primary view do not have a corresponding C# type.
    /// </summary>
    public class UnsupportedRuleAdapter : IRuleAdapter
    {
        /// <summary>
        /// Creates an object to represent the specified rule. If the rule can be executed in .NET,
        /// the rule's expression shall be populated.
        /// </summary>
        /// <param name="descriptor">the rule data from Colleague</param>
        /// <returns>
        /// a rule
        /// </returns>
        public Rule Create(RuleDescriptor descriptor)
        {
            return new Rule<UnknownRuleContext>(descriptor.Id);
        }

        /// <summary>
        /// Returns the record ID for the specified context object. This is used when mapping
        /// from a rule back to Colleague for execution in Envision.
        /// </summary>
        /// <param name="context">must be object of the context type that this adapter deals with</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetRecordId(object context)
        {
            // This should never be called
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the Colleague file suite instance of the specified context object. This is used for objects that are equivalent to
        /// Colleague file suite specific objects so that we can map a file suite rule 
        /// back to Colleague for execution in Envision. For non file suite type objects, this
        /// method should return an empty string.
        /// </summary>
        /// <param name="context">must be an object of the context type that this adapter deals with</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetFileSuiteInstance(object context)
        {
            //This should never be called
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the context type.
        /// </summary>
        public Type ContextType
        {
            get { return typeof(UnknownRuleContext); }
        }


        /// <summary>
        /// Gets the expected primary view
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public string ExpectedPrimaryView
        {
            get { throw new NotImplementedException(); }
        }



    }
}
