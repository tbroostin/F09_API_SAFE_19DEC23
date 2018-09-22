/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Definition of an award letter group
    /// </summary>
    [Serializable]
    public class AwardLetterGroup
    {
        /// <summary>
        /// Title of the group
        /// </summary>
        public string Title { get { return _title; } }
        private readonly string _title;

        /// <summary>
        /// Members of the group
        /// </summary>
        public ReadOnlyCollection<string> Members { get; private set; }
        private readonly List<string> _members;

        /// <summary>
        /// Sequence number of the group
        /// </summary>
        public int SequenceNumber { get { return _sequenceNumber; } }
        private readonly int _sequenceNumber;

        /// <summary>
        /// Group type
        /// </summary>
        public GroupType GroupType { get { return _groupType; } }
        private readonly GroupType _groupType;

        /// <summary>
        /// Constructor
        /// <param name="title">title of the group</param>
        /// <param name="sequenceNumber">sequence number of the group</param>
        /// </summary>
        public AwardLetterGroup(string title, int sequenceNumber, GroupType groupType)
        {
            if (sequenceNumber < 0)
            {
                throw new ArgumentException("sequence number must be greater than or equal to 0", "sequenceNumber");
            }

            _title = title;
            _sequenceNumber = sequenceNumber;
            _groupType = groupType;

            _members = new List<string>();
            this.Members = _members.AsReadOnly();
        }

        /// <summary>
        /// Method to add a member to the read only collection
        /// </summary>
        /// <param name="member">member string</param>
        /// <returns>true/false to indicate the success/failure of the add operation</returns>
        public bool AddGroupMember(string member)
        {
            if (string.IsNullOrEmpty(member))
            {
                throw new ArgumentNullException("member", "member string cannot be null or empty");
            }

            if (!_members.Contains(member))
            {
                _members.Add(member);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to remove a member from the read only collection
        /// </summary>
        /// <param name="member">member string</param>
        /// <returns>true/false to indicate success/failure of the member removal</returns>
        public bool RemoveGroupMember(string member)
        {
            if (string.IsNullOrEmpty(member))
            {
                throw new ArgumentNullException("member", "member string cannot be null or empty");
            }

            if (_members.Contains(member))
            {
                return _members.Remove(member);
            }

            return false;
        }
    }
}
