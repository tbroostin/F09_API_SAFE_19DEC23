using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestCashRcptsRepository
    {
        private static Collection<CashRcpts> _cashRcpts = new Collection<CashRcpts>();
        public static Collection<CashRcpts> CashRcpts
        {
            get
            {
                if (_cashRcpts.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _cashRcpts;
            }
        }
        private class MyCashRcptsRcptNonCash
        {
            public string Id { get; set; }
            public CashRcptsRcptNonCash NonCashPmt { get; set; }
        }
        private static Collection<MyCashRcptsRcptNonCash> _nonCashPayments = new Collection<MyCashRcptsRcptNonCash>();
        private static Collection<MyCashRcptsRcptNonCash> NonCashPayments
        {
            get
            {
                if (!_nonCashPayments.Any())
                {
                    GenerateNonCashPayments();
                }
                return _nonCashPayments;
            }
        }

        private static void GenerateNonCashPayments()
        {
            string[,] nonCashPaymentData = 
            {// Id            PayMeth   Amount      rev amt
              { "0000000556", "PMTH1", " 55600",     "0"},
              { "0000000557", "PMTH1", " 55700",     "0"},
              { "0000000660", "PMTH2", " 66000",     "0"},
              { "0000000661", "PMTH2", " 66100",     "0"},
              { "0000000666", "PMTH2", " 22200",     "0"},// 666 has multiple payments
              { "0000000666", "PMTHA", " 22200",     "0"},// 666 has multiple payments
              { "0000000666", "PMTHB", " 22200",     "0"},// 666 has multiple payments
              { "0000000674", "PMTH2", " 67400",     "0"},
              { "0000000705", "PMTH3", " 70500",     "0"},
              { "0000000706", "PMTH3", " 70600",     "0"},
              { "0000000707", "PMTH3", " 70700",     "0"},
              { "0000000805", "PMTH4", " 80500",     "0"},
              { "0000000806", "PMTH4", " 40300",     "0"},// 806 has multiple payments
              { "0000000806", "PMTH4", " 40300",     "0"},// 806 has multiple payments
              { "0000000929", "PMTH5", " 92900",     "0"},
              { "0000000932", "PMTH5", " 93200",     "0"},
              { "0000000965", "PMTH5", "     0", "96500"},
              { "0000000976", "PMTH5", " 97600",     "0"},
              { "0000001024", "PMTH6", "102400",     "0"},
              { "0000001150", "PMTH7", "     0","115000"},
              { "0000001154", "PMTH7", "115400",     "0"},
              { "0000001336", "PMTH8", "133600",     "0"},
              { "0000001337", "PMTH8", "133700",     "0"},
              { "0000001401", "PMTH8", "140100",     "0"},
              { "0000001401", "PMTH8", "140100",     "0"},
              { "0000000350", "PMTH9", " 35000",     "0"},
              { "0000000540", "PMTH9", " 54000",     "0"}
            };

            int pmts = nonCashPaymentData.Length / 4;
            for (int i = 0; i < pmts; i++)
            {
                _nonCashPayments.Add(new MyCashRcptsRcptNonCash()
                {
                    Id = nonCashPaymentData[i, 0].Trim(),
                    NonCashPmt = new CashRcptsRcptNonCash()
                    {
                        RcptPayMethodsAssocMember = nonCashPaymentData[i, 1].Trim(),
                        RcptNonCashAmtsAssocMember = decimal.Parse(nonCashPaymentData[i, 2].TrimStart()),
                        RcptNonCashReversalAmtsAssocMember = decimal.Parse(nonCashPaymentData[i, 3].TrimStart())
                    }
                });
            }
        }


        private static void GenerateDataContracts()
        {
            int rowSize;
            string[,] receiptsData = GetReceiptData(out rowSize);
            int cashRcptsCount = receiptsData.Length / rowSize;
            var receiptDeposits = GetReceiptDepositIds();

            for (int i = 0; i < cashRcptsCount; i++)
            {
                string id = receiptsData[i, 0].Trim();
                string rcptNo = receiptsData[i, 1].Trim();
                DateTime rcptDate = DateTime.Parse(receiptsData[i, 2].Trim());
                string rcptSession = receiptsData[i, 3].Trim();
                string rcptPayerId = receiptsData[i, 4].Trim();
                string rcptPayerName = "Dr. " + receiptsData[i, 4].Trim();
                string rcptExternalSystem = receiptsData[i, 5].Trim();
                string rcptExternalId = receiptsData[i, 6].Trim();
                string rcptTenderGlDistrCode = receiptsData[i, 7].Trim();
                List<string> rcptDepositIds;
                if (!receiptDeposits.TryGetValue(id, out rcptDepositIds))
                {
                    rcptDepositIds = new List<string>();
                }

                CashRcpts cashRcpt = new CashRcpts()
                {
                    Recordkey = id,
                    RcptNo = rcptNo,
                    RcptDate = rcptDate,
                    RcptSession = rcptSession,
                    RcptPayerId = rcptPayerId,
                    RcptPayerDesc = rcptPayerName,
                    RcptTenderGlDistrCode = rcptTenderGlDistrCode,
                    RcptExternalId = rcptExternalId,
                    RcptExternalSystem = rcptExternalSystem,
                    RcptDeposits = rcptDepositIds
                };

                var nonCashPayments = from x in NonCashPayments where (x.Id == cashRcpt.Recordkey) select x.NonCashPmt;
                cashRcpt.RcptNonCashEntityAssociation = nonCashPayments.ToList();

                _cashRcpts.Add(cashRcpt);
            }
        }

        private static string[,] GetReceiptData(out int rowSize)
        {
            string[,] receiptsData = 
            {  // ID            Rcpt No      Date          Session       Payer      Ext Sys Ext Id     Dist
                { "0000000556", "000000287", "03/11/2014", "       100", "0001567", "ACME", "ACME001", "BANK " },
                { "0000000557", "000000288", "03/11/2014", "       100", "0001577", "ACME", "ACME008", "BANK " },
                { "0000000660", "000000332", "03/12/2014", "       200", "0002588", "ACME", "ACME009", "WEBA " },
                { "0000000661", "000000333", "03/12/2014", "       200", "0002678", "    ", "       ", "WEBB " },
                { "0000000666", "000000334", "03/12/2014", "       200", "0002686", "    ", "       ", "WEBC " },
                { "0000000674", "000000337", "03/12/2014", "       200", "0002745", "    ", "       ", "WEBD " },
                { "0000000705", "000000358", "03/13/2014", "       300", "0003398", "    ", "       ", "WEBE " },
                { "0000000706", "000000359", "03/13/2014", "       300", "0003832", "    ", "       ", "WEBF " },
                { "0000000707", "000000360", "03/13/2014", "       300", "0003912", "    ", "       ", "WEBG " },
                { "0000000805", "000000452", "03/14/2014", "       400", "0004269", "ACME", "ACME012", "BANK " },
                { "0000000806", "000000453", "03/14/2014", "       400", "0004419", "    ", "       ", "TRAV " },
                { "0000000929", "000000556", "03/15/2014", "       500", "0005704", "    ", "       ", "BANK " },
                { "0000000932", "000000557", "03/15/2014", "       500", "0005798", "    ", "       ", "BANK " },
                { "0000000965", "000000584", "03/15/2014", "       500", "0005882", "    ", "       ", "BANK " },
                { "0000000976", "000000594", "03/15/2014", "       500", "0005886", "    ", "       ", "BANK " },
                { "0000001024", "000000635", "03/16/2014", "       600", "0006225", "    ", "       ", "TRAV " },
                { "0000001150", "000000728", "03/17/2014", "       700", "0007047", "ACME", "ACME027", "BANK " },
                { "0000001154", "000000729", "03/18/2014", "       700", "0007163", "ACME", "ACME063", "BANK " },
                { "0000001336", "000000829", "03/18/2014", "       800", "0008261", "    ", "       ", "BANK " },
                { "0000001337", "000000830", "03/18/2014", "       800", "0008362", "    ", "       ", "BANK " },
                { "0000001401", "000000877", "03/18/2014", "       800", "0008463", "    ", "       ", "BANK " },
                { "0000001402", "000000877", "03/18/2014", "       800", "0008564", "    ", "       ", "BANK " },
                { "0000000350", "000000204", "03/19/2014", "       900", "0009539", "    ", "       ", "TRAV " },
                { "0000000540", "000000280", "03/19/2014", "       900", "0009748", "    ", "       ", "TRAV " }
            };

            rowSize = 8;
            return receiptsData;
        }

        private static Dictionary<string, List<string>> GetReceiptDepositIds()
        {
            string[,] inputData = 
            {
                { "1112", "0000000556" },
                { "1114", "0000000557" },
                { "1320", "0000000660" },
                { "1322", "0000000661" },
                { "1332", "0000000666" },
                { "1348", "0000000674" },
                { "1410", "0000000705" },
                { "1412", "0000000706" },
                { "1414", "0000000707" },
                { "1610", "0000000805" },
                { "1612", "0000000806" },
                { "1858", "0000000929" },
                { "1864", "0000000932" },
                { "1930", "0000000965" },
                { "1952", "0000000976" },
                { "2048", "0000001024" },
                { "2300", "0000001150" },
                { "2308", "0000001154" },
                { "2672", "0000001336" },
                { "2674", "0000001337" },
                { "2802", "0000001401" },
                { "2804", "0000001402" },
                { "700 ", "0000000350" },
                { "1080", "0000000540" }
            };

            var table = new Dictionary<string, List<string>>();
            for (int i = 0; i < inputData.Length / 2; i++)
            {
                var depositId = inputData[i, 0].Trim();
                var receiptId = inputData[i, 1].Trim();
                List<string> depositIds;
                if (table.TryGetValue(receiptId, out depositIds))
                {
                    // receipt ID already in the table - add the deposit id to the list in the table
                    depositIds.Add(depositId);
                    table[receiptId] = depositIds;
                }
                else
                {
                    // receipt ID not in table - add it
                    depositIds = new List<string> { depositId };
                    table.Add(receiptId, depositIds);
                }
            }

            return table;
        }
    }
}
