// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Data Transfer Object to return status and processing date of transcript order to cloud.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class TranscriptResponse
    {
        /// <summary>
        /// Base-64 encoded string of XML response data
        /// </summary>
        public string ResponseData { get; set; }
    }
    
}
