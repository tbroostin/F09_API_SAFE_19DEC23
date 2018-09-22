using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestReceivableTypesRepository
    {
        private static List<ReceivableType> _receivableTypes = new List<ReceivableType>();
        public static List<ReceivableType> ReceivableTypes
        {
            get
            {
                if (_receivableTypes.Count == 0)
                {
                    GenerateEntities();
                }
                return _receivableTypes;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var record in TestArTypesRepository.ArTypes)
            {
                _receivableTypes.Add(new ReceivableType(record.Recordkey, record.ArtDesc));
            }
        }
    }
}
