//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Class defines the StudentDocument domain
    /// </summary>
    [Serializable]
    public class StudentDocument
    {
        private readonly string _StudentId;
        private readonly string _Code;

        /// <summary>
        /// Student Id of the StudentDocument
        /// </summary>
        public string StudentId { get { return _StudentId; } }

        /// <summary>
        /// Document Code identifying the StudentDocument
        /// </summary>
        public string Code { get { return _Code; } }

        /// <summary>
        /// Document Instance specifying student specific information
        /// about the document
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// The status of the StudentDocument. Initialized to Incomplete
        /// </summary>
        public DocumentStatus Status { get; set; }

        /// <summary>
        /// The date that the status of this StudentDocument was updated.
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// The DueDate, if any, of the StudentDocument. Initialized to null.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Document status description
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Constructor for a StudentDocument
        /// </summary>
        /// <param name="studentId">The studentId of the StudentDocument</param>
        /// <param name="documentCode">The Document Code identifying the StudentDocument. This code will come from a list of financial aid document codes.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the arguments are null or empty.</exception>
        public StudentDocument(string studentId, string documentCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(documentCode))
            {
                throw new ArgumentNullException("documentCode");
            }

            _StudentId = studentId;
            _Code = documentCode;

            Status = DocumentStatus.Incomplete;
        }
    }
}
