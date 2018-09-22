/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Describes how to display AcademicProgressEvaluations to students
    /// </summary>
    [Serializable]
    public class AcademicProgressConfiguration
    {
        /// <summary>
        /// The Id of the Financial Aid Office to which this configuration belongs
        /// </summary>
        public string OfficeId { get { return officeId; } }
        private readonly string officeId;

        /// <summary>
        /// Indicates whether Satisfactory Academic Progress is available to be seen by Students
        /// </summary>
        public bool IsSatisfactoryAcademicProgressActive { get; set; }

        /// <summary>
        /// Indicates when SAP history is available to be seen by students
        /// </summary>
        public bool IsSatisfactoryAcademicProgressHistoryActive { get; set; }

        /// <summary>
        /// Indicates the number of SAP history records to display
        /// </summary>

        public int NumberOfAcademicProgressHistoryRecordsToDisplay { get; set; }

        /// <summary>
        /// A collection of property configurations that indicate how to display the properties of
        /// an <see cref="AcademicProgressEvaluationDetail">AcademicProgressEvaluationDetail</see> object
        /// </summary>
        public List<AcademicProgressPropertyConfiguration> DetailPropertyConfigurations { get; set; }

        /// <summary>
        /// A collection of Academic Progress Types to display in the UI
        /// </summary>
        public List<string> AcademicProgressTypesToDisplay { get; set; }


        /// <summary>
        /// Create an AcademicProgressConfiguration object that will be applied to all users assigned 
        /// to the specified Financial Aid Office.
        /// </summary>
        /// <param name="officeId">Required: The Id of the Financial Aid Office</param>
        /// <exception cref="ArgumentNullException">Thrown if officeId is null or empty</exception>
        public AcademicProgressConfiguration(string officeId)
        {
            if (string.IsNullOrEmpty(officeId))
            {
                throw new ArgumentNullException("officeId");
            }

            this.officeId = officeId;
            DetailPropertyConfigurations = new List<AcademicProgressPropertyConfiguration>();
            AcademicProgressTypesToDisplay = new List<string>();
        }

        /// <summary>
        /// Two AcademicProgressConfiguration objects are equal when their officeIds are equal.
        /// </summary>
        /// <param name="obj">The AcademicProgressConfiguration object to compare to this</param>
        /// <returns>True if the OfficeIds are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var config = obj as AcademicProgressConfiguration;

            return config.OfficeId == this.OfficeId;            
        }

        /// <summary>
        /// Computes the HashCode of this object based on the OfficeId
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return OfficeId.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of this object based on the OfficeId
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OfficeId.ToString();
        }
    }
}
