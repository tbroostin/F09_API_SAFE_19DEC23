// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration of User Profile information user access.
    /// </summary>
    public class UserProfileConfiguration
    {
        /// <summary>
        /// List of types of addresses the user may view. Will not have any values if AllAddressTypesAreViewable is true.
        /// </summary>
        public List<string> ViewableAddressTypes { get; set; }

        /// <summary>
        /// List of type of emails the user may view. Will not have any values if AllEmailTypesAreViewable is true.
        /// </summary>
        public List<string> ViewableEmailTypes { get; set; }

        /// <summary>
        /// List of the types of phones the user may view.
        /// </summary>
        public List<string> ViewablePhoneTypes { get; set; }

        /// <summary>
        /// Text to display to the user related to the user profile information displayed.
        /// </summary>
        public string Text  { get; set; }

        /// <summary>
        /// Boolean indicates if all address types are viewable. 
        /// </summary>
        public bool AllAddressTypesAreViewable { get; set; }

        /// <summary>
        /// Boolean indicates if all email types are viewable. 
        /// </summary>
        public bool AllEmailTypesAreViewable { get; set; }

        /// <summary>
        /// Boolean indicates if all phone types are viewable. 
        /// </summary>
        public bool AllPhoneTypesAreViewable { get; set; }

        /// <summary>
        /// Boolean indicates if all email types are updatable. Can be true only if AllEmailTypesAreViewable is also true.
        /// </summary>
        public bool AllEmailTypesAreUpdatable { get; set; }

        /// <summary>
        /// List of email types users may update on their user profile
        /// </summary>
        public List<string> UpdatableEmailTypes { get; set; }

        /// <summary>
        /// Indicates if user can update emails without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdateEmailWithoutPermission { get; set; }

        /// <summary>
        /// List of phone types users may update on their user profile
        /// </summary>
        public List<string> UpdatablePhoneTypes { get; set; }

        /// <summary>
        /// Indicates if user can update phones without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdatePhoneWithoutPermission { get; set; }

        /// <summary>
        /// List of address types users may update on their user profile
        /// </summary>
        public List<string> UpdatableAddressTypes { get; set; }

        /// <summary>
        /// Indicates if user can update address without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdateAddressWithoutPermission { get; set; }

        /// <summary>
        /// Boolean indicates if addresses are updatable.
        /// </summary>
        public bool AddressesAreUpdatable { get; set; }
    }
}
