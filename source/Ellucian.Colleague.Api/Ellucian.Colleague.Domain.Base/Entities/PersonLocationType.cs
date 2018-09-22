// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of a type of location information for a person
    /// </summary>
    [Serializable]
    public enum PersonLocationType
    {
        /// <summary>
        /// A location where a person primarily lives, their permanent address.
        /// </summary>
        Home,
        /// <summary>
        /// A location where a person lives while at school.
        /// </summary>
        School,
        /// <summary>
        /// A location where a person lives while on vacation.
        /// </summary>
        Vacation,
        /// <summary>
        /// A location where a person's bills would be sent.
        /// </summary>
        Billing,
        /// <summary>
        /// A location where a person's deliveries would be sent.
        /// </summary>
        Shipping,
        /// <summary>
        /// A location where a person's mail would be sent.
        /// </summary>
        Mailing,
        /// <summary>
        /// A location where a person works
        /// </summary>
        Business,
        /// <summary>
        /// Uncategorized person location type.
        /// </summary>
        Other
    }
}
