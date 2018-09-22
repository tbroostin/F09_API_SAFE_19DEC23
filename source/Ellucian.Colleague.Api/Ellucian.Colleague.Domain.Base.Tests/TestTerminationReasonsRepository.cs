// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestEmploymentTerminationReasonsRepository
    {
        private string[,] employmentTerminationReasons = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "TERM", "Termination"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "LOA", "Leave of Absence"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "DEM", "Demotion"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "RET", "Retired"},
                                            {"80779c4f-b2ac-4ad4-a970-ca5699d9891f", "EOC", "End of Contract"},
                                            {"ae21110e-991e-405e-9d8b-47eeff210a2d", "EEO", "Change EEO Information"}
                                      };

        public IEnumerable<EmploymentTerminationReason> GetEmploymentTerminationReasons()
        {
            var empTerminationReasonList = new List<EmploymentTerminationReason>();

            // There are 3 fields for each employment classification in the array
            var items = employmentTerminationReasons.Length / 3;

            for (int x = 0; x < items; x++)
            {
                empTerminationReasonList.Add(new EmploymentTerminationReason(employmentTerminationReasons[x, 0], employmentTerminationReasons[x, 1], employmentTerminationReasons[x, 2]));
            }
            return empTerminationReasonList;
        }
    }
}
