using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.DataContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public class TestSpecialDaysRepository
    {
        public Collection<DataContracts.CampusSpecialDay> specialDayRecords = new Collection<DataContracts.CampusSpecialDay>()
        {
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "1",
                CmsdCampusCalendar = "HOLIDAY",
                CmsdDesc = "New Years Day",
                CmsdType = "HO",
                CmsdStartDate = new DateTime(2018, 1, 1),
                CmsdEndDate = new DateTime(2018, 1, 1)
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "2",
                CmsdCampusCalendar = "HOLIDAY",
                CmsdDesc = "MLK Day",
                CmsdType = "HO",
                CmsdStartDate = new DateTime(2018, 1, 15),
                CmsdEndDate = new DateTime(2018, 1, 15)
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "3",
                CmsdCampusCalendar = "SPECIALDAYS",
                CmsdDesc = "Mother's Day",
                CmsdType = "HO",
                CmsdStartDate = new DateTime(2018, 5, 13),
                CmsdEndDate = new DateTime(2018, 5, 13)
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "4",
                CmsdCampusCalendar = "SPECIALDAYS",
                CmsdDesc = "Cinco De Mayo",
                CmsdType = "HO",
                CmsdStartDate = new DateTime(2018, 5, 5),
                CmsdEndDate = new DateTime(2018, 5, 5)
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "5",
                CmsdCampusCalendar = "SNOWDAYS",
                CmsdDesc = "February Storm",
                CmsdType = "SNOW",
                CmsdStartDate = new DateTime(2018, 2, 14),
                CmsdStartTime = new DateTime(2018, 2, 14, 8, 0, 0),
                CmsdEndDate = new DateTime(2018, 2, 15),
                CmsdEndTime = new DateTime(2018, 2, 15, 17, 0, 0),
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "6",
                CmsdCampusCalendar = "CAL",
                CmsdDesc = "Calendar day",
                CmsdType = "HO",
                CmsdStartDate = new DateTime(2018, 2, 14),
                CmsdStartTime = new DateTime(2018, 2, 14, 8, 0, 0),
                CmsdEndDate = new DateTime(2018, 2, 14),
                CmsdEndTime = new DateTime(2018, 2, 14, 12, 0, 0),
            },
            new DataContracts.CampusSpecialDay()
            {
                Recordkey = "7",
                CmsdCampusCalendar = "CAL",
                CmsdDesc = "Registration Day",
                CmsdType = "REG",
                CmsdStartDate = new DateTime(2018, 4, 1),
                CmsdStartTime = new DateTime(2018, 4, 1, 8, 0, 0),
                CmsdEndDate = new DateTime(2018, 4, 1),
                CmsdEndTime = new DateTime(2018, 4, 1, 17, 0, 0),
            }
        };

        public List<string> CampusSpecialDayIds
        {
            get
            {
                return specialDayRecords == null ? new List<string>() : specialDayRecords.Select(sd => sd.Recordkey).ToList();
            }
        }

        public ApplValcodes CalendarDayTypes = new ApplValcodes()
        {
            Recordkey = "CALENDAR.DAY.TYPES",
            ValsEntityAssociation = new List<ApplValcodesVals>()
            {
                new ApplValcodesVals()
                {
                    ValInternalCodeAssocMember = "HO",
                    ValActionCode1AssocMember = "HO",
                },
                new ApplValcodesVals()
                {
                    ValInternalCodeAssocMember = "SNOW",
                },
                new ApplValcodesVals()
                {
                    ValInternalCodeAssocMember = "REG",
                }
            }
        };
    }
}
