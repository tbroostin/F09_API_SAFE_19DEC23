// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// State or province code.
    /// </summary>
    [Serializable]
    public class State : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="code">The code of the state or province.</param>
        /// <param name="description">The description of the state or province</param>
        /// <param name="countryCode">The country code associated to the state or province</param>
        public State(string code, string description, string countryCode = null)
            : base(code, description)
        {
           _countryCode = countryCode; 
        }
        private readonly string _countryCode;
        
        /// <summary>
        /// The optional country code connected to the specific state or province 
        /// </summary>
        public string CountryCode { get { return _countryCode; } }
    }
}