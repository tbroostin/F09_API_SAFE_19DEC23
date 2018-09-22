// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Other Degree
    /// </summary>
    [Serializable]
    public class OtherDegree : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OtherDegree"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Degree</param>
        /// <param name="description">Description or Title of the Degree </param>
        public OtherDegree(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}
