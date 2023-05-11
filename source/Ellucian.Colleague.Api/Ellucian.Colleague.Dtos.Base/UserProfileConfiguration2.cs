﻿// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration of User Profile information user access.
    /// </summary>
    public class UserProfileConfiguration2
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
        /// Indicates if user can view phones without types.
        /// </summary>
        public bool CanViewPhonesWithoutTypes { get; set; }

        /// <summary>
        /// Indicates if user can view or update their chosen name
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateChosenName { get; set; }

        /// <summary>
        /// Indicates if user can view or update their nickname
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateNickname { get; set; }

        /// <summary>
        /// Indicates if user can view or update their pronoun
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdatePronoun { get; set; }

        /// <summary>
        /// Indicates if user can view or update their gender identity
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateGenderIdentity { get; set; }

        /// <summary>
        /// List of "web obtained" type of addresses that may be used for Address Change Requests. 
        /// An example is the Graduation Application option "Request this be my new address going forward"
        /// </summary>
        public List<string> ChangeRequestAddressTypes { get; set; }


        /// <summary>
        /// Indicates whether institution wishes to allow the self-service user to authorize a personal phone number for text
        /// </summary>
        public bool? AuthorizePhonesForText { get; set; }
    }
}
