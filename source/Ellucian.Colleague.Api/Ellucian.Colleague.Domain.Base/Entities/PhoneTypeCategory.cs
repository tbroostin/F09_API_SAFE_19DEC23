using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an phone type
    /// </summary>
    [Serializable]
    public enum PhoneTypeCategory
    {
        /// <summary>
        /// A person's/organization's mobile phone.
        /// </summary>
        Mobile,
        /// <summary>
        /// A person's/organization's home phone..
        /// </summary>
        Home,
        /// <summary>
        /// A person's/organization's phone while in school..
        /// </summary>
        School,
        /// <summary>
        /// A person's/organization's phone while on vacation.
        /// </summary>
        Vacation,
        /// <summary>
        /// A person's/organization's work phone.
        /// </summary>
        Business,
        /// <summary>
        /// A person's/organization's personal fax machine.
        /// </summary>
        Fax,
        /// <summary>
        /// A person's/organization's pager.
        /// </summary>
        Pager,
        /// <summary>
        /// A person's/organization's TTY/TDD device.
        /// </summary>
        TDD,
        /// <summary>
        /// A person's/organization's parent phone.
        /// </summary>
        Parent,
        /// <summary>
        /// A person's/organization's family phone.
        /// </summary>
        Family,
        /// <summary>
        /// A person's/organization's main office or headquarters phone.
        /// </summary>
        Main,
        /// <summary>
        /// An person's/organization's branch office phone.
        /// </summary>
        Branch,
        /// <summary>
        /// An person's/organization's regional office phone.
        /// </summary>
        Region,
        /// <summary>
        /// An person's/organization's support or help phone.
        /// </summary>
        Support,
        /// <summary>
        /// An person's/organization's phone number for billing information
        /// </summary>
        Billing,
        /// <summary>
        /// An person's/organization's phone number for matching information
        /// </summary>
        MatchingGifts,
        /// <summary>
        /// Uncategorized phone number
        /// </summary>
        Other
    }
}
