// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Interest codes
    /// </summary>
    [Serializable]
    public class Interest : GuidCodeItem
    {
        private string _type;
        /// <summary>
        /// Interest Type
        /// </summary>
        public string Type { get { return _type; } }
        
       
        /// <summary>
        /// Initializes a new instance of the <see cref="Interest"/> class.
        /// </summary>
        /// <param name="guid">Unique Identifier.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Interest(string guid, string code, string description, string type = null)
            : base(guid, code, description)
        {
            _type = type;
        }

    }
}