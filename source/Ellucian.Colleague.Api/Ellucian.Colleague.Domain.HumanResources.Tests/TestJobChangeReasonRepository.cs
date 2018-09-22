// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.HumanResources.Base.Tests
{
    public class TestJobChangeReasonRepository
    {
        private string[,] jobChangeReasons = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BA", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CA", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CC", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "EP", "Description"}
                                      };

        public IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason> GetJobChangeReasons()
        {
            var jobChangeReasonList = new List<Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason>();

            // There are 3 fields for each job change reason in the array
            var items = jobChangeReasons.Length / 3;

            for (int x = 0; x < items; x++)
            {
                jobChangeReasonList.Add(new Ellucian.Colleague.Domain.HumanResources.Entities.JobChangeReason(jobChangeReasons[x, 0], jobChangeReasons[x, 1], jobChangeReasons[x, 2]));
            }
            return jobChangeReasonList;
        }
    }
}
