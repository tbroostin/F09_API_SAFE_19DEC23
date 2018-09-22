/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// StudentBudgetComponent DTO describes student specific estimated costs
    /// </summary>
    public class StudentBudgetComponent
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom this budget component belongs
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Award Year this budget component belongs to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The Code of the BudgetComponent object, which contains the component's description, shopping sheet group, etc.
        /// </summary>
        public string BudgetComponentCode { get; set; }

        /// <summary>
        /// The budget amount for campus based costs that was originally assigned to this student. This amount
        /// does not change once the budget component is assigned to the student. 
        /// This attribute is required for a StudentBudgetComponent
        /// </summary>
        public int CampusBasedOriginalAmount { get; set; }

        /// <summary>
        /// The override budget amount for campus based costs. Once the budget component is assigned the student's budget 
        /// amount is updated via this attribute.
        /// </summary>
        public int? CampusBasedOverrideAmount { get; set; }
    }
}
