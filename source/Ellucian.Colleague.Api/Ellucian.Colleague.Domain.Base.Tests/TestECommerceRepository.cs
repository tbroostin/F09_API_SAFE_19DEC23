using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    /// <summary>
    /// Class with testing data that mocks the ECommerceRepository
    /// </summary>
    public static class TestECommerceRepository
    {
        private static List<ConvenienceFee> _convenienceFees;
        /// <summary>
        /// List of convenience fee domain entities for testing
        /// </summary>
        public static IEnumerable<ConvenienceFee> ConvenienceFees
        {
            get
            {
                if (_convenienceFees == null)
                {
                    _convenienceFees = TestConvenienceFeesRepository.ConvenienceFees.Select(
                        cf => new ConvenienceFee(cf.Recordkey, cf.ConvfDescription)).ToList();
                }
                return _convenienceFees;
            }
        }

        private static List<Distribution> _distributions;
        /// <summary>
        /// List of distribution domain entities for testing
        /// </summary>
        public static IEnumerable<Distribution> Distributions
        {
            get
            {
                if (_distributions == null)
                {
                    _distributions = TestDistributionsRepository.Distributions.Select(
                            d => new Distribution(d.Recordkey, d.DistrDescription)).ToList();
                }
                return _distributions;
            }
        }

        private static List<PaymentMethod> _paymentMethods;
        /// <summary>
        /// List of payment method domain entities for testing
        /// </summary>
        public static List<PaymentMethod> PaymentMethods
        {
            get
            {
                if (_paymentMethods == null)
                {
                    _paymentMethods = TestPaymentMethodsRepository.PaymentMethods.Select(
                        pm =>
                        {
                            var payMethod = new PaymentMethod(pm.Recordkey, pm.PmthDescription,
                                ECommerceRepository.ConvertCodeToPaymentMethodCategory(pm.PmthCategory),
                                pm.PmthWebPmtFlag == "Y", pm.PmthEcommEnabledFlag == "Y");
                            foreach (var code in pm.PmthOfficeCodes)
                            {
                                payMethod.AddOfficeCode(code);
                            }
                            return payMethod;
                        }).ToList();
                }
                return _paymentMethods;
            }
        }
    }
}
