﻿/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Assign a ShoppingSheetBudgetGroup to a Budget Component to categorize that component on
    /// the Financial Aid Shopping Sheet
    /// </summary>
    [Serializable]
    public enum ShoppingSheetBudgetGroup
    {
        /// <summary>
        /// Categorizes Budget Components that apply to Tuition and Fees
        /// </summary>
        TuitionAndFees,

        /// <summary>
        /// Categorizes Budget Components that apply to Housing and Meals
        /// </summary>
        HousingAndMeals,

        /// <summary>
        /// Categorizes Budget Components that apply to Books and Supplies
        /// </summary>
        BooksAndSupplies,

        /// <summary>
        /// Categorizes Budget Components that apply to Transportation
        /// </summary>
        Transportation,

        /// <summary>
        /// Categorizes Budget Components that apply to any other costs that don't fall into the other categories
        /// </summary>
        OtherCosts
    }
}
