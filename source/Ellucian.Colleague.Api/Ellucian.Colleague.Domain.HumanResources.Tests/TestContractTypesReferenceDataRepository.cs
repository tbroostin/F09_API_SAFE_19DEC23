// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestContractTypesReferenceDataRepository
    {
        private string[,] rehireTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "FT", "Full-Time"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "PT", "Part-Time"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "RT", "Retired"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "PR", "Probation"}
                                      };

        public IEnumerable<HrStatuses> GetHrStatusesAsync()
        {
            var rehireTypeList = new List<HrStatuses>();

            // There are 4 fields for each contract type in the array
            var items = rehireTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                rehireTypeList.Add(new HrStatuses(rehireTypes[x, 0], rehireTypes[x, 1], rehireTypes[x, 2]) { IsEmployeeStatus = true });
            }
            return rehireTypeList;
        }
    }
}
