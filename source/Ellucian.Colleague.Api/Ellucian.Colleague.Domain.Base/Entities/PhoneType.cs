// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// PhoneType
    /// </summary>
    [Serializable]
    public class PhoneType : GuidCodeItem
    {
        /// <summary>
        /// The <see cref="PhoneTypeCategory">type</see> of phone type for this entity
        /// </summary>
        private PhoneTypeCategory _phoneTypeCategory;
        public PhoneTypeCategory PhoneTypeCategory { get { return _phoneTypeCategory; } }

        /// <summary>
        /// Phone number is associated to the person rather than a person's address (i.e. cell phone). These types of phones can be authorized for text messages.
        /// This is determined by the PHONE.TYPES table special processing code 1 (value P).
        /// </summary>
        private bool _isPersonalType;
        public bool IsPersonalType { get { return _isPersonalType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneType"/> class.
        /// </summary>
        /// <param name="phoneTypeCategory">The phone type</param>
        public PhoneType(string guid, string code, string description, PhoneTypeCategory phoneTypeCategory, bool isPersonalType)
            : base(guid, code, description)
        {
            _phoneTypeCategory = phoneTypeCategory;
            _isPersonalType = isPersonalType;
        }
    }
}
