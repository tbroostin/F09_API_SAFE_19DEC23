// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Department codes
    /// </summary>
    [Serializable]
    public class Department : GuidCodeItem
    {
        private readonly bool _IsActive;

        /// <summary>
        /// Gets a value indicating whether department [is active].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is active]; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get { return _IsActive; } }

        /// <summary>
        /// The Institution Id for this Department
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// The School, such as Law School, School of Nursing, etc.
        /// </summary>
        public string School { get; set; }

        /// <summary>
        /// The Division for this Department
        /// </summary>
        public string Division { get; set; }

        /// <summary>
		/// Department Type  (A)cademic or (H)uman Resources
		/// </summary>
		public string DepartmentType { get; set; }


        private List<string> _LocationIds = new List<string>();
        /// <summary>
        /// Departments and their percentages of responsibility for the course
        /// </summary>
        public ReadOnlyCollection<string> LocationIds { get; private set; }

        
        /// <summary>
        /// The list of person Ids assigned to Department as Departmental Oversights
        /// </summary>
        public List<string> DepartmentalOversightIds { get;  set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Department"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public Department(string guid, string code, string description, bool isActive) : base(guid, code, description)
        {
            _IsActive = isActive;
            LocationIds = _LocationIds.AsReadOnly();
            DepartmentalOversightIds = new List<string>();
         
        }

        public void AddLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                throw new ArgumentNullException("locationId", "Location ID must be provided.");
            }
            _LocationIds.Add(locationId);
        }
    }
}
