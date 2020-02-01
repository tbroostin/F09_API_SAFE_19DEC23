// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Returns notices for a student in a given program
    /// </summary>
    public class EvaluationNotice
    {
        /// <summary>
        /// Id of the student to whom this notice applies
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Id of the program relevant to this notice
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// The text of the notice
        /// </summary>
        public List<string> Text { get; set; }
        /// <summary>
        /// The type of the notice text
        /// </summary>
        public EvaluationNoticeType Type { get; set; }
    }
}
