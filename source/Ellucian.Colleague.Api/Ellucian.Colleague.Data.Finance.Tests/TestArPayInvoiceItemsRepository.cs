using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArPayInvoiceItemsRepository
    {
        private static readonly Collection<ArPayInvoiceItems> _arPayInvoiceItems = new Collection<ArPayInvoiceItems>();
        public static Collection<ArPayInvoiceItems> ArPayInvoiceItems
        {
            get
            {
                if (_arPayInvoiceItems.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arPayInvoiceItems;
            }
        }

        private static void GenerateDataContracts()
        {
            var planChargesData = GetPlanChargeData();
            for (int i = 0; i < planChargesData.Length / 5; i++)
            {
                string planId = planChargesData[i, 0].Trim();
                string chargeId = planChargesData[i, 1].Trim();
                decimal amount = Convert.ToDecimal(planChargesData[i, 2].Trim());
                string setupFlag = planChargesData[i, 3].Trim().ToUpperInvariant();
                string modFlag = planChargesData[i, 4].Trim().ToUpperInvariant();
                _arPayInvoiceItems.Add(new ArPayInvoiceItems()
                    {
                        Recordkey = planId + "*" + chargeId,
                        ArpliiAmt = amount,
                        ArpliiSetupInvoiceFlag = setupFlag,
                        ArpliiAllocationFlag = modFlag
                    });
            }
        }

        private static string[,] GetPlanChargeData()
        {
            string[,] planChargesTable = {
                                         // PlanId  ChargeId  Amount  Setup  Mod
                                            { "   1", "13284", " 704.00", "Y", "Y" },
                                            { "   1", "13231", " 175.00", "N", "Y" },
                                            { "   1", "13232", " 225.00", "N", "Y" },
                                            { "   1", "13233", " 125.00", "N", "Y" },
                                            { "   1", "13234", "1875.00", "N", "Y" },
                                            { "   1", "13235", "  55.00", "N", "Y" },
                                            { "   1", "13236", "1875.00", "N", "Y" },
                                            { "   1", "13237", "1875.00", "N", "Y" },
                                            { "   1", "13238", "1875.00", "N", "Y" },
                                            { "1006", "    6", "  60.00", "Y", "Y" },
                                            { "1006", "    7", " 550.00", "N", "N" },
                                            { "1111", "    8", "1000.00", "N", "Y" },
                                            { "1111", "    9", "  50.00", "Y", "N" },
                                            { "2222", "   10", "1000.00", "N", "Y" },
                                            { "2222", "   11", "  50.00", "Y", "N" },
                                            { "3333", "   16", "1000.00", "N", "Y" },
                                            { "3333", "   17", "  50.00", "Y", "N" },
                                            { "4444", "   12", "1000.00", "N", "Y" },
                                            { "4444", "   13", "  50.00", "Y", "N" },
                                            { "5555", "   18", "1000.00", "N", "Y" },
                                            { "5555", "   19", "  50.00", "Y", "N" },
                                            { "6666", "   14", "1000.00", "N", "Y" },
                                            { "6666", "   15", "  50.00", "Y", "N" }
                                         };
            return planChargesTable;
        }
    }
}
