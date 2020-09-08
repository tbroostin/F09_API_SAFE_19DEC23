using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class VendorInfoEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorInfo, Ellucian.Colleague.Dtos.ColleagueFinance.VendorInfo>
    {
        public VendorInfoEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public VendorInfo MapToType(Domain.ColleagueFinance.Entities.VendorInfo Source) {
            var vendorInfoDto = new VendorInfo();
            vendorInfoDto.VendorId = Source.VendorId;
            vendorInfoDto.VendorName = Source.VendorName;
            vendorInfoDto.VendorMiscName = Source.VendorMiscName;
            vendorInfoDto.Address = Source.Address;
            vendorInfoDto.City = Source.City;
            vendorInfoDto.Country = Source.Country;
            vendorInfoDto.State = Source.State;
            vendorInfoDto.Zip = Source.Zip;
            vendorInfoDto.VendorContacts = new List<VendorContactSummary>();
            var vendorContactsAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorContactSummary, Dtos.ColleagueFinance.VendorContactSummary>(adapterRegistry, logger);
            
            foreach (var vendorContact in Source.VendorContacts) {
                var vendorContactDto = vendorContactsAdapter.MapToType(vendorContact);
                // Add the vendor contacts DTOs to the vendor Info DTO
                vendorInfoDto.VendorContacts.Add(vendorContactDto);
            }
            return vendorInfoDto;
        }

    }
}
