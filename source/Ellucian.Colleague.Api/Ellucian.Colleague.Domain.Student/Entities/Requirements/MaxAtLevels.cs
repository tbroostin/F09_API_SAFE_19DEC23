// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public abstract class MaxAtLevels
    {
        private readonly List<string> _Levels;
        public List<string> Levels { get { return _Levels; } }

        protected MaxAtLevels(ICollection<string> levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException("levels");
            }
            _Levels = levels.ToList();
        }

        public void AddLevel(string level)
        {
            _Levels.Add(level);
        }

    }
}
