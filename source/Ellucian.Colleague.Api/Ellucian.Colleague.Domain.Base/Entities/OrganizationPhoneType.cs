using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an phone contact for an organization
    /// </summary>
    [Serializable]
    public enum OrganizationPhoneType
    {
        /// <summary>
        /// An organization's main office or headquarters phone.
        /// </summary>
        Main,
        /// <summary>
        /// An organization's branch office phone.
        /// </summary>
       Branch,
        /// <summary>
       /// An organization's regional office phone.
        /// </summary>
        Region,
        /// <summary>
        /// An organization's support or help phone.
        /// </summary>
        Support,
        /// <summary>
        /// An organization's phone number for billing information
        /// </summary>
        Billing,     
        /// <summary>
        /// Uncategorized phone number
        /// </summary>
        Other
    }
}
