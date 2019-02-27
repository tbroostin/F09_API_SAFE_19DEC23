// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool SuppressInstance { get { return _suppressInstance; } }
        private bool _suppressInstance;

        /// <summary>
        /// Primary sort field
        /// </summary>
        public WebSortField PrimarySortField { get { return _primarySortField; } }
        private WebSortField _primarySortField;

        /// <summary>
        /// Secondary sort field
        /// </summary>
        public WebSortField SecondarySortField { get { return _secondarySortField; } }
        private WebSortField _secondarySortField;

        /// <summary>
        /// Display text for blank status
        /// </summary>
        public string TextForBlankStatus { get { return _textForBlankStatus; } }
        private string _textForBlankStatus;

        /// <summary>
        /// Display text for blank due date
        /// </summary>
        public string TextForBlankDueDate { get { return _textForBlankDueDate; } }
        private string _textForBlankDueDate;

        public RequiredDocumentConfiguration(bool suppressInstance, WebSortField primarySortField, WebSortField secondarySortField, string textForBlankStatus, string textForBlankDueDate)
        {
            _suppressInstance = suppressInstance;
            _primarySortField = primarySortField;
            _secondarySortField = secondarySortField;
            _textForBlankStatus = textForBlankStatus;
            _textForBlankDueDate = textForBlankDueDate;
        }
    }
}
