// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Organizational person position
    /// </summary>
    public class OrganizationalPersonPositionBase
    {
        /// <summary>
        /// The unique Id of the organizational person position, which is an assigned position in an organization.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Id of the position
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// The Title for the position
        /// </summary>
        public string PositionTitle { get; set; }

        /// <summary>
        /// Id of the person assigned to the organizational person position
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The name of the person
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Returns the status of the Organizational Person Position
        /// </summary>
        public OrganizationalPersonPositionStatus Status { get; set; }
    }
}
