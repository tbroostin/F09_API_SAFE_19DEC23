// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Web.Dependency;
using Ellucian.Web.Cache;
using Ellucian.Web.Utility;
using Ellucian.Web.Http.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FacultyRepository : PersonRepository, IFacultyRepository
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);

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
                throw new ArgumentNullException("id", "Must provide the id of the faculty to retrieve.");
            }
            try
            {
                facultyEntity = (await GetAllFacultyAsync())[id];
            }
            catch (Exception)
            {
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
                string errorMessage = "The following requested faculty ids were not found: " + idsNotFound;
                logger.Info(errorMessage);
            }
            return facultyEntities;
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
                logger.Info(errorMessage);
            }
            return facultyEntities;
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
            var facultyDict = await GetOrAddToCacheAsync<Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>>("AllFaculty",
                async () =>
                {
                    var facultyIds = await DataReader.SelectAsync("FACULTY", "");

                    // Bulk read the faculty PERSON records in chunks from the database.
                    var personData = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();
                    for (int i = 0; i < facultyIds.Count(); i += readSize)
                    {
                        var subList = facultyIds.Skip(i).Take(readSize).ToArray();
                        var bulkData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
                        personData.AddRange(bulkData);
                    }

                    // Bulk read the associated faculty addresses - needed for associated phone information.
                    var addressGroups = personData.Select(pa => pa.PersonAddresses).Where(a => a.Any(x => x.Length > 0));
                    List<string> addressIds = new List<string>();
                    if (addressGroups != null & addressGroups.Count() > 0)
                    {
                        foreach (var address in addressGroups)
                        {
                            if (address != null && address.Count() > 0)
                            {
                                addressIds.AddRange(address);
                            }
                        }
                    }
                    var facultyAddressData = new List<Ellucian.Colleague.Data.Base.DataContracts.Address>();
                    for (int i = 0; i < addressIds.Count(); i += readSize)
                    {
                        var subList = addressIds.Skip(i).Take(readSize).ToArray();
                        var bulkData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", subList);
                        facultyAddressData.AddRange(bulkData);
                    }

                    // Send all the info to BuildFaculty
                    var facultyEntities = await BuildFacultyAsync(personData, facultyAddressData);

                    return facultyEntities;
                }
            );
            return facultyDict;
        }

        private async Task<Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>> BuildFacultyAsync(List<Base.DataContracts.Person> personData,
                                                                                                     List<Base.DataContracts.Address> facultyAddressData)
        {
            var facultyResults = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>();

            if (personData != null)
            {
                var facultyAddressDataLookup = facultyAddressData.ToLookup(x => x.Recordkey);
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
                                    LogDataError("Person email address for person ID " + person.Recordkey, emailData.PersonEmailAddressesAssocMember, emailData, ex);
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
                                    LogDataError("Person personal phone for person ID " + person.Recordkey, phoneData.PersonalPhoneNumberAssocMember, phoneData, ex);
                                }
                            }
                        }
                        // Next get the local address phone information from the PSEASON association in person
                        if (person.PseasonEntityAssociation != null && person.PseasonEntityAssociation.Count() > 0)
                        {
                            var currentAddresses = person.PseasonEntityAssociation.Where(pa => pa.AddrEffectiveStartAssocMember == null || pa.AddrEffectiveStartAssocMember <= DateTime.Now);
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
                                            catch (Exception)
                                            {
                                                // Not bother logging these. Likely just dupicate phones.
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
                                                try
                                                {
                                                    Phone addressPhone = new Phone(addrPhone.AddressPhonesAssocMember, addrPhone.AddressPhoneTypeAssocMember, addrPhone.AddressPhoneExtensionAssocMember);
                                                    fac.AddPhone(addressPhone);
                                                }
                                                catch (Exception)
                                                {
                                                    // This is an invalid or duplicate phone and not worth logging.
                                                }
                                            }
                                        }
                                        try
                                        {
                                            var assoc = currentAddresses.Where(p => p.PersonAddressesAssocMember == addressId).FirstOrDefault();
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
                                        catch (Exception)
                                        {
                                            // This is an invalid or duplicate address and not worth logging.
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
                        LogDataError("Faculty", person.Recordkey, person, ex);
                    }
                }
            }
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
