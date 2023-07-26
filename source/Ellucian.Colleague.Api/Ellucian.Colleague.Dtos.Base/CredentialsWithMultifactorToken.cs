// Copyright 2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Proxy authentication credentials with multifactor service token 
    /// and one-time password
    /// </summary>
    public class CredentialsWithMultifactorToken
    {
        /// <summary>
        /// User's login ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Service token from multifactor request
        /// </summary>
        public string ServiceToken { get; set; }

        /// <summary>
        /// Multifactor authentication one-time password
        /// </summary>
        public string MultifactorOneTimePassword { get; set; }
    }
}
