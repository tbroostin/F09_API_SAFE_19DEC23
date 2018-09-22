using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an email address
    /// </summary>
    [Serializable]
    public enum PersonPhoneType
    {
        /// <summary>
        /// A person's mobile phone.
        /// </summary>
        Mobile,
        /// <summary>
        /// A person's home phone..
        /// </summary>
        Home,
        /// <summary>
        /// A person's phone while in school..
        /// </summary>
        School,
        /// <summary>
        /// A person's phone while on vacation.
        /// </summary>
        Vacation,
        /// <summary>
        /// A person's work phone.
        /// </summary>
        Business,
        /// <summary>
        /// A person's personal fax machine.
        /// </summary>
        Fax,
        /// <summary>
        /// A person's pager.
        /// </summary>
        Pager,
        /// <summary>
        /// A person's TTY/TDD device.
        /// </summary>
        TDD,
        /// <summary>
        /// Uncategorized phone number
        /// </summary>
        Other
    }
}
