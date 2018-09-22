// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Other Specializations or Concentrations
    /// </summary>
    [Serializable]
    public class OtherSpecial : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OtherSpecial"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Specialization or Concentration</param>
        /// <param name="description">Description or Title of the Specialization or Concentration</param>
        public OtherSpecial(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}
