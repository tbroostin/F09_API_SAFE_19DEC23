// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// MealPlanRates Domain Entity
    /// </summary>
    [Serializable]
    public class MealPlanRates : GuidCodeItem
    {
       
        public MealPlanRatePeriods MealRatePeriod { get; set; }
        
        public string MealArCode { get; set; }

        public MealPlansMealPlanRates MealPlansMealPlanRates { get; set; }

        public MealPlanRates(string guid, string code, string desc)
            : base(guid, code, desc)
        {

        }
    }
}
