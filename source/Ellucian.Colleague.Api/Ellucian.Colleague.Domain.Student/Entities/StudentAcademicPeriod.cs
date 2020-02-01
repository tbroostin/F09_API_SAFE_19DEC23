// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcademicPeriod
    {
        private string _Guid;
        private string _StudentId;
        private string _Term;
        
        public string Guid { get { return _Guid; } }
        public string StudentId { get { return _StudentId; } }
        public string Term { get { return _Term; } }
        
        //public string StudentLoad { get; set; }
        //public List<StudentTermStatus> StudentTermStatuses { get; set; }
        public List<StudentTerm> StudentTerms { get; set; }

        public StudentAcademicPeriod(string guid, string studentId, string termId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }

            _StudentId = studentId;
            _Term = termId;
            _Guid = guid;
        }
    }
}