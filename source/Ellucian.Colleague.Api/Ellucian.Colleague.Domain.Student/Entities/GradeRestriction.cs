// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class GradeRestriction
    {
        /// <summary>
        /// Indicates if there is a grade restriction. If so IsRestricted will be false. Required.
        /// </summary>
        private bool _IsRestricted;
        public bool IsRestricted { get { return _IsRestricted; } }

        /// <summary>
        /// Restriction reasons if IsRestricted is true.
        /// </summary>
        private List<string> _Reasons = new List<string>();
        public List<string> Reasons { get { return _Reasons;  } }

        public GradeRestriction(bool isRestricted)
        {
            _IsRestricted = isRestricted;
        }

        /// <summary>
        /// Add a message to the enumerable list of messages for this planned course
        /// </summary>
        /// <param name="message"></param>
        public void AddReason(string reason)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                _Reasons.Add(reason);
                // Anytime a reason is added to the list, it implies that there is a grade restriction so set IsRestricted to true.
                _IsRestricted = true;
            }
        }
    }
}
