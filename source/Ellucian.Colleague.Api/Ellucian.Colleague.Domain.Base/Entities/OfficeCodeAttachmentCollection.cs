// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Mapping of an office code to an attachment collection
    /// </summary>
    [Serializable]
    public class OfficeCodeAttachmentCollection
    {

        /// <summary>
        /// Office Code being mapped to a collection (required)
        /// </summary>
        public string OfficeCode { get { return _OfficeCode; } }
        private string _OfficeCode;

        /// <summary>
        /// Attachment collection office code is mapped to (required)
        /// </summary>
        public string AttachmentCollection { get { return _AttachmentCollection; } }
        private string _AttachmentCollection;


        public OfficeCodeAttachmentCollection(string officeCode, string attachmentCollection)
        {
            if (string.IsNullOrEmpty(officeCode))
            {
                throw new ArgumentNullException("officeCode");
            }
            if (string.IsNullOrEmpty(attachmentCollection))
            {
                throw new ArgumentNullException("attachmentCollection");
            }

            _OfficeCode = officeCode;
            _AttachmentCollection = attachmentCollection;
        }
    }
}
