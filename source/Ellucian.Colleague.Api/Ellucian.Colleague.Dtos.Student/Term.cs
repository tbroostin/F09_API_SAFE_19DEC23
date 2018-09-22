using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An academic period of time
    /// </summary>
    public class Term
    {
        /// <summary>
        /// Unique code for the term
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Public name of this term
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Start date for the term
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// End date for the term
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Reporting year in which the term is grouped
        /// </summary>
        public int ReportingYear { get; set; }
        /// <summary>
        /// Sequencing of this term among other terms in the reporting year
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// Parent reporting term of this term
        /// </summary>
        public string ReportingTerm { get; set; }
        /// <summary>
        /// Financial period of this term
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PeriodType FinancialPeriod { get; set; }

        /// <summary>
        /// Does this term appear by default on a new term in a degree plan?
        /// </summary>
        public bool DefaultOnPlan { get; set; }
        /// <summary>
        /// Flag to identify if the term is active on today's date
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Flag to identify if the term is under student planning
        /// </summary>
        public bool ForPlanning { get; set; }
        /// <summary>
        /// Set of Registration dates which may be based on specific locations
        /// </summary>
        public List<RegistrationDate> RegistrationDates { get; set; }
        /// <summary>
        /// Financial Aid Years applicable to this term
        /// </summary>
        public List<int?> FinancialAidYears { get; set; }
        /// <summary>
        /// Session cycles associated to this term 
        /// </summary>
        public List<string> SessionCycles { get; set; }
        /// <summary>
        /// Session cycles associated to this term 
        /// </summary>
        public List<string> YearlyCycles { get; set; }

    }
}
