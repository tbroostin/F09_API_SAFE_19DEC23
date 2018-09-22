// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Session
    {
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id == 0)
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Id cannot be changed");
                }
            }
        }

        private readonly string _Name;
        public string Name { get { return _Name; } }

        public Session()
        {

        }

        public Session(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            _Name = name;
        }

        public Session(int id, string name)
            : this(name)
        {
            _Id = id;
        }

        public override int GetHashCode()
        {
            return _Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Session other = obj as Session;
            if (other == null)
            {
                return false;
            }

            return _Name.Equals(other.Name);
        }
    }
}
