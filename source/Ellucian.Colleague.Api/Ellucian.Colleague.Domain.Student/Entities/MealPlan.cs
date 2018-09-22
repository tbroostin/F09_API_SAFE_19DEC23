using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealPlan : GuidCodeItem
    {
        // Rate Period
        public string RatePeriod { get; set; }

        // Classification of student's residential
        public string Classification { get; set; }

        // Number of components
        public decimal? ComponentNumberOfUnits { get; set; }

        // Unit type of components
        public string ComponentUnitType { get; set; }

        // Time period of component
        public string ComponentTimePeriod { get; set; }

        // Restricted meal types
        public List<string> MealTypes { get; set; }

        // Dining facilities for meal types
        public List<string> DiningFacilities { get; set; }

        // Start day
        public string StartDay { get; set; }

        // End day
        public string EndDay { get; set; }

        // The buildings permitted in the meal plan
        public List<string> Buildings { get; set; }

        // The sites permitted in the meal plan
        public List<string> Sites { get; set; }

        // Start date
        public DateTime? StartDate { get; set; }

        // End date
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Constructor for MealPlan
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public MealPlan(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
