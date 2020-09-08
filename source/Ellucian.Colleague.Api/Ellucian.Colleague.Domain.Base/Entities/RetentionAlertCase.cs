// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertCase
    {
        /// <summary>
        /// Id number of the student
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Case type under the category
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Summary of the case item
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detailed notes of the case
        /// </summary>
        public List<string> Notes { get; set; }

        /// <summary>
        /// Method of contact for the case
        /// </summary>
        public List<string> MethodOfContact { get; set; }

        public RetentionAlertCase(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId must be specified.");
            }

            StudentId = studentId;
        }
    }
}
