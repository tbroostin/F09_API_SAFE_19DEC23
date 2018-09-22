// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Code that defines the GL account to be credited when someone makes a payment
    /// </summary>
    [Serializable]
    public class Distribution2 : GuidCodeItem
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Distribution code</param>
        /// <param name="description">Distribution description</param>
        public Distribution2(string guid, string code, string description)
            : base(guid, code, description)
        {
            
        }
    }
}
