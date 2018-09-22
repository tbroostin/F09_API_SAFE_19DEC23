// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of a type of address information for an organization.
    /// </summary>
    [Serializable]
    public enum OrganizationAddressType
    {
        /// <summary>
        /// An organization's business address.
        /// </summary>
        Business,
        /// <summary>
        /// An organizations' post office box or other mail drop address.
        /// </summary>
         Pobox,
        /// <summary>
        /// An organization's main office or headquarters address.
        /// </summary>
        Main,
        /// <summary>
        /// An organization's branch office address.
        /// </summary>
        Branch,
        /// <summary>
        /// An organization's regional office address.
        /// </summary>
        Region,
        /// <summary>
        /// An organization's support or help address.
        /// </summary>
        Support,
        /// <summary>
        /// Uncategorized organization address type
        /// </summary>
        Other
    }
}
