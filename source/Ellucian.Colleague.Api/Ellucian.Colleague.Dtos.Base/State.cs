// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Code and Description for a state or province
    /// </summary>
    public class State
    {
        /// <summary>
        /// Unique system code for a State or Province
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// State or Province description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The optional country code connected to the specific state or province 
        /// </summary>
        public string CountryCode { get; set; }
    }
}
