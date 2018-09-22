using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public string ResponseData;
    }
    
}
