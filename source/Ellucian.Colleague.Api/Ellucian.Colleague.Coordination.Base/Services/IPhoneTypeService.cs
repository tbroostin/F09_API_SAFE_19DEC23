// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPhoneTypeService
    {
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all phone types
        /// </summary>
        /// <returns>Collection of PhoneType DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.PhoneType2>> GetPhoneTypesAsync(bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a phone type from its GUID
        /// </summary>
        /// <returns>PhoneType DTO object</returns>
        Task<Ellucian.Colleague.Dtos.PhoneType2> GetPhoneTypeByGuidAsync(string guid);

        /// <summary>
        /// Get base phone types
        /// </summary>
        /// <returns>Collection of <see cref="Dtos.Base.PhoneType">Phone type</see> items</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>> GetBasePhoneTypesAsync();
    }
}
