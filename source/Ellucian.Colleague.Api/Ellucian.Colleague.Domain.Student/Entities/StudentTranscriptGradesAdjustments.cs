// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The grade adjustments submitted for the student transcript.
    /// </summary>
    [Serializable]
    public class StudentTranscriptGradesAdjustments
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
        /// The global identifier of the student transcript grade.
        /// </summary>
        public string Guid { get { return _Guid; } }

        /// <summary>
        ///  The grade supplied for the adjustment.
        /// </summary>
        public string VerifiedGrade { get; set; }

        /// <summary>
        ///  The adjusted default final grade to be applied.
        /// </summary>
        public string IncompleteGrade { get; set; }

        /// <summary>
        /// The adjusted date after which the default final grade may be applied.
        /// </summary>
        public DateTime? ExtensionDate { get; set; }

        /// <summary>
        ///  The reason specified for the adjustment.
        /// </summary>
        public string ChangeReason { get; set; }

       
        /// <summary>
        /// Base constructor for student transcript grades. 
        /// </summary>
        /// <param name="id">ID of this student transcript grades</param>
        public StudentTranscriptGradesAdjustments(string id, string guid)
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
            StudentTranscriptGradesAdjustments other = obj as StudentTranscriptGradesAdjustments;
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
