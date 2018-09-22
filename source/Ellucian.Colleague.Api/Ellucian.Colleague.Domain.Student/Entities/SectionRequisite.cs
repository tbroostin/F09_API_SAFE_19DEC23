// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section requisites related to registering for a section. For example, when taking BIOL-100-01
    /// you are required to take BIOL-101L-01 concurrently. 
    ///     
    ///     Required Section Requisites: Can be defined only at the section level, allows a list of
    ///     section Ids with a Number Needed, indicating the number of the listed sections needed to satisfy the requisite. 
    ///     
    ///     Recommended Section Requisites: Can be defined only at the section level, provides a list of section
    ///     Ids to communicate the list of sections recommended to be taken at the same time as the host section.
    ///     
    /// </summary>
    [Serializable]
    public class SectionRequisite
    {
        /// <summary>
        /// Defines the list of sections for this requisite
        /// </summary>
        private List<string> _CorequisiteSectionIds;
        public List<string> CorequisiteSectionIds { get { return _CorequisiteSectionIds; } }

        /// <summary>
        /// If required Corequisite Sections, indicates the number of those sections needed to satisfy this requisite
        /// </summary>
        private int _NumberNeeded;
        public int NumberNeeded { get { return _NumberNeeded; } }

        /// <summary>
        /// Indicates whether the requisite is required or only recommended (true/false)
        /// </summary>
        private bool _IsRequired;
        public bool IsRequired { get { return _IsRequired; } }

        /// <summary>
        /// Single section requisite, use to create a required or recommended requisite
        /// </summary>
        /// <param name="corequisiteSectionId"></param>
        /// <param name="isRequired"></param>
        public SectionRequisite(string corequisiteSectionId, bool isRequired = false)
        {
            if (string.IsNullOrEmpty(corequisiteSectionId))
            {
                throw new ArgumentNullException("corequisiteSectionId", "Must provide the corequisite Section Id.");
            }
            _CorequisiteSectionIds = new List<string>() { corequisiteSectionId };
            _IsRequired = isRequired;
            _NumberNeeded = 1;

        }

        /// <summary>
        /// Required Multi-section requisite. Used to define "x out of y requisite sections". Always required.
        /// Cannot be used when the number needed is equal to the number of sections; use single section requisite constructor instead.
        /// </summary>
        /// <param name="corequisiteSectionIds">List of required section Ids</param>
        /// <param name="numberNeeded">Number of specified sections required</param>
        public SectionRequisite(IEnumerable<string> corequisiteSectionIds, int numberNeeded)
        {
            if (corequisiteSectionIds == null || corequisiteSectionIds.Count() == 0 || corequisiteSectionIds.Count() == 1)
            {
                throw new ArgumentOutOfRangeException("corequisiteSectionIds", "Must supply a list of more than one section for a multi-section requisite.");
            }
            if (numberNeeded >= corequisiteSectionIds.Count() || numberNeeded < 1)
            {
                throw new ArgumentOutOfRangeException("numberNeeded", "Number Needed must greater than zero but less than the total number of requisite sections.");
            }
            _CorequisiteSectionIds = corequisiteSectionIds.ToList();
            _IsRequired = true;
            _NumberNeeded = numberNeeded;
        }
    }
}
