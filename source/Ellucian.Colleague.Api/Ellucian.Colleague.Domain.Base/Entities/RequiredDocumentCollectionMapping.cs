// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Required document Attachment Collection Mapping
    /// </summary>
    [Serializable]
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
        private readonly List<OfficeCodeAttachmentCollection> _OfficeCodeMapping = new List<OfficeCodeAttachmentCollection>();
        public ReadOnlyCollection<OfficeCodeAttachmentCollection> OfficeCodeMapping { get; private set; }

        public RequiredDocumentCollectionMapping()
        {
            OfficeCodeMapping = _OfficeCodeMapping.AsReadOnly();
        }

        /// <summary>
        /// Add an office code attachment collection to the existing list.
        /// </summary>
        public void AddOfficeCodeAttachment(OfficeCodeAttachmentCollection officeCodeCollection)
        {
            // Make sure the specific office code is not already in the mapping. An office code can ONLY be mapped to one collection.
            if (!OfficeCodeMapping.Where(o => o.OfficeCode == officeCodeCollection.OfficeCode).Any())
            {
                _OfficeCodeMapping.Add(officeCodeCollection);
            }
            // Not bothering to log duplicates - just ignore.
        }
    }
}
