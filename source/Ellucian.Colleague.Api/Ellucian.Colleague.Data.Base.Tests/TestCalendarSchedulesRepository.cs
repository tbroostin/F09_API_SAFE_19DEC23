using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public  class TestCalendarSchedulesRepository
    {
        private  Collection<CalendarSchedules> _calendarSchedules = new Collection<CalendarSchedules>();
        public  Collection<CalendarSchedules> CalendarSchedules
        {
            get
            {
                if (_calendarSchedules.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _calendarSchedules;
            }
        }

        /// <summary>
        /// Performs data setup for calendar schedules to be used in tests
        /// </summary>
        private  void GenerateDataContracts()
        {
            string[,] calendarSchedulesData = GetCalendarSchedulesData();
            int calendarSchedulesCount = calendarSchedulesData.Length / 3;
            for (int i = 0; i < calendarSchedulesCount; i++)
            {
                // Parse out the data
                string id = calendarSchedulesData[i, 0].Trim();
                string calendar = calendarSchedulesData[i, 1].Trim();
                DateTime date = DateTime.Parse(calendarSchedulesData[i, 2].Trim());

                CalendarSchedules calendarSchedule = new CalendarSchedules()
                {
                    Recordkey = id,
                    CalsCampusCalendar = calendar,
                    CalsDate = date
                };
                _calendarSchedules.Add(calendarSchedule);
            }
        }

        /// <summary>
        /// Gets calendar schedules raw data
        /// </summary>
        /// <returns>String array of calendar schedules data</returns>
        public  string[,] GetCalendarSchedulesData()
        {
            string[,] calendarSchedulesData = { //ID      Cal    Date
                                                {"51118","MAIN","01/01/2007"},
                                                {"51119","MAIN","01/15/2007"},
                                                {"51120","MAIN","02/19/2007"},
                                                {"51121","MAIN","03/26/2007"},
                                                {"51122","MAIN","05/28/2007"},
                                                {"51123","MAIN","07/04/2007"},
                                                {"51124","MAIN","09/03/2007"},
                                                {"51125","MAIN","10/08/2007"},
                                                {"51126","MAIN","11/12/2007"},
                                                {"51127","MAIN","11/19/2007"},
                                                {"51128","MAIN","12/25/2007"},
                                                {"51129","MAIN","01/01/2008"},
                                                {"51130","MAIN","01/21/2008"},
                                                {"51131","MAIN","02/18/2008"},
                                                {"51132","MAIN","03/24/2008"},
                                                {"51133","MAIN","03/25/2008"},
                                                {"51134","MAIN","03/26/2008"},
                                                {"51135","MAIN","03/27/2008"},
                                                {"51136","MAIN","03/28/2008"},
                                                {"51137","MAIN","05/26/2008"},
                                                {"51138","MAIN","07/04/2008"},
                                                {"51139","MAIN","09/01/2008"},
                                                {"51140","MAIN","10/13/2008"},
                                                {"51141","MAIN","11/11/2008"},
                                                {"51142","MAIN","11/24/2008"},
                                                {"51143","MAIN","12/25/2008"},
                                                {"51144","MAIN","01/01/2009"},
                                                {"51145","MAIN","01/19/2009"},
                                                {"51146","MAIN","02/16/2009"},
                                                {"51147","MAIN","03/23/2009"},
                                                {"51148","MAIN","03/24/2009"},
                                                {"51149","MAIN","03/25/2009"},
                                                {"51150","MAIN","03/26/2009"},
                                                {"51151","MAIN","03/27/2009"},
                                                {"51152","MAIN","05/25/2009"},
                                                {"51153","MAIN","07/03/2009"},
                                                {"51154","MAIN","09/07/2009"},
                                                {"51155","MAIN","10/12/2009"},
                                                {"51156","MAIN","11/11/2009"},
                                                {"51157","MAIN","11/23/2009"},
                                                {"51158","MAIN","12/25/2009"},
                                                {"51159","MAIN","01/01/2010"},
                                                {"51160","MAIN","01/18/2010"},
                                                {"51161","MAIN","02/15/2010"},
                                                {"51162","MAIN","05/31/2010"},
                                                {"51163","MAIN","07/05/2010"},
                                                {"51164","MAIN","09/06/2010"},
                                                {"51165","MAIN","10/11/2010"},
                                                {"51166","MAIN","11/11/2010"},
                                                {"51167","MAIN","11/22/2010"},
                                                {"596675","2001","01/01/2001"},
                                                {"596676","2001","01/15/2001"},
                                                {"596677","2001","02/19/2001"},
                                                {"596678","2001","03/12/2001"},
                                                {"596679","2001","03/13/2001"},
                                                {"596680","2001","03/14/2001"},
                                                {"596681","2001","03/15/2001"},
                                                {"596682","2001","03/16/2001"},
                                                {"596683","2001","03/19/2001"},
                                                {"596684","2001","03/20/2001"},
                                                {"596685","2001","03/21/2001"},
                                                {"596686","2001","03/22/2001"},
                                                {"596687","2001","03/23/2001"},
                                                {"596688","2001","05/28/2001"},
                                                {"596689","2001","09/03/2001"},
                                                {"596690","2001","10/08/2001"},
                                                {"596691","2001","10/31/2001"},
                                                {"596692","2001","11/22/2001"},
                                                {"596693","2001","11/23/2001"},
                                                {"596694","2001","12/24/2001"},
                                                {"596695","2001","12/25/2001"},
                                                {"596696","2001","12/26/2001"},
                                                {"596697","2001","12/27/2001"},
                                                {"596698","2001","12/28/2001"},
                                                {"596699","2001","12/25/2001"},
                                                {"596700","2001","10/24/2014"},
                                                         };
            return calendarSchedulesData;
        }
    }
}
