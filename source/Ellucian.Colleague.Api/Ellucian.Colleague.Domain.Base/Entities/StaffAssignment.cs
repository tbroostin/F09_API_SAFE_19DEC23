// Copyright 2015 Ellucian Company L.P. and its affiliates.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Staff Assignment entity with staff and start and end dates
    /// </summary>
    [Serializable]
    public class StaffAssignment
    {
        private string _staffId;
        /// <summary>
        /// Unique ID of the staff for this assignment instance
        /// </summary>
        public string StaffId
        {
            get { return _staffId; }
            set
            {
                if (_staffId == "")
                {
                    _staffId = value;
                }
                else
                {
                    throw new InvalidOperationException("StaffId cannot be changed");
                }
            }
        }

        /// <summary>
        /// Start date of the assignment (optional)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date of the assignment (optional)
        /// </summary>
        public DateTime? EndDate { get; set; }

        public StaffAssignment(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                throw new ArgumentNullException("staffId", "Must provide a staff Id.");
            }
            _staffId = staffId;
        }
    }
}
