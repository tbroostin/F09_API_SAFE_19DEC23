// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for staff
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StaffRepository : PersonRepository, IStaffRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public StaffRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Returns a Staff entity for the requested Person Id. Colleague STAFF record must be present and contain a type designated for "Staff".
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Staff> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide staff id");
            }
            try
            {
                //Call batch get to retrieve single staff member.
                //Throw key not found exception if nothing is returned
                return (await GetAsync(new List<string>() { id })).Single();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving staff with the id " + id);
                throw;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Cannot find staff with the id " + id);
            }
        }

        /// <summary>
        /// Returns a Staff entity for the requested Person Id. Colleague STAFF record must be present and contain a type designated for "Staff".
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Staff Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide staff id");
            }
            try
            {
                //Call batch get to retrieve single staff member.
                //Throw key not found exception if nothing is returned
                return Get(new List<string>() { id }).Single();
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Cannot find staff with the id " + id);
            }
        }

        /// <summary>
        /// Returns a list of Staff entities for requested Person ids. 
        /// Colleague STAFF record must be present and contain a type designated for "Staff" for each.
        /// </summary>
        /// <param name="ids">List of Person ids</param>
        /// <returns>List of Staff Entities</returns>
        public async Task<IEnumerable<Staff>> GetAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "Must provide staff ids.");
            }

            List<Staff> staffEntities = new List<Staff>();
            //Remove duplicates and empty/null ids from the ids list
            ids = ids.Where(id => !String.IsNullOrEmpty(id)).Distinct().ToList();

            Collection<DataContracts.Staff> staffData = await DataReader.BulkReadRecordAsync<DataContracts.Staff>(ids.ToArray());
            if (staffData != null && staffData.Any())
            {
                var staffTypesValcodes = GetStaffTypesValcode();
                var staffStatusesValcodes = GetStaffStatusesValcode();
                var staffRecords = await GetPersonsAsync<Staff>(ids, person =>
                    {
                        Staff staffEntity = new Staff(person.Recordkey, person.LastName);
                        return staffEntity;
                    }
                );

                foreach (var id in ids)
                {
                    DataContracts.Staff staffDataContact = staffData.FirstOrDefault(sd => sd.Recordkey == id);
                    Staff staff = staffRecords.FirstOrDefault(sr => sr.Id == id);
                    try
                    {
                        ValidateStaffRecord(id, staff, staffDataContact, staffTypesValcodes, staffStatusesValcodes);

                        // Add the privacy codes to the staff record
                        staff.PrivacyCodes = staffDataContact.StaffPrivacyCodesAccess;
                        // Add the staff initials to the staff record
                        staff.StaffInitials = staffDataContact.StaffInitials; 
                        // Add the staff loginid/operator to the staff record
                        staff.StaffLoginId = staffDataContact.StaffLoginId;

                        staffEntities.Add(staff);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Couldn't validate STAFF record.");
                    }
                }
            }
            else
            {
                logger.Info("Unable to get Staff information for specified id(s)");
            }
            return staffEntities;
        }

        /// <summary>
        /// Returns a list of Staff entities for requested Person ids. 
        /// Colleague STAFF record must be present and contain a type designated for "Staff" for each.
        /// </summary>
        /// <param name="ids">List of Person ids</param>
        /// <returns>List of Staff Entities</returns>
        public IEnumerable<Staff> Get(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "Must provide staff ids.");
            }

            List<Staff> staffEntities = new List<Staff>();
            //Remove duplicates and empty/null ids from the ids list
            ids = ids.Where(id => !String.IsNullOrEmpty(id)).Distinct().ToList();

            Collection<DataContracts.Staff> staffData = DataReader.BulkReadRecord<DataContracts.Staff>(ids.ToArray());
            if (staffData != null && staffData.Any())
            {
                var staffTypesValcodes = GetStaffTypesValcode();
                var staffStatusesValcodes = GetStaffStatusesValcode();
                var staffRecords = Task.Run(async () =>
                {
                    return await GetPersonsAsync<Staff>(ids, person =>
                    {
                        Staff staffEntity = new Staff(person.Recordkey, person.LastName);
                        return staffEntity;
                    });
                }).GetAwaiter().GetResult();

                foreach (var id in ids)
                {
                    DataContracts.Staff staffDataContact = staffData.FirstOrDefault(sd => sd.Recordkey == id);
                    Staff staff = staffRecords.FirstOrDefault(sr => sr.Id == id);
                    try
                    {
                        ValidateStaffRecord(id, staff, staffDataContact, staffTypesValcodes, staffStatusesValcodes);

                        // Add the privacy codes to the staff record
                        staff.PrivacyCodes = staffDataContact.StaffPrivacyCodesAccess;

                        staffEntities.Add(staff);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Could not validate STAFF record.");
                    }

                }
            }
            else
            {
                logger.Info("Unable to get Staff information for specified id(s)");
            }
            return staffEntities;
        }

        /// <summary>
        /// Get the staff log in ID based on a person ID.
        /// </summary>
        /// <param name="personId">person ID used to locate the staff record.</param>
        /// <returns>the staff login id for the person.</returns>
        public async Task<string> GetStaffLoginIdForPersonAsync(string personId)
        {
            var staffRecord = await DataReader.ReadRecordAsync<DataContracts.Staff>("STAFF", personId);
            if (staffRecord == null)
            {
                logger.Error("The user", personId, "does not have a STAFF record.");
                throw new ApplicationException("The user " + personId + " does not have a STAFF record.");
            }

            return staffRecord.StaffLoginId;
        }

        /// <summary>
        /// Validates if the requested Staff record is of Staff type, updates IsActive flag, or throws ans exception if record with
        /// the specified id was not found
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="staff">Staff Entity</param>
        /// <param name="staffData">Staff Data contract</param>
        /// <param name="staffTypesValidationTable">STAFF.TYPES Valcodes</param>
        /// <param name="staffStatusesValidationTable">STAFF.STATUSES Valcodes</param>
        private void ValidateStaffRecord(string id, Staff staff, DataContracts.Staff staffData, ApplValcodes staffTypesValidationTable, ApplValcodes staffStatusesValidationTable)
        {
            if (staffData != null)
            {
                // If we have a valcode and a nonblank StaffType, find it and check the special processing code to make sure this is a "Staff" type
                ApplValcodesVals staffType = null;
                if (staffTypesValidationTable != null && staffTypesValidationTable.ValsEntityAssociation != null && !string.IsNullOrEmpty(staffData.StaffType))
                {
                    staffType = staffTypesValidationTable.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == staffData.StaffType).First();
                }

                // If type does not indicate "Staff", throw an error
                if (staffType == null || staffType.ValActionCode1AssocMember != "S")
                {
                    var errorMessage = "Requested staff " + id + " does not have a valid Staff type.";
                    logger.Error(errorMessage);
                    throw new ColleagueWebApiException(errorMessage);
                }

                // update IsActive flag on staff record if status indicates Active
                ApplValcodesVals staffStatus = null;
                if (staffStatusesValidationTable != null && staffStatusesValidationTable.ValsEntityAssociation != null && !string.IsNullOrEmpty(staffData.StaffStatus))
                {
                    staffStatus = staffStatusesValidationTable.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == staffData.StaffStatus).First();
                }

                // Status must have a special processing code of "A" to be considered "Active"
                if (staffStatus != null && staffStatus.ValActionCode1AssocMember == "A")
                {
                    staff.IsActive = true;
                }
                else
                {
                    staff.IsActive = false;
                }
            }
            else
            {
                var errorMessage = ("Colleague STAFF record not found for Person " + id);
                logger.Error(errorMessage);
                throw new ColleagueWebApiException(errorMessage);
            }
        }

        /// <summary>
        /// Gets Staff Statuses Valcodes
        /// </summary>
        /// <returns>STAFF.STATUSES Valcodes</returns>
        private ApplValcodes GetStaffStatusesValcode()
        {
            // Get the STAFF.STATUSES valcode 
            var staffStatusesValidationTable = new ApplValcodes();
            try
            {
                staffStatusesValidationTable = GetOrAddToCache<ApplValcodes>("StaffStatuses",
                    () =>
                    {
                        ApplValcodes staffStatusesValTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES");
                        return staffStatusesValTable;
                    }, Level1CacheTimeoutValue);
            }
            catch (Exception)
            {
                // log the issue and move on. Not likely to happen...
                var errorMessage = "Unable to retrieve STAFF.STATUSES validation table from Colleague.";
                logger.Error(errorMessage);
            }
            return staffStatusesValidationTable;
        }

        /// <summary>
        /// Gets Staff Types Valcodes
        /// </summary>
        /// <returns>STAFF.TYPES Valcodes</returns>
        private ApplValcodes GetStaffTypesValcode()
        {
            var staffTypesValidationTable = new ApplValcodes();
            try
            {
                // Get the STAFF.TYPES valcode, verify that this is staff record contains a "Staff" type. If not, throw an error.
                staffTypesValidationTable = GetOrAddToCache<ApplValcodes>("StaffTypes",
                    () =>
                    {
                        ApplValcodes staffTypesValTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES");
                        if (staffTypesValTable == null)
                        {
                            logger.Error("STAFF.TYPES validation table data is null.");
                            throw new ColleagueWebApiException();
                        }
                        return staffTypesValTable;
                    }, Level1CacheTimeoutValue);
            }
            catch (Exception)
            {
                var errorMessage = "Unable to retrieve STAFF.TYPES validation table from Colleague.";
                logger.Error(errorMessage);
                throw new ColleagueWebApiException(errorMessage);
            }
            return staffTypesValidationTable;
        }
    }
}