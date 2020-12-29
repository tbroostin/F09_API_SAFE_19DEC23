// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.QuickRegistration
{
    /// <summary>
    /// A student's previously selected course sections for a given academic term that may be used in the Colleague Self-Service Quick Registration workflow
    /// </summary>
    [Serializable]
    public class QuickRegistrationTerm
    {
        /// <summary>
        /// Code of the academic term for which the student previously selected course sections to register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public string TermCode { get; private set; }

        /// <summary>
        /// List of course sections for which the student could potentially register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public ReadOnlyCollection<QuickRegistrationSection> Sections { get; private set; }
        private readonly List<QuickRegistrationSection> _sections = new List<QuickRegistrationSection>();

        /// <summary>
        /// Creates a new <see cref="QuickRegistration"/> object
        /// </summary>
        /// <param name="termCode">Code of the academic term for which the student previously selected course sections to register via the Colleague Self-Service Quick Registration workflow</param>
        public QuickRegistrationTerm(string termCode)
        {
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "A term code is required when building a quick registration term.");
            }
            TermCode = termCode;
            Sections = _sections.AsReadOnly();
        }

        /// <summary>
        /// Add a course section to the list of course sections for the quick registration term
        /// </summary>
        /// <param name="section">Unique course section</param>
        public void AddSection(QuickRegistrationSection section)
        {
            if(section == null)
            {
                throw new ArgumentNullException("section", "Section cannot be null or empty on a quick registration term.");
            }
            if(!_sections.Any(s => s.SectionId == section.SectionId))
            {
                _sections.Add(section);
            }
        }
    }
}
