// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiSupportedMethods
    {
        /// <summary>
        /// Supported Methods (GET, PUT, POST, DELETE)
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Permissions Required for Method
        /// </summary>
        public string Permission { get; private set; }

        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// 
        public EthosApiSupportedMethods(string method, string permission)
        {
            Method = method;
            Permission = permission;
        }
    }
}