﻿// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        /// <summary>
        /// Retrieve Address Objects based on a Post transaction with multiple person keys
        /// </summary>
        /// <param name="courseIds">Post in Body a list of person keys</param>
        /// <returns>list of Address objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Address> GetPersonAddressesByIds(IEnumerable<string> personIds)
        {
            AddressQueryCriteria criteria = new AddressQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Address retrieval.");
            }
            try
            {
                // Build url path from qapi path and address path
                string[] pathStrings = new string[] { _qapiPath, _addressPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get Addresses");
                throw;
            }
        }
        /// </summary>
        /// <returns>Returns a list of addresses</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Address> GetPersonAddresses(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Address retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_addressPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Address");
                throw;
            }
        }
        /// <summary>
        /// Gets all Departments, including ones marked as inactive. Use GetActiveDepartments to retrieve only the active ones.
        /// </summary>
        /// <returns>The set of all Departments in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Department> GetDepartments()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_departmentsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Department>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get departments");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get departments");
                throw;
            }
        }

        /// <summary>
        /// Gets all Departments that are marked as Active. Use GetDepartments to retrieve the full set including inactive ones.
        /// </summary>
        /// <returns>The set of all active Departments in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Department> GetActiveDepartments()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_departmentsPath, "active");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Department>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get departments");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get departments");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of locations
        /// </summary>
        /// <returns>A set of Locations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Location> GetLocations()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_locationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Location>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Location>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Location>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of buildings
        /// </summary>
        /// <returns>A set of Buildings</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Building> GetBuildings()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_buildingsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Building>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Building>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Building>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of convenience fees
        /// </summary>
        /// <returns>A set of Convenience Fees</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ConvenienceFee> GetConvenienceFees()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ecommercePath, _convenienceFeesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ConvenienceFee>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<ConvenienceFee>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ConvenienceFee>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of rooms
        /// </summary>
        /// <returns>A set of rooms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Room> GetRooms()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_roomsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Room>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Room>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Room>");
                throw;
            }
        }

        /// <summary>
        /// Get important numbers
        /// </summary>
        /// <returns>Returns a set of important numbers</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ImportantNumber> GetImportantNumbers()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_importantNumbersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ImportantNumber>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<ImportantNumber>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ImportantNumber>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of communication codes
        /// </summary>
        /// <returns>A set of Communication Codes</returns>
        [Obsolete("Obsolete as of version 1.8, use version 2 instead")]
        public IEnumerable<CommunicationCode> GetCommunicationCodes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_communicationCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CommunicationCode>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get communication codes>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of communication codes
        /// </summary>
        /// <returns>A set of Communication Codes</returns>
        public IEnumerable<CommunicationCode2> GetCommunicationCodes2()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_communicationCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CommunicationCode2>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get communication codes>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of denominations
        /// </summary>
        /// <returns>A set of Denominations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Denomination> GetDenominations()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_denominationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Denomination>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get denominations>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get denominations>");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of denominations
        /// </summary>
        /// <returns>A set of Denominations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<DisabilityType> GetDisabilityTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_disabilityTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DisabilityType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get disability types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get disability types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of interests
        /// </summary>
        /// <returns>A set of Interests</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Interest> GetInterests()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_interestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Interest>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get interests");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get interests");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of citizen types
        /// </summary>
        /// <returns>A set of Citizen Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<CitizenType> GetCitizenTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_citizenTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CitizenType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get citizen types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get citizen types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of degree types
        /// </summary>
        /// <returns>A set of Degree Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<DegreeType> GetDegreeTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreeTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreeType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get degree types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get degree types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of institution types
        /// </summary>
        /// <returns>A set of Institution Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<InstitutionType> GetInstitutionTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_institutionTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<InstitutionType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get institution types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get institution types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of languages
        /// </summary>
        /// <returns>A set of Languages</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Language> GetLanguages()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_languagesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Language>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get languages");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get languages");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of marital statuses
        /// </summary>
        /// <returns>A set of Marital Statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.MaritalStatus> GetMaritalStatuses()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_maritalStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.MaritalStatus>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get marital statuses");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get marital statuses");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of office Codes
        /// </summary>
        /// <returns>A set of Office codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<OfficeCode> GetOfficeCodes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_officeCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<OfficeCode>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get office codes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get office codes");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of prospect sources
        /// </summary>
        /// <returns>A set of Prospect Sources</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ProspectSource> GetProspectSources()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_prospectSourcesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ProspectSource>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get prospect sources");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get prospect sources");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of visa types
        /// </summary>
        /// <returns>A set of Visa Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.VisaType> GetVisaTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_visaTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.VisaType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get visa types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get visa types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of prefixes
        /// </summary>
        /// <returns>A set of Prefixes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Prefix> GetPrefixes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_prefixesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Prefix>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get prefixes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get prefixes");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of suffixes
        /// </summary>
        /// <returns>A set of Suffixes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Suffix> GetSuffixes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_suffixesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Suffix>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get suffixes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get suffixes");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all races
        /// </summary>
        /// <returns>The full list of races</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Race> GetRaces()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_racesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Race>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Race>");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all ethnicities
        /// </summary>
        /// <returns>The full list of ethnicities</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Ethnicity> GetEthnicities()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ethnicitiesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Ethnicity>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Ethnicity>");
                throw;
            }
        }

        /// <summary>
        /// Get all Frequency Codes
        /// </summary>
        /// <returns>The set of all MealPlans in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<FrequencyCode> GetFrequencyCodes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_frequencyCodesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FrequencyCode>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get FrequencyCodes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get FrequencyCodes");
                throw;
            }
        }

        /// <summary>
        /// Get full list of Schools
        /// </summary>
        /// <returns>The full list of Schools</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<School> GetSchools()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_schoolsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<School>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get Schools");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Schools");
                throw;
            }
        }
        /// <summary>
        /// Return Phone Numbers for a list of Person Ids
        /// </summary>
        /// <param name="personIds">List of Person Ids to retrieve phone numbers for</param>
        /// <returns>List of PhoneNumber objects for requested person Ids</returns>
        public IEnumerable<PhoneNumber> GetPersonPhonesByIds(IEnumerable<string> personIds)
        {
            PhoneNumberQueryCriteria criteria = new PhoneNumberQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                // Build url path from qapi path and phone path
                string[] pathStrings = new string[] { _qapiPath, _phoneNumberPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PhoneNumber>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get phone numbers.");
                throw;
            }
        }
        /// <summary>
        /// Return Pilot Phone Numbers for a list of Person Ids
        /// </summary>
        /// <param name="personIds">List of Person Ids to retrieve phone numbers for</param>
        /// <returns>List of PilotPhoneNumber objects for requested person Ids</returns>
        public IEnumerable<PilotPhoneNumber> GetPilotPersonPhonesByIds(IEnumerable<string> personIds)
        {
            PhoneNumberQueryCriteria criteria = new PhoneNumberQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                // Build url path from qapi path and phone path
                string[] pathStrings = new string[] { _qapiPath, _phoneNumberPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = ExecutePostRequestWithResponse(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotPhoneNumber>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get Pilot phone numbers.");
                throw;
            }
        }
        /// <summary>
        /// Return Pilot Phone Numbers for a list of Person Ids, asyncronously
        /// </summary>
        /// <param name="personIds">List of Person Ids to retrieve phone numbers for</param>
        /// <returns>List of PilotPhoneNumber objects for requested person Ids</returns>
        public async Task<IEnumerable<PilotPhoneNumber>> GetPilotPersonPhonesByIdsAsync(IEnumerable<string> personIds)
        {
            PhoneNumberQueryCriteria criteria = new PhoneNumberQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                // Build url path from qapi path and phone path
                string[] pathStrings = new string[] { _qapiPath, _phoneNumberPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPilotVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PilotPhoneNumber>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get Pilot phone numbers.");
                throw;
            }
        }
        /// <summary>
        /// Get all phone numbers for a single person Id
        /// </summary>
        /// <param name="id">Id of person to get phone numbers for</param>
        /// <returns>Phone Number object containing all phone numbers for the requested person</returns>
        public PhoneNumber GetPersonPhones(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_phoneNumberPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PhoneNumber>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Phone Numbers");
                throw;
            }
        }

        /// <summary>
        /// Gets all Institutions.
        /// </summary>
        /// <returns>The set of Institutions in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Institution> GetInstitutions()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(_institutionsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Institution>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get institutions");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get institutions");
                throw;
            }
        }

        /// <summary>
        /// Get any restrictions for a staff member.
        /// </summary>
        /// <returns>Returns a list of PersonRestriction</returns>
        /// <exception cref="ArgumentNullException">The staff id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested staff cannot be found.</exception>
        public IEnumerable<PersonRestriction> GetStaffRestrictions(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                throw new ArgumentNullException("staffId", "ID cannot be empty/null staff restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_staffPath, staffId, "restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Staff Restrictions.");
                throw;
            }
        }

        /// <summary>
        /// Get a staff member.
        /// </summary>
        /// <returns>Returns a staff record</returns>
        /// <exception cref="ArgumentNullException">The staff id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested staff cannot be found.</exception>
        public async Task<Staff> GetStaffAsync(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                throw new ArgumentNullException("staffId", "ID cannot be empty/null.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_staffPath, staffId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Staff>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Staff.");
                throw;
            }
        }

        /// <summary>
        /// Get an approvals document.
        /// </summary>
        /// <param name="documentId">ID of approval document</param>
        /// <returns>The ApprovalDocument</returns>
        /// <exception cref="ArgumentNullException">The document id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested document cannot be found.</exception>
        public ApprovalDocument GetApprovalDocument(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID is required.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_approvalsPath, "document", documentId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApprovalDocument>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get approval document " + documentId + ".");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get approval document " + documentId + ".");
                throw;
            }
        }

        /// <summary>
        /// Get an approvals response.
        /// </summary>
        /// <param name="documentId">ID of approval response</param>
        /// <returns>The ApprovalResponse</returns>
        /// <exception cref="ArgumentNullException">The response id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested response cannot be found.</exception>
        public ApprovalResponse GetApprovalResponse(string responseId)
        {
            if (string.IsNullOrEmpty(responseId))
            {
                throw new ArgumentNullException("responseId", "Document ID is required.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_approvalsPath, "response", responseId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApprovalResponse>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get approval response " + responseId + ".");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get approval response " + responseId + ".");
                throw;
            }
        }

        /// <summary>
        /// Gets the specified person's photo.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ApiFileStream GetPersonPhoto(string id)
        {
            try
            {
                string path = UrlUtility.CombineUrlPath(_personPhotoPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(path, headers: headers);
                // 'manual' deserialization since the data is embedded in the actual HTTP response.
                ApiFileStream afs = new ApiFileStream(response.Content.ReadAsStreamAsync().Result, response.Content.Headers.ContentType.MediaType);
                if (response.Content.Headers != null && response.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                {
                    afs.Filename = response.Content.Headers.ContentDisposition.FileName;
                }
                return afs;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get person photo for id '{0}'", id);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all available health conditions
        /// </summary>
        /// <returns>A list of health conditions</returns>
        /// <exception cref="ResourceNotFoundException">Unable to return health conditions</exception>
        public List<HealthConditions> GetHealthConditions()
        {
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath("/health-conditions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<List<HealthConditions>>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HealthConditions");
                throw;
            }

        }
        #region async calls
        /// <summary>
        /// Gets the specified person's photo.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<ApiFileStream> GetPersonPhotoAsync(string id)
        {
            try
            {
                string path = UrlUtility.CombineUrlPath(_personPhotoPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(path, headers: headers);
                // 'manual' deserialization since the data is embedded in the actual HTTP response.
                ApiFileStream afs = new ApiFileStream(await response.Content.ReadAsStreamAsync(), response.Content.Headers.ContentType.MediaType);
                if (response.Content.Headers != null && response.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                {
                    afs.Filename = response.Content.Headers.ContentDisposition.FileName;
                }
                return afs;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get person photo for id '{0}'", id);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all available health conditions
        /// </summary>
        /// <returns>A list of health conditions</returns>
        /// <exception cref="ResourceNotFoundException">Unable to return health conditions</exception>
        public async Task<List<HealthConditions>> GetHealthConditionsAsync()
        {
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath("/health-conditions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<List<HealthConditions>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HealthConditions");
                throw;
            }

        }

        /// <summary>
        /// Retrieve Address Objects based on a Post transaction with multiple person keys
        /// </summary>
        /// <param name="courseIds">Post in Body a list of person keys</param>
        /// <returns>list of Address objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>> GetPersonAddressesByIdsAsync(IEnumerable<string> personIds)
        {
            AddressQueryCriteria criteria = new AddressQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Address retrieval.");
            }
            try
            {
                // Build url path from qapi path and address path
                string[] pathStrings = new string[] { _qapiPath, _addressPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get Addresses");
                throw;
            }
        }
        /// </summary>
        /// <returns>Returns a list of addresses for the specified person</returns>
        /// <exception cref="ArgumentNullException">The person id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>> GetPersonAddressesAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Address retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_addressPath, personId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Address");
                throw;
            }
        }
        /// <summary>
        /// Gets all Departments, including ones marked as inactive. Use GetActiveDepartments to retrieve only the active ones.
        /// </summary>
        /// <returns>The set of all Departments in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_departmentsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Department>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get departments");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get departments");
                throw;
            }
        }

        /// <summary>
        /// Gets all Departments that are marked as Active. Use GetDepartments to retrieve the full set including inactive ones.
        /// </summary>
        /// <returns>The set of all active Departments in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_departmentsPath, "active");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Department>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get departments");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get departments");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of locations
        /// </summary>
        /// <returns>A set of Locations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Location>> GetLocationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_locationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Location>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Location>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Location>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of buildings
        /// </summary>
        /// <returns>A set of Buildings</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Building>> GetBuildingsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_buildingsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Building>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Building>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Building>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of convenience fees
        /// </summary>
        /// <returns>A set of Convenience Fees</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ConvenienceFee>> GetConvenienceFeesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ecommercePath, _convenienceFeesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ConvenienceFee>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<ConvenienceFee>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ConvenienceFee>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of rooms
        /// </summary>
        /// <returns>A set of rooms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Room>> GetRoomsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_roomsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Room>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Room>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Room>");
                throw;
            }
        }

        /// <summary>
        /// Get important numbers
        /// </summary>
        /// <returns>Returns a set of important numbers</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ImportantNumber>> GetImportantNumbersAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_importantNumbersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ImportantNumber>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<ImportantNumber>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<ImportantNumber>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of communication codes
        /// </summary>
        /// <returns>A set of Communication Codes</returns>
        [Obsolete("Obsolete as of version 1.8, use version 2 instead")]
        public async Task<IEnumerable<CommunicationCode>> GetCommunicationCodesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_communicationCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CommunicationCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get communication codes>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of communication codes
        /// </summary>
        /// <returns>A set of Communication Codes</returns>
        public async Task<IEnumerable<CommunicationCode2>> GetCommunicationCodes2Async()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_communicationCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CommunicationCode2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get communication codes>");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of denominations
        /// </summary>
        /// <returns>A set of Denominations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Denomination>> GetDenominationsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_denominationsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Denomination>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get denominations>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get denominations>");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of denominations
        /// </summary>
        /// <returns>A set of Denominations</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<DisabilityType>> GetDisabilityTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_disabilityTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DisabilityType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get disability types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get disability types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of interests
        /// </summary>
        /// <returns>A set of Interests</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Interest>> GetInterestsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_interestsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Interest>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get interests");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get interests");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of citizen types
        /// </summary>
        /// <returns>A set of Citizen Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CitizenType>> GetCitizenTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_citizenTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CitizenType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get citizen types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get citizen types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of degree types
        /// </summary>
        /// <returns>A set of Degree Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<DegreeType>> GetDegreeTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_degreeTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<DegreeType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get degree types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get degree types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of institution types
        /// </summary>
        /// <returns>A set of Institution Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<InstitutionType>> GetInstitutionTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_institutionTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<InstitutionType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get institution types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get institution types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of languages
        /// </summary>
        /// <returns>A set of Languages</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Language>> GetLanguagesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_languagesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Language>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get languages");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get languages");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of marital statuses
        /// </summary>
        /// <returns>A set of Marital Statuses</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.MaritalStatus>> GetMaritalStatusesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_maritalStatusesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.MaritalStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get marital statuses");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get marital statuses");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of office Codes
        /// </summary>
        /// <returns>A set of Office codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<OfficeCode>> GetOfficeCodesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_officeCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<OfficeCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get office codes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get office codes");
                throw;
            }
        }

        /// <summary>
        /// Requests the full set of prospect sources
        /// </summary>
        /// <returns>A set of Prospect Sources</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ProspectSource>> GetProspectSourcesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_prospectSourcesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<ProspectSource>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get prospect sources");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get prospect sources");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of visa types
        /// </summary>
        /// <returns>A set of Visa Types</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.VisaType>> GetVisaTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_visaTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.VisaType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get visa types");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get visa types");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of prefixes
        /// </summary>
        /// <returns>A set of Prefixes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Prefix>> GetPrefixesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_prefixesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Prefix>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get prefixes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get prefixes");
                throw;
            }
        }
        /// <summary>
        /// Requests the full set of suffixes
        /// </summary>
        /// <returns>A set of Suffixes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Suffix>> GetSuffixesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_suffixesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Suffix>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get suffixes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get suffixes");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all races
        /// </summary>
        /// <returns>The full list of races</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Race>> GetRacesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_racesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Race>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Race>");
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all ethnicities
        /// </summary>
        /// <returns>The full list of ethnicities</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Ethnicity>> GetEthnicitiesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_ethnicitiesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Ethnicity>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Ethnicity>");
                throw;
            }
        }

        /// <summary>
        /// Get all Frequency Codes
        /// </summary>
        /// <returns>The set of all MealPlans in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<FrequencyCode>> GetFrequencyCodesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_frequencyCodesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FrequencyCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get FrequencyCodes");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get FrequencyCodes");
                throw;
            }
        }

        /// <summary>
        /// Get full list of Schools
        /// </summary>
        /// <returns>The full list of Schools</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<School>> GetSchoolsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_schoolsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<School>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get Schools");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Schools");
                throw;
            }
        }
        /// <summary>
        /// Return Phone Numbers for a list of Person Ids
        /// </summary>
        /// <param name="personIds">List of Person Ids to retrieve phone numbers for</param>
        /// <returns>List of PhoneNumber objects for requested person Ids</returns>
        public async Task<IEnumerable<PhoneNumber>> GetPersonPhonesByIdsAsync(IEnumerable<string> personIds)
        {
            PhoneNumberQueryCriteria criteria = new PhoneNumberQueryCriteria();
            criteria.PersonIds = personIds;

            if (personIds == null)
            {
                throw new ArgumentNullException("personIds", "IDs cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                // Build url path from qapi path and phone path
                string[] pathStrings = new string[] { _qapiPath, _phoneNumberPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PhoneNumber>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to get phone numbers.");
                throw;
            }
        }
        /// <summary>
        /// Get all phone numbers for a single person Id
        /// </summary>
        /// <param name="id">Id of person to get phone numbers for</param>
        /// <returns>Phone Number object containing all phone numbers for the requested person</returns>
        public async Task<PhoneNumber> GetPersonPhonesAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be empty/null for Phone Number retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_phoneNumberPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PhoneNumber>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Phone Numbers");
                throw;
            }
        }

        /// <summary>
        /// Gets all Institutions.
        /// </summary>
        /// <returns>The set of Institutions in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Institution>> GetInstitutionsAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_institutionsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Institution>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get institutions");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get institutions");
                throw;
            }
        }

        /// <summary>
        /// Get any restrictions for a staff member.
        /// </summary>
        /// <returns>Returns a list of PersonRestriction</returns>
        /// <exception cref="ArgumentNullException">The staff id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested staff cannot be found.</exception>
        public async Task<IEnumerable<PersonRestriction>> GetStaffRestrictionsAsync(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                throw new ArgumentNullException("staffId", "ID cannot be empty/null staff restrictions.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_staffPath, staffId, "restrictions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonRestriction>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Staff Restrictions.");
                throw;
            }
        }

        /// <summary>
        /// Get an approvals document.
        /// </summary>
        /// <param name="documentId">ID of approval document</param>
        /// <returns>The ApprovalDocument</returns>
        /// <exception cref="ArgumentNullException">The document id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested document cannot be found.</exception>
        public async Task<ApprovalDocument> GetApprovalDocumentAsync(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID is required.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_approvalsPath, "document", documentId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApprovalDocument>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get approval document " + documentId + ".");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get approval document " + documentId + ".");
                throw;
            }
        }

        /// <summary>
        /// Get an approvals response.
        /// </summary>
        /// <param name="documentId">ID of approval response</param>
        /// <returns>The ApprovalResponse</returns>
        /// <exception cref="ArgumentNullException">The response id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested response cannot be found.</exception>
        public async Task<ApprovalResponse> GetApprovalResponseAsync(string responseId)
        {
            if (string.IsNullOrEmpty(responseId))
            {
                throw new ArgumentNullException("responseId", "Document ID is required.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_approvalsPath, "response", responseId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApprovalResponse>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get approval response " + responseId + ".");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get approval response " + responseId + ".");
                throw;
            }
        }
        /// <summary>
        /// Gets a person's Profile information
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <param name="useCache">Defaults to true: If true, cached repository data will be returned when possible, otherwise fresh data is returned.</param>
        /// <returns>A Profile dto</returns>
        /// <exception cref="ResourceNotFoundException">Unable to return profile</exception>
        public async Task<Profile> GetProfileAsync(string personId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "You must provide the person ID to return profile information.");
            }
            try
            {
                // Build url path
                string urlPath = UrlUtility.CombineUrlPath(_personsPath, personId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPersonProfileVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers, useCache: useCache);
                return JsonConvert.DeserializeObject<Profile>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Profile for this person");
                throw;
            }
        }

        /// <summary>
        ///Get phone types
        /// </summary>
        /// <returns>List of phone type objects containing code and description</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>> GetPhoneTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_phoneTypesPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Phone Types");
                throw;
            }
        }

        /// <summary>
        ///Get email types
        /// </summary>
        /// <returns>List of email type objects containing code and description</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.EmailType>> GetEmailTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_emailTypesPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.EmailType>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Email Types");
                throw;
            }
        }

        #region Obsolete PayableDepositAccount methods
        /// <summary>
        /// Method is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirectivesAsync instead
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        public Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PayableDepositAccount>> GetPayableDepositAccountsAsync()
        {
            throw new InvalidOperationException("GetPayableDepositAccountsAsync method is obsolete as of API 1.16. Use GetPayableDepositDirectivesAsync instead.");
        }

        /// <summary>
        /// Method is obsolete as of API 1.16 for security reasons. Use GetPayableDepositDirectiveAsync instead
        /// </summary>
        /// <param name="payableDepositId"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        public Task<Ellucian.Colleague.Dtos.Base.PayableDepositAccount> GetPayableDepositAccountAsync(string payableDepositId)
        {
            throw new InvalidOperationException("GetPayableDepositAccountAsync method is obsolete as of API 1.16. Use GetPayableDepositDirectiveAsync instead.");
        }

        /// <summary>
        /// Method is obsolete as of API 1.16 for security reasons. Use CreatePayableDepositDirectiveAsync instead
        /// </summary>
        /// <param name="payableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        public Task<Ellucian.Colleague.Dtos.Base.PayableDepositAccount> CreatePayableDepositAsync(PayableDepositAccount payableDepositAccount)
        {
            throw new InvalidOperationException("CreatePayableDepositAsync method is obsolete as of API 1.16. Use CreatePayableDepositDirectiveAsync instead.");
        }

        /// <summary>
        /// Method is obsolete as of API 1.16 for security reasons. Use UpdatePayableDepositDirectiveAsync instead
        /// </summary>
        /// <param name="payableDepositAccount"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        public Task<Ellucian.Colleague.Dtos.Base.PayableDepositAccount> UpdatePayableDepositAsync(PayableDepositAccount payableDepositAccount)
        {
            throw new InvalidOperationException("UpdatePayableDepositAsync method is obsolete as of API 1.16. Use UpdatePayableDepositDirectiveAsync instead.");
        }

        /// <summary>
        /// Method is obsolete as of API 1.16 for security reasons. Use DeletePayableDepositDirectiveAsync instead
        /// </summary>
        /// <param name="payableDepositId"
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16")]
        public Task DeletePayableDepositAsync(string payableDepositId)
        {
            throw new InvalidOperationException("DeletePayableDepositAsync method is obsolete as of API 1.16. Use DeletePayableDepositDirectiveAsync instead.");
        }

        #endregion

        /// This section supports the revised banking information data structure
        /// <summary>
        /// Get all PayableDepositDirectives for the current user
        /// </summary>
        /// <returns>A list of a person's PayableDepositDirectives</returns>
        public async Task<IEnumerable<PayableDepositDirective>> GetPayableDepositDirectivesAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payableDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<PayableDepositDirective>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Payable Deposit Directives.");
                throw;
            }
        }

        /// <summary>
        /// Get a single PayableDepositDirective for the current user.
        /// </summary>
        /// <param name="payableDepositDirectiveId">Id of the payableDepositDirective</param>
        /// <returns>A PayableDepositDirective</returns>
        public async Task<PayableDepositDirective> GetPayableDepositDirectiveAsync(string payableDepositDirectiveId)
        {

            if (string.IsNullOrEmpty(payableDepositDirectiveId))
            {
                throw new ArgumentNullException("payableDepositDirectiveId");
            }

            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payableDepositDirectivesPath, payableDepositDirectiveId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<PayableDepositDirective>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Payable Deposit Directive.");
                throw;
            }
        }

        /// <summary>
        /// Create a new PayableDepositDirective resource based on the data in the payableDepositDirective argument.
        /// </summary>
        /// <param name="payableDepositDirective"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(PayableDepositDirective payableDepositDirective, BankingAuthenticationToken token)
        {
            if (payableDepositDirective == null)
            {
                throw new ArgumentNullException("payableDepositDirective");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                // Create and execute a request to create a new 
                var urlPath = UrlUtility.CombineUrlPath(_payableDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecutePostRequestWithResponseAsync<PayableDepositDirective>(payableDepositDirective, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PayableDepositDirective>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create new Payable Deposit Directive.");
                throw;
            }
        }

        /// <summary>
        /// Update a PayableDepositDirective
        /// </summary>
        /// <param name="payableDepositDirective"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(PayableDepositDirective payableDepositDirective, BankingAuthenticationToken token)
        {
            if (payableDepositDirective == null)
            {
                throw new ArgumentNullException("payableDepositDirective");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_payableDepositDirectivesPath, payableDepositDirective.Id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecutePostRequestWithResponseAsync<PayableDepositDirective>(payableDepositDirective, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PayableDepositDirective>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update person's Payable Deposit Directive.");
                throw;
            }

        }

        /// <summary>
        /// Delete a PayableDeposit based on the given id.
        /// </summary>
        /// <param name="payableDepositId"></param>
        /// <returns></returns>
        public async Task DeletePayableDepositDirectiveAsync(string payableDepositDirectiveId, BankingAuthenticationToken token)
        {
            if (string.IsNullOrEmpty(payableDepositDirectiveId))
            {
                throw new ArgumentNullException("payableDepositDirectiveId");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_payableDepositDirectivesPath, payableDepositDirectiveId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);
                return;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete person's Payable Deposit Directive.");
                throw;
            }
        }

        /// <summary>
        /// Get a BankingAuthenticationToken object for a specific PayableDepositDirective
        /// </summary>
        /// <param name="payableDepositDirectiveId"></param>
        /// <param name="authenticationValue"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string payableDepositDirectiveId, string authenticationValue, string addressId)
        {


            try
            {
                var urlPath = string.IsNullOrEmpty(payableDepositDirectiveId) ?
                    UrlUtility.CombineUrlPath(_payableDepositDirectivesPath) :
                    UrlUtility.CombineUrlPath(_payableDepositDirectivesPath, payableDepositDirectiveId);

                var body = new PayableDepositDirectiveAuthenticationChallenge()
                {
                    PayableDepositDirectiveId = payableDepositDirectiveId,
                    ChallengeValue = authenticationValue,
                    AddressId = addressId
                };

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeStepUpAuthenticationVersion1);
                var response = await ExecutePostRequestWithResponseAsync<PayableDepositDirectiveAuthenticationChallenge>(body, urlPath, headers: headers);
                var result = JsonConvert.DeserializeObject<BankingAuthenticationToken>(await response.Content.ReadAsStringAsync());
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to authenticate payable deposit directive");
                throw;
            }
        }



        /// <summary>
        /// Get the Configuration object for Colleague Self Service Banking Information
        /// </summary>
        /// <returns></returns>
        public async Task<BankingInformationConfiguration> GetBankingInformationConfigurationAsync()
        {
            try
            {
                var urlPath = _bankingInformationConfigurationPath;
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<BankingInformationConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get configuration");
                throw;
            }
        }

        /// <summary>
        /// Get a Bank based on the given identifier. This includes banks known to Colleague Payroll as well
        /// as banks identified by the FedACH directory. This does not include Canadian Banks unless they are already 
        /// entered into the payroll system.
        /// </summary>
        /// <param name="id">The Id is the routing number for US Banks and the Institution Id for Canadian Banks.</param>
        /// <returns>A Bank DTO</returns>
        /// <exception cref="ResourceNotFoundException">Thrown if no bank exists for the given id.</exception>
        public async Task<Ellucian.Colleague.Dtos.Base.Bank> GetBankAsync(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_banksPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.Bank>(await response.Content.ReadAsStringAsync());
            }
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Bank not found");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Bank");
                throw;
            }

        }

        #endregion

        /// <summary>
        /// Gets a person's Profile information
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <returns>A Profile dto</returns>
        /// <exception cref="ResourceNotFoundException">Unable to return profile</exception>
        public Profile GetProfile(string personId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "You must provide the person ID to return profile information.");
            }
            try
            {
                // Build url path
                string urlPath = UrlUtility.CombineUrlPath(_personsPath, personId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPersonProfileVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, useCache: useCache, headers: headers);
                return JsonConvert.DeserializeObject<Profile>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Profile for this person");
                throw;
            }
        }

        /// <summary>
        /// Get a person's emergency information.
        /// </summary>
        /// <param name="personId">ID of the person whose emergency information is requested.</param>
        /// <returns>An EmergencyInformation object</returns>
        public async Task<EmergencyInformation> GetPersonEmergencyInformationAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "You must provide the person ID to return emergency information.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _emergencyInformationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EmergencyInformation for this person");
                throw;
            }
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An EmergencyInformation object.</param>
        /// <returns>An updated EmergencyInformation object.</returns>
        public EmergencyInformation UpdatePersonEmergencyInformation(EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw new ArgumentNullException("emergencyInformation", "emergencyInformation cannot be null.");
            }
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, emergencyInformation.PersonId, _emergencyInformationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecutePutRequestWithResponse<EmergencyInformation>(emergencyInformation, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmergencyInformation>(response.Content.ReadAsStringAsync().Result);
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonEmergencyInformation");
                throw;
            }
        }

        /// <summary>
        /// Gets the list of tax form consent choices for a specified person
        /// </summary>
        /// <param name="personId">This is the person id.</param>
        /// <returns>A list of tax form consent choices.</returns>
        public async Task<IEnumerable<TaxFormConsent>> GetTaxFormConsents(string personId, TaxForms taxForm)
        {
            try
            {
                // Create and execute a request to get a list of tax form consents
                string[] pathStrings = new string[] { _taxFormConsentsPath, personId, taxForm.ToString() };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<TaxFormConsent>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get tax form consents for Person ID {0}.", personId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get tax form consents.");
                throw;
            }
        }

        /// <summary>
        /// Get a W-2 tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">The record ID where the W-2 pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> GetW2TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxFormW2PdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve W-2 tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Get a 1095-C tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="recordId">The record ID where the 1095-C pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> Get1095cTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "PersonId ID cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("recordId", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxForm1095cPdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve 1095-C tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Get a T4 tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4.</param>
        /// <param name="recordId">The record ID where the T4 pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> GetT4TaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxFormT4PdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve T4 tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Get a T4A tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> GetT4aTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxFormT4aPdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve T4A tax form pdf.");
                throw;
            }
        }

        /// <summary>
        /// Gets the primary relationships for a person or organization
        /// </summary>
        /// <param name="personId">The identifier of the person of interest</param>
        /// <returns>An enumeration of the person's primary relationship with other persons or organizations.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Relationship>> GetPersonPrimaryRelationshipsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "You must provide the person Id for which to retrieve relationships.");
            }

            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _relationshipsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var relationships = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Relationship>>(await response.Content.ReadAsStringAsync());
                return relationships;

            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the person's relationships");
                throw;
            }
        }

        /// <summary>
        /// Returns a List of relationship type objects. 
        /// </summary>
        /// <returns>The requested <see cref="RelationshipType">Relationship Type</see> objects</returns>
        public async Task<IEnumerable<RelationshipType>> GetRelationshipTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_relationshipTypesPath, headers: headers);
                var relationshipTypes = JsonConvert.DeserializeObject<IEnumerable<RelationshipType>>(await responseString.Content.ReadAsStringAsync());

                return relationshipTypes;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the relationship type list.");
                throw;
            }
        }


        /// <summary>
        /// Gets the configuration parameters for Proxy user setup
        /// </summary>
        /// <returns>The proxy configuration parameter values <see cref="ProxyConfiguration"/></returns>
        public async Task<ProxyConfiguration> GetProxyConfigurationAsync()
        {
            try
            {
                // Build url path
                var urlPath = UrlUtility.CombineUrlPath(_configurationPath, "proxy");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var config = JsonConvert.DeserializeObject<ProxyConfiguration>(await response.Content.ReadAsStringAsync());
                return config;

            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the proxy configuration.");
                throw;
            }
        }

        /// <summary>
        /// Create a new tax form consent entry.
        /// </summary>
        /// <param name="consent">TaxFormConsent DTO</param>
        /// <returns>TaxFormConsent DTO</returns>
        public async Task<TaxFormConsent> AddTaxFormConsent(TaxFormConsent consent)
        {
            try
            {
                // Create and execute a request to create a new 
                string[] pathStrings = new string[] { _taxFormConsentsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<TaxFormConsent>(consent, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<TaxFormConsent>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to create new consent entry.");
                throw;
            }
        }

        /// <summary>
        /// Gets the tax form configuration for the tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax Form Configuration for the type of tax form.</returns>
        public async Task<TaxFormConfiguration> GetTaxFormConfiguration(TaxForms taxFormId)
        {
            try
            {
                // Create and execute a request to get the tax form configuration
                string[] pathStrings = new string[] { _configurationPath, _taxFormsPath, taxFormId.ToString() };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<TaxFormConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception e)
            {
                logger.Error(e, "Unable to get tax form consents.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a List of Commencement Site objects. 
        /// </summary>
        /// <returns>The requested <see cref="CommencementSite">Commencement Site</see> objects</returns>
        public async Task<IEnumerable<CommencementSite>> GetCommencementSitesAsync()
        {

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_commencementSitesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<CommencementSite>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the commencement site list.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a list of Country objects.
        /// </summary>
        /// <returns>The requested <see cref="Country">country</see> object</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Country>> GetCountriesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_countriesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.Country>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the country codes.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns a list of State objects.
        /// </summary>
        /// <returns>The requested <see cref="State">state</see> object</returns>
        public async Task<IEnumerable<State>> GetStatesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(_statesPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<IEnumerable<State>>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the state codes.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the User Profile Configuration.
        /// </summary>
        /// <returns>The User Profile Configuration object</returns>
        public async Task<UserProfileConfiguration> GetUserProfileConfigurationAsync()
        {
            try
            {
                string[] pathStrings = new string[] { _configurationPath, "user-profile" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<UserProfileConfiguration>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the User Profile Configuration.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the User Profile Configuration.
        /// </summary>
        /// <returns>The User Profile Configuration object</returns>
        public async Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async()
        {
            try
            {
                string[] pathStrings = new string[] { _configurationPath, "user-profile" };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<UserProfileConfiguration2>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the User Profile Configuration.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously returns the Emergency Information Configuration.
        /// </summary>
        /// <returns>The Emergency Information Configuration object</returns>
        public async Task<EmergencyInformationConfiguration2> GetEmergencyInformationConfigurationAsync()
        {
            try
            {
                string[] pathStrings = new string[] { _configurationPath, _emergencyInformationPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var configuration = JsonConvert.DeserializeObject<EmergencyInformationConfiguration2>(await responseString.Content.ReadAsStringAsync());

                return configuration;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the Emergency Information Configuration.");
                throw;
            }
        }

        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profile">The <see cref="Profile">Profile</see> object to update.</param>
        /// <returns>The updated <see cref="Profile">Profile</see> object.</returns>
        [Obsolete("Obsolete as of version 1.16, use version 2 instead")]
        public async Task<Profile> UpdateProfileAsync(Profile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile", "profile cannot be null.");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, profile.Id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPersonProfileVersion1);
                var response = await ExecutePutRequestWithResponseAsync<Profile>(profile, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Profile>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update Profile.");
                throw;
            }
        }

        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profile">The <see cref="Profile">Profile</see> object to update.</param>
        /// <returns>The updated <see cref="Profile">Profile</see> object.</returns>
        public async Task<Profile> UpdateProfile2Async(Profile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile", "profile cannot be null.");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, profile.Id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPersonProfileVersion2);
                var response = await ExecutePutRequestWithResponseAsync<Profile>(profile, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Profile>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update Profile.");
                throw;
            }
        }

        /// <summary>
        /// Query person by criteria and return the results produced by the matching algorithm
        /// </summary>
        /// <param name="person">The <see cref="Dtos.Base.PersonMatchCriteria">criteria</see> to query by.</param>
        /// <returns>List of matching <see cref="Dtos.Base.PersonMatchResult"> results</see></returns>
        public async Task<IEnumerable<PersonMatchResult>> QueryPersonMatchResultsByPostAsync(PersonMatchCriteria criteria)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _personsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<PersonMatchCriteria>(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonMatchResult>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve person-matching results.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the matching Persons for the ids provided or searches keyword
        /// for the matching Persons if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can be either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        /// <exception cref="ArgumentNullException">Criteria must be provided</exception>
        /// <exception cref="PermissionsException">Person must have permissions to search for persons</exception>
        public async Task<IEnumerable<Dtos.Base.Person>> QueryPersonNamesByPostAsync(Dtos.Base.PersonNameQueryCriteria criteria)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _personsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPersonNameSearchVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.Base.Person>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve person name search results.");
                throw;
            }
        }

        /// <summary>
        /// Creates a new relationship between two persons
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship"/> to create</param>
        /// <returns>The created <see cref="Relationship"/></returns>
        public async Task<Ellucian.Colleague.Dtos.Base.Relationship> PostRelationshipAsync(Ellucian.Colleague.Dtos.Base.Relationship relationship)
        {
            try
            {
                string[] pathStrings = new string[] { _personsPath, relationship.PrimaryEntity, _relationshipsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<Ellucian.Colleague.Dtos.Base.Relationship>(relationship, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.Relationship>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to query matching persons.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of MiscellaneousText DTOs
        /// </summary>
        /// <returns>The institution-defined list of <see cref="MiscellaneousText"/> DTOs</returns>
        public async Task<IEnumerable<MiscellaneousText>> GetAllMiscellaneousTextAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath("miscellaneous-text");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.MiscellaneousText>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve MiscellaneousTextConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the given self service preference for the given user
        /// </summary>
        /// <param name="personId">Person for which to retrieve preferences</param>
        /// <param name="preferenceType">The module for which to retrieve the user's preferences</param>
        /// <returns>Object containing the preferences for the module for the user</returns>
        public async Task<SelfservicePreference> GetSelfservicePreferenceAsync(string personId, string preferenceType)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentNullException("personId", "The personId cannot be null or empty.");
                }
                if (string.IsNullOrEmpty(preferenceType))
                {
                    throw new ArgumentNullException("preferenceType", "The preferenceType cannot be null or empty.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, personId, _selfServicePreferencesPath, preferenceType);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var preference = JsonConvert.DeserializeObject<SelfservicePreference>(await response.Content.ReadAsStringAsync());
                return preference;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving SelfservicePreference");
                throw;
            }
        }

        /// <summary>
        /// Updates the given self service preference
        /// </summary>
        /// <param name="preference">The self service preference</param>
        /// <returns>Object containing the updated preference</returns>
        public async Task<SelfservicePreference> UpdateSelfservicePreferenceAsync(SelfservicePreference preference)
        {
            if (preference == null)
            {
                throw new ArgumentNullException("preference", "SelfservicePreference object cannot be null.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, preference.PersonId, _selfServicePreferencesPath, preference.PreferenceType);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync<SelfservicePreference>(preference, urlPath, headers: headers);
                var updatedPreference = JsonConvert.DeserializeObject<SelfservicePreference>(await response.Content.ReadAsStringAsync());
                return updatedPreference;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving SelfservicePreference");
                throw;
            }
        }

        /// <summary>
        /// Deletes the given self service preference for the given user
        /// </summary>
        /// <param name="personId">person id</param>
        /// <param name="preferenceType">preference type</param>
        /// <returns>true if successful</returns>
        public async Task<bool> DeleteSelfServicePreferenceAsync(string personId, string preferenceType)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentNullException("personId", "The personId cannot be null or empty.");
                }
                if (string.IsNullOrEmpty(preferenceType))
                {
                    throw new ArgumentNullException("preferenceType", "The preferenceType cannot be null or empty.");
                }
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, personId, _selfServicePreferencesPath, preferenceType);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error deleting SelfservicePreference");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of WorkTask DTOs for the given user
        /// </summary>
        /// <returns>List of assigned <see cref="WorkTask">WorkTask</see>> DTOs</returns>
        public async Task<IEnumerable<WorkTask>> GetWorkTasksAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "PersonId cannot be null or empty.");
            }
            try
            {
                string query = UrlUtility.BuildEncodedQueryString("personId", personId);
                string urlPath = UrlUtility.CombineUrlPathAndArguments(_workTasksPath, query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.WorkTask>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving WorkTask items for person " + personId);
                throw;
            }
        }

        /// <summary>
        /// Retrieve a set of tax form statement DTOs.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<TaxFormStatement2>> GetTaxFormStatements2(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is required.");

            try
            {
                // Create and execute a request to get all projects
                string urlPath = UrlUtility.CombineUrlPath(_taxFormStatementsPath, personId, taxForm.ToString());
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<TaxFormStatement2>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get tax form statements.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <returns>A text document</returns>
        /// <exception cref="Exception">Thrown if an error occurred retrieving the text document.</exception>
        public async Task<TextDocument> GetTextDocumentAsync(string documentId, string primaryEntity, string primaryId, string personId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(primaryEntity))
            {
                throw new ArgumentNullException("primaryEntity", "Primary cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", "Primary key cannot be null or empty.");
            }

            try
            {
                // Build url path from qapi path and student statements path
                var baseUrl = UrlUtility.CombineUrlPath(_textDocumentsPath, UrlParameterUtility.EncodeWithSubstitution(documentId));
                var queryString = UrlUtility.BuildEncodedQueryString("primaryEntity", primaryEntity,
                            "primaryId", primaryId, "personId", personId);
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);

                var resource = JsonConvert.DeserializeObject<TextDocument>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve text document.");
                throw;
            }
        }


        /// <summary>
        /// Gets all privacy statuses.
        /// </summary>
        /// <returns>The set of all privacy statuses in the database</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<PrivacyStatus>> GetPrivacyStatusesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _hedtechIntegrationMediaTypeFormatVersion6);
                var response = await ExecuteGetRequestWithResponseAsync(_privacyStatusesPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PrivacyStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get privacy statuses.");
                throw;
            }
        }

        /// <summary>
        /// Get the Privacy Configuration object for Colleague Self Service
        /// </summary>
        /// <returns></returns>
        public async Task<PrivacyConfiguration> GetPrivacyConfigurationAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_configurationPath, _privacyPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<PrivacyConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get privacy configuration");
                throw;
            }
        }

        /// <summary>
        /// Get address types
        /// </summary>
        /// <returns>List of address type objects containing code and description</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>> GetAddressTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _hedtechIntegrationMediaTypeFormatVersion6);
                var response = await ExecuteGetRequestWithResponseAsync(_addressTypesPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Address Types");
                throw;
            }
        }

        /// <summary>
        /// Returns organizational level position assignments for the given person position with direct relationships.
        /// </summary>
        /// <returns>OrganizationalPersonPosition with embedded list of OrganizationalRelationship</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition> GetOrganizationalPersonPositionByIdAsync(string personPositionId)
        {
            if (string.IsNullOrWhiteSpace(personPositionId))
            {
                throw new ArgumentNullException("personPositionId", "Person position ID is required to retrieve organizational person position.");
            }
            try
            {
                string[] pathStrings = new string[] { _organizationalPersonPositionsPath, personPositionId };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Organizational Relationships");
                throw;
            }
        }

        /// <summary>
        /// Returns organizational level position assignments for the specified persons/personPositions with direct relationships for each position assignment.
        /// </summary>
        /// <returns>Returns list of OrganizationalPersonPosition with embedded list of OrganizationalRelationship</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>> GetOrganizationalPersonPositionsAsync(OrganizationalPersonPositionQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _organizationalPersonPositionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Organizational Relationships");
                throw;
            }
        }

        /// <summary>
        /// Creates a new organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship to create</param>
        /// <returns>The new organizational relationship</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship> CreateOrganizationalRelationshipAsync(OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                if (organizationalRelationship == null)
                {
                    throw new ArgumentNullException("organizationalRelationship");
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(organizationalRelationship, _organizationalRelationshipsPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to add Organizational Relationship");
                throw;
            }
        }

        /// <summary>
        /// Updates an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship to update</param>
        /// <returns>The updated organizational relationship</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship> UpdateOrganizationalRelationshipAsync(OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                if (organizationalRelationship == null)
                {
                    throw new ArgumentNullException("organizationalRelationship");
                }
                string[] pathStrings = new string[] { _organizationalRelationshipsPath, organizationalRelationship.Id };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(organizationalRelationship, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update Organizational Relationship");
                throw;
            }
        }

        /// <summary>
        /// Delete an organizational relationship
        /// </summary>
        /// <param name="id">Organizational relationship ID to delete</param>
        public async Task DeleteOrganizationalRelationshipAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id");
                }
                string[] pathStrings = new string[] { _organizationalRelationshipsPath, id };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete Organizational Relationship");
                throw;
            }
        }

        /// <summary>
        /// Get the Organizational Relationship Configuration
        /// </summary>
        /// <returns>Organizational Relationship Configuration</returns>
        public async Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_configurationPath, _organizationalRelationshipsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<OrganizationalRelationshipConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get organizational relationship configuration");
                throw;
            }
        }

        /// <summary>
        /// Get personal pronoun types
        /// </summary>
        /// <returns>List of personal pronoun type objects containing code and description</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>> GetPersonalPronounTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_personalPronounTypesPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Personal Pronoun Types");
                throw;
            }
        }

        /// <summary>
        /// Get gender identity types
        /// </summary>
        /// <returns>List of gender identity type objects containing code and description</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>> GetGenderIdentityTypesAsync()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(_genderIdentityTypesPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Gender Identity Types");
                throw;
            }
        }

        /// <summary>
        /// Gets an organizational position for the given id
        /// </summary>
        /// <param name="id">Organizational position id</param>
        /// <returns>Organizational position matching the id</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalPosition> GetOrganizationalPositionAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty");
            }
            try
            {
                string[] pathStrings = new string[] { _organizationalPositionsPath, id };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.OrganizationalPosition>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Organizational Position");
                throw;
            }
        }

        /// <summary>
        /// Retrieves organizational position matching the given criteria
        /// </summary>
        /// <param name="criteria">Organizational position query criteria</param>
        /// <returns>Matching organizational position dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPosition>> QueryOrganizationalPositionsAsync(OrganizationalPositionQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria cannot be null");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _organizationalPositionsPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPosition>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Organizational Position");
                throw;
            }
        }

        /// <summary>
        /// Creates a new organizational position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationship">The organizational position relationship to create</param>
        /// <returns>The new organizational position relationship</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalPositionRelationship> CreateOrganizationalPositionRelationshipAsync(OrganizationalPositionRelationship organizationalPositionRelationship)
        {
            try
            {
                if (organizationalPositionRelationship == null)
                {
                    throw new ArgumentNullException("organizationalPositionRelationship");
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(organizationalPositionRelationship, _organizationalPositionRelationshipsPath, headers: headers);
                return JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Base.OrganizationalPositionRelationship>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to add Organizational Position Relationship");
                throw;
            }
        }

        /// <summary>
        /// Delete an organizational position relationship
        /// </summary>
        /// <param name="id">Organizational position relationship ID to delete</param>
        public async Task DeleteOrganizationalPositionRelationshipAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id");
                }
                string[] pathStrings = new string[] { _organizationalPositionRelationshipsPath, id };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to delete Organizational Position Relationship");
                throw;
            }
        }

        /// <summary>
        /// Get all PayrollDepositDirectives owned by the Current User
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollDepositDirective>> GetPayrollDepositDirectivesAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PayrollDepositDirective>>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get PayrollDepositDirectives");
                throw;
            }
        }

        /// <summary>
        /// Get a single PayrollDepositDirective
        /// </summary>
        /// <param name="payrollDepositDirectiveId"></param>
        /// <returns></returns>
        public async Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string payrollDepositDirectiveId)
        {
            if (string.IsNullOrEmpty(payrollDepositDirectiveId))
            {
                throw new ArgumentNullException("payrollDepositDirectiveId");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath, payrollDepositDirectiveId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PayrollDepositDirective>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get PayrollDepositDirective");
                throw;
            }
        }


        /// <summary>
        /// Update a single PayrollDepositDirective. BankingAuthenticationToken is required
        /// </summary>
        /// <param name="payrollDepositDirective"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PayrollDepositDirective> UpdatePayrollDepositDirectiveAsync(PayrollDepositDirective payrollDepositDirective, BankingAuthenticationToken token)
        {
            if (payrollDepositDirective == null)
            {
                throw new ArgumentNullException("payrollDepositDirective");
            }
            if (string.IsNullOrEmpty(payrollDepositDirective.Id))
            {
                throw new ArgumentException("Id of directive must be specified", "payrollDepositDirective");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath, payrollDepositDirective.Id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecutePutRequestWithResponseAsync<PayrollDepositDirective>(payrollDepositDirective, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PayrollDepositDirective>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to update PayrollDepositDirectives");
                throw;
            }
        }

        /// <summary>
        /// Batch update a list of PayrollDepositDirectives. BankingAuthenticationToken is required
        /// </summary>
        /// <param name="payrollDepositDirectives"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollDepositDirective>> UpdatePayrollDepositDirectivesAsync(IEnumerable<PayrollDepositDirective> payrollDepositDirectives, BankingAuthenticationToken token)
        {
            if (payrollDepositDirectives == null || !payrollDepositDirectives.Any())
            {
                throw new ArgumentNullException("payrollDepositDirectives");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecutePutRequestWithResponseAsync<IEnumerable<PayrollDepositDirective>>(payrollDepositDirectives, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PayrollDepositDirective>>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to update PayrollDepositDirectives");
                throw;
            }
        }

        /// <summary>
        /// Create a PayrollDepositDirective. BankingAuthenticationToken is required
        /// </summary>
        /// <param name="payrollDepositDirective"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PayrollDepositDirective> CreatePayrollDepositDirectiveAsync(PayrollDepositDirective payrollDepositDirective, BankingAuthenticationToken token)
        {
            if (payrollDepositDirective == null)
            {
                throw new ArgumentNullException("payrollDepositDirective");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecutePostRequestWithResponseAsync<PayrollDepositDirective>(payrollDepositDirective, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<PayrollDepositDirective>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to create PayrollDepositDirective");
                throw;
            }
        }

        /// <summary>
        /// Delete a PayrollDepositDirect. Requires a BankingAuthenticationToken
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task DeletePayrollDepositDirectiveAsync(string id, BankingAuthenticationToken token)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);

                return;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to delete PayrollDepositDirectives");
                throw;
            }
        }

        /// <summary>
        /// Delete multiple PayrollDepositDirects. BankingAuthenticationToken is required
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task DeletePayrollDepositDirectivesAsync(IEnumerable<string> ids, BankingAuthenticationToken token)
        {
            if (token != null && token.ExpirationDateTimeOffset < DateTimeOffset.Now)
            {
                throw new ArgumentException("token is expired", "token");
            }
            try
            {
                List<string> args = new List<string>();
                foreach (var id in ids)
                {
                    args.Add("id");
                    args.Add(id);
                }
                var arguments = UrlUtility.BuildEncodedQueryString(args.ToArray());
                var urlPath = UrlUtility.CombineUrlPathAndArguments(_payrollDepositDirectivesPath, arguments);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                if (token != null)
                {
                    headers.Add(StepUpAuthenticationHeaderKey, token.Token.ToString());
                }
                var response = await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);

                return;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to delete PayrollDepositDirective");
                throw;
            }
        }

        /// <summary>
        /// Get a BankingAuthenticationToken to update a specific PayrollDepositDirective
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authenticationValue"></param>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticatePayrollDepositDirectiveAsync(string id, string authenticationValue)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(authenticationValue))
            {
                throw new ArgumentNullException("authenticationValue");
            }

            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeStepUpAuthenticationVersion1);

                var response = await ExecutePostRequestWithResponseAsync<string>(authenticationValue, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<BankingAuthenticationToken>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get BankingAuthenticationToken for PayrollDepositDirective");
                throw;
            }
        }

        /// <summary>
        /// Get a BankingAuthenticationToken for an employee with no payroll depositDirectives
        /// </summary>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticatePayrollDepositDirectiveAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payrollDepositDirectivesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeStepUpAuthenticationVersion1);

                var response = await ExecutePostRequestWithResponseAsync<string>(null, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<BankingAuthenticationToken>(await response.Content.ReadAsStringAsync());

                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get BankingAuthenticationToken for PayrollDepositDirective");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the matching Employees for the ids provided or searches keyword
        /// for the matching Employees if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can be either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        /// <exception cref="ArgumentNullException">Criteria must be provided</exception>
        /// <exception cref="PermissionsException">Person must have permissions to search for persons</exception>
        public async Task<IEnumerable<Dtos.Base.Person>> QueryEmployeeNamesByPostAsync(Dtos.Base.PersonNameQueryCriteria criteria)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _employeesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderEmployeeNameSearchVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.Base.Person>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve person name search results.");
                throw;
            }
        }
        /// <summary>
        /// Retrieves the matching Employee keys for the ids provided or searches keyword
        /// for the matching Employees/nonemployees if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// Can filter out inactive employees and include non-employees.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can be either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        /// <exception cref="ArgumentNullException">Criteria must be provided</exception>
        /// <exception cref="PermissionsException">Person must have permissions to search for persons</exception>
        public async Task<IEnumerable<Dtos.Base.Person>> QueryEmployeeNamesByPostAsync(Dtos.Base.EmployeeNameQueryCriteria criteria)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _employeesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderEmployeeNameSearchVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Dtos.Base.Person>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve person name search results.");
                throw;
            }
        }
        /// <summary>
        /// Get the faculty contracts by the faculty's Id
        /// </summary>
        /// <param name="facultyId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId)
        {
            try
            {
                string[] pathStrings = new string[] { _facultyPath, facultyId, _contractsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<FacultyContract>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve faculty contract results.");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LoadPeriod>> QueryLoadPeriodsByPostAsync(Dtos.Base.LoadPeriodQueryCriteria criteria)
        {
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _loadPeriodsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<LoadPeriod>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve load period results.");
                throw;
            }
        }

        /// <summary>
        /// Get all campus calendars
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CampusCalendar>> GetCampusCalendarsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_campusCalendarsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<CampusCalendar>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get campus calendars");
                throw;
            }
        }

        /// <summary>
        /// Retrieves Colleague Self-Service configuration information
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        public async Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_configurationPath, _selfServicePath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<SelfServiceConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve Self-Service configuration data.");
                throw;
            }
        }

        /// <summary>
        /// Get a 1099-MISC tax form PDF
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>
        /// <returns>Byte array containing PDF data</returns>
        public async Task<byte[]> Get1099MiscTaxFormPdf(string personId, string recordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId cannot be null or empty.");

            if (string.IsNullOrEmpty(recordId))
                throw new ArgumentNullException("id", "Record ID cannot be null or empty.");

            try
            {
                // Build url path and create and execute a request to get the tax form pdf
                var urlPath = UrlUtility.CombineUrlPath(_personsPath, personId, _taxForm1099MiPdfPath, recordId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPdfVerion1);
                headers.Add("X-Ellucian-Media-Type", _mediaTypeHeaderPdfVerion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve 1099-MISC tax form pdf.");
                throw;
            }
        }
    }
}
