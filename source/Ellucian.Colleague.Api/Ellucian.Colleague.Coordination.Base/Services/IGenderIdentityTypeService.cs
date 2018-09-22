// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IGenderIdentityTypeService
    {

        /// <summary>
        /// Get gender identity types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.GenderIdentityType">GenderIdentityType</see> items consisting of code and description</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>> GetBaseGenderIdentityTypesAsync(bool ignoreCache);
    }
}
