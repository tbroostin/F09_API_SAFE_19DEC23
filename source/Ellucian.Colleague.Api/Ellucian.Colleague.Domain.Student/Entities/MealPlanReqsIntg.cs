// Copyright 2017 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealPlanReqsIntg
    {

        #region Required Fields
        /// <summary>
        /// Meal plan
        /// </summary> 
        public string MealPlan { get; private set; }

       /// <summary>
        /// Person ID the meal plan is assigned to
        /// </summary> 
        public string PersonId { get;  private set; }

        #endregion

        #region Optional Fields

         /// <summary>
        /// Guid
        /// </summary>  
        private string _Guid;
        public string Guid { get { return _Guid; } }

        /// <summary>
        /// meal plan request Id
        /// </summary>  
        private string _Id;
        public string Id { get { return _Id; } }

        /// <summary>
        /// meal plan request end date
        /// </summary>  
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// meal plan request start date
        /// </summary> 
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// meal plan request current status
        /// </summary> 
        public string Status { get; set; }

        /// <summary>
        ///  meal plan request current status date
        /// </summary> 
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Term for meal plan request 
        /// </summary>  
        public string Term { get; set; }

        /// <summary>
        /// meal plan request submitted date
        /// </summary> 
        public DateTime? SubmittedDate { get; set; }

        #endregion

        /// <summary>
        /// ...ctor (string guid, string id, string person, string mealplan)
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id"></param>
        /// <param name="person"></param>
        /// <param name="mealplan"></param>
        public MealPlanReqsIntg(string guid, string id, string person, string mealplan)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id is required, Entity: 'MEAL.PLAN.REQS.INTG'");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("Guid is required, Entity: 'MEAL.PLAN.REQS.INTG', Record Key: '", id, "'"));
            }
            if (string.IsNullOrEmpty(person))
            {
                throw new ArgumentNullException(string.Concat("Person Id is required, Entity: 'MEAL.PLAN.REQS.INTG', Record ID: '", guid, "'"));
            }
            if (string.IsNullOrEmpty(mealplan))
            {
                throw new ArgumentNullException(string.Concat("Meal Plan Id is required, Entity: 'MEAL.PLAN.REQS.INTG', Record ID: '", guid, "'"));
            }
            
            _Guid = guid;
            _Id = id;
            PersonId = person;
            MealPlan = mealplan;
        }

        /// <summary>
        /// ...ctor (string guid, string person, string mealplan)
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="person"></param>
        /// <param name="mealplan"></param>
        public MealPlanReqsIntg(string guid, string person, string mealplan)
        {
            if (string.IsNullOrEmpty(person))
            {
                throw new ArgumentNullException(string.Concat("Person Id is required, Entity: 'MEAL.PLAN.REQS.INTG', Record ID: '", guid, "'"));
            }
            if (string.IsNullOrEmpty(mealplan))
            {
                throw new ArgumentNullException(string.Concat("Meal Plan Id is required, Entity: 'MEAL.PLAN.REQS.INTG', Record ID: '", guid, "'"));
            }

            _Guid = guid;
            PersonId = person;
            MealPlan = mealplan;
        }

       
    }
}
