// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A grade scheme
    /// </summary>
    [Serializable]
    public class GradeScheme
    {
        /// <summary>
        /// Grade Scheme Code
        /// </summary>        
        public string Code { get { return _code; } }
        private readonly string _code;

        /// <summary>
        /// Grade Scheme Description
        /// </summary>        
        public string Description { get { return _description; } }
        private readonly string _description;

        private string _Guid;
        /// <summary>
        /// GUID for the grade scheme; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// The date on which the grade scheme becomes valid.
        /// </summary>
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// The date after which the grade scheme is no longer valid.
        /// </summary>
        public DateTime? EffectiveEndDate { get; set; }

        private List<string> _gradeCodes = new List<string>();
        ///<summary>
        /// Grade Codes assigned to the grade scheme
        /// </summary>
        public ReadOnlyCollection<string> GradeCodes { get; private set; }

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="GradeScheme"/> object
        /// </summary>
        /// <param name="guid">Grade scheme GUID</param>
        /// <param name="code">Grade scheme code</param>
        /// <param name="description">Grade scheme description</param>
        public GradeScheme(string guid, string code, string description)
            : this(code, description)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("code", "A GUID is required for a grade scheme.");
            }
            Guid = guid;
        }

        /// <summary>
        /// Creates a new <see cref="GradeScheme"/> object
        /// </summary>
        /// <param name="code">Grade scheme code</param>
        /// <param name="description">Grade scheme description</param>
        public GradeScheme(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A grade scheme code is required for a grade scheme.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "A grade scheme description is required for a grade scheme.");
            }
            _code = code;
            _description = description;
            GradeCodes = _gradeCodes.AsReadOnly();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a grade code to this grade scheme
        /// </summary>
        /// <param name="code">Grade code to add</param>
        public void AddGradeCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Cannot add a null or empty grade code to a grade scheme.");
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
            GradeScheme other = obj as GradeScheme;
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
