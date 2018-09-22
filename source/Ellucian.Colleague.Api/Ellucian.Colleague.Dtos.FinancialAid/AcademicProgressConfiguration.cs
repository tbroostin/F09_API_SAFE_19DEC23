/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Describes how to display AcademicProgressEvaluations to students
    /// </summary>
    public class AcademicProgressConfiguration
    {
        /// <summary>
        /// Office Id to which this configuration belongs
        /// </summary>
        public string OfficeId { get; set; }

        /// <summary>
        /// Indicates whether Satisfactory Academic Progress is available to be seen by Students
        /// </summary>
        public bool IsSatisfactoryAcademicProgressActive { get; set; }

        /// <summary>
        /// Indicates whether SAP history is available to be seen by students
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
        public IEnumerable<AcademicProgressPropertyConfiguration> DetailPropertyConfigurations { get; set; }

        /// <summary>
        /// A collection of Academic Progress Types to display in the UI
        /// </summary>
        public List<string> AcademicProgressTypesToDisplay { get; set; }
    }
}
