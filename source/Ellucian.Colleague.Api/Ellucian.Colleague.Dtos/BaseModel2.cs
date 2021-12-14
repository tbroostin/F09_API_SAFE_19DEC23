// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// This abstract class is the basis for all entities in the Higher Education Data Model Version 4
    /// </summary>
    public abstract class BaseModel2
    {
        /// <summary>
        /// An object containing metadata
        /// </summary>
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetaDataDtoProperty MetadataObject { get; set; }

        /// <summary>
        /// A Globally Unique ID (GUID)
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Base constructor for object initialization only
        /// </summary>
        protected BaseModel2()
        {
            //MetadataObject = new MetaDataDtoProperty();
        }
    }
}
