// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface ISelfservicePreferencesService
    {
        Task<Dtos.Base.SelfservicePreference> GetPreferenceAsync(string personId, string preferenceType);
        Task<Dtos.Base.SelfservicePreference> UpdatePreferenceAsync(string id, string personId, string preferenceType, IDictionary<string, dynamic> preferences);
        Task DeletePreferenceAsync(string personId, string preferenceType);
    }
}
