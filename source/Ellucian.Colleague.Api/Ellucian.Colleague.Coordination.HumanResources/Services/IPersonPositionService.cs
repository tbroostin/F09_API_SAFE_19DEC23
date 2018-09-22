/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPersonPositionService
    {
        Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null);
    }
}
