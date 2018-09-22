// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Categorizes the AcademicProgressStatus 
    /// </summary>
    [Serializable]
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