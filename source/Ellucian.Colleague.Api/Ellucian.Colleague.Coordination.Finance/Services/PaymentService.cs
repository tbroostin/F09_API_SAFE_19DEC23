// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using Ellucian.Data.Colleague.Exceptions;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class PaymentService : FinanceCoordinationService, IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAccountDueRepository _accountRepository;

        public PaymentService(IAdapterRegistry adapterRegistry, IPaymentRepository paymentRepository, IAccountDueRepository accountRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _paymentRepository = paymentRepository;
            _accountRepository = accountRepository;
        }

        public Ellucian.Colleague.Dtos.Finance.Payments.PaymentConfirmation GetPaymentConfirmation(string distribution, string paymentMethod, string amountToPay)
        {
            try
            {
                var paymentConfirmationEntity = _paymentRepository.GetConfirmation(distribution, paymentMethod, amountToPay);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentConfirmation, Ellucian.Colleague.Dtos.Finance.Payments.PaymentConfirmation>();

                var paymentConfirmationDto = adapter.MapToType(paymentConfirmationEntity);

                return paymentConfirmationDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        private void CheckPayOwnAccountPermission(string personId)
        {
            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                logger.Error(CurrentUser.PersonId + " attempted to pay on account " + personId);
                throw new PermissionsException();
            }
        }

        public Ellucian.Colleague.Dtos.Finance.Payments.PaymentProvider PostPaymentProvider(Ellucian.Colleague.Dtos.Finance.Payments.Payment paymentDetails)
        {
            CheckPayOwnAccountPermission(paymentDetails.PersonId);
            var paymentAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Finance.Payments.Payment, Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment>();

            var paymentEntity = paymentAdapter.MapToType(paymentDetails);

            var paymentConfirmationEntity = _paymentRepository.StartPaymentProvider(paymentEntity);

            var paymentProviderAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentProvider, Ellucian.Colleague.Dtos.Finance.Payments.PaymentProvider>();

            var paymentProviderDto = paymentProviderAdapter.MapToType(paymentConfirmationEntity);

            return paymentProviderDto;
        }

        /// <summary>
        /// Retrieves payment receipt information for specified transaction id or 
        /// cash receipt id
        /// </summary>
        /// <param name="transactionId">transaction id of the payment to retrieve</param>
        /// <param name="cashReceiptId">cash receipt id of the payment to retrieve</param>
        /// <returns>Payment receipt DTO</returns>
        public Ellucian.Colleague.Dtos.Finance.Payments.PaymentReceipt GetPaymentReceipt(string transactionId, string cashReceiptId)
        {
            var paymentReceiptEntity = _paymentRepository.GetCashReceipt(transactionId, cashReceiptId);

            //Check whether the current user has access to the receipt information
            if(paymentReceiptEntity != null)
            {
                //Get personId(account holder id) if this is an ar or deposit payment, get payerId if it is a non-AR payment
                string personId = paymentReceiptEntity.Payments.Any() ? paymentReceiptEntity.Payments.First().PersonId :
                    paymentReceiptEntity.Deposits.Any() ? paymentReceiptEntity.Deposits.First().PersonId :
                    paymentReceiptEntity.OtherItems.Any() ? paymentReceiptEntity.ReceiptPayerId : null;

                if (!string.IsNullOrEmpty(personId))
                {
                    CheckAccountPermission(personId);
                }
                //else
                //{
                //    logger.Info("Could not retrieve the receipt with the specified information");
                //    throw new InvalidOperationException();
                //}             
                //F09
                else if(!System.Text.RegularExpressions.Regex.IsMatch(paymentReceiptEntity.ErrorMessage ??"", @"(?:^[Ff]09|Updated.*payment.*source)",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    logger.Info("Could not retrieve the receipt with the specified information");
                    throw new InvalidOperationException();
                }             
                //end F09
            }
            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentReceipt, Ellucian.Colleague.Dtos.Finance.Payments.PaymentReceipt>();

            var paymentReceiptDto = adapter.MapToType(paymentReceiptEntity);

            return paymentReceiptDto;
        }

        public Ellucian.Colleague.Dtos.Finance.Payments.ElectronicCheckProcessingResult PostProcessElectronicCheck(Ellucian.Colleague.Dtos.Finance.Payments.Payment paymentDetails)
        {
            CheckPayOwnAccountPermission(paymentDetails.PersonId);
            try
            {
                var paymentAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Finance.Payments.Payment, Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment>();

                var paymentEntity = paymentAdapter.MapToType(paymentDetails);

                var checkProcessingResultEntity = _accountRepository.ProcessCheck(paymentEntity);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckProcessingResult, Ellucian.Colleague.Dtos.Finance.Payments.ElectronicCheckProcessingResult>();

                var checkProcessingResultDto = adapter.MapToType(checkProcessingResultEntity);

                return checkProcessingResultDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }

        }

        public Ellucian.Colleague.Dtos.Finance.Payments.ElectronicCheckPayer GetCheckPayerInformation(string personId)
        {
            CheckPayOwnAccountPermission(personId);
            try
            {
                var checkPayerInformationEntity = _accountRepository.GetCheckPayerInformation(personId);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckPayer, Ellucian.Colleague.Dtos.Finance.Payments.ElectronicCheckPayer>();

                var checkPayerInformationDto = adapter.MapToType(checkPayerInformationEntity);

                return checkPayerInformationDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="allPaymentMethods"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Finance.Configuration.AvailablePaymentMethod>> GetRestrictedPaymentMethodsAsync(string studentId, IEnumerable<AvailablePaymentMethod> allPaymentMethods)
        {
            try
            {
                var restrictedPaymentMethodEntities = await _paymentRepository.GetRestrictedPaymentMethodsAsync(studentId, allPaymentMethods);
                var restrictedPaymentMethodDtos = new List<Dtos.Finance.Configuration.AvailablePaymentMethod>();
                var adapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.AvailablePaymentMethod, Dtos.Finance.Configuration.AvailablePaymentMethod>(_adapterRegistry, logger);
                foreach (var restrictedPaymentMethodEntity in restrictedPaymentMethodEntities)
                {
                    restrictedPaymentMethodDtos.Add(adapter.MapToType(restrictedPaymentMethodEntity));
                }
                return restrictedPaymentMethodDtos;
            }
            // add catches
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }
    }
}
