using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestTestResultRepository
    {
        private Dictionary<string, TestResult> testResult = new Dictionary<string, TestResult>();

        public async Task<TestResult> GetAsync(string id)
        {
            return (await GetAsync(new List<string>() { id })).FirstOrDefault();
        }

        public async Task<IEnumerable<TestResult>> GetAsync()
        {
            if (testResult.Count() == 0) { Populate(); }
            return await GetAsync(testResult.Keys);
        }

        public async Task<IEnumerable<TestResult>> GetAsync(IEnumerable<string> ids)
        {
            return await Task.Run(() => DoGetMany(ids.Select(ii => ii)));
        }

        private IEnumerable<TestResult> DoGetMany(IEnumerable<string> ids)
        {

            if (testResult.Count() == 0) { Populate(); }

            ICollection<TestResult> results = new List<TestResult>();
            foreach (var id in ids)
            {
                if (testResult.Keys.Contains(id))
                {
                    results.Add(testResult[id]);
                }
            }

            return results;
        }

        private void Populate()
        {
            string[,] testresultdata = {

                //ID    Student ID  Code    Description                  Date Taken  Score    Percent Category Status/Date   
                {"10001","0000304","ACT-M","ACT - Math                ","2012-08-23" ,"20.33", "80   ","A","AC","2012-08-21"},
                {"10002","0000304","ACT-C","ACT - Composite           ","2011-12-19" ,"30   ", "70   ","A","AC","2012-08-21"},
                {"10003","0000304","SAT  ","SAT Testing               ","2012-05-23" ,"10.25", "50   ","A","NT","2012-08-21"},
                {"10004","0000305","CRS  ","Course Equivalency        ","2015-01-23" ,"67   ", "     ","P","W","2012-08-21"},  
                {"10005","0000305","READ ","Reading Proficiency       ","2015-08-23" ,"72   ", "     ","P","W","2012-08-21"},
                {"10006","0000306","ACT-V","ACT - Verbal              ","2016-01-23" ,"30.55", "     ","A","NC","2012-08-21"},
                {"10007","0000306","ACT-M","ACT - Math                ","2016-08-23" ,"20   ", "79   ","A","NC","2012-08-21"},
                {"10008","0000306","ACT-E","ACT - English             ","2030-01-23" ,"", "96   ","A","NC","2012-08-21"}
                };
            int testcnt = testresultdata.Length / 10;

            string[,] subtestdata = {
                                    {"MATH","20","86"},
                                    {"ENGL","22","89"},
                                    {"HIST","16","72"}
                                    };

            for (int x = 0; x < testcnt; x++)
            {
                var testId = testresultdata[x, 0].Trim();
                var testStudent = testresultdata[x, 1].Trim();
                var testCode = testresultdata[x, 2].Trim();
                var testDesc = testresultdata[x, 3].Trim();
                var testDate = testresultdata[x, 4].Trim();
                var testScore = testresultdata[x, 5].Trim();
                var testPercent = testresultdata[x, 6].Trim();
                var testCategory = testresultdata[x, 7].Trim();
                var testStatus = testresultdata[x, 8].Trim();
                var testStatusDate = testresultdata[x, 9].Trim();
                DateTime testDateTaken = DateTime.Parse(testDate);
                TestType testType = testCategory == "A" ? TestType.Admissions : TestType.Placement;
                TestResult newTestResult = new TestResult(testStudent, testCode, testDesc, testDateTaken, testType);
                if (!string.IsNullOrEmpty(testScore)) { newTestResult.Score = decimal.Parse(testScore); }
                if (!string.IsNullOrEmpty(testPercent)) { newTestResult.Percentile = int.Parse(testPercent); }
                newTestResult.StatusCode = testStatus;
                newTestResult.StatusDate = DateTime.Parse(testStatusDate);

                List<SubTestResult> newSubTestResult = new List<SubTestResult>();
                int subcnt = subtestdata.Length / 3;
                for (int xx = 0; xx < subcnt; xx++)
                {
                    string testSubTestCode = subtestdata[xx, 0].Trim();
                    string testSubTestScore = subtestdata[xx, 1].Trim();
                    string testSubTestPercent = subtestdata[xx, 2].Trim();
                    SubTestResult subTestResult = new SubTestResult(testSubTestCode, testSubTestCode, testDateTaken);
                    if (!string.IsNullOrEmpty(testSubTestScore)) { subTestResult.Score = decimal.Parse(testSubTestScore); }
                    if (!string.IsNullOrEmpty(testSubTestPercent)) { subTestResult.Percentile = int.Parse(testSubTestPercent); }
                    newSubTestResult.Add(subTestResult);
                }
                newTestResult.SubTests = newSubTestResult;
                testResult.Add(testresultdata[x, 0], newTestResult);
            }
        }
    }
}
