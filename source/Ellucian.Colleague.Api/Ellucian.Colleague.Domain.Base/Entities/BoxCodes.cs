// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Box codes.
    /// </summary>
    [Serializable]
    public class BoxCodes : CodeItem
    {        
        /// <summary>
        /// Initializes a new instance of the<see cref="BoxCodes"/> class.
        /// </summary>
        /// <param name = "code" > The code.</param>
        /// <param name = "description" > The description.</param>
        public BoxCodes(string code, string description)
             : base(code, description)
        {
           
        }
        
    }
}

