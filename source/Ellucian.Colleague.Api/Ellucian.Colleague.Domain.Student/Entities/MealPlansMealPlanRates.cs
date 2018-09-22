// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MealPlansMealPlanRates
    {
        public Decimal? MealRates;
        public DateTime? EffectiveDates;
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
