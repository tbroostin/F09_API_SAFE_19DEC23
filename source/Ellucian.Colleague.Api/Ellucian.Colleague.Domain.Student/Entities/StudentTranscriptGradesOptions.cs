// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Stores student acad cred ID, GUID, and grade scheme
    /// </summary>
    [Serializable]
    public class StudentTranscriptGradesOptions
    {
        // Required Ids
        private string _Id;
        /// <summary>
        /// Unique ID.
        /// </summary>
        public string Id { get { return _Id; } }
        
        // Required Ids
        private string _Guid;
        /// <summary>
        /// Unique ID.
        /// </summary>
        public string Guid { get { return _Guid; } }

        /// <summary>
        /// It is used to determine which set of
        // grades can be assigned to this student's academic credit entry.
        /// </summary>
        public string GradeSchemeCode { get; set; }
        

        /// <summary>
        /// Base constructor for student transcript grades. 
        /// </summary>
        /// <param name="id">ID of this student transcript grades</param>
        public StudentTranscriptGradesOptions(string id, string guid)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _Id = id;

            if (guid == null)
            {
                throw new ArgumentNullException("guid");
            }
            _Guid = guid;
        }
  
        /// <summary>
        /// Equals method used for comparisons
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            AcademicCredit other = obj as AcademicCredit;
            if (other == null)
            {
                return false;
            }
            return Id.Equals(other.Id);

        }

        /// <summary>
        /// Needed for Equals comparisons
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }      
    }
}
