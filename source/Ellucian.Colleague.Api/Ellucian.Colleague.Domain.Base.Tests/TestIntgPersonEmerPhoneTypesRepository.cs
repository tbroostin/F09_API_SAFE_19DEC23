// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonEmerPhoneTypesRepository
    { 
        private string[,] personEmerPhoneTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "DAY", "Daytime phone number"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "EVE", "Evening phone number"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "OTH", "Other phone number"}
                                      };

        public IEnumerable<IntgPersonEmerPhoneTypes> GetIntgPersonEmerPhoneTypesAsync()
        {
            var intgPersonEmergencyPhoneTypesList = new List<IntgPersonEmerPhoneTypes>();

            // There are 3 fields for each interest type in the array
            var items = personEmerPhoneTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                intgPersonEmergencyPhoneTypesList.Add(
                    new IntgPersonEmerPhoneTypes(
                        personEmerPhoneTypes[x, 0], personEmerPhoneTypes[x, 1], personEmerPhoneTypes[x, 2]
                     ));
            }
            return intgPersonEmergencyPhoneTypesList;
        }   
    }
}