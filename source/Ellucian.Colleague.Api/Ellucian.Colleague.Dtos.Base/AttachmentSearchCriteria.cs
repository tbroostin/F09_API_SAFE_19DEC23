// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Search criteria for file attachments
    /// </summary>
    public class AttachmentSearchCriteria
    {
        /// <summary>
        /// True if only attachments with an active status must be returned
        /// </summary>
        public bool IncludeActiveAttachmentsOnly { get; set; }

        /// <summary>
        /// The attachment owner to query by
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The start of the attachment modified date range to query by 
        /// </summary>
        public DateTime? ModifiedStartDate { get; set; }


        /// <summary>
        /// The end of the attachment modified date range to query by
        /// </summary>
        public DateTime? ModifiedEndDate { get; set; }

        /// <summary>
        /// List of collection IDs to query by
        /// </summary>
        public IEnumerable<string> CollectionIds { get; set; }
    }
}