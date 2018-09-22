// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArDepositsDueRepository
    {
        private static Collection<ArDepositsDue> _arDepositsDue = new Collection<ArDepositsDue>();
        public static Collection<ArDepositsDue> ArDepositsDue
        {
            get
            {
                if (_arDepositsDue.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arDepositsDue;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] depositsDueData = {
                                             {"1001", "0000001", DateTime.Today.AddYears(-4).ToShortDateString(), "TUIFE", "5000.00", "2012/FA"},
                                             {"1002", "0000002", DateTime.Today.AddYears(-3).ToShortDateString(), "DAMAG", "5000.00", "2013/SP"},
                                             {"1003", "0000003", DateTime.Today.AddYears(-2).ToShortDateString(), "HRPPB", "5000.00", "2013/FA"},
                                             {"1004", "0000004", DateTime.Today.AddYears(-3).ToShortDateString(), "HRPPB", "5000.00", "2015/SP"},
                                             {"1005", "0000005", DateTime.Today.AddMonths(2).ToShortDateString(), "ROOMS", "5000.00", "2014/FA"},
                                             {"1006", "0000001", DateTime.Today.AddMonths(2).ToShortDateString(), "ROOMS", "500.00",  "2012/FA"},
                                             {"1007", "0000002", DateTime.Today.AddYears(-1).ToShortDateString(), "MEALS", "500.00",  "2013/SP"},
                                             {"1008", "0000003", DateTime.Today.AddYears(-2).ToShortDateString(), "DAMAG", "500.00",  "2013/FA"},
                                             {"1009", "0000004", DateTime.Today.AddYears(-1).ToShortDateString(), "MEALS", "500.00",  "2015/SP"},
                                             {"1010", "0000005", DateTime.Today.AddYears(-4).ToShortDateString(), "TUIFE", "500.00",  "ABC123 "},
                                             {"1011", "0000004", DateTime.Today.AddYears(-3).ToShortDateString(), "TUIFE", "500.00",  "2016/SP"},
                                             {"1012", "0000895", DateTime.Today.AddYears(-1).ToShortDateString(), "MEALS", "500.00",  "2014/FA"},
                                             {"1013", "0000895", DateTime.Today.AddYears(-4).ToShortDateString(), "TUIFE", "500.00",  "2014/FA"},
                                             {"1014", "0000895", DateTime.Today.AddYears(-3).ToShortDateString(), "DAMAG", "500.00",  "2015/SP"},                                         };

            int arDepositDueCount = depositsDueData.Length / 6;

            for (int i = 0; i < arDepositDueCount; i++)
            {
                string id = depositsDueData[i, 0].TrimEnd();
                string personId = depositsDueData[i, 1].TrimEnd();
                DateTime date = DateTime.Parse(depositsDueData[i, 2].TrimEnd());
                string depositType = depositsDueData[i, 3].TrimEnd();
                decimal amount = Decimal.Parse(depositsDueData[i, 4].TrimEnd());
                string term = depositsDueData[i, 5].TrimEnd();

                _arDepositsDue.Add(new ArDepositsDue()
                {
                    Recordkey = id,
                    ArddPersonId = personId,
                    ArddDueDate = date,
                    ArddDepositType = depositType,
                    ArddAmount = amount,
                    ArddTerm = term
                });
            }
        }
    }
}
