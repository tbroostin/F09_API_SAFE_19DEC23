// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Services
{
    public static class OrganizationalStructureService
    {

        /// <summary>
        /// Returns the current status of a position based on start and end dates
        /// </summary>
        /// <param name="startDate">Start date of the organizational person position</param>
        /// <param name="endDate">End date of the organizational person position</param>
        /// <returns>
        /// Future if the start date is in the future.
        /// Present if the start date is in the past with a future or no end date.
        /// Past if the start date and end date are in the past.
        /// Unknown if there is no start date or otherwise conflicting dates.
        /// </returns>
        public static OrganizationalPersonPositionStatus GetOrganizationalPersonPositionStatus(DateTime? startDate, DateTime? endDate)
        {
            // No dates results in an unknown status
            if (!startDate.HasValue && !endDate.HasValue)
            {
                return OrganizationalPersonPositionStatus.Unknown;
            }

            if (startDate.HasValue)
            {
                if (endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
                {
                    // Conflicting start and end dates result in Unknown status
                    return OrganizationalPersonPositionStatus.Unknown;
                }

                // Start date in the future results in a Future status
                if (startDate.Value.Date > DateTime.Today)
                {
                    return OrganizationalPersonPositionStatus.Future;
                }

                // Start date was not in the future, so it should be Past or Current depending on the end date
                if (endDate.HasValue)
                {
                    if (endDate.Value.Date < DateTime.Today)
                    {
                        return OrganizationalPersonPositionStatus.Past;
                    }
                    else
                    {
                        return OrganizationalPersonPositionStatus.Current;
                    }
                }
                else
                {
                    // No end date with start date in the past is a Current position
                    return OrganizationalPersonPositionStatus.Current;
                }
            }

            // Position did not have a start date
            return OrganizationalPersonPositionStatus.Unknown;
        }
    }
}
