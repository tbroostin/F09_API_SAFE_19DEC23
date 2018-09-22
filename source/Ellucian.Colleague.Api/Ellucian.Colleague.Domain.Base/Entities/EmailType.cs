// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Ethnicities
    /// </summary>
    [Serializable]
    public class EmailType : GuidCodeItem
    {
        /// <summary>
        /// The <see cref="EmailTypeCategory">type</see> of email address for this entity
        /// </summary>
        private EmailTypeCategory _emailTypeCategory;
        public EmailTypeCategory EmailTypeCategory { get { return _emailTypeCategory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the Email type item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="personEmailType">The related Person Email Type</param>
        public EmailType(string guid, string code, string description, EmailTypeCategory emailType)
            : base(guid, code, description)
        {
            _emailTypeCategory = emailType;
        }
    }
}
