// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonalRelationshipStatusRepository
    {
        private string[,] personalRelationshipStatuses = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "A", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "L", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "O", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "D", "Description"}
                                      };

        public IEnumerable<RelationshipStatus> GetPersonalRelationshipStatuses()
        {
            var personalRelationshipStatusList = new List<RelationshipStatus>();

            // There are 3 fields for each personal relationship status in the array
            var items = personalRelationshipStatuses.Length / 3;

            for (int x = 0; x < items; x++)
            {
                personalRelationshipStatusList.Add(new RelationshipStatus(personalRelationshipStatuses[x, 0], personalRelationshipStatuses[x, 1], personalRelationshipStatuses[x, 2]));
            }
            return personalRelationshipStatusList;
        }
    }
}
