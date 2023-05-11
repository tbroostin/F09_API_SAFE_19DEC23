// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Abstract base requirement evaluation result
    /// </summary>
    public abstract class BaseResult
    {
        /// <summary>
        /// Status based on credits completed <see cref="CompletionStatus"/>
        /// </summary>
        public CompletionStatus CompletionStatus { get; set; }
        /// <summary>
        /// Status based on credits planned <see cref="PlanningStatus"/>
        /// </summary>
        public PlanningStatus PlanningStatus { get; set; }
        /// <summary>
        /// Calculated GPA
        /// </summary>
        public decimal Gpa { get; set; }
        /// <summary>
        /// Messages
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
        public List<string> ModificationMessages { get; set; }
        /// <summary>
        /// Indicates when requirement is not complete because requirement GPA is lower than the stated threshold
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
        public bool MinGpaIsNotMet { get; set; }
        /// <summary>
        /// The total number of institution credits that have been applied 
        /// </summary>
        public decimal InstitutionalCredits { get; set; }
        /// <summary>
        /// Indicates when the requirement is not complete because the number of institutional credits is below the required number.
        /// </summary>
        public bool MinInstitutionalCreditsIsNotMet { get; set; }
        /// <summary>
        /// Constructor for abstract BaseResult
        /// </summary>
        protected BaseResult()
        {

        }

    }
}
