// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how a graduation application should be rendered to a user.
    /// </summary>
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
        /// The questions that should be included in the graduation application
        /// </summary>
        public List<GraduationQuestion> ApplicationQuestions { get; set; }

        /// <summary>
        /// List of terms when graduation is offered
        /// </summary>
        public List<string> GraduationTerms { get; set; }

        /// <summary>
        /// Maximum number of guests a student can invite to commencement. Optional.
        /// </summary>
        public int? MaximumCommencementGuests { get; set; }

        /// <summary>
        /// Default Email Type for the student to receive an email
        /// </summary>
        public string DefaultWebEmailType { get; set; }

        /// <summary>
        /// Email paragraph heading provided for graduation application.
        /// </summary>
        public string EmailGradNotifyPara { get; set; }

        /// <summary>
        /// Is immediate payment required for graduation applications?
        /// </summary>
        public bool RequireImmediatePayment { get; set; }

        /// <summary>
        /// Hiding parameter for anticipateddate for a program
        /// </summary>
        public bool HideAnticipatedCompletionDate { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GraduationConfiguration()
        {
            ApplicationQuestions = new List<GraduationQuestion>();
            GraduationTerms = new List<string>();
        }
    }
}
