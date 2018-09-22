// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of email addresses
    /// </summary>
    [Serializable]
    public class AddressSeasonalDates
    {
        /// <summary>
        /// The Month and Day in MM/DD format for seasonal start with a yearly repeat
        /// </summary>
        public string StartOn { get; private set; }

        /// <summary>
        /// The Month and Day in MM/DD format for seasonal end with yearly repeat
        /// </summary>
        public string EndOn { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressSeasonalDates"/> class.
        /// </summary>
        /// <param name="startOn">The start on time period (MM/DD)</param>
        /// <param name="endOn">The end on time period (MM/DD)</param>
        /// <exception cref="System.ArgumentNullException">
        /// startOn;start on (MM/DD) must be specified
        /// or
        /// endOn;end on (MM/DD) must be specified
        /// </exception>
        public AddressSeasonalDates(string startOn, string endOn)
        {
            if (string.IsNullOrEmpty(startOn))
            {
                throw new ArgumentNullException("startOn", "start on (MM/DD) must be specified");
            }
            if (string.IsNullOrEmpty(endOn))
            {
                throw new ArgumentNullException("endOn", "end on (MM/DD) must be specified");
            }
            StartOn = startOn;
            EndOn = endOn;
        }

        /// <summary>
        /// Determines whether the specified AddressSeasonalDates is equal to this instance.
        /// </summary>
        /// <param name="obj">The AddressSeasonalDates object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified AddressSeasonalDates is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            AddressSeasonalDates other = obj as AddressSeasonalDates;
            if (other == null)
            {
                return false;
            }
            return
                (string.IsNullOrEmpty(StartOn) ? string.IsNullOrEmpty(other.StartOn) : StartOn.Equals(other.StartOn)) &&
                (string.IsNullOrEmpty(EndOn) ? string.IsNullOrEmpty(other.EndOn) : EndOn.Equals(other.EndOn));
        }

    }
}
