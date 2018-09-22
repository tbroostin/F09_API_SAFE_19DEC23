// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Document named query
    /// </summary>
    public class DocumentFilter
    {
        /// <summary>
        /// allows filtering on invoice or student refund. Both type and document ID are required
        /// </summary>
        [JsonProperty("document", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DocumentCredentialsDtoProperty Document { get; set; }
    }

    /// <summary>
    /// Document Credential DTO Property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DocumentCredentialsDtoProperty
    {
        /// <summary>
        /// The invoice type of the document
        /// </summary>
        [FilterProperty("document")]
        [JsonProperty("type")]
        public Dtos.EnumProperties.InvoiceTypes Type { get; set; }

        /// <summary>
        /// The value of the credential
        /// </summary>
        [FilterProperty("document")]
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}