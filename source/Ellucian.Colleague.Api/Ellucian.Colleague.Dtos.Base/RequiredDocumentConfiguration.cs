// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Required document configuration
    /// </summary>
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
        public string TextForBlankDueDate { get; set; }
    }
}
