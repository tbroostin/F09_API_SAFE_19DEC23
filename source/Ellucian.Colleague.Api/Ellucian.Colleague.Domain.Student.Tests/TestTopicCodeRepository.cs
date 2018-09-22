using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestTopicCodeRepository
    {
            
        private string[,] topicCodes = {
                                        //CODE   DESCRIPTION
                                        {"TC1", "Topic Code 1"},
                                        {"TC2", "Topic Code 2"},
                                        {"TC3", "Topic Code 3"},
                                        {"TC4", "Topic Code 4"}
                                      };
        private string[] topicGuids = { "00000000-0000-0000-0000-000000000000",
                                        "00000000-0000-0000-0000-000000000000",
                                        "00000000-0000-0000-0000-000000000000",
                                        "00000000-0000-0000-0000-000000000000"
                                       };

        public IEnumerable<TopicCode> Get()
        {
            var acadLevels = new List<TopicCode>();

            // There are 2 fields for each topicCode in the array
            var items = topicCodes.Length / 2;

            for (int x = 0; x < items; x++)
            {
                acadLevels.Add(new TopicCode(topicGuids[x], topicCodes[x, 0], topicCodes[x, 1]));
            }
            return acadLevels;
        }
    }
}
