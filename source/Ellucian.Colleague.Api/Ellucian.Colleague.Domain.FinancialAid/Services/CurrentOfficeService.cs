//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    /// <summary>
    /// The CurrentOfficeService class uses Financial Aid business logic to retrieve
    /// particular Office objects, most notably, an Office object based on a locationId, which
    /// normally comes from a student record.
    /// </summary>
    public class CurrentOfficeService
    {
        /// <summary>
        /// A collection of Office objects, representing all of the Financial Aid office records
        /// in the Colleague database.
        /// </summary>
        private IEnumerable<FinancialAidOffice> offices;

        /// <summary>
        /// Constructor creates a CurrentOfficeService.
        /// </summary>
        /// <param name="allOffices">This should be a list of all the financial aid office objects from the Colleague database</param>
        /// <exception cref="ArgumentNullException">Thrown when allOffices argument is null</exception>
        public CurrentOfficeService(IEnumerable<FinancialAidOffice> allOffices)
        {
            if (allOffices == null)
            {
                throw new ArgumentNullException("allOffices");
            }
            offices = allOffices;
        }

        /// <summary>
        /// Get an office object based on a locationId. If no office exists for the given
        /// location, the default office is returned. While unlikely, it's possible that the
        /// default office is returned as null. Callers should not assume the return object will have value.
        /// </summary>
        /// <param name="locationId">The locationId of the Office object</param>
        /// <returns>An Office object based on the given locationId, or the default office, or null</returns>
        public FinancialAidOffice GetCurrentOfficeByLocationId(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                return GetDefaultOffice();
            }

            var officeByLocation = offices.FirstOrDefault(o => o.LocationIds.Contains(locationId));
            if (officeByLocation == null)
            {
                return GetDefaultOffice();
            }

            return officeByLocation;
        }

        /// <summary>
        /// Get the institution's default financial aid office. While unlikely, it's possible that the
        /// default office is returned as null. Callers should not assume the return object will have value.
        /// </summary>
        /// <returns>The default financial aid office as defined by the institution</returns>
        public FinancialAidOffice GetDefaultOffice()
        {
            return offices.FirstOrDefault(o => o.IsDefault);
        }

        /// <summary>
        /// Get current office by id. If no office exists for the given
        /// id, the default office is returned. While unlikely, it's possible that the
        /// default office is returned as null. Callers should not assume the return object will have value.
        /// </summary>
        /// <param name="officeId">office id for office look up</param>
        /// <returns>FinancialAidOffice or null</returns>
        public FinancialAidOffice GetCurrentOfficeByOfficeId(string officeId)
        {
            if (string.IsNullOrEmpty(officeId))
            {
                return GetDefaultOffice();
            }

            var officeById = offices.FirstOrDefault(o => o.Id.ToUpper() == officeId.ToUpper());
            if (officeById == null)
            {
                return GetDefaultOffice();
            }

            return officeById;
        }

        /// <summary>
        /// Get all active configurations from all offices
        /// </summary>
        /// <returns>List of fa configurations with Self-Service flag on</returns>
        public IEnumerable<FinancialAidConfiguration> GetActiveOfficeConfigurations()
        {
            return offices.SelectMany(o => o.Configurations.Where(c => c.IsSelfServiceActive));
        }

        /// <summary>
        /// Get current office with active configuration for the specified year
        /// </summary>
        /// <param name="locationId">location id of the office</param>
        /// <param name="yearCode">year code</param>
        /// <returns>Office found by id or default office</returns>
        public FinancialAidOffice GetCurrentOfficeWithActiveConfiguration(string locationId, string yearCode)
        {
            var office = GetCurrentOfficeByLocationId(locationId);
            var currentConfiguration = office.Configurations.FirstOrDefault(c => c.AwardYear == yearCode);
            return (currentConfiguration != null && currentConfiguration.IsSelfServiceActive) ? office : null;            
        }
    }
}
