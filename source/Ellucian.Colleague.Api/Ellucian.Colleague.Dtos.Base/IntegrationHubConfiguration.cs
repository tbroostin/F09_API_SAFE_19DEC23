// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Integration Hub Configuration Information
    /// </summary>
    [DataContract]
    public class IntegrationHubConfiguration
    {
        ///// <summary>
        ///// API Key of the application from the hub admin UI
        ///// </summary>
        //[DataMember(Name = "apiKey")]
        //public string ApiKey { get; set; }

        /// <summary>
        /// URL for the hub token service
        /// </summary>
        [DataMember(Name = "tokenUrl")]
        public string TokenUrl { get; set; }

        /// <summary>
        /// URL for the hub publish service
        /// </summary>
        [DataMember(Name = "publishUrl")]
        public string PublishUrl { get; set; }

        /// <summary>
        /// URL for the hub subscribe service
        /// </summary>
        [DataMember(Name = "subscribeUrl")]
        public string SubscribeUrl { get; set; }

        /// <summary>
        /// URL for the hub error reporting service
        /// </summary>
        [DataMember(Name = "errorUrl")]
        public string ErrorUrl { get; set; }

        /// <summary>
        /// The media type for messages getting published and retrieved (i.e. application/vnd.hedtech.change-notifications.v2+json)
        /// </summary>
        [DataMember(Name = "hubMediaType")]
        public string HubMediaType { get; set; }
    }
}
