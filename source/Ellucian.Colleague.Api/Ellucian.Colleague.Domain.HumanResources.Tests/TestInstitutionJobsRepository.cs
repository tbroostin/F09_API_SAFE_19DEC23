// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestInstitutionJobsRepository
    {
        private string[,] institutionJobs = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "bfea651b-8e27-4fcd-abe3-04573443c04c"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "g5u4827d-1a54-232b-9239-5ac4f6dt3257"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "bfea651b-8e27-4fcd-abe3-04573443c04c"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "bfea651b-8e27-4fcd-abe3-04573443c04c", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "9ae3a175-1dfd-4937-b97b-3c9ad596e023"}
                                      };

        public IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs> GetInstitutionJobs()
        {
            var institutionJobsList = new List<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs>();

            // There are 5 fields for each institution job in the array
            var items = institutionJobs.Length / 5;

            for (int x = 0; x < items; x++)
            {
                institutionJobsList.Add(new Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionJobs(institutionJobs[x, 0], institutionJobs[x, 1], institutionJobs[x, 2], institutionJobs[x, 3], DateTime.Now));
            }
            return institutionJobsList;
        }
    }
}
