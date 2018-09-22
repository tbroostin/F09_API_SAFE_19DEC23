// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an email address
    /// </summary>
    [Serializable]
    public enum EmailTypeCategory
    {
        /// <summary>
        /// A person's/organization's personal email address.
        /// </summary>
        Personal,
        /// <summary>
        /// A person's/organization's business email address.
        /// </summary>
        Business,
        /// <summary>
        /// A person's/organization's school email address.
        /// </summary>
        School,
        /// <summary>
        /// A person's/organization's parent email address.
        /// </summary>
        Parent,
        /// <summary>
        /// A person's/organization's family email address.
        /// </summary>
        Family,
        /// <summary>
        /// An person's/organization's email address for sales inquiries.
        /// </summary>
        Sales,
        /// <summary>
        /// A person's/organization's email address for support inquiries.
        /// </summary>
        Support,
        /// <summary>
        /// A person's/organization's email address for general inquiries.
        /// </summary>
        General,
        /// <summary>
        /// A person's/organization's email address for billing inquiries.
        /// </summary>
        Billing,
        /// <summary>
        /// A person's/organization's email address for legal inquiries.
        /// </summary>
        Legal,
        /// <summary>
        /// A person's/organization's email address for HR inquiries.
        /// </summary>
        HR,
        /// <summary>
        /// A person's/organization's email address for media inquiries.
        /// </summary>
        Media,
        /// <summary>
        /// A person's/organization's email address for matching gifts inquiries.
        /// </summary>
        MatchingGifts,
        /// <summary>
        /// Uncategorized email address.
        /// </summary>
        Other
    }
}
