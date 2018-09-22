//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
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
