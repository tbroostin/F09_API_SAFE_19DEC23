/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// CommunicationCode is the definition of a communication that is sent or received
    /// Version 2 introduced in API 1.8
    /// </summary>
    public class CommunicationCode2
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
        /// A list of the CommunicationCode's hyperlinks. This list specifies hyperlinks
        /// to the document(s) or communication(s) this code represents.
        /// </summary>
        public IEnumerable<CommunicationCodeHyperlink> Hyperlinks { get; set; }

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
        /// Boolean flag to indicate if the communication code is considered a "Required Document" for students
        /// </summary>
        public bool IsStudentViewable { get; set; }

        /// <summary>
        /// Boolean flag to indicate if the communication code allows attachments
        /// </summary>
        public bool AllowsAttachments { get; set; }
    }
}
