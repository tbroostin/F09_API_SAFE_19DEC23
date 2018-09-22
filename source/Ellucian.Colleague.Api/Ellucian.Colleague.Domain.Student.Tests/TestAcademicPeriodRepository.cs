using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAcademicPeriodRepository {

        private const int CODE = 0;
        private const int DESC = 1;
        private const int SDATE = 2;
        private const int EDATE = 3;
        private const int RYEAR = 4;
        private const int SEQ = 5;
        private const int RTERM = 6;
        private const int PREID = 7;
        private const int PAID = 8;
        private const int GUID = 9;

        private string[,] academicPeriods = {   // Code,           Desc,                 StDate,                EndDate,         RepYear, Sequence, RepTerm,                PreId,                                  ParentId,                                   Guid
                                          {"2000RSU",   "Summer Reporting Term", "2000-05-22T00:00:00Z", "2000-08-16T00:00:00Z",  "2000",     "1", "term", "38e0e683-641d-420a-bafa-fe080006f520",  "d1ef94c1-759c-4870-a3f4-34065bb522fe", "d1ef94c1-759c-4870-a3f4-34065bb522fe"},
                                          {"2000/S1",   "Summer Term 1", "2000-05-22T00:00:00Z", "2000-07-01T00:00:00Z",  "2000",     "1", "subterm", null,  "d1ef94c1-759c-4870-a3f4-34065bb522fe", "28a52594-1f3e-44e1-9f8c-26cb7aa6a7aa"},
                                          {"2000CS1",   "Continuing Ed Summer 1", "2000-05-29T00:00:00Z", "2000-07-07T00:00:00Z",  "2000",     "1", "term", "d1ef94c1-759c-4870-a3f4-34065bb522fe",  "8965c1ec-367e-4685-a362-700e47671e5e", "8965c1ec-367e-4685-a362-700e47671e5e"},
                                      };

        public IEnumerable<AcademicPeriod> Get() {
            var acadPds = new List<AcademicPeriod>();
            var items = academicPeriods.Length / (GUID + 1);
            for (int x = 0; x < items; x++) {
                DateTime startDate; DateTime.TryParse(academicPeriods[x, SDATE], out startDate);
                DateTime endDate; DateTime.TryParse(academicPeriods[x, SDATE], out endDate);
                int repYr; int.TryParse(academicPeriods[x, RYEAR], out repYr);
                int sequence; int.TryParse(academicPeriods[x, SEQ], out sequence);
                acadPds.Add(new AcademicPeriod(academicPeriods[x, GUID],
                                           academicPeriods[x, CODE],
                                           academicPeriods[x, DESC],
                                           startDate,
                                           endDate,
                                           repYr,
                                           sequence,
                                           academicPeriods[x, RTERM],
                                           academicPeriods[x, PREID],
                                           academicPeriods[x, PAID],
                                           null));
            }
            return acadPds;
        }

    }
}
