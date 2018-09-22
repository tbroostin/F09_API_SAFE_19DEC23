// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Evaluation notices for a given student and program.
    /// </summary>
    [Serializable]
    public class EvaluationNotice
    {
        /// <summary>
        /// Id of the student
        /// </summary>
        public string StudentId { get { return _studentId; } }
        private string _studentId { get; set; }

        /// <summary>
        /// Id of the program
        /// </summary>
        public string ProgramCode { get { return _programCode; } }
        private string _programCode { get; set; }

        /// <summary>
        /// Text of the notice
        /// </summary>
        public ReadOnlyCollection<string> Text { get; private set; }
        private readonly List<string> _text = new List<string>();

        public EvaluationNoticeType Type { get { return _type; } }
        private EvaluationNoticeType _type;

        /// <summary>
        /// Constructor for program evaluation notices. Text can be empty
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="programId"></param>
        public EvaluationNotice(string studentId, string programCode, IEnumerable<string> text, EvaluationNoticeType type)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Student Id cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException(programCode, "Program Code cannot be null or empty.");
            }
            if (text == null || text.Count() == 0)
            {
                throw new ArgumentNullException("text", "Text cannot be null or an empty string");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type", "Type cannot be empty");
            }
            _studentId = studentId;
            _programCode = programCode;
            _type = type;
            _text.AddRange(text);
            Text = _text.AsReadOnly();
        }
    }
}
