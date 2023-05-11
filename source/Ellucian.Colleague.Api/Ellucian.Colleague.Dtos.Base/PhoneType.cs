// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Phone type
    /// </summary>
    public class PhoneType
    {
        /// <summary>
        /// Phone type code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// User-readable phone type description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Phone number is associated to the person rather than a person's address (i.e. cell phone). These types of phones can be authorized for text messages.
        /// </summary>
        public bool IsPersonalType { get; set; }
    }
}
