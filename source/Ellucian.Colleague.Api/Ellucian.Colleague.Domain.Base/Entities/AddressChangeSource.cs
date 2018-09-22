// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Address Change Source
    /// </summary>
    [Serializable]
    public class AddressChangeSource : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressChangeSource"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the AddressChangeSource</param>
        /// <param name="description">Description or Title of the RAddressChangeSource</param>
        public AddressChangeSource(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}