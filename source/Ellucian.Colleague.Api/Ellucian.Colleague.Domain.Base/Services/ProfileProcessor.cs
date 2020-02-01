// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Domain.Base.Services
{
    /// <summary>
    /// Domain-type processing of person profile data.
    /// </summary>
    public static class ProfileProcessor
    {
        private static ILogger _logger = null;

        /// <summary>
        /// Initialize the logger for this class to use.  Note that only the logger first used
        /// to initialize will be used; subsequent calls will be ignored.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void InitializeLogger(ILogger logger)
        {
            _logger = logger;
        }

        #region Profile processor servicing versions prior to API 1.16 with UserProfileConfiguration
        /// <summary>
        /// Compares the incoming profile information against the profile data from the repository and reconciles the two before updating.
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="repoProfile"></param>
        /// <returns>A Profile object ready for update. Includes only the entity properties that are updated.</returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this method instead.")]
        public static Profile VerifyProfileUpdate(Profile newProfile, Profile repoProfile, UserProfileConfiguration configuration, ICurrentUser currentUser, IEnumerable<string> userPermissions, out bool isProfileChanged)
        {
            // VERIFY ARGUMENTS
            if (newProfile == null)
            {
                string message = "NewProfile argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("newProfile", message);
            }

            if (repoProfile == null)
            {
                string message = "RepoProfile argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("repoProfile", message);
            }

            // Must have configuration for correct verification
            if (configuration == null)
            {
                string message = "User Profile Configuration argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("configuration", message);
            }

            // Must have current user
            if (currentUser == null)
            {
                string message = "Current User argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("currentUser", message);
            }

            // User may have no permissions, but at least expect an empty list.
            if (userPermissions == null)
            {
                string message = "User Permissions argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("userPermissions", message);
            }

            // Users can change their own profile only.
            if (!currentUser.IsPerson(newProfile.Id))
                throw new PermissionsException("Profile ID " + newProfile.Id + " does not match current user person id.");

            // Verify the correct repo item was retrieved and passed 
            if (newProfile.Id != repoProfile.Id)
            {
                string message = "Repository item not for the same user. Repo id: " + repoProfile.Id + " Update id: " + newProfile.Id;
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }

            // Make sure a modification has not been made to this person in the repository since user has accessed the person record.
            if (newProfile.LastChangedDateTime != repoProfile.LastChangedDateTime)
            {
                string message = "Person data has been updated by another source. Cannot make requested changes. Please try again";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }

            // Set this boolean to true if any changes found to profile.
            isProfileChanged = false;
            // Accumulated list of informational messages to put out to the log regarding the updates detected -- only if validation is successful
            List<string> logMessages = new List<string>();

            // Create a Profile object for output. It will contain only the items needed for update. 
            Profile verifiedProfile = new Profile(repoProfile.Id, repoProfile.LastName);



            // EMAIL UPDATE VERIFICATION
            foreach (var email in newProfile.EmailAddresses)
            {
                var repoEmailMatch = repoProfile.EmailAddresses.Where(rp => rp.Equals(email)).FirstOrDefault();
                if (repoEmailMatch == null)
                {
                    // No exact match for this email found on the repo record. Verify that configuration and permissions allow for it to be updated. 
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyEmailUpdateAllowed(configuration, userPermissions, email.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Incoming changed email address: " + email.Value + " type: " + email.TypeCode + "preferred: " + email.IsPreferred.ToString());
                }
                // Exception has not occurred, Add email to verified profile for update.
                verifiedProfile.AddEmailAddress(email);
            }

            // Check for anything in the repo record email addresses that is missing
            foreach (var email in repoProfile.EmailAddresses)
            {
                var incomingEmailMatch = verifiedProfile.EmailAddresses.Where(p => p.Equals(email)).FirstOrDefault();
                if (incomingEmailMatch == null)
                {
                    // No exact match for this repo email in the incoming list. Verify that configuration and permissions allow for it to be updated.
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyEmailUpdateAllowed(configuration, userPermissions, email.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo changed email address: " + email.Value + " type: " + email.TypeCode + "preferred: " + email.IsPreferred.ToString());
                }
            }


            // PHONE UPDATE VERIFICATION
            foreach (var phone in newProfile.Phones)
            {
                var repoPhoneMatch = repoProfile.Phones.Where(x => x.Equals(phone)).FirstOrDefault();
                if (repoPhoneMatch == null)
                {
                    // No exact match for this phone found on the repo record. Verify that configuration and permissions allow for it to be updated. 
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyPhoneUpdateAllowed(configuration, userPermissions, phone.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Incoming changed phone number: " + phone.Number + " type: " + phone.TypeCode + "extension: " + phone.Extension);
                }
                // Exception has not occurred, Add phone to verified profile for update.
                verifiedProfile.AddPhone(phone);
            }

            // Check for anything in the repo record phones that are missing
            foreach (var phone in repoProfile.Phones)
            {
                var incomingPhoneMatch = verifiedProfile.Phones.Where(x => x.Equals(phone)).FirstOrDefault();
                if (incomingPhoneMatch == null)
                {
                    // No exact match for this repo phone in the incoming list. Verify that configuration and permissions allow for it to be updated.
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyPhoneUpdateAllowed(configuration, userPermissions, phone.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo changed phone number: " + phone.Number + " type: " + phone.TypeCode + "extension: " + phone.Extension);
                }
            }


            // ADDRESS UPDATE VERIFICATION
            foreach (var address in newProfile.Addresses)
            {
                Address repoAddressMatch = null;
                if (!string.IsNullOrEmpty(address.AddressId))
                {
                    repoAddressMatch = repoProfile.Addresses.Where(x => x.AddressId == address.AddressId).FirstOrDefault();
                }
                // If the address has no ID, it must be an added address. Or if it comes in with an ID and doesn't match the address from
                // the repo, then we have an address update request. Either way, we must determine if the address can be added/updated.
                if (string.IsNullOrEmpty(address.AddressId) || repoAddressMatch == null || !address.Equals(repoAddressMatch))
                {
                    // Split the (possibly) multiple address type codes stuffed into this one field.
                    var addressTypes = address.TypeCode.Split(',').Select(t => t.Trim()).ToList();
                    // Verify that the address can be updated, based on address types and user permissions.
                    VerifyAddressUpdateAllowed(configuration, userPermissions, addressTypes);
                    // If not thrown out, add the address to the profile for update
                    verifiedProfile.AddAddress(address);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo address change request submitted: AddressLines: " + address.AddressLines.ElementAt(0) + " City: " + address.City + " State: " + address.State + " PostalCode: " + address.PostalCode + " AddressType: " + addressTypes.ElementAt(0));
                }
            }

            // CONFIRMATIONS VERIFICATION
            if (newProfile.AddressConfirmationDateTime == null && repoProfile.AddressConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing address confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.EmailAddressConfirmationDateTime == null && repoProfile.EmailAddressConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing email confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.PhoneConfirmationDateTime == null && repoProfile.PhoneConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing phone confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.AddressConfirmationDateTime != repoProfile.AddressConfirmationDateTime ||
                newProfile.EmailAddressConfirmationDateTime != repoProfile.EmailAddressConfirmationDateTime ||
                newProfile.PhoneConfirmationDateTime != repoProfile.PhoneConfirmationDateTime)
            {
                isProfileChanged = true;
            }
            verifiedProfile.AddressConfirmationDateTime = newProfile.AddressConfirmationDateTime;
            verifiedProfile.EmailAddressConfirmationDateTime = newProfile.EmailAddressConfirmationDateTime;
            verifiedProfile.PhoneConfirmationDateTime = newProfile.PhoneConfirmationDateTime;


            // WRAPUP
            // Last changed date/time needs to be checked again at the moment of database update.
            verifiedProfile.LastChangedDateTime = newProfile.LastChangedDateTime;


            // Log verification messages only if all verification was successful and changes found.
            if (isProfileChanged)
            {
                if (_logger.IsDebugEnabled)
                {
                    foreach (var msg in logMessages)
                    {
                        _logger.Debug(msg);
                    }
                }
            }
            else
            {
                _logger.Debug("No changes found in Profile.");
            }

            return verifiedProfile;
        }

        // Checks user permissions and configuration to make sure user has permission to change the email.
        private static bool VerifyEmailUpdateAllowed(UserProfileConfiguration configuration, IEnumerable<string> userPermissions, string emailType)
        {
            // There is not an exact match found. Verify permissions.
            if (HasEmailPermissions(configuration, userPermissions))
            {
                if (!IsUpdatableEmailType(configuration, emailType))
                {
                    var message = "Update failed: Configuration does not allow update of email type " + emailType;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update emails.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update emails (permissions may not be required)
        private static bool HasEmailPermissions(UserProfileConfiguration configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdateEmailWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnEmail))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given email type can be updated, based on configured email types
        private static bool IsUpdatableEmailType(UserProfileConfiguration configuration, string emailType)
        {
            if (configuration == null || emailType == null)
            {
                return false;
            }
            if (configuration.AllEmailTypesAreUpdatable)
            {
                return true;
            }
            else
            {
                if (configuration.UpdatableEmailTypes.Contains(emailType))
                {
                    return true;
                }
            }
            return false;
        }

        // Checks user permissions and configuration to make sure user has permission to change the phone.
        private static bool VerifyPhoneUpdateAllowed(UserProfileConfiguration configuration, IEnumerable<string> userPermissions, string phoneType)
        {
            // There is not an exact match found. Verify permissions.
            if (HasPhonePermissions(configuration, userPermissions))
            {
                if (!IsUpdatablePhoneType(configuration, phoneType))
                {
                    var message = "Update failed: Configuration does not allow update of phone type " + phoneType;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update phones.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update personal phones (permissions may not be required)
        private static bool HasPhonePermissions(UserProfileConfiguration configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdatePhoneWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnPhone))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given phone type can be updated, based on configured phone types
        private static bool IsUpdatablePhoneType(UserProfileConfiguration configuration, string phoneType)
        {
            if (configuration == null || phoneType == null)
            {
                return false;
            }
            else
            {
                if (configuration.UpdatablePhoneTypes.Contains(phoneType))
                {
                    return true;
                }
            }
            return false;
        }


        // Checks user permissions and configuration to make sure user has permission to change the address.
        private static bool VerifyAddressUpdateAllowed(UserProfileConfiguration configuration, IEnumerable<string> userPermissions, List<string> addressTypes)
        {
            // There is not an exact match found. Verify permissions.
            if (HasAddressPermissions(configuration, userPermissions))
            {
                if (!IsUpdatableAddressType(configuration, addressTypes))
                {
                    var message = "Update failed: Configuration does not allow update of any of these address types " + addressTypes;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update addresses.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update addresses (permissions may not be required)
        private static bool HasAddressPermissions(UserProfileConfiguration configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdateAddressWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnAddress))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given address type can be updated, based on configured address types.
        private static bool IsUpdatableAddressType(UserProfileConfiguration configuration, List<string> addressTypes)
        {
            if (configuration == null || addressTypes == null || addressTypes.Count() == 0)
            {
                return false;
            }
            else
            {
                if (configuration.UpdatableAddressTypes.Intersect(addressTypes).Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Profile processor servicing versions 1.16 and later with UserProfileConfiguration2
        /// <summary>
        /// Compares the incoming profile information against the profile data from the repository and reconciles the two before updating.
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="repoProfile"></param>
        /// <param name="configuration"></param>
        /// <param name="currentUser"></param>
        /// <param name="userPermissions"></param>
        /// <param name="isProfileChanged"></param>
        /// <returns>A Profile object ready for update. Includes only the entity properties that are updated.</returns>
        public static Profile VerifyProfileUpdate2(Profile newProfile, Profile repoProfile, UserProfileConfiguration2 configuration, ICurrentUser currentUser, IEnumerable<string> userPermissions, out bool isProfileChanged)
        {
            // VERIFY ARGUMENTS
            if (newProfile == null)
            {
                string message = "NewProfile argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("newProfile", message);
            }

            if (repoProfile == null)
            {
                string message = "RepoProfile argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("repoProfile", message);
            }

            // Must have configuration for correct verification
            if (configuration == null)
            {
                string message = "User Profile Configuration argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("configuration", message);
            }

            // Must have current user
            if (currentUser == null)
            {
                string message = "Current User argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("currentUser", message);
            }

            // User may have no permissions, but at least expect an empty list.
            if (userPermissions == null)
            {
                string message = "User Permissions argument is required";
                _logger.Error(message);
                throw new ArgumentNullException("userPermissions", message);
            }

            // Users can change their own profile only.
            if (!currentUser.IsPerson(newProfile.Id))
                throw new PermissionsException("Profile ID " + newProfile.Id + " does not match current user person id.");

            // Verify the correct repo item was retrieved and passed 
            if (newProfile.Id != repoProfile.Id)
            {
                string message = "Repository item not for the same user. Repo id: " + repoProfile.Id + " Update id: " + newProfile.Id;
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }

            // Make sure a modification has not been made to this person in the repository since user has accessed the person record.
            if (newProfile.LastChangedDateTime != repoProfile.LastChangedDateTime)
            {
                string message = "Person data has been updated by another source. Cannot make requested changes. Please try again";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }

            // Set this boolean to true if any changes found to profile.
            isProfileChanged = false;
            // Accumulated list of informational messages to put out to the log regarding the updates detected -- only if validation is successful
            List<string> logMessages = new List<string>();

            // Create a Profile object for output. It will contain only the items needed for update. 
            Profile verifiedProfile = new Profile(repoProfile.Id, repoProfile.LastName);



            // EMAIL UPDATE VERIFICATION
            foreach (var email in newProfile.EmailAddresses)
            {
                var repoEmailMatch = repoProfile.EmailAddresses.Where(rp => rp.Equals(email)).FirstOrDefault();
                if (repoEmailMatch == null)
                {
                    // No exact match for this email found on the repo record. Verify that configuration and permissions allow for it to be updated. 
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyEmailUpdateAllowed(configuration, userPermissions, email.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Incoming changed email address: " + email.Value + " type: " + email.TypeCode + "preferred: " + email.IsPreferred.ToString());
                }
                // Exception has not occurred, Add email to verified profile for update.
                verifiedProfile.AddEmailAddress(email);
            }

            // Check for anything in the repo record email addresses that is missing
            foreach (var email in repoProfile.EmailAddresses)
            {
                var incomingEmailMatch = verifiedProfile.EmailAddresses.Where(p => p.Equals(email)).FirstOrDefault();
                if (incomingEmailMatch == null)
                {
                    // No exact match for this repo email in the incoming list. Verify that configuration and permissions allow for it to be updated.
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyEmailUpdateAllowed(configuration, userPermissions, email.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo changed email address: " + email.Value + " type: " + email.TypeCode + "preferred: " + email.IsPreferred.ToString());
                }
            }


            // PHONE UPDATE VERIFICATION
            foreach (var phone in newProfile.Phones)
            {
                var repoPhoneMatch = repoProfile.Phones.Where(x => x.Equals(phone)).FirstOrDefault();
                if (repoPhoneMatch == null)
                {
                    // No exact match for this phone found on the repo record. Verify that configuration and permissions allow for it to be updated. 
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyPhoneUpdateAllowed(configuration, userPermissions, phone.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Incoming changed phone number: " + phone.Number + " type: " + phone.TypeCode + "extension: " + phone.Extension);
                }
                // Exception has not occurred, Add phone to verified profile for update.
                verifiedProfile.AddPhone(phone);
            }

            // Check for anything in the repo record phones that are missing
            foreach (var phone in repoProfile.Phones)
            {
                var incomingPhoneMatch = verifiedProfile.Phones.Where(x => x.Equals(phone)).FirstOrDefault();
                if (incomingPhoneMatch == null)
                {
                    // No exact match for this repo phone in the incoming list. Verify that configuration and permissions allow for it to be updated.
                    // Exception will be thrown if it cannot be updated. If no exception thrown, log change for informational purposes.
                    VerifyPhoneUpdateAllowed(configuration, userPermissions, phone.TypeCode);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo changed phone number: " + phone.Number + " type: " + phone.TypeCode + "extension: " + phone.Extension);
                }
            }


            // ADDRESS UPDATE VERIFICATION
            foreach (var address in newProfile.Addresses)
            {
                Address repoAddressMatch = null;
                if (!string.IsNullOrEmpty(address.AddressId))
                {
                    repoAddressMatch = repoProfile.Addresses.Where(x => x.AddressId == address.AddressId).FirstOrDefault();
                }
                // If the address has no ID, it must be an added address. Or if it comes in with an ID and doesn't match the address from
                // the repo, then we have an address update request. Either way, we must determine if the address can be added/updated.
                if (string.IsNullOrEmpty(address.AddressId) || repoAddressMatch == null || !address.Equals(repoAddressMatch))
                {
                    // Split the (possibly) multiple address type codes stuffed into this one field.
                    var addressTypes = address.TypeCode.Split(',').Select(t => t.Trim()).ToList();
                    // Verify that the address can be updated, based on address types and user permissions.
                    VerifyAddressUpdateAllowed(configuration, userPermissions, addressTypes);
                    // If not thrown out, add the address to the profile for update
                    verifiedProfile.AddAddress(address);
                    isProfileChanged = true;
                    if (_logger.IsDebugEnabled) logMessages.Add("Profile update person ID: " + newProfile.Id + "; Repo address change request submitted: AddressLines: " + address.AddressLines.ElementAt(0) + " City: " + address.City + " State: " + address.State + " PostalCode: " + address.PostalCode + " AddressType: " + addressTypes.ElementAt(0));
                }
            }

            // CONFIRMATIONS VERIFICATION
            if (newProfile.AddressConfirmationDateTime == null && repoProfile.AddressConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing address confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.EmailAddressConfirmationDateTime == null && repoProfile.EmailAddressConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing email confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.PhoneConfirmationDateTime == null && repoProfile.PhoneConfirmationDateTime != null)
            {
                var message = "Cannot overwrite existing phone confirmation date/time with null";
                _logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            if (newProfile.AddressConfirmationDateTime != repoProfile.AddressConfirmationDateTime ||
                newProfile.EmailAddressConfirmationDateTime != repoProfile.EmailAddressConfirmationDateTime ||
                newProfile.PhoneConfirmationDateTime != repoProfile.PhoneConfirmationDateTime)
            {
                isProfileChanged = true;
            }
            verifiedProfile.AddressConfirmationDateTime = newProfile.AddressConfirmationDateTime;
            verifiedProfile.EmailAddressConfirmationDateTime = newProfile.EmailAddressConfirmationDateTime;
            verifiedProfile.PhoneConfirmationDateTime = newProfile.PhoneConfirmationDateTime;

            //Identity Update Verifications 

            if ((newProfile.Nickname != repoProfile.Nickname) || (newProfile.ChosenFirstName != repoProfile.ChosenFirstName) || (newProfile.ChosenMiddleName != repoProfile.ChosenMiddleName) || (newProfile.ChosenLastName != repoProfile.ChosenLastName) || (newProfile.GenderIdentityCode != repoProfile.GenderIdentityCode) || (newProfile.PersonalPronounCode != repoProfile.PersonalPronounCode))
            {
                isProfileChanged = true;
            }

            verifiedProfile.Nickname = newProfile.Nickname;
            verifiedProfile.ChosenFirstName = newProfile.ChosenFirstName;
            verifiedProfile.ChosenMiddleName = newProfile.ChosenMiddleName;
            verifiedProfile.ChosenLastName = newProfile.ChosenLastName;
            verifiedProfile.GenderIdentityCode = newProfile.GenderIdentityCode;
            verifiedProfile.PersonalPronounCode = newProfile.PersonalPronounCode;

            // WRAPUP
            // Last changed date/time needs to be checked again at the moment of database update.
            verifiedProfile.LastChangedDateTime = newProfile.LastChangedDateTime;


            // Log verification messages only if all verification was successful and changes found.
            if (isProfileChanged)
            {
                if (_logger.IsDebugEnabled)
                {
                    foreach (var msg in logMessages)
                    {
                        _logger.Debug(msg);
                    }
                }
            }
            else
            {
                _logger.Debug("No changes found in Profile.");
            }

            return verifiedProfile;
        }

        // Checks user permissions and configuration to make sure user has permission to change the email.
        private static bool VerifyEmailUpdateAllowed(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions, string emailType)
        {
            // There is not an exact match found. Verify permissions.
            if (HasEmailPermissions(configuration, userPermissions))
            {
                if (!IsUpdatableEmailType(configuration, emailType))
                {
                    var message = "Update failed: Configuration does not allow update of email type " + emailType;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update emails.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update emails (permissions may not be required)
        private static bool HasEmailPermissions(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdateEmailWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnEmail))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given email type can be updated, based on configured email types
        private static bool IsUpdatableEmailType(UserProfileConfiguration2 configuration, string emailType)
        {
            if (configuration == null || emailType == null)
            {
                return false;
            }
            if (configuration.AllEmailTypesAreUpdatable)
            {
                return true;
            }
            else
            {
                if (configuration.UpdatableEmailTypes.Contains(emailType))
                {
                    return true;
                }
            }
            return false;
        }

        // Checks user permissions and configuration to make sure user has permission to change the phone.
        private static bool VerifyPhoneUpdateAllowed(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions, string phoneType)
        {
            // There is not an exact match found. Verify permissions.
            if (HasPhonePermissions(configuration, userPermissions))
            {
                if (!IsUpdatablePhoneType(configuration, phoneType))
                {
                    var message = "Update failed: Configuration does not allow update of phone type " + phoneType;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update phones.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update personal phones (permissions may not be required)
        private static bool HasPhonePermissions(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdatePhoneWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnPhone))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given phone type can be updated, based on configured phone types
        private static bool IsUpdatablePhoneType(UserProfileConfiguration2 configuration, string phoneType)
        {
            if (configuration == null || phoneType == null)
            {
                return false;
            }
            else
            {
                if (configuration.UpdatablePhoneTypes.Contains(phoneType))
                {
                    return true;
                }
            }
            return false;
        }


        // Checks user permissions and configuration to make sure user has permission to change the address.
        private static bool VerifyAddressUpdateAllowed(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions, List<string> addressTypes)
        {
            // There is not an exact match found. Verify permissions.
            if (HasAddressPermissions(configuration, userPermissions))
            {
                if (!IsUpdatableAddressType(configuration, addressTypes))
                {
                    var message = "Update failed: Configuration does not allow update of any of these address types " + addressTypes;
                    _logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            else
            {
                var message = "Update failed: User is not configured to update addresses.";
                _logger.Error(message);
                throw new PermissionsException(message);
            }
            return true;
        }

        // Verifies that user has the permissions needed to update addresses (permissions may not be required)
        private static bool HasAddressPermissions(UserProfileConfiguration2 configuration, IEnumerable<string> userPermissions)
        {
            if (configuration == null || userPermissions == null)
            {
                return false;
            }
            if (configuration.CanUpdateAddressWithoutPermission)
            {
                return true;
            }
            else
            {
                if (userPermissions.Contains(BasePermissionCodes.UpdateOwnAddress))
                {
                    return true;
                }
            }
            return false;
        }

        // Verifies that the given address type can be updated, based on configured address types.
        private static bool IsUpdatableAddressType(UserProfileConfiguration2 configuration, List<string> addressTypes)
        {
            if (configuration == null || addressTypes == null || addressTypes.Count() == 0)
            {
                return false;
            }
            else
            {
                if (configuration.UpdatableAddressTypes.Intersect(addressTypes).Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        #endregion

    }
}
