using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestChargeCodesRepository
    {
        private static List<ChargeCode> _chargeCodes;
        public static List<ChargeCode> ChargeCodes
        {
            get
            {
                if (_chargeCodes == null)
                {
                    _chargeCodes = TestArCodesRepository.ArCodes.Select(
                            cc => new ChargeCode(cc.Recordkey, cc.ArcDesc, cc.ArcPriority)).ToList();
                }
                return _chargeCodes;
            }
        }
    }
}
