using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArDepositsRepository
    {
        private static Collection<ArDeposits> _arDeposits = new Collection<ArDeposits>();
        public static Collection<ArDeposits> ArDeposits
        {
            get
            {
                if (_arDeposits.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arDeposits;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] depositsData = {
                                          {"101", "0000001", "01/01/2012", "TUI ", " 1000.00", "1001"},
                                          {"102", "0000001", "02/01/2012", "TUI ", " 1000.00", "1001"},
                                          {"103", "0000001", "02/01/2012", "TUI ", "-1000.00", "1001"},
                                          {"104", "0000001", "02/01/2012", "TUI ", " 1000.00", "1001"},
                                          {"105", "0000001", "03/01/2012", "TUI ", " 1000.00", "1001"},
                                          {"106", "0000001", "04/01/2012", "TUI ", " 1000.00", "1001"},
                                          {"107", "0000001", "05/01/2012", "ROOM", "  500.00", "1006"},
                                          {"108", "0000002", "03/01/2012", "TUI ", " 5000.00", "1002"},
                                          {"109", "0000002", "03/01/2012", "ROOM", "  500.00", "1007"},
                                          {"110", "0000003", "02/01/2012", "TUI ", " 2000.00", "1003"},
                                          {"111", "0000003", "02/01/2012", "ROOM", "  500.00", "1008"},
                                          {"112", "0000003", "03/01/2012", "TUI ", " 2000.00", "1003"},
                                          {"113", "0000003", "04/01/2012", "TUI ", " 1000.00", "1003"},
                                          {"114", "0000004", "04/01/2012", "TUI ", " 5000.00", "1004"},
                                          {"115", "0000005", "05/01/2012", "ROOM", "  500.00", "1010"}
                                      };
            int arDepositCount = depositsData.Length / 6;

            for (int i = 0; i < arDepositCount; i++)
            {
                string id = depositsData[i, 0].TrimEnd();
                string personId = depositsData[i, 1].TrimEnd();
                DateTime date = DateTime.Parse(depositsData[i, 2].Trim());
                string depositType = depositsData[i, 3].TrimEnd();
                decimal amount = Decimal.Parse(depositsData[i, 4].Trim());
                string depositDueId = depositsData[i, 5].Trim();

                ArDeposits arDeposit = new ArDeposits()
                {
                    Recordkey = id,
                    ArdPersonId = personId,
                    ArdDate = date,
                    ArdDepositType = depositType,
                    ArdTerm = "2012/FA",
                    ArdDepositsDue = depositDueId
                };
                if (amount > 0)
                {
                    arDeposit.ArdAmt = amount;
                    arDeposit.ArdReversalAmt = null;
                }
                else
                {
                    arDeposit.ArdReversalAmt = -amount;
                    arDeposit.ArdAmt = null;
                }
                _arDeposits.Add(arDeposit);
            }
        }
    }
}
