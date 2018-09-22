using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArPayPlanItemsRepository
    {
        private static Collection<ArPayPlanItems> _arPayPlanItems = new Collection<ArPayPlanItems>();
        public static Collection<ArPayPlanItems> ArPayPlanItems
        {
            get
            {
                if (_arPayPlanItems.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arPayPlanItems;
            }
        }

        private static void GenerateDataContracts()
        {
            var planItemData = GetPlanScheduleData();
            for (int i = 0; i < planItemData.Length / 4; i++)
            {
                DateTime outDate;
                string itemId = planItemData[i, 0].Trim();
                string planId = planItemData[i, 1].Trim();
                DateTime dueDate = DateTime.TryParse(planItemData[i, 2].Trim(), out outDate) ? outDate : default(DateTime);
                decimal amount = Convert.ToDecimal(planItemData[i, 3].Trim());
                _arPayPlanItems.Add(new ArPayPlanItems()
                    {
                        Recordkey = itemId,
                        ArpliPayPlan = planId,
                        ArpliDueDate = dueDate,
                        ArpliAmt = amount,
                        ArpliAmountPaid = itemId == "2222" ? amount : 0m
                    });
            }
        }

        private static string[,] GetPlanScheduleData()
        {
            string[,] planScheduleTable = {
                                              // ID Plan ID    Due Date    Amount
                                              { " 1", "   1", "11/19/2021", "2062.00" },
                                              { " 2", "   1", "01/31/2022", "1358.00" },
                                              { " 3", "   1", "02/28/2022", "1358.00" },
                                              { " 4", "   1", "03/31/2022", "1358.00" },
                                              { " 5", "   1", "04/30/2022", "1358.00" },
                                              { " 6", "1006", "03/15/2024", " 115.00" },
                                              { " 7", "1006", "04/01/2024", " 165.00" },
                                              { " 8", "1006", "04/08/2024", " 165.00" },
                                              { " 9", "1006", "04/15/2024", " 165.00" },
                                              { "10", "1111", "12/31/2014", " 300.00" },
                                              { "11", "1111", "01/07/2015", " 250.00" },
                                              { "12", "1111", "01/14/2015", " 250.00" },
                                              { "13", "1111", "01/21/2015", " 250.00" },
                                              { "14", "2222", "11/14/2014", " 300.00" },
                                              { "15", "2222", "11/28/2015", " 250.00" },
                                              { "16", "2222", "12/12/2014", " 250.00" },
                                              { "17", "2222", "12/26/2014", " 250.00" },
                                              { "26", "3333", "11/14/2014", " 300.00" },
                                              { "27", "3333", "11/28/2015", " 250.00" },
                                              { "28", "3333", "12/12/2014", " 250.00" },
                                              { "29", "3333", "12/26/2014", " 250.00" },
                                              { "18", "4444", "11/14/2014", " 300.00" },
                                              { "19", "4444", "11/28/2015", " 250.00" },
                                              { "20", "4444", "12/12/2014", " 250.00" },
                                              { "21", "4444", "12/26/2014", " 250.00" },
                                              { "30", "5555", "11/14/2014", " 300.00" },
                                              { "31", "5555", "11/28/2015", " 250.00" },
                                              { "32", "5555", "12/12/2014", " 250.00" },
                                              { "33", "5555", "12/26/2014", " 250.00" },
                                              { "22", "6666", "11/14/2014", " 300.00" },
                                              { "23", "6666", "11/28/2015", " 250.00" },
                                              { "24", "6666", "12/12/2014", " 250.00" },
                                              { "25", "6666", "12/26/2014", " 250.00" }
                                          };
            return planScheduleTable;
        }
    }
}
