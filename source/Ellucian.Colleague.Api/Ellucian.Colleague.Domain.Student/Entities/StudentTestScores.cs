// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentTestScores
       {
        private readonly string _recordKey;
        private readonly string _code;
        private readonly string _description;
        private readonly string _studentId;
        private readonly string _guid;
        private readonly DateTime _dateTaken;

        /// <summary>
        /// Guid
        /// </summary>        
        public string Guid { get { return _guid; }  }

        /// <summary>
        /// Guid
        /// </summary>        
        public string RecordKey { get { return _recordKey; } }

        /// <summary>
        /// Test Code
        /// </summary>
        public string Code { get { return _code; } }
        /// <summary>
        /// Test description - this is not always the name of the test because it can be changed by student (required)
        /// </summary>
        public string Description { get { return _description; } }
        /// <summary>
        /// Student of the test result (required)
        /// </summary>
        public string StudentId { get { return _studentId; } }
        /// <summary>
        /// Date the Test was taken (required)
        /// </summary>
        public DateTime DateTaken { get { return _dateTaken; } }
        /// <summary>
        /// Score received
        /// </summary>
        public decimal? Score { get; set; }
        /// <summary>
        /// Test percentile based on the score
        /// </summary>
        public int? Percentile1 { get; set; }
        /// <summary>
        /// Test percentile based on the score
        /// </summary>
        public int? Percentile2 { get; set; }
        /// <summary>
        /// Form No
        /// </summary>
        public string FormNo { get; set; }
        /// <summary>
        /// Form Name
        /// </summary>
        public string FormName { get; set; }
        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// special factors
        /// </summary>
        public List<string> SpecialFactors { get; set; }
        /// <summary>
        /// Status code
        /// </summary>
        public string StatusCode { get; set; }
        // <summary>
        /// Status code special processing
        /// </summary>
        public string StatusCodeSpProcessing { get; set; }
        /// <summary>
        /// Status date
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Test Source
        /// </summary>
        public string ApplicationTestSource { get; set; }

        // <summary> 
        /// Status code special processing 2
        /// </summary
         public string StatusCodeSpProcessing2 { get; set; }

        public StudentTestScores(string guid, string studentId, string code, string description, DateTime dateTaken)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            if (dateTaken == null)
            {
                throw new ArgumentNullException("Date Taken");
            }
            _studentId = studentId;
            _code = code;
            _description = description;
            _dateTaken = dateTaken;
            _guid = guid;
        }

        public StudentTestScores(string guid, string recordKey, string studentId, string code, string description, DateTime dateTaken)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
           
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            if (dateTaken == null)
            {
                throw new ArgumentNullException("Date Taken");
            }
            _studentId = studentId;
            _code = code;
            _description = description;
            _dateTaken = dateTaken;
            _guid = guid;
            _recordKey = recordKey;
        }

    }
}
