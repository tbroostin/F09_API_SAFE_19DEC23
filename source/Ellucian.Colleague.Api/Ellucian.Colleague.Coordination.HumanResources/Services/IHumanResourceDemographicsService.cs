/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IHumanResourceDemographicsService
    {
        Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographicsAsync(string effectivePersonId = null);

        Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographics2Async(string effectivePersonId = null, System.DateTime? lookupStartDate = null);

        Task<HumanResourceDemographics> GetSpecificHumanResourceDemographicsAsync(string id, string effectivePersonId = null);

        Task<IEnumerable<HumanResourceDemographics>> QueryHumanResourceDemographicsAsync(Dtos.Base.HumanResourceDemographicsQueryCriteria criteria, string effectivePersonId = null);
    }
}
