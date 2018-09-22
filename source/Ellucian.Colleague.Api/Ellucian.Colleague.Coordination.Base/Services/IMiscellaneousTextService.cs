// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IMiscellaneousTextService
    {
        /// <summary>
        /// Returns the Miscellaneous Text Configuration
        /// </summary>
        /// <returns>Miscellaneous Text Configuration DTO</returns>
        Task<IEnumerable<MiscellaneousText>> GetAllMiscellaneousTextAsync();
    }
}
