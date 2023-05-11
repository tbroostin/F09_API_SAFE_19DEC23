// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class ReceiptService : FinanceCoordinationService, IReceiptService
    {
        private IAccountsReceivableRepository _arRepository;
        private ITermRepository _termRepository;
        private IECommerceRepository _eCommRepository;
        private IReceiptRepository _receiptRepository;

        private IEnumerable<Domain.Base.Entities.Distribution> _distributions;
        private IEnumerable<Domain.Base.Entities.Distribution> Distributions
        {
            get
            {
                if (_distributions == null)
                {
                    _distributions = _eCommRepository.Distributions;
                }
                return _distributions;
            }
        }

        private IEnumerable<Domain.Finance.Entities.ExternalSystem> _externalSystems;
        private IEnumerable<Domain.Finance.Entities.ExternalSystem> ExternalSystems
        {
            get
            {
                if (_externalSystems == null)
                {
                    _externalSystems = _arRepository.ExternalSystems;
                }
                return _externalSystems;
            }
        }

        private IEnumerable<Domain.Base.Entities.PaymentMethod> _paymentMethods;
        private IEnumerable<Domain.Base.Entities.PaymentMethod> PaymentMethods
        {
            get
            {
                if (_paymentMethods == null)
                {
                    _paymentMethods = _eCommRepository.PaymentMethods;
                }
                return _paymentMethods;
            }
        }

        private IEnumerable<Domain.Finance.Entities.DepositType> _depositTypes;
        private IEnumerable<Domain.Finance.Entities.DepositType> DepositTypes
        {
            get
            {
                if (_depositTypes == null)
                {
                    _depositTypes = _arRepository.DepositTypes;
                }
                return _depositTypes;
            }
        }

        private IEnumerable<Domain.Student.Entities.Term> _terms;
        private IEnumerable<Domain.Student.Entities.Term> Terms
        {
            get
            {
                if (_terms == null)
                {
                    _terms = _termRepository.Get();
                }
                return _terms;
            }
        }


        /// <summary>
        /// Constructor for the receipt coordination service
        /// </summary>
        /// <param name="adapterRegistry">Interface to Adapter Registry</param>
        /// <param name="receiptRepository">Interface to Receipt Repository</param>
        /// <param name="arRepository">Interface to AccountsReceivableRepository</param>
        /// <param name="termRepository">Interface to TermRepository</param>
        /// <param name="eCommRepository">Interface to ECommerceRepository</param>
        /// <param name="currentUserFactory">Interface to CurrentUserFactory</param>
        /// <param name="roleRepository">Interface to RoleRepository</param>
        /// <param name="logger">Interface to Logger</param>
        public ReceiptService(IAdapterRegistry adapterRegistry, IReceiptRepository receiptRepository, IAccountsReceivableRepository arRepository, ITermRepository termRepository,
            IECommerceRepository eCommRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _arRepository = arRepository;
            _termRepository = termRepository;
            _eCommRepository = eCommRepository;
            _receiptRepository = receiptRepository;
        }

        /// <summary>
        /// Retrieve a cashier
        /// </summary>
        /// <param name="id">The ID of the cashier to retrieve</param>
        /// <returns>The indicated cashier</returns>
        public Domain.Finance.Entities.Cashier GetCashier(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                return _receiptRepository.GetCashier(id);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }


        /// <summary>
        /// Create a receipt and associated financial items
        /// </summary>
        /// <param name="sourceReceipt">The information for the receipt of funds</param>
        /// <param name="sourcePayments">The information for any associated payments to create</param>
        /// <param name="sourceDeposits">The information for any associated deposits to create</param>
        /// <returns></returns>
        public Receipt CreateReceipt(Receipt sourceReceipt, IEnumerable<ReceiptPayment> sourcePayments,
            IEnumerable<Deposit> sourceDeposits)
        {
            CheckCreateReceiptPermission();

            try
            {
                // prepare Receipt argument
                var receiptAdapter = _adapterRegistry.GetAdapter<Dtos.Finance.Receipt, Domain.Finance.Entities.Receipt>();
                var entReceipt = receiptAdapter.MapToType(sourceReceipt);

                // prepare payments argument
                var entPayments = new List<Domain.Finance.Entities.ReceiptPayment>();
                if (sourcePayments != null && sourcePayments.Any())
                {
                    var paymentAdapter = _adapterRegistry.GetAdapter<Dtos.Finance.ReceiptPayment, Domain.Finance.Entities.ReceiptPayment>();
                    entPayments.AddRange(sourcePayments.Select(x => paymentAdapter.MapToType(x)));
                }

                // prepare deposits argument
                var entDeposits = new List<Domain.Finance.Entities.Deposit>();
                if (sourceDeposits != null && sourceDeposits.Any())
                {
                    var depositAdapter = _adapterRegistry.GetAdapter<Dtos.Finance.Deposit, Domain.Finance.Entities.Deposit>();
                    entDeposits.AddRange(sourceDeposits.Select(x => depositAdapter.MapToType(x)));
                }

                ValidateReceipt(entReceipt, entPayments, entDeposits);

                foreach (var entDeposit in entDeposits)
                {
                    ValidateDeposit(entDeposit);
                }

                // create the receipt and return the receipt DTO 
                var createdReceipt = _receiptRepository.CreateReceipt(entReceipt, entPayments, entDeposits);
                var entReceiptAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Receipt, Dtos.Finance.Receipt>();
                return entReceiptAdapter.MapToType(createdReceipt);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
            catch (AutoMapper.AutoMapperMappingException ex)
            {
                throw (ex.InnerException == null ? ex : ex.InnerException);
            }
        }

        #region private helper methods
        private void ValidateReceipt(Domain.Finance.Entities.Receipt receipt, IEnumerable<Domain.Finance.Entities.ReceiptPayment> payments,
            IEnumerable<Domain.Finance.Entities.Deposit> deposits)
        {
            if (!((IPersonRepository)_arRepository).IsPerson(receipt.PayerId))
            {
                throw new ArgumentException("Invalid receipt payer " + receipt.PayerId);
            }

            Domain.Finance.Services.ReceiptProcessor.ValidateReceipt(receipt, payments, deposits,
                Distributions, ExternalSystems, PaymentMethods);
        }

        private void ValidateDeposit(Domain.Finance.Entities.Deposit deposit)
        {
            if (!((IPersonRepository)_arRepository).IsPerson(deposit.PersonId))
            {
                throw new ArgumentException("Invalid deposit holder " + deposit.PersonId);
            }

            Domain.Finance.Services.ReceivableService.ValidateDeposit(deposit, DepositTypes, ExternalSystems, Terms);
        }

        #endregion

    }
}
