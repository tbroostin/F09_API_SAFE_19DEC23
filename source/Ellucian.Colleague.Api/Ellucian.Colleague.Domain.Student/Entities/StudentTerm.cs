// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentTerm
    {
        private string _StudentId;
        private string _Term;
        private string _AcademicLevel;
        private string _Guid;
       

        public string StudentId { get { return _StudentId; } }
        public string Term { get { return _Term; } }
        public string AcademicLevel { get { return _AcademicLevel; } }
        public string StudentLoad { get; set; }
        public List<string> StudentAcademicCredentials { get; set; }      
        public string Guid { get { return _Guid; } }

        public List<StudentTermStatus> StudentTermStatuses { get; set; }
      

      
        public StudentTerm(string studentId, string termId, string levelId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }
            if (string.IsNullOrEmpty(levelId))
            {
                throw new ArgumentNullException("levelId");
            }
            _StudentId = studentId;
            _Term = termId;
            _AcademicLevel = levelId;
        }

        public StudentTerm(string guid, string studentId, string termId, string levelId)
        {

          
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }
            if (string.IsNullOrEmpty(levelId))
            {
                throw new ArgumentNullException("levelId");
            }
            _StudentId = studentId;
            _Term = termId;
            _AcademicLevel = levelId;
            _Guid = guid;
        }
    }
}
