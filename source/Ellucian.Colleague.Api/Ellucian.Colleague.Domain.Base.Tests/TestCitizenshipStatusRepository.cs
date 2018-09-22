// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestCitizenshipStatusRepository
    {
        private string[,] citizenshipStatuses = {
                                            //GUID   CODE   DESCRIPTION   SP1
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "CS", "Canadian citizen", "NA"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "PR", "Permanent resident", "NRA"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "R", "Resident Alien", "NRA"}
                                      };

        public IEnumerable<CitizenshipStatus> Get()
        {
            var citizenshipStatusList = new List<CitizenshipStatus>();

            // There are 4 fields for each citizenship status in the array
            var items = citizenshipStatuses.Length / 4;

            for (int x = 0; x < items; x++)
            {
                var newType = new CitizenshipStatusType();
                switch (citizenshipStatuses[x,3])
                {
                    case "NA":
                         newType = CitizenshipStatusType.Citizen;
                         break;
                    case "NRA":
                        newType = CitizenshipStatusType.NonCitizen;
                        break;
                    default:
                        newType = CitizenshipStatusType.NonCitizen;
                        break;
                }
                citizenshipStatusList.Add(new CitizenshipStatus(citizenshipStatuses[x, 0], citizenshipStatuses[x, 1], citizenshipStatuses[x, 2], newType));
            }
            return citizenshipStatusList;
        }
    }
}