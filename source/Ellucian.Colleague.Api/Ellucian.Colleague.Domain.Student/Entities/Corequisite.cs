// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Corequisite
    {
        /// <summary>
        /// Id of the course or section
        /// </summary>
        private string _Id;
        public string Id { get { return _Id; } }

        /// <summary>
        /// Boolean indicates whether corequisite is required
        /// </summary>
        private bool _Required;
        public bool Required { get { return _Required; } }

        public Corequisite(string id, bool required)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            _Id = id;
            _Required = required;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Corequisite other = obj as Corequisite;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
