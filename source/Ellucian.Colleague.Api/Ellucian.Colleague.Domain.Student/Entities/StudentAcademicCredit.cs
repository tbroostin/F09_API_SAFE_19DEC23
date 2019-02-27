// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcademicCredit
    {
        /// <summary>
        /// Gets the student's GUID
        /// </summary>
        public string PersonSTGuid { get; private set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; private set; }
        public List<StudentGPAInfo> StudentGPAInfoList { get; set; }
        public List<string> AcademicPeriods { get; set; }

        public StudentAcademicCredit(string guid, string recordKey)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("Student guid is required.");
            }

            if (string.IsNullOrWhiteSpace(recordKey))
            {
                throw new ArgumentNullException("Student id is required.");
            }
            PersonSTGuid = guid;
            StudentId = recordKey;
        }

        public StudentAcademicCredit(string recordKey)
        {           
            if (string.IsNullOrWhiteSpace(recordKey))
            {
                throw new ArgumentNullException("Student id is required.");
            }
            StudentId = recordKey;
        }

        public StudentAcademicCredit()
        {
            
        }
    }
}