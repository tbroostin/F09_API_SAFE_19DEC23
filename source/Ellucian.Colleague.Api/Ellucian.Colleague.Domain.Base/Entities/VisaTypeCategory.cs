// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public enum VisaTypeCategory
    {
        /// <summary>
        /// Immigrant Category type for VisaType
        /// </summary>
        Immigrant = 1,

        /// <summary>
        /// Non-Immigrant Category type for VisaType
        /// </summary>
        NonImmigrant = 2
    }
}
