// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class AccountFundsAvailableService : BaseCoordinationService, IAccountFundsAvailableService
    {
        private IGeneralLedgerUserRepository _generalLedgerUserRepository;
        private IPersonRepository _personRepository;
        private readonly IAccountFundsAvailableRepository _accountFundsAvailableRepository;
        private readonly IGeneralLedgerConfigurationRepository _generalLedgerConfigurationRepository;
        private readonly IColleagueFinanceReferenceDataRepository _colleagueFinanceReferenceDataRepository;

        private readonly ILogger _logger;

        public AccountFundsAvailableService(IPersonRepository personRepository, IAccountFundsAvailableRepository accountFundsAvailableRepository, IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository, 
                    IGeneralLedgerUserRepository generalLedgerUserRepository, IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _personRepository = personRepository;
            _generalLedgerUserRepository = generalLedgerUserRepository;
            _colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            _accountFundsAvailableRepository = accountFundsAvailableRepository;
            _generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            _logger = logger;
        }

        /// <summary>
        /// The primary use of the Account Funds Available entity is to validate the status of funds (available, not available, and not available, but may be overridden) 
        /// for an accounting string as of a given date.
        /// </summary>
        /// <param name="accountingStringValue"></param>
        /// <param name="amountValue"></param>
        /// <param name="balanceOn"></param>
        /// <param name="submittedByValue"></param>
        /// <returns></returns>
        public async Task<Dtos.AccountFundsAvailable> GetAccountFundsAvailableByFilterCriteriaAsync(string accountingStringValue, decimal amountValue, DateTime? balanceOn, string submittedByValue)
        {
            Dtos.AccountFundsAvailable fundsAvailableDto = new AccountFundsAvailable();
            try
            {
                CheckUserAccountFundsAvailableViewPermissions();
                
                var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

                var submittedById = "";

                if (!string.IsNullOrWhiteSpace(submittedByValue))
                {
                    submittedById = await _personRepository.GetPersonIdFromGuidAsync(submittedByValue);
                    if (string.IsNullOrWhiteSpace(submittedById))
                        throw new KeyNotFoundException("Could not find submittedBy ID from the GUID supllied.");
                }
                    
                var fundAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingStringValue)
                {
                    SubmittedBy = submittedById,
                    Amount = amountValue,
                    TransactionDate = balanceOn
                };
                
                fundsAvailable.Add(fundAvailable);

                var availableFunds = await _accountFundsAvailableRepository.CheckAvailableFundsAsync(fundsAvailable);

                //We are only sending one accounting string so will review only one coming back after it was checked.
                var availablefund = availableFunds[0];

                fundsAvailableDto.AccountingStringValue = availablefund.AccountString;
                if (availablefund.TransactionDate.HasValue)
                {
                    fundsAvailableDto.BalanceOn = availablefund.TransactionDate.Value.Date;
                } else { fundsAvailableDto.BalanceOn = DateTime.UtcNow; }
                
                switch (availablefund.AvailableStatus)
                {
                    case FundsAvailableStatus.Availbale:
                        fundsAvailableDto.FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available;
                        break;
                    case FundsAvailableStatus.NotAvailable:
                        fundsAvailableDto.FundsAvailable = Dtos.EnumProperties.FundsAvailable.NotAvailable;
                        break;
                    case FundsAvailableStatus.Override:
                        fundsAvailableDto.FundsAvailable = Dtos.EnumProperties.FundsAvailable.OverrideAvailable;
                        break;
                    case FundsAvailableStatus.NotApplicable:
                        fundsAvailableDto.FundsAvailable = Dtos.EnumProperties.FundsAvailable.NotApplicable;
                        break;
                    default:
                        fundsAvailableDto.FundsAvailable = Dtos.EnumProperties.FundsAvailable.NotAvailable;
                        break;
                }
                
                return fundsAvailableDto;
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                throw keyNotFoundException;
            }
            catch (RepositoryException repositoryException)
            {
                throw repositoryException;
            }
            catch (ArgumentNullException argumentNullException)
            {
                throw argumentNullException;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unexpected Error in AccountFundsAvailable Service");
                throw exception;
            }
        }
        
        public async Task<AccountFundsAvailable_Transactions> CheckAccountFundsAvailable_Transactions2Async(AccountFundsAvailable_Transactions transaction)
        {
            var outTransaction = new AccountFundsAvailable_Transactions();

            outTransaction.Transactions = new List<AccountFundsAvailable_Transactionstransactions>();

            var transferTransactions = new List<AccountFundsAvailable_Transactionstransactions>();
            var availableFunds = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            try
            {

                var transTypeValcodes = await _colleagueFinanceReferenceDataRepository.GetGlSourceCodesValcodeAsync(false);
                //condense the transactions and merge duplicates together.
                var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
                int transactionLevel = 0;
                var transactionsTypes = new List<AccountFundsAvailable_TransactionsType>();

                if (transaction == null || transaction.Transactions == null)
                    throw new ArgumentNullException("AccountsFundsAvailableTransactions",
                        "Must provide a request body.");

                foreach (var checkTrans in transaction.Transactions)
                {
                    if (checkTrans.Type == Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.NotSet)
                    {
                        throw new InvalidOperationException("You must have a Transaction type");
                    }
                    if (checkTrans.Type != Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.purchaseJournal && checkTrans.Type != Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.generalEncumbranceCreate && checkTrans.Type != Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.approvedBudgetAdjustment)
                    {
                        throw new InvalidOperationException("Type not supported.");
                    }
                    transactionsTypes.Add(checkTrans.Type);
                    if (checkTrans.TransactionDetailLines == null || checkTrans.TransactionDetailLines.Count <= 0)
                    {
                        throw new InvalidOperationException("You must have a Transaction detail lines");
                    }

                    var transTypeValcode = transTypeValcodes.FirstOrDefault(x => x.GlSourceCodeProcess3 == checkTrans.Type.ToString());
                    if (transTypeValcode == null)
                    {
                        throw new InvalidOperationException("You have sent in a Type that not within Colleague. Type is " + checkTrans.Type.ToString());
                    }

                    #region BPO, PO, REQ
                    if (checkTrans.ReferenceDocument != null && !string.IsNullOrEmpty(checkTrans.ReferenceDocument.ItemNumber))
                    {
                        /*
                            Check if the line item is associated with a BPO, if so produce an error as BPO line itmes are not to be considered.
                            If the key received has the field ITM.BPO.ID populated then return an error (1) 
                        */
                        string bpoId = await _accountFundsAvailableRepository.GetBpoAsync(checkTrans.ReferenceDocument.ItemNumber);
                        if (!string.IsNullOrEmpty(bpoId))
                        {
                            throw new InvalidOperationException("A line item from a Blanket Purchase order is not permitted to determine account funds available.");
                        }

                        /*
                            If the PO.STATUS NE O return an error (2).
                            Check if the line item is associated with a with a PO. Check that the PO is outstanding (PO.STATUS = O). If any other status produce an error.
                            If the key received has the field ITM.PO.ID populated confirm that the key is stored in the array for the PO's PO.ITEMS.ID (array with the entity PURCHASE.ORDERS) and check that the PO's current status (PO.STATUS) is O - Outstanding.
                            If the PO.STATUS NE O return an error (2).
                        */

                        string poStatus = await _accountFundsAvailableRepository.GetPOStatusByItemNumber(checkTrans.ReferenceDocument.ItemNumber);
                        if (!string.IsNullOrEmpty(poStatus) && !poStatus.ToUpper().Equals("O", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("In order to determine account funds available by line number the associated purchase order must have a current status of outstanding.");
                        }

                        /*
                            Check if the line item is associated with a requisition and check that the requisition has a status of Not Approved or Outstanding (REQ.STATUS = N or O).
                            If the key received has the fields ITM.REQ.ID populated, first confirm that both ITM.PO.ID or ITM.BPO.ID are NOT populated and that the key is stored in the 
                            array for the Req's REQ.ITEMS.ID (array with the entity REQUISITIONS) and check that the Req's current status (REQ.STATUS) is O - Outstanding or N - Not Approved.
                            If the REQ.STATUS NE O or N return an error (3).
                        */

                        string reqStatus = await _accountFundsAvailableRepository.GetReqStatusByItemNumber(checkTrans.ReferenceDocument.ItemNumber);
                        if (!string.IsNullOrEmpty(reqStatus) &&
                                (!reqStatus.ToUpper().Equals("O", StringComparison.OrdinalIgnoreCase) &&
                                !reqStatus.ToUpper().Equals("N", StringComparison.OrdinalIgnoreCase))
                           )
                        {
                            throw new InvalidOperationException("In order to determine account funds available by line number the associated requisition must have a current status of outstanding or not approved.");
                        }
                    }
                    #endregion

                    int transactionDetailLevel = 1;

                    if (checkTrans.TransactionDetailLines == null)
                    {
                        throw new InvalidOperationException("You must have a Transaction detail lines.");
                    }

                    foreach (var checkDetails in checkTrans.TransactionDetailLines)
                    {
                        if (checkDetails.Amount != null && checkDetails.Amount.Value.HasValue)
                        {
                            var submittedById = string.Empty;
                            if (checkDetails.SubmittedBy != null && !string.IsNullOrEmpty(checkDetails.SubmittedBy.Id))
                            {
                                submittedById = await _personRepository.GetPersonIdFromGuidAsync(checkDetails.SubmittedBy.Id);
                                if (string.IsNullOrEmpty(submittedById))
                                {
                                    throw new KeyNotFoundException("SubmittedBy Id is not a valid.");
                                }
                            }

                            var fundAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(checkDetails.AccountingString)
                            {
                                SubmittedBy = submittedById,

                                TransactionDate = checkTrans.TransactionDate
                            };

                            if (checkDetails.Type == null)
                            {
                                throw new ArgumentNullException("TransactionDetailLines.Type is required.");
                            }

                            if (checkDetails.Type == Dtos.EnumProperties.CreditOrDebit.Debit)
                                fundAvailable.Amount = Convert.ToDecimal(checkDetails.Amount.Value);
                            else
                                fundAvailable.Amount = Decimal.Negate(Convert.ToDecimal(checkDetails.Amount.Value));

                            string currency = string.Empty;
                            switch (checkDetails.Amount.Currency)
                            {
                                case Dtos.EnumProperties.CurrencyCodes.USD:
                                    currency = "USD";
                                    break;
                                case Dtos.EnumProperties.CurrencyCodes.CAD:
                                    currency = "CAD";
                                    break;
                                default:
                                    break;
                            }

                            fundAvailable.ItemId = (checkTrans.ReferenceDocument != null) ? checkTrans.ReferenceDocument.ItemNumber : "";

                            fundAvailable.CurrencyCode = currency;
                            fundAvailable.Sequence = transactionLevel.ToString(); // string.Concat(transactionLevel, "*", transactionDetailLevel);
                            fundsAvailable.Add(fundAvailable);
                            transactionDetailLevel++;
                        }              
                    }               
                    transactionLevel++;
                }

                availableFunds = await _accountFundsAvailableRepository.CheckAvailableFundsAsync(fundsAvailable);

                transactionLevel = 0;
                if (availableFunds != null)
                {
                    var groupedAvailableFunds = availableFunds.GroupBy(item => item.Sequence);

                    foreach (var groupedAvailableFund in groupedAvailableFunds)
                    {
                        var tempTransaction = new AccountFundsAvailable_Transactionstransactions()
                        {
                            Type = transactionsTypes[transactionLevel]
                        };
                        var sequence = groupedAvailableFund.FirstOrDefault(x => x.Sequence == transactionLevel.ToString());

                        if (sequence != null)
                        {
                            if (sequence.TransactionDate == null)
                            {
                                tempTransaction.TransactionDate = DateTime.Today;
                            }
                            else
                            {
                                tempTransaction.TransactionDate = sequence.TransactionDate;
                            }

                            tempTransaction.TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines>();

                            foreach (var inAccountingString in groupedAvailableFund)
                            {
                                var tempAccountingString = new AccountFundsAvailable_TransactionstransactionDetailLines()
                                {
                                    AccountingString = inAccountingString.AccountString
                                };

                                tempAccountingString.Type = inAccountingString.Amount < 0 ? Dtos.EnumProperties.CreditOrDebit.Credit : Dtos.EnumProperties.CreditOrDebit.Debit;

                                tempAccountingString.Amount = new Dtos.DtoProperties.AmountDtoProperty
                                {
                                    Value = Math.Abs(inAccountingString.Amount),
                                };

                                var currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                switch (inAccountingString.CurrencyCode)
                                {
                                    case "USD":
                                        currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        break;
                                    case "CAD":
                                        currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                                        break;
                                    default:
                                        break;
                                }
                                tempAccountingString.Amount.Currency = currency;

                                if (!string.IsNullOrEmpty(inAccountingString.SubmittedBy))
                                {
                                    tempAccountingString.SubmittedBy = new GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(inAccountingString.SubmittedBy));
                                }

                                var status = Dtos.EnumProperties.FundsAvailable.NotSet;
                                switch (inAccountingString.AvailableStatus)
                                {
                                    case FundsAvailableStatus.Availbale:
                                        status = Dtos.EnumProperties.FundsAvailable.Available; break;
                                    case FundsAvailableStatus.NotAvailable:
                                        status = Dtos.EnumProperties.FundsAvailable.NotAvailable; break;
                                    case FundsAvailableStatus.Override:
                                        status = Dtos.EnumProperties.FundsAvailable.OverrideAvailable; break;
                                    case FundsAvailableStatus.NotApplicable:
                                        status = Dtos.EnumProperties.FundsAvailable.NotApplicable;
                                        break;
                                    default: break;
                                }
                                tempAccountingString.FundsAvailable = status;

                                tempTransaction.TransactionDetailLines.Add(tempAccountingString);

                            }

                            outTransaction.Transactions.Add(tempTransaction);
                        }
                        transactionLevel++;
                    }
                }
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                throw keyNotFoundException;
            }
            catch (RepositoryException repositoryException)
            {
                throw repositoryException;
            }
            catch (ArgumentNullException argumentNullException)
            {
                throw argumentNullException;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unexpected Error in AccountFundsAvailable Service");
                throw exception;
            }

            return outTransaction;
        }

        public async Task<AccountFundsAvailable_Transactions2> CheckAccountFundsAvailable_Transactions3Async(AccountFundsAvailable_Transactions2 transaction)
        {
            var outTransaction = new AccountFundsAvailable_Transactions2();

            outTransaction.Transactions = new List<AccountFundsAvailable_Transactionstransactions2>();

            var transferTransactions = new List<AccountFundsAvailable_Transactionstransactions2>();
            var availableFunds = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            try
            {
                var transTypeValcodes = await _colleagueFinanceReferenceDataRepository.GetGlSourceCodesValcodeAsync(false);
                //condense the transactions and merge duplicates together.
                var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
                int transactionLevel = 0;
                var transactionsTypes = new List<AccountFundsAvailable_TransactionsType>();

                if (transaction == null || transaction.Transactions == null)
                    throw new ArgumentNullException("AccountsFundsAvailableTransactions",
                        "Must provide a request body.");

                foreach (var checkTrans in transaction.Transactions)
                {
                    if (checkTrans.Type == Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.NotSet)
                    {
                        throw new InvalidOperationException("You must have a Transaction type");
                    }
                   
                    transactionsTypes.Add(checkTrans.Type);
                    if (checkTrans.TransactionDetailLines == null || checkTrans.TransactionDetailLines.Count <= 0)
                    {
                        throw new InvalidOperationException("You must have a Transaction detail lines");
                    }
                    
                    int transactionDetailLevel = 1;

                    if(checkTrans.TransactionDetailLines == null)
                    {
                        throw new InvalidOperationException("You must have a Transaction detail lines.");
                    }

                    foreach (var checkDetails in checkTrans.TransactionDetailLines)
                    {
                        if (checkDetails.Amount != null && checkDetails.Amount.Value.HasValue)
                        {
                            var submittedById = string.Empty;
                            if (checkDetails.SubmittedBy != null && !string.IsNullOrEmpty(checkDetails.SubmittedBy.Id))
                            {
                                submittedById = await _personRepository.GetPersonIdFromGuidAsync(checkDetails.SubmittedBy.Id);
                                if (string.IsNullOrEmpty(submittedById))
                                {
                                    throw new KeyNotFoundException("SubmittedBy Id is not a valid.");
                                }
                            }

                            var fundAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(checkDetails.AccountingString)
                            {
                                SubmittedBy = submittedById,

                                TransactionDate = checkTrans.TransactionDate
                            };

                            if (checkDetails.Type == null)
                            {
                                throw new ArgumentNullException("TransactionDetailLines.Type is required.");
                            }

                            if (checkDetails.Type == Dtos.EnumProperties.CreditOrDebit.Debit)
                                fundAvailable.Amount = Convert.ToDecimal(checkDetails.Amount.Value);
                            else
                                fundAvailable.Amount = Decimal.Negate(Convert.ToDecimal(checkDetails.Amount.Value));

                            string currency = string.Empty;
                            switch (checkDetails.Amount.Currency)
                            {
                                case Dtos.EnumProperties.CurrencyCodes.USD:
                                    currency = "USD";
                                    break;
                                case Dtos.EnumProperties.CurrencyCodes.CAD:
                                    currency = "CAD";
                                    break;
                                default:
                                    break;
                            }

                            fundAvailable.ItemId = (checkDetails.ReferenceDocument != null) ? checkDetails.ReferenceDocument.ItemNumber : "";
                            fundAvailable.CurrencyCode = currency;
                            fundAvailable.Sequence = transactionLevel.ToString(); // string.Concat(transactionLevel, "*", transactionDetailLevel);
                            fundsAvailable.Add(fundAvailable);
                            transactionDetailLevel++;

                        }
                        #region BPO, PO, REQ
                        if (checkDetails.ReferenceDocument != null && !string.IsNullOrEmpty(checkDetails.ReferenceDocument.ItemNumber))
                        {
                            /*
                                Check if the line item is associated with a BPO, if so produce an error as BPO line itmes are not to be considered.
                                If the key received has the field ITM.BPO.ID populated then return an error (1) 
                            */
                            string bpoId = await _accountFundsAvailableRepository.GetBpoAsync(checkDetails.ReferenceDocument.ItemNumber);
                            if (!string.IsNullOrEmpty(bpoId))
                            {
                                throw new InvalidOperationException("A line item from a Blanket Purchase order is not permitted to determine account funds available.");
                            }

                            /*
                                If the PO.STATUS NE O return an error (2).
                                Check if the line item is associated with a with a PO. Check that the PO is outstanding (PO.STATUS = O). If any other status produce an error.
                                If the key received has the field ITM.PO.ID populated confirm that the key is stored in the array for the PO's PO.ITEMS.ID (array with the entity PURCHASE.ORDERS) and check that the PO's current status (PO.STATUS) is O - Outstanding.
                                If the PO.STATUS NE O return an error (2).
                            */

                            string poStatus = await _accountFundsAvailableRepository.GetPOStatusByItemNumber(checkDetails.ReferenceDocument.ItemNumber);
                            if (!string.IsNullOrEmpty(poStatus) && !poStatus.ToUpper().Equals("O", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException("In order to determine account funds available by line number the associated purchase order must have a current status of outstanding.");
                            }

                            /*
                                Check if the line item is associated with a requisition and check that the requisition has a status of Not Approved or Outstanding (REQ.STATUS = N or O).
                                If the key received has the fields ITM.REQ.ID populated, first confirm that both ITM.PO.ID or ITM.BPO.ID are NOT populated and that the key is stored in the 
                                array for the Req's REQ.ITEMS.ID (array with the entity REQUISITIONS) and check that the Req's current status (REQ.STATUS) is O - Outstanding or N - Not Approved.
                                If the REQ.STATUS NE O or N return an error (3).
                            */

                            string reqStatus = await _accountFundsAvailableRepository.GetReqStatusByItemNumber(checkDetails.ReferenceDocument.ItemNumber);
                            if (!string.IsNullOrEmpty(reqStatus) &&
                                    (!reqStatus.ToUpper().Equals("O", StringComparison.OrdinalIgnoreCase) &&
                                    !reqStatus.ToUpper().Equals("N", StringComparison.OrdinalIgnoreCase))
                               )
                            {
                                throw new InvalidOperationException("In order to determine account funds available by line number the associated requisition must have a current status of outstanding or not approved.");
                            }
                        }
                        #endregion
                    }
                    transactionLevel++;
                }

                availableFunds = await _accountFundsAvailableRepository.CheckAvailableFundsAsync(fundsAvailable);

                transactionLevel = 0;
                if (availableFunds != null)
                {
                    var groupedAvailableFunds = availableFunds.GroupBy(item => item.Sequence);

                    foreach (var groupedAvailableFund in groupedAvailableFunds)
                    {
                        var tempTransaction = new AccountFundsAvailable_Transactionstransactions2()
                        {
                            Type = transactionsTypes[transactionLevel]
                        };
                        var sequence = groupedAvailableFund.FirstOrDefault(x => x.Sequence == transactionLevel.ToString());

                        if (sequence != null)
                        {
                            if (sequence.TransactionDate == null)
                            {
                                tempTransaction.TransactionDate = DateTime.Today;
                            }
                            else
                            {
                                tempTransaction.TransactionDate = sequence.TransactionDate;
                            }

                            tempTransaction.TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines2>();


                            foreach (var inAccountingString in groupedAvailableFund)
                            {
                                var tempAccountingString = new AccountFundsAvailable_TransactionstransactionDetailLines2()
                                {
                                    AccountingString = inAccountingString.AccountString
                                };

                                tempAccountingString.Type = inAccountingString.Amount < 0 ? Dtos.EnumProperties.CreditOrDebit.Credit : Dtos.EnumProperties.CreditOrDebit.Debit;

                                tempAccountingString.Amount = new Dtos.DtoProperties.AmountDtoProperty
                                {
                                    Value = Math.Abs(inAccountingString.Amount),
                                };

                                var currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                switch (inAccountingString.CurrencyCode)
                                {
                                    case "USD":
                                        currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        break;
                                    case "CAD":
                                        currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                                        break;
                                    default:
                                        break;
                                }
                                tempAccountingString.Amount.Currency = currency;

                                if (!string.IsNullOrEmpty(inAccountingString.SubmittedBy))
                                {
                                    tempAccountingString.SubmittedBy = new GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(inAccountingString.SubmittedBy));
                                }

                                var status = Dtos.EnumProperties.FundsAvailable.NotSet;
                                switch (inAccountingString.AvailableStatus)
                                {
                                    case FundsAvailableStatus.Availbale:
                                        status = Dtos.EnumProperties.FundsAvailable.Available; break;
                                    case FundsAvailableStatus.NotAvailable:
                                        status = Dtos.EnumProperties.FundsAvailable.NotAvailable; break;
                                    case FundsAvailableStatus.Override:
                                        status = Dtos.EnumProperties.FundsAvailable.OverrideAvailable; break;
                                    case FundsAvailableStatus.NotApplicable:
                                        status = Dtos.EnumProperties.FundsAvailable.NotApplicable;
                                        break;
                                    default: break;
                                }
                                tempAccountingString.FundsAvailable = status;
                                if (!string.IsNullOrEmpty(inAccountingString.ItemId))
                                {
                                    var referenceDoc = new Dtos.ReferenceDocumentDtoProperty();
                                    referenceDoc.ItemNumber = inAccountingString.ItemId;
                                    tempAccountingString.ReferenceDocument = referenceDoc;
                                }

                                tempTransaction.TransactionDetailLines.Add(tempAccountingString);

                            }

                            outTransaction.Transactions.Add(tempTransaction);
                        }
                        transactionLevel++;
                    }
                }
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                throw keyNotFoundException;
            }
            catch (RepositoryException repositoryException)
            {
                throw repositoryException;
            }
            catch (ArgumentNullException argumentNullException)
            {
                throw argumentNullException;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unexpected Error in AccountFundsAvailable Service");
                throw exception;
            }

            return outTransaction;
        }

        /// <summary>
        /// Check id user has view persmission
        /// </summary>
        private void CheckUserAccountFundsAvailableViewPermissions()
        {
            // access is ok if the current user has the view account funds available permission
            if (!HasPermission(ColleagueFinancePermissionCodes.ViewAccountFundsAvailable))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view account funds available.");
                throw new PermissionsException("User is not authorized to view account funds available.");
            }
        }        
    }
}
