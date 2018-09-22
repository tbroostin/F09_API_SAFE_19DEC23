// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAcademicHonorsRepository
    {
        private string[,] otherHonors = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CL", "Cum Laude"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "FE", "University Fellow"}
                                      };


        public IEnumerable<OtherHonor> GetOtherHonors()
        {
            var otherHonorsList = new List<OtherHonor>();

            // There are 3 fields for each honor type in the array
            var items = otherHonors.Length / 3;

            for (int x = 0; x < items; x++)
            {
                otherHonorsList.Add(new OtherHonor(otherHonors[x, 0], otherHonors[x, 1], otherHonors[x, 2]));
            }
            return otherHonorsList;
        }

    }
}