// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of a type of address information for a person
    /// </summary>
    [Serializable]
    public enum PersonAddressType
    {
        /// <summary>
        /// A address where a person primarily lives, their permanent address.
        /// </summary>
        Home,
        /// <summary>
        /// A address where a person lives while at school.
        /// </summary>
        School,
        /// <summary>
        /// A address where a person lives while on vacation.
        /// </summary>
        Vacation,
        /// <summary>
        /// A address where a person's bills would be sent.
        /// </summary>
        Billing,
        /// <summary>
        /// A address where a person's deliveries would be sent.
        /// </summary>
        Shipping,
        /// <summary>
        /// A address where a person's mail would be sent.
        /// </summary>
        Mailing,
        /// <summary>
        /// A address where a person works
        /// </summary>
        Business,
        /// <summary>
        /// Uncategorized person address type.
        /// </summary>
        Other
    }
}
