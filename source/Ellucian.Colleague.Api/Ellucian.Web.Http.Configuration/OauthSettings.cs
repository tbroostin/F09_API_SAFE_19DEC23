// Copyright 2023 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Web.Http.Configuration
{
    /// <summary>
    /// Oauth Issuer URL and Oauth Proxy user parameters.
    /// </summary>
    public class OauthSettings
    {
        /// <summary>
        /// Gets or sets the Oauth Issuer Url.
        /// </summary>
        public string OauthIssuerUrl { get; set; }

        /// <summary>
        /// Gets or sets the Oauth Proxy login.
        /// </summary>
        public string OauthProxyLogin { get; set; }

        /// <summary>
        /// Gets or sets the Oauth Proxy password.
        /// </summary>
        public string OauthProxyPassword { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OauthSettings"/> class.
        /// </summary>
        public OauthSettings()
        {
            OauthIssuerUrl = "";
            OauthProxyLogin = "";
            OauthProxyPassword = "";
        }
    }
}
