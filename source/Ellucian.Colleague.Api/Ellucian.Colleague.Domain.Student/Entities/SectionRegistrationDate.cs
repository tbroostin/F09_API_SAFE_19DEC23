﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Registration date overrides specific to a particular section.
    /// These are calculated based on the requesting person's registration group.
    /// </summary>
    [Serializable]
    public class SectionRegistrationDate : RegistrationDate
    {
                /// <summary>
        /// Section Id for which these dates apply.
        /// </summary>
        public string SectionId { get { return _sectionId; } }
        private string _sectionId;

        public SectionRegistrationDate(string sectionId, string location, DateTime? registrationStartDate, DateTime? registrationEndDate,
            DateTime? preRegistrationStartDate, DateTime? preRegistrationEndDate,
            DateTime? addStartDate, DateTime? addEndDate,
            DateTime? dropStartDate, DateTime? dropEndDate,
            DateTime? dropGradeRequiredDate, List<DateTime?> censusDates)            
            : base(location, registrationStartDate, registrationEndDate, preRegistrationStartDate, preRegistrationEndDate, addStartDate, addEndDate, dropStartDate, dropEndDate, dropGradeRequiredDate, censusDates)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Must have a section Id to have a SectionRegistrationDate.");
            }
            _sectionId = sectionId;
        }
    }
}
