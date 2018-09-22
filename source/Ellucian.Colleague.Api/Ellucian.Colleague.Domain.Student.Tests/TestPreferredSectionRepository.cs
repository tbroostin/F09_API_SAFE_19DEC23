// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestPreferredSectionRepository : IPreferredSectionRepository
    {
        public async Task<PreferredSectionsResponse> GetAsync(string studentId)
        {
            var stuPrefCrsSecs = BuildPreferredSectionRepository().Where(ps => ps.StudentId == studentId);
            List<PreferredSection> prefSecs = new List<PreferredSection>();
            foreach (var spcs in stuPrefCrsSecs)
            {
                prefSecs.Add(new PreferredSection(spcs.StudentId,spcs.SectionId,spcs.Credits));
            }
            return await Task.FromResult(new PreferredSectionsResponse(prefSecs, new List<PreferredSectionMessage>()));
        }

        public async Task<IEnumerable<PreferredSectionMessage>> UpdateAsync(string studentId, List<PreferredSection> sections)
        {
            List<PreferredSectionMessage> msgs = new List<PreferredSectionMessage>();
            return await Task.FromResult(msgs);
        }

        public async Task<IEnumerable<PreferredSectionMessage>> DeleteAsync(string studentId, string sectionId)
        {
            List<PreferredSectionMessage> msgs = new List<PreferredSectionMessage>();
            return await Task.FromResult(msgs);
        }

        private IEnumerable<PreferredSection> BuildPreferredSectionRepository()
        {
            var sections = new List<PreferredSection>();
            string[,] prefSecData = 
            {
                // student, section, credits (nullable)
                {"STU001", "SECT001", "3.00"},
                {"STU002", "SECT002",  null },
                {"STU003", "SECT001", "1.00"},
                {"STU003", "SECT002", "3.00"},
                {"STU003", "SECT003", "2.00"}
            };
            int columns = 3;
            int count = prefSecData.Length / columns;
            for (int x = 0; x < count; x++)
            {
                decimal? credits = null;
                if (!string.IsNullOrEmpty(prefSecData[x, 2]))
                {
                    credits = decimal.Parse(prefSecData[x, 2]);
                }
                PreferredSection ps = new PreferredSection(prefSecData[x, 0], prefSecData[x, 1], credits);
                sections.Add(ps);
            }
            return sections;
        }
    }
}
