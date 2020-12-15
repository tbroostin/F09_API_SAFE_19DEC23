// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FacultyRepository : PersonRepository, IFacultyRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public FacultyRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Retrieves a list of Faculty IDs
        /// </summary>
        /// <param name="facultyOnlyFlag">Select only Faculty, no Advisors</param>
        /// <param name="advisorOnlyFlag">Select Advisors only, no Faculty</param>
        /// <returns>A list of Faculty IDs</returns>
        public async Task<IEnumerable<string>> SearchFacultyIdsAsync(bool facultyOnlyFlag = false, bool advisorOnlyFlag = true)
        {
            var facultyIds = new string[] { };
            string selectCriteria = "";
            if (advisorOnlyFlag)
            {
                selectCriteria = "WITH FAC.ADVISE.FLAG = 'Y'";
            }
            else
            {
                if (facultyOnlyFlag)
                {
                    selectCriteria = "WITH FAC.ADVISE.FLAG NE 'Y'";
                }
            }
            facultyIds = await DataReader.SelectAsync("FACULTY", selectCriteria);
            return facultyIds;
        }

        /// <summary>
        /// Get a single faculty entity for the id provided
        /// </summary>
        /// <param name="id">Id of the faculty to return</param>
        /// <returns>A faculty entity or throws an exception if it is not found in cache.</returns>
        public async Task<Ellucian.Colleague.Domain.Student.Entities.Faculty> GetAsync(string id)
        {
            Ellucian.Colleague.Domain.Student.Entities.Faculty facultyEntity;
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide the ID of the faculty to retrieve.");
            }
            try
            {
                facultyEntity = (await GetAllFacultyAsync())[id];
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to retrieve faculty information for ID {0}.", id));
                throw new KeyNotFoundException("Faculty not found for Id " + id);
            }
            return facultyEntity;
        }

        /// <summary>
        /// Gets faculty objects for the given list of Ids
        /// </summary>
        /// <param name="facultyIds">List of faculty person Ids</param>
        /// <returns>List of faculty domain objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Faculty>> GetFacultyByIdsAsync(IEnumerable<string> facultyIds)
        {
            var facultyEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Faculty>();
            string idsNotFound = "";
            if ((facultyIds != null) && (facultyIds.Count() != 0))
            {
                var allFaculty = await GetAllFacultyAsync();
                foreach (var id in facultyIds)
                {
                    try
                    {
                        facultyEntities.Add(allFaculty[id]);
                    }
                    catch (Exception)
                    {
                        idsNotFound += " " + id;
                    }
                }
            }
            if (!string.IsNullOrEmpty(idsNotFound))
            {
                string errorMessage = string.Format("Unable to retrieve faculty information for IDs {0}.", idsNotFound);
                logger.Error(errorMessage);
            }
            return facultyEntities;
        }

        /// <summary>
        /// Gets faculty office hours for the given list of faculty Ids
        /// </summary>
        /// <param name="facultyIds">List of faculty person Ids</param>
        /// <returns>List of faculty office hours objects</returns>
        public async Task<IEnumerable<Domain.Student.Entities.FacultyOfficeHours>> GetFacultyOfficeHoursByIdsAsync(IEnumerable<string> facultyIds)
        {
            var facultyofficeHoursList = new List<Ellucian.Colleague.Domain.Student.Entities.FacultyOfficeHours>();
            string idsNotFound = "";
            if ((facultyIds != null) && (facultyIds.Any()))
            {                                
                foreach (var id in facultyIds)
                {
                    try
                    {
                        var officeHours = await GetFacultyOfficeHoursAsync(id);

                        //Remove any faculty office hour if it's in past or in future
                        if (officeHours.Any())
                        {
                            foreach (var officeHour in officeHours.ToList())
                            {
                                if (officeHour.OfficeEndDate.HasValue && officeHour.OfficeStartDate.HasValue &&
                                (officeHour.OfficeEndDate.Value.Date < DateTimeOffset.UtcNow.Date || officeHour.OfficeStartDate.Value.Date > DateTimeOffset.UtcNow.Date))
                                {
                                    officeHours.Remove(officeHour);
                                }
                            }
                        }

                        Domain.Student.Entities.FacultyOfficeHours facOfficeHours = new Domain.Student.Entities.FacultyOfficeHours()
                        {
                            FacultyId = id,
                            OfficeHours = officeHours
                        };
                        facultyofficeHoursList.Add(facOfficeHours);
                    }
                    catch (Exception)
                    {
                        idsNotFound += " " + id;
                    }
                }
            }
            if (!string.IsNullOrEmpty(idsNotFound))
            {
                string errorMessage = string.Format("Unable to retrieve faculty information for IDs {0}.", idsNotFound);
                logger.Error(errorMessage);
            }
            return facultyofficeHoursList;
        }

        /// <summary>
        /// Gets faculty objects for the given list of Ids
        /// </summary>
        /// <param name="ids">List of faculty person Ids</param>
        /// <returns>List of faculty domain objects</returns>
        [Obsolete("Deprecated on version 1.2 of the Api. Use Get for a single id instead going forward.")]
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Faculty>> GetAsync(IEnumerable<string> ids)
        {
            var facultyEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Faculty>();
            string idsNotFound = "";
            if ((ids != null) && (ids.Count() != 0))
            {
                var allFaculty = await GetAllFacultyAsync();               
                foreach (var id in ids)
                {
                    try
                    {
                        facultyEntities.Add(allFaculty[id]);
                    }
                    catch (Exception)
                    {
                        idsNotFound += " " + id;
                    }
                }
            }
            if (!string.IsNullOrEmpty(idsNotFound))
            {
                string errorMessage = "The following requested faculty ids were not found: " + idsNotFound;
                logger.Error(errorMessage);
            }
            return facultyEntities;
        }

        private async Task<List<OfficeHours>> GetFacultyOfficeHoursAsync(string facultyId)
        {           
               List<OfficeHours> officeHoursdata = new List<OfficeHours>();
               var query = "FACULTY.ID EQ " + facultyId;
               Collection<DataContracts.Faculty> facultyBulkData = await DataReader.BulkReadRecordAsync<DataContracts.Faculty>("FACULTY", query);
               if (facultyBulkData != null && facultyBulkData.FirstOrDefault() != null)
               {
                   var facultyData = facultyBulkData.First();
                   List<string> officeBuildings = new List<string>();
                   List<string> officeRooms = new List<string>();
                   List<DateTime?> officeStartDates = new List<DateTime?>();
                   List<DateTime?> officeEndDates = new List<DateTime?>();
                   List<DateTime?> officeStartTimes = new List<DateTime?>();
                   List<DateTime?> officeEndTimes = new List<DateTime?>();
                   List<string> officeFrequency = new List<string>();
                   List<string> officeMonday = new List<string>();
                   List<string> officeTuesday = new List<string>();
                   List<string> officeWednesday = new List<string>();
                   List<string> officeThursday = new List<string>();
                   List<string> officeFriday = new List<string>();
                   List<string> officeSaturday = new List<string>();
                   List<string> officeSunday = new List<string>();

                   int rowCount = 0;
                   if (facultyData.FacOfficeStartDate != null && facultyData.FacOfficeStartDate.Any())
                   {
                       rowCount = facultyData.FacOfficeStartDate.Count();
                       officeStartDates = facultyData.FacOfficeStartDate;
                   }
                   if (facultyData.FacOfficeEndDate != null && facultyData.FacOfficeEndDate.Any())
                   {
                       officeEndDates = facultyData.FacOfficeEndDate;
                   }
                   if (facultyData.FacOfficeBldg != null && facultyData.FacOfficeBldg.Any())
                   {
                       officeBuildings = facultyData.FacOfficeBldg;
                   }
                   if (facultyData.FacOfficeRoom != null && facultyData.FacOfficeRoom.Any())
                   {
                       officeRooms = facultyData.FacOfficeRoom;
                   }
                   if (facultyData.FacOfficeStartTime != null && facultyData.FacOfficeStartTime.Any())
                   {
                       officeStartTimes = facultyData.FacOfficeStartTime;
                   }
                   if (facultyData.FacOfficeEndTime != null && facultyData.FacOfficeEndTime.Any())
                   {
                       officeEndTimes = facultyData.FacOfficeEndTime;
                   }
                   if (facultyData.FacOfficeRepeat != null && facultyData.FacOfficeRepeat.Any())
                   {
                       officeFrequency = facultyData.FacOfficeRepeat;
                   }
                   if (facultyData.FacOfficeMonday != null && facultyData.FacOfficeMonday.Any())
                   {
                       officeMonday = facultyData.FacOfficeMonday;
                   }
                   if (facultyData.FacOfficeTuesday != null && facultyData.FacOfficeTuesday.Any())
                   {
                       officeTuesday = facultyData.FacOfficeTuesday;
                   }
                   if (facultyData.FacOfficeWednesday != null && facultyData.FacOfficeWednesday.Any())
                   {
                       officeWednesday = facultyData.FacOfficeWednesday;
                   }
                   if (facultyData.FacOfficeThursday != null && facultyData.FacOfficeThursday.Any())
                   {
                       officeThursday = facultyData.FacOfficeThursday;
                   }
                   if (facultyData.FacOfficeFriday != null && facultyData.FacOfficeFriday.Any())
                   {
                       officeFriday = facultyData.FacOfficeFriday;
                   }
                   if (facultyData.FacOfficeSaturday != null && facultyData.FacOfficeSaturday.Any())
                   {
                       officeSaturday = facultyData.FacOfficeSaturday;
                   }
                   if (facultyData.FacOfficeSunday != null && facultyData.FacOfficeSunday.Any())
                   {
                       officeSunday = facultyData.FacOfficeSunday;
                   }
                   for (int i = 0; i < rowCount; i++)
                   {
                       OfficeHours officeHours = new OfficeHours();
                       if (officeBuildings.Count() > i)
                       {
                           officeHours.OfficeBuilding = officeBuildings[i];
                       }
                       if (officeRooms.Count() > i)
                       {
                           officeHours.OfficeRoom = officeRooms[i];
                       }
                       if (officeStartDates.Count() > i)
                       {
                           officeHours.OfficeStartDate = officeStartDates[i];
                       }
                       if (officeEndDates.Count() > i)
                       {
                           officeHours.OfficeEndDate = officeEndDates[i];
                       }
                       if (officeStartTimes.Count() > i)
                       {
                           officeHours.OfficeStartTime = officeStartTimes[i];
                       }
                       if (officeEndTimes.Count() > i)
                       {
                           officeHours.OfficeEndTime = officeEndTimes[i];
                       }
                       if (officeFrequency.Count() > i)
                       {
                           officeHours.OfficeFrequency = officeFrequency[i];
                       }
                        officeHours.DaysOfWeek = new List<DayOfWeek>();
                        if (officeSunday.Count() > i && officeSunday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Sunday);
                        if (officeMonday.Count() > i && officeMonday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Monday);
                        if (officeTuesday.Count() > i && officeTuesday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Tuesday);
                        if (officeWednesday.Count() > i && officeWednesday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Wednesday);
                        if (officeThursday.Count() > i && officeThursday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Thursday);
                        if (officeFriday.Count() > i && officeFriday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Friday);
                        if (officeSaturday.Count() > i && officeSaturday[i].ToUpperInvariant() == "Y")
                            officeHours.DaysOfWeek.Add(DayOfWeek.Saturday);                    
                    officeHoursdata.Add(officeHours);
                   }
               }
               return officeHoursdata;                 
        }        

        /// <summary>
        /// Used to retrieve all faculty records and put them into cache. Following the philosophy that we pull faculty into cache 
        /// and have them all expire together, and that the persons asking for this data will therefore not need to wait to do any direct reads from
        /// Colleague.  These types of cache can be "warmed up" with a script at a set time each day.  This also means that if faculty
        /// information changes, the change will not be reflected till the next business day. 
        /// </summary>
        /// <returns>A dictionary of faculty entities.</returns>
        private async Task<IDictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>> GetAllFacultyAsync()
        {
            logger.Info("Entering FacultyRepository.GetAllFacultyAsync...");
            var facultyDict = await GetOrAddToCacheAsync<Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>>("AllFaculty",
                async () =>
                {
                    string[] facultyIds = new string[] { };
                    try
                    {
                        facultyIds = await DataReader.SelectAsync("FACULTY", "");
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, "An error occurred in DataReader.SelectAsync for the FACULTY file");
                    }

                    // Bulk read the faculty PERSON records in chunks from the database.
                    var personData = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();
                    for (int i = 0; i < facultyIds.Count(); i += readSize)
                    {
                        var subList = facultyIds.Skip(i).Take(readSize).ToArray();
                        try
                        {
                            logger.Debug(string.Join("Reading PERSON file for records: {0}...", string.Join(",", subList)));
                            var bulkData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
                            if (bulkData.BulkRecordsRead == null || !bulkData.BulkRecordsRead.Any())
                            {
                                logger.Debug("Bulk data retrieval complete for PERSON file did not return any valid records.");
                            }
                            if (bulkData.InvalidKeys != null && bulkData.InvalidKeys.Any())
                            {
                                logger.Debug(string.Format("Bulk data retrieval complete for PERSON file returned one or more invalid records: {0}", string.Join(",", bulkData.InvalidKeys)));
                            }
                            if (bulkData.BulkRecordsRead != null && bulkData.BulkRecordsRead.Any())
                            {
                                personData.AddRange(bulkData.BulkRecordsRead);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, "An error occurred in DataReader.BulkReadRecordAsync for the PERSON file for IDs {0}", string.Join(",", subList));
                        }
                    }

                    // Bulk read the associated faculty addresses - needed for associated phone information.
                    var addressGroups = personData.Select(pa => pa.PersonAddresses).Where(a => a.Any(x => x.Length > 0));
                    List<string> addressIds = new List<string>();
                    if (addressGroups != null & addressGroups.Any())
                    {
                        foreach (var address in addressGroups)
                        {
                            if (address != null && address.Any())
                            {
                                addressIds.AddRange(address);
                            }
                        }
                    }
                    var facultyAddressData = new List<Ellucian.Colleague.Data.Base.DataContracts.Address>();
                    for (int i = 0; i < addressIds.Count(); i += readSize)
                    {
                        var subList = addressIds.Skip(i).Take(readSize).ToArray();
                        try
                        {
                            logger.Debug(string.Join("Reading ADDRESS file for records: {0}...", string.Join(",", subList)));
                            var bulkData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", subList);
                            if (bulkData.BulkRecordsRead == null || !bulkData.BulkRecordsRead.Any())
                            {
                                logger.Debug("Bulk data retrieval complete for ADDRESS file did not return any valid records.");
                            }
                            if (bulkData.InvalidKeys != null && bulkData.InvalidKeys.Any())
                            {
                                logger.Debug(string.Format("Bulk data retrieval complete for ADDRESS file returned one or more invalid records: {0}", string.Join(",", bulkData.InvalidKeys)));
                            }
                            if (bulkData.BulkRecordsRead != null && bulkData.BulkRecordsRead.Any())
                            {
                                facultyAddressData.AddRange(bulkData.BulkRecordsRead);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, "An error occurred in DataReader.BulkReadRecordAsync for the ADDRESS file for IDs {0}", string.Join(",", subList));
                        }
                    }

                    // Send all the info to BuildFaculty
                    var facultyEntities = await BuildFacultyAsync(personData, facultyAddressData);

                    return facultyEntities;
                }
            );
            logger.Info("FacultyRepository.GetAllFacultyAsync complete.");
            return facultyDict;
        }

        private async Task<Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>> BuildFacultyAsync(List<Base.DataContracts.Person> personData,
                                                                                                     List<Base.DataContracts.Address> facultyAddressData)
        {
            logger.Info("Entering FacultyRepository.BuildFacultyAsync...");
            var facultyResults = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>();

            if (personData != null)
            {
                ILookup<string, Base.DataContracts.Address> facultyAddressDataLookup = null;
                try
                {
                    facultyAddressDataLookup = facultyAddressData.ToLookup(x => x.Recordkey);
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "An error occurred in FacultyRepository.BuildFacultyAsync while building a lookup of faculty ADDRESS data.");
                    throw ex;
                }
                foreach (var person in personData)
                {
                    try
                    {
                        // Create faculty object using person as a basis
                        var fac = new Ellucian.Colleague.Domain.Student.Entities.Faculty(person.Recordkey, person.LastName);
                        fac.FirstName = person.FirstName;
                        fac.MiddleName = person.MiddleName;
                        fac.Gender = person.Gender;

                        fac.ProfessionalName = GetProfessionalName("FAC", person.PersonFormattedNameTypes, person.PersonFormattedNames);
                        if (person.PeopleEmailEntityAssociation != null && person.PeopleEmailEntityAssociation.Count > 0)
                        {
                            foreach (var emailData in person.PeopleEmailEntityAssociation)
                            {
                                try
                                {
                                    EmailAddress eAddress = new EmailAddress(emailData.PersonEmailAddressesAssocMember, emailData.PersonEmailTypesAssocMember);
                                    fac.AddEmailAddress(eAddress);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, string.Format("An error occurred adding an email address for faculty {0}", person.Recordkey));
                                }
                            }
                        }

                        if (person.PerphoneEntityAssociation != null && person.PerphoneEntityAssociation.Count > 0)
                        {
                            foreach (var phoneData in person.PerphoneEntityAssociation)
                            {
                                try
                                {
                                    Phone personalPhone = new Phone(phoneData.PersonalPhoneNumberAssocMember, phoneData.PersonalPhoneTypeAssocMember, phoneData.PersonalPhoneExtensionAssocMember);
                                    fac.AddPhone(personalPhone);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, string.Format("An error occurred adding a phone number for faculty {0}", person.Recordkey));
                                }
                            }
                        }
                        // Next get the local address phone information from the PSEASON association in person
                        if (person.PseasonEntityAssociation != null && person.PseasonEntityAssociation.Count() > 0)
                        {
                            var currentAddresses = person.PseasonEntityAssociation.Where(pa => pa != null && (pa.AddrEffectiveStartAssocMember == null || pa.AddrEffectiveStartAssocMember <= DateTime.Now));
                            if (currentAddresses != null && currentAddresses.Count() > 0)
                            {
                                foreach (var personAddressData in currentAddresses)
                                {
                                    if (!string.IsNullOrEmpty(personAddressData.AddrLocalPhoneAssocMember))
                                    {
                                        // This could be subvalued so need to split on subvalue mark ASCII 252.
                                        string[] localPhones = personAddressData.AddrLocalPhoneAssocMember.Split(_SM);
                                        string[] localPhoneExts = personAddressData.AddrLocalExtAssocMember.Split(_SM);
                                        string[] localPhoneTypes = personAddressData.AddrLocalPhoneTypeAssocMember.Split(_SM);
                                        for (int i = 0; i < localPhones.Length; i++)
                                        {
                                            try
                                            {
                                                // add in the address override phones into the person's list of phones
                                                Phone personalPhone = new Phone(localPhones[i], localPhoneTypes[i], localPhoneExts[i]);
                                                fac.AddPhone(personalPhone);
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Debug(ex, "Error occurred while adding phone number for faculty member.");
                                            }
                                        }
                                    }
                                }
                                // Finally add in the address phone information from the relevent address records for this person.
                                // Addresses are needed to pull the associated phone numbers so the GetPhone method will work.
                                var addressIds = currentAddresses.Where(p => p.PersonAddressesAssocMember.Length > 0).Select(sp => sp.PersonAddressesAssocMember);
                                foreach (var addressId in addressIds)
                                {
                                    Base.DataContracts.Address addr = facultyAddressDataLookup[addressId].FirstOrDefault();
                                    if (addr != null)
                                    {
                                        if (addr.AdrPhonesEntityAssociation != null && addr.AdrPhonesEntityAssociation.Count > 0)
                                        {
                                            foreach (var addrPhone in addr.AdrPhonesEntityAssociation)
                                            {
                                                if (addrPhone != null)
                                                {
                                                    try
                                                    {
                                                        Phone addressPhone = new Phone(addrPhone.AddressPhonesAssocMember, addrPhone.AddressPhoneTypeAssocMember, addrPhone.AddressPhoneExtensionAssocMember);
                                                        fac.AddPhone(addressPhone);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        logger.Debug(ex, "Error occurred while adding phone number for faculty member from address.");
                                                    }
                                                }
                                            }
                                        }
                                        try
                                        {
                                            var assoc = currentAddresses.Where(p => p != null && p.PersonAddressesAssocMember == addressId).FirstOrDefault();
                                            if (assoc != null)
                                            {
                                                var facultyAddress = new Domain.Base.Entities.Address(addressId, person.Recordkey);

                                                facultyAddress.AddressLines = addr.AddressLines;
                                                facultyAddress.City = addr.City;
                                                facultyAddress.State = addr.State;
                                                facultyAddress.PostalCode = addr.Zip;
                                                facultyAddress.Country = addr.Country;
                                                facultyAddress.CountryCode = addr.Country;
                                                facultyAddress.County = addr.County;
                                                facultyAddress.RouteCode = addr.AddressRouteCode;
                                                facultyAddress.Type = assoc.AddrTypeAssocMember;
                                                facultyAddress.EffectiveStartDate = assoc.AddrEffectiveStartAssocMember;
                                                facultyAddress.EffectiveEndDate = assoc.AddrEffectiveEndAssocMember;
                                                facultyAddress.AddressModifier = assoc.AddrModifierLineAssocMember;

                                                // Build address label
                                                List<string> label = new List<string>();
                                                var country = addr.Country;
                                                var codeCountry = (await GetCountriesAsync()).Where(v => v.Code == country).FirstOrDefault();
                                                if (codeCountry != null)
                                                {
                                                    country = codeCountry.Description;
                                                }
                                                if (!string.IsNullOrEmpty(facultyAddress.AddressModifier))
                                                {
                                                    label.Add(facultyAddress.AddressModifier);
                                                }
                                                if (addr.AddressLines.Count > 0)
                                                {
                                                    label.AddRange(addr.AddressLines);
                                                }
                                                string cityStatePostalCode = GetCityStatePostalCode(addr.City, addr.State, addr.Zip);
                                                if (!String.IsNullOrEmpty(cityStatePostalCode))
                                                {
                                                    label.Add(cityStatePostalCode);
                                                }
                                                if (!String.IsNullOrEmpty(addr.Country))
                                                {
                                                    // Country name gets included in all caps
                                                    label.Add(country.ToUpper());
                                                }
                                                facultyAddress.AddressLabel = label;

                                                fac.AddAddress(facultyAddress);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Debug(ex, "Error occurred while adding address for faculty member.");
                                        }
                                    }
                                }
                            }
                        }
                        facultyResults[fac.Id] = fac;
                    }
                    catch (Exception ex)
                    {
                        // Unable to create a faculty entity for this person. Maybe last name is missing?
                        logger.Error(ex, string.Format("An error occurred building faculty information faculty {0}", person.Recordkey));
                    }
                }
            }
            else
            {
                logger.Debug("Person data passed into FacultyRepository.BuildFacultyAsync was null.");
            }
            logger.Info("FacultyRepository.BuildFacultyAsync complete.");

            return facultyResults;
        }
        /// <summary>
        /// Get the Countries Table to translate code to name in address label
        /// </summary>
        /// <returns>Country Objects</returns>
        private async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            return await GetCodeItemAsync<Ellucian.Colleague.Data.Base.DataContracts.Countries, Country>("AllCountries", "COUNTRIES",
                d => new Country(d.Recordkey, d.CtryDesc, d.CtryIsoCode));
        }

        /// <summary>
        /// Build a string containing the city, state/province, and postal code
        /// </summary>
        /// <param name="city">City</param>
        /// <param name="state">State or Province</param>
        /// <param name="postalCode">Postal Code</param>
        /// <returns>Formatted string with all 3 components</returns>
        private string GetCityStatePostalCode(string city, string state, string postalCode)
        {
            StringBuilder line = new StringBuilder();

            if (!String.IsNullOrEmpty(city))
            {
                line.Append(city);
            }
            if (!String.IsNullOrEmpty(state))
            {
                if (line.Length > 0)
                {
                    line.Append(", ");
                }
                line.Append(state);
            }
            if (!String.IsNullOrEmpty(postalCode))
            {
                if (line.Length > 0)
                {
                    line.Append(" ");
                }
                line.Append(postalCode);
            }
            return line.ToString();
        }

        /// <summary>
        /// Get the professional name using specific type.
        /// </summary>
        /// <param name="type">Type of name</param>
        /// <param name="formattedNameTypes">List of formatted name types</param>
        /// <param name="formattedNames">List of formatted names</param>
        /// <returns>Formatted name</returns>
        private string GetProfessionalName(string type, List<string> formattedNameTypes, List<string> formattedNames)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Type of formatted name must be specified.");
            }
            if (formattedNameTypes == null || formattedNameTypes.Count() == 0)
            {
                return null;
            }

            // Get the position of the specified type in the name types
            int pos = formattedNameTypes.IndexOf(type);
            // If the name type was found, return the corresponding name.
            // Otherwise, return null
            return (pos >= 0) ? formattedNames[pos] : null;
        }
    }
}
