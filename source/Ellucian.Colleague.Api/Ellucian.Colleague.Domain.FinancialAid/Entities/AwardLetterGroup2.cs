/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardLetterGroup2 class that contains group's name, type, and number data;
    /// used for award category row and award period column groupings
    /// </summary>
    [Serializable]
    public class AwardLetterGroup2
    {
        /// <summary>
        /// Group name
        /// </summary>
        public string GroupName { get { return _groupName; } }
        private readonly string _groupName;

        /// <summary>
        /// Group number
        /// </summary>
        public int GroupNumber { get { return _groupNumber; } }
        private readonly int _groupNumber;

        /// <summary>
        /// Group type: award category group or award period group
        /// </summary>        
        public GroupType GroupType { get { return _groupType; } }
        private readonly GroupType _groupType;

        /// <summary>
        /// Constructor
        /// <param name="groupName">group name</param>
        /// <param name="groupNumber">group number</param>
        /// <param name="groupType">group type</param>
        /// </summary>
        public AwardLetterGroup2(string groupName, int groupNumber, GroupType groupType)
        {
            if (groupNumber < 0)
            {
                throw new ArgumentException("group number must be greater than or equal to 0", "groupNumber");
            }

            _groupName = groupName;
            _groupNumber = groupNumber;
            _groupType = groupType;
            
        }        
    }
}
