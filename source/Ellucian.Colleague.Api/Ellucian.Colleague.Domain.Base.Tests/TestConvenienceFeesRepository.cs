using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public static class TestConvenienceFeesRepository
    {
        private static readonly Collection<ConvenienceFees> _convenienceFees = new Collection<ConvenienceFees>();

        public static Collection<ConvenienceFees> ConvenienceFees
        {
            get
            {
                if (_convenienceFees.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _convenienceFees;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize = 0;
            string[,] inputData = GetData(out rowSize);
            for (int i = 0; i < inputData.Length / rowSize; i++)
            {
                string code = inputData[i, 0].Trim();
                string desc = inputData[i, 1].Trim();

                var record = new ConvenienceFees() { Recordkey = code, ConvfDescription = desc };
                _convenienceFees.Add(record);
            }
        }

        private static string[,] GetData(out int rowSize)
        {
            string[,] dataTable =
            { 
                {"CF1  ", "1% 1.00/10.00                 "},
                {"CF2  ", "1.5%  no min/max              "},
                {"CF3  ", "1% flat fee                   "},
                {"CF4  ", "2% flat fee                   "}
            };
            rowSize = 2;
            return dataTable;
        }
    }
}
