// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
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
    public class TestRegistrationPriorityRepository : IRegistrationPriorityRepository
    {

        private Dictionary<string, List<RegistrationPriority>> priorities = new Dictionary<string, List<RegistrationPriority>>();


        public async Task<IEnumerable<Domain.Student.Entities.RegistrationPriority>> GetAsync(string studentId)
        {
            if (priorities.Count() == 0) { Populate(); }
            return priorities[studentId];
        }

        private void Populate()
        {
            priorities = new Dictionary<string, List<RegistrationPriority>>();
            string[,] priorityData = {
                                          // Id, StudentId, TermCode, Priority, Start, End
                                         { "1", "0000001", "2014/FA", "1", "2014-08-01T08:00:00", "2014-08-01T08:59:59" },
                                         { "2", "0000002", "2014/FA", "2", "2014-08-01T09:00:00", "2014-08-01T09:59:59" },
                                         { "3", "0000002", "2014/FA", "4", "2014-08-01T10:00:00", "2014-08-01T10:59:59" },
                                         { "4", "0000003", "",        "1", "",                    ""                    },
                                         { "5", "0000004", null,      "1", "",                    ""                    },
                                         { "6", "0000005", null,      null, "",                   ""                    }
                                     };
            int priorityCnt = priorityData.Length / 6;
            for (int x = 0; x < priorityCnt; x++)
            {
                var id = priorityData[x, 0];
                var studentId = priorityData[x, 1];
                var termCode = priorityData[x, 2];
                long? priority = null;
                if (!string.IsNullOrEmpty(priorityData[x, 3])) { priority = long.Parse(priorityData[x, 3].TrimEnd()); }
                DateTime? start = null;
                if (!string.IsNullOrEmpty(priorityData[x, 4])) { start = DateTime.Parse(priorityData[x, 4]); }
                DateTime? end = null;
                if (!string.IsNullOrEmpty(priorityData[x, 5])) { end = DateTime.Parse(priorityData[x, 5]); }
                RegistrationPriority regPri = new RegistrationPriority(id, studentId, termCode, start, end);
                if (!(priorities.ContainsKey(studentId)))
                {
                    priorities[studentId] = new List<RegistrationPriority>() { regPri };
                }
                else
                {
                    priorities[studentId].Add(regPri);
                }
                
            }
        }
    }
}

