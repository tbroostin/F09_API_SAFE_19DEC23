/*Copyright 2017-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayrollDepositDirectivesRepository : BaseColleagueRepository, IPayrollDepositDirectivesRepository
    {
        private readonly string colleagueTimeZone;
        private readonly int readSize;
        private const int NicknameLength = 50;
        private const int AccountIdLength = 17;
        private const string BanksCacheKey = "PrDepositCodes";

        /// <summary>
        /// Instantiate PayrollDepositDirectivesRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PayrollDepositDirectivesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            readSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            colleagueTimeZone = settings.ColleagueTimeZone;
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Gets a list of payroll deposit directives for a single employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>A task awaiting a list of payroll deposit directives</returns>
        public async Task<PayrollDepositDirectiveCollection> GetPayrollDepositDirectivesAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                logger.Error("employeeId not provided as argument");
                throw new ArgumentNullException("employeeId");
            }

            //read Employes record
            var employeeRecord = await DataReader.ReadRecordAsync<Employes>(employeeId);

            //if no record, throw exception
            if (employeeRecord == null)
            {
                var message = string.Format("Id {0} is not an employee.", employeeId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            //If no direct deposits, return empty PayrollDepositDirective obj
            if (employeeRecord.DirDepEntityAssociation == null || !employeeRecord.DirDepEntityAssociation.Any())
            {
                logger.Error(string.Format("Employee {0} has no direct deposits", employeeId));
                return new PayrollDepositDirectiveCollection(employeeId);
            }

            //Read all payroll banks. throw exception if none exist
            var bankRecords = await GetBanksOrUpdateAndGetBanksFromCache();
            if (bankRecords == null || !bankRecords.Any())
            {
                var message = string.Format("Employee {0} has payroll deposits but no deposit bank codes exist.", employeeId);
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug(string.Format("Fetching Payroll deposit directive collection for employee id {0}", employeeId));
            var payrollDepositDirectives = new PayrollDepositDirectiveCollection(employeeId);

            foreach (var directDeposit in employeeRecord.DirDepEntityAssociation)
            {
                //find the bankRecord that matches the code in the association
                var bankRecord = bankRecords.FirstOrDefault(b => b.Recordkey == directDeposit.EmpDepositCodesAssocMember);
                if (bankRecord == null)
                {
                    LogDataError("PrDepositCodes", directDeposit.EmpDepositCodesAssocMember, new Object());
                    throw new ApplicationException(string.Format("PrDepositCode not found for code {0}", directDeposit.EmpDepositCodesAssocMember));
                }

                if (!string.IsNullOrWhiteSpace(bankRecord.DdcTransitNo)) // US Account
                {
                    logger.Debug(string.Format("US Account - employee id {0}", employeeId));
                    payrollDepositDirectives.Add(new PayrollDepositDirective(
                        directDeposit.EmpDepositIdAssocMember,
                        employeeRecord.Recordkey,
                        bankRecord.DdcTransitNo,
                        GetBankNameByDate(bankRecord, directDeposit.EmpDepositEndDatesAssocMember),
                        ConvertInternalCode(directDeposit.EmpDepositTypesAssocMember),
                        directDeposit.EmpDepAcctsLast4AssocMember,
                        directDeposit.EmpDepositNicknameAssocMember,
                        string.IsNullOrWhiteSpace(directDeposit.EmpDepositChangeFlagsAssocMember) || directDeposit.EmpDepositChangeFlagsAssocMember.Equals("P", StringComparison.InvariantCultureIgnoreCase),
                        directDeposit.EmpDepositPrioritiesAssocMember.Value,
                        directDeposit.EmpDepositAmountsAssocMember,
                        directDeposit.EmpDepositStartDatesAssocMember.Value,
                        directDeposit.EmpDepositEndDatesAssocMember,
                        new Timestamp(
                            directDeposit.EmpDepositAddoprAssocMember,
                            directDeposit.EmpDepositAddtimeAssocMember.ToPointInTimeDateTimeOffset(directDeposit.EmpDepositAdddateAssocMember, colleagueTimeZone).Value,
                            directDeposit.EmpDepositChgoprAssocMember,
                            directDeposit.EmpDepositChgtimeAssocMember.ToPointInTimeDateTimeOffset(directDeposit.EmpDepositChgdateAssocMember, colleagueTimeZone).Value
                        )
                    ));
                }
                else // Canadian Account
                {
                    logger.Debug(string.Format("Canadian Account - employee id {0}", employeeId));
                    payrollDepositDirectives.Add(new PayrollDepositDirective(
                        directDeposit.EmpDepositIdAssocMember,
                        employeeRecord.Recordkey,
                        bankRecord.DdcFinInstNumber,
                        bankRecord.DdcBrTransitNumber,
                        GetBankNameByDate(bankRecord, directDeposit.EmpDepositEndDatesAssocMember),
                        ConvertInternalCode(directDeposit.EmpDepositTypesAssocMember),
                        directDeposit.EmpDepAcctsLast4AssocMember,
                        directDeposit.EmpDepositNicknameAssocMember,
                        string.IsNullOrWhiteSpace(directDeposit.EmpDepositChangeFlagsAssocMember) || directDeposit.EmpDepositChangeFlagsAssocMember.Equals("P", StringComparison.InvariantCultureIgnoreCase),
                        directDeposit.EmpDepositPrioritiesAssocMember.Value,
                        directDeposit.EmpDepositAmountsAssocMember,
                        directDeposit.EmpDepositStartDatesAssocMember.Value,
                        directDeposit.EmpDepositEndDatesAssocMember,
                        new Timestamp(
                            directDeposit.EmpDepositAddoprAssocMember,
                            directDeposit.EmpDepositAddtimeAssocMember.ToPointInTimeDateTimeOffset(directDeposit.EmpDepositAdddateAssocMember, colleagueTimeZone).Value,
                            directDeposit.EmpDepositChgoprAssocMember,
                            directDeposit.EmpDepositChgtimeAssocMember.ToPointInTimeDateTimeOffset(directDeposit.EmpDepositChgdateAssocMember, colleagueTimeZone).Value
                        )
                    ));
                }
            }
            logger.Debug(string.Format("Successfully Fetched Payroll deposit directive collection for employee id {0}", employeeId));
            return payrollDepositDirectives;
        }

        /// <summary>
        /// Gets a single payroll deposit directive
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeeId"></param>
        /// <returns>a task awaiting a payroll deposit directive</returns>
        public async Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string id, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Debug("Payroll Deposit Directive Id is null or empty");
                throw new ArgumentNullException("id");
            }
            logger.Debug(string.Format("Fetching Payroll deposit directives for employee id {0}", employeeId));
            var directives = await GetPayrollDepositDirectivesAsync(employeeId);

            var directive = directives.FirstOrDefault(dir => dir.Id == id);

            if (directive == null)
            {
                var message = string.Format("Deposit record not found for Id {0}", id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            logger.Debug(string.Format("Successfully Fetched Payroll deposit directives for employee id {0}", employeeId));
            return directive;
        }

        /// <summary>
        /// Updates multiple payroll deposit directives
        /// </summary>
        /// <param name="payrollDepositDirectives"></param>
        /// <returns> a task awaiting a payrollDepositDirectiveCollection</returns>
        public async Task<PayrollDepositDirectiveCollection> UpdatePayrollDepositDirectivesAsync(PayrollDepositDirectiveCollection payrollDepositDirectives)
        {
            if (payrollDepositDirectives == null)
            {
                logger.Debug("Payroll Deposit Directive is null or empty");
                throw new ArgumentNullException("payrollDepositDirective");
            }
            logger.Debug("Get Banks or Update and get banks from cache");
            var bankRecords = await GetBanksOrUpdateAndGetBanksFromCache();


            var depositAccountsThatNeedBankRecords = payrollDepositDirectives.Where(directive => !BankCodeExists(bankRecords, directive));
            if (depositAccountsThatNeedBankRecords.Any())
            {
                logger.Debug("Creating bank codes for deposit accounts that need bank records");
                await CreateBankCodesAsync(depositAccountsThatNeedBankRecords);
                logger.Debug("Get Banks or Update and get banks from cache");
                bankRecords = await GetBanksOrUpdateAndGetBanksFromCache(true);
            }

            var depositsToUpdate = new List<PayrollDepositDirectives>();
            foreach (var directive in payrollDepositDirectives)
            {
                var deposit = new PayrollDepositDirectives()
                {
                    DepositIds = directive.Id,
                    DepositAccounts = directive.NewAccountId,
                    DepositAddDate = directive.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone).Date,
                    DepositAddOpr = directive.Timestamp.AddOperator,
                    DepositAddTime = directive.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone),
                    DepositAmounts = directive.DepositAmount,
                    DepositChangeFlags = directive.IsVerified ? "" : "Y",
                    DepositChgOpr = directive.Timestamp.ChangeOperator,
                    DepositCodes = GetBankCode(bankRecords, directive),
                    DepositEndDates = directive.EndDate.HasValue ? DateTime.SpecifyKind(directive.EndDate.Value, DateTimeKind.Unspecified) : directive.EndDate,
                    DepositNicknames = directive.Nickname,
                    DepositPriorites = directive.Priority,
                    DepositStartDates = DateTime.SpecifyKind(directive.StartDate, DateTimeKind.Unspecified),
                    DepositTypes = ConvertEnumToInternalCode(directive.BankAccountType)
                };
                depositsToUpdate.Add(deposit);
            }

            // create the request to update the direct deposits...
            logger.Debug("Create the request to update the Payroll deposits");
            var request = new UpdatePayrollDepositsRequest()
            {
                EmployeeId = payrollDepositDirectives.EmployeeId,
                PayrollDepositDirectives = depositsToUpdate
            };
            // get the response from request...
            var response = await transactionInvoker.ExecuteAsync<UpdatePayrollDepositsRequest, UpdatePayrollDepositsResponse>(request);

            if (response == null)
            {
                var message = "Could not update DirectDeposits. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "EMPLOYES", payrollDepositDirectives.EmployeeId);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }
            logger.Debug("Successfully updated Payroll deposit directives");
            return await GetPayrollDepositDirectivesAsync(payrollDepositDirectives.EmployeeId);
        }

        /// <summary>
        /// Creates a single payroll deposit directive
        /// </summary>
        /// <param name="payrollDepositDirective"></param>
        /// <returns>a task awaiting a single payroll deposit directive</returns>
        public async Task<PayrollDepositDirective> CreatePayrollDepositDirectiveAsync(PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null)
            {
                logger.Debug("Payroll deposit is null or empty");
                throw new ArgumentNullException("payrollDeposit");
            }
            var employeeId = payrollDepositDirective.PersonId;
            var bankRecords = await GetBanksOrUpdateAndGetBanksFromCache();
            String bankCode = null;

            if (!TryGetBankCode(bankRecords, payrollDepositDirective, out bankCode))
            {
                await CreateBankCodesAsync(new PayrollDepositDirectiveCollection(employeeId) { payrollDepositDirective });
                bankRecords = await GetBanksOrUpdateAndGetBanksFromCache(true);
            }

            // create the request to update the direct deposits...
            var request = new CreatePayrollDepositRequest()
            {
                EmployeeId = employeeId,
                DepositAccount = payrollDepositDirective.NewAccountId,
                DepositAmount = payrollDepositDirective.DepositAmount,
                DepositChangeFlag = payrollDepositDirective.IsVerified ? "" : "Y",
                DepositCode = GetBankCode(bankRecords, payrollDepositDirective),
                DepositStartDate = DateTime.SpecifyKind(payrollDepositDirective.StartDate, DateTimeKind.Unspecified),
                DepositEndDate = payrollDepositDirective.EndDate.HasValue ? DateTime.SpecifyKind(payrollDepositDirective.EndDate.Value, DateTimeKind.Unspecified) : payrollDepositDirective.EndDate,
                DepositNickname = payrollDepositDirective.Nickname,
                DepositPriority = payrollDepositDirective.Priority,
                DepositType = ConvertEnumToInternalCode(payrollDepositDirective.BankAccountType),
                AddOperator = employeeId
            };
            // get the response from request...
            logger.Debug("Create request to Create the Payroll deposits");
            var response = await transactionInvoker.ExecuteAsync<CreatePayrollDepositRequest, CreatePayrollDepositResponse>(request);

            if (response == null)
            {
                var message = "Could not update DirectDeposits. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "EMPLOYES", payrollDepositDirective.PersonId);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }

            var createdDirective = (await GetPayrollDepositDirectivesAsync(employeeId)).FirstOrDefault(dir => dir.Id == response.DepositId);
            if (createdDirective == null)
            {
                LogDataError("PayrollDepositDirective", response.DepositId, createdDirective);
                throw new ApplicationException("Created object not found in database");
            }
            logger.Debug("Successfully created Payroll deposit directives");
            return createdDirective;
        }

        /// <summary>
        /// Deletes a payroll deposit directive
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeeId"></param>
        /// <returns>A task awaiting a boolean indicative of deletion success</returns>
        public async Task<bool> DeletePayrollDepositDirectiveAsync(string id, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Debug("Payroll deposit Id is null or empty");
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                logger.Debug("Employee id is null or empty");
                throw new ArgumentNullException("employeeId");
            }
            var bankRecords = await GetBanksOrUpdateAndGetBanksFromCache();

            logger.Debug(string.Format("Fetching Payroll deposit to delete for employee Id {0}", employeeId));
            var directiveToDelete = (await GetPayrollDepositDirectivesAsync(employeeId)).FirstOrDefault(dir => dir.Id == id);
            
            if (directiveToDelete == null)
            {
                LogDataError("PayrollDepositDirective", id, directiveToDelete);
                throw new ApplicationException("object to delete not found in database");
            }
            if (directiveToDelete.PersonId != employeeId)
            {
                var message = string.Format("Person {0} Cannot delete payroll directives for person {1}", employeeId, directiveToDelete.PersonId);
                logger.Error(message);
            }
            logger.Debug(string.Format("Successfully fetched Payroll deposit to delete for employee Id {0}", employeeId));
            var directivesToDelete = new List<PayrollDepositDirectives2>();

            var deposit = new PayrollDepositDirectives2()
            {
                DepositIds = directiveToDelete.Id,
                DepositAccounts = directiveToDelete.NewAccountId,
                DepositAddDate = directiveToDelete.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone).Date,
                DepositAddOpr = directiveToDelete.Timestamp.AddOperator,
                DepositAddTime = directiveToDelete.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone),
                DepositAmounts = directiveToDelete.DepositAmount,
                DepositChangeFlags = directiveToDelete.IsVerified ? "" : "Y",
                DepositChgDate = directiveToDelete.Timestamp.ChangeDateTime.ToLocalDateTime(colleagueTimeZone).Date,
                DepositChgOpr = directiveToDelete.Timestamp.ChangeOperator,
                DepositChgTime = directiveToDelete.Timestamp.ChangeDateTime.ToLocalDateTime(colleagueTimeZone),
                DepositCodes = GetBankCode(bankRecords, directiveToDelete),
                DepositEndDates = directiveToDelete.EndDate.HasValue ? DateTime.SpecifyKind(directiveToDelete.EndDate.Value, DateTimeKind.Unspecified) : directiveToDelete.EndDate,
                DepositNicknames = directiveToDelete.Nickname,
                DepositPriorities = directiveToDelete.Priority,
                DepositStartDates = DateTime.SpecifyKind(directiveToDelete.StartDate, DateTimeKind.Unspecified),
                DepositTypes = ConvertEnumToInternalCode(directiveToDelete.BankAccountType)
            };
            directivesToDelete.Add(deposit);


            // create the request to update the direct deposits...
            logger.Debug("Create request to Delete the Payroll deposit");
            var request = new DeletePayrollDepositsRequest()
            {
                EmployeeId = employeeId,
                PayrollDepositDirectives2 = directivesToDelete
            };
            // get the response from request...
            var response = await transactionInvoker.ExecuteAsync<DeletePayrollDepositsRequest, DeletePayrollDepositsResponse>(request);

            if (response == null)
            {
                var message = "Could not delete Payroll Deposits. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "EMPLOYES", directiveToDelete.PersonId);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }
            else
            {
                logger.Debug(string.Format("Successfully Deleted the Payroll deposit for employee Id {0}", employeeId));
                return true;
            }
                
        }

        /// <summary>
        /// Deletes payroll deposit directives
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="employeeId"></param>
        /// <returns>A task awaiting a boolean indicative of deletion success</returns>
        public async Task<bool> DeletePayrollDepositDirectivesAsync(IEnumerable<string> ids, string employeeId)
        {
            if (!ids.Any())
            {
                logger.Debug("Payroll deposit Ids are null or empty");
                throw new ArgumentNullException("ids");
            }

            var bankRecords = await GetBanksOrUpdateAndGetBanksFromCache();
            logger.Debug(string.Format("Fetching Payroll deposits to delete for employee Id {0}", employeeId));
            var allEmployeepayrollDepositDirectives = await GetPayrollDepositDirectivesAsync(employeeId);

            if (allEmployeepayrollDepositDirectives == null)
            {
                var message = string.Format("No direct deposit directives exist for employee {0} to delete.", employeeId);
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug(string.Format("Successfully Fetched Payroll deposits to delete for employee Id {0}", employeeId));
            var directivesToDelete = new List<PayrollDepositDirectives2>();

            foreach (var payrollDepositDirective in allEmployeepayrollDepositDirectives)
            {
                if (payrollDepositDirective.PersonId != employeeId)
                {
                    var message = string.Format("Person {0} Cannot delete payroll directives for person {1}", employeeId, payrollDepositDirective.PersonId);
                    logger.Error(message);
                }
                else
                {
                    if (ids.Contains(payrollDepositDirective.Id))
                    {
                        var deposit = new PayrollDepositDirectives2()
                        {
                            DepositIds = payrollDepositDirective.Id,
                            DepositAccounts = payrollDepositDirective.NewAccountId,
                            DepositAddDate = payrollDepositDirective.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone).Date,
                            DepositAddOpr = payrollDepositDirective.Timestamp.AddOperator,
                            DepositAddTime = payrollDepositDirective.Timestamp.AddDateTime.ToLocalDateTime(colleagueTimeZone),
                            DepositAmounts = payrollDepositDirective.DepositAmount,
                            DepositChangeFlags = payrollDepositDirective.IsVerified ? "" : "Y",
                            DepositChgDate = payrollDepositDirective.Timestamp.ChangeDateTime.ToLocalDateTime(colleagueTimeZone).Date,
                            DepositChgOpr = payrollDepositDirective.Timestamp.ChangeOperator,
                            DepositChgTime = payrollDepositDirective.Timestamp.ChangeDateTime.ToLocalDateTime(colleagueTimeZone),
                            DepositCodes = GetBankCode(bankRecords, payrollDepositDirective),
                            DepositEndDates = payrollDepositDirective.EndDate.HasValue ? DateTime.SpecifyKind(payrollDepositDirective.EndDate.Value, DateTimeKind.Unspecified) : payrollDepositDirective.EndDate,
                            DepositNicknames = payrollDepositDirective.Nickname,
                            DepositPriorities = payrollDepositDirective.Priority,
                            DepositStartDates = DateTime.SpecifyKind(payrollDepositDirective.StartDate, DateTimeKind.Unspecified),
                            DepositTypes = ConvertEnumToInternalCode(payrollDepositDirective.BankAccountType)
                        };
                        directivesToDelete.Add(deposit);
                    }
                }
            }


            // create the request to update the direct deposits...
            logger.Debug("Create request to Delete the Payroll deposits");
            var request = new DeletePayrollDepositsRequest()
            {
                EmployeeId = employeeId,
                PayrollDepositDirectives2 = directivesToDelete
            };

            // get the response from request...
            var response = await transactionInvoker.ExecuteAsync<DeletePayrollDepositsRequest, DeletePayrollDepositsResponse>(request);

            if (response == null)
            {
                var message = "Could not delete Payroll Deposits. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "EMPLOYES", employeeId);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }
            else {
                logger.Debug(string.Format("Successfully Deleted the Payroll deposits for employee Id {0}", employeeId));
                return true;
            }
                
        }

        #region CLASS UTILITIES
        /// <summary>
        /// Convert code in db to BankAccountType enum
        /// </summary>
        /// <param name="internalCode"></param>
        /// <returns></returns>
        private BankAccountType ConvertInternalCode(string internalCode)
        {
            if (string.IsNullOrEmpty(internalCode))
            {
                throw new ArgumentNullException("internalCode");

            }

            switch (internalCode.ToUpperInvariant())
            {
                case "D":
                    return BankAccountType.Checking;
                case "S":
                    return BankAccountType.Savings;
                default: {
                        logger.Debug("Unknown bank account type internal code");
                        throw new ApplicationException("Unknown bank account type internal code " + internalCode);
                }
                    
            }
        }
        /// <summary>
        /// Convert BankAccountType enum to db code
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string ConvertEnumToInternalCode(BankAccountType type)
        {
            switch (type)
            {
                case BankAccountType.Checking:
                    return "D";
                case BankAccountType.Savings:
                    return "S";
                default:
                    {
                        logger.Debug("Unknown bank account type");
                        throw new ApplicationException("Unknown bank account type " + type);
                    }
                    
            }
        }
        /// <summary>
        /// Adds US routing information or CA institution information to the cache
        /// </summary>
        /// <returns></returns>
        private async Task<List<PrDepositCodes>> GetBanksOrUpdateAndGetBanksFromCache(bool updateCache = false)
        {
            try
            {
                if (updateCache)
                {
                    return await AddOrUpdateCacheAsync<List<PrDepositCodes>>(BanksCacheKey,
                        await ReadPrDepositCodes(),
                        CacheTimeout
                    );
                }

                return await GetOrAddToCacheAsync<List<PrDepositCodes>>(BanksCacheKey,
                    async () => await ReadPrDepositCodes(),
                    CacheTimeout);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error interacting with PrDepositCodes cache");
            }
            return await ReadPrDepositCodes();
        }

        private async Task<List<PrDepositCodes>> ReadPrDepositCodes()
        {
            var prDepositCodes = new List<PrDepositCodes>();
            var keys = await DataReader.SelectAsync("PR.DEPOSIT.CODES", "");
            for (int i = 0; i < keys.Count(); i += readSize)
            {
                var subList = keys.Skip(i).Take(readSize).ToArray();
                var prDepositCodeSet = await DataReader.BulkReadRecordAsync<PrDepositCodes>(subList);
                if (prDepositCodeSet == null)
                {
                    var invalidKeys = string.Empty;
                    foreach (var key in subList) { invalidKeys = string.Format("{0} {1}", invalidKeys, key); }
                    LogDataError("PrDepositCodes", invalidKeys, null);
                    throw new ApplicationException("No PRDepositCodes returned for selected keys");
                }

                prDepositCodes.AddRange(prDepositCodeSet);
            }
            return prDepositCodes;
        }

        /// <summary>
        /// Get the recordKey of the PrDepositCode object that has the same bank identifiers as the given
        /// bank account
        /// </summary>
        /// <param name="bankRecords"></param>
        /// <param name="directive"></param>
        /// <returns></returns>
        private string GetBankCode(IEnumerable<PrDepositCodes> bankRecords, PayrollDepositDirective directive)
        {
            if (bankRecords == null)
            {
                logger.Debug("Bank records cannot be null or empty");
                throw new ArgumentNullException("bankRecords");
            }
            if (directive == null)
            {
                logger.Debug("Payroll deposit directive cannot be null or empty");
                throw new ArgumentNullException("directive");
            }

            var matchingBank = bankRecords
                .Where(bank => !bank.DdcIsArchived.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                .First(bank =>
                    AreEqual(bank.DdcTransitNo, directive.RoutingId) &&
                    AreEqual(bank.DdcFinInstNumber, directive.InstitutionId) &&
                    AreEqual(bank.DdcBrTransitNumber, directive.BranchNumber));


            return matchingBank.Recordkey;
        }

        private bool TryGetBankCode(IEnumerable<PrDepositCodes> bankRecords, PayrollDepositDirective directive, out string bankCode)
        {
            try
            {
                bankCode = GetBankCode(bankRecords, directive);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, string.Format("Unable to find bank record for bank code for directive {0}", directive.Id));
            }
            bankCode = null;
            return false;
        }

        private bool BankCodeExists(IEnumerable<PrDepositCodes> bankRecords, PayrollDepositDirective directive)
        {
            string outCode = null;
            var exists = TryGetBankCode(bankRecords, directive, out outCode);
            return exists;

        }

        private string GetBankNameByDate(PrDepositCodes bankRecord, DateTime? directiveEndDate)
        {
            if (bankRecord == null)
            {
                logger.Debug("Bank records cannot be null or empty");
                throw new ArgumentNullException("bankRecord");
            }


            if (!directiveEndDate.HasValue)
            {
                return bankRecord.DdcDescription;
            }

            return bankRecord.DdcPriorDescsEntityAssociation
                .OrderByDescending(ddc => ddc.DdcPriorDescEndDatesAssocMember)
                .Aggregate<PrDepositCodesDdcPriorDescs, string>(bankRecord.DdcDescription,
                    (result, record) => (directiveEndDate.Value <= record.DdcPriorDescEndDatesAssocMember) ?
                        record.DdcPriorDescriptionsAssocMember :
                        result);

        }

        private async Task<ICollection<PrDepositCodes>> CreateBankCodesAsync(PayrollDepositDirectiveCollection bankAccounts)
        {
            if (bankAccounts == null)
            {
                logger.Debug("Bank Accounts cannot be null or empty");
                throw new ArgumentNullException("bankAccounts");
            }

            var addedBankRecords = new List<PrDepositCodes>();

            var request = new CreatePrDepositCodeRequest()
            {
                NewBank = bankAccounts
                    .Select(acct => new NewBank()
                    {
                        Id = "",
                        Name = acct.BankName,
                        RoutingId = acct.RoutingId,
                        InstitutionId = acct.InstitutionId,
                        BranchId = acct.BranchNumber
                    }).ToList()
            };
            logger.Debug("Create request to Create bank codes");
            var response = await transactionInvoker.ExecuteAsync<CreatePrDepositCodeRequest, CreatePrDepositCodeResponse>(request);

            if (response == null)
            {
                var message = string.Format("Null CTX response batch creating new PR_DEPOSIT_CODEs");
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = string.Format("Error batch creating new PR_DEPOSIT_CODEs error: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            addedBankRecords = response.NewBank
                .Select(newBank => new PrDepositCodes()
                {
                    Recordkey = newBank.Id,
                    DdcDescription = newBank.Name,
                    DdcTransitNo = newBank.RoutingId,
                    DdcFinInstNumber = newBank.InstitutionId,
                    DdcBrTransitNumber = newBank.BranchId
                }).ToList();
            logger.Debug("Successfully added bank codes.");
            return addedBankRecords;
        }

        /// <summary>
        /// Helper compares two strings, considering null/empty to be the same thing
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool AreEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b);
            }
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion


        public async Task<BankingAuthenticationToken> AuthenticatePayrollDepositDirective(string employeeId, string depositDirectiveId, string accountId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                logger.Debug("Employee id cannot be null or empty");
                throw new ArgumentNullException("employeeId");
            }

            var guid = Guid.NewGuid();
            var expiration = DateTimeOffset.Now.AddMinutes(10);

            var request = new AuthenticatePayrollDepositDirectiveRequest()
            {
                EmployeeId = employeeId,
                PayrollDepositDirectiveId = depositDirectiveId,
                BankAccountId = accountId,
                ExpirationDate = expiration.ToLocalDateTime(colleagueTimeZone),
                ExpirationTime = expiration.ToLocalDateTime(colleagueTimeZone),
                Token = guid.ToString()
            };


            logger.Debug("Create request to Authenticate Payroll Deposit Directive");
            //invoke ctx
            var response = await transactionInvoker.ExecuteAsync<AuthenticatePayrollDepositDirectiveRequest, AuthenticatePayrollDepositDirectiveResponse>(request);

            if (response == null)
            {
                logger.Debug("Null response from transaction");
                throw new BankingAuthenticationException(depositDirectiveId, "Null response from transaction");
            }
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new BankingAuthenticationException(depositDirectiveId, response.ErrorMessage);
            }
            if (!response.ExpirationDate.HasValue || !response.ExpirationTime.HasValue)
            {
                logger.Debug(string.Format("Expiration Date and Time should be {0} but CTX returned null", expiration));
                throw new BankingAuthenticationException(depositDirectiveId, string.Format("Expiration Date and Time should be {0} but CTX returned null", expiration));
            }

            var officialExpiration = response.ExpirationTime.ToPointInTimeDateTimeOffset(response.ExpirationDate, colleagueTimeZone);
            if (!officialExpiration.HasValue)
            {
                logger.Debug(string.Format("Unable to parse expiration date and time from CTX \n date: {0} \n time: {1}", response.ExpirationDate.Value, response.ExpirationTime.Value));
                throw new BankingAuthenticationException(depositDirectiveId, string.Format("Unable to parse expiration date and time from CTX \n date: {0} \n time: {1}", response.ExpirationDate.Value, response.ExpirationTime.Value));
            }

            Guid officialToken;
            if (!Guid.TryParse(response.Token, out officialToken))
            {
                logger.Debug(string.Format("Unable to parse token in CTX response - {0}", response.Token));
                throw new BankingAuthenticationException(depositDirectiveId, string.Format("Unable to parse token in CTX response - {0}", response.Token));
            }


            var authenticationToken = new BankingAuthenticationToken(officialExpiration.Value, officialToken);
            return authenticationToken;

        }

    }
}
