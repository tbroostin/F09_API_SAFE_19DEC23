﻿// Copyright 2016-2023 Ellucian Company L.P. and its affiliates
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Implement the IGeneralLedgerTransactionRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GeneralLedgerTransactionRepository : BaseColleagueRepository, IGeneralLedgerTransactionRepository, IEthosExtended
    {
        public int GlSecurityTransactionCallCount { get; set; }
        private RepositoryException exception = new RepositoryException();

        private readonly string _colleagueTimeZone;


        /// <summary>
        /// Constructor to instantiate a general ledger transaction repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public GeneralLedgerTransactionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.GlSecurityTransactionCallCount = 0;
            _colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get the general ledger transaction requested
        /// </summary>
        /// <param name="id">general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A general ledger transaction domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<GeneralLedgerTransaction> GetByIdAsync(string id, string personId, GlAccessLevel glAccessLevel)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the INTG.GL.POSTINGS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "INTG.GL.POSTINGS")
            {
                throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} does not exist.", id));
            }

            var intgGlPostings = await DataReader.ReadRecordAsync<IntgGlPostings>(recordInfo.PrimaryKey);
            if (intgGlPostings == null)
            {
                throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} does not exist.", id));
            }

            bool donationPledge = true;
            if (intgGlPostings.IgpSource != null)
            {
                foreach (var source in intgGlPostings.IgpSource)
                {
                    if (!source.Equals("DN", StringComparison.OrdinalIgnoreCase) && !source.Equals("PL", StringComparison.OrdinalIgnoreCase)
                        && !source.Equals("DNE", StringComparison.OrdinalIgnoreCase) && !source.Equals("PLE", StringComparison.OrdinalIgnoreCase))
                    {
                        donationPledge = false;
                    }
                }
            }
            if (!donationPledge)
            {
                throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} has a source that is not supported by this version of the API.", id));
            }
            if (intgGlPostings.IgpSource == null || !intgGlPostings.IgpSource.Any() || !donationPledge)
            {
                throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} does not exist.", id));
            }

            // Read the INTG.GL.POSTINGS.DETAIL records
            var detailIds = intgGlPostings.IgpTranDetails.ToArray();
            var intgGlPostingsDetail = await DataReader.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds);

            return BuildGeneralLedgerTransaction(intgGlPostings, intgGlPostingsDetail);
        }

        /// <summary>
        /// Get the general ledger transaction requested
        /// </summary>
        /// <param name="id">general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A general ledger transaction domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<GeneralLedgerTransaction> GetById2Async(string id, string personId, GlAccessLevel glAccessLevel)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the INTG.GL.POSTINGS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "INTG.GL.POSTINGS")
            {
                throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} does not exist.", id));
            }
            var intgGlPostings = await DataReader.ReadRecordAsync<IntgGlPostings>(recordInfo.PrimaryKey);
            {
                if (intgGlPostings == null)
                {
                    throw new KeyNotFoundException(string.Format("Integration GL Postings record {0} does not exist.", id));
                }
            }
            // Read the INTG.GL.POSTINGS.DETAIL records
            var detailIds = intgGlPostings.IgpTranDetails.ToArray();
            var intgGlPostingsDetail = await DataReader.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds);

            var generalLedgerTransactionEntity = BuildGeneralLedgerTransaction2(intgGlPostings, intgGlPostingsDetail);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return generalLedgerTransactionEntity;
        }

        /// <summary>
        /// Get the general ledger transaction requested
        /// </summary>
        /// <param name="id">general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A general ledger transaction domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<IEnumerable<GeneralLedgerTransaction>> GetAsync(string personId, GlAccessLevel glAccessLevel)
        {
            var intgGlPostingsEntities = new List<GeneralLedgerTransaction>();
            // Read the INTG.GL.POSTINGS record
            var criteria = "WITH EVERY IGP.SOURCE EQ 'DN''PL''DNE''PLE'";
            var intgGlPostings = await DataReader.BulkReadRecordAsync<IntgGlPostings>(criteria);
            {
                if (intgGlPostings == null)
                {
                    throw new KeyNotFoundException("No records selected from INTG.GL.POSTINGS in Colleague.");
                }
            }
            // Read the INTG.GL.POSTINGS.DETAIL records
            var detailIds = intgGlPostings.SelectMany(igp => igp.IgpTranDetails).ToArray();
            var intgGlPostingsDetail = await DataReader.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds);

            foreach (var intgGlPostingEntity in intgGlPostings)
            {
                intgGlPostingsEntities.Add(BuildGeneralLedgerTransaction(intgGlPostingEntity, intgGlPostingsDetail));
            }
            return intgGlPostingsEntities;
        }

        /// <summary>
        /// Get the general ledger transaction requested
        /// </summary>
        /// <param name="id">general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A general ledger transaction domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<IEnumerable<GeneralLedgerTransaction>> Get2Async(string personId, GlAccessLevel glAccessLevel, bool journalEntryPermission = true, bool budgetEntryPermission = true, bool encumbranceEntryPermission = true)
        {
            var intgGlPostingsEntities = new List<GeneralLedgerTransaction>();
            // Read the INTG.GL.POSTINGS record
            var criteria = "WITH IGP.SOURCE NE ''";
            if (!journalEntryPermission) criteria += " AND WITH IGP.SOURCE NE 'JE' AND WITH IGP.SOURCE NE 'AA'";
            if (!budgetEntryPermission) criteria += " AND WITH IGP.SOURCE NE 'BU' AND WITH IGP.SOURCE NE 'AB' AND WITH IGP.SOURCE NE 'BC'";
            if (!encumbranceEntryPermission) criteria += " AND WITH IGP.SOURCE NE 'AE' AND WITH IGP.SOURCE NE 'EP'";
            var intgGlPostings = await DataReader.BulkReadRecordAsync<IntgGlPostings>(criteria);
            {
                if (intgGlPostings == null)
                {
                    return intgGlPostingsEntities;
                }
            }
            // Read the INTG.GL.POSTINGS.DETAIL records
            var detailIds = intgGlPostings.SelectMany(igp => igp.IgpTranDetails).ToArray();
            var intgGlPostingsDetail = await DataReader.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds);

            foreach (var intgGlPostingEntity in intgGlPostings)
            {
                var generalLedgerTransactionEntity = BuildGeneralLedgerTransaction2(intgGlPostingEntity, intgGlPostingsDetail);
                if (generalLedgerTransactionEntity != null)
                {
                    intgGlPostingsEntities.Add(generalLedgerTransactionEntity);
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return intgGlPostingsEntities;
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to update</param>
        /// <param name="id">The general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<GeneralLedgerTransaction> UpdateAsync(string id, GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
            if (!generalLedgerTransaction.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var recordInfo = await GetRecordInfoFromGuidAsync(id);
                if (recordInfo != null)
                {
                    throw new InvalidOperationException(string.Format("Integration GL Postings record {0} already exists.  Cannot update an existing entity in Colleague.", id));
                }
            }
            var response = await CreateGeneralLedgerTransaction(generalLedgerTransaction, GlConfig);
            if (response.Id.Equals(string.Empty, StringComparison.OrdinalIgnoreCase) || response.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }
            else
            {
                var generalLedgerTransactionEntity = await GetByIdAsync(response.Id, personId, glAccessLevel);
                return generalLedgerTransactionEntity;
            }
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to create</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<GeneralLedgerTransaction> CreateAsync(GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig)
        {
            if (!string.IsNullOrEmpty(generalLedgerTransaction.Id))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!generalLedgerTransaction.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(generalLedgerTransaction.Id);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("Integration GL Postings record {0} already exists.", generalLedgerTransaction.Id));
                    }
                }
            }
            var response = await CreateGeneralLedgerTransaction(generalLedgerTransaction, GlConfig);
            if (response.Id.Equals(string.Empty, StringComparison.OrdinalIgnoreCase) || response.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }
            else
            {
                var generalLedgerTransactionEntity = await GetByIdAsync(response.Id, personId, glAccessLevel);
                return generalLedgerTransactionEntity;
            }
        }

        private GeneralLedgerTransaction BuildGeneralLedgerTransaction(IntgGlPostings integGlPosting, IEnumerable<IntgGlPostingsDetail> inteGlPostingDetails)
        {
            var generalLedgerTransaction = new GeneralLedgerTransaction() { Id = integGlPosting.RecordGuid, ProcessMode = "Update", SubmittedBy = integGlPosting.IgpSubmittedBy };
            generalLedgerTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>();

            foreach (var glTrans in integGlPosting.TranDetailEntityAssociation)
            {
                if (!string.IsNullOrEmpty(glTrans.IgpSourceAssocMember) && glTrans.IgpTrDateAssocMember.HasValue && !string.IsNullOrEmpty(glTrans.IgpTranDetailsAssocMember))
                {
                    GenLedgrTransaction transactionItem = new GenLedgrTransaction(glTrans.IgpSourceAssocMember.ToUpper(), glTrans.IgpTrDateAssocMember.Value.AddHours(12))
                    {
                        ReferenceNumber = glTrans.IgpRefNoAssocMember,
                        ReferencePersonId = glTrans.IgpAcctIdAssocMember,
                        TransactionTypeReferenceDate = glTrans.IgpSysDateAssocMember,
                        TransactionNumber = glTrans.IgpTranNoAssocMember,
                        TransactionDetailLines = new List<GenLedgrTransactionDetail>()
                    };

                    try
                    {
                        var detailItem = inteGlPostingDetails.FirstOrDefault(igp => igp.Recordkey == glTrans.IgpTranDetailsAssocMember);
                        foreach (var transDetail in detailItem.IgpdTranDetailsEntityAssociation)
                        {
                            if (!string.IsNullOrWhiteSpace(transDetail.IgpdGlNoAssocMember))
                            {
                                decimal amount = 0;
                                CreditOrDebit type = CreditOrDebit.Credit;
                                if (transDetail.IgpdCreditAssocMember != null && transDetail.IgpdCreditAssocMember.Value != 0)
                                {
                                    amount = transDetail.IgpdCreditAssocMember.Value;
                                }
                                else
                                {
                                    if (transDetail.IgpdDebitAssocMember != null && transDetail.IgpdDebitAssocMember.Value != 0)
                                    {
                                        amount = transDetail.IgpdDebitAssocMember.Value;
                                        type = CreditOrDebit.Debit;
                                    }
                                }
                                var amountAndCurrency = new AmountAndCurrency(amount, CurrencyCodes.USD);

                                var genLdgrTransactionDetail = new GenLedgrTransactionDetail(transDetail.IgpdGlNoAssocMember, transDetail.IgpdProjectIdsAssocMember, transDetail.IgpdDescriptionAssocMember, type, amountAndCurrency);
                                try
                                {
                                    genLdgrTransactionDetail.SequenceNumber = (!string.IsNullOrEmpty(transDetail.IgpdTranSeqNoAssocMember)) ? (int?)int.Parse(transDetail.IgpdTranSeqNoAssocMember) : null;
                                    genLdgrTransactionDetail.SubmittedBy = transDetail.IgpdSubmittedByAssocMember;
                                }
                                catch (Exception ex)
                                {
                                    // Leave sequence number off the domain entity
                                    logger.Error(ex, "Unable to get sequence number.");
                                }
                                transactionItem.TransactionDetailLines.Add(genLdgrTransactionDetail);
                            }
                        }
                        generalLedgerTransaction.GeneralLedgerTransactions.Add(transactionItem);
                    }
                    catch (ArgumentNullException e)
                    {
                        string message = string.Format("{0}, Id: {1}", e.Message, integGlPosting.Recordkey);
                        throw new ArgumentNullException(message);
                    }
                    catch
                    {
                        throw new KeyNotFoundException(string.Format("INTG.GL.POSTINGS.DETAIL key {0} is missing from Colleague.", glTrans.IgpTranDetailsAssocMember));
                    }
                }
            }
            return generalLedgerTransaction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integGlPosting"></param>
        /// <param name="inteGlPostingDetails"></param>
        /// <returns></returns>
        private GeneralLedgerTransaction BuildGeneralLedgerTransaction2(IntgGlPostings integGlPosting, IEnumerable<IntgGlPostingsDetail> inteGlPostingDetails)
        {
            var generalLedgerTransaction = new GeneralLedgerTransaction()
            {
                Id = integGlPosting.RecordGuid,
                ProcessMode = "Update",
                SubmittedBy = integGlPosting.IgpSubmittedBy.Trim(),
                Comment = integGlPosting.IgpComments
            };
            generalLedgerTransaction.GeneralLedgerTransactions = new List<GenLedgrTransaction>();

            foreach (var glTrans in integGlPosting.TranDetailEntityAssociation)
            {
                if (!string.IsNullOrEmpty(glTrans.IgpSourceAssocMember) && glTrans.IgpTrDateAssocMember.HasValue && !string.IsNullOrEmpty(glTrans.IgpTranDetailsAssocMember))
                {
                    GenLedgrTransaction transactionItem = new GenLedgrTransaction(glTrans.IgpSourceAssocMember.ToUpper(), glTrans.IgpTrDateAssocMember.Value.AddHours(12))
                    {
                        ReferenceNumber = glTrans.IgpRefNoAssocMember,
                        ReferencePersonId = glTrans.IgpAcctIdAssocMember,
                        TransactionTypeReferenceDate = glTrans.IgpSysDateAssocMember,
                        TransactionNumber = glTrans.IgpTranNoAssocMember,
                        ExternalBatchReference = glTrans.IgpExtBatchAssocMember,
                        TransactionDetailLines = new List<GenLedgrTransactionDetail>()
                    };

                    try
                    {
                        var detailItem = inteGlPostingDetails.FirstOrDefault(igp => igp.Recordkey == glTrans.IgpTranDetailsAssocMember);
                        //V12 budget period
                        transactionItem.BudgetPeriodDate = glTrans.IgpTrDateAssocMember.HasValue ? glTrans.IgpTrDateAssocMember.Value : default(DateTime?);

                        foreach (var transDetail in detailItem.IgpdTranDetailsEntityAssociation)
                        {
                            if (!string.IsNullOrWhiteSpace(transDetail.IgpdGlNoAssocMember))
                            {
                                decimal amount = 0;
                                CreditOrDebit type = CreditOrDebit.Credit;
                                if (transDetail.IgpdCreditAssocMember != null && transDetail.IgpdCreditAssocMember.Value != 0)
                                {
                                    amount = transDetail.IgpdCreditAssocMember.Value;
                                }
                                else
                                {
                                    if (transDetail.IgpdDebitAssocMember != null && transDetail.IgpdDebitAssocMember.Value != 0)
                                    {
                                        amount = transDetail.IgpdDebitAssocMember.Value;
                                        type = CreditOrDebit.Debit;
                                    }
                                }
                                var amountAndCurrency = new AmountAndCurrency(amount, CurrencyCodes.USD);

                                var genLdgrTransactionDetail = new GenLedgrTransactionDetail(transDetail.IgpdGlNoAssocMember, transDetail.IgpdProjectIdsAssocMember, transDetail.IgpdDescriptionAssocMember, type, amountAndCurrency);
                                try
                                {
                                    genLdgrTransactionDetail.SequenceNumber = (!string.IsNullOrEmpty(transDetail.IgpdTranSeqNoAssocMember)) ? (int?)int.Parse(transDetail.IgpdTranSeqNoAssocMember) : null;
                                    genLdgrTransactionDetail.SubmittedBy = transDetail.IgpdSubmittedByAssocMember;
                                    genLdgrTransactionDetail.EncGiftUnits = transDetail.IgpdGiftUnitsAssocMember;
                                    genLdgrTransactionDetail.EncRefNumber = transDetail.IgpdEncRefNoAssocMember;
                                    genLdgrTransactionDetail.EncLineItemNumber = transDetail.IgpdEncLineItemNoAssocMember;
                                    genLdgrTransactionDetail.EncSequenceNumber = string.IsNullOrWhiteSpace(transDetail.IgpdEncSeqNoAssocMember) ? default(int?) : Convert.ToInt32(transDetail.IgpdEncSeqNoAssocMember);
                                    genLdgrTransactionDetail.EncAdjustmentType = transDetail.IgpdEncAdjTypeAssocMember;
                                    genLdgrTransactionDetail.EncCommitmentType = transDetail.IgpdEncCommitmentTypeAssocMember;
                                }
                                catch (Exception ex)
                                {
                                    // Leave sequence number off the domain entity
                                    logger.Error(ex, "Unable to get sequence number.");
                                }
                                transactionItem.TransactionDetailLines.Add(genLdgrTransactionDetail);
                            }
                        }
                        generalLedgerTransaction.GeneralLedgerTransactions.Add(transactionItem);
                    }
                    catch (ArgumentNullException e)
                    {
                        string message = string.Format("{0}, Id: {1}", e.Message, integGlPosting.Recordkey);
                        exception.AddError(new RepositoryError("Bad.Data", message)
                        {
                            SourceId = integGlPosting.Recordkey,
                            Id = integGlPosting.RecordGuid
                        });
                    }
                    catch
                    {
                        //throw new KeyNotFoundException(string.Format("INTG.GL.POSTINGS.DETAIL key {0} is missing from Colleague.", glTrans.IgpTranDetailsAssocMember));
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("INTG.GL.POSTINGS.DETAIL key {0} is missing from Colleague.", glTrans.IgpTranDetailsAssocMember))
                        {
                            SourceId = integGlPosting.Recordkey,
                            Id = integGlPosting.RecordGuid
                        });
                    }
                }
            }
            return generalLedgerTransaction;
        }

        /// <summary>
        /// Gets GeneralLedgerTransaction domain entity
        /// </summary>
        /// <param name="generalLedgerTransaction"></param>
        /// <param name="GlConfig"></param>
        /// <returns></returns>
        private async Task<GeneralLedgerTransaction> CreateGeneralLedgerTransaction(GeneralLedgerTransaction generalLedgerTransaction, GeneralLedgerAccountStructure GlConfig)
        {
            var tranType = new List<string>();
            var tranRefNo = new List<string>();
            var tranNo = new List<string>();
            var tranExtBatch = new List<string>();
            var tranTrDate = new List<Nullable<DateTime>>();
            var tranRefDate = new List<Nullable<DateTime>>();
            var tranAcctId = new List<string>();
            var tranDetSeqNo = new List<string>();
            var tranDetGl = new List<string>();
            var tranDetProj = new List<string>();
            var tranDetDesc = new List<string>();
            var tranDetAmt = new List<string>();
            var tranDetType = new List<string>();
            var tranDetSubmittedBy = new List<string>();

            foreach (var transaction in generalLedgerTransaction.GeneralLedgerTransactions)
            {
                tranType.Add(transaction.Source);
                tranRefNo.Add(transaction.ReferenceNumber);
                tranNo.Add(transaction.TransactionNumber);
                tranExtBatch.Add(transaction.ExternalBatchReference);
                tranTrDate.Add(transaction.LedgerDate.Date);
                if (transaction.TransactionTypeReferenceDate.HasValue)
                    tranRefDate.Add(transaction.TransactionTypeReferenceDate.GetValueOrDefault().Date);
                tranAcctId.Add(transaction.ReferencePersonId);

                if (transaction.TransactionDetailLines.Any())
                {
                    int xIndex = transaction.TransactionDetailLines.Count();
                    string[] detailSeqNo = new string[xIndex];
                    string[] detailGl = new string[xIndex];
                    string[] detailProj = new string[xIndex];
                    string[] detailDesc = new string[xIndex];
                    string[] detailAmt = new string[xIndex];
                    string[] detailType = new string[xIndex];
                    string[] detailSubmittedBy = new string[xIndex];

                    int xCtr = 0;
                    foreach (var detail in transaction.TransactionDetailLines)
                    {
                        detailSeqNo[xCtr] = detail.SequenceNumber.ToString();
                        string formatGl = detail.GlAccount.GlAccountNumber;

                        //strip any delimiter sent in so only numbers show.
                        formatGl = Regex.Replace(formatGl, "[^0-9a-zA-Z]", "");
                        //if A GL number is larger then 15 in length then we need to add the Underscores
                        if (formatGl.Length > 15)
                        {
                            int startLoc = 0;
                            string tempGlNo = string.Empty;
                            int x = 0, glCount = GlConfig.MajorComponents.Count();
                            foreach (var glMajor in GlConfig.MajorComponents)
                            {
                                try
                                {
                                    x++;
                                    if (x < glCount) { tempGlNo = tempGlNo + formatGl.Substring(startLoc, glMajor.ComponentLength) + "_"; }
                                    else { tempGlNo = tempGlNo + formatGl.Substring(startLoc, glMajor.ComponentLength); }
                                }
                                catch (ArgumentOutOfRangeException aex)
                                {
                                    throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", formatGl));
                                }
                                startLoc += glMajor.ComponentLength;
                            }
                            formatGl = tempGlNo;
                        }

                        detailGl[xCtr] = formatGl;
                        detailProj[xCtr] = detail.ProjectId;
                        detailDesc[xCtr] = detail.GlAccount.GlAccountDescription;
                        detailAmt[xCtr] = detail.Amount.Value.ToString();
                        detailType[xCtr] = detail.Type.ToString();
                        detailSubmittedBy[xCtr] = detail.SubmittedBy;
                        xCtr += 1;
                    }
                    tranDetSeqNo.Add(string.Join(DmiString.sSM, detailSeqNo));
                    tranDetGl.Add(string.Join(DmiString.sSM, detailGl));
                    tranDetProj.Add(string.Join(DmiString.sSM, detailProj));
                    tranDetDesc.Add(string.Join(DmiString.sSM, detailDesc));
                    tranDetAmt.Add(string.Join(DmiString.sSM, detailAmt));
                    tranDetType.Add(string.Join(DmiString.sSM, detailType));
                    tranDetSubmittedBy.Add(string.Join(DmiString.sSM, detailSubmittedBy));
                }
            }
            var request = new CreateGLPostingRequest()
            {
                PostingGuid = generalLedgerTransaction.Id,
                Mode = generalLedgerTransaction.ProcessMode,
                SubmittedBy = generalLedgerTransaction.SubmittedBy,
                TranType = tranType,
                TranRefNo = tranRefNo,
                TranNo = tranNo,
                TranExtBatch = tranExtBatch,
                TranRefDate = tranRefDate,
                TranTrDate = tranTrDate,
                TranAcctId = tranAcctId,
                TranDetSeqNo = tranDetSeqNo,
                TranDetGl = tranDetGl,
                TranDetProj = tranDetProj,
                TranDetDesc = tranDetDesc,
                TranDetAmt = tranDetAmt,
                TranDetType = tranDetType,
                TranDetSubmittedBy = tranDetSubmittedBy
            };

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.PostingGuid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.PostingGuid = string.Empty;
            }

            // process extended data
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)

            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.Error))
            {
                var errorMessage = string.Format("Error(s) occurred updating general-ledger-transactions for id: '{0}'.", request.PostingGuid);

                foreach (var errMsg in updateResponse.CreateGlPostingError)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(errMsg.ErrorCodes, " ", errMsg.ErrorMessages)));
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw exception;
            }

            // Process the messages returned by colleague
            if (updateResponse.WarningMessages.Any())
            {
                foreach (var message in updateResponse.WarningMessages)
                {
                    logger.Warn(message);
                }
            }

            var generalLedgerTransactionResponse = new GeneralLedgerTransaction()
            {
                Id = updateResponse.PostingGuid,
                ProcessMode = generalLedgerTransaction.ProcessMode,
                SubmittedBy = generalLedgerTransaction.SubmittedBy,
                GeneralLedgerTransactions = generalLedgerTransaction.GeneralLedgerTransactions
            };

            return generalLedgerTransactionResponse;
        }


        #region V12 Changes

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to create</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<GeneralLedgerTransaction> Create2Async(GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig)
        {
            if (!string.IsNullOrEmpty(generalLedgerTransaction.Id))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!generalLedgerTransaction.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(generalLedgerTransaction.Id);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("Integration GL Postings record {0} already exists.", generalLedgerTransaction.Id));
                    }
                }
            }
            var response = await CreateGeneralLedgerTransaction2(generalLedgerTransaction, GlConfig);
            if (response == null || string.IsNullOrEmpty(response.Id) || response.Id.Equals(string.Empty, StringComparison.OrdinalIgnoreCase) || response.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return response;
            }
            else
            {
                var generalLedgerTransactionEntity = await this.GetById2Async(response.Id, personId, glAccessLevel);
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return generalLedgerTransactionEntity;
            }
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to update</param>
        /// <param name="id">The general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<GeneralLedgerTransaction> Update2Async(string id, GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
            if (!generalLedgerTransaction.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var recordInfo = await GetRecordInfoFromGuidAsync(id);
                if (recordInfo != null)
                {
                    throw new InvalidOperationException(string.Format("Integration GL Postings record {0} already exists.  Cannot update an existing entity in Colleague.", id));
                }
            }
            var response = await CreateGeneralLedgerTransaction2(generalLedgerTransaction, GlConfig);
            if (response == null || string.IsNullOrEmpty(response.Id) || response.Id.Equals(string.Empty, StringComparison.OrdinalIgnoreCase) || response.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return response;
            }
            else
            {
                var generalLedgerTransactionEntity = await this.GetById2Async(response.Id, personId, glAccessLevel);
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return generalLedgerTransactionEntity;
            }
        }

        /// <summary>
        /// Gets GeneralLedgerTransaction domain entuty
        /// </summary>
        /// <param name="generalLedgerTransaction"></param>
        /// <param name="GlConfig"></param>
        /// <returns></returns>
        private async Task<GeneralLedgerTransaction> CreateGeneralLedgerTransaction2(GeneralLedgerTransaction generalLedgerTransaction, GeneralLedgerAccountStructure GlConfig)
        {
            var tranType = new List<string>();
            var tranRefNo = new List<string>();
            var tranExtBatch = new List<string>();
            var tranNo = new List<string>();
            var tranTrDate = new List<Nullable<DateTime>>();
            var tranRefDate = new List<Nullable<DateTime>>();
            var tranAcctId = new List<string>();
            var tranDetSeqNo = new List<string>();
            var tranDetGl = new List<string>();
            var tranDetProj = new List<string>();
            var tranDetDesc = new List<string>();
            var tranDetAmt = new List<string>();
            var tranDetType = new List<string>();
            var tranDetSubmittedBy = new List<string>();
            //V12 Encumbrance changes
            var giftUnits = new List<string>();
            var encNumber = new List<string>();
            var encLineItemNumber = new List<string>();
            var encSequenceNumber = new List<string>();
            var encAdjType = new List<string>();
            var encCommitmentType = new List<string>();


            foreach (var transaction in generalLedgerTransaction.GeneralLedgerTransactions)
            {
                tranType.Add(transaction.Source);
                tranRefNo.Add(transaction.ReferenceNumber);
                tranExtBatch.Add(transaction.ExternalBatchReference);
                tranNo.Add(transaction.TransactionNumber);
                tranTrDate.Add(transaction.LedgerDate.Date);
                if (transaction.TransactionTypeReferenceDate.HasValue)
                    tranRefDate.Add(transaction.TransactionTypeReferenceDate.Value.Date);
                tranAcctId.Add(transaction.ReferencePersonId);

                if (transaction.TransactionDetailLines.Any())
                {
                    int xIndex = transaction.TransactionDetailLines.Count();
                    string[] detailSeqNo = new string[xIndex];
                    string[] detailGl = new string[xIndex];
                    string[] detailProj = new string[xIndex];
                    string[] detailDesc = new string[xIndex];
                    string[] detailAmt = new string[xIndex];
                    string[] detailType = new string[xIndex];
                    string[] detailSubmittedBy = new string[xIndex];
                    //BSF
                    string[] detailEncAdjType = new string[xIndex];
                    string[] detailEncCommitmentType = new string[xIndex];
                    string[] detailEncLineItemNumber = new string[xIndex];
                    string[] detailEncRefNumber = new string[xIndex];
                    string[] detailEncSequenceNumber = new string[xIndex];
                    string[] detailEncGiftUnits = new string[xIndex];
                    //end BSF
                    int xCtr = 0;
                    foreach (var detail in transaction.TransactionDetailLines)
                    {
                        detailSeqNo[xCtr] = detail.SequenceNumber.ToString();
                        string formatGl = detail.GlAccount.GlAccountNumber;

                        //strip any delimiter sent in so only numbers show.
                        formatGl = Regex.Replace(formatGl, "[^0-9a-zA-Z]", "");
                        //if A GL number is larger then 15 in length then we need to add the Underscores
                        if (formatGl.Length > 15)
                        {
                            int startLoc = 0;
                            string tempGlNo = string.Empty;
                            int x = 0, glCount = GlConfig.MajorComponents.Count();
                            foreach (var glMajor in GlConfig.MajorComponents)
                            {
                                try
                                {
                                    x++;
                                    if (x < glCount) { tempGlNo = tempGlNo + formatGl.Substring(startLoc, glMajor.ComponentLength) + "_"; }
                                    else { tempGlNo = tempGlNo + formatGl.Substring(startLoc, glMajor.ComponentLength); }

                                    startLoc += glMajor.ComponentLength;
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    //throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", formatGl));
                                    exception.AddError(new RepositoryError("Bad.Data", string.Format("Invalid GL account number: {0}", formatGl))
                                    {
                                        Id = generalLedgerTransaction.Id
                                    });
                                }
                            }
                            formatGl = tempGlNo;
                        }

                        detailGl[xCtr] = formatGl;
                        detailProj[xCtr] = detail.ProjectId;
                        if (detail.GlAccount != null)
                        {
                            detailDesc[xCtr] = detail.GlAccount.GlAccountDescription;
                        }
                        if (detail.Amount != null && detail.Amount.Value.HasValue)
                        {
                            detailAmt[xCtr] = detail.Amount.Value.Value.ToString();
                        }

                        detailType[xCtr] = detail.Type.ToString();
                        detailSubmittedBy[xCtr] = detail.SubmittedBy;
                        if (!string.IsNullOrEmpty(detail.EncAdjustmentType))
                        {
                            detailEncAdjType[xCtr] = detail.EncAdjustmentType;
                        }
                        if (!string.IsNullOrEmpty(detail.EncCommitmentType))
                        {
                            detailEncCommitmentType[xCtr] = detail.EncCommitmentType;
                        }
                        if (!string.IsNullOrEmpty(detail.EncLineItemNumber))
                        {
                            detailEncLineItemNumber[xCtr] = detail.EncLineItemNumber;
                        }
                        if (!string.IsNullOrEmpty(detail.EncRefNumber))
                        {
                            detailEncRefNumber[xCtr] = detail.EncRefNumber;
                        }
                        if (detail.EncSequenceNumber != null && detail.EncSequenceNumber.HasValue)
                        {
                            detailEncSequenceNumber[xCtr] = detail.EncSequenceNumber.Value.ToString();
                        }
                        if (!string.IsNullOrEmpty(detail.EncGiftUnits))
                        {
                            detailEncGiftUnits[xCtr] = detail.EncGiftUnits;
                        }

                        xCtr += 1;
                    }
                    encAdjType.Add(string.Join(DmiString.sSM, detailEncAdjType));
                    encCommitmentType.Add(string.Join(DmiString.sSM, detailEncCommitmentType));
                    encLineItemNumber.Add(string.Join(DmiString.sSM, detailEncLineItemNumber));
                    encNumber.Add(string.Join(DmiString.sSM, detailEncRefNumber));
                    encSequenceNumber.Add(string.Join(DmiString.sSM, detailEncSequenceNumber));
                    giftUnits.Add(string.Join(DmiString.sSM, detailEncGiftUnits));

                    tranDetSeqNo.Add(string.Join(DmiString.sSM, detailSeqNo));
                    tranDetGl.Add(string.Join(DmiString.sSM, detailGl));
                    tranDetProj.Add(string.Join(DmiString.sSM, detailProj));
                    tranDetDesc.Add(string.Join(DmiString.sSM, detailDesc));
                    tranDetAmt.Add(string.Join(DmiString.sSM, detailAmt));
                    tranDetType.Add(string.Join(DmiString.sSM, detailType));
                    tranDetSubmittedBy.Add(string.Join(DmiString.sSM, detailSubmittedBy));
                }
            }
            //Hard coded Mode = "update" per KJ, since these values changed in V12, they may create issues with V6/8
            var request = new CreateGLPostingRequest()
            {
                PostingGuid = generalLedgerTransaction.Id,
                Mode = (generalLedgerTransaction.ProcessMode != null && generalLedgerTransaction.ProcessMode.ToUpperInvariant().Equals("VALIDATE")) ? "VALIDATE" : "UPDATE",
                SubmittedBy = generalLedgerTransaction.SubmittedBy,
                TranType = tranType,
                TranRefNo = tranRefNo,
                TranExtBatch = tranExtBatch,
                TranNo = tranNo,
                TranRefDate = tranRefDate,
                TranTrDate = tranTrDate,
                TranAcctId = tranAcctId,
                TranDetSeqNo = tranDetSeqNo,
                TranDetGl = tranDetGl,
                TranDetProj = tranDetProj,
                TranDetDesc = tranDetDesc,
                TranDetAmt = tranDetAmt,
                TranDetType = tranDetType,
                TranDetSubmittedBy = tranDetSubmittedBy,
                //V12
                Comments = generalLedgerTransaction.Comment,
                TranDetEncAdjType = encAdjType,
                TranDetEncComType = encCommitmentType,
                TranDetEncLineNo = encLineItemNumber,
                TranDetEncRefNo = encNumber,
                TranDetEncSeqNo = encSequenceNumber,
                TranDetGiftUnits = giftUnits
            };

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.PostingGuid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.PostingGuid = string.Empty;
            }

            // process extended data
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)

            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.Error) && !updateResponse.Error.Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                var errorMessage = "Error(s) occurred updating general-ledger-transactions";

                foreach (var errMsg in updateResponse.CreateGlPostingError)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(errMsg.ErrorCodes, " ", errMsg.ErrorMessages))
                    {
                        Id = !string.IsNullOrEmpty(updateResponse.PostingGuid) ? updateResponse.PostingGuid : request.PostingGuid,
                        SourceId = updateResponse.PostingId
                    });
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
            }

            // Process the messages returned by colleague
            if (updateResponse.WarningMessages.Any())
            {
                foreach (var message in updateResponse.WarningMessages)
                {
                    logger.Warn(message);
                }
            }

            var generalLedgerTransactionResponse = new GeneralLedgerTransaction()
            {
                Id = updateResponse.PostingGuid,
                ProcessMode = generalLedgerTransaction.ProcessMode,
                SubmittedBy = generalLedgerTransaction.SubmittedBy,
                GeneralLedgerTransactions = generalLedgerTransaction.GeneralLedgerTransactions,
                Comment = generalLedgerTransaction.Comment
            };

            return generalLedgerTransactionResponse;
        }

        /// <summary>
        /// Gets project ref no's.
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>

        public async Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(projectIds);
            if (projects != null && projects.Any())
            {
                foreach (var project in projects)
                {
                    if (!dict.ContainsKey(project.Recordkey))
                    {
                        dict.Add(project.Recordkey, project.PrjRefNo);
                    }
                }
            }
            return dict;
        }

        #endregion
    }
}
