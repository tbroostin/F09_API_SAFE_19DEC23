// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IMiscellaneousTextRepository
    {
        /// <summary>
        /// Gets all of the miscellaneous text entries in Colleague.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MiscellaneousText>> GetAllMiscellaneousTextAsync();
    }
}
