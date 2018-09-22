/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// PayableDepositDirectiveRepository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayableDepositDirectiveRepository : BaseColleagueRepository, IPayableDepositDirectiveRepository
    {

        private readonly string colleagueTimeZone;
        private int index = 0;

        private const int NicknameLength = 50;
        private const int AccountIdLength = 17;

        /// <summary>
        /// Instatiate a new PayableDepositDirectiveRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PayableDepositDirectiveRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get a list of PayableDepositDirectives for the given payeeId
        /// </summary>
        /// <param name="payeeId">The Colleague Id of the payee, either a Person or an Organization, for whom to get PayableDepositDirectives</param>
        /// <param name="payableDepositDirectiveId">If populated, indicates the single payable deposit directive for which to get a PayableDepositDirective </param>
        /// <returns></returns>
        public async Task<PayableDepositDirectiveCollection> GetPayableDepositDirectivesAsync(string payeeId, string payableDepositDirectiveId = "")
        {
            if (string.IsNullOrEmpty(payeeId))
            {
                throw new ArgumentNullException("payeeId");
            }

            /// If payableDepositDirectiveId is populated, confirm that that single payable deposit directive exists
            if (!string.IsNullOrEmpty(payableDepositDirectiveId))
            {
                var onePersonAddrBankInfoRecord = await DataReader.ReadRecordAsync<PersonAddrBnkInfo>(payableDepositDirectiveId);

                if (onePersonAddrBankInfoRecord == null || onePersonAddrBankInfoRecord.PabiPersonId != payeeId)
                {
                    var message = string.Format("No PERSON_ADDR_BNK_INFO record found for PayeeId {0} with Id {1}.", payeeId, payableDepositDirectiveId);
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }
            }

            /// Get all payable deposit directives for the payeeId; they are all needed so that End Dates can be set
            var personAddrBankInfoRecords = new Collection<PersonAddrBnkInfo>();

            //PABI.PERSON is the index of PABI.PERSON.ID
            var criteria = string.Format("WITH PABI.PERSON EQ '{0}'", payeeId);

            personAddrBankInfoRecords = await DataReader.BulkReadRecordAsync<PersonAddrBnkInfo>("PERSON.ADDR.BNK.INFO", criteria);

            if (personAddrBankInfoRecords == null || !personAddrBankInfoRecords.Any())
            {
                logger.Warn("Payee Id {0} has no PERSON_ADDR_BNK_INFO records.", payeeId);
                return new PayableDepositDirectiveCollection(payeeId);
            }

            var payeePayableDepositDirectiveCollection = new PayableDepositDirectiveCollection(payeeId);
            foreach (var personAddrBankInfoRecord in personAddrBankInfoRecords)
            {
                try
                {
                    if (!string.IsNullOrEmpty(personAddrBankInfoRecord.PabiRoutingNo))
                    {
                        //Create US payable deposit directive
                        payeePayableDepositDirectiveCollection.Add(new PayableDepositDirective(
                            personAddrBankInfoRecord.Recordkey,
                            payeeId,
                            personAddrBankInfoRecord.PabiRoutingNo,
                            null,  // Bank Name should be determined by the consumer process
                            convertRecordColumnToBankAccountType(personAddrBankInfoRecord.PabiAcctType),
                            personAddrBankInfoRecord.PabiBankAcctNoLast4,
                            personAddrBankInfoRecord.PabiNickname,
                            !string.IsNullOrEmpty(personAddrBankInfoRecord.PabiPrenote) && personAddrBankInfoRecord.PabiPrenote.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                            personAddrBankInfoRecord.PabiAddressId,
                            personAddrBankInfoRecord.PabiEffDate.Value, // Start Date
                            null, // End Date is populated by the Add method
                            !string.IsNullOrEmpty(personAddrBankInfoRecord.PabiEcheckFlag) && personAddrBankInfoRecord.PabiEcheckFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                            new Timestamp(
                                personAddrBankInfoRecord.PersonAddrBnkInfoAddopr,
                                personAddrBankInfoRecord.PersonAddrBnkInfoAddtime.ToPointInTimeDateTimeOffset(personAddrBankInfoRecord.PersonAddrBnkInfoAdddate, colleagueTimeZone).Value,
                                personAddrBankInfoRecord.PersonAddrBnkInfoChgopr,
                                personAddrBankInfoRecord.PersonAddrBnkInfoChgtime.ToPointInTimeDateTimeOffset(personAddrBankInfoRecord.PersonAddrBnkInfoChgdate, colleagueTimeZone).Value
                            )
                        ));
                    }
                    else
                    {
                        //Create Canadian payable deposit directive
                        payeePayableDepositDirectiveCollection.Add(new PayableDepositDirective(
                            personAddrBankInfoRecord.Recordkey,
                            payeeId,
                            personAddrBankInfoRecord.PabiFinInstNo,
                            personAddrBankInfoRecord.PabiBrTransitNo,
                            null,  // Bank Name should be determined by the consumer process
                            convertRecordColumnToBankAccountType(personAddrBankInfoRecord.PabiAcctType),
                            personAddrBankInfoRecord.PabiBankAcctNoLast4,
                            personAddrBankInfoRecord.PabiNickname,
                            !string.IsNullOrEmpty(personAddrBankInfoRecord.PabiPrenote) && personAddrBankInfoRecord.PabiPrenote.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                            personAddrBankInfoRecord.PabiAddressId,
                            personAddrBankInfoRecord.PabiEffDate.Value, // Start Date
                            null, // End Date is populated by the Add method
                            !string.IsNullOrEmpty(personAddrBankInfoRecord.PabiEcheckFlag) && personAddrBankInfoRecord.PabiEcheckFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                            new Timestamp(
                                personAddrBankInfoRecord.PersonAddrBnkInfoAddopr,
                                personAddrBankInfoRecord.PersonAddrBnkInfoAddtime.ToPointInTimeDateTimeOffset(personAddrBankInfoRecord.PersonAddrBnkInfoAdddate, colleagueTimeZone).Value,
                                personAddrBankInfoRecord.PersonAddrBnkInfoChgopr,
                                personAddrBankInfoRecord.PersonAddrBnkInfoChgtime.ToPointInTimeDateTimeOffset(personAddrBankInfoRecord.PersonAddrBnkInfoChgdate, colleagueTimeZone).Value
                            )
                        ));
                    }

                }
                catch (Exception e)
                {
                    LogDataError("PERSON_ADDR_BNK_INFO", payeeId, personAddrBankInfoRecord, e, "Error creating the payable deposit directives list.");
                }
            }

            /// If all payableDepositDirectives for payee were requested, or a single payableDepositDirectiveId was 
            /// requested and the collection now includes one directive, then payeePayableDepositDirectiveCollection 
            /// contains the requested directive(s) and is returned as is
            if (string.IsNullOrEmpty(payableDepositDirectiveId) || payeePayableDepositDirectiveCollection.Count == 1)
            {
                return payeePayableDepositDirectiveCollection;
            }
            else
            {
                /// Select only the payable deposit directive for the payableDepositDirectiveId
                var payableDepositDirectiveCollectionOfOne = new PayableDepositDirectiveCollection(payeeId);
                var payableDepositDirective = payeePayableDepositDirectiveCollection
                    .FirstOrDefault(d => d.Id == payableDepositDirectiveId);

                if (payableDepositDirective == null)
                {
                    var message = string.Format("PayableDepositDirectiveId {0} not found in payeePayableDepositDirectiveCollection.", payableDepositDirectiveId);
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }

                payableDepositDirectiveCollectionOfOne.Add(payableDepositDirective);

                return payableDepositDirectiveCollectionOfOne;
            }
        }

        /// <summary>
        /// Helper to convert the record data to BankAccountType enum
        /// </summary>
        /// <param name="accountTypeCode"></param>
        /// <returns></returns>
        private BankAccountType convertRecordColumnToBankAccountType(string accountTypeCode)
        {
            if (string.IsNullOrEmpty(accountTypeCode))
            {
                throw new ArgumentNullException("accountTypeCode", "Cannot convert null or empty accountTypeCode");
            }

            switch (accountTypeCode.ToUpperInvariant())
            {
                case "S":
                    return BankAccountType.Savings;
                case "C":
                    return BankAccountType.Checking;
                default:
                    throw new ApplicationException("Unknown accountTypeCode " + accountTypeCode);
            }

        }

        /// <summary>
        /// Helper to convert the BankAccountType enum to record data
        /// </summary>
        /// <param name="bankAccountType"></param>
        /// <returns></returns>
        private string convertBankAccountTypeToRecordColumn(BankAccountType bankAccountType)
        {
            switch (bankAccountType)
            {
                case BankAccountType.Savings:
                    return "S";
                case BankAccountType.Checking:
                    return "C";
                default:
                    throw new ApplicationException("Unknown bankAccountType " + bankAccountType);
            }

        }


        /// <summary>
        /// Create a record that represents payable deposit directive information in the PersonAddrBnkInfo file  
        /// </summary>
        /// <param name="newPayableDepositDirective">The PayableDepositDirective object containing the new payable deposit data to add to the database</param>
        /// <returns>A PayableDepositDirective object that includes payable deposit info created from the new deposit data</returns>
        public async Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(PayableDepositDirective newPayableDepositDirective)
        {

            if (newPayableDepositDirective == null)
            {
                throw new ArgumentNullException("newPayableDepositDirective");
            }
            if (newPayableDepositDirective.NewAccountId == null)
            {
                throw new ArgumentNullException("newPayableDepositDirective.NewAccountId");
            }

            var request = new CreatePayableDepDirectiveRequest()
             {
                 PersonId = newPayableDepositDirective.PayeeId,
                 AddressId = newPayableDepositDirective.AddressId,
                 RoutingNumber = newPayableDepositDirective.RoutingId,
                 FinancialInstitutionNumber = newPayableDepositDirective.InstitutionId,
                 BranchTransitNumber = newPayableDepositDirective.BranchNumber,
                 BankAccountNumber = newPayableDepositDirective.NewAccountId,
                 EffectiveDate = newPayableDepositDirective.StartDate,
                 AccountType = convertBankAccountTypeToRecordColumn(newPayableDepositDirective.BankAccountType),
                 EcheckFlag = newPayableDepositDirective.IsElectronicPaymentRequested ? "Y" : "N",
                 Nickname = checkNickname(newPayableDepositDirective.Nickname),
                 Prenote = newPayableDepositDirective.IsVerified ? "Y" : "N"
             };

            var response = await transactionInvoker.ExecuteAsync<CreatePayableDepDirectiveRequest, CreatePayableDepDirectiveResponse>(request);

            //check response for errors
            if (response == null || (!string.IsNullOrEmpty(response.ErrorMessage)))
            {
                var message = string.Format("PERSON.ADDR.BANK.INFO record could not be created for payee {0} \n {1}", newPayableDepositDirective.PayeeId, response != null ? response.ErrorMessage : "Response error field unavailable");
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (response.OutPersonAddrBnkInfoId == null)
            {
                var message = string.Format("No Id returned for new PERSON.ADDR.BANK.INFO record for payee {0}", newPayableDepositDirective.PayeeId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            //call GetPayableDepositDirectivesAsync to get the payable deposit using the Id that was just created
            var verifyPayableDepositDirectiveListOfOne = await GetPayableDepositDirectivesAsync(newPayableDepositDirective.PayeeId, response.OutPersonAddrBnkInfoId);

            // verify that the returned PayableDepositDirective is not null or empty
            if (verifyPayableDepositDirectiveListOfOne == null || !verifyPayableDepositDirectiveListOfOne.Any())
            {
                throw new ApplicationException("GetPayableDepositDirectivesAsync did not return expected payable deposit directive");
            }

            //otherwise, return that PayableDepositAccount
            return verifyPayableDepositDirectiveListOfOne.First();
        }

        /// <summary>
        /// Update an existing record that represents payable deposit directive in the PersonAddrBnkInfo file  
        /// </summary>
        /// <param name="payableDepositDirectiveToUpdate"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(PayableDepositDirective payableDepositDirectiveToUpdate)
        {
            if (payableDepositDirectiveToUpdate == null)
            {
                throw new ArgumentNullException("payableDepositDirectiveToUpdate");
            }

            // verify that the payable deposit directive to update already exists
            var existingAddrBankInfoRecord = await DataReader.SelectAsync("PERSON.ADDR.BNK.INFO", new string[1] { payableDepositDirectiveToUpdate.Id }, "");
            if (!existingAddrBankInfoRecord.Any())
            {
                var message = string.Format("No PERSON_ADDR_BNK_INFO record found with Id {0}.", payableDepositDirectiveToUpdate.Id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // create the request to update the payable deposit...
            var request = new UpdatePayableDepDirectiveRequest()
            {
                PersonAddrBnkInfoId = payableDepositDirectiveToUpdate.Id,
                EcheckFlag = payableDepositDirectiveToUpdate.IsElectronicPaymentRequested ? "Y" : "N",
                Nickname = checkNickname(payableDepositDirectiveToUpdate.Nickname),
                AccountType = convertBankAccountTypeToRecordColumn(payableDepositDirectiveToUpdate.BankAccountType),
                BankAccountNumber = payableDepositDirectiveToUpdate.NewAccountId,
                BranchTransitNumber = payableDepositDirectiveToUpdate.BranchNumber,
                FinancialInstitutionNumber = payableDepositDirectiveToUpdate.InstitutionId,
                Prenote = payableDepositDirectiveToUpdate.IsVerified ? "Y" : "N",
                RoutingNumber = payableDepositDirectiveToUpdate.RoutingId,
                EffectiveDate = payableDepositDirectiveToUpdate.StartDate,

            };


            // get the response from request...
            var response = await transactionInvoker.ExecuteAsync<UpdatePayableDepDirectiveRequest, UpdatePayableDepDirectiveResponse>(request);

            //check response for errors
            if (response == null)
            {
                var message = string.Format("Could not update Payable Deposit for person {0}. Unexpected null response from CTX.", payableDepositDirectiveToUpdate.PayeeId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "PERSON_ADDR_BNK_INFO", payableDepositDirectiveToUpdate.Id);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }

            //call GetPayableDepositDirectivesAsync to get the payable deposit using the Id that was just created
            var verifyPayableDepositDirectiveListOfOne = await GetPayableDepositDirectivesAsync(payableDepositDirectiveToUpdate.PayeeId, payableDepositDirectiveToUpdate.Id);

            // verify that the returned PayableDepositDirective is not null or empty
            if (verifyPayableDepositDirectiveListOfOne == null || !verifyPayableDepositDirectiveListOfOne.Any())
            {
                throw new ApplicationException("GetPayableDepositDirectivesAsync did not return expected payable deposit directive");
            }

            //otherwise, return that PayableDepositAccount
            return verifyPayableDepositDirectiveListOfOne.First();
        }

        /// <summary>
        /// Delete a PayableDepositDirective with the given Id.
        /// </summary>
        /// <param name="payableDepositDirectiveIdToDelete"></param>
        /// <returns></returns>
        public async Task DeletePayableDepositDirectiveAsync(string payableDepositDirectiveIdToDelete)
        {
            if (string.IsNullOrEmpty(payableDepositDirectiveIdToDelete))
            {
                throw new ArgumentNullException("payableDepositDirectiveIdToDelete");
            }

            // verify that the payable deposit directive to delete already exists
            var existingAddrBankInfoRecordIds = await DataReader.SelectAsync("PERSON.ADDR.BNK.INFO", new string[1] { payableDepositDirectiveIdToDelete }, "");
            if (existingAddrBankInfoRecordIds == null || !existingAddrBankInfoRecordIds.Any())
            {
                var message = string.Format("No PERSON_ADDR_BNK_INFO record found with Id {0}.", payableDepositDirectiveIdToDelete);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // create the request to delete the payable deposit directive...
            var request = new DeletePayableDepDirectiveRequest()
            {
                PersonAddrBnkInfoId = payableDepositDirectiveIdToDelete,

            };


            // get the response from request...
            var response = await transactionInvoker.ExecuteAsync<DeletePayableDepDirectiveRequest, DeletePayableDepDirectiveResponse>(request);

            //check response for errors
            if (response == null)
            {
                var message = string.Format("Could not delete Payable Deposit Directive Id {0}. Unexpected null response from CTX.", payableDepositDirectiveIdToDelete);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "PERSON_ADDR_BNK_INFO", payableDepositDirectiveIdToDelete);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// Helper to verify the length of the nickname meets database requirements
        /// </summary>
        /// <param name="rawNickname"></param>
        /// <returns></returns>
        private string checkNickname(string rawNickname)
        {
            if (string.IsNullOrEmpty(rawNickname))
            {
                return string.Empty;
            }
            return rawNickname.Length > NicknameLength ? rawNickname.Substring(0, NicknameLength) : rawNickname;
        }

        /// <summary>
        /// Get an authentication token to create or update a payableDepositDirective
        /// </summary>
        /// <param name="payeeId"></param>
        /// <param name="payableDepositDirectiveId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string payeeId, string payableDepositDirectiveId, string accountId, string addressId)
        {
            if (string.IsNullOrWhiteSpace(payeeId))
            {
                throw new ArgumentNullException("payeeId");
            }

            var guid = Guid.NewGuid();
            var expiration = DateTimeOffset.Now.AddMinutes(10);

            var request = new AuthenticatePayableDepositDirectiveRequest()
            {
                PayeeId = payeeId,
                BankAccountId = accountId,
                PayableDepositDirectiveId = payableDepositDirectiveId,
                AddressId = addressId,
                Token = guid.ToString(),
                ExpirationDate = expiration.ToLocalDateTime(colleagueTimeZone),
                ExpirationTime = expiration.ToLocalDateTime(colleagueTimeZone)
            };

            var response = await transactionInvoker.ExecuteAsync<AuthenticatePayableDepositDirectiveRequest, AuthenticatePayableDepositDirectiveResponse>(request);

            if (response == null)
            {
                throw new BankingAuthenticationException(payableDepositDirectiveId, "Null response from transaction");
            }
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new BankingAuthenticationException(payableDepositDirectiveId, response.ErrorMessage);
            }
            if (!response.ExpirationDate.HasValue || !response.ExpirationTime.HasValue)
            {
                throw new BankingAuthenticationException(payableDepositDirectiveId, string.Format("Expiration Date and Time should be {0} but CTX returned null", expiration));
            }

            var officialExpiration = response.ExpirationTime.ToPointInTimeDateTimeOffset(response.ExpirationDate, colleagueTimeZone);
            if (!officialExpiration.HasValue)
            {
                throw new BankingAuthenticationException(payableDepositDirectiveId, string.Format("Unable to parse expiration date and time from CTX \n date: {0} \n time: {1}", response.ExpirationDate.Value, response.ExpirationTime.Value));
            }

            Guid officialToken;
            if (!Guid.TryParse(response.Token, out officialToken))
            {
                throw new BankingAuthenticationException(payableDepositDirectiveId, string.Format("Unable to parse token in CTX response - {0}", response.Token));
            }

            var authenticationToken = new BankingAuthenticationToken(officialExpiration.Value, officialToken);
            return authenticationToken;

        }
    }
}
