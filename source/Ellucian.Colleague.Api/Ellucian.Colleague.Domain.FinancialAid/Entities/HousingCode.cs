//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Housing codes
    /// </summary>
    [Serializable]
    public enum HousingCode
    {   
        /// <summary>
        /// On campus housing
        /// </summary>
        OnCampus,
        /// <summary>
        /// With parents
        /// </summary>
        WithParent,
        /// <summary>
        /// Off campus housing
        /// </summary>
        OffCampus
    }
}
