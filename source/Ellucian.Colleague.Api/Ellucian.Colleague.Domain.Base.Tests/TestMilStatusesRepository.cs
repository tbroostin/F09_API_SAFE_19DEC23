// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestMilStatusesRepository
    {
        private string[,] milStatuses = {
                                            //GUID   CODE   DESCRIPTION
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "ARMY", "Army"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "AF", "Air Force"},
                                            {"eb2e3bed-3bfc-43b6-9305-ac1da21c2f33", "MAR", "Marines"},
                                         
                                      };

        public IEnumerable<MilStatuses> Get()
        {
            var milStatusList = new List<MilStatuses>();

            // There are 3 fields for each  status in the array
            var items = milStatuses.Length / 3;

            for (int x = 0; x < items; x++)
            {
                milStatusList.Add(new MilStatuses(milStatuses[x, 0], milStatuses[x, 1], milStatuses[x, 2]));
            }
            return milStatusList;
        }
    }
}