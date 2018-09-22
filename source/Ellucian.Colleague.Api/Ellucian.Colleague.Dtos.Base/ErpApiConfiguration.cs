// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// ERP API Configuration Information
    /// </summary>
    [DataContract]
    public class ErpApiConfiguration
    {
        /// <summary>
        /// Name of the ERP
        /// </summary>
        [DataMember(Name = "erpName")]
        public string ErpName { get; set; }

        /// <summary>
        /// Username for authentication to the Web API
        /// </summary>
        [DataMember(Name = "username")]
        public string ApiUsername { get; set; }

        ///// <summary>
        ///// Password for authentication to the Web API
        ///// </summary>
        //[DataMember(Name = "password")]
        //public string ApiPassword { get; set; }

        /// <summary>
        /// Collection of resource-to-business event mappings
        /// </summary>
        [DataMember(Name = "resourceMap")]
        public List<ResourceBusinessEventMapping> ResourceMappings { get; set; }

        /// <summary>
        /// Constructor for ErpApiConfiguration
        /// </summary>
        public ErpApiConfiguration()
        {
            ResourceMappings = new List<ResourceBusinessEventMapping>();
        }
    }
}
