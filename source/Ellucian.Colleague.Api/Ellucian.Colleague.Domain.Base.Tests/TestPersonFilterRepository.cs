// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonFilterRepository
    {
        private string[,] personFilters = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BFEDLIST", "Description1"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CHC", "Description2"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SES23", "Description3"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "ADVISEEASSIGN", "Description4"}
                                      };

        public IEnumerable<PersonFilter> GetPersonFilters()
        {
            var personFilterList = new List<PersonFilter>();

            // There are 3 fields for each person filter in the array
            var items = personFilters.Length / 3;

            for (int x = 0; x < items; x++)
            {
                personFilterList.Add(new PersonFilter(personFilters[x, 0], personFilters[x, 1], personFilters[x, 2]));
            }
            return personFilterList;
        }
    }
}
