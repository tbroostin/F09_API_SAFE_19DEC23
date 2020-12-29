// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonEmerTypesRepository
    {
        private string[,] personEmerTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "DAY", "Daytime phone number"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "EVE", "Evening phone number"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "OTH", "Other phone number"}
                                      };

        public IEnumerable<IntgPersonEmerTypes> GetIntgPersonEmerTypesAsync()
        {
            var intgPersonEmergencyTypesList = new List<IntgPersonEmerTypes>();

            // There are 3 fields for each interest type in the array
            var items = personEmerTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                intgPersonEmergencyTypesList.Add(
                    new IntgPersonEmerTypes(
                        personEmerTypes[x, 0], personEmerTypes[x, 1], personEmerTypes[x, 2]
                     ));
            }
            return intgPersonEmergencyTypesList;
        }
    }
}