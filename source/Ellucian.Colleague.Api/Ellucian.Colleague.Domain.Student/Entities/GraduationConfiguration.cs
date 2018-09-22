// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how a graduation application should be rendered to a user.
    /// </summary>
    [Serializable]
    public class GraduationConfiguration
    {
        /// <summary>
        /// Link to a page or website with complete information about graduation and commencement 
        /// </summary>
        public string CommencementInformationLink { get; set; }
        
        /// <summary>
        /// Link to a website with information on how to enter a phonetic spelling for a name
        /// </summary>
        public string PhoneticSpellingLink { get; set; }
        
        /// <summary>
        /// Link to a website where cap and gowns may be ordered
        /// </summary>
        public string CapAndGownLink { get; set; }

        /// <summary>
        /// Link to a website where cap and gowns sizing information is available
        /// </summary>
        public string CapAndGownSizingLink { get; set; }

        /// <summary>
        /// This boolean will indicate the intention to override the display of the Cap and Gown Questions within the Graduation Application.
        /// If the Client has decided to allow Student to see the Cap and Gown questions when they are NOT attending the
        /// Commencement Ceremony, this should be set to true.
        /// </summary>
        public bool OverrideCapAndGownDisplay { get; set; }
        
        /// <summary>
        /// Link to a page or website with information on how to apply for a program other than one of the student's current programs.
        /// </summary>
        public string ApplyForDifferentProgramLink { get; set; }

        /// <summary>
        /// List of terms for which a student can currently apply for graduation
        /// </summary>
        public ReadOnlyCollection<string> GraduationTerms { get; private set; }
        private readonly List<string> _graduationTerms = new List<string>();

        /// <summary>
        /// Maximum number of guests a student can invite to commencement. Optional.
        /// </summary>
        public int? MaximumCommencementGuests { get; set; }

        /// <summary>
        /// The characteristics for optional questions on the graduation application
        /// </summary>
        public ReadOnlyCollection<GraduationQuestion> ApplicationQuestions { get; private set; }
        private readonly List<GraduationQuestion> _applicationQuestions = new List<GraduationQuestion>();

        /// <summary>
        /// Default Email Type for the student to receive an email
        /// </summary>
        public string DefaultWebEmailType { get; set; }

        /// <summary>
        ///Email paragraph heading provided for graduation application.
        /// </summary>
        public string EmailGradNotifyPara { get; set; }

        /// <summary>
        /// Is immediate payment required for graduation applications.
        /// </summary>
        public bool RequireImmediatePayment { get; set; }

        /// <summary>
        /// Constructor for GraduationConfiguration
        /// </summary>
        public GraduationConfiguration()
        {
            ApplicationQuestions = _applicationQuestions.AsReadOnly();
            GraduationTerms = _graduationTerms.AsReadOnly();
            RequireImmediatePayment = false;
        }

        public void AddGraduationTerm(string graduationTermCode)
        {
            if (string.IsNullOrEmpty(graduationTermCode))
            {
                throw new ArgumentNullException("graduationTermCode", "Graduation Term Code must be specified");
            }
            if (!GraduationTerms.Any(r => r.Equals(graduationTermCode)))
            {
                _graduationTerms.Add(graduationTermCode);
            }
        }

        public void AddGraduationQuestion(GraduationQuestionType type, bool isRequired)
        {
            // Can only have one question in the list with a specific type
            if (!ApplicationQuestions.Any(a => a.Type.Equals(type)))
            {
                GraduationQuestion newQuestion = new GraduationQuestion(type, isRequired);
                _applicationQuestions.Add(newQuestion);
            }
        }
    }
}
