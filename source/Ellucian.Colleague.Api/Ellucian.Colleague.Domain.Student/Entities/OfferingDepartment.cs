// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Department codes
    /// </summary>
    [Serializable]
    public class OfferingDepartment
    {
        private string _academicDepartmentCode;
        /// <summary>
        /// Unique identifier for the department
        /// </summary>
        public string AcademicDepartmentCode { get { return _academicDepartmentCode; } }

        private decimal _responsibilityPercentage;
        /// <summary>
        /// Gets a value indicating the percentage of the department's responsibility for a course.
        /// </summary>
        public decimal ResponsibilityPercentage { get { return _responsibilityPercentage; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfferingDepartment"/> class.
        /// </summary>
        /// <param name="code">The academic department code.</param>
        /// <param name="description">The description.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public OfferingDepartment(string academicDepartmentCode, decimal responsibilityPercentage = 100m)
        {
            if (string.IsNullOrEmpty(academicDepartmentCode))
            {
                throw new ArgumentNullException("academicDepartmentCode", "Academic department code must be supplied.");
            }
            if (responsibilityPercentage < 0 || responsibilityPercentage > 100)
            {
                throw new ArgumentOutOfRangeException("responsibilityPercentage", "Percentage of department's responsibility must be between 0 and 100.");
            }

            _academicDepartmentCode = academicDepartmentCode;
            _responsibilityPercentage = responsibilityPercentage;
        }
    }
}