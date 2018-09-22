// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// This API will be used to share a single type of social media account with external systems.
    /// </summary>
    [DataContract]
    public class SocialMediaType :CodeItem2
    {
        /// <summary>
        /// The <see cref="SocialMediaTypeCategory">entity type</see> for the social media type categories
        /// </summary>
        [DataMember(Name = "type")]
        public SocialMediaTypeCategory SocialMediaTypeCategory { get; set; }
    }
}