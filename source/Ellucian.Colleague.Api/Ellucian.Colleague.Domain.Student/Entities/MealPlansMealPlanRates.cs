// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealPlansMealPlanRates
    {
        public Decimal? MealRates { get; set; }
        public DateTime? EffectiveDates { get; set; }
        public MealPlansMealPlanRates() { }
        public MealPlansMealPlanRates(
            Decimal? inMealRates,
            DateTime? inMealRateEffectiveDates)
        {
            MealRates = inMealRates;
            EffectiveDates = inMealRateEffectiveDates;
        }
    }
}
