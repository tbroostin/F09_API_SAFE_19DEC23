/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class FacultyContractAssignment
    {
        public string Id { get; private set; }

        public string HrpId { get; private set; }
        
        public FacultyContractAssignmentType AssignmentType { get; private set; }

        public string PositionId { get; private set; }
        /// <summary>
        /// PLPA.ASGMT.ID
        /// </summary>
        public string AssignmentId { get; private set; }

        public string AssignmentDescription
        {
            get
            {
                return assignmentDescription;
            }
            set
            {
                assignmentDescription = value ?? string.Empty;
            }
        }

        private string assignmentDescription;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Decimal? IntendedLoad { get; set; }
        public string Amount { get; private set; }
        public string Role { get; set; }
        public FacultyContractAssignment(string id, string hrpId, FacultyContractAssignmentType assignmentType, string positionId, string assignmentId, string amount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId", "positionId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(assignmentId))
            {
                throw new ArgumentNullException("assignmentId", "assignmentId cannot be null or empty");
            }
            Id = id;
            HrpId = hrpId;
            AssignmentType = assignmentType;
            PositionId = positionId;
            AssignmentId = assignmentId;
            AssignmentDescription = string.Empty;
            Amount = amount;
        }
    }
}
