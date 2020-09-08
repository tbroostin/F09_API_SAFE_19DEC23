// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IEmailTypeService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmailType>> GetEmailTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.EmailType> GetEmailTypeByGuidAsync(string guid);
        
        /// <summary>
        /// Get email types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.EmailType">EmailType</see> items consisting of code and description</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.EmailType>> GetBaseEmailTypesAsync();

    }
}
