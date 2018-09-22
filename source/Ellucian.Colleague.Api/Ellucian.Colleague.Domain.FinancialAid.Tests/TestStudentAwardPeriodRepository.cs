using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentAwardPeriodRepository
    {
        private string[,] studentAwardPeriods = {
                                                    //ID        YEAR    Award      Awdp    Amount  Status
                                                    {"0003914", "2012", "PELL",   "12/FA", "2775",  "A"},
                                                    {"0003914", "2012", "PELL",   "13/SP", "2081",  "A"},
                                                    {"0003914", "2012", "PELL",   "13/SU", "694",   "A"},
                                                    {"0003914", "2012", "UGTCH",  "12/FA", "1333",  "A"},
                                                    {"0003914", "2012", "UGTCH",  "13/SP", "1333",  "A"},
                                                    {"0003914", "2012", "UGTCH",  "13/SU", "1334",  "A"},

                                                    {"0003914", "2013", "PELL",   "13/FA", "3829",  "A" },
                                                    {"0003914", "2013", "PELL",   "14/SP", "2081",  "A"},
                                                    {"0003914", "2013", "PELL",   "14/SU", "694",   "A"},
                                                    {"0003914", "2013", "UGTCH",  "13/FA", "1333",  "A"},
                                                    {"0003914", "2013", "UGTCH",  "14/SP", "1333",  "A"},
                                                    {"0003914", "2013", "UGTCH",  "14/SU", "1334",  "E"},

                                                    {"0003914", "2014", "ZEBRA",  "14/FA", "2775",  "P"},
                                                    {"0003914", "2014", "ZEBRA",  "15/SP", "2081",  "P"},
                                                    {"0003914", "2014", "FWS",    "14/FA", "1333",  "P"},
                                                    {"0003914", "2014", "FWS",    "15/SP", "1333",  "P"},

                                                };

        /// <summary>
        /// Return a list of StudentAwardPeriod domain objects for a given studentId
        /// </summary>
        /// <param name="studentId">StudentId for which to return domain objects</param>
        /// <returns>List of StudentAwardPeriod domain objects with the given student id</returns>
        public IEnumerable<StudentAwardPeriod> Get(string studentId)
        {
            var studentAwardPeriodList = new List<StudentAwardPeriod>();

            //There are 6 fields for each StudentAwardPeriod in the array
            var items = studentAwardPeriods.Length / 6;

            for (int i = 0; i < items; i++)
            {
                studentAwardPeriodList.Add(new StudentAwardPeriod(studentAwardPeriods[i, 0], studentAwardPeriods[i, 1], studentAwardPeriods[i, 2], studentAwardPeriods[i, 3], Convert.ToDecimal(studentAwardPeriods[i, 4]), studentAwardPeriods[i, 5]));
            }

            return studentAwardPeriodList.FindAll(a => a.StudentId == studentId);
        }

        public StudentAwardPeriod AcceptStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod)
        {
            studentAwardPeriod.AwardStatus = "A";
            return studentAwardPeriod;
        }

        public StudentAwardPeriod RejectStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod)
        {
            studentAwardPeriod.AwardStatus = "R";
            return studentAwardPeriod;
        }
    }
}
