/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// InstitutionJobSupervisors object describes how a supervisor holds a position.
    /// </summary>
    [Serializable]
    public class InstitutionJobSupervisor : GuidCodeItem
    {
        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId
        {
            get { return personId; }
        }

        private readonly string personId;

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        public string PositionId
        {
            get { return positionId; }
        }

        private readonly string positionId;

        /// <summary>
        /// The Id of the person's supervisor for this position
        /// </summary>
        public string SupervisorId { get; set; }

        /// <summary>
        /// The Id of the person's alternate supervisor for this position
        /// </summary>
        public string AlternateSupervisorId { get; set; }

        /// <summary>
        /// The operational unit of the institution to which the job belongs
        /// </summary>
        public string Employer { get; set; }

        /// <summary>
        /// The department of the institution to which the job belongs
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionJobSupervisor"/> class.
        /// </summary>
        public InstitutionJobSupervisor(string guid, string code, string description, string personId, string positionId)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            this.personId = personId;
            this.positionId = positionId;
        }

    }
}