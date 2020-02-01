// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcademicCredential
    {
        /// <summary>
        /// Gets the record GUID
        /// </summary>
        public string RecordGuid { get; private set; }

        /// <summary>
        /// Gets the record GUID
        /// </summary>
        public string RecordKey { get; private set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }
        public DateTime? GraduatedOn { get; set; }
        public string AcademicLevel { get; set; }
        public string AcademicPeriod { get; set; }
        public string AcadPersonId { get; set; }
        public string AcadAcadProgramId { get; set; }
        //public List<AcadCredential> AcadCredentials { get; set; }
        public List<Tuple<string, DateTime?>> Degrees { get; set; }
        public List<Tuple<string, DateTime?>> Ccds { get; set; }
        public string StudentProgramGuid { get; set; }
        public List<string> AcadDisciplines { get; set; }
        //public List<string> AcadMajors { get; set; }
        //public List<string> AcadMinors { get; set; }
        //public List<string> Specializations { get; set; }
        public string AcadThesis { get; set; }
        public string AcadTerm { get; set; }
        public List<string> AcadHonors { get; set; }

        public StudentAcademicCredential(string guid, string recordKey)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("Student guid is required.");
            }

            if (string.IsNullOrWhiteSpace(recordKey))
            {
                throw new ArgumentNullException("Student id is required.");
            }
            RecordGuid = guid;
            RecordKey = recordKey;
        }

        public StudentAcademicCredential()
        {
            
        }
    }
}