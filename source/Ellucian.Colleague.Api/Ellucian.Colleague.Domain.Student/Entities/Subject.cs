// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Subject : GuidCodeItem
    {
        /// <summary>
        /// Flag indicates whether this subject or courses associated with this
        /// subject will be available in the course catalog search.
        /// </summary>
        private readonly bool _ShowInCourseSearch;
        public bool ShowInCourseSearch { get { return _ShowInCourseSearch; } }

        public string IntgDepartment { get; set; }

        public Subject(string guid, string code, string description, bool showInCourseSearch) 
            : base(guid, code, description)
        {
            _ShowInCourseSearch = showInCourseSearch;
        }
    }
}
