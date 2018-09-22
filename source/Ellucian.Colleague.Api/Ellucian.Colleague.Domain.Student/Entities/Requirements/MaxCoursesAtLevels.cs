// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// "MAXIMUM 5 100,200 LEVEL COURSES"
    /// </summary>
    [Serializable]
    public class MaxCoursesAtLevels : MaxAtLevels
    {
        private readonly int _MaxCourses;
        public int MaxCourses { get { return _MaxCourses; } }

        public MaxCoursesAtLevels(int max, ICollection<string> levels)
            : base(levels)
        {
            _MaxCourses = max;
        }

    }
}
