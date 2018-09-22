using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.DataContracts;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public static class TestDistributionsRepository
    {
        private static readonly Collection<Distributions> _distributions = new Collection<Distributions>();

        public static Collection<Distributions> Distributions
        {
            get
            {
                if (_distributions.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _distributions;
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

                var record = new Distributions() { Recordkey = code, DistrDescription = desc, RecordGuid = Guid.NewGuid().ToString() };
                _distributions.Add(record);
            }
        }

        private static string[,] GetData(out int rowSize)
        {
            string[,] dataTable =
            { 
                {"BANK ", "Payment/Credit on Account     "},
                {"TRAV ", "Travel Advance Reconciliation "},
                {"WEBA ", "WebAdvisor Distribution A     "},
                {"WEBB ", "WebAdvisor Distribution B     "},
                {"WEBC ", "WebAdvisor Distribution C     "},
                {"WEBD ", "WebAdvisor Distribution D     "},
                {"WEBE ", "WebAdvisor Distribution E     "},
                {"WEBF ", "WebAdvisor Distribution F     "},
                {"WEBG ", "WebAdvisor Distribution G     "}
            };
            rowSize = 2;
            return dataTable;
        }
    }
}
