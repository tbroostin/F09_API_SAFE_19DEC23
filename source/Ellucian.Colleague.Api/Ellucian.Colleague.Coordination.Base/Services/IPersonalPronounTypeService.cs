// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonalPronounTypeService
    {
        
        /// <summary>
        /// Get personal pronoun types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PersonalPronounType">PersonalPronounType</see> items consisting of code and description</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>> GetBasePersonalPronounTypesAsync(bool ignoreCache);
    }
}
