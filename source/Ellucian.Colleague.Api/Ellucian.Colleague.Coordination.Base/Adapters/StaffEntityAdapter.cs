// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class StaffEntityAdapter
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        public StaffEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }

        public Dtos.Base.Staff MapToType(Ellucian.Colleague.Domain.Base.Entities.Staff source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Must provide source to convert");
            }

            Dtos.Base.Staff staffDto = new Dtos.Base.Staff(source.Id, source.LastName);
            staffDto.PrivacyCodes = source.PrivacyCodes;            

            return staffDto;
        }
    }
}
