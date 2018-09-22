/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentDefaultAwardPeriodRepository : IStudentDefaultAwardPeriodRepository
    {
        
        public class DefaultAwardPeriods
        {
            public string studentId;
            public string awardYear;
            public List<string> awardPeriods;
        }

        public List<DefaultAwardPeriods> DefaultAwardPeriodList = new List<DefaultAwardPeriods>()
        {
            new DefaultAwardPeriods()
            {
                studentId = "0004791",
                awardYear = "2015",
                awardPeriods = new List<string>() {"15/FA","16/WI"}
            },
            new DefaultAwardPeriods()
            {
                studentId = "0004791",
                awardYear = "2014",
                awardPeriods = new List<string>() {"15/FA","16/WI"}
            },
            new DefaultAwardPeriods()
            {
                studentId = "0004791",
                awardYear = "2013",
                awardPeriods = new List<string>() {"15/FA","16/WI"}
            },
            new DefaultAwardPeriods()
            {
                studentId = "0004791",
                awardYear = "2012",
                awardPeriods = new List<string>() {"15/FA","16/WI"}
            },
            new DefaultAwardPeriods()
            {
                studentId = "0004791",
                awardYear = "2017",
                awardPeriods = new List<string>() {"15/FA","16/WI"}
            }
        };

        public class TestStudentAwardYear
        {
            public List<string> defaultAwardYears;
        }


        public List<string> defaultAwardYears = new List<string> { "2012","2013","2014", "2015", "2017" };


        #region SaAcyrData
        public class SaAcyrRecord
        {
            public string AwardYear;
            public string StudentId;
            public List<string> SaTerms; 
        }

        public List<SaAcyrRecord> SaAcyrRecordList = new List<SaAcyrRecord>()
        {
            new SaAcyrRecord()
            {
                AwardYear = "2015",
                StudentId = "0003915",
                SaTerms = new List<string>() {"15/FA", "16/WI"},
            },
            new SaAcyrRecord()
            {
                AwardYear = "2014",
                StudentId = "0003915",
                SaTerms = new List<string>() 
            },
        };
        #endregion

        public async Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId)) throw new ArgumentNullException("studentId");
            if (studentAwardYears == null || studentAwardYears.Count() == 0) throw new ArgumentNullException("studentAwardYears");

            var studentDefaultAwardPeriods = new List<StudentDefaultAwardPeriod>();
            foreach (var defaultAwardYear in defaultAwardYears)
            {
                    var defaultDataRecord = DefaultAwardPeriodList.FirstOrDefault(c => c.awardYear == defaultAwardYear);
                    if (defaultDataRecord != null)
                    {
                        
                        var defaultAwardPeriod = new StudentDefaultAwardPeriod(defaultDataRecord.studentId, defaultDataRecord.awardYear);
                        defaultAwardPeriod.DefaultAwardPeriods = defaultDataRecord.awardPeriods;
 
                        studentDefaultAwardPeriods.Add(defaultAwardPeriod);
                    }
            }
            return await Task.FromResult(studentDefaultAwardPeriods);
        }
    }
}
