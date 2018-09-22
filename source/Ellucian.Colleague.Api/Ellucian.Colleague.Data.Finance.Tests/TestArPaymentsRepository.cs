using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArPaymentsRepository
    {
        private static Collection<ArPayments> _arPayments = new Collection<ArPayments>();
        public static Collection<ArPayments> ArPayments
        {
            get
            {
                if (_arPayments.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arPayments;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] paymentsData = {    //ID      Person ID  AR Type   Date           Cash Rcpt      Term          Location  Amount      Reversal Amount
                                          {"1037", "0003315", "01",     "02/28/2013",  "0000000556",  "2013/SP",    "MC",     "6600.00",  "0.00" },
                                          {"1038", "0003315", "01",     "02/28/2013",  "0000000557",  "2013/SP",    "MC",       "60.00",  "0.00" },
                                          {"1126", "0003315", "01",     "03/18/2013",  "0000000660",  "2013/SP",    "MC",      "322.50",  "0.00" },
                                          {"1128", "0003315", "01",     "02/28/2013",  "0000000661",  "2013/SP",    "MC",     "6600.00",  "0.00" },
                                          {"1132", "0003315", "01",     "02/28/2013",  "0000000666",  "2013/SP",    "MC",       "60.00",  "0.00" },
                                          {"1155", "0003315", "01",     "03/18/2013",  "0000000674",  "2013/SP",    "MC",      "322.50",  "0.00" },
                                          {"1194", "0003315", "01",     "03/18/2013",  "0000000705",  "2013/SP",    "MC",      "100.00",  "0.00" },
                                          {"1195", "0003315", "01",     "03/18/2013",  "0000000706",  "2013/SP",    "MC",       "10.60",  "0.00" },
                                          {"1196", "0003315", "01",     "03/22/2013",  "0000000707",  "2013/SP",    "MC",       "20.00",  "0.00" },
                                          {"1308", "0003315", "01",     "04/02/2013",  "0000000805",  "2013/SP",    "MC",      "700.00",  "0.00" },
                                          {"1309", "0003315", "01",     "04/02/2013",  "0000000806",  "2013/SP",    "MC",        "0.00","700.00" },
                                          {"1445", "0003315", "01",     "04/02/2013",  "0000000929",  "2013/SP",    "MC",      "200.00",  "0.00" },
                                          {"1448", "0003315", "01",     "04/05/2013",  "0000000932",  "2013/SP",    "MC", "15675555.18",  "0.00" },
                                          {"1477", "0003315", "01",     "04/05/2013",  "0000000965",  "2013/SP",    "MC",  "2250224.00",  "0.00" },
                                          {"1478", "0003315", "ABC",    "04/05/2013",  "0000000976",  "2013/SP",    "MC",        "5.00",  "0.00" },
                                          {"1492", "0003315", "02",     "04/23/2013",  "0000001024",  "2013/SP",    "MC",       "10.00",  "0.00" },
                                          {"1605", "0003315", "01",     "04/24/2013",  "0000001150",  "2013/SP",    "MC",      "250.00",  "0.00" },
                                          {"1609", "0003315", "01",     "04/24/2013",  "0000001154",  "2013/SP",    "MC",      "450.00",  "0.00" },
                                          {"1904", "0003315", "01",     "05/01/2013",  "0000001336",  "2013/S1",    "MC",      "100.00",  "0.00" },
                                          {"1905", "0003315", "01",     "05/13/2013",  "0000001337",  "2013/S2",    "MC",        "5.00",  "0.00" },
                                          {"2020", "0003315", "01",     "05/10/2013",  "0000001401",  "2013/S2",    "MC",      "123.45",  "0.00" },
                                          {"2021", "0003315", "04",     "06/13/2013",  "0000001401",  "2013/S2",    "MC",      "500.00",  "0.00" },
                                          {"572" , "0003315", "05",     "06/13/2013",  "0000000350",  "2013/FA",    "MC",     "3000.00",  "0.00" },
                                          {"984" , "0003315", "01",     "02/21/2013",  "0000000540",  "2013/FA",    "MC",        "5.00",  "0.00" },
                                          {"985" , "0003315", "01",     "02/21/2013",  "          ",  "2013/FA",    "MC",        "5.00",  "0.00" }
                                      };
            int arPaymentCount = paymentsData.Length / 9;

            for (int i = 0; i < arPaymentCount; i++)
            {
                string id = paymentsData[i, 0].TrimEnd();
                string personId = paymentsData[i, 1].TrimEnd();
                string receivableType = paymentsData[i, 2].TrimEnd();
                DateTime date = DateTime.Parse(paymentsData[i, 3].Trim());
                string rcptId = paymentsData[i, 4].TrimEnd();
                string termId = paymentsData[i, 5].TrimEnd();
                string location = paymentsData[i, 6].TrimEnd();
                decimal amount = Decimal.Parse(paymentsData[i, 7].Trim());
                decimal revAmount = Decimal.Parse(paymentsData[i, 8].Trim());
                string archive = null;

                ArPayments arPayment = new ArPayments()
                {
                    Recordkey = id,
                    ArpPersonId = personId,
                    ArpArType = receivableType,
                    ArpDate = date,
                    ArpCashRcpt = rcptId,
                    ArpTerm = termId,
                    ArpLocation = location,
                    ArpAmt = amount,
                    ArpReversalAmt = revAmount,
                    ArpArchive = archive
                };

                _arPayments.Add(arPayment);
            }
        }
    }
}
