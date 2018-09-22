// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Class that contains SectionWise Attendances for a student.
    /// </summary>
    [Serializable]
    public class StudentSectionsAttendances
    {
        /// <summary>
        /// StudentId
        /// </summary>
        public string StudentId { get; private set; }

        /// <summary>
        /// SectionWise Attendances
        /// </summary>
        public IDictionary<string, List<StudentAttendance>> SectionWiseAttendances { get; private set; }

        /// <summary>
        /// Creates SectoionWise Attendances from flat list of student attendances
        /// </summary>
        /// <param name="studentAttendances">List of StudentAttendance</param>
        public void  AddStudentAttendances(IEnumerable<StudentAttendance> studentAttendances)
        {
            if(studentAttendances==null)
            {
                throw new ArgumentNullException("studentAttendances", "studentAttendances is required");
            }
            foreach(var attendance in studentAttendances)
            {
                if (attendance != null)
                {
                    var sectionId = attendance.SectionId;
                    if (!SectionWiseAttendances.ContainsKey(sectionId))
                    {
                        SectionWiseAttendances.Add(sectionId, new List<StudentAttendance>());
                    }
                    SectionWiseAttendances[sectionId].Add(attendance);
                }
            }
        }
        /// <summary>
        /// constructor
        /// </summary>
       public  StudentSectionsAttendances(string studentId)
        {
            if (studentId == null || string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId is required");
            }
            this.StudentId = studentId;
            SectionWiseAttendances = new Dictionary<string, List<StudentAttendance>>();

        }

    }
}
