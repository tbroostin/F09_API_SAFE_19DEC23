// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{

    [Serializable]
    public class ExternalEducation
    {
        private string _guid;
        private string _id;

        public string AcadPersonId { get; set; }
        public string AcadInstitutionsId { get; set; }
        public DateTime? AcadStartDate { get; set; }
        public DateTime? AcadEndDate { get; set; }
        public string AcadDegree { get; set; }
        public DateTime? AcadDegreeDate { get; set; }
        public List<string> AcadCcd { get; set; }
        public List<DateTime?> AcadCcdDate { get; set; }
        public List<string> AcadMajors { get; set; }
        public List<string> AcadMinors { get; set; }
        public List<string> AcadSpecialization { get; set; }
        public List<string> AcadHonors { get; set; }
        public List<string> AcadAwards { get; set; }
        public int? AcadNoYears { get; set; }
        public DateTime? AcadCommencementDate { get; set; }
        public Decimal? AcadGpa { get; set; }
        public string AcadThesis { get; set; }
        public string AcadComments { get; set; }
        public string AcadAcadProgram { get; set; }
        public Decimal? AcadRankPercent { get; set; }
        public int? AcadRankNumerator { get; set; }
        public int? AcadRankDenominator { get; set; }
        public List<DateTime?> InstTransciptDate { get; set; }
        public Decimal? InstExtCredits { get; set; }
        public string InstAttendGuid { get; set; }

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

        public ExternalEducation(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("ExternalEducation guid can not be null or empty");
            }
            _guid = guid;
        }

        public ExternalEducation(string id, string guid)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("ExternalEducation id can not be null or empty");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("ExternalEducation guid can not be null or empty");
            }
            _guid = guid;
            _id = id;
        }
    }
}