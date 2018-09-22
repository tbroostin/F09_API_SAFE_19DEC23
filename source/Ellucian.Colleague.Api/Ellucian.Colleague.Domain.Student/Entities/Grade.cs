// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Grade
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == null)
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// Guid
        /// </summary>        
        public string Guid { get { return _guid; } }
        private readonly string _guid;

        /// <summary>
        /// Credit 
        /// </summary>
        public string Credit { get { return _credit; } }
        private readonly string _credit;

        /// <summary>
        /// The externally appearing letter grade (Required)
        /// </summary>
        public string LetterGrade { get { return _letterGrade; } }
        private readonly string _letterGrade;

        /// <summary>
        /// The grade scheme to which this grade belongs (Required)
        /// </summary>
        public string GradeSchemeCode { get { return _gradeSchemeCode; } }
        private readonly string _gradeSchemeCode;

        /// <summary>
        /// The description for this grade (Required)
        /// </summary>
        public string Description { get { return _description; } }
        private readonly string _description;

        /// <summary>
        /// The numeric value associated with this grade
        /// </summary>
        public decimal? GradeValue { get; set; }

        /// <summary>
        /// Indicates if this grade may be assigned when a student Withdraws from a course
        /// </summary>
        public bool IsWithdraw { get; set; }

        /// <summary>
        /// Grade Priority is Repeat Value in Colleague
        /// </summary>
        public decimal? GradePriority { get; set; }

        /// <summary>
        /// Optional comparison grade
        /// </summary>
        public ComparisonGrade ComparisonGrade { get; private set; }

        /// <summary>
        /// The grade to which an incomplete will revert if not updated by the expiration date.
        /// </summary>
        public string IncompleteGrade { get; set; }

        /// <summary>
        /// Indicates if this should be excluded from the list of grades presented to faculty during grading.
        /// </summary>
        public bool ExcludeFromFacultyGrading { get; set; }

        /// <summary>
        /// Indicates if last date of attendance is required if the grade is final grade
        /// </summary>
        public bool RequireLastAttendanceDate { get; set; }

        /// <summary>
        /// Grade object constructor
        /// </summary>
        /// <param name="id">Grade ID</param>
        /// <param name="letterGrade">Letter Grade</param>
        /// <param name="description">Description for this grade</param>
        /// <param name="scheme"></param>
        public Grade(string id, string letterGrade, string description, string scheme)
            : this(letterGrade, description, scheme)
        {
            _id = id;
            ExcludeFromFacultyGrading = false;
        }

        /// <summary>
        /// Grade object constructor
        /// </summary>
        /// <param name="guid">Grade guid</param>
        /// <param name="id">Grade ID</param>
        /// <param name="letterGrade">Letter Grade</param>
        /// <param name="description">Description for this grade</param>
        /// <param name="scheme"></param>
        public Grade(string guid, string id, string letterGrade, string credit, string description, string scheme)
            : this(letterGrade, description, scheme)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            _guid = guid;
            _credit = credit;
            _id = id;
            ExcludeFromFacultyGrading = false;
        }

        public Grade(string letterGrade, string description, string scheme)
        {
            if (string.IsNullOrEmpty(letterGrade))
            {
                throw new ArgumentNullException("letterGrade");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (scheme == null)
            {
                throw new ArgumentNullException("scheme");
            }
            _letterGrade = letterGrade;
            _description = description;
            _gradeSchemeCode = scheme;
            IsWithdraw = false;
            ExcludeFromFacultyGrading = false;
            RequireLastAttendanceDate = false;
        }

        /// <summary>
        /// Set the ComparisonGrade property
        /// </summary>
        /// <param name="comparisonGradeId">Grade ID of the comparison grade</param>
        /// <param name="comparisonGradeValue">Value of the comparison grade</param>
        /// <param name="comparisonGradeSchemeCode">Grade scheme of the comparison grade</param>
        public void SetComparisonGrade(string comparisonGradeId, decimal? comparisonGradeValue, string comparisonGradeSchemeCode)
        {
            // _gradeSchemeCode is an immutable property, and was already set in a constructor
            if (comparisonGradeSchemeCode == this._gradeSchemeCode)
            {
                throw new ArgumentException("The comparison grade cannot be in the same grade scheme as the grade it belongs to.");
            }

            this.ComparisonGrade = new ComparisonGrade(comparisonGradeId, comparisonGradeValue, comparisonGradeSchemeCode);
        }

        /// <summary>
        /// Return a new Grade object which is a deep copy of this object.
        /// </summary>
        /// <returns>A deep copy of the Grade object</returns>
        public Grade DeepCopy()
        {
            // Copy all properties
            Grade copyGrade = (Grade) this.MemberwiseClone();
            // Make a new instance of ComparisonGrade.
            if (this.ComparisonGrade != null)
            {
                copyGrade.ComparisonGrade = new ComparisonGrade(this.ComparisonGrade.ComparisonGradeId, this.ComparisonGrade.ComparisonGradeValue, this.ComparisonGrade.ComparisonGradeSchemeCode);
            }

            return copyGrade;
        }

        /// <summary>
        /// Test whether the value of the current Grade object is great than or equal to the value of a supplied Grade object.
        /// This applies the comparison grade of each Grade when appropriate.
        ///
        /// This handles a null grade value in the same way that Unibasic handles blanks, so that the 
        /// so that min grade comparison made by My Progress produces the same result as Envision EVAL.
        /// 
        /// </summary>
        /// <param name="compGrade">The Grade to compare to this Grade object</param>
        /// <returns>True if the current Grade object is >= the supplied Grade, else False</returns>
        public Boolean IsGreaterOrEqualUsingComparisonGrade( Grade compGrade)
        {
            Boolean result;

            if (compGrade == null)
            {
                // If no grade to compare was specified, consider the current grade >= the specified grade.
                result = true;
            } else
            {
                // If the two grades are of the same scheme, simply compare their values
                if (this.GradeSchemeCode == compGrade.GradeSchemeCode)
                {
                    result = GreaterOrEqualUnibasic(this.GradeValue, compGrade.GradeValue);
                } else
                {
                    // Since the two grades are from different grade schemes, use the value of the comparison grade if a comparison
                    // grade is specified. It is allowed that one grade have a comparison grade, while the other does not.
                    result = GreaterOrEqualUnibasic((this.ComparisonGrade == null ? this.GradeValue : this.ComparisonGrade.ComparisonGradeValue),
                            (compGrade.ComparisonGrade == null ? compGrade.GradeValue : compGrade.ComparisonGrade.ComparisonGradeValue));
                }
            }
            return result;
        }        

        /// <summary>
        /// Implement a >= comparison of two decimal? values, handling nulls in the same way that Unibasic handles
        /// blanks when making the same comparison.
        /// Evalutes (firstVal >= secondVal)
        /// This is used to yield the same results as comparable Envision code when comparing two grade values, given that
        /// the grade value can be blank in the database.
        /// </summary>
        /// <param name="firstVal">The first value in the comparison</param>
        /// <param name="secondVal">The second value in the comparison</param>
        /// <returns></returns>
        private Boolean GreaterOrEqualUnibasic( decimal? firstVal, decimal? secondVal)
        {
            Boolean result;

            // The Unibasic >= comparison always returns true if both values are blank. 
            if ((firstVal == null) && (secondVal == null))
            {
                result = true;

            // Unibasic always consider a blank a less than a value, even a negative value
            } else if (firstVal == null) {
                result = false;
            } else if (secondVal == null) {
                result = true;
            } else
            {
                // A normal comparison
                result = (firstVal >= secondVal);
            }

            return result;
        }

        /// <summary>
        /// Test whether this grade object is equivalent to the supplied grade object, using the comparison grade as appropriate.
        /// 
        /// </summary>
        /// <param name="compGrade">The Grade to compare to this Grade object</param>
        /// <returns>True if the two grade objects are equivalent, else False</returns>
        public Boolean IsEquivalentUsingComparisonGrade(Grade compGrade)
        {
            Boolean result;

            if (compGrade == null)
            {
                result = false;
            }
            else
            {
                // If the two grades are of the same scheme, simply compare the grade IDs
                if (this.GradeSchemeCode == compGrade.GradeSchemeCode)
                {
                    result = (this.Id == compGrade.Id);
                }
                else
                {
                    // Since the two grades are from different grade schemes, use the value of the comparison grade if a comparison
                    // grade is specified. It is allowed that one grade have a comparison grade, while the other does not.
                    result = ((this.ComparisonGrade == null ? this.Id : this.ComparisonGrade.ComparisonGradeId) ==
                            (compGrade.ComparisonGrade == null ? compGrade.Id : compGrade.ComparisonGrade.ComparisonGradeId));
                }
            }
            return result;
        }

    }


    /// <summary>
    /// Information about a comparison grade that can be optionally assigned to a grade.
    /// A comparison grade is a grade in the grades scheme specified in ACD.COMPARISON.GRADE.SCHEME of AC.DEFAULTS.
    /// When comparing a grade to a minimum grade or allowed grades list and the two grades being compared are
    /// from different grade schemes, the comparison grades will be compared instead if they have been specified.
    /// 
    /// This class contains attributes of the comparison GRADES record that are relevant to its use in mininum grade
    /// evaluation.
    /// </summary>
    [Serializable]
    public class ComparisonGrade
    {
        /// <summary>
        /// ID of the comparison grade
        /// </summary>
        public string ComparisonGradeId { get; set; }

        /// <summary>
        /// GradeValue value of the comparison grade
        /// </summary>
        public decimal? ComparisonGradeValue { get; set; }

        /// <summary>
        /// Grade scheme code of the comparison grade
        /// </summary>
        public string ComparisonGradeSchemeCode { get; set; }

        public ComparisonGrade(string comparisonGradeId, decimal? comparisonGradeValue, string comparisonGradeSchemeCode)
        {
            if (string.IsNullOrEmpty(comparisonGradeId))
            {
                throw new ArgumentNullException("comparisonGradeId is required");
            }

            if (string.IsNullOrEmpty(comparisonGradeSchemeCode))
            {
                throw new ArgumentNullException("comparisonGradeSchemeCode is required");
            }

            ComparisonGradeId = comparisonGradeId;
            ComparisonGradeValue = comparisonGradeValue;
            ComparisonGradeSchemeCode = comparisonGradeSchemeCode;
        }
    }
}