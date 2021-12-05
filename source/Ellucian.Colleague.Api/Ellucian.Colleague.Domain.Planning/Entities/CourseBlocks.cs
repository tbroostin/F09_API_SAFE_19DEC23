// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    [Serializable]
    public class CourseBlocks
    {
        /// <summary>
        /// Description of this course block
        /// </summary>
        private readonly string _Description;
        public string Description { get { return _Description; } }

        /// <summary>
        /// List of Course Ids in this course block
        /// </summary>
        private List<string> _CourseIds = new List<string>();
        public List<string> CourseIds { get { return _CourseIds; } }

        /// <summary>
        /// List of Course Placeholder Ids in this course block
        /// </summary>
        private List<string> _CoursePlaceholderIds = new List<string>();
        public List<string> CoursePlaceholderIds { get { return _CoursePlaceholderIds; } }

        /// <summary>
        /// Course Block constructor.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="courseIds">List of courses in this block</param>
        /// <param name="coursePlaceholderIds">Optional list of course placeholders in this block </param>
        public CourseBlocks(string description, IEnumerable<string> courseIds, IEnumerable<string> coursePlaceholderIds)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            if (courseIds != null)
            {
                _CourseIds = courseIds.Where(c => (!string.IsNullOrWhiteSpace(c))).ToList();
            }

            if (coursePlaceholderIds != null)
            {
                _CoursePlaceholderIds = coursePlaceholderIds.Where(cp => (!string.IsNullOrWhiteSpace(cp))).ToList();
            }

            if (_CourseIds.Count == 0 && _CoursePlaceholderIds.Count == 0)
            {
                throw new ArgumentNullException("courseIds", "Either courseIds or coursePlaceholderIds are required, both cannot both be empty.");
            }

            _Description = description;
        }
    }
}
