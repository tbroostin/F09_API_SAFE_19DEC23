// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.Configuration.DataContracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Configuration
{
    public interface IExtendRepository
    {
        Dictionary<string, string> GetEthosExtensibilityConfiguration(bool bypassCache = false);


    }
}
