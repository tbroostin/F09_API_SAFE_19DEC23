// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PreferredSection
    {
        private string _studentId;
        /// <summary>
        /// ID of the student
        /// </summary>
        public string StudentId
        {
            get { return _studentId; }
        }

        private string _sectionId;
        /// <summary>
        /// ID of the section
        /// </summary>
        public string SectionId
        {
            get { return _sectionId; }
        }

        private Decimal? _credits;
        /// <summary>
        /// Credit value to use when registering 
        /// </summary>
        public Decimal? Credits
        {
            get { return _credits; }
        }

        /// <summary>
        /// Create a preferred section for a student to be used in registration. Not applicable for students 
        /// who use Degree Plans for course planning and registration.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="sectionId"></param>
        /// <param name="credits"></param>
        public PreferredSection(string studentId, string sectionId, Decimal? credits)
        {
            if (string.IsNullOrEmpty(studentId)) 
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId");
            }
            if (credits.HasValue && credits.Value < new decimal(0.0))
            {
                throw new ArgumentOutOfRangeException("credits", "Credits cannot be less than zero.");
            }

            _studentId = studentId;
            _sectionId = sectionId;
            _credits = credits;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PreferredSection other = obj as PreferredSection;
            if (other == null)
            {
                return false;
            }
            return (StudentId.Equals(other.StudentId) && SectionId.Equals(other.SectionId));
        }

        public override int GetHashCode()
        {
            string temp = string.Format("{0}{1}", StudentId, SectionId);
            return temp.GetHashCode();
        }

    }
}
