// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Academic Discipline
    /// </summary>
    [Serializable]
    public class AcademicDiscipline : GuidCodeItem
    {

        public AcademicDisciplineType AcademicDisciplineType { get; set; }
        public bool? ActiveMajor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcademicDiscipline"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Academic Discipline</param>
        /// <param name="description">Description or Title of the Academic Discipline</param>
        /// <param name="academicDisciplineType">A type of Academic Discipline (Major, Minor, Concentration)</param>
        public AcademicDiscipline(string guid, string code, string description, AcademicDisciplineType academicDisciplineType)
            : base (guid, code, description)
        {
            AcademicDisciplineType = academicDisciplineType;
        }
    }
}