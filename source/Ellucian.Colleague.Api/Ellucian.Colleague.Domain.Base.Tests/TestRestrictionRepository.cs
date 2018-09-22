using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Web.Http.TestUtil
{
    public class TestRestrictionRepository
    {
        private string[,] restrictions = { // code,   desc,           sev,  off only, title,              details,                    app,  lnk,    form,   label,           mtxt, link
                                             {"ACC30","ACC past due", "1",  "N",      "Account Past Due", "Your Account is past due", "ST", null,   "WMPT", "Click to pay",  "N", "http://www.school.edu/wmpt", "1"},
                                             {"CPR",  "CPR Train",    "3",  "",       "CPR Training",     "U need CPR training",      "ST", "LINK", "CPR",  "Find training", "N", "http://www.google.com/findCPR", "2"},
                                             {"OFF1", "BU Hold",      null, "Y",      "BusnOffice Hold",  "Business Off Hold",        "",   "",     "",     "",              "",  "", "3"},
    
                                         };

        public IEnumerable<Restriction> Get()
        {
            var restrs = new List<Restriction>();
            var items = restrictions.Length / 12;
            for (int x = 0; x < items; x++)
            {
                int severity; Int32.TryParse(restrictions[x, 2], out severity);
                Restriction rest = new Restriction(Guid.NewGuid().ToString(), 
                                                   restrictions[x, 0],
                                                   restrictions[x, 1],
                                                   severity,
                                                   restrictions[x, 3],
                                                   restrictions[x, 4],
                                                   restrictions[x, 5],
                                                   restrictions[x, 6],
                                                   restrictions[x, 7],
                                                   restrictions[x, 8],
                                                   restrictions[x, 9],
                                                   restrictions[x, 10]);
                rest.Hyperlink = restrictions[x, 11];
                int enumNum = 0;
                bool result = int.TryParse(restrictions[x, 12], out enumNum);
                rest.RestIntgCategory = (RestrictionCategoryType) Enum.ToObject(typeof(RestrictionCategoryType), enumNum - 1);
                restrs.Add(rest);
            }
            return restrs;
        }
    }
}
