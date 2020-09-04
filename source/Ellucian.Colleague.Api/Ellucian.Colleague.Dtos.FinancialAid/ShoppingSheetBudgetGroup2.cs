﻿/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Assign a ShoppingSheetBudgetGroup to a Budget Component to categorize that component on
    /// the Financial Aid Shopping Sheet
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShoppingSheetBudgetGroup2
    {
        /// <summary>
        /// Categorizes Budget Components that apply to Tuition and Fees
        /// </summary>
        TuitionAndFees,

        /// <summary>
        /// Categorizes Budget Components that apply to Housing and Meals for Shopping Sheets Pre-2020
        /// </summary>
        HousingAndMeals,

        /// <summary>
        /// Categorizes Budget Components that apply to Housing and Meals for On Campus
        /// </summary>
        HousingAndMealsOnCampus,

        /// <summary>
        /// Categorizes Budget Components that apply to Housing and Meals for Off Campus
        /// </summary>
        HousingAndMealsOffCampus,

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
