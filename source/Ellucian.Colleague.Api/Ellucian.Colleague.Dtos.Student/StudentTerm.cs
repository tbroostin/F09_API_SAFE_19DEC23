// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Term and Academic Level information
    /// </summary>
    public class StudentTerm
    {
        /// <summary>
        /// Reference to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Term associated to this student data
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Academic Level associated to this student data
        /// </summary>
        public string AcademicLevel { get; set; }
        /// <summary>
        /// Student Load, such as Full Time, Part Time, etc.
        /// </summary>
        public string StudentLoad { get; set; }
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public StudentTerm()
        {

        }
    }
}
