// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AcademicHistoryLevel
    {
        private string _AcademicLevelCode;
        public string AcademicLevelCode { get { return _AcademicLevelCode; } }
        private AcademicHistory _AcademicLevelHistory;
        public AcademicHistory AcademicLevelHistory { get { return _AcademicLevelHistory; } }
        private string _StudentId;
        public string StudentId { get { return _StudentId; } }

        public AcademicHistoryLevel(string academicLevelCode, AcademicHistory academicHistory, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (academicLevelCode == null)
            {
                throw new ArgumentNullException("academicLevelCode");
            }
            _AcademicLevelCode = academicLevelCode;
            _AcademicLevelHistory = academicHistory;
            _StudentId = studentId;
        }
    }
}
