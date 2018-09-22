// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of a type of location information for an organization.
    /// </summary>
    [Serializable]
    public enum OrganizationLocationType
    {
        /// <summary>
        /// An organization's business location.
        /// </summary>
        Business,
        /// <summary>
        /// An organizations' post office box or other mail drop location.
        /// </summary>
         Pobox,
        /// <summary>
        /// An organization's main office or headquarters location.
        /// </summary>
        Main,
        /// <summary>
        /// An organization's branch office location.
        /// </summary>
        Branch,
        /// <summary>
        /// An organization's regional office location.
        /// </summary>
        Region,
        /// <summary>
        /// An organization's support or help location.
        /// </summary>
        Support,
        /// <summary>
        /// Uncategorized organization location type
        /// </summary>
        Other
    }
}
