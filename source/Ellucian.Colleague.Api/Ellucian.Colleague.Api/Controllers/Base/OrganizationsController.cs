// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using CredentialType = Ellucian.Colleague.Dtos.EnumProperties.CredentialType;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Organizations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationsController : BaseCompressedApiController
    {
        private readonly IFacilitiesService _institutionService;
        private readonly IEducationalInstitutionsService _educationalInstitutionsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IPersonService _personService;

        /// <summary>
        /// OrganizationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="institutionService">Service of type <see cref="IFacilitiesService">IInstitutionService</see></param>
        /// <param name="personService">Service of type <see cref="IPersonService">IPersonService</see></param>
        /// <param name="educationalInstitutionsService">Service of type <see cref="IEducationalInstitutionsService">IEducationalInstitutionsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public OrganizationsController(IAdapterRegistry adapterRegistry, IFacilitiesService institutionService, IEducationalInstitutionsService educationalInstitutionsService, IPersonService personService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _institutionService = institutionService;
            _educationalInstitutionsService = educationalInstitutionsService;
            _personService = personService;
            _logger = logger;
        }

        /// <summary>
        /// Return all Organizations
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="role">Person Role equal to (guid)</param>
        /// <param name="credentialType">Person Credential Type (colleagueId or ssn)</param>
        /// <param name="credentialValue">Person Credential equal to</param>
        /// <returns>List of Organization <see cref="Dtos.Organization2"/> objects representing matching Organization</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewOrganization, BasePermissionCodes.CreateOrganization, BasePermissionCodes.UpdateOrganization }), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(new string[] { "role", "credentialType", "credentialValue" }, false, true)]

        public async Task<IHttpActionResult> GetOrganizations2Async(Paging page, [FromUri] string role = "", 
            [FromUri] string credentialType = "", [FromUri] string credentialValue = "")
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                if (!string.IsNullOrEmpty(credentialType)
                      && !credentialType.Equals(Dtos.EnumProperties.CredentialType.ColleaguePersonId.ToString(), StringComparison.OrdinalIgnoreCase)
                      && !credentialType.Equals(Dtos.EnumProperties.CredentialType.ElevateID.ToString(), StringComparison.OrdinalIgnoreCase) 
                   )
                {
                    throw new ArgumentException("credentialType", "credentialTypes other than ColleaguePersonId and ElevateID are not supported.");
                }

                if (!string.IsNullOrEmpty(credentialType) && string.IsNullOrEmpty(credentialValue))
                {
                    throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType.");
                }

                if (string.IsNullOrEmpty(credentialType) && !string.IsNullOrEmpty(credentialValue))
                {
                    throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                if (string.IsNullOrEmpty(role) || (role.Equals("partner", StringComparison.OrdinalIgnoreCase) ||
                                                   role.Equals("affiliate", StringComparison.OrdinalIgnoreCase) ||
                                                   role.Equals("constituent", StringComparison.OrdinalIgnoreCase) ||
                                                   role.Equals("vendor", StringComparison.OrdinalIgnoreCase)))
                {
                    var pageOfItems = await _personService.GetOrganizations2Async(page.Offset, page.Limit,
                        role, credentialType, credentialValue);

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                    return new PagedHttpActionResult<IEnumerable<Dtos.Organization2>>(pageOfItems.Item1, page,
                                            pageOfItems.Item2, this.Request);
                }
                else
                {
                    string[] roles = new string[] { "partner", "affiliate", "constituent", "vendor" };
                    if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                    {
                        throw new ArgumentException("role", string.Format("{0} is an invalid enumeration value.", role));
                    }
                    return new PagedHttpActionResult<IEnumerable<Dtos.Organization2>>(new List<Dtos.Organization2>(), page,
                        0, this.Request);
                }

            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) an Organization using a GUID
        /// </summary>
        /// <param name="id">GUID to desired Organization</param>
        /// <returns>An Organization object <see cref="Dtos.Organization2"/> in DataModel format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.ViewOrganization, BasePermissionCodes.CreateOrganization, BasePermissionCodes.UpdateOrganization })]
        public async Task<Dtos.Organization2> GetOrganizationByGuid2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {

                var institution = await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync(id);

                if (institution != null)
                {
                    throw new InvalidConstraintException(string.Concat("The id ", id, " does not belong to an organization, it belongs to an educational-institution."));
                }
            }
            catch (InvalidConstraintException constraintException)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(constraintException));
            }
            catch (ArgumentException argumentException)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("There is bad educational-institutions data preventing Organizations from making a required check.")));
            }
            catch (Exception ex)
            {
                //yes we are hiding other errors on purpose the method could fail with an error if the educational-institution is not found by the id
                _logger.Error(ex.Message, "Error: educational-institution is not found by the id");
            }


            try
            {
                _personService.ValidatePermissions(GetPermissionsMetaData());
                var organization = await _personService.GetOrganization2Async(id);

                if (organization != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { organization.Id }));
                }


                return organization;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new Organization
        /// </summary>
        /// <param name="organization">DTO of the new Organization</param>
        /// <returns>A Organization object <see cref="Dtos.Organization2"/> in Data Model format</returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.CreateOrganization })]
        public async Task<Dtos.Organization2> PostOrganizationAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Organization2 organization)
        {
            
            //call validate method for common validation between create/update
            await ValidateOrganizationContent(organization);

            //make sure the organization object has an Id as it is required
            if (string.IsNullOrEmpty(organization.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body, set the id to a nil guid if the id does not need to be specified.")));
            }

            if (organization.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            //make sure credentials are not a colleague person ID on POST - this is not allowed

            if (organization.Credentials != null && organization.Credentials.Any(c => c.Type == CredentialType.ColleaguePersonId))
            {
                throw CreateHttpResponseException(new IntegrationApiException("credential.Type not allowed",
                               IntegrationApiUtility.GetDefaultApiError("credential.Type of colleaguePersonId not allowed during organization creation.")));

            }

            try
            {
                //check permission
                _personService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the organization
                var organizationCreate = await _personService.CreateOrganizationAsync(organization);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { organizationCreate.Id }));

                return organizationCreate;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing Organization
        /// </summary>
        /// <param name="id">GUID of the Organization to update</param>
        /// <param name="organization">DTO of the updated Organization</param>
        /// <returns>A Organization object <see cref="Dtos.Organization2"/> in Data Model format</returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.UpdateOrganization })]
        public async Task<Dtos.Organization2> PutOrganizationAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Organization2 organization)
        {
           
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            //make sure the organization object has an Id as it is required
            if (string.IsNullOrEmpty(organization.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }
            
            //make sure the id on the url is not a nil one
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid id value",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation.")));
            }

            //make sure the id in the body is not a nil one
            if (organization.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid id value in the body",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation body.")));
            }
            
            //make sure the id in the body and on the url match
            if (!string.Equals(id, organization.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("ID not the same as in request body.")));
            }

            //make sure the organization object has an role type set for any roles included
            if (organization.Roles != null && organization.Roles.Any())
            {
                foreach (var organizationRole in organization.Roles)
                {
                    if (organizationRole.Type == OrganizationRoleType.NotSet)
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("Invalid role argument",
                            IntegrationApiUtility.GetDefaultApiError("A valid Role is required if Roles are included. Role must be one of vendor, partner, affiliate or constituent.")));
                    }
                }
            }
            else if (organization.Roles != null && !organization.Roles.Any())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid role argument",
                    IntegrationApiUtility.GetDefaultApiError("Organization roles cannot be unset.")));
            }

            if (organization.Credentials != null && !organization.Credentials.Any())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid credentials argument",
                    IntegrationApiUtility.GetDefaultApiError("The credentials type property is required if the credentials object is included in the payload.")));
            }

            try
            {
                //check permission
                _personService.ValidatePermissions(GetPermissionsMetaData());

                var institution = await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync(id);

                if (institution != null)
                {
                    throw new InvalidConstraintException(string.Concat("The id ", id,
                        " already exists and it does not belong to an organization, it belongs to an educational-institution, updates to educational institutions are not allowed from the organization endpoint."));
                }
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (InvalidConstraintException constraintException)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(constraintException));
            }
            catch (ArgumentException argumentException)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(new ArgumentException("There is bad educational-institutions data preventing Organizations from making a required check.")));
            }
            catch (Exception ex)
            {
                //yes we are hiding other errors on purpose, this check is being done to make sure the organization being updates is not an institution
                //the method will fail with an error if the educational-institution is not found by the id
                _logger.Error(ex.Message, "Error: educational-institution not found by the id");
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _personService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

               
                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(organization, async () => await _personService.GetOrganization2Async(id), dpList, _logger);
                var organizationReturn = await _personService.UpdateOrganizationAsync(partialmerged);

                 //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _personService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));
                return organizationReturn;
                
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }

        }

        /// <summary>
        /// Delete (DELETE) a Organization
        /// </summary>
        /// <param name="id">GUID to desired Organization</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteOrganizationByGuidAsync(string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }

        /// <summary>
        /// This method does validation on the deserialized json content of organization
        /// Will throw exceptions for errors with the data
        /// </summary>
        /// <param name="organization"></param>
        private async Task ValidateOrganizationContent(Dtos.Organization2 organization)
        {
            //make sure the request body deserialized into an Organization2 object
            if (organization == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null organization argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the organization object has a title as it is required
            if (string.IsNullOrEmpty(organization.Title))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null Title property",
                    IntegrationApiUtility.GetDefaultApiError("The title property is a required property.")));
            }

            //make sure Role has a value if roles are present
            if (organization.Roles != null && organization.Roles.Any())
            {
                if (organization.Roles.Any(r => r.Type == OrganizationRoleType.NotSet))
                {
                    throw CreateHttpResponseException(new IntegrationApiException("Role property not set correctly",
                    IntegrationApiUtility.GetDefaultApiError("A valid Role is required if Roles are included. Role must be one of vendor, partner, affiliate or constituent")));
                }
            }

            //make sure emails have a type value and that there are not multiple marked primary
            if (organization.EmailAddresses != null && organization.EmailAddresses.Any())
            {
                foreach (var email in organization.EmailAddresses)
                {
                    if (email.Type != null)
                    {
                        if (!email.Type.EmailType.HasValue)
                        {
                            throw CreateHttpResponseException(new IntegrationApiException("EmailType not set",
                                IntegrationApiUtility.GetDefaultApiError("EmailType is a required field if emails are included.")));
                        }
                    }
                    else
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("EmailType not set",
                                IntegrationApiUtility.GetDefaultApiError("EmailType is a required field if emails are included.")));
                    }
                }

                if (organization.EmailAddresses.Where(
                        e => e.Preference.HasValue && e.Preference.Value == PersonEmailPreference.Primary).Count() > 1)
                {
                    throw CreateHttpResponseException(new IntegrationApiException("More than one email marked Primary",
                                IntegrationApiUtility.GetDefaultApiError("More than one email is marked Primary, this is not allowed, only one email is allowed to be marked primary.")));
                }
            }

            //make sure credentials if present have a proper type set
            if (organization.Credentials != null && organization.Credentials.Any(c=> c.Type == CredentialType.NotSet))
            {
                throw CreateHttpResponseException(new IntegrationApiException("credential.Type not set",
                               IntegrationApiUtility.GetDefaultApiError("credential.Type is a required field if credentials are included. Valid values are elevateId or colleaguePersonId")));
                
            }

            if (organization.Addresses != null && organization.Addresses.Any())
            {
                foreach (var address in organization.Addresses)
                {
                    if (address.address != null && address.address.Place != null && address.address.Place.Country != null
                        && !string.IsNullOrEmpty(address.address.Place.Country.CarrierRoute) && address.address.Place.Country.CarrierRoute.Length > 4)

                    {
                        throw CreateHttpResponseException(new IntegrationApiException("carrierRoute too long",
                            IntegrationApiUtility.GetDefaultApiError(
                                string.Concat(
                                    "carrierRoute is not allowed to be longer than 4 characters, value received was",
                                    address.address.Place.Country.CarrierRoute))));
                    }
                }
            }
        }
    }
}
