// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A grade subscheme
    /// </summary>
    [Serializable]
    public class GradeSubscheme
    {
        /// <summary>
        /// Grade Subscheme Code
        /// </summary>        
        public string Code { get { return _code; } }
        private readonly string _code;

        /// <summary>
        /// Grade Subscheme Description
        /// </summary>        
        public string Description { get { return _description; } }
        private readonly string _description;

        private List<string> _gradeCodes = new List<string>();
        ///<summary>
        /// Grade Codes assigned to the grade subscheme
        /// </summary>
        public ReadOnlyCollection<string> GradeCodes { get; private set; }

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="GradeSubscheme"/> object
        /// </summary>
        /// <param name="code">Grade subscheme code</param>
        /// <param name="description">Grade subscheme description</param>
        public GradeSubscheme(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A grade subscheme code is required for a grade subscheme.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "A grade subscheme description is required for a grade subscheme.");
            }
            _code = code;
            _description = description;
            GradeCodes = _gradeCodes.AsReadOnly();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a grade code to this grade subscheme
        /// </summary>
        /// <param name="code">Grade code to add</param>
        public void AddGradeCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Cannot add a null or empty grade code to a grade subcheme.");
            }
            if (!_gradeCodes.Contains(code))
            {
                _gradeCodes.Add(code);
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            GradeSubscheme other = obj as GradeSubscheme;
            if (other == null)
            {
                return false;
            }
            return other.Code.Equals(Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
        #endregion
    }
}
