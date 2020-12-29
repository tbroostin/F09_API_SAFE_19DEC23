// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Required document Attachment Collection Mapping
    /// </summary>
    public class RequiredDocumentCollectionMapping
    {
        /// <summary>
        /// When uploading attachments, correspondence requests with no office code should use this attachment collection (optional)
        /// </summary>
        public string RequestsWithoutOfficeCodeCollection { get; set; }

        /// <summary>
        /// If a correspondence request has an office code that is not in the office code mapping, use this attachment collection for attachments (optional)
        /// </summary>
        public string UnmappedOfficeCodeCollection { get; set; }

        /// <summary>
        /// Mapping of Office Codes to specific attachment collections
        /// </summary>
        public List<OfficeCodeAttachmentCollection> OfficeCodeMapping { get; set; }

    }
}
