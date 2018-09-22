/*Copyright 2015 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Exposes the Colleague SAP Appeal Codes
    /// </summary>
    [Serializable]
    public class AcademicProgressAppeal
    {
        /// <summary>
        /// The unique identifier of this appeal object
        /// </summary>
        public string Id { get { return id; } }
        private string id;

        /// <summary>
        /// Colleague PERSON id of the student to whom this evaluation belongs
        /// </summary>
        public string StudentId { get { return studentId; } }
        private string studentId;

        /// <summary>
        /// The status code of the appeal
        /// </summary>
        public string AppealStatusCode { get; set; }
        
        /// <summary>
        /// The date of the appeal
        /// </summary>
        public DateTime? AppealDate { get; set; }

        /// <summary>
        /// The specific counselor associated to this appeal
        /// </summary>
        public string AppealCounselorId { get; set; }

        /// <summary>
        /// Id of the associated Evaluation
        /// </summary>
        public int? AcademicProgressEvaluationId { get; set; }

        public AcademicProgressAppeal(string studentId, string id)
        {
            if (studentId == null)
            {
                throw new ArgumentException(studentId);
            }
            if (id == null)
            {
                throw new ArgumentException(id);
            }
            this.id = id;
            this.studentId = studentId;

        }
    }
}
