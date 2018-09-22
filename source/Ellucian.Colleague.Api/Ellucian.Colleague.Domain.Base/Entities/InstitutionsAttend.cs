// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{

    [Serializable]
    public class InstitutionsAttend
    {
        private string _guid;
        private string _id;

        private string _personId;
        private string _institutionId;


        /// <summary>
        /// Store the transcript type.
        /// </summary>
        public List<string> TranscriptType { get; set; }

        /// <summary>
        /// Date the transcript was received
        /// </summary>
        public List<DateTime?> TranscriptDate { get; set; }

        /// <summary>
        /// Status of a transcript
        /// </summary>
        public List<string> TranscriptStatus { get; set; }

        /// <summary>
        /// Graduation type to specify if it was GED or Graduated
        /// </summary>
        public string GradType { get; set; }

        /// <summary>
        /// Number of credit hours person received at this institution
        /// </summary>
        public Decimal? ExtCredits { get; set; }

        /// <summary>
        /// Contains an external GPA
        /// </summary>
        public Decimal? ExtGpa { get; set; }

        /// <summary>
        /// Rank Percentile
        /// </summary>
        public Decimal? RankPercent { get; set; }

        /// <summary>
        /// To store the numerator of the rank
        /// </summary>
        public int? RankNumerator { get; set; }

        /// <summary>
        /// To store the denominator of the Rank 
        /// </summary>
        public int? RankDenominator { get; set; }

        /// <summary>
        /// Free-form comments about the institution attend
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// pointer to the acad.credentials
        /// </summary>
        public List<string> AcadCredentials { get; set; }

        /// <summary>
        /// Entity assocation consisting of StartDate and EndDate
        /// </summary>
        public List< Tuple<DateTime?, DateTime?>> DatesAttended { get; set; }

        /// <summary>
        /// Entity assocation consisting of StartYear and EndYear
        /// </summary>
        public List<Tuple<int?, string>> YearsAttended { get; set; }

        /// <summary>
        ///  High school graduation date.
        /// </summary>
        public DateTime? InstaIntgHsGradDate { get; set; }

        /// <summary>
        /// Derived from record key
        /// </summary>
        public string PersonId
        {
            get { return _personId; }
            set
            {
                if (string.IsNullOrEmpty(_personId))
                {
                    _personId = value;
                }
                else
                {
                    throw new InvalidOperationException("InstitutionsAttend PersonId cannot be changed");
                }
            }
        }


        /// <summary>
        /// Derived from record key
        /// </summary>
        public string InstitutionId
        {
            get { return _institutionId; }
            set
            {
                if (string.IsNullOrEmpty(_institutionId))
                {
                    _institutionId = value;
                }
                else
                {
                    throw new InvalidOperationException("InstitutionsAttend InstitutionId cannot be changed");
                }
            }
        }



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
                    throw new InvalidOperationException("InstitutionsAttend Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the InstitutionsAttend; not required, but cannot be changed once assigned.
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

       
        public InstitutionsAttend(string guid, string id)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("InstitutionsAttend guid can not be null or empty");
            }
            _guid = guid;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("InstitutionsAttend id can not be null or empty");
            }
            _id = id;

            // This file has a 2-part key consisting of the person ID (INSTA.PERSON.ID) 
            // and the institution ID (INSTA.INSTITUTION.ID) separated by asterisks 
            // (e.g. 0000254*0000065).
            var splitKey = id.Split('*');
            if (splitKey.Count() != 2)
            {
                throw new ArgumentException(string.Concat("InstitutionAttend Record Key is not valid: ", id ), "source.RecordKey");
            }

            this.PersonId = splitKey[0];
            this.InstitutionId = splitKey[1];
        }
    }
}