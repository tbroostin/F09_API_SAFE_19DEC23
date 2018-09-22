// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public class TestArInvoicesRepository
    {
        private static Collection<ArInvoices> _arInvoices = new Collection<ArInvoices>();
        public static Collection<ArInvoices> ArInvoices
        {
            get
            {
                if (_arInvoices.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arInvoices;
            }
        }

        static void GenerateDataContracts()
        {
            string[,] invoicesData = GetInvoiceData();
            int invoiceCount = invoicesData.Length / 10;

            for (int i = 0; i < invoiceCount; i++)
            {
                string id = invoicesData[i, 0].Trim();
                string personId = invoicesData[i, 1].Trim();
                string accountTypeCode = invoicesData[i, 2].Trim();
                string invNumber = invoicesData[i, 3].Trim();
                DateTime date = DateTime.Parse(invoicesData[i, 4].Trim());
                DateTime dueDate = DateTime.Parse(invoicesData[i, 5].Trim());
                DateTime billingStart = DateTime.Parse(invoicesData[i, 6].Trim());
                DateTime billingEnd = DateTime.Parse(invoicesData[i, 7].Trim());
                string description = invoicesData[i, 8].Trim();
                string termId = invoicesData[i, 9].Trim();
                //List<string> items = (itemsTable.ContainsKey(id)) ? itemsTable[id] : null;
                var items = TestArInvoiceItemsRepository.ArInvoiceItems.Where(x => x.InviInvoice == id).Select(x => x.Recordkey);

                ArInvoices arInvoice = new ArInvoices()
                {
                    Recordkey = id,
                    InvPersonId = personId,
                    InvArType = accountTypeCode,
                    InvTerm = termId,
                    InvNo = invNumber,
                    InvDate = date,
                    InvDueDate = dueDate,
                    InvBillingStartDate = billingStart,
                    InvBillingEndDate = billingEnd,
                    InvDesc = description,
                    InvInvoiceItems = (items == null) ? new List<string>() : new List<string>(items)
                };
                if (arInvoice.Recordkey == "4")
                {
                    arInvoice.InvTerm = null;
                }
                if (arInvoice.Recordkey == "2313")
                {
                    arInvoice.InvReferenceNos = new List<string>() { "2313REFNO" };
                }
                if (arInvoice.Recordkey == "22")
                {
                    arInvoice.InvAdjByInvoices = new List<string>() { "23" };
                }
                _arInvoices.Add(arInvoice);
            }
        }

        static string[,] GetInvoiceData()
        {
            string[,] invoicesData = {
                                       // ID      Person ID   AR Type  Invoice No    Date          Due Date      Bill Start    Bill End      Description               Term
                                        {"2313", "0003500",  "01",    "000002001",  "02/16/2012", "01/09/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/SP"},
                                        {"2314", "0003501",  "01",    "000002002",  "02/16/2012", "01/09/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/SP"},
                                        {"2315", "0003502",  "01",    "000002003",  "02/16/2012", "01/09/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/SP"},
                                        {"2316", "0003503",  "01",    "000002004",  "02/16/2012", "01/09/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/SP"},
                                        {"5197", "0003584",  "01",    "000002735",  "02/11/2013", "02/11/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/SP"},
                                        {"7048", "0003583",  "01",    "000003397",  "05/02/2013", "05/09/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7052", "0003583",  "01",    "000003402",  "05/04/2013", "05/09/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7054", "0003583",  "01",    "000003403",  "05/04/2013", "05/04/2013", "01/23/2013", "05/15/2013", "Registration - 2013/SP", "2013/S1"},
                                        {"7055", "0003583",  "01",    "000003405",  "05/04/2013", "05/09/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7057", "0003583",  "01",    "000003407",  "05/04/2013", "05/09/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7133", "0003583",  "01",    "000003470",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7135", "0003583",  "01",    "000003472",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7137", "0003583",  "01",    "000003474",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7139", "0003583",  "01",    "000003476",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7141", "0003583",  "01",    "000003478",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7143", "0003583",  "01",    "000003480",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7145", "0003583",  "01",    "000003482",  "05/13/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7147", "0003583",  "01",    "000003484",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7151", "0003583",  "01",    "000003488",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7153", "0003583",  "01",    "000003490",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7155", "0003583",  "01",    "000003492",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7157", "0003583",  "01",    "000003494",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7159", "0003583",  "01",    "000003496",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7161", "0003583",  "01",    "000003498",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7163", "0003583",  "01",    "000003500",  "05/14/2013", "05/16/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7375", "0003584",  "01",    "000003563",  "05/30/2013", "05/30/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"7722", "0003583",  "01",    "000003602",  "06/11/2013", "06/11/2013", "05/23/2013", "07/03/2013", "Registration - 2013/S1", "2013/S1"},
                                        {"   1", "0000895",  "01",    "000000001",  "11/11/2023", "01/11/2024", "01/23/2024", "05/03/2024", "Registration - 2024/SP", "2024/SP"},
                                        {"   2", "0000895",  "01",    "000000002",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2015/SP", "2014/SP"},
                                        {"   3", "0000895",  "01",    "000000003",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2015/SP", "2015/SP"},
                                        {"   4", "0000895",  "01",    "000000004",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2015/SP", ""},
                                        {"   5", "0000895",  "01",    "000000005",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2014/FA", "2014/FA"},                                    
                                        {"   6", "0000895",  "01",    "000000006",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - Non-Term", ""},    
                                        {"  10", "1234567",  "01",    "000000007",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2013/FA", "2013/FA"},                                    
                                        {"  11", "1234567",  "01",    "000000008",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2013/FA", "2013/FA"},
                                        {"  21", "1234567",  "01",    "000000028",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2013/FA", "2013/FA"},                                                                   
                                        {"  22", "1234567",  "01",    "000000029",  "11/14/2014", "11/14/2014", "11/01/2014", "12/31/2014", "Registration - 2013/FA", "2013/FA"},
                                        {" 345", "0003315",  "01",    "000000030",  "11/14/2017", "11/14/2017", "11/01/2017", "12/31/2017", "Registration - 2017/FA", "2017/FA"},
                                        {" 346", "0003315",  "01",    "000000031",  "01/23/2018", "01/23/2018", "01/05/2018", "02/23/2018", "Registration - 2018/FA", "2018/FA"},
                                     };
            return invoicesData;
        }
    }
}
