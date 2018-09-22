using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonRestrictionRepository : IPersonRestrictionRepository
    {
        private string[,] restrs = {// id, studentId, restrictionId, startDate, endDate, severity, officeUseOnly (inverted - N = not visible to users = yes, office use only
                                    {"1", "S0001", "R0001", "12/01/2012", "12/02/2012", "1", "N"}, 
                                    {"2", "S0001", "R0002", "12/01/2012", "12/03/2012", "3", "N"}, 
                                    {"3", "S0001", "R0003", "12/01/2012", "12/05/2012", "",  "Y"}, 
                                    {"4", "S0002", "R0001", "12/01/2012", "12/11/2012", "1", "N"}, 
                                    {"5", "S0002", "R0002", "12/01/2012", "12/28/2012", "3", "N"},
                                    {"6", "S0003", "R0004", "",           "",           "",  ""},
                                   };

        public async Task<IEnumerable<PersonRestriction>> GetAsync(string personId, bool useCache)
        {
            return await Task.Run(() => Get());
        }

        public async Task<IEnumerable<PersonRestriction>> GetRestrictionsByIdsAsync(IEnumerable<string> ids)
        {
            return await Task.Run(() => Get());
        }

        public async Task<IEnumerable<PersonRestriction>> GetRestrictionsByStudentIdsAsync(IEnumerable<string> ids)
        {
            return await Task.Run(() => Get());
        }

        public IEnumerable<PersonRestriction> Get()
        {
            var restrictions = new List<Ellucian.Colleague.Domain.Base.Entities.PersonRestriction>();
            var items = restrs.Length / 7;
            for (int x = 0; x < items; x++)
            {
                DateTime d1;
                DateTime? d11 = (DateTime.TryParse(restrs[x,3], out d1) ? (DateTime?)d1 : null);
                DateTime d2;
                DateTime? d22 = (DateTime.TryParse(restrs[x,4], out d2) ? (DateTime?)d2 : null);
                int i1;
                int? i11 = (Int32.TryParse(restrs[x,5], out i1) ? (int?)i1 : null);

                PersonRestriction restriction1 = new PersonRestriction(restrs[x,0], restrs[x,1], restrs[x,2], d11, d22, i11, restrs[x,6]);
                restrictions.Add(restriction1);
            }
            return restrictions;
        }

    }
}
