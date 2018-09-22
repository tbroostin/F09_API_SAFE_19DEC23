using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestDepositTypesRepository
    {
        private static List<DepositType> _depositTypes = new List<DepositType>();
        public static List<DepositType> DepositTypes
        {
            get
            {
                if (_depositTypes.Count == 0)
                {
                    GenerateEntities();
                }
                return _depositTypes;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var record in TestArDepositTypesRepository.ArDepositTypes)
            {
                _depositTypes.Add(new DepositType(record.Recordkey, record.ArdtDesc));
            }
        }
    }
}
