// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    ///  Ethos Data Privacy Settings
    /// </summary>
    [Serializable]
    public class EthosSecurity
    {
        /// <summary>
        /// Name of the API having properties secured
        /// </summary>
        public string ApiName { get; set; }

        public IEnumerable<EthosSecurityDefinitions> PropertyDefinitions { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiName">Name of API</param>
        /// <param name="securityDefinitions">list of property setting information</param>
        public EthosSecurity(string apiName, IEnumerable<EthosSecurityDefinitions> securityDefinitions )
        {
            ApiName = apiName;
            PropertyDefinitions = securityDefinitions;
        }
    }
}
