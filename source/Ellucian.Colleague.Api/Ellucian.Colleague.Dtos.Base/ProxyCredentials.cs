// Copyright 2014 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Proxy authentication credentials
    /// </summary>
    public class ProxyCredentials
    {
        /// <summary>
        /// Proxy login ID
        /// </summary>
        public string ProxyId { get; set; }

        /// <summary>
        /// Proxy password
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// User's login ID
        /// </summary>
        public string UserId { get; set; }
    }
}
