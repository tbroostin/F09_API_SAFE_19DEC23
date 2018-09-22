// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPayPeriodsRepository
    {
        private string[,] payPeriods = {
                                            //GUID   CODE   DESCRIPTION
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "DESC2", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "g5u4827d-1a54-232b-9239-5ac4f6dt3257"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "DESC3", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "bfea651b-8e27-4fcd-abe3-04573443c04c", "bfea651b-8e27-4fcd-abe3-04573443c04c", "bfea651b-8e27-4fcd-abe3-04573443c04c"},
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "DESC1", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "bfea651b-8e27-4fcd-abe3-04573443c04c", "bfea651b-8e27-4fcd-abe3-04573443c04c", "bfea651b-8e27-4fcd-abe3-04573443c04c"}, 
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "DESC4", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "9ae3a175-1dfd-4937-b97b-3c9ad596e023"}
                                      };

        public IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod> GetPayPeriods()
        {
            var payPeriodsList = new List<Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod>();

            // There are 6 fields for each pay period in the array
            var items = payPeriods.Length / 6;

            for (int x = 0; x < items; x++)
            {
                payPeriodsList.Add(new Ellucian.Colleague.Domain.HumanResources.Entities.PayPeriod(payPeriods[x, 0], payPeriods[x, 1], DateTime.Now, DateTime.Now, DateTime.Now, payPeriods[x, 2]));
            }
            return payPeriodsList;
        }
    }
}
