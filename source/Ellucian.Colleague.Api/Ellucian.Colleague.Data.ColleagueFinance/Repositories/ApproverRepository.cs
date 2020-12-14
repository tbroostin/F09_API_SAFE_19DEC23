// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ApproverRepository : PersonBaseRepository, IApproverRepository
    {
        /// <summary>
        /// This constructor instantiates an approver repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public ApproverRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger,ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger, settings)
        {
        }

        /// <summary>
        /// Validate an approver ID. 
        /// A next approver ID and an approver ID are the same. They are just
        /// populated under different circumstances. 
        /// </summary>
        /// <param name="approverId">Approver ID.</param>
        /// <returns>An approver validation response entity.</returns>
        public async Task<ApproverValidationResponse> ValidateApproverAsync(string approverId)
        {
            if (string.IsNullOrWhiteSpace(approverId))
            {
                throw new ArgumentNullException("approverId", "approverId is required.");
            }

            // Uppercase the value.
            approverId = approverId.ToUpperInvariant();

            // Initialize the approver validation reponse domain entity.
            ApproverValidationResponse approverValidationResponse = new ApproverValidationResponse(approverId);

            // Initialize the IsValid property to true.
            approverValidationResponse.IsValid = true;

            // Read the Approvals record.
            var approverContract = await DataReader.ReadRecordAsync<Approvals>(approverId);
            if (approverContract == null)
            {
                approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID does not have an approvals record.";
            }
            else
            {
                // Check that the approver ID has a STAFF record.
                // Initialize the criteria.
                var staffCriteria = "WITH STAFF.LOGIN.ID EQ '" + approverId + "'";

                // Select any staff record with the approver ID. It can be more than one.
                var staffIds = await DataReader.SelectAsync("STAFF", staffCriteria);
                if (staffIds == null || !staffIds.Any())
                {
                    approverValidationResponse.ErrorMessage = "Invalid ID - No Staff record was found for the approver ID.";
                }
                else
                {
                    // An approver has to be setup with GL or policy classes or with document amounts greater than $0.00.
                    bool hasClasses = true;

                    if (approverContract.ApprvClasses == null || !approverContract.ApprvClasses.Any())
                    {
                        hasClasses = false;
                    }

                    bool hasBudgetEntryApprovalAmount = true;
                    if (!approverContract.ApprvBeMaxAmt.HasValue || approverContract.ApprvBeMaxAmt <= 0)
                    {
                        hasBudgetEntryApprovalAmount = false;
                    }

                    if (!hasClasses && !hasBudgetEntryApprovalAmount)
                    {
                        approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID is not setup to approve budget adjustments.";
                    }
                    else
                    {
                        // Check the record start and end dates.
                        if (approverContract.ApprvBeginDate.HasValue && DateTime.Today < approverContract.ApprvBeginDate)
                        {
                            // The approver start date is in the future. The approval has not started.
                            approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID is not setup to approve budget adjustments.";
                        }
                        else if (approverContract.ApprvEndDate.HasValue && DateTime.Today > approverContract.ApprvEndDate)
                        {
                            // The approver end date is in the past. The approval has expired.
                            approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID is not setup to approve budget adjustments.";
                        }
                    }
                }
            }

            // If there are no errors check amount and the classes setup.

            if (string.IsNullOrEmpty(approverValidationResponse.ErrorMessage))
            {
                if (approverContract.ApprvBeMaxAmt > 0)
                {
                    // The approver has an amount setup to approve budget adjustments. 
                    // It is a valid approver regardless of the classes setup.
                    approverValidationResponse.IsValid = true;
                }
                else
                {
                    // The approver ID has at least one approval class.
                    var approvalClasses = approverContract.ApprvClasses;
                    var approvalClassesBeginDates = approverContract.ApprvClassesBeginDate;
                    var approvalClassesEndDates = approverContract.ApprvClassesEndDate;
                    var validClassesCount = approvalClasses.Count();

                    // If the classes do not have any begin and end dates, they are valid.
                    if (approvalClassesBeginDates.Any() || approvalClassesEndDates.Any())
                    {
                        var approvalClassesCount = approvalClasses.Count();
                        var classesBeginDatesCount = approvalClassesBeginDates.Count();
                        var classesEndDatesCount = approvalClassesEndDates.Count();

                        // The begin and end date are not in an association with the classes.
                        // Assign null values to the dates to be able to loop through them
                        // when the dates are not filled.
                        if (approvalClassesCount > classesBeginDatesCount)
                        {
                            for (int i = 0; i < approvalClassesCount - classesBeginDatesCount; i++)
                            {
                                approvalClassesBeginDates.Add(default(DateTime?));
                            }
                        }
                        if (approvalClassesCount > classesEndDatesCount)
                        {
                            for (int i = 0; i < approvalClassesCount - classesEndDatesCount; i++)
                            {
                                approvalClassesEndDates.Add(default(DateTime?));
                            }
                        }

                        // The approver ID has at least one approval class with some date.
                        // Check each approval class dates against today's date.
                        bool validClass = true;
                        for (int i = 0; i < approvalClassesBeginDates.Count(); i++)
                        {
                            if (approvalClassesBeginDates[i].HasValue)
                            {
                                var classBeginDate = approvalClassesBeginDates[i].Value;
                                if (DateTime.Today < classBeginDate)
                                {
                                    // This class begins in the future.
                                    validClassesCount -= 1;
                                }
                            }
                        }
                        for (int i = 0; i < approvalClassesEndDates.Count(); i++)
                        {
                            if (approvalClassesEndDates[i].HasValue)
                            {
                                var classEndDate = approvalClassesEndDates[i].Value;
                                if (DateTime.Today > classEndDate)
                                {
                                    // this class ends in the past.
                                    validClassesCount -= 1;
                                }
                            }
                        }
                    }

                    if (validClassesCount == 0)
                    {
                        // None of the classes are valid.
                        approverValidationResponse.IsValid = false;
                        approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID is restricted by the begin and end approval dates.";
                    }
                }
            }
            else
            {
                // There are errors in the setup.
                approverValidationResponse.IsValid = false;
            }

            if (approverValidationResponse.IsValid == true)
            {
                // Obtain the name for the approver ID. In Colleague it comes from OPERS.
                var opersContract = await DataReader.ReadRecordAsync<Opers>("UT.OPERS", approverId);
                if (opersContract == null)
                {
                    approverValidationResponse.IsValid = false;
                    approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID does not have an OPERS record.";
                }
                else
                {
                    approverValidationResponse.ApproverName = approverId;
                    if (!string.IsNullOrWhiteSpace(opersContract.SysUserName))
                    {
                        approverValidationResponse.ApproverName = opersContract.SysUserName;
                    }
                }
            }
            else
            {
                // Assign an error messages if the approver is not valid and there is no error.
                if (string.IsNullOrEmpty(approverValidationResponse.ErrorMessage))
                {
                    approverValidationResponse.ErrorMessage = "Invalid ID - the approver ID is restricted by the begin and end approval dates.";
                }
            }

            return approverValidationResponse;
        }

        /// <summary>
        /// Get an approver name given an approver ID.
        /// </summary>
        /// <param name="approverId">The approver ID.</param>
        /// <returns>An approver name or empty string.</returns>
        public async Task<String> GetApproverNameForIdAsync(string approverId)
        {
            string approverName = string.Empty;
            // Obtain the name for the approver ID. In Colleague it comes from OPERS.
            var opersContract = await DataReader.ReadRecordAsync<Opers>("UT.OPERS", approverId);
            if (opersContract != null)
            {
                if (!string.IsNullOrEmpty(opersContract.SysUserName))
                {
                    approverName = opersContract.SysUserName;
                }
            }
            return approverName;
        }

        /// <summary>
        /// Get the list of NextApprover based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for NextApprover search.</param>
        /// <returns> The NextApprover search results</returns> 
        public async Task<IEnumerable<NextApprover>> QueryNextApproverByKeywordAsync(string searchCriteria)
        {
            List<NextApprover> nextApproverEntities = new List<NextApprover>();
            if (string.IsNullOrEmpty(searchCriteria))
                throw new ArgumentNullException("searchCriteria", "search criteria required to query");

            // Remove extra blank spaces
            var tempString = searchCriteria.Trim();
            Regex regEx = new Regex(@"\s+");
            searchCriteria = regEx.Replace(tempString, @" ");

            List<string> filteredNextApprover = new List<string>();

            filteredNextApprover = await ApplyFilterCriteria(searchCriteria, filteredNextApprover);

            if (!filteredNextApprover.Any())
                return null;

            filteredNextApprover = filteredNextApprover.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            var approverData = await DataReader.BulkReadRecordAsync<Approvals>("APPROVALS", filteredNextApprover.ToArray());
            

            if (approverData != null && approverData.Any()) { 
            
                foreach (var id in filteredNextApprover)
                {
                    Approvals approvalsDataContact = approverData.FirstOrDefault(sd => sd.Recordkey == id);
                    

                    try
                    {
                        string initiatorName = string.Empty;
                        NextApprover approver = new NextApprover(approvalsDataContact.Recordkey);
                        string approverName = await GetApproverNameForIdAsync(approver.NextApproverId);
                        approver.NextApproverPersonId = await GetApproverPersonIdForIdAsync(approver.NextApproverId);
                        approver.SetNextApproverName(approverName);
                        nextApproverEntities.Add(approver);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                }

            }

            return nextApproverEntities.AsEnumerable();
        }

        private async Task<List<string>> ApplyFilterCriteria(string searchKey, List<string> filteredNextApprover)
        {
            long personId = 0;
            string nextApproverIdQuery = "";
            string opersIdQuery = "";
            List<string> filteredOpers = new List<string>();

            // serach criteria is number
            if (long.TryParse(searchKey, out personId))
            {
                if (searchKey.Count() > 1)
                {
                    opersIdQuery = string.Format("WITH SYS.PERSON.ID LIKE ..." + searchKey + "...");
                    filteredOpers = await ExecuteQueryStatement("UT.OPERS", filteredNextApprover, opersIdQuery);

                    if (filteredOpers != null && filteredOpers.Any())
                    {
                        var approvalsQueryCriteria = string.Join(" ", filteredOpers.Select( x => string.Format("'{0}'",x)));
                        approvalsQueryCriteria = "WITH APPROVALS.ID EQ " + approvalsQueryCriteria;

                        filteredNextApprover = await ExecuteQueryStatement("APPROVALS", filteredNextApprover, approvalsQueryCriteria);
                    }
                }
            }
            else
            {
                // try to fetch next approver from next Approver Id if search key does not have "," or whitespace
                if (!(searchKey.Contains(",") || searchKey.Contains(" ")))
                {
                    // As Approval Ids are always in Uppercase , make searchkey to upper case
                    nextApproverIdQuery = string.Format("WITH APPROVALS.ID EQ '" + searchKey.ToUpper() + "'");
                    filteredNextApprover = await ExecuteQueryStatement("APPROVALS", filteredNextApprover, nextApproverIdQuery);
                }

                // If there is no result in approval then checck in person file
                if (filteredNextApprover == null || !(filteredNextApprover.Any()))
                {
                    // Otherwise, we are doing a name search of initiator - parse the search string into name parts
                    List<string> names = CommentsUtility.FormatStringToNames(searchKey);
                    filteredNextApprover = await GetNextApprovalIdsFromPerson(names[0], names[1], names[2]);
                }
            }

            return filteredNextApprover;
        }

        private async Task<List<string>> GetNextApprovalIdsFromPerson(string lastName, string firstName, string middleName)
        {
            List<string> filteredNextApprover = new List<string>();

            var filteredPersonIds = await SearchByNameAsync(lastName, firstName, middleName);
            
            var personQueryCriteria = string.Join(" ", filteredPersonIds.Select(x => string.Format("'{0}'", x)));
            personQueryCriteria = "WITH SYS.PERSON.ID EQ " + personQueryCriteria;

            if (filteredPersonIds != null && filteredPersonIds.Any())
                filteredNextApprover = await ExecuteQueryStatement("UT.OPERS", new List<string>(), personQueryCriteria);
            return filteredNextApprover;
        }

        private async Task<List<string>> ExecuteQueryStatement(string FileName, List<string> filteredApprovers, string queryCriteria)
        {
            string[] filteredByQueryCriteria = null;
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            if (filteredApprovers != null && filteredApprovers.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(FileName, filteredApprovers.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(FileName, queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }

        /// <summary>
        /// Get an approver person id for given an approver ID.
        /// </summary>
        /// <param name="approverId">The approver ID.</param>
        /// <returns>An approver person id or empty string.</returns>
        private async Task<String> GetApproverPersonIdForIdAsync(string approverId)
        {
            string approverPersonId = string.Empty;
            // Obtain the person id for the approver ID. In Colleague it comes from OPERS.
            var opersContract = await DataReader.ReadRecordAsync<Opers>("UT.OPERS", approverId);
            if (opersContract != null)
            {
                if (!string.IsNullOrEmpty(opersContract.SysPersonId))
                {
                    approverPersonId = opersContract.SysPersonId;
                }
            }
            return approverPersonId;
        }


    }
}