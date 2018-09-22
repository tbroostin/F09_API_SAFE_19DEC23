// Copyright 2017 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealPlanAssignment
    {

        #region Required Fields
        /// <summary>
        /// Meal plan
        /// </summary> 
        public string MealPlan { get; private set; }

        /// <summary>
        /// Number of Rate Periods
        /// </summary> 
        public int? NoRatePeriods { get; private set; }

        /// <summary>
        /// Meal plan assignment start date
        /// </summary> 
        public DateTime? StartDate { get;  private set; }

        /// <summary>
        /// Person ID the meal plan is assigned to
        /// </summary> 
        public string PersonId { get;  private set; }

        /// <summary>
        /// meal plan assignment current status
        /// </summary> 
        public string Status { get;  private set; }

        /// <summary>
        ///  meal plan assignment current status date
        /// </summary> 
        public DateTime? StatusDate { get;  private set; }

        #endregion

        #region Optional Fields

        #endregion

        /// <summary>
        /// Guid
        /// </summary>  
        private string _Guid;
        public string Guid { get { return _Guid; } }

        /// <summary>
        /// Meal plan assignment Id
        /// </summary>  
        private string _Id;
        public string Id { get { return _Id; } }

        /// <summary>
        /// meal plan assignment end date
        /// </summary>  
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Term for meal plan assignment 
        /// </summary>  
        public string Term { get; set; }

        /// <summary>
        /// meal plan assignment overrride rate.
        /// </summary>  
        public Decimal? OverrideRate { get; set; }

        /// <summary>
        /// meal plan assignment override reason
        /// </summary>  
        public string RateOverrideReason { get; set; }

        /// <summary>
        /// meal plan assignment override AR code
        /// </summary>  
        public string OverrideArCode { get; set; }

        /// <summary>
        /// meal plan assignment used rate periods
        /// </summary>  
        public int? UsedRatePeriods { get; set; }

        /// <summary>
        /// meal plan assignment percent used
        /// </summary>  
        public int? PercentUsed { get; set; }

        /// <summary>
        /// meal plan assignment card
        /// </summary>  
        public string MealCard { get; set; }

        /// <summary>
        /// meal plan assignment comments
        /// </summary>  
        public string MealComments { get; set; }
        
        public MealPlanAssignment(string guid, string id, string person, string mealplan, DateTime? startDate, int? NoOfPeriod, string status, DateTime? statusDate)
        {
            if (string.IsNullOrEmpty(person))
            {
                throw new ArgumentNullException(string.Concat("Person Id is required, Entity: 'MEAL.PLAN.ASSIGNMENT', Record ID: '",guid, "'"));
            }
            if (string.IsNullOrEmpty(mealplan))
            {
                throw new ArgumentNullException(string.Concat("Meal Plan is required for meal plan assignment', Record ID: '", guid, "'")); 
            }
            if (NoRatePeriods <= 0 )
            {
                throw new ArgumentNullException(string.Concat("Number of rate Periods is required for meal plan assignment.', Record ID: '", guid, "'")); 
            }
            if (startDate == null)
            {
                throw new ArgumentNullException(string.Concat("StartDate is required for meal plan assignment', Record ID: '", guid, "'")); 
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(string.Concat("Status is required for meal plan assignment', Record ID: '", guid, "'")); 
            }
            if (statusDate == null)
            {
                throw new ArgumentNullException(string.Concat("Status date is required for meal plan assignment', Record ID: '", guid, "'")); 
            }

            _Guid = guid;
            _Id = id;
            PersonId = person;
            MealPlan = mealplan;
            NoRatePeriods = NoOfPeriod;
            Status = status;
            StatusDate = statusDate;
            StartDate = startDate;


        }

     
    }
}
