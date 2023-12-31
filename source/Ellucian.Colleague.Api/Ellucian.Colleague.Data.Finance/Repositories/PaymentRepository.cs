﻿// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class PaymentRepository : BaseColleagueRepository, IPaymentRepository
    {
        public const string Mnemonic = "SFPAY";
        private readonly string _colleagueTimeZone;

        public PaymentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        public PaymentConfirmation GetConfirmation(string Distribution, string PaymentMethod, string AmountToPay)
        {
            try
            {
                return ExecutePaymentConfirmationCTX(Distribution, PaymentMethod, AmountToPay);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        public PaymentProvider StartPaymentProvider(Payment paymentDetails)
        {
            return ExecuteStartPaymentCTX(paymentDetails);
        }

        public PaymentReceipt GetCashReceipt(string EcPayTransId, string CashRcptsId)
        {
            return ExecuteCashReceiptCTX(EcPayTransId, CashRcptsId);
        }

        public async Task<IEnumerable<AvailablePaymentMethod>> GetRestrictedPaymentMethodsAsync(string studentId, IEnumerable<AvailablePaymentMethod> allPaymentMethods)
        {
            return await ExecuteRestrictedPaymentCTXAsync(studentId, allPaymentMethods);
        }

        #region Private Methods

        private PaymentConfirmation ExecutePaymentConfirmationCTX(string Distribution, string PaymentMethod, string AmountToPay)
        {
            ReviewPaymentInfoRequest colleagueTxRequest = new ReviewPaymentInfoRequest();
            colleagueTxRequest.InDistribution = Distribution;
            colleagueTxRequest.InPaymentAmt = Decimal.Parse(AmountToPay);
            colleagueTxRequest.InPaymentMethod = PaymentMethod;

            try
            {
                ReviewPaymentInfoResponse colleagueTxResponse = transactionInvoker.Execute<ReviewPaymentInfoRequest, ReviewPaymentInfoResponse>(colleagueTxRequest);

                // Create the data model
                PaymentConfirmation confirmation = new PaymentConfirmation();

                // Populate the information from Colleague
                confirmation.ProviderAccount = colleagueTxResponse.OutProviderAcct;
                confirmation.ConvenienceFeeCode = colleagueTxResponse.OutConvFeeCode;
                confirmation.ConvenienceFeeDescription = colleagueTxResponse.OutConvFeeDesc;
                confirmation.ConvenienceFeeGeneralLedgerNumber = colleagueTxResponse.OutConvFeeGlNo;
                confirmation.ConvenienceFeeAmount = colleagueTxResponse.OutConvFeeAmt;
                confirmation.ConfirmationText = colleagueTxResponse.OutConfirmText;
                confirmation.ErrorMessage = colleagueTxResponse.OutErrorMsg;

                return confirmation;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private PaymentProvider ExecuteStartPaymentCTX(Payment paymentDetails)
        {
            PaymentProvider paymentProvider = new PaymentProvider();

            // Build outgoing Colleague Transaction
            StartStudentPaymentRequest colleagueTxRequest = new StartStudentPaymentRequest();
            colleagueTxRequest.InPersonId = paymentDetails.PersonId;
            colleagueTxRequest.InPayerId = string.IsNullOrEmpty(paymentDetails.PayerId) ? paymentDetails.PersonId : paymentDetails.PayerId;
            colleagueTxRequest.InMnemonic = Mnemonic;
            colleagueTxRequest.InReturnUrl = paymentDetails.ReturnUrl.ToString();
            colleagueTxRequest.InDistribution = paymentDetails.Distribution;
            colleagueTxRequest.InPayMethod = paymentDetails.PayMethod;
            colleagueTxRequest.InAmtToPay = paymentDetails.AmountToPay;
            colleagueTxRequest.InProviderAcct = paymentDetails.ProviderAccount;
            colleagueTxRequest.InConvFee = paymentDetails.ConvenienceFee;
            colleagueTxRequest.InConvFeeAmt = paymentDetails.ConvenienceFeeAmount;
            colleagueTxRequest.InConvFeeGlNo = paymentDetails.ConvenienceFeeGeneralLedgerNumber;
            colleagueTxRequest.InPayments = new List<InPayments>();

            foreach (PaymentItem paymentItem in paymentDetails.PaymentItems)
            {
                colleagueTxRequest.InPayments.Add(
                    new InPayments()
                    {
                        InPmtAmts = paymentItem.PaymentAmount,
                        InPmtDescs = paymentItem.Description,
                        InPmtArTypes = paymentItem.AccountType,
                        InPmtTerms = paymentItem.Term,
                        InPmtInvoices = paymentItem.InvoiceId,
                        InPmtPlans = paymentItem.PaymentPlanId,
                        InPmtOverdues = paymentItem.Overdue,
                        InPmtDepositsDue = paymentItem.DepositDueId
                    }
                );
            }

            try
            {
                StartStudentPaymentResponse colleagueTxResponse = transactionInvoker.Execute<StartStudentPaymentRequest, StartStudentPaymentResponse>(colleagueTxRequest);

                // Populate the redirect URL for successful transfer to a payment provider
                paymentProvider.RedirectUrl = colleagueTxResponse.OutStartUrl;

                // Capture processing errors
                paymentProvider.ErrorMessage = colleagueTxResponse.OutErrorMsg;

                return paymentProvider;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private PaymentReceipt ExecuteCashReceiptCTX(string EcPayTransId, string CashRcptsId)
        {
            PaymentReceipt cashReceipt = new PaymentReceipt();

            // Build outgoing Colleague Transaction
            GetCashReceiptAckInfoRequest colleagueTxRequest = new GetCashReceiptAckInfoRequest();

            // Build the input arguments to the transaction
            colleagueTxRequest.InEcPayTransId = EcPayTransId;
            colleagueTxRequest.IoCashRcptsId = CashRcptsId;

            try {
                GetCashReceiptAckInfoResponse colleagueTxResponse = transactionInvoker.Execute<GetCashReceiptAckInfoRequest, GetCashReceiptAckInfoResponse>(colleagueTxRequest);

                // Base receipt information
                cashReceipt.CashReceiptsId = colleagueTxResponse.IoCashRcptsId;
                cashReceipt.ReceiptNo = colleagueTxResponse.OutRcptNo;
                cashReceipt.ReceiptDate = colleagueTxResponse.OutRcptDate;
                cashReceipt.ReceiptTime = colleagueTxResponse.OutRcptTime;
                cashReceipt.ReceiptPayerId = colleagueTxResponse.OutRcptPayerId;
                cashReceipt.ReceiptPayerName = colleagueTxResponse.OutRcptPayerName;
                var combinedDateTime = new DateTime?();
                combinedDateTime = cashReceipt.ReceiptDate.Value.AddTicks(cashReceipt.ReceiptTime.Value.TimeOfDay.Ticks);
                cashReceipt.ReceiptDateTimeOffset = combinedDateTime.ToPointInTimeDateTimeOffset(combinedDateTime, _colleagueTimeZone).GetValueOrDefault();

                // Merchant information
                cashReceipt.MerchantNameAddress = colleagueTxResponse.OutMerchantNameAddr;
                cashReceipt.MerchantPhone = colleagueTxResponse.OutMerchantPhone;
                cashReceipt.MerchantEmail = colleagueTxResponse.OutMerchantEmail;

                // Acknowledgement text and footer info
                cashReceipt.ReceiptAcknowledgeText = colleagueTxResponse.OutRcptAckText;
                cashReceipt.AcknowledgeFooterImageUrl = string.IsNullOrEmpty(colleagueTxResponse.OutRcptFooterImage) ? null : new Uri(colleagueTxResponse.OutRcptFooterImage);
                cashReceipt.AcknowledgeFooterText = colleagueTxResponse.OutRcptFooterText;

                // Error message
                cashReceipt.ErrorMessage = colleagueTxResponse.OutErrorMsg;

                // Return URL
                cashReceipt.ReturnUrl = colleagueTxResponse.OutIpcReturnUrl;

                // AR Payments
                if (colleagueTxResponse.OutArPayments != null)
                {
                    foreach (OutArPayments payment in colleagueTxResponse.OutArPayments)
                    {
                        cashReceipt.Payments.Add(
                            new AccountsReceivablePayment()
                            {
                                PersonId = payment.OutArpPersonId,
                                PersonName = payment.OutArpPersonName,
                                Type = payment.OutArpArType,
                                Description = payment.OutArpArDesc,
                                Term = payment.OutArpTerm,
                                TermDescription = payment.OutArpTermDesc,
                                Location = payment.OutArpLocation,
                                LocationDescription = payment.OutArpLocationDesc,
                                PaymentDescription = payment.OutArpPmtDesc,
                                NetAmount = payment.OutArpNetAmt,
                                PaymentControlId = payment.OutIpcRegControlId
                            }
                        );
                    }
                }

                // AR Deposits
                if (colleagueTxResponse.OutArDeposits != null)
                {
                    foreach (OutArDeposits deposit in colleagueTxResponse.OutArDeposits)
                    {
                        cashReceipt.Deposits.Add(
                            new AccountsReceivableDeposit()
                            {
                                PersonId = deposit.OutArdPersonId,
                                PersonName = deposit.OutArdPersonName,
                                Type = deposit.OutArdDepType,
                                Description = deposit.OutArdDepDesc,
                                Term = deposit.OutArdTerm,
                                TermDescription = deposit.OutArdTermDesc,
                                Location = deposit.OutArdLocation,
                                LocationDescription = deposit.OutArdLocationDesc,
                                NetAmount = deposit.OutArdNetAmt
                            }
                        );
                    }
                }

                // Non-AR receipt items
                if (colleagueTxResponse.OutNonArItems != null)
                {
                    foreach (OutNonArItems payment in colleagueTxResponse.OutNonArItems)
                    {
                        cashReceipt.OtherItems.Add(
                            new GeneralPayment()
                            {
                                Code = payment.OutNonArCode,
                                Description = payment.OutNonArDesc,
                                Location = payment.OutNonArLocation,
                                LocationDescription = payment.OutNonArLocationDesc,
                                NetAmount = payment.OutNonArNetAmt
                            }
                         );
                    }
                }

                // Convenience fees
                if (colleagueTxResponse.OutConvenienceFees != null)
                {
                    foreach (OutConvenienceFees fee in colleagueTxResponse.OutConvenienceFees)
                    {
                        cashReceipt.ConvenienceFees.Add(
                            new ConvenienceFee()
                            {
                                Code = fee.OutConvFeeCode,
                                Description = fee.OutConvFeeDesc,
                                Amount = fee.OutConvFeeAmt
                            }
                         );
                    }
                }

                // Payments tendered
                if (colleagueTxResponse.OutPaymentMethods != null)
                {
                    foreach (OutPaymentMethods payment in colleagueTxResponse.OutPaymentMethods)
                    {
                        cashReceipt.PaymentMethods.Add(
                            new PaymentMethod()
                            {
                                PayMethodCode = payment.OutRcptPayMethods,
                                PayMethodDescription = payment.OutRcptPayMethodDescs,
                                ControlNumber = payment.OutRcptControlNos,
                                ConfirmationNumber = payment.OutRcptConfirmNos,
                                TransactionNumber = payment.OutRcptTransNos,
                                TransactionDescription = payment.OutRcptTransDescs,
                                TransactionAmount = payment.OutRcptTransAmts
                            }
                         );
                    }
                }

                // Change Returned
                cashReceipt.ChangeReturned = colleagueTxResponse.OutRcptChangeReturnedAmt.GetValueOrDefault();

                return cashReceipt;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private async Task<IEnumerable<AvailablePaymentMethod>> ExecuteRestrictedPaymentCTXAsync(string studentId, IEnumerable<AvailablePaymentMethod> allPaymentMethods)
        {

            var inPayMethods = new List<InPayMethods>();
            foreach (var paymethod in allPaymentMethods)
            {
                var inPayMethod = new InPayMethods();
                inPayMethod.InPmthCodes = paymethod.InternalCode;
                inPayMethod.InPmthDescs = paymethod.Description;
                inPayMethod.InPmthTypes = paymethod.Type;
                inPayMethods.Add(inPayMethod);
            }
            GetValidPayMethodsForPayerRequest request = new GetValidPayMethodsForPayerRequest();
            request.InPayerId = studentId;
            request.InPayMethods = inPayMethods;
            var colleagueTxResponse = transactionInvoker.ExecuteAsync<GetValidPayMethodsForPayerRequest, GetValidPayMethodsForPayerResponse>(request);

            var restrictedPaymentMethodList = new List<AvailablePaymentMethod>();

            foreach (var outPayMethod in colleagueTxResponse.Result.OutPayMethods)
            {
                var restrictedPaymentMethod = new AvailablePaymentMethod();
                restrictedPaymentMethod.InternalCode = outPayMethod.OutValidPmthCodes;
                restrictedPaymentMethod.Description = outPayMethod.OutValidPmthDescs;
                restrictedPaymentMethod.Type = outPayMethod.OutValidPmthTypes;
                restrictedPaymentMethodList.Add(restrictedPaymentMethod);
            }
            return restrictedPaymentMethodList;
        }

        #endregion
    }
}
