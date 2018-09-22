// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{

    [Serializable]
    public class AcademicCredential
    {

        private string _guid;
        private string _id;

        /// <summary>
        /// ACAD.PERSON.ID
        /// </summary>       
        public string AcadPersonId { get; set; }

        /// <summary>
        /// ACAD.INSTITUTIONS.ID
        /// </summary>        
        public string AcadInstitutionsId { get; set; }

        /// <summary>
        ///  ACAD.START.DATE
        /// </summary>        
        public DateTime? AcadStartDate { get; set; }

        /// <summary>
        /// ACAD.END.DATE
        /// </summary>      
        public DateTime? AcadEndDate { get; set; }

        /// <summary>
        /// ACAD.DEGREE
        /// </summary>
         public string AcadDegree { get; set; }

        /// <summary>
        ///  ACAD.DEGREE.DATE
        /// </summary>
         public DateTime? AcadDegreeDate { get; set; }

        /// <summary>
        /// ACAD.CCD
        /// </summary>
       public List<string> AcadCcd { get; set; }

        /// <summary>
        /// ACAD.MAJORS
        /// </summary>
        public List<string> AcadMajors { get; set; }

        /// <summary>
        /// ACAD.MINORS
        /// </summary>
        public List<string> AcadMinors { get; set; }

        /// <summary>
        /// ACAD.SPECIALIZATION
        /// </summary>
         public List<string> AcadSpecialization { get; set; }

        /// <summary>
        /// ACAD.HONORS
        /// </summary>
        public List<string> AcadHonors { get; set; }

        /// <summary>
        /// ACAD.AWARDS
        /// </summary>
         public List<string> AcadAwards { get; set; }

        /// <summary>
        ///  ACAD.NO.YEARS
        /// </summary>
       public int? AcadNoYears { get; set; }

        /// <summary>
        /// ACAD.COMMENCEMENT.DATE
        /// </summary>
         public DateTime? AcadCommencementDate { get; set; }

        /// <summary>
        /// ACAD.GPA
        /// </summary>
         public Decimal? AcadGpa { get; set; }

        /// <summary>
        /// ACAD.THESIS
        /// </summary>
         public string AcadThesis { get; set; }

        /// <summary>
        /// ACAD.COMMENTS
        /// </summary>
        public string AcadComments { get; set; }

        /// <summary>
        /// ACAD.ACAD.PROGRAM
        /// </summary>
        public string AcadAcadProgram { get; set; }

        /// <summary>
        /// RankDenominator
        /// </summary>
        public int? AcadRankDenominator { get; set; }

        /// <summary>
        /// RankPercent
        /// </summary>
        public decimal? AcadRankPercent { get; set; }

        /// <summary>
        /// RankNumerator
        /// </summary>
        public int? AcadRankNumerator { get; set; }
        
        /// <summary>
        /// CCD Dates
        /// </summary>
        public List<DateTime?> AcadCddDate { get; set; }


        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("AcademicCredential Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the remark; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

       

        public AcademicCredential(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("AcademicCredential guid can not be null or empty");
            }
            _guid = guid;
        }
    }
}