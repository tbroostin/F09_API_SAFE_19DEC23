using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestIntlParamsRepository
    {
        private static Collection<IntlParams> _intlParams = new Collection<IntlParams>();
        public static Collection<IntlParams> IntlParams
        {
            get
            {
                if (_intlParams.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _intlParams;
            }
        }

        /// <summary>
        /// Performs data setup for international parameters to be used in tests
        /// </summary>
        private static void GenerateDataContracts()
        {
            string[,] intlParamsData = GetIntlParamsData();
            int intlParamsCount = intlParamsData.Length / 3;
            for (int i = 0; i < intlParamsCount; i++)
            {
                // Parse out the data
                string id = intlParamsData[i, 0].Trim();
                string hostShortDateFormat = intlParamsData[i, 1].Trim();
                string hostDateDelimiter = intlParamsData[i, 2].Trim();

                IntlParams intlParams = new IntlParams()
                {
                    Recordkey = id,
                    HostShortDateFormat = hostShortDateFormat,
                    HostDateDelimiter = hostDateDelimiter
                };
                _intlParams.Add(intlParams);
            }
        }

        /// <summary>
        /// Gets international parameters raw data
        /// </summary>
        /// <returns>String array of international parameters data</returns>
        private static string[,] GetIntlParamsData()
        {
            string[,] calendarSchedulesData =   {   //ID                  Date Format    Delimiter
                                                    {"INTERNATIONAL"    ,"MDY",         "/"},
                                                    {"INTERNATIONAL.CAN","YMD",         "/"},
                                                    {"INTERNATIONAL.USA","MDY",         "/"}
                                                };
            return calendarSchedulesData;
        }
    }
}
