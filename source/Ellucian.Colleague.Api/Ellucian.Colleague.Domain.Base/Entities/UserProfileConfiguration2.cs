// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Institution-defined UserProfile options
    /// </summary>
    [Serializable]
    public class UserProfileConfiguration2
    {
        #region Address View/Update

        /// <summary>
        /// Boolean indicates if all address types are viewable. When set to true, ViewableAddressTypes are irrelevant.
        /// </summary>
        public bool AllAddressTypesAreViewable { get { return _allAddressTypesAreViewable; } }
        private bool _allAddressTypesAreViewable;

        ///<summary>
        ///List of address types users may view on their user profile
        ///</summary>
        public ReadOnlyCollection<string> ViewableAddressTypes { get; private set; }
        private readonly List<string> _viewableAddressTypes = new List<string>();

        /// <summary>
        /// List of address types users may update on their user profile
        /// </summary>
        public ReadOnlyCollection<string> UpdatableAddressTypes { get; private set; }
        private readonly List<string> _updatableAddressTypes = new List<string>();

        /// <summary>
        /// List of "web" address types that can be used for address change requests -- such as with graduation application "Request this address going forward"
        /// For address requests, we are only allowing web address type updates from the list of updatable address types defined on WUPP.
        /// </summary>
        public ReadOnlyCollection<string> ChangeRequestAddressTypes { get; private set; }
        private readonly List<string> _changeRequestAddressTypes = new List<string>();

        /// <summary>
        /// Indicates if user can update address without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdateAddressWithoutPermission { get; set; }

        /// <summary>
        /// Configuration cannot have flag indicating all address types viewable set to true and a list of
        /// address types, only one or the other is allowed.
        /// </summary>
        /// <param name="allAddressTypesAreViewable">Indicates that all addresses are viewable</param>
        /// <param name="viewableAddressTypes">ADREL.TYPES indentified as being viewable</param>
        /// <param name="updatableAddressTypes">ADREL.TYPEs identified as being updatable. They must also be in the viewable list</param>
        /// <param name="allAdrelTypes">All address types - used to determine which are of what type</param>
        public void UpdateAddressTypeConfiguration(bool allAddressTypesAreViewable, List<string> viewableAddressTypes, List<string> updatableAddressTypes, List<AddressRelationType> allAdrelTypes = null)
        {
            if (allAddressTypesAreViewable && viewableAddressTypes != null && viewableAddressTypes.Count() > 0)
                throw new ArgumentException("List of viewable address types must be empty if all address types viewable is true");

            _allAddressTypesAreViewable = allAddressTypesAreViewable;
            if (viewableAddressTypes != null && viewableAddressTypes.Count() > 0)
            {
                foreach (var addressType in viewableAddressTypes)
                {
                    if (!string.IsNullOrEmpty(addressType))
                    {
                        if (_viewableAddressTypes.Where(t => t == addressType).FirstOrDefault() == null)
                        {
                            _viewableAddressTypes.Add(addressType);
                        }
                    }
                }
            }
            List<string> webAdrelTypes = null;
            if (allAdrelTypes != null && allAdrelTypes.Any())
            {
                webAdrelTypes = allAdrelTypes.Where(adt => adt.SpecialProcessingAction1 == "3").Select(c => c.Code).ToList();
            }
            
            // Prevent adding updatable addresses that are not also viewable
            if (updatableAddressTypes != null && updatableAddressTypes.Count() > 0)
            {
                foreach (var addressType in updatableAddressTypes)
                {
                    if (!viewableAddressTypes.Contains(addressType))
                    {
                        throw new ArgumentException("Cannot add updatable address type that is not also viewable: " + addressType);
                    }
                    if (!string.IsNullOrEmpty(addressType))
                    {
                        if (_updatableAddressTypes.Where(t => t == addressType).FirstOrDefault() == null)
                        {
                            _updatableAddressTypes.Add(addressType);

                            // Can it also be added to the changeRequestAddressTypes list?
                            if (webAdrelTypes != null && webAdrelTypes.Any())
                            {
                                //Determine if this type is a webaddress type and if so it can also be used as an address change request address type.
                                if (webAdrelTypes.Contains(addressType))
                                {
                                    _changeRequestAddressTypes.Add(addressType);
                                }
                            }
                        }
                    }
                }
            }

        }

        #endregion Address View/Update

        #region Email View/Update

        /// <summary>
        /// Boolean indicates if all email types are viewable. When set to true, ViewableEmailTypes are irrelevant.
        /// </summary>
        public bool AllEmailTypesAreViewable { get { return _allEmailTypesAreViewable; } }
        private bool _allEmailTypesAreViewable;

        /// <summary>
        /// List of email types users may view on their user profile
        /// </summary>
        public ReadOnlyCollection<string> ViewableEmailTypes { get; private set; }
        private readonly List<string> _viewableEmailTypes = new List<string>();

        /// <summary>
        /// Boolean indicates if all email types are updatable. When set to true, UpdatableEmailTypes are irrelevant.
        /// Cannot be true unless AllEmailTypesAreViewable is also true.
        /// </summary>
        public bool AllEmailTypesAreUpdatable { get { return _allEmailTypesAreUpdatable; } }
        private bool _allEmailTypesAreUpdatable;

        /// <summary>
        /// List of email types users may update on their user profile
        /// </summary>
        public ReadOnlyCollection<string> UpdatableEmailTypes { get; private set; }
        private readonly List<string> _updatableEmailTypes = new List<string>();

        /// <summary>
        /// Indicates if user can update emails without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdateEmailWithoutPermission { get; set; }

        /// <summary>
        /// Configuration cannot have flag indicating all email types viewable set to true and a list of
        /// email types, only one or the other is allowed. Same for updatable email types. Also enforces that
        /// all updatable email types are also viewable.
        /// </summary>
        /// <param name="allEmailTypesAreViewable"></param>
        /// <param name="viewableEmailTypes"></param>
        /// <param name="allEmailTypesAreUpdatable"></param>
        /// <param name="updatableEmailTypes"></param>
        public void UpdateEmailTypeConfiguration(bool allEmailTypesAreViewable, List<string> viewableEmailTypes, bool allEmailTypesAreUpdatable, List<string> updatableEmailTypes)
        {
            if (allEmailTypesAreViewable && viewableEmailTypes != null && viewableEmailTypes.Count() > 0)
                throw new ArgumentException("List of viewable email types must be empty if all email types viewable is true");
            if (!allEmailTypesAreViewable && allEmailTypesAreUpdatable)
                throw new ArgumentException("All email types updatable cannot be true if All email types viewable is false");

            _allEmailTypesAreViewable = allEmailTypesAreViewable;
            if (viewableEmailTypes != null && viewableEmailTypes.Count() > 0)
            {
                foreach (var emailType in viewableEmailTypes)
                {
                    if (!string.IsNullOrEmpty(emailType))
                    {
                        if (_viewableEmailTypes.Where(t => t == emailType).FirstOrDefault() == null)
                        {
                            _viewableEmailTypes.Add(emailType);
                        }
                    }
                }
            }

            _allEmailTypesAreUpdatable = allEmailTypesAreUpdatable;
            if (updatableEmailTypes != null && updatableEmailTypes.Count() > 0)
            {
                foreach (var emailType in updatableEmailTypes)
                {
                    if (!viewableEmailTypes.Contains(emailType))
                    {
                        throw new ArgumentException("Cannot add updatable email type that is not also viewable: " + emailType);
                    }
                    if (!string.IsNullOrEmpty(emailType))
                    {
                        if (_updatableEmailTypes.Where(t => t == emailType).FirstOrDefault() == null)
                        {
                            _updatableEmailTypes.Add(emailType);
                        }
                    }
                }
            }
        }

        #endregion Email View/Update

        #region Phone Vew/Update

        /// <summary>
        /// Boolean indicates if all phone types are viewable. When set to true, ViewablePhoneTypes are irrelevant.
        /// </summary>
        public bool AllPhoneTypesAreViewable { get { return _allPhoneTypesAreViewable; } }
        private bool _allPhoneTypesAreViewable;

        /// <summary>
        /// List of phone types users may view on their user profile
        /// </summary>
        public ReadOnlyCollection<string> ViewablePhoneTypes { get; private set; }
        private readonly List<string> _viewablePhoneTypes = new List<string>();

        /// <summary>
        /// List of phone types users may update on their user profile
        /// </summary>
        public ReadOnlyCollection<string> UpdatablePhoneTypes { get; private set; }
        private readonly List<string> _updatablePhoneTypes = new List<string>();

        /// <summary>
        /// Indicates if user can update phones without role permissions. Defaults to false.
        /// </summary>
        public bool CanUpdatePhoneWithoutPermission { get; set; }

        /// <summary>
        /// Indicates if user can view phones without types (i.e. null type)
        /// </summary>
        public bool CanViewPhonesWithoutTypes { get { return _canViewPhonesWithoutTypes; } }
        private bool _canViewPhonesWithoutTypes;

        /// <summary>
        /// Configuration cannot have flag indicating all phone types viewable set to true and a list of
        /// phone types, only one or the other is allowed.
        /// </summary>
        /// <param name="allPhoneTypesAreViewable"></param>
        /// <param name="viewablePhoneTypes"></param>
        /// <param name="updatablePhoneTypes"></param>
        public void UpdatePhoneTypeConfiguration(bool allPhoneTypesAreViewable, List<string> viewablePhoneTypes, List<string> updatablePhoneTypes, bool canViewPhonesWithoutTypes)
        {
            if (allPhoneTypesAreViewable && viewablePhoneTypes != null && viewablePhoneTypes.Count() > 0)
                throw new ArgumentException("List of viewable phone types must be empty if all phone types viewable is true");

            if (allPhoneTypesAreViewable && updatablePhoneTypes != null && updatablePhoneTypes.Count() > 0)
                throw new ArgumentException("List of updatable phone types must be empty if all phone types viewable is true");

            _allPhoneTypesAreViewable = allPhoneTypesAreViewable;
            _canViewPhonesWithoutTypes = canViewPhonesWithoutTypes;
            if (viewablePhoneTypes != null && viewablePhoneTypes.Count() > 0)
            {
                foreach (var phoneType in viewablePhoneTypes)
                {
                    if (!string.IsNullOrEmpty(phoneType))
                    {
                        if (_viewablePhoneTypes.Where(t => t == phoneType).FirstOrDefault() == null)
                        {
                            _viewablePhoneTypes.Add(phoneType);
                        }
                    }
                }
            }

            if (updatablePhoneTypes != null && updatablePhoneTypes.Count() > 0)
            {
                foreach (var phoneType in updatablePhoneTypes)
                {
                    if (!viewablePhoneTypes.Contains(phoneType))
                    {
                        throw new ArgumentException("Cannot add updatable phone type that is not also viewable: " + phoneType);
                    }
                    if (!string.IsNullOrEmpty(phoneType))
                    {
                        if (_updatablePhoneTypes.Where(t => t == phoneType).FirstOrDefault() == null)
                        {
                            _updatablePhoneTypes.Add(phoneType);
                        }
                    }
                }
            }
        }

        #endregion Phone Vew/Update

        /// <summary>
        /// Institutional message to display to the User Profile form user
        /// </summary>
        public string Text { get; set; }

        #region Identity View/Update

        /// <summary>
        /// Indicates if user can view/update their chosen name
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateChosenName { get; set; }

        /// <summary>
        /// Indicates if user can view/update their nickname
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateNickname { get; set; }

        /// <summary>
        /// Indicates if user can view/update their pronoun
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdatePronoun { get; set; }

        /// <summary>
        /// Indicates if user can view/update their gender identity
        /// </summary>
        public UserProfileViewUpdateOption CanViewUpdateGenderIdentity { get; set; }

        /// <summary>
        /// UserProfileConfiguration2 constructor. Only exposes readonly lists
        /// </summary>
        public UserProfileConfiguration2()
        {
            ViewableAddressTypes = _viewableAddressTypes.AsReadOnly();
            UpdatableAddressTypes = _updatableAddressTypes.AsReadOnly();
            ViewableEmailTypes = _viewableEmailTypes.AsReadOnly();
            UpdatableEmailTypes = _updatableEmailTypes.AsReadOnly();
            ViewablePhoneTypes = _viewablePhoneTypes.AsReadOnly();
            UpdatablePhoneTypes = _updatablePhoneTypes.AsReadOnly();
            ChangeRequestAddressTypes = _changeRequestAddressTypes.AsReadOnly();

        }
        #endregion
    }
}
