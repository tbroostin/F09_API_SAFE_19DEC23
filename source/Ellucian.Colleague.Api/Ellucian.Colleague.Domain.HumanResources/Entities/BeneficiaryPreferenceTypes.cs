//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// BeneficiaryTypes
    /// </summary>
    [Serializable]
    public class BeneficiaryTypes : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="BeneficiaryTypes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public BeneficiaryTypes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}