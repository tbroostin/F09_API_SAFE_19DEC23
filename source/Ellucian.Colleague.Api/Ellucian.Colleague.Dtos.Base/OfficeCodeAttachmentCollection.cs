// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Mapping of an office code to an attachment collection
    /// </summary>
    public class OfficeCodeAttachmentCollection
    {
        /// <summary>
        /// Office Code being mapped to a collection 
        /// </summary>
        public string OfficeCode { get; set; }

        /// <summary>
        /// Attachment collection office code is mapped to 
        /// </summary>
        public string AttachmentCollection { get; set; }
    }
}
