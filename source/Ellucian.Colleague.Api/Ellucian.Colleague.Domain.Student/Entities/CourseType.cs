// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CourseType : GuidCodeItem
    {
        /// <summary>
        /// Flag indicates whether this course type is a searchable or
        /// filterable type when searching for courses or course sections in the catalog. True by default.
        /// </summary>
        private readonly bool _showInCourseSearch;
        public bool ShowInCourseSearch { get { return _showInCourseSearch; } }
        
       
        public string Categorization { get; set; }

        public CourseType(string guid, string code, string description, bool showInCourseSearch = true)
            : base(guid, code, description)
        {
            _showInCourseSearch = showInCourseSearch;
        }

    }
}
