// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using ConvenienceFee = Ellucian.Colleague.Domain.Base.Entities.ConvenienceFee;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for E-Commerce
    /// </summary>
    [RegisterType]
    public class ECommerceRepository : BaseColleagueRepository, IECommerceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ECommerceRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public ECommerceRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Gets the convenience fees
        /// </summary>
        public IEnumerable<ConvenienceFee> ConvenienceFees
        {
            get
            {
                // Cache this entry for 1 day
                return GetCodeItem<ConvenienceFees, ConvenienceFee>("AllConvenienceFees", "CONVENIENCE.FEE",
                    cf => new ConvenienceFee(cf.Recordkey, cf.ConvfDescription), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Gets all distributions
        /// </summary>
        public IEnumerable<Distribution> Distributions
        {
            get
            {
                return GetCodeItem<Distributions, Distribution>("AllDistributions", "DISTRIBUTION",
                    d => new Distribution(d.Recordkey, d.DistrDescription), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Gets all payment methods
        /// </summary>
        public IEnumerable<PaymentMethod> PaymentMethods
        {
            get
            {
                return GetCodeItem<PaymentMethods, PaymentMethod>("AllPaymentMethods", "PAYMENT.METHOD",
                    pm =>
                    {
                        var payMethod = new PaymentMethod(pm.Recordkey, pm.PmthDescription, ConvertCodeToPaymentMethodCategory(pm.PmthCategory),
                            pm.PmthWebPmtFlag == "Y", pm.PmthEcommEnabledFlag == "Y");
                        if (pm.PmthOfficeCodes != null && pm.PmthOfficeCodes.Any())
                        {
                            foreach (var code in pm.PmthOfficeCodes)
                            {
                                payMethod.AddOfficeCode(code);
                            }
                        }
                        return payMethod;
                    }, Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Convert a Colleague payment method category code into its corresponding PaymentMethodCategory
        /// </summary>
        /// <param name="category">Colleague payment method category code</param>
        /// <returns>PaymentMethodCategory value</returns>
        public static PaymentMethodCategory ConvertCodeToPaymentMethodCategory(string category)
        {
            //if (string.IsNullOrEmpty(category))
            //{
            //    return PaymentMethodCategory.Other;
            //}
            switch (category)
            {
                case "CA":
                    return PaymentMethodCategory.Cash;
                case "CK":
                    return PaymentMethodCategory.Check;
                case "CC":
                    return PaymentMethodCategory.CreditCard;
                case "SS":
                    return PaymentMethodCategory.StocksAndSecurities;
                case "RP":
                    return PaymentMethodCategory.RealTangibleProperty;
                case "IR":
                    return PaymentMethodCategory.InsuranceAndRetirement;
                case "CS":
                    return PaymentMethodCategory.ContributedServices;
                case "OI":
                    return PaymentMethodCategory.OtherInKind;
                case "PD":
                    return PaymentMethodCategory.PayrollDeduction;
                case "EF":
                    return PaymentMethodCategory.ElectronicFundsTransfer;
                default:
                    return PaymentMethodCategory.Other;
            }
        }
    }
}
