// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Defines the category associated to an AcademicProgressStatus.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AcademicProgressStatusCategory
    {
        /// <summary>
        /// This category indicates that the AcademicProgressStatus represents Satisfactory 
        /// progress.
        /// </summary>
        Satisfactory,
        /// <summary>
        /// This category indicates that the AcademicProgressStatus represents Unsatisfactory
        /// progress.
        /// </summary>
        Unsatisfactory,
        /// <summary>
        /// This category indicates that the AcademicProgressStatus represents a Warning situation.
        /// </summary>
        Warning,
        /// <summary>
        /// This category7 indicates that the AcademicProgress record should not be displayed to the student.
        /// </summary>
        DoNotDisplay
    }
}
