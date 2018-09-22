// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentSectionWaitlist
    {
        private readonly string _guid;
        private readonly string _PersonId;
        private readonly string _SectionId;
        private readonly int?   _Priority;

        /// <summary>
        /// Guid
        /// </summary>        
        public string Guid { get { return _guid; } }

        /// <summary>
        /// GUID of the associated section
        /// </summary>
        public string SectionId { get { return _SectionId; } }

        /// <summary>
        /// GUID of the associated Person 
        /// </summary>
        public string PersonId { get { return _PersonId; } }
        
        /// <summary>
        /// Priority rating
        /// </summary>
        public int? Priority { get { return _Priority; } }


        /// <summary>
        /// Initialize the StudentProgram Method
        /// </summary>
        /// <param name="guid">Guid for this waitlist record</param>
        /// <param name="personId">Person guid </param>
        /// <param name="sectionId">Section guid</param>
        /// <param name="priority">The priority rating from the waitlist record</param>
        public StudentSectionWaitlist(string guid, string personId, string sectionId, int? priority)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            _guid = guid;
            _PersonId = personId;
            _SectionId = sectionId;
            _Priority = priority;
            
        }
    }
}
