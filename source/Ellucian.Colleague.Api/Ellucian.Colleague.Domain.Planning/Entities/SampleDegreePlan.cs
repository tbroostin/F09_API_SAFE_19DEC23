// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    [Serializable]
    public class SampleDegreePlan
    {
        /// <summary>
        /// Unique identifier of this sample degree plan
        /// </summary>
        private readonly string _Code;
        public string Code { get { return _Code; } }

        /// <summary>
        /// Description of this sample degree plan
        /// </summary>
        private readonly string _Description;
        public string Description { get { return _Description; } }

        /// <summary>
        /// List of Ids of course blocks assigned to this sample degree plan
        /// </summary>
        private List<CourseBlocks> _CourseBlocks;
        public List<CourseBlocks> CourseBlocks { get { return _CourseBlocks; } }

        public SampleDegreePlan(string code, string description, IEnumerable<CourseBlocks> courseBlocks)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (courseBlocks == null || courseBlocks.Count() == 0)
            {
                throw new ArgumentNullException("courseBlocks");
            }
            _Code = code; 
            _Description = description;
            _CourseBlocks = courseBlocks.ToList();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SampleDegreePlan other = obj as SampleDegreePlan;
            if (other == null)
            {
                return false;
            }

            return Code.Equals(other.Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}
