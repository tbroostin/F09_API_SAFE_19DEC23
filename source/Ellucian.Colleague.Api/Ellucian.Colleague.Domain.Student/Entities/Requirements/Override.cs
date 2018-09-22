// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class Override
    {
        /// <summary>
        /// Group Id to which these overrides apply
        /// </summary>
        private string _GroupId;
        public string GroupId { get { return _GroupId; } }
        /// <summary>
        /// AcademicCredit Ids that are forced to be allowed to apply to this group
        /// </summary>
        private List<string> _CreditsAllowed;
        public List<string> CreditsAllowed { get { return _CreditsAllowed; } }
        /// <summary>
        /// AcademicCredit Ids that are forced to be disallowed from applying to this group
        /// </summary>
        private List<string> _CreditsDenied;
        public List<string> CreditsDenied { get { return _CreditsDenied; } }

        public Override(string groupId, IEnumerable<string> allowed, IEnumerable<string> denied)
        {
            if (groupId == null) throw new ArgumentNullException("groupId");

            _CreditsDenied = new List<string>();
            _CreditsAllowed = new List<string>();
            _GroupId = groupId;

            if (allowed != null) { _CreditsAllowed = allowed.ToList(); }
            if (denied != null) { _CreditsDenied = denied.ToList(); }

            IEnumerable<string> intersections;
            intersections = _CreditsAllowed.Intersect(_CreditsDenied);

            if (intersections.Count() > 0)
            {
                throw new NotSupportedException("You cannot override to both include and exclude id(s): " + string.Join(",", intersections).ToString());
            }

        }

        public bool AllowsCredit(string academiccreditid)
        {
            return (academiccreditid == null) ? false : _CreditsAllowed.Contains((string)academiccreditid);
        }

        public bool DeniesCredit(string academiccreditid)
        {
            return (academiccreditid == null) ? false : _CreditsDenied.Contains((string)academiccreditid);
        }

        

        public override string ToString()
        {
            string allowed = "";
            string denied = "";

            if (_CreditsAllowed.Count() > 0)
            {
                allowed = " Allows " + string.Join(",", _CreditsAllowed.ToArray());
            }
            if (_CreditsDenied.Count() > 0)
            {
                denied = " Denies " + string.Join(",", _CreditsDenied.ToArray());
            }

            return "Override Grp "+ GroupId +":"+allowed + " " + denied;
        }



    }
}
