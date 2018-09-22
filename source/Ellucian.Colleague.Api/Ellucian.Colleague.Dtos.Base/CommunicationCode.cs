// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Communication code and description
    /// </summary>
    public class CommunicationCode
    {
        /// <summary>
        /// Unique system code for this communication code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Communication code description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The CommunicationCode's Url. Allows users to specify an external hyperlink to the document(s) this code represents.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// A long form explanation of this communication code. Usually used to describe what a person should do with
        /// the document(s) this code represents.
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// The award year that this code is assigned to. In many cases, financial aid offices assign
        /// communication codes to specific award years.
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The OfficeCodeId of the Office that owns this CommunicationCode.
        /// </summary>
        public string OfficeCodeId { get; set; }

        /// <summary>
        /// Boolean flag to indicate if the communication code is included
        /// on the CCWP form based on which we will (not) display a document
        /// </summary>
        public bool IsStudentViewable { get; set; }
    }
}