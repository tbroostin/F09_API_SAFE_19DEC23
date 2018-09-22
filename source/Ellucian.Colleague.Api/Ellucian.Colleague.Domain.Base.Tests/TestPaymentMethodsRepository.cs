using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public static class TestPaymentMethodsRepository
    {
        private static readonly Collection<PaymentMethods> _paymentMethods = new Collection<PaymentMethods>();

        public static Collection<PaymentMethods> PaymentMethods
        {
            get
            {
                if (_paymentMethods.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _paymentMethods;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize;
            string[,] inputData = GetData(out rowSize);
            for (int i = 0; i < inputData.Length / rowSize; i++)
            {
                string code = inputData[i, 0].Trim();
                string desc = inputData[i, 1].Trim();
                string cat = inputData[i, 2].Trim();
                string web = inputData[i, 3].Trim();
                string ec = inputData[i, 4].Trim();
                string officeCodes = inputData[i, 5] == null ? null : inputData[i, 5].Trim();
                var offCodes = officeCodes == null ? null : string.IsNullOrEmpty(officeCodes) ? new List<string>() : officeCodes.Split(';').ToList();

                var record = new PaymentMethods()
                {
                    Recordkey = code,
                    PmthDescription = desc,
                    PmthCategory = cat,
                    PmthWebPmtFlag = web,
                    PmthEcommEnabledFlag = ec,
                    PmthOfficeCodes = offCodes
                };

                _paymentMethods.Add(record);
            }
        }

        private static string[,] GetData(out int rowSize)
        {
            string[,] dataTable =
            {
                {"AR  ", "Contribution Artwork          ", "RP", "N", "N", "AD         "},
                {"AX  ", "Contribution PayPal CC1       ", "CC", "N", "Y", "AD         "},
                {"BD  ", "Contribution Bonds            ", "SS", "N", "N", "AD         "},
                {"CA  ", "Cash Contribution             ", "CA", "N", "N", "AD         "},
                {"CC  ", "Credit Card                   ", "CC", "Y", "Y", "ADM;BUS;CE "},
                {"CD  ", "Contribution Canadian Dollars ", "CA", "N", "N", "AD         "},
                {"CK  ", "Check                         ", "CK", "N", "N", "           "},
                {"CS  ", "Contributed Serv Contribution ", "CS", "N", "N", "AD         "},
                {"DI  ", "Contribution Discover         ", "CC", "N", "Y", "AD         "},
                {"DISC", "Discover                      ", "CC", "Y", "Y", "           "},
                {"DN  ", "Contribution Diners Club      ", "OT", "N", "N", "AD         "},
                {"ECHK", "e-Check                       ", "CK", "Y", "Y", "ADM;BUS;CE "},
                {"ECK ", "Contribution PayPal E-Check   ", "CK", "Y", "Y", "AD         "},
                {"ECKO", "Contribution OPC E-Check      ", "CK", "Y", "Y", "AD         "},
                {"EFTR", "Electronic Funds Transfer     ", "EF", "N", "N", "BUS        "},
                {"EQ  ", "Contribution Equipment        ", "RP", "N", "N", "AD         "},
                {"FA  ", "Financial Aid via Cash Receipt", "CK", "N", "N", "BUS        "},
                {"FC  ", "Contribution  Foreign Currency", "CA", "N", "N", "AD         "},
                {"IK  ", "In-Kind Contribution          ", "OI", "N", "N", "AD         "},
                {"IN  ", "Contribution Life Insurance   ", "IR", "N", "N", "AD         "},
                {"LI  ", "Life Income Plan Contribution ", "IR", "N", "N", "AD         "},
                {"MAST", "MasterCard                    ", "CC", "Y", "Y", "           "},
                {"MC  ", "Contribution PayPal CC2       ", "CC", "N", "Y", "AD         "},
                {"MO  ", "Contribution Money Order      ", "CA", "N", "N", "AD         "},
                {"NCC ", "Contribution Credit Card no e-", "OT", "N", "N", "AD         "},
                {"NCK ", "Contribution Check no e-commer", "CK", "N", "N", "AD         "},
                {"PC  ", "Parent Check                  ", "CK", "N", "N", "BUS        "},
                {"PD  ", "Contribution Payroll Deduction", "PD", "N", "N", "AD         "},
                {"RE  ", "Real Estate Contribution      ", "RP", "N", "N", "AD         "},
                {"ST  ", "Stock/Securities Contribution ", "SS", "N", "N", "AD         "},
                {"TNCC", "TouchNet Credit Card          ", "CC", "Y", "Y", "           "},
                {"TNCK", "TouchNet e-Check              ", "CK", "Y", "Y", "           "},
                {"TRAV", "Travelers Checks              ", "CK", "N", "N", "           "},
                {"UN  ", "ContributionUnspecified Pledge", "CA", "N", "N", "AD         "},
                {"VI  ", "OPC CC Contribution           ", "CC", "N", "Y", "AD         "},
                {"VISA", "VISA                          ", "CC", "Y", "Y", "           "},
                {"VS  ", "Contribution Volunteer Service", "CS", "N", "N", "AD         "},
                {"WMC ", "Contribution PayPal Web CC    ", "CC", "Y", "Y", "AD         "},
                {"WVI ", "Contribution OPC Web CC       ", "CC", "Y", "Y", "AD         "},
                {"ZZZZ", "Bogus payment method for tests", "  ", " ", " ", null         }
            };

            rowSize = 6;
            return dataTable;
        }
    }
}
