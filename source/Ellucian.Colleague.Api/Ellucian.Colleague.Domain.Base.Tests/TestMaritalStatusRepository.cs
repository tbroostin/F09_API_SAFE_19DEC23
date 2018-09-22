// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestMaritalStatusRepository
    {
        private string[,] maritalStatuses = {
                                            //GUID   CODE   DESCRIPTION
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "U", "Single, Never Married"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "M", "Married"},
                                            {"eb2e3bed-3bfc-43b6-9305-ac1da21c2f33", "D", "Divorced"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "W", "Widowed"}, 
                                            {"b5315454-36b7-412e-9c9b-557a9af013bc", "S", "Separated"}
                                      };

        public IEnumerable<MaritalStatus> Get()
        {
            var maritalStatusList = new List<MaritalStatus>();

            // There are 3 fields for each marital status in the array
            var items = maritalStatuses.Length / 3;

            for (int x = 0; x < items; x++)
            {
                maritalStatusList.Add(new MaritalStatus(maritalStatuses[x, 0], maritalStatuses[x, 1], maritalStatuses[x, 2]));
            }
            return maritalStatusList;
        }
    }
}