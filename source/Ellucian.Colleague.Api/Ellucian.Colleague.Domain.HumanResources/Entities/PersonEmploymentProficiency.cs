//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// HR.IND.SKILL record entity for PersonEmploymentProficiencies API
    /// </summary>
    [Serializable]
    public class PersonEmploymentProficiency
    {
        /// <summary>
        /// HR.IND.SKILL ID
        /// </summary>
        public string RecordKey { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// HRPER.ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Job skill ID
        /// </summary>
        public string ProficiencyId { get; set; }

        /// <summary>
        /// License Date
        /// </summary>
        public DateTime? StartOn { get; set; }

        /// <summary>
        /// Expire date of skill
        /// </summary>
        public DateTime? EndOn { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comment { get; set; }

    }
}
