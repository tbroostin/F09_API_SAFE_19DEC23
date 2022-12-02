// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IGeneralLedgerUserRepository interface to get general ledger user information from Colleague.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GeneralLedgerUserRepository : PersonRepository, IGeneralLedgerUserRepository
    {
        private const int ThirtyMinuteCacheTimeout = 30;
        /// <summary>
        /// This constructor allows us to instantiate a general ledger user repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        /// <param name="apiSettings">Pass in an apiSettings object.</param>
        public GeneralLedgerUserRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            // Nothing to do here.
        }

        /// <summary>
        /// This method gets the general ledger expense accounts that are assigned to the user. 
        /// It is cached for 30 minutes.
        /// </summary>
        /// <param name="id">This is the ID of the user logged in.</param>
        /// <param name="fullAccessRole">This is the role that defines full access for the general ledger.</param>
        /// <param name="glClassificationName">This is the name of the general ledger classification component.</param>
        /// <param name="expenseClassValues">This is the list of general ledger expense account classification values.</param>
        /// <returns>Returns the general ledger user domain entity.</returns>
        public async Task<GeneralLedgerUser> GetGeneralLedgerUserAsync(string id, string fullAccessRole, string glClassificationName, IEnumerable<string> expenseClassValues)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID may not be null or empty");
            }

            if (string.IsNullOrEmpty(glClassificationName))
            {
                throw new ArgumentNullException("glClassificationName", "GL classification Name may not be null or empty");
            }

            if (expenseClassValues == null || expenseClassValues.Count() == 0)
            {
                throw new ArgumentNullException("expenseClassValues", "Expense class values may not be null or empty");
            }

            return await GetGeneralLedgerUserDataAsync(id, fullAccessRole, glClassificationName, expenseClassValues, null);
        }

        /// <summary>
        /// Returns a general ledger user object.
        /// </summary>
        /// <param name="id">This is the user id.</param>
        /// <param name="fullAccessRole">Role that defines full GL account access.</param>
        /// <param name="glClassConfiguration">GL Class Configuration</param>
        /// <returns>Returns a general ledger user domain entity.</returns>
        public async Task<GeneralLedgerUser> GetGeneralLedgerUserAsync2(string id, string fullAccessRole, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID may not be null or empty");
            }

            var glClassificationName = glClassConfiguration.ClassificationName;
            if (string.IsNullOrEmpty(glClassificationName))
            {
                throw new ArgumentNullException("glClassificationName", "GL classification Name may not be null or empty");
            }

            var expenseClassValues = glClassConfiguration.ExpenseClassValues;
            if (expenseClassValues == null || expenseClassValues.Count() == 0)
            {
                throw new ArgumentNullException("expenseClassValues", "Expense class values may not be null or empty");
            }

            var revenueClassValues = glClassConfiguration.RevenueClassValues;
            if (revenueClassValues == null || revenueClassValues.Count() == 0)
            {
                throw new ArgumentNullException("revenueClassValues", "Revenue class values may not be null or empty");
            }

            var assetClassValues = glClassConfiguration.AssetClassValues;
            if (assetClassValues == null || assetClassValues.Count() == 0)
            {
                throw new ArgumentNullException("assetClassValues", "Asset class values may not be null or empty");
            }

            var liabilityClassValues = glClassConfiguration.LiabilityClassValues;
            if (liabilityClassValues == null || liabilityClassValues.Count() == 0)
            {
                throw new ArgumentNullException("liabilityClassValues", "Liability class values may not be null or empty");
            }

            var fundBalanceClassValues = glClassConfiguration.FundBalanceClassValues;
            if (fundBalanceClassValues == null || fundBalanceClassValues.Count() == 0)
            {
                throw new ArgumentNullException("fundBalanceClassValues", "Fund Balance class values may not be null or empty");
            }

            return await GetGeneralLedgerUserDataAsync(id, fullAccessRole, glClassificationName, expenseClassValues, revenueClassValues);
        }

        private async Task<GeneralLedgerUser> GetGeneralLedgerUserDataAsync(string id, string fullAccessRole, string glClassificationName,
            IEnumerable<string> expenseClassValues, IEnumerable<string> revenueClassValues)
        {
            return await GetOrAddToCacheAsync<GeneralLedgerUser>("GLUser" + id, async () =>
            {
                // Get PERSON data contract.
                Person personContract = await DataReader.ReadRecordAsync<Person>("PERSON", id);
                if (personContract == null)
                {
                    throw new ArgumentNullException("personContract", "Person record cannot be null.");
                }

                // Determine the list of general ledger expense accounts that the user has access to.
                // First, build the GeneralLedgerUser domain entity with an empty list of expense general
                // ledger account IDs. Then, determine the user's GL access level and get the list of expense
                // general ledger accounts that the user has access to and then add them to the domain entity.

                // If we have a person data contract, build the general ledger user domain entity.
                // Build the Person object.
                var generalLedgerUserEntity = Get<GeneralLedgerUser>(personContract.Recordkey,
                    person => new GeneralLedgerUser(person.Recordkey, person.LastName));

                // Re-initialize the GL User properties because they may have been loaded from cache.
                generalLedgerUserEntity.RemoveAllAccounts();
                generalLedgerUserEntity.RemoveAllExpenseAccounts();
                generalLedgerUserEntity.RemoveAllRevenueAccounts();

                // Set the default GL access to none.
                generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.No_Access);

                // Determine if the full GL access role is valid for use on the web.
                bool validFullGlAccessRole = false;
                if (!string.IsNullOrEmpty(fullAccessRole))
                {
                    Glroles glRolesContract = await DataReader.ReadRecordAsync<Glroles>("GLROLES", fullAccessRole);
                    if (glRolesContract != null)
                    {
                        if (glRolesContract.GlrRoleUse == "W" || glRolesContract.GlrRoleUse == "B")
                        {
                            validFullGlAccessRole = true;
                        }
                        else
                        {
                            logger.Info(string.Format("The full GL access role is not for use on the web.", fullAccessRole));
                        }
                    }
                    else
                    {
                        logger.Info(string.Format("The full GL access record is missing.", fullAccessRole));
                    }

                }
                else
                {
                    logger.Info(string.Format("The full GL access record is not defined.", fullAccessRole));
                }

                // Determine the access level for the user. If there is a valid full Gl access role and the user has
                // an active GLUSERS record that has the full GL access role active, then the user has full access.
                // Otherwise, if the user has some other active GL role, then the user has possible access.

                var activeUserRoles = new List<string>();

                var staffContract = await DataReader.ReadRecordAsync<Staff>("STAFF", id);
                if (staffContract == null)
                {
                    // The general ledger user has no Staff record.
                    logger.Error(string.Format("Person {0} has no Staff record.", id));
                }
                else
                {
                    if (string.IsNullOrEmpty(staffContract.StaffLoginId))
                    {
                        // There is no login (operator ID) on the Staff record.
                        logger.Error(string.Format("Person {0} has an incomplete Staff record.", id));
                    }
                    else
                    {
                        // Confirm that the user has GLUSERS record that is active for the current date.
                        var glUsersContract = await DataReader.ReadRecordAsync<Glusers>("GLUSERS", staffContract.StaffLoginId);
                        if (glUsersContract == null)
                        {
                            // There is no GLUSERS record for the general ledger user.
                            logger.Error(string.Format("Person {0} has no GL access definition.", id));
                        }
                        else
                        {
                            if (!glUsersContract.GlusStartDate.HasValue)
                            {
                                // GLUSERS record has no start date.
                                logger.Error(string.Format("Person {0} has insufficient GL access definition. GLUSERS record has no start date.", id));
                            }
                            else
                            {
                                if (DateTime.Today < glUsersContract.GlusStartDate)
                                {
                                    // GLUSERS record is in the future.
                                    logger.Error(string.Format("Person {0} has insufficient GL access definition. GLUSERS record is in the future.", id));
                                }
                                else
                                {
                                    if ((DateTime.Today >= glUsersContract.GlusEndDate) && glUsersContract.GlusEndDate.HasValue)
                                    {
                                        // GLUSERS record is expired.
                                        logger.Error(string.Format("Person {0} has insufficient GL access definition. GLUSERS record is expired.", id));
                                    }
                                    else
                                    {
                                        if ((glUsersContract.GlusRoleIds != null) && (glUsersContract.GlusRoleStartDates != null) && (glUsersContract.GlusRoleStartDates.Count() > 0))
                                        {
                                            // Determine which of the user's roles are for use on the web.
                                            var searchString = "WITH GLR.ROLE.USE EQ 'W' OR WITH GLR.ROLE.USE EQ 'B'";
                                            var webRoles = await DataReader.SelectAsync("GLROLES", glUsersContract.GlusRoleIds.ToArray(), searchString);

                                            // If any of the user's roles are for use on the web, loop through roles and determine the GL access
                                            // level based on whether the role is active for the current date and the role is for use on the web.
                                            // Also, when the user has partial access, get the list of the user's active roles to select the appropriate GL accounts.

                                            #region Loop through the user's roles
                                            if (webRoles != null)
                                            {
                                                if (webRoles.Count() > 0)
                                                {

                                                    DateTime? roleStartDate;
                                                    DateTime? roleEndDate;

                                                    for (int x = 0; x < glUsersContract.GlusRoleIds.Count(); x++)
                                                    {
                                                        roleStartDate = null;
                                                        roleEndDate = null;
                                                        if (glUsersContract.GlusRoleStartDates != null)
                                                        {
                                                            if (x <= glUsersContract.GlusRoleStartDates.Count - 1)
                                                            {
                                                                roleStartDate = glUsersContract.GlusRoleStartDates[x];
                                                            }
                                                        }
                                                        if (glUsersContract.GlusRoleEndDates != null)
                                                        {
                                                            if (x <= glUsersContract.GlusRoleEndDates.Count - 1)
                                                            {
                                                                roleEndDate = glUsersContract.GlusRoleEndDates[x];
                                                            }
                                                        }
                                                        if (roleStartDate != null && webRoles.Contains(glUsersContract.GlusRoleIds[x]))
                                                        {
                                                            if (roleStartDate <= DateTime.Today)
                                                            {
                                                                if (roleEndDate != null)
                                                                {
                                                                    if (DateTime.Today < roleEndDate)
                                                                    {
                                                                        if (!activeUserRoles.Contains(glUsersContract.GlusRoleIds[x]))
                                                                        {
                                                                            activeUserRoles.Add(glUsersContract.GlusRoleIds[x]);
                                                                        }

                                                                        if (glUsersContract.GlusRoleIds[x] == fullAccessRole)
                                                                        {
                                                                            if (validFullGlAccessRole)
                                                                            {
                                                                                generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.Full_Access);
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if (generalLedgerUserEntity.GlAccessLevel != GlAccessLevel.Full_Access)
                                                                            {
                                                                                generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.Possible_Access);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (!activeUserRoles.Contains(glUsersContract.GlusRoleIds[x]))
                                                                    {
                                                                        activeUserRoles.Add(glUsersContract.GlusRoleIds[x]);
                                                                    }

                                                                    if (glUsersContract.GlusRoleIds[x] == fullAccessRole)
                                                                    {
                                                                        if (validFullGlAccessRole)
                                                                        {
                                                                            generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.Full_Access);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (generalLedgerUserEntity.GlAccessLevel != GlAccessLevel.Full_Access)
                                                                        {
                                                                            generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.Possible_Access);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            // if the user has any active web GL roles, get the list of GL.ACCTS by selecting against
                                            // the GL.ACCTS.ROLES entity. Then, if there are any GL class expense values, sub-select 
                                            // the list of GL.ACCTS that the user has access to for those that are expense accounts.
                                            if (activeUserRoles != null)
                                            {
                                                if (activeUserRoles.Count > 0)
                                                {
                                                    // Logging the number of activeUserRoles for the person for TLS/troubleshooting.
                                                    logger.Debug("Person {0} has {1} activeUserRoles.", id, activeUserRoles.Count);
                                                    string[] userGlAccounts = null;
                                                    if (generalLedgerUserEntity.GlAccessLevel == GlAccessLevel.Full_Access)
                                                    {
                                                        userGlAccounts = await DataReader.SelectAsync("GL.ACCTS.ROLES", "");
                                                    }
                                                    else if (generalLedgerUserEntity.GlAccessLevel == GlAccessLevel.Possible_Access)
                                                    {
                                                        var criteria = "GLAR.INCL.IN.ROLES EQ '?'";
                                                        userGlAccounts = await DataReader.SelectAsync("GL.ACCTS.ROLES", criteria, activeUserRoles.ToArray());
                                                    }

                                                    if (userGlAccounts != null && userGlAccounts.Length > 0)
                                                    {
                                                        // Add the GL accounts.
                                                        generalLedgerUserEntity.AddAllAccounts(userGlAccounts);

                                                        #region Get revenue accounts if revenue class values are present.
                                                        if (revenueClassValues != null && revenueClassValues.Any())
                                                        {
                                                            // Get the list of revenue GL accounts accessible to the user.
                                                            StringBuilder revenueValues = new StringBuilder(null);
                                                            foreach (string revenueValue in revenueClassValues)
                                                            {
                                                                if (!string.IsNullOrEmpty(revenueValue))
                                                                {
                                                                    revenueValues.Append("'" + revenueValue + "'");
                                                                }
                                                            }

                                                            var glRevenueCriteria = glClassificationName + " EQ " + revenueValues;
                                                            var userRevenueAccounts = await DataReader.SelectAsync("GL.ACCTS", userGlAccounts, glRevenueCriteria);

                                                            if (userRevenueAccounts != null && userRevenueAccounts.Any())
                                                            {
                                                                generalLedgerUserEntity.AddRevenueAccounts(userRevenueAccounts);
                                                            }
                                                        }
                                                        #endregion

                                                        #region Get the list of expense GL accounts accessible to the user.
                                                        if (expenseClassValues != null && expenseClassValues.Any())
                                                        {
                                                            StringBuilder expenseValues = new StringBuilder(null);
                                                            foreach (string value in expenseClassValues)
                                                            {
                                                                if (!string.IsNullOrEmpty(value))
                                                                {
                                                                    expenseValues.Append("'" + value + "'");
                                                                }
                                                            }

                                                            var glExpenseCriteria = glClassificationName + " EQ " + expenseValues;
                                                            var userExpenseAccounts = await DataReader.SelectAsync("GL.ACCTS", userGlAccounts, glExpenseCriteria);

                                                            if (userExpenseAccounts != null && userExpenseAccounts.Length > 0)
                                                            {
                                                                generalLedgerUserEntity.AddExpenseAccounts(userExpenseAccounts);
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        logger.Debug("Person {0} has no userGlAccounts based on their activeUserRoles", id);
                                                        generalLedgerUserEntity.SetGlAccessLevel(GlAccessLevel.No_Access);
                                                    }
                                                }
                                                else
                                                {
                                                    // Logging the absence of activeUserRoles for the person for TLS/troubleshooting.
                                                    logger.Debug("Person {0} has no activeUserRoles.", id);
                                                }
                                            }

                                            if (generalLedgerUserEntity.GlAccessLevel == GlAccessLevel.No_Access)
                                            {
                                                logger.Error(string.Format("Person {0} has insufficient GL access definition.", id));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return generalLedgerUserEntity;
            }, ThirtyMinuteCacheTimeout);
        }

        /// <summary>
        /// Obtain the combined list of GL accounts the user can access and approve.
        /// </summary>
        /// <param name="id">This is the user ID.</param>
        /// <param name="glAccessAccounts">List of GL accounts the user has GL access to.</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetGlUserApprovalAndGlAccessAccountsAsync(string id, IEnumerable<string> glAccessAccounts)
        {
            return await GetOrAddToCacheAsync<IEnumerable<string>>("GLUserAccessAndApproval" + id, async () =>
            {
                // Determine the current approval roles for the user. There are approval roles that
                // have an amount limit and there are approval policy roles. We only need to determine
                // what GL accounts are included in the user's current approval roles regardless of
                // type or amount.

                var activeUserApprovalRoles = new List<string>();

                // Add the GL access accounts to the return oject.
                // These are the GL accounts the user has access to based on their current GL access roles.
                List<string> allAccessAndApprovalAccounts = glAccessAccounts.ToList();

                // Initialize the object that will get the GL accounts the user can approve based on their GL approval roles.
                IEnumerable<string> userApprovalGlAccounts = new List<string>();

                // Read the STAFF record for the user to get their login ID.
                var staffContract = await DataReader.ReadRecordAsync<Staff>("STAFF", id);
                if (staffContract == null)
                {
                    // The general ledger user has no Staff record.
                    logger.Error("==> The person ID has no Staff record <==");
                }
                else
                {
                    if (string.IsNullOrEmpty(staffContract.StaffLoginId))
                    {
                        // There is no login (operator ID) on the Staff record.
                        logger.Error("==> The person ID has an incomplete Staff record <==");
                    }
                    else
                    {
                        // Confirm that the user has GLUSERS record that is active for the current date.
                        var glUsersContract = await DataReader.ReadRecordAsync<Glusers>("GLUSERS", staffContract.StaffLoginId);
                        if (glUsersContract == null)
                        {
                            // There is no GLUSERS record for the general ledger user.
                            logger.Error("==> The person ID has no GL access definition <==");
                        }
                        else
                        {
                            if (!glUsersContract.GlusStartDate.HasValue)
                            {
                                // GLUSERS record has no start date.
                                logger.Error("==> The person ID has insufficient GL access definition. GLUSERS record has no start date. <==");
                            }
                            else
                            {
                                if (DateTime.Today < glUsersContract.GlusStartDate)
                                {
                                    // GLUSERS record is in the future.
                                    logger.Error("==> The person ID has insufficient GL access definition. GLUSERS record is in the future. <==");
                                }
                                else
                                {
                                    if ((DateTime.Today >= glUsersContract.GlusEndDate) && glUsersContract.GlusEndDate.HasValue)
                                    {
                                        // GLUSERS record is expired.
                                        logger.Error("==> The person ID has insufficient GL access definition. GLUSERS record is expired. <==");
                                    }
                                    else
                                    {
                                        // If any of the user's approval roles are for use on the web, 
                                        // loop through them and determine which approval roles are current.

                                        if (glUsersContract.ApprovalRolesEntityAssociation != null && glUsersContract.ApprovalRolesEntityAssociation.Any())
                                        {
                                            // Get a list of current GL approval roles for the user.
                                            IEnumerable<GlusersApprovalRoles> activeApprovalRoles = new List<GlusersApprovalRoles>();
                                            activeApprovalRoles = glUsersContract.ApprovalRolesEntityAssociation.Where(x => x != null && !string.IsNullOrWhiteSpace(x.GlusApprRoleIdsAssocMember)
                                            && (x.GlusApprStartDatesAssocMember != null) && (DateTime.Today >= x.GlusApprStartDatesAssocMember)
                                            && (x.GlusApprEndDatesAssocMember.HasValue ? DateTime.Today < x.GlusApprEndDatesAssocMember.Value : true)).ToList();

                                            if (activeApprovalRoles != null && activeApprovalRoles.Any())
                                            {
                                                // Remove role ID duplicates just in case.
                                                string[] activeApprovalRoleIDs;
                                                activeApprovalRoleIDs = activeApprovalRoles.Select(x => x.GlusApprRoleIdsAssocMember).Distinct().ToArray();

                                                // Determine which of these roles are for use on the web.
                                                var searchString = "WITH GLR.ROLE.USE EQ 'W' OR WITH GLR.ROLE.USE EQ 'B'";
                                                var webApprovalRoles = await DataReader.SelectAsync("GLROLES", activeApprovalRoleIDs, searchString);

                                                // If the user has any active web GL roles, get the list of GL.ACCTS
                                                // by selecting against the GL.ACCTS.ROLES entity. 
                                                if (webApprovalRoles != null && webApprovalRoles.Any())
                                                {
                                                    var criteria = "GLAR.INCL.IN.ROLES EQ '?'";
                                                    userApprovalGlAccounts = await DataReader.SelectAsync("GL.ACCTS.ROLES", criteria, webApprovalRoles.ToArray());

                                                    // If there are any GL accounts the user can approve, add them
                                                    // to the list of GL access accounts to get the combined list
                                                    // removing duplicates, nulls and empty or blanks.
                                                    if (userApprovalGlAccounts != null && userApprovalGlAccounts.Any())
                                                    {
                                                        allAccessAndApprovalAccounts.AddRange(userApprovalGlAccounts.Except(allAccessAndApprovalAccounts).Distinct().Where(x => !string.IsNullOrEmpty(x)));
                                                    }
                                                    else
                                                    {
                                                        logger.Debug("==> User has no GL accounts that they can approve <==");
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Debug("==> User has no active approval roles <==");
                                                }
                                            }
                                            else
                                            {
                                                logger.Debug("==> User has no approval roles <==");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return allAccessAndApprovalAccounts;
            }, ThirtyMinuteCacheTimeout);
        }


        public async Task<bool> CheckOverride(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Info("GeneralLedgerUserRepository failed to check if override, ID is null");
                return false;
            }
            var criteria = "WITH SYS.PERSON.ID = " + id;
            var user = await DataReader.SelectAsync("UT.OPERS", criteria);
            bool hasOverride = false;

            string login;

            if (user == null)
            {
                var staffContract = await DataReader.ReadRecordAsync<Staff>("STAFF", id);
                login = staffContract.StaffLoginId;
            }
            else
            {
                login = user[0];
            }

            if (!string.IsNullOrWhiteSpace(login))
            {
                var utOpers = await DataReader.ReadRecordAsync<DataContracts.Opers>("UT.OPERS", login);
                var classId = utOpers.SysUserClasses.FirstOrDefault();
                var chars = await GetSeclassCharacteristics(classId);
                hasOverride = false;
                if (chars != null && chars.Any())
                    hasOverride = true;
            }

            return hasOverride;
        }


        /// <summary>
        /// Gets CLASS.CHARACTERISTICS
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetSeclassCharacteristics(string classId)
        {
            List<string> chars = new List<string>();
            // Get General Ledger parameters from the ACCT.STRUCTURE record in ACCOUNT.PARAMETERS.
            var glStruct = new Glstruct();

            glStruct = await DataReader.ReadRecordAsync<Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE");
            if (glStruct == null)
            {
                // GLSTRUCT must exist for Colleague Financials to function properly
                throw new ConfigurationException("GL account structure is not defined.");
            }

            if (!string.IsNullOrEmpty(glStruct.AcctOverrideTokens.FirstOrDefault()))
            {
                var seclasses = await DataReader.BulkReadRecordAsync<DataContracts.ApplSeclass>("SECLASS", string.Format("WITH SYS.CLASS.ID EQ '{0}'", classId));
                if (seclasses == null)
                {
                    return null;
                }
                var seclass = seclasses.FirstOrDefault();
                seclass.ClassCharacteristics.ForEach(i =>
                {
                    chars.Add(i);
                });
            }
            return chars.Any() ? chars : null;
        }
    }
}
