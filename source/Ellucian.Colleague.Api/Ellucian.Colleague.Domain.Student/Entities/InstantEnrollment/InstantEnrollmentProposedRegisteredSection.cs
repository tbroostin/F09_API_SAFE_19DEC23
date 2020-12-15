// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the section and related information for a section in which an 
    /// individual has successfully enrolled.
    /// </summary>
    public class InstantEnrollmentProposedRegisteredSection
    {
        public InstantEnrollmentProposedRegisteredSection(string sectionId, double? sectionCost)
        {
            if(sectionId==null)
            {
                throw new ArgumentNullException("sectionId", "sectionId cannot be null");
            }
            SectionId = sectionId;
            SectionCost = sectionCost.HasValue ? sectionCost.Value : 0;
        }

        /// <summary>
        /// The identifier of the section of interest.
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// The cost of the section.
        /// </summary>
        public double SectionCost { get; private set; }
    }
}
