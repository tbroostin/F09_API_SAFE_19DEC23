// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Linq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ApproverRepository : BaseColleagueRepository, IApproverRepository
    {
        /// <summary>
        /// This constructor instantiates an approver repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public ApproverRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
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
    }
}