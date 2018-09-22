// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Address Types
    /// </summary>
    [Serializable]
    public class AddressType2 : GuidCodeItem
    {
        /// <summary>
        /// The <see cref="AddressTypeCategory">type</see> of address type for this entity
        /// </summary>
        private AddressTypeCategory _addressTypeCategory;
        public AddressTypeCategory AddressTypeCategory { get { return _addressTypeCategory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressType2"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the Address type item</param>
        /// /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="addressTypeCategory">The related Person Address Type</param>
        public AddressType2(string guid, string code, string description, AddressTypeCategory addressTypeCategory)
            : base(guid, code, description)
        {
            _addressTypeCategory = addressTypeCategory;
        }
    }
}
