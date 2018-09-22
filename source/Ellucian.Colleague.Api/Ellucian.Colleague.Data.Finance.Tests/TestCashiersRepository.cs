using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestCashiersRepository
    {
        private static Collection<Cashiers> _cashiers = new Collection<Cashiers>();
        public static Collection<Cashiers> Cashiers
        {
            get
            {
                if (_cashiers.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _cashiers;
            }
        }

        private static Dictionary<string, string> _logins = new Dictionary<string, string>();
        public static Dictionary<string, string> Logins
        {
            get
            {
                if (_logins.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _logins;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize;
            var inputData = GetCashiersData(out rowSize);

            for (int i = 0; i < inputData.Length / rowSize; i++)
            {
                var cashierId = inputData[i, 0].Trim();
                var opersId = inputData[i, 1].Trim();
                var eCommEnabled = inputData[i, 2].Trim();
                decimal? ccLimit = inputData[i, 3].Trim().Length > 0 ? Decimal.Parse(inputData[i, 3].Trim()) : (decimal?)null;
                decimal? ckLimit = inputData[i, 4].Trim().Length > 0 ? Decimal.Parse(inputData[i, 4].Trim()) : (decimal?)null;

                var cashier = new Cashiers()
                {
                    Recordkey = cashierId,
                    CshrEcommerceFlag = eCommEnabled,
                    CshrCrCardAmt = ccLimit,
                    CshrCheckAmt = ckLimit
                };

                _cashiers.Add(cashier);
                _logins.Add(cashierId, opersId);
            }
        }

        private static string[,] GetCashiersData(out int rowSize)
        {
            string[,] inputData =
            {
                { "0000012", "DATATEL   ", " ", "    .00", "    .00" },
                { "0000362", "WEBCASHIER", "Y", "       ", "       " },
                { "0000512", "LXO       ", "Y", "       ", "       " },
                { "0000893", "JAM       ", "N", "       ", "       " },
                { "0000898", "BKM       ", "Y", "       ", "       " },
                { "0000927", "MGH       ", "Y", "       ", "       " },
                { "0000955", "FTU       ", "Y", "       ", "       " },
                { "0001604", "MXF       ", "Y", "       ", "       " },
                { "0001700", "TDJ       ", "N", "       ", "9999.00" },
                { "0002852", "MXF1      ", "Y", "       ", "       " },
                { "0003315", "JPM2      ", "Y", "       ", "       " },
                { "0003884", "GTT       ", "N", "       ", "       " },
                { "0003893", "PJN       ", "N", "       ", "       " },
                { "0003895", "WMK       ", "Y", "       ", "       " },
                { "0003900", "JTM       ", "Y", "       ", "       " },
                { "0003910", "DAT       ", "Y", "       ", "       " },
                { "0003946", "AJK       ", "Y", "       ", "       " },
                { "0003950", "SXO       ", "Y", "       ", "       " },
                { "0003960", "BDM1      ", "Y", "       ", "       " },
                { "0003973", "DUS       ", "Y", "       ", "       " },
                { "0003977", "BMA       ", "Y", "       ", "       " },
                { "0003983", "AGG       ", "N", "       ", "       " },
                { "0004525", "DSO       ", "Y", " 100.00", " 200.00" },
                { "0010845", "DHARDGROVE", "N", "9999.00", "9999.00" },
                { "0011029", "MWK       ", "N", "       ", "       " }
            };

            rowSize = 5;
            return inputData;
        }
    }
}
