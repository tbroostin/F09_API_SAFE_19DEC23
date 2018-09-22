// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestReligionRepository
    {
        private string[,] denominations = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BA", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CA", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CC", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "EP", "Description"}
                                      };

        public IEnumerable<Denomination> GetDenominations()
        {
            var denominationList = new List<Denomination>();

            // There are 3 fields for each denomination in the array
            var items = denominations.Length / 3;

            for (int x = 0; x < items; x++)
            {
                denominationList.Add(new Denomination(denominations[x, 0], denominations[x, 1], denominations[x, 2]));
            }
            return denominationList;
        }
    }
}
