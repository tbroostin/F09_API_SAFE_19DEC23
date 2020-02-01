// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Data.BudgetManagement.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.BudgetManagement.Repositories
{
    [RegisterType]
    public class BudgetDevelopmentRepository : BaseColleagueRepository, IBudgetDevelopmentRepository
    {
        public BudgetDevelopmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Gets the list of budget officers for the working budget.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <returns>A list of budget officer domain entities for the working budget.</returns>
        /// <exception cref="ArgumentNullException">An exception is returned when no working budget or no current user PERSON ID is specified.</exception>
        public async Task<List<BudgetOfficer>> GetBudgetDevelopmentBudgetOfficersAsync(string workingBudgetId, string currentUserPersonId)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }

            // get the OPERS ID associated with STAFF ID.
            string userOperatorId = await GetOperatorIdFromStaffRecordAsync(currentUserPersonId);

            // Initialize the list of budget officer domain entities to be returned.
            var workingBudgetBudgetOfficerEntities = new List<BudgetOfficer>();

            // If the user has an OPERS record, find the budget officers, get the associated responsibility IDs from the BOC
            // records, and then get the BWK record IDs from the BCT records.
            if (!string.IsNullOrEmpty(userOperatorId))
            {
                // Get the list of budget officers for the current user for the working budget.
                var selectionCriteria = "WITH BO.BUDGET EQ '" + workingBudgetId + "'" + " AND BO.LOGIN EQ '" + userOperatorId + "'";
                var possibleWorkingBudgetOfficersIds = await DataReader.SelectAsync("BUD.OFCR", selectionCriteria);

                // If there are any budget officer records containing the working budget and the current user ID,
                // read the budget officer records and then get the BUD.OFCR records where there is an association entry
                // that contains both the working budget and current user login ID, which means that the current user is identified as
                // one of the budget officers for the working budget.
                if (possibleWorkingBudgetOfficersIds != null && possibleWorkingBudgetOfficersIds.Any())
                {
                    Collection<BudOfcr> budgetOfficerRecords = new Collection<BudOfcr>();
                    budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(possibleWorkingBudgetOfficersIds);
                    string[] workingBudgetOfficerRecordIds;

                    if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                    {
                        // Get a string array of BUD.OFCR record IDs where the BUD.OFCR record has an association row where the budget is working budget
                        // and the login is the user staff operator ID.
                        workingBudgetOfficerRecordIds = budgetOfficerRecords.Where(x => x != null && x.OfcrinfoEntityAssociation != null
                        && x.OfcrinfoEntityAssociation.Where(y => y != null && y.BoBudgetAssocMember == workingBudgetId && y.BoLoginAssocMember == userOperatorId).Any()).Select(x => x.Recordkey).ToArray();

                        if (workingBudgetOfficerRecordIds != null && workingBudgetOfficerRecordIds.Any())
                        {
                            // Take the string array of BUD.OFCR IDs that the user is an officer for the working budget, and read the BOC.workingbudget
                            // records to get the associated responsibility IDs. 
                            var bocSuiteFileName = "BOC." + workingBudgetId;
                            var bocDataContracts = await DataReader.BulkReadRecordAsync<BudOctl>(bocSuiteFileName, workingBudgetOfficerRecordIds, true);
                            if (bocDataContracts != null && bocDataContracts.Any())
                            {
                                // Determine the list of budget officers for the working budget for the user, and add
                                // budget officer domain entities to the list of domain entities to be returned by this method.
                                string[] allBudOfcrIds = await DetermineBudgetOfficersAsync(workingBudgetId, bocDataContracts);
                                if (allBudOfcrIds != null && allBudOfcrIds.Any())
                                {
                                    foreach (var budOfcr in allBudOfcrIds)
                                    {
                                        workingBudgetBudgetOfficerEntities.Add(new BudgetOfficer(budOfcr));
                                    }
                                }

                                // If there were any budget officers for the working budget, update the budget officer domain entities with
                                // the budget officer login (OPERS) ID and name.
                                await UpdateBudgetOfficerWithIdAndNameAsync(workingBudgetId, workingBudgetBudgetOfficerEntities, allBudOfcrIds);
                            }
                        }
                    }
                }
            }
            return workingBudgetBudgetOfficerEntities;
        }

        /// <summary>
        /// Gets the list of reporting units for the user for the working budget.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <returns>A list of reporting unit domain entities for the user the working budget.</returns>
        /// <exception cref="ArgumentNullException">An exception is returned when no working budget or no current user PERSON ID is specified.</exception>
        public async Task<List<BudgetReportingUnit>> GetBudgetDevelopmentReportingUnitsAsync(string workingBudgetId, string currentUserPersonId)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }

            // Initialize the list of reporting unit domain entities to be returned.
            var workingBudgetReportingUnitEntities = new List<BudgetReportingUnit>();

            // get the OPERS ID associated with STAFF ID.
            string userOperatorId = await GetOperatorIdFromStaffRecordAsync(currentUserPersonId);

            // If the user has an OPERS record, find his/hers budget officers and then get the associated
            // responsibility IDs for the user and down the hierarch tree from the BCT records.
            if (!string.IsNullOrEmpty(userOperatorId))
            {
                {// Get the BUD.OCTL records for the budget officers assigned to the user for the working budget.
                    Collection<BudOctl> bocDataContracts = await GetBudOctlRecordsForBudgetOfficersAssignedToUser(workingBudgetId, userOperatorId);

                    if (bocDataContracts != null && bocDataContracts.Any())
                    {
                        // Determine all the uppermost responsibility unit IDs from the BUD.OCTL records.
                        List<string> masterResponsibilityIds = new List<string>();
                        masterResponsibilityIds = DetermineMasterResponsibilityIds(workingBudgetId, bocDataContracts, masterResponsibilityIds);

                        if (masterResponsibilityIds != null && masterResponsibilityIds.Any())
                        {
                            // Obtain a list with all the responsibility unit domain entities for the user for the working budget.
                            await DetermineResponsibilityUnitIdsAsync(workingBudgetId, masterResponsibilityIds, workingBudgetReportingUnitEntities);
                        }

                        // Add the description to the reporting units.
                        if (workingBudgetReportingUnitEntities != null && workingBudgetReportingUnitEntities.Any())
                        {
                            await PopulateReportingUnitsDescriptionAsync(workingBudgetReportingUnitEntities);
                        }
                    }
                }
            }
            return workingBudgetReportingUnitEntities;
        }

        /// <summary>
        /// Update budget officer domain entities with login ID and name.
        /// </summary>
        /// <param name="workingBudgetId"></param>
        /// <param name="workingBudgetBudgetOfficerEntities"></param>
        /// <param name="allBudOfcrIds"></param>
        /// <returns></returns>
        private async Task UpdateBudgetOfficerWithIdAndNameAsync(string workingBudgetId, List<BudgetOfficer> workingBudgetBudgetOfficerEntities, string[] allBudOfcrIds)
        {
            if (allBudOfcrIds != null && allBudOfcrIds.Any())
            {
                var uniqueLoginIds = new List<string>();
                // Read the BUD.OFCR records for the budget officer IDs.
                // The different budget officer IDs can have the same login, different logins, or no login.
                var allBudgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(allBudOfcrIds);
                if (allBudgetOfficerRecords != null && allBudgetOfficerRecords.Any())
                {
                    foreach (var budOfcr in allBudgetOfficerRecords)
                    {
                        // Get the login ID for the budget officer.
                        var loginIdAssocMember = budOfcr.OfcrinfoEntityAssociation.Where(x => x != null && x.BoBudgetAssocMember == workingBudgetId).FirstOrDefault();
                        if (loginIdAssocMember != null && !string.IsNullOrWhiteSpace(loginIdAssocMember.BoLoginAssocMember))
                        {
                            var loginId = loginIdAssocMember.BoLoginAssocMember;

                            // Obtain the list of budget line items that have this budget officer record ID and populate the budget officer login
                            var budOfcrEntities = workingBudgetBudgetOfficerEntities.Where(x => x.BudgetOfficerId != null && x.BudgetOfficerId == budOfcr.Recordkey).ToList();
                            if (budOfcrEntities != null && budOfcrEntities.Any())
                            {
                                foreach (var ofcr in budOfcrEntities)
                                {
                                    ofcr.BudgetOfficerLogin = loginId;
                                }
                            }
                            // Build a list of unique login IDs.
                            if (!uniqueLoginIds.Contains(loginId))
                            {
                                uniqueLoginIds.Add(loginId);
                            }
                        }
                    }
                }

                if (uniqueLoginIds != null && uniqueLoginIds.Any())
                {
                    // Read the OPERS records.
                    var operRecords = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueLoginIds.ToArray(), true);
                    if (operRecords != null && operRecords.Any())
                    {
                        foreach (var oper in operRecords)
                        {
                            if (!string.IsNullOrWhiteSpace(oper.SysUserName))
                            {
                                // Obtain the list of budget line items that have a budget officer with this login id and populate the name.
                                var budOfcrEntities = workingBudgetBudgetOfficerEntities.Where(x => x.BudgetOfficerId != null && x.BudgetOfficerLogin == oper.Recordkey).ToList();
                                if (budOfcrEntities != null && budOfcrEntities.Any())
                                {
                                    foreach (var ofcr in budOfcrEntities)
                                    {
                                        ofcr.BudgetOfficerName = oper.SysUserName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a working budget for a user.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="budgetConfigurationComparables">A list of budget comparables defined for the working budget.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <param name="glAccountStructure">The GL account structure domain entity.</param>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>A working budget domain entity.</returns>
        /// <exception cref="ArgumentNullException">An exception is returned when no working budget or no current user PERSON ID is specified.</exception>
        public async Task<WorkingBudget2> GetBudgetDevelopmentWorkingBudget2Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables,
            WorkingBudgetQueryCriteria criteria, string currentUserPersonId, GeneralLedgerAccountStructure glAccountStructure, int startPosition, int recordCount)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }

            LogData(criteria, null);

            IList<string> majorComponentStartPosition = glAccountStructure.MajorComponentStartPositions;

            // get the OPERS ID associated with STAFF ID.
            string userOperatorId = await GetOperatorIdFromStaffRecordAsync(currentUserPersonId);

            List<string> masterResponsibilityIds = new List<string>();

            // Initialize the working budget domain entity to be returned.
            var workingBudgetEntity = new WorkingBudget2();

            // If the user has an OPERS record, find the budget officers, get the associated responsibility IDs from the BOC
            // records, and then get the BWK record IDs from the BCT records.
            if (!string.IsNullOrEmpty(userOperatorId))
            {
                // Get the list of budget officers for the current user for the working budget.
                var selectionCriteria = "WITH BO.BUDGET EQ '" + workingBudgetId + "'" + " AND BO.LOGIN EQ '" + userOperatorId + "'";
                var possibleWorkingBudgetOfficersIds = await DataReader.SelectAsync("BUD.OFCR", selectionCriteria);

                // If there are any budget officer records containing the working budget and the current user ID,
                // read the budget officer records and then get the BUD.OFCR records where there is an association entry
                // that contains both the working budget and current user login ID, which means that the current user is identified as
                // one of the budget officers for the working budget.
                if (possibleWorkingBudgetOfficersIds != null && possibleWorkingBudgetOfficersIds.Any())
                {
                    Collection<BudOfcr> budgetOfficerRecords = new Collection<BudOfcr>();
                    budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(possibleWorkingBudgetOfficersIds);
                    string[] workingBudgetOfficerRecordIds;

                    if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                    {
                        // Get a string array of BUD.OFCR record IDs where the BUD.OFCR record has an association row where the budget is working budget
                        // and the login is the user staff operator ID.
                        workingBudgetOfficerRecordIds = budgetOfficerRecords.Where(x => x != null && x.OfcrinfoEntityAssociation != null
                        && x.OfcrinfoEntityAssociation.Where(y => y != null && y.BoBudgetAssocMember == workingBudgetId && y.BoLoginAssocMember == userOperatorId).Any()).Select(x => x.Recordkey).ToArray();

                        if (workingBudgetOfficerRecordIds != null && workingBudgetOfficerRecordIds.Any())
                        {
                            // Take the string array of BUD.OFCR IDs that the user is an officer for the working budget, and read the BOC.workingbudget
                            // records to get the associated responsibility IDs. 
                            var bocSuiteFileName = "BOC." + workingBudgetId;
                            var bocDataContracts = await DataReader.BulkReadRecordAsync<BudOctl>(bocSuiteFileName, workingBudgetOfficerRecordIds, true);
                            if (bocDataContracts != null && bocDataContracts.Any())
                            {
                                // Determine the BWK.workingbudget IDs using the BOC.workingbudget records.
                                string[] allBwkIds = await DetermineBudWorkIdsAsync(workingBudgetId, bocDataContracts, criteria, masterResponsibilityIds);
                                if (allBwkIds != null && allBwkIds.Any())
                                {
                                    // Apply the filter, get the specific rage requested, bulk read BWK.workingbudget records, and build the domain entity.
                                    workingBudgetEntity = await AddBudgetLineItemsToWorkingBudgetEntity2Async(workingBudgetEntity, workingBudgetId, allBwkIds, criteria, startPosition, recordCount, budgetConfigurationComparables, glAccountStructure);
                                }

                                if (workingBudgetEntity.LineItems != null && workingBudgetEntity.LineItems.Any())
                                {
                                    // Get the login and name for the budget officers in each line items
                                    workingBudgetEntity = await PopulateBudgetOfficerAsync(workingBudgetEntity, workingBudgetId);

                                    // Get the description and authorization date for the reporting unit in each line items
                                    workingBudgetEntity = await PopulateReportingUnits2Async(workingBudgetEntity, workingBudgetId);

                                    // Determine if the user's authorization date for the reporting unit has passed.
                                    workingBudgetEntity = await DetermineIfUserAuthorizationDateHasPassed2Async(workingBudgetEntity, workingBudgetId, masterResponsibilityIds);
                                }
                            }
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Gets a list of working budget line items.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <param name="budgetAccountIds">A list of budget account IDs.</param>
        /// <returns>A list of budget line item domain entities.</returns>
        /// <exception cref="ArgumentNullException">An exception is returned when no working budget, no current user PERSON ID, or no budget accounts are specified.</exception>
        public async Task<List<BudgetLineItem>> GetBudgetDevelopmentBudgetLineItemsAsync(string workingBudgetId, string currentUserPersonId, List<string> budgetAccountIds)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }
            if (budgetAccountIds == null || !budgetAccountIds.Any())
            {
                throw new ArgumentNullException("budgetAccountId", "There is no budget account ID defined.");
            }

            // Initialize the budget line item entity that will be returned.
            List<BudgetLineItem> budgetLineItems = new List<BudgetLineItem>();

            // get the OPERS ID associated with STAFF ID.
            string userOperatorId = await GetOperatorIdFromStaffRecordAsync(currentUserPersonId);

            // If the user has an OPERS record, find the budget officers, get the associated responsibility IDs from the BOC
            // records, and then get the BWK record IDs from the BCT records.
            if (!string.IsNullOrEmpty(userOperatorId))
            {
                // Get the list of budget officers for the current user for the working budget.
                var selectionCriteria = "WITH BO.BUDGET EQ '" + workingBudgetId + "'" + " AND BO.LOGIN EQ '" + userOperatorId + "'";
                var possibleWorkingBudgetOfficersIds = await DataReader.SelectAsync("BUD.OFCR", selectionCriteria);

                // If there are any budget officer records containing the working budget and the current user ID,
                // read the budget officer records and then get the BUD.OFCR records where there is an association entry
                // that contains both the working budget and current user login ID, which means that the current user is identified as
                // one of the budget officers for the working budget.
                if (possibleWorkingBudgetOfficersIds != null && possibleWorkingBudgetOfficersIds.Any())
                {
                    Collection<BudOfcr> budgetOfficerRecords = new Collection<BudOfcr>();
                    budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(possibleWorkingBudgetOfficersIds);
                    string[] workingBudgetOfficerRecordIds;
                    // List with all the uppermost responsibility unit IDs from the BUD.OCTL records.
                    List<string> masterResponsibilityIds = new List<string>();

                    if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                    {
                        // Get a string array of BUD.OFCR record IDs where the BUD.OFCR record has an association row where the budget is working budget
                        // and the login is the user staff operator ID.
                        workingBudgetOfficerRecordIds = budgetOfficerRecords.Where(x => x != null && x.OfcrinfoEntityAssociation != null
                        && x.OfcrinfoEntityAssociation.Where(y => y != null && y.BoBudgetAssocMember == workingBudgetId && y.BoLoginAssocMember == userOperatorId).Any()).Select(x => x.Recordkey).ToArray();

                        if (workingBudgetOfficerRecordIds != null && workingBudgetOfficerRecordIds.Any())
                        {
                            // Take the string array of BUD.OFCR IDs that the user is an officer for the working budget, and read the BOC.workingbudget
                            // records to get the associated responsibility IDs. 
                            var bocSuiteFileName = "BOC." + workingBudgetId;
                            var bocDataContracts = await DataReader.BulkReadRecordAsync<BudOctl>(bocSuiteFileName, workingBudgetOfficerRecordIds, true);
                            if (bocDataContracts != null && bocDataContracts.Any())
                            {
                                // Determine the BWK.workingbudget IDs using the BOC.workingbudget records.
                                string[] allBwkIds = await DetermineBudWorkIdsAsync(workingBudgetId, bocDataContracts, null, masterResponsibilityIds);
                                if (allBwkIds != null && allBwkIds.Any())
                                {
                                    // Read the BWK record.
                                    string[] budgetAccountArray = budgetAccountIds.ToArray();
                                    var bwkSuiteFileName = "BWK." + workingBudgetId;
                                    var budWorkDataContracts = await DataReader.BulkReadRecordAsync<BudWork>(bwkSuiteFileName, budgetAccountArray, true);

                                    foreach (var account in budgetAccountIds)
                                    {
                                        if (allBwkIds.ToList().Contains(account))
                                        {
                                            var budWorkDataContract = budWorkDataContracts.Where(x => x.Recordkey == account).FirstOrDefault();

                                            if (budWorkDataContract != null)
                                            {
                                                var budgetLineItem = new BudgetLineItem(account);
                                                // get the working budget amount for the line item.
                                                budgetLineItem.WorkingAmount = budWorkDataContract.BwLineAmt.HasValue ? budWorkDataContract.BwLineAmt.Value : 0;
                                                budgetLineItems.Add(budgetLineItem);
                                            }
                                            else
                                            {
                                                logger.Error(string.Format("The file {0} has no record for budget account {1}.", bwkSuiteFileName, account));
                                                throw new ArgumentNullException("budWorkDataContract", "There is no budget line item for the budget account ID.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return budgetLineItems;
        }

        /// <summary>
        /// Updates a working budget line item.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="budgetAccountIds">A list of budget line item budget account IDs.</param>
        /// <param name="newBudgetAmounts">A list of budget line item new budget amounts.</param>
        /// <param name="justificationNotes">A list of budget line item justification notes.</param>
        /// <returns>A budget line item domain entity.</returns>
        public async Task<List<BudgetLineItem>> UpdateBudgetDevelopmentBudgetLineItemsAsync(string currentUserPersonId, string workingBudgetId, List<string> budgetAccountIds, List<long?> newBudgetAmounts, List<string> justificationNotes)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }

            if (budgetAccountIds == null || (!budgetAccountIds.Any()))
            {
                throw new ArgumentException("There are no budget line items to update.");
            }

            if ((newBudgetAmounts != null && newBudgetAmounts.Any()) && (justificationNotes != null && justificationNotes.Any()))
            {
                if ((budgetAccountIds.Count() != newBudgetAmounts.Count()) || budgetAccountIds.Count() != justificationNotes.Count())
                {
                    throw new ArgumentException("Mismatch number of values for budget account IDs, amounts, and justification notes.");
                }
            }
            else
            {
                throw new ArgumentException("Mismatch number of values for budget account IDs, amounts, and justification notes.");
            }

            // Initialize the list of domain entities to be returned.
            List<BudgetLineItem> budgetLineItems = new List<BudgetLineItem>();

            // Initialize lists to be passed into transaction.
            List<string> lineGlNumbers = new List<string>();
            List<long?> lineNewAmount = new List<long?>();
            List<string> lineItemJustificationNotes = new List<string>();

            // Build the lists using method arguments that have all have values.
            var budgetItems = new List<Transactions.budgetLineItems2>();
            int count = budgetAccountIds.Count;
            for (int i = 0; i < count; i++)
            {
                var lineItem = new Transactions.budgetLineItems2()
                {
                    AlLineGlNo = budgetAccountIds[i],
                    AlLineNewAmt = newBudgetAmounts[i],
                    AlLineNotes = justificationNotes[i]
                };
                budgetItems.Add(lineItem);
            }

            // If the lists have data, call the transaction to update the budget line items information.
            if (budgetItems.Any())
            {
                var request = new TxUpdateWorkingBudgetLineItems2Request()
                {
                    AWorkingBudget = workingBudgetId,
                    APersonId = currentUserPersonId
                };
                request.budgetLineItems2 = budgetItems;

                var response = await transactionInvoker.ExecuteAsync<TxUpdateWorkingBudgetLineItems2Request, TxUpdateWorkingBudgetLineItems2Response>(request);

                // If the response contains budget line items that have been updated, build budget line item domain entities
                // and add them to the list of budget line item domain entities that are returned.
                if (response != null && response.budgetLineItems2.Any())
                {
                    foreach (var item in response.budgetLineItems2)
                    {
                        if (!string.IsNullOrEmpty(item.AlLineGlNo))
                        {
                            BudgetLineItem lineItem = new BudgetLineItem(item.AlLineGlNo);
                            lineItem.WorkingAmount = item.AlLineNewAmt.Value;
                            lineItem.JustificationNotes = item.AlLineNotes;
                            budgetLineItems.Add(lineItem);
                        }
                    }
                }
            }
            return budgetLineItems;
        }

        /// <summary>
        /// Given a working budget ID and a list of associated BOC.workingbudget data contracts, find the 
        /// associated BWK.workingbudget record IDs.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="bocDataContracts">The collection of BOC.workingbudget data contracts.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="masterResponsibilityIds">List of all the uppermost responsibility unit IDs.</param>
        /// <returns>A sorted string array of BWK record IDs for a working budget.</returns>
        private async Task<string[]> DetermineBudWorkIdsAsync(string workingBudgetId, Collection<BudOctl> bocDataContracts, WorkingBudgetQueryCriteria criteria, List<string> masterResponsibilityIds)
        {
            string[] allBwkArray = null;

            if (bocDataContracts != null && bocDataContracts.Any())
            {
                // Determine all the uppermost responsibility unit IDs from the BUD.OCTL records.
                masterResponsibilityIds = DetermineMasterResponsibilityIds(workingBudgetId, bocDataContracts, masterResponsibilityIds);

                if (masterResponsibilityIds != null && masterResponsibilityIds.Any())
                {
                    // Now that we have master list of uppermost responsibility IDs associated with the BUD.OFCR IDs for which the user is a budget officer
                    // for the working budget, then get all of the subordinate responsibility units by selecting records that begin with the same string.
                    //
                    // Note: We are getting this information from BCT.budgetname file suite file, and not from BUD.RESP since the responsibility
                    // tree could have been changed since the time that the working budget was generated.
                    string bctCriteria = string.Empty;
                    List<string> allBwkIds = new List<string>();

                    foreach (var responsibility in masterResponsibilityIds)
                    {
                        if (!string.IsNullOrEmpty(responsibility))
                        {
                            // the criteria gets the uppermost responsibility unit and its children.
                            bctCriteria = "WITH @ID LIKE '" + responsibility + "_...' OR WITH @ID EQ '" + responsibility + "'";
                        }

                        var bctSuiteFileName = "BCT." + workingBudgetId;
                        var responsbilityDataContracts = await DataReader.BulkReadRecordAsync<BudCtrl>(bctSuiteFileName, bctCriteria, true);

                        // If the filter criteria contains reporting unit IDs, filter the BCT records by the selected ones.
                        IEnumerable<BudCtrl> filteredByUnitBctRecords = null;
                        if (criteria != null && criteria.BudgetReportingUnitIds != null && criteria.BudgetReportingUnitIds.Any())
                        {
                            var tempRecords = new List<BudCtrl>();
                            // Loop through the reporting unit IDs included in the filter.
                            // These have been uppercased in the adapter.
                            foreach (var filteredId in criteria.BudgetReportingUnitIds)
                            {
                                // Loop through each of the seledcted BUD.CTRL records for the user. 
                                foreach (var record in responsbilityDataContracts)
                                {
                                    // See if we need an exact match on the id or not.
                                    if (criteria.IncludeBudgetReportingUnitsChildren)
                                    {
                                        // Select the reporting units in the filter and their descendants.
                                        if (record.Recordkey.ToUpperInvariant().StartsWith(filteredId))
                                        {
                                            // Only add the record if it is not in the list already.
                                            // If the criteria has OB_FIN and OB_FIN_IT with children,
                                            // we don't want to add OB_FIN_IT twice.
                                            var recordAlreadyAdded = tempRecords.Where(x => x.Recordkey == record.Recordkey).FirstOrDefault();
                                            if (recordAlreadyAdded == null)
                                            {
                                                tempRecords.Add(record);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Only select the reporting units included in the filter without their descendants.
                                        if (filteredId == record.Recordkey.ToUpperInvariant())
                                        {
                                            tempRecords.Add(record);
                                        }
                                    }
                                }
                            }
                            filteredByUnitBctRecords = tempRecords;
                        }
                        else
                        {
                            // There is no reporting unit criteria in the filter so use all the BUD.CTRL records for the user.
                            filteredByUnitBctRecords = responsbilityDataContracts;
                        }

                        // From the responsibility data contracts, get the list of GL numbers from each data contract from BC.WORK.LINE.NO. But,
                        // BC.WORK.LINE.NO also contains a value for the responsibility ID that needs to not be included in the resulting total
                        // list of GL numbers (budget line items) for the working budget for the user.
                        if (filteredByUnitBctRecords != null && filteredByUnitBctRecords.Any())
                        {
                            // Obtain the list of reponsibility IDs from the BUD.CRL records.
                            var responsibilityIds = filteredByUnitBctRecords.Select(x => x.Recordkey).ToList();
                            string[] tempBwkIds;

                            // If the filter criteria contains budget officer IDs, filter the BCT records by 
                            // the selected ones and then get the GL accounts for each filtered BCT record,
                            // excluding those values in BcWorkLineNo that are not GL accounts but responsibility IDs
                            if (criteria != null && criteria.BudgetOfficerIds != null && criteria.BudgetOfficerIds.Any())
                            {
                                var filteredByBudOfcrBctRecords = filteredByUnitBctRecords.Where(bct => criteria.BudgetOfficerIds.Contains(bct.BcBofId));
                                tempBwkIds = filteredByBudOfcrBctRecords.SelectMany(x => x.BcWorkLineNo).Where(y => !responsibilityIds.Contains(y)).ToArray();

                            }
                            // Otherwise, get the GL accounts from all the selected BCT records,
                            // excluding those values in BcWorkLineNo that are not GL accounts but responsibility IDs.
                            else
                            {
                                tempBwkIds = filteredByUnitBctRecords.SelectMany(x => x.BcWorkLineNo).Where(y => !responsibilityIds.Contains(y)).ToArray();
                            }

                            if (tempBwkIds != null && tempBwkIds.Any())
                            {
                                foreach (var tempId in tempBwkIds)
                                {
                                    if (!allBwkIds.Contains(tempId))
                                    {
                                        allBwkIds.Add(tempId);
                                    }
                                }
                            }
                        }
                        allBwkArray = allBwkIds.OrderBy(x => x).ToArray();
                    }
                }
            }

            return allBwkArray;
        }

        /// <summary>
        /// Method to add budget line items to a given working budget domain entity from a list of BWK.working budget record IDs.
        /// </summary>
        /// <param name="workingBudgetEntity">The domain entity to which budget line items entities will be added.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="allBwkIds">A string array of all BWK.workingbudget record IDs assign to the user's budget officers.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="budgetConfigurationComparables">The list of budget comparables defined for the working budget.</param>
        /// <param name="majorComponentStartPosition">The list of major component start positions.</param>
        /// <returns>A working budget domain entity that has been updated with budget line items.</returns>
        private async Task<WorkingBudget2> AddBudgetLineItemsToWorkingBudgetEntity2Async(WorkingBudget2 workingBudgetEntity, string workingBudgetId, string[] allBwkIds,
            WorkingBudgetQueryCriteria criteria, int startPosition, int recordCount, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables, GeneralLedgerAccountStructure glAccountStructure)
        {
            IList<string> majorComponentStartPosition = glAccountStructure.MajorComponentStartPositions;

            // Set the specific BUD.WORK file suite name.
            var bwkSuiteFileName = "BWK." + workingBudgetId;

            // This method is only called if bwkIds contains any IDs.

            logger.Debug("List of all BUD.WORK IDs assigned to the user (allBwkIds):\n " + string.Join(", ", allBwkIds));

            // Apply the filter criteria to the list of resolved BUD.WORK IDs.
            // To avoid a selection statment that may reach the length limit, 
            // do a SelectAsync for each criteria element.
            // The returned budget accounts are sorted by the user criteria if they selected any.
            string[] filteredBwkIds = await ApplyFilterCriteriaAsync(allBwkIds, criteria, bwkSuiteFileName);

            logger.Debug("After ApplyFilterCriteriaAsync.");
            logger.Debug("List of BUD.WORK IDs after applying filter criteria (filteredBwkIds):\n " + string.Join(", ", filteredBwkIds));

            if (filteredBwkIds != null && filteredBwkIds.Any())
            {
                // Set the count of all possible budget line items for the working budget.
                int totalLineItems = filteredBwkIds.Count();
                workingBudgetEntity.TotalLineItems = totalLineItems;

                logger.Debug(string.Format("Total number of budget accounts after applying filter and before paging is {0}.", totalLineItems));

                // Obtain the specific range requested for pagination.
                string[] pagedBwksArray = CalculateRequestedLineItems(startPosition, recordCount, filteredBwkIds.ToArray());

                logger.Debug("After CalculateRequestedLineItems.");
                logger.Debug("List of BUD.WORK IDs after applying paging (pagedBwksArray):\n " + string.Join(", ", pagedBwksArray));

                if (pagedBwksArray != null && pagedBwksArray.Any())
                {
                    // filteredBwkIds contains the selected/sorted budget accounts for all possible pages.
                    // pagedBwksArray contains the budget accounts for the requested page.

                    // The array of BWK IDs to bulkread. If no subtotals, it will contain pagedBwksArray.
                    // If there are subtotals, it will contain additional IDs needed for subtotal calculation.
                    string[] fullRangeSubtotalsBwkIds = null;

                    // The user may choose, for instance, to sort by Fund, by Department, by Location and subtotal on Department and Location.
                    // In this case Department will be the first subtotal and Location will be the second subtotal.
                    // If they subtotal on all three, Fund will be the first subtotal, Department will be the second subtotal and Location will be the third subtotal.

                    // If there is sort/subtotals in the criteria, determine if subtotals are requested.
                    int noOfSubtotals = 0;
                    bool hasChosenSubtotals = false;
                    // If the start position is 0, the user has requested the first page.
                    bool isFirstPage = false;
                    if (startPosition == 0)
                    {
                        isFirstPage = true;
                    }
                    // Has the user requested the last page or there is only one page.
                    bool isLastPage = false;
                    // Does the page have any subtotals displayed.
                    bool pageHasSubtotals = false;

                    string[] subtotalName = new string[3];
                    string[] subtotalType = new string[3];
                    int[] subtotalOrder = new int[3];
                    int[] subtotalStartPositions = new int[3];
                    int[] subtotalStartLengths = new int[3];

                    // If there is sort/subtotal criteria, set up the information.
                    if (criteria != null && criteria.SortSubtotalComponentQueryCriteria.Any())
                    {
                        // Obtain and sort the components from the filter that are subtotals.
                        var subtotalComponents = criteria.SortSubtotalComponentQueryCriteria.Where(x => x.IsDisplaySubTotal == true).OrderBy(x => x.Order);

                        // Populate the subtotal arrays used for value comparison.
                        int j = 0;
                        for (int i = 0; i < subtotalComponents.Count(); i++)
                        {
                            hasChosenSubtotals = true;
                            var sortComp = subtotalComponents.ElementAt(i);

                            // If the subtotal object does not have a type, something is wrong.
                            // Skip this subtotal.
                            if (!string.IsNullOrWhiteSpace(sortComp.SubtotalType))
                            {
                                // If the subtotal is by budget officer .
                                if (sortComp.SubtotalType == "BO")
                                {
                                    subtotalType[j] = "BO";
                                    subtotalName[j] = "Budget Officer";
                                    subtotalOrder[j] = j + 1;
                                    j++;
                                }
                                // If the subtotal is a GL account component/subcomponent.
                                else
                                {
                                    var subtotalComponent = glAccountStructure.Subcomponents.FirstOrDefault(x => x.ComponentName.ToUpperInvariant() == sortComp.SubtotalName.ToUpperInvariant());
                                    if (subtotalComponent != null)
                                    {
                                        subtotalStartPositions[j] = subtotalComponent.StartPosition;
                                        subtotalStartLengths[j] = subtotalComponent.ComponentLength;
                                        subtotalName[j] = sortComp.SubtotalName.ToUpperInvariant();
                                        subtotalType[j] = "GL";
                                        subtotalOrder[j] = j + 1;
                                        j++;
                                    }
                                    else
                                    {
                                        logger.Debug(string.Format("Cannot find subtotal {0} in the GL account structure; skip it.", i));
                                    }
                                }
                            }
                            else
                            {

                                logger.Debug(string.Format("Subtotal number {0} does not have a type; skip it.", i));
                            }

                            // Get the number of subtotals.
                            // j is the position which has been increased by one, so it matches the number.
                            noOfSubtotals = j;
                        }

                        logger.Debug("subtotalType " + string.Join(", ", subtotalType));
                        logger.Debug("subtotalName " + string.Join(", ", subtotalName));
                        logger.Debug("subtotalOrder " + string.Join(", ", subtotalOrder));
                        logger.Debug("subtotalStartPositions " + string.Join(", ", subtotalStartPositions));
                        logger.Debug("subtotalStartLengths " + string.Join(", ", subtotalStartLengths));
                    }

                    // Obtain the first budget account ID for the requested page.
                    string firstPagedBudgetAccountId = pagedBwksArray[0];
                    // Obtain the last budget account ID for the requested page.
                    string lastPagedBudgetAccountId = pagedBwksArray[pagedBwksArray.Length - 1];
                    // Obtain the last budget account for the working budget after the filter was applied.
                    string lastFilteredBudgetAccountId = filteredBwkIds[filteredBwkIds.Length - 1];

                    // Index of the first budget account in the requested page in the list of filtered BWK IDs.
                    int firstPagedBudgetAccountIndex = 0;
                    // Index of the last budget account in the requested page in the list of filtered BWK IDs.
                    // Initialize it to the last position of the selected IDs.
                    int lastPagedBudgetAccountIndex = 0;

                    // If there are no subtotals do not read more BUD.WORK records than we need to.
                    if (!hasChosenSubtotals)
                    {
                        fullRangeSubtotalsBwkIds = pagedBwksArray;
                        // Set the first budget account to the first position.
                        firstPagedBudgetAccountIndex = 0;
                        // Get the position for the last paged budget account.
                        // This is the position index relative to 0.
                        lastPagedBudgetAccountIndex = pagedBwksArray.Length - 1;

                        logger.Debug("No subtotals chosen.");
                        logger.Debug(string.Format("firstPagedBudgetAccountIndex is {0}", firstPagedBudgetAccountIndex));
                        logger.Debug(string.Format("lastPagedBudgetAccountIndex is {0}", lastPagedBudgetAccountIndex));
                        logger.Debug("List of BUD.WORK IDs to read (fullRangeSubtotalsBwkIds):\n " + string.Join(", ", fullRangeSubtotalsBwkIds));
                    }
                    else
                    {
                        // If the last budget account in the page is the last budget account in the
                        // working budget, the user has requested the last page, or there is only
                        // one page for the working budget.
                        if (lastPagedBudgetAccountId == lastFilteredBudgetAccountId)
                        {
                            isLastPage = true;
                            // We need to read all the selected BUD.WORK records.
                            fullRangeSubtotalsBwkIds = filteredBwkIds;
                            // Set the index of the first budget account in the requested page in the list of filtered BWK IDs.
                            firstPagedBudgetAccountIndex = Array.IndexOf(filteredBwkIds, firstPagedBudgetAccountId);
                            // Set the index of the last budget account to the last one.
                            lastPagedBudgetAccountIndex = filteredBwkIds.Length - 1;

                            logger.Debug(string.Format("Subtotals chosen; it is the last page."));
                            logger.Debug(string.Format("firstPagedBudgetAccountIndex is {0}", firstPagedBudgetAccountIndex));
                            logger.Debug(string.Format("lastPagedBudgetAccountIndex is {0}", lastPagedBudgetAccountIndex));
                            logger.Debug("List of BUD.WORK IDs to read (fullRangeSubtotalsBwkIds):\n " + string.Join(", ", fullRangeSubtotalsBwkIds));
                        }
                        else
                        {
                            //The user has requested a page other than the last one.
                            // Get the position for the first paged budget account.
                            firstPagedBudgetAccountIndex = Array.IndexOf(filteredBwkIds, firstPagedBudgetAccountId);
                            // Get the position for the last paged budget account.
                            // This is the position index relative to 0.
                            lastPagedBudgetAccountIndex = Array.IndexOf(filteredBwkIds, lastPagedBudgetAccountId);
                            // The number of records to read is from the first one to the last one on the page. 
                            // lastPagedBudgetAccountIndex gives us the position index of the last one on the page
                            // so we have to add one to offset the index. Then we need to add another one because we
                            // also need to read the first record of the next page.
                            var numberBwkRecordsToRead = lastPagedBudgetAccountIndex + 2;

                            // The BUD.WORK records to read will be from the first one selected to the last one in the page plus one from the next page.
                            fullRangeSubtotalsBwkIds = filteredBwkIds.Take(numberBwkRecordsToRead).ToArray();

                            logger.Debug(string.Format("Subtotals chosen; it is not the last page."));
                            logger.Debug(string.Format("firstPagedBudgetAccountIndex is {0}", firstPagedBudgetAccountIndex));
                            logger.Debug(string.Format("lastPagedBudgetAccountIndex is {0}", lastPagedBudgetAccountIndex));
                            logger.Debug("List of BUD.WORK IDs to read (fullRangeSubtotalsBwkIds):\n " + string.Join(", ", fullRangeSubtotalsBwkIds));
                        }
                    }

                    // Read a specific range of BWK records from the ones selected depending on whether there are subtotals,
                    // what page has been requested and the number of budget line items per page.
                    var budWorkDataContracts = await DataReader.BulkReadRecordAsync<BudWork>(bwkSuiteFileName, fullRangeSubtotalsBwkIds, true);
                    if (budWorkDataContracts != null && budWorkDataContracts.Any())
                    {
                        // Initialize the sequence number for the line items to display.
                        int sequenceNo = 0;
                        string[] subtotalValues = new string[3];
                        long[] subtotalWorkingAmount = new long[3];
                        long[] subtotalBaseAmount = new long[3];
                        long[] subtotalC1Amount = new long[3];
                        long[] subtotalC2Amount = new long[3];
                        long[] subtotalC3Amount = new long[3];
                        long[] subtotalC4Amount = new long[3];
                        long[] subtotalC5Amount = new long[3];
                        bool firstBudgetAccount = true;
                        bool subtotal1Detected = false;
                        int firstSubtotal1DisplayLine = 0;
                        bool subtotal2Detected = false;
                        int firstSubtotal2DisplayLine = 0;
                        bool subtotal3Detected = false;
                        int firstSubtotal3DisplayLine = 0;

                        // Add only the comparables that are defined in the budget development configuration.
                        List<string> comparableIds = budgetConfigurationComparables.Select(x => x.ComparableId).ToList();
                        bool IsComp1Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C1")).Any();
                        bool IsComp2Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C2")).Any();
                        bool IsComp3Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C3")).Any();
                        bool IsComp4Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C4")).Any();
                        bool IsComp5Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C5")).Any();

                        // Loop through each selected BUD.WORK record from the first to the last budget accounts on the PAGE.
                        // Do not process the last record because that is the first budget account of the next page.
                        // Do not process any possible preceding records because those are used for subtotals only.

                        logger.Debug(string.Format("Reading BUD.WORK records from (firstPagedBudgetAccountIndex) {0} to (lastPagedBudgetAccountIndex) {1}.", firstPagedBudgetAccountIndex, lastPagedBudgetAccountIndex));

                        for (int b = firstPagedBudgetAccountIndex; b <= lastPagedBudgetAccountIndex; b++)
                        {
                            var budworkRecord = budWorkDataContracts[b];

                            // Every BWK record must have an expense account. Otherwise, it is corrupt data, and the data is not
                            // included in the domain entity.
                            if (string.IsNullOrEmpty(budworkRecord.BwExpenseAcct))
                            {
                                //log the data error and exclude the budget line item from the working budget that is returned.
                                LogDataError("BW.EXPENSE.ACCOUNT", budworkRecord.Recordkey, budworkRecord);
                            }
                            else
                            {
                                if (hasChosenSubtotals)
                                {
                                    for (int i = 0; i < noOfSubtotals; i++)
                                    {
                                        // If this is the first record, do not force a subtotal.
                                        if (firstBudgetAccount)
                                        {
                                            // Set up the subtotal values from the first account.
                                            if (subtotalType[i] == "BO")
                                            {
                                                subtotalValues[i] = budworkRecord.BwOfcrLink;
                                            }
                                            else
                                            {

                                                subtotalValues[i] = budworkRecord.Recordkey.Substring(subtotalStartPositions[i], subtotalStartLengths[i]);
                                            }

                                            if (i == noOfSubtotals - 1)
                                            {
                                                firstBudgetAccount = false;
                                            }
                                        }

                                        var subValue = "";
                                        if (subtotalType[i] == "BO")
                                        {
                                            subValue = budworkRecord.BwOfcrLink;
                                        }
                                        else
                                        {
                                            subValue = budworkRecord.Recordkey.Substring(subtotalStartPositions[i], subtotalStartLengths[i]);
                                        }

                                        if (subtotalValues[i] != subValue)
                                        {
                                            // If we have 3 subtotal components, and this break if for the second subtotal,
                                            // we also have to subtotal the third component before we subtotal the second.
                                            for (int j = noOfSubtotals - 1; j >= i; j--)
                                            {
                                                pageHasSubtotals = true;

                                                // Increase the items display order count.
                                                sequenceNo += 1;
                                                var subtotalLineItem = new Domain.BudgetManagement.Entities.LineItem(sequenceNo);
                                                var subtotalItem = new SubtotalLineItem();

                                                subtotalItem.SubtotalType = subtotalType[j];
                                                subtotalItem.SubtotalOrder = subtotalOrder[j];
                                                subtotalItem.SubtotalValue = subtotalValues[j];
                                                subtotalItem.SubtotalName = subtotalName[j];
                                                subtotalItem.SubtotalWorkingAmount = subtotalWorkingAmount[j];
                                                subtotalItem.SubtotalBaseBudgetAmount = subtotalBaseAmount[j];
                                                if (IsComp1Defined)
                                                {
                                                    subtotalItem.AddBudgetComparable(new BudgetComparable("C1")
                                                    {
                                                        ComparableAmount = subtotalC1Amount[j]
                                                    });
                                                }
                                                if (IsComp2Defined)
                                                {
                                                    subtotalItem.AddBudgetComparable(new BudgetComparable("C2")
                                                    {
                                                        ComparableAmount = subtotalC2Amount[j]
                                                    });
                                                }
                                                if (IsComp3Defined)
                                                {
                                                    subtotalItem.AddBudgetComparable(new BudgetComparable("C3")
                                                    {
                                                        ComparableAmount = subtotalC3Amount[j]
                                                    });
                                                }
                                                if (IsComp4Defined)
                                                {
                                                    subtotalItem.AddBudgetComparable(new BudgetComparable("C4")
                                                    {
                                                        ComparableAmount = subtotalC4Amount[j]
                                                    });
                                                }
                                                if (IsComp5Defined)
                                                {
                                                    subtotalItem.AddBudgetComparable(new BudgetComparable("C5")
                                                    {
                                                        ComparableAmount = subtotalC5Amount[j]
                                                    });
                                                }

                                                // Assign the line item and add it to the working budget.
                                                subtotalLineItem.SubtotalLineItem = subtotalItem;
                                                workingBudgetEntity.AddLineItem(subtotalLineItem);

                                                logger.Debug(string.Format("Created subtotal line with sequence number {0} for value {1} name {2}.", sequenceNo, subtotalItem.SubtotalValue, subtotalItem.SubtotalName));

                                                // Reset the applicable subtotal comparison values.
                                                if (subtotalType[j] == "BO")
                                                {
                                                    subtotalValues[j] = budworkRecord.BwOfcrLink;
                                                }
                                                else
                                                {
                                                    subtotalValues[j] = budworkRecord.Recordkey.Substring(subtotalStartPositions[j], subtotalStartLengths[j]);
                                                }

                                                logger.Debug(string.Format("Reset subtotal arrays to subtotal type {0} subtotal value {1} subtotal name {2}.", subtotalType[j], subtotalValues[j], subtotalName[j]));

                                                // Reset the applicable subtotal amounts.
                                                subtotalWorkingAmount[j] = 0;
                                                subtotalBaseAmount[j] = 0;
                                                subtotalC1Amount[j] = 0;
                                                subtotalC2Amount[j] = 0;
                                                subtotalC3Amount[j] = 0;
                                                subtotalC4Amount[j] = 0;
                                                subtotalC5Amount[j] = 0;

                                                // Set up variables that will help us determine later whether we need
                                                // to look at budget accounts from previous pages to update subtotals.
                                                switch (j)
                                                {
                                                    case 0:
                                                        if (!subtotal1Detected)
                                                        {
                                                            subtotal1Detected = true;
                                                            firstSubtotal1DisplayLine = sequenceNo;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (!subtotal2Detected)
                                                        {
                                                            subtotal2Detected = true;
                                                            firstSubtotal2DisplayLine = sequenceNo;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (!subtotal3Detected)
                                                        {
                                                            subtotal3Detected = true;
                                                            firstSubtotal3DisplayLine = sequenceNo;
                                                        }
                                                        break;
                                                    default:
                                                        logger.Debug(string.Format("Trying to detect a subtotal number has incorrect value {0}", j));
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Increase the items display order count.
                                sequenceNo += 1;
                                var lineItem = new Domain.BudgetManagement.Entities.LineItem(sequenceNo);
                                var budgetLineItem = new BudgetLineItem(budworkRecord.BwExpenseAcct);

                                // Get the formatted GL account.
                                budgetLineItem.FormattedBudgetAccountId = GlAccountUtility.ConvertGlAccountToExternalFormat(budworkRecord.BwExpenseAcct, majorComponentStartPosition);

                                // Convert the justification notes into a paragraph.
                                var notes = string.Empty;
                                if (budworkRecord.BwNotes != null && budworkRecord.BwNotes.Any())
                                {
                                    var justificationNotes = new StringBuilder();
                                    foreach (var note in budworkRecord.BwNotes)
                                    {
                                        if (string.IsNullOrWhiteSpace(note))
                                        {
                                            justificationNotes.Append(Environment.NewLine + Environment.NewLine);
                                        }
                                        else
                                        {
                                            if (justificationNotes.Length > 0)
                                            {
                                                justificationNotes.Append(" ");
                                            }
                                            justificationNotes.Append(note);
                                        }
                                    }

                                    notes = justificationNotes.ToString();
                                }
                                budgetLineItem.JustificationNotes = notes;

                                // Assign the budget officer ID to the line item.
                                budgetLineItem.BudgetOfficer = new BudgetOfficer(budworkRecord.BwOfcrLink);

                                // Assign the reporting unit ID to the line item.
                                budgetLineItem.BudgetReportingUnit = new BudgetReportingUnit(budworkRecord.BwControlLink);

                                // Get the base budget amount for the budget line item. If the budget line item does not have a BASE version, log a data error beca
                                if (budworkRecord.BwfreezeEntityAssociation != null && budworkRecord.BwfreezeEntityAssociation.Any())
                                {
                                    var baseAmountRow = budworkRecord.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                                    if (baseAmountRow != null)
                                    {
                                        budgetLineItem.BaseBudgetAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                                    }
                                    else
                                    {
                                        LogDataError("BW.VERSION", budworkRecord.Recordkey, budworkRecord);
                                        budgetLineItem.BaseBudgetAmount = 0;
                                    }
                                }

                                // get the working budget amount for the line item.
                                budgetLineItem.WorkingAmount = budworkRecord.BwLineAmt.HasValue ? budworkRecord.BwLineAmt.Value : 0;

                                if (budworkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                                {
                                    budgetLineItem.AddBudgetComparable(new BudgetComparable("C1")
                                    {
                                        ComparableAmount = budworkRecord.BwComp1Amount.Value
                                    });
                                }

                                if (budworkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                                {
                                    budgetLineItem.AddBudgetComparable(new BudgetComparable("C2")
                                    {
                                        ComparableAmount = budworkRecord.BwComp2Amount.Value
                                    });
                                }

                                if (budworkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                                {
                                    budgetLineItem.AddBudgetComparable(new BudgetComparable("C3")
                                    {
                                        ComparableAmount = budworkRecord.BwComp3Amount.Value
                                    });
                                }

                                if (budworkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                                {
                                    budgetLineItem.AddBudgetComparable(new BudgetComparable("C4")
                                    {
                                        ComparableAmount = budworkRecord.BwComp4Amount.Value
                                    });
                                }

                                if (budworkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                                {
                                    budgetLineItem.AddBudgetComparable(new BudgetComparable("C5")
                                    {
                                        ComparableAmount = budworkRecord.BwComp5Amount.Value
                                    });
                                }

                                if (hasChosenSubtotals)
                                {
                                    // Add the amounts to the subtotals.
                                    for (int i = 0; i < noOfSubtotals; i++)
                                    {
                                        subtotalWorkingAmount[i] += budgetLineItem.WorkingAmount;
                                        subtotalBaseAmount[i] += budgetLineItem.BaseBudgetAmount;
                                        // Add the budget line item first comparable amount to the subtotals.
                                        if (budworkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                                        {
                                            subtotalC1Amount[i] += budworkRecord.BwComp1Amount.Value;
                                        }
                                        // Add the budget line item second comparable amount to the subtotals.
                                        if (budworkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                                        {
                                            subtotalC2Amount[i] += budworkRecord.BwComp2Amount.Value;
                                        }
                                        // Add the budget line item third comparable amount to the subtotals.
                                        if (budworkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                                        {
                                            subtotalC3Amount[i] += budworkRecord.BwComp3Amount.Value;
                                        }
                                        // Add the budget line item fourth comparable amount to the subtotals.
                                        if (budworkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                                        {
                                            subtotalC4Amount[i] += budworkRecord.BwComp4Amount.Value;
                                        }
                                        // Add the budget line item fifth comparable amount to the subtotals.
                                        if (budworkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                                        {
                                            subtotalC5Amount[i] += budworkRecord.BwComp5Amount.Value;
                                        }
                                    }
                                }

                                // Add the budget line item to the to the list of line items in the working budget domain entity.
                                lineItem.BudgetLineItem = budgetLineItem;
                                workingBudgetEntity.AddLineItem(lineItem);
                            }
                        }

                        // If there is at least one subtotal.
                        // If it is the first page, we don't need to look at any previous budget accounts, but
                        // we have to check whether we need to display subtotals at the end of the page.
                        // If it is the last page, we need to print all subtotals, and we also need to look at
                        // budget accounts on previous pages to update subtotals if necessary.
                        // If it is a page in the middle, we need to look at budget accounts on previous pages 
                        // to possibly update subtotals on the page if there are any displaying, and we need to 
                        // look at the first budget account on the next page to see if we have to display subtotals 
                        // at the end of the page.

                        if (hasChosenSubtotals)
                        {
                            // If it is not the last page (it is the first or a middle page) we need to determine if
                            // any subtotal values change between the last budget account on this page and the first
                            // one on the next page, to see what subtotals, if any, have to display at the end of this page.
                            if (isLastPage)
                            {
                                logger.Debug("Adding subtotal lines to the end of the last page.");

                                // It is the last page so we need to display all calculated subtotals.
                                for (int j = noOfSubtotals - 1; j >= 0; j--)
                                {
                                    pageHasSubtotals = true;

                                    // Increase the items display order count.
                                    sequenceNo += 1;
                                    var subtotalLineItem = new Domain.BudgetManagement.Entities.LineItem(sequenceNo);
                                    var subtotalItem = new SubtotalLineItem();
                                    subtotalItem.SubtotalType = subtotalType[j];
                                    subtotalItem.SubtotalOrder = subtotalOrder[j];
                                    subtotalItem.SubtotalValue = subtotalValues[j];
                                    subtotalItem.SubtotalName = subtotalName[j];
                                    subtotalItem.SubtotalWorkingAmount = subtotalWorkingAmount[j];
                                    subtotalItem.SubtotalBaseBudgetAmount = subtotalBaseAmount[j];
                                    if (IsComp1Defined)
                                    {
                                        subtotalItem.AddBudgetComparable(new BudgetComparable("C1")
                                        {
                                            ComparableAmount = subtotalC1Amount[j]
                                        });
                                    }
                                    if (IsComp2Defined)
                                    {
                                        subtotalItem.AddBudgetComparable(new BudgetComparable("C2")
                                        {
                                            ComparableAmount = subtotalC2Amount[j]
                                        });
                                    }
                                    if (IsComp3Defined)
                                    {
                                        subtotalItem.AddBudgetComparable(new BudgetComparable("C3")
                                        {
                                            ComparableAmount = subtotalC3Amount[j]
                                        });
                                    }
                                    if (IsComp4Defined)
                                    {
                                        subtotalItem.AddBudgetComparable(new BudgetComparable("C4")
                                        {
                                            ComparableAmount = subtotalC4Amount[j]
                                        });
                                    }
                                    if (IsComp5Defined)
                                    {
                                        subtotalItem.AddBudgetComparable(new BudgetComparable("C5")
                                        {
                                            ComparableAmount = subtotalC5Amount[j]
                                        });
                                    }

                                    subtotalLineItem.SubtotalLineItem = subtotalItem;
                                    workingBudgetEntity.AddLineItem(subtotalLineItem);

                                    logger.Debug(string.Format("Adding subtotal line with sequence number {0}, subtotal type {1} subtotal value {2} subtotal name {3}.",
                                        sequenceNo, subtotalType[j], subtotalValues[j], subtotalName[j]));

                                    switch (j)
                                    {
                                        case 0:
                                            if (!subtotal1Detected)
                                            {
                                                subtotal1Detected = true;
                                                firstSubtotal1DisplayLine = sequenceNo;
                                            }
                                            break;
                                        case 1:
                                            if (!subtotal2Detected)
                                            {
                                                subtotal2Detected = true;
                                                firstSubtotal2DisplayLine = sequenceNo;
                                            }
                                            break;
                                        case 2:
                                            if (!subtotal3Detected)
                                            {
                                                subtotal3Detected = true;
                                                firstSubtotal3DisplayLine = sequenceNo;
                                            }
                                            break;
                                        default:
                                            logger.Debug(string.Format("Trying to detect a subtotal number when processing the last page has incorrect value {0}", j));
                                            break;
                                    }

                                }
                            }

                            // Determine if any subtotals have to be displayed at the end of the page.
                            if (!isLastPage)
                            {

                                logger.Debug("Adding subtotal lines to the end of a page that is not the last.");

                                // Assign the first budget account in the next page.
                                string nextBudgetAccountId = filteredBwkIds[lastPagedBudgetAccountIndex + 1];
                                var nextBudgetAccountIBwkRecord = budWorkDataContracts[lastPagedBudgetAccountIndex + 1];

                                // Look at the subtotal values for the first budget account on the next page.
                                // Add subtotal display line items for any applicable subtotals.
                                for (int i = 0; i < noOfSubtotals; i++)
                                {
                                    var subValue = "";
                                    if (subtotalType[i] == "BO")
                                    {
                                        subValue = nextBudgetAccountIBwkRecord.BwOfcrLink;
                                    }
                                    else
                                    {
                                        subValue = nextBudgetAccountId.Substring(subtotalStartPositions[i], subtotalStartLengths[i]);
                                    }

                                    if (subtotalValues[i] != subValue)
                                    {
                                        // If we have 3 subtotal components, and this break if for the second subtotal,
                                        // we also have to subtotal the third component before we subtotal the second.
                                        for (int j = noOfSubtotals - 1; j >= i; j--)
                                        {
                                            pageHasSubtotals = true;

                                            // Increase the items display order count.
                                            sequenceNo += 1;
                                            var subtotalLineItem = new Domain.BudgetManagement.Entities.LineItem(sequenceNo);
                                            var subtotalItem = new SubtotalLineItem();
                                            subtotalItem.SubtotalType = subtotalType[j];
                                            subtotalItem.SubtotalOrder = subtotalOrder[j];
                                            subtotalItem.SubtotalValue = subtotalValues[j];
                                            subtotalItem.SubtotalName = subtotalName[j];
                                            subtotalItem.SubtotalWorkingAmount = subtotalWorkingAmount[j];
                                            subtotalItem.SubtotalBaseBudgetAmount = subtotalBaseAmount[j];
                                            if (IsComp1Defined)
                                            {
                                                subtotalItem.AddBudgetComparable(new BudgetComparable("C1")
                                                {
                                                    ComparableAmount = subtotalC1Amount[j]
                                                });
                                            }
                                            if (IsComp2Defined)
                                            {
                                                subtotalItem.AddBudgetComparable(new BudgetComparable("C2")
                                                {
                                                    ComparableAmount = subtotalC2Amount[j]
                                                });
                                            }
                                            if (IsComp3Defined)
                                            {
                                                subtotalItem.AddBudgetComparable(new BudgetComparable("C3")
                                                {
                                                    ComparableAmount = subtotalC3Amount[j]
                                                });
                                            }
                                            if (IsComp4Defined)
                                            {
                                                subtotalItem.AddBudgetComparable(new BudgetComparable("C4")
                                                {
                                                    ComparableAmount = subtotalC4Amount[j]
                                                });
                                            }
                                            if (IsComp5Defined)
                                            {
                                                subtotalItem.AddBudgetComparable(new BudgetComparable("C5")
                                                {
                                                    ComparableAmount = subtotalC5Amount[j]
                                                });
                                            }

                                            subtotalLineItem.SubtotalLineItem = subtotalItem;
                                            workingBudgetEntity.AddLineItem(subtotalLineItem);

                                            logger.Debug(string.Format("Adding subtotal line with sequence number {0}, subtotal type {1} subtotal value {2} subtotal name {3}.",
                                                                          sequenceNo, subtotalType[j], subtotalValues[j], subtotalName[j]));

                                            switch (j)
                                            {
                                                case 0:
                                                    if (!subtotal1Detected)
                                                    {
                                                        subtotal1Detected = true;
                                                        firstSubtotal1DisplayLine = sequenceNo;
                                                    }
                                                    break;
                                                case 1:
                                                    if (!subtotal2Detected)
                                                    {
                                                        subtotal2Detected = true;
                                                        firstSubtotal2DisplayLine = sequenceNo;
                                                    }
                                                    break;
                                                case 2:
                                                    if (!subtotal3Detected)
                                                    {
                                                        subtotal3Detected = true;
                                                        firstSubtotal3DisplayLine = sequenceNo;
                                                    }
                                                    break;
                                                default:
                                                    logger.Debug(string.Format("Trying to detect a subtotal number when it is not the last page has incorrect value {0}", j));
                                                    break;
                                            }
                                        }
                                        // Exit the loop once the necessary subtotals have been added.
                                        i = noOfSubtotals;
                                    }
                                }
                            }

                            // If it is not the first page, if there are any subtotals on the page determine if they have to be updated.
                            // Start looping through budget accounts from previous pages to check if the subtotals on the page need
                            // to be updated with amounts from budget accounts in previous pages.
                            if (!isFirstPage && pageHasSubtotals)
                            {

                                logger.Debug("Checking to see if amounts for the subtotal lines on the page need to be updated with amounts from previous pages.");

                                // Obtain the subtotal line items.
                                var lineItemsWithSubtotals = workingBudgetEntity.LineItems.Where(x => x.SubtotalLineItem != null);

                                // Determine the position of the last budget account on the previous page to the one requested.
                                var firstPreviousBudgetAccountIndex = firstPagedBudgetAccountIndex - 1;

                                // There is one subtotal to check at least because hasChosenSubtotals is true.
                                string comparisonSubtotal1Value = "";
                                if (subtotalType[0] == "BO")
                                {
                                    // If the subtotal is budget officer, get the value from the first record on the page.
                                    comparisonSubtotal1Value = budWorkDataContracts[firstPagedBudgetAccountIndex].BwOfcrLink;
                                }
                                else
                                {
                                    // The subtotal is a GL account component/subcomponent that we get from the ID of the first budget account on the page.
                                    comparisonSubtotal1Value = firstPagedBudgetAccountId.Substring(subtotalStartPositions[0], subtotalStartLengths[0]);
                                }

                                // There is at least one subtotal chosen. Determine if there is a second and a third subtotal.
                                bool haveSubtotal2 = false;
                                bool haveSubtotal3 = false;
                                // If the user chose two subtotals, set the boolean for the second.
                                string comparisonSubtotal2Value = "";
                                string comparisonSubtotal3Value = "";
                                if (noOfSubtotals >= 2)
                                {
                                    haveSubtotal2 = true;
                                    // Set the comparison values in case we need to update a display line for the second subtotal.
                                    if (subtotalType[1] == "BO")
                                    {
                                        // If the subtotal is budget officer, get the value from the first record on the page.
                                        comparisonSubtotal2Value = budWorkDataContracts[firstPagedBudgetAccountIndex].BwOfcrLink;
                                    }
                                    else
                                    {
                                        // The subtotal is a GL account component/subcomponent that we get from the ID of the first budget account on the page.
                                        comparisonSubtotal2Value = firstPagedBudgetAccountId.Substring(subtotalStartPositions[1], subtotalStartLengths[1]);
                                    }
                                }
                                // If the user chose three subtotals, set the boolean for the second and the third.
                                if (noOfSubtotals == 3)
                                {
                                    haveSubtotal2 = true;
                                    haveSubtotal3 = true;
                                    // Set the comparison values in case we need to update a display line for the second subtotal.
                                    if (subtotalType[2] == "BO")
                                    {
                                        // If the subtotal is budget officer, get the value from the first record on the page.
                                        comparisonSubtotal3Value = budWorkDataContracts[firstPagedBudgetAccountIndex].BwOfcrLink;
                                    }
                                    else
                                    {
                                        // The subtotal is a GL account component/subcomponent that we get from the ID of the first budget account on the page.
                                        comparisonSubtotal3Value = firstPagedBudgetAccountId.Substring(subtotalStartPositions[2], subtotalStartLengths[2]);
                                    }
                                }
                                // Variables to determine if any subtotal line on the page was updated.
                                bool subtotal1Updated = false;
                                bool subtotal2Updated = false;
                                bool subtotal3Updated = false;

                                SubtotalLineItem subtotal1Item = new SubtotalLineItem();
                                SubtotalLineItem subtotal2Item = new SubtotalLineItem();
                                SubtotalLineItem subtotal3Item = new SubtotalLineItem();
                                // subtotal1Detected indicates that the page has at least one display line for the first subtotal chosen.
                                if (subtotal1Detected)
                                {
                                    subtotal1Item = lineItemsWithSubtotals.Where(y => y.SequenceNumber == firstSubtotal1DisplayLine).FirstOrDefault().SubtotalLineItem;
                                }
                                // subtotal2Detected indicates that the page has at least one display line for the second subtotal chosen.
                                if (subtotal2Detected)
                                {
                                    subtotal2Item = lineItemsWithSubtotals.Where(y => y.SequenceNumber == firstSubtotal2DisplayLine).FirstOrDefault().SubtotalLineItem;
                                }
                                // subtotal3Detected indicates that the page has at least one display line for the third subtotal chosen.
                                if (subtotal3Detected)
                                {
                                    subtotal3Item = lineItemsWithSubtotals.Where(y => y.SequenceNumber == firstSubtotal3DisplayLine).FirstOrDefault().SubtotalLineItem;
                                }

                                // Start looping backwards through the BUD.WORK records from the last one on the previous page.

                                logger.Debug(string.Format("Reading previous BUD.WORK records from (firstPreviousBudgetAccountIndex) {0}.", firstPreviousBudgetAccountIndex));

                                for (int i = firstPreviousBudgetAccountIndex; i >= 0; i--)
                                {
                                    var previousBudgetAccountId = filteredBwkIds[i];
                                    var previousBwkRecord = budWorkDataContracts[i];

                                    // Get the first subtotal value from this WK record/ID.
                                    // At this point we don't know if we have a second or third subtotals to check.
                                    var thisRecordSubtotal1Value = "";
                                    if (subtotalType[0] == "BO")
                                    {
                                        thisRecordSubtotal1Value = previousBwkRecord.BwOfcrLink;
                                    }
                                    else
                                    {
                                        thisRecordSubtotal1Value = previousBudgetAccountId.Substring(subtotalStartPositions[0], subtotalStartLengths[0]);
                                    }

                                    // Compare the first subtotal value between the first budget account on the page and this budget account from a previous page.
                                    // We still have to check the value even if subtotal1Detected is false in case there is still a subtotal2Detected or subtotal3Detected being processed.
                                    if (thisRecordSubtotal1Value == comparisonSubtotal1Value)
                                    {
                                        // If the requested page has a display line for the first subtotal, see if the subtotal 
                                        // amounts for this subtotal need to be updated with amounts from previous records.
                                        // Once the subtotal1 changes, we are done.
                                        if (subtotal1Detected)
                                        {
                                            // Obtain the line item that contains this first subtotal.
                                            if (subtotal1Item != null)
                                            {
                                                subtotal1Updated = true;

                                                // Update this subtotal line item with amounts from this budget account.
                                                subtotal1Item.SubtotalWorkingAmount += previousBwkRecord.BwLineAmt.HasValue ? previousBwkRecord.BwLineAmt.Value : 0;
                                                if (previousBwkRecord.BwfreezeEntityAssociation != null && previousBwkRecord.BwfreezeEntityAssociation.Any())
                                                {
                                                    var baseAmountRow = previousBwkRecord.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                                                    if (baseAmountRow != null)
                                                    {
                                                        subtotal1Item.SubtotalBaseBudgetAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                                                    }
                                                }
                                                if (previousBwkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                                                {
                                                    subtotal1Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C1").First().ComparableAmount += previousBwkRecord.BwComp1Amount.Value;
                                                }
                                                if (previousBwkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                                                {
                                                    subtotal1Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C2").First().ComparableAmount += previousBwkRecord.BwComp2Amount.Value;
                                                }
                                                if (previousBwkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                                                {
                                                    subtotal1Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C3").First().ComparableAmount += previousBwkRecord.BwComp3Amount.Value;
                                                }
                                                if (previousBwkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                                                {
                                                    subtotal1Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C4").First().ComparableAmount += previousBwkRecord.BwComp4Amount.Value;
                                                }
                                                if (previousBwkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                                                {
                                                    subtotal1Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C5").First().ComparableAmount += previousBwkRecord.BwComp5Amount.Value;
                                                }
                                            }
                                        }

                                        // If the requested page has a display line for the second subtotal, see if the subtotal 
                                        // amounts for this subtotal need to be updated with amounts from previous records.
                                        // If the value for the first subtotal changes, there is no need to check the second
                                        // subtotal because it implies a change for it also.
                                        if (haveSubtotal2)
                                        {
                                            var thisRecordSubtotal2Value = "";
                                            if (subtotalType[1] == "BO")
                                            {
                                                thisRecordSubtotal2Value = previousBwkRecord.BwOfcrLink;
                                            }
                                            else
                                            {
                                                thisRecordSubtotal2Value = previousBudgetAccountId.Substring(subtotalStartPositions[1], subtotalStartLengths[1]);
                                            }

                                            // We still have to check the value even if subtotal2Detected is false in case there is still a subtotal3Detected being processed.
                                            if (thisRecordSubtotal2Value == comparisonSubtotal2Value)
                                            {
                                                if (subtotal2Detected)
                                                {
                                                    // Obtain the line item that contains this first subtotal.
                                                    if (subtotal2Item != null)
                                                    {
                                                        subtotal2Updated = true;
                                                        // Update this subtotal line item with amounts from this budget account.
                                                        subtotal2Item.SubtotalWorkingAmount += previousBwkRecord.BwLineAmt.HasValue ? previousBwkRecord.BwLineAmt.Value : 0;
                                                        if (previousBwkRecord.BwfreezeEntityAssociation != null && previousBwkRecord.BwfreezeEntityAssociation.Any())
                                                        {
                                                            var baseAmountRow = previousBwkRecord.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                                                            if (baseAmountRow != null)
                                                            {
                                                                subtotal2Item.SubtotalBaseBudgetAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                                                            }
                                                        }
                                                        if (previousBwkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                                                        {
                                                            subtotal2Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C1").First().ComparableAmount += previousBwkRecord.BwComp1Amount.Value;
                                                        }
                                                        if (previousBwkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                                                        {
                                                            subtotal2Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C2").First().ComparableAmount += previousBwkRecord.BwComp2Amount.Value;
                                                        }
                                                        if (previousBwkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                                                        {
                                                            subtotal2Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C3").First().ComparableAmount += previousBwkRecord.BwComp3Amount.Value;
                                                        }
                                                        if (previousBwkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                                                        {
                                                            subtotal2Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C4").First().ComparableAmount += previousBwkRecord.BwComp4Amount.Value;
                                                        }
                                                        if (previousBwkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                                                        {
                                                            subtotal2Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C5").First().ComparableAmount += previousBwkRecord.BwComp5Amount.Value;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // We are done with the second and third subtotal.
                                                subtotal2Detected = false;
                                                subtotal3Detected = false;
                                                // If there is no line item for the first subtotal on the page, stop reading records.
                                                if (!subtotal1Detected)
                                                {
                                                    // Stop reading previous budget accounts.
                                                    i = -1;
                                                }
                                            }

                                            // If the requested page has a display line for the third subtotal, see if the subtotal 
                                            // amounts for this subtotal need to be updated with amounts from previous records.
                                            // If the value for the first or second subtotal changes, there is no need to check
                                            // the third subtotal because it implies a change for it also.
                                            if (haveSubtotal3)
                                            {
                                                var thisRecordSubtotal3Value = "";
                                                if (subtotalType[2] == "BO")
                                                {
                                                    thisRecordSubtotal3Value = previousBwkRecord.BwOfcrLink;
                                                }
                                                else
                                                {
                                                    thisRecordSubtotal3Value = previousBudgetAccountId.Substring(subtotalStartPositions[2], subtotalStartLengths[2]);
                                                }
                                                if (thisRecordSubtotal3Value == comparisonSubtotal3Value)
                                                {
                                                    if (subtotal3Detected)
                                                    {
                                                        // Obtain the line item that contains this first subtotal.
                                                        if (subtotal3Item != null)
                                                        {
                                                            subtotal3Updated = true;
                                                            // Update this subtotal line item with amounts from this budget account.
                                                            subtotal3Item.SubtotalWorkingAmount += previousBwkRecord.BwLineAmt.HasValue ? previousBwkRecord.BwLineAmt.Value : 0;
                                                            if (previousBwkRecord.BwfreezeEntityAssociation != null && previousBwkRecord.BwfreezeEntityAssociation.Any())
                                                            {
                                                                var baseAmountRow = previousBwkRecord.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                                                                if (baseAmountRow != null)
                                                                {
                                                                    subtotal3Item.SubtotalBaseBudgetAmount += baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                                                                }
                                                            }
                                                            if (previousBwkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                                                            {
                                                                subtotal3Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C1").First().ComparableAmount += previousBwkRecord.BwComp1Amount.Value;
                                                            }
                                                            if (previousBwkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                                                            {
                                                                subtotal3Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C2").First().ComparableAmount += previousBwkRecord.BwComp2Amount.Value;
                                                            }
                                                            if (previousBwkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                                                            {
                                                                subtotal3Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C3").First().ComparableAmount += previousBwkRecord.BwComp3Amount.Value;
                                                            }
                                                            if (previousBwkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                                                            {
                                                                subtotal3Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C4").First().ComparableAmount += previousBwkRecord.BwComp4Amount.Value;
                                                            }
                                                            if (previousBwkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                                                            {
                                                                subtotal3Item.SubtotalBudgetComparables.Where(c => c.ComparableNumber == "C5").First().ComparableAmount += previousBwkRecord.BwComp5Amount.Value;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // We are done with the third subtotal.
                                                    subtotal3Detected = false;
                                                    // If there is no line item for the first subtotal on the page, stop reading records.
                                                    if (!subtotal1Detected && !subtotal2Detected)
                                                    {
                                                        // Stop reading previous budget accounts.
                                                        i = -1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // We are done with any other subtotals.
                                        subtotal2Detected = false;
                                        subtotal3Detected = false;
                                        // Stop reading previous budget accounts.
                                        i = -1;
                                    }
                                }

                                // Update the applicable subtotal line items in the working budget with the new amounts.
                                if (subtotal1Updated)
                                {
                                    workingBudgetEntity.LineItems.Where(y => y.SequenceNumber == firstSubtotal1DisplayLine).FirstOrDefault().SubtotalLineItem = subtotal1Item;

                                    logger.Debug(string.Format("Amounts updated for subtotal line with sequence number {0}, subtotal type {1} subtotal value {2} subtotal name {3}.",
                                                                  subtotal1Item.SubtotalOrder, subtotal1Item.SubtotalType, subtotal1Item.SubtotalValue, subtotal1Item.SubtotalName));

                                }
                                if (subtotal2Updated)
                                {
                                    workingBudgetEntity.LineItems.Where(y => y.SequenceNumber == firstSubtotal2DisplayLine).FirstOrDefault().SubtotalLineItem = subtotal2Item;

                                    logger.Debug(string.Format("Amounts updated for subtotal line with sequence number {0}, subtotal type {1} subtotal value {2} subtotal name {3}.",
                                                                  subtotal2Item.SubtotalOrder, subtotal2Item.SubtotalType, subtotal2Item.SubtotalValue, subtotal2Item.SubtotalName));
                                }
                                if (subtotal3Updated)
                                {
                                    workingBudgetEntity.LineItems.Where(y => y.SequenceNumber == firstSubtotal3DisplayLine).FirstOrDefault().SubtotalLineItem = subtotal3Item;

                                    logger.Debug(string.Format("Amounts updated for subtotal line with sequence number {0}, subtotal type {1} subtotal value {2} subtotal name {3}.",
                                                                  subtotal3Item.SubtotalOrder, subtotal3Item.SubtotalType, subtotal3Item.SubtotalValue, subtotal3Item.SubtotalName));
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.Debug("pagedBwksArray is null or empty. No budget line items returned.");
                }
            }
            else
            {
                logger.Debug("filteredBwkIds is null or empty. No budget line items returned.");
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Reduce the full list of budget line items to the requested set of budget line items.
        /// </summary>
        /// <param name="startPosition">Start position of the set of requested budget line items.</param>
        /// <param name="recordCount">Number of requested budget line items.</param>
        /// <param name="bwkIds">Sorted subset string array of all budget line items for the working budget for the user.</param>
        /// <returns></returns>
        private static string[] CalculateRequestedLineItems(int startPosition, int recordCount, string[] bwkIds)
        {
            // Sort the list of BWK record IDs, and then calculate the specified range of IDs.
            var tempBwkIds = bwkIds;
            int arrayCount = tempBwkIds.Length;
            string[] bwkArray;

            int possibleReturnCount;

            // If the requested start position is negative, set it to the first position.
            if (startPosition < 0)
            {
                startPosition = 0;
            }
            else
            {
                // if the requested start position is greater than the number of BWK 
                // record IDs, set it to the first position.
                if (startPosition >= arrayCount)
                {
                    startPosition = 0;
                }
            }

            // If the record count requested is negative, set it to 1.
            if (recordCount <= 0)
            {
                recordCount = 1;
            }

            // If the record count requested is more than what can be returned given the
            // start position, set it to what can be returned.
            possibleReturnCount = arrayCount - startPosition;
            if (recordCount > possibleReturnCount)
            {
                recordCount = possibleReturnCount;
            }

            bwkArray = tempBwkIds.Skip(startPosition).Take(recordCount).ToArray();
            return bwkArray;
        }

        /// <summary>
        /// Apply the filter query criteria to the list of budget accounts including any sort criteria.
        /// </summary>
        /// <param name="allBwkIds">List of BUD.WORK IDs assigned to the user's budget officers.</param>
        /// <param name="criteria">Query filter criteria.</param>
        /// <param name="bwkSuiteFileName">The BUD.WORK file suite file.</param>
        /// <returns>An array containing the filtered and sorted budget accounts.</returns>
        private async Task<string[]> ApplyFilterCriteriaAsync(string[] allBwkIds, WorkingBudgetQueryCriteria criteria, string bwkSuiteFileName)
        {
            logger.Debug("About to apply filter criteria.");

            string[] filteredBudgetAccountIDs = allBwkIds;
            if (criteria != null && criteria.ComponentCriteria.Any())
            {
                string componentFilterCriteria = string.Empty;
                string valueFilterCriteria = string.Empty;
                string rangeFilterCriteria = string.Empty;
                string[] tempBwkIds;
                foreach (var filterComp in criteria.ComponentCriteria)
                {
                    // Set a running limiting list of budget accounts that are filtered using the filtering criteria for each component.
                    // We are selecting against a file for the BUK.WORK template so the components need to have the prefix 'BWK.'
                    if (filterComp != null && filteredBudgetAccountIDs.Any())
                    {
                        tempBwkIds = filteredBudgetAccountIDs;
                        valueFilterCriteria = string.Empty;
                        rangeFilterCriteria = string.Empty;
                        componentFilterCriteria = string.Empty;

                        // Set the value filter criteria string from the individual component values.
                        if (filterComp.IndividualComponentValues != null && filterComp.IndividualComponentValues.Any())
                        {
                            foreach (var value in filterComp.IndividualComponentValues)
                            {
                                valueFilterCriteria = valueFilterCriteria + "'" + value + "' ";
                            }
                        }

                        // Set the range filter criteria string from the component range values.
                        if (filterComp.RangeComponentValues != null && filterComp.RangeComponentValues.Any())
                        {
                            foreach (var range in filterComp.RangeComponentValues)
                            {
                                if (range != null && filterComp.ComponentName != null && range.StartValue != null && range.EndValue != null)
                                {
                                    if (string.IsNullOrEmpty(rangeFilterCriteria))
                                    {
                                        rangeFilterCriteria = rangeFilterCriteria + "(BWK." + filterComp.ComponentName.ToUpperInvariant() + " GE '"
                                            + range.StartValue + "' AND BWK." + filterComp.ComponentName.ToUpperInvariant() + " LE '" + range.EndValue + "') ";
                                    }
                                    else
                                    {
                                        rangeFilterCriteria = rangeFilterCriteria + "OR (BWK." + filterComp.ComponentName.ToUpperInvariant() + " GE '"
                                            + range.StartValue + "' AND BWK." + filterComp.ComponentName.ToUpperInvariant() + " LE '" + range.EndValue + "') ";
                                    }
                                }
                            }
                        }

                        // Update the value filter criteria string to contain the component name.
                        if (!string.IsNullOrEmpty(valueFilterCriteria))
                        {
                            valueFilterCriteria = "BWK." + filterComp.ComponentName.ToUpperInvariant() + " EQ " + valueFilterCriteria;
                        }

                        // Set the full component filter criteria string based on the value filter string
                        // and the range value filter string.
                        componentFilterCriteria = valueFilterCriteria;
                        if (!string.IsNullOrEmpty(rangeFilterCriteria))
                        {
                            if (string.IsNullOrEmpty(componentFilterCriteria))
                            {
                                componentFilterCriteria = rangeFilterCriteria;
                            }
                            else
                            {
                                componentFilterCriteria = componentFilterCriteria + "OR " + rangeFilterCriteria;
                            }
                        }

                        logger.Debug(string.Format("About to SelectAsync componentFilterCriteria {0}", componentFilterCriteria));

                        // Select the filtered IDs for the specific BUD.WORK file.
                        // Each selection works off the previously selected list. The criteria is connected by ANDs.
                        filteredBudgetAccountIDs = await DataReader.SelectAsync(bwkSuiteFileName, tempBwkIds, componentFilterCriteria);
                    }
                }
            }

            // If there is any sort criteria, sort the selected budget accounts.
            if (criteria != null && criteria.SortSubtotalComponentQueryCriteria.Any() && filteredBudgetAccountIDs.Any())
            {
                string[] tempBwkIdsToSort;
                string sortFilterCriteria = string.Empty;

                // Sort the filter sort components.
                var SortSubtotalComponents = criteria.SortSubtotalComponentQueryCriteria.OrderBy(x => x.Order);

                foreach (var sortComp in SortSubtotalComponents)
                {
                    // Put the sort criteria together.
                    // We are selecting against a file for the BUK.WORK template so the components need to have the prefix 'BWK.'
                    if (sortComp != null && !string.IsNullOrWhiteSpace(sortComp.SubtotalName))
                    {
                        if (sortComp.SubtotalType == "BO")
                        {
                            // If one of the sort criterion is by budget officer.
                            sortFilterCriteria = sortFilterCriteria + "BY BW.OFCR.LINK" + " ";
                        }
                        else
                        {
                            // If one of the sort criterion is by a GL account component/subcomponent.
                            sortFilterCriteria = sortFilterCriteria + "BY BWK." + sortComp.SubtotalName.ToUpperInvariant() + " ";
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(sortFilterCriteria))
                {
                    // The budget accounts need to also be sorted by the ID.
                    sortFilterCriteria = sortFilterCriteria + "BY BUD.WORK.ID";

                    logger.Debug(string.Format("About to SelectAsync sort criteria sortFilterCriteria {0}", sortFilterCriteria));

                    tempBwkIdsToSort = filteredBudgetAccountIDs;
                    filteredBudgetAccountIDs = await DataReader.SelectAsync(bwkSuiteFileName, tempBwkIdsToSort, sortFilterCriteria);
                }
            }

            return filteredBudgetAccountIDs;
        }

        /// <summary>
        /// Given a PERSON ID, return the OPERS ID from the associated STAFF record.
        /// </summary>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <returns>An OPERS ID.</returns>
        private async Task<string> GetOperatorIdFromStaffRecordAsync(string currentUserPersonId)
        {
            // get the operator ID from the STAFF record associated with the PERSON ID.
            string userOperatorId = string.Empty;
            var staffContract = await DataReader.ReadRecordAsync<DataContracts.Staff>("STAFF", currentUserPersonId);
            if (staffContract == null)
            {
                // The general ledger user has no Staff record.
                logger.Error(string.Format("Person {0} has no Staff record.", currentUserPersonId));
            }
            else
            {
                if (string.IsNullOrEmpty(staffContract.StaffLoginId))
                {
                    // There is no login (operator ID) on the Staff record.
                    logger.Error(string.Format("Person {0} has an incomplete Staff record.", currentUserPersonId));
                }
                else
                {
                    userOperatorId = staffContract.StaffLoginId;
                }
            }
            return userOperatorId;
        }

        /// <summary>
        /// Method to populate the budget officers with login and name.
        /// </summary>
        /// <param name="workingBudgetEntity">The working budget domain entity.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <returns>A working budget domain entity whose budget line items have been updated with budget officer information.</returns>
        private async Task<WorkingBudget2> PopulateBudgetOfficerAsync(WorkingBudget2 workingBudgetEntity, string workingBudgetId)
        {
            // Obtain the names for the budget officer IDs in the line items.

            // First get a unique list of budget officer IDs to avoid reading the same record more than once.
            var budgetItems = workingBudgetEntity.LineItems.Where(x => x.BudgetLineItem != null).Select(x => x.BudgetLineItem);
            var distinctBudgetOfficerIds = budgetItems.Where(x => x.BudgetOfficer != null &&
            !string.IsNullOrEmpty(x.BudgetOfficer.BudgetOfficerId)).Select(x => x.BudgetOfficer.BudgetOfficerId).Distinct().ToArray();

            logger.Debug("List of distinctBudgetOfficerIds " + string.Join(", ", distinctBudgetOfficerIds));

            if (distinctBudgetOfficerIds != null && distinctBudgetOfficerIds.Any())
            {
                var uniqueLoginIds = new List<string>();
                // Read the BUD.OFCR records for the budget officer IDs.
                // The different budget officer IDs can have the same login, different logins, or no login.
                var budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(distinctBudgetOfficerIds);
                if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                {
                    foreach (var budOfcr in budgetOfficerRecords)
                    {
                        // Get the login ID for the budget officer.
                        var loginIdAssocMember = budOfcr.OfcrinfoEntityAssociation.Where(x => x != null && x.BoBudgetAssocMember == workingBudgetId).FirstOrDefault();
                        if (loginIdAssocMember != null && !string.IsNullOrWhiteSpace(loginIdAssocMember.BoLoginAssocMember))
                        {
                            var loginId = loginIdAssocMember.BoLoginAssocMember;

                            // Obtain the list of budget line items that have this budget officer record ID and populate the budget officer login
                            var lineItemEntities = budgetItems.Where(x => x.BudgetOfficer != null && x.BudgetOfficer.BudgetOfficerId == budOfcr.Recordkey).ToList();
                            if (lineItemEntities != null && lineItemEntities.Any())
                            {
                                foreach (var lineItem in lineItemEntities)
                                {
                                    lineItem.BudgetOfficer.BudgetOfficerLogin = loginId;
                                }
                            }
                            // Build a list of unique login IDs.
                            if (!uniqueLoginIds.Contains(loginId))
                            {
                                uniqueLoginIds.Add(loginId);
                            }
                        }
                    }
                }

                logger.Debug("List of uniqueLoginIds " + string.Join(", ", uniqueLoginIds));

                if (uniqueLoginIds != null && uniqueLoginIds.Any())
                {
                    // Read the OPERS records.
                    var operRecords = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueLoginIds.ToArray(), true);
                    if (operRecords != null && operRecords.Any())
                    {
                        foreach (var oper in operRecords)
                        {
                            if (!string.IsNullOrWhiteSpace(oper.SysUserName))
                            {
                                // Obtain the list of budget line items that have a budget officer with this login id and populate the name.
                                var lineItemEntities = budgetItems.Where(x => x.BudgetOfficer != null && x.BudgetOfficer.BudgetOfficerLogin == oper.Recordkey).ToList();
                                if (lineItemEntities != null && lineItemEntities.Any())
                                {
                                    foreach (var lineItem in lineItemEntities)
                                    {
                                        lineItem.BudgetOfficer.BudgetOfficerName = oper.SysUserName;
                                    }
                                }
                            }
                        }
                    }

                    // Add the budget officer name to any applicable subtotal line items.

                    // Obtain the budget officer type subtotal line items.
                    var subtotalLineItems = workingBudgetEntity.LineItems.Where(x => x.SubtotalLineItem != null && x.SubtotalLineItem.SubtotalType == "BO").Select(x => x.SubtotalLineItem);
                    foreach (var subtotalLine in subtotalLineItems)
                    {
                        if (subtotalLine != null && !string.IsNullOrWhiteSpace(subtotalLine.SubtotalValue))
                        {
                            var budgetItemForBudgetOfficerId = budgetItems.Where(y => y.BudgetOfficer != null && y.BudgetOfficer.BudgetOfficerId == subtotalLine.SubtotalValue).FirstOrDefault();
                            if (budgetItemForBudgetOfficerId != null && budgetItemForBudgetOfficerId.BudgetOfficer != null && !string.IsNullOrWhiteSpace(budgetItemForBudgetOfficerId.BudgetOfficer.BudgetOfficerName))
                            {
                                subtotalLine.SubtotalDescription = budgetItemForBudgetOfficerId.BudgetOfficer.BudgetOfficerName;
                            }

                            logger.Debug(string.Format("Adding budget officer name {0} to subtotal line with value {1}.",
                                budgetItemForBudgetOfficerId.BudgetOfficer.BudgetOfficerName, subtotalLine.SubtotalValue));
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Method to populate the reporting units description and authorization dates.
        /// </summary>
        /// <param name="workingBudgetEntity">The working budget domain entity.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <returns>A working budget domain entity whose budget line items have been updated with reporting unit information.</returns>
        private async Task<WorkingBudget2> PopulateReportingUnits2Async(WorkingBudget2 workingBudgetEntity, string workingBudgetId)
        {
            // Obtain the description and authorization date for the reporting units.

            // Get all the budget line items that are not subtotals.
            var budgetItems = workingBudgetEntity.LineItems.Where(x => x.BudgetLineItem != null).Select(x => x.BudgetLineItem);

            // First get a unique list of reporting unit IDs to avoid reading the same BUD.RESP record more than once.
            var distinctReportingUnitIds = budgetItems.Where(x => x.BudgetReportingUnit != null &&
            !string.IsNullOrEmpty(x.BudgetReportingUnit.ReportingUnitId)).Select(x => x.BudgetReportingUnit.ReportingUnitId).Distinct().ToArray();

            if (distinctReportingUnitIds != null && distinctReportingUnitIds.Any())
            {
                // Read the BUD.RESP records for the reporting unit IDs.
                var budRespRecords = await DataReader.BulkReadRecordAsync<BudResp>(distinctReportingUnitIds);
                if (budRespRecords != null && budRespRecords.Any())
                {
                    // Get the reporting unit descriptions.
                    foreach (var budResp in budRespRecords)
                    {
                        // Obtain the list of budget line items that have this reporting unit record ID and populate the description.
                        var lineItemEntities = budgetItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId == budResp.Recordkey);
                        if (lineItemEntities != null && lineItemEntities.Any())
                        {
                            foreach (var lineItem in lineItemEntities)
                            {
                                lineItem.BudgetReportingUnit.Description = budResp.BrDesc;
                            }
                        }
                    }
                }

                // Populate the authorization date.
                // If a reporting unit does not have an authorization date, read the superior reporting unit
                // and so on, and use the first authorization date found as the authorization date. 
                // If no authorization date is found higher in the tree, use the Budget Finalization Date from BCID.
                // Developers note: we are choosing to do single reads instead of reading the entire BCT file because
                // most reporting units will have an authorization date if clients follow procedure.
                // If we have a performance problem, we will do a bulkread of BCT at the beginning of the main method where
                // another bulkread of BCT happens with criteria. We would rework the code around this first bulkread so 
                // that we only do it once.

                // Store the populated authorization dates in a dictionary to avoid reading the same record over and over.
                Dictionary<string, DateTime?> authorizationDatesDictionary = new Dictionary<string, DateTime?>();
                foreach (var unitId in distinctReportingUnitIds)
                {
                    DateTime? authorizationDate = null;
                    // Check if the reporting unit has an entry in the dictionary, otherwise 
                    // read the BUD.CTRL record for the reporting unit and see if it has an authorization date.
                    if (authorizationDatesDictionary.ContainsKey(unitId))
                    {
                        authorizationDatesDictionary.TryGetValue(unitId, out authorizationDate);
                    }
                    else
                    {
                        var bctSuiteFileName = "BCT." + workingBudgetId;
                        var bctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, unitId);
                        if (bctRecord != null)
                        {
                            string superiorUnitId = bctRecord.BcSup;
                            if (bctRecord.BcAuthDate != null)
                            {
                                authorizationDate = bctRecord.BcAuthDate;
                                if (!authorizationDatesDictionary.ContainsKey(unitId))
                                {
                                    authorizationDatesDictionary.Add(unitId, authorizationDate);
                                }
                            }
                            else
                            {
                                // Check the dictionary or read up the tree to obtain the next authorization date.
                                while (!string.IsNullOrWhiteSpace(superiorUnitId))
                                {
                                    if (authorizationDatesDictionary.ContainsKey(superiorUnitId))
                                    {
                                        authorizationDatesDictionary.TryGetValue(superiorUnitId, out authorizationDate);
                                        // Last null out the superior unit ID to stop reading up the tree.
                                        superiorUnitId = null;
                                    }
                                    else
                                    {
                                        var superiorBctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, superiorUnitId);
                                        if (superiorBctRecord != null)
                                        {
                                            if (superiorBctRecord.BcAuthDate != null)
                                            {
                                                authorizationDate = superiorBctRecord.BcAuthDate;
                                                if (!authorizationDatesDictionary.ContainsKey(superiorUnitId))
                                                {
                                                    authorizationDatesDictionary.Add(superiorUnitId, authorizationDate);
                                                }
                                                // Last null out the superior unit ID to stop reading up the tree.
                                                superiorUnitId = null;
                                            }
                                            else
                                            {
                                                superiorUnitId = superiorBctRecord.BcSup;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // If we found an authorization date in the reporting unit itself or up the tree,
                    // assign it to the line items for that reporting unit.
                    if (authorizationDate.HasValue)
                    {
                        // Obtain the list of budget line items that have this reporting unit record ID and populate the authorization date.
                        var unitLineItemEntities = budgetItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId == unitId);
                        if (unitLineItemEntities != null && unitLineItemEntities.Any())
                        {
                            foreach (var lineItem in unitLineItemEntities)
                            {
                                lineItem.BudgetReportingUnit.AuthorizationDate = authorizationDate;
                            }
                        }
                    }
                }
            }

            // If there are any line items whose reporting unit does not have an authorization date,
            // get the budget finalization date and assign it to these line items' reporting units.
            var noAuthorizationDateLineItems = budgetItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.AuthorizationDate == null);
            if (noAuthorizationDateLineItems != null && noAuthorizationDateLineItems.Any())
            {
                // Read the BUDGET record to get the finalization date
                var budgetDataContract = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId);

                if (budgetDataContract.BuFinalDate != null)
                {
                    foreach (var lineItem in noAuthorizationDateLineItems)
                    {
                        lineItem.BudgetReportingUnit.AuthorizationDate = budgetDataContract.BuFinalDate;
                    }
                }
                else
                {
                    logger.Error(string.Format("The BUDGET record {0} does not have required field for finalized date.", workingBudgetId));
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Method to determine if the user's authorization date for each reporting unit has passed.
        /// </summary>
        /// <param name="workingBudgetEntity">The working budget domain entity.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="masterResponsibilityIds">List of all the uppermost responsibility unit IDs.</param>
        /// <returns>A working budget domain entity whose budget line items have been updated with authorization date has passed boolean.</returns>
        private async Task<WorkingBudget2> DetermineIfUserAuthorizationDateHasPassed2Async(WorkingBudget2 workingBudgetEntity, string workingBudgetId, List<string> masterResponsibilityIds)
        {
            // Loop through the list of master reponsibility unit IDs to check if the authorization
            // date for each of its children reporting units and itself has expired.
            foreach (var unitId in masterResponsibilityIds)
            {
                // Obtain the list of budget line items that are a children reporting units or the master itself.
                var budgetItems = workingBudgetEntity.LineItems.Where(x => x.BudgetLineItem != null).Select(x => x.BudgetLineItem);
                var unitLineItemEntities = budgetItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId.StartsWith(unitId));
                if (unitLineItemEntities != null && unitLineItemEntities.Any())
                {
                    // Get the line item that contains this master responsibility unit to check the authorization date.
                    var masterUnitLineItem = unitLineItemEntities.Where(y => y.BudgetReportingUnit.ReportingUnitId != null && y.BudgetReportingUnit.ReportingUnitId == unitId).FirstOrDefault();
                    if (masterUnitLineItem != null)
                    {
                        // The master responsibility unit is included in the line items.
                        // Use its authorization date for comparison.
                        foreach (var lineItem in unitLineItemEntities)
                        {
                            // If the authorization date for the master has expired set it accordingly for itself and its children.
                            if (masterUnitLineItem.BudgetReportingUnit.AuthorizationDate == null || masterUnitLineItem.BudgetReportingUnit.AuthorizationDate < DateTime.Today)
                            {
                                lineItem.BudgetReportingUnit.HasAuthorizationDatePassed = true;
                            }
                        }
                    }
                    else
                    {
                        // If the master responsibility unit does not have any budget accounts, it will not be in any line item.
                        // We need to read BCT to obtain the authorization date for the master unit for comparison.

                        DateTime? authorizationDate = null;

                        // Read the master responsibility unit BUD.CTRL record to get its authorization date if it has one.
                        var bctSuiteFileName = "BCT." + workingBudgetId;
                        var bctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, unitId);
                        if (bctRecord != null)
                        {
                            string superiorUnitId = bctRecord.BcSup;
                            if (bctRecord.BcAuthDate != null)
                            {
                                authorizationDate = bctRecord.BcAuthDate;
                            }
                            else
                            {
                                // Read up the tree to obtain the next authorization date.
                                while (!string.IsNullOrWhiteSpace(superiorUnitId))
                                {
                                    var superiorBctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, superiorUnitId);
                                    if (superiorBctRecord != null)
                                    {
                                        if (superiorBctRecord.BcAuthDate != null)
                                        {
                                            authorizationDate = superiorBctRecord.BcAuthDate;
                                            // Null out the superior unit ID to stop reading up the tree.
                                            superiorUnitId = null;
                                        }
                                        else
                                        {
                                            superiorUnitId = superiorBctRecord.BcSup;
                                        }
                                    }

                                }

                                if (authorizationDate == null)
                                {
                                    // Read the BUDGET record to get the finalization date
                                    var budgetDataContract = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId);
                                    if (budgetDataContract.BuFinalDate != null)
                                    {
                                        authorizationDate = budgetDataContract.BuFinalDate;
                                    }
                                    else
                                    {
                                        logger.Error(string.Format("The BUDGET record {0} does not have required field for finalized date.", workingBudgetId));
                                    }
                                }
                            }

                            foreach (var lineItem in unitLineItemEntities)
                            {
                                // If the authorization date for the master has expired set it accordingly for itself and its children.
                                // It shouldn't be null but if it is, make it expired.
                                if (authorizationDate == null || authorizationDate < DateTime.Today)
                                {
                                    lineItem.BudgetReportingUnit.HasAuthorizationDatePassed = true;
                                }
                            }
                        }
                        else
                        {
                            logger.Error(string.Format("The {0} in the file {1} does not exist.", unitId, bctSuiteFileName));
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Given a working budget ID and a list of associated BOC.workingbudget data contracts,
        /// get the list of all uppermost responsibility unit IDs for the user. 
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="bocDataContracts">The collection of BOC.workingbudget data contracts.</param>
        /// <param name="masterResponsibilityIds">List of all the uppermost responsibility unit IDs.</param>
        /// <returns>The list of all uppermost responsibility unit IDs for the working budget for the user.</returns>
        private List<string> DetermineMasterResponsibilityIds(string workingBudgetId, Collection<BudOctl> bocDataContracts, List<string> masterResponsibilityIds)
        {
            // Get the unique responsibility IDs from the BOC records, sorted in alphanumeric order so that 
            // superior IDs are processed before subordinate IDs. That will save us having to do a second check 
            // to see if the responsibility ID is a superior to any master already in the master list.
            var bocResponsibilityIds = bocDataContracts.SelectMany(x => x.BoBcId).Distinct().OrderBy(x => x).ToArray();
            if (bocResponsibilityIds != null && bocResponsibilityIds.Any())
            {
                // Develop a list of uppermost responsibility units. For example, if the user has responsibility units OB_FIN,
                // OB_PROV, OB_PROV_DART, and OB_PROV_DART_CART, the resulting list is OB_FIN and OB_PROV.
                foreach (var responsibilityId in bocResponsibilityIds)
                {
                    if (!masterResponsibilityIds.Any())
                    {
                        masterResponsibilityIds.Add(responsibilityId.ToUpperInvariant());
                    }
                    else
                    {
                        // Check if any master ID is a superior to this responsibility ID.
                        bool IsUnitInMaster = false;
                        foreach (var masterId in masterResponsibilityIds)
                        {
                            if (responsibilityId.ToUpperInvariant().StartsWith(masterId + "_"))
                            {
                                IsUnitInMaster = true;
                            }

                        }
                        if (!IsUnitInMaster)
                        {
                            masterResponsibilityIds.Add(responsibilityId.ToUpperInvariant());
                        }
                    }
                }
            }

            return masterResponsibilityIds;
        }

        /// <summary>
        /// Create responsibility unit domain entities and add them to the list.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="masterResponsibilityIds">List of all uppermost responsibility unit IDs for the user.</param>
        /// <param name="workingBudgetReportingUnitEntities">List of responsibility unit domain entities.</param>
        /// <returns>The list of responsibility unit domain entities with the ID populated.</returns>
        private async Task<List<BudgetReportingUnit>> DetermineResponsibilityUnitIdsAsync(string workingBudgetId, List<string> masterResponsibilityIds, List<BudgetReportingUnit> workingBudgetReportingUnitEntities)
        {
            // Now that we the have master list of uppermost responsibility IDs associated with the BUD.OFCR IDs for which the user is a budget officer
            // for the working budget, then get all of the subordinate responsibility units by selecting records that begin with the same string.
            //
            // Note: We are getting this information from BCT.budgetname file suite file, and not from BUD.RESP since the responsibility
            // tree could have been changed since the time that the working budget was generated.
            string bctCriteria = string.Empty;
            List<string> tempReponsibilityIds = new List<string>();

            foreach (var responsibility in masterResponsibilityIds)
            {
                if (!string.IsNullOrEmpty(responsibility))
                {
                    // the criteria gets the uppermost responsibility unit and its children.
                    bctCriteria = "WITH @ID LIKE '" + responsibility + "_...' OR WITH @ID EQ '" + responsibility + "'";

                    // Select the responsibility unit IDs and add them to a list.
                    var bctSuiteFileName = "BCT." + workingBudgetId;
                    var responsbilityIds = await DataReader.SelectAsync(bctSuiteFileName, bctCriteria);

                    if (responsbilityIds != null && responsbilityIds.Any())
                    {
                        tempReponsibilityIds.AddRange(responsbilityIds);
                    }
                }
            }

            var allResponsibilityUnitIds = tempReponsibilityIds.Distinct();
            foreach (var unitId in allResponsibilityUnitIds)
            {
                workingBudgetReportingUnitEntities.Add(new BudgetReportingUnit(unitId));
            }

            return workingBudgetReportingUnitEntities;
        }

        /// <summary>
        /// Populate the description for the reporting unit IDs.
        /// </summary>
        /// <param name="workingBudgetReportingUnitEntities">List of reporting unit IDs.</param>
        /// <returns>List of reporting unit IDs.</returns>
        private async Task<List<BudgetReportingUnit>> PopulateReportingUnitsDescriptionAsync(List<BudgetReportingUnit> workingBudgetReportingUnitEntities)
        {
            // Get the list of reporting unit IDs.
            string[] reportingUnitIds = workingBudgetReportingUnitEntities.Where(x => x.ReportingUnitId != null &&
                                         !string.IsNullOrEmpty(x.ReportingUnitId)).Select(x => x.ReportingUnitId).Distinct().ToArray();

            // Read the BUD.RESP records for the reporting unit IDs.
            var budRespRecords = await DataReader.BulkReadRecordAsync<BudResp>(reportingUnitIds);
            if (budRespRecords != null && budRespRecords.Any())
            {
                // Get the reporting unit descriptions.
                foreach (var budResp in budRespRecords)
                {
                    // Populate the description for the reporting unit.
                    var unit = workingBudgetReportingUnitEntities.Where(x => x.ReportingUnitId != null && x.ReportingUnitId == budResp.Recordkey).FirstOrDefault();
                    unit.Description = budResp.BrDesc;
                }
            }
            return workingBudgetReportingUnitEntities;
        }

        /// <summary>
        /// Given a working budget ID and a list of associated BOC.workingbudget data contracts, find the 
        /// associated budget officers.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="bocDataContracts">The collection of BOC.workingbudget data contracts.</param>
        /// <returns>A string array of all budget officer IDs for the working budget for the user.</returns>
        private async Task<string[]> DetermineBudgetOfficersAsync(string workingBudgetId, Collection<BudOctl> bocDataContracts)
        {
            string[] allBudOfcrArray = null;
            List<string> allBudOfcrList = new List<string>();
            List<string> tempRespList = new List<string>();

            //get the unique responsibility IDs from the BOC records
            var bocResponsibilityIds = bocDataContracts.SelectMany(x => x.BoBcId).Distinct().OrderBy(x => x).ToArray();
            if (bocResponsibilityIds != null && bocResponsibilityIds.Any())
            {
                // Develop a list of uppermost responsibility units. For example, if the user has responsibility units OB_FIN,
                // OB_PROV, OB_PROV_DART, and OB_PROV_DART_CART, the resulting list is OB_FIN and OB_PROV.
                List<string> masterResponsibilityIds = new List<string>();
                foreach (var responsibilityId in bocResponsibilityIds)
                {
                    if (!masterResponsibilityIds.Any())
                    {
                        masterResponsibilityIds.Add(responsibilityId);
                    }
                    else
                    {
                        bool IsUnitInMaster = false;
                        foreach (var masterId in masterResponsibilityIds)
                        {
                            if (responsibilityId.StartsWith(masterId + "_"))
                            {
                                IsUnitInMaster = true;
                            }

                        }
                        if (!IsUnitInMaster)
                        {
                            masterResponsibilityIds.Add(responsibilityId);
                        }
                    }
                }

                // Now that we the have master list of uppermost responsibility IDs associated with the BUD.OFCR IDs for which the user is a budget officer
                // for the working budget, then get all of the subordinate responsibility units by selecting records that begin with the same string.
                //
                // Note: We are getting this information from BCT.budgetname file suite file, and not from BUD.RESP since the responsibility
                // tree could have been changed since the time that the working budget was generated.
                string bctCriteria = string.Empty;
                List<string> allBudOfcrIds = new List<string>();

                foreach (var responsibility in masterResponsibilityIds)
                {
                    if (!string.IsNullOrEmpty(responsibility))
                    {
                        // the criteria gets the uppermost responsibility unit and its children.
                        bctCriteria = "WITH @ID LIKE '" + responsibility + "_...' OR WITH @ID EQ '" + responsibility + "'";
                    }

                    var bctSuiteFileName = "BCT." + workingBudgetId;
                    var responsbilityDataContracts = await DataReader.BulkReadRecordAsync<BudCtrl>(bctSuiteFileName, bctCriteria, true);

                    // For each set of responsibility data contracts, build a unique string array of budget officer IDs and add them to a master list
                    // of budget officer IDs for all master responsibility IDs. 
                    tempRespList = responsbilityDataContracts.Select(x => x.BcBofId).Distinct().OrderBy(x => x).ToList();
                    allBudOfcrList.AddRange(tempRespList);
                }

                // If there is a master list of budget officer IDs, make it unique and ordered, and make it the master array that is returned.
                if (allBudOfcrList.Any())
                {
                    allBudOfcrArray = allBudOfcrList.Distinct().OrderBy(x => x).ToArray();
                }
            }
            return allBudOfcrArray;
        }

        /// <summary>
        /// Obtain the BUD.OCTL records for the budget officers assigned to the user for the working budget.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="userOperatorId">The login ID for the user.</param>
        /// <returns>A collection of BUD.OCTL data contracts.</returns>
        private async Task<Collection<BudOctl>> GetBudOctlRecordsForBudgetOfficersAssignedToUser(string workingBudgetId, string userOperatorId)
        {
            Collection<BudOctl> bocDataContracts = new Collection<BudOctl>();

            // Get the list of budget officers for the current user for the working budget.
            var selectionCriteria = "WITH BO.BUDGET EQ '" + workingBudgetId + "'" + " AND BO.LOGIN EQ '" + userOperatorId + "'";
            var possibleWorkingBudgetOfficersIds = await DataReader.SelectAsync("BUD.OFCR", selectionCriteria);

            // If there are any budget officer records containing the working budget and the current user ID,
            // read the budget officer records and then get the BUD.OFCR records where there is an association entry
            // that contains both the working budget and current user login ID, which means that the current user is identified as
            // one of the budget officers for the working budget.
            if (possibleWorkingBudgetOfficersIds != null && possibleWorkingBudgetOfficersIds.Any())
            {
                Collection<BudOfcr> budgetOfficerRecords = new Collection<BudOfcr>();
                budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(possibleWorkingBudgetOfficersIds);
                string[] workingBudgetOfficerRecordIds;

                if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                {
                    // Get a string array of BUD.OFCR record IDs where the BUD.OFCR record has an association
                    // row where the budget is the working budget and the login is the user staff operator ID.
                    workingBudgetOfficerRecordIds = budgetOfficerRecords.Where(x => x != null && x.OfcrinfoEntityAssociation != null
                    && x.OfcrinfoEntityAssociation.Where(y => y != null && y.BoBudgetAssocMember == workingBudgetId && y.BoLoginAssocMember == userOperatorId).Any()).Select(x => x.Recordkey).ToArray();

                    if (workingBudgetOfficerRecordIds != null && workingBudgetOfficerRecordIds.Any())
                    {
                        // Take the string array of BUD.OFCR IDs for which the user is an officer for the working budget, 
                        // and read the BOC.workingbudget records to get the associated responsibility IDs. 
                        var bocSuiteFileName = "BOC." + workingBudgetId;
                        bocDataContracts = await DataReader.BulkReadRecordAsync<BudOctl>(bocSuiteFileName, workingBudgetOfficerRecordIds, true);
                    }
                }
            }

            return bocDataContracts;
        }


        private void LogData(object objectToLog, string stringToLog)
        {
            if (objectToLog != null)
            {
                logger.Debug(JsonConvert.SerializeObject(objectToLog));
            }
        }

        #region Obsolete/Deprecated

        /// <summary>
        /// Gets a working budget for a user.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="budgetConfigurationComparables">A list of budget comparables defined for the working budget.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <param name="majorComponentStartPosition">The list of major component start positions.</param>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>A working budget domain entity.</returns>
        /// <exception cref="ArgumentNullException">An exception is returned when no working budget or no current user PERSON ID is specified.</exception>
        /// ("Obsolete as of Colleague Web API 1.25. Use GetBudgetDevelopmentWorkingBudget2Async")
        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables,
            WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            // Verify the contents of the method arguments.
            if (string.IsNullOrEmpty(workingBudgetId))
            {
                throw new ArgumentNullException("workingBudgetId", "There is no working budget ID defined.");
            }
            if (string.IsNullOrEmpty(currentUserPersonId))
            {
                throw new ArgumentNullException("currentUserPersonId", "There is no current user PERSON ID.");
            }

            // get the OPERS ID associated with STAFF ID.
            string userOperatorId = await GetOperatorIdFromStaffRecordAsync(currentUserPersonId);

            List<string> masterResponsibilityIds = new List<string>();

            // Initialize the working budget domain entity to be returned.
            var workingBudgetEntity = new WorkingBudget();

            // If the user has an OPERS record, find the budget officers, get the associated responsibility IDs from the BOC
            // records, and then get the BWK record IDs from the BCT records.
            if (!string.IsNullOrEmpty(userOperatorId))
            {
                // Get the list of budget officers for the current user for the working budget.
                var selectionCriteria = "WITH BO.BUDGET EQ '" + workingBudgetId + "'" + " AND BO.LOGIN EQ '" + userOperatorId + "'";
                var possibleWorkingBudgetOfficersIds = await DataReader.SelectAsync("BUD.OFCR", selectionCriteria);

                // If there are any budget officer records containing the working budget and the current user ID,
                // read the budget officer records and then get the BUD.OFCR records where there is an association entry
                // that contains both the working budget and current user login ID, which means that the current user is identified as
                // one of the budget officers for the working budget.
                if (possibleWorkingBudgetOfficersIds != null && possibleWorkingBudgetOfficersIds.Any())
                {
                    Collection<BudOfcr> budgetOfficerRecords = new Collection<BudOfcr>();
                    budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(possibleWorkingBudgetOfficersIds);
                    string[] workingBudgetOfficerRecordIds;

                    if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                    {
                        // Get a string array of BUD.OFCR record IDs where the BUD.OFCR record has an association row where the budget is working budget
                        // and the login is the user staff operator ID.
                        workingBudgetOfficerRecordIds = budgetOfficerRecords.Where(x => x != null && x.OfcrinfoEntityAssociation != null
                        && x.OfcrinfoEntityAssociation.Where(y => y != null && y.BoBudgetAssocMember == workingBudgetId && y.BoLoginAssocMember == userOperatorId).Any()).Select(x => x.Recordkey).ToArray();

                        if (workingBudgetOfficerRecordIds != null && workingBudgetOfficerRecordIds.Any())
                        {
                            // Take the string array of BUD.OFCR IDs that the user is an officer for the working budget, and read the BOC.workingbudget
                            // records to get the associated responsibility IDs. 
                            var bocSuiteFileName = "BOC." + workingBudgetId;
                            var bocDataContracts = await DataReader.BulkReadRecordAsync<BudOctl>(bocSuiteFileName, workingBudgetOfficerRecordIds, true);
                            if (bocDataContracts != null && bocDataContracts.Any())
                            {
                                // Determine the BWK.workingbudget IDs using the BOC.workingbudget records.
                                string[] allBwkIds = await DetermineBudWorkIdsAsync(workingBudgetId, bocDataContracts, criteria, masterResponsibilityIds);
                                if (allBwkIds != null && allBwkIds.Any())
                                {
                                    // Apply the filter, get the specific rage requested, bulk read BWK.workingbudget records, and build the domain entity.
                                    workingBudgetEntity = await AddBudgetLineItemsToWorkingBudgetEntityAsync(workingBudgetEntity, workingBudgetId, allBwkIds, criteria, startPosition, recordCount, budgetConfigurationComparables, majorComponentStartPosition);
                                }

                                // Get the description and authorization date for the reporting unit in each line items
                                workingBudgetEntity = await PopulateReportingUnitsAsync(workingBudgetEntity, workingBudgetId);
                                // Determine if the user's authorization date for the reporting unit has passed.
                                workingBudgetEntity = await DetermineIfUserAuthorizationDateHasPassedAsync(workingBudgetEntity, workingBudgetId, masterResponsibilityIds);
                            }
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Method to add budget line items to a given working budget domain entity from a list of BWK.working budget record IDs.
        /// </summary>
        /// <param name="workingBudgetEntity">The domain entity to which budget line items entities will be added.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="allBwkIds">A string array of all BWK.workingbudget record IDs assign to the user's budget officers.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="budgetConfigurationComparables">The list of budget comparables defined for the working budget.</param>
        /// <param name="majorComponentStartPosition">The list of major component start positions.</param>
        /// <returns>A working budget domain entity that has been updated with budget line items.</returns>
        /// ("Obsolete as of Colleague Web API 1.25. Use AddBudgetLineItemsToWorkingBudgetEntity2Async")
        private async Task<WorkingBudget> AddBudgetLineItemsToWorkingBudgetEntityAsync(WorkingBudget workingBudgetEntity, string workingBudgetId, string[] allBwkIds, WorkingBudgetQueryCriteria criteria, int startPosition, int recordCount, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables, IList<string> majorComponentStartPosition)
        {
            // Set the specific BUD.WORK file suite name.
            var bwkSuiteFileName = "BWK." + workingBudgetId;

            // This method is only called if bwkIds contains any IDs.

            // Apply the filter criteria to the list of resolved BUD.WORK IDs.
            // To avoid a selection statment that may reach the length limit, 
            // do a SelectAsync for each criteria element.
            string[] filteredBwkIds = await ApplyFilterCriteriaAsync(allBwkIds, criteria, bwkSuiteFileName);

            if (filteredBwkIds != null && filteredBwkIds.Any())
            {
                // Set the count of all possible budget line items for the working budget.
                workingBudgetEntity.TotalLineItems = filteredBwkIds.ToList().Count;

                // Obtain the specific range requested for pagination.
                string[] bwkArray = CalculateRequestedLineItems(startPosition, recordCount, filteredBwkIds.ToArray());

                // Read the BWK records.
                var budWorkDataContracts = await DataReader.BulkReadRecordAsync<BudWork>(bwkSuiteFileName, bwkArray, true);
                if (budWorkDataContracts != null && budWorkDataContracts.Any())
                {
                    foreach (var budworkRecord in budWorkDataContracts)
                    {
                        // Every BWK record must have an expense account. Otherwise, it is corrupt data, and the data is not
                        // included in the domain entity.
                        if (string.IsNullOrEmpty(budworkRecord.BwExpenseAcct))
                        {
                            //log the data error and exclude the budget line item from the working budget that is returned.
                            LogDataError("BW.EXPENSE.ACCOUNT", budworkRecord.Recordkey, budworkRecord);
                        }
                        else
                        {
                            var lineItem = new BudgetLineItem(budworkRecord.BwExpenseAcct);

                            // Get the formatted GL account.
                            lineItem.FormattedBudgetAccountId = GlAccountUtility.ConvertGlAccountToExternalFormat(budworkRecord.BwExpenseAcct, majorComponentStartPosition);

                            // Convert the justification notes into a paragraph.
                            var notes = string.Empty;
                            if (budworkRecord.BwNotes != null && budworkRecord.BwNotes.Any())
                            {
                                var justificationNotes = new StringBuilder();
                                foreach (var note in budworkRecord.BwNotes)
                                {
                                    if (string.IsNullOrWhiteSpace(note))
                                    {
                                        justificationNotes.Append(Environment.NewLine + Environment.NewLine);
                                    }
                                    else
                                    {
                                        if (justificationNotes.Length > 0)
                                        {
                                            justificationNotes.Append(" ");
                                        }
                                        justificationNotes.Append(note);
                                    }
                                }

                                notes = justificationNotes.ToString();
                            }
                            lineItem.JustificationNotes = notes;

                            // Assign the budget officer ID to the line item.
                            lineItem.BudgetOfficer = new BudgetOfficer(budworkRecord.BwOfcrLink);

                            // Assign the reporting unit ID to the line item.
                            lineItem.BudgetReportingUnit = new BudgetReportingUnit(budworkRecord.BwControlLink);

                            // Get the base budget amount for the budget line item. If the budget line item does not have a BASE version, log a data error beca
                            if (budworkRecord.BwfreezeEntityAssociation != null && budworkRecord.BwfreezeEntityAssociation.Any())
                            {
                                var baseAmountRow = budworkRecord.BwfreezeEntityAssociation.FirstOrDefault(x => x != null && x.BwVersionAssocMember == "BASE");
                                if (baseAmountRow != null)
                                {
                                    lineItem.BaseBudgetAmount = baseAmountRow.BwVlineAmtAssocMember.HasValue ? baseAmountRow.BwVlineAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    LogDataError("BW.VERSION", budworkRecord.Recordkey, budworkRecord);
                                    lineItem.BaseBudgetAmount = 0;
                                }
                            }

                            // get the working budget amount for the line item.
                            lineItem.WorkingAmount = budworkRecord.BwLineAmt.HasValue ? budworkRecord.BwLineAmt.Value : 0;

                            // Add only the comparables that are defined in the budget development configuration.
                            List<string> comparableIds = budgetConfigurationComparables.Select(x => x.ComparableId).ToList();
                            bool IsComp1Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C1")).Any();
                            bool IsComp2Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C2")).Any();
                            bool IsComp3Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C3")).Any();
                            bool IsComp4Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C4")).Any();
                            bool IsComp5Defined = budgetConfigurationComparables.Where(comp => comparableIds.Contains("C5")).Any();

                            if (budworkRecord.BwComp1Amount.HasValue && IsComp1Defined)
                            {
                                lineItem.AddBudgetComparable(new BudgetComparable("C1")
                                {
                                    ComparableAmount = budworkRecord.BwComp1Amount.Value
                                });
                            }

                            if (budworkRecord.BwComp2Amount.HasValue && IsComp2Defined)
                            {
                                lineItem.AddBudgetComparable(new BudgetComparable("C2")
                                {
                                    ComparableAmount = budworkRecord.BwComp2Amount.Value
                                });
                            }

                            if (budworkRecord.BwComp3Amount.HasValue && IsComp3Defined)
                            {
                                lineItem.AddBudgetComparable(new BudgetComparable("C3")
                                {
                                    ComparableAmount = budworkRecord.BwComp3Amount.Value
                                });
                            }

                            if (budworkRecord.BwComp4Amount.HasValue && IsComp4Defined)
                            {
                                lineItem.AddBudgetComparable(new BudgetComparable("C4")
                                {
                                    ComparableAmount = budworkRecord.BwComp4Amount.Value
                                });
                            }

                            if (budworkRecord.BwComp5Amount.HasValue && IsComp5Defined)
                            {
                                lineItem.AddBudgetComparable(new BudgetComparable("C5")
                                {
                                    ComparableAmount = budworkRecord.BwComp5Amount.Value
                                });
                            }

                            // Add the budget line item to the working budget domain entity.
                            workingBudgetEntity.AddBudgetLineItem(lineItem);
                        }
                    }

                    // Obtain the names for the budget officer IDs in the line items.

                    // First get a unique list of budget officer IDs to avoid reading the same record more than once.
                    var distinctBudgetOfficerIds = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetOfficer != null &&
                    !string.IsNullOrEmpty(x.BudgetOfficer.BudgetOfficerId)).Select(x => x.BudgetOfficer.BudgetOfficerId).Distinct().ToArray();

                    if (distinctBudgetOfficerIds != null && distinctBudgetOfficerIds.Any())
                    {
                        var uniqueLoginIds = new List<string>();
                        // Read the BUD.OFCR records for the budget officer IDs.
                        // The different budget officer IDs can have the same login, different logins, or no login.
                        var budgetOfficerRecords = await DataReader.BulkReadRecordAsync<BudOfcr>(distinctBudgetOfficerIds);
                        if (budgetOfficerRecords != null && budgetOfficerRecords.Any())
                        {
                            foreach (var budOfcr in budgetOfficerRecords)
                            {
                                // Get the login ID for the budget officer.
                                var loginIdAssocMember = budOfcr.OfcrinfoEntityAssociation.Where(x => x != null && x.BoBudgetAssocMember == workingBudgetId).FirstOrDefault();
                                if (loginIdAssocMember != null && !string.IsNullOrWhiteSpace(loginIdAssocMember.BoLoginAssocMember))
                                {
                                    var loginId = loginIdAssocMember.BoLoginAssocMember;

                                    // Obtain the list of budget line items that have this budget officer record ID and populate the budget officer login
                                    var lineItemEntities = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetOfficer != null && x.BudgetOfficer.BudgetOfficerId == budOfcr.Recordkey).ToList();
                                    if (lineItemEntities != null && lineItemEntities.Any())
                                    {
                                        foreach (var lineItem in lineItemEntities)
                                        {
                                            lineItem.BudgetOfficer.BudgetOfficerLogin = loginId;
                                        }
                                    }
                                    // Build a list of unique login IDs.
                                    if (!uniqueLoginIds.Contains(loginId))
                                    {
                                        uniqueLoginIds.Add(loginId);
                                    }
                                }
                            }
                        }

                        if (uniqueLoginIds != null && uniqueLoginIds.Any())
                        {
                            // Read the OPERS records.
                            var operRecords = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueLoginIds.ToArray(), true);
                            if (operRecords != null && operRecords.Any())
                            {
                                foreach (var oper in operRecords)
                                {
                                    if (!string.IsNullOrWhiteSpace(oper.SysUserName))
                                    {
                                        // Obtain the list of budget line items that have a budget officer with this login id and populate the name.
                                        var lineItemEntities = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetOfficer != null && x.BudgetOfficer.BudgetOfficerLogin == oper.Recordkey).ToList();
                                        if (lineItemEntities != null && lineItemEntities.Any())
                                        {
                                            foreach (var lineItem in lineItemEntities)
                                            {
                                                lineItem.BudgetOfficer.BudgetOfficerName = oper.SysUserName;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Method to populate the reporting units description and authorization dates.
        /// </summary>
        /// <param name="workingBudgetEntity">The domain entity to which budget line items entities will be added.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <returns>A working budget domain entity whose budget line items have been updated with reporting unit information.</returns>
        /// ("Obsolete as of Colleague Web API 1.25. Use PopulateReportingUnits2Async")
        private async Task<WorkingBudget> PopulateReportingUnitsAsync(WorkingBudget workingBudgetEntity, string workingBudgetId)
        {
            // Obtain the description and authorization date for the reporting units.

            // First get a unique list of reporting unit IDs to avoid reading the same BUD.RESP record more than once.
            var distinctReportingUnitIds = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetReportingUnit != null &&
            !string.IsNullOrEmpty(x.BudgetReportingUnit.ReportingUnitId)).Select(x => x.BudgetReportingUnit.ReportingUnitId).Distinct().ToArray();

            if (distinctReportingUnitIds != null && distinctReportingUnitIds.Any())
            {
                // Read the BUD.RESP records for the reporting unit IDs.
                var budRespRecords = await DataReader.BulkReadRecordAsync<BudResp>(distinctReportingUnitIds);
                if (budRespRecords != null && budRespRecords.Any())
                {
                    // Get the reporting unit descriptions.
                    foreach (var budResp in budRespRecords)
                    {
                        // Obtain the list of budget line items that have this reporting unit record ID and populate the description.
                        var lineItemEntities = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId == budResp.Recordkey);
                        if (lineItemEntities != null && lineItemEntities.Any())
                        {
                            foreach (var lineItem in lineItemEntities)
                            {
                                lineItem.BudgetReportingUnit.Description = budResp.BrDesc;
                            }
                        }
                    }
                }

                // Populate the authorization date.
                // If a reporting unit does not have an authorization date, read the superior reporting unit
                // and so on, and use the first authorization date found as the authorization date. 
                // If no authorization date is found higher in the tree, use the Budget Finalization Date from BCID.
                // Developers note: we are choosing to do single reads instead of reading the entire BCT file because
                // most reporting units will have an authorization date if clients follow procedure.
                // If we have a performance problem, we will do a bulkread of BCT at the beginning of the main method where
                // another bulkread of BCT happens with criteria. We would rework the code around this first bulkread so 
                // that we only do it once.

                // Store the populated authorization dates in a dictionary to avoid reading the same record over and over.
                Dictionary<string, DateTime?> authorizationDatesDictionary = new Dictionary<string, DateTime?>();
                foreach (var unitId in distinctReportingUnitIds)
                {
                    DateTime? authorizationDate = null;
                    // Check if the reporting unit has an entry in the dictionary, otherwise 
                    // read the BUD.CTRL record for the reporting unit and see if it has an authorization date.
                    if (authorizationDatesDictionary.ContainsKey(unitId))
                    {
                        authorizationDatesDictionary.TryGetValue(unitId, out authorizationDate);
                    }
                    else
                    {
                        var bctSuiteFileName = "BCT." + workingBudgetId;
                        var bctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, unitId);
                        if (bctRecord != null)
                        {
                            string superiorUnitId = bctRecord.BcSup;
                            if (bctRecord.BcAuthDate != null)
                            {
                                authorizationDate = bctRecord.BcAuthDate;
                                if (!authorizationDatesDictionary.ContainsKey(unitId))
                                {
                                    authorizationDatesDictionary.Add(unitId, authorizationDate);
                                }
                            }
                            else
                            {
                                // Check the dictionary or read up the tree to obtain the next authorization date.
                                while (!string.IsNullOrWhiteSpace(superiorUnitId))
                                {
                                    if (authorizationDatesDictionary.ContainsKey(superiorUnitId))
                                    {
                                        authorizationDatesDictionary.TryGetValue(superiorUnitId, out authorizationDate);
                                        // Last null out the superior unit ID to stop reading up the tree.
                                        superiorUnitId = null;
                                    }
                                    else
                                    {
                                        var superiorBctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, superiorUnitId);
                                        if (superiorBctRecord != null)
                                        {
                                            if (superiorBctRecord.BcAuthDate != null)
                                            {
                                                authorizationDate = superiorBctRecord.BcAuthDate;
                                                if (!authorizationDatesDictionary.ContainsKey(superiorUnitId))
                                                {
                                                    authorizationDatesDictionary.Add(superiorUnitId, authorizationDate);
                                                }
                                                // Last null out the superior unit ID to stop reading up the tree.
                                                superiorUnitId = null;
                                            }
                                            else
                                            {
                                                superiorUnitId = superiorBctRecord.BcSup;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // If we found an authorization date in the reporting unit itself or up the tree,
                    // assign it to the line items for that reporting unit.
                    if (authorizationDate.HasValue)
                    {
                        // Obtain the list of budget line items that have this reporting unit record ID and populate the authorization date.
                        var unitLineItemEntities = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId == unitId);
                        if (unitLineItemEntities != null && unitLineItemEntities.Any())
                        {
                            foreach (var lineItem in unitLineItemEntities)
                            {
                                lineItem.BudgetReportingUnit.AuthorizationDate = authorizationDate;
                            }
                        }
                    }
                }
            }

            // If there are any line items whose reporting unit does not have an authorization date,
            // get the budget finalization date and assign it to these line items' reporting units.
            var noAuthorizationDateLineItems = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.AuthorizationDate == null);
            if (noAuthorizationDateLineItems != null && noAuthorizationDateLineItems.Any())
            {
                // Read the BUDGET record to get the finalization date
                var budgetDataContract = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId);

                if (budgetDataContract.BuFinalDate != null)
                {
                    foreach (var lineItem in noAuthorizationDateLineItems)
                    {
                        lineItem.BudgetReportingUnit.AuthorizationDate = budgetDataContract.BuFinalDate;
                    }
                }
                else
                {
                    logger.Error(string.Format("The BUDGET record {0} does not have required field for finalized date.", workingBudgetId));
                }
            }
            return workingBudgetEntity;
        }

        /// <summary>
        /// Method to determine if the user's authorization date for each reporting unit has passed.
        /// </summary>
        /// <param name="workingBudgetEntity">The domain entity to which budget line items entities will be added.</param>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="masterResponsibilityIds">List of all the uppermost responsibility unit IDs.</param>
        /// <returns>A working budget domain entity whose budget line items have been updated with authorization date has passed boolean.</returns>
        /// ("Obsolete as of Colleague Web API 1.25. Use DetermineIfUserAuthorizationDateHasPassed2Async")
        private async Task<WorkingBudget> DetermineIfUserAuthorizationDateHasPassedAsync(WorkingBudget workingBudgetEntity, string workingBudgetId, List<string> masterResponsibilityIds)
        {
            // Loop through the list of master reponsibility unit IDs to check if the authorization
            // date for each of its children reporting units and itself has expired.
            foreach (var unitId in masterResponsibilityIds)
            {
                // Obtain the list of budget line items that are a children reporting units or the master itself.
                var unitLineItemEntities = workingBudgetEntity.BudgetLineItems.Where(x => x.BudgetReportingUnit != null && x.BudgetReportingUnit.ReportingUnitId.StartsWith(unitId));
                if (unitLineItemEntities != null && unitLineItemEntities.Any())
                {
                    // Get the line item that contains this master responsibility unit to check the authorization date.
                    var masterUnitLineItem = unitLineItemEntities.Where(y => y.BudgetReportingUnit.ReportingUnitId != null && y.BudgetReportingUnit.ReportingUnitId == unitId).FirstOrDefault();
                    if (masterUnitLineItem != null)
                    {
                        // The master responsibility unit is included in the line items.
                        // Use its authorization date for comparison.
                        foreach (var lineItem in unitLineItemEntities)
                        {
                            // If the authorization date for the master has expired set it accordingly for itself and its children.
                            if (masterUnitLineItem.BudgetReportingUnit.AuthorizationDate == null || masterUnitLineItem.BudgetReportingUnit.AuthorizationDate < DateTime.Today)
                            {
                                lineItem.BudgetReportingUnit.HasAuthorizationDatePassed = true;
                            }
                        }
                    }
                    else
                    {
                        // If the master responsibility unit does not have any budget accounts, it will not be in any line item.
                        // We need to read BCT to obtain the authorization date for the master unit for comparison.

                        DateTime? authorizationDate = null;

                        // Read the master responsibility unit BUD.CTRL record to get its authorization date if it has one.
                        var bctSuiteFileName = "BCT." + workingBudgetId;
                        var bctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, unitId);
                        if (bctRecord != null)
                        {
                            string superiorUnitId = bctRecord.BcSup;
                            if (bctRecord.BcAuthDate != null)
                            {
                                authorizationDate = bctRecord.BcAuthDate;
                            }
                            else
                            {
                                // Read up the tree to obtain the next authorization date.
                                while (!string.IsNullOrWhiteSpace(superiorUnitId))
                                {
                                    var superiorBctRecord = await DataReader.ReadRecordAsync<BudCtrl>(bctSuiteFileName, superiorUnitId);
                                    if (superiorBctRecord != null)
                                    {
                                        if (superiorBctRecord.BcAuthDate != null)
                                        {
                                            authorizationDate = superiorBctRecord.BcAuthDate;
                                            // Null out the superior unit ID to stop reading up the tree.
                                            superiorUnitId = null;
                                        }
                                        else
                                        {
                                            superiorUnitId = superiorBctRecord.BcSup;
                                        }
                                    }

                                }

                                if (authorizationDate == null)
                                {
                                    // Read the BUDGET record to get the finalization date
                                    var budgetDataContract = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", workingBudgetId);
                                    if (budgetDataContract.BuFinalDate != null)
                                    {
                                        authorizationDate = budgetDataContract.BuFinalDate;
                                    }
                                    else
                                    {
                                        logger.Error(string.Format("The BUDGET record {0} does not have required field for finalized date.", workingBudgetId));
                                    }
                                }
                            }

                            foreach (var lineItem in unitLineItemEntities)
                            {
                                // If the authorization date for the master has expired set it accordingly for itself and its children.
                                // It shouldn't be null but if it is, make it expired.
                                if (authorizationDate == null || authorizationDate < DateTime.Today)
                                {
                                    lineItem.BudgetReportingUnit.HasAuthorizationDatePassed = true;
                                }
                            }
                        }
                        else
                        {
                            logger.Error(string.Format("The {0} in the file {1} does not exist.", unitId, bctSuiteFileName));
                        }
                    }
                }
            }
            return workingBudgetEntity;
        }

        #endregion
    }
}