/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Academic Progress Appeals
    /// </summary>
 
    public class AcademicProgressAppeal
    {
        /// <summary>
        /// Unique id of this appeal object
        /// </summary>
        public string Id { get; set;}

        /// <summary>
        /// Colleague person id that this appeal belongs to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The status code of the appeal
        /// </summary>
        public string AppealStatusCode { get; set; }

        /// <summary>
        /// The date of the appeal
        /// </summary>
        public DateTime AppealDate { get; set; }

        /// <summary>
        /// The specific counselor associated to this appeal
        /// </summary>
        public string AppealCounselorId { get; set; }

        /// <summary>
        /// Id of the associated Evaluation
        /// </summary>
        public string AcademicProgressEvaluationId { get; set; }

    }
}
