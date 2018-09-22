// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestEthnicityRepository
    {
        private string[,] ethnicities = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "HIS", "Hispanic/Latino"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "NHS", "Non Hispanic/Latino"}
                                      };

        public IEnumerable<Ethnicity> Get()
        {
            var ethnicityList = new List<Ethnicity>();

            // There are 3 fields for each ethnicity in the array
            var items = ethnicities.Length / 3;

            for (int x = 0; x < items; x++)
            {
                ethnicityList.Add(new Ethnicity(ethnicities[x, 0], ethnicities[x, 1], ethnicities[x, 2], EthnicityType.Hispanic));
            }
            return ethnicityList;
        }
    }
}