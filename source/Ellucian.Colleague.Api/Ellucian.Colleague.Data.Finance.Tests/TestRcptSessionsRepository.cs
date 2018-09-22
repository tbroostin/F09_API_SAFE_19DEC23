using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestRcptSessionsRepository
    {
        private static Collection<RcptSessions> _rcptSessions;
        public static Collection<RcptSessions> RcptSessions
        {
            get
            {
                if (_rcptSessions == null)
                {
                    GenerateDataContracts();
                }
                return _rcptSessions;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize;
            var inputData = GetRcptSessionsData(out rowSize);

            _rcptSessions = new Collection<RcptSessions>();
            for (int i = 0; i < inputData.Length / rowSize; i++)
            {
                var recordId = inputData[i, 0].Trim();
                var status = inputData[i, 1].Trim();
                var cashier = inputData[i, 2].Trim();
                var rcptDate = DateTime.Parse(inputData[i, 3].Trim());
                var isEComm = inputData[i, 4].Trim();
                var location = inputData[i, 5].Trim();
                var startDate = DateTime.Parse(inputData[i, 6].Trim());
                var startTime = DateTime.Parse(inputData[i, 7].Trim());
                var endDate = DateTime.Parse(inputData[i, 8].Trim());
                var endTime = DateTime.Parse(inputData[i, 9].Trim());

                var session = new RcptSessions()
                {
                    Recordkey = recordId,
                    RcptsStatus = status,
                    RcptSessionsAddopr = cashier,
                    RcptsDate = rcptDate,
                    RcptsEcommerceFlag = isEComm,
                    RcptsLocation = location,
                    RcptSessionsAdddate = startDate,
                    RcptsStartTime = startTime,
                    RcptsEndDate = endDate,
                    RcptsEndTime = endTime
                };

                _rcptSessions.Add(session);
            }
        }

        private static string[,] GetRcptSessionsData(out int rowSize)
        {
            string[,] inputData =
            {   //  Record ID  Status Cashier    Rcpt Date   eComm Location Start Date   Start Time   End Date      End Time
                { "        10", "V", "0000012", "2014-03-01", "N", "MC  ", "2014-03-01", "00:00:00", "2014-03-01", "23:59:59" },
                { "       100", "O", "0000362", "2014-03-11", "Y", "MC  ", "2014-03-11", "00:00:00", "2014-03-11", "23:59:59" },
                { "       200", "C", "0000362", "2014-03-12", "Y", "MC  ", "2014-03-12", "00:00:00", "2014-03-12", "23:59:59" },
                { "       300", "C", "0000362", "2014-03-13", "Y", "MC  ", "2014-03-13", "00:00:00", "2014-03-13", "23:59:59" },
                { "       400", "O", "0000362", "2014-03-14", "Y", "MC  ", "2014-03-14", "00:00:00", "2014-03-14", "23:59:59" },
                { "       500", "R", "0000362", "2014-03-15", "Y", "MC  ", "2014-03-15", "00:00:00", "2014-03-15", "23:59:59" },
                { "       600", "R", "0000362", "2014-03-16", "Y", "MC  ", "2014-03-16", "00:00:00", "2014-03-16", "23:59:59" },
                { "       700", "O", "0000362", "2014-03-17", "Y", "MC  ", "2014-03-17", "00:00:00", "2014-03-17", "23:59:59" },
                { "       800", "R", "0000362", "2014-03-18", "Y", "MC  ", "2014-03-18", "00:00:00", "2014-03-18", "23:59:59" },
                { "       900", "R", "0000362", "2014-03-19", "Y", "MC  ", "2014-03-19", "00:00:00", "2014-03-19", "23:59:59" }
            };

            rowSize = 10;
            return inputData;
        }
    }
}
