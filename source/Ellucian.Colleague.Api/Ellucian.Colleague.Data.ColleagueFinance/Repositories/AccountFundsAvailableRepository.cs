// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Dmi.Runtime;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IAccountingStringRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AccountFundsAvailableRepository : BaseColleagueRepository, IAccountFundsAvailableRepository
    {
        private readonly string _colleagueTimeZone;
        public static char _SM = Convert.ToChar(DynamicArray.SM);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AccountFundsAvailableRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }        

        /// <summary>
        /// Gets available funds
        /// </summary>
        /// <param name="accountingString"></param>
        /// <param name="amount"></param>
        /// <param name="projectNumber"></param>
        /// <param name="year"></param>
        /// <param name="submittedBy"></param>
        /// <returns></returns>
        public async Task<FundsAvailable> GetAvailableFundsAsync(string accountingString, decimal amount, string year)
        {
            if (string.IsNullOrEmpty(accountingString))
            {
                throw new ArgumentNullException("The accounting string must be specified to get available funds.");
            }

            if (amount == 0)
            {
                throw new ArgumentNullException("The amount must be specified to get available funds.");
            }

            var result = await DataReader.ReadRecordAsync<DataContracts.GlAccts>(accountingString);

            if (result == null)
            {
                throw new KeyNotFoundException("The accounting string specified is not valid.");
            }

            Domain.ColleagueFinance.Entities.FundsAvailable fundsAvailable = BuildGLFunds(result, amount, year);

            return fundsAvailable;
        }

        /// <summary>
        /// Check Available Funds
        /// </summary>
        /// <returns>The created FundsAvailable domain entity</returns>       
        public async Task<List<FundsAvailable>>  CheckAvailableFundsAsync(List<FundsAvailable> fundsAvailable, 
            string purchaseOrderId = "", string voucherId = "", string blanketPurchaseOrderNumber = "", string documentSubmittedBy = "", string requisitionId = "", List<string> bpoReqIds = null)
        {
            if (fundsAvailable == null || !fundsAvailable.Any())
            {
                throw new ArgumentNullException("The accounting string must be specified to get available funds.");
            }
            var fundsResponseList = new List<FundsAvailable>();
            
            // The AcctCheckAvailFunds parameter from GLAP is checked in the account funds
            // available subroutines used to verify if funds are available so we don't
            // need to identify this as non-applicable here in the API.  Instead, let the 
            // CTX determine the status to be returned.  The reason is that the GL account
            // validation/lookup needs to make sure that the oper ID has access based on
            // the submitted By values.  Otherwise, the submitted by isn't validated.
            // SRM - 01/05/2021
            //var glStruct = await GetGlstructAsync();
            //if (glStruct != null && glStruct.AcctCheckAvailFunds == "N")
            //{
            //    foreach (var fundAvailable in fundsAvailable)
            //    {
            //        var fundsResponse = new FundsAvailable(fundAvailable.AccountString);
            //        fundsResponse.TransactionDate = fundAvailable.TransactionDate;
            //        fundsResponse.AvailableStatus = FundsAvailableStatus.NotApplicable;
            //        fundsResponse.Sequence = fundAvailable.Sequence;
            //        fundsResponse.ItemId = fundAvailable.ItemId;
            //        fundsResponse.Amount = fundAvailable.Amount;
            //        fundsResponse.CurrencyCode = fundAvailable.CurrencyCode;
            //        fundsResponseList.Add(fundsResponse);
            //    }
            //    return fundsResponseList;
            //}

            var glAvailableList = new List<GlAvailableList>();
            foreach (var fundAvailable in fundsAvailable)
            {
                var glAvailable = new GlAvailableList();
                glAvailable.AccountingStrings = fundAvailable.AccountString;
                glAvailable.Currency = fundAvailable.CurrencyCode;
                glAvailable.GlAmts = fundAvailable.Amount;
                glAvailable.ItemsId = fundAvailable.ItemId;
                glAvailable.SubmittedBy = fundAvailable.SubmittedBy;
                glAvailable.TransactionDate = fundAvailable.TransactionDate.ToLocalDateTime(_colleagueTimeZone);
                glAvailable.RecordKey = fundAvailable.Sequence;
                glAvailableList.Add(glAvailable);
            }

            var createRequest = new CheckAvailableFundsRequest() { GlAvailableList = glAvailableList,
                DocSubmittedBy = documentSubmittedBy,
                BpoNo = blanketPurchaseOrderNumber,  
                PoId = purchaseOrderId,
                VouId = voucherId,
                ReqId = requisitionId,
                BpoReqIds = bpoReqIds ?? null
            };

            // write the data
            var createResponse = await transactionInvoker.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(createRequest);
            
            // Write informational messages to API log
            if (createResponse.InfoMessages != null && createResponse.InfoMessages.Any())
            {
                string messages = string.Empty;
                createResponse.InfoMessages.ForEach(m => messages = string.Concat(messages, m, "\r\n"));
                logger.Info(messages);
            }

            if (createResponse.ErrorMessages.Any())
            {
                var errorMessage = "Error(s) occurred checking funds availability ";
                var exception = new RepositoryException(errorMessage);
                createResponse.ErrorMessages.ForEach
                    (e => exception.AddError(new RepositoryError("Validation.Exception", e)));
                
                logger.Error(errorMessage);
                throw exception;
            }

            //The GL Class Definition (GLCD)form shows where to get the "GL Class" component of
            // a general ledger number.This is stored in record GL.CLASS.DEF in file 
            // ACCOUNT.PARAMETERS.Field 1(GL.CLASS.LOCATION) value 1 is the starting character 
            // of the GL class and field 1 value 2 is its length.Extract the GL class from the 
            // GL account being examined, and if the GL class is not found in the list 
            // GL.CLASS.EXPENSE.VALUES (field 6), then return notApplicable.
            var glClassConfiguration = await BuildGlClassConfiguration();
            var expenseClassValues = glClassConfiguration.ExpenseClassValues;
            var glClassLength = glClassConfiguration.GlClassLength;
            var glStartPosition = glClassConfiguration.GlClassStartPosition;
      
            foreach (var glAvailableResponse in createResponse.GlAvailableList)
            {
                var fundsResponse = new FundsAvailable(glAvailableResponse.AccountingStrings);

                var glClass = string.Empty;
                if (!string.IsNullOrEmpty(glAvailableResponse.AccountingStrings))
                {
                    try
                    {
                        // In Colleague, if the GL number is greater than 15 characters, then it gets stored with an underscore ("_")
                        // in place of the delimiter and therefore needs to have the delimiter included before we extract the GL class.
                        // If the length of the GL number is less than or equal to 15, then we strip out the delimiter completly before
                        // we extract the GL class.  This is because the GL structure setup has the starting position for the class
                        // based on delimiters when the number is 16 or greater in length.
                        var glNumberWithProject = glAvailableResponse.AccountingStrings.Split('*');
                        var glNumber = glNumberWithProject.Count() > 0 ? glNumberWithProject[0] : glAvailableResponse.AccountingStrings;
                        var unFormattedGlAccount = Regex.Replace(glNumber, "[^0-9a-zA-Z]", "");
                        if (unFormattedGlAccount.Length < 16)
                        {
                            glClass = unFormattedGlAccount.Substring(glStartPosition, glClassLength);
                        }
                        else
                        {
                            glClass = glAvailableResponse.AccountingStrings.Substring(glStartPosition, glClassLength);
                        }
                    }
                    catch(Exception ex)
                    {
                        //var exception = new RepositoryException(ex.Message);
                        //exception.AddError(new RepositoryError("Invalid.AccountingString", ex.Message));
                        logger.Error(ex, "FundsAvailable");
                        //throw exception;
                    }
                }

                if (expenseClassValues != null && expenseClassValues.Contains(glClass))
                {
                    if (!string.IsNullOrEmpty(glAvailableResponse.AvailableStatus))
                    {
                        fundsResponse.AvailableStatus = ConvertToFundsAvailableStatus(glAvailableResponse.AvailableStatus);
                    }
                }
                else
                {
                    fundsResponse.AvailableStatus = FundsAvailableStatus.NotApplicable;
                    if (glAvailableResponse.AvailableStatus == "INVALID.ACCOUNTING.STRING")
                    {
                        fundsResponse.AvailableStatus = FundsAvailableStatus.Invalid;
                    }
                }
                if (glAvailableResponse.GlAmts != null && glAvailableResponse.GlAmts.HasValue)
                {
                    fundsResponse.Amount = Convert.ToDecimal(glAvailableResponse.GlAmts);                    
                }
                fundsResponse.TransactionDate = glAvailableResponse.TransactionDate;
                fundsResponse.CurrencyCode = glAvailableResponse.Currency;
                fundsResponse.ItemId = glAvailableResponse.ItemsId;
                fundsResponse.Sequence = glAvailableResponse.RecordKey;
                fundsResponse.SubmittedBy = glAvailableResponse.SubmittedBy;
                fundsResponse.OverrideMessage = glAvailableResponse.OverrideMessage;
                fundsResponseList.Add(fundsResponse);
            }
            return fundsResponseList;          
        }

        /// <summary>
        /// Convert to FundsAvailableStatus
        /// </summary>
        /// <param name="availableStatus"></param>
        /// <returns>FundsAvailableStatus</returns>
        private FundsAvailableStatus ConvertToFundsAvailableStatus(string availableStatus)
        {
            if (string.IsNullOrEmpty(availableStatus))
            {
                throw new ArgumentNullException("availableStatus is a required field");
            }
            switch (availableStatus)
            {
                case "AVAILABLE":
                    return FundsAvailableStatus.Available;
                case "NOT.AVAILABLE":
                    return FundsAvailableStatus.NotAvailable;
                case "OVERRIDE":
                    return FundsAvailableStatus.Override;
                case "INVALID.ACCOUNTING.STRING":
                    return FundsAvailableStatus.Invalid;
                case "NOT.APPLICABLE":
                    return FundsAvailableStatus.NotApplicable;
                default:
                    throw new ArgumentException(string.Concat("Invalid FundsAvailableStatus: ", availableStatus));
            }
        }

        private async Task<GeneralLedgerClassConfiguration> BuildGlClassConfiguration()
        {
            var dataContract = await DataReader.ReadRecordAsync<Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
            if (dataContract == null)
            {
                throw new ConfigurationException("GL class definition is not defined.");
            }

            if (string.IsNullOrEmpty(dataContract.GlClassDict))
            {
                throw new ConfigurationException("GL class name is not defined.");
            }

            if (dataContract.GlClassExpenseValues == null)
            {
                throw new ConfigurationException("GL class expense values are not defined.");
            }

            if (dataContract.GlClassRevenueValues == null)
            {
                throw new ConfigurationException("GL class revenue values are not defined.");
            }

            if (dataContract.GlClassAssetValues == null)
            {
                throw new ConfigurationException("GL class asset values are not defined.");
            }

            if (dataContract.GlClassLiabilityValues == null)
            {
                throw new ConfigurationException("GL class liability values are not defined.");
            }

            if (dataContract.GlClassFundBalValues == null)
            {
                throw new ConfigurationException("GL class fund balance values are not defined.");
            }

            var glClassConfiguration = new GeneralLedgerClassConfiguration(dataContract.GlClassDict,
                dataContract.GlClassExpenseValues,
                dataContract.GlClassRevenueValues,
                dataContract.GlClassAssetValues,
                dataContract.GlClassLiabilityValues,
                dataContract.GlClassFundBalValues);

            var glStruct = await GetGlstructAsync();

            // Locate the GL Class name, dataContract.GlGlClassDict, in the list of subcomponents and get the start position and length.

            var subcomponentList = BuildSubcomponentList(glStruct);
            var subcomponentStartList = BuildSubcomponentStartList(glStruct);
            var subcomponentLengthList = BuildSubcomponentLengthtList(glStruct);

            var glClassPosition = subcomponentList.FindIndex(x => x.Equals(dataContract.GlClassDict));
            var glClassStartPosition = subcomponentStartList[glClassPosition];
            var glClassLength = subcomponentLengthList[glClassPosition];

            if (string.IsNullOrEmpty(glClassStartPosition))
                throw new ArgumentNullException("glClassStartPosition", "glClassStartPosition must have a value.");

            if (string.IsNullOrEmpty(glClassLength))
                throw new ArgumentNullException("glClassLength", "glClassLength must have a value.");

            int requestedStartPosition;
            if (Int32.TryParse(glClassStartPosition, out requestedStartPosition))
            {
                if ((requestedStartPosition - 1) < 0)
                {
                    throw new ApplicationException("The GL class subcomponent start position cannot be negative.");
                }
            }
            else
            {
                throw new ApplicationException("The GL class subcomponent start position is not an integer.");
            }

            int requestedLength;
            bool result = Int32.TryParse(glClassLength, out requestedLength);
            if (result)
            {
                if ((requestedLength - 1) < 0)
                {
                    throw new ApplicationException("The GL class subcomponent has an invalid length specified.");
                }
            }
            else
            {
                throw new ApplicationException("The GL class subcomponent length is not an integer.");
            }

            // Adjust the start position since C# index starts at zero.
            glClassConfiguration.GlClassStartPosition = requestedStartPosition - 1;
            glClassConfiguration.GlClassLength = requestedLength;

            return glClassConfiguration;
        }

        private List<string> BuildSubcomponentList(Glstruct glStruct)
        {
            // AcctSubName is a list of strings but each string contains each major component's subcomponents separated by subvalue marks.
            // Example glStruct.AcctSubName[0] contains "FUND.GROUP":@SV:"FUND"
            var subcomponentList = new List<string>();
            if (glStruct.AcctSubName != null && glStruct.AcctSubName.Any())
            {
                foreach (var subName in glStruct.AcctSubName)
                {
                    string[] subvalues = subName.Split(_SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentList.Add(sub);
                    }
                }
            }
            return subcomponentList;
        }
     
        // AcctSubStart is a list of strings but each string contains each subcomponent's start position separated by subvalue marks.
        // Example glStruct.AcctSubStart[0] contains "1":@SV:"3"
        private List<string> BuildSubcomponentStartList(Glstruct glStruct)
        {
            var subcomponentStartList = new List<string>();
            if (glStruct.AcctSubStart != null && glStruct.AcctSubStart.Any())
            {
                foreach (var subStart in glStruct.AcctSubStart)
                {
                    string[] subvalues = subStart.Split(_SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentStartList.Add(sub);
                    }
                }
            }
            return subcomponentStartList;
        }

        // AcctSubLgth is a list of strings but each string contains each subcomponent' length separated by subvalue marks.
        // Example glStruct.AcctSubLgth[0] contains "1":@SV:"2"
        private List<string> BuildSubcomponentLengthtList(Glstruct glStruct)
        {
            var subcomponentLengthList = new List<string>();
            if (glStruct.AcctSubLgth != null && glStruct.AcctSubLgth.Any())
            {
                foreach (var subLgth in glStruct.AcctSubLgth)
                {
                    string[] subvalues = subLgth.Split(_SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentLengthList.Add(sub);
                    }
                }
            }
            return subcomponentLengthList;
        }

        /// <summary>
        /// Obtain the ACCT.STRUCTURE record from Colleague.
        ///  This record contains General Ledger setup parameters.
        /// </summary>
        /// <returns>The Glstruct data record.</returns>
        private async Task<Glstruct> GetGlstructAsync()
        {
            // Get General Ledger parameters from the ACCT.STRUCTURE record in ACCOUNT.PARAMETERS.
            var glStruct = new Glstruct();

            glStruct = await DataReader.ReadRecordAsync<Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE");
            if (glStruct == null)
                // GLSTRUCT must exist for Colleague Financials to function properly
                throw new Exception("GL account structure is not defined.");

            return glStruct;
        }

        /// <summary>
        /// Gets available funds for a project
        /// </summary>
        /// <param name="accountingString"></param>
        /// <param name="amountValue"></param>
        /// <param name="projectNumber"></param>
        /// <param name="balanceOn"></param>
        /// <returns></returns>
        public async Task<FundsAvailable> GetProjectAvailableFundsAsync(string accountingString, decimal amountValue, string projectNumber, DateTime? balanceOn)
        {
            string criteriaProject = string.Empty; 
            string criteriaProjectLineItem = string.Empty;

            if (string.IsNullOrEmpty(accountingString))
            {
                throw new ArgumentNullException("The accounting string must be specified to get available funds.");
            }

            if (string.IsNullOrEmpty(projectNumber))
            {
                throw new ArgumentNullException("The project number must be specified to get available funds for a project.");
            }

            criteriaProject = string.Format("WITH PRJ.REF.NO EQ '{0}'", projectNumber);
            var resultProject = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(criteriaProject);
            if (resultProject == null || !resultProject.Any()) 
            {
                throw new KeyNotFoundException("The project specified is not valid.");
            }            

            criteriaProjectLineItem = string.Format("WITH PRJLN.PROJECTS.CF EQ '{0}' AND WITH PRJLN.GL.ACCTS EQ '{1}'", resultProject[0].Recordkey, accountingString);
            var resultProjectLineItem = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(criteriaProjectLineItem);
            if (resultProjectLineItem == null || !resultProjectLineItem.Any())
            {
                throw new KeyNotFoundException(string.Format("Project line item not found for project number: {0} and accounting string: {1}", projectNumber, accountingString));
            }

            Domain.ColleagueFinance.Entities.FundsAvailable fundsAvailable = await BuildProjectFunds(resultProjectLineItem.ElementAt(0), accountingString, projectNumber, amountValue, balanceOn);

            fundsAvailable.ProjectStatus = resultProject[0].PrjCurrentStatus;
            return fundsAvailable;
        }

        /// <summary>
        /// Build entity with project related fund info.
        /// </summary>
        /// <param name="projectsLineItems"></param>
        /// <param name="accountingString"></param>
        /// <param name="projectNumber"></param>
        /// <param name="amountValue"></param>
        /// <param name="balanceOn"></param>
        /// <returns></returns>
        private async Task<FundsAvailable> BuildProjectFunds(ProjectsLineItems projectsLineItems, string accountingString, string projectNumber, decimal amountValue, DateTime? balanceOn)
        {
            Domain.ColleagueFinance.Entities.FundsAvailable fundsAvailable = null;
            var result = await DataReader.ReadRecordAsync<DataContracts.ProjectsCf>(projectsLineItems.PrjlnProjectsCf);
            if (result == null)
            {
                throw new KeyNotFoundException("The project specified is not valid.");
            }

            fundsAvailable = new FundsAvailable(accountingString)
            {
                ProjectNumber = projectNumber,
                BalanceOn = balanceOn,
                ProjectBudgets = projectsLineItems.PrjlnBudgetAmts,
                ProjectActualMemos = projectsLineItems.PrjlnActualMemos,
                ProjectActualPosted = projectsLineItems.PrjlnActualPosted,
                ProjectEncumbranceMemos = projectsLineItems.PrjlnEncumbranceMemos,
                ProjectEncumbrancePosted = projectsLineItems.PrjlnEncumbrancePosted,
                ProjectRequisitionMemos = projectsLineItems.PrjlnRequisitionMemos,
                Amount = amountValue,
                ProjectStartDates = result.PrjcfPeriodStartDates,
                ProjectEndDates = result.PrjcfPeriodEndDates
            };            
            return fundsAvailable;
        }

        /// <summary>
        /// Gets funds vailable entity
        /// </summary>
        /// <param name="result"></param>
        /// <param name="amount"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private FundsAvailable BuildGLFunds(GlAccts result, decimal amount, string year)
        {
            //Get only the open period
            var memos = result.MemosEntityAssociation
                .FirstOrDefault(repo => repo.AvailFundsControllerAssocMember.Equals(year, StringComparison.OrdinalIgnoreCase) && 
                                        repo.GlFreezeFlagsAssocMember.ToUpper().Equals("O", StringComparison.OrdinalIgnoreCase));

            if (memos == null) 
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError(string.Concat("accounting string: ", result.Recordkey, " is not valid for the current fiscal year.")));
                throw exception;
            }            

            FundsAvailable fundsAvailable = new FundsAvailable(result.Recordkey) 
            {
                TotalBudget = GetTotalBudget(memos),
                TotalExpenses = GetTotalExpenses(amount, memos)                
            };
            
            return fundsAvailable;
        }

        /// <summary>
        /// Gets totoal budget amount
        /// </summary>
        /// <param name="memos"></param>
        /// <returns></returns>
        private decimal GetTotalBudget(GlAcctsMemos memos)
        {
            decimal totalBudget = 0;

            if (memos.GlBudgetPostedAssocMember.HasValue)
            {
                totalBudget = memos.GlBudgetPostedAssocMember.Value;
            }

            if (memos.GlBudgetMemosAssocMember.HasValue)
            {
                totalBudget += memos.GlBudgetMemosAssocMember.Value;
            }

            return totalBudget;
        }

        /// <summary>
        /// Gets total expenses
        /// </summary>
        /// <param name="amountRequested"></param>
        /// <param name="memos"></param>
        /// <returns></returns>
        private decimal GetTotalExpenses(decimal amountRequested, GlAcctsMemos memos)
        {
            decimal totalExpenses = 0;

            if (memos.GlActualPostedAssocMember.HasValue)
            {
                totalExpenses = memos.GlActualPostedAssocMember.Value;
            }

            if (memos.GlActualMemosAssocMember.HasValue) 
            {
                totalExpenses += memos.GlActualMemosAssocMember.Value;
            }

            if (memos.GlEncumbrancePostedAssocMember.HasValue) 
            {
                totalExpenses += memos.GlEncumbrancePostedAssocMember.Value;
            }

            if (memos.GlEncumbranceMemosAssocMember.HasValue) 
            {
                totalExpenses += memos.GlEncumbranceMemosAssocMember.Value;
            }

            if (memos.GlRequisitionMemosAssocMember.HasValue) 
            {
                totalExpenses += memos.GlRequisitionMemosAssocMember.Value;
            }
            totalExpenses = totalExpenses + amountRequested;

            return totalExpenses;
        }

        /// <summary>
        /// Get the person ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetPersonIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        /// <summary>
        /// Gets BPO Id
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public async Task<string> GetBpoAsync(string itemNumber)
        {
            if (string.IsNullOrEmpty(itemNumber))
            {
                return string.Empty;
            }

            var result = await DataReader.ReadRecordAsync<DataContracts.Items>(itemNumber);
            if (result == null)
            {
                throw new KeyNotFoundException(string.Format("No item found for item number: {0}.", itemNumber));
            }
            return result.ItmBpoId;
        }

        /// <summary>
        /// Gets PO Status
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public async Task<string> GetPOStatusByItemNumber(string itemNumber)
        {
            if (string.IsNullOrEmpty(itemNumber))
            {
                return string.Empty;
            }

            var result = await DataReader.ReadRecordAsync<DataContracts.Items>(itemNumber);
            if (result == null)
            {
                throw new KeyNotFoundException(string.Format("No item found for item number: {0}.", itemNumber));
            }

            if (!string.IsNullOrEmpty(result.ItmPoId)) 
            {
                string poCriteria = string.Format("WITH PURCHASE.ORDERS.ID EQ '{0}'", result.ItmPoId);
                var purchaseOrders = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", poCriteria);

                if (purchaseOrders == null || !purchaseOrders.Any())
                {
                    throw new KeyNotFoundException(string.Format("No purchase orders found for item number: {0}.", itemNumber));
                }
                if (purchaseOrders.FirstOrDefault().PoStatEntityAssociation != null & purchaseOrders.FirstOrDefault().PoStatEntityAssociation.Any())
                {
                    var poStatus = purchaseOrders.FirstOrDefault().PoStatEntityAssociation.FirstOrDefault();

                    return string.IsNullOrEmpty(poStatus.PoStatusAssocMember) ? string.Empty : poStatus.PoStatusAssocMember;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets requisition status
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public async Task<string> GetReqStatusByItemNumber(string itemNumber)
        {
            if (string.IsNullOrEmpty(itemNumber))
            {
                return string.Empty;
            }

            var result = await DataReader.ReadRecordAsync<DataContracts.Items>(itemNumber);
            if (result == null)
            {
                throw new KeyNotFoundException(string.Format("No item found for item number: {0}.", itemNumber));
            }

            // If we have a purchase order attached to the line item then we don't care what the status of the requisition is.
            // We only care about the status of the Purchase Order which has already been verified in the service method.
            if (!string.IsNullOrEmpty(result.ItmReqId) && string.IsNullOrEmpty(result.ItmPoId))
            {
                string reqCriteria = string.Format("WITH REQUISITIONS.ID EQ '{0}'", result.ItmReqId);
                var requisitions = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", reqCriteria);

                if (requisitions == null || !requisitions.Any())
                {
                    throw new KeyNotFoundException(string.Format("No requisitions found for item number: {0}.", itemNumber));
                }
                if (requisitions.FirstOrDefault().ReqStatusesEntityAssociation != null & requisitions.FirstOrDefault().ReqStatusesEntityAssociation.Any())
                {
                    var reqStatus = requisitions.FirstOrDefault().ReqStatusesEntityAssociation.FirstOrDefault();

                    return string.IsNullOrEmpty(reqStatus.ReqStatusAssocMember) ? string.Empty : reqStatus.ReqStatusAssocMember;
                }
            }
            return string.Empty;
        }
    }
}
