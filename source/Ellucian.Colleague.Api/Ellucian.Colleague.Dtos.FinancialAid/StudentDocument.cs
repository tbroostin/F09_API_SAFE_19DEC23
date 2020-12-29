//Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Student Financial Aid Document DTO
    /// </summary>
    public class StudentDocument
    {
        /// <summary>
        /// The student id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The document code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The document instance uniquely identifies the associated 
        /// document
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// The status type of the document
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentStatus Status { get; set; }

        /// <summary>
        /// Document status description
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// The date that the status of this StudentDocument was updated.
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// The document's due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// The correspondence request's assign date. Needed to match up to correct item in the database along with Code and person.
        /// </summary>
        public DateTime? AssignDate { get; set; }
    }
}
