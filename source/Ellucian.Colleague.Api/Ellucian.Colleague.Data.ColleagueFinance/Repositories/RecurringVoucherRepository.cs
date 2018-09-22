// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IRecurringVoucherRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RecurringVoucherRepository : BaseColleagueRepository, IRecurringVoucherRepository
    {
        /// <summary>
        /// This constructor allows us to instantiate a recurring voucher repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public RecurringVoucherRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get a specific recurring voucher
        /// </summary>
        /// <param name="recurringVoucherId">The requested recurring voucher ID</param>
        /// <param name="personId">Person ID of user</param>
        /// <param name="glAccessLevel">GL Access level of user</param>
        /// <returns>Recurring voucher domain entity</returns>
        public async Task<RecurringVoucher> GetRecurringVoucherAsync(string recurringVoucherId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts)
        {
            if (string.IsNullOrEmpty(recurringVoucherId))
            {
                throw new ArgumentNullException("recurringVoucherId");
            }

            var recurringVoucher = await DataReader.ReadRecordAsync<RcVouchers>(recurringVoucherId);
            if (recurringVoucher == null)
            {
                throw new KeyNotFoundException(string.Format("Recurring Voucher record {0} does not exist.", recurringVoucherId));
            }

            if (glAccessAccounts == null)
            {
                glAccessAccounts = new List<string>();
            }

            // Translate the status code into a RecurringVoucherStatus enumeration value
            RecurringVoucherStatus recurringVoucherStatus = new RecurringVoucherStatus();

            // Get the first status in the list of recurring voucher statuses, and check that it has a value
            if (!string.IsNullOrEmpty(recurringVoucher.RcvStatus.FirstOrDefault()))
            {
                switch (recurringVoucher.RcvStatus.FirstOrDefault().ToUpper())
                {
                    case "X":
                        recurringVoucherStatus = RecurringVoucherStatus.Cancelled;
                        break;
                    case "C":
                        recurringVoucherStatus = RecurringVoucherStatus.Closed;
                        break;
                    case "N":
                        recurringVoucherStatus = RecurringVoucherStatus.NotApproved;
                        break;
                    case "O":
                        recurringVoucherStatus = RecurringVoucherStatus.Outstanding;
                        break;
                    case "V":
                        recurringVoucherStatus = RecurringVoucherStatus.Voided;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid recurring voucher status for recurring voucher: " + recurringVoucher.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for recurring voucher: " + recurringVoucher.Recordkey);
            }

            // The recurring voucher status date contains one to many dates
            DateTime? statusDate = recurringVoucher.RcvStatusDate.First();
            if (statusDate == null)
            {
                throw new ApplicationException("Missing status date for recurring voucher: " + recurringVoucher.Recordkey);
            }
            var recurringVoucherStatusDate = recurringVoucher.RcvStatusDate.First().Value.Date;

            // Determine the vendor name for the recurring voucher. If there is a misc name, use it. 
            // Otherwise, get the AP.CHECK hierarchy name.
            string recurringVoucherVendorName = "";
            if ((recurringVoucher.RcvVenName != null) && (recurringVoucher.RcvVenName.Count() > 0))
            {
                recurringVoucherVendorName = String.Join(" ", recurringVoucher.RcvVenName.ToArray());
            }
            else if (!string.IsNullOrEmpty(recurringVoucher.RcvVendor))
            {
                // Call a colleague transaction to get the AP.CHECK hierarchy.
                TxGetHierarchyNameRequest request = new TxGetHierarchyNameRequest()
                {
                    IoPersonId = recurringVoucher.RcvVendor,
                    InHierarchy = "AP.CHECK"
                };

                TxGetHierarchyNameResponse response = transactionInvoker.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(request);

                // The transaction returns the hierarchy name.
                if (!((response.OutPersonName == null) || (response.OutPersonName.Count < 1)))
                {
                    recurringVoucherVendorName = String.Join(" ", response.OutPersonName.ToArray());
                }
            }

            if (!recurringVoucher.RcvDate.HasValue)
            {
                throw new ApplicationException("Missing recurring voucher date for recurring voucher: " + recurringVoucher.Recordkey);
            }

            if (!recurringVoucher.RcvDefaultInvoiceDate.HasValue)
            {
                throw new ApplicationException("Missing invoice date for recurring voucher: " + recurringVoucher.Recordkey);
            }

            if ((string.IsNullOrEmpty(recurringVoucher.RcvApType)) && (recurringVoucherStatus != RecurringVoucherStatus.Cancelled))
            {
                throw new ApplicationException("Missing AP type for recurring voucher: " + recurringVoucher.Recordkey);
            }

            var recurringVoucherDomainEntity = new RecurringVoucher(recurringVoucher.Recordkey, recurringVoucher.RcvDate.Value, recurringVoucherStatus, recurringVoucherStatusDate, recurringVoucherVendorName, recurringVoucher.RcvDefaultInvoiceNo, recurringVoucher.RcvDefaultInvoiceDate.Value);

            recurringVoucherDomainEntity.Amount = 0;
            recurringVoucherDomainEntity.CurrencyCode = recurringVoucher.RcvCurrencyCode;

            // Make sure the exchange rate is valid
            if ((recurringVoucher.RcvExchangeRate.HasValue && !recurringVoucher.RcvExchangeRateDate.HasValue) ||
                (!recurringVoucher.RcvExchangeRate.HasValue && recurringVoucher.RcvExchangeRateDate.HasValue))
                throw new ApplicationException("Invalid exchange rate/date combination.");

            if (recurringVoucher.RcvExchangeRate <= 0)
                throw new ApplicationException("Invalid exchange rate.");

            recurringVoucherDomainEntity.ExchangeRate = recurringVoucher.RcvExchangeRate;
            recurringVoucherDomainEntity.VendorId = recurringVoucher.RcvVendor;

            if (recurringVoucher.RcvMaintGlTranDate.HasValue)
            {
                recurringVoucherDomainEntity.MaintenanceDate = recurringVoucher.RcvMaintGlTranDate.Value.Date;
            }
            recurringVoucherDomainEntity.ApType = recurringVoucher.RcvApType;
            recurringVoucherDomainEntity.Comments = recurringVoucher.RcvComments;

            // Read the OPERS records associated with the approval signatures and next 
            // approvers on the voucher, and build approver objects.
            var operators = new List<string>();
            if (recurringVoucher.RcvAuthorizations != null)
            {
                operators.AddRange(recurringVoucher.RcvAuthorizations);
            }
            if (recurringVoucher.RcvNextApprovalIds != null)
            {
                operators.AddRange(recurringVoucher.RcvNextApprovalIds);
            }
            var uniqueOperators = operators.Distinct().ToList();

            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the recurring voucher, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((recurringVoucher.RcvAuthEntityAssociation != null) && (recurringVoucher.RcvAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in recurringVoucher.RcvAuthEntityAssociation)
                            {
                                if (approval.RcvAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    approver.ApprovalDate = approval.RcvAuthorizationDatesAssocMember.Value.Date;
                                }
                            }
                        }
                        recurringVoucherDomainEntity.AddApprover(approver);
                    }
                }
            }

            // If the recurring voucher is using foreign currency, calculate the recurring voucher amount
            // from the foreign amount fields on the line items. Otherwise, calculate the recurring voucher
            // amount from the local amount fields on the line items.

            // Also, determine if the user has access to any GL numbers on the recurring voucher line items, line
            // item GL distribution GL numbers or tax GL numbers. If not, issue a permission exception.

            bool hasGlAccess = false;
            var lineItemIds = recurringVoucher.RcvItemsId;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
                if ((lineItemRecords != null) && (lineItemRecords.Count > 0))
                {
                    foreach (var lineItem in lineItemRecords)
                    {
                        if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Count > 0))
                        {
                            foreach (var glDistr in lineItem.VouchGlEntityAssociation)
                            {
                                if (hasGlAccess == false)
                                {
                                    if (glAccessLevel == GlAccessLevel.Full_Access)
                                    {
                                        hasGlAccess = true;
                                    }
                                    else if (glAccessLevel == GlAccessLevel.Possible_Access)
                                    {
                                        if (glAccessAccounts.Contains(glDistr.ItmVouGlNoAssocMember))
                                        {
                                            hasGlAccess = true;
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(recurringVoucher.RcvCurrencyCode))
                                {
                                    recurringVoucherDomainEntity.Amount += glDistr.ItmVouGlAmtAssocMember.HasValue ? glDistr.ItmVouGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    recurringVoucherDomainEntity.Amount += glDistr.ItmVouGlForeignAmtAssocMember.HasValue ? glDistr.ItmVouGlForeignAmtAssocMember.Value : 0;
                                }
                            }
                        }
                        if ((lineItem.VouGlTaxesEntityAssociation != null) && (lineItem.VouGlTaxesEntityAssociation.Count > 0))
                        {
                            foreach (var taxGlDistr in lineItem.VouGlTaxesEntityAssociation)
                            {
                                if (hasGlAccess == false)
                                {
                                    if (glAccessLevel == GlAccessLevel.Full_Access)
                                    {
                                        hasGlAccess = true;
                                    }
                                    else if (glAccessLevel == GlAccessLevel.Possible_Access)
                                    {
                                        if (glAccessAccounts.Contains(taxGlDistr.ItmVouTaxGlNoAssocMember))
                                        {
                                            hasGlAccess = true;
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(recurringVoucher.RcvCurrencyCode))
                                {
                                    recurringVoucherDomainEntity.Amount += taxGlDistr.ItmVouGlTaxAmtAssocMember.HasValue ? taxGlDistr.ItmVouGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    recurringVoucherDomainEntity.Amount += taxGlDistr.ItmVouGlForeignTaxAmtAssocMember.HasValue ? taxGlDistr.ItmVouGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }
                    }
                }
            }
            if (hasGlAccess == false)
            {
                throw new PermissionsException("Insufficient permission to access recurring voucher.");
            }

            // Get the recurring voucher schedule data.

            var scheduleIds = recurringVoucher.RcvRcvsId;
            if (scheduleIds != null && scheduleIds.Count > 0)
            {
                var scheduleRecords = await DataReader.BulkReadRecordAsync<RcVouSchedules>(scheduleIds.ToArray());
                if ((scheduleRecords != null) && (scheduleRecords.Count > 0))
                {
                    foreach (var schedule in scheduleRecords)
                    {
                        if (schedule != null)
                        {
                            if (!schedule.RcvsScheduleDate.HasValue)
                            {
                                throw new ApplicationException("Missing schedule date for recurring voucher: " + recurringVoucher.Recordkey);
                            }

                            if (!schedule.RcvsItemsTotal.HasValue)
                            {
                                throw new ApplicationException("Missing schedule amount for recurring voucher: " + recurringVoucher.Recordkey);
                            }

                            var schedDate = schedule.RcvsScheduleDate.GetValueOrDefault();
                            var schedAmount = schedule.RcvsItemsTotal.GetValueOrDefault();
                            RecurringVoucherSchedule recurVoucherSchedule = new RecurringVoucherSchedule(schedDate, schedAmount);

                            recurVoucherSchedule.TaxAmount = schedule.RcvsItemsTaxTotal.HasValue ? schedule.RcvsItemsTaxTotal.Value : 0;

                            if (!string.IsNullOrEmpty(schedule.RcvsVouId) && !string.IsNullOrEmpty(schedule.RcvsPurgedVouId))
                            {
                                throw new ApplicationException("Recurring voucher schedule cannot have both a voucher ID and purged voucher ID" + recurringVoucher.Recordkey);
                            }

                            if (!string.IsNullOrEmpty(schedule.RcvsVouId))
                            {
                                recurVoucherSchedule.VoucherId = schedule.RcvsVouId;
                            }

                            if (!string.IsNullOrEmpty(schedule.RcvsPurgedVouId))
                            {
                                recurVoucherSchedule.PurgedVoucherId = schedule.RcvsPurgedVouId;
                            }

                            recurringVoucherDomainEntity.AddSchedule(recurVoucherSchedule);
                        }
                    }
                }
            }
            return recurringVoucherDomainEntity;
        }
    }
}