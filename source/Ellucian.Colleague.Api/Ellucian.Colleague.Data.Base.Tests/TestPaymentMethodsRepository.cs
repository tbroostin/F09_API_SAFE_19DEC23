using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestPaymentMethodsRepository
    {
        private static Collection<PaymentMethods> _paymentMethods = new Collection<PaymentMethods>();
        public static Collection<PaymentMethods> PaymentMethods
        {
            get
            {
                if (!_paymentMethods.Any())
                {
                    GeneratePaymentMethods();
                }
                return _paymentMethods;
            }
        }

        private static void GeneratePaymentMethods()
        {
            string[,] pmData = {
          //  Id       Desc                Category                   Web  eComm
            { "PMTH1", "Payment Method 1", "Check",                   "N", "N" },
            { "PMTH2", "Payment Method 2", "Check",                   "Y", "Y" },
            { "PMTH3", "Payment Method 3", "Check",                   "Y", "Y" },
            { "PMTH4", "Payment Method 4", "CreditCard",              "N", "N" },
            { "PMTH5", "Payment Method 5", "CreditCard",              "Y", "Y" },
            { "PMTH6", "Payment Method 6", "CreditCard",              "Y", "Y" },
            { "PMTH7", "Payment Method 7", "ElectronicFundsTransfer", "Y", "Y" },
            { "PMTH8", "Payment Method 8", "ElectronicFundsTransfer", "N", "N" },
            { "PMTH9", "Payment Method 9", "Cash",                    "N", "N" },
            { "PMTHA", "Payment Method A", "Other",                   "Y", "Y" },
            { "PMTHB", "Payment Method B", "Other",                   "N", "N" }
                               };

            int numRows = pmData.Length / 5;
            for (int i = 0; i < numRows; i++)
            {
                _paymentMethods.Add(new PaymentMethods()
                {
                    Recordkey = pmData[i, 0],
                    PmthDescription = pmData[i, 1],
                    PmthCategory = pmData[i, 2],
                    PmthWebPmtFlag = pmData[i, 3],
                    PmthEcommEnabledFlag = pmData[i, 4]
                });
            }
        }
    }
}