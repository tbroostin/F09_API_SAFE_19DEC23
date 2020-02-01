// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
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
        /// List of Ids in this course block
        /// </summary>
        private List<string> _CourseIds = new List<string>();
        public List<string> CourseIds { get { return _CourseIds; } }

        /// <summary>
        /// Course Block constructor.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="courseIds">List of courses in this block</param>
        /// <param name="termCode">Optional term to add these courses to in degree plan</param>
        public CourseBlocks(string description, IEnumerable<string> courseIds)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (courseIds != null)
            {
                courseIds = courseIds.Where(c => (!string.IsNullOrEmpty(c)));
            }
            if (courseIds == null || courseIds.Count() == 0)
            {
                throw new ArgumentNullException("courseIds");
            }
            _Description = description;
            _CourseIds = courseIds.ToList();
        }
    }
}
