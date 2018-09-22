// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPrivacyStatusRepository
    {
        private string[,] privacyStatus = {
                                            //GUID   CODE   DESCRIPTION  
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "A", "Don't alter this record"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "X", "Duplicate"},
                                            {"eb2e3bed-3bfc-43b6-9305-ac1da21c2f33", "H", "HIPAA Restriction"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "S", "Secure all information"}
                                      };

        public IEnumerable<PrivacyStatus> Get()
        {
            var privacyStatusList = new List<PrivacyStatus>();

            // There are 3 fields for each privacy status in the array
            var items = privacyStatus.Length / 3;

            for (int x = 0; x < items; x++)
            {
                privacyStatusList.Add(new PrivacyStatus(privacyStatus[x, 0], privacyStatus[x, 1], privacyStatus[x, 2], PrivacyStatusType.restricted));
            }
            return privacyStatusList;
        }
    }
}