// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Web.Http.Configuration
{
    public interface IApiSettingsRepository
    {
        IEnumerable<string> GetNames();
        ApiSettings Get(string name);
        bool Update(ApiSettings apiSettings);
    }
}
