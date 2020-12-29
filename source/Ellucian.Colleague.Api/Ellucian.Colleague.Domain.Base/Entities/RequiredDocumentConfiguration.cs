// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Required document configuration
    /// </summary>
    [Serializable]
    public class RequiredDocumentConfiguration
    {
        /// <summary>
        /// Suppress instance text after description
        /// </summary>
        public bool SuppressInstance { get; set; }

        /// <summary>
        /// Primary sort field
        /// </summary>
        public WebSortField PrimarySortField { get; set; }


        /// <summary>
        /// Secondary sort field
        /// </summary>
        public WebSortField SecondarySortField { get; set; }


        /// <summary>
        /// Display text for blank status
        /// </summary>
        public string TextForBlankStatus { get; set; }

        /// <summary>
        /// Display text for blank due date
        /// </summary>
        public string TextForBlankDueDate { get; set;  }


        /// <summary>
        /// Office Code to Attachment Collection Mapping used for Required Documents
        /// </summary>
        public RequiredDocumentCollectionMapping RequiredDocumentCollectionMapping { get; set; }

        public RequiredDocumentConfiguration()
        {
            // For clarity
            SuppressInstance = false;
        }

    }
}
