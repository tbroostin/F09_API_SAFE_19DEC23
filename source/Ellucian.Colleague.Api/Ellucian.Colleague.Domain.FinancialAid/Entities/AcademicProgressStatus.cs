// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Indicates the status of an academic progress evaluation
    /// </summary>
    /// 
    [Serializable]
    public class AcademicProgressStatus
    {
        /// <summary>
        /// Identifying code of the AcademicProgressStatus
        /// </summary>
        public string Code { get { return code; } }
        private readonly string code;

        /// <summary>
        /// Description of the AcademicProgressStatus
        /// </summary>
        public string Description { get { return description; } }
        private readonly string description;

        /// <summary>
        /// Category of the AcademicProgressStatus
        /// </summary>
        public AcademicProgressStatusCategory? Category { get; set;}

        /// <summary>
        /// Explanation of the AcademicProgressStatus
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// The constructor expects two required arguments, code and 
        /// description.  The code represents the AcademicProgressStatus
        /// value and description is its external representation.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public AcademicProgressStatus(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
                        
            this.code = code;
            this.description = description;
            
         }

        /// <summary>
        /// Equals method to compare AcademicProgressStatus objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns false when the object is null or the type is not an AcademicProgressStatusType
        /// Returns true if the codes of the two AcademicProgressStatus objects are equal. </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            var academicProgressStatus = obj as AcademicProgressStatus;
            if (academicProgressStatus.Code == this.Code)
            {

                return true;
            }
            return false;

        }
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
     }

}
