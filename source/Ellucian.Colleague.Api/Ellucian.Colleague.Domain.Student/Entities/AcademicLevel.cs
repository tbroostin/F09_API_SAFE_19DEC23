// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AcademicLevel : GuidCodeItem
    {
        /// <summary>
        /// Grade Scheme
        /// </summary>
        public string GradeScheme { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public bool Category { get; set; }

        public AcademicLevel(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}