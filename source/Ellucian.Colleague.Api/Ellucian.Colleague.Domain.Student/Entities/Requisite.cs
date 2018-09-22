// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Requirements (requisites) related to registering for a course or section. For example, when taking BIOL-100
    /// you are required to take BIOL-101L concurrently. A requisite must either have a requirement code defined
    /// or a course corequisite. 
    ///     
    ///     Requirement-based: Can be defined on a course or section, uses DA syntax to define the
    ///     requisite. RequirementCode, IsRequired and CompletionOrder must be supplied to the constructor
    ///     
    ///     Course CoRequisite: Temporary form of requisite, will only be present only until this type of requisite
    ///     is no longer available in Colleague and then this will be an unused property.
    /// </summary>
    [Serializable]
    public class Requisite
    {
        /// <summary>
        /// RequirementCode  
        /// </summary>
        private string _RequirementCode;
        public string RequirementCode { get { return _RequirementCode; } }

        /// <summary>
        /// Indicates whether the requisite is required or only recommended (true/false)
        /// </summary>
        private bool _IsRequired;
        public bool IsRequired { get { return _IsRequired; } }

        /// <summary>
        /// Defines whether the requisite must be completed prior to, concurrent with, or either
        /// </summary>
        private RequisiteCompletionOrder _CompletionOrder;
        public RequisiteCompletionOrder CompletionOrder { get { return _CompletionOrder; } }

        /// <summary>
        /// Temporary: Only needed when Colleague is still in old format. This will ONLY be filled in if Requirement is blank
        /// </summary>
        private string _CorequisiteCourseId;
        public string CorequisiteCourseId { get { return _CorequisiteCourseId; } }

        /// <summary>
        /// Indicates whether the requisite is protected on the course and therefore cannot be overridden at the section level (true/false)
        /// </summary>
        private bool _IsProtected;
        public bool IsProtected { get { return _IsProtected; } }

        /// <summary>
        /// Primary constructor for Requisites. Moving forward - once Colleague converts to the new format of requisites - this 
        /// will be the main way requisites are created
        /// </summary>
        /// <param name="requirementCode">Requirement Code (required)</param>
        /// <param name="IsRequired">Whether or not the requisite is required or just recommended</param>
        /// <param name="completionOrder">Describes whether the requisite must be met prior to or concurrent with </param>
        public Requisite(string requirementCode, bool isRequired, RequisiteCompletionOrder completionOrder, bool isProtected)
        {
            if (String.IsNullOrEmpty(requirementCode))
            {
                throw new ArgumentNullException("requirementCode", "Must provide the requirement code.");
            }
            _RequirementCode = requirementCode;
            _IsRequired = isRequired;
            _CompletionOrder = completionOrder;
            _IsProtected = isProtected;
        }

        /// <summary>
        /// This constructor would be used to set up a corequisite course when Colleague is still in old format - using Corequisites with course Ids.
        /// Completion Order will always be Concurrent. May pertain to a course corequisite defined for either a course or section.
        /// </summary>
        /// <param name="corequisiteCourseId">Id of corequisite course</param>
        /// <param name="isRequired">Indicate whether this course is a required corequisite</param>
        public Requisite(string corequisiteCourseId, bool isRequired)
        {
            if (String.IsNullOrEmpty(corequisiteCourseId))
            {
                throw new ArgumentNullException("corequisiteCourseId", "Must provide the corequisite Course Id when building an old format coreq version of the requisite.");
            }
            _CorequisiteCourseId = corequisiteCourseId;
            _IsRequired = isRequired;
            _CompletionOrder = RequisiteCompletionOrder.PreviousOrConcurrent;
            _IsProtected = false;
        }

    }
}
