// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class Program
    {

        /// <summary>
        /// Program Code such as ENGL.BA (required)
        /// </summary>
        private string _Code;
        public string Code { get { return _Code; } }

        /// <summary>
        /// Title such as Bachelor of Arts in English (required)
        /// </summary>
        private string _Title;
        public string Title { get { return _Title; } }

        /// <summary>
        /// Description of the program 
        /// </summary>
        private string _Description;
        public string Description { get { return _Description; } }

        /// <summary>
        /// Departments this programs is associated with (one of these is required)
        /// </summary>
        private List<string> _Departments;
        public List<string> Departments { get { return _Departments; } }

        /// <summary>
        /// Filters credits that can apply to this program by academic level (required) and other factors (usually not)
        /// Equivalent to a Colleague "Transcript Grouping"
        /// </summary>
        private CreditFilter _CreditFilter;
        public CreditFilter CreditFilter { get { return _CreditFilter; } }

        /// <summary>
        /// Academic level of the program 
        /// </summary>
        private string _AcademicLevelCode;
        public string AcademicLevelCode { get { return _AcademicLevelCode; } }

        private bool _IsActive;
        public bool IsActive { get { return _IsActive; } }

        private bool _IsGraduationAllowed;
        public bool IsGraduationAllowed { get { return _IsGraduationAllowed; } }

        public bool IsSelectable { get; set; }

        public int? MonthsToComplete { get; set; }

        public List<string> Majors { get; set; }
        public List<string> Minors { get; set; }
        public List<string> Ccds { get; set; }
        public List<string> Specializations { get; set; }

        public string Degree { get; set; }

        /// <summary>
        /// List of catalogs in which this program is valid
        /// </summary>
        public List<string> Catalogs { get; set; }

        /// <summary>
        /// Anticiapted Completion Date of the Program
        /// </summary>
        public DateTime? AnticipatedCompletionDate { get; set; }
        /// <summary>
        /// Start Date of Student Program
        /// </summary>
        public DateTime? ProgramStartDate { get; set; }
        /// <summary>
        /// End Date of Student Program (when inactive or graduated).
        /// </summary>
        public DateTime? ProgramEndDate { get; set; }
        /// <summary>
        /// List of programs related to this program for fastest path to completion
        /// </summary>
        public List<string> RelatedPrograms { get; set; }

        /// <summary>
        /// The transcript grouping associated to this program
        /// </summary>
        public string TranscriptGrouping { get; set; }

        /// <summary>
        /// The transcript grouping used to display an unofficial transcript for this program. 
        /// </summary>
        public string UnofficialTranscriptGrouping { get; set; }

        /// <summary>
        /// The transcript grouping used to display an uofficial transcript for this program. 
        /// </summary>
        public string OfficialTranscriptGrouping { get; set; }

        /// <summary>
        /// Locations where this program is offered.
        /// </summary>
        public List<string> Locations { get; set; }

        /// <summary>
        /// Constructs an academic program.
        /// </summary>
        /// <param name="code">The unique code for this program</param>
        /// <param name="title">The title of this program</param>
        /// <param name="depts">List of departments to which this program belongs</param>
        /// <param name="isActive">Boolean indicates whether this is an active program</param>
        /// <param name="academicLevelCode">The academic level of the program</param>
        /// <param name="creditfilter">The credit filter object to describe all the included/excluded items for evaluation of this program</param>
        /// <param name="description">The program description</param>
        public Program(string code, string title, IEnumerable<string> depts, bool isActive, string academicLevelCode, CreditFilter creditfilter, bool isGraduationAllowed, string description = null, IEnumerable<string> locations=null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Program code is required for a program");
            }
            _Code = code;
            if (string.IsNullOrEmpty(title)) 
            {
                throw new ArgumentNullException("title", "Title is required for a program");
            }
            _Title = title;
            if (depts==null || !depts.Any())
            {
                throw new ArgumentNullException("depts", "At least one department code is required for a program");
            }
            if (string.IsNullOrEmpty(academicLevelCode))
            {
                throw new ArgumentNullException("academicLevelCode", "Academic level is required for a program");
            }
            if (creditfilter == null)
            {
                throw new ArgumentNullException("creditfilter", "Credit filter is required for a program");
            }

            _IsActive = isActive;
            _IsGraduationAllowed = isGraduationAllowed;
            _Departments = depts.ToList();
            _Description = description ?? "";
            _AcademicLevelCode = academicLevelCode;
            _CreditFilter = creditfilter;

            Catalogs = new List<string>();
            Majors = new List<string>();
            Minors = new List<string>();
            Ccds = new List<string>();
            Specializations = new List<string>();
            RelatedPrograms = new List<string>();
            Locations = new List<string>();

            if (null != locations)
            {
                this.Locations = locations.ToList<string>();
            }

        }


        public override string ToString()
        {
            return Code;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Program other = obj as Program;
            if (other == null)
            {
                return false;
            }
            return (other.Code == Code && other.Title == Title);
        }

        public override int GetHashCode()
        {
            if (Code == null)
            {
                return base.GetHashCode();
            }

            return Code.GetHashCode();
        }

        /// <summary>
        /// Get the current catalog code for this program. 
        /// This is the code with a start date closest to today that is not in the future, and with no end date, or an end date in the future.
        /// </summary>
        /// <param name="allCatalogs">Collection of all Catalogs - needed to get the start dates</param>
        /// <returns>Most recent catalog year for the program (can be null)</returns>
        public string GetCurrentCatalogCode(ICollection<Catalog> allCatalogs)
        {
            // Get the most recent (not future) catalog year for the program
            string currentCatalogCode = null;
            List<Catalog> programCatalogs = new List<Catalog>();
            foreach (var catalogCode in Catalogs)
            {
                Catalog cat = allCatalogs.Where(c => c.Code == catalogCode).FirstOrDefault();
                if (cat != null)
                {
                    programCatalogs.Add(cat);
                }
            }
            Catalog currentCatalog = programCatalogs.Where(c => c.EndDate == null || c.EndDate > DateTime.Now).Where(cx => cx.StartDate <= DateTime.Now).OrderByDescending(s => s.StartDate).FirstOrDefault();
            if (currentCatalog != null)
            {
                currentCatalogCode = currentCatalog.Code;
            }
            return currentCatalogCode;
        }
    }
}
