// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    /// <summary>
    /// Repository for Receipts
    /// </summary>
    [RegisterType]
    public class ReceiptRepository : BaseColleagueRepository, IReceiptRepository
    {
        private string _colleagueTimeZone;

        /// <summary>
        /// Constructor for receipts repository
        /// </summary>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public ReceiptRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        #region Create Receipt

        /// <summary>
        /// Create a receipt
        /// </summary>
        /// <param name="sourceReceipt">The receipt to create</param>
        /// <param name="sourcePayments">The payments to create along with this receipt.  This parameter is currently ignored.</param>
        /// <param name="sourceDeposits">The deposits to create along with this receipt.  This parameter must have a value.</param>
        /// <returns>The created receipt</returns>
        public Receipt CreateReceipt(Receipt sourceReceipt, IEnumerable<ReceiptPayment> sourcePayments, IEnumerable<Deposit> sourceDeposits)
        {
            if (sourceReceipt == null)
            {
                throw new ArgumentNullException("sourceReceipt", "The receipt to create must be specified.");
            }
            if ((sourcePayments == null || !sourcePayments.Any()) && (sourceDeposits == null || !sourceDeposits.Any()))
            {
                throw new ArgumentException("The payments or deposits to create must be specified.");
            }

            // build list of deposits to create
            var deposits = (sourceDeposits.Select(deposit => new DepositInformation
            {
                DepositHolderId = deposit.PersonId,
                DepositAmount = deposit.Amount,
                DepositTypeCode = deposit.DepositType,
                DepositTermCode = deposit.TermId
            })).ToList();

            // build list of non-cash payments
            var payments = sourceReceipt.NonCashPayments.Select(
                    x => new NonCashInformation { NonCashPayMethodCode = x.PaymentMethodCode, NonCashAmount = x.Amount }).ToList();

            // build receipt
            var request = new CreateCashReceiptRequest()
            {
                PayerId = sourceReceipt.PayerId,
                PayerName = sourceReceipt.PayerName,
                WebPaymentInd = false,
                Distribution = sourceReceipt.DistributionCode,
                ReceiptDate = sourceReceipt.Date,
                CashierId = sourceReceipt.CashierId,
                Mnemonic = sourceReceipt.ExternalSystem,
                ExternalSystemCode = sourceReceipt.ExternalSystem,
                ExternalIdentifier = sourceReceipt.ExternalIdentifier,
                NonCashInformation = payments,
                DepositInformation = deposits
            };

            CreateCashReceiptResponse response;
            try
            {
                response = transactionInvoker.Execute<CreateCashReceiptRequest, CreateCashReceiptResponse>(request);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw ex;
            }

            if (response.Messages != null && response.Messages.Count > 0)
            {
                // Update failed
                logger.Error(response.Messages.ToString());
                throw new InvalidOperationException(String.Join("\n", response.Messages));
            }

            return GetReceipt(response.CashReceiptId);
        }

        #endregion

        #region GetReceipt

        /// <summary>
        /// Retrieve a receipt
        /// </summary>
        /// <param name="id">The id of the receipt to retrieve</param>
        /// <returns>The indicated receipt</returns>
        public Receipt GetReceipt(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Receipt ID must be specified.");
            }

            CashRcpts cashRcpt = DataReader.ReadRecord<CashRcpts>(id);
            if (cashRcpt == null)
            {
                throw new KeyNotFoundException("Receipt ID " + id + " is not valid.");
            }

            var nonCashPayments = cashRcpt.RcptNonCashEntityAssociation.Select(ncp =>
                new NonCashPayment(ncp.RcptPayMethodsAssocMember,
                    ncp.RcptNonCashAmtsAssocMember.GetValueOrDefault() - ncp.RcptNonCashReversalAmtsAssocMember.GetValueOrDefault())).ToList();

            var receipt = new Receipt(cashRcpt.Recordkey, cashRcpt.RcptNo, cashRcpt.RcptDate.GetValueOrDefault(), cashRcpt.RcptPayerId,
                cashRcpt.RcptTenderGlDistrCode, cashRcpt.RcptDeposits, nonCashPayments);

            // add external info if provided
            if (!string.IsNullOrEmpty(cashRcpt.RcptExternalSystem) && !string.IsNullOrEmpty(cashRcpt.RcptExternalId))
            {
                receipt.AddExternalSystemAndId(cashRcpt.RcptExternalSystem, cashRcpt.RcptExternalId);
            }
            receipt.PayerName = cashRcpt.RcptPayerDesc;
            // get the cashier from the session
            var session = GetReceiptSession(cashRcpt.RcptSession);
            receipt.CashierId = session.CashierId;

            return receipt;
        }

        #endregion

        #region GetCashier

        /// <summary>
        /// Retrieve a cashier
        /// </summary>
        /// <param name="id">The ID of the cashier to retrieve</param>
        /// <returns>The indicated cashier</returns>
        public Cashier GetCashier(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Cashier ID must be specified.");
            }

            Cashiers cashier = DataReader.ReadRecord<Cashiers>(id);
            if (cashier == null)
            {
                throw new KeyNotFoundException("Cashier id " + id + " is not valid.");
            }

            //bool isECommEnabled = (string.IsNullOrEmpty(cashier.CshrEcommerceFlag) ? false : (cashier.CshrEcommerceFlag == "Y"));
            return new Cashier(cashier.Recordkey, GetPersonLogin(id), cashier.CshrEcommerceFlag == "Y")
            {
                CreditCardLimitAmount = cashier.CshrCrCardAmt,
                CheckLimitAmount = cashier.CshrCheckAmt
            };
        }

        // This method is just temporary until the Platform team gives us a method that returns this data
        private string GetPersonLogin(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be null.");
            }

            string criteria = string.Format("WITH SYS.PERSON.ID EQ '{0}'", id);
            var foo = DataReader.Select("UT.OPERS", criteria);
            if (foo == null || foo.Length == 0)
            {
                throw new ArgumentOutOfRangeException("id", "Person ID " + id + " does not have a valid OPERS record.");
            }

            return foo.First();
        }

        #endregion

        #region GetReceiptSession

        /// <summary>
        /// Retrieve a session
        /// </summary>
        /// <param name="id">The ID of the session to retrieve</param>
        /// <returns>The indicated session</returns>
        private ReceiptSession GetReceiptSession(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Receipt session id must be specified.");
            }

            var session = DataReader.ReadRecord<RcptSessions>(id);
            if (session == null)
            {
                throw new KeyNotFoundException("Session id " + id + " is not valid.");
            }

            DateTimeOffset startDateTime = session.RcptsStartTime.ToPointInTimeDateTimeOffset(session.RcptSessionsAdddate, _colleagueTimeZone).Value;
            DateTimeOffset? endDateTime = session.RcptsEndTime.ToPointInTimeDateTimeOffset(session.RcptsEndDate, _colleagueTimeZone);

            return new ReceiptSession(session.Recordkey, ConvertSessionStatusCodeToSessionStatus(session.RcptsStatus), session.RcptSessionsAddopr,
                                session.RcptsDate.GetValueOrDefault(), startDateTime, session.RcptsEcommerceFlag == "Y", session.RcptsLocation) { End = endDateTime };

        }

        #endregion

        public static SessionStatus ConvertSessionStatusCodeToSessionStatus(string status)
        {
            switch (status)
            {
                case "O":
                    return SessionStatus.Open;
                case "C":
                    return SessionStatus.Closed;
                case "R":
                    return SessionStatus.Reconciled;
                case "V":
                    return SessionStatus.Voided;
                default:
                    throw new ArgumentOutOfRangeException("status", "Invalid receipt session status code: " + status ?? "null");
            }
        }
    }
}
