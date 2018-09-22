// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayCycles2Repository
    {
        private string[,] payCycles = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BA", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CA", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CC", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "EP", "Description"}
                                      };

        public IEnumerable<PayCycle2> GetPayCycles()
        {
            var payCycleList = new List<PayCycle2>();

            // There are 3 fields for each pay cycle in the array
            var items = payCycles.Length / 3;

            for (int x = 0; x < items; x++)
            {
                payCycleList.Add(new PayCycle2(payCycles[x, 0], payCycles[x, 1], payCycles[x, 2]));
            }
            return payCycleList;
        }
    }
}
