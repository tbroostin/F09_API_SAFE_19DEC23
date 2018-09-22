using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArDepositTypesRepository
    {
        private static Collection<ArDepositTypes> _arDepositTypes = new Collection<ArDepositTypes>();
        public static Collection<ArDepositTypes> ArDepositTypes
        {
            get
            {
                if (_arDepositTypes.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arDepositTypes;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] inputData = GetData();
            for (int i = 0; i < inputData.Length / 3; i++)
            {
                string code = inputData[i, 0].Trim();
                string desc = inputData[i, 1].Trim();

                ArDepositTypes record = new ArDepositTypes() { Recordkey = code, ArdtDesc = desc };
                _arDepositTypes.Add(record);
            }
        }

        private static string[,] GetData()
        {
            string[,] arDepositTypesString = {
                                        {"DAMAG", "Residence Life Damage Deposit"},
                                        {"HRPPB", "Hr Prepaid Benefit Deposits  "},
                                        {"MEALS", "Meal Plan Deposit            "},
                                        {"ROOMS", "Room Deposit                 "},
                                        {"TUIFE", "Tuition/Fees Deposit         "}
                                    };
            return arDepositTypesString;
        }
    }
}
