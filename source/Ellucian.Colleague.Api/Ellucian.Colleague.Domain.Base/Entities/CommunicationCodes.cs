/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Communication codes
    /// </summary>
    [Serializable]
    public class CommunicationCode : GuidCodeItem
    {
        /// <summary>
        /// A list of the CommunicationCode's hyperlinks. This list specifies hyperlinks
        /// to the document(s) or communication(s) this code represents.
        /// </summary>
        public List<CommunicationCodeHyperlink> Hyperlinks { get; set; }

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
        /// Flag to indicate whether a code is to be displayed to a user
        /// </summary>
        public bool IsStudentViewable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationCode"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CommunicationCode(string guid, string code, string description)
            : base(guid, code, description)
        {
            Hyperlinks = new List<CommunicationCodeHyperlink>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationCode"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CommunicationCode(string guid, string code, string description, string officeCode)
            : base(guid, code, description)
        {
            Hyperlinks = new List<CommunicationCodeHyperlink>();
            OfficeCodeId = officeCode;
        }
    }
}

