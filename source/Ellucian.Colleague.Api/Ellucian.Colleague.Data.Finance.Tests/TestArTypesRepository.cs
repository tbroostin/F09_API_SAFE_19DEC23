using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArTypesRepository
    {
        private static readonly Collection<ArTypes> arTypes = new Collection<ArTypes>();
        public static Collection<ArTypes> ArTypes
        {
            get
            {
                if (arTypes.Count == 0)
                {
                    GenerateDataContracts();
                }
                return arTypes;
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

                var record = new ArTypes { Recordkey = code, ArtDesc = desc };
                arTypes.Add(record);
            }

        }

        private static string[,] GetData(out int rowSize)
        {
            string[,] arTypesString = {
                                        {"01", "Student Receivable            "},
                                        {"02", "Continuing Ed Receivable      "},
                                        {"03", "Sponsor Receivable            "},
                                        {"04", "Travel Advance Receivable     "},
                                        {"05", "Salary Advance Receivable     "},
                                        {"06", "Miscellaneous Receivable      "},
                                        {"07", "Employee Receivable           "},
                                        {"08", "Bad Debt Receivable           "},
                                        {"09", "HR Employee Arrears Receivable"}
                                    };
            rowSize = 2;
            return arTypesString;
        }
    }
}
