// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Response from the repository method to update a list of grade data items for a single section
    /// </summary>
    [Serializable]
    public class SectionGradeSectionResponse
    {

        /// <summary>
        ///  The results of each individual student's grade data update
        /// </summary>
        public List<SectionGradeResponse> StudentResponses {get; set;}

        public List<String> InformationalMessages { get; set; }

        public SectionGradeSectionResponse()
        {
            StudentResponses = new List<SectionGradeResponse>();
            InformationalMessages = new List<String>();
        }
    }
}
